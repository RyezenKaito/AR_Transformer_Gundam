using System;
using GundamGame.CombatDemo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GundamGame.Story.StoryHudUi;

namespace GundamGame.Story
{
    public sealed partial class StoryBattleHud
    {
        readonly TimelineCard[] timeline=new TimelineCard[5];
        readonly SkillCard[] skills=new SkillCard[4];
        readonly MechaPlateGraphic[] burstCells=new MechaPlateGraphic[CharacterCombatProfile.MaximumBurst];
        readonly Color[] skillColors={Cyan,new(.57f,.76f,1),new(.77f,.54f,1),Amber};
        RectTransform skillsPanel;
        MechaPlateGraphic burstFrame;
        TMP_Text turnLabel, healthText, armorText, burstText, burstReadyText, fullLabel, previewText, inputHint;
        Image healthFill, armorFill;
        StoryHudGraphic fullBurstHalo;
        float glow;

        void BuildTimeline()
        {
            var panel=Area(safeRoot,"Action Order",.022f,.518f,.176f,.972f);
            Image(panel,"Timeline guide",.026f,.025f,.033f,.881f,new Color(.28f,.59f,.72f,.52f));
            turnLabel=Text(panel,"Turn Phase","",displayFont,.043f,.914f,.985f,1,32,Cyan,bold:true);
            for(int i=0;i<timeline.Length;i++)
            {
                float top=.888f-.17f*i;
                var plate=Plate(panel,i==0?"Current Turn Card":$"Upcoming Turn {i+1:00}",.072f,top-.143f,.934f,top,Line,11);
                var number=Text(plate.transform,"Order Position",i==0?"NOW":$"{i+1:00}",displayFont,
                    .058f,.11f,.245f,.91f,24,Muted,bold:true);
                var portrait=Art(plate.transform,"Actor portrait",.255f,.05f,.493f,.95f,null);
                var name=Text(plate.transform,"Actor call sign","",displayFont,.505f,.08f,.942f,.92f,21,White,bold:true);
                var arrow=Mark(plate.transform,"Current Turn Arrow",-.126f,.31f,-.024f,.69f,
                    StoryHudGraphic.Mark.Arrow,Cyan,Color.clear);
                arrow.gameObject.SetActive(i==0);
                timeline[i]=new TimelineCard(plate,number,portrait,name,arrow);
            }
        }

        void BuildStageIdentity()
        {
            var operation=Plate(safeRoot,"Stage Identity",.727f,.919f,.885f,.973f,Line,12);
            Text(operation.transform,"Stage Name",stage.stageId+" // "+stage.locationName.ToUpperInvariant(),displayFont,
                .07f,.05f,.94f,.95f,24,White,TextAlignmentOptions.Center,true);
            var button=Plate(safeRoot,"PAUSE",.897f,.919f,.978f,.973f,Cyan,12);
            Button(button,()=>pause?.Invoke());
            Text(button.transform,"Pause Text","PAUSE",displayFont,.06f,.05f,.94f,.95f,25,Cyan,TextAlignmentOptions.Center,true);
        }

