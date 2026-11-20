using UnityEngine.EventSystems;
using UnityEngine;

namespace GundamLobby
{
    public class BattleMapDrag : MonoBehaviour,IDragHandler
    {
        public BattleController owner;
        public void OnDrag(PointerEventData data){if(owner)owner.RotateMap(-data.delta.x*.12f);}
    }
}
