using UnityEngine;

namespace HaoLing.Editor
{
    /// <summary>Articulated hard-surface Feng / Huang phoenix units, authored at body origin.</summary>
    public static class HaoLingPhoenix
    {
        public static Transform Build(Transform parent, string birdName, bool mirrorPose)
        {
            Transform root = HLM.Group(birdName, parent);
            Transform fuselage = HLM.Group("01_Phoenix_Fuselage", root);
            fuselage.localScale=new Vector3(.88f,.78f,1.25f);
            BuildBody(fuselage, mirrorPose);
            Transform neck=HLM.Group("02_Cervical_Gimbal",root,V(0,.08f,-.03f),V(12,mirrorPose?52f:-52f,mirrorPose?8f:-9f));
            neck.localScale=new Vector3(.80f,.73f,.88f);
            BuildHead(neck, mirrorPose);
            BuildWing(root, -1, mirrorPose);
            BuildWing(root, 1, mirrorPose);
            Transform talons=HLM.Group("Retracted_Underbody_Talons",root,V(0,-.09f,.025f));
            talons.localScale=Vector3.one*.65f;
            BuildTalons(talons, -1);
            BuildTalons(talons, 1);
            BuildTail(root, -1, mirrorPose);
            BuildTail(root, 1, mirrorPose);
            HLM.Group("Backpack_Attachment_Dock", root, new Vector3(0, -.02f, .29f), new Vector3(0, 180, 0));
            return root;
        }

