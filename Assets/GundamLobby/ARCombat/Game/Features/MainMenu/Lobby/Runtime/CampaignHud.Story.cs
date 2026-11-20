using System;
using GundamGame.CombatDemo;
using GundamGame.Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    internal sealed partial class CampaignHud
    {
        static readonly Color Midnight = new(.014f, .029f, .061f);
        static readonly Color DrawerGlass = new(.024f, .060f, .11f, .96f);
        static readonly Color MissionGlass = new(.023f, .054f, .10f, .98f);
        static readonly Color SelectedStageFill = new(.70f, .94f, .99f);
        static readonly Color SelectedStageInk = new(.027f, .098f, .146f);

        readonly StageItem[] stageItems = new StageItem[2];
        RectTransform storySafeArea, drawer, briefing, progressFill;
        CanvasGroup mapInput, drawerLayer;
        ScrollRect chapterScroll;
        Image drawerShade, missionImage, enemyPortrait;
        AspectRatioFitter enemyPortraitAspect;
        TMP_Text chapterProgress, lockedNotice, missionName, missionBrief, enemyIntel, selectedName, selectedStats;
        Button chapterOne, deploy;
        float lockedNoticeUntil;

        void BuildStoryCampaign(RectTransform root, Sprite backdrop, GameObject rosterCanvas,
            StoryStage firstStage, StoryStage secondStage, Action back, Action openChapter,
            Action<int> selectStage, Action deployStage)
        {
            ImageBox("Deep space base",root,0,0,1,1,Midnight);
            if(backdrop)
                Banner("Deep Space Tactical Map",root,backdrop,0,0,1,1);
            Gradient(Area("Space vignette",root,0,0,1,1),new Color(.007f,.018f,.052f,.29f),new Color(.008f,.021f,.046f,.12f));
            storySafeArea=Area("Story safe area",root,0,0,1,1);
            BuildChapterMap(storySafeArea,firstStage,openChapter);

            drawerLayer=Area("Chapter missions overlay",storySafeArea,0,0,1,1).gameObject.AddComponent<CanvasGroup>();
            drawerShade=ImageBox("Dimmed chapter map",drawerLayer.transform,0,0,1,1,new Color(.012f,.025f,.055f,.63f));
            drawerShade.raycastTarget=true;
            BuildStageDrawer(drawerLayer.transform,firstStage,secondStage,back,selectStage);
            BuildMissionCard(drawerLayer.transform,deployStage);
            FinishDrawerTransition(false);
            Header(storySafeArea,rosterCanvas,"STORY CAMPAIGN // OPERATION LOG",back);
        }

        void BuildChapterMap(RectTransform parent, StoryStage firstStage, Action openChapter)
        {
            var map=Area("Campaign node map",parent,0,0,1,.926f);
            mapInput=map.gameObject.AddComponent<CanvasGroup>();
            var heading=ChapterHeading("CAMPAIGN ROUTE",map,.044f,.848f,.64f,.974f,50,White);
            heading.font=font;
            ImageBox("Campaign heading accent",map,.045f,.835f,.094f,.839f,Cyan);

            chapterScroll=Area("Chapter route scroll",map,0,.212f,1,.825f).gameObject.AddComponent<ScrollRect>();
            chapterScroll.horizontal=true;
            chapterScroll.vertical=false;
            chapterScroll.movementType=ScrollRect.MovementType.Clamped;
            chapterScroll.viewport=Area("Chapter route viewport",chapterScroll.transform,0,0,1,1);
            chapterScroll.viewport.gameObject.AddComponent<RectMask2D>();
            ImageBox("Chapter drag surface",chapterScroll.viewport,0,0,1,1,Color.clear).raycastTarget=true;
            chapterScroll.content=Area("Connected chapter nodes",chapterScroll.viewport,0,0,1.105f,1);
            chapterScroll.content.pivot=new Vector2(0,.5f);

            var route=chapterScroll.content;
            ImageBox("Route glow outer",route,.006f,.474f,.972f,.524f,new Color(.11f,.75f,1,.07f));
            ImageBox("Route glow inner",route,.006f,.487f,.972f,.512f,new Color(.24f,.82f,1,.23f));
            ImageBox("Energy route",route,.006f,.498f,.972f,.503f,new Color(.38f,.91f,1,.91f));
            foreach(float gap in new[] { .337f, .647f })
            {
                var node=Panel(Area("Route junction",route,gap-.009f,.473f,gap+.009f,.528f),Midnight,Cyan,0);
                node.GetComponent<LobbyShape>().geometry=LobbyShape.Geometry.Diamond;
            }

            chapterOne=MakeButton("",route,.038f,.112f,.302f,.956f,openChapter,28,true);
            chapterOne.name="CHAPTER 1";
            var card=chapterOne.transform;
            var surface=(LobbyShape)chapterOne.targetGraphic;
            surface.cut=23;
            surface.color=new Color(.028f,.112f,.193f,.99f);
            Panel(Area("Active chapter illuminated edge",card,-.010f,-.012f,1.010f,1.012f),Color.clear,
                new Color(.22f,.79f,1,.24f),26).GetComponent<LobbyShape>().thickness=7;
            Banner("Desert chapter preview",card,firstStage.thumbnail,.037f,.317f,.963f,.948f);
            Gradient(Area("Chapter image fade",card,.037f,.317f,.963f,.651f),Color.clear,new Color(.021f,.048f,.084f,.98f));
            var badge=Panel(Area("Active chapter badge",card,.061f,.799f,.435f,.909f),Midnight,Cyan,7);
            ChapterHeading("IN PROGRESS",badge,.061f,0,.939f,1,20,Cyan,TextAlignmentOptions.Center);
            ChapterHeading("CHAPTER 1",card,.070f,.329f,.936f,.515f,57,White);
            var subtitle=Label("Secure the supply outpost and open the route into the city.",
                card,.070f,.154f,.925f,.307f,25,White);
            subtitle.fontSizeMin=14;
            chapterProgress=Label("",card,.070f,.055f,.50f,.143f,22,Cyan);
            var progress=ImageBox("Chapter progress track",card,.543f,.096f,.924f,.110f,Line);
            progressFill=ImageBox("Completed stage proportion",progress.transform,0,0,0,1,Cyan).rectTransform;
            BuildLockedChapter(route,.369f,.612f,"CHAPTER 2");
            BuildLockedChapter(route,.678f,.921f,"CHAPTER 3");

            var track=Panel(Area("Chapter scroll track",map,.043f,.177f,.237f,.193f),Midnight,Line,3);
            track.GetComponent<LobbyShape>().raycastTarget=true;
            var sliding=Area("Sliding area",track,.016f,.17f,.984f,.83f);
            var handle=ImageBox("Chapter scroll handle",sliding,0,0,1,1,Cyan);
            handle.raycastTarget=true;
            var scrollbar=track.gameObject.AddComponent<Scrollbar>();
            scrollbar.targetGraphic=handle;
            scrollbar.handleRect=handle.rectTransform;
            chapterScroll.horizontalScrollbar=scrollbar;
            lockedNotice=Label("",map,.32f,.103f,.68f,.164f,26,Amber,TextAlignmentOptions.Center);
            lockedNotice.gameObject.SetActive(false);
        }

        void BuildLockedChapter(Transform parent,float left,float right,string name)
        {
            var button=MakeButton("",parent,left,.18f,right,.87f,()=>ShowLockedChapter(name),28,false);
            button.name=name;
            var card=button.transform;
            var shape=(LobbyShape)button.targetGraphic;
            shape.color=new Color(.019f,.036f,.076f,.96f);
            shape.border=new Color(.18f,.40f,.55f,.76f);
            Panel(Area("Locked card inset",card,.033f,.039f,.967f,.961f),Color.clear,new Color(.24f,.48f,.61f,.26f),13);
            var grid=Panel(Area("Locked node coordinates",card,.04f,.04f,.96f,.96f),new Color(.28f,.58f,.74f,.11f),Color.clear,0);
            grid.GetComponent<LobbyShape>().geometry=LobbyShape.Geometry.Grid;
            DrawLock(card,.39f,.526f,.61f,.780f,new Color(.46f,.60f,.69f));
            ChapterHeading(name,card,.06f,.296f,.94f,.496f,48,Soft,TextAlignmentOptions.Center);
            ChapterHeading("LOCKED",card,.10f,.156f,.90f,.304f,23,Amber,TextAlignmentOptions.Center);
        }

        void ShowLockedChapter(string name)
        {
            lockedNotice.text=name+"  //  LOCKED";
            lockedNotice.gameObject.SetActive(true);
            lockedNoticeUntil=Time.unscaledTime+1.8f;
        }

        void BuildStageDrawer(Transform parent, StoryStage firstStage, StoryStage secondStage,
            Action back, Action<int> selectStage)
        {
            drawer=Panel(Area("Left Stage Drawer",parent,0,0,.38f,.926f),DrawerGlass,Line,25);
            ImageBox("Drawer edge",drawer,.996f,.02f,1,.979f,new Color(.36f,.88f,1,.54f));
            MakeButton("← CHAPTERS",drawer,.058f,.881f,.446f,.957f,back,25,false);
            ChapterHeading("CHAPTER 1",drawer,.061f,.754f,.917f,.879f,58,White);
            ImageBox("Drawer heading divider",drawer,.061f,.732f,.930f,.734f,Line);

            for(int i=0;i<stageItems.Length;i++)
            {
                int index=i;
                var stage=i==0?firstStage:secondStage;
                float top=.688f-i*.206f;
                var button=MakeButton("",drawer,.060f,top-.170f,.935f,top,()=>selectStage(index),24,false);
                button.name=stage.stageId;
                var row=button.transform;
                var tile=Panel(Area("Combat icon field",row,.034f,.21f,.156f,.79f),new Color(.12f,.27f,.38f,.55f),Line,10);
                var icon=Panel(Area("Combat icon",tile,.09f,.08f,.91f,.92f),Cyan,Color.clear,0).GetComponent<LobbyShape>();
                icon.geometry=LobbyShape.Geometry.Crosshair;
                icon.thickness=3f;
                var marker=ChapterHeading($"STAGE {i+1:00}  /  {stage.stageId}",row,.197f,.61f,.73f,.89f,22,Cyan);
                var name=ChapterHeading(stage.locationName.ToUpperInvariant(),row,.197f,.22f,.960f,.62f,38,White);
                var status=ChapterHeading("",row,.745f,.61f,.960f,.89f,21,Cyan,TextAlignmentOptions.MidlineRight);
                stageItems[i]=new StageItem(button,tile.GetComponent<LobbyShape>(),icon,marker,name,status);
            }
        }

        TMP_Text rewardText;
        void BuildMissionCard(Transform parent,Action deployStage)
        {
            briefing=Panel(Area("Right Mission Briefing",parent,.550f,.035f,.971f,.899f),MissionGlass,Line,27);
            ImageBox("Briefing top accent",briefing,.035f,.991f,.241f,.995f,Cyan);
            missionImage=Banner("Map preview",briefing,null,.032f,.635f,.968f,.964f);
            Gradient(Area("Mission photo shade",briefing,.032f,.635f,.968f,.830f),Color.clear,new Color(.007f,.018f,.038f,.98f));
            missionName=ChapterHeading("",briefing,.063f,.648f,.939f,.758f,46,White);
            missionBrief=Label("",briefing,.064f,.553f,.940f,.626f,24,White);
            ChapterHeading("ENEMY INTEL",briefing,.064f,.504f,.938f,.551f,21,Amber);
            var portraitFrame=Panel(Area("Enemy portrait plate",briefing,.064f,.400f,.170f,.503f),Midnight,Line,8);
            enemyPortrait=Banner("Enemy portrait",portraitFrame,null,.055f,.055f,.945f,.945f);
            enemyPortraitAspect=enemyPortrait.GetComponent<AspectRatioFitter>();
            enemyIntel=Label("",briefing,.190f,.400f,.936f,.502f,24,White);
            ImageBox("Briefing intel divider",briefing,.064f,.381f,.936f,.383f,Line);
            ChapterHeading("SELECTED UNIT",briefing,.064f,.333f,.938f,.373f,20,Cyan);
            selectedName=ChapterHeading("",briefing,.064f,.276f,.938f,.334f,29,White);
            selectedStats=Label("",briefing,.064f,.226f,.938f,.273f,22,Soft);
            var reward=Panel(Area("Rewards placeholder",briefing,.064f,.139f,.937f,.215f),Midnight,Line,7);
            rewardText=ChapterHeading("VICTORY REWARDS",reward,.036f,.10f,.963f,.90f,19,Cyan);
            deploy=MakeButton("DEPLOY",briefing,.064f,.033f,.937f,.118f,deployStage,30,true);
        }

        public void SetProgress(StoryStage firstStage,StoryStage secondStage)
        {
            int cleared=(firstStage.Cleared?1:0)+(secondStage.Cleared?1:0);
            chapterProgress.text=cleared+"/2 STAGES";
            progressFill.anchorMax=new Vector2(cleared/2f,1);
            stageItems[0].Status.text=firstStage.Cleared?"CLEAR":"READY";
            stageItems[1].Status.text=secondStage.Cleared?"CLEAR":"READY";
        }

        public void BeginDrawerTransition()
        {
            chapterScroll.StopMovement();
            lockedNotice.gameObject.SetActive(false);
            mapInput.interactable=mapInput.blocksRaycasts=false;
            drawerLayer.gameObject.SetActive(true);
        }

        public void SetDrawerProgress(float progress)
        {
            drawerLayer.alpha=progress;
            float distance=storySafeArea.rect.width*.26f*(1-progress);
            drawer.anchoredPosition=new Vector2(-distance,0);
            briefing.anchoredPosition=new Vector2(distance,0);
        }

        public void FinishDrawerTransition(bool open)
        {
            SetDrawerProgress(open?1:0);
            drawerLayer.gameObject.SetActive(open);
            mapInput.interactable=mapInput.blocksRaycasts=!open;
        }

        public void ShowDeployError(string message) => missionBrief.text=message;

        public void ShowMission(StoryStage stage,int index,CharacterCombatProfile profile)
        {
            missionName.text=$"{stage.stageId} / {stage.locationName.ToUpperInvariant()}";
            missionBrief.text=stage.briefing;var prep=GameItems.Find(PlayerProgress.PreparedItem(GundamGame.CombatDemo.PlayerSelectionData.EquippedId));if(prep!=null)missionBrief.text+="\nPREPARED: "+GameItems.Summary(prep);
            rewardText.text=SortieSession.Practice?"FREE PRACTICE / NO REWARDS":"VICTORY  +"+SortieSession.Reward(stage.creditReward).ToString("N0")+" CREDITS  /  +"+SortieSession.Reward(stage.fCoreReward)+" F-CORE";
            missionImage.sprite=stage.thumbnail;
            var enemy=stage.opponents[0];
            enemyPortrait.sprite=enemy.portrait;
            if(enemy.portrait)
                enemyPortraitAspect.aspectRatio=enemy.portrait.rect.width/enemy.portrait.rect.height;
            enemyIntel.text=stage.opponents.Length+"× RX-78-2 GUNDAM / "+
                (enemy.role==EnemyProfile.Role.Ranged?"PART 1 · RANGED":"PART 2 · MELEE");
            selectedName.text=profile?profile.displayName.ToUpperInvariant():"NO GUNDAM SELECTED";
            var id=GundamGame.CombatDemo.PlayerSelectionData.EquippedId;selectedStats.text=profile?$"HP {PlayerProgress.Scaled(id,profile.maxHealth,"hp")}  /  ARM {PlayerProgress.Scaled(id,profile.maxArmor,"armor")}  /  ATK {PlayerProgress.Scaled(id,profile.attack,"attack")}  /  SPD {PlayerProgress.Scaled(id,profile.speed,"speed")}":"";
            deploy.interactable=profile!=null;
            for(int i=0;i<stageItems.Length;i++)
                stageItems[i].SetSelected(i==index);
        }

        sealed class StageItem
        {
            public Button Button { get; }
            public TMP_Text Status { get; }
            readonly LobbyShape tile, icon;
            readonly TMP_Text marker, name;

            public StageItem(Button button,LobbyShape tile,LobbyShape icon,TMP_Text marker,TMP_Text name,TMP_Text status)
            {
                Button=button;
                this.tile=tile;
                this.icon=icon;
                this.marker=marker;
                this.name=name;
                Status=status;
            }

            public void SetSelected(bool selected)
            {
                var surface=(LobbyShape)Button.targetGraphic;
                surface.color=selected?SelectedStageFill:Midnight;
                surface.border=selected?White:Line;
                surface.SetVerticesDirty();
                tile.color=selected?new Color(.43f,.78f,.88f):new Color(.12f,.27f,.38f,.55f);
                icon.color=selected?SelectedStageInk:Cyan;
                marker.color=Status.color=selected?SelectedStageInk:Cyan;
                name.color=selected?SelectedStageInk:White;
            }
        }
    }
}
