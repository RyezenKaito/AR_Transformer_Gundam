using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace MechaDemo
{
 public sealed class MechaPlaytest : MonoBehaviour
 {
  public string outputPath="C:/Users/LENOVO/Documents/Codex/2026-10-26/kie/work/PlayModeResults.md";
  public bool Completed {get;private set;}
  public int Passed {get;private set;} public int Failed {get;private set;}
  MechaPlayer player;MechaHealth dummy;readonly List<string> lines=new List<string>();
  int originalCap,originalVsync;
  public void Run(){StartCoroutine(Suite());}
  void Check(string name,bool good,string detail)
  {
   if(good)Passed++;else Failed++;lines.Add("- "+(good?"PASS":"FAIL")+" **"+name+"**: "+detail);Write();
  }
  void Write(){File.WriteAllText(outputPath,"# Unity Play Mode verification\n\n"+Passed+" pass, "+Failed+" fail. Completed: "+Completed+"\n\n"+string.Join("\n",lines));}
  IEnumerator Suite()
  {
   player=UnityEngine.Object.FindAnyObjectByType<MechaPlayer>();
   foreach(var h in MechaHealth.Active)if(h.trainingDummy){dummy=h;break;}
   if(player==null||dummy==null){Check("Scene references",false,"Missing player or dummy");Completed=true;Write();yield break;}
   originalCap=Application.targetFrameRate;originalVsync=QualitySettings.vSyncCount;QualitySettings.vSyncCount=0;
   foreach(var e in UnityEngine.Object.FindObjectsByType<MechaEnemy>(FindObjectsSortMode.None))e.aggressive=false;
   yield return new WaitForSeconds(.25f);Setup(2);
   yield return new WaitForSeconds(.15f);Check("Grounded",player.Grounded,"Grounded="+player.Grounded+", y="+player.transform.position.y.ToString("F3"));
   foreach(string id in new[]{"Attack1","Attack2","Attack3","Q","E","R","Fire"})
   {
    Setup(id=="Q"?6:id=="E"?6:id=="R"?8:2);
    yield return new WaitForSeconds(.10f);
    float hp=dummy.HP;int hits=dummy.receivedHits;float energy=player.Energy;
    bool started=player.TestStartAction(id);float began=Time.realtimeSinceStartup;int captureIndex=0;float[] captureTimes={.9f,1.5f,2.03f,3.3f};
    while(player.ActionName.Length>0&&Time.realtimeSinceStartup-began<7){if(id=="R"&&captureIndex<captureTimes.Length&&player.ActionTime>=captureTimes[captureIndex]){Capture(player.viewCamera,"C:/Users/LENOVO/Documents/Codex/2026-10-26/kie/work/R_runtime_"+captureIndex+".png");captureIndex++;}yield return null;}
    yield return new WaitForSeconds(.10f);
    Check(id+" activation/exit",started&&player.ActionName.Length==0,"started="+started+", action="+player.ActionName+", elapsed="+(Time.realtimeSinceStartup-began).ToString("F3"));
    Check(id+" confirmed hit",dummy.receivedHits-hits==1,"hits="+(dummy.receivedHits-hits)+", damage="+(hp-dummy.HP).ToString("F1")+", energy="+player.Energy.ToString("F1"));
   }
   Setup(6);yield return new WaitForSeconds(.10f);player.TestStartAction("E");yield return new WaitForSeconds(.20f);bool cancel=player.TestStartAction("Dash");yield return new WaitForSeconds(.30f);
   Check("E precommit dash cancel",cancel&&Mathf.Abs(player.Energy-82)<1&&player.ECooldown<=1,"cancel="+cancel+", energy="+player.Energy.ToString("F1")+", E CD="+player.ECooldown.ToString("F2"));
   foreach(int fps in new[]{30,60,120})
   {
    Application.targetFrameRate=fps;Setup(12);yield return new WaitForSeconds(.15f);Vector3 start=player.transform.position;player.TestStartAction("Dash");float began=Time.realtimeSinceStartup;
    while(player.ActionName.Length>0&&Time.realtimeSinceStartup-began<2)yield return null;
    Vector3 delta=player.transform.position-start;delta.y=0;
    Check("Dash range at "+fps+" cap",Mathf.Abs(delta.magnitude-4.6f)<.23f,"range="+delta.magnitude.ToString("F3")+"m, realtime="+(Time.realtimeSinceStartup-began).ToString("F3")+"s");
   }
   Application.targetFrameRate=60;Setup(12);yield return new WaitForSeconds(.15f);
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Automated collision fixture";wall.layer=8;wall.transform.position=new Vector3(0,1.5f,2);wall.transform.localScale=new Vector3(4,3,.15f);Physics.SyncTransforms();player.TestStartAction("Dash");yield return new WaitForSeconds(.4f);
   Check("Dash wall obstruction",player.transform.position.z<1.65f,"Stopped z="+player.transform.position.z.ToString("F3"));Destroy(wall);yield return null;
   Setup(8);yield return new WaitForSeconds(.1f);player.TestStartAction("R");yield return new WaitForSeconds(.7f);float rt=player.ActionTime,energyR=player.Energy;player.TogglePause();yield return new WaitForSecondsRealtime(.2f);
   Check("Pause freezes action/energy",Mathf.Abs(player.ActionTime-rt)<.002f&&Mathf.Abs(player.Energy-energyR)<.002f,"Rtime="+player.ActionTime.ToString("F3")+", energy="+player.Energy.ToString("F2"));player.TogglePause();
   dummy.TakeDamage(999999,dummy.AimPosition,Vector3.forward);yield return new WaitForSeconds(.15f);Check("R target lost before approach",player.ActionName!="R","action="+player.ActionName);
   Setup(8);yield return new WaitForSeconds(.1f);player.TestStartAction("R");yield return new WaitForSeconds(.7f);player.GetComponent<MechaHealth>().TakeDamage(999999,player.transform.position,Vector3.back);yield return new WaitForSeconds(.2f);
   Check("Death interrupts cinematic",player.IsDead&&player.ActionName=="Death"&&player.animator.GetLayerWeight(1)<.01f,"dead="+player.IsDead+", action="+player.ActionName);
   Setup(8);yield return new WaitForSeconds(.15f);Check("Reset restores player",!player.IsDead&&player.Energy>99&&player.RCooldown==0&&player.Target!=null,"HP="+player.HP+", energy="+player.Energy+", R CD="+player.RCooldown);
   bool jump=player.TestStartAction("Jump");float top=0;float jumpBegin=Time.time;
   while(Time.time-jumpBegin<2.3f){top=Mathf.Max(top,player.transform.position.y);yield return null;}
   Check("Jump and landing",jump&&top>1.5f&&player.Grounded,"accepted="+jump+", apex="+top.ToString("F3")+", grounded="+player.Grounded);
   Setup(6);yield return new WaitForSeconds(.15f);
   Application.targetFrameRate=originalCap;QualitySettings.vSyncCount=originalVsync;Completed=true;Write();Debug.Log("MECHA_PLAYTEST_COMPLETE "+Passed+" PASS / "+Failed+" FAIL");
  }
  static void Capture(Camera camera,string path)
  {
   var old=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,800,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,800),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());camera.targetTexture=old;RenderTexture.active=active;rt.Release();Destroy(image);Destroy(rt);
  }
  void Setup(float distance)
  {
   if(player.IsPaused)player.TogglePause();player.ResetDemo();
   var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);cc.enabled=true;
   var dc=dummy.GetComponent<CharacterController>();if(dc!=null)dc.enabled=false;dummy.transform.position=Vector3.forward*distance;if(dc!=null)dc.enabled=true;
   dummy.ResetHealth();player.LockTarget(dummy);Physics.SyncTransforms();
   var camera=player.viewCamera!=null?player.viewCamera.GetComponent<MechaCamera>():null;if(camera!=null)camera.Snap();
  }
 }
}

