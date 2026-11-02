from pathlib import Path
p=Path('Assets/TurnBasedBattle/Scripts/Editor')
(p/'TurnBattleVerification.cs').write_text('''using UnityEditor;
namespace TurnBasedBattle.Editor
{
    public static class TurnBattleVerification
    {
        [MenuItem("Turn Battle/Verify Running Battle")]
        public static void Run()=>BattleAcceptance.Run();
    }
}
''',encoding='utf-8')
f=p/'TurnBattleReview.cs';s=f.read_text(encoding='utf-8')
s=s.replace('orbit.inputEnabled=false;m.manualSimulation=true;', 'orbit.inputEnabled=false;orbit.manualSimulation=true;m.manualSimulation=true;')
s=s.replace('AdvanceTo(m,1.8f)','AdvanceTo(m,3.35f)').replace('AdvanceTo(m,3.75f)','AdvanceTo(m,5.30f)').replace('AdvanceTo(m,5.55f)','AdvanceTo(m,7.10f)').replace('AdvanceTo(m,6.8f)','AdvanceTo(m,8.35f)')
s=s.replace('AdvanceTo(m,1.05f)','AdvanceTo(m,1.60f)').replace('AdvanceTo(m,2.4f)','AdvanceTo(m,3.60f)').replace('m.AdvanceForTest(.21f);AdvanceTo(m,1.03f);m.SubmitDodge(1.03);AdvanceTo(m,1.18f)','m.AdvanceForTest(.26f);AdvanceTo(m,1.42f);m.SubmitDodge(1.42);AdvanceTo(m,1.58f)')
s=s.replace('orbit.inputEnabled=true;m.manualSimulation=false;', 'orbit.inputEnabled=true;orbit.manualSimulation=false;m.manualSimulation=false;')
s=s.replace('m.hud.ClearFloating();','m.hud.ClearFloating();var camera=m.battleCamera.GetComponent<BattleOrbitCamera>();if(camera)for(int i=0;i<90;i++)camera.Step(1f/60);')
s=s.replace('static IEnumerator Capture(TurnBattleManager m)', 'public static IEnumerator Capture(TurnBattleManager m)')
f.write_text(s,encoding='utf-8')
print('Review schedule matches current data; old verification menu forwards to current acceptance.')
