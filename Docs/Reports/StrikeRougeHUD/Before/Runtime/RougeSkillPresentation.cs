using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GundamGame.CombatDemo
{
    /// <summary>Animation, weapon sockets, local-space VFX and optional virtual-camera shots.</summary>
    public sealed class RougeSkillPresentation : MonoBehaviour
    {
        public RougeBattleDemo battle;
        public RougeAnimationDriver animationRig;
        public Transform rifleMuzzle, beamMuzzle, railMuzzle, swordBase, swordTip, core, shield;
        public Transform[] missilePorts = Array.Empty<Transform>();
        public Camera virtualCamera;
        public RougeArenaPlacement placement;
        public RougeArenaCamera arenaCamera;
        public RougeCinematicDirector cinematicDirector;
        public Shader glowShader;
        public Shader solidShader;
        public bool soundEnabled = true;
        public int ActiveEffectCount => fx.Count;
        public string LastSkill { get; private set; } = "Ready";
        public bool CinematicActive { get; private set; }
        public int DamageEvents { get; private set; }
        public int MissilesLaunched { get; private set; }
        public int SourceAnimationsPlayed => animationRig != null ? animationRig.ImportedClipsPlayed : 0;
        public bool AnimationReady => animationRig != null && animationRig.idle != null && animationRig.launcher != null && animationRig.rifle != null && animationRig.blade != null;
        readonly List<GameObject> fx = new List<GameObject>();
        readonly List<Material> materials = new List<Material>();
        readonly Dictionary<Color,Material> glowMaterials = new Dictionary<Color,Material>();
        AudioSource audioSource;
        AudioClip shotSound, slashSound, impactSound, chargeSound;
        Material metal;
        Canvas cinemaCanvas;
        Text cinemaLabel;
        bool initialized;
        float cinemaTime, defenseTime, barrageTime=-1;
        RougeBattleDemo.DefenseResult defense;
        Vector3 visualHome;
        Quaternion visualRotation;
        Transform Arena => battle.arenaRoot;
        DemoFighter Player => battle.player;
        float Scale => Mathf.Abs(Arena.lossyScale.x);
        bool Running => battle != null && battle.gameplayEnabled;
        static readonly Color Green = new Color(.2f,1,.45f,.9f);
        static readonly Color Pink = new Color(1,.12f,.47f,.9f);
        static readonly Color Gold = new Color(1,.62f,.12f,.9f);
        static readonly Color Cyan = new Color(.15f,.8f,1,.9f);

        public void Initialize()
        {
            if(initialized || battle==null || Player==null) return;
            visualHome=Player.visual.localPosition; visualRotation=Player.visual.localRotation;
            animationRig.Initialize();
            audioSource=gameObject.AddComponent<AudioSource>(); audioSource.playOnAwake=false; audioSource.volume=.3f; audioSource.spatialBlend=0;
            shotSound=Tone("Rifle pulse",.16f,700,110,.12f);
            slashSound=Tone("Blade sweep",.24f,160,45,.7f);
            impactSound=Tone("Missile impact",.3f,100,28,.8f);
            chargeSound=Tone("Core charging",.85f,120,800,.04f);
            metal=new Material(solidShader!=null?solidShader:Shader.Find("Universal Render Pipeline/Unlit")); metal.color=new Color(.8f,.86f,.9f); materials.Add(metal);
            BuildCinemaHUD(); initialized=true;
            if(arenaCamera==null && virtualCamera!=null)
            {
                arenaCamera=virtualCamera.GetComponent<RougeArenaCamera>();
                if(arenaCamera==null)arenaCamera=virtualCamera.gameObject.AddComponent<RougeArenaCamera>();
            }
            if(arenaCamera!=null){arenaCamera.arenaRoot=Arena;arenaCamera.presentation=this;arenaCamera.placement=placement;arenaCamera.Initialize();}
            if(cinematicDirector==null)cinematicDirector=GetComponent<RougeCinematicDirector>();
            if(cinematicDirector==null)cinematicDirector=gameObject.AddComponent<RougeCinematicDirector>();
            cinematicDirector.Initialize(battle,virtualCamera,placement,cinemaCanvas.transform);
        }

        void LateUpdate()
        {
            if(!initialized) return;
            float dt=Running?Time.deltaTime:0;
            animationRig.Evaluate(dt);
            if(barrageTime>=0 && dt>0)
            {
                barrageTime+=dt;
                float t=Mathf.Clamp01((barrageTime-.16f)/1.35f);
                Quaternion flip=Quaternion.Euler(-360*Mathf.SmoothStep(0,1,t),0,0);
                Vector3 pivot=Vector3.up*Player.height*.5f;
                Player.visual.localRotation=visualRotation*flip;
                Player.visual.localPosition=visualHome+pivot-flip*pivot+Vector3.up*Mathf.Sin(t*Mathf.PI)*.75f;
                if(barrageTime>=1.65f){barrageTime=-1;Player.visual.localPosition=visualHome;Player.visual.localRotation=visualRotation;}
            }
            if(defenseTime>0 && dt>0)
            {
                defenseTime-=dt;
                if(defense==RougeBattleDemo.DefenseResult.Dodge)
                {
                    float t=1-Mathf.Clamp01(defenseTime/.65f);
                    Player.visual.localPosition=visualHome+new Vector3(Mathf.Sin(t*Mathf.PI)*.9f,Mathf.Sin(t*Mathf.PI)*.18f,0);
                    Player.visual.localRotation=visualRotation*Quaternion.Euler(0,0,-Mathf.Sin(t*Mathf.PI)*24);
                }
                if(defenseTime<=0) EndDefense();
            }
            if(CinematicActive && dt>0)
            {
                cinemaTime+=dt;
                if(cinematicDirector!=null)cinematicDirector.SetClock(cinemaTime);
            }
        }

        public void SetPaused(bool paused)
        {
            if(audioSource==null)return;
            if(paused)audioSource.Pause();else audioSource.UnPause();
        }

        public void ResetPresentation()
        {
            StopAllCoroutines();
            foreach(var go in fx)if(go!=null)Destroy(go); fx.Clear();
            EndCinema();cinemaTime=0; defenseTime=0; barrageTime=-1;
            if(cinemaCanvas!=null)cinemaCanvas.gameObject.SetActive(false);
            if(audioSource!=null)audioSource.Stop();
            if(animationRig!=null)animationRig.Idle(true);
            if(battle!=null && Player!=null && Player.visual!=null){Player.visual.localPosition=visualHome;Player.visual.localRotation=visualRotation;}
            LastSkill="Ready"; DamageEvents=0; MissilesLaunched=0;
        }

        public IEnumerator Arrival()
        {
            LastSkill="Arrival";
            animationRig.Play(animationRig.arrival,0,3);
            Vector3 home=Player.RestVisualWorldPosition;
            Vector3 localHome=Arena.InverseTransformPoint(home);
            float elapsed=0;
            while(elapsed<2.9f)
            {
                elapsed+=Time.deltaTime;
                float drop=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/1.3f));
                Player.MoveVisualToWorld(Arena.TransformPoint(localHome+Vector3.up*drop*2.8f));
                if(elapsed<1.35f && core!=null) RunFx(Flash(core.position,Cyan,.24f,.12f));
                yield return null;
            }
            BurstAt(Player.transform.position,Cyan,.8f);
            Player.visual.localPosition=visualHome; Player.visual.localRotation=visualRotation;
            animationRig.Idle(); LastSkill="Ready";
        }

        public IEnumerator Perform(int skill, DemoFighter target, DemoFighter[] enemies, Action<DemoFighter,int> damage)
        {
            EndDefense();
            if(skill==0) yield return Rifle(target,damage);
            else if(skill==1) yield return Blade(target,damage);
            else if(skill==2) yield return Barrage(enemies,damage);
            else yield return Ultimate(target,enemies,damage);
            Player.visual.localPosition=visualHome; Player.visual.localRotation=visualRotation;
            animationRig.Idle();
        }

        void Hit(DemoFighter victim,int amount,Action<DemoFighter,int> damage,Color color)
        {
            if(victim==null || !victim.IsAlive)return;
            if(amount>0){DamageEvents++;damage(victim,amount);}
            BurstAt(victim.AimPoint,color,.4f);
        }
        void Face(DemoFighter target)
        {
            Vector3 d=Arena.InverseTransformPoint(target.transform.position)-Arena.InverseTransformPoint(Player.transform.position);d.y=0;
            Player.visual.localRotation=d.sqrMagnitude>.001f?Quaternion.LookRotation(d.normalized,Vector3.up):visualRotation;
        }
        IEnumerator Rifle(DemoFighter target,Action<DemoFighter,int> damage)
        {
            LastSkill="Beam Rifle / 3-round burst"; Face(target); animationRig.Play(animationRig.rifle);
            yield return Wait(.31f);
            int total=battle.beamDamage;
            for(int i=0;i<3;i++)
            {
                Sound(shotSound);
                yield return Beam(rifleMuzzle,target,Green,.045f,.14f,()=>Hit(target,i==2?total-2*(total/3):total/3,damage,Green));
                if(i<2)yield return Wait(.17f);
            }
            yield return Wait(.25f);
        }
        IEnumerator Blade(DemoFighter target,Action<DemoFighter,int> damage)
        {
            LastSkill="Anti-Ship Blade Assault"; Face(target); animationRig.Play(animationRig.blade);
            Vector3 home=Arena.InverseTransformPoint(Player.RestVisualWorldPosition);
            Vector3 d=Arena.InverseTransformPoint(target.transform.position)-home;d.y=0;d.Normalize();
            Vector3 strike=Arena.InverseTransformPoint(target.transform.position)-d*.95f;strike.y=home.y+.16f;
            yield return Move(home,strike,.45f);
            yield return Wait(.12f); Sound(slashSound);
            yield return SwordTrail(.27f,()=>Hit(target,battle.saberDamage/2,damage,Pink));
            yield return Wait(.18f); Sound(slashSound);
            yield return SwordTrail(.27f,()=>Hit(target,battle.saberDamage-battle.saberDamage/2,damage,Pink));
            yield return Wait(.14f); yield return Move(strike,home,.34f);
        }
        IEnumerator Barrage(DemoFighter[] enemies,Action<DemoFighter,int> damage)
        {
            LastSkill="Ootori Missile Barrage"; barrageTime=0; animationRig.Play(animationRig.barrage);
            yield return Wait(.48f);
            for(int volley=0;volley<3;volley++)
            {
                int amount=volley==2?battle.barrageDamage-2*(battle.barrageDamage/3):battle.barrageDamage/3;
                for(int i=0;i<enemies.Length;i++)
                {
                    var victim=enemies[i]; if(victim==null || !victim.IsAlive)continue;
                    RunFx(Missile(victim,volley*3+i,amount,damage));
                }
                Sound(shotSound); yield return Wait(.19f);
            }
            yield return Wait(.95f);
        }
        IEnumerator Ultimate(DemoFighter target,DemoFighter[] enemies,Action<DemoFighter,int> damage)
        {
            LastSkill="Full Armament Burst"; Face(target); animationRig.Play(animationRig.launcher,0,2);
            BeginCinema(target); Sound(chargeSound);
            for(int i=0;i<4;i++)
            {
                if(core!=null) { RunFx(Ring(core.position,Pink,.22f+i*.15f,.6f)); RunFx(Flash(core.position,Gold,.3f+i*.08f,.3f)); }
                yield return Wait(.3f);
            }
            yield return Wait(.35f);
            cinemaLabel.text="FULL ARMAMENT BURST";
            cinematicDirector.SetShot(RougeCinematicDirector.Shot.Salvo);
            Sound(shotSound);
            int railDamage=battle.ultimatePrimaryDamage/3;
            yield return Beam(railMuzzle,target,Gold,.07f,.21f,()=>Hit(target,railDamage,damage,Gold));
            yield return Wait(.15f);
            int remaining=battle.ultimatePrimaryDamage-railDamage;
            for(int tick=0;tick<4;tick++)
            {
                if(tick==3)
                {
                    cinematicDirector.SetShot(RougeCinematicDirector.Shot.Impact);
                    foreach(var enemy in enemies)
                    {
                        if(enemy==null || !enemy.IsAlive)continue;
                        int splash=enemy==target?0:battle.ultimateSplashDamage;
                        RunFx(Missile(enemy,6+Array.IndexOf(enemies,enemy),splash,damage));
                    }
                }
                int amount=tick==3?remaining-3*(remaining/4):remaining/4;
                yield return Beam(beamMuzzle,target,Pink,.2f,.25f,()=>Hit(target,amount,damage,Pink));
            }
            yield return Wait(.85f);
            // The imported launcher returns through a short blend to the imported idle.
            animationRig.Idle(); yield return Wait(.35f);
            EndCinema();
        }

        public void React(RougeBattleDemo.DefenseResult result)
        {
            defense=result; defenseTime=result==RougeBattleDemo.DefenseResult.Dodge || result==RougeBattleDemo.DefenseResult.Parry || result==RougeBattleDemo.DefenseResult.PerfectParry ? .65f : 0;
            if(result==RougeBattleDemo.DefenseResult.Dodge)
            { animationRig.Play(animationRig.dodge); RunFx(Ring(Player.AimPoint,Cyan,.65f,.35f)); }
            else if(result==RougeBattleDemo.DefenseResult.Parry || result==RougeBattleDemo.DefenseResult.PerfectParry)
            { animationRig.Play(animationRig.guard); BurstAt(shield!=null?shield.position:Player.AimPoint,result==RougeBattleDemo.DefenseResult.PerfectParry?Gold:Cyan,.55f); Sound(shotSound); }
            else { RunFx(Flash(Player.AimPoint,Gold,.3f,.18f)); }
        }
        public void EndDefense()
        {
            defenseTime=0;
            if(!initialized)return;
            animationRig.Idle(); Player.visual.localPosition=visualHome; Player.visual.localRotation=visualRotation;
        }
        IEnumerator Move(Vector3 from,Vector3 to,float seconds)
        {
            float elapsed=0;
            while(elapsed<seconds){elapsed+=Time.deltaTime;Player.MoveVisualToWorld(Arena.TransformPoint(Vector3.Lerp(from,to,Mathf.SmoothStep(0,1,elapsed/seconds))));yield return null;}
        }
        IEnumerator Wait(float seconds){float elapsed=0;while(elapsed<seconds){elapsed+=Time.deltaTime;yield return null;}}
        void RunFx(IEnumerator sequence){StartCoroutine(Pausable(sequence));}
        IEnumerator Pausable(IEnumerator sequence)
        {
            var stack=new Stack<IEnumerator>();stack.Push(sequence);
            while(stack.Count>0)
            {
                if(!Running){yield return null;continue;}
                var item=stack.Peek();if(!item.MoveNext()){stack.Pop();continue;}
                if(item.Current is IEnumerator nested)stack.Push(nested);else yield return item.Current;
            }
        }
        GameObject Effect(string label)
        {
            var go=new GameObject(label+" FX");go.transform.SetParent(Arena,false);fx.Add(go);return go;
        }
        void Release(GameObject go){fx.Remove(go);if(go!=null)Destroy(go);}
        Material Glow(Color color)
        {
            if(glowMaterials.TryGetValue(color,out var found))return found;
            var shader=glowShader!=null?glowShader:Shader.Find("Universal Render Pipeline/Unlit");
            var material=new Material(shader);material.SetColor("_BaseColor",color);materials.Add(material);glowMaterials[color]=material;return material;
        }
        LineRenderer Line(GameObject go,Color color,int count,float width)
        {
            var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=Glow(color);line.positionCount=count;
            line.startWidth=line.endWidth=width*Scale;line.numCapVertices=4;line.numCornerVertices=3;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
            return line;
        }
        IEnumerator Beam(Transform socket,DemoFighter victim,Color color,float width,float seconds,Action hit)
        {
            var outer=Effect("Weapon beam halo");var inner=Effect("Weapon beam core");
            var halo=Line(outer,color,2,width*2.2f);var white=Line(inner,new Color(1,1,1,.95f),2,width*.38f);
            if(socket!=null)RunFx(Flash(socket.position,color,width*4,.15f));
            bool impacted=false;float elapsed=0;
            while(elapsed<seconds)
            {
                elapsed+=Time.deltaTime;
                Vector3 from=Arena.InverseTransformPoint(socket!=null?socket.position:Player.MuzzlePoint);
                Vector3 to=Arena.InverseTransformPoint(victim.AimPoint);
                float reach=Mathf.Clamp01(elapsed/.07f);Vector3 end=Vector3.Lerp(from,to,reach);
                halo.SetPosition(0,from);halo.SetPosition(1,end);white.SetPosition(0,from);white.SetPosition(1,end);
                float pulse=1+.12f*Mathf.Sin(elapsed*90);
                halo.startWidth=halo.endWidth=width*2.2f*pulse*Scale;white.startWidth=white.endWidth=width*.38f*Scale;
                if(!impacted && reach>=1){impacted=true;hit();}
                yield return null;
            }
            if(!impacted)hit();Release(outer);Release(inner);
        }
        IEnumerator SwordTrail(float seconds,Action hit)
        {
            var go=Effect("Blade sweep");var line=Line(go,Pink,12,.07f);var points=new Queue<Vector3>();float elapsed=0;bool impacted=false;
            while(elapsed<seconds)
            {
                elapsed+=Time.deltaTime;
                Vector3 tip=Arena.InverseTransformPoint(swordTip!=null?swordTip.position:Player.MuzzlePoint);
                points.Enqueue(tip);while(points.Count>12)points.Dequeue();
                var array=points.ToArray();line.positionCount=array.Length;line.SetPositions(array);
                line.startWidth=.018f*Scale;line.endWidth=.13f*Scale;
                if(!impacted && elapsed>=.1f){impacted=true;hit();}
                yield return null;
            }
            if(!impacted)hit();Release(go);
        }
        IEnumerator Missile(DemoFighter victim,int seed,int amount,Action<DemoFighter,int> damage)
        {
            MissilesLaunched++;
            Transform port=missilePorts.Length>0?missilePorts[seed%missilePorts.Length]:core;
            Vector3 from=Arena.InverseTransformPoint(port!=null?port.position:Player.AimPoint);
            Vector3 to=Arena.InverseTransformPoint(victim.AimPoint);
            Vector3 control=(from+to)*.5f+new Vector3((seed%3-1)*.65f,1.1f+(seed%2)*.4f,0);
            var body=Effect("Homing missile");var capsule=GameObject.CreatePrimitive(PrimitiveType.Capsule);capsule.name="Warhead";capsule.transform.SetParent(body.transform,false);
            Destroy(capsule.GetComponent<Collider>());capsule.transform.localRotation=Quaternion.Euler(90,0,0);capsule.transform.localScale=new Vector3(.055f,.13f,.055f);capsule.GetComponent<Renderer>().sharedMaterial=metal;
            var trail=Effect("Missile smoke trail");var line=Line(trail,new Color(.68f,.77f,.85f,.55f),9,.045f);
            float duration=.6f+(seed%3)*.06f, elapsed=0;
            while(elapsed<duration)
            {
                elapsed+=Time.deltaTime;float t=Mathf.Clamp01(elapsed/duration);
                Vector3 pos=Bezier(from,control,to,t);Vector3 next=Bezier(from,control,to,Mathf.Min(1,t+.03f));
                body.transform.localPosition=pos;if((next-pos).sqrMagnitude>.00001f)body.transform.localRotation=Quaternion.LookRotation(next-pos);
                for(int i=0;i<9;i++)line.SetPosition(i,Bezier(from,control,to,Mathf.Clamp01(t-i*.025f)));
                line.startWidth=.06f*Scale;line.endWidth=.008f*Scale;
                yield return null;
            }
            Hit(victim,amount,damage,Gold);Sound(impactSound);Release(body);Release(trail);
        }
        static Vector3 Bezier(Vector3 a,Vector3 b,Vector3 c,float t){float n=1-t;return n*n*a+2*n*t*b+t*t*c;}
        IEnumerator Flash(Vector3 world,Color color,float size,float seconds)
        {
            var go=Effect("Muzzle flash");var line=Line(go,color,8,.06f);Vector3 p=Arena.InverseTransformPoint(world);
            float elapsed=0;
            while(elapsed<seconds)
            {
                elapsed+=Time.deltaTime;float radius=size*(1-elapsed/seconds);
                for(int i=0;i<8;i++)line.SetPosition(i,p+(i%2==0?Vector3.zero:new Vector3(Mathf.Cos(i*Mathf.PI/4),Mathf.Sin(i*Mathf.PI/4),0)*radius));
                line.startWidth=line.endWidth=.07f*(1-elapsed/seconds)*Scale;yield return null;
            }
            Release(go);
        }
        public void BurstAt(Vector3 world,Color color,float size)
        {
            RunFx(Ring(world,color,size,.35f));RunFx(Flash(world,color,size*.7f,.2f));
            RunFx(Sparks(world,color,size));
        }
        IEnumerator Ring(Vector3 world,Color color,float size,float seconds)
        {
            var go=Effect("Energy shock ring");var line=Line(go,color,40,.035f);line.loop=true;Vector3 center=Arena.InverseTransformPoint(world);float elapsed=0;
            while(elapsed<seconds)
            {
                elapsed+=Time.deltaTime;float t=Mathf.Clamp01(elapsed/seconds),r=Mathf.Lerp(.03f,size,t);
                for(int i=0;i<40;i++){float a=i*Mathf.PI*2/40;line.SetPosition(i,center+new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,0));}
                line.startWidth=line.endWidth=Mathf.Lerp(.055f,0,t)*Scale;yield return null;
            }
            Release(go);
        }
        IEnumerator Sparks(Vector3 world,Color color,float size)
        {
            var go=Effect("Impact sparks");var line=Line(go,color,18,.025f);Vector3 center=Arena.InverseTransformPoint(world);float elapsed=0;
            while(elapsed<.3f)
            {
                elapsed+=Time.deltaTime;float t=elapsed/.3f;
                for(int i=0;i<6;i++){float a=i*2.399f;Vector3 d=new Vector3(Mathf.Sin(a),Mathf.Cos(a),Mathf.Sin(a*2)*.5f);line.SetPosition(i*3,center+d*size*t*.4f);line.SetPosition(i*3+1,center+d*size*t);line.SetPosition(i*3+2,center+d*size*t*.4f);}
                line.startWidth=line.endWidth=.025f*(1-t)*Scale;yield return null;
            }
            Release(go);
        }

        void BuildCinemaHUD()
        {
            var root=new GameObject("Rouge Burst Cinematic",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));root.transform.SetParent(transform,false);
            cinemaCanvas=root.GetComponent<Canvas>();cinemaCanvas.renderMode=RenderMode.ScreenSpaceOverlay;cinemaCanvas.sortingOrder=40;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);
            for(int i=0;i<2;i++)
            {
                var go=new GameObject("Cinema bar",typeof(RectTransform),typeof(Image));go.transform.SetParent(root.transform,false);var r=go.GetComponent<RectTransform>();r.anchorMin=new Vector2(0,i==0?0:.9f);r.anchorMax=new Vector2(1,i==0?.075f:1);r.offsetMin=r.offsetMax=Vector2.zero;go.GetComponent<Image>().color=new Color(0,0,0,.88f);go.GetComponent<Image>().raycastTarget=false;
            }
            var label=new GameObject("Burst title",typeof(RectTransform),typeof(Text));label.transform.SetParent(root.transform,false);cinemaLabel=label.GetComponent<Text>();cinemaLabel.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");cinemaLabel.fontSize=26;cinemaLabel.fontStyle=FontStyle.Bold;cinemaLabel.alignment=TextAnchor.MiddleCenter;cinemaLabel.color=new Color(1,.7f,.8f);cinemaLabel.raycastTarget=false;
            var rect=label.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.05f,.9f);rect.anchorMax=new Vector2(.85f,.99f);rect.offsetMin=rect.offsetMax=Vector2.zero;
            var reset=new GameObject("Reset during cinematic",typeof(RectTransform),typeof(Image),typeof(Button));reset.transform.SetParent(root.transform,false);
            var resetRect=reset.GetComponent<RectTransform>();resetRect.anchorMin=new Vector2(.87f,.925f);resetRect.anchorMax=new Vector2(.98f,.98f);resetRect.offsetMin=resetRect.offsetMax=Vector2.zero;
            reset.GetComponent<Image>().color=new Color(.3f,.12f,.2f);reset.GetComponent<Button>().onClick.AddListener(battle.ResetBattle);
            var caption=new GameObject("RESET",typeof(RectTransform),typeof(Text));caption.transform.SetParent(reset.transform,false);var text=caption.GetComponent<Text>();text.font=cinemaLabel.font;text.fontSize=20;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;text.text="RESET";text.raycastTarget=false;
            var captionRect=caption.GetComponent<RectTransform>();captionRect.anchorMin=Vector2.zero;captionRect.anchorMax=Vector2.one;captionRect.offsetMin=captionRect.offsetMax=Vector2.zero;
            root.SetActive(false);
        }
        void BeginCinema(DemoFighter target)
        {
            CinematicActive=true;cinemaTime=0;cinemaCanvas.gameObject.SetActive(true);cinemaLabel.text="CORE 100%  /  OOTORI SYSTEM ONLINE";
            cinematicDirector.Begin(target);
        }
        void EndCinema()
        {
            if(cinematicDirector!=null)cinematicDirector.End();
            CinematicActive=false;if(cinemaCanvas!=null)cinemaCanvas.gameObject.SetActive(false);
        }
        void Sound(AudioClip clip){if(soundEnabled && clip!=null && audioSource!=null)audioSource.PlayOneShot(clip,.7f);}
        static AudioClip Tone(string name,float seconds,float first,float last,float noise)
        {
            const int hz=22050;var samples=new float[Mathf.CeilToInt(seconds*hz)];var random=new System.Random(130);float phase=0;
            for(int i=0;i<samples.Length;i++){float t=i/(float)samples.Length;phase+=2*Mathf.PI*Mathf.Lerp(first,last,t)/hz;float envelope=Mathf.Sin(Mathf.PI*t)*Mathf.Pow(1-t,.6f);samples[i]=(Mathf.Sin(phase)*(1-noise)+(float)(random.NextDouble()*2-1)*noise)*envelope*.4f;}
            var clip=AudioClip.Create(name,samples.Length,1,hz,false);clip.SetData(samples,0);return clip;
        }
        void OnDestroy()
        {
            ResetPresentation();foreach(var material in materials)if(material!=null)Destroy(material);
            foreach(var clip in new[]{shotSound,slashSound,impactSound,chargeSound})if(clip!=null)Destroy(clip);
        }
    }
}
