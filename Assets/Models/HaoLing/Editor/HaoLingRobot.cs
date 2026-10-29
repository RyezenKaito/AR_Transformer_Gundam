using UnityEngine;

namespace HaoLing.Editor
{
    // Hand-authored, articulated interpretation of EMP-02's phoenix armour.
    // Every decorative plate remains an individual, named mesh for editing.
    public static class HaoLingRobot
    {
        static Vector3 V(float x, float y, float z) { return new Vector3(x,y,z); }
        static GameObject P(string n, Transform t, Material m, float d, float r, params Vector3[] p)
        { return HLM.Plate(n,t,p,d,m,r); }
        static GameObject MP(string n, Transform t, int s, Material m, float d, float r, params Vector3[] p)
        {
            for (int i=0;i<p.Length;i++) p[i].x*=s;
            if(s<0) System.Array.Reverse(p);
            return HLM.Plate(n,t,p,d,m,r);
        }
        static void Line(string n,Transform t,Vector3 a,Vector3 b,Material m,float r=.006f)
        { HLM.Beam(n,t,a,b,r,m,6); }
        static void Fastener(string n,Transform t,Vector3 p,float r=.018f)
        {
            HLM.Beam(n+" recessed socket",t,p+V(0,0,.005f),p-V(0,0,.008f),r,HLM.Frame,8);
            HLM.Beam(n+" titanium head",t,p-V(0,0,.009f),p-V(0,0,.014f),r*.55f,HLM.Gold,6);
        }
        static void Vent(string n,Transform t,Vector3 p,Vector3 size,int count=5)
        {
            HLM.Box(n+" cavity",t,p,size,HLM.Black,default(Vector3),.08f);
            for(int i=0;i<count;i++)
            {
                float y=p.y+size.y*((i+.5f)/count-.5f);
                HLM.Box(n+" fin "+(i+1),t,V(p.x,y,p.z-size.z*.55f),V(size.x*.86f,size.y/count*.30f,.012f),HLM.Frame,default(Vector3),.10f);
            }
        }
        public static Transform Build(Transform parent)
        {
            Transform root=HLM.Group("EMP-02 · HAO LING WHITE EMPEROR",parent);
            Transform hip=HLM.Group("01 · Pelvis / master hip",root,V(0,1.67f,0));
            BuildPelvis(hip);
            Transform spine=HLM.Group("02 · Segmented lumbar / spine",hip,V(0,.18f,.015f));
            BuildSpine(spine);
            Transform chest=HLM.Group("03 · Imperial phoenix cuirass",spine,V(0,.35f,0));
            BuildChest(chest);
            Transform neck=HLM.Group("04 · Neck gimbal",chest,V(0,.31f,-.005f));
            HLM.Beam("Neck dark alloy shaft",neck,V(0,-.08f,0),V(0,.19f,0),.075f,HLM.Frame,12);
            HLM.Ring("Neck articulated collar",neck,V(0,.015f,0),.093f,.018f,HLM.Gold,V(90,0,0));
            Transform head=HLM.Group("05 · Crowned avian helmet",neck,V(0,.19f,-.025f));
            BuildHead(head);
            head.localScale=V(.80f,.82f,.86f);
            BuildBackpack(chest);
            for(int s=-1;s<=1;s+=2)
            {
                BuildLeg(root,s);
                BuildArm(chest,s);
            }
            chest.localScale=V(.94f,1,.96f);
            return root;
        }
        static void BuildPelvis(Transform t)
        {
            HLM.Box("Pelvis titanium skeleton",t,V(0,-.035f,.025f),V(.40f,.25f,.28f),HLM.Frame,default(Vector3),.25f);
            P("Codpiece navy diamond",t,HLM.Navy,.13f,.04f,V(-.16f,.05f,-.19f),V(-.11f,-.22f,-.19f),V(0,-.34f,-.16f),V(.11f,-.22f,-.19f),V(.16f,.05f,-.19f));
            P("Central gold heraldic escutcheon",t,HLM.Gold,.025f,.015f,V(-.075f,.055f,-.238f),V(-.062f,-.12f,-.238f),V(0,-.235f,-.24f),V(.062f,-.12f,-.238f),V(.075f,.055f,-.238f),V(0,-.015f,-.25f));
            P("Central white narrow armour",t,HLM.White,.02f,.012f,V(-.038f,-.045f,-.266f),V(0,-.18f,-.273f),V(.038f,-.045f,-.266f),V(0,.015f,-.27f));
            HLM.Orb("Pelvis jade telemetry",t,V(0,.045f,-.27f),V(.072f,.042f,.032f),HLM.Teal);
            for(int s=-1;s<=1;s+=2)
            {
                string side=s<0?"Left":"Right";
                HLM.Orb(side+" hip ball alloy",t,V(s*.205f,-.10f,0),V(.18f,.20f,.18f),HLM.Frame);
                MP(side+" gold belt segment",t,s,HLM.Gold,.045f,.01f,V(.085f,.085f,-.17f),V(.26f,.04f,-.16f),V(.34f,-.04f,-.12f),V(.14f,-.015f,-.20f));
                Transform skirt=HLM.Group(side+" floating skirt hinge",t,V(s*.15f,-.08f,-.01f),V(0,s*-6,0));
                MP(side+" skirt navy backing",skirt,s,HLM.Navy,.09f,.025f,V(.00f,.05f,-.155f),V(.21f,.06f,-.12f),V(.31f,-.36f,-.18f),V(.16f,-.61f,-.24f),V(.045f,-.37f,-.225f));
                MP(side+" long front skirt porcelain",skirt,s,HLM.White,.065f,.045f,V(.02f,.035f,-.20f),V(.18f,.03f,-.16f),V(.24f,-.30f,-.23f),V(.13f,-.54f,-.30f),V(.07f,-.28f,-.275f));
                MP(side+" skirt gold vein",skirt,s,HLM.Gold,.016f,.006f,V(.155f,-.015f,-.226f),V(.195f,-.22f,-.267f),V(.135f,-.41f,-.323f),V(.157f,-.20f,-.28f),V(.13f,-.04f,-.23f));
                MP(side+" skirt edge break",skirt,s,HLM.Navy,.011f,.003f,V(.055f,-.125f,-.28f),V(.093f,-.20f,-.314f),V(.106f,-.31f,-.323f),V(.073f,-.235f,-.302f));
                HLM.Blade(side+" side skirt upper white quill",t,V(s*.26f,.01f,.01f),V(s*.48f,-.41f,-.025f),.24f,.07f,HLM.White);
                HLM.Blade(side+" side skirt lower navy quill",t,V(s*.30f,-.05f,.09f),V(s*.50f,-.55f,.07f),.21f,.08f,HLM.Navy);
                HLM.Blade(side+" side skirt champagne spine",t,V(s*.35f,-.02f,-.055f),V(s*.44f,-.32f,-.095f),.065f,.035f,HLM.Gold);
                HLM.Blade(side+" rear skirt pearl scale",t,V(s*.14f,-.07f,.18f),V(s*.27f,-.49f,.28f),.24f,.07f,HLM.Pearl);
                HLM.Blade(side+" rear skirt graphite insert",t,V(s*.24f,-.12f,.215f),V(s*.35f,-.42f,.31f),.11f,.04f,HLM.Navy);
                Fastener(side+" belt fastener",t,V(s*.245f,-.005f,-.181f),.014f);
            }
        }
        static void BuildSpine(Transform t)
        {
            HLM.Box("Abdomen graphite inner body",t,V(0,.00f,.015f),V(.28f,.32f,.25f),HLM.Black,default(Vector3),.20f);
            for(int i=0;i<4;i++)
            {
                float y=-.105f+i*.073f;
                P("Ventral overlapping chevron "+(i+1),t,i%2==0?HLM.Navy:HLM.Frame,.065f,.022f,V(-.15f,y+.05f,-.13f),V(-.11f,y-.012f,-.155f),V(0,y-.045f,-.19f),V(.11f,y-.012f,-.155f),V(.15f,y+.05f,-.13f),V(0,y+.012f,-.18f));
                Line("Abdominal gold edge "+i,t,V(-.105f,y+.005f,-.178f),V(0,y-.025f,-.205f),HLM.Gold,.007f);
                Line("Abdominal gold edge mirrored "+i,t,V(.105f,y+.005f,-.178f),V(0,y-.025f,-.205f),HLM.Gold,.007f);
                HLM.Box("Dorsal segmented vertebra "+i,t,V(0,y,.17f),V(.13f,.05f,.065f),HLM.Frame,default(Vector3),.20f);
            }
            for(int s=-1;s<=1;s+=2)
            {
                HLM.Beam("Oblique abdomen piston "+s,t,V(s*.14f,-.10f,.035f),V(s*.22f,.18f,.035f),.03f,HLM.Gold,10);
                HLM.Beam("Oblique abdomen hydraulic "+s,t,V(s*.135f,-.11f,.045f),V(s*.175f,.015f,.045f),.048f,HLM.Frame,10);
                HLM.Blade("Flank overlapping white shield "+s,t,V(s*.20f,.21f,-.03f),V(s*.13f,-.09f,-.16f),.13f,.06f,HLM.White);
            }
        }
        static void BuildChest(Transform t)
        {
            HLM.Box("Ribcage inner alloy block",t,V(0,.00f,.035f),V(.52f,.36f,.32f),HLM.Frame,default(Vector3),.23f);
            P("Dark falcon breastplate",t,HLM.Navy,.20f,.065f,V(-.34f,.18f,-.115f),V(-.31f,-.02f,-.18f),V(-.14f,-.21f,-.19f),V(0,-.27f,-.20f),V(.14f,-.21f,-.19f),V(.31f,-.02f,-.18f),V(.34f,.18f,-.115f),V(0,.12f,-.19f));
            for(int s=-1;s<=1;s+=2)
            {
                string n=s<0?"Left ":"Right ";
                MP(n+"clavicle leading porcelain",t,s,HLM.White,.09f,.045f,V(.02f,.15f,-.185f),V(.16f,.26f,-.12f),V(.34f,.22f,-.08f),V(.44f,.13f,-.13f),V(.28f,.10f,-.23f),V(.13f,.005f,-.275f));
                MP(n+"clavicle champagne underfeather",t,s,HLM.Gold,.04f,.025f,V(.07f,.10f,-.25f),V(.31f,.14f,-.20f),V(.35f,.10f,-.21f),V(.19f,-.045f,-.28f),V(.11f,-.11f,-.27f));
                MP(n+"pectoral main white feather",t,s,HLM.White,.06f,.045f,V(.16f,.04f,-.26f),V(.32f,.10f,-.215f),V(.31f,-.10f,-.27f),V(.22f,-.22f,-.285f),V(.13f,-.16f,-.29f));
                MP(n+"pectoral ink blue inset",t,s,HLM.Navy,.015f,.009f,V(.205f,.025f,-.315f),V(.277f,.052f,-.28f),V(.255f,-.075f,-.326f),V(.202f,-.13f,-.34f));
                MP(n+"lower rib white tusk",t,s,HLM.Pearl,.07f,.018f,V(.29f,-.09f,-.10f),V(.36f,-.15f,-.12f),V(.22f,-.32f,-.19f),V(.18f,-.18f,-.20f));
                HLM.Blade(n+"suspended gold pectoral tusk",t,V(s*.16f,.015f,-.26f),V(s*.105f,-.32f,-.25f),.075f,.03f,HLM.Gold);
                HLM.Ring(n+"avian clavicle gold hinge rim",t,V(s*.305f,.07f,-.255f),.065f,.012f,HLM.Gold);
                HLM.Beam(n+"avian clavicle black hinge socket",t,V(s*.305f,.07f,-.247f),V(s*.305f,.07f,-.267f),.046f,HLM.Black,12);
                MP(n+"air intake gold surround",t,s,HLM.Gold,.022f,.005f,V(.25f,.15f,-.259f),V(.365f,.16f,-.20f),V(.35f,.10f,-.24f),V(.25f,.09f,-.283f));
                for(int i=0;i<4;i++) Line(n+"clavicle intake slit "+i,t,V(s*(.268f+i*.021f),.115f,-.29f+i*.009f),V(s*(.275f+i*.021f),.15f,-.28f+i*.009f),HLM.Black,.007f);
                Line(n+"pectoral etched seam",t,V(s*.22f,-.05f,-.345f),V(s*.23f,-.135f,-.337f),HLM.Gold,.004f);
                Fastener(n+"clavicle titanium pin",t,V(s*.302f,.185f,-.177f),.015f);
                HLM.Box(n+"dorsal plate",t,V(s*.17f,.035f,.225f),V(.18f,.30f,.065f),HLM.Pearl,V(0,s*8,0),.25f);
                Vent(n+"back radiator",t,V(s*.16f,-.04f,.268f),V(.10f,.15f,.025f),6);
            }
            P("Central gold phoenix V crest",t,HLM.Gold,.022f,.015f,V(-.095f,.12f,-.27f),V(-.063f,-.12f,-.31f),V(0,-.22f,-.32f),V(.063f,-.12f,-.31f),V(.095f,.12f,-.27f),V(0,.025f,-.325f));
            P("Imperial turquoise heart lens",t,HLM.Teal,.025f,.022f,V(-.063f,.055f,-.32f),V(-.047f,-.045f,-.35f),V(0,-.115f,-.365f),V(.047f,-.045f,-.35f),V(.063f,.055f,-.32f),V(0,.095f,-.32f));
            P("Ruby breast crown",t,HLM.Ruby,.02f,.007f,V(-.022f,.13f,-.25f),V(0,.20f,-.23f),V(.022f,.13f,-.25f),V(0,.06f,-.31f));
            HLM.Blade("Sternum porcelain spear",t,V(0,-.07f,-.36f),V(0,-.30f,-.27f),.08f,.035f,HLM.White);
        }
        static void BuildHead(Transform t)
        {
            HLM.Orb("Helmet graphite cranium",t,V(0,.045f,.015f),V(.29f,.33f,.25f),HLM.Navy);
            P("Recessed black face cavity",t,HLM.Black,.06f,.012f,V(-.125f,.105f,-.13f),V(-.125f,-.035f,-.155f),V(-.065f,-.145f,-.15f),V(.065f,-.145f,-.15f),V(.125f,-.035f,-.155f),V(.125f,.105f,-.13f),V(0,.16f,-.13f));
            P("Face porcelain avian mask",t,HLM.White,.07f,.025f,V(-.084f,.035f,-.195f),V(-.087f,-.060f,-.215f),V(-.038f,-.14f,-.237f),V(0,-.20f,-.24f),V(.038f,-.14f,-.237f),V(.087f,-.060f,-.215f),V(.084f,.035f,-.195f),V(0,-.005f,-.255f));
            P("Forehead white sagittal cap",t,HLM.White,.14f,.025f,V(-.09f,.08f,-.14f),V(-.07f,.22f,-.10f),V(0,.30f,-.01f),V(.07f,.22f,-.10f),V(.09f,.08f,-.14f),V(0,.035f,-.19f));
            P("Forehead ruby jewel gold housing",t,HLM.Gold,.018f,.006f,V(-.043f,.17f,-.17f),V(0,.255f,-.12f),V(.043f,.17f,-.17f),V(0,.065f,-.205f));
            P("Forehead ruby jewel",t,HLM.Ruby,.018f,.012f,V(-.028f,.165f,-.19f),V(0,.226f,-.15f),V(.028f,.165f,-.19f),V(0,.092f,-.222f));
            P("Tall magenta central imperial crown",t,HLM.Ruby,.035f,.018f,V(-.035f,.20f,-.12f),V(-.028f,.36f,-.055f),V(0,.47f,-.025f),V(.028f,.36f,-.055f),V(.035f,.20f,-.12f),V(0,.13f,-.17f));
            P("Chin porcelain bird beak",t,HLM.Pearl,.027f,.016f,V(-.027f,-.105f,-.257f),V(0,-.223f,-.255f),V(.027f,-.105f,-.257f),V(0,-.085f,-.27f));
            Line("Mask gold median ridge",t,V(0,-.075f,-.28f),V(0,-.17f,-.276f),HLM.Gold,.004f);
            for(int s=-1;s<=1;s+=2)
            {
                string n=s<0?"Left ":"Right ";
                MP(n+"jade eye illuminated lens",t,s,HLM.Teal,.012f,.006f,V(.014f,.02f,-.195f),V(.104f,.055f,-.175f),V(.091f,.007f,-.192f),V(.029f,-.008f,-.211f));
                MP(n+"sharp porcelain eyebrow",t,s,HLM.White,.04f,.012f,V(.006f,.062f,-.197f),V(.107f,.103f,-.153f),V(.145f,.058f,-.145f),V(.034f,.031f,-.217f));
                MP(n+"cheek outer spear",t,s,HLM.White,.065f,.02f,V(.104f,.058f,-.148f),V(.154f,.012f,-.095f),V(.153f,-.13f,-.10f),V(.112f,-.205f,-.105f),V(.068f,-.045f,-.174f));
                MP(n+"cheek gold inner feather",t,s,HLM.Gold,.02f,.009f,V(.105f,-.025f,-.195f),V(.121f,-.065f,-.17f),V(.109f,-.12f,-.175f),V(.097f,-.07f,-.216f));
                HLM.Blade(n+"temple rear white crown",t,V(s*.125f,.12f,.015f),V(s*.29f,.29f,.15f),.15f,.07f,HLM.White);
                HLM.Blade(n+"rear navy feather",t,V(s*.11f,.08f,.10f),V(s*.205f,-.19f,.19f),.13f,.06f,HLM.Navy);
                HLM.Blade(n+"crown rising porcelain antler",t,V(s*.095f,.17f,-.07f),V(s*.15f,.57f,.035f),.079f,.045f,HLM.White);
                HLM.Blade(n+"crown golden spear",t,V(s*.10f,.21f,-.10f),V(s*.12f,.63f,.015f),.038f,.020f,HLM.Gold);
                HLM.Blade(n+"sweeping white lateral V antenna",t,V(s*.11f,.16f,-.08f),V(s*.31f,.30f,-.025f),.066f,.035f,HLM.White);
                HLM.Blade(n+"lateral V gold inset",t,V(s*.17f,.192f,-.11f),V(s*.30f,.305f,-.055f),.022f,.013f,HLM.Gold);
                HLM.Beam(n+"ear hinge",t,V(s*.145f,-.02f,.015f),V(s*.174f,-.02f,.015f),.047f,HLM.Gold,12);
                Fastener(n+"helmet inset rivet",t,V(s*.127f,-.025f,-.153f),.010f);
                for(int i=0;i<3;i++) Line(n+"cheek micro ventilation "+i,t,V(s*(.106f+i*.009f),-.095f,-.185f+i*.009f),V(s*(.110f+i*.009f),-.124f,-.18f+i*.009f),HLM.Navy,.003f);
            }
            HLM.Blade("Rear occipital white crest",t,V(0,.21f,.12f),V(0,.41f,.27f),.14f,.07f,HLM.Pearl);
            HLM.Box("Rear crown jade telemetry",t,V(0,.13f,.17f),V(.045f,.067f,.018f),HLM.Teal,default(Vector3),.2f);
        }
        static void BuildBackpack(Transform chest)
        {
            Transform t=HLM.Group("06 · Feng Huang carrier backpack",chest,V(0,.025f,.27f));
            HLM.Box("Pack central titanium casting",t,V(0,0,.055f),V(.24f,.34f,.20f),HLM.Frame,default(Vector3),.2f);
            HLM.Box("Pack central ivory shield",t,V(0,.025f,.185f),V(.16f,.26f,.055f),HLM.White,default(Vector3),.25f);
            for(int s=-1;s<=1;s+=2)
            {
                HLM.Group(s<0?"FengDock":"HuangDock",t,V(s*.20f,.14f,.08f),V(0,s*20,0));
                HLM.Beam("Dock articulated arm "+s,t,V(s*.08f,.09f,.06f),V(s*.25f,.15f,.12f),.055f,HLM.Frame,12);
                HLM.Orb("Dock gold swivel "+s,t,V(s*.25f,.15f,.12f),V(.13f,.13f,.13f),HLM.Gold);
                HLM.Box("Thruster housing "+s,t,V(s*.16f,-.12f,.12f),V(.14f,.25f,.17f),HLM.Navy,V(s*5,0,s*9),.2f);
                HLM.Beam("Thruster gold rim "+s,t,V(s*.16f,-.22f,.11f),V(s*.16f,-.26f,.13f),.065f,HLM.Gold,12);
                HLM.Beam("Thruster nozzle black "+s,t,V(s*.16f,-.264f,.13f),V(s*.16f,-.27f,.13f),.049f,HLM.Black,12);
                HLM.Beam("Thruster teal turbine "+s,t,V(s*.16f,-.273f,.13f),V(s*.16f,-.28f,.13f),.028f,HLM.Teal,10);
                HLM.Blade("Backpack ivory flight stabilizer "+s,t,V(s*.10f,.13f,.13f),V(s*.34f,.40f,.29f),.15f,.06f,HLM.Pearl);
                HLM.Blade("Backpack gold stabilizer stripe "+s,t,V(s*.18f,.21f,.095f),V(s*.32f,.395f,.25f),.035f,.02f,HLM.Gold);
            }
        }
        static void BuildLeg(Transform root,int s)
        {
            string side=s<0?"Left":"Right";
            Transform thigh=HLM.Group("07 · "+side+" hip / upper leg",root,V(s*.255f,1.58f,.015f),V(0,0,s*5.5f));
            thigh.localScale=V(.90f,1.09f,.95f);
            HLM.Orb(side+" hip rotation ball",thigh,V(0,0,0),V(.20f,.20f,.20f),HLM.Frame);
            HLM.Beam(side+" femur structural spar",thigh,V(0,-.03f,.015f),V(s*.015f,-.53f,.01f),.075f,HLM.Frame,10);
            HLM.Box(side+" thigh graphite armour base",thigh,V(0,-.26f,0),V(.22f,.40f,.22f),HLM.Navy,default(Vector3),.2f);
            P(side+" thigh main porcelain lance",thigh,HLM.White,.08f,.05f,V(-.13f,-.055f,-.105f),V(-.14f,-.25f,-.12f),V(-.07f,-.49f,-.15f),V(.06f,-.51f,-.15f),V(.14f,-.23f,-.12f),V(.10f,-.045f,-.10f));
            P(side+" thigh face graphite recess",thigh,HLM.Navy,.012f,.004f,V(-.062f,-.12f,-.177f),V(-.04f,-.29f,-.192f),V(.027f,-.41f,-.193f),V(.07f,-.26f,-.18f),V(.04f,-.15f,-.17f));
            HLM.Blade(side+" thigh gold external ridge",thigh,V(s*.11f,-.10f,-.11f),V(s*.09f,-.44f,-.16f),.05f,.02f,HLM.Gold);
            HLM.Blade(side+" outer thigh sweeping scale",thigh,V(s*.13f,-.12f,.01f),V(s*.17f,-.50f,.015f),.16f,.06f,HLM.Pearl);
            HLM.Beam(side+" rear thigh piston cylinder",thigh,V(s*.055f,-.12f,.12f),V(s*.055f,-.30f,.12f),.038f,HLM.Frame,10);
            HLM.Beam(side+" rear thigh piston ram",thigh,V(s*.055f,-.26f,.12f),V(s*.055f,-.53f,.12f),.021f,HLM.Gold,10);
            Fastener(side+" upper thigh fastener",thigh,V(-s*.08f,-.11f,-.155f),.013f);
            Transform knee=HLM.Group(side+" knee hinge",thigh,V(s*.015f,-.58f,0),V(2,0,0));
            HLM.Beam(side+" knee axle",knee,V(-.12f,0,0),V(.12f,0,0),.085f,HLM.Frame,12);
            for(int j=-1;j<=1;j+=2)
            {
                HLM.Beam(side+" knee joint gold cap "+j,knee,V(j*.122f,0,0),V(j*.136f,0,0),.068f,HLM.Gold,12);
                HLM.Beam(side+" knee joint navy hub "+j,knee,V(j*.137f,0,0),V(j*.142f,0,0),.044f,HLM.Navy,10);
            }
            P(side+" pointed knee ivory crest",knee,HLM.White,.08f,.055f,V(-.13f,.06f,-.09f),V(-.105f,-.12f,-.14f),V(0,-.205f,-.17f),V(.105f,-.12f,-.14f),V(.13f,.06f,-.09f),V(0,.18f,-.13f));
            P(side+" kneecap gold inset",knee,HLM.Gold,.014f,.01f,V(-.053f,.055f,-.193f),V(0,-.102f,-.23f),V(.053f,.055f,-.193f),V(0,.13f,-.17f));
            P(side+" kneecap white gem",knee,HLM.Pearl,.013f,.004f,V(-.026f,.045f,-.211f),V(0,-.055f,-.244f),V(.026f,.045f,-.211f),V(0,.092f,-.196f));
            P(side+" knee ruby triangular identification",knee,HLM.Ruby,.012f,.004f,V(-.030f,.10f,-.190f),V(0,.045f,-.215f),V(.030f,.10f,-.190f));
            Transform shin=HLM.Group(side+" shin / lower leg",knee,V(0,-.055f,.015f));
            HLM.Beam(side+" tibia mechanical spine",shin,V(0,-.015f,.02f),V(0,-.55f,.02f),.063f,HLM.Frame,10);
            HLM.Box(side+" calf navy core",shin,V(0,-.26f,.045f),V(.19f,.44f,.20f),HLM.Navy,default(Vector3),.15f);
            P(side+" elegant long white greave",shin,HLM.White,.085f,.07f,V(-.12f,-.075f,-.09f),V(-.11f,-.28f,-.13f),V(-.066f,-.53f,-.17f),V(0,-.60f,-.15f),V(.085f,-.49f,-.13f),V(.15f,-.20f,-.07f),V(.08f,-.055f,-.09f));
            P(side+" shin long dark split",shin,HLM.Navy,.012f,.004f,V(-.025f,-.16f,-.201f),V(-.02f,-.32f,-.233f),V(.018f,-.515f,-.224f),V(.048f,-.40f,-.217f),V(.03f,-.235f,-.215f));
            HLM.Blade(side+" shin champagne leading spear",shin,V(s*.065f,-.20f,-.17f),V(s*.025f,-.56f,-.18f),.047f,.022f,HLM.Gold);
            HLM.Blade(side+" outer calf porcelain flying fin",shin,V(s*.12f,-.13f,.01f),V(s*.25f,-.49f,.13f),.18f,.065f,HLM.Pearl);
            HLM.Blade(side+" calf navy feather underlayer",shin,V(s*.13f,-.19f,.08f),V(s*.21f,-.57f,.16f),.13f,.055f,HLM.Navy);
            HLM.Blade(side+" calf rear gold spur",shin,V(s*.08f,-.18f,.14f),V(s*.10f,-.43f,.24f),.065f,.035f,HLM.Gold);
            HLM.Blade(side+" calf ruby vane",shin,V(s*.16f,-.23f,.11f),V(s*.27f,-.45f,.20f),.055f,.025f,HLM.Ruby);
            Vent(side+" calf aft cooling",shin,V(0,-.19f,.16f),V(.10f,.15f,.026f),5);
            HLM.Beam(side+" Achilles chrome piston",shin,V(0,-.23f,.16f),V(0,-.58f,.105f),.021f,HLM.Gold,10);
            Fastener(side+" shin service screw",shin,V(-s*.075f,-.19f,-.16f),.011f);
            for(int i=0;i<2;i++) Line(side+" shin technical panel "+i,shin,V(-s*.088f,-.29f-i*.035f,-.174f),V(-s*.055f,-.305f-i*.035f,-.208f),HLM.Frame,.004f);
            Transform foot=HLM.Group(side+" ankle / talon foot",shin,V(0,-.61f,-.015f),V(-2,s*9,0));
            BuildFoot(foot,side,s);
        }
        static void BuildFoot(Transform t,string side,int s)
        {
            HLM.Orb(side+" ankle gimbal",t,V(0,.03f,.015f),V(.13f,.13f,.15f),HLM.Frame);
            HLM.Box(side+" foot sole",t,V(0,-.06f,-.09f),V(.22f,.08f,.39f),HLM.Frame,default(Vector3),.20f);
            P(side+" foot aerodynamic toe top",t,HLM.White,.15f,.04f,V(-.11f,-.015f,-.265f),V(-.12f,.045f,-.09f),V(-.075f,.08f,.025f),V(.075f,.08f,.025f),V(.12f,.045f,-.09f),V(.07f,-.035f,-.36f),V(0,-.065f,-.39f));
            HLM.Blade(side+" pointed navy toe",t,V(0,-.025f,-.205f),V(0,-.055f,-.43f),.14f,.07f,HLM.Navy);
            HLM.Blade(side+" toe gold median ridge",t,V(0,.04f,-.14f),V(0,-.025f,-.37f),.035f,.025f,HLM.Gold);
            HLM.Blade(side+" ankle front porcelain cuff",t,V(0,.135f,-.06f),V(0,-.005f,-.18f),.22f,.055f,HLM.White);
            for(int j=-1;j<=1;j+=2)
            {
                HLM.Blade(side+" foot side spur "+j,t,V(j*.08f,.02f,.01f),V(j*.145f,-.06f,-.25f),.085f,.055f,HLM.Pearl);
                HLM.Beam(side+" ankle pivot "+j,t,V(j*.089f,.04f,.01f),V(j*.112f,.04f,.01f),.04f,HLM.Gold,10);
            }
            HLM.Box(side+" heel stabilizer",t,V(0,-.025f,.115f),V(.15f,.10f,.16f),HLM.Navy,default(Vector3),.2f);
            HLM.Blade(side+" heel ivory spur",t,V(0,.03f,.08f),V(0,-.015f,.23f),.11f,.055f,HLM.White);
            for(int i=0;i<3;i++) Line(side+" toe articulation joint "+i,t,V(-.065f,-.008f,-.16f-i*.038f),V(.065f,-.008f,-.16f-i*.038f),HLM.Frame,.004f);
        }
        static void BuildArm(Transform chest,int s)
        {
            string side=s<0?"Left":"Right";
            Transform shoulder=HLM.Group("08 · "+side+" shoulder gimbal",chest,V(s*.40f,.13f,.005f),V(0,s*-6,s*10));
            HLM.Orb(side+" shoulder alloy sphere",shoulder,V(s*.025f,-.02f,0),V(.24f,.24f,.24f),HLM.Frame);
            HLM.Beam(side+" shoulder rotary gold axle",shoulder,V(0,0,0),V(s*.15f,0,0),.092f,HLM.Gold,12);
            MP(side+" shoulder dark feather foundation",shoulder,s,HLM.Navy,.13f,.05f,V(-.04f,.12f,-.105f),V(.15f,.23f,-.065f),V(.46f,.07f,-.03f),V(.31f,-.05f,-.14f),V(.12f,-.21f,-.17f),V(-.035f,-.11f,-.13f));
            MP(side+" shoulder porcelain canopy",shoulder,s,HLM.White,.085f,.055f,V(-.055f,.14f,-.15f),V(.16f,.255f,-.10f),V(.39f,.15f,-.09f),V(.49f,.025f,-.055f),V(.24f,.045f,-.21f),V(.07f,-.13f,-.23f));
            MP(side+" shoulder gold leading accent",shoulder,s,HLM.Gold,.025f,.009f,V(.13f,.20f,-.17f),V(.36f,.12f,-.16f),V(.43f,.05f,-.14f),V(.22f,.095f,-.25f),V(.13f,-.02f,-.25f),V(.085f,-.06f,-.255f));
            HLM.Blade(side+" shoulder rising white phoenix plume",shoulder,V(s*.07f,.16f,.015f),V(s*.24f,.64f,.16f),.20f,.075f,HLM.White);
            HLM.Blade(side+" shoulder rising champagne plume tip",shoulder,V(s*.207f,.525f,.094f),V(s*.24f,.65f,.133f),.056f,.025f,HLM.Gold);
            HLM.Blade(side+" shoulder secondary porcelain rising feather",shoulder,V(s*.18f,.17f,.075f),V(s*.37f,.47f,.24f),.13f,.06f,HLM.Pearl);
            HLM.Blade(side+" shoulder aft navy blade",shoulder,V(s*.21f,.10f,.11f),V(s*.40f,-.25f,.20f),.18f,.08f,HLM.Navy);
            HLM.Blade(side+" shoulder trailing porcelain feather",shoulder,V(s*.26f,.08f,-.005f),V(s*.38f,-.31f,.08f),.19f,.065f,HLM.Pearl);
            HLM.Blade(side+" shoulder ruby inset feather",shoulder,V(s*.25f,.01f,-.035f),V(s*.37f,-.29f,.055f),.072f,.03f,HLM.Ruby);
            Fastener(side+" shoulder service screw",shoulder,V(s*.15f,.16f,-.19f),.014f);
            for(int i=0;i<3;i++) Line(side+" shoulder feather panel cut "+i,shoulder,V(s*(.20f+i*.035f),.13f-i*.012f,-.183f),V(s*(.24f+i*.035f),.07f-i*.012f,-.219f),HLM.Navy,.004f);
            Transform upper=HLM.Group(side+" upper arm pivot",shoulder,V(s*.08f,-.13f,0),V(0,0,s*4));
            HLM.Beam(side+" humerus frame",upper,V(0,.00f,0),V(0,-.32f,0),.062f,HLM.Frame,12);
            HLM.Box(side+" biceps graphite block",upper,V(0,-.12f,.015f),V(.15f,.25f,.17f),HLM.Navy,default(Vector3),.2f);
            P(side+" upper arm frontal white chevron",upper,HLM.White,.065f,.03f,V(-.095f,-.025f,-.08f),V(-.095f,-.15f,-.10f),V(0,-.265f,-.135f),V(.095f,-.15f,-.10f),V(.095f,-.025f,-.08f),V(0,-.07f,-.125f));
            P(side+" bicep central gold quill",upper,HLM.Gold,.018f,.006f,V(-.028f,-.07f,-.155f),V(0,-.215f,-.178f),V(.028f,-.07f,-.155f),V(0,-.09f,-.17f));
            HLM.Beam(side+" tricep piston",upper,V(s*.055f,-.03f,.10f),V(s*.055f,-.29f,.10f),.018f,HLM.Gold,10);
            Transform elbow=HLM.Group(side+" elbow hinge",upper,V(0,-.33f,0),V(-10,0,s*-5));
            HLM.Beam(side+" elbow alloy axle",elbow,V(-.10f,0,0),V(.10f,0,0),.067f,HLM.Frame,12);
            HLM.Beam(side+" elbow gold outer cap",elbow,V(s*.103f,0,0),V(s*.12f,0,0),.055f,HLM.Gold,10);
            HLM.Blade(side+" elbow rear hooked white spur",elbow,V(0,.04f,.075f),V(0,.17f,.26f),.15f,.055f,HLM.White);
            Transform fore=HLM.Group(side+" forearm articulated cuff",elbow,V(0,-.06f,0),V(s<0?-3:0,0,0));
            BuildForearm(fore,side,s);
            Transform hand=HLM.Group(side+" wrist / hand",fore,V(0,-.345f,-.005f),V(0,s*7,0));
            BuildHand(hand,side,s);
            if(s<0) BuildLance(hand);
            else BuildShield(fore);
        }
        static void BuildForearm(Transform t,string side,int s)
        {
            HLM.Beam(side+" radius structural frame",t,V(0,.035f,0),V(0,-.30f,0),.06f,HLM.Frame,10);
            HLM.Box(side+" forearm dark body",t,V(0,-.13f,.015f),V(.18f,.28f,.20f),HLM.Navy,default(Vector3),.2f);
            P(side+" forearm long ivory face",t,HLM.White,.08f,.04f,V(-.11f,.05f,-.095f),V(-.13f,-.10f,-.115f),V(-.065f,-.32f,-.145f),V(.075f,-.32f,-.14f),V(.13f,-.09f,-.105f),V(.10f,.06f,-.09f),V(0,-.03f,-.16f));
            P(side+" forearm navy arrow recess",t,HLM.Navy,.016f,.006f,V(-.042f,.015f,-.184f),V(0,-.18f,-.219f),V(.045f,-.045f,-.19f),V(.025f,.005f,-.183f));
            HLM.Blade(side+" forearm champagne cutting edge",t,V(s*.08f,.025f,-.15f),V(s*.055f,-.29f,-.17f),.043f,.02f,HLM.Gold);
            HLM.Blade(side+" forearm turquoise power blade stripe",t,V(s*.027f,-.035f,-.211f),V(s*.016f,-.265f,-.216f),.023f,.013f,HLM.Teal);
            HLM.Blade(side+" forearm outer swept ivory fin",t,V(s*.09f,.035f,.025f),V(s*.255f,-.34f,.065f),.17f,.065f,HLM.White);
            HLM.Blade(side+" forearm ruby quill",t,V(s*.145f,.005f,-.005f),V(s*.235f,-.31f,.025f),.072f,.028f,HLM.Ruby);
            HLM.Ring(side+" wrist gold bearing",t,V(0,-.325f,0),.075f,.014f,HLM.Gold,V(90,0,0));
            Fastener(side+" wrist maintenance screw",t,V(-s*.072f,-.20f,-.162f),.011f);
            Vent(side+" forearm rear radiator",t,V(0,-.14f,.14f),V(.11f,.13f,.02f),4);
        }
        static void BuildHand(Transform t,string side,int s)
        {
            HLM.Beam(side+" wrist shaft",t,V(0,.025f,0),V(0,-.07f,0),.045f,HLM.Frame,10);
            HLM.Box(side+" hand palm",t,V(0,-.09f,0),V(.145f,.15f,.105f),HLM.Frame,default(Vector3),.20f);
            P(side+" backhand white armour",t,HLM.White,.035f,.015f,V(-.071f,-.035f,-.063f),V(-.07f,-.13f,-.066f),V(0,-.17f,-.073f),V(.07f,-.13f,-.066f),V(.071f,-.035f,-.063f));
            HLM.Box(side+" hand gold crest",t,V(0,-.09f,-.092f),V(.03f,.062f,.011f),HLM.Gold,default(Vector3),.15f);
            for(int i=0;i<4;i++)
            {
                float x=(i-1.5f)*.033f;
                Transform finger=HLM.Group(side+" finger "+(i+1)+" proximal",t,V(x,-.15f,.015f));
                HLM.Box(side+" finger "+i+" phalanx 1",finger,V(0,-.035f,.002f),V(.027f,.066f,.035f),HLM.Navy,V(12,0,0),.16f);
                HLM.Orb(side+" finger "+i+" knuckle",finger,V(0,-.066f,.018f),V(.031f,.028f,.03f),HLM.Frame);
                HLM.Box(side+" finger "+i+" phalanx 2",finger,V(0,-.080f,.037f),V(.025f,.033f,.05f),HLM.Frame,V(65,0,0),.16f);
                HLM.Box(side+" finger "+i+" gold knuckle cap",finger,V(0,-.017f,-.019f),V(.025f,.028f,.012f),HLM.Gold,default(Vector3),.20f);
            }
            Transform thumb=HLM.Group(side+" thumb opposition pivot",t,V(-s*.075f,-.085f,0),V(0,0,-s*35));
            HLM.Box(side+" thumb first phalanx",thumb,V(0,-.029f,.018f),V(.039f,.065f,.045f),HLM.Frame,default(Vector3),.20f);
            HLM.Box(side+" thumb distal phalanx",thumb,V(s*.01f,-.067f,.025f),V(.035f,.04f,.04f),HLM.Navy,V(10,0,s*20),.20f);
        }
        static void BuildLance(Transform hand)
        {
            // Blade axis extends down and away from the robot; separate grip and pommel.
            Transform t=HLM.Group("09 · GUAN RI / LING XIAO combined spear",hand,V(0,-.125f,.05f));
            Vector3 a=V(.11f,.29f,0), b=V(-1.43f,-1.045f,-.10f);
            Vector3 dir=(b-a).normalized, cross=V(-dir.y,dir.x,0);
            HLM.Beam("Long gunmetal shaft",t,a,b,.026f,HLM.Frame,12);
            HLM.Beam("Wrapped dark handle",t,-dir*.17f,dir*.20f,.043f,HLM.Navy,12);
            for(int i=0;i<9;i++)
            {
                Vector3 q=dir*(-.14f+i*.038f);
                HLM.Beam("Handle gold knurl ring "+i,t,q-dir*.004f,q+dir*.004f,.046f,HLM.Gold,12);
            }
            HLM.Beam("Blade neck collar",t,dir*.30f,dir*.39f,.07f,HLM.Gold,8);
            HLM.Blade("Long white spear main edge",t,dir*.36f+V(0,0,-.018f),b+dir*.27f,.145f,.045f,HLM.White);
            HLM.Blade("Long spear navy inner fuller",t,dir*.46f+V(0,0,-.08f),b+dir*.19f+V(0,0,-.06f),.070f,.020f,HLM.Navy);
            HLM.Blade("Long spear gold central edge",t,dir*.44f+cross*.029f+V(0,0,-.105f),b+dir*.12f+cross*.015f+V(0,0,-.09f),.015f,.010f,HLM.Gold);
            HLM.Blade("Spear crossguard left white bird",t,dir*.27f-cross*.04f,dir*.18f-cross*.27f,.12f,.07f,HLM.White);
            HLM.Blade("Spear crossguard right gold bird",t,dir*.27f+cross*.04f,dir*.40f+cross*.26f,.10f,.055f,HLM.Gold);
            HLM.Orb("Spear turquoise guard lens",t,dir*.29f+V(0,0,-.075f),V(.08f,.08f,.035f),HLM.Teal);
            HLM.Blade("Spear upper short ling xiao counterblade",t,-dir*.23f,a-dir*.35f,.15f,.045f,HLM.Pearl);
            HLM.Blade("Spear counterblade ruby insert",t,-dir*.28f+V(0,0,-.04f),a-dir*.30f+V(0,0,-.04f),.065f,.02f,HLM.Ruby);
        }
        static void BuildShield(Transform fore)
        {
            Transform t=HLM.Group("10 · Phoenix feather vambrace shield",fore,V(.13f,-.06f,-.025f),V(0,-8,-8));
            P("Shield navy aerodynamic base",t,HLM.Navy,.095f,.05f,V(-.05f,.18f,-.10f),V(.20f,.30f,-.08f),V(.30f,.15f,-.06f),V(.24f,-.40f,-.12f),V(.03f,-.69f,-.16f),V(-.06f,-.26f,-.15f));
            P("Shield main white avian feather",t,HLM.White,.065f,.05f,V(-.025f,.17f,-.15f),V(.15f,.28f,-.12f),V(.23f,.135f,-.11f),V(.165f,-.30f,-.19f),V(.015f,-.59f,-.23f),V(.015f,-.15f,-.24f));
            P("Shield leading champagne inlay",t,HLM.Gold,.018f,.009f,V(.135f,.20f,-.18f),V(.17f,.10f,-.18f),V(.12f,-.26f,-.255f),V(.03f,-.49f,-.28f),V(.07f,-.20f,-.28f),V(.095f,.08f,-.21f));
            P("Shield navy tapered inset",t,HLM.Navy,.015f,.008f,V(.03f,.11f,-.233f),V(.07f,.035f,-.255f),V(.055f,-.14f,-.297f),V(.025f,-.30f,-.29f),V(-.005f,-.07f,-.27f));
            HLM.Blade("Shield broad ruby outer quill",t,V(.21f,.11f,-.015f),V(.38f,-.51f,-.015f),.16f,.045f,HLM.Ruby);
            HLM.Blade("Shield ivory outer quill",t,V(.255f,.18f,.015f),V(.42f,-.29f,.035f),.14f,.055f,HLM.White);
            HLM.Blade("Shield long navy aft quill",t,V(.14f,.20f,.055f),V(.25f,-.66f,.07f),.14f,.055f,HLM.Navy);
            HLM.Orb("Shield turquoise activation gem",t,V(.095f,.14f,-.225f),V(.07f,.11f,.025f),HLM.Teal);
            for(int i=0;i<3;i++) Line("Shield etched feather gill "+i,t,V(.08f,-.10f-i*.055f,-.28f),V(.135f,-.06f-i*.055f,-.24f),HLM.Frame,.004f);
            Fastener("Shield top retaining pin",t,V(.17f,.20f,-.174f),.014f);
        }
    }
}
