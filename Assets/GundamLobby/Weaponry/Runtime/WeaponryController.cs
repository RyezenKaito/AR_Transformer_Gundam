using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GundamLobby
{
    [DefaultExecutionOrder(210)]
    public class WeaponryController : MonoBehaviour
    {
        public WeaponryCatalog catalog;
        public HomeLobbyDashboard dashboard;
        public GameObject weaponryCanvas, homeCanvas, previewMount;
        public HomeLobbyController lobby;
        public StrikerSelectionController striker;
        public InventoryPanel inventory;
        public Camera previewCamera;
        public WeaponrySkillCard[] cards;
        public TMP_Text unitLabel, serialLabel, roleLabel, categoryLabel, titleLabel, descriptionLabel, badgeLabel;
        public TMP_Text[] statNameLabels, statValueLabels;
        public WeaponryGlyph detailGlyph, demoGlyph;
        public LobbyShape activeTab, passiveTab;
        public CanvasGroup toast, demoBeam;
        public TMP_Text toastLabel;
        public int SelectedSkill { get; private set; }
        public bool IsOpen => weaponryCanvas && weaponryCanvas.activeSelf;
        public GameObject PreviewModel => previewModel;
        public WeaponryLoadout Loadout {get;private set;}
        GameObject previewModel;
        Vector3 savedPosition;
        Quaternion savedRotation;
        float savedFov, toastUntil, demoUntil;
        bool pivotWasActive, lobbyWasEnabled;
        int width, height;

        void Awake() { weaponryCanvas.SetActive(false); previewMount.SetActive(false); }
        public void Open()
        {
            if (IsOpen) return;
            if (striker && striker.IsOpen) striker.Close();
            if (inventory && inventory.IsOpen) inventory.Close();
            if (!lobby.gundamPrefab || catalog.skills.Length == 0) return;
            savedPosition = previewCamera.transform.position;
            savedRotation = previewCamera.transform.rotation; savedFov = previewCamera.fieldOfView;
            pivotWasActive = lobby.modelPivot.gameObject.activeSelf; lobbyWasEnabled = lobby.enabled;
            lobby.enabled = false; lobby.modelPivot.gameObject.SetActive(false); homeCanvas.SetActive(false);
            previewMount.transform.SetPositionAndRotation(lobby.modelPivot.position, Quaternion.Euler(0, -12, 0));
            previewMount.SetActive(true);
            previewModel = Instantiate(lobby.gundamPrefab, previewMount.transform);
            previewModel.name = "Weaponry Preview / " + lobby.gundamName;
            previewModel.transform.localPosition = Vector3.zero;
            previewModel.transform.localRotation = Quaternion.identity;
            weaponryCanvas.SetActive(true);
            var unit = dashboard.ActiveUnit;
            unitLabel.text = unit != null ? unit.displayName : lobby.gundamName;
            serialLabel.text = unit != null ? unit.code : "MOBILE SUIT";
            roleLabel.text = (unit != null ? unit.role : lobby.role) + "  /  LV. " + lobby.level;
            toast.alpha = 0; demoBeam.alpha = 0; demoUntil = toastUntil = 0;
            Fit(); FramePreview(); SelectSkill(0);
            var skin=previewModel.AddComponent<GundamSkinBinding>();skin.dashboard=dashboard;
            Loadout=GetComponent<WeaponryLoadout>();if(!Loadout)Loadout=gameObject.AddComponent<WeaponryLoadout>();Loadout.owner=this;Loadout.Opened();
        }
        void FramePreview()
        {
            var renderers = previewModel.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            Vector3 target = new Vector3(bounds.center.x, bounds.min.y + bounds.size.y * .52f, bounds.center.z);
            float aspect = Screen.width / (float)Mathf.Max(1, Screen.height);
            float distance = Mathf.Max(bounds.size.y * 2.1f, bounds.size.x * 1.65f / Mathf.Max(.8f, aspect));
            previewCamera.transform.position = target + new Vector3(.55f, -.4f, -distance);
            previewCamera.transform.LookAt(target); previewCamera.fieldOfView = 34;
        }
        public void SelectSkill(int index)
        {
            if (index < 0 || index >= catalog.skills.Length) return;
            if(Loadout){if(index==4){Loadout.SetMode(true);return;}Loadout.SetMode(false);}
            SelectedSkill = index; var skill = catalog.skills[index];
            categoryLabel.text = skill.category + "  /  LV. 01";
            titleLabel.text = skill.title; descriptionLabel.text = skill.description; badgeLabel.text = skill.badge;
            var unit = dashboard.ActiveUnit;
            if (unit != null && unit.damageType == "MELEE" && index == 0)
            { titleLabel.text = "BASIC ATTACK"; descriptionLabel.text = "Strike one enemy with a melee weapon for 100% ATK damage. Restores 1 team skill point."; }
            for (int i = 0; i < statNameLabels.Length; i++)
            {
                statNameLabels[i].text = i < skill.statNames.Length ? skill.statNames[i] : "";
                statValueLabels[i].text = i < skill.statValues.Length ? skill.statValues[i] : "";
            }
            detailGlyph.kind = demoGlyph.kind = skill.glyph;
            detailGlyph.SetVerticesDirty(); demoGlyph.SetVerticesDirty();
            for (int i = 0; i < cards.Length; i++) cards[i].Bind(catalog.skills[i], i == index);
            var profile=unit!=null?GundamGame.CombatDemo.PlayerSelectionData.Load(unit.id):null;
            if(profile&&index<4&&index<profile.skills.Length)
            {
             var real=profile.skills[index];titleLabel.text=real.displayName;categoryLabel.text=new[]{"BASIC ATTACK","SKILL I","SKILL II","FULL BURST"}[index]+" / "+real.burstCost+" BURST";
             descriptionLabel.text="Combat profile: "+real.displayName+".\nDamage scaling: "+(real.damageMultiplier*100).ToString("0")+"% ATK. Armor multiplier: "+real.armorMultiplier.ToString("0.0")+"x.\nBurst cost: "+real.burstCost+". Test the complete animation and timing in Training.";
             if(statNameLabels.Length>0){statNameLabels[0].text="CURRENT ATK";statValueLabels[0].text=PlayerProgress.Scaled(unit.id,unit.attack,"attack").ToString();}
             for(int i=1;i<statNameLabels.Length;i++){statNameLabels[i].text="";statValueLabels[i].text="";}
            }
            else if(unit!=null&&index==4){var module=GameItems.Find(PlayerProgress.Module(unit.id));titleLabel.text=module!=null?module.name:"NO MODULE EQUIPPED";descriptionLabel.text=module!=null?module.description:"Open Inventory and equip an Assault Circuit, Aegis Plating or Vector Thruster. Select a body part to install compatible equipment.";categoryLabel.text="EQUIPMENT / REUSABLE";}
            for(int i=0;i<cards.Length;i++){if(i<4&&profile&&i<profile.skills.Length){cards[i].titleLabel.text=profile.skills[i].displayName;cards[i].categoryLabel.text=new[]{"BASIC","SKILL I","SKILL II","FULL BURST"}[i];}else if(i==4){cards[i].titleLabel.text="EQUIPMENT";cards[i].categoryLabel.text="MODULE";}}
            SetTab(activeTab, index < 4); SetTab(passiveTab, index >= 4);
        }
        void SetTab(LobbyShape tab, bool selected)
        {
            tab.color = selected ? new Color(.03f,.34f,.46f) : new Color(.025f,.075f,.10f);
            tab.border = selected ? new Color(.125f,.785f,.965f) : Color.clear; tab.SetVerticesDirty();
        }
        public void ShowActive() { SelectSkill(0); }
        public void ShowPassive() { if(Loadout)Loadout.SetMode(true);else SelectSkill(4); }
        public void PlayDemonstration()
        {
            if (!IsOpen) return;
            dashboard.Battle();dashboard.battlePanel.SelectMode(3);dashboard.battlePanel.Launch();
        }

        public void Notify(string message) { toastLabel.text=message;toastUntil=Time.unscaledTime+2.8f; }
        public void RotatePreview(float degrees) { if (IsOpen&&!LobbyServices.BlocksInput) previewMount.transform.Rotate(0,degrees,0,Space.Self); }
        public void ZoomPreview(float amount) { if (IsOpen&&!LobbyServices.BlocksInput) previewCamera.fieldOfView = Mathf.Clamp(previewCamera.fieldOfView + amount, 25, 48); }
        public void Close()
        {
            if (!IsOpen) return;
            weaponryCanvas.SetActive(false); previewMount.SetActive(false);
            if (previewModel) { previewModel.SetActive(false); Destroy(previewModel); previewModel = null; }
            previewCamera.transform.SetPositionAndRotation(savedPosition,savedRotation); previewCamera.fieldOfView = savedFov;
            lobby.modelPivot.gameObject.SetActive(pivotWasActive); lobby.enabled = lobbyWasEnabled;
            homeCanvas.SetActive(true);
        }
        public void OpenInventory() { Close(); inventory.Open(); }
        public void OpenStrikers() { Close(); striker.Open(); }
        void Fit()
        {
            width=Screen.width; height=Screen.height;
            weaponryCanvas.GetComponent<CanvasScaler>().matchWidthOrHeight=width/(float)Mathf.Max(1,height)>=16f/9f?1:0;
        }
        void Update()
        {
            if (!IsOpen) return;
            if (width!=Screen.width || height!=Screen.height) Fit();
            toast.alpha=Mathf.MoveTowards(toast.alpha,Time.unscaledTime<toastUntil?1:0,Time.unscaledDeltaTime*7);
            demoBeam.alpha=Time.unscaledTime<demoUntil ? Mathf.Abs(Mathf.Sin((demoUntil-Time.unscaledTime)*12))*.9f : .22f;
#if ENABLE_INPUT_SYSTEM
            if (!LobbyServices.BlocksInput && Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) Close();
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape)) Close();
#endif
        }
    }
}
