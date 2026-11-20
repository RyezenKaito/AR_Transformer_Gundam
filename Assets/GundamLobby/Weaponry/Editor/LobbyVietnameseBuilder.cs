using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby.Editor
{
    public static class LobbyVietnameseBuilder
    {
        const string Root="Assets/GundamLobby/Weaponry";
        static TMP_FontAsset font;
        static Color C(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
        static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform));var t=go.GetComponent<RectTransform>();t.SetParent(parent,false);
            t.anchorMin=t.anchorMax=t.pivot=new Vector2(0,1);t.anchoredPosition=new Vector2(x,-y);t.sizeDelta=new Vector2(w,h);return t;
        }
        static LobbyShape Shape(string name,Transform p,float x,float y,float w,float h,Color fill,Color border,LobbyShape.Geometry geometry=LobbyShape.Geometry.Panel)
        {
            var rt=Rect(name,p,x,y,w,h);rt.gameObject.AddComponent<CanvasRenderer>();var s=rt.gameObject.AddComponent<LobbyShape>();s.geometry=geometry;s.color=fill;s.border=border;s.raycastTarget=false;return s;
        }
        static TMP_Text Text(string name,string value,Transform p,float x,float y,float w,float h,float size,Color color)
        {
            var t=Rect(name,p,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Ellipsis;t.margin=Vector4.zero;return t;
        }
        static Button Button(string name,string label,Transform p,float x,float y,float w,float h)
        {
            var s=Shape(name,p,x,y,w,h,C("071117"),C("214654"));s.raycastTarget=true;var b=s.gameObject.AddComponent<Button>();b.targetGraphic=s;b.transition=Selectable.Transition.None;
            var hover=s.gameObject.AddComponent<LobbyHover>();hover.surface=s;hover.normal=s.color;hover.hover=C("12506A");hover.pressed=C("177CA2");
            var text=Text("Label",label,s.transform,10,0,w-20,h,23,C("E7F0F4"));text.alignment=TextAlignmentOptions.Center;return b;
        }
        static readonly Dictionary<string,string> words=new Dictionary<string,string>
        {
            {"LABORATORY","PHÒNG NGHIÊN CỨU"},{"BATTLE","CHIẾN ĐẤU"},{"INVENTORY","KHO ĐỒ"},{"SHOP","CỬA HÀNG"},{"PLAY     >","XUẤT KÍCH   >"},{"WEAPONRY","VŨ KHÍ"},
            {"CHỈ HUY  /  LV.20","CHỈ HUY  /  CẤP 20"},{"CĂN CỨ  /  SECTOR 07","CĂN CỨ  /  KHU 07"},{"MAINTENANCE DOCK   /   07","BỆ BẢO TRÌ   /   07"},
            {"TẤN CÔNG  /  NĂNG LƯỢNG","TẤN CÔNG  /  NĂNG LƯỢNG"},{"HP","HP"},{"ATK","TẤN CÔNG"},{"DEF","PHÒNG THỦ"},{"SPD","TỐC ĐỘ"},{"LV.01","CẤP 01"},
            {"ALL","TẤT CẢ"},{"MODS","MÔ-ĐUN"},{"PAINT","SƠN"},{"PATTERNS","HỌA TIẾT"},{"WEAPON","VŨ KHÍ"},{"VEHICLE","PHƯƠNG TIỆN"},{"DEVICES","THIẾT BỊ"},
            {"TYPE","LOẠI"},{"BULK DISCARD","HỦY HÀNG LOẠT"},{"BULK SELL","BÁN HÀNG LOẠT"},{"INVENTORY EXPANSION","MỞ RỘNG KHO"},
            {"Flawless","Hoàn hảo"},{"Superior","Cao cấp"},{"MILITARY SUPPLY SALE","GIÁ TIẾP TẾ"},{"MARKETPLACE","CHỢ GIAO DỊCH"},{"Space   USE","Space   SỬ DỤNG"},{"LIST ON MARKETPLACE","ĐĂNG BÁN TRÊN CHỢ"},
            {"COMMS","LIÊN LẠC"},{"Enter   START CHAT","Enter   BẮT ĐẦU CHAT"},{"Drag  ROTATE     Scroll  ZOOM","KÉO: XOAY   CUỘN: THU PHÓNG"},{"Esc  RETURN","Esc   QUAY LẠI"},{"Esc   RETURN","Esc   QUAY LẠI"},
            {"SWITCH BREAK STRIKER","ĐỔI GUNDAM"},{"STRIKER ROSTER","DANH SÁCH GUNDAM"},{"UNIT SPECIFICATION","THÔNG SỐ GUNDAM"},{"CORE STATS","CHỈ SỐ CƠ BẢN"},{"OFFENSE","TẤN CÔNG"},{"SURVIVABILITY","KHẢ NĂNG SINH TỒN"},{"TURN ECONOMY","TÀI NGUYÊN THEO LƯỢT"},
            {"Max HP","HP TỐI ĐA"},{"Attack","TẤN CÔNG"},{"Defense","PHÒNG THỦ"},{"Speed","TỐC ĐỘ"},{"Crit Rate","TỈ LỆ BẠO KÍCH"},{"Crit DMG","SÁT THƯƠNG BẠO KÍCH"},{"Break Effect","HIỆU ỨNG PHÁ VỠ"},{"Damage Type","LOẠI SÁT THƯƠNG"},
            {"Effect Resistance","KHÁNG HIỆU ỨNG"},{"Energy Regeneration","HIỆU SUẤT HỒI NĂNG LƯỢNG"},{"Aggro","MỨC ĐỘ THU HÚT"},{"Shield Capacity","ĐỘ BỀN KHIÊN"},{"Max Energy","NĂNG LƯỢNG TỐI ĐA"},{"Skill Cost","TIÊU HAO KỸ NĂNG"},{"Basic Attack Gain","HỒI SP ĐÁNH THƯỜNG"},{"Ultimate Target","MỤC TIÊU TUYỆT KỸ"},
            {"COMBAT POWER","SỨC MẠNH CHIẾN ĐẤU"},{"ACTIVE STRIKER","GUNDAM ĐANG TRANG BỊ"},{"STRIKER PREVIEW","XEM TRƯỚC GUNDAM"},{"STRIKER EQUIPPED","ĐÃ TRANG BỊ"},{"CONFIRM STRIKER","XÁC NHẬN GUNDAM"},{"ACTIVE","ĐANG DÙNG"},
            {"Enter    START CHAT","Enter    BẮT ĐẦU CHAT"},{"DRAG  ROTATE     SCROLL  ZOOM     ↑ ↓  SELECT     ENTER  CONFIRM","KÉO: XOAY   CUỘN: THU PHÓNG   ↑ ↓: CHỌN   ENTER: XÁC NHẬN"}
        };
        static string Translate(string s)
        {
            if(words.TryGetValue(s,out var translated))return translated;
            return s.Replace("  /  LV.","  /  CẤP ").Replace("  /  Lv. ","  /  CẤP ");
        }
        static void LocalizeTexts(GameObject root)
        {
            foreach(var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                string next=Translate(t.text);if(t.text==next)continue;t.text=next;
                if(!t.enableAutoSizing){t.enableAutoSizing=true;t.fontSizeMax=t.fontSize;t.fontSizeMin=t.fontSize*.82f;}
                EditorUtility.SetDirty(t);
            }
        }
        static LobbyNavigationBar CreateTemplate()
        {
            var root=Rect("UnifiedNavigationBar",null,0,0,1920,96);var nav=root.gameObject.AddComponent<LobbyNavigationBar>();
            Shape("Bar background",root,0,0,1920,96,C("071117"),Color.clear,LobbyShape.Geometry.Box);
            Shape("Bar rule",root,24,95,1872,1,C("214654"),Color.clear,LobbyShape.Geometry.Box);
            nav.profile=Button("Character Profile","",root,24,12,280,72);
            var image=Rect("Gundam Portrait",nav.profile.transform,12,5,83,62).gameObject.AddComponent<Image>();image.preserveAspect=true;image.raycastTarget=false;nav.portrait=image;
            nav.playerLabel=Text("Player","AntAkA",nav.profile.transform,107,4,160,33,26,C("E7F0F4"));
            Text("Commander level","CHỈ HUY  /  CẤP 20",nav.profile.transform,107,39,160,25,17,C("829EAC"));
            nav.laboratory=Button("Navigate Laboratory","PHÒNG NGHIÊN CỨU",root,500,18,222,58);
            nav.battle=Button("Navigate Battle","CHIẾN ĐẤU",root,740,18,222,58);
            nav.inventory=Button("Navigate Inventory","KHO ĐỒ",root,980,18,222,58);
            nav.shop=Button("Navigate Shop","CỬA HÀNG",root,1220,18,222,58);
            string[] values={"120 / 180","17,020","925"};string[] colors={"5CAD77","C9A846","20C8F6"};
            for(int i=0;i<3;i++){float x=1480+i*139;Shape("Resource symbol "+i,root,x,35,20,20,C(colors[i]),C(colors[i]),LobbyShape.Geometry.Hexagon);Text("Resource amount "+i,values[i],root,x+26,19,106,52,22,C("E7F0F4"));}
            return nav;
        }
        static LobbyNavigationBar InstallNavigation(GameObject prefab,Transform parent,LobbyNavigationBar.Page page,HomeLobbyDashboard dashboard)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name="Navigation - Vietnamese";var nav=go.GetComponent<LobbyNavigationBar>();nav.page=page;nav.dashboard=dashboard;nav.Refresh();return nav;
        }
        static void Hide(Transform root,params string[] names){foreach(string name in names){var child=root.Find(name);if(child)child.gameObject.SetActive(false);}}
        static void Feedback(Button button,InventoryPanel inv,string text)
        {
            button.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddStringPersistentListener(button.onClick,inv.Notify,text+"  ·  Chức năng đang phát triển");
        }
        [MenuItem("Tools/Gundam Lobby/Unify Vietnamese Navigation")]
        public static string Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/GundamLobby/Scenes/HomeLobby.unity")throw new InvalidOperationException("Open HomeLobby");
            var d=UnityEngine.Object.FindFirstObjectByType<HomeLobbyDashboard>();if(!d)throw new InvalidOperationException("Home dashboard missing");
            if(UnityEngine.Object.FindObjectsByType<LobbyNavigationBar>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length>0)throw new InvalidOperationException("Unified navigation already installed");
            EditorSceneManager.SaveScene(scene,"Assets/GundamLobby/Backups/HomeLobby_BeforeVietnamese_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/GundamLobby/Inventory/Fonts/Inventory Barlow Condensed SDF.asset");
            var template=CreateTemplate();var prefab=PrefabUtility.SaveAsPrefabAsset(template.gameObject,Root+"/Prefabs/UnifiedNavigationBar.prefab");UnityEngine.Object.DestroyImmediate(template.gameObject);
            var home=d.scaler.transform.Find("Home Dashboard - Wireframe Layout");Hide(home,"Home Header");
            var homeNav=InstallNavigation(prefab,home,LobbyNavigationBar.Page.Home,d);d.playerLabel=homeNav.playerLabel;d.portrait=homeNav.portrait;
            var inv=d.inventory;var inventoryRoot=inv.inventoryCanvas.transform.Find("Inventory Screen");Hide(inventoryRoot,"Inventory Header");
            var inventoryNav=InstallNavigation(prefab,inventoryRoot,LobbyNavigationBar.Page.Inventory,d);inventoryNav.unitCaption=inventoryRoot.Find("Preview Label").GetComponent<TMP_Text>();inventoryNav.Refresh();
            PolishInventoryLayout(inv);
            var weaponryRoot=d.weaponry.weaponryCanvas.transform.Find("Weaponry Screen");
            Hide(weaponryRoot,"Top bar","Page title","Weaponry Home","Weaponry Current","Weaponry Strikers","Weaponry Inventory","Weaponry resources");
            InstallNavigation(prefab,weaponryRoot,LobbyNavigationBar.Page.Laboratory,d);
            var change=Button("Weaponry Change Gundam","ĐỔI GUNDAM",weaponryRoot,1204,118,200,44);UnityEventTools.AddPersistentListener(change.onClick,d.weaponry.OpenStrikers);
            string[] roles={"TẤN CÔNG HẠNG NẶNG","TẤN CÔNG CÂN BẰNG","ĐỘT KÍCH CƠ ĐỘNG","TẤN CÔNG NĂNG LƯỢNG","TẤN CÔNG BÙNG NỔ","HỖ TRỢ CHIẾN THUẬT","ĐẤU SĨ CẬN CHIẾN","TẤN CÔNG TẦM XA","CHUYÊN GIA SÚNG TRƯỜNG","CẬN CHIẾN"};
            for(int i=0;i<d.striker.catalog.units.Length&&i<roles.Length;i++)d.striker.catalog.units[i].role=roles[i];EditorUtility.SetDirty(d.striker.catalog);
            foreach(var item in inv.items)
            {
                item.itemName=item.itemName.Replace("Pattern · Soft Leaf","Họa tiết · Lá mềm").Replace("Supply Crate · ","Hòm tiếp tế · ").Replace("Vanguard","Tiên phong").Replace("Frontline","Tiền tuyến").Replace("Orbital","Quỹ đạo").Replace("Beam Rifle · RX Series","Súng tia · Dòng RX").Replace("Paint Cell · ","Ống sơn · ").Replace("Arctic Cyan","Cyan băng giá").Replace("Quỹ đạo Blue","Xanh quỹ đạo").Replace("Signal Violet","Tím tín hiệu").Replace("Reactor Core · Mk ","Lõi phản ứng · Mk ").Replace("Armor Plate · Lunar","Giáp · Mặt trăng").Replace("Pilot Module · Mindbridge","Mô-đun phi công · Liên kết tâm trí").Replace("Federation Merit Emblem","Huy hiệu công trạng Liên bang");
                item.rarity=Translate(item.rarity);
                item.description=item.category=="PATTERNS"?"Họa tiết dùng để tùy chỉnh lớp sơn trên Gundam.":"Vật tư trang bị cho Gundam. Chọn SỬ DỤNG để xem phản hồi mẫu.";
            }
            EditorUtility.SetDirty(inv);
            foreach(var root in scene.GetRootGameObjects())LocalizeTexts(root);
            var detail=inventoryRoot.Find("Selected Item Details");
            inv.itemNameLabel.enableAutoSizing=true;inv.itemNameLabel.fontSizeMin=28;inv.itemNameLabel.fontSizeMax=39;
            inv.descriptionLabel.rectTransform.sizeDelta=new Vector2(535,76);inv.descriptionLabel.fontSize=20;inv.descriptionLabel.enableAutoSizing=true;inv.descriptionLabel.fontSizeMin=18;inv.descriptionLabel.fontSizeMax=20;
            foreach(string name in new[]{"Supply Label","Supply Price"}){var rt=detail.Find(name).GetComponent<RectTransform>();rt.anchoredPosition=new Vector2(rt.anchoredPosition.x,-178);}
            foreach(string name in new[]{"Market Label","Selling Price"}){var rt=detail.Find(name).GetComponent<RectTransform>();rt.anchoredPosition=new Vector2(rt.anchoredPosition.x,-212);}
            inv.rarityLabel.text=inv.items[inv.selectedIndex].rarity;inv.itemNameLabel.text=inv.items[inv.selectedIndex].itemName;inv.descriptionLabel.text=inv.items[inv.selectedIndex].description;
            inv.priceLabel.text="Giá:  "+inv.items[inv.selectedIndex].price.ToString("N0");inv.sellLabel.text="Giá bán tối thiểu:  "+inv.items[inv.selectedIndex].sellPrice.ToString("N0");
            foreach(var b in inventoryRoot.GetComponentsInChildren<Button>(true))
            {
                string label=b.GetComponentInChildren<TMP_Text>()?.text;
                if(new[]{"BULK DISCARD","BULK SELL","INVENTORY EXPANSION","List on Marketplace","Inventory Comms"}.Contains(b.name))Feedback(b,inv,label);
            }
            // Keep English category IDs used by filtering; only their visible labels are localized.
            for(int i=0;i<inv.categoryButtons.Length;i++)inv.categoryButtons[i].GetComponentInChildren<TMP_Text>().text=Translate(inv.categories[i]);
            d.Refresh();
            foreach(var nav in UnityEngine.Object.FindObjectsByType<LobbyNavigationBar>(FindObjectsInactive.Include,FindObjectsSortMode.None))nav.Refresh();
            // Update reusable labels too; existing scene overrides remain intact.
            foreach(string path in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/GundamLobby/Inventory/Prefabs",Root+"/Prefabs","Assets/GundamLobby/Striker/Prefabs"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>!p.EndsWith("UnifiedNavigationBar.prefab")&&!p.Contains("Models")))
            {
                if(path.Contains("/Striker/Prefabs/")&&!path.EndsWith("StrikerRosterCard.prefab")&&!path.EndsWith("StrikerStatRow.prefab"))continue;
                var content=PrefabUtility.LoadPrefabContents(path);LocalizeTexts(content);PrefabUtility.SaveAsPrefabAsset(content,path);PrefabUtility.UnloadPrefabContents(content);
            }
            var chars=string.Concat(scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<TMP_Text>(true)).Select(t=>t.text))+string.Concat(inv.items.Select(t=>t.itemName+t.description+t.rarity));
            font.TryAddCharacters(new string(chars.Distinct().ToArray()),out string missing);
            EditorUtility.SetDirty(font);EditorUtility.SetDirty(d);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            return "Unified 3 navigation bars with four Vietnamese destinations; translated inventory items, actions, feedback, stats and roster. Missing font characters: "+missing;
        }
        public static void PolishInventoryLayout(InventoryPanel inv)
        {
            var root=inv.inventoryCanvas.transform.Find("Inventory Screen");
            var capacity=inv.capacityLabel;capacity.rectTransform.anchoredPosition=new Vector2(701,-101);capacity.rectTransform.sizeDelta=new Vector2(130,26);capacity.fontSize=18;capacity.enableAutoSizing=false;
            for(int i=0;i<inv.categoryButtons.Length;i++){var rt=inv.categoryButtons[i].GetComponent<RectTransform>();rt.anchoredPosition=new Vector2(24+i*112,-130);}
            root.Find("Category Rule").GetComponent<RectTransform>().anchoredPosition=new Vector2(24,-173);
            var area=inv.scroll.GetComponent<RectTransform>();area.anchoredPosition=new Vector2(24,-192);area.sizeDelta=new Vector2(820,724);
            inv.scroll.viewport.sizeDelta=new Vector2(808,724);
            area.Find("Scrollbar").GetComponent<RectTransform>().sizeDelta=new Vector2(3,724);
        }
    }
}
