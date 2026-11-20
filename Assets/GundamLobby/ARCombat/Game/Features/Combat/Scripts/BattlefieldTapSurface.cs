using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GundamGame.CombatDemo
{
    internal sealed class BattlefieldTapSurface : MonoBehaviour,IPointerClickHandler,IBeginDragHandler,IDragHandler
    {
        public Action<Vector2> Tapped { private get; set; }
        public void OnPointerClick(PointerEventData data)
        {
            if(data.button==PointerEventData.InputButton.Left && !data.dragging)
                Tapped?.Invoke(data.position);
        }
        public void OnBeginDrag(PointerEventData data) => data.eligibleForClick=false;
        public void OnDrag(PointerEventData data) { }
    }
}
