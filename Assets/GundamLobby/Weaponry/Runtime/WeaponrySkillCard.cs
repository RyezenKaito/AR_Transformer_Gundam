using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [RequireComponent(typeof(Button))]
    public class WeaponrySkillCard : MonoBehaviour
    {
        public WeaponryController owner;
        public int index;
        public TMP_Text categoryLabel, titleLabel;
        public WeaponryGlyph glyph;
        public GameObject selectionBorder;
        public LobbyHover hover;
        void Awake() { GetComponent<Button>().onClick.AddListener(Select); }
        void Select() { if (owner) owner.SelectSkill(index); }
        public void Bind(WeaponrySkill skill, bool selected)
        {
            categoryLabel.text = skill.category; titleLabel.text = skill.title;
            glyph.kind = skill.glyph;
            glyph.color = selected ? new Color(.125f,.785f,.965f) : new Color(.32f,.49f,.57f);
            glyph.SetVerticesDirty(); selectionBorder.SetActive(selected);
            hover.normal = selected ? new Color(.035f,.23f,.31f,.97f) : new Color(.025f,.075f,.105f,.94f);
            hover.surface.color = hover.normal;
        }
    }
}
