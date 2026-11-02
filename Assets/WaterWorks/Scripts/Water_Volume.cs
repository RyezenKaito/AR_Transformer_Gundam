using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Unity 6 / Render Graph compatibility adapter. Original source is backed up
// in Backups/SunkenHarbor. Old blit materials are not bound to a modern pass.
public class Water_Volume : FullScreenPassRendererFeature
{
    [System.Serializable]
    public class _Settings
    {
        public Material material;
        public RenderPassEvent renderPass = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    public _Settings settings = new _Settings();
    public override void Create()
    {
        injectionPoint = InjectionPoint.BeforeRenderingPostProcessing;
        requirements = ScriptableRenderPassInput.Depth;
        fetchColorBuffer = true;
        passMaterial = settings.material && settings.material.shader &&
                       settings.material.shader.name == "SunkenHarbor/Underwater Optics"
            ? settings.material : null;
        base.Create();
    }
}
