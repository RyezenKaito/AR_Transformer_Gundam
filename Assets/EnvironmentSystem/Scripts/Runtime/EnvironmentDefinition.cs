using UnityEngine;

namespace ProjectEnvironments
{
    [CreateAssetMenu(menuName = "Environments/Environment Definition")]
    public sealed class EnvironmentDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Texture2D thumbnail;
        public Color accent = new Color(.15f,.7f,.8f);
        public bool available = true;
        [Tooltip("Optional variant in the existing battle scenery switcher. -1 for standalone scenes.")]
        public int battleVariant = -1;
        [SerializeField] string scenePath;
        public string ScenePath => scenePath;
#if UNITY_EDITOR
        public UnityEditor.SceneAsset scene;
        public void SyncScene() { scenePath = scene ? UnityEditor.AssetDatabase.GetAssetPath(scene) : ""; }
        void OnValidate() { SyncScene(); }
#endif
    }
}
