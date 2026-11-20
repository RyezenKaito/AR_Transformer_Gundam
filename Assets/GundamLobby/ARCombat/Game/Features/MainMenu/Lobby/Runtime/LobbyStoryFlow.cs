using System.Collections;
using GundamGame;
using GundamGame.Story;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GundamLobby
{
    [AddComponentMenu("Gundam Lobby/Main Menu Controller")]
    public sealed class LobbyStoryFlow : MonoBehaviour
    {
        public enum Page { Closed, Modes, Chapters, Stages, Loading }

        [SerializeField] RectTransform campaignCanvas;
        [SerializeField] TMP_FontAsset bodyFont, displayFont;
        [SerializeField] Sprite battleBackdrop, storyBackdrop;
        [SerializeField] StoryStage stageOne, stageTwo;

        CampaignHud hud;
        GameObject previousSelection;
        int selectedStage;
        bool transitioning, enteredAtChapters;
        int closedAtFrame=-1;
        public Page CurrentPage { get; private set; }
        public bool IsOpen => CurrentPage!=Page.Closed || transitioning || closedAtFrame==Time.frameCount;
        public StoryStage SelectedStage => selectedStage==0 ? stageOne : stageTwo;

        void Awake()
        {
            var roster=FindAnyObjectByType<StrikerSelectionController>();
            hud=new CampaignHud(campaignCanvas,bodyFont,displayFont,battleBackdrop,storyBackdrop,roster?roster.selectionCanvas:null,
                stageOne,stageTwo,Back,ShowChapters,ShowStages,SelectMission,Deploy);
            hud.Root.SetAsLastSibling();
        }

        public void Open()
        {
            if(CurrentPage!=Page.Closed || transitioning)return;
            if(EventSystem.current)previousSelection=EventSystem.current.currentSelectedGameObject;
            StartCoroutine(ChangePage(Page.Modes,1));
        }

        public void OpenChapters()
        {
            if(IsOpen)return;
            if(EventSystem.current)previousSelection=EventSystem.current.currentSelectedGameObject;
            enteredAtChapters=true;
            selectedStage=0;
            hud.FinishDrawerTransition(false);
            hud.SetProgress(stageOne,stageTwo);
            StartCoroutine(ChangePage(Page.Chapters,1));
        }

        void Update()
        {
            if(CurrentPage==Page.Closed && !transitioning)return;
            hud.RefreshWallet();hud.SafeArea();
            hud.TickRadar();
            if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame)Back();
        }

        public void Back()
        {
            if(transitioning)return;
            switch(CurrentPage)
            {
                case Page.Modes: StartCoroutine(ChangePage(Page.Closed,-1)); break;
                case Page.Chapters: StartCoroutine(ChangePage(enteredAtChapters?Page.Closed:Page.Modes,-1)); break;
                case Page.Stages: StartCoroutine(ChangeDrawer(false)); break;
            }
        }

        void ShowChapters()
        {
            if(CurrentPage!=Page.Modes || transitioning)return;
            hud.SetProgress(stageOne,stageTwo);
            StartCoroutine(ChangePage(Page.Chapters,1));
        }

        void ShowStages()
        {
            if(CurrentPage!=Page.Chapters || transitioning)return;
            selectedStage=0;
            hud.SetProgress(stageOne,stageTwo);
            hud.ShowMission(SelectedStage,selectedStage,GameSessionManager.EquippedGundam);
            StartCoroutine(ChangeDrawer(true));
        }

        void SelectMission(int index)
        {
            if(CurrentPage!=Page.Stages || transitioning)return;
            selectedStage=index;
            hud.ShowMission(SelectedStage,index,GameSessionManager.EquippedGundam);
        }

        void Deploy()
        {
            if(CurrentPage!=Page.Stages || transitioning)return;
            if(!GameSessionManager.EquippedGundam)
            {
                hud.ShowDeployError("Equipped Gundam data is unavailable.");
                return;
            }
            var stage=SelectedStage;
            int scene=SceneUtility.GetBuildIndexByScenePath(stage.scenePath);
            if(scene<0)
            {
                hud.ShowDeployError("Scene is not enabled in Build Settings.");
                return;
            }
            hud.ShowLoading(stage.locationName);
            StartCoroutine(LoadStage(scene));
        }

        IEnumerator LoadStage(int scene)
        {
            yield return ChangePage(Page.Loading,1);
            SceneManager.LoadScene(scene);
        }

        IEnumerator ChangeDrawer(bool open)
        {
            BeginTransition();
            hud.BeginDrawerTransition();
            CurrentPage=open?Page.Stages:Page.Chapters;
            float duration=open?.28f:.20f;
            for(float elapsed=0;elapsed<duration;elapsed+=Time.unscaledDeltaTime)
            {
                float progress=Mathf.SmoothStep(0,1,elapsed/duration);
                hud.SetDrawerProgress(open?progress:1-progress);
                yield return null;
            }
            hud.FinishDrawerTransition(open);
            EndTransition(CurrentPage);
        }

        IEnumerator ChangePage(Page page,int direction)
        {
            BeginTransition();
            var from=CurrentPage;
            if(from!=Page.Closed)
                yield return Animate(from,false,40f*direction,.11f,page==Page.Closed);

            if(from==Page.Closed)hud.SetCurtain(0);
            hud.SetVisible(page);
            CurrentPage=page;
            if(page!=Page.Closed)
                yield return Animate(page,true,54f*direction,.22f,from==Page.Closed);

            EndTransition(page);
        }

        void BeginTransition()
        {
            transitioning=true;
            hud.SetInput(false);
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(null);
        }

        void EndTransition(Page page)
        {
            transitioning=false;
            if(page==Page.Closed)
            {
                closedAtFrame=Time.frameCount;
                enteredAtChapters=false;
                if(EventSystem.current)EventSystem.current.SetSelectedGameObject(previousSelection);
            }
            else if(page!=Page.Loading)
            {
                hud.SetInput(true);
                hud.Focus(page);
            }
        }

        IEnumerator Animate(Page page,bool entering,float distance,float duration,bool fadeCurtain)
        {
            for(float elapsed=0;elapsed<duration;elapsed+=Time.unscaledDeltaTime)
            {
                float progress=Mathf.SmoothStep(0,1,elapsed/duration);
                float alpha=entering?progress:1-progress;
                hud.SetTransition(page,alpha,entering?distance*(1-progress):-distance*progress);
                if(fadeCurtain)hud.SetCurtain(alpha);
                yield return null;
            }
            hud.SetTransition(page,entering?1:0,0);
            if(fadeCurtain)hud.SetCurtain(entering?1:0);
        }
    }
}
