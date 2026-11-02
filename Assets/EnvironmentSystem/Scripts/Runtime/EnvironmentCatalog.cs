using UnityEngine;
namespace ProjectEnvironments
{
    [CreateAssetMenu(menuName = "Environments/Environment Catalog")]
    public sealed class EnvironmentCatalog : ScriptableObject
    {
        public string hubScenePath = "Assets/EnvironmentSystem/Scenes/EnvironmentHub.unity";
        public EnvironmentDefinition[] environments = new EnvironmentDefinition[0];
        public EnvironmentDefinition Find(string id)
        {
            foreach(var d in environments) if(d && d.id == id) return d;
            return null;
        }
    }
}
