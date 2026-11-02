using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TurnBasedBattle.Editor
{
    public static class SphereCityEnvironmentBuilder
    {
        const string Kit="Assets/ThirdParty/SphereCity";
        const string Output="Assets/TurnBasedBattle/Environment";
        [Serializable] class Definition {public string name,path,baseMap,metalMap,aoMap,emissionMap,normal;}
        [Serializable] class Definitions {public Definition[] materials;}
        static Texture2D Tex(string path)=>string.IsNullOrEmpty(path)?null:AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        public static bool Available=>File.Exists(Kit+"/MaterialImport.json");
        [MenuItem("Turn Battle/Environment/Prepare SphereCity URP")]
        public static void PrepareMaterials()
        {
            AssetDatabase.Refresh();
            var data=JsonUtility.FromJson<Definitions>(File.ReadAllText(Kit+"/MaterialImport.json"));
            foreach(string p in data.materials.SelectMany(d=>new[]{d.baseMap,d.normal,d.metalMap,d.aoMap,d.emissionMap}).Where(p=>!string.IsNullOrEmpty(p)).Distinct())
            {
                var ti=AssetImporter.GetAtPath(p) as TextureImporter;if(!ti)continue;
                bool normal=p.Contains("_Normal."),linear=p.Contains("_MetalSmooth")||p.Contains("_AO.");
                ti.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;ti.sRGBTexture=!normal&&!linear;ti.maxTextureSize=2048;ti.mipmapEnabled=true;ti.SaveAndReimport();
            }
            // Create a clean URP material and copy it over the imported asset, retaining its GUID.
            foreach(var d in data.materials)
            {
                var target=AssetDatabase.LoadAssetAtPath<Material>(d.path);
                var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=d.name,enableInstancing=true};
                mat.SetColor("_BaseColor",string.IsNullOrEmpty(d.baseMap)?new Color(.46f,.53f,.6f):Color.white);
                mat.SetFloat("_Metallic",.45f);mat.SetFloat("_Smoothness",.4f);
                if(Tex(d.baseMap))mat.SetTexture("_BaseMap",Tex(d.baseMap));
                if(Tex(d.normal)){mat.SetTexture("_BumpMap",Tex(d.normal));mat.SetFloat("_BumpScale",.65f);mat.EnableKeyword("_NORMALMAP");}
                if(Tex(d.metalMap)){mat.SetTexture("_MetallicGlossMap",Tex(d.metalMap));mat.SetFloat("_Smoothness",.65f);mat.EnableKeyword("_METALLICSPECGLOSSMAP");}
                if(Tex(d.aoMap)){mat.SetTexture("_OcclusionMap",Tex(d.aoMap));mat.SetFloat("_OcclusionStrength",.7f);mat.EnableKeyword("_OCCLUSIONMAP");}
                if(Tex(d.emissionMap)){mat.SetTexture("_EmissionMap",Tex(d.emissionMap));mat.SetColor("_EmissionColor",new Color(.7f,.7f,.7f));mat.EnableKeyword("_EMISSION");}
                if(target){EditorUtility.CopySerialized(mat,target);EditorUtility.SetDirty(target);UnityEngine.Object.DestroyImmediate(mat);}else AssetDatabase.CreateAsset(mat,d.path);
            }
            AssetDatabase.SaveAssets();Debug.Log("SPHERE_CITY_URP_READY: "+data.materials.Length);
        }
        static Material Mat(string name)=>AssetDatabase.LoadAssetAtPath<Material>(Kit+"/Art/Materials/"+name+".mat");
        static Material Resolve(string name,string model)
        {
            // The shell's UVs belong to a custom HDRP shader; a neutral URP surface avoids stretched atlas panels.
            if(model=="Sphere")return Mat("InnerSphere");
            if(model=="gravityDriveAndSpokes")return Mat("Metal_Plate_012");
            string key=name.Replace("_basecolor","");
            var direct=Mat(key);if(direct)return direct;
            if(model=="City")return Mat("City");
            if(name.Contains("Sci-fi"))return Mat("Sci-fi_Walll_001");
            return Mat("Metal_Plate_012");
        }
        static GameObject Model(string name,Transform parent,float scale)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Kit+"/Art/Models/World/"+name+".fbx");
            if(!source)throw new Exception("Missing SphereCity model: "+name);
            var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.name=name;g.transform.SetParent(parent,false);g.transform.localScale=Vector3.one*scale;
            foreach(var r in g.GetComponentsInChildren<Renderer>())
            {r.sharedMaterials=r.sharedMaterials.Select(m=>Resolve(m?m.name:"",name)).ToArray();r.shadowCastingMode=name=="Sphere" || name=="gravityDriveAndSpokes" || name=="City"?ShadowCastingMode.Off:ShadowCastingMode.On;}
            foreach(var c in g.GetComponentsInChildren<Camera>())UnityEngine.Object.DestroyImmediate(c);
            foreach(var l in g.GetComponentsInChildren<Light>())UnityEngine.Object.DestroyImmediate(l);
            return g;
        }
        static void PlaceOnGround(GameObject g,Vector3 point,float height)
        {
            var rs=g.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
            g.transform.position+=new Vector3(point.x-b.center.x,height-b.min.y,point.z-b.center.z);
        }
        [MenuItem("Turn Battle/Environment/Build SphereCity and Environment Switcher")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
            var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();if(!battle)throw new Exception("Open TurnBattleDemo first.");
            var board=battle.player.transform.parent;var old=board.Find("SphereCityArena");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var root=new GameObject("SphereCityArena");root.transform.SetParent(board,false);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cylinder);floor.name="SphereCity Battle Deck";floor.transform.SetParent(root.transform,false);floor.transform.localScale=new Vector3(48,.4f,48);floor.transform.localPosition=new Vector3(0,-.405f,0);
            UnityEngine.Object.DestroyImmediate(floor.GetComponent<Collider>());
            string floorPath=Output+"/SphereCityDeck.mat";var deck=AssetDatabase.LoadAssetAtPath<Material>(floorPath);
            if(!deck){deck=new Material(Mat("Metal_Plate_020"));AssetDatabase.CreateAsset(deck,floorPath);}
            deck.SetTextureScale("_BaseMap",new Vector2(12,12));deck.SetColor("_BaseColor",new Color(.42f,.51f,.6f));deck.SetFloat("_Smoothness",.32f);EditorUtility.SetDirty(deck);floor.GetComponent<Renderer>().sharedMaterial=deck;
            // Keep only peripheral arena architecture; the battle lane and flight volume stay clear.
            var arena=Model("Arena",root.transform,1.1f);
            var clear=new Bounds(board.TransformPoint(new Vector3(0,6,0)),new Vector3(36,16,28));
            foreach(var r in arena.GetComponentsInChildren<Renderer>())if(r.bounds.Intersects(clear))r.gameObject.SetActive(false);
            for(int i=0;i<16;i++)
            {
                string name=new[]{"DistantBuilding1_low","DistantBuilding9_low","DistantBuilding10_low","DistantBuilding4_low"}[i%4];
                var g=Model(name,root.transform,.6f+(i%3)*.16f);float angle=(i*22.5f+11)*Mathf.Deg2Rad;
                PlaceOnGround(g,board.TransformPoint(new Vector3(Mathf.Cos(angle)*43,0,Mathf.Sin(angle)*43)),0);
            }
            var shell=Model("Sphere",root.transform,.075f);shell.transform.localPosition=new Vector3(0,-12,0);
            var spokes=Model("gravityDriveAndSpokes",root.transform,.075f);spokes.transform.localPosition=new Vector3(0,-12,0);
            for(int i=0;i<4;i++)
            {
                var pivot=new GameObject("City Quarter "+i).transform;pivot.SetParent(root.transform,false);pivot.localPosition=new Vector3(0,-12,0);pivot.localRotation=Quaternion.Euler(0,i*90,0);
                Model("City",pivot,.075f);
            }
            var probe=new GameObject("SphereCity Reflection").AddComponent<ReflectionProbe>();probe.transform.SetParent(root.transform,false);probe.transform.localPosition=new Vector3(0,3,0);probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.IndividualFaces;probe.resolution=128;probe.size=new Vector3(55,20,55);probe.clearFlags=ReflectionProbeClearFlags.SolidColor;probe.backgroundColor=new Color(.13f,.23f,.3f);probe.intensity=.6f;
            battle.battleCamera.farClipPlane=350;
            Directory.CreateDirectory(Output);PrefabUtility.SaveAsPrefabAsset(root,Output+"/SphereCityArena.prefab");
            ConfigureSwitcher(battle,root);
            EditorSceneManager.MarkSceneDirty(battle.gameObject.scene);EditorSceneManager.SaveScene(battle.gameObject.scene);AssetDatabase.SaveAssets();
            Debug.Log("SPHERE_CITY_ARENA_SAVED: "+root.GetComponentsInChildren<Renderer>().Length+" active renderers");
        }
        static void ConfigureSwitcher(TurnBattleManager battle,GameObject city)
        {
            var board=battle.player.transform.parent;var stage=board.Find("BattleArena");var hangar=board.Find("MegaKitHangar");
            var switcher=battle.GetComponent<BattleEnvironmentSwitcher>();if(!switcher)switcher=battle.gameObject.AddComponent<BattleEnvironmentSwitcher>();
            switcher.battle=battle;switcher.desktopOrbit=battle.battleCamera.GetComponent<BattleOrbitCamera>();switcher.trainingRenderers=stage.GetComponentsInChildren<MeshRenderer>(true);
            var entries=new List<BattleEnvironmentSwitcher.EnvironmentOption>{new BattleEnvironmentSwitcher.EnvironmentOption{title="SÀN TẬP"}};
            if(hangar)entries.Add(new BattleEnvironmentSwitcher.EnvironmentOption{title="SCI-FI HANGAR",root=hangar.gameObject});
            entries.Add(new BattleEnvironmentSwitcher.EnvironmentOption{title="SPHERE CITY",root=city,ambient=new Color(.48f,.58f,.66f),background=new Color(.12f,.21f,.29f)});
            switcher.environments=entries.ToArray();
            var hud=battle.hud.transform;var old=hud.Find("EnvironmentSwitch");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var g=new GameObject("EnvironmentSwitch",typeof(RectTransform),typeof(Image),typeof(Button));var rect=(RectTransform)g.transform;rect.SetParent(hud,false);
            rect.anchorMin=new Vector2(.025f,.635f);rect.anchorMax=new Vector2(.25f,.704f);rect.offsetMin=rect.offsetMax=Vector2.zero;
            var image=g.GetComponent<Image>();image.color=new Color(.035f,.10f,.15f,.97f);var button=g.GetComponent<Button>();button.targetGraphic=image;
            var label=new GameObject("Label",typeof(RectTransform),typeof(Text));var lr=(RectTransform)label.transform;lr.SetParent(rect,false);lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=new Vector2(10,3);lr.offsetMax=new Vector2(-10,-3);
            var text=label.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=19;text.resizeTextForBestFit=true;text.resizeTextMinSize=12;text.resizeTextMaxSize=19;text.alignment=TextAnchor.MiddleLeft;text.color=new Color(.45f,.9f,1);text.raycastTarget=false;
            // Insert before modal overlays so Pause/Result still intercept input.
            rect.SetSiblingIndex(3);switcher.nextButton=button;switcher.buttonLabel=text;switcher.Apply(entries.Count-1);EditorUtility.SetDirty(switcher);
        }
    }
}
