using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GundamLobby.Editor
{
    public static class ShopScreenBuilder
    {
        const string Root="Assets/GundamLobby/Shop";
        static TMP_FontAsset font;static ShopController owner;
        static Color C(string s){ColorUtility.TryParseHtmlString("#"+s,out var color);return color;}
        static Color A(string s,float alpha){var c=C(s);c.a=alpha;return c;}
        static RectTransform Rect(string name,Transform p,float x,float y,float w,float h)
        {
            var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(p,false);rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;
        }
        static LobbyShape Shape(string name,Transform p,float x,float y,float w,float h,Color fill,Color border,LobbyShape.Geometry geometry=LobbyShape.Geometry.Box)
        {
            var rt=Rect(name,p,x,y,w,h);rt.gameObject.AddComponent<CanvasRenderer>();var s=rt.gameObject.AddComponent<LobbyShape>();s.geometry=geometry;s.color=fill;s.border=border;s.thickness=border.a>0?1:0;s.raycastTarget=false;return s;
        }
        static TMP_Text Label(string name,string value,Transform p,float x,float y,float w,float h,float size,Color color,TextAlignmentOptions align=TextAlignmentOptions.Left)
        {
            var rt=Rect(name,p,x,y,w,Mathf.Max(h,Mathf.Ceil(size*1.25f)));var t=rt.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.raycastTarget=false;t.margin=Vector4.zero;t.alignment=align;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Ellipsis;return t;
        }
        static Button Button(string name,string label,Transform p,float x,float y,float w,float h,UnityAction action,bool primary=false)
        {
            var s=Shape(name,p,x,y,w,h,primary?C("087CAB"):A("071117",.85f),primary?C("20C8F6"):C("214654"),primary?LobbyShape.Geometry.Tab:LobbyShape.Geometry.Panel);s.raycastTarget=true;
            var b=s.gameObject.AddComponent<Button>();b.targetGraphic=s;b.transition=Selectable.Transition.None;
            var hover=s.gameObject.AddComponent<LobbyHover>();hover.surface=s;hover.normal=s.color;hover.hover=C("153D50");hover.pressed=C("1B607B");hover.duration=.18f;
            Label("Label",label,b.transform,12,0,w-24,h,primary?25:20,C("E7F0F4"),TextAlignmentOptions.Center);
            if(action!=null)UnityEventTools.AddPersistentListener(b.onClick,action);return b;
        }
        static Image Picture(string name,Transform p,float x,float y,float w,float h)
        {
            var image=Rect(name,p,x,y,w,h).gameObject.AddComponent<Image>();image.raycastTarget=false;image.preserveAspect=true;return image;
        }
        static ShopArtwork Artwork(string name,ShopArtwork.Kind kind,Transform p,float x,float y,float w,float h,Color color)
        {
            var rt=Rect(name,p,x,y,w,h);rt.gameObject.AddComponent<CanvasRenderer>();var g=rt.gameObject.AddComponent<ShopArtwork>();g.kind=kind;g.color=color;g.raycastTarget=false;return g;
        }
        static void Rule(Transform p,float x,float y,float w){Shape("Rule",p,x,y,w,1,C("214654"),Color.clear);}
        static void CreditsIcon(Transform p,float x,float y,float size)
        {
            var g=Rect("Credits icon",p,x,y,size,size).gameObject.AddComponent<LobbyHeaderIcon>();g.symbol=LobbyHeaderIcon.Symbol.Credits;g.color=C("D6B761");g.raycastTarget=false;
        }
        static ShopCatalog Catalog()
        {
            var c=ScriptableObject.CreateInstance<ShopCatalog>();c.categories=new[]{"NGOẠI TRANG","HỌA TIẾT","SƠN","MÔ-ĐUN","KIỂU DÁNG","BỘ TRANG BỊ","VẬT TƯ","VŨ KHÍ","THIẾT BỊ BAY","THIẾT BỊ"};
            var items=new List<ShopItemDefinition>();int[] prices={1611,1674,1830,1862,1862,1893,1917,1924,1999,2029,2100,2180,2300,2450,2590,2710,2860,3100};
            for(int i=0;i<18;i++)items.Add(new ShopItemDefinition{title=i<12?"Khung · Mưa lửa":"Khung · "+new[]{"Quỹ đạo","Tiên phong","Ngân hà","Bão plasma","Chiến tuyến","Bình minh"}[i-12],description="Khung trang trí cho hồ sơ chỉ huy. Thiết kế góc cạnh kết hợp viền kim loại vàng và họa tiết kỹ thuật.",code="MS-"+(i+1).ToString("0000"),category=0,rarity=2,price=prices[i],unitPrice=prices[i],auction=i<12,bids=i%6==0?1:i%5,remainingMinutes=1327-i*17,isMyListing=i%7==0,isMyBid=i<12&&i%6==1,frameArtwork=true});
            string[] art={"","PatternLeaf","EnergyCell","ReactorCore","Emblem","ArmorPlate","SupplyCrate","BeamRifle","ArmorPlate","PilotModule"};
            string[] stems={"","Họa tiết","Ống sơn","Lõi phản ứng","Biểu tượng","Bộ giáp","Hòm tiếp tế","Súng tia","Cánh hỗ trợ","Thiết bị điều khiển"};
            string[] suffix={"Quỹ đạo","Băng giá","Tiên phong","Bão điện","Chiến tuyến","Bạch kim"};
            for(int cat=1;cat<10;cat++)for(int i=0;i<6;i++)
            {
                int index=items.Count;var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GundamLobby/Inventory/Art/"+art[cat]+".png");int price=800+cat*310+i*127;
                items.Add(new ShopItemDefinition{title=stems[cat]+" · "+suffix[i],description="Vật phẩm thuộc danh mục "+c.categories[cat].ToLowerInvariant()+", dùng để tùy chỉnh và trang bị cho Gundam. Chọn thao tác giao dịch để xem phản hồi mẫu.",code="MS-"+(index+1).ToString("0000"),category=cat,rarity=i%3,price=price,unitPrice=price,quantity=i+1,auction=i%2==0,bids=i%4,remainingMinutes=720+cat*41+i*19,isMyListing=i==0,isMyBid=i==2,icon=sprite});
            }
            c.items=items.ToArray();AssetDatabase.CreateAsset(c,Root+"/Data/ShopCatalog.asset");return c;
        }
        static void Sidebar(Transform root)
        {
            Shape("Sidebar backing",root,16,110,348,893,A("071117",.7f),Color.clear,LobbyShape.Geometry.Panel);
            Label("Categories heading","DANH MỤC",root,30,121,318,30,22,C("829EAC"));Rule(root,30,158,318);
            owner.categoryButtons=new Button[owner.catalog.categories.Length];
            for(int i=0;i<owner.categoryButtons.Length;i++){var b=Button("Shop Category "+i,owner.catalog.categories[i],root,30,170+i*49,318,43,null);b.GetComponentInChildren<TMP_Text>().alignment=TextAlignmentOptions.Left;UnityEventTools.AddIntPersistentListener(b.onClick,owner.SetCategory,i);owner.categoryButtons[i]=b;}
            Rule(root,30,701,318);owner.listingButton=Button("My Listings","ĐANG ĐĂNG CỦA TÔI",root,30,727,318,46,owner.ShowMyListings);owner.bidsButton=Button("My Bids","ĐÃ ĐẶT GIÁ",root,30,784,318,46,owner.ShowMyBids);
            Label("Market caption","CHỢ VẬT PHẨM",root,30,883,318,30,20,C("20C8F6"));
            var hint=Label("Shop hint","Chọn vật phẩm để xem thông tin\nvà lựa chọn giao dịch.",root,30,924,318,62,19,C("829EAC"));hint.textWrappingMode=TextWrappingModes.Normal;
        }
        static void Toolbar(Transform root)
        {
            owner.tradeButtons=new Button[3];string[] tabs={"TẤT CẢ","ĐANG BÁN","ĐẤU GIÁ"};
            for(int i=0;i<3;i++){var b=Button("Shop Trade Tab "+i,tabs[i],root,400+i*152,117,144,42,null);b.GetComponent<LobbyShape>().geometry=LobbyShape.Geometry.Tab;UnityEventTools.AddIntPersistentListener(b.onClick,owner.SetTradeTab,i);owner.tradeButtons[i]=b;}
            Rule(root,400,160,448);
            var filter=Button("Shop Filter","BỘ LỌC",root,1414,117,190,38,owner.ToggleFilter);owner.filterLabel=filter.GetComponentInChildren<TMP_Text>();owner.filterLabel.fontSize=18;
            var sort=Button("Shop Sort","GIÁ TĂNG DẦN",root,1616,117,272,38,owner.Sort);owner.sortLabel=sort.GetComponentInChildren<TMP_Text>();owner.sortLabel.fontSize=19;
            var background=Shape("Shop Search Input",root,400,176,1264,44,A("041018",.95f),C("214654"));background.raycastTarget=true;
            var viewport=Rect("Text Area",background.transform,14,0,1236,44);viewport.gameObject.AddComponent<RectMask2D>();
            var text=Label("Input Text","",viewport,0,0,1236,44,21,C("E7F0F4"));text.overflowMode=TextOverflowModes.Overflow;
            var placeholder=Label("Placeholder","Nhập tên vật phẩm...",viewport,0,0,1236,44,20,C("53778A"));
            var input=background.gameObject.AddComponent<TMP_InputField>();input.textViewport=viewport;input.textComponent=(TextMeshProUGUI)text;input.placeholder=placeholder;input.targetGraphic=background;input.fontAsset=font;input.lineType=TMP_InputField.LineType.SingleLine;input.characterLimit=80;input.customCaretColor=true;input.caretColor=C("20C8F6");input.selectionColor=A("20C8F6",.25f);input.text="";owner.searchField=input;
            Button("Shop Search","TÌM KIẾM",root,1680,176,208,44,owner.Search,true);
            owner.viewLabel=Label("Shop current view","NGOẠI TRANG",root,400,226,1110,24,17,C("829EAC"));owner.resultLabel=Label("Shop result count","18 VẬT PHẨM",root,1688,226,200,24,17,C("20C8F6"),TextAlignmentOptions.Right);
        }
        static void Grid(Transform root)
        {
            var grid=Rect("Shop Items Grid",root,400,258,1488,664);var layout=grid.gameObject.AddComponent<GridLayoutGroup>();layout.cellSize=new Vector2(284,320);layout.spacing=new Vector2(16,24);layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=5;layout.childAlignment=TextAnchor.UpperLeft;
            var cardRoot=Shape("ShopItemCard",null,0,0,284,320,Color.clear,Color.clear);cardRoot.raycastTarget=true;var b=cardRoot.gameObject.AddComponent<Button>();b.transition=Selectable.Transition.None;
            var thumb=Shape("Thumbnail",cardRoot.transform,0,0,284,200,C("102235"),C("1D4258"));thumb.gradient=true;thumb.gradientVertical=true;thumb.gradientEnd=C("071422");
            Shape("Artwork grid",thumb.transform,1,1,282,163,A("25475A",.22f),Color.clear,LobbyShape.Geometry.Grid);
            Shape("Title strip",thumb.transform,1,164,282,35,C("091923"),Color.clear);
            var hover=cardRoot.gameObject.AddComponent<LobbyHover>();hover.surface=thumb;hover.normal=thumb.color;hover.hover=C("17415A");hover.pressed=C("1F5D78");b.targetGraphic=thumb;
            var card=cardRoot.gameObject.AddComponent<ShopItemCard>();card.titleLabel=Label("Item title","Khung · Mưa lửa",thumb.transform,11,166,262,31,24,C("D8EAF2"));card.titleLabel.enableAutoSizing=true;card.titleLabel.fontSizeMin=18;card.titleLabel.fontSizeMax=24;
            card.frameArt=Artwork("Frame artwork",ShopArtwork.Kind.Frame,thumb.transform,101,18,82,128,C("E0B75D"));card.icon=Picture("Item icon",thumb.transform,38,19,208,126);
            card.rarityMark=Shape("Rarity corner",thumb.transform,0,0,44,44,A("E0B75D",.55f),Color.clear,LobbyShape.Geometry.Triangle);
            card.auctionArt=Artwork("Auction symbol",ShopArtwork.Kind.Auction,thumb.transform,249,10,24,26,C("1E8ABC"));card.tradeLabel=Label("Buy now tag","MUA NGAY",thumb.transform,193,9,80,24,13,C("57B8D6"),TextAlignmentOptions.Right);
            card.priceHeading=Label("Price heading","GIÁ ĐẶT",cardRoot.transform,0,207,90,35,20,C("ADC1CB"));CreditsIcon(cardRoot.transform,94,216,20);card.priceLabel=Label("Primary price","1,611",cardRoot.transform,122,204,162,39,29,C("F15D30"));
            Rule(cardRoot.transform,0,245,284);Label("Unit price heading","ĐƠN GIÁ",cardRoot.transform,0,250,104,23,17,C("829EAC"));card.unitPriceLabel=Label("Unit price","1,611",cardRoot.transform,113,249,171,25,19,C("E7F0F4"),TextAlignmentOptions.Right);
            card.timeHeading=Label("Time heading","THỜI GIAN",cardRoot.transform,0,274,109,22,17,C("829EAC"));card.timeLabel=Label("Remaining time","22 giờ 07 phút",cardRoot.transform,112,273,172,23,17,C("9EB8C7"),TextAlignmentOptions.Right);
            card.bidsHeading=Label("Bids heading","LƯỢT ĐẶT",cardRoot.transform,0,298,109,22,17,C("829EAC"));card.bidsLabel=Label("Bids","1 lượt",cardRoot.transform,112,298,172,22,17,C("9EB8C7"),TextAlignmentOptions.Right);
            var prefab=PrefabUtility.SaveAsPrefabAsset(cardRoot.gameObject,Root+"/Prefabs/ShopItemCard.prefab");UnityEngine.Object.DestroyImmediate(cardRoot.gameObject);owner.cards=new ShopItemCard[10];
            for(int i=0;i<10;i++){var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,grid);go.name="Shop Item Slot "+i;var view=go.GetComponent<ShopItemCard>();view.owner=owner;owner.cards[i]=view;}
            var empty=Rect("Shop No Results",root,450,458,1388,230);Label("Empty title","KHÔNG CÓ VẬT PHẨM PHÙ HỢP",empty,0,25,1388,50,32,C("ADC1CB"),TextAlignmentOptions.Center);Button("Clear Filters","XÓA BỘ LỌC",empty,510,112,368,53,owner.ClearFilters);owner.emptyState=empty.gameObject;
            owner.previousButton=Button("Shop Previous Page","<",root,400,952,45,40,owner.PreviousPage);owner.pageLabel=Label("Page count","1 / 2",root,458,952,112,40,23,C("20C8F6"),TextAlignmentOptions.Center);owner.nextButton=Button("Shop Next Page",">",root,584,952,45,40,owner.NextPage);
        }
        static Button Backdrop(string name,Transform p,Color color,UnityAction close)
        {
            var rt=Rect(name,p,0,0,1920,1080);var img=rt.gameObject.AddComponent<Image>();img.color=color;var b=rt.gameObject.AddComponent<Button>();b.targetGraphic=img;b.transition=Selectable.Transition.None;UnityEventTools.AddPersistentListener(b.onClick,close);return b;
        }
        static void Filter(Transform root)
        {
            var overlay=Rect("Shop Rarity Filter",root,0,0,1920,1080);owner.filterPanel=overlay.gameObject;Backdrop("Dismiss filter",overlay,Color.clear,owner.CloseFilter);
            var panel=Shape("Rarity menu",overlay,1396,165,248,266,C("091D29"),C("20C8F6"),LobbyShape.Geometry.Panel);panel.raycastTarget=true;Label("Filter heading","ĐỘ HIẾM",panel.transform,14,9,220,27,18,C("829EAC"));
            owner.rarityButtons=new Button[4];string[] labels={"TẤT CẢ","HIẾM","SỬ THI","HUYỀN THOẠI"};
            for(int i=0;i<4;i++){var b=Button("Rarity option "+i,labels[i],panel.transform,12,43+i*43,224,37,null);UnityEventTools.AddIntPersistentListener(b.onClick,owner.SetRarity,i-1);owner.rarityButtons[i]=b;}
            Button("Reset Shop Filters","XÓA BỘ LỌC",panel.transform,12,224,224,30,owner.ClearFilters);owner.filterPanel.SetActive(false);
        }
        static void Detail(Transform root)
        {
            var overlay=Rect("Shop Item Detail",root,0,0,1920,1080);owner.detailPanel=overlay.gameObject;Backdrop("Dismiss item detail",overlay,A("02080D",.8f),owner.CloseDetail);
            var panel=Shape("Item Detail Dialog",overlay,465,263,990,540,C("0B1D29"),C("2C657D"),LobbyShape.Geometry.Panel);panel.raycastTarget=true;
            Label("Detail heading","THÔNG TIN VẬT PHẨM",panel.transform,26,20,600,30,18,C("829EAC"));Button("Close Item Detail","X",panel.transform,920,18,42,36,owner.CloseDetail);
            Shape("Detail artwork well",panel.transform,26,72,184,286,C("07141F"),C("214654"),LobbyShape.Geometry.Panel);owner.detailFrame=Artwork("Detail frame artwork",ShopArtwork.Kind.Frame,panel.transform,46,96,144,224,C("E0B75D"));owner.detailIcon=Picture("Detail item icon",panel.transform,35,100,166,220);
            owner.detailCategory=Label("Detail category","NGOẠI TRANG / HUYỀN THOẠI",panel.transform,242,65,664,28,18,C("20C8F6"));owner.detailTitle=Label("Detail item title","Khung · Mưa lửa",panel.transform,242,101,690,49,35,C("E7F0F4"));owner.detailTitle.enableAutoSizing=true;owner.detailTitle.fontSizeMin=26;owner.detailTitle.fontSizeMax=35;
            owner.detailDescription=Label("Detail description","",panel.transform,242,164,688,87,22,C("ADC1CB"));owner.detailDescription.textWrappingMode=TextWrappingModes.Normal;
            owner.detailMeta=Label("Detail trade metadata","",panel.transform,242,269,688,59,21,C("829EAC"));owner.detailMeta.textWrappingMode=TextWrappingModes.Normal;
            CreditsIcon(panel.transform,242,349,28);owner.detailPrice=Label("Detail price","1,611 TÍN DỤNG",panel.transform,284,337,646,48,35,C("E2C575"));
            var action=Button("Trade Selected Item","ĐẶT GIÁ",panel.transform,242,422,410,64,owner.Trade,true);owner.detailActionLabel=action.GetComponentInChildren<TMP_Text>();owner.detailActionLabel.fontSize=30;
            Button("Return to Shop","QUAY LẠI",panel.transform,672,422,258,64,owner.CloseDetail);owner.detailPanel.SetActive(false);
        }
        [MenuItem("Tools/Gundam Lobby/Install Vietnamese Shop")]
        public static string Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/GundamLobby/Scenes/HomeLobby.unity")throw new InvalidOperationException("Open HomeLobby");
            if(UnityEngine.Object.FindFirstObjectByType<ShopController>())throw new InvalidOperationException("Shop already installed");
            Directory.CreateDirectory(Root+"/Data");Directory.CreateDirectory(Root+"/Prefabs");EditorSceneManager.SaveScene(scene,"Assets/GundamLobby/Backups/HomeLobby_BeforeShop_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/GundamLobby/Inventory/Fonts/Inventory Barlow Condensed SDF.asset");var d=UnityEngine.Object.FindFirstObjectByType<HomeLobbyDashboard>();var sys=new GameObject("Shop Systems");Undo.RegisterCreatedObjectUndo(sys,"Install Shop");owner=sys.AddComponent<ShopController>();owner.dashboard=d;owner.catalog=Catalog();d.shopPanel=owner;
            var canvas=new GameObject("Shop Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<Canvas>().sortingOrder=80;var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;owner.shopCanvas=canvas;
            var root=Rect("Shop Screen",canvas.transform,0,0,1920,1080);root.anchorMin=root.anchorMax=root.pivot=new Vector2(.5f,.5f);root.anchoredPosition=Vector2.zero;
            var backdrop=Rect("Atmospheric Shop Background",root,0,0,1920,1080);backdrop.gameObject.AddComponent<CanvasRenderer>();var graphic=backdrop.gameObject.AddComponent<ShopBackdrop>();graphic.raycastTarget=false;
            Sidebar(root);Toolbar(root);Grid(root);
            var headerPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GundamLobby/Weaponry/Prefabs/CommandNavigationBar.prefab");var header=(GameObject)PrefabUtility.InstantiatePrefab(headerPrefab,root);header.name="Shop Command Header";var nav=header.GetComponent<LobbyNavigationBar>();nav.dashboard=d;nav.page=LobbyNavigationBar.Page.Shop;nav.Refresh();
            Rule(root,24,1024,1872);Button("Shop Comms","LIÊN LẠC",root,24,1036,132,32,owner.Comms);Label("Chat hint","Enter   BẮT ĐẦU CHAT",root,179,1035,300,34,18,C("829EAC"));Button("Shop Return","Esc   QUAY LẠI",root,1680,1036,208,32,owner.Close);
            Filter(root);Detail(root);
            var toast=Shape("Shop Toast",root,540,865,840,86,A("071117",.97f),C("20C8F6"),LobbyShape.Geometry.Panel);owner.toast=toast.gameObject.AddComponent<CanvasGroup>();owner.toast.alpha=0;owner.toast.blocksRaycasts=false;owner.toast.interactable=false;owner.toastLabel=Label("Shop message","",toast.transform,18,10,804,66,22,C("E7F0F4"),TextAlignmentOptions.Center);owner.toastLabel.textWrappingMode=TextWrappingModes.Normal;
            owner.RefreshResults();canvas.SetActive(false);EditorUtility.SetDirty(d);EditorUtility.SetDirty(owner);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Selection.activeGameObject=sys;
            File.WriteAllText(Root+"/README_VI.md","# Cửa hàng / chợ vật phẩm\n\nMở HomeLobby và bấm CỬA HÀNG. Menu và tiền tệ dùng cùng CommandNavigationBar với các màn hiện có. 10 danh mục, 3 tab Tất cả/Đang bán/Đấu giá, lưới 5 cột x 2 hàng, tìm kiếm không phân biệt dấu, lọc độ hiếm, sắp xếp và phân trang chạy trên dữ liệu mẫu. Đang đăng của tôi/Đã đặt giá lọc các lô mẫu tương ứng.\n\nBấm vật phẩm để mở popup chi tiết; Mua ngay/Đặt giá có phản hồi mẫu. Chưa có thanh toán, trừ tiền, cập nhật inventory hay đấu giá online. Esc đóng popup/bộ lọc trước, sau đó quay về lobby. Phòng nghiên cứu, Chiến đấu và Kho đồ trên header đều điều hướng tới màn hiện có.\n\nShop/Data/ShopCatalog.asset chứa 72 lô hàng mẫu. Chỉnh tên, giá, chế độ bán, icon và các thuộc tính trong Inspector. ShopItemCard.prefab có từng thành phần chữ, nền, giá và artwork riêng. Khung ngoại trang là vector UI; icon các danh mục khác tái sử dụng asset hiện có. Không dùng nguyên ảnh tham chiếu làm giao diện. Scene được sao lưu trong GundamLobby/Backups trước khi cài đặt.\n",new System.Text.UTF8Encoding(false));
            return "Installed Vietnamese Shop: 10 categories, 72 sample listings, 5x2 grid, accent-insensitive search, rarity filter, sorting, pagination and item detail dialog with prototype trade feedback.";
        }
    }
}
