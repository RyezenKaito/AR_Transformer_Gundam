using UnityEngine;
using UnityEngine.EventSystems;
namespace GundamLobby
{
 public class InventoryPreviewDrag:MonoBehaviour,IDragHandler,IScrollHandler
 {
  public Transform modelMount;
  public Camera previewCamera;
  public void OnDrag(PointerEventData e){if(modelMount)modelMount.Rotate(0,-e.delta.x*.35f,0,Space.Self);}
  public void OnScroll(PointerEventData e){if(previewCamera)previewCamera.orthographicSize=Mathf.Clamp(previewCamera.orthographicSize-e.scrollDelta.y*.22f,3.4f,6.7f);}
 }
}
