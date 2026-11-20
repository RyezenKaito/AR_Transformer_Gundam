using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
namespace GundamLobby
{
 public sealed class StrikerDesignController : MonoBehaviour
 {
  public HomeLobbyDashboard dashboard;public GameObject designCanvas;public RectTransform body;public TMP_FontAsset font;public GundamSkinBinding skinBinding;
  readonly string[] names={"FACTORY ORIGINAL","MIDNIGHT SENTINEL","CRIMSON COMET"};
  readonly Button[] cards=new Button[3];readonly TMP_Text[] states=new TMP_Text[3];readonly Image[] portraits=new Image[3];readonly GameObject[] lockBadges=new GameObject[3];
  TMP_Text title,wallet,detail,notice;Button action;int selected,version=-1;string shown;
  public bool IsOpen=>designCanvas&&designCanvas.activeSelf;
  void Awake(){Build();designCanvas.SetActive(false);}
  void Build()
  {
   ProgressionUi.Text("Section","CUSTOM DESIGN",body,font,42,178,430,32,23,ProgressionUi.Muted);
   title=ProgressionUi.Text("Active Gundam","",body,font,42,209,440,42,31,Color.white);
   for(int i=0;i<3;i++)
   {
    int index=i;float y=272+i*153;
    cards[i]=ProgressionUi.Button("Design "+i,"",body,font,40,y,455,137,()=>Select(index));
    var image=ProgressionUi.Rect("Gundam thumbnail",cards[i].transform,280,8,167,121).gameObject.AddComponent<Image>();image.preserveAspect=true;image.raycastTarget=false;portraits[i]=image;
    ProgressionUi.Text("Skin name",names[i],cards[i].transform,font,18,18,246,40,25,Color.white);
    states[i]=ProgressionUi.Text("Skin status","",cards[i].transform,font,18,77,250,35,20,ProgressionUi.Cyan);
    lockBadges[i]=ProgressionUi.Rect("Locked indicator",cards[i].transform,426,106,43,47).gameObject;ProgressionUi.Lock(lockBadges[i].transform,font,0,0);lockBadges[i].transform.localScale=Vector3.one*.42f;
   }
   wallet=ProgressionUi.Text("Premium balance","",body,font,42,755,440,36,24,ProgressionUi.Cyan);
   detail=ProgressionUi.Text("Selected skin","",body,font,42,801,455,52,24,Color.white);detail.textWrappingMode=TextWrappingModes.Normal;
   action=ProgressionUi.Button("Skin action","",body,font,40,866,455,64,PurchaseOrEquip,true);
   notice=ProgressionUi.Text("Design notice","",body,font,42,942,570,38,21,ProgressionUi.Muted);
   ProgressionUi.Text("Controls","DRAG TO ROTATE  /  SKIN PREVIEW",body,font,760,1015,650,36,20,ProgressionUi.Muted).alignment=TextAlignmentOptions.Center;
   ProgressionUi.Button("Return to Lobby","ESC  RETURN",body,font,1680,1010,190,42,Close);
  }
  public void Open()
  {
   if(IsOpen)return;dashboard.OpenLaboratory();dashboard.striker.lobbyCanvas.SetActive(false);designCanvas.SetActive(true);
   selected=PlayerProgress.EquippedSkin(dashboard.ActiveUnit.id);shown=null;Refresh();
  }
  public void Close(){if(!IsOpen)return;designCanvas.SetActive(false);skinBinding.previewSkin=-1;dashboard.striker.lobbyCanvas.SetActive(true);}
  public void Select(int index){selected=Mathf.Clamp(index,0,2);Refresh();}
  public void PurchaseOrEquip()
  {
   string id=dashboard.ActiveUnit.id;
   if(!PlayerProgress.OwnsSkin(id,selected)&&!PlayerProgress.BuySkin(id,selected,out var message)){notice.text=message;return;}
   PlayerProgress.EquipSkin(id,selected);notice.text="DESIGN EQUIPPED";Refresh();
  }
  void Refresh()
  {
   var unit=dashboard.ActiveUnit;if(unit==null)return;shown=unit.id;version=PlayerProgress.Version;
   title.text=unit.displayName+"  /  SKIN COLLECTION";wallet.text="CRYSTALS  "+PlayerProgress.Crystals.ToString("N0");
   for(int i=0;i<3;i++)
   {
    bool owned=PlayerProgress.OwnsSkin(unit.id,i);portraits[i].sprite=unit.CardPortrait;portraits[i].color=GundamSkinBinding.Tint(i);
    states[i].text=owned?(PlayerProgress.EquippedSkin(unit.id)==i?"SELECTED  /  EQUIPPED":"OWNED"):("LOCKED  /  "+PlayerProgress.Rules.SkinPrice(i)+" CRYSTALS");
    lockBadges[i].SetActive(!owned);states[i].color=owned?ProgressionUi.Cyan:ProgressionUi.Muted;
    var hover=cards[i].GetComponent<LobbyHover>();hover.normal=i==selected?new Color(.035f,.18f,.24f,.97f):new Color(.025f,.07f,.10f,.96f);hover.hover=i==selected?new Color(.045f,.23f,.30f,.98f):new Color(.035f,.12f,.17f,.97f);hover.pressed=new Color(.055f,.25f,.32f,.99f);
    var s=cards[i].GetComponent<LobbyShape>();s.color=hover.normal;s.border=i==selected?ProgressionUi.Cyan:new Color(.13f,.28f,.34f);s.thickness=i==selected?2:1;s.SetVerticesDirty();
   }
   bool available=PlayerProgress.OwnsSkin(unit.id,selected);
   detail.text=names[selected]+"\n"+(available?"COSMETIC DESIGN":"PREMIUM COSMETIC  /  CRYSTALS ONLY");
   action.GetComponentInChildren<TMP_Text>().text=available?"EQUIP DESIGN":("UNLOCK  /  "+PlayerProgress.Rules.SkinPrice(selected)+" CRYSTALS");
   action.interactable=available||PlayerProgress.Crystals>=PlayerProgress.Rules.SkinPrice(selected);skinBinding.previewSkin=selected;
  }
  void Update(){if(!IsOpen||LobbyServices.BlocksInput)return;if(version!=PlayerProgress.Version||shown!=dashboard.ActiveUnit.id)Refresh();if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)Close();}
 }
}