        private static void BuildBody(Transform p, bool variant)
        {
            HLM.Orb("Titanium_Flight_Core", p, new Vector3(0, -.03f, .055f), new Vector3(.39f, .61f, .44f), HLM.Navy);
            HLM.Beam("Dorsal_Spine", p, new Vector3(0, -.34f, .15f), new Vector3(0, .28f, .09f), .065f, HLM.Frame);
            HLM.Plate("Gilded_Heart_Retainer", p, new[] {
                V(-.15f,.12f,-.21f), V(-.11f,.25f,-.21f), V(0,.32f,-.25f),
                V(.11f,.25f,-.21f), V(.15f,.12f,-.21f), V(.10f,-.10f,-.28f),
                V(0,-.25f,-.31f), V(-.10f,-.10f,-.28f)}, .035f, HLM.Gold, .018f);
            HLM.Plate("White_Keeled_Breastplate", p, new[] {
                V(-.15f,.14f,-.25f), V(-.12f,.27f,-.26f), V(0,.20f,-.34f),
                V(.12f,.27f,-.26f), V(.15f,.14f,-.25f), V(.08f,-.12f,-.32f),
                V(0,-.27f,-.40f), V(-.08f,-.12f,-.32f)}, .06f, HLM.White, .035f);
            HLM.Plate("Split_V_Breast_Navy", p, new[] {
                V(-.095f,.17f,-.34f), V(0,.115f,-.39f), V(.095f,.17f,-.34f),
                V(.055f,.035f,-.37f), V(0,-.08f,-.42f), V(-.055f,.035f,-.37f)}, .018f, HLM.Navy, .009f);
            HLM.Plate("Teal_Crystal_Heart", p, new[] {
                V(0,.105f,-.415f), V(.023f,.075f,-.412f), V(0,.025f,-.44f), V(-.023f,.075f,-.412f)}, .018f, HLM.Teal, .014f);
            HLM.Blade("Keel_Gold_Inlay", p, V(0,-.09f,-.417f), V(0,-.27f,-.455f), .025f, .012f, HLM.Gold);

            for (int s = -1; s <= 1; s += 2)
            {
                Transform side = HLM.Group(s < 0 ? "Port_Lamellar_Armor" : "Starboard_Lamellar_Armor", p);
                for (int i = 0; i < 6; i++)
                {
                    float y = .18f - i * .073f;
                    float x = s * (.11f + Mathf.Sin(i * .49f) * .035f);
                    Feather("Rib_" + i, side, V(x,y,-.12f), V(s * (.24f + i * .012f),y-.12f,.01f), .115f, false, i == 5);
                    HLM.Beam("Gold_Intercostal_" + i, side, V(s*.105f,y,-.201f), V(s*.20f,y-.06f,-.12f), .009f, HLM.Gold, 6);
                }
                HLM.Blade("Hip_Swept_Stabilizer", side, V(s*.13f,-.18f,.1f), V(s*.42f,-.52f,.23f), .18f, .055f, HLM.Navy);
                HLM.Blade("Hip_Pearl_Stabilizer", side, V(s*.17f,-.20f,.055f), V(s*.39f,-.48f,.15f), .13f, .035f, HLM.Pearl);
                HLM.Blade("Hip_Gold_Spine", side, V(s*.18f,-.21f,.018f), V(s*.36f,-.43f,.10f), .025f, .018f, HLM.Gold);

                HLM.Orb("Dorsal_Thruster_Nacelle", p, V(s*.155f,-.12f,.235f), V(.18f,.34f,.22f), HLM.Frame);
                HLM.Ring("Gilded_Exhaust_Lip", p, V(s*.155f,-.21f,.36f), .063f, .013f, HLM.Gold);
                HLM.Orb("Thruster_Turbine", p, V(s*.155f,-.21f,.357f), V(.103f,.103f,.028f), HLM.Black);
                HLM.Orb("Thruster_Cyan_Emitter", p, V(s*.155f,-.21f,.378f), V(.050f,.050f,.013f), HLM.Teal);
                for (int k=0;k<6;k++)
                {
                    float a=k*Mathf.PI/3;
                    HLM.Beam("Exhaust_Radial_"+k,p,V(s*.155f+Mathf.Cos(a)*.025f,-.21f+Mathf.Sin(a)*.025f,.383f),V(s*.155f+Mathf.Cos(a)*.055f,-.21f+Mathf.Sin(a)*.055f,.383f),.004f,HLM.Frame,5);
                }
            }
            for (int i=0;i<5;i++)
            {
                float y=.23f-i*.10f;
                HLM.Blade("Dorsal_White_Scute_"+i,p,V(0,y,.235f),V(0,y-.15f,.37f),.18f-i*.014f,.028f,HLM.White);
                HLM.Blade("Dorsal_Crimson_Fin_"+i,p,V(0,y+.03f,.29f),V(0,y-.12f,.49f),.062f,.026f,i%2==0?HLM.Ruby:HLM.Gold);
            }
        }

