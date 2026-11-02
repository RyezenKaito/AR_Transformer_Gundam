using UnityEngine;

namespace TurnBasedBattle
{
    public sealed class BattleEffects : MonoBehaviour
    {
        public LineRenderer[] projectiles;
        public ParticleSystem[] impactPool;
        public AudioSource oneShot, sustain;
        public AudioClip shotClip, impactClip, dodgeClip, chargeClip, groundClip, electricClip;
        int cursor;
        bool ultimateGround, ultimateElectric;
        BattleActionDefinition active;
        readonly Vector3[] linePoints=new Vector3[14];

        void Awake()
        {
            foreach(var p in impactPool){var main=p.main;main.simulationSpace=ParticleSystemSimulationSpace.Custom;main.customSimulationSpace=transform;main.scalingMode=ParticleSystemScalingMode.Hierarchy;}
            foreach(var line in projectiles)line.useWorldSpace=false;
        }
        void SetLine(LineRenderer line)
        {
            for(int i=0;i<linePoints.Length;i++)linePoints[i]=transform.InverseTransformPoint(linePoints[i]);
            line.SetPositions(linePoints);
        }
        public void BeginAction(BattleActionDefinition definition,BattleActor source,BattleActor target)
        {
            EndAction();active=definition;ultimateGround=ultimateElectric=false;
        }
        public void Release(BattleActionKind kind)
        {
            if(kind!=BattleActionKind.Guard)Play(shotClip,kind==BattleActionKind.EarthPrison?.6f:.28f);
        }
        public void Present(BattleActionDefinition action,BattleActor source,BattleActor target,float time,DodgeAttempt[] attempts)
        {
            if(action.kind==BattleActionKind.EarthPrison)
            {
                time-=action.vfxStart;if(time<0)return;
                if(!sustain.isPlaying && !ultimateElectric && time<4){sustain.clip=chargeClip;sustain.loop=true;sustain.volume=.20f;sustain.Play();}
                if(!ultimateGround && time>=4.10f){ultimateGround=true;Play(groundClip,.65f);}
                if(!ultimateElectric && time>=5.05f){ultimateElectric=true;sustain.clip=electricClip;sustain.volume=.17f;sustain.Play();}
                if(time>=6.45f)sustain.volume=Mathf.Lerp(.17f,0,Mathf.InverseLerp(6.45f,7.4f,time));
                return;
            }
            if(action.kind==BattleActionKind.Guard)return;
            if(action.melee)
            {
                float hit=action.hits.Length>0?action.hits[0].time:1;
                float sweep=Mathf.InverseLerp(hit-.22f,hit+.13f,time);
                for(int l=0;l<projectiles.Length;l++)
                {
                    var line=projectiles[l];line.enabled=time>=hit-.22f && time<=hit+.13f;
                    if(!line.enabled)continue;
                    line.widthMultiplier=l==0?.045f:.14f;Color color=source.player?new Color(.1f,2,2.5f,1):new Color(2.4f,.3f,.1f,1);color.a=l==0?.9f:.3f;line.startColor=line.endColor=color;
                    if(source.weapons==null || !source.weapons.TrailActive){line.enabled=false;continue;}
                    for(int i=0;i<linePoints.Length;i++)linePoints[i]=source.weapons.TrailPoints[i];
                    SetLine(line);
                }
                return;
            }
            bool enemy=action.kind==BattleActionKind.EnemyStrike || action.kind==BattleActionKind.EnemyCombo;
            for(int j=0;j<projectiles.Length;j++)projectiles[j].enabled=false;
            for(int h=0;h<action.hits.Length;h++)
            {
                float hit=action.hits[h].time,start=hit-.40f,end=hit+.12f;
                if(time<start || time>end)continue;
                float p=Mathf.Clamp01((time-start)/(hit-start));
                bool dodged=enemy && h<attempts.Length && attempts[h].outcome==DodgeOutcome.Dodged;
                Vector3 destination=target.AimPoint+(dodged?target.transform.TransformVector(new Vector3(0,.8f,-.25f)):Vector3.zero);
                Vector3 origin=source.CastPoint;
                Vector3 head=Vector3.Lerp(origin,destination,p);
                Vector3 tail=Vector3.Lerp(origin,destination,Mathf.Clamp01(p-.25f));
                float fade=1-Mathf.InverseLerp(hit,end,time);
                Color color=enemy?new Color(2.1f,.35f,.16f,fade):action.kind==BattleActionKind.Skill?new Color(.65f,.8f,2.8f,fade):new Color(.12f,1.7f,2.1f,fade);
                for(int l=0;l<projectiles.Length;l++)
                {
                    var line=projectiles[l];line.enabled=true;line.widthMultiplier=(l==0?.045f:.16f)*(action.kind==BattleActionKind.Skill?1.5f:1);
                    Color c=color;c.a*=l==0?1:.3f;line.startColor=line.endColor=c;
                    for(int i=0;i<linePoints.Length;i++)
                    {
                        float u=i/(float)(linePoints.Length-1);
                        linePoints[i]=Vector3.Lerp(tail,head,u)+source.transform.up*Mathf.Sin(u*Mathf.PI)*Mathf.Sin(time*50+i)*.03f*Mathf.Abs(source.transform.lossyScale.y);
                    }
                    SetLine(line);
                }
                break;
            }
        }
        public void Hit(BattleActor target,bool dodge,float strength)
        {
            if(dodge){Play(dodgeClip,.32f);return;}
            var p=impactPool[cursor++%impactPool.Length];p.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
            p.transform.position=target.AimPoint;p.transform.rotation=Quaternion.LookRotation(-target.transform.forward);
            p.Emit(Mathf.RoundToInt(strength*24));p.Pause();Play(impactClip,strength*.25f);
        }
        public void Tick(float dt){foreach(var p in impactPool)if(p.particleCount>0)p.Simulate(dt,false,false,false);}
        public void EndAction(){foreach(var l in projectiles)if(l!=null)l.enabled=false;if(sustain!=null)sustain.Stop();active=null;}
        public void StopAll(){EndAction();foreach(var p in impactPool)if(p!=null)p.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);if(oneShot!=null)oneShot.Stop();}
        public void SetPaused(bool paused){if(paused){oneShot?.Pause();sustain?.Pause();}else{oneShot?.UnPause();sustain?.UnPause();}}
        void Play(AudioClip clip,float volume){if(oneShot!=null && clip!=null)oneShot.PlayOneShot(clip,Mathf.Clamp01(volume));}
    }
}
