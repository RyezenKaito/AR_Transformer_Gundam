using UnityEngine;
using UnityEngine.EventSystems;

namespace GundamLobby
{
    public class StrikerPreviewDrag : MonoBehaviour, IDragHandler, IScrollHandler
    {
        public StrikerSelectionController owner;
        public void OnDrag(PointerEventData e) => owner.RotatePreview(-e.delta.x * .28f);
        public void OnScroll(PointerEventData e) => owner.ZoomPreview(-e.scrollDelta.y * 1.8f);
    }
}