        private static void BuildHead(Transform p, bool variant)
        {
            HLM.Beam("Neck_Flexible_Alloy",p,V(0,.12f,-.03f),V(0,.42f,-.25f),.078f,HLM.Frame,12);
            for(int i=0;i<5;i++)
            {
                float t=i/4f;
                Vector3 c=Vector3.Lerp(V(0,.13f,-.06f),V(0,.41f,-.27f),t);
                HLM.Orb("Cervical_Articulation_"+i,p,c,V(.17f-i*.009f,.095f,.15f),HLM.Navy);
                HLM.Blade("Cervical_White_Collar_"+i,p,c+V(0,.055f,-.07f),c+V(0,-.08f,-.12f),.205f-i*.014f,.025f,HLM.White);
                for(int s=-1;s<=1;s+=2)
                    HLM.Blade("Neck_Gilded_Vane_"+s+"_"+i,p,c+V(s*.055f,.02f,-.087f),c+V(s*.10f,-.07f,-.105f),.019f,.012f,HLM.Gold);
            }
            HLM.Orb("Avian_Cranium_Frame",p,V(0,.49f,-.38f),V(.29f,.27f,.38f),HLM.Navy);
            HLM.Plate("Crown_Titanium_Outline",p,new[]{V(-.15f,.46f,-.54f),V(-.135f,.60f,-.45f),V(0,.69f,-.46f),V(.135f,.60f,-.45f),V(.15f,.46f,-.54f),V(0,.33f,-.69f)},.075f,HLM.Gold,.035f);
            HLM.Plate("Faceted_Phoenix_Face",p,new[]{V(-.14f,.47f,-.56f),V(-.12f,.60f,-.47f),V(0,.655f,-.50f),V(.12f,.60f,-.47f),V(.14f,.47f,-.56f),V(.055f,.42f,-.64f),V(0,.34f,-.70f),V(-.055f,.42f,-.64f)},.06f,HLM.White,.045f);
            HLM.Plate("Forehead_Diamond_Setting",p,new[]{V(0,.645f,-.553f),V(.043f,.578f,-.609f),V(0,.52f,-.649f),V(-.043f,.578f,-.609f)},.015f,HLM.Gold,.01f);
            HLM.Plate("Forehead_Ruby_Diamond",p,new[]{V(0,.625f,-.574f),V(.027f,.579f,-.628f),V(0,.539f,-.660f),V(-.027f,.579f,-.628f)},.01f,HLM.Ruby,.014f);

            HLM.Plate("Upper_Beak_Gold_Frame",p,new[]{V(-.10f,.455f,-.615f),V(0,.482f,-.65f),V(.10f,.455f,-.615f),V(.052f,.39f,-.88f),V(0,.36f,-1.20f),V(-.052f,.39f,-.88f)},.025f,HLM.Gold,.012f);
            HLM.Plate("Upper_Beak_Ivory_Carapace",p,new[]{V(-.076f,.46f,-.639f),V(0,.48f,-.68f),V(.076f,.46f,-.639f),V(.034f,.41f,-.89f),V(0,.377f,-1.10f),V(-.034f,.41f,-.89f)},.024f,HLM.White,.025f);
            HLM.Plate("Lower_Beak_Graphite",p,new[]{V(-.07f,.395f,-.66f),V(.07f,.395f,-.66f),V(.029f,.34f,-.88f),V(0,.328f,-1.08f),V(-.029f,.34f,-.88f)},.025f,HLM.Navy,.008f);
            for(int s=-1;s<=1;s+=2)
            {
                HLM.Plate("Eye_Socket_"+s,p,new[]{V(s*.038f,.51f,-.646f),V(s*.13f,.554f,-.564f),V(s*.126f,.505f,-.59f),V(s*.06f,.478f,-.653f)},.012f,HLM.Black,.005f);
                HLM.Plate("Luminous_Phoenix_Eye_"+s,p,new[]{V(s*.052f,.511f,-.659f),V(s*.119f,.54f,-.591f),V(s*.108f,.512f,-.613f),V(s*.061f,.493f,-.663f)},.008f,HLM.Teal,.004f);
                HLM.Blade("Overhanging_Brow_"+s,p,V(s*.02f,.552f,-.65f),V(s*.17f,.595f,-.48f),.058f,.035f,HLM.White);
                HLM.Blade("Swept_Auricular_Plate_"+s,p,V(s*.112f,.48f,-.49f),V(s*.225f,.56f,.18f),.115f,.045f,HLM.White);
                HLM.Blade("Ear_Gold_Inlay_"+s,p,V(s*.145f,.52f,-.504f),V(s*.21f,.576f,.12f),.025f,.016f,HLM.Gold);
                HLM.Blade("Raked_Crown_Gold_Horn_"+s,p,V(s*.077f,.605f,-.384f),V(s*(variant ? .19f : .16f),variant ? .76f : .71f,.42f),.095f,.048f,HLM.Gold);
                HLM.Blade("Raked_Crown_Pearl_Horn_"+s,p,V(s*.061f,.627f,-.429f),V(s*(variant ? .17f : .14f),variant ? .79f : .74f,.51f),.065f,.037f,HLM.Pearl);
                HLM.Blade("Crimson_Occipital_Crest_"+s,p,V(s*.09f,.58f,-.30f),V(s*.15f,.65f,.54f),.14f,.032f,HLM.Ruby);
                HLM.Beam("Mandibular_Hinge_Pin_"+s,p,V(s*.11f,.407f,-.49f),V(s*.156f,.407f,-.49f),.034f,HLM.Frame,10);
                HLM.Orb("Mandibular_Gold_Cap_"+s,p,V(s*.15f,.407f,-.503f),V(.035f,.055f,.035f),HLM.Gold);
            }
            HLM.Blade("Central_Crown_Keel",p,V(0,.615f,-.34f),V(0,.75f,.61f),.13f,.055f,HLM.White);
            HLM.Blade("Central_Crest_Obsidian_Inset",p,V(0,.645f,-.375f),V(0,.73f,.46f),.042f,.019f,HLM.Navy);
            if(variant)
            {
                // Huang's open gold leaf crest is a conspicuous part of the product silhouette.
                Vector3[] leaf={V(.08f,.58f,.06f),V(.23f,.77f,.09f),V(.33f,.99f,.12f),V(.34f,1.21f,.15f),V(.26f,1.40f,.18f),V(.13f,1.23f,.14f),V(.035f,1.04f,.10f),V(.017f,.82f,.07f),V(.08f,.58f,.06f)};
                for(int i=0;i<leaf.Length-1;i++)
                    HLM.Beam("Open_Gold_Leaf_Crest_"+i,p,leaf[i],leaf[i+1],.019f,HLM.Gold,7);
                HLM.Blade("Leaf_Crest_Root_Armor",p,V(.07f,.54f,-.015f),V(.12f,.81f,.025f),.09f,.028f,HLM.White);
            }
        }

