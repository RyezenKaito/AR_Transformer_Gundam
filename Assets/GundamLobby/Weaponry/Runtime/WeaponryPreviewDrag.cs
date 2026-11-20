using UnityEngine;
using UnityEngine.EventSystems;

namespace GundamLobby
{
    public class WeaponryPreviewDrag : MonoBehaviour, IDragHandler, IScrollHandler, IPointerDownHandler, IPointerUpHandler
    {
        public WeaponryController owner;
        Vector2 press;bool dragged;
        public void OnPointerDown(PointerEventData data){press=data.position;dragged=false;}
        public void OnPointerUp(PointerEventData data){if(owner&&!dragged&&(data.position-press).sqrMagnitude<=36)owner.Loadout?.Pick(data.position);}
        public void OnDrag(PointerEventData data) { if((data.position-press).sqrMagnitude>36)dragged=true;if (owner) owner.RotatePreview(-data.delta.x * .28f); }
        public void OnScroll(PointerEventData data) { if (owner) owner.ZoomPreview(-data.scrollDelta.y * 1.8f); }
    }
}
