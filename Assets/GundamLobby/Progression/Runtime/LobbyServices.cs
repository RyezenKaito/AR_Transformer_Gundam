using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace GundamLobby
{
 [DefaultExecutionOrder(500)] public sealed class LobbyServices:MonoBehaviour
 {
  public HomeLobbyDashboard dashboard;
  static LobbyServices instance; static int closedFrame=-1;
  public static bool BlocksInput=>instance&&instance.root&&instance.root.activeSelf||closedFrame==Time.frameCount;
  GameObject root;RectTransform box;TMP_FontAsset font;TMP_Text title,notice;string page;int version=-1;TMP_InputField nameInput;
  public static LobbyServices For(HomeLobbyDashboard d){if(!instance){instance=d.gameObject.GetComponent<LobbyServices>();if(!instance)instance=d.gameObject.AddComponent<LobbyServices>();instance.dashboard=d;}return instance;}
  void Awake(){instance=this;AudioListener.volume=PlayerPrefs.GetFloat("Gundam.AudioVolume",.8f);if(!Application.isEditor&&PlayerPrefs.HasKey("Gundam.Fullscreen"))Screen.fullScreen=PlayerPrefs.GetInt("Gundam.Fullscreen")==1;}
  void Start()
  {
   if(!dashboard)dashboard=FindFirstObjectByType<HomeLobbyDashboard>();
   foreach(var b in dashboard.inventory.inventoryCanvas.GetComponentsInChildren<Button>(true))
   {
    if(b.name=="BULK SELL")Bind(b,dashboard.inventory.BeginBulkSell);
    else if(b.name=="BULK DISCARD")Bind(b,dashboard.inventory.BeginBulkDiscard);
    else if(b.name=="List on Marketplace"){Bind(b,dashboard.inventory.SellSelected);b.GetComponentInChildren<TMP_Text>().text="SELL SELECTED";}
    else if(b.name=="Inventory Comms")Bind(b,()=>Open("ACTIVITY"));
   }
   var selection=dashboard.striker.selectionCanvas;
   foreach(var b in selection.GetComponentsInChildren<Button>(true))if(b.name.Contains("Comms"))Bind(b,()=>Open("ACTIVITY"));
  }
  static void Bind(Button b,UnityEngine.Events.UnityAction action){b.onClick=new Button.ButtonClickedEvent();b.onClick.AddListener(action);}
  public void Open(string section)
  {
   page=section;BuildRoot();root.SetActive(true);Render();
  }
  void BuildRoot()
  {
   if(root)return;font=dashboard.unitLabel.font;
   root=new GameObject("Lobby Feature Dialog",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));root.transform.SetParent(transform,false);
   var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=300;
   var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
   var shade=ProgressionUi.Shape("Modal Scrim",root.transform,0,0,1920,1080,new Color(.004f,.014f,.025f,.92f),Color.clear);shade.raycastTarget=true;var sr=shade.rectTransform;sr.anchorMin=Vector2.zero;sr.anchorMax=Vector2.one;sr.offsetMin=sr.offsetMax=Vector2.zero;
   box=ProgressionUi.Rect("Feature Panel",root.transform,460,172,1000,750);box.anchorMin=box.anchorMax=new Vector2(.5f,.5f);box.pivot=new Vector2(.5f,.5f);box.anchoredPosition=Vector2.zero;
  }
  void Clear(){foreach(Transform c in box){c.gameObject.SetActive(false);Destroy(c.gameObject);}}
  TMP_Text Text(string label,float x,float y,float w,float h,int size=25,Color? color=null){var t=ProgressionUi.Text("Text",label,box,font,x,y,w,h,size,color??Color.white);t.textWrappingMode=TextWrappingModes.Normal;return t;}
  Button Button(string label,float x,float y,float w,Action action,bool primary=false)=>ProgressionUi.Button(label,label,box,font,x,y,w,48,()=>action(),primary);
  public void Close(){if(root)root.SetActive(false);closedFrame=Time.frameCount;}
  public void Confirm(string heading,string description,Action action)
  {
   page="CONFIRM";BuildRoot();root.SetActive(true);Base(heading);Text(description,40,146,920,290,27,ProgressionUi.Muted);
   Button("CONFIRM",40,594,440,()=>{Close();action();},true);Button("CANCEL",520,594,440,Close);
  }
  void Base(string heading)
  {
   Clear();ProgressionUi.Shape("Panel",box,0,0,1000,750,new Color(.025f,.06f,.095f,.99f),new Color(.11f,.35f,.44f));
   ProgressionUi.Shape("Accent",box,0,0,6,70,ProgressionUi.Cyan,Color.clear);
   Text("COMMAND SYSTEM  /  LOCAL SAVE",40,22,830,27,18,ProgressionUi.Muted);title=Text(heading,40,58,860,55,38);
   ProgressionUi.Shape("Rule",box,40,124,920,1,new Color(.12f,.30f,.38f),Color.clear);
   notice=Text("",40,682,780,40,21,ProgressionUi.Cyan);Button("CLOSE",830,687,130,Close);
  }
  void Render()
  {
   version=PlayerProgress.Version;Base(page);
   switch(page)
   {
    case "UPDATE":Upgrade();break;
    case "COMMANDER PROFILE":Profile();break;
    case "SETTINGS":Settings();break;
    case "MAIL":Mail();break;
    case "MISSIONS":Missions();break;
    case "ITEM GUIDE":Guide();break;
    case "CREDITS":case "CRYSTALS":Resources();break;
    default:Activity();break;
   }
  }
  void Perform(Func<string> action){string message=action();Render();notice.text=message;}
  void Upgrade()
  {
   var u=dashboard.ActiveUnit;if(u==null)return;
   Text(u.displayName+"   /   LV. "+PlayerProgress.UnitLevel(u.id)+" / 40",40,150,900,42,31);
   Text("Permanent calibration. Stats apply to the lobby and every battle.\nEach level adds 5% of base HP, ATK, DEF, armor and break damage.\nSpeed increases by 1 every two levels.",40,210,900,105,25,ProgressionUi.Muted);
   var stats=EnglishLobbyStats.Numbers(u);string[] names={"HP","ATK","DEF","SPD","BREAK","ARMOR"};
   for(int i=0;i<stats.Length;i++)Text(names[i]+"  "+stats[i].ToString("N0"),40+(i%3)*304,338+(i/3)*53,290,42,28,i==1?ProgressionUi.Cyan:Color.white);
   int credits=PlayerProgress.UpgradeCredits(u.id),cores=PlayerProgress.UpgradeCores(u.id);
   Text("COST  "+credits+" CREDITS  +  "+cores+" F-CORE  +  1 DATA CHIP\nOWNED  "+PlayerProgress.Credits.ToString("N0")+" CREDITS  /  "+PlayerProgress.FCores+" CORES  /  "+PlayerProgress.Count("xp_chip")+" CHIPS",40,468,920,82,24,ProgressionUi.Muted);
   var buy=Button(PlayerProgress.Rank(u.id)>=20?"MAX LEVEL 40":"UPGRADE TO LV. "+(PlayerProgress.UnitLevel(u.id)+1),40,584,570,()=>Perform(()=>{PlayerProgress.UpgradeUnit(u.id,out var msg);return msg;}),true);buy.interactable=PlayerProgress.Rank(u.id)<20&&PlayerProgress.Credits>=credits&&PlayerProgress.FCores>=cores&&PlayerProgress.Count("xp_chip")>0;
   Button("GET MATERIALS",638,584,322,()=>{Close();dashboard.Shop();dashboard.shopPanel.SetCategory(1);});
  }
  void Profile()
  {
   var u=dashboard.ActiveUnit;Text("COMMANDER  /  "+PlayerProgress.CommanderName,40,155,900,46,31);
   Text("Active Gundam: "+u.displayName+"  /  Lv. "+PlayerProgress.UnitLevel(u.id)+"\nOwned Gundam: "+(PlayerProgress.OwnsGundam("StrikeRouge")?2:1)+" / 2\nVictories: "+PlayerProgress.MissionProgress(0)+"   Upgrades: "+PlayerProgress.MissionProgress(3)+"\nStorage: "+PlayerProgress.UsedSlots+" / "+PlayerProgress.InventorySlots+" stacks",40,220,900,180,27,ProgressionUi.Muted);
   Text("COMMANDER CALLSIGN",40,425,850,34,21,ProgressionUi.Cyan);
   var field=ProgressionUi.Shape("Callsign",box,40,469,600,60,new Color(.015f,.035f,.055f),ProgressionUi.Cyan);var text=ProgressionUi.Text("Value",PlayerProgress.CommanderName,field.transform,font,15,0,565,60,29,Color.white);text.raycastTarget=true;
   nameInput=field.gameObject.AddComponent<TMP_InputField>();nameInput.textViewport=field.rectTransform;nameInput.textComponent=(TextMeshProUGUI)text;nameInput.targetGraphic=field;nameInput.characterLimit=20;nameInput.text=PlayerProgress.CommanderName;
   Button("SAVE CALLSIGN",665,475,295,()=>Perform(()=>{PlayerProgress.Rename(nameInput.text);return "Callsign saved.";}),true);
  }
  void Settings()
  {
   Text("AUDIO VOLUME   "+Mathf.RoundToInt(AudioListener.volume*100)+"%",40,167,900,40,30);
   Button("- 10%",40,228,200,()=>SetVolume(-.1f));Button("+ 10%",260,228,200,()=>SetVolume(.1f));
   Text("DISPLAY   "+(Screen.fullScreen?"FULLSCREEN":"WINDOWED"),40,326,900,40,30);
   Button("TOGGLE FULLSCREEN",40,387,420,()=>{Screen.fullScreen=!Screen.fullScreen;PlayerPrefs.SetInt("Gundam.Fullscreen",Screen.fullScreen?1:0);PlayerPrefs.Save();});
   Text("Progress is saved automatically after every purchase, use, upgrade and reward.\nYour existing Gundam, skin, currencies and inventory expansion are preserved.",40,500,900,100,25,ProgressionUi.Muted);
  }
  void SetVolume(float delta){AudioListener.volume=Mathf.Clamp01(AudioListener.volume+delta);PlayerPrefs.SetFloat("Gundam.AudioVolume",AudioListener.volume);PlayerPrefs.Save();Render();}
  void Mail()
  {
   Text("WELCOME TO SECTOR 07",40,151,630,38,28);Text("500 Credits + 1 Maintenance Crate\nOne-time command starter package.",40,198,620,73,24,ProgressionUi.Muted);
   var welcome=Button(PlayerProgress.Claimed("welcome")?"CLAIMED":"CLAIM",715,183,245,()=>Perform(()=>{PlayerProgress.Claim("welcome",out var msg);return msg;}),true);welcome.interactable=!PlayerProgress.Claimed("welcome");
   ProgressionUi.Shape("Mail Divider",box,40,299,920,1,new Color(.12f,.27f,.34f),Color.clear);
   Text("ARMORY FIELD TRIAL DELIVERY",40,326,890,40,28,ProgressionUi.Cyan);
   Text("1 F-Core Container + 1 Tactical Data Archive\n2 Nano Repair Gels + 2 Overdrive Ampoules\n\nClaim once, then open containers and prepare supplies from Inventory. A preparation is consumed only on real sortie entry.",40,383,900,171,24,ProgressionUi.Muted);
   var trial=Button(PlayerProgress.Claimed("armory-delivery")?"DELIVERY CLAIMED":"CLAIM FIELD TRIAL KIT",40,592,920,()=>Perform(()=>{PlayerProgress.Claim("armory-delivery",out var msg);return msg;}),true);trial.interactable=!PlayerProgress.Claimed("armory-delivery");
  }
  void Missions()
  {
   string[] names={"WIN A SORTIE","PURCHASE AN ITEM","USE OR EQUIP AN ITEM","UPGRADE A GUNDAM"};
   for(int i=0;i<4;i++)
   {
    int m=i;float y=155+i*117;bool done=PlayerProgress.MissionProgress(i)>0,claimed=PlayerProgress.Claimed("mission-"+i);
    Text(names[i]+"   "+(done?"1 / 1":"0 / 1"),40,y,640,36,27);Text("REWARD  "+(500+i*100)+" CREDITS + 1 "+(i==3?"MAINTENANCE CRATE":"DATA CHIP"),40,y+43,650,31,20,ProgressionUi.Muted);
    var b=Button(claimed?"CLAIMED":"CLAIM",742,y+12,218,()=>Perform(()=>{PlayerProgress.Claim("mission-"+m,out var msg);return msg;}),true);b.interactable=done&&!claimed;
   }
  }
  void Guide()
  {
   var area=ProgressionUi.Rect("Item Guide Scroll",box,30,142,940,510);var scroll=area.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.scrollSensitivity=32;scroll.movementType=ScrollRect.MovementType.Clamped;
   var view=ProgressionUi.Rect("Viewport",area,0,0,940,510);view.gameObject.AddComponent<RectMask2D>();var hit=view.gameObject.AddComponent<Image>();hit.color=Color.clear;
   var content=ProgressionUi.Rect("Items",view,0,0,920,GameItems.All.Length*70);scroll.viewport=view;scroll.content=content;
   for(int i=0;i<GameItems.All.Length;i++)
   {
    var item=GameItems.All[i];float y=i*70;var art=ProgressionUi.Rect("Item icon",content,0,y+2,82,62).gameObject.AddComponent<ItemArtwork>();art.Show(item.id);
    ProgressionUi.Text("Name",item.name,content,font,100,y+8,327,47,23,item.color).textWrappingMode=TextWrappingModes.Normal;
    ProgressionUi.Text("Purpose",GameItems.Summary(item),content,font,445,y+8,470,47,23,ProgressionUi.Muted).textWrappingMode=TextWrappingModes.Normal;
    ProgressionUi.Shape("Rule",content,100,y+68,810,1,new Color(.1f,.23f,.28f,.6f),Color.clear);
   }
  }
  void Resources()
  {
   string description=page=="CREDITS"?"BALANCE  "+PlayerProgress.Credits.ToString("N0")+" CREDITS\nEarn Credits from victories, missions and Credit Caches.\nSpend on Gundam, modules, supplies, upgrades and storage.":"BALANCE  "+PlayerProgress.Crystals.ToString("N0")+" CRYSTALS\nCosmetic currency for Strike Design skins.\nMidnight Sentinel: 300 / Crimson Comet: 450.\nSkins change appearance only. No real-money payment is connected.";
   Text(description,40,175,900,260,29,ProgressionUi.Muted);
   Button(page=="CRYSTALS"?"OPEN STRIKE DESIGN":"OPEN INVENTORY",40,515,600,()=>{Close();if(page=="CRYSTALS")dashboard.OpenDesign();else dashboard.OpenInventory();},true);
  }
  void Activity()
  {
   var log=PlayerProgress.History;Text(log.Length==0?"No transactions yet. Purchases, item use and rewards appear here.":string.Join("\n\n",log.Take(7)),40,160,920,490,24,ProgressionUi.Muted);
  }
  void Update()
  {
   if(!root||!root.activeSelf)return;
   if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){Close();return;}
   if(page!="CONFIRM"&&page!="COMMANDER PROFILE"&&version!=PlayerProgress.Version)Render();
  }
 }
}