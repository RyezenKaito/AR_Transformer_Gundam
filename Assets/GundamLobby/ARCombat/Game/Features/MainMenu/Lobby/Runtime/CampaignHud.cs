using System;
using GundamGame.CombatDemo;
using GundamGame.Story;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GundamLobby
{
    internal sealed partial class CampaignHud
    {
        static readonly Color Deep = new(.025f,.047f,.074f);
        static readonly Color Glass = new(.045f,.094f,.137f,.96f);
        static readonly Color Line = new(.18f,.39f,.48f);
        static readonly Color Cyan = new(.37f,.88f,1);
        static readonly Color White = new(.93f,.97f,1);
        static readonly Color Soft = new(.59f,.74f,.80f);
        static readonly Color Amber = new(1,.73f,.33f);
        readonly TMP_FontAsset font, displayFont;
        readonly CanvasGroup input;
        readonly Image curtain;
        readonly RectTransform mode, modeSafeArea, modeContent, storyCampaign, loading;
        readonly RectTransform[] radarSweeps = new RectTransform[2];
        readonly TMP_Text loadText;
        readonly Button story;

        public CampaignHud(Transform parent, TMP_FontAsset font, TMP_FontAsset displayFont, Sprite backdrop, Sprite spaceBackdrop,
            GameObject rosterCanvas, StoryStage firstStage, StoryStage secondStage, Action back, Action openStory,
            Action openChapter, Action<int> selectStage, Action deployStage)
        {
            this.font=font;
            this.displayFont=displayFont;
            Root=Area("Campaign UI",parent,0,0,1,1);
            input=Root.gameObject.AddComponent<CanvasGroup>();
            curtain=ImageBox("Transition curtain",Root,0,0,1,1,Deep);
            curtain.raycastTarget=true;
            mode=FadingPage("Battle selection",Root);
            modeSafeArea=Area("Battle safe area",mode,0,0,1,1);
            var modeFrame=Area("Hangar framing",modeSafeArea,0,0,1,1);
            var frameAspect=modeFrame.gameObject.AddComponent<AspectRatioFitter>();
            frameAspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
            frameAspect.aspectRatio=16f/9f;
            modeContent=Area("Battle HUD",modeFrame,0,0,1,1);
            BuildBattleHeader(modeFrame,modeContent,backdrop,back);
            story=BuildModeCards(modeContent,firstStage.thumbnail,openStory,!backdrop);

            storyCampaign=FadingPage("Story Campaign",Root);
            BuildStoryCampaign(storyCampaign,spaceBackdrop,rosterCanvas,firstStage,secondStage,
                back,openChapter,selectStage,deployStage);

            loading=FadingPage("Loading stage",Root);
            ImageBox("Loading backdrop",loading,0,0,1,1,Deep);
            loadText=Label("",loading,.18f,.42f,.82f,.59f,38,White,TextAlignmentOptions.Center);
            SetVisible(LobbyStoryFlow.Page.Closed);
        }

        public RectTransform Root { get; }

        public void SetVisible(LobbyStoryFlow.Page page)
        {
            Root.gameObject.SetActive(page!=LobbyStoryFlow.Page.Closed);
            mode.gameObject.SetActive(page==LobbyStoryFlow.Page.Modes);
            storyCampaign.gameObject.SetActive(page==LobbyStoryFlow.Page.Chapters||page==LobbyStoryFlow.Page.Stages);
            loading.gameObject.SetActive(page==LobbyStoryFlow.Page.Loading);
        }

        public void SetTransition(LobbyStoryFlow.Page page,float alpha,float offset)
        {
            var view=page switch
            {
                LobbyStoryFlow.Page.Modes => mode,
                LobbyStoryFlow.Page.Chapters or LobbyStoryFlow.Page.Stages => storyCampaign,
                _ => loading
            };
            view.GetComponent<CanvasGroup>().alpha=alpha;
            var content=view==mode?modeContent:view==storyCampaign?storySafeArea:view;
            content.anchoredPosition=new Vector2(offset,0);
        }

        public void SetCurtain(float alpha)
        {
            var color=Deep;
            color.a=alpha;
            curtain.color=color;
        }

        public void SetInput(bool enabled) => input.interactable=enabled;

        public void Focus(LobbyStoryFlow.Page page)
        {
            var focused=page==LobbyStoryFlow.Page.Modes?story:page==LobbyStoryFlow.Page.Chapters?chapterOne:
                stageItems[0].Button;
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(focused.gameObject);
        }

        public void SafeArea()
        {
            if(Screen.width<1||Screen.height<1)return;
            var safe=Screen.safeArea;
            var min=new Vector2(safe.xMin/Screen.width,safe.yMin/Screen.height);
            var max=new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height);
            storySafeArea.anchorMin=modeSafeArea.anchorMin=min;
            storySafeArea.anchorMax=modeSafeArea.anchorMax=max;
        }

        public void TickRadar()
        {
            if(lockedNotice.gameObject.activeSelf && Time.unscaledTime>=lockedNoticeUntil)
                lockedNotice.gameObject.SetActive(false);
            if(!mode.gameObject.activeInHierarchy)return;
            float phase=-Mathf.Repeat(Time.unscaledTime*42f,360f);
            for(int i=0;i<radarSweeps.Length;i++)radarSweeps[i].localRotation=Quaternion.Euler(0,0,phase+110f*i);
        }

        public void ShowLoading(string location) => loadText.text = "Loading\n" + location;

        void BuildBattleHeader(RectTransform frame,RectTransform content,Sprite backdrop,Action back)
        {
            if(backdrop)
            {
                Picture("Mecha repair hangar",frame,backdrop,0,0,1,1,White).transform.SetAsFirstSibling();
            }
            else
            {
                var border=Panel(Area("HUD frame",content,.016f,.024f,.984f,.976f),Glass,Line,30);
                Panel(Area("Hangar scanning grid",border,0,0,1,1),Line,Color.clear,0)
                    .GetComponent<LobbyShape>().geometry=LobbyShape.Geometry.Grid;
            }

            var heading=ChapterHeading("BATTLE",content,.048f,.843f,.436f,.963f,113,new Color(.52f,1,1));
            heading.characterSpacing=4;
            var subtitle=Label("OP: CAMPAIGN LOG // STAGE-01",content,.05f,.804f,.47f,.853f,27,Cyan);
            subtitle.characterSpacing=1.6f;
            ImageBox("Heading accent",content,.035f,.818f,.038f,.954f,new Color(.32f,.94f,.99f,.75f));
            var backButton=MakeButton("ESC // BACK",content,.838f,.876f,.962f,.939f,back,28,true);
            var backSurface=(LobbyShape)backButton.targetGraphic;
            backSurface.color=new Color(.63f,.89f,.91f,.98f);
            backSurface.gradient=backSurface.gradientVertical=true;
            backSurface.gradientEnd=new Color(.30f,.65f,.72f,.98f);
            var backLabel=backButton.GetComponentInChildren<TMP_Text>();
            backLabel.color=Deep;
            backLabel.font=displayFont?displayFont:font;
            backLabel.fontStyle=FontStyles.Bold;
            Panel(Area("Back glow",backButton.transform,-.027f,-.10f,1.027f,1.10f),Color.clear,
                new Color(.4f,.91f,1,.28f),19);

            radarSweeps[0]=RadarSweep(content,.041f,.053f,.111f,.184f);
            radarSweeps[1]=RadarSweep(content,.884f,.049f,.962f,.190f);
        }

        static RectTransform RadarSweep(Transform parent,float x0,float y0,float x1,float y1)
        {
            var radar=Area("Radar sweep",parent,x0,y0,x1,y1);
            var ring=Panel(Area("Sweep ring",radar,0,0,1,1),Color.clear,new Color(.34f,.91f,1,.21f),0);
            ring.GetComponent<LobbyShape>().geometry=LobbyShape.Geometry.Ring;
            var arm=ImageBox("Sweep signal",radar,.5f,.494f,.5f,.506f,new Color(.44f,.97f,1,.47f)).rectTransform;
            arm.pivot=new Vector2(0,.5f);
            arm.sizeDelta=new Vector2(71,0);
            return arm;
        }

        Button BuildModeCards(RectTransform parent,Sprite preview,Action openStory,bool drawBrackets)
        {
            if(drawBrackets)
            {
                var storySlot=Panel(Area("Story scanning brackets",parent,.216f,.255f,.417f,.713f),new Color(.40f,.94f,1,.72f),Color.clear,27);
                var rankSlot=Panel(Area("Rank scanning brackets",parent,.609f,.255f,.784f,.713f),new Color(.60f,.73f,.76f,.51f),Color.clear,27);
                storySlot.GetComponent<LobbyShape>().geometry=LobbyShape.Geometry.Corners;
                rankSlot.GetComponent<LobbyShape>().geometry=LobbyShape.Geometry.Corners;
            }
            var playable=MakeButton("",parent,.231f,.276f,.402f,.691f,openStory,28,true);
            playable.name="STORY";
            var storyCard=playable.transform;
            var surface=(LobbyShape)playable.targetGraphic;
            surface.cut=30;
            surface.color=new Color(.018f,.12f,.17f,.98f);
            surface.gradient=true;
            surface.gradientVertical=true;
            surface.gradientEnd=new Color(.026f,.20f,.25f,.99f);
            Panel(Area("Story cyan glow",storyCard,-.026f,-.018f,1.026f,1.018f),Color.clear,
                new Color(.20f,.93f,1,.22f),34).GetComponent<LobbyShape>().thickness=11;
            Panel(Area("Story inner frame",storyCard,.024f,.018f,.976f,.979f),Color.clear,
                new Color(.39f,.94f,1,.62f),25);
            Banner("Battlefield thumbnail",storyCard,preview,.054f,.391f,.946f,.947f);
            Panel(Area("Preview edge",storyCard,.054f,.391f,.946f,.947f),Color.clear,Line,0);
            ChapterHeading("STORY",storyCard,.06f,.205f,.94f,.388f,64,White,TextAlignmentOptions.Center);
            var description=Label("Explore the story and conquer each outpost",storyCard,.075f,.065f,.925f,.21f,26,White,TextAlignmentOptions.Center);
            description.fontSizeMin=12;

            var rank=Panel(Area("RANK",parent,.614f,.276f,.779f,.691f),new Color(.04f,.055f,.07f,.98f),
                new Color(.27f,.34f,.38f),32);
            Panel(Area("Rank inner frame",rank,.026f,.018f,.974f,.979f),Color.clear,new Color(.24f,.31f,.35f,.75f),26);
            DrawLock(rank,.32f,.52f,.68f,.82f,Soft);
            var ribbon=ImageBox("Rank status ribbon",rank,-.025f,.408f,1.025f,.518f,Amber);
            ribbon.rectTransform.localRotation=Quaternion.Euler(0,0,5f);
            var status=Label("IN DEVELOPMENT",ribbon.transform,.023f,0,.977f,1,19,Deep,TextAlignmentOptions.Center);
            status.fontStyle=FontStyles.Bold;
            status.fontSizeMin=11;
            ChapterHeading("RANK",rank,.05f,.205f,.95f,.388f,61,new Color(.53f,.59f,.63f),TextAlignmentOptions.Center);
            Label("Ranked battles",rank,.06f,.075f,.94f,.20f,25,new Color(.58f,.65f,.68f),TextAlignmentOptions.Center);
            return playable;
        }

        static void DrawLock(Transform parent,float x0,float y0,float x1,float y1,Color tint)
        {
            var lockArea=Area("Padlock",parent,x0,y0,x1,y1);
            var shackle=Panel(Area("Shackle",lockArea,.15f,.30f,.85f,.99f),Color.clear,tint,0);
            var outline=shackle.GetComponent<LobbyShape>();
            outline.geometry=LobbyShape.Geometry.Ring;
            outline.thickness=6f;
            Panel(Area("Padlock body",lockArea,.07f,.06f,.93f,.57f),tint,White,8);
            var keyhole=Panel(Area("Keyhole",lockArea,.41f,.22f,.59f,.44f),Deep,Deep,0);
            keyhole.GetComponent<LobbyShape>().geometry=LobbyShape.Geometry.Circle;
            ImageBox("Keyway",lockArea,.469f,.17f,.531f,.34f,Deep);
        }

        readonly TMP_Text[] economyLabels=new TMP_Text[2];
        int economyVersion=-1;
        void Header(RectTransform parent,GameObject rosterCanvas,string heading,Action back)
        {
            var bar=Panel(Area("Campaign header",parent,0,.926f,1,1),new Color(.025f,.045f,.066f),Line,0);
            MakeButton("ESC // BACK",bar,.015f,.12f,.147f,.88f,back,24,false);
            ChapterHeading(heading,bar,.168f,.12f,.675f,.88f,30,White);
            var names=new[]{"CREDITS","CRYSTALS"};
            for(int i=0;i<2;i++)
            {
                float x=.78f+i*.105f;
                var icon=Panel(Area(names[i]+" icon",bar,x,.30f,x+.010f,.68f),i==0?Amber:Cyan,Color.clear,0);
                icon.GetComponent<LobbyShape>().geometry=LobbyShape.Geometry.Hexagon;
                Label(names[i],bar,x+.014f,.56f,x+.071f,.94f,11,Soft);
                economyLabels[i]=Label("",bar,x+.014f,.08f,x+.071f,.61f,21,White);
            }
            RefreshWallet();
        }
        public void RefreshWallet()
        {
            if(economyVersion==PlayerProgress.Version)return;
            economyVersion=PlayerProgress.Version;
            string[] amounts={PlayerProgress.Credits.ToString("N0"),PlayerProgress.Crystals.ToString("N0")};
            for(int i=0;i<2;i++)if(economyLabels[i])economyLabels[i].text=amounts[i];
        }

        static RectTransform FadingPage(string name,Transform parent)
        {
            var view=Area(name,parent,0,0,1,1);
            view.gameObject.AddComponent<CanvasGroup>().alpha=0;
            return view;
        }

        static RectTransform Area(string name,Transform parent,float x0,float y0,float x1,float y1)
        {
            var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;
            rect.SetParent(parent,false);
            rect.anchorMin=new Vector2(x0,y0);
            rect.anchorMax=new Vector2(x1,y1);
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            return rect;
        }

        static RectTransform Panel(RectTransform rect,Color fill,Color border,float cut=20)
        {
            var shape=rect.gameObject.AddComponent<LobbyShape>();
            shape.color=fill;shape.border=border;shape.cut=cut;shape.thickness=1.5f;
            shape.raycastTarget=false;
            return rect;
        }

        static void Gradient(RectTransform rect,Color top,Color bottom)
        {
            var shade=Panel(rect,top,Color.clear,0).GetComponent<LobbyShape>();
            shade.gradient=shade.gradientVertical=true;
            shade.gradientEnd=bottom;
        }

        static Image ImageBox(string name,Transform parent,float x0,float y0,float x1,float y1,Color fill)
        {
            var image=Area(name,parent,x0,y0,x1,y1).gameObject.AddComponent<Image>();
            image.color=fill;image.raycastTarget=false;
            return image;
        }

        static Image Picture(string name,Transform parent,Sprite sprite,float x0,float y0,float x1,float y1,Color color)
        {
            var image=ImageBox(name,parent,x0,y0,x1,y1,color);
            image.sprite=sprite;
            return image;
        }

        static Image Banner(string name,Transform parent,Sprite sprite,float x0,float y0,float x1,float y1)
        {
            var frame=Area(name+" frame",parent,x0,y0,x1,y1);
            frame.gameObject.AddComponent<RectMask2D>();
            var image=Picture(name,frame,sprite,0,0,1,1,White);
            var aspect=image.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
            aspect.aspectRatio=16f/9f;
            return image;
        }

        TMP_Text Label(string value,Transform parent,float x0,float y0,float x1,float y1,float size,Color color,
            TextAlignmentOptions alignment=TextAlignmentOptions.MidlineLeft)
        {
            var label=Area("Label",parent,x0,y0,x1,y1).gameObject.AddComponent<TextMeshProUGUI>();
            label.font=font;label.fontSize=size;label.enableAutoSizing=true;label.fontSizeMax=size;label.fontSizeMin=size*.67f;
            label.alignment=alignment;label.text=value;label.color=color;label.raycastTarget=false;
            return label;
        }

        TMP_Text ChapterHeading(string value,Transform parent,float x0,float y0,float x1,float y1,float size,Color color,
            TextAlignmentOptions alignment=TextAlignmentOptions.MidlineLeft)
        {
            var label=Label(value,parent,x0,y0,x1,y1,size,color,alignment);
            label.font=displayFont?displayFont:font;
            label.fontStyle=FontStyles.Bold;
            label.textWrappingMode=TextWrappingModes.NoWrap;
            label.fontSizeMin=12;
            return label;
        }

        Button MakeButton(string value,Transform parent,float x0,float y0,float x1,float y1,Action action,int size,bool emphasized)
        {
            var rect=Panel(Area(value,parent,x0,y0,x1,y1),emphasized?new Color(.075f,.24f,.32f):Glass,
                emphasized?Cyan:Line,16);
            var graphic=rect.GetComponent<LobbyShape>();
            graphic.raycastTarget=true;
            var button=rect.gameObject.AddComponent<Button>();
            button.targetGraphic=graphic;
            var colors=button.colors;
            colors.highlightedColor=new Color(.82f,1,1);
            colors.selectedColor=colors.highlightedColor;
            colors.colorMultiplier=1.15f;
            button.colors=colors;
            button.onClick.AddListener(()=>action());
            var nav=button.navigation;nav.mode=Navigation.Mode.None;button.navigation=nav;
            if(value.Length>0)Label(value,rect,.045f,.10f,.955f,.90f,size,White,TextAlignmentOptions.Center);
            rect.gameObject.AddComponent<CampaignCardMotion>().surface=graphic;
            return button;
        }
    }
}
