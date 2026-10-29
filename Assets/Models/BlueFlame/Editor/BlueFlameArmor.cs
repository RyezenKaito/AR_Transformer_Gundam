using System;
using System.Linq;
using UnityEngine;
using static BlueFlame.Editor.BFG;

namespace BlueFlame.Editor
{
    public static partial class BlueFlameBuilder
    {
        static Material white,porcelain,blue,navy,frame,steel,gold,red,black,cyan,glass,decal,floor;
        static Transform G(string n,Transform t,Vector3 p=default,Vector3 e=default)=>Group(n,t,p,e);
        static GameObject P(string n,Transform t,Material m,float d,params Vector3[] v)=>Panel(n,t,m,d,Mathf.Min(.018f,d*.22f),v);
        static GameObject M(string n,Transform t,int s,Material m,float d,params Vector3[] v)=>P(n,t,m,d,v.Select(p=>V(p.x*s,p.y,p.z)).ToArray());
        static void B(string n,Transform t,Vector3 p,Vector3 size,Material m,Vector3 e=default)=>Box(n,t,p,size,m,Mathf.Min(.025f,size.z*.2f),e);
        static void Seam(Transform t,params Vector3[] p)=>Line("Recessed panel division",t,black,.009f,p);
        static void Bolt(Transform t,Vector3 p,float r=.025f,Vector3 e=default)
        {
            var g=G("Countersunk captive fastener",t,p,e);
            Lathe("Dark socket",g,Vector3.zero,black,new[]{new Vector2(-.004f,r),new Vector2(.006f,r)},24);
            Lathe("Recessed titanium head",g,V(0,0,-.007f),steel,new[]{new Vector2(0,r*.67f),new Vector2(.004f,r*.78f)},6);
            B("Hex tool recess",g,V(0,0,-.009f),V(r*.55f,r*.18f,.003f),black);
        }
        static void Bearing(Transform t,Vector3 p,float r,Vector3 e=default,bool luminous=false)
        {
            var g=G("Rotary joint / bearing stack",t,p,e);
            Lathe("Axle block",g,V(0,0,.04f),frame,new[]{new Vector2(-.08f,r*.8f),new Vector2(-.06f,r),new Vector2(.12f,r),new Vector2(.15f,r*.8f)});
            Tube("Machined bearing rim",g,V(0,0,-.06f),r*.86f,r*.68f,.045f,steel);
            Tube("Locking race",g,V(0,0,-.086f),r*.65f,r*.53f,.016f,gold);
            Lathe("Bearing cover",g,V(0,0,-.083f),luminous?glass:navy,new[]{new Vector2(-.008f,r*.49f),new Vector2(.01f,r*.52f)});
            if(luminous)Tube("Energy annulus",g,V(0,0,-.1f),r*.50f,r*.40f,.012f,cyan);
            Bolt(g,V(0,0,-.1f),r*.14f);
            for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Bolt(g,V(Mathf.Cos(a)*r*.75f,Mathf.Sin(a)*r*.75f,-.09f),r*.065f);}
        }
        static void Piston(Transform t,Vector3 a,Vector3 b,float r)
        {
            var d=b-a;
            Rod("Hydraulic outer cylinder",t,a,a+d*.58f,r,frame);
            Rod("Polished actuator ram",t,a+d*.44f,b,r*.58f,steel);
            Rod("Cylinder retaining sleeve",t,a+d*.52f,a+d*.60f,r*1.14f,gold);
            for(int i=0;i<3;i++)Rod("Seal groove",t,a+d*(.12f+i*.06f),a+d*(.135f+i*.06f),r*1.035f,black);
        }
        static void Vent(Transform t,Vector3 p,Vector2 size,int count,Vector3 e=default)
        {
            var g=G("Recessed cooling cassette",t,p,e);
            B("Cast intake housing",g,V(0,0,.025f),V(size.x+.055f,size.y+.055f,.10f),frame);
            B("Intake shadow cavity",g,V(0,0,-.024f),V(size.x,size.y,.018f),black);
            for(int i=0;i<count;i++)B("Angled heat exchanger fin "+i,g,V(0,(i+.5f)*size.y/count-size.y*.5f,-.043f),V(size.x*.89f,size.y/count*.29f,.032f),steel,V(-22,0,0));
            B("Left inset lip",g,V(-size.x*.5f,0,-.04f),V(.018f,size.y,.035f),porcelain);
            B("Right inset lip",g,V(size.x*.5f,0,-.04f),V(.018f,size.y,.035f),porcelain);
        }
        static void Body(Transform root)
        {
            var body=G("01 / Thorax - reactor, breastplates, dorsal frame",root);
            Hull("Forged thoracic chassis",body,frame,S(4.68f,.42f,.28f),S(4.88f,.66f,.38f),S(5.34f,.96f,.40f),S(5.68f,.80f,.32f),S(5.78f,.43f,.25f));
            var spine=G("Dorsal service spine",body,V(0,5.12f,.43f),V(0,180,0));
            for(int i=0;i<5;i++){B("Segmented vertebral cover",spine,V(0,(i-2)*.17f,0),V(.31f,.125f,.14f),steel);Bolt(spine,V(0,(i-2)*.17f,-.08f),.023f);}
            for(int s=-1;s<=1;s+=2)
            {
                Piston(body,V(s*.39f,4.75f,-.08f),V(s*.89f,5.48f,-.09f),.067f);
                M("Cobalt pectoral wrap",body,s,blue,.18f,V(.13f,5.56f,-.49f),V(.50f,5.77f,-.42f),V(.94f,5.63f,-.27f),V(1.10f,5.38f,-.18f),V(.85f,5.19f,-.37f),V(.41f,5.25f,-.58f));
                M("Upper ceramic breastplate",body,s,white,.10f,V(.16f,5.61f,-.57f),V(.49f,5.73f,-.49f),V(.90f,5.60f,-.40f),V(.80f,5.46f,-.49f),V(.51f,5.45f,-.61f),V(.31f,5.39f,-.63f));
                M("Raised collar edge",body,s,porcelain,.065f,V(.22f,5.76f,-.34f),V(.39f,5.85f,-.25f),V(.82f,5.72f,-.28f),V(.93f,5.62f,-.37f),V(.77f,5.61f,-.45f),V(.43f,5.74f,-.39f));
                M("Lower pectoral cheek",body,s,porcelain,.11f,V(.43f,5.36f,-.61f),V(.79f,5.39f,-.51f),V(.91f,5.26f,-.36f),V(.80f,5.06f,-.38f),V(.52f,5.12f,-.55f));
                M("Flank armor return",body,s,navy,.28f,V(.81f,5.21f,-.24f),V(.96f,5.35f,-.13f),V(.90f,4.97f,-.05f),V(.65f,4.78f,-.23f),V(.58f,5.02f,-.30f));
                Vent(body,V(s*.64f,5.29f,-.61f),new Vector2(.25f,.13f),4,V(0,s*-12,0));
                var side=G("Underarm machined ribs",body,V(s*.64f,4.95f,.10f),V(0,s*63,0));
                for(int k=0;k<4;k++)B("Cast rib",side,V(0,k*.10f,0),V(.32f,.05f,.13f),steel,V(0,0,s*-12));
                Seam(body,V(s*.42f,5.64f,-.535f),V(s*.54f,5.60f,-.558f),V(s*.76f,5.58f,-.46f));
                Bolt(body,V(s*.84f,5.54f,-.448f));
                Decal(body,V(s*.55f,5.55f,-.613f),new Vector2(.22f,.09f),s<0?0:2,0);
            }
            // Separate reactor cage, retaining ring and lens reveal true depth under grazing light.
            var core=G("Sternal optical reactor",body,V(0,5.39f,-.55f),V(0,0,0));
            Lathe("Octagonal reactor cradle",core,Vector3.zero,navy,new[]{new Vector2(-.08f,.26f),new Vector2(.02f,.32f),new Vector2(.17f,.27f)},8);
            Tube("Titanium retaining bezel",core,V(0,0,-.09f),.226f,.179f,.07f,steel);
            Tube("Brass electrical insulation ring",core,V(0,0,-.13f),.18f,.156f,.026f,gold);
            Lathe("Optical lens dome",core,V(0,0,-.145f),glass,new[]{new Vector2(-.035f,.10f),new Vector2(-.025f,.143f),new Vector2(0,.16f),new Vector2(.02f,.16f)});
            Tube("Blue reactor corona",core,V(0,0,-.152f),.143f,.125f,.012f,cyan);
            for(int i=0;i<4;i++){float a=(i+.5f)*Mathf.PI*.5f;Bolt(core,V(Mathf.Cos(a)*.248f,Mathf.Sin(a)*.248f,-.10f),.025f);}
            P("Sternum cobalt keel",body,blue,.16f,V(-.27f,5.16f,-.56f),V(-.25f,4.99f,-.53f),V(-.10f,4.69f,-.44f),V(0,4.62f,-.45f),V(.10f,4.69f,-.44f),V(.25f,4.99f,-.53f),V(.27f,5.16f,-.56f));
            P("Sternal ceramic inset",body,white,.045f,V(-.12f,5.09f,-.585f),V(-.11f,4.92f,-.56f),V(0,4.77f,-.51f),V(.11f,4.92f,-.56f),V(.12f,5.09f,-.585f));
            var waist=G("02 / Articulated abdominal block",root);
            Hull("Waist flexible cast frame",waist,black,S(4.12f,.35f,.24f),S(4.37f,.32f,.24f),S(4.74f,.40f,.29f));
            for(int i=0;i<3;i++)
            {
                float y=4.24f+i*.17f;
                P("Overlapping abdomen scale "+i,waist,i==2?white:frame,.11f,V(-.28f,y+.13f,-.31f),V(-.23f,y,-.35f),V(.23f,y,-.35f),V(.28f,y+.13f,-.31f));
                B("Central abdominal latch",waist,V(0,y+.065f,-.365f),V(.15f,.052f,.018f),steel);
            }
            for(int s=-1;s<=1;s+=2){Piston(waist,V(s*.30f,4.18f,-.02f),V(s*.43f,4.68f,-.02f),.055f);B("Red oblique energy insert",waist,V(s*.43f,4.48f,-.20f),V(.13f,.34f,.14f),red,V(0,0,-s*14));}
            Rod("Neck yaw axle",body,V(0,5.69f,0),V(0,6.06f,0),.14f,steel);
            Tube("Neck rotary collar",body,V(0,5.84f,0),.24f,.15f,.10f,frame,V(90,0,0));
            Head(root);
        }
        static void Head(Transform root)
        {
            var h=G("03 / Helmet - face mask, sensor cage, antenna",root,V(0,6.12f,-.02f));
            Hull("Helmet ceramic outer casting",h,porcelain,S(-.18f,.23f,.22f),S(.05f,.31f,.25f),S(.39f,.29f,.25f),S(.55f,.16f,.14f));
            for(int s=-1;s<=1;s+=2)
            {
                M("Helmet lateral ceramic shell",h,s,white,.15f,V(.13f,.50f,-.13f),V(.25f,.43f,-.22f),V(.32f,.22f,-.20f),V(.31f,-.11f,-.14f),V(.20f,-.25f,-.16f),V(.20f,.04f,-.28f),V(.12f,.24f,-.25f));
                M("Temple blue cap",h,s,blue,.11f,V(.24f,.38f,-.24f),V(.35f,.30f,-.20f),V(.35f,.02f,-.23f),V(.27f,-.15f,-.27f),V(.23f,.04f,-.31f));
                var ear=G("Temple sensor mount",h,V(s*.30f,.10f,0),V(0,-s*90,0));
                Bearing(ear,V(0,0,0),.115f);
                M("Cheekbone armor",h,s,porcelain,.08f,V(.12f,.08f,-.32f),V(.23f,.04f,-.33f),V(.24f,-.18f,-.28f),V(.14f,-.27f,-.32f),V(.12f,-.10f,-.39f));
                M("Angular brow hood",h,s,white,.065f,V(.025f,.23f,-.35f),V(.15f,.28f,-.35f),V(.255f,.22f,-.29f),V(.21f,.15f,-.37f),V(.08f,.14f,-.40f));
                M("Deep eye socket",h,s,black,.025f,V(.03f,.125f,-.365f),V(.21f,.15f,-.34f),V(.18f,.065f,-.37f),V(.065f,.075f,-.40f));
                M("Cyan optical sensor",h,s,cyan,.012f,V(.055f,.12f,-.405f),V(.184f,.137f,-.38f),V(.162f,.091f,-.39f),V(.067f,.087f,-.413f));
                M("Gold primary V-fin",h,s,gold,.065f,V(.045f,.31f,-.34f),V(.17f,.34f,-.32f),V(.72f,1.01f,-.15f),V(.42f,.40f,-.29f),V(.14f,.22f,-.38f));
                M("V-fin polished ridge",h,s,gold,.008f,V(.155f,.33f,-.363f),V(.65f,.92f,-.182f),V(.39f,.42f,-.315f));
                M("Auxiliary cobalt blade antenna",h,s,blue,.055f,V(.25f,.21f,.02f),V(.40f,.32f,.05f),V(.49f,.89f,.18f),V(.29f,.49f,.07f));
                Seam(h,V(s*.285f,.28f,-.26f),V(s*.29f,.10f,-.273f),V(s*.255f,.02f,-.30f));
                Bolt(h,V(s*.30f,.18f,-.24f),.016f);
            }
            for(int s=-1;s<=1;s+=2)M("Folded ceramic face mask",h,s,white,.055f,V(0,.105f,-.45f),V(.12f,.05f,-.37f),V(.11f,-.12f,-.39f),V(0,-.23f,-.45f));
            for(int i=0;i<3;i++)Seam(h,V(-.065f,-.02f-i*.043f,-.431f),V(0,-.045f-i*.043f,-.46f),V(.065f,-.02f-i*.043f,-.431f));
            P("Crimson chin lock",h,red,.085f,V(-.063f,-.16f,-.40f),V(0,-.31f,-.42f),V(.063f,-.16f,-.40f),V(.045f,-.105f,-.45f),V(-.045f,-.105f,-.45f));
            P("Crown forehead cap",h,porcelain,.13f,V(-.10f,.26f,-.36f),V(-.09f,.56f,-.21f),V(0,.66f,-.15f),V(.09f,.56f,-.21f),V(.10f,.26f,-.36f));
            B("Forehead recessed red camera",h,V(0,.41f,-.302f),V(.095f,.13f,.028f),red,V(-24,0,0));
            P("Median gold crest",h,gold,.07f,V(-.047f,.51f,-.23f),V(0,.92f,-.04f),V(.047f,.51f,-.23f));
            var rear=G("Rear helmet inspection hatch",h,V(0,.20f,.26f),V(0,180,0));
            B("Occipital panel",rear,Vector3.zero,V(.33f,.28f,.06f),white);Vent(rear,V(0,-.02f,-.04f),new Vector2(.18f,.10f),3);Decal(rear,V(0,.09f,-.033f),new Vector2(.15f,.055f),1);
        }
        static void Pelvis(Transform root)
        {
            var p=G("04 / Pelvis and floating skirt panels",root);
            Hull("Pelvic gearbox",p,frame,S(3.70f,.46f,.27f),S(3.91f,.58f,.29f),S(4.15f,.43f,.28f));
            P("White belt buckle",p,white,.16f,V(-.39f,4.17f,-.34f),V(-.42f,4.00f,-.38f),V(-.23f,3.90f,-.44f),V(.23f,3.90f,-.44f),V(.42f,4.00f,-.38f),V(.39f,4.17f,-.34f));
            B("Recessed crimson belt tab",p,V(0,4.025f,-.45f),V(.20f,.17f,.04f),red);
            P("Crotch armored keel",p,porcelain,.18f,V(-.21f,3.91f,-.45f),V(-.17f,3.53f,-.47f),V(0,3.34f,-.39f),V(.17f,3.53f,-.47f),V(.21f,3.91f,-.45f));
            B("Lower pelvic red insert",p,V(0,3.55f,-.49f),V(.14f,.19f,.035f),red);
            for(int s=-1;s<=1;s+=2)
            {
                var skirt=G((s<0?"L":"R")+" front skirt hinge",p,V(s*.43f,3.99f,-.20f),V(-8,0,-s*9));
                Rod("Skirt hinge axle",skirt,V(-.17f,0,0),V(.17f,0,0),.06f,steel);
                P("Skirt graphite carrier",skirt,navy,.17f,V(-.25f,-.10f,-.15f),V(-.26f,-.71f,-.24f),V(-.10f,-.92f,-.23f),V(.24f,-.72f,-.23f),V(.29f,-.13f,-.14f));
                P("Floating white hip plate",skirt,white,.10f,V(-.23f,-.12f,-.23f),V(-.23f,-.64f,-.32f),V(-.08f,-.86f,-.32f),V(.21f,-.67f,-.32f),V(.23f,-.16f,-.23f),V(.08f,-.08f,-.22f));
                P("Skirt cobalt inset",skirt,blue,.025f,V(.10f,-.17f,-.26f),V(.20f,-.21f,-.26f),V(.17f,-.59f,-.35f),V(.09f,-.65f,-.35f));
                Seam(skirt,V(-.16f,-.25f,-.26f),V(-.04f,-.31f,-.28f),V(-.04f,-.55f,-.32f),V(-.16f,-.64f,-.345f));
                Decal(skirt,V(-.12f,-.46f,-.32f),new Vector2(.12f,.18f),4);
                Bolt(skirt,V(-.16f,-.18f,-.25f),.020f);Bolt(skirt,V(.10f,-.71f,-.345f),.018f);
                var side=G("Lateral hip outrigger "+s,p,V(s*.62f,3.88f,.02f),V(0,-s*65,-s*13));
                P("Blue hip carrier",side,blue,.19f,V(-.18f,.03f,-.09f),V(-.26f,-.62f,-.15f),V(.07f,-.92f,-.19f),V(.23f,-.61f,-.15f),V(.20f,-.02f,-.09f));
                P("White side skirt",side,white,.09f,V(-.15f,-.03f,-.19f),V(-.17f,-.52f,-.25f),V(.07f,-.76f,-.27f),V(.14f,-.50f,-.25f),V(.14f,-.07f,-.19f));
                Vent(side,V(-.01f,-.22f,-.27f),new Vector2(.15f,.20f),4);Decal(side,V(.04f,-.51f,-.28f),new Vector2(.12f,.12f),2);
                var rear=G("Rear skirt "+s,p,V(s*.33f,3.94f,.28f),V(0,180,s*7));
                P("Rear white shell",rear,white,.14f,V(-.25f,-.05f,-.06f),V(-.23f,-.71f,-.17f),V(.24f,-.76f,-.17f),V(.27f,-.05f,-.06f));
                Vent(rear,V(0,-.38f,-.20f),new Vector2(.30f,.23f),5);
            }
        }
        static void Legs(Transform root)
        {
            for(int s=-1;s<=1;s+=2)
            {
                var leg=G((s<0?"05 L":"06 R")+" / Leg assembly",root,V(s*.53f,3.79f,0),V(0,-s*5,s*5));
                Bearing(leg,V(s*.23f,0,0),.25f,V(0,-s*90,0));
                Hull("Titanium femur core",leg,frame,S(-1.14f,.15f,.17f),S(-.82f,.18f,.18f),S(-.17f,.19f,.19f));
                Piston(leg,V(-.12f,-.20f,.14f),V(-.12f,-1.23f,.18f),.055f);Piston(leg,V(.12f,-.20f,.14f),V(.12f,-1.23f,.18f),.055f);
                Hull("Upper leg ceramic envelope",leg,white,S(-1.15f,.22f,.24f),S(-1.05f,.29f,.29f),S(-.42f,.32f,.29f),S(-.23f,.24f,.23f));
                P("Thigh frontal access panel",leg,porcelain,.06f,V(-.22f,-.39f,-.30f),V(-.20f,-.92f,-.30f),V(-.10f,-1.07f,-.28f),V(.15f,-1.03f,-.28f),V(.22f,-.42f,-.30f));
                for(int j=-1;j<=1;j+=2)
                {
                    var side=G("Femoral lateral cobalt insert "+j,leg,V(j*.285f,-.65f,0),V(0,-j*90,0));
                    P("Lateral blue inset",side,blue,.045f,V(-.16f,.23f,-.015f),V(-.18f,-.24f,-.015f),V(.05f,-.38f,-.015f),V(.18f,-.19f,-.015f),V(.16f,.27f,-.015f));
                    Seam(side,V(-.08f,.17f,-.02f),V(-.08f,-.17f,-.02f),V(.02f,-.24f,-.02f));
                    Bolt(side,V(.08f,.16f,-.025f),.021f);
                }
                Seam(leg,V(-.15f,-.49f,-.365f),V(-.15f,-.79f,-.365f),V(-.05f,-.88f,-.365f));
                Decal(leg,V(.11f,-.58f,-.365f),new Vector2(.105f,.19f),s<0?4:5);
                var knee=G("Knee double hinge",leg,V(0,-1.38f,0));
                Bearing(knee,V(s*.23f,0,0),.23f,V(0,-s*90,0),true);
                Rod("Knee transverse axle",knee,V(-.29f,0,0),V(.29f,0,0),.13f,steel);
                P("Kneecap floating shield",knee,white,.16f,V(-.24f,.17f,-.25f),V(-.32f,-.03f,-.31f),V(-.12f,-.32f,-.38f),V(.12f,-.32f,-.38f),V(.32f,-.03f,-.31f),V(.24f,.17f,-.25f));
                P("Kneecap blue crest",knee,blue,.05f,V(-.13f,.14f,-.29f),V(0,.24f,-.28f),V(.13f,.14f,-.29f),V(.10f,-.11f,-.40f),V(0,-.20f,-.43f),V(-.10f,-.11f,-.40f));
                Bolt(knee,V(s*.21f,.01f,-.37f),.025f);
                var shin=G("Lower leg / calf casing",leg,V(0,-1.43f,0));
                Hull("Tibial cast load frame",shin,frame,S(-1.57f,.16f,.19f),S(-1.15f,.20f,.22f),S(-.35f,.25f,.28f),S(-.16f,.20f,.22f));
                Piston(shin,V(-.19f,-.25f,.20f),V(-.17f,-1.55f,.20f),.06f);Piston(shin,V(.19f,-.25f,.20f),V(.17f,-1.55f,.20f),.06f);
                Hull("Cobalt calf armor volume",shin,blue,S(-1.47f,.23f,.24f),S(-1.21f,.31f,.31f),S(-.61f,.39f,.40f,.05f),S(-.23f,.30f,.31f,.04f));
                for(int j=-1;j<=1;j+=2)
                {
                    M("Wraparound white shin cheek",shin,j,white,.13f,V(.05f,-.31f,-.34f),V(.22f,-.24f,-.34f),V(.35f,-.50f,-.29f),V(.32f,-1.03f,-.26f),V(.23f,-1.43f,-.24f),V(.07f,-1.53f,-.31f),V(.09f,-.97f,-.39f));
                    M("Calf side shell return",shin,j,porcelain,.22f,V(.30f,-.34f,-.17f),V(.42f,-.52f,-.09f),V(.37f,-1.04f,-.10f),V(.25f,-1.34f,-.10f),V(.26f,-.87f,-.24f));
                    var fin=G("Calf lateral maintenance hatch",shin,V(j*.355f,-.68f,.07f),V(0,-j*90,0));
                    P("Calf side hatch",fin,white,.08f,V(-.24f,.20f,-.04f),V(-.28f,-.12f,-.04f),V(.07f,-.40f,-.04f),V(.21f,-.23f,-.04f),V(.24f,.23f,-.04f));
                    Vent(fin,V(.03f,-.02f,-.13f),new Vector2(.22f,.27f),6);Bolt(fin,V(.15f,.16f,-.13f),.025f);
                    Seam(shin,V(j*.17f,-.42f,-.397f),V(j*.22f,-.61f,-.368f),V(j*.18f,-.93f,-.355f),V(j*.14f,-1.05f,-.36f));
                }
                P("Long shin blade insert",shin,blue,.07f,V(-.07f,-.41f,-.39f),V(-.08f,-1.14f,-.43f),V(0,-1.59f,-.37f),V(.08f,-1.14f,-.43f),V(.07f,-.41f,-.39f));
                P("Central titanium tibial rib",shin,steel,.02f,V(-.018f,-.53f,-.47f),V(-.022f,-1.18f,-.49f),V(0,-1.40f,-.43f),V(.022f,-1.18f,-.49f),V(.018f,-.53f,-.47f));
                Decal(shin,V(-.22f,-.91f,-.385f),new Vector2(.13f,.20f),6);Bolt(shin,V(.20f,-1.20f,-.30f));
                var rear=G("Calf exhaust compartment",shin,V(0,-.66f,.44f),V(0,180,0));Vent(rear,V(0,0,0),new Vector2(.35f,.44f),7);
                P("Calf rear sliding armor",rear,white,.09f,V(-.20f,-.29f,-.03f),V(-.15f,-.66f,-.01f),V(.15f,-.66f,-.01f),V(.20f,-.29f,-.03f));
                var ankle=G("Ankle universal joint",leg,V(0,-3.10f,0));Bearing(ankle,V(s*.20f,0,0),.18f,V(0,-s*90,0));
                var foot=G("Segmented foot and sole",leg,V(0,-3.36f,-.20f));
                B("Rubberized sole",foot,V(0,-.11f,-.12f),V(.51f,.13f,.94f),black);
                P("Red toe alloy shell",foot,red,.42f,V(-.255f,-.045f,-.63f),V(-.235f,.10f,-.63f),V(-.16f,.19f,-.59f),V(.16f,.19f,-.59f),V(.235f,.10f,-.63f),V(.255f,-.045f,-.63f));
                P("Ceramic toe cap",foot,white,.28f,V(-.205f,.06f,-.65f),V(-.17f,.23f,-.56f),V(.17f,.23f,-.56f),V(.205f,.06f,-.65f));
                B("Instep shell",foot,V(0,.14f,-.12f),V(.46f,.28f,.36f),porcelain,V(-12,0,0));
                B("Heel segmented block",foot,V(0,.01f,.29f),V(.42f,.22f,.25f),blue);
                for(int i=0;i<5;i++)B("Sole tread",foot,V(0,-.18f,-.48f+i*.17f),V(.44f,.025f,.06f),frame);
                Seam(foot,V(-.15f,.11f,-.669f),V(0,.07f,-.672f),V(.15f,.11f,-.669f));
                Decal(foot,V(0,.145f,-.66f),new Vector2(.15f,.06f),1);
                for(int j=-1;j<=1;j+=2){var guard=G("Ankle ceramic guard",ankle,V(j*.23f,-.10f,-.02f),V(0,-j*90,0));P("Ankle wrap",guard,white,.085f,V(-.21f,.18f,-.06f),V(-.23f,-.17f,-.06f),V(.16f,-.24f,-.06f),V(.22f,.09f,-.06f));Bolt(guard,V(.05f,.04f,-.07f),.032f);}
            }
        }
        static void Arms(Transform root)
        {
            for(int s=-1;s<=1;s+=2)
            {
                var arm=G((s<0?"07 L":"08 R")+" / Shoulder and articulated arm",root,V(s*1.15f,5.43f,.02f),V(0,0,s*9));
                Bearing(arm,V(s*.10f,0,0),.28f,V(0,-s*90,0));
                Rod("Shoulder transversal shaft",arm,V(-.23f,0,0),V(.23f,0,0),.15f,steel);
                var shoulder=G("Layered shoulder pauldron",arm,V(s*.12f,.13f,0));
                M("Shoulder cobalt upper casting",shoulder,s,blue,.52f,V(-.25f,-.18f,-.25f),V(-.29f,.12f,-.28f),V(-.04f,.38f,-.27f),V(.46f,.20f,-.13f),V(.72f,-.31f,-.03f),V(.44f,-.34f,-.23f),V(.14f,-.19f,-.35f));
                M("White shoulder crown",shoulder,s,white,.20f,V(-.25f,.10f,-.32f),V(-.02f,.35f,-.32f),V(.43f,.17f,-.22f),V(.57f,-.07f,-.18f),V(.36f,-.01f,-.33f),V(.06f,.16f,-.43f),V(-.14f,-.04f,-.43f));
                M("Shoulder floating outer edge",shoulder,s,porcelain,.17f,V(.36f,.05f,-.24f),V(.59f,-.06f,-.17f),V(.79f,-.52f,-.05f),V(.54f,-.42f,-.22f),V(.39f,-.25f,-.35f));
                M("Shoulder blue spear fascia",shoulder,s,blue,.11f,V(.02f,.04f,-.45f),V(.25f,-.05f,-.42f),V(.60f,-.76f,-.21f),V(.28f,-.43f,-.40f),V(.08f,-.32f,-.48f));
                Seam(shoulder,V(s*.02f,.25f,-.37f),V(s*.18f,.15f,-.39f),V(s*.37f,.075f,-.31f));
                Decal(shoulder,V(s*.08f,.12f,-.445f),new Vector2(.18f,.08f),0,-s*12);Bolt(shoulder,V(s*.44f,-.16f,-.33f));
                Hull("Upper arm alloy strut",arm,frame,S(-.86f,.14f,.17f),S(-.29f,.16f,.18f));
                Hull("Upper arm ceramic sleeve",arm,white,S(-.78f,.22f,.23f),S(-.66f,.26f,.26f),S(-.29f,.25f,.26f),S(-.23f,.18f,.20f));
                P("Bicep inset armor",arm,porcelain,.065f,V(-.16f,-.32f,-.27f),V(-.15f,-.65f,-.27f),V(0,-.73f,-.27f),V(.17f,-.61f,-.27f),V(.17f,-.32f,-.27f));
                Seam(arm,V(-.07f,-.36f,-.338f),V(-.07f,-.57f,-.338f),V(.03f,-.63f,-.338f));
                Piston(arm,V(s*.22f,-.27f,.07f),V(s*.18f,-.95f,.05f),.045f);
                Bearing(arm,V(s*.19f,-.94f,0),.19f,V(0,-s*90,0));
                var fore=G("Forearm wrist assembly",arm,V(0,-1.03f,0),V(-8,0,0));
                Hull("Forearm internal chassis",fore,frame,S(-.91f,.14f,.16f),S(-.12f,.18f,.20f));
                Hull("Forearm cobalt body",fore,blue,S(-.81f,.21f,.23f),S(-.63f,.29f,.30f),S(-.12f,.26f,.28f));
                P("White gauntlet face shell",fore,white,.10f,V(-.23f,-.15f,-.30f),V(-.24f,-.48f,-.33f),V(-.15f,-.83f,-.25f),V(.15f,-.83f,-.25f),V(.24f,-.48f,-.33f),V(.23f,-.15f,-.30f));
                P("Gauntlet removable center inset",fore,porcelain,.035f,V(-.13f,-.22f,-.41f),V(-.14f,-.49f,-.44f),V(-.07f,-.68f,-.39f),V(.07f,-.68f,-.39f),V(.14f,-.49f,-.44f),V(.13f,-.22f,-.41f));
                Seam(fore,V(-.08f,-.27f,-.448f),V(-.08f,-.43f,-.467f),V(.02f,-.50f,-.46f));
                Decal(fore,V(.04f,-.34f,-.451f),new Vector2(.10f,.12f),2);Bolt(fore,V(-.17f,-.58f,-.36f),.021f);
                var outside=G("Forearm lateral rail",fore,V(s*.27f,-.40f,0),V(0,-s*90,0));Vent(outside,V(0,0,0),new Vector2(.22f,.28f),6);
                Rod("Wrist swivel",fore,V(0,-.83f,0),V(0,-1.05f,0),.125f,steel);
                var hand=G("Hand / articulated knuckles",fore,V(0,-1.08f,-.01f));Hand(hand,s);
                if(s<0)Rifle(hand);
                else Shield(fore);
            }
        }
        static void Hand(Transform h,int s)
        {
            B("Palm cast housing",h,V(0,-.10f,0),V(.30f,.28f,.20f),frame);
            B("Ceramic hand back",h,V(0,-.10f,-.125f),V(.31f,.24f,.065f),white);
            for(int i=0;i<4;i++)
            {
                float x=(i-1.5f)*.071f;
                var finger=G("Finger "+(i+1),h,V(x,-.23f,-.015f));
                B("Proximal phalanx",finger,V(0,-.045f,-.015f),V(.060f,.09f,.10f),steel);
                B("Intermediate phalanx",finger,V(0,-.100f,.025f),V(.056f,.075f,.08f),frame,V(-40,0,0));
                B("Distal fingertip",finger,V(0,-.11f,.08f),V(.053f,.06f,.07f),frame);
                Rod("Knuckle pin",finger,V(-.032f,-.003f,-.02f),V(.032f,-.003f,-.02f),.023f,black,20);
                B("White knuckle cap",finger,V(0,-.025f,-.078f),V(.055f,.05f,.020f),porcelain);
            }
            var thumb=G("Opposed thumb",h,V(-s*.18f,-.09f,.015f),V(0,0,-s*30));
            B("Thumb metacarpal",thumb,V(0,-.035f,0),V(.085f,.11f,.095f),steel);B("Thumb tip",thumb,V(0,-.11f,.035f),V(.075f,.09f,.085f),frame,V(-35,0,0));
            Bolt(h,V(0,-.12f,-.165f),.024f);
        }
    }
}
