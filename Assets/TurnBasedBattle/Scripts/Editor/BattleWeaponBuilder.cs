using UnityEngine;
using static TurnBasedBattle.Editor.BattleAssetUtility;

namespace TurnBasedBattle.Editor
{
    public static class BattleWeaponBuilder
    {
        public static void Build(BattleActor actor)
        {
            var rig=actor.rig;rig.ResetPose();var root=actor.transform;
            var frame=Lit("WeaponFrame",new Color(.055f,.08f,.11f),default,.65f);
            var glow=Lit(actor.player?"PlayerBlade":"EnemyBlade",actor.player?new Color(.07f,.8f,.95f):new Color(.95f,.18f,.06f),actor.player?new Color(.1f,2.5f,3):new Color(3,.3f,.05f));
            var wr=actor.gameObject.AddComponent<BattleWeaponRig>();wr.actor=actor;actor.weapons=wr;
            float shoulder=Mathf.Abs(root.InverseTransformPoint(rig.Joint("UpperArm_R").position).x);
            float hip=root.InverseTransformPoint(rig.Joint("Pelvis").position).y;
            wr.saberHolster=Node("SaberHolster",rig.Joint("Pelvis"),new Vector3(shoulder+.12f,-.03f,-.04f)).transform;
            wr.cannonHolster=Node("CannonHolster",rig.Joint("Pelvis"),new Vector3(-shoulder-.14f,.04f,-.09f)).transform;
            wr.saberHolster.localRotation=wr.cannonHolster.localRotation=Quaternion.Euler(80,0,0);
            wr.saberHand=Node("SaberHandGrip",rig.Joint("Hand_R"),new Vector3(0,-.035f,.02f)).transform;wr.saberHand.localRotation=Quaternion.Euler(80,0,0);
            wr.cannonHand=Node("CannonHandGrip",rig.Joint("Hand_L"),new Vector3(0,-.035f,.02f)).transform;wr.cannonHand.localRotation=Quaternion.Euler(80,0,0);
            wr.saber=Node("BeamSaber",root).transform;
            Shape("Grip",PrimitiveType.Cylinder,wr.saber,Vector3.zero,new Vector3(.075f,.13f,.075f),frame,Quaternion.Euler(90,0,0));
            Shape("Emitter",PrimitiveType.Cylinder,wr.saber,new Vector3(0,0,.15f),new Vector3(.11f,.045f,.11f),frame,Quaternion.Euler(90,0,0));
            // A rounded white-hot core, a camera-facing soft halo and local spill light.
            // The assembly extends from the emitter; its root Z scale drives ignition/retraction.
            wr.blade=Node("Blade",wr.saber,new Vector3(0,0,.17f)).transform;
            var core=Lit(actor.player?"PlayerBladeCore":"EnemyBladeCore",Color.white,actor.player?new Color(4,8,9):new Color(9,3,1));
            Shape("PlasmaCore",PrimitiveType.Capsule,wr.blade,new Vector3(0,0,.6f),new Vector3(.065f,.6f,.065f),core,Quaternion.Euler(90,0,0));
            var mask=Texture("BeamHalo",16,64,(u,v)=>new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-Mathf.Abs(v*2-1)),2)));
            var haloMaterial=Energy(actor.player?"PlayerBeamHalo":"EnemyBeamHalo",actor.player?new Color(.15f,1.8f,3,.7f):new Color(3,.25f,.06f,.7f),mask);
            var halo=Line("SoftGlow",wr.blade,haloMaterial,2);halo.useWorldSpace=false;halo.SetPosition(0,Vector3.zero);halo.SetPosition(1,new Vector3(0,0,1.2f));halo.widthMultiplier=.25f;halo.numCapVertices=8;
            wr.bladeLight=Node("BladeSpill",wr.blade,new Vector3(0,0,.35f)).AddComponent<Light>();wr.bladeLight.type=LightType.Point;wr.bladeLight.color=actor.player?new Color(.12f,.8f,1):new Color(1,.12f,.03f);wr.bladeLight.shadows=UnityEngine.LightShadows.None;
            wr.bladeTip=Node("BladeTip",wr.saber,new Vector3(0,0,1.37f)).transform;
            wr.cannon=Node("EnergyRifle",root).transform;
            Shape("Body",PrimitiveType.Cube,wr.cannon,new Vector3(0,.03f,.20f),new Vector3(.20f,.20f,.58f),frame);
            Shape("Barrel",PrimitiveType.Cylinder,wr.cannon,new Vector3(0,.03f,.61f),new Vector3(.1f,.20f,.1f),frame,Quaternion.Euler(90,0,0));
            Shape("EnergyRail",PrimitiveType.Cube,wr.cannon,new Vector3(0,.14f,.24f),new Vector3(.07f,.02f,.46f),glow);
            wr.muzzle=Node("Muzzle",wr.cannon,new Vector3(0,.03f,.84f)).transform;
            // Offhand sits across the front of the receiver; the two-bone solver owns only this hand while equipped.
            wr.offHandGrip=Node("OffHandGrip",wr.cannon,new Vector3(.14f,-.04f,.25f)).transform;wr.offHandGrip.localRotation=Quaternion.Euler(0,0,-20);
            actor.castSocket=wr.muzzle;actor.meleeSocket=wr.bladeTip;actor.meleeWeapon=wr.saber.gameObject;
            actor.focusSocket=Node("PalmCastFocus",rig.Joint("Hand_L"),new Vector3(0,-.08f,.08f)).transform;
            var jetMask=Texture("ThrusterSoftJet",64,64,(u,v)=>{
                float edge=Mathf.Pow(Mathf.Clamp01(1-Mathf.Abs(v*2-1)),2.5f);
                float tail=Mathf.Pow(1-u,1.7f)*Mathf.SmoothStep(0,1,u/.06f);
                return new Color(1,1,1,edge*tail);
            });
            var flashMask=Texture("ThrusterSoftFlash",64,64,(u,v)=>new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-Vector2.Distance(new Vector2(u,v),new Vector2(.5f,.5f))*2),2)));
            var boostMat=Energy("ThrusterJetAdditive",Color.white,jetMask);
            var flashMat=Energy("ThrusterFlashAdditive",Color.white,flashMask);
            wr.boosters=new Transform[2];
            for(int i=0;i<2;i++)
            {
                Transform socket=rig.thrusterSockets!=null && i<rig.thrusterSockets.Length && rig.thrusterSockets[i]?rig.thrusterSockets[i]:rig.Joint("Backpack");
                var plume=Node("ThrusterFlash_"+i,socket);wr.boosters[i]=plume.transform;
                if(socket==rig.Joint("Backpack")){plume.transform.localPosition=new Vector3(i==0?-.15f:.15f,0,-.12f);plume.transform.localRotation=Quaternion.Euler(0,180,0);}
                var fx=plume.AddComponent<BattleThrusterVfx>();
                fx.core=Line("JetCore",plume.transform,boostMat,2);fx.halo=Line("JetHalo",plume.transform,boostMat,2);fx.flash=Line("NozzleFlash",plume.transform,flashMat,2);
                foreach(var line in new[]{fx.core,fx.halo,fx.flash}){line.useWorldSpace=false;line.alignment=LineAlignment.View;line.numCapVertices=0;line.textureMode=LineTextureMode.Stretch;}
                var taper=new AnimationCurve(new Keyframe(0,1),new Keyframe(.2f,.85f),new Keyframe(1,.05f));fx.core.widthCurve=taper;fx.halo.widthCurve=taper;
                fx.flash.SetPosition(0,Vector3.left*.09f);fx.flash.SetPosition(1,Vector3.right*.09f);
                fx.Evaluate(0,0);
            }
            wr.ResetWeapons();
        }
    }
}
