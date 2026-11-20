using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby.Editor
{
    public static class CommandHeaderBuilder
    {
        const string Root="Assets/GundamLobby/Weaponry";
        static TMP_FontAsset font;
        static Color C(string value){ColorUtility.TryParseHtmlString("#"+value,out var c);return c;}
        static RectTransform Rect(string name,Transform p,float x,float y,float w,float h)
        {
            var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(p,false);rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;
        }
        static LobbyShape Shape(string name,Transform p,float x,float y,float w,float h,string fill,string border="00000000",float cut=6)
        {
            var rt=Rect(name,p,x,y,w,h);rt.gameObject.AddComponent<CanvasRenderer>();var s=rt.gameObject.AddComponent<LobbyShape>();s.geometry=LobbyShape.Geometry.Panel;s.cut=cut;s.color=C(fill);s.border=C(border);s.thickness=s.border.a>0?1:0;s.raycastTarget=false;return s;
        }
        static TMP_Text Text(string name,string value,Transform p,float x,float y,float w,float h,float size,string color)
        {
            var t=Rect(name,p,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=C(color);t.raycastTarget=false;t.margin=Vector4.zero;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Ellipsis;return t;
        }
        static Button Button(string name,Transform p,float x,float y,float w,float h,string fill="091721",string border="244250")
        {
            var shape=Shape(name,p,x,y,w,h,fill,border);shape.raycastTarget=true;
            var b=shape.gameObject.AddComponent<Button>();b.targetGraphic=shape;b.transition=Selectable.Transition.None;
            var hover=shape.gameObject.AddComponent<LobbyHover>();hover.surface=shape;hover.normal=shape.color;hover.hover=C("12394A");hover.pressed=C("18536A");hover.duration=.18f;return b;
        }
        static void Icon(string name,LobbyHeaderIcon.Symbol symbol,Transform p,float x,float y,float w,float h,string color)
        {
            var rt=Rect(name,p,x,y,w,h);rt.gameObject.AddComponent<CanvasRenderer>();var g=rt.gameObject.AddComponent<LobbyHeaderIcon>();g.symbol=symbol;g.color=C(color);g.raycastTarget=false;
        }
        static LobbyNavigationBar Template()
        {
            var root=Rect("CommandNavigationBar",null,0,0,1920,96);var nav=root.gameObject.AddComponent<LobbyNavigationBar>();
            var bg=Shape("Header background",root,0,0,1920,96,"061019FA",cut:0);bg.gradient=true;bg.gradientEnd=C("091722FA");
            Shape("Header lower rule",root,24,95,1872,1,"1D3D4F",cut:0);
            nav.profile=Button("Commander Profile",root,24,12,320,72,"0B1B27","294B5C");
            Shape("Profile left accent",nav.profile.transform,0,13,2,44,"20C8F6",cut:0);
            var frame=Shape("Avatar frame",nav.profile.transform,8,5,62,62,"0C2D3C","3F94AE",8);
            var window=Rect("Avatar window",frame.transform,4,4,54,54);window.gameObject.AddComponent<RectMask2D>();
            var image=Rect("Commander Avatar",window,0,0,54,54).gameObject.AddComponent<Image>();image.preserveAspect=true;image.raycastTarget=false;nav.portrait=image;
            var badge=Shape("Commander level badge",nav.profile.transform,53,50,26,19,"102C39","B49B54",3);
            nav.levelBadge=Text("Level number","20",badge.transform,0,0,26,19,14,"E2C77D");nav.levelBadge.alignment=TextAlignmentOptions.Center;
            nav.playerLabel=Text("Commander name","AntAkA",nav.profile.transform,89,5,187,34,27,"E5F2F8");
            nav.commanderLabel=Text("Commander rank","CHỈ HUY  /  CẤP 20",nav.profile.transform,90,38,190,22,15,"85A9BC");
            Shape("Experience track",nav.profile.transform,90,64,174,3,"24414F",cut:0);
            nav.experienceFill=Shape("Experience progress",nav.profile.transform,90,64,111,3,"20C8F6",cut:0).rectTransform;
            Icon("Open profile",LobbyHeaderIcon.Symbol.Chevron,nav.profile.transform,292,27,12,18,"6196AC");
            float[] x={420,684,894,1104};float[] widths={252,198,198,198};
            string[] labels={"PHÒNG NGHIÊN CỨU","CHIẾN ĐẤU","KHO ĐỒ","CỬA HÀNG"};
            var buttons=new Button[4];
            for(int i=0;i<4;i++){buttons[i]=Button("Navigate "+i,root,x[i],18,widths[i],58);var text=Text("Label",labels[i],buttons[i].transform,10,0,widths[i]-20,58,23,"DFECF4");text.alignment=TextAlignmentOptions.Center;}
            nav.laboratory=buttons[0];nav.battle=buttons[1];nav.inventory=buttons[2];nav.shop=buttons[3];
            nav.resourceValues=new TMP_Text[3];nav.resourceButtons=new Button[3];
            string[] resources={"NĂNG LƯỢNG","TÍN DỤNG","PHA LÊ"},values={"120 / 180","17,020","925"},colors={"61CBB5","E2C575","51C7F4"};
            LobbyHeaderIcon.Symbol[] symbols={LobbyHeaderIcon.Symbol.Energy,LobbyHeaderIcon.Symbol.Credits,LobbyHeaderIcon.Symbol.Crystal};
            for(int i=0;i<3;i++)
            {
                var b=Button("Resource "+resources[i],root,1400+i*168,19,158,58,"0B1C28","264250");nav.resourceButtons[i]=b;
                Shape("Icon socket",b.transform,9,14,31,31,"112C39",cut:4);
                Icon("Currency symbol",symbols[i],b.transform,12,17,25,25,colors[i]);
                Text("Resource name",resources[i],b.transform,48,6,84,18,12.5f,"82A1B1");
                nav.resourceValues[i]=Text("Resource amount",values[i],b.transform,47,27,100,29,23,"E1EFF5");
                Icon("Add resource",LobbyHeaderIcon.Symbol.Plus,b.transform,137,8,12,12,"678FA3");
            }
            return nav;
        }
        [MenuItem("Tools/Gundam Lobby/Polish Commander Header and Correct Lab Route")]
        public static string Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/GundamLobby/Scenes/HomeLobby.unity")throw new InvalidOperationException("Open HomeLobby");
            var d=UnityEngine.Object.FindFirstObjectByType<HomeLobbyDashboard>();if(!d)throw new InvalidOperationException("Dashboard missing");
            EditorSceneManager.SaveScene(scene,"Assets/GundamLobby/Backups/HomeLobby_BeforeCommandHeader_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/GundamLobby/Inventory/Fonts/Inventory Barlow Condensed SDF.asset");
            if(!d.playerAvatar)d.playerAvatar=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GundamLobby/Inventory/Art/CommanderPortrait.png");
            d.commanderLevel=20;d.commanderProgress=.64f;d.resourceAmounts=new[]{"120 / 180","17,020","925"};
            var template=Template();var prefab=PrefabUtility.SaveAsPrefabAsset(template.gameObject,Root+"/Prefabs/CommandNavigationBar.prefab");UnityEngine.Object.DestroyImmediate(template.gameObject);
            var oldBars=UnityEngine.Object.FindObjectsByType<LobbyNavigationBar>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach(var old in oldBars)
            {
                var parent=old.transform.parent;LobbyNavigationBar.Page page;
                bool isHome=old.GetComponentInParent<Canvas>(true).gameObject==d.scaler.gameObject;
                bool isInventory=old.GetComponentInParent<Canvas>(true).gameObject==d.inventory.inventoryCanvas;
                page=isHome?LobbyNavigationBar.Page.Laboratory:isInventory?LobbyNavigationBar.Page.Inventory:LobbyNavigationBar.Page.Weaponry;
                Undo.DestroyObjectImmediate(old.gameObject);
                var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name="Command Header";
                var bar=go.GetComponent<LobbyNavigationBar>();bar.dashboard=d;bar.page=page;
                if(isHome){d.playerLabel=bar.playerLabel;d.portrait=bar.portrait;}
                if(isInventory)bar.unitCaption=parent.Find("Preview Label").GetComponent<TMP_Text>();
                bar.Refresh();
            }
            var weaponButton=d.scaler.transform.Find("Home Dashboard - Wireframe Layout/Home Gundam Stats/Open Weaponry").GetComponent<Button>();
            Undo.RecordObject(weaponButton,"Weaponry uses a separate action");weaponButton.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddPersistentListener(weaponButton.onClick,d.OpenWeaponry);EditorUtility.SetDirty(weaponButton);
            d.Refresh();EditorUtility.SetDirty(d);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Selection.activeGameObject=d.gameObject;
            return "Laboratory now opens the hangar lobby; Weaponry stays on its own button. Three shared headers rebuilt with a dedicated avatar, commander profile and individual resource chips.";
        }
    }
}
