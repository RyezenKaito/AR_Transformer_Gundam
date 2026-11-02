using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using static TurnBasedBattle.Editor.BattleAssetUtility;

namespace TurnBasedBattle.Editor
{
    public static class BattleVfxBuilder
    {
        public static Material energy,glow,dust,white,rock;
        public static Mesh[] rockMeshes;
        public static Mesh shard;
        public static void BuildMaterials()
        {
            var soft=Texture("SoftGlow",128,128,(u,v)=>{float r=new Vector2((u-.5f)*2,(v-.5f)*2).magnitude;float a=Mathf.Pow(Mathf.Clamp01(1-r),2.2f);return new Color(1,1,1,a);});
            var line=Texture("LightningStrip",128,64,(u,v)=>{float a=Mathf.Pow(Mathf.Clamp01(1-Mathf.Abs(v*2-1)),2);return new Color(1,1,1,a);},true);
            var smoke=Texture("DustNoise",128,128,(u,v)=>{float r=new Vector2((u-.5f)*2,(v-.5f)*2).magnitude;float n=Mathf.PerlinNoise(u*7+3,v*7+4)*.65f+Mathf.PerlinNoise(u*17,v*17)*.35f;return new Color(n,n,n,Mathf.Pow(Mathf.Clamp01(1-r),1.5f)*Mathf.SmoothStep(0,1,n)*.85f);});
            var streak=Texture("SwirlStreakMask",512,128,(u,v)=>
            {
                float warp=v+.17f*Mathf.Sin(u*Mathf.PI*6)+.05f*Mathf.Sin(u*Mathf.PI*18);
                float noise=Mathf.PerlinNoise(u*5+11,warp*19)*.65f+Mathf.PerlinNoise(u*14+3,warp*41)*.35f;
                float streakAmount=Mathf.SmoothStep(.44f,.67f,noise);
                float breakup=Mathf.SmoothStep(.26f,.62f,Mathf.PerlinNoise(u*9+27,v*4+11));
                float edge=Mathf.Pow(Mathf.Sin(v*Mathf.PI),.55f);float a=streakAmount*breakup*edge;
                return new Color(Mathf.Clamp01(noise),1,1,a);
            },true);
            energy=Energy("GoldLightning",new Color(3.2f,2.7f,.35f,1),line);
            glow=Energy("GoldGlow",new Color(2.6f,1.8f,.14f,1),soft);
            white=Energy("WhiteEnergy",Color.white,line);
            Energy("SwirlGold",new Color(2.5f,1.75f,.18f,1),streak,new Vector2(-.28f,.035f));
            var dm=new Material(Shader.Find("TurnBattle/SoftDust")){name="SoftDust"};dm.SetTexture("_BaseMap",smoke);dm.SetColor("_BaseColor",Color.white);dust=Save(dm,Root+"/Materials/SoftDust.mat");
            var rm=new Material(Shader.Find("TurnBattle/RockDissolve")){name="EarthRock"};rm.SetColor("_BaseColor",new Color(.30f,.25f,.17f));rm.SetColor("_EdgeColor",new Color(4,2.8f,.1f));rock=Save(rm,Root+"/Materials/EarthRock.mat");
            rockMeshes=new[]{Rock("PillarRockA",51,true),Rock("PillarRockB",93,true),Rock("PillarRockC",119,true)};shard=Rock("RubbleRock",29,false);
        }
        public static EarthPrisonController BuildEarth(Camera camera)
        {
            var root=Node("EarthPrison_VFX");var fx=root.AddComponent<EarthPrisonController>();fx.viewCamera=camera;fx.radius=3.8f;fx.pillarHeight=3.8f;
            fx.area=Node("EnemyAreaAnchor",root.transform).transform;
            fx.orb=Quad("ChargeCore",root.transform,glow);fx.orbGlow=Quad("ChargeHalo",root.transform,glow);fx.impactGlow=Quad("GroundFlash",root.transform,glow);
            fx.chargeRings=Lines("ChargeOrbit",3,root.transform,energy);fx.chargeArcs=Lines("ChargeArc",6,root.transform,energy);
            fx.transferCore=Line("TransferCore",root.transform,energy);fx.transferGlow=Line("TransferGlow",root.transform,energy);
            fx.groundRing=Line("GroundRing",root.transform,energy,97);
            fx.domes=new Transform[3];fx.domeRenderers=new Renderer[3];
            var swirl=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/SwirlGold.mat");
            for(int i=0;i<3;i++){var dome=MeshObject("SwirlRibbon_"+i,fx.area,Dome("SwirlRibbon"+i,i),swirl);fx.domes[i]=dome.transform;fx.domeRenderers[i]=dome.GetComponent<Renderer>();fx.domeRenderers[i].shadowCastingMode=ShadowCastingMode.Off;}
            fx.pillars=new Transform[6];fx.rubbleBases=new Transform[6];fx.tips=new Transform[6];fx.tipGlows=new Transform[6];fx.pillarRenderers=new Renderer[6];
            fx.baseDust=new ParticleSystem[6];fx.debris=new ParticleSystem[6];fx.tipSparks=new ParticleSystem[6];var rubbleList=new List<Renderer>();
            for(int i=0;i<6;i++)
            {
                float a=(i*60+30)*Mathf.Deg2Rad;Vector3 pos=new Vector3(Mathf.Cos(a)*fx.radius,0,Mathf.Sin(a)*fx.radius);
                var pillar=Node("Pillar_"+i,fx.area,pos);fx.pillars[i]=pillar.transform;
                float height=fx.pillarHeight*(.94f+.022f*i);
                var body=MeshObject("RockBody",pillar.transform,rockMeshes[i%3],rock);body.transform.localScale=new Vector3(.85f,height,.85f);body.transform.localRotation=Quaternion.Euler(0,17+i*57,0);fx.pillarRenderers[i]=body.GetComponent<Renderer>();
                var b=Node("RubbleBase_"+i,fx.area,pos);fx.rubbleBases[i]=b.transform;
                for(int j=0;j<11;j++)
                {
                    float angle=j*Mathf.PI*2/11;float r=.28f+(j%3)*.23f;
                    var pebble=MeshObject("Stone_"+j,b.transform,shard,rock,new Vector3(Mathf.Cos(angle)*r,-.02f,Mathf.Sin(angle)*r));
                    pebble.transform.localRotation=Quaternion.Euler(j*13,j*53,i*17);
                    pebble.transform.localScale=new Vector3(.32f+(j%3)*.12f,.13f+(j%4)*.055f,.36f+(j%2)*.2f);
                }
                BattleMeshOptimizer.Combine(b.transform,"PrisonRubble"+i);rubbleList.AddRange(b.GetComponentsInChildren<Renderer>());
                var tip=Shape("TipCore",PrimitiveType.Sphere,pillar.transform,Vector3.up*height,Vector3.one*.14f,energy);fx.tips[i]=tip.transform;
                fx.tipGlows[i]=Quad("TipGlow_"+i,fx.area,glow);
                fx.baseDust[i]=Particles("BaseDust_"+i,fx.area,dust,new Color(.67f,.39f,.10f,.75f),.95f,.9f,1.4f,40);fx.baseDust[i].transform.localPosition=pos+Vector3.up*.05f;
                fx.debris[i]=Particles("RockDebris_"+i,fx.area,rock,new Color(.5f,.45f,.35f,1),1.1f,.22f,3.5f,24,shard);fx.debris[i].transform.localPosition=pos+Vector3.up*.1f;
                fx.tipSparks[i]=Particles("TipSparks_"+i,root.transform,energy,Color.white,.3f,.055f,1.3f,28);
            }
            fx.rubbleRenderers=rubbleList.ToArray();fx.cageCore=Lines("CageCore",6,root.transform,energy);fx.cageGlow=Lines("CageGlow",6,root.transform,energy);
            fx.verticalArcs=Lines("PillarArc",6,root.transform,energy);fx.crossArcs=Lines("CrossArc",3,root.transform,energy);
            fx.chargeSparks=Particles("ChargeSparks",root.transform,energy,new Color(1,.9f,.4f,1),.45f,.07f,1.8f,80);var chargeShape=fx.chargeSparks.shape;chargeShape.shapeType=ParticleSystemShapeType.Sphere;chargeShape.radius=.5f;
            fx.transferSparks=Particles("TransferSparks",root.transform,energy,new Color(1,.9f,.4f,1),.4f,.06f,2.6f,80);var tsh=fx.transferSparks.shape;tsh.shapeType=ParticleSystemShapeType.Box;tsh.scale=new Vector3(7,.2f,.5f);
            fx.impactSparks=Particles("ImpactStreaks",root.transform,energy,Color.white,.45f,.11f,5,60);var ir=fx.impactSparks.GetComponent<ParticleSystemRenderer>();ir.renderMode=ParticleSystemRenderMode.Stretch;ir.lengthScale=3;ir.velocityScale=.1f;
            fx.impactDust=Particles("ImpactDust",root.transform,dust,new Color(.46f,.40f,.28f,.75f),.9f,1.4f,1.3f,32);
            fx.residualDust=Particles("ResidualDust",root.transform,dust,new Color(.34f,.32f,.28f,.55f),1.1f,1.3f,.5f,40);var shape=fx.residualDust.shape;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=3;
            foreach(var renderer in root.GetComponentsInChildren<Renderer>()){renderer.enabled=false;renderer.shadowCastingMode=ShadowCastingMode.Off;}
            PrefabUtility.SaveAsPrefabAsset(root,Root+"/Prefabs/EarthPrison_VFX.prefab");return fx;
        }
        static Transform Quad(string name,Transform parent,Material material)=>Shape(name,PrimitiveType.Quad,parent,Vector3.zero,Vector3.one,material).transform;
        static LineRenderer[] Lines(string name,int count,Transform parent,Material material){var lines=new LineRenderer[count];for(int i=0;i<count;i++)lines[i]=Line(name+"_"+i,parent,material);return lines;}
        public static BattleEffects BuildEffects()
        {
            var effects=Node("BattlePresentationEffects").AddComponent<BattleEffects>();effects.projectiles=new[]{Line("ProjectileCore",effects.transform,white,14),Line("ProjectileGlow",effects.transform,white,14)};
            effects.impactPool=new ParticleSystem[8];
            for(int i=0;i<8;i++)effects.impactPool[i]=Particles("HitPool_"+i,effects.transform,energy,new Color(1,.78f,.3f,1),.32f,.07f,3.2f,64);
            effects.oneShot=Node("OneShots",effects.transform).AddComponent<AudioSource>();effects.oneShot.playOnAwake=false;effects.oneShot.volume=.65f;effects.oneShot.spatialBlend=0;
            effects.sustain=Node("Sustain",effects.transform).AddComponent<AudioSource>();effects.sustain.playOnAwake=false;effects.sustain.spatialBlend=0;
            effects.shotClip=Sound("Synth_Discharge",.20f,550,95,.2f);effects.impactClip=Sound("Synth_Impact",.30f,130,28,.65f);
            effects.dodgeClip=Sound("Synth_Dodge",.22f,180,650,.6f);effects.chargeClip=Sound("Synth_ChargeLoop",.8f,140,410,.05f,true);
            effects.groundClip=Sound("Synth_StoneRise",.7f,70,22,.8f);effects.electricClip=Sound("Synth_ElectricLoop",.6f,100,120,.5f,true);
            foreach(var l in effects.projectiles)l.enabled=false;return effects;
        }
    }
}
