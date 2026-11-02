using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Events;
using static TurnBasedBattle.Editor.BattleAssetUtility;

namespace TurnBasedBattle.Editor
{
    public static class BattleHudBuilder
    {
        static Font font;
        static Color ink=new Color(.025f,.055f,.078f,.96f),cyan=new Color(.2f,.86f,.95f),gold=new Color(1,.78f,.28f),muted=new Color(.48f,.61f,.69f);
        static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 low=default,Vector2 high=default)
        {
            var g=new GameObject(name,typeof(RectTransform));var r=(RectTransform)g.transform;r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=low;r.offsetMax=high;return r;
        }
        static RectTransform Box(string name,Transform parent,float x,float y,float width,float height)
        {
            var r=Rect(name,parent,Vector2.zero,Vector2.zero);r.pivot=Vector2.zero;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(width,height);return r;
        }
        static Image Panel(RectTransform r,Color c){var image=r.gameObject.AddComponent<Image>();image.color=c;return image;}
        static Text Label(string name,Transform parent,string text,int size,Color color,Vector2 min,Vector2 max,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var r=Rect(name,parent,min,max,new Vector2(12,0),new Vector2(-12,0));var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=text;t.fontSize=size;t.color=color;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
        }
        static Button Button(string name,Transform parent,string label,float x,float y,float w,float h,Color accent,UnityAction click,int fontSize=24)
        {
            var r=Box(name,parent,x,y,w,h);var image=Panel(r,new Color(.08f,.14f,.19f,.98f));var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;
            var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.3f,1.3f,1.3f,1);colors.pressedColor=new Color(.7f,.9f,1,1);colors.disabledColor=new Color(.5f,.5f,.5f,.5f);b.colors=colors;
            var stripe=Rect("Accent",r,new Vector2(0,0),new Vector2(0,1),Vector2.zero,new Vector2(4,0));Panel(stripe,accent).raycastTarget=false;
            Label("Label",r,label,fontSize,accent,Vector2.zero,Vector2.one,TextAnchor.MiddleCenter);b.onClick.AddListener(click);return b;
        }
        static Image Bar(string name,Transform parent,Vector2 min,Vector2 max,Color color)
        {
            var background=Rect(name,parent,min,max,new Vector2(16,0),new Vector2(-16,0));Panel(background,new Color(.10f,.17f,.21f));
            var fill=Rect("Fill",background,Vector2.zero,Vector2.one);var image=Panel(fill,color);image.type=Image.Type.Filled;image.fillMethod=Image.FillMethod.Horizontal;image.fillOrigin=0;image.raycastTarget=false;return image;
        }
        public static BattleHUD Build(TurnBattleManager battle)
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject=new GameObject("Turn Battle HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=0;
            var root=(RectTransform)canvasObject.transform;var hud=canvasObject.AddComponent<BattleHUD>();hud.battle=battle;
            var events=new GameObject("Battle EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var header=Rect("Header",root,new Vector2(0,.875f),Vector2.one);Panel(header,new Color(.02f,.04f,.065f,.91f));
            Label("Brand",header,"MECHA / DUEL",34,Color.white,new Vector2(.025f,.35f),new Vector2(.30f,.94f));
            Label("Subtitle",header,"TACTICAL COMBAT  /  EARTH PRISON",15,muted,new Vector2(.025f,.08f),new Vector2(.35f,.40f));
            hud.phaseText=Label("Phase",header,"LƯỢT CỦA BẠN",28,cyan,new Vector2(.34f,.40f),new Vector2(.72f,.90f),TextAnchor.MiddleCenter);
            hud.roundText=Label("Round",header,"ROUND 01",16,muted,new Vector2(.34f,.10f),new Vector2(.72f,.40f),TextAnchor.MiddleCenter);
            Button("Debug",header,"F1 · TÙY CHỌN",1450,38,215,56,muted,()=>hud.ToggleDebug(),19);
            Button("Pause",header,"P · PAUSE",1690,38,170,56,muted,()=>battle.TogglePause(),19);
            var player=Rect("PlayerCard",root,new Vector2(.025f,.715f),new Vector2(.25f,.855f));Panel(player,ink);
            Label("Name",player,"RX-0  /  UNICORN",25,Color.white,new Vector2(.02f,.66f),new Vector2(.98f,.97f));
            hud.playerHPText=Label("HP",player,"1000 / 1000",17,muted,new Vector2(.02f,.44f),new Vector2(.98f,.68f));
            hud.playerHPFill=Bar("Health",player,new Vector2(0,.32f),new Vector2(1,.40f),cyan);
            hud.ultimateText=Label("UltimateLabel",player,"ULTIMATE  0 / 100",15,gold,new Vector2(.02f,.09f),new Vector2(.98f,.30f));
            hud.ultimateFill=Bar("UltimateGauge",player,new Vector2(0,.04f),new Vector2(1,.08f),gold);
            hud.enemyButtons=new Button[3];hud.enemyNames=new Text[3];hud.enemyHPTexts=new Text[3];hud.enemyHPFills=new Image[3];hud.enemyBorders=new Image[3];
            for(int i=0;i<3;i++)
            {
                int index=i;var card=Rect("EnemyCard_"+i,root,new Vector2(.755f,.768f-i*.09f),new Vector2(.975f,.855f-i*.09f));hud.enemyBorders[i]=Panel(card,new Color(.16f,.27f,.34f));
                var inset=Rect("Inset",card,Vector2.zero,Vector2.one,new Vector2(2,2),new Vector2(-2,-2));Panel(inset,ink).raycastTarget=false;
                var b=card.gameObject.AddComponent<Button>();b.targetGraphic=hud.enemyBorders[i];b.onClick.AddListener(()=>battle.SelectTarget(index));hud.enemyButtons[i]=b;
                hud.enemyNames[i]=Label("Name",card,"SENTINEL",20,Color.white,new Vector2(.02f,.54f),new Vector2(.98f,.95f));
                hud.enemyHPTexts[i]=Label("HP",card,"600 / 600",15,muted,new Vector2(.02f,.23f),new Vector2(.98f,.55f));
                hud.enemyHPFills[i]=Bar("Health",card,new Vector2(0,.10f),new Vector2(1,.18f),new Color(1,.43f,.24f));
            }
            var bottom=Rect("CommandBar",root,Vector2.zero,new Vector2(1,.17f));Panel(bottom,ink);hud.commandPanel=bottom.gameObject;
            hud.selectionText=Label("Selection",bottom,"",18,muted,new Vector2(.03f,.74f),new Vector2(.97f,.98f));
            hud.attackButton=Button("Attack",bottom,"1  TẤN CÔNG\nÁp sát chém kiếm",60,28,258,96,cyan,()=>battle.Select(BattleActionKind.Attack));
            hud.skillButton=Button("Skill",bottom,"2  KỸ NĂNG\nPháo năng lượng",334,28,258,96,new Color(.65f,.65f,1),()=>battle.Select(BattleActionKind.Skill));
            hud.guardButton=Button("Guard",bottom,"3  PHÒNG THỦ\nGiảm 50% sát thương",608,28,270,96,muted,()=>battle.Select(BattleActionKind.Guard));
            hud.ultimateButton=Button("Ultimate",bottom,"R  EARTH PRISON\nULTIMATE",894,28,330,96,gold,()=>battle.Select(BattleActionKind.EarthPrison));
            hud.confirmButton=Button("Confirm",bottom,"XÁC NHẬN  ↵",1280,28,560,96,Color.white,()=>battle.Confirm());hud.confirmText=hud.confirmButton.GetComponentInChildren<Text>();
            var action=Rect("ActionStatus",root,new Vector2(.55f,.185f),new Vector2(.975f,.245f));
            hud.actionText=Label("ActionName",action,"",20,Color.white,new Vector2(0,.25f),Vector2.one,TextAnchor.MiddleCenter);
            hud.actionProgress=Bar("Progress",action,Vector2.zero,new Vector2(1,.05f),gold);
            hud.messageText=Label("Message",root,"",23,Color.white,new Vector2(.24f,.18f),new Vector2(.76f,.235f),TextAnchor.MiddleCenter);

            var cue=Rect("DodgeCue",root,new Vector2(.30f,.255f),new Vector2(.70f,.405f));Panel(cue,new Color(.02f,.045f,.065f,.97f));hud.cuePanel=cue.gameObject;
            hud.cueTitle=Label("CueTitle",cue,"SPACE  ·  NÉ ĐÚNG NHỊP",27,Color.white,new Vector2(.02f,.65f),new Vector2(.98f,.98f),TextAnchor.MiddleCenter);
            hud.cueHint=Label("CueHint",cue,"Bấm một lần khi vạch vào vùng vàng",18,muted,new Vector2(.02f,.05f),new Vector2(.98f,.30f),TextAnchor.MiddleCenter);
            hud.cueTrack=Rect("TimingTrack",cue,new Vector2(.065f,.36f),new Vector2(.935f,.57f));Panel(hud.cueTrack,new Color(.10f,.18f,.22f));
            var fill=Rect("TimingFill",hud.cueTrack,Vector2.zero,Vector2.one);hud.cueProgress=Panel(fill,new Color(.10f,.40f,.49f));hud.cueProgress.type=Image.Type.Filled;hud.cueProgress.fillMethod=Image.FillMethod.Horizontal;
            var win=Rect("SuccessWindow",hud.cueTrack,new Vector2(.72f,0),new Vector2(.95f,1));hud.cueWindow=Panel(win,new Color(1,.78f,.25f,.85f));
            var cursor=Rect("InputCursor",hud.cueTrack,new Vector2(0,.5f),new Vector2(0,.5f));cursor.sizeDelta=new Vector2(5,46);hud.cueCursor=Panel(cursor,Color.white);cue.gameObject.SetActive(false);
            var dodge=Button("TouchDodge",cue,"NÉ",0,0,100,50,gold,()=>battle.DodgeNow(),24);
            var dodgeRect=(RectTransform)dodge.transform;dodgeRect.anchorMin=new Vector2(1,.5f);dodgeRect.anchorMax=new Vector2(1,.5f);dodgeRect.pivot=new Vector2(0,.5f);dodgeRect.anchoredPosition=new Vector2(12,0);dodgeRect.sizeDelta=new Vector2(110,90);
            var cameraHint=Label("CameraHint",root,"GIỮ CHUỘT PHẢI / VUỐT: XOAY 360°   ·   LĂN CHUỘT / CHỤM: ZOOM   ·   HOME: GÓC SAU",14,muted,new Vector2(.025f,0),new Vector2(.98f,.025f),TextAnchor.MiddleCenter);
            cameraHint.raycastTarget=false;

            var debug=Rect("DebugPanel",root,new Vector2(.66f,.43f),new Vector2(.975f,.74f));Panel(debug,ink);hud.debugPanel=debug.gameObject;
            Label("DebugTitle",debug,"PHÒNG THỬ / DEBUG",23,gold,new Vector2(.02f,.83f),new Vector2(.98f,.99f));
            hud.debugText=Label("DebugDetails",debug,"",17,muted,new Vector2(.02f,.58f),new Vector2(.98f,.83f));
            hud.speedText=Label("Speed",debug,"",17,Color.white,new Vector2(.02f,.48f),new Vector2(.98f,.58f));
            Button("FillUlt",debug,"Nạp Ultimate",20,96,177,48,gold,()=>battle.FillUltimate(),17);
            Button("OneEnemy",debug,"Trận 1v1",210,96,177,48,cyan,()=>battle.Restart(1),17);
            Button("ThreeEnemies",debug,"Trận 1v3",400,96,177,48,cyan,()=>battle.Restart(3),17);
            Button("HalfSpeed",debug,"0,5×",20,40,100,42,muted,()=>battle.SetPlaybackSpeed(.5f),17);
            Button("NormalSpeed",debug,"1×",130,40,100,42,muted,()=>battle.SetPlaybackSpeed(1),17);
            Button("DoubleSpeed",debug,"2×",240,40,100,42,muted,()=>battle.SetPlaybackSpeed(2),17);
            Button("EasyDodge",debug,"Đổi mức né",352,40,225,42,muted,()=>battle.ToggleEasy(),17);debug.gameObject.SetActive(false);

            var pause=Rect("PauseOverlay",root,Vector2.zero,Vector2.one);Panel(pause,new Color(.01f,.025f,.04f,.92f));hud.pausePanel=pause.gameObject;
            Label("Title",pause,"TẠM DỪNG",48,Color.white,new Vector2(.2f,.55f),new Vector2(.8f,.68f),TextAnchor.MiddleCenter);
            Button("Resume",pause,"TIẾP TỤC  /  P",710,420,500,86,cyan,()=>battle.TogglePause(),28);pause.gameObject.SetActive(false);
            var result=Rect("ResultOverlay",root,Vector2.zero,Vector2.one);Panel(result,new Color(.01f,.025f,.04f,.91f));hud.resultPanel=result.gameObject;
            hud.resultTitle=Label("ResultTitle",result,"CHIẾN THẮNG",52,gold,new Vector2(.2f,.56f),new Vector2(.8f,.72f),TextAnchor.MiddleCenter);
            Label("ResultSubtitle",result,"Giữ đội hình. Nắm thời điểm. Làm chủ trận đấu.",25,muted,new Vector2(.2f,.46f),new Vector2(.8f,.55f),TextAnchor.MiddleCenter);
            Button("Retry",result,"CHƠI LẠI 1v1",570,340,360,85,cyan,()=>battle.Restart(1),25);Button("Retry3",result,"THỬ THÁCH 1v3",990,340,360,85,gold,()=>battle.Restart(3),25);result.gameObject.SetActive(false);
            hud.floatingRoot=Rect("FloatingResults",root,Vector2.zero,Vector2.one);hud.floatingTexts=new Text[10];
            for(int i=0;i<10;i++){var r=Rect("HitText_"+i,hud.floatingRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f));r.sizeDelta=new Vector2(230,70);var t=r.gameObject.AddComponent<Text>();t.font=font;t.fontSize=42;t.fontStyle=FontStyle.Bold;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;hud.floatingTexts[i]=t;r.gameObject.SetActive(false);}
            return hud;
        }
    }
}
