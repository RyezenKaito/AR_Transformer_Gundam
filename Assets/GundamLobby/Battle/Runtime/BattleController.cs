using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GundamGame.CombatDemo;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GundamLobby
{
    [DefaultExecutionOrder(220)]
    public class BattleController : MonoBehaviour
    {
        public HomeLobbyDashboard dashboard;
        public BattleModeCatalog catalog;
        public GameObject battleCanvas,stage,modelMount;
        public Transform mapPivot;
        public Camera previewCamera;
        public BattleModeCard[] cards;
        public BattleMapPin[] pins;
        public TMP_Text modeLabel,descriptionLabel,codeLabel,levelLabel,energyLabel,locationLabel,unitLabel,launchLabel,difficultyLabel;
        public Image unitPortrait;
        public Button normalButton,hardButton;
        public CanvasGroup toast;
        public TMP_Text toastLabel;
        [Header("Chapter and stage selection")]
        public LobbyStoryFlow campaignFlow;
        public int SelectedMode {get;private set;}
        public int SelectedLocation {get;private set;}
        public bool Advanced {get;private set;}
        public bool IsOpen=>battleCanvas&&battleCanvas.activeSelf;
        public GameObject PreviewModel=>previewModel;
        public int LaunchCount {get;private set;}
        GameObject previewModel;
        Vector3 savedCameraPosition;Quaternion savedCameraRotation;float savedFov,toastUntil,mapYaw;
        bool pivotWasActive,lobbyWasEnabled,resumeAfterStriker;
        int screenW,screenH;

        void Awake(){battleCanvas.SetActive(false);stage.SetActive(false);}
        public void Open()
        {
            if(IsOpen)return;
            dashboard.OpenLaboratory();
            savedCameraPosition=previewCamera.transform.position;savedCameraRotation=previewCamera.transform.rotation;savedFov=previewCamera.fieldOfView;
            pivotWasActive=dashboard.lobby.modelPivot.gameObject.activeSelf;lobbyWasEnabled=dashboard.lobby.enabled;
            dashboard.lobby.enabled=false;dashboard.lobby.modelPivot.gameObject.SetActive(false);dashboard.striker.lobbyCanvas.SetActive(false);
            stage.SetActive(true);battleCanvas.SetActive(true);SpawnPreview();
            previewCamera.transform.position=new Vector3(.6f,7.6f,-18.5f);previewCamera.transform.LookAt(new Vector3(.6f,2.8f,-.8f));previewCamera.fieldOfView=46;
            Fit();SelectMode(SelectedMode);SelectLocation(SelectedLocation);SetDifficulty(Advanced);toast.alpha=0;toastUntil=0;
        }
        void SpawnPreview()
        {
            if(previewModel){previewModel.SetActive(false);Destroy(previewModel);}
            var unit=dashboard.ActiveUnit;var prefab=unit!=null?unit.prefab:dashboard.lobby.gundamPrefab;
            if(!prefab)return;
            previewModel=Instantiate(prefab,modelMount.transform);previewModel.name="Battle Gundam Preview";var skin=previewModel.AddComponent<GundamSkinBinding>();skin.dashboard=dashboard;previewModel.transform.localPosition=Vector3.zero;previewModel.transform.localRotation=Quaternion.identity;
            var renderers=previewModel.GetComponentsInChildren<Renderer>();if(renderers.Length>0){var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);previewModel.transform.localScale*=9f/Mathf.Max(.1f,bounds.size.y);}
            if(unit!=null){unitLabel.text=unit.displayName+"\nLV. "+PlayerProgress.UnitLevel(unit.id);unitPortrait.sprite=unit.CardPortrait;unitPortrait.preserveAspect=true;unitPortrait.enabled=unit.CardPortrait;}
        }
        public void SelectMode(int index)
        {
            if(index<0||index>=catalog.modes.Length)return;SelectedMode=index;var mode=catalog.modes[index];
            modeLabel.text=mode.title;descriptionLabel.text=mode.description;codeLabel.text="TURN-BASED COMBAT  /  "+mode.code;
            energyLabel.text=index>=3?"PRACTICE / NO REWARDS":"UNRESTRICTED DEPLOYMENT";
            var preparation=GameItems.Find(PlayerProgress.PreparedItem(dashboard.ActiveUnit.id));if(preparation!=null)energyLabel.text=GameItems.Summary(preparation);
            launchLabel.text="DEPLOY   >";
            for(int i=0;i<cards.Length;i++)cards[i].Bind(catalog.modes[i],i==index);
            SetDifficulty(Advanced);
        }
        public void SelectLocation(int index)
        {
            if(index<0||index>=catalog.locationNames.Length)return;SelectedLocation=index;locationLabel.text=catalog.locationNames[index];
            for(int i=0;i<pins.Length;i++)pins[i].SetSelected(i==index);
        }
        public void NormalDifficulty(){SetDifficulty(false);}
        public void AdvancedDifficulty(){SetDifficulty(true);}
        public void SetDifficulty(bool advanced)
        {
            Advanced=advanced;difficultyLabel.text=advanced?"ADVANCED":"STANDARD";
            levelLabel.text=(catalog.modes[SelectedMode].recommendedLevel+(advanced?10:0)).ToString();
            StyleDifficulty(normalButton,!advanced);StyleDifficulty(hardButton,advanced);
        }
        void StyleDifficulty(Button b,bool selected)
        {
            var hover=b.GetComponent<LobbyHover>();hover.normal=selected?new Color(.03f,.36f,.48f):new Color(.025f,.075f,.105f);hover.surface.color=hover.normal;
        }
        public void RotateMap(float degrees){if(IsOpen){mapYaw=Mathf.Clamp(mapYaw+degrees,-28,28);mapPivot.localRotation=Quaternion.Euler(0,mapYaw,0);}}
        public void Launch()
        {
            if(!IsOpen||(campaignFlow&&campaignFlow.IsOpen))return;
            var unit=dashboard.ActiveUnit;
            if(unit==null||!PlayerSelectionData.Supports(unit.id))
            {
                Notify("Select a playable Gundam before deploying.");
                return;
            }
            if(!campaignFlow)
            {
                Notify("Chapter selection is unavailable.");
                return;
            }
            SortieSession.Mode=SelectedMode;SortieSession.Advanced=Advanced;
            PlayerSelectionData.Equip(unit.id);
            LaunchCount++;
            campaignFlow.OpenChapters();
        }
        public void Notify(string message){toastLabel.text=message;toastUntil=Time.unscaledTime+3;}
        public void OpenStrikers(){if(!IsOpen)return;Close();resumeAfterStriker=true;dashboard.striker.Open();}
        public void Close()
        {
            if(!IsOpen)return;battleCanvas.SetActive(false);stage.SetActive(false);
            if(previewModel){previewModel.SetActive(false);Destroy(previewModel);previewModel=null;}
            previewCamera.transform.SetPositionAndRotation(savedCameraPosition,savedCameraRotation);previewCamera.fieldOfView=savedFov;
            dashboard.lobby.modelPivot.gameObject.SetActive(pivotWasActive);dashboard.lobby.enabled=lobbyWasEnabled;dashboard.striker.lobbyCanvas.SetActive(true);
        }
        void Fit(){screenW=Screen.width;screenH=Screen.height;battleCanvas.GetComponent<CanvasScaler>().matchWidthOrHeight=screenW/(float)Mathf.Max(1,screenH)>=16f/9f?1:0;}
        void Update()
        {
            if(LobbyServices.BlocksInput)return;
            if(campaignFlow&&campaignFlow.IsOpen)return;
            if(resumeAfterStriker&&!dashboard.striker.IsOpen){resumeAfterStriker=false;Open();return;}
            if(!IsOpen)return;if(screenW!=Screen.width||screenH!=Screen.height)Fit();
            toast.alpha=Mathf.MoveTowards(toast.alpha,Time.unscaledTime<toastUntil?1:0,Time.unscaledDeltaTime*7);
#if ENABLE_INPUT_SYSTEM
            if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)Close();
#elif ENABLE_LEGACY_INPUT_MANAGER
            if(Input.GetKeyDown(KeyCode.Escape))Close();
#endif
        }
    }
}
