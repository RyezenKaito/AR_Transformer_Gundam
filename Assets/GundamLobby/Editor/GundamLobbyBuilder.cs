using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace GundamLobby.Editor
{
    public static class GundamLobbyBuilder
    {
        const string Root = "Assets/GundamLobby";
        static Color cyan = new Color(.125f, .785f, .965f);
        static Color white = new Color(.88f, .94f, .97f);
        static Color muted = new Color(.48f, .63f, .69f);
        static Color line = new Color(.12f, .39f, .49f, .65f);
        static Color panel = new Color(.018f, .045f, .065f, .82f);
        static Material bone, blue, steel, black, red, gold, glow, warm, wall, floor;
        static Mesh armorMesh;
        static TMP_FontAsset font;
        static HomeLobbyController controller;
        static readonly List<Transform> prefabGroups = new List<Transform>();
        static readonly List<Image> skills = new List<Image>();

        [MenuItem("Tools/Gundam Lobby/Create Home Lobby")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before building the lobby.");
            if (File.Exists(Root + "/Scenes/HomeLobby.unity")) throw new InvalidOperationException("HomeLobby already exists. Open the existing scene to keep your edits.");
            foreach (string path in new[] { Root, Root + "/Editor", Root + "/Runtime", Root + "/Scenes", Root + "/Materials", Root + "/Meshes", Root + "/Prefabs", Root + "/Fonts", Root + "/Textures", Root + "/Backups" }) Directory.CreateDirectory(path);
            var old = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (old.isDirty)
            {
                string backup = Root + "/Backups/" + (string.IsNullOrEmpty(old.name) ? "Untitled" : old.name) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity";
                if (!EditorSceneManager.SaveScene(old, backup, true)) throw new IOException("Could not back up the open scene.");
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            prefabGroups.Clear(); skills.Clear();
            MakeMaterials(); MakeFont();
            armorMesh = CreateArmorMesh();
            AssetDatabase.CreateAsset(armorMesh, Root + "/Meshes/BeveledArmor.asset");
            var systems = new GameObject("Lobby Systems");
            controller = systems.AddComponent<HomeLobbyController>();
            var envMount = new GameObject("Environment Mount — assign replacement in Lobby Systems").transform;
            var environment = new GameObject("Placeholder Hangar").transform; environment.SetParent(envMount, false);
            BuildHangar(environment);
            controller.environmentMount = envMount;
            controller.placeholderHangar = environment.gameObject;
            var pivot = new GameObject("Gundam Preview Pivot").transform;
            pivot.localPosition = new Vector3(0, .26f, 0);
            pivot.localRotation = Quaternion.Euler(0, -16, 0);
            var model = new GameObject("AEGIS — procedural Gundam placeholder").transform; model.SetParent(pivot, false);
            BuildMech(model);
            controller.modelPivot = pivot; controller.placeholderModel = model.gameObject;
            PrefabUtility.SaveAsPrefabAssetAndConnect(model.gameObject, Root + "/Prefabs/PlaceholderGundam.prefab", InteractionMode.AutomatedAction);
            PrefabUtility.SaveAsPrefabAssetAndConnect(environment.gameObject, Root + "/Prefabs/PlaceholderHangar.prefab", InteractionMode.AutomatedAction);
            Camera camera = BuildCamera();
            Texture2D thumbnail = RenderThumbnail(model, false);
            Texture2D avatar = RenderThumbnail(model, true);
            BuildUI(thumbnail, avatar);
            controller.skillImages = skills.ToArray();
            controller.ApplyPresentation();
            foreach (var group in prefabGroups) PrefabUtility.SaveAsPrefabAssetAndConnect(group.gameObject, Root + "/Prefabs/" + group.name + ".prefab", InteractionMode.AutomatedAction);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.2f, .26f, .34f);
            RenderSettings.ambientEquatorColor = new Color(.1f, .145f, .19f);
            RenderSettings.ambientGroundColor = new Color(.035f, .045f, .06f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.025f, .055f, .075f);
            RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = .013f;
            RenderSettings.skybox = null;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, Root + "/Scenes/HomeLobby.unity");
            File.WriteAllText(Root + "/README_VI.md", Guide(), new System.Text.UTF8Encoding(false));
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Selection.activeGameObject = systems;
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            Debug.Log("Gundam HomeLobby created: " + Root + "/Scenes/HomeLobby.unity. Press Play to preview.");
        }

        static Material Mat(string name, Color c, float metal = 0, float smooth = .4f, bool emission = false)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.name = name;
            m.SetColor("_BaseColor", c); m.SetFloat("_Metallic", metal); m.SetFloat("_Smoothness", smooth);
            if (emission) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", c * 3); }
            AssetDatabase.CreateAsset(m, Root + "/Materials/" + name + ".mat");
            return m;
        }
        static void MakeMaterials()
        {
            bone = Mat("Ceramic Ivory", new Color(.68f, .75f, .79f), .52f, .55f);
            blue = Mat("Cobalt Armor", new Color(.025f, .09f, .18f), .65f, .48f);
            steel = Mat("Titanium", new Color(.22f, .28f, .32f), .8f, .5f);
            black = Mat("Carbon Joints", new Color(.017f, .025f, .033f), .6f, .34f);
            red = Mat("Crimson Markings", new Color(.46f, .035f, .035f), .45f);
            gold = Mat("Brass V-Fin", new Color(.68f, .39f, .08f), .7f);
            glow = Mat("Cyan Emissive", new Color(.03f, .75f, .95f), .2f, .5f, true);
            warm = Mat("Amber Emissive", new Color(1f, .43f, .09f), .1f, .4f, true);
            wall = Mat("Hangar Panels", new Color(.055f, .08f, .105f), .65f, .35f);
            floor = Mat("Deck Steel", new Color(.07f, .09f, .11f), .7f, .65f);
        }
        static void MakeFont()
        {
            string path = Root + "/Fonts/SegoeUI.ttf";
            string source = "C:/Windows/Fonts/segoeui.ttf";
            if (File.Exists(source)) File.Copy(source, path, true);
            else File.Copy("Assets/TextMesh Pro/Fonts/LiberationSans.ttf", path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var original = AssetDatabase.LoadAssetAtPath<Font>(path);
            font = TMP_FontAsset.CreateFontAsset(original, 48, 6, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048, TMPro.AtlasPopulationMode.Dynamic, true);
            font.name = "Lobby Vietnamese SDF";
            AssetDatabase.CreateAsset(font, Root + "/Fonts/Lobby Vietnamese SDF.asset");
            foreach (Texture2D texture in font.atlasTextures) if (texture && !AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture, font);
            if (font.material && !AssetDatabase.Contains(font.material)) AssetDatabase.AddObjectToAsset(font.material, font);
            string all = string.Concat(Enumerable.Range(32, 95).Select(i => (char)i)) + string.Concat(Enumerable.Range(0x00C0, 0x180 - 0xC0).Select(i => (char)i)) + string.Concat(Enumerable.Range(0x1EA0, 0x1EFA - 0x1EA0).Select(i => (char)i)) + "Đđ·—–◈◇◆✦＋›✓";
            font.TryAddCharacters(all, out string missing);
            EditorUtility.SetDirty(font);
        }
        static Mesh CreateArmorMesh()
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            Vector2[] oct = { new Vector2(-.34f,-.5f), new Vector2(.34f,-.5f), new Vector2(.5f,-.34f), new Vector2(.5f,.34f), new Vector2(.34f,.5f), new Vector2(-.34f,.5f), new Vector2(-.5f,.34f), new Vector2(-.5f,-.34f) };
            Action<Vector3,Vector3,Vector3> tri = (a,b,c) => { int n=vertices.Count; vertices.Add(a); vertices.Add(b); vertices.Add(c); triangles.Add(n); triangles.Add(n+1); triangles.Add(n+2); };
            for(int i=0;i<8;i++)
            {
                int j=(i+1)%8;
                Vector3 a=new Vector3(oct[i].x*.82f,oct[i].y*.82f,-.5f), b=new Vector3(oct[j].x*.82f,oct[j].y*.82f,-.5f);
                Vector3 c=new Vector3(oct[i].x,oct[i].y,-.32f), d=new Vector3(oct[j].x,oct[j].y,-.32f);
                Vector3 e=new Vector3(oct[i].x,oct[i].y,.5f), f=new Vector3(oct[j].x,oct[j].y,.5f);
                tri(Vector3.back*.5f,b,a); tri(a,b,d); tri(a,d,c); tri(c,d,f); tri(c,f,e); tri(Vector3.forward*.5f,e,f);
            }
            Mesh mesh = new Mesh(); mesh.name="Chamfered hard-surface armor";
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
        static Transform Group(string name, Transform parent)
        {
            var t=new GameObject(name).transform; t.SetParent(parent,false); return t;
        }
        static GameObject Part(Transform parent,string name,Vector3 pos,Vector3 size,Material mat,Vector3 rotation=default, bool bevel=true)
        {
            GameObject go;
            if(bevel) { go=new GameObject(name); go.AddComponent<MeshFilter>().sharedMesh=armorMesh; go.AddComponent<MeshRenderer>().sharedMaterial=mat; }
            else { go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); go.GetComponent<Renderer>().sharedMaterial=mat; }
            go.transform.SetParent(parent,false); go.transform.localPosition=pos; go.transform.localScale=size; go.transform.localEulerAngles=rotation; return go;
        }
        static GameObject Cylinder(Transform p,string n,Vector3 pos,Vector3 scale,Material m,Vector3 rotation=default)
        {
            GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cylinder); go.name=n; go.transform.SetParent(p,false);
            go.transform.localPosition=pos; go.transform.localScale=scale; go.transform.localEulerAngles=rotation;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); go.GetComponent<Renderer>().sharedMaterial=m; return go;
        }
        static void BuildMech(Transform p)
        {
            var torso=Group("Torso - layered armor",p);
            Part(torso,"Reactor chassis",new Vector3(0,5.95f,0),new Vector3(2,1.8f,1),black);
            Part(torso,"Chest armor",new Vector3(0,6.25f,-.32f),new Vector3(2.12f,1.06f,.95f),blue,new Vector3(-8,0,0));
            Part(torso,"Upper collar",new Vector3(0,6.92f,-.2f),new Vector3(1.3f,.22f,.8f),bone);
            Part(torso,"Reactor surround",new Vector3(0,6.18f,-.88f),new Vector3(.64f,.62f,.18f),steel,new Vector3(0,0,45));
            Part(torso,"Reactor light",new Vector3(0,6.18f,-1),new Vector3(.32f,.32f,.08f),glow,new Vector3(0,0,45));
            Part(torso,"Abdomen",new Vector3(0,5.25f,0),new Vector3(1.05f,.8f,.82f),steel);
            for(int j=0;j<3;j++) Part(torso,"Abdominal rib "+j,new Vector3(0,5.02f+j*.22f,-.47f),new Vector3(.85f,.1f,.14f),black);
            for(int s=-1;s<=1;s+=2)
            {
                Part(torso,"Clavicle",new Vector3(s*.78f,6.67f,-.63f),new Vector3(.7f,.25f,.2f),bone,new Vector3(0,0,s*13));
                Part(torso,"Chest vent housing",new Vector3(s*.67f,6.13f,-.83f),new Vector3(.48f,.48f,.16f),gold);
                for(int v=0;v<4;v++) Part(torso,"Vent blade",new Vector3(s*.67f,5.98f+v*.1f,-.94f),new Vector3(.36f,.045f,.04f),black,default,false);
                Part(torso,"Lower chest plate",new Vector3(s*.71f,5.66f,-.45f),new Vector3(.8f,.43f,.48f),bone,new Vector3(0,0,-s*12));
            }
            var head=Group("Head - V-fin and visor",p);
            Cylinder(head,"Neck",new Vector3(0,7.14f,0),new Vector3(.45f,.25f,.45f),black);
            Part(head,"Helmet",new Vector3(0,7.77f,0),new Vector3(.95f,1.03f,.85f),bone);
            Part(head,"Crown",new Vector3(0,8.24f,.05f),new Vector3(.34f,.43f,.69f),bone);
            Part(head,"Face recess",new Vector3(0,7.73f,-.46f),new Vector3(.64f,.55f,.1f),black);
            for(int s=-1;s<=1;s+=2)
            {
                Part(head,"Optical sensor",new Vector3(s*.17f,7.88f,-.53f),new Vector3(.25f,.09f,.045f),glow,new Vector3(0,0,s*8));
                Part(head,"Cheek armor",new Vector3(s*.35f,7.55f,-.43f),new Vector3(.25f,.5f,.24f),bone,new Vector3(0,0,s*12));
                Part(head,"Gold V-fin",new Vector3(s*.63f,8.3f,-.35f),new Vector3(.12f,1.35f,.12f),gold,new Vector3(0,0,-s*48));
                Part(head,"White antenna",new Vector3(s*.52f,8.12f,-.28f),new Vector3(.07f,.8f,.08f),bone,new Vector3(0,0,-s*67));
                Part(head,"Ear pod",new Vector3(s*.53f,7.79f,.03f),new Vector3(.18f,.48f,.42f),steel);
            }
            Part(head,"Chin",new Vector3(0,7.46f,-.55f),new Vector3(.23f,.28f,.23f),red);
            Part(head,"Forehead crest",new Vector3(0,8.14f,-.5f),new Vector3(.21f,.26f,.13f),red);
            Part(head,"Face mask",new Vector3(0,7.63f,-.56f),new Vector3(.33f,.24f,.16f),bone);
            for(int i=-1;i<=1;i++) Part(head,"Face vent",new Vector3(i*.08f,7.62f,-.65f),new Vector3(.025f,.14f,.02f),black,default,false);
            var waist=Group("Waist and skirt armor",p);
            Part(waist,"Pelvis",new Vector3(0,4.65f,0),new Vector3(1.6f,.65f,1),black);
            Part(waist,"Belt",new Vector3(0,4.83f,-.12f),new Vector3(1.7f,.3f,1),bone);
            Part(waist,"Belt clasp",new Vector3(0,4.8f,-.72f),new Vector3(.35f,.3f,.2f),gold);
            Part(waist,"Central skirt",new Vector3(0,4.35f,-.53f),new Vector3(.47f,.86f,.4f),red,new Vector3(-12,0,0));
            for(int s=-1;s<=1;s+=2)
            {
                Part(waist,"Front skirt",new Vector3(s*.61f,4.25f,-.65f),new Vector3(.68f,1.07f,.34f),bone,new Vector3(-13,0,s*10));
                Part(waist,"Skirt inset",new Vector3(s*.61f,4.28f,-.87f),new Vector3(.34f,.55f,.08f),blue,new Vector3(-13,0,s*10));
                Part(waist,"Side skirt",new Vector3(s*1.02f,4.3f,.03f),new Vector3(.4f,1.18f,.95f),blue,new Vector3(0,0,s*20));
                var arm=Group((s<0?"Right":"Left")+" arm",p);
                Cylinder(arm,"Shoulder joint",new Vector3(s*1.55f,6.58f,0),new Vector3(.65f,.45f,.65f),black,new Vector3(0,0,90));
                Part(arm,"Shoulder main",new Vector3(s*1.97f,6.68f,0),new Vector3(1.45f,1.05f,1.25f),bone,new Vector3(0,0,-s*10));
                Part(arm,"Shoulder cap",new Vector3(s*1.97f,7.14f,.04f),new Vector3(1.5f,.32f,1.33f),blue,new Vector3(0,0,-s*10));
                Part(arm,"Shoulder inset",new Vector3(s*2.05f,6.68f,-.69f),new Vector3(.92f,.42f,.12f),blue);
                Part(arm,"Shoulder status",new Vector3(s*2.12f,6.83f,-.79f),new Vector3(.54f,.07f,.05f),glow);
                Part(arm,"Upper arm",new Vector3(s*2.13f,5.91f,.02f),new Vector3(.65f,.95f,.69f),steel,new Vector3(0,0,-s*5));
                Cylinder(arm,"Elbow mechanism",new Vector3(s*2.2f,5.35f,-.04f),new Vector3(.53f,.39f,.53f),black,new Vector3(0,0,90));
                Part(arm,"Forearm",new Vector3(s*2.24f,4.87f,-.2f),new Vector3(.86f,1.14f,.93f),bone,new Vector3(-8,0,s*4));
                Part(arm,"Forearm blue panel",new Vector3(s*2.24f,4.97f,-.73f),new Vector3(.6f,.7f,.14f),blue,new Vector3(-8,0,s*4));
                Part(arm,"Forearm marker",new Vector3(s*2.24f,5.12f,-.83f),new Vector3(.34f,.06f,.04f),glow);
                Part(arm,"Wrist",new Vector3(s*2.27f,4.21f,-.33f),new Vector3(.47f,.25f,.48f),black);
                Part(arm,"Gauntlet",new Vector3(s*2.27f,3.95f,-.36f),new Vector3(.63f,.47f,.66f),steel);
                for(int f=0;f<4;f++) Part(arm,"Finger armor",new Vector3(s*2.27f-.22f+f*.145f,3.86f,-.71f),new Vector3(.115f,.22f,.16f),black);
                var leg=Group((s<0?"Right":"Left")+" leg",p);
                float x=s*.69f;
                Part(leg,"Thigh piston",new Vector3(x,3.5f,.12f),new Vector3(.66f,1.35f,.72f),black);
                Part(leg,"Thigh armor",new Vector3(x,3.49f,-.07f),new Vector3(.92f,1.35f,.96f),bone,new Vector3(0,0,s*3));
                Part(leg,"Thigh facing",new Vector3(x,3.45f,-.61f),new Vector3(.5f,.95f,.12f),steel);
                Cylinder(leg,"Knee joint",new Vector3(x,2.77f,0),new Vector3(.7f,.45f,.7f),black,new Vector3(0,0,90));
                Part(leg,"Knee armor",new Vector3(x,2.77f,-.53f),new Vector3(.77f,.65f,.44f),blue,new Vector3(-12,0,0));
                Part(leg,"Shin armor",new Vector3(s*.78f,1.7f,.05f),new Vector3(1.03f,1.82f,1.07f),bone,new Vector3(0,0,-s*3));
                Part(leg,"Shin center",new Vector3(s*.78f,1.67f,-.56f),new Vector3(.48f,1.31f,.2f),steel,new Vector3(-5,0,0));
                Part(leg,"Shin rail",new Vector3(s*.99f,1.77f,-.66f),new Vector3(.065f,.8f,.06f),glow);
                Part(leg,"Ankle",new Vector3(s*.83f,.65f,.1f),new Vector3(.64f,.45f,.7f),black);
                Part(leg,"Armored foot",new Vector3(s*.83f,.35f,-.43f),new Vector3(1.12f,.57f,1.95f),blue);
                Part(leg,"Toe armor",new Vector3(s*.83f,.4f,-1.1f),new Vector3(1.08f,.38f,.65f),red);
                Part(leg,"Ankle cover",new Vector3(s*.83f,.72f,-.4f),new Vector3(.92f,.5f,.65f),bone,new Vector3(-16,0,0));
                Part(p,"Backpack thruster",new Vector3(s*.72f,6.12f,.94f),new Vector3(.58f,1.55f,.65f),blue);
                Cylinder(p,"Thruster nozzle",new Vector3(s*.72f,5.34f,.93f),new Vector3(.44f,.18f,.44f),steel);
                Part(p,"Back stabilizer",new Vector3(s*1.31f,7.18f,.92f),new Vector3(.25f,2.25f,.48f),blue,new Vector3(-12,0,-s*24));
            }
            var rifle=Group("Beam rifle",p);
            Part(rifle,"Rifle body",new Vector3(-2.55f,3.9f,-.86f),new Vector3(.46f,1.63f,.75f),black,new Vector3(0,0,-4));
            Part(rifle,"Receiver",new Vector3(-2.55f,4.45f,-.91f),new Vector3(.66f,.6f,.9f),steel);
            Part(rifle,"Long barrel",new Vector3(-2.65f,2.2f,-.96f),new Vector3(.24f,2.05f,.28f),black,new Vector3(0,0,-4));
            Part(rifle,"Barrel cyan rail",new Vector3(-2.65f,2.56f,-1.13f),new Vector3(.07f,1.3f,.04f),glow,new Vector3(0,0,-4));
            Part(rifle,"Muzzle",new Vector3(-2.73f,1.14f,-.96f),new Vector3(.39f,.27f,.44f),steel);
            var shield=Group("Shield",p); shield.localPosition=new Vector3(2.82f,4.55f,-.24f); shield.localEulerAngles=new Vector3(0,-13,-7);
            Part(shield,"Shield frame",Vector3.zero,new Vector3(1.25f,3.48f,.4f),bone);
            Part(shield,"Shield field",new Vector3(0,.05f,-.24f),new Vector3(.98f,3.12f,.15f),blue);
            Part(shield,"Shield center",new Vector3(0,.05f,-.35f),new Vector3(.2f,2.65f,.1f),steel);
            Part(shield,"Shield emission",new Vector3(.35f,-.05f,-.35f),new Vector3(.07f,2.2f,.07f),glow);
            Part(shield,"Shield crest A",new Vector3(-.08f,.66f,-.43f),new Vector3(.16f,.63f,.07f),gold,new Vector3(0,0,35));
            Part(shield,"Shield crest B",new Vector3(.1f,.66f,-.43f),new Vector3(.16f,.63f,.07f),gold,new Vector3(0,0,-35));
        }

        static void BuildHangar(Transform p)
        {
            Part(p,"Deck",new Vector3(0,-.18f,0),new Vector3(40,.3f,50),floor,default,false);
            Part(p,"Rear bulkhead",new Vector3(0,7,12),new Vector3(34,15,.5f),wall,default,false);
            Part(p,"Port bulkhead",new Vector3(-13,7,0),new Vector3(.7f,15,28),wall,default,false);
            Part(p,"Starboard bulkhead",new Vector3(13,7,0),new Vector3(.7f,15,28),wall,default,false);
            Part(p,"Ceiling",new Vector3(0,14,1),new Vector3(27,.4f,27),wall,default,false);
            Cylinder(p,"Maintenance platform",new Vector3(0,.05f,0),new Vector3(8.8f,.13f,8.8f),steel);
            Cylinder(p,"Platform inset",new Vector3(0,.19f,0),new Vector3(8.25f,.06f,8.25f),black);
            for(int i=0;i<48;i++)
            {
                float a=i*7.5f*Mathf.Deg2Rad;
                Part(p,"Platform radial light",new Vector3(Mathf.Sin(a)*4.2f,.213f,Mathf.Cos(a)*4.2f),new Vector3(.18f,.035f,.08f),i%6==0?warm:glow,new Vector3(0,i*7.5f,0),false);
            }
            for(int i=-5;i<=5;i++)
            {
                Part(p,"Deck seam",new Vector3(i*2.8f,-.012f,0),new Vector3(.025f,.02f,30),black,default,false);
                Part(p,"Deck seam",new Vector3(0,-.01f,i*2.8f),new Vector3(30,.02f,.025f),black,default,false);
            }
            for(int i=0;i<6;i++)
            {
                float z=-8+i*4;
                for(int s=-1;s<=1;s+=2)
                {
                    Part(p,"Structural column",new Vector3(s*11.7f,6.7f,z),new Vector3(.65f,13.4f,.9f),steel);
                    Part(p,"Column inset",new Vector3(s*11.32f,6.6f,z),new Vector3(.07f,7.8f,.28f),black,default,false);
                    Part(p,"Roof brace",new Vector3(s*8.8f,12.1f,z),new Vector3(6.1f,.5f,.6f),steel,new Vector3(0,0,-s*27));
                    Part(p,"Ceiling strip",new Vector3(s*5.9f,13.55f,z),new Vector3(3,.055f,.4f),glow,default,false);
                    Part(p,"Wall strip",new Vector3(s*11.28f,8.5f,z),new Vector3(.06f,2.5f,.12f),glow,default,false);
                    Cylinder(p,"Service pipe",new Vector3(s*12.05f,2.6f,z),new Vector3(.18f,2.6f,.18f),black);
                }
                Part(p,"Overhead beam",new Vector3(0,13.4f,z),new Vector3(25,.4f,.65f),steel);
            }
            for(int i=-3;i<=3;i++)
            {
                Part(p,"Rear panel",new Vector3(i*3.5f,6.6f,11.65f),new Vector3(3.25f,11,.22f),wall);
                Part(p,"Rear panel rib",new Vector3(i*3.5f,6.6f,11.45f),new Vector3(.16f,11,.18f),steel);
                Part(p,"Rear caution light",new Vector3(i*3.5f,10.6f,11.32f),new Vector3(.45f,.15f,.1f),warm);
                for(int j=0;j<5;j++) Part(p,"Vent slat",new Vector3(i*3.5f,2.8f+j*.25f,11.4f),new Vector3(2.6f,.07f,.08f),black,default,false);
            }
            for(int s=-1;s<=1;s+=2)
            {
                Part(p,"Maintenance gantry",new Vector3(s*6.5f,4.3f,5),new Vector3(3.7f,.22f,3),steel);
                for(int i=0;i<4;i++) Part(p,"Gantry railing",new Vector3(s*(5+i*.9f),4.95f,3.6f),new Vector3(.07f,1.2f,.07f),gold,default,false);
                Part(p,"Gantry rail top",new Vector3(s*6.4f,5.54f,3.6f),new Vector3(3.7f,.07f,.07f),gold,default,false);
                Part(p,"Supply crate",new Vector3(s*6.5f,.7f,-1),new Vector3(1.7f,1.4f,1.5f),wall);
                Part(p,"Crate stripe",new Vector3(s*6.5f,.7f,-1.78f),new Vector3(1.25f,.11f,.03f),gold,default,false);
                Part(p,"Landing guide",new Vector3(s*5,.006f,-5),new Vector3(.06f,.02f,7),warm,default,false);
            }
            var key=LightAt("Main softbox",new Vector3(-5,11,-7),new Color(.77f,.88f,1),200,25,p);
            key.type=LightType.Spot; key.spotAngle=80; key.transform.LookAt(new Vector3(0,4,0)); key.shadows=LightShadows.Soft;
            var fill=LightAt("Front fill",new Vector3(4,7,-9),new Color(.42f,.7f,1),80,22,p); fill.type=LightType.Spot; fill.spotAngle=90; fill.transform.LookAt(new Vector3(0,4,0));
            LightAt("Cyan rim",new Vector3(4,6,3),new Color(.05f,.65f,1),70,15,p);
            LightAt("Warm rim",new Vector3(-5,4,2),new Color(1,.52f,.2f),65,15,p);
            LightAt("Ceiling bounce",new Vector3(0,11,3),new Color(.55f,.75f,1),45,20,p);
            var sun=new GameObject("Hangar ambient directional").AddComponent<Light>(); sun.transform.SetParent(p,false); sun.type=LightType.Directional; sun.color=new Color(.66f,.76f,.87f); sun.intensity=.7f; sun.transform.rotation=Quaternion.Euler(35,-25,0); sun.shadows=LightShadows.Soft;
        }
        static Light LightAt(string name,Vector3 pos,Color c,float intensity,float range,Transform parent)
        {
            var l=new GameObject(name).AddComponent<Light>(); l.transform.SetParent(parent,false); l.transform.localPosition=pos; l.type=LightType.Point; l.color=c; l.intensity=intensity; l.range=range; return l;
        }
        static Camera BuildCamera()
        {
            var c=new GameObject("Lobby Camera").AddComponent<Camera>(); c.tag="MainCamera";
            c.transform.position=new Vector3(.2f,4.25f,-17.8f); c.transform.LookAt(new Vector3(0,4.75f,0));
            c.fieldOfView=37; c.nearClipPlane=.1f; c.farClipPlane=100; c.clearFlags=CameraClearFlags.SolidColor; c.backgroundColor=new Color(.018f,.035f,.055f);
            c.gameObject.AddComponent<AudioListener>();
            var data=c.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>(); data.renderPostProcessing=true;
            var volume=new GameObject("Lobby Post Processing").AddComponent<Volume>(); volume.isGlobal=true;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();
            var bloom=profile.Add<UnityEngine.Rendering.Universal.Bloom>(); bloom.intensity.Override(.2f); bloom.threshold.Override(1.6f); bloom.scatter.Override(.55f);
            var tonemap=profile.Add<UnityEngine.Rendering.Universal.Tonemapping>(); tonemap.mode.Override(UnityEngine.Rendering.Universal.TonemappingMode.ACES);
            var vignette=profile.Add<UnityEngine.Rendering.Universal.Vignette>(); vignette.intensity.Override(.24f); vignette.smoothness.Override(.7f);
            AssetDatabase.CreateAsset(profile,Root+"/Materials/Lobby Volume.asset"); volume.sharedProfile=profile;
            return c;
        }
        static Texture2D RenderThumbnail(Transform model,bool portrait)
        {
            var go=new GameObject("Thumbnail camera"); var c=go.AddComponent<Camera>();
            go.transform.position=portrait?new Vector3(-.15f,7.95f,-3):new Vector3(-1.8f,5.6f,-14);
            go.transform.LookAt(portrait?new Vector3(0,7.7f,0):new Vector3(0,4.5f,0));
            c.fieldOfView=portrait?30:39; c.clearFlags=CameraClearFlags.SolidColor; c.backgroundColor=new Color(.015f,.05f,.075f,1);
            var rt=new RenderTexture(portrait?256:384,portrait?256:448,24); c.targetTexture=rt;
            c.Render(); var prev=RenderTexture.active; RenderTexture.active=rt;
            var texture=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false); texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0); texture.Apply();
            string path=Root+"/Textures/"+(portrait?"MechPortrait":"MechThumbnail")+".png"; File.WriteAllBytes(path,texture.EncodeToPNG());
            RenderTexture.active=prev; c.targetTexture=null; rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(go);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h,Vector2? anchor=null)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=anchor??new Vector2(0,1); r.pivot=new Vector2(0,1); r.anchoredPosition=new Vector2(x,-y); r.sizeDelta=new Vector2(w,h); return r;
        }
        static LobbyShape Shape(Transform parent,string name,float x,float y,float w,float h,Color color,LobbyShape.Geometry geometry=LobbyShape.Geometry.Panel,Color? edge=null,float thickness=1)
        {
            var r=Rect(name,parent,x,y,w,h); r.gameObject.AddComponent<CanvasRenderer>(); var g=r.gameObject.AddComponent<LobbyShape>(); g.geometry=geometry; g.color=color; g.border=edge??line; g.thickness=thickness; g.raycastTarget=false; return g;
        }
        static TMP_Text Text(Transform p,string name,string content,float x,float y,float w,float h,float size,Color? c=null,FontStyles style=FontStyles.Normal,TextAlignmentOptions align=TextAlignmentOptions.Left)
        {
            var r=Rect(name,p,x,y,w,h); var t=r.gameObject.AddComponent<TextMeshProUGUI>(); t.font=font; t.text=content; t.fontSize=size; t.color=c??white; t.fontStyle=style; t.alignment=align; t.raycastTarget=false; t.textWrappingMode=TextWrappingModes.NoWrap; t.overflowMode=TextOverflowModes.Ellipsis; t.margin=Vector4.zero; return t;
        }
        static void Rule(Transform p,float x,float y,float width,Color? c=null)
        {
            var g=Shape(p,"Hairline",x,y,width,1,c??line,edge:Color.clear,thickness:0); g.cut=0;
        }
        static Button Button(Transform p,string name,string label,float x,float y,float w,float h,bool primary=false,bool selected=false,float size=18,LobbyShape.Geometry geometry=LobbyShape.Geometry.Panel)
        {
            Color normal=primary?new Color(.025f,.25f,.36f,.92f):selected?new Color(.025f,.3f,.44f,.9f):panel;
            var g=Shape(p,name,x,y,w,h,normal,geometry,primary||selected?cyan:line,primary?1.5f:1);
            g.raycastTarget=true;
            var b=g.gameObject.AddComponent<Button>(); b.targetGraphic=g; b.transition=Selectable.Transition.None;
            var hover=g.gameObject.AddComponent<LobbyHover>(); hover.surface=g; hover.normal=normal;
            var action=g.gameObject.AddComponent<LobbyButtonAction>(); action.actionLabel=string.IsNullOrEmpty(label)?name:label;
            Text(g.transform,"Label",label,12,0,w-24,h,size,primary||selected?white:muted,primary?FontStyles.Bold:FontStyles.Normal,TextAlignmentOptions.Center);
            return b;
        }
        static RawImage Raw(Transform p,string name,Texture texture,float x,float y,float w,float h)
        {
            var r=Rect(name,p,x,y,w,h); var raw=r.gameObject.AddComponent<RawImage>(); raw.texture=texture; raw.raycastTarget=false; return raw;
        }
        static Image SpriteSlot(Transform p,string name,float x,float y,float w,float h)
        {
            var r=Rect(name,p,x,y,w,h); var img=r.gameObject.AddComponent<Image>(); img.preserveAspect=true; img.raycastTarget=false; img.enabled=false; return img;
        }
        static RectTransform PGroup(Transform parent,string name,float x,float y,float w,float h,Vector2? anchor=null)
        {
            var r=Rect(name,parent,x,y,w,h,anchor); prefabGroups.Add(r); return r;
        }
        static void BuildUI(Texture2D thumb,Texture2D avatar)
        {
            var canvas=new GameObject("Home Lobby Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=1;
            var t=canvas.transform;
            var eventGO=new GameObject("Lobby EventSystem",typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventGO.AddComponent<InputSystemUIInputModule>();
#else
            eventGO.AddComponent<StandaloneInputModule>();
#endif
            // All background decoration is non-raycasting; only actual controls block robot dragging.
            Shape(t,"Left information veil",0,88,465,916,new Color(.008f,.018f,.03f,.68f),edge:Color.clear,thickness:0);
            var rightVeil=Shape(t,"Right information veil",-464,88,464,916,new Color(.008f,.018f,.03f,.6f),edge:Color.clear,thickness:0);
            rightVeil.rectTransform.anchorMin=rightVeil.rectTransform.anchorMax=new Vector2(1,1);
            var header=PGroup(t,"Header",0,0,1920,90);
            // Stretch the header and footer horizontally for ultrawide displays.
            header.anchorMax=new Vector2(1,1); header.sizeDelta=new Vector2(0,90);
            Shape(header,"Top strip",0,0,1920,86,new Color(.005f,.014f,.023f,.95f),edge:Color.clear,thickness:0).rectTransform.anchorMax=new Vector2(1,1);
            var topBg=header.Find("Top strip").GetComponent<RectTransform>(); topBg.sizeDelta=new Vector2(0,86);
            Raw(header,"Player portrait placeholder",avatar,28,14,60,60);
            controller.avatarImage=SpriteSlot(header,"Replace avatar",28,14,60,60);
            controller.playerLabel=Text(header,"Player name","COMMANDER",103,15,215,28,21,white,FontStyles.Bold);
            Text(header,"Rank","LV. 20  /  ORBITAL DIVISION",103,45,240,20,12,muted);
            Shape(header,"Status dot",105,72,5,5,cyan,LobbyShape.Geometry.Diamond,Color.clear,0);
            var nav=Rect("Navigation",header,-415,0,830,86,new Vector2(.5f,1));
            string[] tabs={"CĂN CỨ","GUNDAM","ĐỘI HÌNH","CHIẾN DỊCH","KHO ĐỒ"};
            for(int i=0;i<tabs.Length;i++) Button(nav,"Nav "+i,tabs[i],i*164,12,162,60,false,i==0,17,LobbyShape.Geometry.Tab);
            var money=Rect("Currencies",header,-363,20,335,48,new Vector2(1,1));
            string[] moneyValue={"120 / 180","128,640","1,960"}; string[] symbols={"E","C","◆"};
            for(int i=0;i<3;i++)
            {
                Shape(money,"Currency icon",i*113,9,24,24,i==1?new Color(.7f,.47f,.1f,.35f):new Color(.06f,.4f,.5f,.35f),LobbyShape.Geometry.Hexagon,i==1?new Color(.9f,.65f,.2f):cyan);
                Text(money,"Currency symbol",symbols[i],i*113,9,24,24,12,i==1?new Color(.95f,.75f,.35f):cyan,FontStyles.Bold,TextAlignmentOptions.Center);
                Text(money,"Currency value",moneyValue[i],i*113+30,9,88,26,15,white,FontStyles.Bold);
            }
            var rule=Shape(header,"Header underline",0,85,1920,1,line,edge:Color.clear,thickness:0); rule.rectTransform.anchorMax=new Vector2(1,1); rule.rectTransform.sizeDelta=new Vector2(0,1);
            var sub=Rect("Hangar heading",t,-330,107,660,35,new Vector2(.5f,1));
            Text(sub,"Breadcrumb","CĂN CỨ    /    NHÀ CHỨA 01",0,0,660,24,13,muted,FontStyles.Normal,TextAlignmentOptions.Center);

            var left=PGroup(t,"Gundam Information",40,142,370,770);
            Text(left,"Eyebrow","MOBILE SUIT  /  ACTIVE UNIT",0,0,360,22,12,muted);
            Button(left,"Switch Gundam","ĐỔI GUNDAM     ›",0,38,366,55,true,false,18);
            Text(left,"Serial","RX SERIES  ·  EARTH FEDERATION",0,128,365,20,12,cyan);
            controller.nameLabel=Text(left,"Gundam Name",controller.gundamName,0,157,385,56,38,white,FontStyles.Bold);
            controller.roleLabel=Text(left,"Role and level","TẤN CÔNG   /   Lv. 20",2,219,365,30,17,muted);
            Rule(left,0,268,366);
            Text(left,"Stats heading","THÔNG SỐ CHIẾN ĐẤU",0,291,366,24,12,muted);
            controller.statLabels=new TMP_Text[4]; string[] statNames={"HP","ATK","DEF","SPD"};
            for(int i=0;i<4;i++)
            {
                int col=i%2,row=i/2; float x=col*190,y=333+row*72;
                Shape(left,"Stat bar",x,y+3,2,44,cyan,edge:Color.clear,thickness:0);
                Text(left,"Stat "+statNames[i],statNames[i],x+14,y,145,20,12,muted);
                controller.statLabels[i]=Text(left,"Value "+statNames[i],controller.statValues[i],x+14,y+22,145,35,25,white,FontStyles.Bold);
            }
            Rule(left,0,493,366);
            Text(left,"Skills heading","HỆ THỐNG VŨ KHÍ",0,515,366,24,12,muted);
            string[] names={"ĐÁNH THƯỜNG","KỸ NĂNG","TUYỆT KỸ"}; string[] marks={"I","II","III"};
            for(int i=0;i<3;i++)
            {
                var b=Button(left,"Skill "+i,"",i*124,557,100,82,false,false,16,LobbyShape.Geometry.Panel);
                Shape(b.transform,"Skill diamond",29,14,42,42,new Color(.04f,.2f,.29f,.8f),LobbyShape.Geometry.Diamond,cyan);
                Text(b.transform,"Skill glyph",marks[i],26,12,48,46,22,cyan,FontStyles.Bold,TextAlignmentOptions.Center);
                skills.Add(SpriteSlot(b.transform,"Replace skill icon",22,8,56,56));
                Text(left,"Skill caption",names[i],i*124-4,649,112,20,11,muted,FontStyles.Normal,TextAlignmentOptions.Center);
            }
            Button(left,"Details","CHI TIẾT GUNDAM     ›",0,704,366,44,false,false,15);
            Text(left,"Prototype note","HANGAR PREVIEW  /  PROTOTYPE 01",0,763,370,18,10,new Color(.32f,.46f,.52f));

            var right=PGroup(t,"Activities",-426,142,386,846,new Vector2(1,1));
            var banner=Button(right,"Event banner","",0,0,386,176);
            Shape(banner.transform,"Banner grid",0,0,386,176,new Color(.08f,.37f,.48f,.16f),LobbyShape.Geometry.Grid);
            Raw(banner.transform,"Banner mech",thumb,229,0,151,176);
            controller.bannerImage=SpriteSlot(banner.transform,"Replace banner image",0,0,386,176);
            Text(banner.transform,"Event tag","SỰ KIỆN GIỚI HẠN",20,19,310,24,11,cyan);
            Text(banner.transform,"Event title","ORBITAL\nAWAKENING",20,52,250,76,25,white,FontStyles.Bold);
            Text(banner.transform,"Event subtitle","TÍN HIỆU ĐÃ ĐƯỢC KÍCH HOẠT",20,143,310,18,10,muted);
            for(int i=0;i<5;i++) Shape(right,"Banner page",298+i*16,190,9,4,i==0?cyan:line,edge:Color.clear,thickness:0);
            Text(right,"Mission heading","NHIỆM VỤ HÔM NAY",0,231,270,27,16,white,FontStyles.Bold);
            Text(right,"Mission count","01 / 03",294,233,90,24,12,cyan,align:TextAlignmentOptions.Right);
            Rule(right,0,270,386,cyan);
            string[] missions={"Đăng nhập căn cứ","Hoàn thành chiến dịch","Nâng cấp một Gundam"};
            string[] progress={"1 / 1","0 / 3","0 / 1"};
            for(int i=0;i<3;i++)
            {
                var b=Button(right,"Mission "+i,"",0,285+i*96,386,84);
                Shape(b.transform,"Task marker",16,23,30,30,new Color(.02f,.15f,.2f,.8f),LobbyShape.Geometry.Diamond,i==0?cyan:line);
                Text(b.transform,"Task number",(i+1).ToString("00"),15,23,32,30,11,i==0?cyan:muted,align:TextAlignmentOptions.Center);
                Text(b.transform,"Task title",missions[i],63,14,310,28,17,white);
                Text(b.transform,"Task status",i==0?"HOÀN THÀNH":"ĐANG THỰC HIỆN",63,49,230,18,10,i==0?cyan:muted);
                Text(b.transform,"Task progress",progress[i],293,47,72,20,13,muted,align:TextAlignmentOptions.Right);
                if(i==0) Rule(b.transform,63,75,289,cyan);
            }
            Text(right,"Campaign eyebrow","TIẾP TỤC CHIẾN DỊCH",0,627,386,22,12,muted);
            Rule(right,0,661,386);
            Text(right,"Chapter","CHƯƠNG 01",0,682,386,23,12,cyan);
            Text(right,"Chapter title","TÍN HIỆU TỪ QUỸ ĐẠO",0,712,386,35,23,white,FontStyles.Bold);
            var launch=Button(right,"Launch","XUẤT KÍCH     ›",0,774,386,72,true,false,26);
            Shape(launch.transform,"Launch accent",0,0,5,72,cyan,edge:Color.clear,thickness:0);

            var team=PGroup(t,"Team Strip",-324,-176,648,112,new Vector2(.5f,0));
            Text(team,"Team eyebrow","ĐỘI HÌNH XUẤT KÍCH",0,-28,648,23,11,muted);
            for(int i=0;i<4;i++)
            {
                var b=Button(team,"Team Slot "+(i+1),"",i*164,0,156,96,false,i==0);
                if(i<3)
                {
                    var raw=Raw(b.transform,"Unit thumbnail",thumb,2,2,78,91); raw.color=i==1?new Color(.6f,.83f,1):i==2?new Color(.8f,.76f,.68f):Color.white;
                    Text(b.transform,"Unit number","0"+(i+1),110,9,36,22,11,cyan,align:TextAlignmentOptions.Right);
                    Text(b.transform,"Unit name",new[]{"AEGIS","STRIKER","BASTION"}[i],75,38,76,24,11,white,FontStyles.Bold);
                    Text(b.transform,"Unit level","Lv. "+(20-i*2),78,67,70,20,11,muted);
                    if(i==0) Rule(b.transform,0,94,156,cyan);
                }
                else { Text(b.transform,"Empty plus","+",0,11,156,48,34,muted,align:TextAlignmentOptions.Center); Text(b.transform,"Empty caption","VỊ TRÍ TRỐNG",0,65,156,19,10,muted,align:TextAlignmentOptions.Center); }
            }
            var preview=Rect("Preview hint",t,-210,-225,420,25,new Vector2(.5f,0));
            Text(preview,"Hint","‹    KÉO CHUỘT ĐỂ XOAY GUNDAM    ›",0,0,420,25,11,muted,align:TextAlignmentOptions.Center);
            var footer=PGroup(t,"Footer",0,-52,1920,52,new Vector2(0,0)); footer.anchorMax=new Vector2(1,0); footer.sizeDelta=new Vector2(0,52);
            var footBg=Shape(footer,"Footer background",0,0,1920,52,new Color(.005f,.014f,.023f,.94f),edge:Color.clear,thickness:0); footBg.rectTransform.anchorMax=new Vector2(1,1); footBg.rectTransform.sizeDelta=new Vector2(0,52);
            Button(footer,"Mail","THƯ",25,8,112,35,false,false,13);
            Button(footer,"Quests","NHIỆM VỤ",147,8,156,35,false,false,13);
            Text(footer,"System status","●  HỆ THỐNG SẴN SÀNG     /     HANGAR 01",335,12,600,26,11,muted);
            var settings=Button(footer,"Settings","CÀI ĐẶT",-182,8,156,35,false,false,13); var sr=settings.GetComponent<RectTransform>(); sr.anchorMin=sr.anchorMax=new Vector2(1,1);
            var toast=Shape(t,"Feedback Toast",-290,155,580,52,new Color(.025f,.14f,.2f,.97f),edge:cyan);
            toast.rectTransform.anchorMin=toast.rectTransform.anchorMax=new Vector2(.5f,1);
            controller.toast=toast.gameObject.AddComponent<CanvasGroup>(); controller.toast.alpha=0; controller.toast.blocksRaycasts=false;
            controller.toastLabel=Text(toast.transform,"Message","Chức năng đang phát triển",12,0,556,52,17,white,align:TextAlignmentOptions.Center);
        }
        static string Guide()
        {
            return "# Gundam Home Lobby\n\nMở `Assets/GundamLobby/Scenes/HomeLobby.unity`, chọn Game rồi nhấn Play. Đây chỉ là giao diện trang chủ; các nút hiện thông báo mẫu. Kéo chuột tại vùng trống ở giữa để xoay Gundam.\n\n## Thay asset\nChọn `Lobby Systems` và component `HomeLobbyController`:\n- `Gundam Prefab`: kéo prefab Gundam thật vào. Prefab dùng đơn vị mét, chân ở y=0, cao khoảng 8.8 đơn vị, mặt hướng -Z. Placeholder tự ẩn khi Play.\n- `Hangar Prefab`: thay nhà chứa nếu muốn; placeholder tự ẩn khi Play.\n- `Avatar Sprite`, `Banner Sprite`, `Skill Sprites`: kéo sprite tương ứng vào các trường Inspector.\n- Chỉnh tên, cấp và bốn chỉ số ở Sample Data.\n- Thumbnail mẫu ở Textures và các RawImage trong prefab Team Strip, Header, Activities có thể thay trực tiếp. Ba thumbnail đội hình hiện dùng cùng một model placeholder, không đại diện ba asset Gundam riêng.\n\n## Cấu trúc\nRuntime chứa controller, hover và hình học HUD. Editor chứa công cụ dựng scene (Tools > Gundam Lobby > Create Home Lobby, chỉ dùng khi chưa có scene). Prefabs chứa Header, Footer, Gundam Information, Activities, Team Strip, PlaceholderGundam và PlaceholderHangar. UI là uGUI/TextMeshPro có thể sửa trực tiếp, không phải ảnh nền chụp màn hình.\n\n## Giới hạn\nGundam, avatar, banner và hangar là placeholder hình học gốc dựng trong project; cần model và artwork thật để đạt chi tiết như ảnh tham chiếu. Chưa có gameplay, inventory, nâng cấp hay scene phụ. Pipeline URP hiện có không bị đổi. Scene AR cũ có thay đổi chưa lưu được chụp bản sao vào Backups trước khi tạo lobby; file scene AR gốc không bị ghi đè. Build Settings của project không bị thay đổi.\n";
        }
    }
}
