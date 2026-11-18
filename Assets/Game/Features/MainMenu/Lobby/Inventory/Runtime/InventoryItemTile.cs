using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace GundamLobby
{
 public class InventoryItemTile : MonoBehaviour
 {
  public Image artwork;
  public TMP_Text quantity;
  public LobbyShape surface;
  public Graphic[] rarityMarks;
  public GameObject selectedCorners;
  public int dataIndex;
  InventoryPanel owner;
  void Awake(){GetComponent<Button>().onClick.AddListener(()=>{if(owner)owner.Select(dataIndex);});}
  public void Bind(InventoryPanel panel,int index,InventoryItemData item)
  {
   owner=panel;dataIndex=index;artwork.sprite=item.icon;artwork.preserveAspect=true;quantity.text=item.quantity>1?item.quantity.ToString():"";
   foreach(var mark in rarityMarks)mark.color=item.rarityColor;
   SetSelected(index==panel.selectedIndex);
  }
  public void SetSelected(bool value){surface.border=value?new Color(.125f,.784f,.965f):new Color(.13f,.275f,.33f);surface.thickness=value?2:1;surface.SetVerticesDirty();if(selectedCorners)selectedCorners.SetActive(value);}
 }
}
