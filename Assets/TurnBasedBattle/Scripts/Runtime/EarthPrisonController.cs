using System;
using System.Collections.Generic;
using UnityEngine;

namespace TurnBasedBattle
{
    public sealed class EarthPrisonController : MonoBehaviour
    {
        public Camera viewCamera;
        public Transform area;
        public Transform orb, orbGlow, impactGlow;
        public Transform[] pillars, rubbleBases, tips, tipGlows, domes;
        public Renderer[] pillarRenderers, rubbleRenderers, domeRenderers;
        public LineRenderer[] chargeRings, chargeArcs, cageCore, cageGlow, verticalArcs, crossArcs;
        public LineRenderer transferCore, transferGlow, groundRing;
        public ParticleSystem chargeSparks, transferSparks, impactSparks, impactDust, residualDust;
        public ParticleSystem[] baseDust, debris, tipSparks;
        public float radius = 3.8f, pillarHeight = 3.8f;
        public bool Active { get; private set; }
        public float EffectTime { get; private set; }
        public string Stage => EffectTime<1.1f?"TỤ ĐIỆN":EffectTime<2.65f?"CẦU ĐIỆN":EffectTime<3.35f?"PHÓNG SÉT":EffectTime<4.1f?"VÒM XOÁY":EffectTime<4.75f?"TRỤ ĐÁ":EffectTime<6.45f?"LỒNG SÉT":"TAN RÃ";
        Transform source;
        Vector3[] pillarLocal;
        readonly Dictionary<LineRenderer,Vector3[]> buffers = new Dictionary<LineRenderer,Vector3[]>();
        ParticleSystem[] particles;
        Renderer[] allRenderers;
        MaterialPropertyBlock block;
        float previousTime, chargeRemainder, tipRemainder;
        int seed;
        bool initialized, impactEmitted, pillarsEmitted, residualEmitted, transferEmitted;

