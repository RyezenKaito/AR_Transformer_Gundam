using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GundamLobby.Editor
{
    public static class WeaponryDashboardBuilder
    {
        const string Root = "Assets/GundamLobby/Weaponry";
        static TMP_FontAsset font;
        static HomeLobbyDashboard dash;
        static WeaponryController weaponry;
        static Color bg=Hex("071117"), surface=Hex("0B1B25"), panel=Hex("102A37"), line=Hex("214654"), cyan=Hex("20C8F6"), white=Hex("E7F0F4"), muted=Hex("829EAC");
        static Color Hex(string s) { ColorUtility.TryParseHtmlString("#"+s,out var c); return c; }
        static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform)); var rt=go.GetComponent<RectTransform>(); rt.SetParent(parent,false);
            rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(0,1); rt.anchoredPosition=new Vector2(x,-y); rt.sizeDelta=new Vector2(w,h); return rt;
        }
        static RectTransform RootRect(string name,Transform parent)
        {
            var rt=Rect(name,parent,0,0,1920,1080); rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f); rt.anchoredPosition=Vector2.zero; return rt;
        }
        static LobbyShape Shape(string name,Transform p,float x,float y,float w,float h,Color fill,Color border,LobbyShape.Geometry geo=LobbyShape.Geometry.Panel)
        {
            var rt=Rect(name,p,x,y,w,h); rt.gameObject.AddComponent<CanvasRenderer>(); var s=rt.gameObject.AddComponent<LobbyShape>();
            s.geometry=geo; s.color=fill; s.border=border; s.thickness=border.a>0?1:0; s.raycastTarget=false; return s;
        }
        static TMP_Text Label(string name,string value,Transform p,float x,float y,float w,float h,float size,Color color,TextAlignmentOptions align=TextAlignmentOptions.Left)
        {
            var rt=Rect(name,p,x,y,w,Mathf.Max(h,Mathf.Ceil(size*1.25f))); var t=rt.gameObject.AddComponent<TextMeshProUGUI>(); t.font=font; t.fontSize=size; t.color=color; t.text=value;
            t.alignment=align; t.textWrappingMode=TextWrappingModes.NoWrap; t.overflowMode=TextOverflowModes.Ellipsis; t.raycastTarget=false; t.margin=Vector4.zero; return t;
        }
        static Button Button(string name,string text,Transform p,float x,float y,float w,float h,UnityAction action,bool primary=false)
        {
            var shape=Shape(name,p,x,y,w,h,primary?Hex("087CAB"):bg,primary?cyan:line,primary?LobbyShape.Geometry.Tab:LobbyShape.Geometry.Panel); shape.raycastTarget=true;
            var b=shape.gameObject.AddComponent<Button>(); b.targetGraphic=shape; b.transition=Selectable.Transition.None;
            var hover=shape.gameObject.AddComponent<LobbyHover>(); hover.surface=shape; hover.normal=shape.color; hover.hover=Hex("12506A"); hover.pressed=Hex("177CA2"); hover.duration=.18f;
            Label("Label",text,shape.transform,12,0,w-24,h,primary?27:22,white,TextAlignmentOptions.Center);
            if(action!=null) UnityEventTools.AddPersistentListener(b.onClick,action); return b;
        }
        static Image Picture(string name,Sprite sprite,Transform p,float x,float y,float w,float h)
        {
            var rt=Rect(name,p,x,y,w,h); var i=rt.gameObject.AddComponent<Image>(); i.sprite=sprite; i.enabled=sprite; i.preserveAspect=true; i.raycastTarget=false; return i;
        }
        static WeaponryGlyph Glyph(string name,WeaponryGlyph.Kind kind,Transform p,float x,float y,float w,float h,Color color)
        {
            var rt=Rect(name,p,x,y,w,h); rt.gameObject.AddComponent<CanvasRenderer>(); var g=rt.gameObject.AddComponent<WeaponryGlyph>(); g.kind=kind; g.color=color; g.raycastTarget=false; return g;
        }
        static void Rule(Transform p,float x,float y,float w,Color color) { Shape("Rule",p,x,y,w,1,color,Color.clear,LobbyShape.Geometry.Box); }
        static Color Alpha(Color c,float a) { c.a=a; return c; }
        static void SavePart(GameObject go,string name) { PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+name+".prefab"); }

        [MenuItem("Tools/Gundam Lobby/Install Weaponry and Home Dashboard")]
        public static string Build()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before installing.");
            var scene=SceneManager.GetActiveScene();
            if(scene.path!="Assets/GundamLobby/Scenes/HomeLobby.unity") throw new InvalidOperationException("Open HomeLobby first.");
            if(UnityEngine.Object.FindFirstObjectByType<HomeLobbyDashboard>()) throw new InvalidOperationException("Dashboard already exists; edit the installed scene.");
            Directory.CreateDirectory(Root+"/Prefabs"); Directory.CreateDirectory(Root+"/Data");
            EditorSceneManager.SaveScene(scene,"Assets/GundamLobby/Backups/HomeLobby_BeforeWeaponry_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/GundamLobby/Inventory/Fonts/Inventory Barlow Condensed SDF.asset");
            if(!font) throw new InvalidOperationException("Existing UI font unavailable.");
            var fallback=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/GundamLobby/Fonts/Lobby Vietnamese SDF.asset");
            if(fallback && !font.fallbackFontAssetTable.Contains(fallback)) { font.fallbackFontAssetTable.Add(fallback); EditorUtility.SetDirty(font); }
            var striker=UnityEngine.Object.FindFirstObjectByType<StrikerSelectionController>();
            var lobby=UnityEngine.Object.FindFirstObjectByType<HomeLobbyController>();
            var inventory=UnityEngine.Object.FindFirstObjectByType<InventoryPanel>();
            if(!striker || !lobby || !inventory) throw new InvalidOperationException("Existing Striker, lobby and inventory required.");
            var sys=new GameObject("Home Dashboard and Weaponry Systems"); Undo.RegisterCreatedObjectUndo(sys,"Install dashboard and weaponry");
            dash=sys.AddComponent<HomeLobbyDashboard>(); weaponry=sys.AddComponent<WeaponryController>();
            dash.lobby=lobby; dash.striker=striker; dash.inventory=inventory; dash.weaponry=weaponry;
            weaponry.dashboard=dash; weaponry.lobby=lobby; weaponry.striker=striker; weaponry.inventory=inventory;
            weaponry.homeCanvas=striker.lobbyCanvas; weaponry.previewCamera=striker.lobbyCamera;
            weaponry.previewMount=new GameObject("Weaponry 3D Preview Mount"); weaponry.previewMount.SetActive(false);
            var catalog=ScriptableObject.CreateInstance<WeaponryCatalog>(); catalog.skills=Skills(); AssetDatabase.CreateAsset(catalog,Root+"/Data/WeaponryCatalog.asset"); weaponry.catalog=catalog;
            BuildHome(striker.lobbyCanvas); BuildWeaponry();
            dash.Refresh();
            weaponry.weaponryCanvas.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Selection.activeGameObject=sys;
            File.WriteAllText(Root+"/README_VI.md",Guide(),new System.Text.UTF8Encoding(false));
            return "Installed Home dashboard matching the wireframe and Weaponry with live selected Gundam, 5 skills, drag/zoom, demo and return navigation.";
        }
        static void BuildHome(GameObject canvas)
        {
            dash.scaler=canvas.GetComponent<CanvasScaler>(); dash.scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; dash.scaler.referenceResolution=new Vector2(1920,1080); dash.scaler.matchWidthOrHeight=.5f;
            // Keep the original editable UI and all its references as an inactive archive.
            foreach(Transform child in canvas.transform) if(child.gameObject!=dash.lobby.toast.gameObject) child.gameObject.SetActive(false);
            var root=RootRect("Home Dashboard - Wireframe Layout",canvas.transform);
            var top=Shape("Top readability",root,0,0,1920,192,Alpha(bg,.96f),Color.clear,LobbyShape.Geometry.Box); top.gradient=true; top.gradientVertical=true; top.gradientEnd=Alpha(bg,0);
            var left=Shape("Event readability",root,0,180,460,820,Alpha(bg,.65f),Color.clear,LobbyShape.Geometry.Box); left.gradient=true; left.gradientEnd=Alpha(bg,0);
            var right=Shape("Stats readability",root,1400,180,520,860,Alpha(bg,0),Color.clear,LobbyShape.Geometry.Box); right.gradient=true; right.gradientEnd=Alpha(bg,.78f);
            var header=Rect("Home Header",root,0,0,1920,150);
            var profile=Button("Character Profile","",header,30,25,278,94,dash.Profile);
            var unit=dash.striker.catalog.units[1];
            dash.portrait=Picture("Active Gundam Portrait",unit.portrait,profile.transform,12,10,82,70);
            dash.playerLabel=Label("Commander Name",dash.lobby.playerName,profile.transform,105,12,158,33,26,white);
            Label("Commander Level","CHỈ HUY  /  LV.20",profile.transform,105,47,158,26,17,muted);
            Button("Laboratory","LABORATORY",header,500,31,222,66,dash.OpenLaboratory,true);
            Button("Battle","BATTLE",header,740,31,222,66,dash.Battle);
            Button("Inventory","INVENTORY",header,980,31,222,66,dash.OpenInventory);
            Button("Shop","SHOP",header,1220,31,222,66,dash.Shop);
            string[] amounts={"120 / 180","17,020","925"}; Color[] colors={Hex("5CAD77"),Hex("C9A846"),cyan};
            for(int i=0;i<3;i++) { float x=1480+i*139; Shape("Resource icon",header,x,49,20,20,Alpha(colors[i],.22f),colors[i],LobbyShape.Geometry.Hexagon); Label("Resource amount",amounts[i],header,x+26,34,106,51,22,white); }
            Rule(header,31,132,1858,Alpha(line,.8f));
            Label("Home location","CĂN CỨ  /  SECTOR 07",root,52,155,440,34,23,muted);
            Label("Dock status","HỆ THỐNG TRỰC TUYẾN",root,1500,155,356,32,18,cyan,TextAlignmentOptions.Right);
            var eventPanel=Shape("Home Event Card",root,56,300,338,282,Alpha(panel,.92f),line);
            eventPanel.gameObject.AddComponent<RectMask2D>();
            Shape("Event technical grid",eventPanel.transform,0,0,338,180,Alpha(line,.28f),Color.clear,LobbyShape.Geometry.Grid);
            Shape("Orbit outer",eventPanel.transform,139,-19,199,199,Color.clear,Alpha(cyan,.3f),LobbyShape.Geometry.Ring);
            Shape("Orbit inner",eventPanel.transform,167,9,143,143,Color.clear,Alpha(cyan,.55f),LobbyShape.Geometry.Ring);
            Glyph("Event reactor",WeaponryGlyph.Kind.Reactor,eventPanel.transform,173,24,132,96,Alpha(cyan,.55f));
            Label("Event eyebrow","SỰ KIỆN  /  01",eventPanel.transform,22,16,290,30,19,cyan);
            dash.eventArt=Picture("Replace Event Artwork",null,eventPanel.transform,0,0,338,178);
            dash.eventTitleLabel=Label("Event Title",dash.eventTitle,eventPanel.transform,22,132,294,60,31,white); dash.eventTitleLabel.textWrappingMode=TextWrappingModes.Normal;
            Label("Event description","Tín hiệu lạ đang chờ giải mã.",eventPanel.transform,22,199,294,28,21,muted);
            Button("Event","XEM SỰ KIỆN   >",eventPanel.transform,22,236,294,31,dash.Event);
            var stats=Shape("Home Gundam Stats",root,1476,342,388,430,Alpha(bg,.90f),line);
            Label("Stats heading","THÔNG TIN GUNDAM",stats.transform,24,16,340,28,19,cyan);
            Rule(stats.transform,24,55,340,line);
            dash.unitLabel=Label("Active Unit Name","UNICORN",stats.transform,24,68,340,51,38,white); dash.unitLabel.enableAutoSizing=true; dash.unitLabel.fontSizeMin=26; dash.unitLabel.fontSizeMax=38;
            dash.roleLabel=Label("Role and Level","BALANCED ATTACKER / LV.20",stats.transform,24,121,340,28,19,muted);
            dash.serialLabel=Label("Serial","RX-0 / PSYCHO FRAME",stats.transform,24,152,340,26,18,cyan);
            dash.statLabels=new TMP_Text[4]; string[] labels={"HP","ATK","DEF","SPD"};
            for(int i=0;i<4;i++)
            {
                int col=i%2,row=i/2; float x=24+col*178,y=200+row*65;
                Label("Stat "+labels[i],labels[i],stats.transform,x,y,140,23,18,muted);
                dash.statLabels[i]=Label("Value "+labels[i],"--",stats.transform,x,y+23,140,33,29,white);
            }
            Rule(stats.transform,24,334,340,line);
            Button("Switch Break Striker","ĐỔI GUNDAM",stats.transform,24,354,162,49,dash.OpenStrikers);
            Button("Open Weaponry","WEAPONRY",stats.transform,202,354,162,49,dash.OpenLaboratory,true);
            var play=Button("Play","PLAY     >",root,1476,835,388,99,dash.Play,true); play.GetComponentInChildren<TMP_Text>().fontSize=42;
            Label("Play caption","CHƯƠNG 01  /  TÍN HIỆU TỪ QUỸ ĐẠO",root,1476,950,388,33,20,muted,TextAlignmentOptions.Center);
            Label("Center location","MAINTENANCE DOCK   /   07",root,643,963,700,35,20,Alpha(white,.75f),TextAlignmentOptions.Center);
            var footer=Shape("Home Footer",root,0,1028,1920,52,Alpha(bg,.94f),Color.clear,LobbyShape.Geometry.Box);
            Rule(footer.transform,24,0,1872,line);
            Button("Mail","THƯ",footer.transform,30,10,118,32,dash.Mail);
            Button("Quests","NHIỆM VỤ",footer.transform,160,10,162,32,dash.Quests);
            Label("Home drag hint","KÉO CHUỘT ĐỂ XOAY GUNDAM",footer.transform,700,10,520,32,17,muted,TextAlignmentOptions.Center);
            Button("Settings","CÀI ĐẶT",footer.transform,1704,10,160,32,dash.Settings);
            SavePart(eventPanel.gameObject,"HomeEventCard"); SavePart(stats.gameObject,"HomeGundamStats"); SavePart(header.gameObject,"HomeHeader");
            dash.lobby.toast.transform.SetAsLastSibling(); dash.lobby.toast.gameObject.SetActive(true);
            var toastRt=dash.lobby.toast.GetComponent<RectTransform>(); toastRt.anchorMin=toastRt.anchorMax=toastRt.pivot=new Vector2(.5f,.5f); toastRt.anchoredPosition=new Vector2(0,-300); toastRt.sizeDelta=new Vector2(680,64);
        }
        static void BuildWeaponry()
        {
            var canvas=new GameObject("Weaponry Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var c=canvas.GetComponent<Canvas>(); c.renderMode=RenderMode.ScreenSpaceOverlay; c.sortingOrder=60;
            var scale=canvas.GetComponent<CanvasScaler>(); scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scale.referenceResolution=new Vector2(1920,1080); scale.matchWidthOrHeight=.5f;
            weaponry.weaponryCanvas=canvas; var root=RootRect("Weaponry Screen",canvas.transform);
            var left=Shape("Left readability",root,0,60,566,970,Alpha(bg,.98f),Color.clear,LobbyShape.Geometry.Box); left.gradient=true; left.gradientEnd=Alpha(bg,0);
            Shape("Details readability",root,1442,60,478,970,Alpha(bg,.97f),Color.clear,LobbyShape.Geometry.Box);
            var zone=Rect("3D Preview - Drag and Zoom",root,406,114,1014,914); var hit=zone.gameObject.AddComponent<Image>(); hit.color=Color.clear; zone.gameObject.AddComponent<WeaponryPreviewDrag>().owner=weaponry;
            Shape("Top bar",root,0,0,1920,64,bg,Color.clear,LobbyShape.Geometry.Box); Rule(root,0,63,1920,line);
            Label("Page title","LABORATORY",root,30,0,340,64,28,white);
            Button("Weaponry Home","CĂN CỨ",root,500,0,180,64,weaponry.Close);
            Button("Weaponry Current","VŨ KHÍ & KỸ NĂNG",root,682,0,286,64,null,true);
            Button("Weaponry Strikers","GUNDAM",root,970,0,180,64,weaponry.OpenStrikers);
            Button("Weaponry Inventory","INVENTORY",root,1152,0,200,64,weaponry.OpenInventory);
            Label("Weaponry resources","120 / 180      17,020      925",root,1455,0,430,64,23,muted,TextAlignmentOptions.Right);
            Label("Roster heading","HỆ THỐNG TÁC CHIẾN",root,28,112,325,37,21,muted); Label("Skill count","05",root,350,112,40,37,21,cyan,TextAlignmentOptions.Right); Rule(root,28,163,362,line);
            BuildSkillCards(root);
            Label("Data hint","CẤU HÌNH VŨ KHÍ TÍCH HỢP",root,30,952,360,32,18,muted);
            weaponry.serialLabel=Label("Selected unit serial","RX-0 / PSYCHO FRAME",root,448,112,790,32,19,cyan);
            weaponry.unitLabel=Label("Selected unit name","UNICORN",root,448,149,920,70,47,white);
            weaponry.roleLabel=Label("Selected unit role","BALANCED ATTACKER / LV.20",root,450,218,840,30,21,muted);
            Rule(root,451,259,68,cyan);
            var right=Rect("Skill Detail Panel",root,1468,94,424,930);
            weaponry.activeTab=Button("Active Skills","CHỦ ĐỘNG",right,0,0,206,47,weaponry.ShowActive,true).GetComponent<LobbyShape>();
            weaponry.passiveTab=Button("Passive Skills","NỘI TẠI",right,214,0,210,47,weaponry.ShowPassive).GetComponent<LobbyShape>();
            weaponry.categoryLabel=Label("Skill category","ĐÁNH THƯỜNG / CẤP 01",right,0,71,424,29,18,cyan);
            weaponry.titleLabel=Label("Skill title","PHÁO TIA CHÍNH",right,0,109,424,50,36,white); weaponry.titleLabel.enableAutoSizing=true; weaponry.titleLabel.fontSizeMin=28; weaponry.titleLabel.fontSizeMax=36;
            Rule(right,0,177,424,line);
            weaponry.detailGlyph=Glyph("Selected skill vector",WeaponryGlyph.Kind.Rifle,right,0,190,108,79,cyan);
            Label("Integrated label","VŨ KHÍ TÍCH HỢP",right,132,196,292,26,17,muted);
            weaponry.badgeLabel=Label("Resource cost","+1 SP",right,132,225,292,40,29,cyan);
            weaponry.descriptionLabel=Label("Skill description","",right,0,290,424,118,22,Hex("ADC1CB")); weaponry.descriptionLabel.textWrappingMode=TextWrappingModes.Normal;
            Label("Stats heading","THÔNG SỐ KỸ NĂNG",right,0,421,424,28,19,muted); Rule(right,0,458,424,line);
            weaponry.statNameLabels=new TMP_Text[6]; weaponry.statValueLabels=new TMP_Text[6];
            var statPrefab=Rect("WeaponryStatRow",null,0,0,424,37);
            Label("Name","Hệ số sát thương",statPrefab,0,0,250,36,21,muted);
            Label("Value","100% ATK",statPrefab,245,0,179,36,22,white,TextAlignmentOptions.Right); Rule(statPrefab,0,36,424,line);
            var asset=PrefabUtility.SaveAsPrefabAsset(statPrefab.gameObject,Root+"/Prefabs/WeaponryStatRow.prefab"); UnityEngine.Object.DestroyImmediate(statPrefab.gameObject);
            for(int i=0;i<6;i++) { var row=(GameObject)PrefabUtility.InstantiatePrefab(asset,right); var rt=row.GetComponent<RectTransform>(); rt.anchoredPosition=new Vector2(0,-(465+i*38)); weaponry.statNameLabels[i]=row.transform.Find("Name").GetComponent<TMP_Text>(); weaponry.statValueLabels[i]=row.transform.Find("Value").GetComponent<TMP_Text>(); }
            Label("Demo heading","MÔ PHỎNG TÁC CHIẾN",right,0,712,424,28,18,muted);
            var demo=Shape("Skill Demonstration",right,0,749,424,126,panel,line);
            Shape("Grid",demo.transform,8,8,408,109,Alpha(line,.6f),Color.clear,LobbyShape.Geometry.Grid);
            weaponry.demoGlyph=Glyph("Demo skill vector",WeaponryGlyph.Kind.Rifle,demo.transform,10,23,119,86,cyan);
            Shape("Enemy silhouette",demo.transform,344,35,44,72,Alpha(cyan,.17f),muted,LobbyShape.Geometry.Bust);
            var beam=Shape("Energy Beam",demo.transform,139,53,197,9,cyan,Color.clear,LobbyShape.Geometry.Box); beam.gradient=true; beam.gradientEnd=Alpha(white,.85f); weaponry.demoBeam=beam.gameObject.AddComponent<CanvasGroup>(); weaponry.demoBeam.alpha=.22f; weaponry.demoBeam.blocksRaycasts=false;
            Button("Play Skill Demonstration","XEM MINH HỌA",right,0,890,424,47,weaponry.PlayDemonstration,true);
            var footer=Shape("Weaponry Footer",root,0,1028,1920,52,bg,Color.clear,LobbyShape.Geometry.Box); Rule(footer.transform,0,0,1920,line);
            Label("View hint","KÉO: XOAY GUNDAM       CUỘN: THU PHÓNG",footer.transform,445,6,960,40,19,muted,TextAlignmentOptions.Center);
            Button("Weaponry Return","Esc   QUAY LẠI",footer.transform,1680,9,212,34,weaponry.Close);
            var toast=Shape("Weaponry Toast",root,590,850,720,56,Alpha(bg,.95f),cyan); weaponry.toast=toast.gameObject.AddComponent<CanvasGroup>(); weaponry.toast.alpha=0; weaponry.toast.blocksRaycasts=false; weaponry.toast.interactable=false;
            weaponry.toastLabel=Label("Toast message","",toast.transform,12,0,696,56,23,white,TextAlignmentOptions.Center);
        }
        static void BuildSkillCards(Transform root)
        {
            var shape=Shape("WeaponrySkillCard",null,0,0,362,129,surface,line); shape.raycastTarget=true;
            shape.gameObject.AddComponent<RectMask2D>(); var b=shape.gameObject.AddComponent<Button>(); b.targetGraphic=shape; b.transition=Selectable.Transition.None;
            var hover=shape.gameObject.AddComponent<LobbyHover>(); hover.surface=shape; hover.normal=surface; hover.hover=Hex("123E50"); hover.pressed=Hex("17627C");
            Shape("Technical stripe",shape.transform,226,-20,35,190,Alpha(cyan,.06f),Color.clear,LobbyShape.Geometry.Slant);
            var card=shape.gameObject.AddComponent<WeaponrySkillCard>(); card.hover=hover;
            card.categoryLabel=Label("Category","ĐÁNH THƯỜNG",shape.transform,20,13,302,25,17,cyan);
            card.titleLabel=Label("Title","PHÁO TIA CHÍNH",shape.transform,20,42,322,37,25,white);
            Label("Level","LV.01",shape.transform,20,95,150,25,17,muted);
            card.glyph=Glyph("Skill vector icon",WeaponryGlyph.Kind.Rifle,shape.transform,226,58,118,73,cyan);
            card.selectionBorder=Shape("Selection border",shape.transform,1,1,360,127,Color.clear,cyan).gameObject;
            var asset=PrefabUtility.SaveAsPrefabAsset(shape.gameObject,Root+"/Prefabs/WeaponrySkillCard.prefab"); UnityEngine.Object.DestroyImmediate(shape.gameObject);
            weaponry.cards=new WeaponrySkillCard[weaponry.catalog.skills.Length];
            for(int i=0;i<weaponry.cards.Length;i++)
            {
                var go=(GameObject)PrefabUtility.InstantiatePrefab(asset,root); go.name="Skill "+i; go.GetComponent<RectTransform>().anchoredPosition=new Vector2(28,-(183+i*143));
                var view=go.GetComponent<WeaponrySkillCard>(); view.owner=weaponry; view.index=i; view.Bind(weaponry.catalog.skills[i],i==0); weaponry.cards[i]=view;
            }
        }
        static WeaponrySkill[] Skills()
        {
            string[] statNames={"Hệ số sát thương","Số lần đánh","Hồi năng lượng","Điểm kỹ năng","Giảm sức bền","Mục tiêu"};
            return new[]
            {
                new WeaponrySkill {category="ĐÁNH THƯỜNG",title="PHÁO TIA CHÍNH",badge="+1 SP",glyph=WeaponryGlyph.Kind.Rifle,description="Bắn hai luồng năng lượng vào một kẻ địch, gây sát thương bằng 100% ATK. Mỗi lần sử dụng hồi 1 điểm kỹ năng cho đội.",statNames=statNames,statValues=new[]{"100% ATK","2","20","+1 SP","30","Đơn mục tiêu"}},
                new WeaponrySkill {category="KỸ NĂNG",title="HỎA LỰC LIÊN HOÀN",badge="−1 SP",glyph=WeaponryGlyph.Kind.Salvo,description="Tấn công mục tiêu chính với 180% ATK và gây 60% ATK lên các kẻ địch lân cận. Tiêu hao 1 điểm kỹ năng của đội.",statNames=new[]{"Sát thương chính","Sát thương lan","Hồi năng lượng","Điểm kỹ năng","Giảm sức bền","Mục tiêu"},statValues=new[]{"180% ATK","60% ATK","30","−1 SP","60","Khuếch tán"}},
                new WeaponrySkill {category="TUYỆT KỸ",title="BÙNG NỔ NĂNG LƯỢNG",badge="120 NĂNG LƯỢNG",glyph=WeaponryGlyph.Kind.Burst,description="Giải phóng năng lượng vào toàn bộ kẻ địch, gây sát thương bằng 240% ATK. Kích hoạt khi tích đủ 120 năng lượng.",statNames=new[]{"Hệ số sát thương","Số lần đánh","Tiêu hao năng lượng","Điểm kỹ năng","Giảm sức bền","Mục tiêu"},statValues=new[]{"240% ATK","1","120","0 SP","60","Toàn bộ địch"}},
                new WeaponrySkill {category="THIÊN PHÚ",title="LÒ PHẢN ỨNG",badge="TỰ ĐỘNG",glyph=WeaponryGlyph.Kind.Reactor,description="Tích một tầng Nhiệt Lõi sau mỗi lần tấn công. Khi đạt 3 tầng, đòn đánh tiếp theo tăng 30% sát thương.",statNames=new[]{"Tăng sát thương","Số tầng tối đa","Thời gian duy trì","Điểm kỹ năng","Điều kiện","Mục tiêu"},statValues=new[]{"30%","3","Đến khi kích hoạt","0 SP","3 lần tấn công","Bản thân"}},
                new WeaponrySkill {category="NỘI TẠI",title="KHIÊN NĂNG LƯỢNG",badge="BỊ ĐỘNG",glyph=WeaponryGlyph.Kind.Shield,description="Khi bắt đầu trận, nhận khiên hấp thụ sát thương bằng 15% HP tối đa. Hiệu lực trong 2 lượt của Gundam.",statNames=new[]{"Độ bền khiên","Thời gian duy trì","Số lần kích hoạt","Điểm kỹ năng","Điều kiện","Mục tiêu"},statValues=new[]{"15% HP tối đa","2 lượt","1 lần / trận","0 SP","Bắt đầu trận","Bản thân"}}
            };
        }
        static string Guide()
        {
            return "# Home Dashboard + Weaponry\n\nMở Assets/GundamLobby/Scenes/HomeLobby.unity và Play. Home theo sơ đồ: Hồ sơ trái trên, Laboratory/Battle/Inventory/Shop phía trên, sự kiện trái, Gundam giữa, thông tin và Play phải. Laboratory hoặc Weaponry mở màn kỹ năng. Đổi Gundam vẫn mở roster thật; Inventory giữ màn hiện có. Battle/Shop/Play và sự kiện dùng phản hồi mẫu.\n\nWeaponry: 5 thẻ đổi nội dung thật, kéo vùng giữa xoay model, cuộn zoom; Xem minh họa nháy sơ đồ năng lượng; Esc/Quay lại trả camera và model về lobby. Preview dùng Gundam đang trang bị. Nút Gundam mở roster, Inventory mở kho. Chưa có chiến đấu hoặc tiêu hao tài nguyên.\n\nDữ liệu mẫu ở Weaponry/Data/WeaponryCatalog.asset. Các chỉ số và kỹ năng dùng cho prototype theo lượt, chưa phải dữ liệu cân bằng. Home lấy HP/ATK/DEF/SPD từ StrikerCatalog.asset.\n\nChọn Home Dashboard and Weaponry Systems để đổi sprite sự kiện, tiêu đề sự kiện và liên kết UI. Prefabs trong Weaponry/Prefabs có HomeHeader, HomeEventCard, HomeGundamStats, WeaponrySkillCard và WeaponryStatRow. Chữ, nút, panel, icon là uGUI/TMP/vector có thể sửa riêng. UI cũ nằm bất hoạt trong Home Lobby Canvas để tham khảo; bản scene trước sửa ở GundamLobby/Backups. Không đổi pipeline, chất lượng render hoặc môi trường.\n";
        }
    }
}
