using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.ARCore;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;

namespace UnicornDemo.Editor
{
    public static class UnicornDemoBuilder
    {
        public const string Root = "Assets/Models/Unicorn Gundam/Demo";
        public const string ScenePath = Root + "/Scenes/UnicornPhoneDemo.unity";
        public const string PrefabPath = Root + "/Prefabs/UnicornAnimated.prefab";
        static string ProjectRoot => Path.GetDirectoryName(Application.dataPath);
        static string ReportPath => Path.Combine(ProjectRoot,"Builds","UnicornDemo","build-status.json");

        [MenuItem("Unicorn Demo/1 Build demo scene and animated prefab")]
        public static void Prepare()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play mode before setup.");
            if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save the current scene before setup.");
            foreach(var p in new[]{"Generated","Materials","Prefabs","Scenes"})Directory.CreateDirectory(Root+"/"+p);
            AssetDatabase.Refresh();
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Unicorn Gundam/Unicorn Gundam.prefab");
            if(!source)throw new InvalidOperationException("Original Unicorn prefab not found.");
            var original=PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(source));
            GameObject baked=null;
            try
            {
                var renderers=original.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.enabled&&r.gameObject.activeSelf).ToArray();
                var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
                float scale=2f/bounds.size.y;
                Vector3 foot=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
                Func<Vector3,Vector3> normalized=p=>(p-foot)*scale;
                Func<string,Vector3,Vector3> find=(pattern,fallback)=>{
                    var r=renderers.FirstOrDefault(x=>x.name.ToLowerInvariant().Contains(pattern));
                    return r?normalized(r.bounds.center):fallback;
                };
                var pivot=new Dictionary<string,Vector3>{
                    {"Body",Vector3.zero}, {"Head",new Vector3(0,1.64f,.015f)}, {"Backpack",new Vector3(0,1.4f,-.17f)},
                    {"ArmL",find("l_shoulder_rotor",new Vector3(-.3f,1.58f,0))}, {"ArmR",find("r_shoulder_rotor",new Vector3(.3f,1.58f,0))},
                    {"ForearmL",find("l_arm_assy_g_whole_arm_g_elbow_fulcrum",new Vector3(-.35f,1.4f,-.04f))},
                    {"ForearmR",find("r_arm_assy_g_whole_arm_g_elbow_fulcrum",new Vector3(.35f,1.4f,-.04f))},
                    {"LegL",find("waist_ball_joint_waist_ball_joint_g_l_leg",new Vector3(-.12f,1.2f,0))},
                    {"LegR",find("waist_ball_joint_waist_ball_joint_g_r_leg",new Vector3(.12f,1.2f,0))},
                    {"ShinL",find("l_leg_assy_g_leg_assy_g_lower_leg_assy_g_knee_frame_part_a",new Vector3(-.18f,.75f,.09f))},
                    {"ShinR",find("r_leg_assy_g_leg_assy_g_lower_leg_assy_g_knee_frame_part_a",new Vector3(.18f,.75f,.09f))}
                };
                baked=new GameObject("Unicorn Animated Demo");
                var pose=new GameObject("Pose").transform;pose.SetParent(baked.transform,false);
                var bones=new Dictionary<string,Transform>();
                foreach(var key in pivot.Keys)
                {
                    var t=new GameObject(key).transform;t.SetParent(pose,false);t.localPosition=pivot[key];bones[key]=t;
                }
                foreach(var pair in new[]{new[]{"ForearmL","ArmL"},new[]{"ForearmR","ArmR"},new[]{"ShinL","LegL"},new[]{"ShinR","LegR"}})
                {bones[pair[0]].SetParent(bones[pair[1]],false);bones[pair[0]].localPosition=pivot[pair[0]]-pivot[pair[1]];}
                var materialMap=new Dictionary<string,Material>();
                var groups=new Dictionary<string,Dictionary<Material,List<CombineInstance>>>();
                foreach(var key in pivot.Keys)groups[key]=new Dictionary<Material,List<CombineInstance>>();
                foreach(var r in renderers)
                {
                    var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;
                    string group=Classify(r.name,normalized(r.bounds.center));
                    var mesh=mf.sharedMesh;
                    for(int sub=0;sub<mesh.subMeshCount;sub++)
                    {
                        var mat=r.sharedMaterials[Mathf.Min(sub,r.sharedMaterials.Length-1)];
                        string key=MaterialKey(mat);
                        if(!materialMap.TryGetValue(key,out var canonical))
                        {
                            canonical=new Material(mat);canonical.name="Unicorn material "+materialMap.Count.ToString("D2");canonical.enableInstancing=true;
                            if(canonical.HasProperty("_Smoothness"))canonical.SetFloat("_Smoothness",Mathf.Min(canonical.GetFloat("_Smoothness"),.4f));
                            string path=Root+"/Materials/Unicorn_"+materialMap.Count.ToString("D2")+".mat";
                            SaveAsset(canonical,path);canonical=AssetDatabase.LoadAssetAtPath<Material>(path);materialMap[key]=canonical;
                        }
                        if(!groups[group].ContainsKey(canonical))groups[group][canonical]=new List<CombineInstance>();
                        Matrix4x4 matrix=Matrix4x4.Translate(-pivot[group])*Matrix4x4.Scale(Vector3.one*scale)*Matrix4x4.Translate(-foot)*r.transform.localToWorldMatrix;
                        groups[group][canonical].Add(new CombineInstance{mesh=mesh,subMeshIndex=sub,transform=matrix});
                    }
                }
                int draws=0;
                foreach(var entry in groups)
                {
                    var temporary=new List<Mesh>();var combines=new List<CombineInstance>();var mats=new List<Material>();
                    foreach(var matGroup in entry.Value)
                    {
                        var merged=new Mesh{indexFormat=IndexFormat.UInt32};
                        merged.CombineMeshes(matGroup.Value.ToArray(),true,true,false);temporary.Add(merged);
                        combines.Add(new CombineInstance{mesh=merged,transform=Matrix4x4.identity});mats.Add(matGroup.Key);
                    }
                    if(combines.Count==0)continue;
                    var final=new Mesh{indexFormat=IndexFormat.UInt32,name="Unicorn_"+entry.Key};
                    final.CombineMeshes(combines.ToArray(),false,false,false);final.RecalculateBounds();
                    string path=Root+"/Generated/"+final.name+".asset";SaveAsset(final,path);
                    var go=bones[entry.Key].gameObject;
                    go.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterials=mats.ToArray();renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                    var col=go.AddComponent<BoxCollider>();var savedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);col.center=savedMesh.bounds.center;col.size=savedMesh.bounds.size;
                    draws+=mats.Count;
                    foreach(var temp in temporary)UnityEngine.Object.DestroyImmediate(temp);
                }
                var motion=baked.AddComponent<UnicornMotion>();motion.poseRoot=pose;motion.head=bones["Head"];motion.backpack=bones["Backpack"];
                motion.armL=bones["ArmL"];motion.armR=bones["ArmR"];motion.forearmL=bones["ForearmL"];motion.forearmR=bones["ForearmR"];
                motion.legL=bones["LegL"];motion.legR=bones["LegR"];motion.shinL=bones["ShinL"];motion.shinR=bones["ShinR"];
                var animated=PrefabUtility.SaveAsPrefabAsset(baked,PrefabPath);
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var root=new GameObject("Unicorn Demo Controller");var app=root.AddComponent<UnicornDemoController>();app.unicornPrefab=animated;
                var fx=new Material(Shader.Find("Universal Render Pipeline/Unlit"));fx.color=new Color(.1f,.8f,1);
                SaveAsset(fx,Root+"/Materials/DemoEffect.mat");app.effectTemplate=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/DemoEffect.mat");
                var ground=new Material(Shader.Find("Universal Render Pipeline/Lit"));ground.color=new Color(.05f,.09f,.13f);
                SaveAsset(ground,Root+"/Materials/DemoFloor.mat");app.floorTemplate=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/DemoFloor.mat");
                EditorSceneManager.SaveScene(scene,ScenePath);
                ConfigureAndroid();RemoveDemoShaderOverrides();
                var old=EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath).ToArray();
                EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)}.Concat(old.Select(s=>new EditorBuildSettingsScene(s.path,false))).ToArray();
                AssetDatabase.SaveAssets();
                Debug.Log("UNICORN_DEMO_READY | Source renderers="+renderers.Length+" Optimized renderers="+groups.Count+" Materials="+materialMap.Count+" Material passes="+draws+" Height=2m | "+ScenePath);
            }
            finally{if(baked)UnityEngine.Object.DestroyImmediate(baked);PrefabUtility.UnloadPrefabContents(original);}
        }
        static string Classify(string name,Vector3 center)
        {
            string n=name.ToLowerInvariant();
            if(n.Contains("head_assy")||n.Contains("upper_helm")||n.Contains("v_fin"))return "Head";
            if(n.Contains("back_pack")||n.Contains("backpack"))return "Backpack";
            bool left=center.x<0;
            if(n.Contains("arm_assy")||n.Contains("shoulder_armor")||n.Contains("r_shoulder_assy")||n.Contains("g_shoulder")||n.Contains("g_ball_joint"))
            {
                bool lower=n.Contains("lower_arm")||n.Contains("palm")||n.Contains("finger")||n.Contains("hand_pad")||n.Contains("elbow");
                return (lower?"Forearm":"Arm")+(left?"L":"R");
            }
            if(n.Contains("leg_assy")||n.Contains("feet_assy"))
            {
                bool lower=n.Contains("lower_leg")||n.Contains("ankle")||n.Contains("feet")||n.Contains("knee");
                return (lower?"Shin":"Leg")+(left?"L":"R");
            }
            return "Body";
        }
        static string MaterialKey(Material m)
        {
            var key=new System.Text.StringBuilder(m.shader.name);
            for(int i=0;i<m.shader.GetPropertyCount();i++)
            {
                string n=m.shader.GetPropertyName(i);key.Append('|').Append(n).Append('=');
                switch(m.shader.GetPropertyType(i))
                {
                    case ShaderPropertyType.Texture:var t=m.GetTexture(n);key.Append(t?AssetDatabase.GetAssetPath(t)+":"+t.name:"none").Append(m.GetTextureScale(n)).Append(m.GetTextureOffset(n));break;
                    case ShaderPropertyType.Color:key.Append(m.GetColor(n).ToString("F5"));break;
                    case ShaderPropertyType.Vector:key.Append(m.GetVector(n).ToString("F5"));break;
                    default:key.Append(m.GetFloat(n).ToString("R",System.Globalization.CultureInfo.InvariantCulture));break;
                }
            }
            key.Append('|').Append(string.Join(",",m.shaderKeywords.OrderBy(x=>x)));return key.ToString();
        }
        static void SaveAsset(UnityEngine.Object obj,string path)
        {
            var existing=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if(existing){EditorUtility.CopySerialized(obj,existing);UnityEngine.Object.DestroyImmediate(obj);}
            else AssetDatabase.CreateAsset(obj,path);
        }
        static void IncludeRuntimeShaders()
        {
            var graphics=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset").FirstOrDefault();
            if(!graphics)return;
            var serialized=new SerializedObject(graphics);var shaders=serialized.FindProperty("m_AlwaysIncludedShaders");
            foreach(string name in new[]{"Universal Render Pipeline/Lit","Universal Render Pipeline/Unlit"})
            {
                var shader=Shader.Find(name);if(!shader)continue;
                bool exists=false;for(int i=0;i<shaders.arraySize;i++)if(shaders.GetArrayElementAtIndex(i).objectReferenceValue==shader)exists=true;
                if(!exists){int i=shaders.arraySize;shaders.InsertArrayElementAtIndex(i);shaders.GetArrayElementAtIndex(i).objectReferenceValue=shader;}
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        static void RemoveDemoShaderOverrides()
        {
            var graphics=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset").FirstOrDefault();
            if(!graphics)return;
            var serialized=new SerializedObject(graphics);var shaders=serialized.FindProperty("m_AlwaysIncludedShaders");
            for(int i=shaders.arraySize-1;i>=0;i--)
            {
                var shader=shaders.GetArrayElementAtIndex(i).objectReferenceValue as Shader;
                if(shader&&(shader.name=="Universal Render Pipeline/Lit"||shader.name=="Universal Render Pipeline/Unlit"))
                {shaders.GetArrayElementAtIndex(i).objectReferenceValue=null;shaders.DeleteArrayElementAtIndex(i);}
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        public static void ConfigureAndroid()
        {
            PlayerSettings.productName="Unicorn Gundam Demo";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.kaito.unicorndemo");
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android,ManagedStrippingLevel.Low);
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
            PlayerSettings.colorSpace=ColorSpace.Linear;
            PlayerSettings.Android.androidTVCompatibility=false;
            EditorUserBuildSettings.buildAppBundle=false;
            var ar=ARCoreSettings.GetOrCreateSettings();ar.requirement=ARCoreSettings.Requirement.Optional;ar.depth=ARCoreSettings.Requirement.Optional;
            if(!ARCoreSettings.currentSettings)ARCoreSettings.currentSettings=ar;
            foreach(var target in new[]{BuildTargetGroup.Android,BuildTargetGroup.Standalone})
            {
                var settings=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(target);
                if(settings){settings.InitManagerOnStart=false;EditorUtility.SetDirty(settings);}
            }
            var pipeline=GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if(pipeline)
            {
                string path=Root+"/Generated/UnicornMobilePipeline.asset";
                var copy=UnityEngine.Object.Instantiate(pipeline);copy.name="Unicorn Mobile Pipeline";
                var so=new SerializedObject(copy);
                Set(so,"m_MSAA",1);Set(so,"m_SupportsHDR",false);Set(so,"m_MainLightShadowsSupported",false);Set(so,"m_AdditionalLightShadowsSupported",false);
                var rs=so.FindProperty("m_RenderScale");if(rs!=null)rs.floatValue=.85f;
                so.ApplyModifiedPropertiesWithoutUndo();SaveAsset(copy,path);
                var saved=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
                GraphicsSettings.defaultRenderPipeline=saved;QualitySettings.renderPipeline=saved;
            }
        }
        static void Set(SerializedObject so,string name,int value){var p=so.FindProperty(name);if(p!=null)p.intValue=value;}
        static void Set(SerializedObject so,string name,bool value){var p=so.FindProperty(name);if(p!=null)p.boolValue=value;}
        [MenuItem("Unicorn Demo/2 Build Android APK")]
        public static void BuildAndroid()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath,"{\"state\":\"building\",\"started\":\""+DateTime.UtcNow.ToString("o")+"\"}");
            try
            {
                ConfigureAndroid();AssetDatabase.SaveAssets();
                var apk=Path.Combine(ProjectRoot,"Builds","UnicornDemo","UnicornGundamDemo.apk");
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=apk,target=BuildTarget.Android,options=BuildOptions.CompressWithLz4});
                var errors=report.steps.SelectMany(x=>x.messages).Where(x=>x.type==LogType.Error||x.type==LogType.Exception).Select(x=>x.content).Take(12).ToArray();
                var status=new BuildStatus{state=report.summary.result.ToString(),apk=apk,size=(long)report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds,errors=errors};
                File.WriteAllText(ReportPath,JsonUtility.ToJson(status,true));Debug.Log("UNICORN_BUILD="+status.state+" "+apk);
            }
            catch(Exception e){File.WriteAllText(ReportPath,JsonUtility.ToJson(new BuildStatus{state="Failed",errors=new[]{e.ToString()}},true));Debug.LogException(e);}
        }
        [Serializable]class BuildStatus{public string state,apk;public long size;public double seconds;public string[] errors;}
    }
}
