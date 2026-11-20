using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace GundamLobby
{
 public sealed class WeaponryModelMarker:MonoBehaviour{}
 public sealed class WeaponryLoadout:MonoBehaviour
 {
  public WeaponryController owner;
  public GameObject leftPanel,detailPanel,hotspotPanel;
  public Button[] partButtons=new Button[7],optionButtons=new Button[4],markers=new Button[7];
  public Image[] partIcons=new Image[7],optionIcons=new Image[4];
  public TMP_Text[] partStates=new TMP_Text[7],optionStates=new TMP_Text[4],statRows=new TMP_Text[6];
  public TMP_Text partTitle,currentLabel,itemTitle,itemSummary,itemDescription,availability;
  public Image selectedIcon;public Button equip,remove,shop,kit;public RectTransform selectionFrame;
  public EquipmentSlot SelectedPart{get;private set;}=EquipmentSlot.RightArm;
  public string Candidate{get;private set;}="";
  public bool LoadoutMode{get;private set;}=true;
  public GundamPartRig Rig{get;private set;}
  GameItem[] options=Array.Empty<GameItem>();int version=-1;GameObject shownModel;bool wired;
  string UnitId=>owner.dashboard.ActiveUnit.id;
  static readonly Color Cyan=ProgressionUi.Cyan,Muted=ProgressionUi.Muted,Green=new Color(.4f,.95f,.72f);
  [ContextMenu("Build Loadout UI")]
  public void EnsureUi()
  {
   if(!owner)owner=GetComponent<WeaponryController>();if(leftPanel)return;
   var font=owner.titleLabel.font;var root=owner.cards[0].transform.parent;var right=owner.activeTab.transform.parent;
   leftPanel=ProgressionUi.Rect("Body Part Slots",root,28,180,362,800).gameObject;
   for(int i=0;i<7;i++){
    int n=i;var b=ProgressionUi.Button("Part "+(EquipmentSlot)i,"",leftPanel.transform,font,0,i*96,362,87,()=>SelectPart((EquipmentSlot)n));partButtons[i]=b;
    partIcons[i]=Icon("Installed module",b.transform,10,10,72,62);
    ProgressionUi.Text("Part name",(i+1).ToString("00")+"  "+GundamEquipment.Name((EquipmentSlot)i),b.transform,font,92,9,258,25,21,Cyan);
    partStates[i]=ProgressionUi.Text("Installed state","STOCK",b.transform,font,92,39,254,35,20,Color.white);
   }
   kit=ProgressionUi.Button("Engineering Kit","CLAIM ENGINEERING KIT",leftPanel.transform,font,0,686,362,42,ClaimKit);
   var hint=ProgressionUi.Text("Mount instructions","SELECT A BODY PART\nClick the Gundam or use the list.\nDrag to rotate  /  Scroll to zoom",leftPanel.transform,font,6,741,350,64,18,Muted);hint.textWrappingMode=TextWrappingModes.Normal;
   detailPanel=ProgressionUi.Rect("Part Detail",right,0,66,424,856).gameObject;
   partTitle=ProgressionUi.Text("Part title","RIGHT ARM",detailPanel.transform,font,0,0,424,38,30,Cyan);
   currentLabel=ProgressionUi.Text("Installed module","INSTALLED / STOCK",detailPanel.transform,font,0,41,424,30,18,Muted);
   ProgressionUi.Shape("Divider",detailPanel.transform,0,80,424,1,Muted,Color.clear);
   selectedIcon=Icon("Equipment illustration",detailPanel.transform,0,92,116,96);
   itemTitle=ProgressionUi.Text("Candidate name","STOCK",detailPanel.transform,font,130,100,294,42,28,Color.white);
   itemSummary=ProgressionUi.Text("Effect","NO BONUS",detailPanel.transform,font,130,145,294,35,20,Cyan);
   itemDescription=ProgressionUi.Text("Description","",detailPanel.transform,font,0,201,424,85,20,Muted);itemDescription.textWrappingMode=TextWrappingModes.Normal;
   ProgressionUi.Text("Compatible heading","COMPATIBLE EQUIPMENT  /  STORAGE",detailPanel.transform,font,0,294,424,29,18,Cyan);
   for(int i=0;i<4;i++){
    int n=i;var b=ProgressionUi.Button("Compatible option "+i,"",detailPanel.transform,font,0,331+i*49,424,45,()=>ChooseOption(n));optionButtons[i]=b;
    optionIcons[i]=Icon("Artwork",b.transform,5,3,52,39);
    optionStates[i]=ProgressionUi.Text("Module", "",b.transform,font,68,0,344,45,19,Color.white);
   }
   availability=ProgressionUi.Text("Availability","",detailPanel.transform,font,0,531,424,30,17,Muted);
   ProgressionUi.Text("Preview heading","COMBAT STATS                 NOW  >  PREVIEW",detailPanel.transform,font,0,568,424,28,18,Cyan);
   for(int i=0;i<6;i++)statRows[i]=ProgressionUi.Text("Stat "+i,"",detailPanel.transform,font,0,601+i*24,424,24,20,Color.white);
   equip=ProgressionUi.Button("Equip Part","EQUIP PART",detailPanel.transform,font,0,754,424,44,Equip,true);
   remove=ProgressionUi.Button("Remove Part","RESTORE STOCK",detailPanel.transform,font,0,808,204,38,Remove);
   shop=ProgressionUi.Button("Get Equipment","GET MODULES",detailPanel.transform,font,216,808,208,38,OpenShop);
   hotspotPanel=ProgressionUi.Rect("Body Part Hotspots",root,0,0,1920,1080).gameObject;
   for(int i=0;i<7;i++){
    int n=i;markers[i]=ProgressionUi.Button("Hotspot "+(EquipmentSlot)i,(i+1).ToString("00"),hotspotPanel.transform,font,0,0,34,29,()=>SelectPart((EquipmentSlot)n));markers[i].GetComponentInChildren<TMP_Text>().fontSizeMax=16;
   }
   var frame=ProgressionUi.Shape("Selected part outline",hotspotPanel.transform,0,0,90,90,Color.clear,Cyan);frame.geometry=LobbyShape.Geometry.Box;frame.thickness=1;selectionFrame=frame.rectTransform;
   // Above the 3D hit region, below navigation and modal/toast layers.
   hotspotPanel.transform.SetSiblingIndex(owner.cards[0].transform.GetSiblingIndex());
   wired=true;
  }
  static Image Icon(string name,Transform parent,float x,float y,float w,float h){var img=ProgressionUi.Rect(name,parent,x,y,w,h).gameObject.AddComponent<Image>();img.preserveAspect=true;img.raycastTarget=false;return img;}
  void Wire()
  {
   if(wired)return;wired=true;
   for(int i=0;i<7;i++){int n=i;partButtons[i].onClick.RemoveAllListeners();partButtons[i].onClick.AddListener(()=>SelectPart((EquipmentSlot)n));markers[i].onClick.RemoveAllListeners();markers[i].onClick.AddListener(()=>SelectPart((EquipmentSlot)n));}
   for(int i=0;i<4;i++){int n=i;optionButtons[i].onClick.RemoveAllListeners();optionButtons[i].onClick.AddListener(()=>ChooseOption(n));}
   Bind(equip,Equip);Bind(remove,Remove);Bind(shop,OpenShop);Bind(kit,ClaimKit);
  }
  static void Bind(Button b,UnityEngine.Events.UnityAction action){b.onClick.RemoveAllListeners();b.onClick.AddListener(action);}
  public void ApplyLayout()
  {
   var change=owner.cards[0].transform.parent.Find("Weaponry Change Gundam") as RectTransform;if(change)change.anchoredPosition=new Vector2(1204,-168);
   foreach(var tab in new[]{owner.activeTab,owner.passiveTab}){var r=tab.rectTransform;r.anchoredPosition=new Vector2(r.anchoredPosition.x,-54);r.sizeDelta=new Vector2(r.sizeDelta.x,40);}
   ((RectTransform)detailPanel.transform).anchoredPosition=new Vector2(0,-104);
   SetY(itemDescription,183,76);
   detailPanel.transform.Find("Compatible heading").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-266);
   for(int i=0;i<optionButtons.Length;i++)((RectTransform)optionButtons[i].transform).anchoredPosition=new Vector2(0,-(301+i*47));
   SetY(availability,484,36);availability.textWrappingMode=TextWrappingModes.Normal;detailPanel.transform.Find("Preview heading").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-524);
   for(int i=0;i<6;i++)SetY(statRows[i],557+i*24,24);
   MoveY(equip,712);MoveY(remove,766);MoveY(shop,766);
  }
  static void MoveY(Button b,float y){var r=(RectTransform)b.transform;r.anchoredPosition=new Vector2(r.anchoredPosition.x,-y);}
  static void SetY(TMP_Text t,float y,float h){t.rectTransform.anchoredPosition=new Vector2(t.rectTransform.anchoredPosition.x,-y);t.rectTransform.sizeDelta=new Vector2(t.rectTransform.sizeDelta.x,h);}
  public void Opened()
  {
   EnsureUi();ApplyLayout();Wire();shownModel=owner.PreviewModel;shownModel.AddComponent<WeaponryModelMarker>();Rig=shownModel.GetComponent<GundamPartRig>();if(!Rig)Rig=shownModel.AddComponent<GundamPartRig>();Rig.Configure(UnitId,true);
   SetMode(true);SelectPart(SelectedPart);
  }
  public void SetMode(bool loadout)
  {
   EnsureUi();Wire();LoadoutMode=loadout;leftPanel.SetActive(loadout);detailPanel.SetActive(loadout);hotspotPanel.SetActive(loadout);
   foreach(var c in owner.cards)c.gameObject.SetActive(!loadout);
   foreach(Transform t in owner.activeTab.transform.parent){if(t==owner.activeTab.transform||t==owner.passiveTab.transform||t==detailPanel.transform)continue;t.gameObject.SetActive(!loadout);}
   foreach(var name in new[]{"Roster heading","Skill count","Data hint"}){var t=owner.cards[0].transform.parent.Find(name);if(t)t.gameObject.SetActive(!loadout);}
   owner.activeTab.GetComponentInChildren<TMP_Text>().text="SKILLS";owner.passiveTab.GetComponentInChildren<TMP_Text>().text="LOADOUT";
   Style(owner.activeTab,!loadout);Style(owner.passiveTab,loadout);
   if(loadout)Refresh();
  }
  static void Style(LobbyShape s,bool selected){s.color=selected?new Color(.035f,.26f,.34f):new Color(.025f,.07f,.10f,.92f);s.border=selected?Cyan:new Color(.12f,.24f,.29f);s.SetVerticesDirty();var h=s.GetComponent<LobbyHover>();if(h){h.normal=s.color;h.hover=selected?new Color(.05f,.34f,.44f):new Color(.05f,.17f,.23f);}}
  public void SelectPart(EquipmentSlot slot)
  {
   if(LobbyServices.BlocksInput)return;SelectedPart=slot;Candidate=PlayerProgress.PartModule(UnitId,slot);SetMode(true);
  }
  public void FocusItem(string id){SelectPart(GundamEquipment.Preferred(id));Candidate=id;Refresh();}
  void ChooseOption(int n){if(n<0||n>=options.Length)return;Candidate=options[n].id;Refresh();}
  public void SelectCandidate(string id){if(GundamEquipment.Fits(id,SelectedPart)){Candidate=id;Refresh();}}
  public void Refresh()
  {
   if(!owner||owner.dashboard.ActiveUnit==null)return;version=PlayerProgress.Version;
   string installed=PlayerProgress.PartModule(UnitId,SelectedPart);partTitle.text=GundamEquipment.Name(SelectedPart);
   currentLabel.text="INSTALLED  /  "+(GameItems.Find(installed)?.name??"STOCK CONFIGURATION");
   var selected=GameItems.Find(Candidate);itemTitle.text=selected?.name??"STOCK CONFIGURATION";itemSummary.text=selected!=null?GameItems.Summary(selected):"NO MODULE BONUS";
   selectedIcon.sprite=MechaItemVisuals.Icon(Candidate);selectedIcon.enabled=selectedIcon.sprite;
   itemDescription.text=selected!=null?"Reusable equipment. "+GameItems.Summary(selected)+". Fits "+GundamEquipment.Compatible(Candidate)+".\nRemoving it returns the copy to storage.":"Factory configuration. Select a compatible module from storage below. Each body part has an independent slot.";
   options=GameItems.All.Where(t=>GundamEquipment.Fits(t.id,SelectedPart)).ToArray();
   for(int i=0;i<optionButtons.Length;i++){
    optionButtons[i].gameObject.SetActive(i<options.Length);if(i>=options.Length)continue;var item=options[i];int free=Mathf.Max(0,PlayerProgress.Count(item.id)-PlayerProgress.MountedCopies(UnitId,item.id,SelectedPart));
    optionStates[i].text=item.name+"   "+(installed==item.id?"[LINKED]":free>0?"x"+free:"[SHOP]");optionStates[i].color=free>0?Color.white:Muted;
    optionIcons[i].sprite=MechaItemVisuals.Icon(item.id);Style(optionButtons[i].GetComponent<LobbyShape>(),Candidate==item.id);
   }
   bool allowed=PlayerProgress.CanEquipPart(UnitId,SelectedPart,Candidate,out var reason);
   availability.text=Candidate==installed?"CURRENT CONFIGURATION":allowed?"AVAILABLE  /  NO CURRENCY COST":reason;
   availability.color=allowed?Muted:new Color(1,.69f,.36f);equip.interactable=allowed&&Candidate!=installed;equip.GetComponentInChildren<TMP_Text>().text=Candidate==installed?"EQUIPPED":Candidate==""?"RESTORE STOCK":"EQUIP PART";remove.interactable=installed!="";
   var u=owner.dashboard.ActiveUnit;int[] raw={u.hp,u.attack,u.defense,u.speed,u.breakDamage,u.armor};string[] keys={"hp","attack","defense","speed","break","armor"};
   for(int i=0;i<6;i++){int now=PlayerProgress.Scaled(UnitId,raw[i],keys[i]),next=PlayerProgress.PreviewPartStat(UnitId,raw[i],keys[i],SelectedPart,Candidate);string col=next>now?"76E7B1":next<now?"F29D73":"AEC7D1";statRows[i].text=EnglishLobbyStats.StatNames[i]+"<pos=230>"+now+"<pos=304><color=#"+col+">>  "+next+"</color>";}
   for(int i=0;i<7;i++){
    var id=PlayerProgress.PartModule(UnitId,(EquipmentSlot)i);partStates[i].text=GameItems.Find(id)?.name??"STOCK CONFIGURATION";partIcons[i].sprite=MechaItemVisuals.Icon(id);partIcons[i].enabled=partIcons[i].sprite;Style(partButtons[i].GetComponent<LobbyShape>(),SelectedPart==(EquipmentSlot)i);
   }
   kit.interactable=!PlayerProgress.Claimed("parts-delivery");kit.GetComponentInChildren<TMP_Text>().text=kit.interactable?"CLAIM ENGINEERING KIT":"ENGINEERING KIT RECEIVED";
   owner.roleLabel.text=u.role+" / LV. "+PlayerProgress.UnitLevel(UnitId)+"   /   "+PlayerProgress.EquippedPartCount(UnitId)+" / 7 MOUNTS";
  }
  public void Equip(){if(LobbyServices.BlocksInput)return;PlayerProgress.EquipPart(UnitId,SelectedPart,Candidate,out var message);owner.Notify(message);Refresh();}
  public void Remove(){if(LobbyServices.BlocksInput)return;PlayerProgress.EquipPart(UnitId,SelectedPart,"",out var message);Candidate="";owner.Notify(message);Refresh();}
  void ClaimKit(){if(LobbyServices.BlocksInput)return;LobbyServices.For(owner.dashboard).Confirm("ENGINEERING DELIVERY","Receive 7 reusable module copies: Precision Array, Guard Matrix, Aegis Plating, 2 Vector Thrusters, Seraph Controller and Breach Coupler. One delivery per save; requires free storage slots.",()=>{PlayerProgress.Claim("parts-delivery",out var message);owner.Notify(message);Refresh();});}
  void OpenShop(){if(!LobbyServices.BlocksInput){owner.dashboard.Shop();owner.dashboard.shopPanel.ClearFilters();owner.dashboard.shopPanel.SetCategory(3);}}
  public void Pick(Vector2 screen){if(owner.IsOpen&&!LobbyServices.BlocksInput&&Rig&&Rig.Pick(owner.previewCamera,screen,out var slot))SelectPart(slot);}
  void LateUpdate()
  {
   if(!owner||!owner.IsOpen||!LoadoutMode||!Rig)return;if(version!=PlayerProgress.Version)Refresh();
   var parent=(RectTransform)hotspotPanel.transform;var camera=owner.previewCamera;
   for(int i=0;i<7;i++){
    var p=camera.WorldToScreenPoint(Rig.anchors[i].position);RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,p,null,out var local);var rt=markers[i].GetComponent<RectTransform>();rt.anchoredPosition=new Vector2(local.x,local.y);
    bool front=Vector3.Dot(owner.PreviewModel.transform.forward,camera.transform.position-Rig.anchors[i].position)<0;
    markers[i].gameObject.SetActive(p.z>0&&local.x>416&&local.x<1424&&(i!=6||!front));Style(markers[i].GetComponent<LobbyShape>(),SelectedPart==(EquipmentSlot)i);
   }
   var b=Rig.targets[(int)SelectedPart].bounds;Vector2 min=new Vector2(float.MaxValue,float.MaxValue),max=new Vector2(float.MinValue,float.MinValue);
   for(int i=0;i<8;i++){var p=camera.WorldToScreenPoint(new Vector3((i&1)==0?b.min.x:b.max.x,(i&2)==0?b.min.y:b.max.y,(i&4)==0?b.min.z:b.max.z));RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,p,null,out var v);min=Vector2.Min(min,v);max=Vector2.Max(max,v);}
   selectionFrame.anchoredPosition=new Vector2(min.x,max.y);selectionFrame.sizeDelta=max-min;
  }
 }
}
