using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [RequireComponent(typeof(Button))]
    public class BattleMapPin : MonoBehaviour
    {
        public BattleController owner;
        public int index;
        public TMP_Text label;
        public LobbyHover hover;
        void Awake(){GetComponent<Button>().onClick.AddListener(()=>owner.SelectLocation(index));}
        public void SetSelected(bool selected)
        {
            hover.normal=selected?new Color(.02f,.35f,.46f,.94f):new Color(.02f,.11f,.16f,.9f);hover.surface.color=hover.normal;
            label.color=selected?Color.white:new Color(.56f,.79f,.88f);
        }
        void LateUpdate()
        {
            if(!owner||!owner.IsOpen)return;
            Vector3 world=owner.mapPivot.TransformPoint(owner.catalog.locationPoints[index]+Vector3.up*.75f);
            var screen=owner.previewCamera.WorldToScreenPoint(world);
            var rt=(RectTransform)transform;
            if(screen.z>0&&RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent,screen,null,out var local))rt.localPosition=new Vector3(local.x,local.y,0);
        }
    }
}
