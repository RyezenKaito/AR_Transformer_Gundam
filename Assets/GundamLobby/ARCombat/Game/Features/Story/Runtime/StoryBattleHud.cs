using System;
using System.Collections.Generic;
using GundamGame.CombatDemo;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using static GundamGame.Story.StoryHudUi;

namespace GundamGame.Story
{
    [DefaultExecutionOrder(200)]
    [RequireComponent(typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster))]
    public sealed partial class StoryBattleHud : MonoBehaviour
    {
        [SerializeField] TMP_FontAsset bodyFont, displayFont;
        readonly List<DemoFighter> order=new(5);
        StoryBattleHudState battle;
        StoryStage stage;
        RectTransform safeRoot;
        GameObject targetSurface;
        StoryEnemyOverhead[] overhead;
        Action pause;
        int selectedSkill=-1;
        bool wasPlayerTurn;
        Rect lastSafeArea;
        int screenWidth, screenHeight;
        float nextTelemetryRefresh;
        DemoFighter timelineActor;
        bool timelinePlayerTurn;

        public void Bind(CharacterBattlePackage package,CharacterCombatProfile profile,StoryStage stage,Action pause)
        {
            this.stage=stage;
            this.pause=pause;
            battle=new StoryBattleHudState(package,profile);
            var canvas=GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder=150;
            var scaler=GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            scaler.matchWidthOrHeight=.55f;

            if(!FindAnyObjectByType<EventSystem>())
            {
                var events=new GameObject("Story Battle EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform,false);
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            var field=Image(transform,"Battlefield Tap Surface",0,0,1,1,Color.clear);
            field.raycastTarget=true;
            targetSurface=field.gameObject;
            field.gameObject.AddComponent<BattlefieldTapSurface>().Tapped=position=>TrySelectTargetAtScreenPoint(position);
            safeRoot=Area(transform,"HUD Safe Area",0,0,1,1);
            BuildTimeline();
            BuildStageIdentity();
            BuildTelemetry(profile);
            BuildSkills();
            BuildReaction();
            overhead=new StoryEnemyOverhead[battle.Opponents.Length];
            for(int i=0;i<overhead.Length;i++)
                overhead[i]=new StoryEnemyOverhead(package.transform,battle.Opponents[i],battle.Camera,displayFont);
            battle.Subscribe(ShowReactionResult);
        }

        public bool TrySelectTargetAtScreenPoint(Vector2 position) => battle!=null && battle.PickTarget(position);

        void Update()
        {
            if(battle==null)return;
            var keyboard=Keyboard.current;
            if(keyboard!=null && keyboard.pKey.wasPressedThisFrame)pause?.Invoke();
            bool playerTurn=battle.PlayerTurn;
            if(!battle.Paused)
            {
                if(playerTurn&&!wasPlayerTurn)selectedSkill=-1;
                wasPlayerTurn=playerTurn;
            }
            if(keyboard==null || !battle.Visible)return;
            if(playerTurn)
            {
                if(keyboard.leftArrowKey.wasPressedThisFrame)battle.CycleTarget(-1);
                if(keyboard.rightArrowKey.wasPressedThisFrame)battle.CycleTarget(1);
                if(keyboard.digit1Key.wasPressedThisFrame)SelectSkill(0,false);
                if(keyboard.digit2Key.wasPressedThisFrame)SelectSkill(1,false);
                if(keyboard.digit3Key.wasPressedThisFrame)SelectSkill(2,false);
                if(keyboard.digit4Key.wasPressedThisFrame)SelectSkill(3,false);
                if(keyboard.spaceKey.wasPressedThisFrame)ConfirmSkill();
            }
            else if(battle.CanDefend)
            {
                if(keyboard.qKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)battle.Defend(true);
                else if(keyboard.eKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)battle.Defend(false);
            }
        }

        void LateUpdate()
        {
            if(battle==null)return;
            bool visible=battle.Visible;
            safeRoot.gameObject.SetActive(visible);
            bool fieldActive=battle.PlayerTurn;
            targetSurface.SetActive(fieldActive);
            var target=battle.Target;
            for(int i=0;i<overhead.Length;i++)
                overhead[i].Refresh(battle.Camera,visible,fieldActive&&target==battle.Opponents[i],
                    battle.Paused?0:Time.unscaledDeltaTime);
            if(!visible)return;

            UpdateSafeArea();
            skillsPanel.gameObject.SetActive(fieldActive);
            defensePanel.gameObject.SetActive(battle.Incoming);
            bool turnChanged=battle.Actor!=timelineActor || fieldActive!=timelinePlayerTurn;
            bool refreshVitals=turnChanged || Time.unscaledTime>=nextTelemetryRefresh;
            if(refreshVitals)
            {
                timelineActor=battle.Actor;
                timelinePlayerTurn=fieldActive;
                RefreshTimeline();
                nextTelemetryRefresh=Time.unscaledTime+.06f;
                RefreshTelemetry();
                if(fieldActive)RefreshSkills();
            }
            RefreshBurstGlow();
            if(battle.Incoming)RefreshReactionGauge();
            RefreshReactionResult();
        }

        void UpdateSafeArea()
        {
            Rect safe=Screen.safeArea;
            if(safe==lastSafeArea && screenWidth==Screen.width && screenHeight==Screen.height)return;
            screenWidth=Mathf.Max(1,Screen.width);screenHeight=Mathf.Max(1,Screen.height);
            lastSafeArea=safe;
            safeRoot.anchorMin=new Vector2(safe.xMin/screenWidth,safe.yMin/screenHeight);
            safeRoot.anchorMax=new Vector2(safe.xMax/screenWidth,safe.yMax/screenHeight);
        }

        void SelectSkill(int index,bool confirm)
        {
            if(!battle.PlayerTurn)return;
            if(confirm && selectedSkill==index){ConfirmSkill();return;}
            selectedSkill=index;
            RefreshSkills();
        }

        void ConfirmSkill()
        {
            if(selectedSkill<0 || !battle.Cast(selectedSkill))return;
            selectedSkill=-1;
        }

        void OnDestroy()
        {
            battle?.Unsubscribe(ShowReactionResult);
            if(overhead!=null)
                foreach(var item in overhead)item?.Release();
            if(reactionSounds!=null)
                foreach(var sound in reactionSounds)if(sound)Destroy(sound);
        }
    }
}
