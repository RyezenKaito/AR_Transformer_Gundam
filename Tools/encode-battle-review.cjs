// Encode exported Unity Game View frames locally with Edge's native MediaRecorder.
const fs=require('fs'),path=require('path'),http=require('http');
const {chromium}=require('C:/Users/LENOVO/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const root=path.resolve('ReviewMedia/Frames'),out=path.resolve(process.argv[2]||'ReviewMedia/CombatReview_20261029.webm');
const meta=JSON.parse(fs.readFileSync(path.join(root,'manifest.json'),'utf8'));
const server=http.createServer((req,res)=>{
 if(req.url==='/'){res.setHeader('Content-Type','text/html');res.end('<canvas id="canvas"></canvas>');return;}
 const match=/^\/frame_(\d{5})\.jpg$/.exec(req.url);if(!match){res.writeHead(404);res.end();return;}
 const file=path.join(root,match[0].slice(1));if(!fs.existsSync(file)){res.writeHead(404);res.end();return;}
 res.setHeader('Content-Type','image/jpeg');fs.createReadStream(file).pipe(res);
});
(async()=>{
 await new Promise(r=>server.listen(0,'127.0.0.1',r));
 const browser=await chromium.launch({channel:'msedge',headless:true,args:['--disable-background-timer-throttling','--disable-renderer-backgrounding']});
 try{
  const page=await browser.newPage();await page.goto(`http://127.0.0.1:${server.address().port}`);
  const bytes=await page.evaluate(async meta=>{
   const c=document.querySelector('canvas'),ctx=c.getContext('2d');
   const get=async i=>createImageBitmap(await(await fetch(`/frame_${String(i).padStart(5,'0')}.jpg`)).blob());
   let first=await get(0);c.width=first.width;c.height=first.height;ctx.drawImage(first,0,0);first.close();
   const stream=c.captureStream(0),track=stream.getVideoTracks()[0],chunks=[];
   const recorder=new MediaRecorder(stream,{mimeType:'video/webm;codecs=vp9',videoBitsPerSecond:6000000});
   recorder.ondataavailable=e=>chunks.push(e.data);const done=new Promise(r=>recorder.onstop=r);recorder.start();
   for(let i=0;i<meta.frames;i++){const start=performance.now(),img=await get(i);ctx.drawImage(img,0,0);img.close();track.requestFrame();await new Promise(r=>setTimeout(r,Math.max(1,1000/meta.fps-(performance.now()-start))));}
   recorder.stop();await done;return Array.from(new Uint8Array(await new Blob(chunks,{type:'video/webm'}).arrayBuffer()));
  },meta);
  fs.writeFileSync(out,Buffer.from(bytes));
  // Decode the resulting local video to verify it is usable, not just a nonempty file.
  const verify=await browser.newPage();await verify.setContent('<video muted></video>');
  const data=fs.readFileSync(out).toString('base64');
  const result=await verify.evaluate(async data=>{let v=document.querySelector('video');v.src='data:video/webm;base64,'+data;await new Promise((ok,bad)=>{v.onloadeddata=ok;v.onerror=bad;});return {width:v.videoWidth,height:v.videoHeight,readyState:v.readyState};},data);
  console.log(JSON.stringify({file:out,bytes:bytes.length,frames:meta.frames,...result}));
 }finally{await browser.close();server.close();}
})().catch(e=>{console.error(e);server.close();process.exitCode=1;});
