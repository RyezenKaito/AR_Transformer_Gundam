using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GundamLobby.Editor
{
    public static class BattleScreenBuilder
    {
        const string Root="Assets/GundamLobby/Battle";
        static TMP_FontAsset font;static BattleController owner;
        static Material hologram,steel,dark,cyan,amber;
        static Color C(string s){ColorUtility.TryParseHtmlString("#"+s,out var color);return color;}
        static Color A(string s,float alpha){var c=C(s);c.a=alpha;return c;}
        static RectTransform Rect(string name,Transform p,float x,float y,float w,float h)
        {
            var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(p,false);rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;
        }
        static LobbyShape Shape(string name,Transform p,float x,float y,float w,float h,Color fill,Color border,LobbyShape.Geometry geometry=LobbyShape.Geometry.Panel)
        {
            var rt=Rect(name,p,x,y,w,h);rt.gameObject.AddComponent<CanvasRenderer>();var s=rt.gameObject.AddComponent<LobbyShape>();s.geometry=geometry;s.color=fill;s.border=border;s.thickness=border.a>0?1:0;s.raycastTarget=false;return s;
        }
        static TMP_Text Label(string name,string value,Transform p,float x,float y,float w,float h,float size,Color color,TextAlignmentOptions align=TextAlignmentOptions.Left)
        {
            var rt=Rect(name,p,x,y,w,Mathf.Max(h,Mathf.Ceil(size*1.25f)));var t=rt.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.raycastTarget=false;t.margin=Vector4.zero;t.alignment=align;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Ellipsis;return t;
        }
        static Button Button(string name,string text,Transform p,float x,float y,float w,float h,UnityAction action,bool primary=false)
        {
            var s=Shape(name,p,x,y,w,h,primary?C("087CAB"):C("071117"),primary?C("20C8F6"):C("214654"),primary?LobbyShape.Geometry.Tab:LobbyShape.Geometry.Panel);s.raycastTarget=true;
            var b=s.gameObject.AddComponent<Button>();b.targetGraphic=s;b.transition=Selectable.Transition.None;
            var hover=s.gameObject.AddComponent<LobbyHover>();hover.surface=s;hover.normal=s.color;hover.hover=C("14516A");hover.pressed=C("167EA1");hover.duration=.18f;
            Label("Label",text,b.transform,12,0,w-24,h,primary?29:21,C("E7F0F4"),TextAlignmentOptions.Center);
            if(action!=null)UnityEventTools.AddPersistentListener(b.onClick,action);return b;
        }
        static void Rule(Transform p,float x,float y,float w){Shape("Rule",p,x,y,w,1,C("214654"),Color.clear,LobbyShape.Geometry.Box);}
        static Material Material(string name,string color,bool light=false)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.SetColor("_BaseColor",C(color));m.SetFloat("_Metallic",light?0:.4f);m.SetFloat("_Smoothness",.25f);
            if(light){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",C(color)*.8f);}
            AssetDatabase.CreateAsset(m,Root+"/Materials/"+name+".mat");return m;
        }
        static GameObject Part(PrimitiveType type,string name,Transform p,Vector3 pos,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(p,false);go.transform.localPosition=pos;go.transform.localScale=scale;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());var r=go.GetComponent<Renderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return go;
        }
        static GameObject MeshObject(string name,Transform p,Mesh mesh)
        {
            AssetDatabase.CreateAsset(mesh,Root+"/Meshes/"+name+".asset");var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(p,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=hologram;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return go;
        }
        static float Height(float x,float z)
        {
            float edge=Mathf.Clamp01((1-Mathf.Max(Mathf.Abs(x)/10,Mathf.Abs(z)/6))*3);
            float a=Mathf.Exp(-((x+3)*(x+3)+(z-1)*(z-1))/12)*1.15f;
            float b=Mathf.Exp(-((x-4)*(x-4)+(z+1)*(z+1))/7)*.85f;
            return edge*(.12f+Mathf.PerlinNoise(x*.34f+11,z*.34f+7)*.55f+a+b);
        }
        static void Ring(Transform p,string name,float radius,float y,Color color)
        {
            var verts=new Vector3[128];var indices=new int[256];var colors=new Color[128];for(int i=0;i<128;i++){float angle=i*Mathf.PI/64;verts[i]=new Vector3(Mathf.Sin(angle)*radius,y,Mathf.Cos(angle)*radius);colors[i]=color;indices[i*2]=i;indices[i*2+1]=(i+1)%128;}
            var mesh=new Mesh{name=name};mesh.vertices=verts;mesh.colors=colors;mesh.SetIndices(indices,MeshTopology.Lines,0);MeshObject(name,p,mesh);
        }
        static void BuildStage()
        {
            hologram=new Material(Shader.Find("GundamLobby/BattleHologram"));hologram.name="Holographic Cyan";hologram.SetColor("_BaseColor",new Color(.15f,.85f,1,.7f));AssetDatabase.CreateAsset(hologram,Root+"/Materials/Holographic Cyan.mat");
            steel=Material("Tactical Table Steel","182B37");dark=Material("Tactical Table Recess","071119");cyan=Material("Table Cyan Indicator","148FB5",true);amber=Material("Table Amber Markings","B6944E",true);
            owner.stage=new GameObject("Battle Holographic Tactical Stage");var table=new GameObject("Tactical Table").transform;table.SetParent(owner.stage.transform,false);table.localPosition=new Vector3(0,0,-2.5f);table.localScale=new Vector3(1,1,.68f);
            Part(PrimitiveType.Cylinder,"Lower plinth",table,new Vector3(0,.75f,0),new Vector3(20,.4f,20),dark);
            Part(PrimitiveType.Cylinder,"Armored table rim",table,new Vector3(0,1.25f,0),new Vector3(22,.2f,22),steel);
            Part(PrimitiveType.Cylinder,"Projection surface",table,new Vector3(0,1.54f,0),new Vector3(21.6f,.09f,21.6f),dark);
            Ring(table,"Projection ring outer",10.78f,1.66f,new Color(.08f,.65f,.95f,.7f));Ring(table,"Projection ring inner",10.1f,1.67f,new Color(.08f,.65f,.95f,.3f));
            for(int i=0;i<24;i++)
            {
                float a=i*Mathf.PI/12;var pos=new Vector3(Mathf.Sin(a)*10.65f,1.57f,Mathf.Cos(a)*10.65f);
                var go=Part(PrimitiveType.Cube,"Rim indicator "+i,table,pos,new Vector3(.72f,.06f,.17f),i%3==0?amber:cyan);go.transform.localRotation=Quaternion.Euler(0,i*15,0);
            }
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Part(PrimitiveType.Cube,"Table support",table,new Vector3(x*7,.25f,z*6),new Vector3(1.8f,.5f,2),steel);
            var map=new GameObject("Rotating Holographic Terrain").transform;map.SetParent(owner.stage.transform,false);map.localPosition=new Vector3(0,2.05f,-2.5f);owner.mapPivot=map;
            const int nx=81,nz=49;var vertices=new Vector3[nx*nz];var fillColors=new Color[vertices.Length];var wireColors=new Color[vertices.Length];var triangles=new List<int>();var lines=new List<int>();
            for(int z=0;z<nz;z++)for(int x=0;x<nx;x++)
            {
                int i=z*nx+x;float px=-10+20*x/(nx-1f),pz=-6+12*z/(nz-1f),h=Height(px,pz);vertices[i]=new Vector3(px,h,pz);
                float edge=Mathf.Clamp01((1-Mathf.Sqrt(px*px/(10.7f*10.7f)+pz*pz/(7.25f*7.25f)))*9);
                fillColors[i]=new Color(.05f,.63f,.88f,(.045f+h*.035f)*edge);wireColors[i]=new Color(.08f,.68f,.94f,(.24f+h*.11f)*edge);
                if(x<nx-1){lines.Add(i);lines.Add(i+1);}if(z<nz-1){lines.Add(i);lines.Add(i+nx);}
                if(x<nx-1&&z<nz-1){triangles.AddRange(new[]{i,i+nx,i+1,i+1,i+nx,i+nx+1});}
            }
            var surface=new Mesh{name="Holographic terrain surface"};surface.vertices=vertices;surface.colors=fillColors;surface.triangles=triangles.ToArray();surface.RecalculateNormals();MeshObject("Holographic Terrain Surface",map,surface);
            var grid=new Mesh{name="Holographic terrain grid"};grid.vertices=vertices;grid.colors=wireColors;grid.SetIndices(lines.ToArray(),MeshTopology.Lines,0);MeshObject("Holographic Terrain Grid",map,grid);
            var points=owner.catalog.locationPoints;var beamVerts=new List<Vector3>();var beamColors=new List<Color>();var beamIndices=new List<int>();
            for(int i=0;i<points.Length;i++)
            {
                var point=points[i];Part(PrimitiveType.Cylinder,"Deployment beacon "+i,map,point+Vector3.up*.07f,new Vector3(.52f,.04f,.52f),cyan);
                for(int axis=0;axis<2;axis++)
                {
                    int start=beamVerts.Count;Vector3 side=axis==0?Vector3.right*.45f:Vector3.forward*.45f;
                    beamVerts.Add(point-side);beamVerts.Add(point+side);beamVerts.Add(point+side+Vector3.up*2.4f);beamVerts.Add(point-side+Vector3.up*2.4f);
                    beamColors.Add(new Color(.12f,.75f,1,.28f));beamColors.Add(new Color(.12f,.75f,1,.28f));beamColors.Add(new Color(.12f,.75f,1,0));beamColors.Add(new Color(.12f,.75f,1,0));beamIndices.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
                }
            }
            var beams=new Mesh{name="Deployment light beams"};beams.SetVertices(beamVerts);beams.SetColors(beamColors);beams.SetTriangles(beamIndices,0);MeshObject("Deployment Light Beams",map,beams);
            owner.modelMount=new GameObject("Replaceable Battle Gundam Mount");owner.modelMount.transform.SetParent(owner.stage.transform,false);owner.modelMount.transform.localPosition=new Vector3(0,0,5);owner.modelMount.transform.localRotation=Quaternion.Euler(0,-8,0);
            owner.stage.SetActive(false);
        }
        static void BuildUI(HomeLobbyDashboard dashboard)
        {
            var canvas=new GameObject("Battle Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<Canvas>().sortingOrder=70;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;owner.battleCanvas=canvas;
            var root=Rect("Battle Screen",canvas.transform,0,0,1920,1080);root.anchorMin=root.anchorMax=root.pivot=new Vector2(.5f,.5f);root.anchoredPosition=Vector2.zero;
            Shape("Left information backing",root,0,96,366,932,A("071117",.88f),Color.clear,LobbyShape.Geometry.Box);
            var left=Shape("Left readability",root,366,96,160,932,A("071117",.88f),Color.clear,LobbyShape.Geometry.Box);left.gradient=true;left.gradientEnd=A("071117",0);
            Shape("Mode list readability",root,1442,96,478,932,A("061015",.96f),Color.clear,LobbyShape.Geometry.Box);
            var drag=Rect("Drag to rotate holographic map",root,366,268,1056,750);drag.gameObject.AddComponent<Image>().color=Color.clear;drag.gameObject.AddComponent<BattleMapDrag>().owner=owner;
            var headerPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GundamLobby/Weaponry/Prefabs/CommandNavigationBar.prefab");var header=(GameObject)PrefabUtility.InstantiatePrefab(headerPrefab,root);header.name="Battle Command Header";var nav=header.GetComponent<LobbyNavigationBar>();nav.dashboard=dashboard;nav.page=LobbyNavigationBar.Page.Battle;nav.Refresh();
            Label("Battle heading","TRUNG TÂM TÁC CHIẾN",root,32,125,340,35,22,C("829EAC"));
            owner.codeLabel=Label("Mode code","TÁC CHIẾN THEO LƯỢT / C01",root,32,174,340,28,17,C("20C8F6"));
            owner.modeLabel=Label("Mode title","CHIẾN DỊCH",root,30,208,342,60,39,C("E7F0F4"));owner.modeLabel.enableAutoSizing=true;owner.modeLabel.fontSizeMin=29;owner.modeLabel.fontSizeMax=39;
            owner.descriptionLabel=Label("Mode description","",root,32,284,320,145,21,C("ADC1CB"));owner.descriptionLabel.textWrappingMode=TextWrappingModes.Normal;
            Rule(root,32,448,320);
            Label("Recommended heading","CẤP KHUYẾN NGHỊ",root,32,470,240,35,18,C("829EAC"));owner.levelLabel=Label("Recommended level","20",root,282,466,70,40,28,C("E7F0F4"),TextAlignmentOptions.Right);
            Label("Difficulty heading","ĐỘ KHÓ",root,32,517,110,32,18,C("829EAC"));owner.difficultyLabel=Label("Difficulty value","TIÊU CHUẨN",root,151,514,201,35,23,C("20C8F6"),TextAlignmentOptions.Right);
            Rule(root,32,560,320);Label("Location heading","ĐỊA ĐIỂM TRIỂN KHAI",root,32,581,320,26,17,C("829EAC"));
            owner.locationLabel=Label("Selected location","CẢNG BÌNH MINH",root,32,614,320,65,27,C("E7F0F4"));owner.locationLabel.textWrappingMode=TextWrappingModes.Normal;
            var unit=Shape("Equipped Gundam",root,32,718,320,134,A("102A37",.85f),C("214654"));
            Label("Gundam caption","GUNDAM ĐANG CHỌN",unit.transform,16,9,288,26,17,C("829EAC"));
            var pic=Rect("Gundam portrait",unit.transform,12,39,295,52).gameObject.AddComponent<Image>();pic.preserveAspect=false;pic.raycastTarget=false;owner.unitPortrait=pic;
            owner.unitLabel=Label("Gundam name","UNICORN / CẤP 20",unit.transform,16,96,288,29,20,C("E7F0F4"));owner.unitLabel.enableAutoSizing=true;owner.unitLabel.fontSizeMin=16;owner.unitLabel.fontSizeMax=20;
            Label("Map caption","BẢN ĐỒ TRIỂN KHAI",root,456,124,850,30,18,C("72ACBF"),TextAlignmentOptions.Center);
            var pinArea=Rect("Projected Location Labels",root,366,268,1056,750);pinArea.gameObject.AddComponent<RectMask2D>();owner.pins=new BattleMapPin[owner.catalog.locationNames.Length];
            for(int i=0;i<owner.pins.Length;i++)
            {
                var b=Button("Deploy Location "+i,owner.catalog.locationNames[i],pinArea,0,0,230,38,null);var rt=b.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);
                b.GetComponentInChildren<TMP_Text>().fontSize=18;var pin=b.gameObject.AddComponent<BattleMapPin>();pin.owner=owner;pin.index=i;pin.label=b.GetComponentInChildren<TMP_Text>();pin.hover=b.GetComponent<LobbyHover>();owner.pins[i]=pin;
            }
            Label("Mode section heading","CHỌN CHẾ ĐỘ",root,1468,126,330,35,23,C("E7F0F4"));Label("Mode count","05",root,1800,126,92,35,23,C("20C8F6"),TextAlignmentOptions.Right);Rule(root,1468,170,424);
            BuildCards(root);
            Button("Battle Change Gundam","ĐỔI GUNDAM",root,1468,801,424,42,owner.OpenStrikers);
            owner.normalButton=Button("Normal Difficulty","TIÊU CHUẨN",root,1468,854,208,40,owner.NormalDifficulty);owner.hardButton=Button("Advanced Difficulty","NÂNG CAO",root,1684,854,208,40,owner.AdvancedDifficulty);
            owner.energyLabel=Label("Energy cost","TIÊU HAO: 20 NĂNG LƯỢNG",root,1468,902,424,25,17,C("829EAC"),TextAlignmentOptions.Center);
            var launch=Button("Battle Launch","XUẤT KÍCH   >",root,1468,934,424,77,owner.Launch,true);owner.launchLabel=launch.GetComponentInChildren<TMP_Text>();owner.launchLabel.fontSize=37;
            var footer=Shape("Battle Footer",root,0,1028,1920,52,A("071117",.98f),Color.clear,LobbyShape.Geometry.Box);Rule(footer.transform,24,0,1872);
            Label("Map control hint","KÉO: XOAY BẢN ĐỒ   /   CHỌN ĐIỂM TRIỂN KHAI",footer.transform,430,9,1030,35,18,C("829EAC"),TextAlignmentOptions.Center);
            Button("Battle Return","Esc   QUAY LẠI",footer.transform,1680,10,212,33,owner.Close);
            var toast=Shape("Battle Toast",root,531,844,850,82,A("071117",.97f),C("20C8F6"));owner.toast=toast.gameObject.AddComponent<CanvasGroup>();owner.toast.alpha=0;owner.toast.blocksRaycasts=false;owner.toast.interactable=false;
            owner.toastLabel=Label("Battle message","",toast.transform,18,8,814,66,23,C("E7F0F4"),TextAlignmentOptions.Center);owner.toastLabel.textWrappingMode=TextWrappingModes.Normal;
            canvas.SetActive(false);
        }
        static void BuildCards(Transform root)
        {
            var shape=Shape("BattleModeCard",null,0,0,424,108,C("0B1B25"),C("214654"));shape.raycastTarget=true;
            var b=shape.gameObject.AddComponent<Button>();b.targetGraphic=shape;b.transition=Selectable.Transition.None;
            var hover=shape.gameObject.AddComponent<LobbyHover>();hover.surface=shape;hover.normal=shape.color;hover.hover=C("123E50");hover.pressed=C("17627C");
            var card=shape.gameObject.AddComponent<BattleModeCard>();card.hover=hover;
            card.numberLabel=Label("Number","01",shape.transform,17,21,44,44,28,C("20C8F6"));
            card.titleLabel=Label("Title","CHIẾN DỊCH",shape.transform,75,15,276,42,28,C("E7F0F4"));
            card.subtitleLabel=Label("Subtitle","Cốt truyện & khám phá",shape.transform,76,62,280,28,18,C("829EAC"));
            var icon=Rect("Mode icon",shape.transform,359,25,48,48).gameObject.AddComponent<BattleModeIcon>();icon.color=C("20C8F6");icon.raycastTarget=false;card.icon=icon;
            card.selectionBorder=Shape("Selected border",shape.transform,1,1,422,106,Color.clear,C("20C8F6")).gameObject;
            var prefab=PrefabUtility.SaveAsPrefabAsset(shape.gameObject,Root+"/Prefabs/BattleModeCard.prefab");UnityEngine.Object.DestroyImmediate(shape.gameObject);owner.cards=new BattleModeCard[owner.catalog.modes.Length];
            for(int i=0;i<owner.cards.Length;i++){var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);go.name="Battle Mode "+i;go.GetComponent<RectTransform>().anchoredPosition=new Vector2(1468,-(186+i*119));var view=go.GetComponent<BattleModeCard>();view.owner=owner;view.index=i;view.Bind(owner.catalog.modes[i],i==0);owner.cards[i]=view;}
        }
        static BattleModeCatalog Catalog()
        {
            var c=ScriptableObject.CreateInstance<BattleModeCatalog>();c.modes=new[]
            {
                new BattleModeDefinition{title="CHIẾN DỊCH",subtitle="Cốt truyện & khám phá",code="C01",icon=BattleModeIcon.Kind.Campaign,recommendedLevel=20,energyCost=20,description="Khám phá các khu vực và tiến triển cốt truyện. Chuẩn bị Gundam, lựa chọn địa điểm rồi triển khai đội hình chiến đấu theo lượt."},
                new BattleModeDefinition{title="ĐẤU TRƯỜNG",subtitle="So tài chiến thuật",code="A01",icon=BattleModeIcon.Kind.Arena,recommendedLevel=25,energyCost=10,description="So tài chiến thuật với các đội hình đối thủ. Tận dụng thứ tự hành động, kỹ năng và khả năng phối hợp của Gundam."},
                new BattleModeDefinition{title="THỬ THÁCH",subtitle="Mục tiêu & giới hạn lượt",code="T01",icon=BattleModeIcon.Kind.Challenge,recommendedLevel=30,energyCost=15,description="Vượt qua các mục tiêu đặc biệt trong số lượt giới hạn. Xây dựng chiến thuật phù hợp để đạt đánh giá cao."},
                new BattleModeDefinition{title="HUẤN LUYỆN",subtitle="Luyện tập tự do",code="H01",icon=BattleModeIcon.Kind.Training,recommendedLevel=1,energyCost=0,description="Làm quen với kỹ năng, thứ tự lượt và cơ chế phá vỡ sức bền. Thử nghiệm Gundam mà không tiêu hao năng lượng."},
                new BattleModeDefinition{title="PHÒNG TÙY CHỈNH",subtitle="Thiết lập trận đấu",code="P01",icon=BattleModeIcon.Kind.CustomRoom,recommendedLevel=1,energyCost=0,description="Chọn địa điểm và mức độ phù hợp cho một trận đấu tùy chỉnh. Chuẩn bị Gundam và chiến thuật trước khi tạo phòng."}
            };
            c.locationNames=new[]{"CẢNG BÌNH MINH","TRẠM QUỸ ĐẠO","THÀNH PHỐ THIÊN KHÔNG","HẺM NÚI SƯƠNG","KHU CÔNG NGHIỆP"};
            float[] x={-4.6f,0,4.4f,-2.8f,4.1f},z={-1.8f,1.8f,.4f,-4.4f,-4.1f};c.locationPoints=new Vector3[5];for(int i=0;i<5;i++)c.locationPoints[i]=new Vector3(x[i],Height(x[i],z[i]),z[i]);
            AssetDatabase.CreateAsset(c,Root+"/Data/BattleModeCatalog.asset");return c;
        }
        [MenuItem("Tools/Gundam Lobby/Install Battle Mode Screen")]
        public static string Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/GundamLobby/Scenes/HomeLobby.unity")throw new InvalidOperationException("Open HomeLobby");
            if(UnityEngine.Object.FindFirstObjectByType<BattleController>())throw new InvalidOperationException("Battle screen already installed");
            if(!Shader.Find("GundamLobby/BattleHologram"))throw new InvalidOperationException("Import BattleHologram shader first");
            foreach(string folder in new[]{"Data","Prefabs","Materials","Meshes"})Directory.CreateDirectory(Root+"/"+folder);
            EditorSceneManager.SaveScene(scene,"Assets/GundamLobby/Backups/HomeLobby_BeforeBattle_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/GundamLobby/Inventory/Fonts/Inventory Barlow Condensed SDF.asset");
            var d=UnityEngine.Object.FindFirstObjectByType<HomeLobbyDashboard>();var system=new GameObject("Battle Preparation Systems");Undo.RegisterCreatedObjectUndo(system,"Install Battle screen");owner=system.AddComponent<BattleController>();owner.dashboard=d;owner.previewCamera=d.striker.lobbyCamera;owner.catalog=Catalog();d.battlePanel=owner;
            BuildStage();BuildUI(d);owner.SelectMode(0);owner.SelectLocation(0);EditorUtility.SetDirty(d);EditorUtility.SetDirty(owner);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Selection.activeGameObject=system;
            File.WriteAllText(Root+"/README_VI.md","# Chiến đấu — giao diện chọn chế độ\n\nMở HomeLobby và bấm CHIẾN ĐẤU hoặc XUẤT KÍCH. Cột phải có 5 chế độ tiếng Việt, không có danh sách Missions. Chọn thẻ để đổi nội dung; chọn nhãn trên bản đồ để đổi địa điểm; kéo vùng bản đồ để xoay hologram. Đổi mức độ Tiêu chuẩn/Nâng cao và bấm Xuất kích để xem phản hồi mẫu. Chưa tạo màn chiến đấu, tiêu hao tài nguyên, ghép trận hoặc phòng mạng.\n\nPhòng nghiên cứu, Esc và Quay lại trả camera về lobby. Kho đồ mở màn kho hiện có. Đổi Gundam mở roster và tự quay lại màn Chiến đấu khi chọn xong.\n\nBattle/Data/BattleModeCatalog.asset: chỉnh tên, mô tả, cấp khuyến nghị, năng lượng và địa điểm. Các dữ liệu này là mẫu thiết kế theo lượt. Battle Preparation Systems: các liên kết UI, stage và model mount. Model xem trước lấy Gundam đang trang bị. Prefab BattleModeCard tách riêng; header dùng CommandNavigationBar hiện có. Địa hình hologram là mesh 3D và shader Unlit riêng, UI chữ/nút/panel tách rời có thể sửa. Không đổi render pipeline hay cài plugin trả phí.\n",new System.Text.UTF8Encoding(false));
            return "Installed 5-mode Vietnamese Battle preparation UI with a 3D holographic table, selectable locations, live Gundam preview, map rotation and prototype launch feedback.";
        }
    }
}
