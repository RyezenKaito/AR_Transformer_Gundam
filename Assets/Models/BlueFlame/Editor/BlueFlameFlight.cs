using System;
using UnityEngine;
using static BlueFlame.Editor.BFG;

namespace BlueFlame.Editor
{
    public static partial class BlueFlameBuilder
    {
        static Transform Flight(Transform root)
        {
            var back=G("09 / Flight pack and articulated blade binders",root);
            B("Backpack central reactor",back,V(0,5.29f,.64f),V(.83f,.92f,.55f),frame);
            var rear=G("Rear avionics cover",back,V(0,5.29f,.95f),V(0,180,0));
            B("White dorsal access panel",rear,Vector3.zero,V(.65f,.73f,.10f),white);Vent(rear,V(0,.05f,-.07f),new Vector2(.46f,.37f),8);Decal(rear,V(0,-.25f,-.058f),new Vector2(.34f,.12f),7);
            for(int s=-1;s<=1;s+=2)
            {
                var engine=G("Vector thruster "+s,back,V(s*.44f,4.94f,.74f),V(18,0,0));
                Hull("Cobalt engine cowling",engine,navy,S(-.37f,.20f,.20f),S(.30f,.22f,.22f));
                var nozzle=G("Exhaust bell",engine,V(0,-.39f,0),V(90,0,0));
                Lathe("Titanium nozzle wall",nozzle,Vector3.zero,steel,new[]{new Vector2(-.12f,.17f),new Vector2(-.07f,.20f),new Vector2(.16f,.12f),new Vector2(.16f,.085f),new Vector2(-.09f,.13f),new Vector2(-.12f,.17f)},48,default,false);
                Tube("Blue exhaust throat",nozzle,V(0,0,.06f),.10f,.06f,.02f,cyan);
                for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Rod("Nozzle reinforcement",nozzle,V(Mathf.Cos(a)*.18f,Mathf.Sin(a)*.18f,-.06f),V(Mathf.Cos(a)*.13f,Mathf.Sin(a)*.13f,.12f),.012f,frame,12);}
                var w=G((s<0?"L":"R")+" wing root gimbal",back,V(s*.70f,5.48f,.66f),V(0,-s*8,0));
                Bearing(w,V(s*.10f,.08f,-.03f),.31f);
                Rod("Wing deployment main spar",w,V(s*.1f,.10f,.1f),V(s*1.13f,.60f,.16f),.145f,frame);
                Piston(w,V(s*.12f,-.20f,-.04f),V(s*1.05f,.37f,-.04f),.07f);
                M("Root cobalt carrier",w,s,blue,.34f,V(.10f,.31f,-.12f),V(.56f,.86f,-.10f),V(1.04f,1.16f,-.03f),V(1.34f,.70f,-.06f),V(1.09f,.23f,-.09f),V(.68f,-.06f,-.20f),V(.29f,-.28f,-.23f));
                M("White root leading armor",w,s,white,.17f,V(.24f,.37f,-.23f),V(.64f,.92f,-.19f),V(.94f,1.12f,-.14f),V(1.10f,.82f,-.20f),V(.77f,.59f,-.29f),V(.56f,.16f,-.30f));
                M("Root gold heat shield",w,s,gold,.08f,V(.57f,-.05f,-.24f),V(.90f,.12f,-.21f),V(1.16f,.39f,-.16f),V(1.28f,.14f,-.15f),V(.91f,-.57f,-.20f),V(.58f,-.68f,-.21f));
                M("Root dark removable hatch",w,s,frame,.06f,V(.64f,-.08f,-.335f),V(1.09f,.25f,-.25f),V(1.14f,.11f,-.25f),V(.87f,-.47f,-.29f),V(.64f,-.53f,-.30f));
                Vent(w,V(s*.82f,-.20f,-.36f),new Vector2(.22f,.30f),5);
                Bearing(w,V(s*1.10f,.66f,-.23f),.24f);
                // Main span has three overlapping shell sections with dark joint gaps.
                M("Main wing torsion box",w,s,navy,.27f,V(.88f,.92f,.03f),V(1.48f,1.08f,.10f),V(2.05f,.89f,.16f),V(2.89f,.30f,.19f),V(5.83f,-1.03f,.25f),V(5.55f,-1.10f,.24f),V(2.46f,-.02f,.11f),V(1.57f,.31f,.03f),V(1.09f,.42f,-.05f));
                M("Main wing ceramic shoulder",w,s,white,.13f,V(1.00f,.99f,-.08f),V(1.47f,1.02f,-.03f),V(2.02f,.82f,.04f),V(2.39f,.48f,.06f),V(1.93f,.50f,-.01f),V(1.65f,.61f,-.06f),V(1.38f,.57f,-.10f));
                M("Wing cobalt structural stripe",w,s,blue,.08f,V(1.31f,.92f,-.11f),V(1.48f,.93f,-.09f),V(1.88f,.77f,-.03f),V(1.91f,.67f,-.05f),V(1.53f,.80f,-.12f),V(1.40f,.74f,-.13f));
                M("Articulated midspan white shell",w,s,porcelain,.12f,V(1.43f,.53f,-.13f),V(1.73f,.53f,-.10f),V(2.03f,.39f,-.05f),V(2.52f,.31f,.00f),V(2.79f,.09f,.01f),V(2.48f,.04f,-.04f),V(2.03f,.22f,-.11f),V(1.69f,.20f,-.15f));
                Bearing(w,V(s*2.34f,.20f,-.07f),.15f);
                M("Long outer white blade",w,s,white,.095f,V(2.73f,.24f,.04f),V(3.16f,.04f,.07f),V(5.70f,-1.02f,.15f),V(4.74f,-.78f,.12f),V(2.67f,-.04f,.02f),V(2.52f,.10f,.01f));
                M("Outer blade polished cutting bevel",w,s,steel,.025f,V(2.72f,-.06f,.008f),V(4.74f,-.79f,.105f),V(5.83f,-1.05f,.14f),V(5.17f,-.98f,.12f),V(2.70f,-.12f,.005f));
                M("Outer blade cobalt dorsal line",w,s,blue,.028f,V(2.77f,.25f,.013f),V(3.24f,.04f,.045f),V(5.70f,-1.025f,.12f),V(4.74f,-.66f,.085f),V(2.85f,.16f,.01f));
                Line("Main wing engraved tracking channel",w,black,.012f,V(s*2.93f,.035f,-.005f),V(s*3.65f,-.28f,.03f),V(s*5.00f,-.83f,.10f));
                for(int i=0;i<5;i++)
                {
                    float x=2.86f+i*.20f,y=.105f-(x-2.86f)*.42f;
                    Line("Wing heat discharge slot",w,black,.020f,V(s*x,y,.005f),V(s*(x+.11f),y-.09f,.010f));
                }
                for(int i=0;i<4;i++){float x=1.24f+i*.30f;Bolt(w,V(s*x,.82f-(x-1.24f)*.43f,-.14f+(x-1.24f)*.1f),.027f);}
                Decal(w,V(s*1.63f,.74f,-.135f),new Vector2(.29f,.12f),s<0?4:5,-s*22);
                // Separate downward sabre binders; broad armored bases taper into narrow distal blades.
                M("Long cobalt ventral binder",w,s,blue,.21f,V(.65f,.50f,-.34f),V(1.00f,.46f,-.32f),V(1.43f,-.12f,-.30f),V(2.63f,-2.91f,-.14f),V(1.44f,-1.11f,-.32f),V(.78f,-.09f,-.40f));
                M("Ventral binder ceramic insert",w,s,white,.085f,V(.79f,.40f,-.45f),V(.94f,.35f,-.43f),V(1.23f,-.13f,-.42f),V(2.42f,-2.56f,-.22f),V(1.31f,-.70f,-.43f),V(.90f,-.13f,-.49f));
                M("Ventral binder cobalt blade edge",w,s,blue,.04f,V(1.12f,-.28f,-.52f),V(1.25f,-.40f,-.51f),V(2.63f,-2.91f,-.19f),V(1.67f,-1.45f,-.38f));
                M("Inner trailing white blade",w,s,porcelain,.16f,V(.80f,-.33f,.18f),V(1.09f,-.22f,.23f),V(1.40f,-1.43f,.24f),V(1.39f,-2.94f,.24f),V(1.02f,-2.25f,.15f),V(.80f,-1.02f,.13f));
                M("Trailing blade cobalt inset",w,s,blue,.065f,V(.95f,-.49f,.08f),V(1.06f,-.55f,.10f),V(1.23f,-1.45f,.10f),V(1.31f,-2.63f,.11f),V(1.11f,-1.95f,.055f));
                M("Secondary aft stabilizer",w,s,white,.12f,V(.33f,-.41f,.43f),V(.58f,-.34f,.45f),V(.82f,-1.22f,.48f),V(.53f,-2.38f,.45f),V(.37f,-1.45f,.40f));
                for(int i=0;i<4;i++)Bolt(w,V(s*(1.01f+i*.18f),-.33f-i*.40f,-.51f+i*.04f),.021f);
                var rearWing=G("Wing rear service covers",w,V(s*1.20f,.54f,.34f),V(0,180,0));
                B("Wing locking actuator",rearWing,V(0,0,0),V(.41f,.43f,.12f),frame);Vent(rearWing,V(0,0,-.08f),new Vector2(.27f,.26f),5);Bolt(rearWing,V(.13f,.16f,-.08f));
            }
            return back;
        }
        static void Rifle(Transform hand)
        {
            var gun=G("10 / Long-range BlueFlame rifle",hand,V(0,-.17f,.07f),V(0,0,-10));
            // Receiver and grip sit in the closed hand. Barrel points down along local -Y.
            B("Pistol grip",gun,V(0,.06f,.0f),V(.15f,.25f,.17f),black);
            B("Rifle titanium receiver",gun,V(0,-.25f,-.22f),V(.40f,.86f,.42f),frame);
            B("Dorsal sight rail",gun,V(0,.03f,-.47f),V(.15f,.51f,.08f),steel);
            for(int i=0;i<7;i++)B("Sight rail notch",gun,V(0,-.19f+i*.065f,-.52f),V(.18f,.022f,.018f),black);
            P("Receiver ceramic side shield",gun,porcelain,.08f,V(-.18f,.16f,-.46f),V(-.20f,-.45f,-.46f),V(-.12f,-.69f,-.46f),V(.15f,-.57f,-.46f),V(.20f,.09f,-.46f));
            P("Cobalt magazine housing",gun,blue,.18f,V(-.16f,-.57f,-.44f),V(-.24f,-1.17f,-.43f),V(0,-1.44f,-.42f),V(.24f,-1.17f,-.43f),V(.16f,-.57f,-.44f));
            for(int s=-1;s<=1;s+=2)
            {
                Rod("Long twin accelerator barrel",gun,V(s*.11f,-.63f,-.22f),V(s*.11f,-2.12f,-.22f),.08f,steel);
                Rod("Barrel cooling jacket",gun,V(s*.11f,-.84f,-.22f),V(s*.11f,-1.70f,-.22f),.115f,frame);
                for(int i=0;i<10;i++)Rod("Barrel heat sink ring",gun,V(s*.11f,-.91f-i*.065f,-.22f),V(s*.11f,-.932f-i*.065f,-.22f),.132f,steel,32);
                var mu=G("Open muzzle "+s,gun,V(s*.11f,-2.17f,-.22f),V(-90,0,0));
                Tube("Muzzle collar",mu,Vector3.zero,.105f,.066f,.14f,frame);Tube("Cyan bore",mu,V(0,0,.035f),.065f,.043f,.02f,cyan);
                Bolt(gun,V(s*.12f,-.27f,-.55f),.022f);
            }
            P("Gold accelerator brace",gun,gold,.07f,V(-.20f,-1.05f,-.48f),V(-.22f,-1.49f,-.46f),V(-.10f,-1.60f,-.45f),V(.10f,-1.60f,-.45f),V(.22f,-1.49f,-.46f),V(.20f,-1.05f,-.48f),V(.11f,-1.17f,-.49f),V(.10f,-1.42f,-.48f),V(-.10f,-1.42f,-.48f),V(-.11f,-1.17f,-.49f));
            P("Underbarrel cobalt blade",gun,blue,.13f,V(-.17f,-1.45f,-.39f),V(-.21f,-1.89f,-.39f),V(0,-2.82f,-.31f),V(.21f,-1.89f,-.39f),V(.17f,-1.45f,-.39f));
            P("Blade titanium cutting face",gun,steel,.035f,V(-.11f,-1.73f,-.54f),V(0,-2.72f,-.45f),V(.11f,-1.73f,-.54f));
            B("Weapon reactor strip",gun,V(0,-1.11f,-.54f),V(.045f,.45f,.018f),cyan);
            Decal(gun,V(0,-.21f,-.55f),new Vector2(.22f,.12f),7);Seam(gun,V(-.12f,.04f,-.55f),V(-.12f,-.11f,-.55f),V(-.04f,-.19f,-.55f));
        }
        static void Shield(Transform arm)
        {
            var g=G("11 / Forearm shield and detachable blade",arm,V(.26f,-.26f,-.20f),V(0,-10,-7));
            B("Shield locking bracket",g,V(-.17f,-.11f,.13f),V(.28f,.38f,.20f),frame);
            P("Shield blue substructure",g,blue,.24f,V(-.29f,.54f,-.19f),V(-.38f,.20f,-.21f),V(-.30f,-.63f,-.27f),V(.04f,-1.69f,-.20f),V(.36f,-.64f,-.25f),V(.40f,.22f,-.18f),V(.23f,.59f,-.16f));
            P("Shield ceramic main shell",g,white,.12f,V(-.22f,.48f,-.32f),V(-.30f,.17f,-.35f),V(-.23f,-.49f,-.39f),V(-.06f,-.77f,-.40f),V(.13f,-.62f,-.39f),V(.22f,.16f,-.34f),V(.16f,.49f,-.29f));
            P("Shield ceramic outer shell",g,porcelain,.10f,V(.28f,.34f,-.28f),V(.34f,.17f,-.28f),V(.30f,-.53f,-.35f),V(.05f,-1.38f,-.30f),V(.10f,-.77f,-.41f),V(.20f,-.53f,-.41f));
            P("Shield steel distal blade",g,steel,.06f,V(-.11f,-.71f,-.41f),V(.04f,-1.69f,-.29f),V(.11f,-.81f,-.43f),V(.02f,-.67f,-.43f));
            Seam(g,V(-.17f,.31f,-.455f),V(-.07f,.23f,-.465f),V(-.07f,.04f,-.475f),V(.06f,-.08f,-.478f));
            Seam(g,V(-.16f,-.08f,-.48f),V(-.18f,-.36f,-.51f),V(-.06f,-.48f,-.52f));
            for(int i=0;i<3;i++)Line("Shield vent slot",g,black,.018f,V(.00f,.04f-i*.07f,-.478f),V(.11f,.13f-i*.07f,-.465f));
            Bolt(g,V(-.19f,.35f,-.45f));Bolt(g,V(.04f,-.51f,-.515f));Decal(g,V(.09f,.30f,-.435f),new Vector2(.12f,.18f),6);
        }
        static void Decal(Transform parent,Vector3 pos,Vector2 size,int cell,float angle=0)
        {
            var mesh=new Mesh{name="BF_TechnicalMark_"+cell};
            mesh.vertices=new[]{V(-size.x/2,-size.y/2),V(-size.x/2,size.y/2),V(size.x/2,size.y/2),V(size.x/2,-size.y/2)};
            float u=(cell%4)*.25f,v=1-(cell/4+1)*.5f;
            mesh.uv=new[]{new Vector2(u,v),new Vector2(u,v+.5f),new Vector2(u+.25f,v+.5f),new Vector2(u+.25f,v)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateNormals();mesh.RecalculateBounds();Meshes.Add(mesh);
            var o=Object("Printed service marking "+cell,parent,mesh,decal);o.transform.localPosition=pos;o.transform.localEulerAngles=V(0,0,angle);o.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
