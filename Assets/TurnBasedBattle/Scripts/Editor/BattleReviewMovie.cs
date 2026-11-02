using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Unity.Profiling;

namespace TurnBasedBattle.Editor
{
    public static class BattleReviewMovie
    {
        public static IEnumerator Capture(TurnBattleManager m)
        {
            var orbit=m.battleCamera.GetComponent<BattleOrbitCamera>();
            string folder=Path.GetFullPath("ReviewMedia/Frames");Directory.CreateDirectory(folder);
            int frame=0;const int fps=30;
            m.manualSimulation=true;orbit.manualSimulation=true;orbit.inputEnabled=false;
            try
            {
                foreach(var kind in new[]{BattleActionKind.Attack,BattleActionKind.Skill,BattleActionKind.EarthPrison})
                {
                    m.Restart(kind==BattleActionKind.EarthPrison?3:1);orbit.ResetView(true);
                    if(kind==BattleActionKind.Attack){orbit.Orbit(35,0);for(int i=0;i<100;i++)orbit.Step(1f/60);}
                    float length=kind==BattleActionKind.EarthPrison?m.tuning.ultimate.duration:kind==BattleActionKind.Skill?m.tuning.skill.duration:m.tuning.attack.duration;
                    int total=Mathf.CeilToInt((length+1)*fps);
                    for(int f=0;f<total;f++)
                    {
                        if(f==15){if(kind==BattleActionKind.EarthPrison)m.FillUltimate();m.Select(kind);m.Confirm();}
                        if(f>=15 && f<15+Mathf.CeilToInt(length*fps))m.AdvanceForTest(1f/fps);
                        // Keep the completed actor at home for a short inspection before the next segment.
                        if(kind==BattleActionKind.EarthPrison && f>fps*2 && f<fps*9)orbit.Orbit(360f/(7*fps),0);
                        orbit.Step(1f/fps);Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
                        var texture=ScreenCapture.CaptureScreenshotAsTexture();
                        File.WriteAllBytes(Path.Combine(folder,$"frame_{frame++:00000}.jpg"),texture.EncodeToJPG(85));Object.Destroy(texture);
                        yield return null;
                    }
                }
                File.WriteAllText(Path.Combine(folder,"manifest.json"),$"{{\"frames\":{frame},\"fps\":{fps},\"description\":\"Deterministic Unity Game View export. Silent. Not a real-time FPS benchmark.\"}}");
                Debug.Log("BATTLE_MOVIE_FRAMES_COMPLETE "+frame);
            }
            finally{m.Restart(1);m.manualSimulation=false;orbit.manualSimulation=false;orbit.inputEnabled=true;orbit.ResetView(true);}
        }

        public static IEnumerator Profile(TurnBattleManager m)
        {
            var report=new List<string>{"# Editor performance sample",System.DateTime.Now.ToString("s"),"Unity "+Application.unityVersion,"CPU: "+SystemInfo.processorType,"GPU: "+SystemInfo.graphicsDeviceName,"Game View: "+Screen.width+" × "+Screen.height,"VSync: "+QualitySettings.vSyncCount+"; targetFrameRate: "+Application.targetFrameRate,"","Samples include Editor/UI overhead. Not Android/AR device validation. No recording during measurement."};
            m.manualSimulation=false;
            using var batchCounter=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Batches Count");
            foreach(int count in new[]{1,3})
            {
                m.Restart(count);for(int i=0;i<30;i++)yield return null;
                var samples=new List<float>();var batches=new List<int>();
                for(int i=0;i<120;i++){yield return null;samples.Add(Time.unscaledDeltaTime*1000);batches.Add(batchCounter.Valid?(int)batchCounter.LastValue:-1);}
                samples.Sort();report.Add($"Idle 1v{count}: mean {samples.Average():F2} ms, p95 {samples[(int)(samples.Count*.95f)]:F2} ms, mean batches {batches.Average():F0}.");
            }
            m.Restart(3);m.FillUltimate();m.Select(BattleActionKind.EarthPrison);m.Confirm();
            var peak=new List<float>();var draws=new List<int>();
            while(m.Phase==BattlePhase.PlayerAction){yield return null;if(m.ActionTime>=5.1f && m.ActionTime<=8){peak.Add(Time.unscaledDeltaTime*1000);draws.Add(batchCounter.Valid?(int)batchCounter.LastValue:-1);}}
            if(peak.Count>0){peak.Sort();report.Add($"Earth Prison 1v3, action 5.1–8s: {peak.Count} samples, mean {peak.Average():F2} ms, p95 {peak[(int)(peak.Count*.95f)]:F2} ms, mean batches {draws.Average():F0}.");}
            File.WriteAllLines("Assets/TurnBasedBattle/Reports/Performance_20261029.md",report);m.Restart(1);Debug.Log("BATTLE_PROFILE_COMPLETE");
        }
    }
}
