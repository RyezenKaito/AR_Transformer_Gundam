using UnityEditor;
namespace GundamGame.CombatDemo.Editor
{
    // The percent-Energy test suite is archived with the pre-HUD implementation.
    public static class RougeCombatValidation
    {
        [MenuItem("Gundam/Strike Rouge/Validate Combat Demo")]
        public static void RequestValidation(){RougeHudValidation.Run();}
    }
}