        void BuildTelemetry(CharacterCombatProfile profile)
        {
            var panel=Plate(safeRoot,"Cockpit Telemetry",.023f,.036f,.388f,.265f,Line,25);
            panel.rectTransform.localEulerAngles=new Vector3(0,0,-1.4f);
            panel.raycastTarget=true;
            Image(panel.transform,"Telemetry accent",.026f,.890f,.040f,.957f,Cyan);
            var portrait=Plate(panel.transform,"Pilot portrait plate",.037f,.272f,.265f,.908f,Line,18);
            Art(portrait.transform,"Player portrait",.045f,.02f,.955f,.98f,battle.Art?battle.Art.playerPortrait:null);
            Text(panel.transform,"Gundam name",profile.displayName.ToUpperInvariant(),displayFont,
                .297f,.704f,.843f,.945f,40,White,bold:true);
            string badge=profile.gradeBadge;
            int start=string.IsNullOrEmpty(badge)?-1:badge.LastIndexOf('[');
            if(start>=0)
            {
                var grade=Plate(panel.transform,"Gundam Grade",.846f,.785f,.958f,.928f,Cyan,7);
                Text(grade.transform,"Grade",badge.Substring(start),displayFont,.06f,.04f,.94f,.96f,24,Cyan,
                    TextAlignmentOptions.Center,true);
            }
            Text(panel.transform,"HP Label","HP",displayFont,.298f,.591f,.435f,.723f,23,Green,bold:true);
            healthText=Text(panel.transform,"Player HP amount","",displayFont,.460f,.591f,.956f,.725f,26,White,
                TextAlignmentOptions.MidlineRight,true);
            healthFill=Meter(panel.transform,"Player HP",.298f,.548f,.958f,.589f,Green);
            armorText=Text(panel.transform,"Player Armor amount","",displayFont,.460f,.353f,.956f,.482f,25,Cyan,
                TextAlignmentOptions.MidlineRight,true);
            Text(panel.transform,"Armor Label","ARMOR",displayFont,.298f,.353f,.454f,.482f,22,Cyan,bold:true);
            armorFill=Meter(panel.transform,"Player Armor",.298f,.310f,.958f,.350f,Cyan);

            burstFrame=Plate(panel.transform,"Burst Frame",.037f,.055f,.961f,.235f,Line,11);
            Text(burstFrame.transform,"Burst Label","BURST",displayFont,.044f,.10f,.195f,.90f,25,Amber,bold:true);
            for(int i=0;i<burstCells.Length;i++)
            {
                float x=.198f+i*.101f;
                burstCells[i]=Plate(burstFrame.transform,"Burst Cell "+(i+1),x,.179f,x+.090f,.819f,Line,8);
            }
            burstText=Text(burstFrame.transform,"Burst amount","",displayFont,.718f,.075f,.968f,.925f,25,Amber,
                TextAlignmentOptions.MidlineRight,true);
            burstReadyText=Text(panel.transform,"Full Burst Ready","FULL BURST READY",displayFont,
                .361f,.240f,.954f,.305f,18,Amber,TextAlignmentOptions.MidlineRight,true);
        }

        void BuildSkills()
        {
            skillsPanel=Area(safeRoot,"Direct Cast Skills",.426f,.037f,.982f,.302f);
            var preview=Plate(skillsPanel,"Selected Skill Preview",.004f,.752f,.812f,.993f,Line,10);
            preview.raycastTarget=true;
            previewText=Text(preview.transform,"Damage and Burst Preview","",displayFont,.034f,.46f,.969f,.93f,26,White,bold:true);
            inputHint=Text(preview.transform,"Cast Input Hint","",bodyFont,.035f,.085f,.968f,.447f,20,Muted);
            for(int i=0;i<3;i++)
            {
                int slot=i;
                float x=.004f+i*.268f;
                var plate=Plate(skillsPanel,"Skill "+(i+1),x,0,x+.255f,.708f,Line,16);
                Button(plate,()=>SelectSkill(slot,true));
                Image(plate.transform,"Skill accent",.035f,.929f,.306f,.960f,skillColors[i]);
                var artwork=Art(plate.transform,"Skill icon",.07f,.285f,.930f,.905f,GetSkillIcon(i));
                var tag=Plate(plate.transform,"Burst Cost Tile",.78f,.71f,.957f,.932f,skillColors[i],6);
                Text(tag.transform,"Skill Burst cost",battle.Cost(i).ToString(),displayFont,.04f,.02f,.96f,.98f,29,skillColors[i],
                    TextAlignmentOptions.Center,true);
                var name=Text(plate.transform,"Skill name",battle.SkillName(i).ToUpperInvariant(),displayFont,
                    .067f,.12f,.946f,.345f,26,White,bold:true);
                Text(plate.transform,"Skill description",battle.SkillScope(i),displayFont,.068f,.033f,.938f,.146f,16,Muted,bold:true);
                skills[i]=new SkillCard(plate,artwork,name);
            }

            var ultimateArea=Area(skillsPanel,"Full Burst Slot",.820f,0,1,.942f);
            var iconFrame=Area(ultimateArea,"Full Burst Hexagon Sizing",0,.100f,1,.98f);
            var aspect=iconFrame.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent;aspect.aspectRatio=1.08f;
            fullBurstHalo=Mark(iconFrame,"Full Burst Halo",-.045f,-.045f,1.045f,1.045f,
                StoryHudGraphic.Mark.Hexagon,Color.clear,Amber,6);
            var full=Mark(iconFrame,"Full Burst Skill",0,0,1,1,StoryHudGraphic.Mark.Hexagon,Ink,Line,3);
            Button(full,()=>SelectSkill(3,true));
            var fullPicture=Art(full.transform,"Full Burst Icon",.15f,.19f,.85f,.83f,GetSkillIcon(3));
            var fullTitle=Text(full.transform,"Full Burst Title","FULL BURST",displayFont,.095f,.120f,.905f,.336f,23,Amber,
                TextAlignmentOptions.Center,true);
            Text(full.transform,"Full Burst Cost",battle.Cost(3).ToString(),displayFont,.39f,.737f,.62f,.952f,30,Amber,
                TextAlignmentOptions.Center,true);
            fullLabel=Text(ultimateArea,"Ultimate Status","",displayFont,-.038f,-.02f,1.038f,.102f,19,Amber,
                TextAlignmentOptions.Center,true);
            skills[3]=new SkillCard(full,fullPicture,fullTitle);
        }

