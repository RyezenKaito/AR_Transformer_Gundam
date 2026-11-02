using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace W7
{
    // Minimal MQTT 3.1.1 client over TCP: QoS 0, Last Will, no external dependency.
    // Works on Editor / Windows / macOS / Android / iOS (NOT WebGL).
    public class MiniMqttClient
    {
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string, string> OnMessage;

        readonly string host;
        readonly int port;
        readonly string clientId;
        readonly int keepAliveSec;

        TcpClient tcp;
        NetworkStream stream;
        readonly object writeLock = new object();
        readonly Queue<Action> mainQueue = new Queue<Action>();
        volatile bool running;
        volatile bool connected;
        long lastSendTicks;

        public bool IsConnected { get { return connected; } }

        public MiniMqttClient(string host, int port, string clientId, int keepAliveSec = 20)
        {
            this.host = host;
            this.port = port;
            this.clientId = clientId;
            this.keepAliveSec = keepAliveSec;
        }

        // Call from the main thread to receive events on the main thread
        public void Pump()
        {
            while (true)
            {
                Action a;
                lock (mainQueue)
                {
                    if (mainQueue.Count == 0) return;
                    a = mainQueue.Dequeue();
                }
                a();
            }
        }

        void Post(Action a) { lock (mainQueue) mainQueue.Enqueue(a); }

        public void Connect(string willTopic, string willMessage)
        {
            if (running) return;
            running = true;
            var t = new Thread(() => Run(willTopic, willMessage));
            t.IsBackground = true;
            t.Start();
        }

        void Run(string willTopic, string willMessage)
        {
            string reason = "closed";
            try
            {
                tcp = new TcpClient();
                tcp.NoDelay = true;
                var ar = tcp.BeginConnect(host, port, null, null);
                if (!ar.AsyncWaitHandle.WaitOne(8000)) throw new IOException("connect timeout");
                tcp.EndConnect(ar);
                if (!running) throw new IOException("cancelled");
                stream = tcp.GetStream();

                var ping = new Thread(PingLoop);
                ping.IsBackground = true;
                ping.Start();

                SendConnect(willTopic, willMessage);
                ReadLoop();
            }
            catch (Exception e)
            {
                if (running) reason = e.Message;
            }
            finally
            {
                running = false;
                connected = false;
                try { if (stream != null) stream.Close(); } catch { }
                try { if (tcp != null) tcp.Close(); } catch { }
                Post(() => { if (OnDisconnected != null) OnDisconnected(reason); });
            }
        }

        void PingLoop()
        {
            while (running)
            {
                Thread.Sleep(1000);
                if (!connected) continue;
                long idle = DateTime.UtcNow.Ticks - Interlocked.Read(ref lastSendTicks);
                if (idle > keepAliveSec * TimeSpan.TicksPerSecond / 2)
                {
                    try { Send(new byte[] { 0xC0, 0x00 }); } catch { }
                }
            }
        }

        void SendConnect(string willTopic, string willMessage)
        {
            var v = new MemoryStream();
            WriteString(v, "MQTT");
            v.WriteByte(4); // protocol level 3.1.1
            int flags = 0x02; // clean session
            if (willTopic != null) flags |= 0x04; // Last Will, QoS 0, not retained
            v.WriteByte((byte)flags);
            v.WriteByte((byte)(keepAliveSec >> 8));
            v.WriteByte((byte)(keepAliveSec & 255));
            WriteString(v, clientId);
            if (willTopic != null)
            {
                WriteString(v, willTopic);
                WriteString(v, willMessage ?? "");
            }
            Send(Packet(0x10, v.ToArray()));
        }

        void ReadLoop()
        {
            while (running)
            {
                int first = stream.ReadByte();
                if (first < 0) throw new IOException("broker closed the connection");
                int mult = 1, len = 0, b;
                do
                {
                    b = stream.ReadByte();
                    if (b < 0) throw new IOException("broker closed the connection");
                    len += (b & 127) * mult;
                    mult *= 128;
                } while ((b & 128) != 0);

                byte[] body = new byte[len];
                int off = 0;
                while (off < len)
                {
                    int n = stream.Read(body, off, len - off);
                    if (n <= 0) throw new IOException("broker closed the connection");
                    off += n;
                }
                Handle(first, body);
            }
        }

        void Handle(int first, byte[] body)
        {
            int type = first >> 4;
            switch (type)
            {
                case 2: // CONNACK
                {
                    if (body.Length >= 2 && body[1] == 0)
                    {
                        connected = true;
                        Post(() => { if (OnConnected != null) OnConnected(); });
                    }
                    else
                    {
                        throw new IOException("CONNACK refused, code " + (body.Length > 1 ? body[1] : -1));
                    }
                    break;
                }
                case 3: // PUBLISH
                {
                    int qos = (first >> 1) & 3;
                    int tl = (body[0] << 8) | body[1];
                    int pos = 2;
                    string topic = Encoding.UTF8.GetString(body, pos, tl);
                    pos += tl;
                    if (qos > 0)
                    {
                        byte hi = body[pos], lo = body[pos + 1];
                        pos += 2;
                        if (qos == 1) Send(new byte[] { 0x40, 0x02, hi, lo }); // PUBACK
                    }
                    string payload = Encoding.UTF8.GetString(body, pos, body.Length - pos);
                    Post(() => { if (OnMessage != null) OnMessage(topic, payload); });
                    break;
                }
                // SUBACK (9) and PINGRESP (13) need no handling
            }
        }

        public bool Publish(string topic, string payload, bool retain = false)
        {
            if (!connected) return false;
            try
            {
                var v = new MemoryStream();
                WriteString(v, topic);
                byte[] p = Encoding.UTF8.GetBytes(payload ?? "");
                v.Write(p, 0, p.Length);
                Send(Packet(0x30 | (retain ? 1 : 0), v.ToArray()));
                return true;
            }
            catch (Exception) { return false; }
        }

        public bool Subscribe(string filter)
        {
            if (!connected) return false;
            try
            {
                var v = new MemoryStream();
                v.WriteByte(0); v.WriteByte(1); // packet id
                WriteString(v, filter);
                v.WriteByte(0); // QoS 0
                Send(Packet(0x82, v.ToArray()));
                return true;
            }
            catch (Exception) { return false; }
        }

        // Clean disconnect: the broker will NOT publish the Last Will
        public void Disconnect()
        {
            if (!running) return;
            try { if (connected) Send(new byte[] { 0xE0, 0x00 }); } catch { }
            running = false;
            connected = false;
            try { if (stream != null) stream.Close(); } catch { }
            try { if (tcp != null) tcp.Close(); } catch { }
        }

        void Send(byte[] data)
        {
            lock (writeLock)
            {
                if (stream == null) throw new IOException("not connected");
                stream.Write(data, 0, data.Length);
                stream.Flush();
                Interlocked.Exchange(ref lastSendTicks, DateTime.UtcNow.Ticks);
            }
        }

        static void WriteString(MemoryStream ms, string s)
        {
            byte[] b = Encoding.UTF8.GetBytes(s);
            ms.WriteByte((byte)(b.Length >> 8));
            ms.WriteByte((byte)(b.Length & 255));
            ms.Write(b, 0, b.Length);
        }

        static byte[] Packet(int header, byte[] body)
        {
            var ms = new MemoryStream();
            ms.WriteByte((byte)header);
            int x = body.Length;
            do
            {
                int d = x % 128;
                x /= 128;
                if (x > 0) d |= 128;
                ms.WriteByte((byte)d);
            } while (x > 0);
            ms.Write(body, 0, body.Length);
            return ms.ToArray();
        }
    }
}
