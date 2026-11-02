using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TurnBasedBattle.Editor
{
    public static class MegaKitEnvironmentBuilder
    {
        const string Kit="Assets/ThirdParty/Quaternius/ModularSciFiMegaKit";
        const string Output="Assets/TurnBasedBattle/Environment";
        [Serializable] class MaterialInfo {public string name,baseMap,normal,orm,emissionMap,alpha;public float[] color,emission;public float metallic,roughness;}
        [Serializable] class MaterialList {public MaterialInfo[] materials;}
        static Texture2D Tex(string name)=>string.IsNullOrEmpty(name)?null:AssetDatabase.LoadAssetAtPath<Texture2D>(Kit+"/Textures/"+name);
        static T Save<T>(T asset,string path) where T:UnityEngine.Object
        {
            var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(asset,old);UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);return old;}
            AssetDatabase.CreateAsset(asset,path);return asset;
        }
        [MenuItem("Turn Battle/Environment/Prepare MegaKit URP Materials")]
        public static void PrepareMaterials()
        {
            Directory.CreateDirectory(Output+"/Materials");AssetDatabase.Refresh();
            foreach(string path in Directory.GetFiles(Kit+"/Textures","*.png",SearchOption.AllDirectories))
            {
                string p=path.Replace('\\','/');var ti=(TextureImporter)AssetImporter.GetAtPath(p);if(ti==null)continue;
                bool normal=p.Contains("_Normal"),linear=p.Contains("/URP/")||p.Contains("_ORM")||p.Contains("DetailMask");
                ti.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;ti.sRGBTexture=!normal&&!linear;ti.maxTextureSize=2048;ti.mipmapEnabled=true;ti.wrapMode=TextureWrapMode.Repeat;ti.SaveAndReimport();
            }
            var definitions=JsonUtility.FromJson<MaterialList>(File.ReadAllText(Kit+"/MaterialImport.json"));var materials=new Dictionary<string,Material>();
            foreach(var d in definitions.materials)
            {
                var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=d.name,enableInstancing=true};
                mat.SetColor("_BaseColor",new Color(d.color[0],d.color[1],d.color[2],d.color[3]));mat.SetFloat("_Metallic",d.metallic);mat.SetFloat("_Smoothness",1-d.roughness);
                if(!string.IsNullOrEmpty(d.baseMap))mat.SetTexture("_BaseMap",Tex(d.baseMap));
                if(!string.IsNullOrEmpty(d.normal)){mat.SetTexture("_BumpMap",Tex(d.normal));mat.SetFloat("_BumpScale",.8f);mat.EnableKeyword("_NORMALMAP");}
                if(!string.IsNullOrEmpty(d.orm))
                {
                    mat.SetTexture("_MetallicGlossMap",Tex("URP/"+d.orm.Replace("_ORM","_MetallicSmoothness")));mat.EnableKeyword("_METALLICSPECGLOSSMAP");mat.SetFloat("_Metallic",1);mat.SetFloat("_Smoothness",.7f);
                    mat.SetTexture("_OcclusionMap",Tex("URP/"+d.orm.Replace("_ORM","_Occlusion")));mat.SetFloat("_OcclusionStrength",.75f);mat.EnableKeyword("_OCCLUSIONMAP");
                }
                var emission=new Color(d.emission[0],d.emission[1],d.emission[2]);
                if(d.name=="M_Light")emission=new Color(1.4f,1.8f,2.1f);
                if(d.name=="M_Decal_Screen")emission=new Color(.08f,.4f,.6f);
                if(!string.IsNullOrEmpty(d.emissionMap)){mat.SetTexture("_EmissionMap",Tex(d.emissionMap));if(emission.maxColorComponent==0)emission=Color.white;}
                if(emission.maxColorComponent>0){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",emission);}
                if(d.name.Contains("Decal") && d.baseMap=="T_Decals.png")
                {mat.SetFloat("_AlphaClip",1);mat.SetFloat("_Cutoff",.35f);mat.EnableKeyword("_ALPHATEST_ON");mat.renderQueue=2450;mat.SetOverrideTag("RenderType","TransparentCutout");}
                if(d.alpha=="BLEND" || d.name=="M_Glass" || d.name.Contains("LightFade"))
                {mat.SetFloat("_Surface",1);mat.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);mat.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);mat.SetFloat("_ZWrite",0);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");mat.renderQueue=3000;mat.SetOverrideTag("RenderType","Transparent");if(d.name=="M_Glass")mat.SetColor("_BaseColor",new Color(.15f,.32f,.4f,.18f));}
                if(d.name=="MI_Trim_02_Variation"){mat.SetTexture("_BaseMap",Tex("T_Trim_02_BaseColor_Blue.png"));mat.SetFloat("_Metallic",.5f);mat.SetFloat("_Smoothness",.35f);}
                materials[d.name]=Save(mat,Output+"/Materials/"+d.name+".mat");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach(string path in Directory.GetFiles(Kit+"/Models","*.fbx",SearchOption.AllDirectories))
                {
                    string p=path.Replace('\\','/');var model=AssetDatabase.LoadAssetAtPath<GameObject>(p);var importer=(ModelImporter)AssetImporter.GetAtPath(p);
                    foreach(var source in model.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m).Distinct())
                        if(materials.TryGetValue(source.name,out var replacement))importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),source.name),replacement);
                    importer.importAnimation=false;importer.addCollider=false;importer.SaveAndReimport();
                }
            }
            finally{AssetDatabase.StopAssetEditing();}AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("MEGAKIT_MATERIALS_READY: "+materials.Count);
        }
        static GameObject Module(string category,string name,Transform parent,Vector3 center,float yaw=0,float baseY=0)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Kit+"/Models/"+category+"/"+name+".fbx");if(!prefab)throw new Exception("Missing MegaKit model: "+name);
            var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.name=name;g.transform.SetParent(parent,false);g.transform.localRotation=Quaternion.Euler(0,yaw,0);
            var bounds=new Bounds();bool has=false;
            foreach(var mf in g.GetComponentsInChildren<MeshFilter>())
            {var b=mf.sharedMesh.bounds;for(int i=0;i<8;i++){var p=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));p=parent.InverseTransformPoint(mf.transform.TransformPoint(p));if(!has){bounds=new Bounds(p,Vector3.zero);has=true;}else bounds.Encapsulate(p);}}
            g.transform.localPosition+=new Vector3(center.x-bounds.center.x,baseY-bounds.min.y,center.z-bounds.center.z);
            foreach(var r in g.GetComponentsInChildren<Renderer>()){r.receiveShadows=true;r.shadowCastingMode=ShadowCastingMode.On;}
            return g;
        }
        static void Wall(Transform root,Vector3 center,float yaw)
        {
            Module("Walls","WallAstra_Straight",root,center,yaw);
            Module("Walls","WallAstra_Straight_Flat",root,center,yaw,3);
            Module("Walls","TopAstra_Straight",root,center,yaw,6);
            var collision=new GameObject("CameraWall");collision.layer=2;collision.transform.SetParent(root,false);collision.transform.localPosition=center+Vector3.up*4;collision.transform.localRotation=Quaternion.Euler(0,yaw,0);collision.AddComponent<BoxCollider>().size=new Vector3(1.2f,8,4);
        }
        public static bool Available=>AssetDatabase.LoadAssetAtPath<Material>(Output+"/Materials/MI_Trim_01.mat");
        [MenuItem("Turn Battle/Environment/Apply MegaKit Hangar")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
            var m=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();if(!m)throw new Exception("Open TurnBattleDemo first");
            var board=m.player.transform.parent;var old=board.Find("MegaKitHangar");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var stage=board.Find("BattleArena");foreach(var r in stage.GetComponentsInChildren<MeshRenderer>(true))r.enabled=false;
            foreach(var l in stage.GetComponentsInChildren<LineRenderer>(true))l.useWorldSpace=false;
            var oldFloor=stage.GetComponentInChildren<BoxCollider>();if(oldFloor)oldFloor.size=new Vector3(48,.5f,44);
            var root=new GameObject("MegaKitHangar");root.transform.SetParent(board,false);
            // 44 x 40 clear room. All decor remains outside the combat lanes and prison radius.
            for(int x=-5;x<=5;x++)for(int z=-4;z<=5;z++)
                Module("Platforms",Mathf.Abs(z)<=1?"Platform_CenterPlate":"Platform_Simple2",root.transform,new Vector3(x*4,0,z*4-2),0,-.012f);
            for(int z=-4;z<=5;z++){Wall(root.transform,new Vector3(22,0,z*4-2),0);Wall(root.transform,new Vector3(-22,0,z*4-2),180);}
            for(int x=-5;x<=5;x++){Wall(root.transform,new Vector3(x*4,0,20),-90);Wall(root.transform,new Vector3(x*4,0,-20),90);}
            for(int side=-1;side<=1;side+=2)for(int x=-4;x<=4;x+=2)
            {
                Module("Columns","Column_Astra",root.transform,new Vector3(x*4,0,side*18.8f),side==1?180:0);
                Module("Props","Prop_Light_Wide",root.transform,new Vector3(x*4,0,side*18.2f),0,6.5f);
                Module("Props","Prop_Computer",root.transform,new Vector3(x*4+1.3f,0,side*17.2f),side==1?180:0);
                Module("Props","Prop_Crate3",root.transform,new Vector3(x*4-1.5f,0,side*16.8f),side*20);
            }
            var reflection=new GameObject("HangarReflection").AddComponent<ReflectionProbe>();reflection.transform.SetParent(root.transform,false);reflection.transform.localPosition=new Vector3(0,3,0);reflection.mode=ReflectionProbeMode.Realtime;reflection.refreshMode=ReflectionProbeRefreshMode.OnAwake;reflection.timeSlicingMode=ReflectionProbeTimeSlicingMode.IndividualFaces;reflection.resolution=128;reflection.size=new Vector3(46,16,42);reflection.boxProjection=true;reflection.intensity=.7f;reflection.clearFlags=ReflectionProbeClearFlags.SolidColor;reflection.backgroundColor=new Color(.04f,.06f,.09f);
            RenderSettings.ambientLight=new Color(.4f,.46f,.52f);RenderSettings.ambientIntensity=1;
            m.battleCamera.backgroundColor=new Color(.035f,.05f,.075f);
            Directory.CreateDirectory(Output);PrefabUtility.SaveAsPrefabAsset(root,Output+"/MegaKitHangar.prefab");
            var switcher=m.GetComponent<BattleEnvironmentSwitcher>();
            if(switcher && switcher.environments!=null)
            {
                foreach(var option in switcher.environments)if(option.title=="SCI-FI HANGAR")option.root=root;
                switcher.Apply(switcher.SelectedIndex);EditorUtility.SetDirty(switcher);
            }
            EditorSceneManager.MarkSceneDirty(m.gameObject.scene);EditorSceneManager.SaveScene(m.gameObject.scene);AssetDatabase.SaveAssets();
            Debug.Log("MEGAKIT_HANGAR_SAVED: "+root.GetComponentsInChildren<Renderer>().Length+" renderers");
        }
    }
}