        private static void BuildWing(Transform root, int side, bool raised)
        {
            string sideName=side<0?"Port":"Starboard";
            float zAngle=side*(raised?18f:-11f)+(side<0?10f:-3f);
            Transform shoulder=HLM.Group("03_"+sideName+"_Wing_Shoulder",root,V(side*.16f,.13f,.015f),V(0,side*(raised?19f:-14f),zAngle));
            Vector3 elbowPos=V(side*.39f,.24f,.035f);
            Vector3 wristPos=V(side*.37f,.20f,.03f);
            WingJoint(shoulder,"Shoulder",Vector3.zero,.095f);
            HLM.Beam("Wing_Humerus_Gunmetal",shoulder,V(0,0,.03f),elbowPos,.070f,HLM.Frame,10);
            HLM.Beam("Wing_Humerus_Gold_Piston",shoulder,V(side*.04f,.04f,-.065f),elbowPos+V(0,-.045f,-.065f),.022f,HLM.Gold,8);
            Feather("Scapular_Leading_Shell",shoulder,V(side*.005f,.06f,-.075f),V(side*.47f,.34f,-.035f),.22f,true,false);
            HLM.Blade("Shoulder_Upper_Spear",shoulder,V(side*.02f,.10f,.01f),V(side*.27f,.46f,.035f),.125f,.05f,HLM.Pearl);
            HLM.Blade("Shoulder_Crimson_Subfin",shoulder,V(side*.07f,.12f,.10f),V(side*.37f,.49f,.15f),.105f,.03f,HLM.Ruby);
            for(int i=0;i<4;i++)
            {
                Vector3 a=V(side*(.035f+i*.074f),.035f+i*.034f,-.025f-i*.013f);
                Vector3 b=V(side*(.20f+i*.09f),-.34f+i*.065f,.055f);
                Feather("Scapular_Feather_"+i,shoulder,a,b,.205f-i*.010f,i%3==0,false);
            }
            Transform elbow=HLM.Group(sideName+"_Wing_Elbow",shoulder,elbowPos,V(0,side*18f,side*(raised?12f:-8f)));
            WingJoint(elbow,"Elbow",Vector3.zero,.075f);
            HLM.Beam("Radius_Alloy_Spar",elbow,V(0,0,.02f),wristPos,.052f,HLM.Frame,10);
            HLM.Beam("Ulna_Double_Actuator",elbow,V(side*.01f,-.048f,-.035f),wristPos+V(0,-.045f,-.025f),.022f,HLM.Gold,8);
            Feather("Elbow_Leading_Edge",elbow,V(-side*.025f,.065f,-.09f),wristPos+V(side*.10f,.08f,-.08f),.19f,true,false);
            for(int i=0;i<4;i++)
            {
                Vector3 a=V(side*(.015f+i*.082f),i*.048f,-.035f-i*.015f);
                Vector3 b=V(side*(.13f+i*.116f),-.52f+i*.10f,.035f+i*.018f);
                Feather("Secondary_Remex_"+i,elbow,a,b,.245f-i*.010f,i%4==1,i==0||i==2);
            }
            Transform wrist=HLM.Group(sideName+"_Wing_Wrist",elbow,wristPos,V(0,-side*16f,side*(raised?-12f:-9f)));
            WingJoint(wrist,"Wrist",Vector3.zero,.062f);
            HLM.Beam("Metacarpal_Flight_Rail",wrist,V(0,0,.02f),V(side*.36f,.29f,.035f),.035f,HLM.Navy,8);
            HLM.Beam("Metacarpal_Gold_Line",wrist,V(0,.032f,-.02f),V(side*.34f,.30f,-.01f),.014f,HLM.Gold,6);
            for(int i=0;i<6;i++)
            {
                Vector3 a=V(side*(.010f+i*.049f),.015f+i*.045f,-.025f-i*.010f);
                Vector3 b=V(side*(.20f+i*.12f),-.53f+i*.188f,.09f-i*.014f);
                Feather("Primary_Remex_"+i,wrist,a,b,.240f-i*.012f,i%3==1,i==0||i==3||i==5);
            }
            // Long dark primaries sit beneath four larger white outer armor-feathers.
            for(int i=0;i<5;i++)
            {
                Vector3 a=V(side*(.07f+i*.04f),.045f+i*.04f,.083f);
                Vector3 b=V(side*(.23f+i*.14f),-.57f+i*.19f,.145f);
                HLM.Blade("Long_Navy_Flight_Underfeather_"+i,wrist,a,b,.175f,.044f,HLM.Navy);
                HLM.Blade("Long_Ruby_Flight_Tip_"+i,wrist,Vector3.Lerp(a,b,.57f)+V(0,0,-.019f),b+V(0,0,-.02f),.12f,.028f,HLM.Ruby);
                HLM.Blade("Underfeather_Gold_Quill_"+i,wrist,a+V(0,0,-.023f),Vector3.Lerp(a,b,.68f)+V(0,0,-.023f),.022f,.015f,HLM.Gold);
            }
            for(int i=0;i<4;i++)
            {
                Vector3 a=V(side*(.025f+i*.06f),.09f+i*.05f,-.115f);
                Vector3 b=V(side*(.22f+i*.17f),-.24f+i*.19f,-.065f);
                Feather("Broad_White_Outer_Flight_Plate_"+i,wrist,a,b,.205f-i*.012f,i==2,false);
            }
            // Small upper coverts hide the flight rail and retain the layered feather overlap.
            for(int i=0;i<4;i++)
            {
                Vector3 a=V(side*(.01f+i*.085f),.08f+i*.052f,-.13f);
                Vector3 b=a+V(side*.12f,-.18f,-.005f);
                Feather("Wrist_Upper_Covert_"+i,wrist,a,b,.145f,false,false);
            }
            HLM.Blade("Alula_White_Thumb_Feather",wrist,V(side*.06f,.13f,-.13f),V(side*.28f,.53f,-.12f),.14f,.045f,HLM.White);
            HLM.Blade("Alula_Gilded_Edge",wrist,V(side*.09f,.18f,-.158f),V(side*.27f,.51f,-.145f),.029f,.015f,HLM.Gold);
            HLM.Blade("Alula_Inner_Crystal",wrist,V(side*.055f,.20f,-.035f),V(side*.13f,.49f,.035f),.085f,.025f,HLM.Ruby);
        }

