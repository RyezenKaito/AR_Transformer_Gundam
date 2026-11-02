using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
namespace GundamLobby.Editor
{
 public static class InventoryBuilder
 {
  const string Root="Assets/GundamLobby/Inventory";
  static TMP_FontAsset font;
  static InventoryPanel panel;
  static Color bg=Hex("071117"),surface=Hex("0B1B25"),line=Hex("214654"),cyan=Hex("20C8F6"),white=Hex("E7F0F4"),muted=Hex("829EAC");
  static Color Hex(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
  static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
  {
   var g=new GameObject(name,typeof(RectTransform));var t=g.GetComponent<RectTransform>();t.SetParent(parent,false);t.anchorMin=t.anchorMax=new Vector2(0,1);t.pivot=new Vector2(0,1);t.anchoredPosition=new Vector2(x,-y);t.sizeDelta=new Vector2(w,h);return t;
  }
  static LobbyShape Shape(string name,Transform parent,float x,float y,float w,float h,Color fill,Color stroke,LobbyShape.Geometry geometry=LobbyShape.Geometry.Box)
  {
   var t=Rect(name,parent,x,y,w,h);t.gameObject.AddComponent<CanvasRenderer>();var s=t.gameObject.AddComponent<LobbyShape>();s.geometry=geometry;s.color=fill;s.border=stroke;s.thickness=stroke.a>0?1:0;s.raycastTarget=false;return s;
  }
  static TMP_Text Text(string name,string value,Transform parent,float x,float y,float w,float h,float size,Color color,TextAlignmentOptions align=TextAlignmentOptions.Left)
  {
   var t=Rect(name,parent,x,y,w,h);var l=t.gameObject.AddComponent<TextMeshProUGUI>();l.font=font;l.text=value;l.fontSize=size;l.color=color;l.alignment=align;l.textWrappingMode=TextWrappingModes.NoWrap;l.overflowMode=TextOverflowModes.Overflow;l.raycastTarget=false;l.margin=Vector4.zero;return l;
  }
  static Button Button(string name,string label,Transform p,float x,float y,float w,float h,UnityAction action,bool active=false)
  {
   var s=Shape(name,p,x,y,w,h,active?Hex("102A37"):bg,active?cyan:line,active?LobbyShape.Geometry.Tab:LobbyShape.Geometry.Box);s.raycastTarget=true;var b=s.gameObject.AddComponent<Button>();b.targetGraphic=s;b.transition=Selectable.Transition.None;
   var hover=s.gameObject.AddComponent<LobbyHover>();hover.surface=s;hover.normal=s.color;hover.hover=new Color(.055f,.23f,.30f);hover.pressed=new Color(.07f,.36f,.46f);
   if(action!=null)UnityEventTools.AddPersistentListener(b.onClick,action);
   Text("Label",label,s.transform,8,0,w-16,h,18,active?cyan:white,TextAlignmentOptions.Center);return b;
  }
  static Image Picture(string name,Sprite sprite,Transform p,float x,float y,float w,float h)
  {
   var t=Rect(name,p,x,y,w,h);var im=t.gameObject.AddComponent<Image>();im.sprite=sprite;im.preserveAspect=true;im.raycastTarget=false;return im;
  }
  [MenuItem("Tools/Gundam Lobby/Build Inventory from Figma")]
  public static string Build()
  {
   if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");var scene=SceneManager.GetActiveScene();if(scene.path!="Assets/GundamLobby/Scenes/HomeLobby.unity")throw new InvalidOperationException("Open HomeLobby");
   if(UnityEngine.Object.FindFirstObjectByType<InventoryPanel>())throw new InvalidOperationException("Inventory already installed");
   foreach(var p in new[]{Root,Root+"/Prefabs",Root+"/Fonts"})Directory.CreateDirectory(p);
   foreach(var path in Directory.GetFiles(Root+"/Art","*.png"))
   {
    string assetPath=path.Replace('\\','/');var importer=AssetImporter.GetAtPath(assetPath) as TextureImporter;
    if(importer){importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
   }
   EditorSceneManager.SaveScene(scene,"Assets/GundamLobby/Backups/HomeLobby_BeforeInventory_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);
   var f=AssetDatabase.LoadAssetAtPath<Font>(Root+"/Fonts/BarlowCondensed-Medium.ttf");if(!f)throw new InvalidOperationException("Inventory font missing");
   font=TMP_FontAsset.CreateFontAsset(f,48,6,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic,true);font.name="Inventory Barlow Condensed SDF";AssetDatabase.CreateAsset(font,Root+"/Fonts/Inventory Barlow Condensed SDF.asset");
   font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,:;/-+·()",out string missing);if(font.atlasTexture&&!AssetDatabase.Contains(font.atlasTexture))AssetDatabase.AddObjectToAsset(font.atlasTexture,font);if(font.material&&!AssetDatabase.Contains(font.material))AssetDatabase.AddObjectToAsset(font.material,font);
   var system=new GameObject("Inventory Systems");Undo.RegisterCreatedObjectUndo(system,"Create Inventory");panel=system.AddComponent<InventoryPanel>();panel.lobby=UnityEngine.Object.FindFirstObjectByType<HomeLobbyController>();panel.lobbyCanvas=GameObject.Find("Home Lobby Canvas");panel.modelPrefab=panel.lobby.gundamPrefab;
   var canvasGO=new GameObject("Inventory Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));Undo.RegisterCreatedObjectUndo(canvasGO,"Inventory UI");var canvas=canvasGO.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=40;var scaler=canvasGO.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;panel.inventoryCanvas=canvasGO;
   var full=Shape("Inventory Fullscreen Background",canvasGO.transform,0,0,1920,1080,bg,Color.clear);full.rectTransform.anchorMin=Vector2.zero;full.rectTransform.anchorMax=Vector2.one;full.rectTransform.offsetMin=full.rectTransform.offsetMax=Vector2.zero;
   var root=Shape("Inventory Screen",canvasGO.transform,0,0,1920,1080,bg,Color.clear);root.raycastTarget=true;root.rectTransform.anchorMin=root.rectTransform.anchorMax=root.rectTransform.pivot=new Vector2(.5f,.5f);root.rectTransform.anchoredPosition=Vector2.zero;
   var backdrop=Shape("Preview Background",root.transform,856,64,1064,1016,Hex("173F50"),Color.clear);backdrop.gradient=true;backdrop.gradientEnd=Hex("08131B");backdrop.gradientVertical=true;
   BuildPreview(root.transform);
   BuildHeader(root.transform);BuildCategories(root.transform);BuildGrid(root.transform);BuildDetails(root.transform);BuildFooter(root.transform);BuildToast(root.transform);
   panel.items=SampleItems();panel.selectedIndex=3;
   var nav=panel.lobbyCanvas.GetComponentsInChildren<Button>(true).First(b=>b.name=="Nav INVENTORY");var old=nav.GetComponent<LobbyButtonAction>();if(old)Undo.DestroyObjectImmediate(old);Undo.RecordObject(nav,"Wire Inventory");UnityEventTools.AddPersistentListener(nav.onClick,panel.Open);EditorUtility.SetDirty(nav);
   canvasGO.SetActive(false);panel.previewRig.SetActive(false);
   AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   File.WriteAllText(Root+"/README_VI.md","# Inventory / Figma to Unity\n\nFigma: https://www.figma.com/design/2b9E6oxBS3ky4vy433zjbP?node-id=10-13\n\nMo scene Assets/GundamLobby/Scenes/HomeLobby.unity, Play va bam INVENTORY. Esc hoac STRIKER BAY de quay lai lobby. Bam vat pham de doi thong tin; tab de loc; cuon de xem them; TYPE de sap xep; keo o Gundam de xoay, cuon tren Gundam de zoom. USE va cac nut giao dich chi hien phan hoi mau, khong co backend hoac giao dich that.\n\nInventory Systems > InventoryPanel cho phep thay danh sach items, icon va modelPrefab. ItemTile, ActionButton va CategoryTab la prefab rieng. Inventory Canvas la UI uGUI + TextMeshPro co the chinh sua tung nut, chu va panel; khong dung anh chup giao dien. Canvas reference 1920x1080. PreviewRig cach ly tai X=2000, render qua RenderTexture trong suot, khong doi camera lobby. Hinh vat pham la thumbnail placeholder tu mesh don gian; thay icon trong items bang asset that khi co.\n\nBarlow Condensed Medium tu Google Fonts, SIL Open Font License.\n");
   return "Inventory installed: 40 items, 5-column scroll grid, category filtering, selection details, 3D rotation/zoom, return navigation. Saved HomeLobby.";
  }
  static Sprite Art(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/"+name+".png");
  static void BuildHeader(Transform p)
  {
   var header=Shape("Inventory Header",p,0,0,1920,72,bg,Color.clear);Shape("Header Rule",header.transform,0,71,1920,1,line,Color.clear);
   Text("Commander","AntAkA",header.transform,30,10,160,27,23,white);Text("Title","Sapphire Seahawk",header.transform,30,38,220,18,13,muted);Picture("Avatar",Art("CommanderPortrait"),header.transform,222,3,74,66);
   string[] names={"STRIKER BAY","TACTICAL STATION","OPERATION STORM","MARKETPLACE","LOGISTICS","INVENTORY","STORE"};
   for(int i=0;i<names.Length;i++){int index=i;var b=Button("Inventory Nav "+names[i],names[i],header.transform,430+i*154,0,146,70,index==0?(UnityAction)panel.Close:null,i==5);b.GetComponent<LobbyShape>().thickness=0;if(i!=0&&i!=5)UnityEventTools.AddStringPersistentListener(b.onClick,panel.Notify,names[i]+"  ·  Function in development");}
   Text("Currency Mark","M",header.transform,1597,23,28,30,20,Hex("C9A846"));Text("Credits","17,020",header.transform,1635,23,100,30,23,white);Shape("Token",header.transform,1808,25,20,20,Hex("C9A846"),Color.clear,LobbyShape.Geometry.Diamond);Text("Tokens","0",header.transform,1840,23,60,30,23,white);
  }
  static void BuildCategories(Transform p)
  {
   panel.categoryButtons=new Button[7];for(int i=0;i<7;i++)panel.categoryButtons[i]=Button("Category "+panel.categories[i],panel.categories[i],p,24+i*112,110,112,42,null,i==0);
   Shape("Category Rule",p,24,153,806,1,line,Color.clear);panel.capacityLabel=Text("Capacity","113/240",p,701,83,130,25,21,cyan);
   var proto=UnityEngine.Object.Instantiate(panel.categoryButtons[1].gameObject);proto.name="InventoryCategoryTab";PrefabUtility.SaveAsPrefabAsset(proto,Root+"/Prefabs/InventoryCategoryTab.prefab");UnityEngine.Object.DestroyImmediate(proto);
  }
  static void BuildGrid(Transform p)
  {
   var area=Rect("Inventory Scroll",p,24,172,820,744);var scroll=area.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=35;panel.scroll=scroll;
   var view=Rect("Viewport",area,0,0,808,744);view.gameObject.AddComponent<RectMask2D>();var hit=view.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;
   var content=Rect("Item Grid",view,0,0,808,852);var grid=content.gameObject.AddComponent<GridLayoutGroup>();grid.cellSize=new Vector2(152,96);grid.spacing=new Vector2(12,12);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=5;grid.startCorner=GridLayoutGroup.Corner.UpperLeft;grid.childAlignment=TextAnchor.UpperLeft;var size=content.gameObject.AddComponent<ContentSizeFitter>();size.verticalFit=ContentSizeFitter.FitMode.PreferredSize;panel.content=content;scroll.content=content;scroll.viewport=view;
   var track=Rect("Scrollbar",area,816,0,3,744);track.gameObject.AddComponent<Image>().color=line;var sb=track.gameObject.AddComponent<Scrollbar>();sb.direction=Scrollbar.Direction.BottomToTop;var handle=Rect("Handle",track,0,0,3,300);handle.anchorMin=Vector2.zero;handle.anchorMax=Vector2.one;handle.offsetMin=handle.offsetMax=Vector2.zero;handle.pivot=new Vector2(.5f,.5f);var hi=handle.gameObject.AddComponent<Image>();hi.color=cyan;sb.handleRect=handle;sb.targetGraphic=hi;scroll.verticalScrollbar=sb;
   var tile=Shape("InventoryItemTile",null,0,0,152,96,surface,line);tile.raycastTarget=true;var button=tile.gameObject.AddComponent<Button>();button.targetGraphic=tile;button.transition=Selectable.Transition.None;var hover=tile.gameObject.AddComponent<LobbyHover>();hover.surface=tile;hover.normal=surface;hover.hover=Hex("123948");hover.pressed=Hex("175468");var tv=tile.gameObject.AddComponent<InventoryItemTile>();tv.surface=tile;
   tv.rarityMarks=new Graphic[]{Shape("Rarity Slash",tile.transform,0,0,4,28,Hex("9D47D5"),Color.clear),Shape("Rarity Track",tile.transform,8,86,54,2,Hex("9D47D5"),Color.clear)};
   Picture("Technical Hatch",Art("FigmaHatch"),tile.transform,0,0,152,96);
   tv.artwork=Picture("Item Artwork",Art("SupplyCrate"),tile.transform,8,4,136,82);tv.quantity=Text("Quantity","30",tile.transform,104,67,40,25,21,muted);
   var corners=Shape("Selected Corners",tile.transform,0,0,152,96,cyan,Color.clear,LobbyShape.Geometry.Corners);corners.cut=12;corners.thickness=2;tv.selectedCorners=corners.gameObject;corners.gameObject.SetActive(false);
   var prefab=PrefabUtility.SaveAsPrefabAsset(tile.gameObject,Root+"/Prefabs/InventoryItemTile.prefab");panel.tilePrefab=prefab.GetComponent<InventoryItemTile>();UnityEngine.Object.DestroyImmediate(tile.gameObject);
   string[] labels={"TYPE","BULK DISCARD","BULK SELL","INVENTORY EXPANSION"};for(int i=0;i<4;i++){var b=Button(labels[i],labels[i],p,24+i*202,936,194,42,i==0?(UnityAction)panel.SortItems:null);if(i==0)panel.sortLabel=b.GetComponentInChildren<TMP_Text>();else UnityEventTools.AddStringPersistentListener(b.onClick,panel.Notify,labels[i]+"  ·  Prototype feedback");if(i==0){var proto=UnityEngine.Object.Instantiate(b.gameObject);proto.name="InventoryActionButton";proto.GetComponent<Button>().onClick=new Button.ButtonClickedEvent();PrefabUtility.SaveAsPrefabAsset(proto,Root+"/Prefabs/InventoryActionButton.prefab");UnityEngine.Object.DestroyImmediate(proto);}}
  }
  static void BuildPreview(Transform p)
  {
   // A single-sample target avoids URP native-render-pass resolve errors on this project.
   var rt=new RenderTexture(1100,1100,24,RenderTextureFormat.ARGB32);rt.name="Inventory Preview RT";rt.antiAliasing=1;rt.bindTextureMS=false;AssetDatabase.CreateAsset(rt,Root+"/InventoryPreview.renderTexture");
   var rig=new GameObject("Inventory Preview Rig - isolated at X2000");rig.transform.position=new Vector3(2000,0,0);panel.previewRig=rig;var mount=new GameObject("Replaceable Gundam Mount").transform;mount.SetParent(rig.transform,false);mount.localRotation=Quaternion.Euler(0,-22,0);panel.modelMount=mount;
   var cg=new GameObject("Inventory Preview Camera");cg.transform.SetParent(rig.transform,false);cg.transform.localPosition=new Vector3(2.5f,4.1f,-16);cg.transform.LookAt(rig.transform.position+new Vector3(0,4.2f,0));var cam=cg.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=5.2f;cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;cam.allowHDR=false;cam.allowMSAA=false;cam.useOcclusionCulling=false;cam.stereoTargetEye=StereoTargetEyeMask.None;cam.targetTexture=rt;cam.nearClipPlane=.1f;cam.farClipPlane=40;var cd=cam.GetUniversalAdditionalCameraData();cd.renderPostProcessing=false;cd.stopNaN=true;cd.allowXRRendering=false;cd.requiresDepthOption=CameraOverrideOption.Off;cd.requiresColorOption=CameraOverrideOption.Off;panel.previewCamera=cam;
   var key=new GameObject("Preview Key Light");key.transform.SetParent(rig.transform,false);key.transform.localPosition=new Vector3(-4,8,-6);key.transform.LookAt(rig.transform.position+Vector3.up*4);var light=key.AddComponent<Light>();light.type=LightType.Spot;light.range=24;light.spotAngle=90;light.intensity=30;light.cullingMask=1<<30;light.color=new Color(.85f,.94f,1);
   var slot=Rect("Gundam 3D Preview - Drag and Zoom",p,1050,92,830,870);var raw=slot.gameObject.AddComponent<RawImage>();raw.texture=rt;raw.raycastTarget=true;var drag=slot.gameObject.AddComponent<InventoryPreviewDrag>();drag.modelMount=mount;drag.previewCamera=cam;
   cam.aspect=830f/870f;
  }
  static void BuildDetails(Transform p)
  {
   var detail=Rect("Selected Item Details",p,904,650,550,330);
   panel.rarityLabel=Text("Rarity","Flawless",detail,0,0,450,25,18,muted);panel.itemNameLabel=Text("Item Name","Pattern · Soft Leaf",detail,0,30,550,48,39,cyan);
   panel.descriptionLabel=Text("Description","Contains the corresponding pattern which can be used for\npaint jobs.",detail,0,91,535,57,20,muted);panel.descriptionLabel.textWrappingMode=TextWrappingModes.Normal;
   Text("Supply Label","MILITARY SUPPLY SALE",detail,0,164,200,26,15,muted);panel.priceLabel=Text("Supply Price","Price: 1,600",detail,218,161,240,30,21,white);
   Text("Market Label","MARKETPLACE",detail,0,199,170,26,15,muted);panel.sellLabel=Text("Selling Price","Min. selling price: 64",detail,170,197,310,30,20,muted);
   Button("Use Selected Item","Space   USE",detail,0,244,440,42,panel.UseSelected);var list=Button("List on Marketplace","LIST ON MARKETPLACE",detail,0,292,440,42,null);UnityEventTools.AddStringPersistentListener(list.onClick,panel.Notify,"LIST ON MARKETPLACE  ·  Prototype feedback");
  }
  static void BuildFooter(Transform p)
  {
   Shape("Footer Rule",p,0,1012,1920,1,line,Color.clear);var comms=Button("Inventory Comms","COMMS",p,24,1030,120,32,null);UnityEventTools.AddStringPersistentListener(comms.onClick,panel.Notify,"COMMS  ·  Function in development");Text("Chat","Enter   START CHAT",p,170,1032,200,28,18,muted);Text("Preview Label","UNICORN / RX-0",p,1468,950,320,26,15,muted);Text("Preview Controls","Drag  ROTATE     Scroll  ZOOM",p,1510,1032,260,28,17,muted);Button("Inventory Return","Esc  RETURN",p,1790,1030,120,32,panel.Close);
  }
  static void BuildToast(Transform p)
  {
   var box=Shape("Inventory Toast",p,550,82,820,48,new Color(.02f,.1f,.14f,.96f),cyan);var group=box.gameObject.AddComponent<CanvasGroup>();group.alpha=0;group.blocksRaycasts=false;panel.toast=group;panel.toastLabel=Text("Message","",box.transform,14,0,792,48,20,white,TextAlignmentOptions.Center);
  }
  static InventoryItemData[] SampleItems()
  {
   string[] art={"SupplyCrate","BeamRifle","EnergyCell","PatternLeaf","SupplyCrate","SupplyCrate","SupplyCrate","ArmorPlate","PilotModule","EnergyCell","BeamRifle","ArmorPlate","EnergyCell","EnergyCell","EnergyCell","ReactorCore","PilotModule","ReactorCore","PilotModule","PilotModule","ArmorPlate","ReactorCore","BeamRifle","EnergyCell","Emblem","BeamRifle","Emblem","ReactorCore","ReactorCore","ReactorCore","ReactorCore","ReactorCore","ReactorCore","ReactorCore","SupplyCrate","EnergyCell","ArmorPlate","PilotModule","SupplyCrate","BeamRifle"};
   int[] qty={30,40,2,1,1,1,6,1,1,4,1,1,4,4,2,1,1,1,2,1,1,1,1,1,1,1,173,1,1,1,1,1,1,1,1,5,2,3,10,1};
   Color[] rare={Hex("9D47D5"),cyan,Hex("C9A846"),Hex("C9A846"),cyan,Hex("C9A846"),cyan,cyan,cyan,Hex("3E9E79")};
   var result=new InventoryItemData[art.Length];for(int i=0;i<result.Length;i++)
   {
    string category=art[i]=="BeamRifle"?"WEAPON":art[i]=="PatternLeaf"?"PATTERNS":art[i]=="EnergyCell"?"PAINT":art[i]=="ArmorPlate"?"VEHICLE":art[i]=="ReactorCore"||art[i]=="PilotModule"?"MODS":"DEVICES";
    string name=art[i]=="PatternLeaf"?"Pattern · Soft Leaf":art[i]=="SupplyCrate"?"Supply Crate · "+new[]{"Vanguard","Frontline","Orbital"}[i%3]:art[i]=="BeamRifle"?"Beam Rifle · RX Series":art[i]=="EnergyCell"?"Paint Cell · "+new[]{"Arctic Cyan","Orbital Blue","Signal Violet"}[i%3]:art[i]=="ReactorCore"?"Reactor Core · Mk "+(i%4+1):art[i]=="ArmorPlate"?"Armor Plate · Lunar":art[i]=="PilotModule"?"Pilot Module · Mindbridge":"Federation Merit Emblem";
    result[i]=new InventoryItemData{itemName=name,category=category,rarity=i%3==0?"Flawless":"Superior",icon=Art(art[i]),quantity=qty[i],price=1600+i*80,sellPrice=64+i*4,rarityColor=rare[i%10],description=art[i]=="PatternLeaf"?"Contains the corresponding pattern which can be used for\npaint jobs.":"An equipment supply for your mobile suit.\nSelect USE to preview this item."};
   }
   result[3].price=1600;result[3].sellPrice=64;return result;
  }
 }
}
