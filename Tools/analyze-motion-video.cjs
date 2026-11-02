const fs=require('fs'),http=require('http'),path=require('path');
const {chromium}=require('C:/Users/LENOVO/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const source=process.env.MOTION_VIDEO||'D:/Downloads/Gundam_AR_Test - Untitled - Android - Unity 6.5 (6000.5.8f1)_ _DX12_ 2026-10-28 23-43-56.mp4';
const output=path.resolve(process.env.MOTION_OUTPUT||'Prompts/MotionReference');fs.mkdirSync(output,{recursive:true});
const size=fs.statSync(source).size;
const server=http.createServer((req,res)=>{
 if(req.url!='/reference.mp4'){res.writeHead(404);res.end();return;}
 const range=/bytes=(\d+)-(\d*)/.exec(req.headers.range||'');
 const start=range?+range[1]:0,end=range&&range[2]?Math.min(+range[2],size-1):size-1;
 res.writeHead(range?206:200,{'Content-Type':'video/mp4','Accept-Ranges':'bytes','Access-Control-Allow-Origin':'*','Content-Length':end-start+1,...(range?{'Content-Range':`bytes ${start}-${end}/${size}`}:{})});
 fs.createReadStream(source,{start,end}).pipe(res);
});
(async()=>{
 await new Promise(r=>server.listen(0,'127.0.0.1',r));
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage();
  await page.setContent(`<video id="v" crossorigin="anonymous" muted src="http://127.0.0.1:${server.address().port}/reference.mp4"></video><canvas id="c"></canvas>`);
  await page.waitForFunction(()=>document.querySelector('video').readyState>=2,{},{timeout:60000});
  const meta=await page.evaluate(()=>{const v=document.querySelector('video');return {duration:v.duration,width:v.videoWidth,height:v.videoHeight};});
  fs.writeFileSync(path.join(output,'metadata.json'),JSON.stringify(meta,null,2));console.log(meta);
  const times=process.argv.slice(2).length?process.argv.slice(2).map(Number):Array.from({length:16},(_,i)=>Math.max(.05,i*(meta.duration-.2)/15));
  for(const time of times){
   const data=await page.evaluate(async t=>{const v=document.querySelector('video'),c=document.querySelector('canvas');await new Promise(r=>{v.onseeked=r;v.currentTime=t;});c.width=v.videoWidth;c.height=v.videoHeight;c.getContext('2d').drawImage(v,0,0);return c.toDataURL('image/jpeg',.9).split(',')[1];},time);
   fs.writeFileSync(path.join(output,`frame_${time.toFixed(3)}.jpg`),Buffer.from(data,'base64'));
  }
  console.log('Extracted '+times.length+' frames to '+output);
 }finally{await browser.close();server.close();}
})().catch(e=>{console.error(e);server.close();process.exitCode=1;});
