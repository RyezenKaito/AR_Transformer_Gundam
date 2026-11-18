using UnityEngine;
using UnityEngine.EventSystems;

namespace GundamLobby
{
    internal sealed class CampaignCardMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public LobbyShape surface;
        bool hover, pressed;

        public void OnPointerEnter(PointerEventData data) => hover=true;
        public void OnPointerExit(PointerEventData data) { hover=false; pressed=false; }
        public void OnPointerDown(PointerEventData data) => pressed=true;
        public void OnPointerUp(PointerEventData data) => pressed=false;

        void Update()
        {
            float scale=pressed?.989f:hover?1.025f:1f;
            transform.localScale=Vector3.Lerp(transform.localScale,Vector3.one*scale,Time.unscaledDeltaTime*15);
            float thickness=hover||pressed?2.8f:1.5f;
            if(surface.thickness==thickness)return;
            surface.thickness=thickness;
            surface.SetVerticesDirty();
        }

        void OnDisable()
        {
            hover=pressed=false;
            transform.localScale=Vector3.one;
        }
    }
}
