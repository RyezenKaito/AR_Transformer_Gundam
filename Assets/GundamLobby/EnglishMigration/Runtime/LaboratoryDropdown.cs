using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace GundamLobby
{
    // Preserve the existing script identity and scene bindings while supporting a fixed tab strip.
    public sealed class LaboratoryDropdown : MonoBehaviour
    {
        public HomeLobbyDashboard dashboard;
        public GameObject menuRoot;
        public CanvasGroup group;
        public Button dismiss, lobby, weaponry, strikeDesign, update;
        public bool horizontalMode;
        public bool IsOpen => menuRoot && menuRoot.activeSelf;
        int styledPage=int.MinValue;
        void Awake()
        {
            dismiss.onClick.AddListener(Close);
            lobby.onClick.AddListener(()=>Choose(0));
            weaponry.onClick.AddListener(()=>Choose(1));
            strikeDesign.onClick.AddListener(()=>Choose(2));
            update.onClick.AddListener(()=>Choose(3));
            if(horizontalMode)RefreshTabs();
            else Close();
        }
        void OnEnable(){if(horizontalMode&&menuRoot){styledPage=int.MinValue;RefreshTabs();}}
        public void Toggle()
        {
            if(horizontalMode){Choose(0);return;}
            if(IsOpen){Close();return;}
            menuRoot.SetActive(true);group.alpha=0;
        }
        public void Close(){if(!horizontalMode&&menuRoot)menuRoot.SetActive(false);}
        void OnDisable(){Close();}
        void Update()
        {
            if(horizontalMode){RefreshTabs();return;}
            if(!IsOpen)return;
            group.alpha=Mathf.MoveTowards(group.alpha,1,Time.unscaledDeltaTime/0.18f);
            if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)Close();
        }
        public void RefreshTabs()
        {
            if(horizontalMode&&menuRoot)
            {
                var navigation=GetComponent<LobbyNavigationBar>();
                bool laboratorySection=navigation&&(navigation.page==LobbyNavigationBar.Page.Laboratory||navigation.page==LobbyNavigationBar.Page.Weaponry);
                if(menuRoot.activeSelf!=laboratorySection)menuRoot.SetActive(laboratorySection);
                if(group){group.alpha=laboratorySection?1:0;group.interactable=laboratorySection;group.blocksRaycasts=laboratorySection;}
                if(!laboratorySection)return;
            }
            if(!dashboard)return;
            int page=LobbyServices.BlocksInput?3:dashboard.weaponry.IsOpen?1:dashboard.design&&dashboard.design.IsOpen?2:dashboard.striker.IsOpen?2:
                dashboard.inventory.IsOpen||(dashboard.battlePanel&&dashboard.battlePanel.IsOpen)||(dashboard.shopPanel&&dashboard.shopPanel.IsOpen)?-1:0;
            if(page==styledPage)return;styledPage=page;
            var buttons=new[]{lobby,weaponry,strikeDesign,update};
            for(int i=0;i<buttons.Length;i++)
            {
                if(!buttons[i])continue;
                var shape=buttons[i].GetComponent<LobbyShape>();var hover=buttons[i].GetComponent<LobbyHover>();
                if(!shape||!hover)continue;
                bool selected=i==page;
                hover.normal=selected?new Color(.03f,.34f,.46f,.98f):new Color(.025f,.07f,.10f,.94f);
                shape.color=hover.normal;shape.border=selected?new Color(.125f,.785f,.965f):new Color(.10f,.22f,.28f,.75f);shape.SetVerticesDirty();
            }
        }
        public void Choose(int item)
        {
            Close();
            if(item==0)dashboard.OpenLaboratory();
            else if(item==1)dashboard.OpenWeaponry();
            else if(item==2)dashboard.OpenDesign();
            else LobbyServices.For(dashboard).Open("UPDATE");
            if(horizontalMode){styledPage=int.MinValue;RefreshTabs();}
        }
    }
}
