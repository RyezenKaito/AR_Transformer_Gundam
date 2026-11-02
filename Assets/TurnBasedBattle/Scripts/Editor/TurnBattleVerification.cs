using UnityEditor;
namespace TurnBasedBattle.Editor
{
    public static class TurnBattleVerification
    {
        [MenuItem("Turn Battle/Verify Running Battle")]
        public static void Run()=>BattleAcceptance.Run();
    }
}
