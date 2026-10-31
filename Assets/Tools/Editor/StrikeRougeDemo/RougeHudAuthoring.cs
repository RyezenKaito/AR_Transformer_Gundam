using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GundamGame.CombatDemo.Editor
{
    [InitializeOnLoad]
    public static class RougeHudAuthoring
    {
        const string Request="Temp/RougeHud.request";
        public const string Folder="Assets/Game/Features/Combat/UI/StrikeRouge";
        static RougeHudAuthoring(){EditorApplication.update+=Poll;}
        static void Poll()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(Request))return;
            string mode;try{mode=File.ReadAllText(Request).Trim();File.Delete(Request);}catch(IOException){return;}
            try{if(mode=="upgrade")Upgrade();else if(mode=="validate")RougeHudValidation.Run();else if(mode=="layout")RougeHudValidation.RunLayouts();else if(mode=="target-zoom")RougeTargetZoomValidation.Run();}
            catch(Exception ex){Directory.CreateDirectory("Docs/Reports/StrikeRougeHUD");File.WriteAllText("Docs/Reports/StrikeRougeHUD/SetupError.txt",ex.ToString());Debug.LogException(ex);}
        }
        [MenuItem("Gundam/Strike Rouge/Upgrade Mecha HUD")]
        public static void Upgrade()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before upgrading HUD.");
            string backup="Assets/Sandbox/StrikeRougeCombat/PreservedScenes";Directory.CreateDirectory(backup);
            var current=EditorSceneManager.GetActiveScene();
            if(current.isDirty && !EditorSceneManager.SaveScene(current,backup+"/BeforeHud_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true))throw new IOException("Cannot preserve current scene.");
            if(current.path!=RougeCombatSceneBuilder.ScenePath)EditorSceneManager.OpenScene(RougeCombatSceneBuilder.ScenePath);
            if(!EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),backup+"/CombatBeforeHud_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true))throw new IOException("Cannot preserve demo scene.");
            var battle=UnityEngine.Object.FindAnyObjectByType<RougeBattleDemo>();
            if(battle==null)throw new InvalidOperationException("Battle controller missing.");
            Directory.CreateDirectory(Folder+"/Art");Directory.CreateDirectory(Folder+"/Prefabs");AssetDatabase.Refresh();
            var art=AssetDatabase.LoadAssetAtPath<RougeHudArt>(Folder+"/StrikeRougeHudArt.asset");
            if(art==null){art=ScriptableObject.CreateInstance<RougeHudArt>();AssetDatabase.CreateAsset(art,Folder+"/StrikeRougeHudArt.asset");}
            art.playerPortrait=Portrait(battle.player,"Portrait_Rouge_Close");
            art.enemyPortraits=new Sprite[battle.enemies.Length];
            for(int i=0;i<battle.enemies.Length;i++)art.enemyPortraits[i]=Portrait(battle.enemies[i],"Portrait_Enemy_"+i+"_Close");
            art.weaponIcons=new[]{Weapon(battle.player,"_048","Icon_Rifle"),Weapon(battle.player,"_046","Icon_Blade"),Weapon(battle.player,"_035","Icon_Missiles"),art.playerPortrait};
            string atlasPath=Folder+"/Art/MechaWeaponsAtlas.png";
            if(File.Exists(atlasPath))
            {
                AssetDatabase.ImportAsset(atlasPath,ImportAssetOptions.ForceSynchronousImport);
                var atlasImporter=(TextureImporter)AssetImporter.GetAtPath(atlasPath);
                atlasImporter.GetSourceTextureWidthAndHeight(out int w,out int h);
                atlasImporter.textureType=TextureImporterType.Sprite;atlasImporter.spriteImportMode=SpriteImportMode.Multiple;
                atlasImporter.alphaIsTransparency=true;atlasImporter.mipmapEnabled=false;atlasImporter.maxTextureSize=4096;
                atlasImporter.textureCompression=TextureImporterCompression.Uncompressed;atlasImporter.isReadable=true;atlasImporter.SaveAndReimport();
                var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);var pixels=atlas.GetPixels32();
                atlasImporter.spritesheet=Enumerable.Range(0,4).Select(i=>new SpriteMetaData{name="Weapon_"+i,rect=TightRect(pixels,w,h,i),pivot=new Vector2(.5f,.5f),alignment=(int)SpriteAlignment.Center}).ToArray();
                atlasImporter.textureCompression=TextureImporterCompression.Compressed;atlasImporter.isReadable=false;
                atlasImporter.SaveAndReimport();
                art.weaponIcons=AssetDatabase.LoadAllAssetsAtPath(atlasPath).OfType<Sprite>().OrderBy(x=>x.name).ToArray();
                if(art.weaponIcons.Length!=4)throw new InvalidOperationException("Weapon atlas needs four sprites.");
            }
            EditorUtility.SetDirty(art);
            var root=new GameObject("Strike Rouge Mecha HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(MechaBattleHUD));
            try
            {
                var hud=root.GetComponent<MechaBattleHUD>();hud.art=art;hud.BuildHierarchy();
                string prefabPath=Folder+"/Prefabs/StrikeRougeMechaHUD.prefab";
                PrefabUtility.SaveAsPrefabAsset(root,prefabPath);battle.hudPrefab=AssetDatabase.LoadAssetAtPath<MechaBattleHUD>(prefabPath);
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
            battle.player.maxArmor=300;battle.player.attack=100;battle.player.defense=80;battle.player.speed=144;battle.player.breakDamage=60;
            int[] speeds={132,152,160};
            for(int i=0;i<battle.enemies.Length;i++){var enemy=battle.enemies[i];enemy.maxArmor=200+i*40;enemy.attack=95+i*20;enemy.defense=60+i*10;enemy.speed=speeds[i%speeds.Length];enemy.breakDamage=40+i*10;EditorUtility.SetDirty(enemy);}
            if(battle.Placement!=null){battle.Placement.showOverlay=false;EditorUtility.SetDirty(battle.Placement);}
            // The virtual arena is intentionally an Editor simulation, not a fake camera feed.
            var cam=battle.presentation.virtualCamera;cam.transform.position=new Vector3(7,6,-11);cam.transform.LookAt(new Vector3(0,.7f,.4f));
            var orbit=cam.GetComponent<RougeArenaCamera>();if(orbit!=null){orbit.pivot=new Vector3(0,.7f,.4f);EditorUtility.SetDirty(orbit);}
            EditorUtility.SetDirty(cam);EditorUtility.SetDirty(battle.player);EditorUtility.SetDirty(battle);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            Debug.Log("Mecha HUD ready / five-cell Burst / authored prefab and source-model artwork.");
        }
        static GameObject Clone(DemoFighter actor)
        {
            var clone=UnityEngine.Object.Instantiate(actor.visual.gameObject);clone.name="HUD Art Preview";clone.transform.position=Vector3.zero;clone.transform.rotation=Quaternion.identity;
            foreach(var animator in clone.GetComponentsInChildren<Animator>(true))animator.enabled=false;
            var driver=actor.GetComponent<RougeAnimationDriver>();
            if(driver!=null && driver.idle!=null)driver.idle.SampleAnimation(clone.transform.GetChild(0).gameObject,2);
            foreach(var renderer in clone.GetComponentsInChildren<SkinnedMeshRenderer>(true))renderer.updateWhenOffscreen=true;
            return clone;
        }
        static Sprite Portrait(DemoFighter actor,string name)
        {
            string path=Folder+"/Art/"+name+".png";
            if(File.Exists(path))return ImportSprite(path);
            var clone=Clone(actor);var preview=new PreviewRenderUtility();
            try
            {
                var head=clone.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name.StartsWith("head",StringComparison.OrdinalIgnoreCase));
                Bounds bounds=BoundsOf(clone);Vector3 aim=head!=null?head.position-Vector3.up*actor.height*.02f:bounds.center+Vector3.up*bounds.extents.y*.55f;
                preview.AddSingleGO(clone);Setup(preview);
                preview.camera.transform.position=aim+new Vector3(.14f,.05f,.70f)*actor.height;preview.camera.transform.LookAt(aim);preview.camera.fieldOfView=26;
                Render(preview,path,512,512);return ImportSprite(path);
            }
            finally{preview.Cleanup();}
        }
        static Sprite Weapon(DemoFighter actor,string token,string name)
        {
            string path=Folder+"/Art/"+name+".png";if(File.Exists(path))return ImportSprite(path);
            var clone=Clone(actor);var container=new GameObject("Weapon Art");var preview=new PreviewRenderUtility();var meshes=new List<Mesh>();
            try
            {
                int triangles=0;
                foreach(var skin in clone.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    var baked=new Mesh();skin.BakeMesh(baked);meshes.Add(baked);
                    var weights=skin.sharedMesh.boneWeights;var vertices=baked.vertices;var selected=new bool[vertices.Length];
                    bool IsWeapon(int bone)=>bone>=0&&bone<skin.bones.Length && skin.bones[bone]!=null && (skin.bones[bone].name.EndsWith(token,StringComparison.Ordinal) || Ancestor(skin.bones[bone],token));
                    for(int i=0;i<vertices.Length;i++)
                    {
                        var w=weights[i];float score=(IsWeapon(w.boneIndex0)?w.weight0:0)+(IsWeapon(w.boneIndex1)?w.weight1:0)+(IsWeapon(w.boneIndex2)?w.weight2:0)+(IsWeapon(w.boneIndex3)?w.weight3:0);
                        selected[i]=score>.45f;vertices[i]=skin.transform.TransformPoint(vertices[i]);
                    }
                    var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};meshes.Add(mesh);mesh.vertices=vertices;mesh.uv=baked.uv;mesh.subMeshCount=baked.subMeshCount;
                    for(int sub=0;sub<baked.subMeshCount;sub++)
                    {
                        int[] source=baked.GetTriangles(sub);var indices=new List<int>();
                        for(int i=0;i<source.Length;i+=3)if((selected[source[i]]?1:0)+(selected[source[i+1]]?1:0)+(selected[source[i+2]]?1:0)>=2){indices.Add(source[i]);indices.Add(source[i+1]);indices.Add(source[i+2]);triangles++;}
                        mesh.SetTriangles(indices,sub);
                    }
                    mesh.RecalculateNormals();mesh.RecalculateBounds();
                    var piece=new GameObject("Weapon Mesh",typeof(MeshFilter),typeof(MeshRenderer));piece.transform.SetParent(container.transform,false);piece.GetComponent<MeshFilter>().sharedMesh=mesh;piece.GetComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;
                }
                if(triangles==0)throw new InvalidOperationException("No weapon geometry for "+token);
                // Bounds from selected triangles, not unused vertices from the full skin.
                bool found=false;Bounds bounds=new Bounds();
                foreach(var filter in container.GetComponentsInChildren<MeshFilter>())
                {var mesh=filter.sharedMesh;var vertices=mesh.vertices;foreach(int index in mesh.triangles){var point=filter.transform.TransformPoint(vertices[index]);if(!found){bounds=new Bounds(point,Vector3.zero);found=true;}else bounds.Encapsulate(point);}}
                preview.AddSingleGO(container);Setup(preview);preview.camera.fieldOfView=30;
                float span=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);Vector3 direction=new Vector3(.25f,.12f,1).normalized;
                preview.camera.transform.position=bounds.center+direction*Mathf.Max(.05f,span*2.15f);preview.camera.transform.LookAt(bounds.center);
                Render(preview,path,640,400);return ImportSprite(path);
            }
            finally{preview.Cleanup();UnityEngine.Object.DestroyImmediate(clone);foreach(var mesh in meshes)if(mesh!=null)UnityEngine.Object.DestroyImmediate(mesh);}
        }
        static bool Ancestor(Transform t,string token){for(var p=t.parent;p!=null;p=p.parent)if(p.name.EndsWith(token,StringComparison.Ordinal))return true;return false;}
        static Rect TightRect(Color32[] pixels,int w,int h,int slot)
        {
            int left=slot*w/4,right=(slot+1)*w/4,minX=right,maxX=left,minY=h,maxY=0;
            for(int y=0;y<h;y++)for(int x=left;x<right;x++)if(pixels[y*w+x].a>40){minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);}
            if(maxX<minX)return new Rect(left,0,right-left,h);
            minX=Math.Max(left,minX-6);maxX=Math.Min(right-1,maxX+6);minY=Math.Max(0,minY-6);maxY=Math.Min(h-1,maxY+6);
            return new Rect(minX,minY,maxX-minX+1,maxY-minY+1);
        }
        static Bounds BoundsOf(GameObject go){var renderers=go.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);return bounds;}
        static void Setup(PreviewRenderUtility preview)
        {
            preview.camera.nearClipPlane=.001f;preview.camera.farClipPlane=1000;preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.015f,.025f,.035f,0);
            preview.lights[0].intensity=1.6f;preview.lights[0].transform.rotation=Quaternion.Euler(35,25,0);
            preview.lights[1].intensity=1.0f;preview.lights[1].color=new Color(.72f,.88f,1);preview.lights[1].transform.rotation=Quaternion.Euler(340,210,0);
            preview.ambientColor=new Color(.48f,.52f,.58f);
        }
        static void Render(PreviewRenderUtility preview,string path,int width,int height)
        {
            preview.BeginPreview(new Rect(0,0,width,height),GUIStyle.none);preview.Render(true);var texture=preview.EndPreview();
            var previous=RenderTexture.active;var rt=RenderTexture.GetTemporary(width,height,24,RenderTextureFormat.ARGB32);Graphics.Blit(texture,rt);RenderTexture.active=rt;
            var image=new Texture2D(width,height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
            RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(image);
        }
        static Sprite ImportSprite(string path)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=1024;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
