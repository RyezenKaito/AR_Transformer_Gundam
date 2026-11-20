using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [DefaultExecutionOrder(200)]
    public class HomeLobbyDashboard : MonoBehaviour
    {
        public HomeLobbyController lobby;
        public StrikerDesignController design;
        public StrikerSelectionController striker;
        public InventoryPanel inventory;
        public WeaponryController weaponry;
        public BattleController battlePanel;
        public ShopController shopPanel;
        public TMP_Text playerLabel, unitLabel, roleLabel, serialLabel;
        public TMP_Text[] statLabels;
        public Image portrait;
        public CanvasScaler scaler;
        [Header("Commander profile and sample currency")]
        public Sprite playerAvatar;
        public int commanderLevel=20;
        [Range(0,1)] public float commanderProgress=.64f;
        public string[] resourceAmounts={"17,020","925"};
        [Header("Editable event artwork")]
        public Image eventArt;
        public Sprite eventSprite;
        public string eventTitle = "SIGNAL FROM ORBIT";
        public TMP_Text eventTitleLabel;
        StrikerDefinition displayed;
        int width, height, progressVersion=-1;

        public StrikerDefinition ActiveUnit
        {
            get
            {
                if (!striker || !striker.catalog || striker.catalog.units == null || striker.catalog.units.Length == 0) return null;
                foreach (var unit in striker.catalog.units) if (unit.prefab == lobby.gundamPrefab) return unit;
                return striker.catalog.units[Mathf.Clamp(Application.isPlaying ? striker.EquippedIndex : striker.defaultIndex, 0, striker.catalog.units.Length - 1)];
            }
        }
        void Start() { Refresh(); Fit(); }
        public void Refresh()
        {
            displayed = ActiveUnit;
            resourceAmounts=new[]{PlayerProgress.Credits.ToString("N0"),PlayerProgress.Crystals.ToString("N0")};progressVersion=PlayerProgress.Version;
            var playerData=lobby.GetComponent<LobbyScreenData>();
            if (playerLabel) playerLabel.text = PlayerProgress.CommanderName;
            if (eventTitleLabel) eventTitleLabel.text = eventTitle;
            if (eventArt) { eventArt.sprite = eventSprite; eventArt.enabled = eventSprite; }
            if (displayed == null) return;
            unitLabel.text = displayed.displayName;
            roleLabel.text = displayed.role + "  /  LV. " + PlayerProgress.UnitLevel(displayed.id);
            serialLabel.text = displayed.code;
            string[] values = System.Array.ConvertAll(EnglishLobbyStats.Numbers(displayed),n=>n.ToString("N0"));
            for (int i = 0; i < statLabels.Length && i < values.Length; i++) statLabels[i].text = values[i];
            if (portrait) { portrait.sprite = playerAvatar?playerAvatar:displayed.portrait; portrait.enabled = portrait.sprite; }
        }
        void Fit()
        {
            width = Screen.width; height = Screen.height;
            if (scaler) scaler.matchWidthOrHeight = width / (float)Mathf.Max(1, height) >= 16f / 9f ? 1 : 0;
        }
        void LateUpdate()
        {
            if (displayed != ActiveUnit || progressVersion!=PlayerProgress.Version) Refresh();
            if (width != Screen.width || height != Screen.height) Fit();
        }
        public void OpenLaboratory()
        {
            if(design&&design.IsOpen)design.Close();
            if(shopPanel&&shopPanel.IsOpen)shopPanel.Close();
            if(battlePanel&&battlePanel.IsOpen)battlePanel.Close();
            if(weaponry.IsOpen)weaponry.Close();
            if(inventory.IsOpen)inventory.Close();
            if(striker.IsOpen)striker.Close();
            striker.lobbyCanvas.SetActive(true);
            lobby.enabled=true; lobby.modelPivot.gameObject.SetActive(true);
            Refresh();
        }
        public void OpenWeaponry() { if(design&&design.IsOpen)design.Close(); if(shopPanel&&shopPanel.IsOpen)shopPanel.Close();if(battlePanel&&battlePanel.IsOpen)battlePanel.Close();weaponry.Open(); }
        public void OpenInventory() { if(design&&design.IsOpen)design.Close(); if(shopPanel&&shopPanel.IsOpen)shopPanel.Close();if(battlePanel&&battlePanel.IsOpen)battlePanel.Close();if(weaponry.IsOpen)weaponry.Close();if(striker.IsOpen)striker.Close();inventory.Open(); }
        public void OpenStrikers() { if(design&&design.IsOpen)design.Close(); if(shopPanel&&shopPanel.IsOpen)shopPanel.Close();if(battlePanel&&battlePanel.IsOpen){battlePanel.OpenStrikers();return;}striker.Open(); }
        public void OpenDesign(){if(design)design.Open();else lobby.ShowFeedback("STRIKE DESIGN");}
        public void Play() { Battle(); }
        public void Battle() { if(battlePanel)battlePanel.Open();else lobby.ShowFeedback("BATTLE"); }
        public void Shop() { if(shopPanel)shopPanel.Open();else lobby.ShowFeedback("SHOP"); }
        public void Profile() { LobbyServices.For(this).Open("COMMANDER PROFILE"); }
        public void Event() { Battle(); }
        public void Settings() { LobbyServices.For(this).Open("SETTINGS"); }
        public void Mail() { LobbyServices.For(this).Open("MAIL"); }
        public void Quests() { LobbyServices.For(this).Open("MISSIONS"); }
    }
}
