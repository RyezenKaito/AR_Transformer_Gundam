using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using static BlueFlame.Editor.BFG;
namespace BlueFlame.Editor {
public static partial class BlueFlameBuilder {
        static void Lighting()
        {
            var original=QualitySettings.renderPipeline?QualitySettings.renderPipeline:GraphicsSettings.defaultRenderPipeline;
            if(original&& (original.name.StartsWith("BlueFlame")||original.name.StartsWith("HaoLing")))original=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Models/Unicorn Gundam/Demo/Generated/UnicornMobilePipeline.asset");
            var pipeline=UnityEngine.Object.Instantiate(original);pipeline.name="BlueFlame_Studio_Pipeline";
            var so=new SerializedObject(pipeline);
            foreach(string key in new[]{"m_MainLightShadowsSupported","m_SupportsHDR","m_SoftShadowsSupported","m_ReflectionProbeBlending"}){var p=so.FindProperty(key);if(p!=null)p.boolValue=true;}
            foreach(var pair in new Dictionary<string,int>{{"m_AdditionalLightsRenderingMode",1},{"m_AdditionalLightsPerObjectLimit",8},{"m_MSAA",4},{"m_MainLightShadowmapResolution",4096},{"m_ShadowCascadeCount",2}}){var p=so.FindProperty(pair.Key);if(p!=null)p.intValue=pair.Value;}
            var shadow=so.FindProperty("m_ShadowDistance");if(shadow!=null)shadow.floatValue=40;var scale=so.FindProperty("m_RenderScale");if(scale!=null)scale.floatValue=1.25f;so.ApplyModifiedPropertiesWithoutUndo();
            string pipelinePath=Root+"/Settings/BlueFlame_Studio_Pipeline.asset";Save(pipeline,pipelinePath);
            var savedPipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(pipelinePath);
            var rendererPath=Root+"/Settings/BlueFlame_Studio_Renderer.asset";
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if(!renderer)
            {
                renderer=ScriptableObject.CreateInstance<UniversalRendererData>();renderer.name="BlueFlame_Studio_Renderer";AssetDatabase.CreateAsset(renderer,rendererPath);
                var ao=ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();ao.name="Fine armor contact shadows";AssetDatabase.AddObjectToAsset(ao,renderer);renderer.rendererFeatures.Add(ao);
                var aoData=new SerializedObject(ao);aoData.FindProperty("m_Settings.Intensity").floatValue=1.1f;aoData.FindProperty("m_Settings.Radius").floatValue=.10f;aoData.FindProperty("m_Settings.DirectLightingStrength").floatValue=.18f;aoData.FindProperty("m_Settings.Samples").enumValueIndex=0;aoData.FindProperty("m_Settings.Downsample").boolValue=false;aoData.ApplyModifiedPropertiesWithoutUndo();ao.Create();EditorUtility.SetDirty(renderer);
            }
            renderer.postProcessData=AssetDatabase.LoadAssetAtPath<PostProcessData>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");EditorUtility.SetDirty(renderer);
            var pipelineData=new SerializedObject(savedPipeline);pipelineData.FindProperty("m_RendererDataList").GetArrayElementAtIndex(0).objectReferenceValue=renderer;pipelineData.ApplyModifiedPropertiesWithoutUndo();
            var controller=G("Studio render settings",null).gameObject.AddComponent<BlueFlame.BlueFlameStudio>();controller.originalPipeline=original;controller.studioPipeline=savedPipeline;QualitySettings.renderPipeline=controller.studioPipeline;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.22f,.27f,.34f);RenderSettings.ambientEquatorColor=new Color(.12f,.15f,.20f);RenderSettings.ambientGroundColor=new Color(.035f,.043f,.055f);RenderSettings.skybox=null;RenderSettings.fog=false;
            Reflection();
            var probe=G("Studio custom reflection probe",null).gameObject.AddComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Custom;probe.customBakedTexture=RenderSettings.customReflectionTexture;probe.size=V(100,100,100);probe.intensity=.75f;
            Light("Key | broad neutral softbox",V(32,-28,0),new Color(1,.96f,.92f),1.8f,true);
            Light("Fill | blue silver",V(18,47,0),new Color(.70f,.83f,1),1.1f,false);
            Light("Rim | pale gold",V(25,161,0),new Color(1,.89f,.73f),1.6f,false);
            var camera=G("BlueFlame Camera",null).gameObject.AddComponent<Camera>();camera.tag="MainCamera";camera.fieldOfView=32;camera.nearClipPlane=.03f;camera.farClipPlane=200;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.011f,.017f,.027f);camera.allowHDR=true;camera.gameObject.AddComponent<AudioListener>();
            var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var volume=G("Studio tone and bloom",null).gameObject.AddComponent<Volume>();volume.isGlobal=true;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);var adjust=profile.Add<ColorAdjustments>();adjust.postExposure.Override(.0f);adjust.contrast.Override(8);adjust.saturation.Override(-5);
            var bloom=profile.Add<Bloom>();bloom.intensity.Override(.10f);bloom.threshold.Override(1.5f);
            Save(profile,Root+"/Settings/AtelierVolume.asset");volume.sharedProfile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Root+"/Settings/AtelierVolume.asset");
            foreach(var component in volume.sharedProfile.components)if(!EditorUtility.IsPersistent(component))AssetDatabase.AddObjectToAsset(component,volume.sharedProfile);
            EditorUtility.SetDirty(volume.sharedProfile);
        }
        static void Save(UnityEngine.Object obj,string path)
        {
            obj.name=Path.GetFileNameWithoutExtension(path);
            var old=AssetDatabase.LoadMainAssetAtPath(path);if(old){EditorUtility.CopySerialized(obj,old);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(obj);}else AssetDatabase.CreateAsset(obj,path);
        }
        static void Light(string name,Vector3 rotation,Color color,float power,bool shadow)
        {
            var light=G(name,null,V(0,6),rotation).gameObject.AddComponent<Light>();light.type=LightType.Directional;light.color=color;light.intensity=power;light.shadows=shadow?LightShadows.Soft:LightShadows.None;light.shadowBias=.008f;light.shadowNormalBias=.015f;
        }
        static void Reflection()
        {
            int n=128;var cube=new Cubemap(n,TextureFormat.RGBAHalf,true);cube.name="BlueFlame_SoftboxReflection";
            for(int face=0;face<6;face++)
            {
                var pixels=new Color[n*n];
                for(int y=0;y<n;y++)for(int x=0;x<n;x++)
                {
                    float u=(x+.5f)*2/n-1,v=(y+.5f)*2/n-1;Vector3 d;
                    switch(face){case 0:d=V(1,-v,-u);break;case 1:d=V(-1,-v,u);break;case 2:d=V(u,1,v);break;case 3:d=V(u,-1,-v);break;case 4:d=V(u,-v,1);break;default:d=V(-u,-v,-1);break;}d.Normalize();
                    float key=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,V(-.7f,.8f,-.8f).normalized)),32)*5;
                    float rim=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,V(.8f,.5f,.5f).normalized)),26)*3;
                    float fill=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,V(.6f,.1f,-1).normalized)),14)*1.4f;
                    float b=.08f+Mathf.Max(d.y,0)*.20f;pixels[y*n+x]=new Color(b+key+rim+fill*.7f,b+key*.94f+rim*.85f+fill*.85f,b+key*.85f+rim*.65f+fill,1);
                }
                cube.SetPixels(pixels,(CubemapFace)face);
            }
            cube.Apply(true,false);Save(cube,Root+"/Settings/StudioReflection.asset");RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;RenderSettings.customReflectionTexture=AssetDatabase.LoadAssetAtPath<Cubemap>(Root+"/Settings/StudioReflection.asset");RenderSettings.reflectionIntensity=1.1f;
        }

}}