        void Ensure()
        {
            if(initialized)return;
            initialized=true;block=new MaterialPropertyBlock();
            if(viewCamera==null)viewCamera=Camera.main;
            allRenderers=GetComponentsInChildren<Renderer>(true);
            particles=GetComponentsInChildren<ParticleSystem>(true);
            pillarLocal=new Vector3[pillars.Length];
            for(int i=0;i<pillars.Length;i++)pillarLocal[i]=pillars[i].localPosition;
            foreach(var line in GetComponentsInChildren<LineRenderer>(true))
            {
                line.useWorldSpace=false;line.positionCount=line==groundRing?97:33;
                buffers.Add(line,new Vector3[line.positionCount]);
            }
            foreach(var ps in particles){ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.simulationSpace=ParticleSystemSimulationSpace.Custom;main.customSimulationSpace=transform;main.scalingMode=ParticleSystemScalingMode.Hierarchy;}
        }
        public void PlayEffect(Transform socket,Vector3 center,int variation=1)
        {
            Ensure();StopEffect();source=socket;seed=variation;area.position=center;Active=true;
            EffectTime=previousTime=0;chargeRemainder=tipRemainder=0;
            impactEmitted=pillarsEmitted=residualEmitted=transferEmitted=false;
            for(int i=0;i<particles.Length;i++){particles[i].useAutoRandomSeed=false;particles[i].randomSeed=(uint)(31+i*43+Math.Abs(seed)%1000);}
            Evaluate(0,0);
        }
        public void StopEffect()
        {
            Ensure();Active=false;
            foreach(var r in allRenderers)if(r!=null)r.enabled=false;
            foreach(var ps in particles)if(ps!=null)ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        static float Smooth(float a,float b,float t)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,t));
        void Opacity(Renderer renderer,float value,float dissolve=0,float time=0)
        {
            if(renderer==null)return;
            renderer.enabled=value>.001f;
            renderer.GetPropertyBlock(block);block.SetFloat("_Opacity",Mathf.Clamp01(value));block.SetFloat("_Dissolve",Mathf.Clamp01(dissolve));block.SetFloat("_Phase",time);renderer.SetPropertyBlock(block);
        }
        void Glow(Transform shape,Vector3 position,float size,float strength)
        {
            shape.position=transform.TransformPoint(position);shape.localScale=Vector3.one*size;
            if(viewCamera!=null)shape.rotation=viewCamera.transform.rotation;
            Opacity(shape.GetComponent<Renderer>(),strength);
        }
        public void Evaluate(float time,float dt)
        {
            Ensure();if(!Active)return;
            EffectTime=time;
            Vector3 origin=source!=null?transform.InverseTransformPoint(source.position):area.localPosition+Vector3.left*10+Vector3.up*1.8f;
            float chargeFade=(1-Smooth(2.65f,2.9f,time))*Smooth(0,.18f,time);
            float orbSize=Mathf.Lerp(.13f,.43f,Smooth(.7f,1.4f,time));
            Glow(orb,origin,orbSize*(1+.06f*Mathf.Sin(time*27)),chargeFade);
            Glow(orbGlow,origin,orbSize*3.1f,chargeFade*.48f);
            for(int i=0;i<chargeRings.Length;i++)
            {
                var points=buffers[chargeRings[i]];float r=orbSize*(.65f+i*.16f);
                Quaternion rot=Quaternion.Euler(25+i*49,time*(67+i*21),i*48);
                for(int k=0;k<points.Length;k++){float a=k*Mathf.PI*2/(points.Length-1);points[k]=origin+rot*new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,0);}
                chargeRings[i].SetPositions(points);chargeRings[i].widthMultiplier=.018f;Opacity(chargeRings[i],chargeFade*.7f);
            }
            for(int i=0;i<chargeArcs.Length;i++)
            {
                float a=i*Mathf.PI*2/chargeArcs.Length+time*.5f;
                float length=Mathf.Lerp(1.2f,.45f,Smooth(.6f,1.3f,time));
                Vector3 p=origin+new Vector3(Mathf.Cos(a)*length,Mathf.Sin(a*1.3f)*.55f,Mathf.Sin(a)*length);
                Bolt(chargeArcs[i],origin,p,time,.13f,i+seed,.022f,chargeFade);
            }
            if(time<2.65f)
            {
                chargeSparks.transform.localPosition=origin;chargeRemainder+=dt*42;
                int amount=Mathf.FloorToInt(chargeRemainder);chargeRemainder-=amount;Emit(chargeSparks,amount);
            }
            float transmission=Smooth(2.65f,2.85f,time)*(1-Smooth(3.15f,3.5f,time));
            Vector3 end=area.localPosition+Vector3.up*.15f;
            float reveal=Smooth(2.65f,2.98f,time);
            Vector3 target=Vector3.Lerp(origin,end,reveal);
            Bolt(transferCore,origin,target,time,.15f,23+seed,.055f,transmission);
            Bolt(transferGlow,origin,target,time,.15f,23+seed,.20f,transmission*.36f);
            if(!transferEmitted && time>=2.82f)
            {
                transferEmitted=true;transferSparks.transform.localPosition=Vector3.Lerp(origin,end,.55f);Emit(transferSparks,42);
            }
            float flash=Smooth(3.34f,3.39f,time)*(1-Smooth(3.42f,3.70f,time));
            Glow(impactGlow,area.localPosition+Vector3.up*.55f,3.6f,flash*.9f);
            if(!impactEmitted && time>=3.35f)
            {
                impactEmitted=true;impactSparks.transform.localPosition=area.localPosition+Vector3.up*.15f;impactDust.transform.localPosition=area.localPosition+Vector3.up*.08f;
                Emit(impactSparks,34);Emit(impactDust,20);
            }
            float domeFade=Smooth(3.43f,3.52f,time)*(1-Smooth(4.05f,4.34f,time));
            float expansion=Mathf.Lerp(.28f,1,Smooth(3.43f,3.92f,time));
            for(int i=0;i<domes.Length;i++)
            {
                domes[i].localScale=new Vector3(radius*1.14f*expansion,2.25f*expansion,radius*1.14f*expansion);
                domes[i].localRotation=Quaternion.Euler(0,time*(55+i*12)+i*120,0);
                Opacity(domeRenderers[i],domeFade*.72f,Smooth(4.03f,4.35f,time),time);
            }
            var ring=buffers[groundRing];float ringRadius=radius*1.13f*expansion;
            for(int k=0;k<ring.Length;k++){float a=k*Mathf.PI*2/(ring.Length-1);ring[k]=area.localPosition+new Vector3(Mathf.Cos(a)*ringRadius,.055f,Mathf.Sin(a)*ringRadius);}
            groundRing.SetPositions(ring);groundRing.widthMultiplier=.06f;Opacity(groundRing,domeFade*.75f);