        private static void WingJoint(Transform p,string name,Vector3 c,float radius)
        {
            HLM.Orb(name+"_Swivel_Ball",p,c,V(radius*1.9f,radius*1.9f,radius*1.7f),HLM.Frame);
            HLM.Ring(name+"_Golden_Bearing",p,c+V(0,0,-radius*.75f),radius*.77f,radius*.14f,HLM.Gold);
            HLM.Orb(name+"_Bearing_Recess",p,c+V(0,0,-radius*.85f),V(radius*1.14f,radius*1.14f,radius*.20f),HLM.Navy);
            HLM.Orb(name+"_Turquoise_Axis",p,c+V(0,0,-radius*.97f),V(radius*.47f,radius*.47f,radius*.13f),HLM.Teal);
            for(int i=0;i<4;i++)
            {
                float a=i*Mathf.PI*.5f+.785f;
                HLM.Orb(name+"_Fastener_"+i,p,c+V(Mathf.Cos(a)*radius*.58f,Mathf.Sin(a)*radius*.58f,-radius*.99f),V(radius*.13f,radius*.13f,radius*.10f),HLM.Gold);
            }
        }

        private static void Feather(string name,Transform parent,Vector3 a,Vector3 b,float width,bool gilded,bool crystal)
        {
            Transform feather=HLM.Group(name,parent);
            Vector3 d=b-a;
            Vector3 side=Vector3.Cross(d.normalized,Vector3.forward);
            if(side.sqrMagnitude<.001f) side=Vector3.right;
            side.Normalize();
            // A swept, shouldered polygon rather than a flat triangle.
            Vector3[] outline=new[] {
                a-side*width*.16f,
                a+d*.12f-side*width*.48f,
                a+d*.44f-side*width*.52f,
                a+d*.78f-side*width*.24f,
                b,
                a+d*.68f+side*width*.36f,
                a+d*.27f+side*width*.56f,
                a+d*.055f+side*width*.30f
            };
            HLM.Plate("Navy_Edge_Seam",feather,Shift(outline,V(0,0,.012f)),.045f,HLM.Navy,.015f);
            Vector3[] shell=new Vector3[outline.Length];
            Vector3 center=a+d*.39f;
            for(int i=0;i<outline.Length;i++) shell[i]=Vector3.Lerp(center,outline[i],.91f)+V(0,0,-.017f);
            shell[4]=b+V(0,0,-.015f);
            HLM.Plate("Pearlescent_Faceted_Remex",feather,shell,.032f,gilded?HLM.Pearl:HLM.White,.024f);
            HLM.Blade("Inlaid_Central_Vane",feather,a+d*.08f+V(0,0,-.057f),a+d*.79f+V(0,0,-.057f),width*.14f,.011f,gilded?HLM.Gold:HLM.Navy);
            HLM.Blade("Gold_Quill_Root",feather,a+d*.025f+V(0,0,-.067f),a+d*.31f+V(0,0,-.067f),width*.19f,.009f,HLM.Gold);
            HLM.Beam("Etched_Offset_Vane",feather,a+d*.32f+side*width*.18f+V(0,0,-.051f),a+d*.64f+side*width*.10f+V(0,0,-.052f),.0045f,HLM.Gold,5);
            if(crystal)
            {
                Vector3[] insert={a+d*.63f-side*width*.13f+V(0,0,-.052f),a+d*.81f-side*width*.13f+V(0,0,-.044f),b+V(0,0,-.025f),a+d*.79f+side*width*.15f+V(0,0,-.048f),a+d*.60f+side*width*.08f+V(0,0,-.054f)};
                HLM.Plate("Crimson_Crystal_Inset",feather,insert,.018f,HLM.Ruby,.013f);
            }
        }

