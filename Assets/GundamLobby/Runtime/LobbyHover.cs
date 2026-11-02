using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GundamLobby
{
    [RequireComponent(typeof(Button))]
    public class LobbyHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Graphic surface;
        public Color normal = new Color(.025f, .07f, .1f, .8f);
        public Color hover = new Color(.04f, .35f, .48f, .95f);
        public Color pressed = new Color(.06f, .55f, .7f, 1);
        public float duration = .18f;
        bool inside, down;
        void OnEnable() { if (surface) surface.color = normal; }
        void OnDisable() { inside = down = false; if (surface) surface.color = normal; }
        void Update()
        {
            if (surface) surface.color = Color.Lerp(surface.color, down ? pressed : inside ? hover : normal, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 5 / Mathf.Max(.01f, duration)));
        }
        public void OnPointerEnter(PointerEventData e) { inside = true; }
        public void OnPointerExit(PointerEventData e) { inside = down = false; }
        public void OnPointerDown(PointerEventData e) { down = true; }
        public void OnPointerUp(PointerEventData e) { down = false; }
    }
}
