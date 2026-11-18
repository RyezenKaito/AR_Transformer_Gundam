using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GundamGame.UnicornPrototype.Editor
{
    [InitializeOnLoad]
    public static class UnicornMotionValidation
    {
        const string Running="UnicornMotionPrototype.TestRunning";
        static int step;
        static double wait, deadline;
        static UnicornMotionViewer viewer;
        static Transform head,foot;
        static Quaternion headStart;
        static Vector3 footStart;
        static readonly List<string> results=new List<string>();
        static UnicornMotionValidation()
        {
            EditorApplication.update+=Tick;
            EditorApplication.playModeStateChanged+=state=>
            {
                if(state==PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Running,false))
                {
                    SessionState.SetBool(Running,false);
                    var old=SessionState.GetString("UnicornMotionPrototype.OldStartScene","");
                    EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(old)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(old);
                }
            };
        }
        [MenuItem("Gundam/Unicorn/Validate Animation Prototype")]
        public static void Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play first.");
            SessionState.SetString("UnicornMotionPrototype.OldStartScene",AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(UnicornMotionAuthoring.ScenePath);
            SessionState.SetBool(Running,true);step=0;results.Clear();
            SetPreviewResolution();
            EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
            EditorApplication.isPlaying=true;
        }
        static void Check(bool pass,string text){results.Add((pass?"PASS: ":"FAIL: ")+text);if(!pass)throw new Exception(text);}
        static void Capture(string name) { ScreenCapture.CaptureScreenshot(Path.GetFullPath(UnicornMotionAuthoring.Report+"/"+name+".png")); }
        static void SetPreviewResolution()
        {
            const System.Reflection.BindingFlags flags=System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Static;
            var assembly=typeof(UnityEditor.Editor).Assembly;var type=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(ScriptableSingleton<>).MakeGenericType(type);var sizes=singleton.GetProperty("instance",flags).GetValue(null);
            var group=type.GetProperty("currentGroup",flags).GetValue(sizes);
            var labels=(string[])group.GetType().GetMethod("GetDisplayTexts",flags).Invoke(group,null);
            int index=Array.FindIndex(labels,s=>s.Contains("Unicorn Preview 1920x1080"));
            if(index<0)
            {
                var entry=Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"),flags,null,new object[]{Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),1920,1080,"Unicorn Preview 1920x1080"},null);
                group.GetType().GetMethod("AddCustomSize",flags).Invoke(group,new[]{entry});
                index=(int)group.GetType().GetMethod("GetTotalCount",flags).Invoke(group,null)-1;
            }
            var viewType=assembly.GetType("UnityEditor.GameView");var window=EditorWindow.GetWindow(viewType);
            viewType.GetProperty("selectedSizeIndex",flags).SetValue(window,index);window.Repaint();
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            try
            {
                double now=EditorApplication.timeSinceStartup;
                if(step==0)
                {
                    viewer=UnityEngine.Object.FindFirstObjectByType<UnicornMotionViewer>();if(!viewer)return;
                    Check(viewer.animator&&viewer.previewCamera,"Preview controls, Animator and camera loaded");
                    var transforms=viewer.animator.GetComponentsInChildren<Transform>();head=transforms.Single(t=>t.name=="Head");foot=transforms.Single(t=>t.name=="Foot.L");
                    Check(!viewer.animator.applyRootMotion,"Root motion disabled for turn-based placement");
                    var skins=viewer.animator.GetComponentsInChildren<SkinnedMeshRenderer>();
                    Check(skins.Length==16,"16 skinned meshes imported from 375 rigid source parts");
                    Check(skins.All(s=>s.sharedMesh&&s.bones.Length>0&&s.sharedMaterials.All(m=>m&&m.shader&&!m.shader.name.Contains("Error"))),"Meshes, bones and materials assigned");
                    footStart=foot.position;headStart=head.localRotation;wait=now+.73;deadline=now+12;step=1;
                }
                else if(now>deadline)throw new Exception("Animation validation timed out at step "+step);
                else if(now<wait)return;
                else if(step==1)
                {
                    Check(Quaternion.Angle(headStart,head.localRotation)>.05f,"Idle moves the head in Play Mode");
                    Check(Vector3.Distance(footStart,foot.position)<.001f,"Idle keeps feet planted");Capture("Unity_Idle");
                    viewer.PlayHit();wait=now+.16;step=2;
                }
                else if(step==2)
                {
                    Check(viewer.animator.GetCurrentAnimatorStateInfo(0).IsName("HitReact"),"Hit button trigger enters HitReact");
                    Check(Vector3.Distance(footStart,foot.position)<.001f,"Hit reaction keeps feet planted");
                    viewer.paused=true;Capture("Unity_HitReact");wait=now+.4;step=3;
                }
                else if(step==3){viewer.paused=false;wait=now+1.2;step=4;}
                else if(step==4)
                {
                    Check(viewer.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),"HitReact returns to Idle automatically");
                    viewer.PlayIdle();wait=now+.25;step=5;
                }
                else if(step==5)
                {
                    Check(viewer.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),"Idle button can reset playback");
                    Check(File.Exists(UnicornMotionAuthoring.Report+"/Unity_Idle.png")&&File.Exists(UnicornMotionAuthoring.Report+"/Unity_HitReact.png"),"Both Unity screenshots captured");
                    File.WriteAllLines(UnicornMotionAuthoring.Report+"/PlayModeValidation.txt",results);
                    EditorApplication.isPlaying=false;
                }
            }
            catch(Exception e)
            {
                results.Add("FAIL: "+e);Directory.CreateDirectory(UnicornMotionAuthoring.Report);File.WriteAllLines(UnicornMotionAuthoring.Report+"/PlayModeValidation.txt",results);Debug.LogException(e);EditorApplication.isPlaying=false;
            }
        }
    }
}