        private static void BuildTalons(Transform root,int s)
        {
            Transform hip=HLM.Group((s<0?"Port":"Starboard")+"_Retractable_Talon",root,V(s*.13f,-.27f,-.04f));
            HLM.Orb("Hip_Pivot",hip,Vector3.zero,V(.13f,.13f,.13f),HLM.Gold);
            HLM.Beam("Tarsus_Navy_Strut",hip,V(0,-.025f,0),V(s*.025f,-.21f,-.06f),.035f,HLM.Navy,8);
            HLM.Blade("Tarsus_White_Guard",hip,V(0,-.02f,-.055f),V(s*.04f,-.25f,-.13f),.13f,.035f,HLM.White);
            HLM.Blade("Tarsus_Gold_Keel",hip,V(0,-.045f,-.081f),V(s*.03f,-.21f,-.16f),.027f,.014f,HLM.Gold);
            Transform foot=HLM.Group("Talon_Ankle",hip,V(s*.025f,-.21f,-.06f));
            HLM.Orb("Ankle_Hinge",foot,Vector3.zero,V(.085f,.08f,.09f),HLM.Frame);
            for(int i=0;i<3;i++)
            {
                float x=(i-1)*.059f;
                Transform digit=HLM.Group("Articulated_Digit_"+i,foot,V(x,-.025f,-.02f));
                HLM.Beam("Proximal_Phalanx",digit,Vector3.zero,V(x*.35f,-.04f,-.13f),.019f,HLM.Frame,8);
                HLM.Blade("Ivory_Digit_Scale",digit,V(0,.018f,-.015f),V(x*.35f,-.035f,-.14f),.045f,.023f,HLM.White);
                HLM.Beam("Distal_Phalanx",digit,V(x*.35f,-.04f,-.13f),V(x*.5f,-.085f,-.21f),.014f,HLM.Gold,7);
                HLM.Blade("Obsidian_Curved_Claw",digit,V(x*.5f,-.075f,-.18f),V(x*.55f,-.15f,-.25f),.036f,.022f,HLM.Navy);
            }
            HLM.Blade("Reverse_Hallux",foot,V(0,-.01f,.015f),V(0,-.12f,.15f),.055f,.025f,HLM.Gold);
        }

