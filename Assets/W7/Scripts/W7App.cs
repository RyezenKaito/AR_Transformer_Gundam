using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace W7
{
    public class W7App : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] Text countText;
        [SerializeField] Button submitButton;
        [SerializeField] Button pictureButton;
        [SerializeField] Text statusText; // optional, small debug line (safe to delete)

        [Header("MQTT broker (cloud / server)")]
        [SerializeField] string brokerHost = "broker.hivemq.com";
        [SerializeField] int brokerPort = 1883;
        [Tooltip("Change this to something unique so other people on the public broker do not mix with you")]
        [SerializeField] string topicRoot = "ntdev-w7";

        [Header("Messages")]
        [SerializeField] string helloMessage = "Hello, I'am here!";
        [SerializeField] string lastWillMessage = "Goodbye, I have left the application!";

        [Serializable]
        class ClickEvent
        {
            public string clientId;
            public string source;
            public int clicks;
            public string time;
        }

        MiniMqttClient client;
        string clientId;
        int clicks;
        readonly List<string> log = new List<string>();

        string PresenceTopic(string id) { return topicRoot + "/presence/" + id; }
        string EventTopic { get { return topicRoot + "/events/" + clientId; } }

        void Awake()
        {
            Application.runInBackground = true;
        }

        void Start()
        {
            clientId = "unity-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            countText.text = clicks.ToString();
            submitButton.onClick.AddListener(OnSubmit);
            pictureButton.onClick.AddListener(OnPictureClick);
            ConnectToBroker();
        }

        void Update()
        {
            if (client != null) client.Pump();
        }

        // 1. Joining the app -> say hello to everyone else
        // 2. Last Will -> broker tells everyone if we disappear without a clean disconnect
        void ConnectToBroker()
        {
            AddLog("Connecting to " + brokerHost + "...");
            client = new MiniMqttClient(brokerHost, brokerPort, clientId);
            client.OnConnected += HandleConnected;
            client.OnDisconnected += HandleDisconnected;
            client.OnMessage += HandleMessage;
            client.Connect(PresenceTopic(clientId), lastWillMessage);
        }

        void HandleConnected()
        {
            client.Subscribe(topicRoot + "/presence/+");
            client.Publish(PresenceTopic(clientId), helloMessage);
            AddLog("Connected as " + clientId + ". Hello sent.");
        }

        void HandleDisconnected(string reason)
        {
            AddLog("Disconnected: " + reason);
        }

        void HandleMessage(string topic, string payload)
        {
            string sender = topic.Substring(topic.LastIndexOf('/') + 1);
            if (sender == clientId) return; // ignore our own hello
            AddLog(sender + ": " + payload);
        }

        // 3. User events
        void OnSubmit()
        {
            clicks++;
            countText.text = clicks.ToString();
            SendClickEvent("submit");
        }

        void OnPictureClick()
        {
            SendClickEvent("picture");
        }

        void SendClickEvent(string source)
        {
            var e = new ClickEvent
            {
                clientId = clientId,
                source = source,
                clicks = clicks,
                time = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture)
            };
            string json = JsonUtility.ToJson(e);
            bool ok = client != null && client.Publish(EventTopic, json);
            AddLog((ok ? "sent " : "NOT sent " ) + json);
        }

        void OnApplicationQuit()
        {
            if (client != null && client.IsConnected)
            {
                // A clean DISCONNECT does not trigger the broker Last Will, so say goodbye ourselves.
                client.Publish(PresenceTopic(clientId), lastWillMessage);
                client.Disconnect();
            }
        }

        void OnDestroy()
        {
            if (client != null) client.Disconnect();
        }

        void AddLog(string line)
        {
            Debug.Log("[W7] " + line);
            log.Add(line);
            if (log.Count > 3) log.RemoveAt(0);
            if (statusText != null) statusText.text = string.Join("\n", log.ToArray());
        }
    }
}
