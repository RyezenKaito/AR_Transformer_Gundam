using System.Collections.Generic;
using GundamGame.CombatDemo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GundamGame.Unicorn
{
    internal sealed class UnicornBattleHud : MonoBehaviour
    {
        static readonly Color Cyan = new(.32f,.91f,1f);
        static readonly Color Rose = new(1f,.29f,.42f);
        static readonly Color Amber = new(1f,.69f,.24f);
        static readonly Color White = new(.93f,.97f,1f);
        static readonly Color Muted = new(.62f,.72f,.79f);
        static readonly Color Border = new(.23f,.34f,.42f);
        static readonly Color[] SkillColors = { Cyan, Rose, new(.61f,.80f,1f), Amber };
        static readonly string[] Names = { "TWIN VULCAN", "PSYCHO-FRAME", "BEAM MAGNUM", "FULL BURST" };
        static readonly string[] Details =
        {
            "Twin Vulcan / Single target", "Psycho-Frame Awakening / Self buff",
            "Beam Magnum / Single target", "Melee combo + Ultra Beam / Single target"
        };

        readonly List<DemoFighter> order = new(5);
        readonly List<RaycastResult> pointerHits = new();
        readonly Text[] names = new Text[4];
        readonly Text[] costs = new Text[4];
        readonly Image[] skillImages = new Image[4];
        readonly MechaPlateGraphic[] costFrames = new MechaPlateGraphic[4];
        UnicornCombatDemo battle;
        MechaBattleHUD view;
        Text pauseLabel;
        bool visible;
        float refreshAt;

        public static UnicornBattleHud Create(UnicornCombatDemo battle,MechaBattleHUD prefab,RougeHudArt art)
        {
            var view=prefab?Instantiate(prefab,battle.transform):MechaBattleHUD.CreateFallback();
            view.transform.SetParent(battle.transform,false);
            view.name="Unicorn Mecha HUD";
            view.enabled=false;
            var hud=view.gameObject.AddComponent<UnicornBattleHud>();
            hud.battle=battle;
            hud.view=view;
            hud.Bind(art);
            return hud;
        }

        void Bind(RougeHudArt art)
        {
            if(EventSystem.current==null && FindAnyObjectByType<EventSystem>()==null)
            {
                var eventSystem=new GameObject("Unicorn EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
                eventSystem.transform.SetParent(transform,false);
                eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            view.art=art;
            LayoutMenu();
            foreach(var label in view.GetComponentsInChildren<Text>(true))
            {
                if(label.text=="STRIKE ROUGE / OOTORI")label.text="RX-0 UNICORN / DESTROY MODE";
                else if(label.text=="STRIKE ROUGE")label.text="UNICORN GUNDAM";
                else if(label.text=="MBF-02")label.text="RX-0";
            }
            var detail=ButtonNamed("Character Details");
            Outfit(detail.GetComponentInChildren<Image>(true),art?art.playerPortrait:null);
            detail.onClick.AddListener(()=>{Toggle(view.statsPanel,view.menuPanel);Refresh();});
            var systemButton = ButtonNamed("System Menu");
            if (battle.PauseRequested != null)
            {
                systemButton.GetComponentInChildren<Text>(true).text = "PAUSE";
                systemButton.onClick.AddListener(() => battle.PauseRequested());
            }
            else systemButton.onClick.AddListener(()=>{Toggle(view.menuPanel,view.statsPanel);Refresh();});
            MenuAction("Reset Battle",()=>battle.ResetBattle());
            MenuAction("Charge Burst",()=>{battle.ChargeForTest();battle.SelectSkill(3);});
            var pause=ButtonNamed("Virtual Arena");
            pauseLabel=pause.GetComponentInChildren<Text>(true);
            MenuAction("Virtual Arena",()=>battle.SetPaused(!battle.IsPaused));
            ButtonNamed("AR Mode").gameObject.SetActive(false);
            ButtonNamed("Place Again").gameObject.SetActive(false);
            MenuAction("Reset Camera",battle.ResetCameraView);

            for(int i=0;i<view.skillButtons.Length;i++)
            {
                int slot=i;
                var skill=view.skillButtons[i];
                names[i]=skill.transform.Find("Skill Name").GetComponent<Text>();
                names[i].text=Names[i];
                names[i].rectTransform.anchorMin=new Vector2(.072f,.127f);
                names[i].rectTransform.anchorMax=new Vector2(.77f,.32f);
                var cost=skill.transform.Find("Cost");
                costs[i]=cost.GetComponentInChildren<Text>(true);
                costs[i].text=battle.GetSkill(i).cost.ToString();
                costFrames[i]=cost.GetComponent<MechaPlateGraphic>();
                skillImages[i]=skill.transform.Find("Artwork").GetComponent<Image>();
                Outfit(skillImages[i],art&&i<art.weaponIcons.Length?art.weaponIcons[i]:null);
                skillImages[i].rectTransform.anchorMin=new Vector2(.075f,.34f);
                skillImages[i].rectTransform.anchorMax=new Vector2(.925f,.955f);
                skill.onClick.AddListener(()=>{battle.SelectSkill(slot);Refresh();});
            }
            for(int i=0;i<view.targetButtons.Length;i++)
            {
                int index=i;
                view.targetButtons[i].onClick.AddListener(()=>{battle.SelectTarget(index);Refresh();});
            }
            view.executeButton.onClick.AddListener(()=>
            {
                battle.ExecuteSkill();
                SyncVisibility();
                if(visible)Refresh();
            });
            view.parryButton.onClick.AddListener(()=>{battle.Defend(true);Refresh();});
            view.dodgeButton.onClick.AddListener(()=>{battle.Defend(false);Refresh();});
            Refresh();
        }

        void LayoutMenu()
        {
            string[] keys={"Reset Battle","Charge Burst","Virtual Arena","Reset Camera"};
            for(int i=0;i<keys.Length;i++)
            {
                var row=(RectTransform)ButtonNamed(keys[i]).transform;
                row.anchorMin=new Vector2(.07f,.69f-i*.18f);
                row.anchorMax=new Vector2(.93f,.82f-i*.18f);
            }
        }

        Button ButtonNamed(string key) => System.Array.Find(view.GetComponentsInChildren<Button>(true),button=>button.name==key);
        void MenuAction(string name,System.Action action) => ButtonNamed(name).onClick.AddListener(()=>
        {
            view.menuPanel.gameObject.SetActive(false);
            action();
            Refresh();
        });
        static void Toggle(RectTransform shown,RectTransform hidden)
        {
            hidden.gameObject.SetActive(false);
            shown.gameObject.SetActive(!shown.gameObject.activeSelf);
        }

        public bool BlocksWorldPointer(Vector2 position)
        {
            if(view.WorldSelectionBlocked)return true;
            if(EventSystem.current==null)return false;
            pointerHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=position},pointerHits);
            return pointerHits.Exists(hit=>hit.gameObject.transform.IsChildOf(transform));
        }

        void LateUpdate()
        {
            SyncVisibility();
            if(visible)UpdateReticle();
        }

        void SyncVisibility()
        {
            bool display=battle.HasStarted && battle.showHud && !battle.Cinematic;
            bool changed=visible!=display;
            visible=display;
            if(view.safeRoot.gameObject.activeSelf!=visible)view.safeRoot.gameObject.SetActive(visible);
            if(changed)
            {
                view.menuPanel.gameObject.SetActive(false);
                view.statsPanel.gameObject.SetActive(false);
            }
            if(!visible)return;
            if(changed || Time.unscaledTime>=refreshAt)Refresh();
        }

        void Refresh()
        {
            refreshAt=Time.unscaledTime+.05f;
            view.SetSafeAreaPreview(null);
            bool own=battle.CanChooseAction;
            bool incoming=battle.State==UnicornCombatDemo.Phase.Windup || battle.State==UnicornCombatDemo.Phase.EnemyRecovery;
            var p=battle.player;
            var target=battle.Target;
            int selected=battle.SelectedSkill;
            int burst=battle.Burst;
            view.phaseText.text=battle.IsPaused?"PAUSED":battle.State switch
            {
                UnicornCombatDemo.Phase.Deploying=>"DEPLOYING",
                UnicornCombatDemo.Phase.Select=>"YOUR TURN",
                UnicornCombatDemo.Phase.Skill=>"UNICORN ACTION",
                UnicornCombatDemo.Phase.Victory=>"VICTORY",
                UnicornCombatDemo.Phase.Defeat=>"DEFEAT",
                _=>"ENEMY ACTION"
            };
            view.phaseText.color=own?Cyan:Amber;
            view.trackingText.text=battle.PauseRequested!=null?"STORY COMBAT":
                battle.arPresentation?(battle.TrackingPaused?"AR TRACKING PAUSED":"AR TRACKING"):"VIRTUAL ARENA / SIMULATION";
            view.trackingPanel.gameObject.SetActive(battle.TrackingPaused);
            pauseLabel.text=battle.IsPaused?"RESUME BATTLE":"PAUSE BATTLE";
            view.messageText.text=battle.BuffTurns>0?$"{battle.Notice}\n{AwakeningBonuses}  ·  {battle.BuffTurns} TURN(S)":battle.Notice;
            view.healthText.text=$"{p.Health} / {p.maxHealth}";
            view.armorText.text=$"{Mathf.CeilToInt(p.Armor)} / {p.maxArmor}";
            view.burstText.text=$"BURST  {burst} / 5";
            Fill(view.healthFill,p.Health,p.maxHealth);
            Fill(view.armorFill,p.Armor,p.maxArmor);
            for(int i=0;i<view.burstCells.Length;i++)
            {
                bool lit=i<burst;
                Tint(view.burstCells[i],lit?new Color(1,.84f,.42f):new Color(.10f,.15f,.19f),
                    lit?new Color(.76f,.31f,.04f):new Color(.035f,.065f,.09f),lit?Amber:Border);
            }
            view.targetName.text=target.displayName;
            view.targetHealth.text=$"HP  {target.Health} / {target.maxHealth}";
            view.targetArmor.text=target.ArmorBroken?"ARMOR BROKEN":$"ARMOR  {Mathf.CeilToInt(target.Armor)} / {target.maxArmor}";
            Fill(view.targetHealthFill,target.Health,target.maxHealth);
            Fill(view.targetArmorFill,target.Armor,target.maxArmor);
            for(int i=0;i<view.targetButtons.Length;i++)
            {
                view.targetButtons[i].gameObject.SetActive(i<battle.enemies.Length);
                if(i>=battle.enemies.Length)continue;
                view.targetButtons[i].GetComponentInChildren<Text>().text=battle.enemies[i].displayName;
                view.targetButtons[i].interactable=own && battle.enemies[i].IsAlive;
                view.targetButtons[i].GetComponent<MechaPlateGraphic>().TintEdge(i==battle.TargetIndex?Rose:Border);
            }
            RefreshSkills(own,incoming,selected,burst);
            RefreshTurns();
            if(view.statsPanel.gameObject.activeSelf)
                view.statsText.text=$"HP       {p.Health} / {p.maxHealth}\nARMOR    {Mathf.CeilToInt(p.Armor)} / {p.maxArmor}\nATTACK   {battle.EffectiveAttack}\nDEF      {battle.EffectiveDefense}\nSPEED    {p.speed}\nBREAK    {p.breakDamage}\nBURST    {burst} / 5";
            if(incoming)RefreshDefense();
        }

        void RefreshSkills(bool own,bool incoming,int selected,int burst)
        {
            view.actionPanel.gameObject.SetActive(!incoming);
            view.previewText.enabled=!incoming;
            view.defensePanel.gameObject.SetActive(incoming);
            for(int i=0;i<view.skillButtons.Length;i++)
            {
                int missing=battle.GetSkill(i).cost-burst;
                bool enough=missing<=0, active=i==selected, charged=i==3 && enough;
                Color accent=SkillColors[i];
                view.skillButtons[i].interactable=own;
                Color edge=active?accent:charged?Amber:Border;
                Color top=new(.105f,.155f,.20f,.98f);
                Color bottom=new(.024f,.036f,.054f,.99f);
                Tint(view.skillFrames[i],Color.Lerp(top,accent,active?.18f:charged?.11f:0f),bottom,edge);
                if(costFrames[i])costFrames[i].TintEdge(enough?accent:Border);
                if(costs[i])costs[i].color=enough?accent:Muted;
                names[i].color=enough?White:Muted;
                skillImages[i].color=enough?Color.white:new Color(.61f,.68f,.75f,.63f);
                view.skillState[i].text=missing>0?$"NEED {missing} BURST":i==0?"FREE / RECOVER +1":
                    i==1?"SELF BUFF":charged?"READY":"SINGLE TARGET";
                view.skillState[i].color=enough&&(i==1||charged)?accent:Muted;
            }
            view.executeButton.interactable=battle.CanExecuteSelected;
            view.executeFrame.TintEdge(battle.CanExecuteSelected?SkillColors[selected]:Border);
            int cost=battle.GetSkill(selected).cost;
            int next=Mathf.Min(5,burst-cost+(selected==0?2:1));
            string target=selected==1?"UNICORN":battle.Target.displayName;
            string detail=selected==1?$"Awakening / {AwakeningBonuses}":Details[selected];
            view.previewText.text=burst<cost?$"INSUFFICIENT BURST / {cost-burst} REQUIRED":
                $"{target} / {detail}   |   BURST {burst} → {next}";
        }

        string AwakeningBonuses => $"ATK +{battle.buffSettings.attackBonus:P0} / DEF +{battle.buffSettings.defenseBonus:P0}";

        void RefreshTurns()
        {
            battle.PreviewTurns(order,view.turnNames.Length);
            for(int i=0;i<view.turnNames.Length;i++)
            {
                bool occupied=i<order.Count;
                view.turnFrames[i].gameObject.SetActive(occupied);
                if(!occupied)continue;
                var actor=order[i];
                view.turnNames[i].text=$"{(i==0?"NOW":(i+1).ToString("00"))} / {actor.displayName.ToUpperInvariant()}";
                view.turnFrames[i].TintEdge(i==0?Cyan:Border);
                if(actor.TryGetComponent<EnemyBrain>(out var brain) && brain.profile.portrait)
                    Outfit(view.turnPortraits[i],brain.profile.portrait);
                else if(view.art)
                {
                    int enemy=System.Array.IndexOf(battle.enemies,actor);
                    Outfit(view.turnPortraits[i],actor==battle.player?view.art.playerPortrait:
                        enemy>=0&&enemy<view.art.enemyPortraits.Length?view.art.enemyPortraits[enemy]:null);
                }
            }
        }

        void RefreshDefense()
        {
            bool react=battle.CanDefend;
            float remaining=battle.DefenseRemaining;
            Fill(view.timingFill,react?battle.DefenseProgress:1,1);
            view.timingFill.color=remaining<=.2f?Cyan:Amber;
            view.defenseText.text=!react?battle.Notice:remaining<=.2f?"PARRY NOW":
                remaining<=.4f?"DODGE NOW / PARRY SOON":"INCOMING / WAIT FOR THE CUE";
            var attacker=battle.Acting;
            view.dodgeText.text=attacker&&battle.player.speed>attacker.speed?"DODGE\n100% evade if timed":"DODGE\n50% evade if timed";
            view.parryText.text=attacker&&RougeBattleDemo.ParryMultiplier(battle.EffectiveDefense,attacker.attack)==0?
                "PARRY\nBlock if timed":"PARRY\nTake 50% if timed";
            view.dodgeButton.interactable=view.parryButton.interactable=react;
        }

        void UpdateReticle()
        {
            bool active=battle.SelectedSkill!=1 && battle.Target.IsAlive && !battle.IsPaused;
            if(active)
            {
                var point=battle.battleCamera.WorldToScreenPoint(battle.Target.AimPoint);
                active=point.z>0 && Screen.safeArea.Contains(point);
                if(active && RectTransformUtility.ScreenPointToLocalPointInRectangle(view.safeRoot,point,null,out var local))
                    view.targetReticle.anchoredPosition=local;
            }
            view.targetReticle.gameObject.SetActive(active);
        }

        static void Outfit(Image image,Sprite sprite)
        {
            image.sprite=sprite;
            image.enabled=sprite!=null;
        }
        static void Fill(Image image,float value,float max) => image.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(value/Mathf.Max(1,max)),1);
        static void Tint(MechaPlateGraphic plate,Color top,Color bottom,Color edge)
        {
            if(plate.top==top && plate.bottom==bottom && plate.edge==edge)return;
            plate.top=top;
            plate.bottom=bottom;
            plate.edge=edge;
            plate.SetVerticesDirty();
        }
    }
}