        private static void BuildTail(Transform root,int side,bool variant)
        {
            Transform tail=HLM.Group((side<0?"05_Port":"06_Starboard")+"_Segmented_Phoenix_Tail",root,V(side*.105f,-.25f,.13f));
            // Explicit flowing control path produces the hanging ribbon loops visible in the reference.
            float sway=variant?1.08f:1f;
            Vector3[] cp={
                V(0,0,0),V(side*.17f,-.22f,.10f),V(side*.43f,-.47f,.12f),
                V(side*.65f,-.76f,.09f),V(side*.73f,-1.04f,.02f),V(side*.61f,-1.28f,-.06f),
                V(side*.34f,-1.38f,-.14f),V(side*.10f,-1.28f,-.23f)
            };
            if(side>0)
            {
                for(int i=1;i<cp.Length;i++) cp[i]+=V(.10f*i/7f,.13f*Mathf.Sin(i*.5f),.10f);
            }
            if(variant)
            {
                cp=new[] {
                    V(0,0,0),V(side*.12f,.23f,.09f),V(side*.23f,.48f,.15f),
                    V(side*.31f,.75f,.13f),V(side*.34f,1.02f,.08f),V(side*.30f,1.29f,.02f),
                    V(side*.22f,1.52f,-.05f),V(side*.20f,1.69f,-.11f)
                };
                for(int i=1;i<cp.Length;i++) cp[i]=new Vector3(cp[i].x*sway,cp[i].y*(side<0 ? .94f : 1.04f),cp[i].z+.045f*Mathf.Sin(i));
            }
            Vector3 prev=cp[0];
            const int segments=25;
            for(int i=0;i<segments;i++)
            {
                float t=(i+1)/(float)segments;
                Vector3 next=Catmull(cp,t);
                Vector3 mid=(prev+next)*.5f;
                // Each vertebra has a named pivot, a dark flexible link and overlapping armor scale.
                Transform joint=HLM.Group("Tail_Vertebra_"+i.ToString("00"),tail,prev);
                HLM.Beam("Flexible_Gold_Link",joint,Vector3.zero,next-prev,.014f,HLM.Gold,7);
                HLM.Orb("Link_Bearing",joint,Vector3.zero,V(.041f,.041f,.041f),HLM.Frame);
                Vector3 tangent=(next-prev).normalized;
                Vector3 orth=Vector3.Cross(tangent,Vector3.forward).normalized;
                float w=.071f+Mathf.Sin(t*Mathf.PI)*.025f;
                Vector3 start=-tangent*.022f+V(0,0,-.027f);
                Vector3 end=next-prev+tangent*.044f+V(0,0,-.028f);
                HLM.Blade("White_Interlocking_Scale",joint,start,end,w,.027f,i%4==0?HLM.Pearl:HLM.White);
                HLM.Blade("Scale_Gilded_Ridge",joint,start+V(0,0,-.030f),end-tangent*.018f+V(0,0,-.030f),.016f,.010f,HLM.Gold);
                if(i%3==1)
                    HLM.Blade("Outward_Barbed_White_Scale",joint,mid-prev+orth*w*.16f+V(0,0,-.005f),next-prev+orth*w*.85f+tangent*.025f,.05f,.021f,HLM.White);
                prev=next;
            }
            Vector3 tailEnd=Catmull(cp,1);
            Vector3 tailTangent=(tailEnd-Catmull(cp,.94f)).normalized;
            Transform terminal=HLM.Group("Crimson_Phoenix_Terminal_Spear",tail,Vector3.zero);
            Vector3 spearEnd=tailEnd+tailTangent*.41f;
            HLM.Blade("Terminal_Navy_Setting",terminal,tailEnd-tailTangent*.09f,spearEnd,.21f,.055f,HLM.Navy);
            HLM.Blade("Terminal_Gold_Bezel",terminal,tailEnd-tailTangent*.065f+V(0,0,-.025f),spearEnd-tailTangent*.015f+V(0,0,-.026f),.177f,.033f,HLM.Gold);
            HLM.Blade("Ruby_Crystal_Flame",terminal,tailEnd-tailTangent*.025f+V(0,0,-.055f),spearEnd-tailTangent*.022f+V(0,0,-.05f),.141f,.030f,HLM.Ruby);
            HLM.Blade("Ruby_Crystal_Internal_Vane",terminal,tailEnd+tailTangent*.025f+V(0,0,-.080f),spearEnd-tailTangent*.075f+V(0,0,-.075f),.027f,.012f,HLM.Red);
            HLM.Orb("Terminal_Turquoise_Lock",terminal,tailEnd+V(0,0,-.092f),V(.035f,.061f,.019f),HLM.Teal);
        }

        private static Vector3 Catmull(Vector3[] p,float t)
        {
            float f=Mathf.Clamp01(t)*(p.Length-1);
            int i=Mathf.Min(Mathf.FloorToInt(f),p.Length-2);
            float u=f-i;
            Vector3 a=p[Mathf.Max(i-1,0)],b=p[i],c=p[i+1],d=p[Mathf.Min(i+2,p.Length-1)];
            return .5f*((2*b)+(-a+c)*u+(2*a-5*b+4*c-d)*u*u+(-a+3*b-3*c+d)*u*u*u);
        }

        private static Vector3[] Shift(Vector3[] points,Vector3 shift)
        {
            Vector3[] r=new Vector3[points.Length];
            for(int i=0;i<r.Length;i++) r[i]=points[i]+shift;
            return r;
        }

        private static Vector3 V(float x,float y,float z) { return new Vector3(x,y,z); }
    }
}
