using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;

namespace TurnBasedBattle.Editor
{
    public static class BattleAcceptance
    {
        static TurnBattleManager m;static BattleOrbitCamera camera;
        static readonly List<string> results=new List<string>();
        static int failures;
        static void Need(bool ok,string label){if(!ok)throw new Exception(label);}
        static void Eq(float a,float b,string label){Need(Mathf.Abs(a-b)<.002f,$"{label}: {a} != {b}");}
        static void Home(BattleActor a){Need(Vector3.Distance(a.AnchorPosition,a.transform.position)<.001f,"Root did not return");Need(Quaternion.Angle(a.AnchorRotation,a.transform.rotation)<.1f,"Heading did not return");Need(a.weapons.Equipped==BattleWeaponKind.None && a.weapons.BladeAmount==0,"Weapon not holstered");}
        static void Case(string name,Action test)
        {
            try{m.manualSimulation=true;m.tuning.enemyCombos=false;m.SetPlaybackSpeed(1);if(m.EasyDodge)m.ToggleEasy();m.Restart(1);camera.ResetView(true);test();results.Add("PASS — "+name);}
            catch(Exception e){failures++;results.Add("FAIL — "+name+": "+e.Message);}
        }
        static void Cast(BattleActionKind kind){m.Select(kind);Need(m.Confirm(),"Action rejected: "+kind);}
        static void At(float t,float hz=60)
        {
            int id=m.ActionId;int limit=10000;
            while(m.ActionId==id && m.InAction && m.ActionTime<t-.00001f && limit-->0){m.AdvanceForTest(Mathf.Min(1/hz,t-m.ActionTime));camera.Step(1/hz);}
            Need(limit>0,"Action hung");
        }
        static void End(){At(m.Current.duration+.002f);}
        static void Enemies(){int n=0;while(m.Phase==BattlePhase.EnemyAction && n++<5)End();Need(n<5,"Enemy queue hung");}
        [MenuItem("Turn Battle/Run Latest Acceptance Tests")]
        public static void Run()
        {
            Need(EditorApplication.isPlaying,"Enter Play Mode first");m=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();Need(m && m.tuning,"Missing serialized tuning");camera=m.battleCamera.GetComponent<BattleOrbitCamera>();Need(camera,"Missing camera");
            camera.manualSimulation=true;camera.inputEnabled=false;bool combo=m.tuning.enemyCombos;results.Clear();failures=0;
            try
            {
                Case("Serialized configuration and idle loadout",()=>{
                    Need(m.tuning.attack && m.tuning.skill && m.tuning.ultimate,"Action reference lost");Need(m.player.clips.All(c=>c!=null),"Clip missing");
                    foreach(var path in new[]{"BattleTuning","BasicAttack","EnergyCannon","EarthPrison"})Need(!File.ReadAllText("Assets/TurnBasedBattle/Config/"+path+".asset").Contains("m_Script: {fileID: 0}"),"Unbound script asset");
                    for(int i=0;i<600;i++)m.AdvanceForTest(1f/60);Home(m.player);Need(!m.player.weapons.blade.gameObject.activeSelf,"Blade in idle");
                    Need(Vector3.Distance(m.player.weapons.cannon.position,m.player.weapons.cannonHolster.position)<.001f,"Rifle held in idle");
                    Need(m.player.GetComponentsInChildren<Animator>(true).All(a=>!a.enabled),"Two pose owners");
                });
                Case("Mouse/UI selection and single commitment",()=>{
                    Need(!m.Confirm(),"Confirmation without selection");m.hud.attackButton.onClick.Invoke();Need(m.Phase==BattlePhase.PlayerTargeting,"Lost UI listener");m.hud.confirmButton.onClick.Invoke();int id=m.ActionId;Need(!m.Confirm() && m.ActionId==id,"Duplicate action");End();Eq(m.enemies[0].HP,500,"Attack damage");Eq(m.Ultimate,25,"Attack meter");Home(m.player);End();Need(m.PlayerTurn==2 && m.CommandsAvailable,"Turn not returned");Eq(m.player.HP,920,"Enemy damage");Eq(m.Ultimate,35,"Enemy gain");
                });
                foreach(var kind in new[]{BattleActionKind.Attack,BattleActionKind.Skill,BattleActionKind.EarthPrison})
                {
                    var actionKind=kind;
                    Case(actionKind+" movement policy, grip, contact timing and full return",()=>{
                        if(actionKind==BattleActionKind.EarthPrison)m.FillUltimate();Cast(actionKind);var a=m.Current;
                        Need(a.approachTarget==(actionKind!=BattleActionKind.Skill),"Wrong movement policy");Vector3 anchor=m.player.AnchorPosition;
                        if(a.weapon!=BattleWeaponKind.None){At(a.gripTime);Need(m.player.weapons.LastGripError<.025f,"Hand misses holster: "+m.player.weapons.LastGripError);}
                        At(a.arriveTime);Need(actionKind==BattleActionKind.Skill?Vector3.Distance(m.player.transform.position,anchor)<.001f:Vector3.Distance(m.player.transform.position,anchor)>1,"Incorrect movement");Need(m.HitRecords.Count==0,"Damage before contact");
                        if(actionKind==BattleActionKind.EarthPrison){Need(!m.earthPrison.Active,"VFX started in flight");Need(Vector3.Distance(m.player.transform.position,m.enemyFormation.position)>m.earthPrison.radius+.5f,"Caster inside prison");}
                        At(a.HitTime(0)-.015f);Need(m.HitRecords.Count==0,"Early damage");At(a.HitTime(0));Need(m.HitRecords.Count==1,"Missing contact damage");
                        if(actionKind==BattleActionKind.Attack){Need(m.player.weapons.BladeAmount>.99f,"Blade off at hit");Vector3 start=m.player.weapons.saber.position,end=m.player.weapons.bladeTip.position,target=m.enemies[0].AimPoint;float u=Mathf.Clamp01(Vector3.Dot(target-start,end-start)/(end-start).sqrMagnitude);Need(Vector3.Distance(target,Vector3.Lerp(start,end,u))<.4f,"Blade misses target volume");}
                        At(a.returnEnd+.01f);Need(m.Phase==BattlePhase.PlayerAction,"Turn ended before sheathe/settle");
                        if(a.weapon!=BattleWeaponKind.None){At(a.holsterTime);Need(m.player.weapons.LastGripError<.025f,"Holster return misses grip");}
                        End();Home(m.player);Need(m.Phase==BattlePhase.EnemyAction,"No enemy handoff");Need(!m.earthPrison.Active,"Effect survived return");
                    });
                }
                Case("Guard applies across the whole three-enemy phase",()=>{m.Restart(3);Cast(BattleActionKind.Guard);End();Enemies();Eq(m.player.HP,880,"Guard damage");Eq(m.Ultimate,30,"Meter per enemy");Need(!m.Guarding && m.EnemyActionsStarted==3,"Guard/queue lifetime");foreach(var e in m.enemies)Home(e);});
                Case("Skill cooldown is two subsequent command opportunities",()=>{Cast(BattleActionKind.Skill);End();Enemies();Need(!m.CanSelect(BattleActionKind.Skill),"Cooldown round2");Cast(BattleActionKind.Guard);End();Enemies();Need(!m.CanSelect(BattleActionKind.Skill),"Cooldown round3");Cast(BattleActionKind.Guard);End();Enemies();Need(m.CanSelect(BattleActionKind.Skill),"Cooldown round4");});
                foreach(int hz in new[]{30,60,120})
                {
                    int rate=hz;Case("Dodge timestamps and damage at "+rate+" Hz simulation",()=>{
                        foreach(int mode in new[]{0,1,2,3}){m.Restart(1);Cast(BattleActionKind.Guard);End();var h=m.Current.hits[0];double press=mode==0?h.open-.08:mode==1?(h.open+h.close)*.5:mode==2?h.close+.03:-1;bool sent=false;int limit=Mathf.CeilToInt(rate*(m.Current.duration+1));
                            while(m.Phase==BattlePhase.EnemyAction && limit-->0){if(!sent && press>=0 && m.ActionTime+1f/rate>=press){Need(m.SubmitDodge(press),"Press not consumed");Need(!m.SubmitDodge(press+.01),"Retry accepted");sent=true;}m.AdvanceForTest(1f/rate);}
                            Need(limit>0,"Enemy stuck");Eq(m.player.HP,mode==1?1000:960,"Dodge HP");Eq(m.Ultimate,mode==1?15:10,"Dodge gain");Need(m.HitRecords.Count==1,"Duplicate hit");Home(m.enemies[0]);}
                    });
                }
                Case("Dodge boundaries and missing press",()=>{var h=m.tuning.enemyStrike.hits[0];foreach(double t in new[]{(double)h.open,h.close}){var a=new DodgeAttempt();Need(a.Submit(t,h)&&a.Resolve()==DodgeOutcome.Dodged,"Boundary failed");}var n=new DodgeAttempt();Need(n.Resolve()==DodgeOutcome.Failed&&!n.Submit(h.open,h),"Resolved attempt accepted");});
                Case("Combo independent hits and single meter award",()=>{m.tuning.enemyCombos=true;Cast(BattleActionKind.Guard);End();Enemies();Cast(BattleActionKind.Guard);End();Need(m.Current.hits.Length==2,"Expected combo");var first=m.Current.hits[0];At(first.open+.05f);m.SubmitDodge(m.ActionTime);At(first.time+.05f);var second=m.Current.hits[1];At(second.cue+.05f);m.SubmitDodge(m.ActionTime);End();Eq(m.player.HP,932.5f,"Combo HP");Eq(m.Ultimate,25,"Combo gain");});
                Case("Earth Prison shifted VFX clock, five hits x three and no implicit stun",()=>{m.Restart(3);m.FillUltimate();Cast(BattleActionKind.EarthPrison);var a=m.Current;Eq(m.Ultimate,0,"Ultimate cost");At(a.vfxStart-.02f);Need(!m.earthPrison.Active,"Early charge");At(a.vfxStart+.5f);Eq(m.earthPrison.EffectTime,.5f,"VFX clock");At(a.HitTime(4)+.03f);Need(m.HitRecords.Count==15,"Expected 15 damage records");foreach(var e in m.enemies)Eq(e.HP,240,"AoE damage");Eq(m.Ultimate,0,"Ultimate self charge");Need(m.EnemyActionsStarted==0,"Early enemy turn");End();Need(!m.earthPrison.Active,"VFX cleanup");Enemies();Need(m.EnemyActionsStarted==3,"Enemies lost turn");});
                Case("Dead targets skip later pulses and enemy queue",()=>{m.Restart(3);m.enemies[1].Initialize(50);m.FillUltimate();Cast(BattleActionKind.EarthPrison);End();Enemies();Need(m.HitRecords.Count(h=>h.targetId==11)==1,"Hits on corpse");Need(m.EnemyActionsStarted==2,"Dead actor attacked");});
                Case("Victory waits for return; defeat terminates queue",()=>{m.enemies[0].Initialize(50);m.FillUltimate();Cast(BattleActionKind.EarthPrison);At(m.Current.HitTime(0)+.03f);Need(m.Phase==BattlePhase.PlayerAction,"Victory interrupted return");End();Need(m.Phase==BattlePhase.Victory,"No victory");Home(m.player);m.Restart(3);m.player.Initialize(20);Cast(BattleActionKind.Guard);End();At(m.Current.HitTime(0)+.03f);Need(m.Phase==BattlePhase.Defeat && m.EnemyActionsStarted==1,"Defeat leaked queue");});
                Case("Pause and restart during draw, flight, cast and sheathe",()=>{
                    foreach(float t in new[]{.25f,.9f,4.95f,7.3f,10.4f}){m.Restart(1);m.FillUltimate();Cast(BattleActionKind.EarthPrison);At(t);Vector3 pos=m.player.transform.position;float clock=m.ActionTime;int records=m.HitRecords.Count;m.TogglePause();m.AdvanceForTest(5);Eq(m.ActionTime,clock,"Pause clock");Need(m.player.transform.position==pos && m.HitRecords.Count==records,"Pause movement/damage");m.TogglePause();m.SetPlaybackSpeed(.5f);Eq(m.ActionTime,clock,"Speed jump");int id=m.BattleId;m.Restart(1);Need(m.BattleId>id && m.HitRecords.Count==0 && !m.earthPrison.Active,"Reset leaked events");Home(m.player);}
                });
                Case("Ten repeated attacks: no weapon instances or hit duplication",()=>{int objects=m.player.GetComponentsInChildren<Transform>(true).Length;for(int i=0;i<10;i++){m.Restart(1);Cast(BattleActionKind.Attack);End();Enemies();Home(m.player);Need(m.player.GetComponentsInChildren<Transform>(true).Length==objects,"Weapon instance leak");Need(m.HitRecords.Count==2,"Hit duplication");}});
                Case("Camera home composition, two full orbits and retained yaw through actions",()=>{
                    Need(!m.battleCamera.orthographic,"Orthographic lens");Need(Vector3.Dot(camera.transform.position-m.player.transform.position,m.player.transform.forward)<0,"Not behind player");Eq(camera.Pitch,18,"Home pitch");Vector3 start=camera.transform.position;
                    camera.Orbit(720,0);for(int i=0;i<240;i++)camera.Step(1f/60);Need(Vector3.Distance(start,camera.transform.position)<.03f,"720 seam");
                    camera.Orbit(95,0);for(int i=0;i<120;i++)camera.Step(1f/60);float yaw=camera.Yaw;Cast(BattleActionKind.Skill);At(1.5f);Eq(camera.Yaw,yaw,"Action stole yaw");Need(Vector3.Distance(camera.Pivot,m.player.transform.position)<3,"Camera failed to follow");End();Enemies();Eq(camera.Yaw,yaw,"Turn reset camera");
                    camera.Orbit(0,200);camera.Zoom(-300);for(int i=0;i<120;i++)camera.Step(1f/60);Need(camera.Pitch<=camera.maxPitch+.01f&&camera.Distance>=camera.minDistance-.01f,"Camera limits");
                });
                Case("Camera obstruction retracts and restores smoothly",()=>{
                    var block=GameObject.CreatePrimitive(PrimitiveType.Cube);block.name="CameraObstruction_TEST";block.layer=2;block.transform.position=camera.Pivot+(camera.transform.position-camera.Pivot).normalized*3;block.transform.localScale=Vector3.one*1.8f;
                    try{Physics.SyncTransforms();for(int i=0;i<120;i++)camera.Step(1f/60);Need(Vector3.Distance(camera.transform.position,camera.Pivot)<4,"Camera failed to retract");}
                    finally{UnityEngine.Object.DestroyImmediate(block);}for(int i=0;i<120;i++)camera.Step(1f/60);Need(Vector3.Distance(camera.transform.position,camera.Pivot)>6,"Boom failed to restore");
                });
                Case("External camera ownership and desktop restoration",()=>{
                    var external=new GameObject("ExternalCamera_TEST").AddComponent<Camera>();external.enabled=false;external.fieldOfView=63;external.transform.SetPositionAndRotation(new Vector3(2,3,4),Quaternion.Euler(15,20,0));
                    try{camera.UseExternalCamera(external);camera.Orbit(80,30);camera.Step(1);Need(external.transform.position==new Vector3(2,3,4)&&!external.enabled,"External pose/state overwritten");Eq(external.fieldOfView,63,"Projection overwritten");Need(m.battleCamera==external&&m.earthPrison.viewCamera==external,"Consumers stale");}
                    finally{camera.RestoreDesktopCamera();UnityEngine.Object.DestroyImmediate(external.gameObject);}Need(m.battleCamera==camera.GetComponent<Camera>(),"Desktop not restored");
                });
                Case("Board translation/rotation/scale preserve flight, anchors and VFX",()=>{
                    var board=camera.arenaRoot;Vector3 p=board.position,s=board.localScale;Quaternion q=board.rotation;
                    try{board.SetPositionAndRotation(new Vector3(2,.5f,-1),Quaternion.Euler(7,37,0));board.localScale=Vector3.one*.2f;m.Restart(3);m.FillUltimate();Cast(BattleActionKind.EarthPrison);At(m.Current.vfxStart+5.4f);Need(Vector3.Distance(m.earthPrison.area.position,m.enemyFormation.position)<.001f,"Prison missed board");Need(m.earthPrison.cageCore.All(l=>!l.useWorldSpace),"World-space cage");Need(m.earthPrison.GetComponentsInChildren<ParticleSystem>().All(particle=>particle.main.simulationSpace==ParticleSystemSimulationSpace.Custom),"World-space particles");End();Home(m.player);}
                    finally{board.SetPositionAndRotation(p,q);board.localScale=s;m.Restart(1);camera.ResetView(true);}
                });
                Case("Running uses grounded root, alternating feet, planted stance and bounded speed",()=>{
                    Cast(BattleActionKind.Attack);var a=m.Current;Need(a.travelStyle==BattleTravelStyle.Run,"Wrong run style");
                    var l=m.player.rig.Joint("Foot_L");var r=m.player.rig.Joint("Foot_R");m.player.rig.ResetPose();float groundAnkle=l.position.y;Vector3 prev=m.player.transform.position,foot=l.position;float low=999,high=-999;int planted=0;float drift=0,maxSpeed=0;
                    for(int i=0;i<Mathf.CeilToInt(a.arriveTime*120);i++){
                        m.AdvanceForTest(1f/120);Vector3 p=m.player.transform.position;maxSpeed=Mathf.Max(maxSpeed,Vector3.Distance(p,prev)*120);prev=p;
                        Need(Mathf.Abs(p.y-m.player.AnchorPosition.y)<.001f,"Run became a hop");Need(m.player.weapons.boosters.All(b=>!b.gameObject.activeSelf),"Running with boosters");
                        float diff=l.position.y-r.position.y;low=Mathf.Min(low,diff);high=Mathf.Max(high,diff);
                        // A swing foot also has zero vertical speed at its apex. Measure slip only at ground contact.
                        if(m.player.TravelWeight>.999f && l.position.y<groundAnkle+.005f && foot.y<groundAnkle+.005f && Mathf.Abs(l.position.y-foot.y)<.0005f){drift=Mathf.Max(drift,Vector3.Distance(l.position,foot));planted++;}foot=l.position;
                    }
                    Need(low<-.08f && high>.08f,"Feet do not alternate");Need(maxSpeed<8,"Run snaps too fast: "+maxSpeed);Need(planted>8 && drift<.035f,"Stance foot slides: "+drift+", samples="+planted);End();Home(m.player);
                });
                Case("R rises above one actor height, holds cast altitude, then lands",()=>{
                    m.FillUltimate();Cast(BattleActionKind.EarthPrison);var a=m.Current;Need(a.travelStyle==BattleTravelStyle.Fly,"Wrong flight style");float span=a.arriveTime-a.travelStart;
                    At(a.travelStart+.002f);Need(m.player.transform.localPosition.y<.001f,"Takeoff vertical pop");
                    At(a.arriveTime);Need(m.player.CastPositionReady,"Cast anchor not reached");Need(!m.earthPrison.Active,"Charged before settling");
                    float floor=m.player.transform.parent.InverseTransformPoint(m.player.rig.Joint("Foot_L").position).y;Need(floor>m.player.rig.referenceHeight,"R not high enough");
                    foreach(float localTime in new[]{.1f,1.5f,3.4f,5.5f,7.8f}){At(a.vfxStart+localTime);Need(m.player.AirborneWeight>.999f,"Caster landed during VFX");Need(m.player.transform.parent.InverseTransformPoint(m.player.rig.Joint("Foot_L").position).y>m.player.rig.referenceHeight,"Feet lost altitude");Need(m.player.weapons.boosters.Any(b=>b.gameObject.activeSelf),"Hover thrust off");Need(Mathf.Abs(m.earthPrison.area.localPosition.y)<.01f,"Ground VFX rose with caster");}
                    At(a.returnEnd);Need(Mathf.Abs(m.player.transform.localPosition.y)<.001f,"Landing floats");End();Home(m.player);
                });
                Case("Long strides have slower cadence and opposite arm-leg swing",()=>{
                    Cast(BattleActionKind.Attack);var a=m.Current;Need(a.runStride>=2,"Stride too short");float distance=Vector3.Distance(m.player.AnchorPosition,m.enemies[0].AnchorPosition)-a.stopDistance;float speed=distance/((a.arriveTime-a.travelStart)*.76f);float cycle=a.runStride/speed;Need(cycle>.85f && cycle<1.3f,"Cadence out of range: "+cycle);
                    At(a.travelStart+1.3f);var l=m.player.rig.Joint("UpperArm_L");var r=m.player.rig.Joint("UpperArm_R");float min=999,max=-999;
                    for(int i=0;i<120;i++){m.AdvanceForTest(1f/120);float lp=Mathf.DeltaAngle(0,l.localEulerAngles.x),rp=Mathf.DeltaAngle(0,r.localEulerAngles.x);Need((lp+8)*(rp+12)<=2,"Arms swinging together");min=Mathf.Min(min,lp);max=Mathf.Max(max,lp);}Need(max-min>25,"No deliberate arm swing");End();Home(m.player);
                });
                Case("High R preserves yaw, ground target framing and shifted damage timing",()=>{
                    m.Restart(3);m.FillUltimate();Cast(BattleActionKind.EarthPrison);var a=m.Current;At(a.vfxStart+1.5f);float yaw=camera.Yaw;
                    for(int i=0;i<120;i++)camera.Step(1f/60);Vector3 caster=m.battleCamera.WorldToViewportPoint(m.player.AimPoint),target=m.battleCamera.WorldToViewportPoint(m.enemyFormation.position+Vector3.up);
                    Need(caster.z>0 && caster.y>.17f && caster.y<.88f && target.z>0 && target.y>.17f && target.y<.88f,"High camera lost actor/ground: "+caster+" / "+target);
                    Eq(a.HitTime(0),5.9f,"Airborne impact marker");At(5.89f);Need(m.HitRecords.Count==0,"Damage during ascent/charge");At(5.91f);Need(m.HitRecords.Count==3,"Airborne impact missing");Eq(camera.Yaw,yaw,"High R stole yaw");End();Home(m.player);
                });
                Case("Run lower-body and root transitions have no angular pop",()=>{
                    Cast(BattleActionKind.Attack);var tracked=new[]{m.player.transform,m.player.rig.Joint("UpperLeg_L"),m.player.rig.Joint("LowerLeg_L"),m.player.rig.Joint("Foot_L"),m.player.rig.Joint("UpperLeg_R"),m.player.rig.Joint("LowerLeg_R"),m.player.rig.Joint("Foot_R")};
                    var prev=tracked.Select(j=>j.localRotation).ToArray();int id=m.ActionId;float maxRoot=0,maxLeg=0;
                    while(m.ActionId==id){m.AdvanceForTest(1f/120);for(int i=0;i<tracked.Length;i++){float angle=Quaternion.Angle(prev[i],tracked[i].localRotation);if(i==0)maxRoot=Mathf.Max(maxRoot,angle);else maxLeg=Mathf.Max(maxLeg,angle);prev[i]=tracked[i].localRotation;}}
                    Need(maxRoot<12 && maxLeg<25,"Angular discontinuity root="+maxRoot+", legs="+maxLeg);Home(m.player);
                });
                Case("Ranged action stays planted and aims from the equipped muzzle",()=>{
                    Cast(BattleActionKind.Skill);int id=m.ActionId;int frames=0;
                    while(m.ActionId==id && frames++<400){m.AdvanceForTest(1f/60);Need(Vector3.Distance(m.player.transform.position,m.player.AnchorPosition)<.001f,"Ranged root moved");Need(m.player.weapons.boosters.All(b=>!b.gameObject.activeSelf),"Ranged booster active");if(m.ActionTime>1.3f && m.ActionTime<1.9f){Need(m.player.weapons.Equipped==BattleWeaponKind.Cannon,"Rifle not drawn");Need(Vector3.Dot(m.player.weapons.cannon.forward,(m.enemies[0].AimPoint-m.player.weapons.cannon.position).normalized)>.98f,"Muzzle points away");}}
                    Need(frames<400,"Ranged action stuck");Home(m.player);
                    Need(!m.tuning.enemyCombo.approachTarget,"Enemy ranged policy");
                });
                Case("Beam blade restores full geometry after serialized idle retraction",()=>{
                    Need(m.player.weapons.blade.localScale.z==0,"Idle blade not retracted");Cast(BattleActionKind.Attack);At(m.Current.HitTime(0)-.1f);
                    var w=m.player.weapons;Need(w.BladeAmount>.99f && w.blade.localScale.z>.99f,"Blade remains zero length after reload");
                    var core=w.blade.GetComponentInChildren<MeshRenderer>();Need(core && core.bounds.size.magnitude>1,"Collapsed plasma geometry");Need(w.bladeLight && w.bladeLight.enabled,"No spill light");
                    At(m.Current.bladeOff+.2f);Need(!w.blade.gameObject.activeSelf && !w.bladeLight.enabled,"Blade failed to extinguish");End();Home(m.player);
                });
                Case("Enemy HP and damage text follow head through camera orbit",()=>{
                    m.Restart(3);Canvas.ForceUpdateCanvases();m.hud.ShowResult(m.enemies[0],"120",false);
                    foreach(float yaw in new[]{0f,60f,120f,180f}){
                        camera.Orbit(yaw,0);for(int i=0;i<100;i++)camera.Step(1f/60);m.hud.SendMessage("LateUpdate");
                        for(int i=0;i<3;i++){var e=m.enemies[i];Vector3 p=m.battleCamera.WorldToScreenPoint(e.HealthBarPoint);var card=(RectTransform)m.hud.enemyButtons[i].transform;
                            bool visible=p.z>m.battleCamera.nearClipPlane && p.x>=0 && p.x<=Screen.width && p.y>=0 && p.y<=Screen.height;
                            Need(card.gameObject.activeSelf==visible,"HP visibility");if(visible)Need(Vector2.Distance(RectTransformUtility.WorldToScreenPoint(null,card.position),p)<2,"HP not over head");}
                    }
                });
                Case("Real mouse wheel normalized/raw, drag and UI exclusion",()=>{
                    var original=Mouse.current;var mouse=InputSystem.AddDevice<Mouse>();
                    void Send(MouseState state){InputSystem.QueueStateEvent(mouse,state);InputSystem.Update();camera.SendMessage("ReadInput");for(int i=0;i<100;i++)camera.Step(1f/60);}
                    try{
                        mouse.MakeCurrent();Canvas.ForceUpdateCanvases();Vector2 p=new Vector2(Screen.width*.5f,Screen.height*.5f);
                        Send(new MouseState{position=p,scroll=Vector2.up});Need(camera.Distance<camera.homeDistance-.5f,"Normalized wheel did not zoom");float before=camera.Distance;
                        Send(new MouseState{position=p,scroll=new Vector2(0,-120)});Need(camera.Distance>before+.5f,"Raw wheel did not zoom out");
                        float yaw=camera.Yaw;Send(new MouseState{position=p}.WithButton(MouseButton.Right));Send(new MouseState{position=p+new Vector2(140,0),delta=new Vector2(140,0)}.WithButton(MouseButton.Right));Need(Mathf.Abs(camera.Yaw-yaw)>10,"Mouse drag did not orbit");
                        Send(new MouseState{position=p});Vector2 button=RectTransformUtility.WorldToScreenPoint(null,m.hud.attackButton.transform.position);before=camera.Distance;yaw=camera.Yaw;
                        Send(new MouseState{position=button,scroll=Vector2.up,delta=new Vector2(140,0)}.WithButton(MouseButton.Right));Eq(camera.Distance,before,"Zoom over button");Eq(camera.Yaw,yaw,"Drag started on button");
                    }finally{InputSystem.RemoveDevice(mouse);if(original!=null)original.MakeCurrent();}
                });
                Case("Real Input System edge, held key and UI dodge button",()=>{
                    var original=Keyboard.current;var keyboard=InputSystem.AddDevice<Keyboard>();
                    try{keyboard.MakeCurrent();Cast(BattleActionKind.Guard);End();At(m.Current.hits[0].open+.06f);m.manualSimulation=false;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space),InputState.currentTime);InputSystem.Update();m.SendMessage("Update");m.manualSimulation=true;Need(m.GetAttempt(0).outcome==DodgeOutcome.Dodged,"Input timestamp mismatch");End();Eq(m.player.HP,1000,"Keyboard dodge damage");
                        m.Restart(1);Cast(BattleActionKind.Guard);End();At(m.Current.hits[0].open+.06f);m.manualSimulation=false;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space),InputState.currentTime);InputSystem.Update();m.SendMessage("Update");m.manualSimulation=true;Need(!m.GetAttempt(0).pressed,"Hold generated edge");End();Eq(m.player.HP,960,"Held-key dodge");
                        m.Restart(1);Cast(BattleActionKind.Guard);End();At(m.Current.hits[0].open+.06f);m.manualSimulation=false;m.hud.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b=>b.name=="TouchDodge").onClick.Invoke();m.manualSimulation=true;Need(m.GetAttempt(0).outcome==DodgeOutcome.Dodged,"Touch UI listener");
                    }finally{InputSystem.RemoveDevice(keyboard);if(original!=null)original.MakeCurrent();m.manualSimulation=true;}
                });
            }
            finally
            {
                m.tuning.enemyCombos=combo;m.Restart(1);m.SetPlaybackSpeed(1);m.manualSimulation=false;camera.manualSimulation=false;camera.inputEnabled=true;camera.ResetView(true);
                File.WriteAllText("Assets/TurnBasedBattle/Reports/Acceptance_20261029.md","# Full prompt acceptance\n\n"+DateTime.Now.ToString("s")+" · Unity "+Application.unityVersion+"\n\n"+results.Count(s=>s.StartsWith("PASS"))+" passed; "+failures+" failed.\n\n"+string.Join("\n",results.Select(s=>"- "+s))+"\n\n30/60/120 Hz are simulation steps, not measured FPS. External camera tests do not establish AR device support. See visual review and performance notes separately.\n");
            }
            Debug.Log("BATTLE_ACCEPTANCE: "+(results.Count-failures)+" passed, "+failures+" failed");
        }
    }
}