        Sprite GetSkillIcon(int slot)
        {
            var art=battle.Art;
            return art&&art.weaponIcons!=null&&slot<art.weaponIcons.Length?art.weaponIcons[slot]:null;
        }

        void RefreshTimeline()
        {
            battle.PreviewTurns(order);
            var actor=battle.Actor;
            bool own=actor==battle.Player;
            Color accent=own?Cyan:RedOrange;
            turnLabel.text=actor ? own ? battle.PlayerTurn ? "YOUR TURN" : "PLAYER ACTION" : "ENEMY ACTION" : "DEPLOYING";
            turnLabel.color=actor?accent:Muted;
            for(int i=0;i<timeline.Length;i++)
            {
                var card=timeline[i];
                var fighter=i<order.Count?order[i]:null;
                card.Plate.gameObject.SetActive(fighter);
                if(!fighter)continue;
                var portrait=fighter==battle.Player?battle.Art?battle.Art.playerPortrait:null:EnemyPortrait(fighter);
                card.Portrait.enabled=portrait;
                card.Portrait.sprite=portrait;
                card.Name.text=fighter==battle.Player?battle.CallSign:
                    fighter.displayName.Replace("RX-78-2 ","").ToUpperInvariant();
                card.Number.color=i==0?accent:Muted;
                card.Plate.TintEdge(i==0?accent:Line);
                card.Plate.edgeWidth=i==0?3.5f:1.5f;
                card.Arrow.color=accent;
            }
        }

        Sprite EnemyPortrait(DemoFighter fighter)
        {
            int index=Array.IndexOf(battle.Opponents,fighter);
            return index>=0?stage.opponents[index].portrait:null;
        }

        void RefreshTelemetry()
        {
            var player=battle.Player;
            healthText.text=$"{player.Health} / {player.maxHealth}";
            armorText.text=player.ArmorBroken?$"ARMOR BROKEN  ·  0 / {player.maxArmor}":
                $"{Mathf.CeilToInt(player.Armor)} / {player.maxArmor}";
            armorText.color=player.ArmorBroken?Red:Cyan;
            Fill(healthFill,player.Health/(float)Mathf.Max(1,player.maxHealth));
            Fill(armorFill,player.Armor/Mathf.Max(1,player.maxArmor));
            var armorTrack=armorFill.transform.parent.GetComponent<UnityEngine.UI.Image>();
            armorTrack.color=player.ArmorBroken?new Color(.48f,.086f,.112f):new Color(.11f,.19f,.25f);
            burstText.text=$"{battle.Burst} / {burstCells.Length}";
            for(int i=0;i<burstCells.Length;i++)
            {
                bool charged=i<battle.Burst;
                var cell=burstCells[i];
                cell.top=charged?new Color(1,.83f,.37f):new Color(.108f,.170f,.202f);
                cell.bottom=charged?new Color(.88f,.39f,.08f):Ink;
                cell.TintEdge(charged?Amber:Line);
                cell.SetVerticesDirty();
            }
        }

