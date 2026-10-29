using UnityEngine;
using UnityEngine.Rendering;

namespace HaoLing
{
    [ExecuteAlways]
    public sealed class HaoLingStudioLighting : MonoBehaviour
    {
        public RenderPipelineAsset studioPipeline;
        public RenderPipelineAsset originalPipeline;
        void OnEnable(){if(studioPipeline)QualitySettings.renderPipeline=studioPipeline;}
        void OnDisable(){if(QualitySettings.renderPipeline==studioPipeline)QualitySettings.renderPipeline=originalPipeline;}
    }
}
