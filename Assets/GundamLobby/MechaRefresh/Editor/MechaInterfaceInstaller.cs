using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace GundamLobby.Editor
{
 public static class MechaInterfaceInstaller
 {
  static void Layout(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
  static TMP_Text Text(string name,string value,Transform p,TMP_FontAsset font,float x,float y,float w,float h,float size,Color c)=>ProgressionUi.Text(name,value,p,font,x,y,w,h,size,c);
  public static string Apply()
  {
   if(EditorApplication.isPlaying)throw new Exception("Stop Play first.");var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/GundamLobby/Scenes/HomeLobby.unity")throw new Exception("Open HomeLobby.");
   var d=UnityEngine.Object.FindFirstObjectByType<HomeLobbyDashboard>();
   foreach(var nav in UnityEngine.Object.FindObjectsByType<LobbyNavigationBar>(FindObjectsInactive.Include,FindObjectsSortMode.None))
   {
    if(nav.resourceButtons.Length==3){nav.resourceButtons[0].gameObject.SetActive(false);nav.resourceButtons=new[]{nav.resourceButtons[1],nav.resourceButtons[2]};nav.resourceValues=new[]{nav.resourceValues[1],nav.resourceValues[2]};}
    for(int i=0;i<2;i++){Layout((RectTransform)nav.resourceButtons[i].transform,1510+i*195,19,184,58);var shape=nav.resourceButtons[i].GetComponent<LobbyShape>();shape.cut=6;shape.color=new Color(.02f,.055f,.078f,.95f);}
    nav.resourceValues[0].text=PlayerProgress.Credits.ToString("N0");nav.resourceValues[1].text=PlayerProgress.Crystals.ToString("N0");EditorUtility.SetDirty(nav);
   }
   d.resourceAmounts=new[]{PlayerProgress.Credits.ToString("N0"),PlayerProgress.Crystals.ToString("N0")};
   var inv=d.inventory;var canvas=inv.inventoryCanvas;var screen=canvas.transform.Find("Inventory Screen");var font=inv.itemNameLabel.font;
   if(screen.Find("Preview Background"))screen.Find("Preview Background").gameObject.SetActive(false);
   if(!screen.Find("Inspection Backdrop")){var backdrop=ProgressionUi.Rect("Inspection Backdrop",screen,856,96,1064,916).gameObject.AddComponent<MechaBackdrop>();backdrop.showroom=true;backdrop.raycastTarget=false;backdrop.transform.SetAsFirstSibling();}
   var preview=screen.GetComponentsInChildren<RawImage>(true).First(t=>t.name.Contains("Gundam 3D Preview"));Layout(preview.rectTransform,1040,118,840,858);inv.previewCamera.aspect=840f/858f;
   var details=(RectTransform)inv.itemNameLabel.transform.parent;Layout(details,904,600,560,380);
   var panel=details.Find("Inventory Readability Panel").GetComponent<LobbyShape>();Layout(panel.rectTransform,-20,-5,560,365);panel.color=new Color(.006f,.016f,.025f,.80f);panel.border=Color.clear;panel.geometry=LobbyShape.Geometry.Box;panel.gradient=true;panel.gradientVertical=false;panel.gradientEnd=new Color(.006f,.016f,.025f,0);panel.cut=0;
   Layout(inv.rarityLabel.rectTransform,0,0,450,25);inv.rarityLabel.fontSizeMax=18;Layout(inv.itemNameLabel.rectTransform,0,29,520,45);inv.itemNameLabel.fontSizeMax=36;inv.itemNameLabel.color=ProgressionUi.Cyan;
   Layout(inv.descriptionLabel.rectTransform,0,89,470,80);inv.descriptionLabel.fontSizeMax=22;inv.descriptionLabel.fontSizeMin=18;
   Layout(inv.priceLabel.rectTransform,0,177,480,25);inv.priceLabel.fontSizeMax=19;Layout(inv.sellLabel.rectTransform,0,211,480,23);inv.sellLabel.fontSizeMax=17;
   foreach(var button in details.GetComponentsInChildren<Button>(true))if(button.name=="Use Selected Item")Layout((RectTransform)button.transform,0,252,440,44);else if(button.name=="List on Marketplace")Layout((RectTransform)button.transform,0,306,440,40);
   for(int i=0;i<inv.categoryButtons.Length;i++){bool active=i<4;inv.categoryButtons[i].gameObject.SetActive(active);if(active)Layout((RectTransform)inv.categoryButtons[i].transform,24+i*204,152,194,38);}
   Layout(inv.capacityLabel.rectTransform,650,111,180,28);inv.capacityLabel.fontSizeMax=23;inv.capacityLabel.alignment=TextAlignmentOptions.Right;
   if(!screen.Find("Storage section"))Text("Storage section","FIELD STORAGE",screen,font,24,110,550,30,21,ProgressionUi.Muted);
   var rule=screen.Find("Category Rule");if(rule)Layout((RectTransform)rule,24,194,806,1);
   Layout((RectTransform)inv.scroll.transform,24,212,820,704);Layout(inv.scroll.viewport,0,0,808,704);var scrollBar=inv.scroll.verticalScrollbar;if(scrollBar)Layout((RectTransform)scrollBar.transform,816,0,3,704);
   var grid=inv.content.GetComponent<GridLayoutGroup>();grid.cellSize=new Vector2(152,118);grid.spacing=new Vector2(12,12);
   var ui=inv.GetComponent<MechaInventoryPresentation>();if(!ui)ui=inv.gameObject.AddComponent<MechaInventoryPresentation>();inv.presentation=ui;ui.panel=inv;ui.modelViewport=preview.gameObject;
   if(!screen.Find("Inspection item"))ui.itemPreview=ProgressionUi.Rect("Inspection item",screen,1020,210,790,480).gameObject.AddComponent<ItemArtwork>();else ui.itemPreview=screen.Find("Inspection item").GetComponent<ItemArtwork>();
   ui.itemPreview.gameObject.SetActive(false);
   ui.stageTitle=screen.Find("Inspection title")?screen.Find("Inspection title").GetComponent<TMP_Text>():Text("Inspection title","ITEM INSPECTION",screen,font,904,131,720,31,20,ProgressionUi.Muted);
   ui.stageSubtitle=screen.Find("Inspection subtitle")?screen.Find("Inspection subtitle").GetComponent<TMP_Text>():Text("Inspection subtitle","",screen,font,904,174,720,33,24,new Color(.83f,.91f,.95f));
   ui.effectLabel=screen.Find("Inspection effect")?screen.Find("Inspection effect").GetComponent<TMP_Text>():Text("Inspection effect","",screen,font,910,238,290,84,32,ProgressionUi.Cyan);ui.effectLabel.textWrappingMode=TextWrappingModes.Normal;ui.stageTitle.gameObject.SetActive(false);ui.stageSubtitle.gameObject.SetActive(false);ui.effectLabel.gameObject.SetActive(false);
   foreach(var t in screen.GetComponentsInChildren<TMP_Text>(true))if(t.name=="Preview Label")t.gameObject.SetActive(false);
   var shop=d.shopPanel;var catalog=shop.catalog;var entries=catalog.items.Where(t=>t.code!=null&&t.code.StartsWith("gundam:")).ToList();
   catalog.categories=new[]{"GUNDAM","UPGRADE MATERIALS","SUPPLIES","MODULES","STRIKE DESIGNS"};
   foreach(var item in entries){item.category=0;item.auction=false;}
   foreach(var item in GameItems.All.Where(t=>!GameItems.IsRewardOnly(t)))entries.Add(new ShopItemDefinition{title=item.name,code=item.id,description=item.description,price=item.price,unitPrice=item.price,quantity=1,rarity=item.tier,category=GameItems.IsEquipment(item)?3:item.effect==ItemEffect.Upgrade?1:2,icon=MechaItemVisuals.Icon(item.id),auction=false});
   string[] names={"MIDNIGHT SENTINEL","CRIMSON COMET"};for(int i=1;i<=2;i++)entries.Add(new ShopItemDefinition{title=names[i-1],code="skin:"+i,category=4,rarity=2,price=PlayerProgress.Rules.SkinPrice(i),quantity=1,auction=false,description="Permanent cosmetic design for the currently selected Gundam. Purchased with Crystals only. Changes its appearance without adding combat stats. Owned designs can be equipped again at no cost."});
   catalog.items=entries.ToArray();EditorUtility.SetDirty(catalog);
   var shopRoot=shop.shopCanvas.GetComponentsInChildren<RectTransform>(true).First(t=>t.name=="Shop Screen");
   if(!shopRoot.Find("Exchange Backdrop")){var bg=ProgressionUi.Rect("Exchange Backdrop",shopRoot,0,96,1920,916).gameObject.AddComponent<MechaBackdrop>();bg.raycastTarget=false;bg.transform.SetAsFirstSibling();}
   foreach(var t in shop.shopCanvas.GetComponentsInChildren<TMP_Text>(true))if(t.text=="ITEM MARKETPLACE")t.text="BASE EXCHANGE";
   var well=shop.detailIcon.transform.parent.Find("Detail artwork well");if(well)Layout((RectTransform)well,24,72,266,346);
   Layout(shop.detailIcon.rectTransform,30,88,250,242);Layout(shop.detailCategory.rectTransform,318,65,610,28);Layout(shop.detailTitle.rectTransform,318,102,612,49);Layout(shop.detailDescription.rectTransform,318,166,612,116);Layout(shop.detailMeta.rectTransform,318,289,612,54);Layout(shop.detailPrice.rectTransform,358,349,572,48);
   var coin=shop.detailIcon.transform.parent.Find("Credits icon");if(coin)Layout((RectTransform)coin,318,359,28,28);
   Layout((RectTransform)shop.detailActionLabel.transform.parent,318,422,370,64);foreach(var b in shop.detailPanel.GetComponentsInChildren<Button>(true))if(b.name=="Return to Shop")Layout((RectTransform)b.transform,708,422,222,64);
   foreach(var mode in d.battlePanel.catalog.modes){mode.energyCost=0;mode.description=mode.title=="CAMPAIGN"?"Choose a chapter, inspect mission rewards and deploy freely. Victories grant Credits and F-Cores.":mode.title=="AI SKIRMISH"?"Offline battles against AI squads. Earn Credits and F-Cores without an entry cost.":mode.title=="CHALLENGE"?"Elite enemies with 25% more HP and ATK. Receive 50% more rewards. No entry resource required.":mode.title=="TRAINING"?"Free practice with your selected Gundam. Test skills and equipment without rewards or campaign progress.":"Local test room using the available battlefields. Standard or Advanced enemy strength. No rewards.";}EditorUtility.SetDirty(d.battlePanel.catalog);
   foreach(var button in details.GetComponentsInChildren<Button>(true).Concat(inv.categoryButtons.Take(4)).Concat(shop.detailPanel.GetComponentsInChildren<Button>(true).Where(b=>b.name=="Trade Selected Item"||b.name=="Return to Shop")))
   {
    var label=button.GetComponentInChildren<TMP_Text>(true);if(!label)continue;var r=label.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,.5f);r.offsetMin=new Vector2(10,0);r.offsetMax=new Vector2(-10,0);label.alignment=TextAlignmentOptions.Center;label.enableAutoSizing=true;label.fontSizeMin=15;label.fontSizeMax=button.transform.parent==details?23:label.fontSizeMax;
   }
   foreach(var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))foreach(var component in c.GetComponentsInChildren<Component>(true))if(component&&PrefabUtility.IsPartOfPrefabInstance(component))PrefabUtility.RecordPrefabInstancePropertyModifications(component);
   EditorUtility.SetDirty(inv);EditorUtility.SetDirty(ui);EditorUtility.SetDirty(d);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Applied two-currency headers, contextual Inventory inspection and expanded Mecha storefront.";
  }
 }
}