            if(!pillarsEmitted && time>=4.12f)
            {
                pillarsEmitted=true;
                for(int i=0;i<6;i++){Emit(baseDust[i],12);Emit(debris[i],12);}
            }
            float dissolve=Smooth(6.45f,7.05f,time);
            float baseDissolve=Smooth(6.85f,7.45f,time);
            float energy=Smooth(4.65f,5.08f,time)*(1-Smooth(6.4f,6.88f,time));
            float cage=Smooth(4.93f,5.15f,time)*(1-Smooth(6.45f,6.85f,time));
            for(int i=0;i<6;i++)
            {
                float rise=Smooth(4.10f+i*.008f,4.7f+i*.008f,time);
                pillars[i].localPosition=pillarLocal[i]+Vector3.down*pillarHeight*(1-rise);
                Opacity(pillarRenderers[i],time>=4.1f && time<7.06f?1:0,dissolve);
                rubbleBases[i].localScale=Vector3.one*Mathf.Lerp(.25f,1,Smooth(4.05f,4.4f,time));
                float e=energy*(.9f+.1f*Mathf.Sin(time*23+i));
                Glow(tipGlows[i],transform.InverseTransformPoint(tips[i].position),.90f,e*.75f);
                var tipBody=tips[i].GetComponent<Renderer>();Opacity(tipBody,e);
                Bolt(verticalArcs[i],transform.InverseTransformPoint(tips[i].position),transform.InverseTransformPoint(pillars[i].position)+Vector3.up*.15f,time,.13f,i+50,.023f,energy);
                int j=(i+1)%6;
                Bolt(cageCore[i],transform.InverseTransformPoint(tips[i].position),transform.InverseTransformPoint(tips[j].position),time,.13f,100+i,.045f,cage);
                Bolt(cageGlow[i],transform.InverseTransformPoint(tips[i].position),transform.InverseTransformPoint(tips[j].position),time,.13f,100+i,.14f,cage*.4f);
            }
            foreach(var r in rubbleRenderers)Opacity(r,time>=4.08f && time<7.46f?1:0,baseDissolve);
            for(int i=0;i<crossArcs.Length;i++)
            {
                float pulse=.45f+.55f*Mathf.Pow(Mathf.Sin(time*(7+i)+i),2);
                Bolt(crossArcs[i],transform.InverseTransformPoint(tips[i].position),transform.InverseTransformPoint(tips[i+3].position)-Vector3.up*.65f,time,.24f,200+i,.025f,cage*pulse*.7f);
            }
            if(time>=4.7f && time<6.45f)
            {
                tipRemainder+=dt*12;int n=Mathf.FloorToInt(tipRemainder);tipRemainder-=n;
                for(int i=0;i<6;i++){tipSparks[i].transform.localPosition=transform.InverseTransformPoint(tips[i].position);Emit(tipSparks[i],n);}
            }
            if(!residualEmitted && time>=6.5f){residualEmitted=true;residualDust.transform.localPosition=area.localPosition+Vector3.up*.5f;Emit(residualDust,24);}
            foreach(var ps in particles)if(ps.particleCount>0 && dt>0)ps.Simulate(dt,false,false,false);
            previousTime=time;
        }

        void Emit(ParticleSystem system,int amount)
        {
            if(system==null || amount<=0)return;
            system.GetComponent<ParticleSystemRenderer>().enabled=true;system.Emit(amount);system.Pause();
        }
        void Bolt(LineRenderer line,Vector3 a,Vector3 b,float time,float amplitude,int variation,float width,float strength)
        {
            if(strength<=.001f){line.enabled=false;return;}
            var points=buffers[line];Vector3 delta=b-a;
            Vector3 side=Vector3.Cross(delta.normalized,Vector3.up).normalized;
            if(side.sqrMagnitude<.1f)side=Vector3.right;
            Vector3 up=Vector3.Cross(side,delta.normalized).normalized;
            for(int i=0;i<points.Length;i++)
            {
                float u=i/(float)(points.Length-1),envelope=Mathf.Sin(u*Mathf.PI);
                float n=(Mathf.PerlinNoise(i*1.713f+variation,time*9)-.5f)*2;
                float m=(Mathf.PerlinNoise(i*2.631f+variation+23,time*11)-.5f)*2;
                points[i]=Vector3.Lerp(a,b,u)+(side*n+up*m)*amplitude*envelope;
            }
            line.SetPositions(points);line.widthMultiplier=width;Opacity(line,strength);
        }
        public void PreviewAt(float time,Transform socket,Vector3 center)
        {
            PlayEffect(socket,center,7);
            float t=0;while(t<time){float step=Mathf.Min(1f/60,time-t);t+=step;Evaluate(t,step);}
        }
        void LateUpdate()
        {
            if(!Active || viewCamera==null)return;
            orb.rotation=orbGlow.rotation=impactGlow.rotation=viewCamera.transform.rotation;
            foreach(var glow in tipGlows)glow.rotation=viewCamera.transform.rotation;
        }
        void OnDisable(){if(initialized)StopEffect();}
    }
}
