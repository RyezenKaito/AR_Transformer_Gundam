using UnityEngine;
using UnityEngine.EventSystems;

namespace UnicornDemo
{
    public sealed class DemoJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform handle;
        public Vector2 Value { get; private set; }
        int pointer = int.MinValue;
        public void OnPointerDown(PointerEventData data)
        {
            if (pointer != int.MinValue) return;
            pointer = data.pointerId;
            OnDrag(data);
        }
        public void OnDrag(PointerEventData data)
        {
            if (pointer != data.pointerId) return;
            var rect = (RectTransform)transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, data.position, data.pressEventCamera, out var local);
            local -= rect.rect.center;
            float radius = rect.rect.width * .34f;
            Value = Vector2.ClampMagnitude(local / radius, 1f);
            handle.anchoredPosition = Value * radius;
        }
        public void OnPointerUp(PointerEventData data)
        {
            if (data.pointerId == pointer) ResetInput();
        }
        void OnDisable() { ResetInput(); }
        public void ResetInput()
        {
            Value = Vector2.zero;
            pointer = int.MinValue;
            if (handle) handle.anchoredPosition = Vector2.zero;
        }
    }
}
