using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GundamGame.CombatDemo
{
    /// <summary>Authored HUD prefab. Owns UI selection, never owns combat values.</summary>
    public sealed class MechaBattleHUD : MonoBehaviour
    {
        public RougeHudArt art;
        public RectTransform safeRoot,statusPanel,actionPanel,defensePanel,targetPanel,statsPanel,menuPanel,trackingPanel;
        public Text phaseText,trackingText,healthText,armorText,burstText,targetName,targetHealth,targetArmor,previewText,messageText,defenseText,dodgeText,parryText,statsText;
        public Image healthFill,armorFill,targetHealthFill,targetArmorFill,timingFill;
        public MechaPlateGraphic[] burstCells=new MechaPlateGraphic[5],skillFrames=new MechaPlateGraphic[4];
        public Text[] skillState=new Text[4],turnNames=new Text[5];
        public Image[] turnPortraits=new Image[5];
        public MechaPlateGraphic[] turnFrames=new MechaPlateGraphic[5];
        public Button[] skillButtons=new Button[4],targetButtons=new Button[3];
        public Button executeButton,dodgeButton,parryButton;
        public MechaPlateGraphic executeFrame;
        public RectTransform targetReticle;
        public int SelectedSkill { get; private set; }
        public string PreviewLabel => previewText!=null?previewText.text:"";
        public bool DefenseVisible=>defensePanel!=null && defensePanel.gameObject.activeSelf;
        public bool MenuVisible=>menuPanel!=null && menuPanel.gameObject.activeSelf;
        public bool WorldSelectionBlocked=>MenuVisible || statsPanel!=null && statsPanel.gameObject.activeSelf;
        public int LitBurstCells { get; private set; }
        RougeBattleDemo battle;
        Font font;
        readonly List<DemoFighter> upcoming=new List<DemoFighter>();
        readonly Vector3[] corners=new Vector3[4];
        float refreshAt;
        bool bound;
        Rect? safeOverride;
        static readonly Color White=new Color(.91f,.96f,1),Muted=new Color(.58f,.69f,.77f),Cyan=new Color(.32f,.91f,1),Amber=new Color(1,.66f,.20f),Red=new Color(1,.30f,.39f),Pink=new Color(1,.45f,.65f);
        static readonly string[] Names={"BEAM RIFLE","ANTI-SHIP","OOTORI MISSILES","FULL BURST"};
        static readonly string[] Details={"Single target / Rifle volley","Single target / Blade assault","All enemies / Missile barrage","Primary + splash / Cinematic"};

        public static MechaBattleHUD CreateFallback()
        {
            var go=new GameObject("Mecha Battle HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(MechaBattleHUD));
            var view=go.GetComponent<MechaBattleHUD>();view.BuildHierarchy();return view;
        }
        public void Bind(RougeBattleDemo controller)
        {
            battle=controller;if(bound)return;bound=true;
            if(safeRoot==null)BuildHierarchy();
            for(int i=0;i<4;i++){int slot=i;skillButtons[i].onClick.AddListener(()=>SelectSkill(slot));}
            for(int i=0;i<targetButtons.Length;i++){int index=i;targetButtons[i].onClick.AddListener(()=>{battle.SelectTarget(index);Refresh(true);});}
            executeButton.onClick.AddListener(()=>ExecuteSelected());
            dodgeButton.onClick.AddListener(()=>battle.TryDefense(false));parryButton.onClick.AddListener(()=>battle.TryDefense(true));
            ButtonNamed("System Menu").onClick.AddListener(()=>{menuPanel.gameObject.SetActive(!menuPanel.gameObject.activeSelf);statsPanel.gameObject.SetActive(false);Refresh(true);});
            ButtonNamed("Character Details").onClick.AddListener(()=>{statsPanel.gameObject.SetActive(!statsPanel.gameObject.activeSelf);menuPanel.gameObject.SetActive(false);Refresh(true);});
            ButtonNamed("Reset Battle").onClick.AddListener(()=>{HideWindows();battle.ResetBattle();});
            ButtonNamed("Charge Burst").onClick.AddListener(()=>{HideWindows();battle.ChargeUltimateForTest();SelectSkill(3);});
            ButtonNamed("Virtual Arena").onClick.AddListener(()=>{HideWindows();if(battle.Placement!=null)battle.Placement.UseVirtualMode();});
            ButtonNamed("AR Mode").onClick.AddListener(()=>{HideWindows();if(battle.Placement!=null)battle.Placement.RequestARMode();});
            ButtonNamed("Place Again").onClick.AddListener(()=>{HideWindows();if(battle.Placement!=null)battle.Placement.Reposition();});
            ButtonNamed("Reset Camera").onClick.AddListener(()=>{HideWindows();if(battle.presentation!=null && battle.presentation.arenaCamera!=null)battle.presentation.arenaCamera.ResetView();});
            Refresh(true);
        }
        Button ButtonNamed(string name){foreach(var button in GetComponentsInChildren<Button>(true))if(button.name==name)return button;throw new InvalidOperationException("HUD button missing: "+name);}
        public void ClearSelection(){SelectedSkill=0;HideWindows();Refresh(true);}
        void HideWindows(){if(menuPanel!=null)menuPanel.gameObject.SetActive(false);if(statsPanel!=null)statsPanel.gameObject.SetActive(false);}
        public void SelectSkill(int index)
        {
            if(battle==null || battle.Phase!=RougeBattleDemo.BattlePhase.PlayerTurn || !battle.gameplayEnabled || index<0||index>3)return;
            SelectedSkill=index;HideWindows();Refresh(true);
        }
        public bool ExecuteSelected()
        {
            if(battle==null || !battle.CanUseSkill(SelectedSkill))return false;
            HideWindows();bool accepted=battle.TryUseSkill(SelectedSkill);Refresh(true);return accepted;
        }
        public void SetSafeAreaPreview(Rect? value){safeOverride=value;Refresh(true);}
        void LateUpdate(){UpdateReticle();}
        public void Refresh(bool force=false)
        {
            ApplySafeArea();if(battle==null || battle.player==null)return;
            if(!force && Time.unscaledTime<refreshAt)return;refreshAt=Time.unscaledTime+.06f;
            var own=battle.Phase==RougeBattleDemo.BattlePhase.PlayerTurn && battle.gameplayEnabled;
            var p=battle.player;bool defend=battle.Phase==RougeBattleDemo.BattlePhase.EnemyWindup || battle.Phase==RougeBattleDemo.BattlePhase.EnemyAction;
            actionPanel.gameObject.SetActive(!defend);defensePanel.gameObject.SetActive(defend);
            healthText.text=p.Health+" / "+p.maxHealth;armorText.text=Mathf.CeilToInt(p.Armor)+" / "+p.maxArmor;
            Fill(healthFill,p.Health/(float)Mathf.Max(1,p.maxHealth));Fill(armorFill,p.Armor/Mathf.Max(1,p.maxArmor));
            burstText.text="BURST  "+battle.Burst+" / 5";LitBurstCells=0;
            for(int i=0;i<5;i++)
            {
                bool lit=i<battle.Burst;if(lit)LitBurstCells++;
                var cell=burstCells[i];Color top=lit?new Color(1,.84f,.42f):new Color(.10f,.15f,.19f);
                Color bottom=lit?new Color(.76f,.31f,.04f):new Color(.035f,.065f,.09f);
                if(cell.top!=top||cell.bottom!=bottom){cell.top=top;cell.bottom=bottom;cell.edge=lit?Amber:new Color(.22f,.29f,.34f);cell.SetVerticesDirty();}
            }
            phaseText.text=battle.IsOpening?"DEPLOYING":battle.Phase==RougeBattleDemo.BattlePhase.Victory?"VICTORY":battle.Phase==RougeBattleDemo.BattlePhase.Defeat?"DEFEAT":own?"YOUR TURN":"ENEMY ACTION";
            phaseText.color=own?Cyan:Amber;
            var placement=battle.Placement;
            trackingText.text=placement==null||!placement.IsARMode?"VIRTUAL ARENA / SIMULATION":battle.gameplayEnabled?"AR TRACKING STABLE":"AR TRACKING PAUSED";
            bool stopped=!battle.gameplayEnabled;trackingPanel.gameObject.SetActive(stopped);
            messageText.text=battle.LastMessage;
            for(int i=0;i<4;i++)
            {
                skillButtons[i].interactable=own;
                bool enough=battle.Burst>=RougeBattleDemo.SkillCost(i),selected=i==SelectedSkill;
                skillFrames[i].TintEdge(selected?(i==3&&enough?Amber:White):i==3&&enough?Amber:new Color(.3f,.4f,.48f));
                skillState[i].text=!enough?"NEED "+(RougeBattleDemo.SkillCost(i)-battle.Burst)+" MORE":i==0?"FREE / RECOVER +1":i==3?"READY":"COST "+RougeBattleDemo.SkillCost(i);
                skillState[i].color=enough?(i==3?Amber:Muted):new Color(.68f,.56f,.55f);
            }
            executeButton.interactable=battle.CanUseSkill(SelectedSkill);
            executeFrame.TintEdge(executeButton.interactable?(SelectedSkill==3?Amber:Cyan):new Color(.25f,.32f,.38f));
            string targetDetail=SelectedSkill==2?"ALL ENEMIES":battle.Target!=null?battle.Target.displayName.ToUpperInvariant():"NO TARGET";
            previewText.text=battle.Burst<RougeBattleDemo.SkillCost(SelectedSkill)?"INSUFFICIENT BURST / "+(RougeBattleDemo.SkillCost(SelectedSkill)-battle.Burst)+" CELLS REQUIRED":targetDetail+" / "+Details[SelectedSkill]+"   |   BURST "+battle.Burst+" > "+battle.PreviewBurstAfterTurn(SelectedSkill)+" AFTER TURN";
            var target=battle.Target;
            targetName.text=target!=null?target.displayName.ToUpperInvariant():"NO TARGET";
            targetHealth.text=target!=null?"HP  "+target.Health+" / "+target.maxHealth:"";
            targetArmor.text=target!=null?(target.ArmorBroken?"ARMOR BROKEN":"ARMOR  "+Mathf.CeilToInt(target.Armor)+" / "+target.maxArmor):"";
            Fill(targetHealthFill,target!=null?target.Health/(float)Mathf.Max(1,target.maxHealth):0);Fill(targetArmorFill,target!=null?target.Armor/Mathf.Max(1,target.maxArmor):0);
            for(int i=0;i<targetButtons.Length;i++)
            {
                var enemy=i<battle.enemies.Length?battle.enemies[i]:null;targetButtons[i].interactable=own&&enemy!=null&&enemy.IsAlive;
                targetButtons[i].GetComponent<MechaPlateGraphic>().TintEdge(i==battle.SelectedTarget?Red:new Color(.29f,.36f,.42f));
            }
            if(defend)
            {
                Fill(timingFill,battle.IncomingProgress);
                double remaining=battle.IncomingSecondsRemaining;
                timingFill.color=remaining<=battle.parryWindow?Cyan:Amber;
                defenseText.text=battle.DefenseConsumed?battle.LastMessage:remaining<=battle.parryWindow?"PARRY NOW":remaining<=battle.dodgeWindow?"DODGE NOW / PARRY SOON":"INCOMING / WAIT FOR THE CUE";
                dodgeText.text="DODGE\n"+Mathf.RoundToInt(battle.DodgeChance*100)+"% evade if timed";
                parryText.text="PARRY\nTake "+Mathf.RoundToInt(battle.ParryDamageMultiplier*100)+"% if timed";
                dodgeButton.interactable=parryButton.interactable=battle.Phase==RougeBattleDemo.BattlePhase.EnemyWindup&&!battle.DefenseConsumed;
            }
            if(statsPanel.gameObject.activeSelf)statsText.text="HP       "+p.Health+" / "+p.maxHealth+"\nARMOR    "+Mathf.CeilToInt(p.Armor)+" / "+p.maxArmor+"\nATTACK   "+p.attack+"\nDEF      "+p.defense+"\nSPEED    "+p.speed+"\nBREAK    "+p.breakDamage+"\nBURST    "+battle.Burst+" / 5";
            battle.PreviewTurns(upcoming,5);
            for(int i=0;i<turnNames.Length;i++)
            {
                var actor=i<upcoming.Count?upcoming[i]:null;
                turnFrames[i].gameObject.SetActive(actor!=null);
                if(actor==null)continue;
                turnNames[i].text=(i==0?"NOW":(i+1).ToString("00"))+" / "+(actor==p?"ROUGE":actor.displayName.ToUpperInvariant());
                turnFrames[i].TintEdge(i==0?Cyan:new Color(.33f,.43f,.5f));
                turnPortraits[i].sprite=Portrait(actor);turnPortraits[i].enabled=turnPortraits[i].sprite!=null;
            }
        }
        Sprite Portrait(DemoFighter actor)
        {
            if(art==null)return null;if(actor==battle.player)return art.playerPortrait;
            int index=Array.IndexOf(battle.enemies,actor);return index>=0&&index<art.enemyPortraits.Length?art.enemyPortraits[index]:null;
        }
        void ApplySafeArea()
        {
            if(safeRoot==null||Screen.width<1||Screen.height<1)return;
            Rect r=safeOverride??Screen.safeArea;
            safeRoot.anchorMin=new Vector2(r.xMin/Screen.width,r.yMin/Screen.height);safeRoot.anchorMax=new Vector2(r.xMax/Screen.width,r.yMax/Screen.height);
            safeRoot.offsetMin=safeRoot.offsetMax=Vector2.zero;
        }
        void UpdateReticle()
        {
            if(targetReticle==null||battle==null)return;
            Camera camera=battle.TargetingCamera;
            var actor=battle.Target;bool visible=actor!=null&&camera!=null&&battle.gameplayEnabled;
            if(visible)
            {
                var point=camera.WorldToScreenPoint(actor.AimPoint);visible=point.z>0&&point.x>0&&point.x<Screen.width&&point.y>0&&point.y<Screen.height;
                if(visible){RectTransformUtility.ScreenPointToLocalPointInRectangle(safeRoot,point,null,out Vector2 local);targetReticle.anchoredPosition=local;}
            }
            targetReticle.gameObject.SetActive(visible);
        }
        public void BuildHierarchy()
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas=GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=30;
            var scaler=GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=1;
            safeRoot=Rect("Safe Area",transform,0,0,1,1);
            var logo=Rect("Identity",safeRoot,.025f,.885f,.44f,.985f);
            Label(logo,"AR TRANSFORMER",0,.66f,1,1,25,Muted);
            Label(logo,"GUNDAM",0,.15f,1,.72f,62,White,FontStyle.BoldAndItalic);
            Label(logo,"STRIKE ROUGE / OOTORI",.015f,0,1,.22f,22,Pink);
            var system=Button("System Menu",safeRoot,.81f,.925f,.975f,.976f,"SYSTEM / ARENA",20);system.GetComponent<MechaPlateGraphic>().edge=Cyan;
            trackingText=Label(safeRoot,"VIRTUAL ARENA / SIMULATION",.65f,.88f,.975f,.92f,19,Cyan,FontStyle.Normal,TextAnchor.MiddleRight);
            phaseText=Label(safeRoot,"YOUR TURN",.13f,.82f,.46f,.87f,27,Cyan,FontStyle.Bold);
            targetPanel=Plate("Target Readout",safeRoot,.72f,.67f,.975f,.86f,Red);
            targetName=Label(targetPanel,"RX-78-2",.055f,.70f,.92f,.96f,30,White,FontStyle.Bold);
            targetHealth=Label(targetPanel,"HP",.055f,.46f,.94f,.69f,21,White);
            targetHealthFill=Bar(targetPanel,"Target HP",.055f,.39f,.94f,.44f,Red);
            targetArmor=Label(targetPanel,"ARMOR",.055f,.13f,.94f,.36f,20,Cyan);
            targetArmorFill=Bar(targetPanel,"Target Armor",.055f,.07f,.94f,.12f,Cyan);
            var targets=Rect("Target Selectors",safeRoot,.72f,.605f,.975f,.66f);
            for(int i=0;i<3;i++)targetButtons[i]=Button("Target "+i,targets,i/3f+.008f,0,(i+1)/3f-.008f,1,new[]{"RX-78-2","JUSTICE","EXTREME"}[i],18);
            var timeline=Rect("Action Order",safeRoot,.025f,.355f,.112f,.825f);
            for(int i=0;i<5;i++)
            {
                var tile=Plate("Turn "+i,timeline,0,1-(i+1)*.198f,1,1-i*.198f-.009f,i==0?Cyan:Muted);
                turnFrames[i]=tile.GetComponent<MechaPlateGraphic>();
                turnPortraits[i]=Picture(tile,art!=null?art.playerPortrait:null,.025f,.21f,.975f,.97f);
                turnNames[i]=Label(tile,"NOW / ROUGE",.06f,.02f,.97f,.22f,14,White,FontStyle.Bold);
            }
            statusPanel=Plate("Character Status",safeRoot,.025f,.025f,.375f,.295f,Muted);
            var detail=Button("Character Details",statusPanel,.025f,.30f,.28f,.96f,"",20);
            Picture(detail.transform,art!=null?art.playerPortrait:null,.02f,.02f,.98f,.98f);
            Label(statusPanel,"MBF-02",.31f,.79f,.96f,.95f,21,Muted);
            Label(statusPanel,"STRIKE ROUGE",.31f,.64f,.96f,.82f,30,White,FontStyle.Bold);
            Label(statusPanel,"HP",.31f,.49f,.42f,.64f,19,White);
            healthText=Label(statusPanel,"",.44f,.49f,.95f,.64f,25,White,FontStyle.Bold);
            healthFill=Bar(statusPanel,"Player HP",.31f,.445f,.95f,.485f,White);
            Label(statusPanel,"ARMOR",.31f,.30f,.49f,.445f,19,Cyan);
            armorText=Label(statusPanel,"",.51f,.30f,.95f,.445f,23,Cyan,FontStyle.Bold);
            armorFill=Bar(statusPanel,"Player Armor",.31f,.255f,.95f,.295f,Cyan);
            var capacitors=Rect("Five Burst Cells",statusPanel,.04f,.055f,.64f,.20f);
            for(int i=0;i<5;i++){var cell=Plate("Burst Cell "+i,capacitors,i*.2f+.012f,0,(i+1)*.2f-.012f,1,Amber).GetComponent<MechaPlateGraphic>();cell.cut=9;cell.edgeWidth=2;burstCells[i]=cell;}
            burstText=Label(statusPanel,"BURST 2 / 5",.68f,.055f,.98f,.20f,25,Amber,FontStyle.Bold);
            actionPanel=Rect("Weapon Dock",safeRoot,.48f,.025f,.975f,.305f);
            for(int i=0;i<4;i++)
            {
                var button=Button("Skill "+i,actionPanel,i*.25f+.006f,.33f,(i+1)*.25f-.006f,.98f,"",20);
                skillButtons[i]=button;skillFrames[i]=button.GetComponent<MechaPlateGraphic>();skillFrames[i].cut=19;
                Picture(button.transform,art!=null&&art.weaponIcons.Length>i?art.weaponIcons[i]:null,.06f,.30f,.94f,.94f);
                var skillName=Label(button.transform,Names[i],.07f,.12f,.73f,.32f,i==2?18:20,White,FontStyle.Bold);skillName.name="Skill Name";
                var cost=Plate("Cost",button.transform,.77f,.08f,.96f,.30f,i==3?Amber:Cyan);cost.GetComponent<MechaPlateGraphic>().cut=10;
                Label(cost,RougeBattleDemo.SkillCost(i).ToString(),.05f,.01f,.95f,.98f,27,i==3?Amber:White,FontStyle.Bold,TextAnchor.MiddleCenter);
                skillState[i]=Label(button.transform,"",.08f,.015f,.76f,.13f,13,Muted);
            }
            executeButton=Button("Execute",actionPanel,.17f,.025f,.88f,.26f,"EXECUTE",36);executeFrame=executeButton.GetComponent<MechaPlateGraphic>();executeFrame.cut=26;executeFrame.edgeWidth=3;
            previewText=Label(safeRoot,"",.46f,.31f,.975f,.355f,19,White,FontStyle.Normal,TextAnchor.MiddleRight);
            messageText=Label(safeRoot,"",.14f,.355f,.68f,.395f,18,Muted);
            defensePanel=Plate("Defense Dock",safeRoot,.48f,.025f,.975f,.305f,Amber);
            defenseText=Label(defensePanel,"INCOMING",.05f,.76f,.95f,.96f,23,White,FontStyle.Bold);
            timingFill=Bar(defensePanel,"Timing",.05f,.65f,.95f,.70f,Amber);
            var zone=Rect("Parry Timing Window",timingFill.transform.parent,.85f,.64f,.95f,.71f);var zoneImage=zone.gameObject.AddComponent<Image>();zoneImage.color=new Color(.3f,1,1,.3f);zoneImage.raycastTarget=false;
            dodgeButton=Button("Dodge",defensePanel,.04f,.13f,.49f,.58f,"",24);dodgeText=Label(dodgeButton.transform,"DODGE",.06f,.08f,.94f,.94f,24,White,FontStyle.Bold,TextAnchor.MiddleCenter);
            parryButton=Button("Parry",defensePanel,.51f,.13f,.96f,.58f,"",24);parryText=Label(parryButton.transform,"PARRY",.06f,.08f,.94f,.94f,24,White,FontStyle.Bold,TextAnchor.MiddleCenter);
            Label(defensePanel,"ONE REACTION PER ATTACK / Q PARRY / E DODGE",.05f,.02f,.95f,.12f,14,Muted);
            targetReticle=Rect("Target Reticle",safeRoot,.5f,.5f,.5f,.5f);targetReticle.sizeDelta=new Vector2(130,150);targetReticle.pivot=new Vector2(.5f,.5f);
            for(int i=0;i<4;i++){float x=i%2==0?0:.86f,y=i<2?0:.98f;Flat(targetReticle,"Corner H",x,y,x+.14f,y+.02f,Red);float vx=i%2==0?0:.98f,vy=i<2?0:.86f;Flat(targetReticle,"Corner V",vx,vy,vx+.02f,vy+.14f,Red);}
            menuPanel=Plate("System Window",safeRoot,.57f,.40f,.94f,.87f,Muted);
            Label(menuPanel,"COMBAT LAB / SYSTEM",.07f,.87f,.93f,.97f,28,White,FontStyle.Bold);
            string[] menuNames={"Reset Battle","Charge Burst","Virtual Arena","AR Mode","Place Again","Reset Camera"};
            string[] menuLabels={"RESET BATTLE","TEST / BURST 5 CELLS","VIRTUAL ARENA","SWITCH TO AR","PLACE ARENA AGAIN","RESET CAMERA"};
            for(int i=0;i<menuNames.Length;i++)Button(menuNames[i],menuPanel,.07f,.74f-i*.12f,.93f,.84f-i*.12f,menuLabels[i],22);
            statsPanel=Plate("Attribute Window",safeRoot,.14f,.40f,.43f,.85f,Cyan);Label(statsPanel,"CHARACTER ATTRIBUTES",.07f,.86f,.95f,.98f,24,Cyan,FontStyle.Bold);statsText=Label(statsPanel,"",.08f,.06f,.95f,.84f,27,White);
            trackingPanel=Plate("Tracking Paused",safeRoot,.28f,.43f,.72f,.62f,Amber);Label(trackingPanel,"BATTLE PAUSED",.05f,.5f,.95f,.92f,34,Amber,FontStyle.Bold,TextAnchor.MiddleCenter);Label(trackingPanel,"Find the original surface to resume",.05f,.1f,.95f,.5f,23,White,FontStyle.Normal,TextAnchor.MiddleCenter);
            menuPanel.gameObject.SetActive(false);statsPanel.gameObject.SetActive(false);trackingPanel.gameObject.SetActive(false);defensePanel.gameObject.SetActive(false);
        }
        static RectTransform Rect(string name,Transform parent,float x0,float y0,float x1,float y1)
        {var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        static RectTransform Plate(string name,Transform parent,float x0,float y0,float x1,float y1,Color edge)
        {var r=Rect(name,parent,x0,y0,x1,y1);var g=r.gameObject.AddComponent<MechaPlateGraphic>();g.edge=edge;g.raycastTarget=false;return r;}
        Text Label(Transform parent,string value,float x0,float y0,float x1,float y1,int size,Color color,FontStyle style=FontStyle.Normal,TextAnchor align=TextAnchor.MiddleLeft)
        {var r=Rect("Text "+value,parent,x0,y0,x1,y1);var t=r.gameObject.AddComponent<Text>();t.font=font;t.fontSize=size;t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(12,Mathf.RoundToInt(size*.72f));t.resizeTextMaxSize=size;t.color=color;t.text=value;t.fontStyle=style;t.alignment=align;t.raycastTarget=false;t.supportRichText=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
        Button Button(string name,Transform parent,float x0,float y0,float x1,float y1,string caption,int size)
        {var r=Plate(name,parent,x0,y0,x1,y1,Muted);var b=r.gameObject.AddComponent<Button>();var g=r.GetComponent<MechaPlateGraphic>();g.raycastTarget=true;b.targetGraphic=g;var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.18f,1.18f,1.18f);colors.pressedColor=new Color(.72f,.86f,.94f);colors.disabledColor=new Color(.48f,.52f,.55f);b.colors=colors;if(caption!="")Label(r,caption,.04f,.04f,.96f,.96f,size,White,FontStyle.Bold,TextAnchor.MiddleCenter);return b;}
        static Image Picture(Transform parent,Sprite sprite,float x0,float y0,float x1,float y1)
        {var r=Rect("Artwork",parent,x0,y0,x1,y1);var image=r.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;image.enabled=sprite!=null;return image;}
        static Image Flat(Transform parent,string name,float x0,float y0,float x1,float y1,Color color)
        {var r=Rect(name,parent,x0,y0,x1,y1);var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
        static Image Bar(Transform parent,string name,float x0,float y0,float x1,float y1,Color color)
        {var r=Rect(name,parent,x0,y0,x1,y1);Flat(r,"Track",0,0,1,1,new Color(.19f,.25f,.29f));return Flat(r,"Fill",0,0,1,1,color);}
        static void Fill(Image image,float value){if(image!=null)image.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(value),1);}
    }
}
