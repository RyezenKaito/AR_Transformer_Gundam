using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GundamGame.Unicorn
{
    public sealed class UnicornCombatEffects : MonoBehaviour
    {
        sealed class Stroke
        {
            public LineRenderer line;
            public float age, lifetime, width;
            public Color tint;
            public Vector3 travel;
            public Vector3[] points;
            public Vector3 center;
            public Vector3 shellEnd;
            public System.Func<float> shellClock;
            public System.Func<Vector3> follow;
            public float shellTrail;
            public float fromRadius, toRadius;
        }
        readonly List<Stroke> pool = new();
        readonly List<(Renderer renderer,int slot)> frameSlots = new();
        MaterialPropertyBlock block;
        readonly AudioClip[] sounds=new AudioClip[6];
        AudioSource audioSource;
        Material lineMaterial;
        LineRenderer javelinProngs;
        float nextAura, nextConvergence;
        public int ActiveCount => pool.Count(p=>p.line.enabled);
        static readonly Color Pink=new(1,.17f,.36f), Core=new(1,.91f,.85f), Gold=new(1,.62f,.15f);

        public void Initialize(GameObject model)
        {
            block=new MaterialPropertyBlock();
            var shader=Shader.Find("Universal Render Pipeline/Particles/Unlit");
            lineMaterial=new Material(shader){name="Unicorn additive VFX"};
            lineMaterial.SetFloat("_Surface",1);
            lineMaterial.SetFloat("_Blend",2);
            lineMaterial.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
            lineMaterial.SetFloat("_DstBlend",(float)BlendMode.One);
            lineMaterial.SetFloat("_ZWrite",0);
            lineMaterial.SetOverrideTag("RenderType","Transparent");
            lineMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            lineMaterial.renderQueue=(int)RenderQueue.Transparent;
            for(int i=0;i<64;i++)pool.Add(new Stroke{line=NewLine("Pooled VFX "+i)});
            javelinProngs=NewLine("Beam Javelin plasma prongs");
            javelinProngs.positionCount=5;
            javelinProngs.startWidth=javelinProngs.endWidth=.055f;
            javelinProngs.startColor=javelinProngs.endColor=Pink*1.65f;
            foreach(var r in model.GetComponentsInChildren<Renderer>())
                for(int i=0;i<r.sharedMaterials.Length;i++)
                    if(r.sharedMaterials[i] && r.sharedMaterials[i].HasProperty("_EmissionMap") && r.sharedMaterials[i].GetTexture("_EmissionMap") is Texture map &&
                        map.name.Contains("_RED",System.StringComparison.OrdinalIgnoreCase))
                        frameSlots.Add((r,i));
            audioSource=gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake=false;audioSource.volume=.68f;
            for(int i=0;i<sounds.Length;i++)sounds[i]=BuildSound(i);
        }

        LineRenderer NewLine(string label)
        {
            var go=new GameObject(label);go.transform.SetParent(transform,false);
            var line=go.AddComponent<LineRenderer>();line.enabled=false;line.useWorldSpace=true;
            line.sharedMaterial=lineMaterial;line.numCapVertices=3;line.numCornerVertices=2;
            line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
            return line;
        }
        Stroke Draw(Vector3[] points, Color tint,float width,float life,Vector3 travel=default)
        {
            var s=pool.FirstOrDefault(p=>!p.line.enabled)??pool.OrderByDescending(p=>p.age/p.lifetime).First();
            s.age=0;s.lifetime=life;s.width=width;s.tint=tint;s.travel=travel;s.points=points;s.fromRadius=s.toRadius=0;
            s.shellClock=null;s.follow=null;
            s.line.enabled=true;s.line.positionCount=points.Length;s.line.SetPositions(points);
            s.line.startColor=s.line.endColor=tint;s.line.startWidth=s.line.endWidth=width;
            return s;
        }

        public void Tick(float dt)
        {
            nextAura=Mathf.Max(0,nextAura-dt);
            nextConvergence=Mathf.Max(0,nextConvergence-dt);
            foreach(var s in pool)
            {
                if(!s.line.enabled)continue;
                s.age=s.shellClock!=null?s.shellClock():s.age+dt;
                float t=Mathf.Clamp01(s.age/s.lifetime);
                if(t>=1){s.line.enabled=false;continue;}
                var c=s.tint;c.a*=1-t*t;
                s.line.startColor=s.line.endColor=c;
                s.line.startWidth=s.line.endWidth=s.width*(1-.7f*t);
                if(s.shellClock!=null)
                {
                    Vector3 Head(float phase)=>Vector3.LerpUnclamped(s.center,s.shellEnd,phase)+Vector3.up*(4*phase*(1-phase)*.5f);
                    s.line.SetPosition(0,Head(Mathf.Max(0,t-s.shellTrail)));
                    s.line.SetPosition(1,Head(t));
                }
                else if(s.toRadius>0)
                {
                    float radius=Mathf.Lerp(s.fromRadius,s.toRadius,Mathf.Sqrt(t));
                    Vector3 center=s.follow!=null?s.follow():s.center;
                    for(int i=0;i<s.points.Length;i++)s.line.SetPosition(i,center+s.points[i]*radius);
                }
                else if(s.follow!=null)s.line.SetPosition(0,s.follow());
                else if(s.travel!=Vector3.zero)
                    for(int i=0;i<s.points.Length;i++)s.line.SetPosition(i,s.points[i]+s.travel*t-Vector3.up*.24f*t*t);
            }
        }
        public void Clear()
        {
            foreach(var s in pool)s.line.enabled=false;
            javelinProngs.enabled=false;
            nextAura=nextConvergence=0;
            SetResonance(0);
            if(audioSource)audioSource.Stop();
        }
        public void SetPaused(bool paused) { if(audioSource){if(paused)audioSource.Pause();else audioSource.UnPause();} }
        public void Play(int sound) { if(audioSource) audioSource.PlayOneShot(sounds[sound]); }
        public void Tracer(Vector3 origin,Vector3 target)
        {
            var dir=(target-origin).normalized;
            Draw(new[]{origin,origin+dir*.27f},Core,.115f,.082f);
            Draw(new[]{origin+dir*.17f,target},Gold,.026f,.13f);
            Play(1);
        }
        public void Beam(Vector3 origin,Vector3 target,bool enemy=false)
        {
            var dir=(target-origin).normalized;
            var tint=enemy?new Color(.36f,.83f,1):Pink;
            Draw(new[]{origin,target},tint*1.5f,.24f,.24f);
            Draw(new[]{origin,target},Core*1.8f,.075f,.19f);
            Draw(new[]{origin,origin+dir*.35f},Core*1.8f,.20f,.11f);
            Play(2);
        }
        public void Impact(Vector3 point,float size,Color tint,bool large)
        {
            int count=large?14:7;
            for(int i=0;i<count;i++)
            {
                float a=i*2.39996f;
                Vector3 dir=new(Mathf.Cos(a),i%3*.42f-.24f,Mathf.Sin(a));
                Draw(new[]{point+dir*size*.12f,point+dir*size*.65f},i%3==0?Core:tint,size*.062f,.16f+i%4*.041f,dir*size*.85f);
            }
            if(large)Ring(point,.08f,size*1.6f,tint,.39f);
        }
        public void Ring(Vector3 point,float from,float to,Color color,float time)
        { AddRing(point,Vector3.right,Vector3.forward,from,to,color,time,.04f); }

        public void EnemyWarning(System.Func<Vector3> origin,Vector3 aim,float radius,float time)
        {
            var red=new Color(1f,.18f,.08f);
            Vector3 start=origin();
            Draw(new[]{start,aim},red,.022f,time).follow=origin;
            if(radius>0)
            {
                Ring(aim,radius,radius,red,time);
                Ring(aim,radius*.35f,radius,Gold,time);
            }
            else
            {
                Vector3 axis=(aim-start).normalized;
                Vector3 side=Vector3.Cross(Vector3.up,axis).normalized;
                AddRing(start,side,Vector3.Cross(axis,side),.20f,.045f,Gold,time,.045f).follow=origin;
            }
        }

        public void EnemyShell(Vector3 origin,Vector3 aim,float flightTime,System.Func<float> shellClock)
        {
            var trail=Draw(new[]{origin,origin},new Color(.67f,.39f,.20f,.32f),.27f,flightTime);
            trail.shellClock=shellClock;trail.center=origin;trail.shellEnd=aim;trail.shellTrail=.48f;
            var projectile=Draw(new[]{origin,origin},Gold*1.8f,.22f,flightTime);
            projectile.shellClock=shellClock;projectile.center=origin;projectile.shellEnd=aim;projectile.shellTrail=.12f;
            Impact(origin,.28f,Gold,false);
            Play(2);
        }

        public void EnemySlash(Vector3 grip,Vector3 tip,Vector3 previousDirection)
        {
            Vector3 edge=tip-grip;
            if(edge.sqrMagnitude<.02f)return;
            var ribbon=new Vector3[13];
            for(int i=0;i<ribbon.Length;i++)
            {
                float t=i/(ribbon.Length-1f);
                ribbon[i]=grip+Vector3.Slerp(previousDirection,edge.normalized,t)*edge.magnitude*Mathf.Lerp(.72f,1,t);
            }
            Draw(ribbon,Pink*1.7f,.115f,.22f);
            Draw(ribbon,Core*1.6f,.032f,.14f);
            Play(1);
        }

        public void EnemyLane(Vector3 start,Vector3 end,bool thrust,float lifetime)
        {
            start.y=end.y;
            var side=Vector3.Cross(Vector3.up,(end-start).normalized)*(thrust?.34f:.18f);
            var red=new Color(1,.15f,.08f,.78f);
            Draw(new[]{start-side,end-side,end+side,start+side},red,.033f,lifetime);
            if(thrust)Draw(new[]{end-side*1.6f,end+(end-start).normalized*.37f,end+side*1.6f},red,.067f,lifetime);
        }

        public void EnemyThrust(Vector3 grip,Vector3 tip)
        {
            var axis=(tip-grip).normalized;
            var side=Vector3.Cross(axis,Vector3.up).normalized;
            Draw(new[]{grip-axis*.68f,tip},Pink*2,.15f,.23f);
            Draw(new[]{grip,tip},Core*1.6f,.045f,.16f);
            Draw(new[]{tip-side*.12f-axis*.3f,tip,tip+side*.12f-axis*.3f},Pink*1.7f,.06f,.22f);
            AddRing(tip,side,Vector3.Cross(side,axis),.10f,.43f,Pink,.24f,.075f);
            Play(2);
        }

        public void SetEnemyJavelin(bool visible,Vector3 grip=default,Vector3 tip=default)
        {
            javelinProngs.enabled=visible;
            if(!visible)return;
            Vector3 axis=(tip-grip).normalized;
            Vector3 side=Vector3.Cross(axis,Vector3.up).normalized;
            Vector3 fork=tip-axis*.46f;
            javelinProngs.SetPosition(0,tip-axis*.09f-side*.18f);
            javelinProngs.SetPosition(1,fork-side*.12f);
            javelinProngs.SetPosition(2,fork);
            javelinProngs.SetPosition(3,fork+side*.12f);
            javelinProngs.SetPosition(4,tip-axis*.09f+side*.18f);
        }
        Stroke AddRing(Vector3 point,Vector3 right,Vector3 up,float from,float to,Color color,float time,float width)
        {
            var unit=new Vector3[41];
            for(int i=0;i<unit.Length;i++)
            {float a=i*Mathf.PI/20;unit[i]=right*Mathf.Cos(a)+up*Mathf.Sin(a);}
            var s=Draw(unit.Select(v=>point+v*from).ToArray(),color,width,time);
            s.points=unit;s.center=point;s.fromRadius=from;s.toRadius=to;
            return s;
        }
        public void Shield(Vector3 center,Vector3 forward)
        {
            var right=Vector3.Cross(Vector3.up,forward).normalized;
            var up=Vector3.Cross(forward,right);
            Color cyan=new(.13f,.8f,1);
            AddRing(center,right,up,.22f,.94f,cyan,.31f,.075f);
            AddRing(center,right,up,.35f,1.12f,Core,.22f,.028f);
            Play(5);
        }
        public void Eject(Vector3 point,Vector3 right)
        {
            var edge=right*.06f;
            var up=Vector3.up*.09f;
            Draw(new[]{point-edge,point+edge,point+edge+up,point-edge+up,point-edge},Gold,.026f,.46f,right*.58f+Vector3.up*.31f);
            Play(0);
        }
        public void SetResonance(float power)
        {
            foreach(var slot in frameSlots)
            {
                slot.renderer.GetPropertyBlock(block,slot.slot);
                block.SetColor("_EmissionColor",new Color(1,.013f,.06f)*(power*4.75f));
                slot.renderer.SetPropertyBlock(block,slot.slot);
                block.Clear();
            }
        }

        public void Aura(Vector3 feet,float power)
        {
            if(power<.05f || nextAura>0)return;
            nextAura=.07f;
            for(int i=0;i<7;i++)
            {
                float a=Random.value*Mathf.PI*2;
                Vector3 outward=new(Mathf.Cos(a),0,Mathf.Sin(a));
                Vector3 start=feet+outward*Random.Range(.49f,1.0f)+Vector3.up*Random.Range(.03f,1.8f);
                Vector3 end=start+outward*.13f+Vector3.up*Random.Range(.15f,.49f);
                Draw(new[]{start,end},(i%4==0?Core:Pink)*power,.012f+.030f*power,.34f,Vector3.up*.84f);
            }
            for(int i=0;i<3;i++)
            {
                float angle=Random.value*Mathf.PI*2;
                var wisp=new Vector3[8];
                for(int j=0;j<wisp.Length;j++)
                {
                    float t=j/(wisp.Length-1f), spin=angle+.6f*t;
                    float radius=Mathf.Lerp(.72f,1.08f,t);
                    wisp[j]=feet+new Vector3(Mathf.Cos(spin)*radius,.06f+t*2.05f,Mathf.Sin(spin)*radius);
                }
                Draw(wisp,new Color(1.25f,.02f,.10f,.62f)*power,.12f*power,.21f,Vector3.up*.21f);
            }
        }

        public void BuffPulse(Vector3 feet)
        {
            feet+=Vector3.up*.055f;
            Ring(feet,.24f,1.63f,Pink,.48f);
            Ring(feet+Vector3.up*.06f,.42f,1.18f,Core,.32f);
            Play(4);
        }

        public void Converge(Vector3 torso,Vector3 hands,float power)
        {
            if(power<.08f || nextConvergence>0)return;
            nextConvergence=.065f;
            for(int i=0;i<8;i++)
            {
                float angle=Random.value*Mathf.PI*2;
                Vector3 from=torso+new Vector3(Mathf.Cos(angle)*.63f,Random.Range(-1.28f,1.15f),Mathf.Sin(angle)*.64f);
                Vector3 travel=hands-from;
                Draw(new[]{from,from+travel*.20f},(i%3==0?Core:Pink)*Mathf.Lerp(.8f,2f,power),.031f,.21f,travel*.86f);
            }
            float radius=Mathf.Lerp(.045f,.24f,power);
            AddRing(hands,Vector3.right,Vector3.up,radius*.88f,radius,Pink*2f,.13f,.09f);
            AddRing(hands,Vector3.forward,Vector3.up,radius*.7f,radius,Core*1.7f,.11f,.055f);
        }

        public void MartialImpact(Vector3 point,Vector3 forward,bool kick)
        {
            var side=Vector3.Cross(Vector3.up,forward).normalized;
            float width=kick?.43f:.23f;
            AddRing(point,side,Vector3.up,.09f,width,Core*1.35f,.17f,.065f);
            Impact(point,width*1.25f,Gold,false);
            Play(kick?5:1);
        }

        public void UltraBeam(Vector3 origin,Vector3 target)
        {
            Vector3 axis=(target-origin).normalized;
            Vector3 side=Vector3.Cross(axis,Vector3.up).normalized;
            Vector3 up=Vector3.Cross(side,axis);
            Draw(new[]{origin,target},new Color(2.3f,.040f,.19f),1.13f,.76f);
            Draw(new[]{origin,target},new Color(3.5f,.28f,.49f),.66f,.63f);
            Draw(new[]{origin,target},Core*2.4f,.25f,.53f);
            for(int band=0;band<4;band++)
            {
                var points=new Vector3[24];
                for(int i=0;i<points.Length;i++)
                {
                    float t=i/(points.Length-1f), phase=t*18+band*Mathf.PI*.5f;
                    points[i]=Vector3.Lerp(origin,target,t)+(.21f+.44f*t)*(side*Mathf.Cos(phase)+up*Mathf.Sin(phase));
                }
                Draw(points,Pink*1.9f,.075f,.59f);
            }
            AddRing(origin,side,up,.18f,.65f,Core*2,.26f,.13f);
            AddRing(target,side,up,.32f,1.17f,Pink*2,.48f,.16f);
            Impact(target,1.48f,Pink,true);
            Ring(new Vector3(target.x,.06f,target.z),.36f,2.6f,Pink*1.35f,.59f);
            Play(3);
        }

        static AudioClip BuildSound(int id)
        {
            const int rate=22050;
            float[] seconds={.15f,.22f,.34f,.85f,.47f,.27f};
            int length=Mathf.RoundToInt(rate*seconds[id]);
            var samples=new float[length];
            var random=new System.Random(90+id);
            float phase=0,low=0;
            for(int i=0;i<length;i++)
            {
                float t=i/(float)rate,n=i/(float)length;
                float f=id==0?Mathf.Lerp(820,180,n):id==1?Mathf.Lerp(520,120,n):id==4?Mathf.Lerp(58,153,n):id==5?Mathf.Lerp(750,335,n):Mathf.Lerp(id==3?122:215,38,n);
                phase+=2*Mathf.PI*f/rate;
                float noise=(float)random.NextDouble()*2-1;
                low=Mathf.Lerp(low,noise,id==1?.45f:.075f);
                float envelope=Mathf.Min(1,t*240)*Mathf.Pow(1-n,id==3?2:3);
                samples[i]=envelope*(Mathf.Sin(phase)*.26f+low*.62f+Mathf.Sin(phase*2.91f)*.09f);
            }
            var clip=AudioClip.Create("Unicorn synth "+id,length,1,rate,false);clip.SetData(samples,0);return clip;
        }

        void OnDestroy(){if(lineMaterial)Destroy(lineMaterial);foreach(var s in sounds)if(s)Destroy(s);}
    }
}