        void RefreshBurstGlow()
        {
            bool full=battle.Burst==burstCells.Length;
            glow=.5f+.5f*Mathf.Sin(Time.unscaledTime*7.1f);
            burstFrame.TintEdge(full?Color.Lerp(Amber,White,glow*.62f):Line);
            burstReadyText.gameObject.SetActive(full);
            fullBurstHalo.SetEdge(full?new Color(Amber.r,Amber.g,Amber.b,.28f+.56f*glow):Color.clear);
        }

        void RefreshSkills()
        {
            for(int i=0;i<skills.Length;i++)
            {
                var skill=skills[i];
                bool enough=battle.Burst>=battle.Cost(i);
                float opacity=enough?1:.47f;
                skill.Art.color=new Color(1,1,1,opacity);
                skill.Name.color=enough?White:Muted;
                Color edge=i==selectedSkill||i==3&&enough?skillColors[i]:Line;
                if(skill.Surface is MechaPlateGraphic plate)plate.TintEdge(edge);
                else if(skill.Surface is StoryHudGraphic hex)
                {
                    hex.SetEdge(edge);hex.color=enough?new Color(.27f,.15f,.045f):Ink;
                }
            }
            bool full=battle.Burst>=battle.Cost(3);
            fullLabel.text=full?"FULL BURST READY":$"{battle.Cost(3)-battle.Burst} BURST NEEDED";
            fullLabel.color=full?Amber:Muted;
            if(selectedSkill<0)
            {
                previewText.text=battle.Target?$"TARGET LOCK // {battle.Target.displayName.ToUpperInvariant()}":"SELECT A TARGET";
                inputHint.text="SELECT SKILL  /  1–4";
                return;
            }
            int cost=battle.Cost(selectedSkill);
            string direction=$"BURST {battle.Burst} → {battle.BurstAfterTurn(selectedSkill)}";
            if(cost>battle.Burst)
            {
                previewText.text=$"{battle.SkillName(selectedSkill).ToUpperInvariant()}   /   NEED {cost-battle.Burst} BURST";
                inputHint.text="NOT ENOUGH BURST";
                return;
            }
            string forecast=DamageReadout(selectedSkill);
            previewText.text=$"{forecast}   |   {direction}";
            inputHint.text=$"{battle.SkillName(selectedSkill).ToUpperInvariant()}  ·  TAP AGAIN / SPACE";
        }

        string DamageReadout(int slot)
        {
            if(battle.IsBuff(slot))return "SELF BUFF  ·  ATK + DEF";
            if(battle.IsArea(slot))
            {
                int total=0;
                foreach(var enemy in battle.Opponents)
                    if(enemy.IsAlive)total+=battle.EstimateHealthDamage(slot,enemy);
                return $"EST {total} HP TOTAL  ·  AOE";
            }
            string estimate=$"EST {battle.EstimateHealthDamage(slot,battle.Target)} HP";
            if(slot==3 && battle.SkillScope(slot)=="TARGET + SPLASH")
            {
                int splash=0;
                foreach(var enemy in battle.Opponents)
                    if(enemy.IsAlive && enemy!=battle.Target)splash+=battle.EstimateHealthDamage(slot,enemy,true);
                estimate+=$" + {splash} SPLASH";
            }
            return estimate;
        }

        readonly struct TimelineCard
        {
            public readonly MechaPlateGraphic Plate;
            public readonly TMP_Text Number, Name;
            public readonly Image Portrait;
            public readonly StoryHudGraphic Arrow;
            public TimelineCard(MechaPlateGraphic plate,TMP_Text number,Image portrait,TMP_Text name,StoryHudGraphic arrow)
            { Plate=plate;Number=number;Portrait=portrait;Name=name;Arrow=arrow; }
        }

        readonly struct SkillCard
        {
            public readonly Graphic Surface;
            public readonly Image Art;
            public readonly TMP_Text Name;
            public SkillCard(Graphic surface,Image art,TMP_Text name)
            { Surface=surface;Art=art;Name=name; }
        }
    }
}
