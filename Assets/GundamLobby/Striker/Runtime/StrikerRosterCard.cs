using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [RequireComponent(typeof(Button))]
    public class StrikerRosterCard : MonoBehaviour
    {
        public StrikerSelectionController owner;
        public int index;
        public TMP_Text titleLabel, roleLabel, activeLabel;
        public Image portrait;
        public GameObject selectedBorder, selectedCheck;
        void Awake() => GetComponent<Button>().onClick.AddListener(() => owner.Select(index));
        public void Bind(StrikerDefinition unit, bool selected, bool equipped)
        {
            titleLabel.text = unit.displayName;
            roleLabel.text = unit.role;
            portrait.sprite = unit.portrait;
            portrait.enabled = unit.portrait;
            selectedBorder.SetActive(selected);
            selectedCheck.SetActive(selected);
            activeLabel.gameObject.SetActive(equipped);
        }
    }
}
