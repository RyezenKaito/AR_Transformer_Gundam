using GundamGame.CombatDemo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GundamGame.Story.StoryHudUi;

namespace GundamGame.Story
{
    public sealed partial class StoryBattleHud
    {
        MechaPlateGraphic defensePanel, dodgePlate, parryPlate, resultPlate;
        TMP_Text incomingText, dodgeDetail, parryDetail, reactionResult;
        Image timingFill, dodgeZone, parryZone, timeNeedle;
        Button dodgeButton, parryButton;
        CanvasGroup resultGroup;
        AudioSource reactionAudio;
        AudioClip[] reactionSounds;
        float reactionEnd;

        void BuildReaction()
        {
            defensePanel=Plate(safeRoot,"Tactical Reaction HUD",.534f,.041f,.977f,.335f,RedOrange,23);
            defensePanel.raycastTarget=true;
            var frame=defensePanel.transform;
            Image(frame,"Incoming danger strip",.027f,.938f,.125f,.966f,RedOrange);
            incomingText=Text(frame,"Enemy Attack Name","",displayFont,.028f,.808f,.970f,.932f,34,White,bold:true);
            var bar=Plate(frame,"Reaction Timing Gauge",.030f,.668f,.969f,.795f,Line,6);
            var track=Image(bar.transform,"Timing Track",.012f,.11f,.986f,.89f,new Color(.095f,.153f,.187f));
            timingFill=Image(track.transform,"Timing Progress",0,0,0,1,new Color(.12f,.43f,.55f,.72f));
            dodgeZone=Image(track.transform,"Dodge Sweet Spot",0,.04f,1,.46f,new Color(.08f,.88f,1,.83f));
            parryZone=Image(track.transform,"Parry Sweet Spot",0,.52f,1,.96f,new Color(1,.76f,.32f,.99f));
            timeNeedle=Image(track.transform,"Impact Needle",0,-.18f,.009f,1.18f,White);
            Text(frame,"Dodge window legend","DODGE WINDOW",displayFont,.035f,.547f,.476f,.647f,21,Cyan,bold:true);
            Text(frame,"Parry window legend","PARRY WINDOW",displayFont,.530f,.547f,.962f,.647f,21,Amber,
                TextAlignmentOptions.MidlineRight,true);

            dodgePlate=BuildDefenseButton(frame,"DODGE","E / A",.030f,.486f,Cyan,StoryHudGraphic.Mark.Evade,false,
                out dodgeButton,out dodgeDetail);
            parryPlate=BuildDefenseButton(frame,"PARRY","Q / D",.514f,.970f,Amber,StoryHudGraphic.Mark.Shield,true,
                out parryButton,out parryDetail);

            resultPlate=Plate(safeRoot,"Reaction Feedback",.366f,.574f,.634f,.670f,Line,21);
            resultGroup=resultPlate.gameObject.AddComponent<CanvasGroup>();resultGroup.blocksRaycasts=false;
            reactionResult=Text(resultPlate.transform,"Reaction Result","",displayFont,.05f,.05f,.95f,.95f,53,White,
                TextAlignmentOptions.Center,true);
            resultPlate.gameObject.SetActive(false);

            reactionAudio=gameObject.AddComponent<AudioSource>();
            reactionAudio.playOnAwake=false;reactionAudio.spatialBlend=0;reactionAudio.volume=.42f;
            reactionSounds=new[] { Tone("Tactical hit",150,60,.18f),Tone("Tactical evade",440,1080,.17f),
                Tone("Tactical full block",760,420,.23f),Tone("Tactical reduced damage",430,150,.22f) };
        }

