using UnityEngine;
using UnityEngine.Rendering;

namespace BlueFlame
{
    [ExecuteAlways]
    public sealed class BlueFlameStudio : MonoBehaviour
    {
        public RenderPipelineAsset studioPipeline;
        public RenderPipelineAsset originalPipeline;
        void OnEnable() { if (studioPipeline) QualitySettings.renderPipeline = studioPipeline; }
        void OnDisable() { if (QualitySettings.renderPipeline == studioPipeline) QualitySettings.renderPipeline = originalPipeline; }
    }
}

