using TMPro;
using UnityEngine;
namespace GundamLobby
{
 public sealed class MechaInventoryPresentation:MonoBehaviour
 {
  public InventoryPanel panel;public GameObject modelViewport;public ItemArtwork itemPreview;public TMP_Text stageTitle,stageSubtitle,effectLabel;
  void KeepGundam()
  {
   if(modelViewport)modelViewport.SetActive(true);if(panel.previewCamera)panel.previewCamera.enabled=panel.IsOpen;
   if(itemPreview)itemPreview.gameObject.SetActive(false);if(stageTitle)stageTitle.gameObject.SetActive(false);if(stageSubtitle)stageSubtitle.gameObject.SetActive(false);if(effectLabel)effectLabel.gameObject.SetActive(false);
  }
  public void Show(InventoryItemData item){KeepGundam();}
  public void Empty(){KeepGundam();}
 }
}