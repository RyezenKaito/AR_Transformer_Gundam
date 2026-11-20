using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    public class LobbyNavigationBar : MonoBehaviour
    {
        public enum Page { Home, Laboratory, Inventory, Weaponry, Battle, Shop }
        public Page page;
        public HomeLobbyDashboard dashboard;
        public Button profile, laboratory, battle, inventory, shop;
        public TMP_Text playerLabel, unitCaption;
        public TMP_Text commanderLabel, levelBadge;
        public TMP_Text[] resourceValues;
        public Button[] resourceButtons;
        public RectTransform experienceFill;
        public Image portrait;
        GameObject lastModel;
        int progressVersion=-1;

        void Awake()
        {
            profile.onClick.AddListener(()=>Notify("COMMANDER PROFILE"));
            laboratory.onClick.AddListener(OpenLaboratory);
            battle.onClick.AddListener(OpenBattle);
            inventory.onClick.AddListener(OpenInventory);
            shop.onClick.AddListener(OpenShop);
            if(resourceButtons!=null)for(int i=0;i<resourceButtons.Length;i++){int index=i;resourceButtons[i].onClick.AddListener(()=>Notify(new[]{"CREDITS","CRYSTALS"}[index]));}
        }
        void OnEnable() { Refresh(); }
        public void Refresh()
        {
            if(!dashboard)return;
            var data=dashboard.lobby.GetComponent<LobbyScreenData>();
            playerLabel.text=PlayerProgress.CommanderName;
            var unit=dashboard.ActiveUnit;
            if(unit!=null){portrait.sprite=dashboard.playerAvatar?dashboard.playerAvatar:unit.portrait;portrait.enabled=portrait.sprite;if(unitCaption)unitCaption.text=unit.displayName;}
            if(commanderLabel)commanderLabel.text="COMMANDER  /  LV. "+dashboard.commanderLevel;
            if(levelBadge)levelBadge.text=dashboard.commanderLevel.ToString();
            if(experienceFill)experienceFill.sizeDelta=new Vector2(174*Mathf.Clamp01(dashboard.commanderProgress),3);
            if(resourceValues!=null)for(int i=0;i<resourceValues.Length;i++)
            {
                string amount=i==0?PlayerProgress.Credits.ToString("N0"):PlayerProgress.Crystals.ToString("N0");
                if(i==0&&amount.Contains("/")){var parts=amount.Split('/');amount=parts[0].Trim()+" <size=70%><color=#829EAC>/ "+parts[1].Trim()+"</color></size>";}
                resourceValues[i].text=amount;
            }
            lastModel=dashboard.lobby.gundamPrefab;progressVersion=PlayerProgress.Version;
            Style(laboratory,page==Page.Laboratory);Style(inventory,page==Page.Inventory);Style(battle,page==Page.Battle);Style(shop,page==Page.Shop);
        }
        void Style(Button button,bool selected)
        {
            var shape=button.GetComponent<LobbyShape>();var hover=button.GetComponent<LobbyHover>();
            hover.normal=selected?new Color(.031f,.486f,.671f):new Color(.027f,.067f,.09f,.96f);
            shape.color=hover.normal;shape.border=selected?new Color(.125f,.785f,.965f):new Color(.13f,.275f,.33f);
            shape.SetVerticesDirty();
        }
        void LateUpdate(){if(dashboard&&(lastModel!=dashboard.lobby.gundamPrefab||progressVersion!=PlayerProgress.Version))Refresh();}
        public void OpenLaboratory(){var menu=GetComponent<LaboratoryDropdown>();if(menu)menu.Toggle();else dashboard.OpenLaboratory();}
        public void OpenBattle(){dashboard.Battle();}
        public void OpenShop(){dashboard.Shop();}
        public void OpenInventory()
        {
            dashboard.OpenInventory();
        }
        public void Notify(string title)
        {
            LobbyServices.For(dashboard).Open(title);
        }
    }
}
