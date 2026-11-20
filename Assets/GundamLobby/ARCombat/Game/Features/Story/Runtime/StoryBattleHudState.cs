using System;
using System.Collections.Generic;
using GundamGame.CombatDemo;
using GundamGame.Unicorn;
using UnityEngine;

namespace GundamGame.Story
{
    internal sealed class StoryBattleHudState
    {
        static readonly float[] UnicornComboShares={.12f,.12f,.22f,.54f};
        readonly CharacterBattlePackage package;
        readonly CharacterCombatProfile profile;
        readonly List<int> targetOrder=new(3);
        UnicornCombatDemo Unicorn => package.unicorn;
        RougeBattleDemo Rouge => package.rouge;

        public StoryBattleHudState(CharacterBattlePackage package,CharacterCombatProfile profile)
        { this.package=package; this.profile=profile; }

        public bool Visible => !Paused && !package.Cinematic && !package.Victory && !package.Defeat;
        public bool Paused => Unicorn ? Unicorn.IsPaused : !Rouge.gameplayEnabled;
        public bool PlayerTurn => Visible && (Unicorn ? Unicorn.HasStarted && Unicorn.CanChooseAction :
            Rouge.Phase==RougeBattleDemo.BattlePhase.PlayerTurn);
        public bool Incoming => Visible && (Unicorn ? Unicorn.IsIncomingAttack :
            Rouge.Phase==RougeBattleDemo.BattlePhase.EnemyWindup && Rouge.IncomingCanDefend);
        public bool CanDefend => Incoming && (Unicorn ? Unicorn.CanDefend : !Rouge.DefenseConsumed);
        public DemoFighter Player => package.player;
        public string CallSign => Unicorn?"UNICORN":"STRIKE ROUGE";
        public DemoFighter Actor => Unicorn ? Unicorn.Acting : Rouge.CurrentActor;
        public DemoFighter Target => Unicorn ? Unicorn.Target : Rouge.Target;
        public DemoFighter[] Opponents => package.Opponents;
        public Camera Camera => package.battleCamera;
        public RougeHudArt Art => Unicorn ? Unicorn.hudArt : Rouge.hudPrefab ? Rouge.hudPrefab.art : null;
        public int Burst => Unicorn ? Unicorn.Burst : Rouge.Burst;
        public float Duration => Unicorn ? Unicorn.WindupDuration : Rouge.IncomingDuration;
        public float Remaining => Unicorn ? Unicorn.DefenseRemaining : (float)Rouge.IncomingSecondsRemaining;
        public float DodgeWindow => Unicorn ? UnicornCombatDemo.DodgeWindow : Rouge.dodgeWindow;
        public float ParryWindow => Unicorn ? UnicornCombatDemo.ParryWindow : Rouge.parryWindow;
        public string AttackName => Unicorn ? Unicorn.IncomingMove : Rouge.IncomingMove;
        int Attack => Unicorn ? Unicorn.EffectiveAttack : Player.attack;
        int Defense => Unicorn ? Unicorn.EffectiveDefense : Player.defense;
        public bool GuaranteedEvade => Actor && Player.speed>Actor.speed;
        public bool FullBlock => Actor && RougeBattleDemo.ParryMultiplier(Defense,Actor.attack)==0;

        public void Subscribe(Action<BattleReactionResult> callback)
        { if(Unicorn)Unicorn.ReactionResolved+=callback; else Rouge.ReactionResolved+=callback; }

        public void Unsubscribe(Action<BattleReactionResult> callback)
        { if(Unicorn)Unicorn.ReactionResolved-=callback; else if(Rouge)Rouge.ReactionResolved-=callback; }

        public void PreviewTurns(List<DemoFighter> turns)
        { if(Unicorn)Unicorn.PreviewTurns(turns,5); else Rouge.PreviewTurns(turns,5); }

        public string SkillName(int slot) => profile.skills[slot].displayName;
        public int Cost(int slot) => profile.skills[slot].burstCost;
        public int BurstAfterTurn(int slot) => Mathf.Min(CharacterCombatProfile.MaximumBurst,
            Burst-Cost(slot)+(slot==0?1:0)+CharacterCombatProfile.TurnEndBurst);
        public bool IsBuff(int slot) => Unicorn && slot==1;
        public bool IsArea(int slot) => !Unicorn && slot==2;
        public string SkillScope(int slot) => IsBuff(slot)?"SELF BUFF":IsArea(slot)?"ALL ENEMIES":
            !Unicorn && slot==3?"TARGET + SPLASH":"SINGLE TARGET";

