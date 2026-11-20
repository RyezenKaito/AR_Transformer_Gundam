using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace GundamLobby
{
 public class InventoryItemTile:MonoBehaviour
 {
  public Image artwork;public TMP_Text quantity;public LobbyShape surface;public Graphic[] rarityMarks;public GameObject selectedCorners;public int dataIndex;
  ItemArtwork art;TMP_Text caption,emptyLabel,slotLabel,equippedLabel;GameObject lockRoot;LobbyShape rarity;InventoryPanel owner;InventoryItemData shown;
  void Awake(){GetComponent<Button>().onClick.AddListener(()=>{if(owner)owner.OnTileClick(dataIndex);});}
  void Build()
  {
   if(art)return;
   foreach(Transform child in transform)if(child!=quantity.transform)child.gameObject.SetActive(false);
   art=ProgressionUi.Rect("Item render",transform,2,3,148,90).gameObject.AddComponent<ItemArtwork>();
   caption=ProgressionUi.Text("Item name","",transform,quantity.font,8,94,138,19,15,new Color(.79f,.87f,.92f));
   equippedLabel=ProgressionUi.Text("Equipped marker","",transform,quantity.font,8,5,94,18,12,ProgressionUi.Cyan);
   rarity=ProgressionUi.Shape("Rarity edge",transform,0,12,2,79,Color.cyan,Color.clear);rarity.geometry=LobbyShape.Geometry.Box;
   emptyLabel=ProgressionUi.Text("Empty marker","+",transform,quantity.font,0,30,152,42,26,new Color(.22f,.35f,.42f,.35f));emptyLabel.alignment=TextAlignmentOptions.Center;
   slotLabel=ProgressionUi.Text("Slot number","",transform,quantity.font,8,94,136,19,12,new Color(.26f,.38f,.45f,.6f));
   lockRoot=ProgressionUi.Rect("Slot lock",transform,62,36,43,47).gameObject;ProgressionUi.Lock(lockRoot.transform,quantity.font,0,0);lockRoot.transform.localScale=Vector3.one*.63f;var group=lockRoot.AddComponent<CanvasGroup>();group.alpha=.36f;
   quantity.rectTransform.anchorMin=quantity.rectTransform.anchorMax=quantity.rectTransform.pivot=new Vector2(0,1);quantity.rectTransform.anchoredPosition=new Vector2(112,-5);quantity.rectTransform.sizeDelta=new Vector2(33,26);quantity.fontSize=20;quantity.color=new Color(.72f,.84f,.9f);quantity.alignment=TextAlignmentOptions.Right;quantity.transform.SetAsLastSibling();
  }
  static string Short(string id,string fallback)=>id=="xp_chip"?"DATA CHIP":id=="assault_module"?"ASSAULT CIRCUIT":id=="bulwark_module"?"AEGIS PLATING":id=="thruster_module"?"VECTOR THRUSTER":id=="supply_crate"?"SUPPLY CRATE":fallback;
  public void Bind(InventoryPanel panel,int index,InventoryItemData item)
  {
   owner=panel;dataIndex=index;shown=item;Build();bool occupied=!item.locked&&!item.empty;
   art.gameObject.SetActive(occupied);if(occupied)art.Show(item.id);caption.text=occupied?Short(item.id,item.itemName):"";rarity.color=occupied?item.rarityColor:Color.clear;
   emptyLabel.gameObject.SetActive(item.empty&&!item.locked);slotLabel.text=item.locked?"LOCKED":item.empty?"SLOT "+(index+1).ToString("00"):"";lockRoot.SetActive(item.locked);quantity.text=occupied?item.quantity.ToString("N0"):"";
   equippedLabel.text=occupied&&PlayerProgress.IsArmed(item.id)?"ARMED":occupied&&PlayerProgress.IsEquipped(item.id)?"LINKED":"";SetSelected(index==panel.selectedIndex&&!item.locked);
  }
  public void SetSelected(bool selected)
  {
   if(shown==null)return;bool faint=shown.locked||shown.empty;
   surface.color=faint?new Color(.018f,.034f,.049f,.60f):new Color(.027f,.055f,.079f,.95f);surface.border=selected?ProgressionUi.Cyan:faint?new Color(.10f,.18f,.23f,.55f):new Color(.13f,.25f,.32f);surface.thickness=selected?1.7f:1;surface.SetVerticesDirty();
   if(selectedCorners)selectedCorners.SetActive(false);var hover=GetComponent<LobbyHover>();if(hover){hover.normal=surface.color;hover.hover=new Color(.04f,.12f,.17f,.97f);hover.pressed=new Color(.04f,.21f,.28f,.98f);}
  }
 }
}