        MechaPlateGraphic BuildDefenseButton(Transform root,string title,string keys,float left,float right,Color accent,
            StoryHudGraphic.Mark icon,bool parry,out Button button,out TMP_Text details)
        {
            var plate=Plate(root,title,left,.083f,right,.510f,accent,17);
            plate.top=Color.Lerp(Ink,accent,.18f);
            plate.bottom=Color.Lerp(Ink,accent,.05f);
            button=Button(plate,()=>battle.Defend(parry));
            Mark(plate.transform,title+" Icon",.044f,.173f,.213f,.845f,icon,Color.clear,accent,4);
            Text(plate.transform,title+" Label",title,displayFont,.245f,.449f,.882f,.906f,36,accent,bold:true);
            details=Text(plate.transform,title+" Effect","",displayFont,.247f,.095f,.944f,.460f,22,White,bold:true);
            Text(plate.transform,title+" Keys",keys,displayFont,.630f,.512f,.928f,.859f,19,Muted,
                TextAlignmentOptions.MidlineRight,true);
            return plate;
        }

        void RefreshReactionGauge()
        {
            float duration=Mathf.Max(.001f,battle.Duration),remaining=battle.Remaining;
            float progress=Mathf.Clamp01(1-remaining/duration);
            Fill(timingFill,progress);
            dodgeZone.rectTransform.anchorMin=new Vector2(Mathf.Clamp01(1-battle.DodgeWindow/duration),.04f);
            parryZone.rectTransform.anchorMin=new Vector2(Mathf.Clamp01(1-battle.ParryWindow/duration),.52f);
            timeNeedle.rectTransform.anchorMin=new Vector2(Mathf.Clamp01(progress-.006f),-.18f);
            timeNeedle.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(progress+.006f),1.18f);
            incomingText.text="ENEMY ATTACK // "+(battle.AttackName??"").ToUpperInvariant();
            dodgeDetail.text=battle.GuaranteedEvade?"100% EVADE":"50% EVADE";
            parryDetail.text=battle.FullBlock?"BLOCK 100% DMG":"REDUCE 50% DMG";
            dodgeButton.interactable=parryButton.interactable=battle.CanDefend;
            dodgePlate.TintEdge(battle.CanDefend&&remaining<=battle.DodgeWindow?White:Cyan);
            parryPlate.TintEdge(battle.CanDefend&&remaining<=battle.ParryWindow?White:Amber);
        }

        void ShowReactionResult(BattleReactionResult outcome)
        {
            (string caption,Color accent)=outcome switch
            {
                BattleReactionResult.Dodge=>("DODGE!",Cyan),
                BattleReactionResult.FullBlock=>("PARRY BLOCK!",Amber),
                BattleReactionResult.HalfBlock=>("50% MITIGATED",Amber),
                _=>("HIT!",RedOrange)
            };
            reactionResult.text=caption;reactionResult.color=accent;
            resultPlate.TintEdge(accent);
            resultGroup.alpha=1;
            reactionEnd=Time.unscaledTime+1.12f;
            resultPlate.rectTransform.localScale=Vector3.one*1.12f;
            resultPlate.gameObject.SetActive(true);
            reactionAudio.PlayOneShot(reactionSounds[(int)outcome]);
        }

        void RefreshReactionResult()
        {
            if(!resultPlate.gameObject.activeSelf)return;
            float remaining=reactionEnd-Time.unscaledTime;
            if(remaining<=0){resultPlate.gameObject.SetActive(false);return;}
            resultGroup.alpha=Mathf.Clamp01(remaining/.28f);
            resultPlate.rectTransform.localScale=Vector3.one*(1+Mathf.Clamp01((remaining-.93f)/.19f)*.12f);
        }

        static AudioClip Tone(string name,float from,float to,float duration)
        {
            const int rate=24000;
            var samples=new float[Mathf.RoundToInt(duration*rate)];
            float phase=0;
            for(int i=0;i<samples.Length;i++)
            {
                float time=i/(float)samples.Length;
                phase+=2*Mathf.PI*Mathf.Lerp(from,to,time)/rate;
                float envelope=Mathf.Clamp01(time*38)*Mathf.Pow(1-time,2);
                samples[i]=(Mathf.Sin(phase)*.64f+Mathf.Sin(phase*2.02f)*.23f)*envelope;
            }
            var sound=AudioClip.Create(name,samples.Length,1,rate,false);sound.SetData(samples,0);
            return sound;
        }
    }
}
