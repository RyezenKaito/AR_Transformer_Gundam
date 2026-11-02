using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TurnBasedBattle.Editor
{
    [InitializeOnLoad]
    public static class TurnBattleReview
    {
        const string Pending="TurnBattleReview.Pending";
        static TurnBattleReview(){EditorApplication.playModeStateChanged+=StateChanged;}
        [MenuItem("Turn Battle/Rebuild, Verify and Capture")]
        public static void Begin()
        {
            if(EditorApplication.isPlaying){Debug.LogWarning("Exit Play Mode before rebuilding.");return;}
            TurnBattleBuilder.Build();SessionState.SetBool(Pending,true);EditorApplication.isPlaying=true;
        }
        static void StateChanged(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending,false))EditorApplication.update+=WaitForStart;
        }
        static void WaitForStart()
        {
            var m=Object.FindAnyObjectByType<TurnBattleManager>();if(!EditorApplication.isPlaying){EditorApplication.update-=WaitForStart;return;}
            if(m==null || m.player.HP<=0)return;
            EditorApplication.update-=WaitForStart;SessionState.SetBool(Pending,false);
            try{TurnBattleVerification.Run();m.StartCoroutine(Capture(m));}
            catch(System.Exception error){File.WriteAllText("Assets/TurnBasedBattle/Reports/ReviewFailure.txt",error.ToString());Debug.LogException(error);m.manualSimulation=false;}
        }
        static void AdvanceTo(TurnBattleManager m,float time)
        {
            int actionId=m.ActionId;
            while(m.InAction && m.ActionId==actionId && m.ActionTime<time-.0001f)m.AdvanceForTest(Mathf.Min(1f/60,time-m.ActionTime));
            m.hud.ClearFloating();var camera=m.battleCamera.GetComponent<BattleOrbitCamera>();if(camera)for(int i=0;i<90;i++)camera.Step(1f/60);
        }
        static IEnumerator Shot(string name)
        {
            Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Assets/TurnBasedBattle/Reports/"+name+".png"));yield return null;yield return null;
        }
        public static IEnumerator Capture(TurnBattleManager m)
        {
            var orbit=m.battleCamera.GetComponent<BattleOrbitCamera>();orbit.inputEnabled=false;orbit.manualSimulation=true;m.manualSimulation=true;m.Restart(1);orbit.ResetView(true);
            yield return Shot("Review_01_Default");
            m.Restart(3);m.FillUltimate();m.Select(BattleActionKind.EarthPrison);m.Confirm();AdvanceTo(m,m.Current.vfxStart+1.8f);yield return Shot("Review_02_Charge");
            AdvanceTo(m,m.Current.vfxStart+3.75f);yield return Shot("Review_03_Dome");AdvanceTo(m,m.Current.vfxStart+5.55f);yield return Shot("Review_04_Cage");
            orbit.Orbit(90,0);for(int i=0;i<100;i++)orbit.Step(1f/60);yield return Shot("Review_05_CageSide");
            orbit.Orbit(90,0);for(int i=0;i<100;i++)orbit.Step(1f/60);yield return Shot("Review_06_CageReverse");
            orbit.ResetView(true);AdvanceTo(m,m.Current.vfxStart+6.8f);yield return Shot("Review_07_Dissolve");
            m.Restart(1);m.Select(BattleActionKind.Attack);m.Confirm();AdvanceTo(m,m.Current.HitTime(0));yield return Shot("Review_08_Melee");
            AdvanceTo(m,m.Current.returnEnd+.2f);yield return Shot("Review_09_Return");AdvanceTo(m,m.Current.duration+.001f);AdvanceTo(m,m.Current.hits[0].open+.07f);m.SubmitDodge(m.ActionTime);AdvanceTo(m,m.Current.hits[0].time-.02f);yield return Shot("Review_10_Dodge");
            m.Restart(1);m.Select(BattleActionKind.Attack);m.Confirm();AdvanceTo(m,m.Current.HitTime(0)-.25f);orbit.Orbit(60,0);for(int i=0;i<100;i++)orbit.Step(1f/60);yield return Shot("Review_11_BeamSaber");
            m.Restart(1);orbit.ResetView(true);m.Select(BattleActionKind.Skill);m.Confirm();AdvanceTo(m,1.68f);yield return Shot("Review_12_RangedStationary");
            m.Restart(1);orbit.ResetView(true);orbit.Zoom(-2.3f);orbit.Orbit(35,0);for(int i=0;i<100;i++)orbit.Step(1f/60);yield return Shot("Review_13_ZoomOrbit");
            m.Restart(1);orbit.ResetView(true);m.Select(BattleActionKind.Attack);m.Confirm();AdvanceTo(m,2.15f);orbit.Orbit(65,0);for(int i=0;i<100;i++)orbit.Step(1f/60);yield return Shot("Review_14_Run");
            m.Restart(1);orbit.ResetView(true);m.FillUltimate();m.Select(BattleActionKind.EarthPrison);m.Confirm();AdvanceTo(m,2.25f);orbit.Orbit(65,0);for(int i=0;i<100;i++)orbit.Step(1f/60);yield return Shot("Review_15_Flight");
            File.WriteAllText("Assets/TurnBasedBattle/Reports/ReviewCapture.md","# Visual capture run\n\n"+System.DateTime.Now.ToString("s")+"\n\n10 Game View screenshots captured: rear-high home, charge, dome, cage at three yaw angles, dissolve, melee contact, return, and dodge.\n\nPlayer renderers: "+m.player.bodyRenderers.Length+". Rubble renderers: "+m.earthPrison.rubbleRenderers.Length+".\n\nThese are Editor captures, not AR device validation. Refer to Verification.md for assertions.\n");
            m.Restart(1);orbit.ResetView(true);orbit.inputEnabled=true;orbit.manualSimulation=false;m.manualSimulation=false;
            Debug.Log("TURN_BATTLE_REVIEW_COMPLETE — ready to play");
        }
    }
}