        public bool Cast(int slot)
        {
            if(!PlayerTurn || Burst<Cost(slot))return false;
            return Unicorn ? Unicorn.SelectSkill(slot)&&Unicorn.ExecuteSkill() : Rouge.TryUseSkill(slot);
        }

        public void Defend(bool parry)
        {
            if(!CanDefend)return;
            if(Unicorn)Unicorn.Defend(parry);
            else Rouge.TryDefense(parry);
        }

        public bool PickTarget(Vector2 screenPoint)
        {
            if(!PlayerTurn)return false;
            int index=BattleTargeting.Pick(Camera,screenPoint,Opponents);
            if(index<0)return false;
            SelectTarget(index);
            return true;
        }

        public void CycleTarget(int direction)
        {
            if(!PlayerTurn)return;
            targetOrder.Clear();
            for(int i=0;i<Opponents.Length;i++)
                if(Opponents[i] && Opponents[i].IsAlive)targetOrder.Add(i);
            if(targetOrder.Count==0)return;
            targetOrder.Sort(CompareOnScreen);
            int current=Array.IndexOf(Opponents,Target);
            int position=targetOrder.IndexOf(current);
            int next=position<0?0:(position+direction+targetOrder.Count)%targetOrder.Count;
            SelectTarget(targetOrder[next]);
        }

        int CompareOnScreen(int first,int second)
        {
            float left=Camera.WorldToViewportPoint(Opponents[first].AimPoint).x;
            float right=Camera.WorldToViewportPoint(Opponents[second].AimPoint).x;
            int order=left.CompareTo(right);
            return order!=0?order:first.CompareTo(second);
        }

        void SelectTarget(int index)
        { if(Unicorn)Unicorn.SelectTarget(index); else Rouge.SelectTarget(index); }

        public int EstimateHealthDamage(int slot,DemoFighter target,bool splash=false)
        {
            if(!target || IsBuff(slot))return 0;
            var skill=profile.skills[slot];
            float guard=1;
            if(target.TryGetComponent<EnemyBrain>(out var brain) && brain.Guarding &&
                Vector3.Dot(target.transform.forward,(Player.transform.position-target.transform.position).normalized)>.25f)
                guard=.42f;
            float raw=Attack*skill.damageMultiplier*100f/(100f+target.defense);
            float armorDamage=(Attack*.5f+Player.breakDamage*.5f)*skill.armorMultiplier*guard;
            float armor=target.Armor;
            int remaining=target.Health;
            int hits=Unicorn ? slot==3 ? UnicornComboShares.Length : 1 : splash ? 1 : slot==1 ? 2 : slot==3 ? 5 : 3;
            for(int hit=0;hit<hits && remaining>0;hit++)
            {
                int floor=Unicorn && slot==3 && hit<hits-1?1:0;
                if(remaining<=floor)continue;
                float share=Unicorn ? slot==3?UnicornComboShares[hit]:1 : RougeShare(slot,hit,hits,splash);
                if(share<=0)continue;
                float multiplier=guard*(armor>.001f?.5f:1);
                int hp=Unicorn ? Mathf.Max(1,Mathf.RoundToInt(raw*share*multiplier)) :
                    Mathf.RoundToInt(Mathf.Max(1,Mathf.RoundToInt(raw*share))*multiplier);
                remaining=Mathf.Max(floor,remaining-hp);
                armor=Mathf.Max(0,armor-armorDamage*share);
            }
            return target.Health-remaining;
        }

        float RougeShare(int slot,int hit,int hits,bool splash)
        {
            int total=Rouge.GetSkillDamageTotal(slot);
            if(total<=0)return 0;
            if(splash)return Rouge.ultimateSplashDamage/(float)total;
            int available=total;
            if(slot==3)
            {
                int beam=total/3;
                if(hit==0)return beam/(float)total;
                available-=beam;
                hit--;hits--;
            }
            int piece=available/hits;
            return (hit==hits-1?available-piece*(hits-1):piece)/(float)total;
        }
    }
}
