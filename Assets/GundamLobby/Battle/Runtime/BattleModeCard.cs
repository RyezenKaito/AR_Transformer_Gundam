using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [RequireComponent(typeof(Button))]
    public class BattleModeCard : MonoBehaviour
    {
        public BattleController owner;
        public int index;
        public TMP_Text titleLabel,subtitleLabel,numberLabel;
        public BattleModeIcon icon;
        public GameObject selectionBorder;
        public LobbyHover hover;
        void Awake(){GetComponent<Button>().onClick.AddListener(()=>owner.SelectMode(index));}
        public void Bind(BattleModeDefinition mode,bool selected)
        {
            titleLabel.text=mode.title;subtitleLabel.text=mode.subtitle;numberLabel.text=(index+1).ToString("00");icon.kind=mode.icon;
            icon.color=selected?new Color(.125f,.785f,.965f):new Color(.27f,.47f,.56f);icon.SetVerticesDirty();
            selectionBorder.SetActive(selected);hover.normal=selected?new Color(.03f,.24f,.33f,.98f):new Color(.025f,.075f,.105f,.94f);hover.surface.color=hover.normal;
        }
    }
}
