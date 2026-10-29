using System.Linq;
using UnityEngine;
using static BlueFlame.Editor.BFG;

namespace BlueFlame.Editor
{
    public static partial class BlueFlameBuilder
    {
        static Transform Child(Transform root,string starts)=>root.Cast<Transform>().First(t=>t.name.StartsWith(starts));
        static void Refine(Transform root,bool study)
        {
            var chest=Child(root,"01 /");
            for(int s=-1;s<=1;s+=2)
            {
                M("Lower breast ceramic structural shell",chest,s,white,.12f,V(.35f,5.18f,-.49f),V(.51f,5.10f,-.52f),V(.79f,5.02f,-.36f),V(.66f,4.79f,-.31f),V(.42f,4.72f,-.39f),V(.26f,4.96f,-.48f));
                M("Lower breast inner blue inset",chest,s,blue,.045f,V(.34f,5.10f,-.56f),V(.42f,5.04f,-.57f),V(.52f,4.82f,-.45f),V(.42f,4.79f,-.47f),V(.30f,4.98f,-.54f));
                M("Abdominal ceramic cheek",chest,s,porcelain,.075f,V(.47f,4.71f,-.33f),V(.62f,4.76f,-.27f),V(.53f,4.50f,-.25f),V(.38f,4.37f,-.31f),V(.34f,4.53f,-.35f));
                Seam(chest,V(s*.56f,5.02f,-.49f),V(s*.61f,4.94f,-.438f),V(s*.55f,4.85f,-.425f));
                Decal(chest,V(s*.62f,4.95f,-.46f),new Vector2(.13f,.07f),2);
                // A raised collar physically encloses the neck bearing.
                M("Neck ceramic protector",chest,s,white,.16f,V(.20f,5.80f,-.13f),V(.27f,5.95f,-.08f),V(.43f,5.86f,-.08f),V(.48f,5.73f,-.10f));
            }
            var head=Child(root,"03 /");
            for(int s=-1;s<=1;s+=2)
            {
                M("Helmet crown segmented shell",head,s,white,.07f,V(.10f,.51f,-.15f),V(.16f,.54f,-.10f),V(.26f,.37f,-.19f),V(.23f,.24f,-.22f),V(.12f,.28f,-.25f));
                M("Cheek vent cutout",head,s,black,.012f,V(.17f,-.04f,-.35f),V(.205f,-.02f,-.33f),V(.20f,-.14f,-.32f),V(.17f,-.16f,-.34f));
                for(int i=0;i<3;i++)Line("Cheek intake louver",head,steel,.008f,V(s*.174f,-.05f-i*.028f,-.36f),V(s*.20f,-.03f-i*.028f,-.35f));
                Bolt(head,V(s*.17f,.39f,-.20f),.012f);
            }
            if(study)return;
            for(int s=-1;s<=1;s+=2)
            {
                var leg=Child(root,s<0?"05 L":"06 R");
                // Distinct sliding thigh shell sections break the uninterrupted cuboid silhouette.
                M("Thigh split upper shell",leg,s,white,.065f,V(.02f,-.27f,-.24f),V(.22f,-.31f,-.23f),V(.30f,-.51f,-.20f),V(.20f,-.72f,-.27f),V(.04f,-.67f,-.31f));
                M("Thigh lower sliding plate",leg,s,porcelain,.05f,V(.03f,-.70f,-.30f),V(.20f,-.76f,-.27f),V(.23f,-.98f,-.25f),V(.11f,-1.11f,-.29f),V(-.01f,-1.02f,-.31f));
                M("Thigh cobalt taper inset",leg,s,blue,.024f,V(-.19f,-.31f,-.30f),V(-.14f,-.39f,-.32f),V(-.16f,-.83f,-.32f),V(-.22f,-.92f,-.28f));
                var knee=Child(leg,"Knee double");
                var kneelens=G("Knee condenser protective housing",knee,V(s*.18f,-.02f,-.36f),V(0,-s*30,0));
                Tube("Knee gold bezel",kneelens,Vector3.zero,.115f,.09f,.045f,gold);
                Lathe("Knee blue glass",kneelens,V(0,0,-.027f),glass,new[]{new Vector2(0,.087f),new Vector2(.025f,.09f)});
                Tube("Knee energy ring",kneelens,V(0,0,-.029f),.08f,.065f,.009f,cyan);
                var shin=Child(leg,"Lower leg");
                foreach(var t in shin.Cast<Transform>().ToArray())if(t.name=="Wraparound white shin cheek"||t.name=="Calf side shell return"||t.name=="Recessed panel division")UnityEngine.Object.DestroyImmediate(t.gameObject);
                for(int j=-1;j<=1;j+=2)
                {
                    M("Tibial ceramic upper shell",shin,j,white,.11f,V(.075f,-.29f,-.42f),V(.24f,-.25f,-.42f),V(.34f,-.49f,-.42f),V(.265f,-1.04f,-.42f),V(.11f,-1.14f,-.42f),V(.095f,-.70f,-.42f));
                    M("Tibial lower sliding armor",shin,j,porcelain,.095f,V(.11f,-1.17f,-.36f),V(.255f,-1.09f,-.36f),V(.21f,-1.35f,-.36f),V(.08f,-1.49f,-.36f));
                    Seam(shin,V(j*.16f,-.40f,-.424f),V(j*.235f,-.54f,-.424f),V(j*.205f,-.79f,-.424f));
                    Bolt(shin,V(j*.23f,-.91f,-.425f),.018f);
                    Piston(shin,V(j*.18f,-.29f,.44f),V(j*.15f,-1.50f,.34f),.043f);
                    B("Rear calf piston bracket",shin,V(j*.18f,-.30f,.42f),V(.12f,.12f,.15f),steel);
                }
                var arm=Child(root,s<0?"07 L":"08 R");
                var shoulder=Child(arm,"Layered");
                M("Shoulder front locking cassette",shoulder,s,navy,.08f,V(-.11f,-.10f,-.43f),V(.04f,-.04f,-.46f),V(.18f,-.21f,-.46f),V(.13f,-.34f,-.43f),V(-.09f,-.28f,-.40f));
                M("Shoulder white locking slide",shoulder,s,white,.045f,V(-.09f,-.11f,-.49f),V(.02f,-.08f,-.50f),V(.11f,-.21f,-.51f),V(.09f,-.27f,-.49f),V(-.07f,-.23f,-.47f));
                Bolt(shoulder,V(s*.015f,-.18f,-.535f),.024f);
                M("Shoulder lateral pointed vane",shoulder,s,blue,.095f,V(.39f,.02f,-.16f),V(.71f,-.09f,-.06f),V(1.00f,-.23f,.04f),V(.63f,-.23f,-.10f),V(.45f,-.20f,-.23f));
            }
            var flight=Child(root,"09 /");
            for(int s=-1;s<=1;s+=2)
            {
                var wing=Child(flight,s<0?"L wing":"R wing");
                var rear=G("Rear wing layered armor and spar covers",wing,V(0,0,.48f),V(0,180,0));int k=-s;
                M("Rear wing root ceramic shell",rear,k,white,.095f,V(.97f,.98f,-.03f),V(1.47f,1.02f,-.04f),V(1.95f,.83f,-.06f),V(2.31f,.50f,-.08f),V(1.93f,.43f,-.08f),V(1.55f,.54f,-.04f),V(1.30f,.41f,-.02f));
                M("Rear wing midspan shell",rear,k,porcelain,.075f,V(1.57f,.47f,-.055f),V(1.89f,.38f,-.07f),V(2.47f,.25f,-.09f),V(2.75f,.07f,-.10f),V(2.42f,-.01f,-.09f),V(1.88f,.17f,-.06f));
                M("Rear outer blade armor",rear,k,white,.065f,V(2.77f,.20f,-.12f),V(3.20f,.01f,-.13f),V(5.70f,-1.03f,-.20f),V(4.76f,-.79f,-.18f),V(2.73f,-.06f,-.12f),V(2.55f,.08f,-.11f));
                M("Rear wing cobalt service rail",rear,k,blue,.025f,V(1.40f,.84f,-.14f),V(1.53f,.88f,-.14f),V(1.93f,.72f,-.15f),V(2.00f,.59f,-.16f),V(1.68f,.66f,-.15f));
                Line("Rear blade engraved channel",rear,black,.008f,V(k*2.97f,.015f,-.19f),V(k*3.74f,-.31f,-.21f),V(k*5.08f,-.87f,-.25f));
                Bearing(rear,V(k*1.16f,.66f,-.12f),.22f);
                Bearing(rear,V(k*2.34f,.20f,-.17f),.13f);
                for(int i=0;i<5;i++)Bolt(rear,V(k*(1.43f+i*.28f),.80f-i*.14f,-.15f),.018f);
                Decal(rear,V(k*1.73f,.68f,-.17f),new Vector2(.24f,.11f),7,k*20);
                var fuselage=G("Rear ventral binder panel",rear);
                M("Ventral binder reverse ceramic skin",fuselage,k,white,.065f,V(.85f,.24f,.50f),V(1.06f,.07f,.48f),V(1.32f,-.46f,.45f),V(2.45f,-2.63f,.26f),V(1.33f,-.91f,.44f),V(.94f,-.23f,.51f));
                for(int i=0;i<4;i++)Bolt(fuselage,V(k*(1.08f+i*.18f),-.22f-i*.37f,.39f-i*.03f),.017f);
            }
            foreach(var r in root.GetComponentsInChildren<MeshRenderer>())if(r.name.Contains("panel division")||r.name.Contains("engraved")||r.name.Contains("vent slot"))r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
