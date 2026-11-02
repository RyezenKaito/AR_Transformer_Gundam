using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MechaDemo
{
    [DefaultExecutionOrder(-50)]
    public sealed class MechaEffects : MonoBehaviour
    {
        public Transform muzzle, bladeTip, bladeBase, bladeVisual, chargeOrigin;
        public Transform[] nozzles = Array.Empty<Transform>();
        public TrailRenderer swordTrail;
        public Material energyMaterial, softMaterial, sparkMaterial, coreMaterial;
        [Range(0,1)] public float flashStrength = 0.45f;
        [Range(0,1)] public float shakeStrength = 0.55f;
        public float Shake { get; private set; }
        ParticleSystem[] jets;
        ParticleSystem[] impacts;
        ParticleSystem charge;
        LineRenderer beamCore, beamGlow;
        LineRenderer[] rings;
        float[] ringAge, ringPower;
        Vector3[] ringNormal;
        float beamAge, beamDuration, chargeWeight;
        Vector3 beamFrom, beamTo;
        Transform orb;
        AudioSource oneShot, jetAudio, chargeAudio;
        AudioClip jetClip, chargeClip, slashClip, hitClip, shotClip, ultimateClip;
        bool thrusting, initialized;
        MechaPlayer owner; Vector3 tipRest, visualRestScale, visualRestPosition; MaterialPropertyBlock bladeProperties;
        int impactCursor, ringCursor;
        Material fallbackMaterial;
        readonly Vector3[] circle = new Vector3[49];
        readonly Vector3[] ribbonBases=new Vector3[24],ribbonTips=new Vector3[24];
        readonly float[] ribbonTimes=new float[24];
        readonly Vector3[] ribbonVertices=new Vector3[48];
        readonly Vector2[] ribbonUV=new Vector2[48];
        readonly Color[] ribbonColors=new Color[48];
        readonly int[] ribbonIndices=new int[138];
        int ribbonCount;bool ribbonEmitting;Mesh ribbonMesh;MeshRenderer ribbonRenderer;

        void Awake() { Ensure(); }
        public void Ensure()
        {
            if(initialized) return;
            initialized = true;owner=GetComponent<MechaPlayer>();if(bladeTip!=null)tipRest=bladeTip.localPosition;if(bladeVisual!=null){visualRestScale=bladeVisual.localScale;visualRestPosition=bladeVisual.localPosition;bladeProperties=new MaterialPropertyBlock();}
            if(energyMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if(shader == null) shader = Shader.Find("Sprites/Default");
                fallbackMaterial = new Material(shader);
                energyMaterial = fallbackMaterial;
            }
            if(softMaterial == null) softMaterial = energyMaterial;
            if(sparkMaterial == null) sparkMaterial = energyMaterial;
            jets = new ParticleSystem[nozzles.Length];
            for(int i=0;i<jets.Length;i++)
            {
                jets[i]=MakeParticles("Jet_"+i, nozzles[i], softMaterial, new Color(.2f,.65f,1f,1f), true);
                var main=jets[i].main; main.startLifetime=new ParticleSystem.MinMaxCurve(.06f,.16f); main.startSpeed=new ParticleSystem.MinMaxCurve(6f,12f); main.startSize=new ParticleSystem.MinMaxCurve(.04f,.14f); main.maxParticles=96;
                var emission=jets[i].emission; emission.rateOverTime=100;
                var shape=jets[i].shape; shape.shapeType=ParticleSystemShapeType.Cone; shape.angle=9; shape.radius=.035f;
                var size=jets[i].sizeOverLifetime; size.enabled=true; size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,1,1,0));
            }
            impacts=new ParticleSystem[12];
            rings=new LineRenderer[8]; ringAge=new float[8];ringPower=new float[8];ringNormal=new Vector3[8];
            for(int i=0;i<impacts.Length;i++)
            {
                impacts[i]=MakeParticles("ImpactPool_"+i,transform,sparkMaterial,new Color(1f,.61f,.16f,1),false);
                var m=impacts[i].main;m.startLifetime=new ParticleSystem.MinMaxCurve(.10f,.28f);m.startSpeed=new ParticleSystem.MinMaxCurve(2f,7f);m.startSize=new ParticleSystem.MinMaxCurve(.015f,.055f);m.gravityModifier=.55f;m.maxParticles=96;
                var sh=impacts[i].shape;sh.shapeType=ParticleSystemShapeType.Cone;sh.angle=60;sh.radius=.025f;
                var r=impacts[i].GetComponent<ParticleSystemRenderer>();r.renderMode=ParticleSystemRenderMode.Stretch;r.lengthScale=2;r.velocityScale=.12f;
            }
            for(int i=0;i<rings.Length;i++){rings[i]=MakeLine("RingPool_"+i,energyMaterial,.04f);rings[i].positionCount=49; rings[i].enabled=false;ringAge[i]=10;}
            charge=MakeParticles("ChargeParticles",transform,softMaterial,new Color(.2f,.8f,1,1),true);
            var cm=charge.main;cm.startLifetime=.30f;cm.startSpeed=-.8f;cm.startSize=new ParticleSystem.MinMaxCurve(.015f,.045f);cm.simulationSpace=ParticleSystemSimulationSpace.Local;
            var cs=charge.shape;cs.shapeType=ParticleSystemShapeType.Sphere;cs.radius=.35f;
            var ce=charge.emission;ce.rateOverTime=0;
            var orbGo=GameObject.CreatePrimitive(PrimitiveType.Sphere);orbGo.name="ChargeCore";orbGo.transform.SetParent(transform,false);orb=orbGo.transform;
            var collider=orbGo.GetComponent<Collider>();if(collider!=null)Destroy(collider);
            orbGo.GetComponent<Renderer>().sharedMaterial=coreMaterial!=null?coreMaterial:energyMaterial;orbGo.SetActive(false);
            beamCore=MakeLine("BeamCore",energyMaterial,.065f);beamGlow=MakeLine("BeamGlow",softMaterial,.22f);beamCore.enabled=beamGlow.enabled=false;
            oneShot=gameObject.AddComponent<AudioSource>();oneShot.spatialBlend=.35f;oneShot.volume=.42f;
            jetAudio=gameObject.AddComponent<AudioSource>();jetAudio.spatialBlend=.4f;jetAudio.loop=true;jetAudio.volume=0;
            chargeAudio=gameObject.AddComponent<AudioSource>();chargeAudio.spatialBlend=.35f;chargeAudio.volume=.32f;
            jetClip=Tone("SynthJet",.6f,70,70,.4f,true);chargeClip=Tone("SynthCharge",.7f,160,650,.05f,false);
            slashClip=Tone("SynthSlash",.18f,380,80,.7f,false);hitClip=Tone("SynthImpact",.23f,140,35,.65f,false);shotClip=Tone("SynthShot",.12f,700,95,.10f,false);ultimateClip=Tone("SynthUltimate",.7f,90,30,.5f,false);
            jetAudio.clip=jetClip;
            var ribbonGo=new GameObject("SwordArcRibbon");ribbonGo.layer=11;ribbonGo.transform.SetParent(transform,false);ribbonMesh=new Mesh();ribbonMesh.name="Pooled Sword Arc";ribbonMesh.MarkDynamic();ribbonGo.AddComponent<MeshFilter>().sharedMesh=ribbonMesh;ribbonRenderer=ribbonGo.AddComponent<MeshRenderer>();ribbonRenderer.sharedMaterial=softMaterial;ribbonRenderer.shadowCastingMode=ShadowCastingMode.Off;ribbonRenderer.receiveShadows=false;
            for(int i=0;i<23;i++){int v=i*2,k=i*6;ribbonIndices[k]=v;ribbonIndices[k+1]=v+1;ribbonIndices[k+2]=v+2;ribbonIndices[k+3]=v+2;ribbonIndices[k+4]=v+1;ribbonIndices[k+5]=v+3;}
        }
        ParticleSystem MakeParticles(string n,Transform parent,Material mat,Color color,bool loop)
        {
            var g=new GameObject(n);g.layer=11;g.transform.SetParent(parent,false);
            var p=g.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=p.main;main.playOnAwake=false;main.loop=loop;main.duration=.6f;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startColor=color;main.maxParticles=128;
            var e=p.emission;e.enabled=true;e.rateOverTime=0;
            var c=p.colorOverLifetime;c.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(color,.18f),new GradientColorKey(color,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.6f,.35f),new GradientAlphaKey(0,1)});c.color=gradient;
            var renderer=p.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            return p;
        }
        LineRenderer MakeLine(string n,Material mat,float width)
        {
            var g=new GameObject(n);g.layer=11;g.transform.SetParent(transform,false);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=mat;l.positionCount=2;l.useWorldSpace=true;l.startWidth=l.endWidth=1;l.widthMultiplier=width;l.numCapVertices=4;l.shadowCastingMode=ShadowCastingMode.Off;l.receiveShadows=false;return l;
        }
        public void Thrusters(bool on)
        {
            Ensure();if(thrusting==on)return;thrusting=on;
            foreach(var p in jets){if(on)p.Play();else p.Stop(true,ParticleSystemStopBehavior.StopEmitting);}
            if(on&&!jetAudio.isPlaying)jetAudio.Play();
        }
        public void SwordTrail(bool on)
        {
            Ensure();ribbonEmitting=on;if(swordTrail==null)return;
            if(on&&!swordTrail.emitting)swordTrail.Clear();swordTrail.emitting=on;
        }
        public void Charge(float amount)
        {
            Ensure();float previous=chargeWeight;chargeWeight=Mathf.Clamp01(amount);
            if(chargeWeight>.01f&&previous<=.01f){charge.Play();chargeAudio.clip=chargeClip;chargeAudio.Play();}
            if(chargeWeight<=.01f){charge.Stop(true,ParticleSystemStopBehavior.StopEmitting);chargeAudio.Stop();}
            var em=charge.emission;em.rateOverTime=chargeWeight*90;
        }
        public void Beam(Vector3 from,Vector3 to,float duration)
        {
            Ensure();beamFrom=from;beamTo=to;beamDuration=Mathf.Max(.04f,duration);beamAge=0;beamCore.enabled=beamGlow.enabled=true;
            beamCore.SetPosition(0,from);beamCore.SetPosition(1,to);beamGlow.SetPosition(0,from);beamGlow.SetPosition(1,to);
        }
        public void Impact(Vector3 point,Vector3 normal,float strength)
        {
            Ensure();var p=impacts[impactCursor++%impacts.Length];p.transform.position=point;p.transform.rotation=Quaternion.LookRotation(normal.sqrMagnitude>.001f?normal:Vector3.up);p.Emit(Mathf.RoundToInt(Mathf.Clamp(strength, .4f,3)*18));
            int i=ringCursor++%rings.Length;ringAge[i]=0;ringPower[i]=Mathf.Clamp(strength,.5f,3);ringNormal[i]=normal.sqrMagnitude>.001f?normal.normalized:Vector3.up;rings[i].transform.position=point+ringNormal[i]*.015f;rings[i].enabled=true;
            Shake=Mathf.Max(Shake,Mathf.Clamp01(strength*.3f)*shakeStrength);oneShot.PlayOneShot(strength>2?ultimateClip:hitClip,Mathf.Clamp(strength*.6f,.35f,1));
        }
        public void MuzzleFlash()
        {
            Ensure();if(muzzle==null)return;var p=impacts[impactCursor++%impacts.Length];p.transform.SetPositionAndRotation(muzzle.position,muzzle.rotation);p.Emit(6);oneShot.PlayOneShot(shotClip,.7f);
        }
        public void DashBurst(Vector3 direction)
        {
            Ensure();int i=ringCursor++%rings.Length;ringAge[i]=0;ringPower[i]=.9f;ringNormal[i]=direction.sqrMagnitude>.001f?direction.normalized:transform.forward;rings[i].transform.position=transform.position+Vector3.up; rings[i].enabled=true;oneShot.PlayOneShot(slashClip,.45f);
        }
        public void PlayCue(string cue)
        {
            Ensure();if(string.IsNullOrEmpty(cue))return;var c=cue.ToLowerInvariant();
            if(c.Contains("charge")){if(!chargeAudio.isPlaying){chargeAudio.clip=chargeClip;chargeAudio.Play();}}
            else if(c.Contains("land")||c.Contains("hit"))oneShot.PlayOneShot(hitClip,.4f);
            else if(c.Contains("fire")||c.Contains("shot"))oneShot.PlayOneShot(shotClip,.6f);
            else if(c.Contains("ultimate")||c=="r")oneShot.PlayOneShot(ultimateClip,.6f);
            else oneShot.PlayOneShot(slashClip,.5f);
        }
        void LateUpdate()
        {
            if(!initialized)return;
            float dt=Time.deltaTime;if(dt<=0)return;
            Shake=Mathf.MoveTowards(Shake,0,dt*4);
            jetAudio.volume=Mathf.MoveTowards(jetAudio.volume,thrusting?.22f:0,dt*1.8f);if(!thrusting&&jetAudio.volume<=.001f&&jetAudio.isPlaying)jetAudio.Stop();
            float bladeFactor=1;if(owner!=null&&owner.ActionName=="R"){float t=owner.ActionTime;bladeFactor=t<1.15f?Mathf.Lerp(1,2.5f,Mathf.SmoothStep(0,1,(t-.45f)/.7f)):t<2.35f?2.5f:Mathf.Lerp(2.5f,1,Mathf.Clamp01((t-2.35f)/.85f));}SetBladeFactor(bladeFactor);UpdateRibbon();Transform anchor=owner!=null&&owner.ActionName=="R"&&bladeTip!=null?bladeTip:chargeOrigin;if(anchor!=null){charge.transform.position=anchor.position;orb.position=anchor.position;}
            orb.gameObject.SetActive(chargeWeight>.01f);orb.localScale=Vector3.one*(.055f+chargeWeight*.20f)*(1+Mathf.Sin(Time.time*45)*.06f);
            if(beamCore.enabled){beamAge+=dt;float a=1-Mathf.Clamp01(beamAge/beamDuration);beamCore.widthMultiplier=.075f*a;beamGlow.widthMultiplier=.24f*a;beamCore.enabled=beamGlow.enabled=a>0;}
            for(int i=0;i<rings.Length;i++)if(rings[i].enabled)
            {
                ringAge[i]+=dt;float t=ringAge[i]/.32f;if(t>=1){rings[i].enabled=false;continue;}
                float radius=Mathf.Lerp(.08f,1.35f*ringPower[i],1-(1-t)*(1-t));Quaternion q=Quaternion.LookRotation(ringNormal[i]);Vector3 center=rings[i].transform.position;
                for(int k=0;k<circle.Length;k++){float a=k*(Mathf.PI*2/(circle.Length-1));circle[k]=center+q*new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0);}
                rings[i].SetPositions(circle);rings[i].widthMultiplier=.045f*(1-t)*ringPower[i];rings[i].startColor=rings[i].endColor=new Color(.4f,.85f,1,1-t);
            }
        }
        void UpdateRibbon()
        {
            if(ribbonMesh==null||bladeBase==null||bladeTip==null)return;
            while(ribbonCount>0&&Time.time-ribbonTimes[0]>.14f){for(int i=1;i<ribbonCount;i++){ribbonBases[i-1]=ribbonBases[i];ribbonTips[i-1]=ribbonTips[i];ribbonTimes[i-1]=ribbonTimes[i];}ribbonCount--;}
            if(ribbonEmitting)
            {
                if(ribbonCount==24){for(int i=1;i<24;i++){ribbonBases[i-1]=ribbonBases[i];ribbonTips[i-1]=ribbonTips[i];ribbonTimes[i-1]=ribbonTimes[i];}ribbonCount--;}
                ribbonBases[ribbonCount]=bladeBase.position;ribbonTips[ribbonCount]=bladeTip.position;ribbonTimes[ribbonCount]=Time.time;ribbonCount++;
            }
            ribbonRenderer.enabled=ribbonCount>1;if(ribbonCount<2)return;
            for(int i=0;i<ribbonCount;i++){ribbonVertices[i*2]=transform.InverseTransformPoint(ribbonBases[i]);ribbonVertices[i*2+1]=transform.InverseTransformPoint(ribbonTips[i]);float u=i/(float)(ribbonCount-1);ribbonUV[i*2]=new Vector2(u,.1f);ribbonUV[i*2+1]=new Vector2(u,.9f);float alpha=Mathf.Clamp01(1-(Time.time-ribbonTimes[i])/.14f);ribbonColors[i*2]=new Color(.15f,.6f,1,alpha*.25f);ribbonColors[i*2+1]=new Color(.5f,.95f,1,alpha*.8f);}
            ribbonMesh.Clear();ribbonMesh.SetVertices(ribbonVertices,0,ribbonCount*2);ribbonMesh.SetUVs(0,ribbonUV,0,ribbonCount*2);ribbonMesh.SetColors(ribbonColors,0,ribbonCount*2);ribbonMesh.SetTriangles(ribbonIndices,0,(ribbonCount-1)*6,0,true);
        }
        void SetBladeFactor(float factor)
        {
            if(bladeTip==null||bladeBase==null||bladeVisual==null)return;Vector3 b=bladeBase.localPosition;bladeTip.localPosition=b+(tipRest-b)*factor;bladeVisual.localScale=new Vector3(visualRestScale.x,visualRestScale.y*factor,visualRestScale.z);bladeVisual.localPosition=b+(tipRest-b)*(.5f*factor);
            var renderer=bladeVisual.GetComponent<Renderer>();if(renderer!=null&&bladeProperties!=null){bladeProperties.SetColor("_EmissionColor",new Color(.08f,.65f,1)*Mathf.Lerp(8,16,(factor-1)/1.5f));renderer.SetPropertyBlock(bladeProperties);}
        }
        public void Cleanup()
        {
            if(!initialized)return;ribbonCount=0;ribbonEmitting=false;if(ribbonRenderer!=null)ribbonRenderer.enabled=false;SetBladeFactor(1);Thrusters(false);SwordTrail(false);Charge(0);beamCore.enabled=beamGlow.enabled=false;jetAudio.Stop();chargeAudio.Stop();Shake=0;
        }
        void OnDisable(){Cleanup();}
        void OnDestroy()
        {
            if(fallbackMaterial!=null)Destroy(fallbackMaterial);if(ribbonMesh!=null)Destroy(ribbonMesh);
            foreach(var clip in new[]{jetClip,chargeClip,slashClip,hitClip,shotClip,ultimateClip})if(clip!=null)Destroy(clip);
        }
        static AudioClip Tone(string name,float duration,float f0,float f1,float noise,bool loop)
        {
            int rate=22050,count=Mathf.CeilToInt(rate*duration);var samples=new float[count];var random=new System.Random(name.Length*7919);double phase=0,filtered=0;
            for(int i=0;i<count;i++){float t=i/(float)count;phase+=2*Math.PI*Mathf.Lerp(f0,f1,t)/rate;filtered=filtered*.65+(random.NextDouble()*2-1)*.35;float env=loop?.7f:Mathf.Sin(Mathf.PI*Mathf.Pow(t,.45f))*Mathf.Pow(1-t,.65f);samples[i]=(float)((Math.Sin(phase)*.5+Math.Sin(phase*1.51)*.17+filtered*noise)*env*.36);}
            var clip=AudioClip.Create(name,count,1,rate,false);clip.SetData(samples,0);return clip;
        }
    }
}




