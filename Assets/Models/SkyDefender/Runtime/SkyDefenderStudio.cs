using UnityEngine;
using UnityEngine.Rendering;

namespace SkyDefender
{
    [ExecuteAlways]
    public sealed class SkyDefenderStudio : MonoBehaviour
    {
        public RenderPipelineAsset studioPipeline;
        public RenderPipelineAsset originalPipeline;
        void OnEnable() { if (studioPipeline) QualitySettings.renderPipeline = studioPipeline; }
        void OnDisable() { if (QualitySettings.renderPipeline == studioPipeline) QualitySettings.renderPipeline = originalPipeline; }
    }
}
