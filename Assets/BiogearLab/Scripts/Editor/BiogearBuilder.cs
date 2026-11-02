using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ProjectEnvironments;
using ProjectEnvironments.Editor;

namespace BiogearLab.Editor
{
    public static class BiogearBuilder
    {
        public const string ScenePath="Assets/BiogearLab/Scenes/BiogearLab_Reconstruction.unity";
        const string Base="Assets/BiogearLab";
        static Transform root;
        static BiogearLayoutConfig cfg;
        static Material ivory,dark,steel,orange,yellow,cyan,grass,rock,blue,glass,beam,water,textMaterial;
        static Mesh boxMesh,deckMesh;
        static int meshId;
        static readonly Dictionary<string,Mesh> meshCache=new Dictionary<string,Mesh>();
        static readonly Dictionary<string,Material> materialCache=new Dictionary<string,Material>();
        static Vector3 P(float radius,float degrees,float y=0){float a=degrees*Mathf.Deg2Rad;return new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius);}
        static Transform Group(string name,Transform parent){var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
        static Mesh SaveMesh(Mesh mesh,string name)
        {
            mesh.name=name;string path=Base+"/Meshes/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old)
            {
                // Copy vertex data explicitly: native Mesh GPU buffers must be refreshed on rebuild.
                old.Clear();old.indexFormat=mesh.indexFormat;old.vertices=mesh.vertices;old.triangles=mesh.triangles;old.normals=mesh.normals;old.uv=mesh.uv;
                old.RecalculateBounds();old.UploadMeshData(false);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);return old;
            }
            AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        class Geometry
        {
            public List<Vector3> v=new List<Vector3>();public List<int> t=new List<int>();public List<Vector2> uv=new List<Vector2>();
            public void Tri(Vector3 a,Vector3 b,Vector3 c,Vector3 normal)
            {
                if(Vector3.Dot(Vector3.Cross(b-a,c-a),normal)<0){var d=b;b=c;c=d;}
                int n=v.Count;v.Add(a);v.Add(b);v.Add(c);t.Add(n);t.Add(n+1);t.Add(n+2);
                foreach(var p in new[]{a,b,c})uv.Add(new Vector2(p.x+p.y*.35f,p.z+p.y*.7f)*.22f);
            }
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector3 normal){Tri(a,b,c,normal);Tri(a,c,d,normal);}
            public Mesh Mesh(){var m=new Mesh();m.indexFormat=IndexFormat.UInt32;m.SetVertices(v);m.SetTriangles(t,0);m.SetUVs(0,uv);m.RecalculateNormals();m.RecalculateBounds();return m;}
        }
        static Mesh Ring(float inner,float outer,float innerY,float outerY,float thickness,float start=0,float end=360,int segments=64)
        {
            string key=$"{inner:F3}_{outer:F3}_{innerY:F3}_{outerY:F3}_{thickness:F3}_{start:F3}_{end:F3}_{segments}";
            if(meshCache.TryGetValue(key,out var cached))return cached;
            var g=new Geometry();
            for(int i=0;i<segments;i++)
            {
                float a=Mathf.Lerp(start,end,i/(float)segments),b=Mathf.Lerp(start,end,(i+1f)/segments);
                var p=P(inner,a,innerY);var q=P(outer,a,outerY);var r=P(outer,b,outerY);var s=P(inner,b,innerY);var down=Vector3.down*thickness;
                if(inner<=0){g.Tri(p,q,r,Vector3.up);g.Tri(p+down,r+down,q+down,Vector3.down);}
                else {g.Quad(p,q,r,s,Vector3.up);g.Quad(p+down,s+down,r+down,q+down,Vector3.down);g.Quad(p,s,s+down,p+down,-P(1,(a+b)/2));}
                g.Quad(q,q+down,r+down,r,P(1,(a+b)/2));
                if(i==0)g.Quad(p,p+down,q+down,q,P(1,a-90));if(i==segments-1)g.Quad(s,r,r+down,s+down,P(1,b+90));
            }
            var m=SaveMesh(g.Mesh(),"Ring_"+(meshId++).ToString("D4"));meshCache[key]=m;return m;
        }
        static Mesh BoxMesh(bool floor=false)
        {
            var g=new Geometry();float[] ys={-.5f,-.43f,.43f,.5f};float[] xs=floor?new[]{.495f,.5f,.5f,.495f}:new[]{.43f,.5f,.5f,.43f};
            Vector3[][] levels=new Vector3[4][];
            for(int j=0;j<4;j++){float a=xs[j],b=a*(floor?.985f:.77f);levels[j]=new[]{new Vector3(-b,ys[j],-a),new Vector3(b,ys[j],-a),new Vector3(a,ys[j],-b),new Vector3(a,ys[j],b),new Vector3(b,ys[j],a),new Vector3(-b,ys[j],a),new Vector3(-a,ys[j],b),new Vector3(-a,ys[j],-b)};}
            for(int j=0;j<3;j++)for(int i=0;i<8;i++){int k=(i+1)%8;g.Quad(levels[j][i],levels[j][k],levels[j+1][k],levels[j+1][i],(levels[j][i]+levels[j][k])*.5f-Vector3.up*ys[j]);}
            for(int i=0;i<8;i++){g.Tri(Vector3.up*.5f,levels[3][i],levels[3][(i+1)%8],Vector3.up);g.Tri(Vector3.down*.5f,levels[0][(i+1)%8],levels[0][i],Vector3.down);}
            return SaveMesh(g.Mesh(),floor?"DeckPanelUnitBox":"BeveledUnitBox");
        }
        static GameObject MeshObject(string name,Mesh mesh,Material mat,Transform parent,Vector3 pos,bool collision=true)
        {
            var o=new GameObject(name);o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;
            if(collision)o.AddComponent<MeshCollider>().sharedMesh=mesh;return o;
        }
        static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 scale,Material mat,Quaternion? rotation=null,bool collision=true)
        {bool floor=name=="Deck panel"||name.StartsWith("Ramp panel")||name=="Loose ivory floor panel";var o=MeshObject(name,floor&&deckMesh?deckMesh:boxMesh,mat,parent,pos,false);o.transform.localScale=scale;if(rotation.HasValue)o.transform.localRotation=rotation.Value;if(collision)o.AddComponent<BoxCollider>();return o;}
        static GameObject Annulus(string name,Transform p,Vector3 pos,float ri,float ro,float yi,float yo,float thick,Material m,float a=0,float b=360,int n=64,bool collision=true)
        {return MeshObject(name,Ring(ri,ro,yi,yo,thick,a,b,n),m,p,pos,collision);}
        static GameObject Cylinder(string name,Transform parent,Vector3 pos,float radius,float height,Material mat,Quaternion? rotation=null,bool collision=true)
        {var o=Annulus(name,parent,pos,0,radius,height*.5f,height*.5f,height,mat,0,360,24,collision);if(rotation.HasValue)o.transform.localRotation=rotation.Value;return o;}
        static GameObject Bar(string name,Transform p,Vector3 a,Vector3 b,float width,Material mat,bool collision=false)
        {return Box(name,p,(a+b)*.5f,new Vector3(width,(b-a).magnitude,width),mat,Quaternion.FromToRotation(Vector3.up,b-a),collision);}
        static void Label(string text,Transform p,Vector3 pos,float size,Color color,Quaternion rotation)
        {var o=new GameObject(text);o.transform.SetParent(p,false);o.transform.localPosition=pos;o.transform.localRotation=rotation;var t=o.AddComponent<TextMesh>();t.text=text;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.characterSize=size*.24f;t.fontSize=64;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.color=color;o.GetComponent<MeshRenderer>().sharedMaterial=textMaterial;o.AddComponent<BiogearFontAtlas>();}
        static Material Material(string name,Color color,float smooth=.35f,bool emissive=false)
        {
            if(materialCache.TryGetValue(name,out var cached))return cached;
            string path=Base+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",0.08f);m.enableInstancing=true;
            if(emissive){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*2.2f);}
            EditorUtility.SetDirty(m);materialCache[name]=m;return m;
        }
        static void Textured(Material mat,string name,Color a,Color b,float scale)
        {
            string path=Base+"/Textures/"+name+".asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(!texture)
            {
                texture=new Texture2D(256,256,TextureFormat.RGBA32,true);texture.name=name;texture.wrapMode=TextureWrapMode.Repeat;
                var pixels=new Color[256*256];for(int y=0;y<256;y++)for(int x=0;x<256;x++)
                {float n=Mathf.PerlinNoise(x*.028f,y*.028f)*.75f+Mathf.PerlinNoise(x*.27f,y*.27f)*.25f;pixels[y*256+x]=Color.Lerp(a,b,n);}
                texture.SetPixels(pixels);texture.Apply();AssetDatabase.CreateAsset(texture,path);
            }
            mat.SetColor("_BaseColor",Color.white);mat.SetTexture("_BaseMap",texture);mat.SetTextureScale("_BaseMap",Vector2.one*scale);EditorUtility.SetDirty(mat);
        }
        static void Materials()
        {
            ivory=Material("Ivory Paint",new Color(.73f,.84f,.84f));dark=Material("Structural Graphite",new Color(.075f,.13f,.17f));steel=Material("Steel Blue",new Color(.26f,.38f,.46f));orange=Material("Orange Paint",new Color(.92f,.43f,.07f));yellow=Material("Safety Lime",new Color(.70f,.78f,.14f));cyan=Material("Cyan Emissive",new Color(.1f,.85f,1),.5f,true);grass=Material("Grass",new Color(.4f,.64f,.12f),.12f);rock=Material("Rock",new Color(.38f,.42f,.33f),.15f);blue=Material("Dome Panels",new Color(.45f,.69f,.76f));glass=Material("Aquarium",new Color(.015f,.57f,.64f),.8f,true);
            Textured(ivory,"PaintVariation",new Color(.7f,.78f,.78f),new Color(.86f,.91f,.88f),1);
            Textured(rock,"StoneVariationSubtle",new Color(.40f,.43f,.36f),new Color(.51f,.52f,.43f),.30f);
            Textured(grass,"GrassVariation",new Color(.23f,.43f,.055f),new Color(.54f,.7f,.19f),2);
            beam=Material("Energy Beam",new Color(.08f,.7f,1,.14f),.5f,true);
            beam.SetFloat("_Surface",1);beam.SetFloat("_SrcBlend",(int)BlendMode.SrcAlpha);beam.SetFloat("_DstBlend",(int)BlendMode.One);beam.SetFloat("_ZWrite",0);beam.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");beam.renderQueue=3000;
            string path=Base+"/Materials/Ocean.mat";water=AssetDatabase.LoadAssetAtPath<Material>(path);if(!water){water=new Material(Shader.Find("BiogearLab/Stylized Ocean"));AssetDatabase.CreateAsset(water,path);}else water.shader=Shader.Find("BiogearLab/Stylized Ocean");
            textMaterial=AssetDatabase.LoadAssetAtPath<Material>(Base+"/Materials/Signage.mat");if(!textMaterial){textMaterial=new Material(Shader.Find("BiogearLab/Depth Tested Text"));AssetDatabase.CreateAsset(textMaterial,Base+"/Materials/Signage.mat");}
        }
        static void Rail(Transform p,float radius,float y,float start,float end)
        {
            int n=Mathf.CeilToInt((end-start)/12);for(int i=0;i<n;i++){float a=Mathf.Lerp(start,end,i/(float)n),b=Mathf.Lerp(start,end,(i+1f)/n);Bar("Orange handrail",p,P(radius,a,y+1.15f),P(radius,b,y+1.15f),.09f,orange);Bar("Orange lower rail",p,P(radius,a,y+.65f),P(radius,b,y+.65f),.07f,orange);Bar("Rail post",p,P(radius,a,y),P(radius,a,y+1.15f),.07f,orange);}
        }
        static void Hazard(Transform p,Vector3 pos,float width,Quaternion rotation)
        {
            var strip=Group("Hazard trim",p);strip.localPosition=pos;strip.localRotation=rotation;
            Box("Black stripe base",strip,Vector3.zero,new Vector3(width,.18f,.16f),dark);
            for(float x=-width/2+.2f;x<width/2;x+=.45f)Box("Yellow marking",strip,new Vector3(x,0,-.095f),new Vector3(.22f,.18f,.025f),yellow,Quaternion.Euler(0,0,-22),false);
        }
        static void Objective(Transform p,Vector3 position,bool energy=true)
        {
            var g=Group("Objective pedestal",p);g.localPosition=position;
            Cylinder("Pedestal",g,Vector3.zero,2.5f,.35f,dark);Annulus("Cyan objective ring",g,Vector3.up*.2f,1.9f,2.05f,0,0,.035f,cyan);
            Cylinder("Inset core",g,Vector3.up*.23f,.8f,.09f,steel);
            for(int i=0;i<8;i++)Box("Pedestal inset",g,P(2.2f,i*45,.21f),new Vector3(.36f,.05f,.22f),orange,Quaternion.Euler(0,-i*45,0),false);
            if(energy){Cylinder("Energy shaft glow",g,Vector3.up*45,.16f,90,beam,null,false);Cylinder("Energy core",g,Vector3.up*45,.035f,90,cyan,null,false);}
        }
        static void Arena()
        {
            var g=Group("02_CentralArena",root);float r=cfg.arenaRadius;
            var ringCollision=new GameObject("Continuous main ring collision");ringCollision.transform.SetParent(g,false);ringCollision.AddComponent<MeshCollider>().sharedMesh=Ring(r-4.8f,r,4.05f,4.05f,.65f);
            for(int i=0;i<24;i++)Annulus("Lower circulation deck",g,Vector3.zero,3.9f,(i>=10&&i<=13)?18:27,-.85f,-.85f,.65f,steel,i*15+.1f,(i+1)*15-.1f,4);
            for(int i=0;i<32;i++)
            {
                float a=i*11.25f+.16f,b=(i+1)*11.25f-.16f;
                Annulus("Ivory ring deck",g,Vector3.zero,r-4.8f,r,4.1f,4.1f,.65f,ivory,a,b,3);
                Annulus("Orange inner edge",g,Vector3.zero,r-4.92f,r-4.74f,4.16f,4.16f,.06f,orange,a,b,3,false);
                Annulus("Graphite fascia",g,Vector3.zero,r-.2f,r+.12f,3.7f,3.7f,1,dark,a,b,3);
                Annulus("Cyan perimeter",g,Vector3.zero,r+.12f,r+.2f,3.46f,3.46f,.08f,cyan,a,b,3,false);
                if(i%8>1)Annulus("Lower structural wall",g,Vector3.zero,r-.9f,r-.3f,2.8f,2.8f,3.7f,steel,a,b,3);
            }
            for(int i=0;i<12;i++)
            {
                float a=i*30+1,b=(i+1)*30-1;
                if(i!=2 && i!=6 && i!=10)Annulus("Bowl sloping sector",g,Vector3.zero,3.4f,12.1f,.1f,4.05f,.35f,i==8?orange:steel,a,b,8);
                Bar("Bowl radial seam",g,P(3.5f,a,.17f),P(11.95f,a,4.12f),.12f,dark);
                if(i%2==0)Pod(g,P(9.9f,i*30+15,3.55f),i*30+15);
            }
            Cylinder("Bowl lower floor",g,new Vector3(0,-.05f,0),4,.25f,steel);Objective(g,new Vector3(0,.2f,0));
            for(int i=0;i<12;i++)
            {
                float a=i*30+10;Cover(g,P(15.1f,a,4.15f),-a+90,i%4==0);
                if(i%3==0)Crate(g,P(18.2f,a,-.5f));
                if(i%3==1)TubeRack(g,P(14.9f,a+13,4.2f),-a);
            }
            for(int i=0;i<5;i++)
            {
                float a=new[]{-90,-25,36,145,211}[i];var near=P(16.3f,a,4);var far=P(29,a,.15f);
                Walkway(g,near,far,4.2f,"Radial ramp");
                var gate=Group("Gate "+(i+1),g);gate.localPosition=P(19.5f,a,.1f);gate.localRotation=Quaternion.Euler(0,90-a,0);
                for(int side=-1;side<=1;side+=2){Box("Gate orange panel",gate,new Vector3(side*3.1f,1.7f,0),new Vector3(1.8f,3.4f,1.1f),dark);Box("Gate number plate",gate,new Vector3(side*3.1f,2,-.62f),new Vector3(1.5f,1.6f,.1f),orange);Label((i+1).ToString("D2"),gate,new Vector3(side*3.1f,2,-.7f),.31f,Color.white,Quaternion.identity);}
            }
            for(int i=0;i<24;i++)
            {
                float a=i*15,b=a+14.8f;if(i>=10 && i<=13)continue;
                Annulus("Outer walkway",g,Vector3.zero,27,32,.05f,.05f,.65f,ivory,a,b,4);
                Annulus("Outer orange trim",g,Vector3.zero,31.85f,32.08f,.1f,.1f,.1f,orange,a,b,4,false);
                Annulus("Outer retaining edge",g,Vector3.zero,32,32.4f,-.18f,-.18f,1.2f,dark,a,b,4);
                if(i%3==0)Cover(g,P(30,a+5,.14f),-a,false);
            }
        }
        static void Walkway(Transform p,Vector3 a,Vector3 b,float width,string name)
        {
            var group=Group(name,p);var offset=(a+b)*.5f;group.localPosition=offset;group.localRotation=Quaternion.FromToRotation(Vector3.forward,(b-a).normalized);
            float len=(b-a).magnitude;Box("Walkway structure",group,new Vector3(0,-.35f,0),new Vector3(width,.65f,len),dark);
            int parts=Mathf.CeilToInt(len/3);
            for(int i=0;i<parts;i++)Box("Deck panel",group,new Vector3(0,0,(i+.5f)*len/parts-len/2),new Vector3(width-.2f,.17f,len/parts-.045f),ivory);
            foreach(int side in new[]{-1,1})Box("Yellow edge",group,new Vector3(side*(width/2-.1f),.105f,0),new Vector3(.12f,.045f,len),orange);
        }
        static void Pod(Transform p,Vector3 pos,float angle)
        {
            var g=Group("Reactor pod",p);g.localPosition=pos;g.localRotation=Quaternion.Euler(0,-angle,0)*Quaternion.Euler(0,0,-20);
            Cylinder("Pod base",g,new Vector3(0,.05f,0),1.05f,.3f,dark);
            Cylinder("Pod yellow belt",g,new Vector3(0,.5f,0),.9f,.4f,yellow);
            Cylinder("White barrel",g,new Vector3(0,1.3f,0),.87f,1.4f,ivory);
            Cylinder("Pod cap",g,new Vector3(0,2.05f,0),.72f,.22f,dark);
            Annulus("Pod cap light",g,new Vector3(0,2.18f,0),.35f,.5f,0,0,.04f,cyan,0,360,20,false);
            Box("Pod inset",g,new Vector3(0,1.15f,-.87f),new Vector3(.32f,.82f,.14f),steel);
        }
        static void Cover(Transform p,Vector3 pos,float yaw,bool orangeDouble)
        {
            var g=Group(orangeDouble?"Orange double cover":"Ivory cover",p);g.localPosition=pos;g.localRotation=Quaternion.Euler(0,yaw,0);
            if(orangeDouble)
            {
                Box("Cover base",g,new Vector3(0,.2f,0),new Vector3(4.1f,.4f,1.3f),dark);
                for(int i=-1;i<=1;i+=2){Box("Orange shield",g,new Vector3(i*.95f,1.5f,0),new Vector3(1.75f,2.8f,.6f),orange);Box("Shield rib",g,new Vector3(i*1.7f,1.4f,-.33f),new Vector3(.18f,2.7f,.15f),steel);Box("Status lamp",g,new Vector3(i*.95f,2.5f,-.33f),new Vector3(.1f,.25f,.08f),cyan,null,false);}
            }
            else
            {
                Box("Cover foot",g,new Vector3(0,.13f,0),new Vector3(2.2f,.25f,1.4f),dark);
                var shield=Group("Shield",g);shield.localRotation=Quaternion.Euler(-7,0,0);
                Box("Ivory shield",shield,new Vector3(0,1.5f,0),new Vector3(1.9f,2.8f,.55f),ivory);
                Box("Inset graphite panel",shield,new Vector3(0,1.6f,-.3f),new Vector3(1.4f,1.85f,.08f),steel);
                Box("Small cyan lamp",shield,new Vector3(0,.7f,-.36f),new Vector3(.46f,.1f,.05f),cyan,null,false);
            }
        }
        static void Crate(Transform p,Vector3 pos)
        {
            var g=Group("Cargo crate",p);g.localPosition=pos;Box("Crate body",g,new Vector3(0,1.2f,0),new Vector3(2.3f,2.4f,2.3f),steel);
            for(int s=-1;s<=1;s+=2){Bar("Crate diagonal",g,new Vector3(-1,.2f,s*1.2f),new Vector3(1,2.2f,s*1.2f),.14f,ivory);Box("Crate cyan lock",g,new Vector3(0,2.05f,s*1.2f),new Vector3(.5f,.1f,.06f),cyan,null,false);}
        }
        static void TubeRack(Transform p,Vector3 pos,float yaw)
        {
            var g=Group("Tube rack",p);g.localPosition=pos;g.localRotation=Quaternion.Euler(0,yaw,0);
            Box("Rack base",g,new Vector3(0,.15f,0),new Vector3(2.3f,.3f,1.6f),dark);
            for(int x=0;x<2;x++)for(int y=0;y<2;y++){var pos1=new Vector3((x-.5f)*.95f,.7f+y*.95f,0);Cylinder("Tube",g,pos1,.44f,1.1f,ivory,Quaternion.Euler(90,0,0));Cylinder("Tube dark end",g,pos1+Vector3.back*.6f,.34f,.08f,dark,Quaternion.Euler(90,0,0));Cylinder("Yellow tube core",g,pos1+Vector3.back*.66f,.16f,.04f,yellow,Quaternion.Euler(90,0,0));}
        }
        static Mesh ExtrudedPolygon(Vector2[] points,float depth,string name)
        {
            var g=new Geometry();var center=Vector2.zero;foreach(var v in points)center+=v;center/=points.Length;
            for(int i=0;i<points.Length;i++)
            {
                var a=points[i];var b=points[(i+1)%points.Length];
                g.Tri(new Vector3(center.x,center.y,-depth/2),new Vector3(a.x,a.y,-depth/2),new Vector3(b.x,b.y,-depth/2),Vector3.back);
                g.Tri(new Vector3(center.x,center.y,depth/2),new Vector3(a.x,a.y,depth/2),new Vector3(b.x,b.y,depth/2),Vector3.forward);
                var normal=new Vector3((a.y-b.y),b.x-a.x,0);var edgeMid=(a+b)*.5f-center;if(Vector2.Dot(new Vector2(normal.x,normal.y),edgeMid)<0)normal=-normal;
                g.Quad(new Vector3(a.x,a.y,-depth/2),new Vector3(a.x,a.y,depth/2),new Vector3(b.x,b.y,depth/2),new Vector3(b.x,b.y,-depth/2),normal);
            }
            return SaveMesh(g.Mesh(),name);
        }
        static void UpperDisk()
        {
            var g=Group("03_UpperDiskAndTwinBridge",root);g.localPosition=new Vector3(0,14,0);
            Annulus("Beveled ivory hull",g,Vector3.zero,1.8f,9.4f,0,-.15f,1.05f,ivory);
            Annulus("Graphite flight deck",g,Vector3.zero,1.8f,7.55f,.15f,.15f,.16f,dark);
            Annulus("Thin yellow deck edge",g,Vector3.zero,7.55f,7.7f,.16f,.16f,.025f,yellow);
            Annulus("Orange hull pinstripe",g,Vector3.down*.83f,8.8f,9.43f,0,0,.08f,orange);
            Annulus("Dark underside",g,Vector3.down*1.09f,1.8f,8.65f,0,0,.18f,dark);
            Annulus("Cyan underside ring",g,Vector3.down*1.28f,3.5f,3.68f,0,0,.045f,cyan,0,360,64,false);
            for(int i=0;i<3;i++)
            {
                float a=-90+i*120;var fin=Group("Inclined stabilizer fin",g);fin.localPosition=P(8.3f,a,.15f);fin.localRotation=Quaternion.Euler(0,-a+90,0);
                var mesh=ExtrudedPolygon(new[]{new Vector2(-1.3f,0),new Vector2(1.3f,0),new Vector2(1,3.6f),new Vector2(.2f,4.1f),new Vector2(-.7f,3.8f)},.55f,"DiskFin"+i);
                MeshObject("Ivory fin shell",mesh,ivory,fin,Vector3.zero);Box("Orange fin inset",fin,new Vector3(0,1.2f,-.32f),new Vector3(.9f,1.4f,.06f),orange);
                for(int side=-1;side<=1;side+=2)Box("Fin cyan status",fin,new Vector3(side*.4f,2.5f,-.35f),new Vector3(.06f,.42f,.05f),cyan,null,false);
            }
            Rail(g,8.8f,.05f,-160,40);Rail(g,8.8f,.05f,130,190);
            for(int side=-1;side<=1;side+=2)
            {
                Walkway(g,new Vector3(side*4.8f,0,7.2f),new Vector3(side*3.8f,4.2f,31),2.7f,"Twin bridge "+side);
                Bar("Bridge orange rail",g,new Vector3(side*6,1,7.5f),new Vector3(side*5,5.3f,30.5f),.09f,orange);
                Box("Bridge support column",g,new Vector3(side*4.3f,-2,24),new Vector3(1.5f,12,1.7f),steel);
                Box("Column orange belt",g,new Vector3(side*4.3f,-1,24),new Vector3(1.65f,.5f,1.8f),orange);
            }
            Label("EVERLIFE",g,new Vector3(0,-.45f,-9.42f),.65f,new Color(.04f,.62f,.61f),Quaternion.identity);
            for(int i=0;i<6;i++)Box("Underside rim light",g,P(7.8f,i*60,-1.25f),new Vector3(1.8f,.08f,.5f),cyan,Quaternion.Euler(0,-i*60,0),false);
        }
        static void Tower()
        {
            var g=Group("04_TowerComplex",root);g.localPosition=cfg.towerPosition;
            foreach(int side in new[]{-1,1}){Box("Foundation side",g,new Vector3(side*5.6f,2.5f,2),new Vector3(4,5,11),ivory);Box("Gate01 jamb",g,new Vector3(side*3.4f,3.5f,-3.7f),new Vector3(.7f,7,.8f),dark);}
            Walkway(g,new Vector3(0,-1.5f,-3),new Vector3(0,0,2),5,"Gate01 interior ramp");
            Box("Gate01 orange lintel",g,new Vector3(0,8,-4),new Vector3(7,1.7f,.5f),orange);
            Label("GATE 01",g,new Vector3(0,8,-4.3f),.36f,Color.white,Quaternion.identity);
            var reactor=Group("Tilted circular reactor",g);reactor.localPosition=new Vector3(0,14,1);reactor.localRotation=Quaternion.Euler(-72,0,0);
            Cylinder("Reactor housing",reactor,Vector3.zero,7.4f,1.7f,ivory);
            Annulus("Orange reactor frame",reactor,Vector3.up*.92f,5.5f,7.1f,0,0,.5f,orange);
            Annulus("Graphite reactor collar",reactor,Vector3.up*1.18f,3.7f,5.5f,0,0,.6f,dark);
            Annulus("Luminous reactor ring",reactor,Vector3.up*1.52f,3.65f,4.05f,0,0,.13f,cyan);
            Cylinder("Reactor orange center",reactor,Vector3.up*1.45f,3.4f,.55f,orange);
            for(int i=0;i<8;i++){float a=i*45;Box("Reactor luminous tick",reactor,P(3.5f,a,1.8f),new Vector3(.28f,.06f,.85f),cyan,Quaternion.Euler(0,90-a,0),false);Box("Outer reactor clamp",reactor,P(6.8f,a,1.4f),new Vector3(1.1f,.7f,2.1f),dark,Quaternion.Euler(0,90-a,0));}
            var bladeMesh=ExtrudedPolygon(new[]{new Vector2(-2.2f,0),new Vector2(2.2f,0),new Vector2(1.5f,5),new Vector2(.7f,13),new Vector2(.15f,39),new Vector2(-.55f,40),new Vector2(-1.25f,38),new Vector2(-1.5f,12),new Vector2(-2.8f,3)},1.1f,"TowerBlade");
            for(int i=0;i<3;i++)
            {
                var blade=Group("Tower blade "+(i+1),g);blade.localPosition=P(4.1f,i*120+30,14);blade.localRotation=Quaternion.Euler(0,-i*120,0);
                MeshObject("Tapered ivory blade",bladeMesh,ivory,blade,Vector3.zero);
                Box("Tall orange inset",blade,new Vector3(-.35f,31,-.59f),new Vector3(.62f,12,.09f),orange);
                Box("Blade mount",blade,new Vector3(-.2f,.6f,0),new Vector3(4.8f,2.5f,2.2f),steel);
                Bar("Dark inner rib",blade,new Vector3(.8f,4,.6f),new Vector3(.1f,37,.6f),.55f,dark);
                for(int k=0;k<8;k++)Box("Inner service light",blade,new Vector3(.45f,8+k*3.7f,.96f),new Vector3(.14f,.2f,.05f),cyan,null,false);
            }
            Annulus("Blade crosspiece",g,new Vector3(0,27,1),2.1f,4.5f,0,0,.65f,orange);
            Cylinder("Suspended cyan lamp",g,new Vector3(0,26.2f,1),2.4f,.13f,cyan,null,false);
        }
        static Vector3 DomePoint(float radius,float height,float latitude,float longitude)
        {float t=latitude*Mathf.Deg2Rad;return P(radius*Mathf.Cos(t),longitude,height*Mathf.Sin(t));}
        static void Dome()
        {
            var g=Group("05_DomeLab",root);g.localPosition=cfg.domePosition;g.localRotation=Quaternion.Euler(0,-90,0);
            Annulus("Dome foundation ring",g,Vector3.zero,10.8f,12.6f,0,0,.8f,dark);
            Cylinder("Dome interior floor",g,Vector3.zero,11.7f,.25f,ivory);
            Annulus("Inner floor inset",g,Vector3.up*.15f,3.5f,8,0,0,.06f,steel);
            Annulus("Inner cyan floor perimeter",g,Vector3.up*.2f,7.6f,7.8f,0,0,.05f,cyan);
            Objective(g,new Vector3(0,.3f,0));
            float[] lat={0,17,34,51,67,75};
            for(int k=0;k<lat.Length-1;k++)for(int j=0;j<24;j++)
            {
                float a=j*15,b=(j+1)*15;if(((j>=16 && j<=19)||j==10||j==11) && k<2)continue;
                var p=DomePoint(12,10,lat[k],a);var q=DomePoint(12,10,lat[k],b);var r=DomePoint(12,10,lat[k+1],b);var s=DomePoint(12,10,lat[k+1],a);
                var geo=new Geometry();var normal=(p+q+r+s).normalized;geo.Quad(p,q,r,s,normal);geo.Quad(s,r,q,p,-normal);
                var mat=j>=10 && j<=17?yellow:j%6<3?ivory:blue;
                MeshObject("Triangular dome panel",SaveMesh(geo.Mesh(),"DomePanel_"+k+"_"+j),k==4?steel:mat,g,Vector3.zero);
                Bar("Meridian structural rib",g,p,s,.19f,dark);Bar("Diagonal brace",g,p,r,.095f,dark);Bar("Latitude rib",g,p,q,.12f,dark);
            }
            Annulus("Open oculus ring",g,new Vector3(0,9.66f,0),2.9f,3.35f,0,0,.27f,dark);
            Vector3[] portal={new Vector3(-4.5f,0,-11.45f),new Vector3(-4.5f,3.5f,-11.45f),new Vector3(-3,5.9f,-11.45f),new Vector3(3,5.9f,-11.45f),new Vector3(4.5f,3.5f,-11.45f),new Vector3(4.5f,0,-11.45f)};
            for(int i=0;i<portal.Length-1;i++)Bar("Dome entrance frame",g,portal[i],portal[i+1],.6f,dark,true);
            Hazard(g,new Vector3(0,5.6f,-11.85f),6.5f,Quaternion.identity);
            // Curved aquarium wall and submerged vegetation are separate meshes.
            Annulus("Curved aquarium window",g,Vector3.zero,10.95f,11.15f,3.8f,3.8f,2.5f,glass,15,145,30,false);
            Annulus("Aquarium top frame",g,Vector3.zero,10.8f,11.35f,4.05f,4.05f,.25f,dark,15,145,30);
            Annulus("Aquarium bottom frame",g,Vector3.zero,10.8f,11.35f,1.3f,1.3f,.25f,dark,15,145,30);
            Annulus("Yellow interior base stripe",g,Vector3.zero,11.1f,11.25f,.65f,.65f,.16f,yellow,0,245,50);
            for(int i=0;i<24;i++)
            {
                float a=i*15+.18f,b=(i+1)*15-.18f;
                if(i>=16&&i<=19||i==10||i==11)continue;
                if(i<1||i>9)Annulus("Ivory interior wall",g,Vector3.zero,11.3f,11.6f,4.1f,4.1f,4.1f,ivory,a,b,3);
                Annulus("Interior truss band",g,Vector3.zero,11.2f,11.6f,4.85f,4.85f,.55f,dark,a,b,3);
                Bar("Interior triangle truss",g,P(11.14f,a,4.3f),P(11.14f,(a+b)/2,4.8f),.09f,steel);
                Bar("Interior triangle truss",g,P(11.14f,(a+b)/2,4.8f),P(11.14f,b,4.3f),.09f,steel);
            }
            for(int i=0;i<18;i++){float a=20+i*6;var p=P(10.8f,a,1.5f);Bar("Aquarium plant",g,p,p+new Vector3(.1f,1.5f+Mathf.Sin(i)*.6f,.2f),.045f,grass);}
            Box("Console base",g,P(9,82,.6f),new Vector3(2.5f,1.2f,1.5f),ivory);
            Box("Console screen",g,P(9,82,1.4f),new Vector3(1.4f,.7f,.12f),cyan,Quaternion.Euler(10,0,0),false);
            var gate03=Group("Gate03 interior portal",g);gate03.localPosition=P(11,165);gate03.localRotation=Quaternion.Euler(0,-75,0);
            foreach(int side in new[]{-1,1})Box("Gate03 jamb",gate03,new Vector3(side*1.8f,2.1f,0),new Vector3(.5f,4.2f,.5f),steel);
            Box("Gate03 lintel",gate03,new Vector3(0,4.3f,0),new Vector3(4.1f,.6f,.6f),dark);
            Label("GATE 03",gate03,new Vector3(0,4.4f,-.34f),.25f,Color.white,Quaternion.identity);
            Hazard(gate03,new Vector3(0,3.95f,-.34f),3.1f,Quaternion.identity);
            Walkway(g,P(10.8f,165,.05f),P(16,165,.05f),3.2f,"Gate03 exterior deck");
            for(int i=0;i<3;i++){var l=new GameObject("Aquarium cyan bounce").AddComponent<Light>();l.transform.SetParent(g,false);l.transform.localPosition=P(7,i*120,3.5f);l.type=LightType.Point;l.range=14;l.intensity=5;l.color=new Color(.15f,.65f,.8f);l.shadows=LightShadows.None;}
            var frames=Group("08_WaterPlatforms",root);
            for(int i=0;i<5;i++)
            {
                float z=(i-2)*4.2f;var platform=Group("Water platform "+(i+1),frames);platform.localPosition=new Vector3(-23.7f+Mathf.Abs(i-2)*1.5f,-2.5f,z);platform.localRotation=Quaternion.Euler(0,(i-2)*-10,0);
                Box("Copper float body",platform,Vector3.down*.35f,new Vector3(4.1f,.9f,3.8f),orange);Box("Dark rim",platform,new Vector3(0,.08f,0),new Vector3(4.25f,.25f,3.95f),dark);Box("Metal deck",platform,new Vector3(0,.23f,0),new Vector3(3.9f,.17f,3.6f),steel);
                foreach(int side in new[]{-1,1})
                {Box("White deck marking",platform,new Vector3(side*1.72f,.33f,0),new Vector3(.055f,.015f,2.9f),ivory,null,false);Box("White deck marking",platform,new Vector3(0,.33f,side*1.5f),new Vector3(3.4f,.015f,.055f),ivory,null,false);Box("Float strap",platform,new Vector3(side*1.2f,-.25f,0),new Vector3(.13f,1.1f,3.95f),dark);}
                Box("Four-lobe cross",platform,new Vector3(0,.34f,0),new Vector3(1.6f,.025f,.52f),dark,Quaternion.Euler(0,45,0),false);Box("Four-lobe cross",platform,new Vector3(0,.34f,0),new Vector3(1.6f,.025f,.52f),dark,Quaternion.Euler(0,-45,0),false);
            }
            Walkway(frames,new Vector3(-22,-2.15f,-10),new Vector3(-17,3.8f,-8),3,"South water access");Walkway(frames,new Vector3(-22,-2.15f,10),new Vector3(-17,3.8f,8),3,"North water access");
            Walkway(frames,new Vector3(-21.9f,-2.15f,0),new Vector3(-26,-.95f,0),3,"Dome entrance ramp");
        }
        static void Corridor()
        {
            var g=Group("06_CorridorAndLowerRoutes",root);g.localPosition=new Vector3(0,-.3f,17);
            for(int j=0;j<5;j++)
            {
                float z=j*3.4f,y=j*.75f;
                Box("Ramp panel left",g,new Vector3(-1.65f,y,z+1.7f),new Vector3(3.1f,.22f,3.5f),ivory,Quaternion.Euler(-12.45f,0,0));Box("Ramp panel right",g,new Vector3(1.65f,y,z+1.7f),new Vector3(3.1f,.22f,3.5f),ivory,Quaternion.Euler(-12.45f,0,0));Box("Center floor channel",g,new Vector3(0,y+.02f,z+1.7f),new Vector3(.22f,.1f,3.5f),dark,Quaternion.Euler(-12.45f,0,0));
                Vector3[] cross={new Vector3(-3.6f,0,z),new Vector3(-3.6f,3.2f,z),new Vector3(-2.2f,5,z),new Vector3(2.2f,5,z),new Vector3(3.6f,3.2f,z),new Vector3(3.6f,0,z)};
                for(int i=0;i<cross.Length;i++)cross[i]+=Vector3.up*y;
                for(int i=0;i<cross.Length-1;i++)
                {
                    Bar("Angular portal rib",g,cross[i],cross[i+1],.25f,dark,true);
                    var geo=new Geometry();var offset=new Vector3(0,.75f,3.4f);geo.Quad(cross[i],cross[i+1],cross[i+1]+offset,cross[i]+offset,-(cross[i]+cross[i+1])*.5f+new Vector3(0,y+2,z));
                    MeshObject("Corridor wall panel",SaveMesh(geo.Mesh(),"CorridorWall_"+j+"_"+i),steel,g,Vector3.zero).GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.TwoSided;
                }
                foreach(int side in new[]{-1,1})
                {
                    Bar("Yellow foot stripe",g,new Vector3(side*3.45f,y+.6f,z),new Vector3(side*3.45f,y+1.35f,z+3.4f),.13f,yellow);
                    Bar("Ceiling cyan strip",g,new Vector3(side*1.55f,y+4.82f,z+.15f),new Vector3(side*1.55f,y+5.5f,z+3.25f),.065f,cyan);
                    Box("Slanted cyan wall fixture",g,new Vector3(side*3.24f,y+2.6f,z+1.5f),new Vector3(.13f,1.2f,1.5f),cyan,Quaternion.Euler(0,0,side*15),false);
                }
                var l=new GameObject("Cyan corridor fill").AddComponent<Light>();l.transform.SetParent(g,false);l.transform.localPosition=new Vector3(0,y+3.5f,z+1);l.type=LightType.Point;l.color=new Color(.07f,.7f,1);l.intensity=3;l.range=6;l.shadows=LightShadows.None;
            }
        }
        static void RockChunk(Transform parent,Vector3 pos,Vector3 size,int index,bool mountain=false)
        {
            var geo=new Geometry();var cap=new Geometry();int n=9;Vector3[][] rings=new Vector3[4][];
            float twist=UnityEngine.Random.Range(0,360);float[] height={0,.3f,.67f,1};float[] radii=mountain?new[]{1f,.78f,.4f,.025f}:new[]{.85f,1,.88f,.65f};
            for(int k=0;k<4;k++)
            {
                rings[k]=new Vector3[n];for(int j=0;j<n;j++){float jitter=UnityEngine.Random.Range(.82f,1.17f);var v=P(radii[k]*jitter,j*360f/n+twist+k*5,height[k]+Mathf.Sin(j*1.7f+index)*.045f*k/3);rings[k][j]=Vector3.Scale(v,size);if(mountain)rings[k][j]+=new Vector3(Mathf.Sin(index*1.7f)*size.x*.75f,0,Mathf.Cos(index*2.1f)*size.z*.4f)*Mathf.Pow(k/3f,1.5f);}
            }
            for(int k=0;k<3;k++)for(int j=0;j<n;j++)
            {int b=(j+1)%n;geo.Quad(rings[k][j],rings[k][b],rings[k+1][b],rings[k+1][j],(rings[k][j]+rings[k][b]).normalized);}
            var top=new Vector3(0,size.y*1.03f,0);
            for(int j=0;j<n;j++)cap.Tri(top,rings[3][j]+Vector3.up*.05f,rings[3][(j+1)%n]+Vector3.up*.05f,Vector3.up);
            var mat=mountain?Material("Distant Mountain Blue",new Color(.16f,.30f,.39f),.1f):rock;
            MeshObject("Faceted stone "+index,SaveMesh(geo.Mesh(),"Rock_"+index),mat,parent,pos);
            MeshObject("Grass cap "+index,SaveMesh(cap.Mesh(),"GrassCap_"+index),mountain?mat:grass,parent,pos);
        }
        static void RockArch(Transform parent,Vector3 pos,float yaw,int index)
        {
            var group=Group("Natural rock arch "+index,parent);group.localPosition=pos;group.localRotation=Quaternion.Euler(0,yaw,0);
            var geo=new Geometry();var cap=new Geometry();int steps=16,corners=8;
            Vector3[][] sections=new Vector3[steps+1][];
            for(int i=0;i<=steps;i++)
            {
                float t=Mathf.PI*i/steps;var center=new Vector3(Mathf.Cos(t)*8,Mathf.Sin(t)*9,0);var normal=new Vector3(Mathf.Cos(t),Mathf.Sin(t),0);sections[i]=new Vector3[corners];
                for(int j=0;j<corners;j++){float a=j*Mathf.PI*2/corners;float r=2.15f+UnityEngine.Random.Range(-.24f,.24f);sections[i][j]=center+normal*(Mathf.Cos(a)*r)+Vector3.forward*(Mathf.Sin(a)*r*1.1f);}
            }
            for(int i=0;i<steps;i++)for(int j=0;j<corners;j++)
            {
                int next=(j+1)%corners;Vector3 normal=(sections[i][j]+sections[i][next])*.5f-new Vector3(Mathf.Cos(Mathf.PI*i/steps)*8,Mathf.Sin(Mathf.PI*i/steps)*9,0);
                geo.Quad(sections[i][j],sections[i+1][j],sections[i+1][next],sections[i][next],normal);
                if(j==0||j==7)cap.Quad(sections[i][j]+Vector3.up*.08f,sections[i+1][j]+Vector3.up*.08f,sections[i+1][next]+Vector3.up*.08f,sections[i][next]+Vector3.up*.08f,Vector3.up);
            }
            MeshObject("Arch stone",SaveMesh(geo.Mesh(),"RockArch_"+index),rock,group,Vector3.zero);
            MeshObject("Arch grass",SaveMesh(cap.Mesh(),"ArchGrass_"+index),grass,group,Vector3.zero,false);
        }
        static void Palm(Transform p,Vector3 pos,int index)
        {
            var g=Group("Palm "+index,p);g.localPosition=pos;
            Bar("Palm trunk",g,Vector3.zero,new Vector3(.5f,7,0),.3f,orange);
            for(int j=0;j<7;j++)
            {
                float a=j*360f/7;var geo=new Geometry();Vector3 start=new Vector3(.5f,7,0),mid=start+P(2,a,1.2f),end=start+P(4,a,-.2f),side=P(.55f,a+90);
                geo.Quad(start,mid+side,end,mid-side,Vector3.up);geo.Quad(mid-side,end,mid+side,start,Vector3.down);
                MeshObject("Palm frond",SaveMesh(geo.Mesh(),"Frond_"+index+"_"+j),grass,g,Vector3.zero,false);
            }
        }
        static void Nature()
        {
            var g=Group("01_TerrainAndCliffs",root);
            int index=0;
            for(int i=0;i<22;i++)
            {
                float a=i*11-30;float radius=UnityEngine.Random.Range(44,50);var pos=P(radius,a,-4);var size=new Vector3(UnityEngine.Random.Range(6,11),UnityEngine.Random.Range(9,17),UnityEngine.Random.Range(5,9));
                if(Vector2.Distance(new Vector2(pos.x,pos.z),new Vector2(cfg.domePosition.x,cfg.domePosition.z))<22)continue;
                if(a>105&&a<140)size.y=8;
                RockChunk(g,pos,size,index++);
            }
            RockChunk(g,new Vector3(-11,-5,41),new Vector3(5,14,9),index++);RockChunk(g,new Vector3(11,-5,41),new Vector3(5,14,9),index++);
            RockChunk(g,new Vector3(-62,-5,-13),new Vector3(9,22,9),index++);
            RockChunk(g,new Vector3(-59,-5,23),new Vector3(8,16,8),index++);
            RockChunk(g,new Vector3(22,-4,-35),new Vector3(12,7,10),index++);
            RockChunk(g,new Vector3(-24,-4,-29),new Vector3(12,7,9),index++);
            RockArch(g,new Vector3(29,0,-17),-35,0);RockArch(g,new Vector3(-23,3,34),-12,1);
            for(int i=0;i<6;i++)
            {
                var pos=new Vector3(-105+i*40,-12,145+UnityEngine.Random.Range(-8,15));
                RockChunk(g,pos,new Vector3(26,UnityEngine.Random.Range(48,80),26),index++,true);
            }
            Palm(g,new Vector3(-26,3,-29),0);Palm(g,new Vector3(35,8,24),1);Palm(g,new Vector3(-48,10,23),2);
            var shore=Group("Grass terraces and loose floor panels",g);
            for(int side=-1;side<=1;side+=2)
            {
                Box("Grass terrace",shore,new Vector3(-23,.35f,side*17),new Vector3(11,1.2f,8),grass);
                for(int j=0;j<4;j++)Box("Loose ivory floor panel",shore,new Vector3(-26+j%2*3,.99f,side*17+j/2*2.3f),new Vector3(2.7f,.16f,2.1f),ivory,Quaternion.Euler(0,j*3,0));
            }
            var ocean=Group("08_Ocean",root);Box("Ocean surface",ocean,new Vector3(0,-3.2f,0),new Vector3(1200,.12f,1200),water,null,false);
        }
        static void Lighting()
        {
            var g=Group("10_LightingAndSky",root);var light=new GameObject("Tropical Sun").AddComponent<Light>();light.transform.SetParent(g,false);light.transform.rotation=Quaternion.Euler(46,-35,0);light.type=LightType.Directional;light.intensity=2.1f;light.color=new Color(1,.94f,.81f);light.shadows=LightShadows.Soft;RenderSettings.sun=light;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.51f,.72f,.83f);RenderSettings.ambientEquatorColor=new Color(.4f,.51f,.55f);RenderSettings.ambientGroundColor=new Color(.25f,.29f,.28f);RenderSettings.ambientIntensity=1;
            var path=Base+"/Materials/TropicalSky.mat";var sky=AssetDatabase.LoadAssetAtPath<Material>(path);if(!sky){sky=new Material(Shader.Find("BiogearLab/Tropical Sky"));AssetDatabase.CreateAsset(sky,path);}sky.shader=Shader.Find("BiogearLab/Tropical Sky");RenderSettings.skybox=sky;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0024f;RenderSettings.fogColor=new Color(.35f,.65f,.74f);
            var volume=new GameObject("Environment Post Processing").AddComponent<Volume>();volume.transform.SetParent(g,false);volume.isGlobal=true;
            string profilePath=Base+"/Lighting/BiogearVolume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if(!profile)
            {profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,profilePath);var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.24f);bloom.threshold.Override(1.1f);AssetDatabase.AddObjectToAsset(bloom,profile);var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.Neutral);AssetDatabase.AddObjectToAsset(tone,profile);}
            volume.sharedProfile=profile;
        }
        static void Cameras()
        {
            var g=Group("12_ReviewCameras",root);var points=new List<Transform>();
            Vector3[] pos={cfg.overviewPosition,new Vector3(24,11,-27),new Vector3(-16,6,-15),new Vector3(-27,1.4f,-3),new Vector3(0,2.5f,18),new Vector3(25,15,18),new Vector3(12,6,-18),new Vector3(4,82,-55)};
            Vector3[] targets={cfg.overviewTarget,new Vector3(0,13,6),new Vector3(-36,3,0),new Vector3(-37,1.7f,0),new Vector3(0,5,33),new Vector3(0,31,38),new Vector3(0,12,0),new Vector3(-4,0,7)};
            string[] names={"01_Overview","02_CentralFront","03_DomeApproach","04_DomeInterior","05_Corridor","06_TowerSide","07_UnderDisk","08_AerialLayout"};
            for(int i=0;i<pos.Length;i++){var t=Group(names[i],g);t.localPosition=pos[i];t.LookAt(targets[i]);points.Add(t);}
            var camera=new GameObject("Biogear Exploration Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.transform.SetParent(g,false);camera.transform.SetPositionAndRotation(points[0].position,points[0].rotation);camera.fieldOfView=55;camera.nearClipPlane=.08f;camera.farClipPlane=1200;camera.allowHDR=true;camera.gameObject.AddComponent<AudioListener>();camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            var explorer=camera.gameObject.AddComponent<BiogearExplorer>();explorer.viewpoints=points.ToArray();
        }
        [MenuItem("Tools/Biogear Lab/Build or Update Environment")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode before building.");
            foreach(string dir in new[]{"Scenes","Meshes","Materials","Textures","Prefabs","Lighting","Data","Documentation"})Directory.CreateDirectory(Base+"/"+dir);AssetDatabase.Refresh();
            cfg=AssetDatabase.LoadAssetAtPath<BiogearLayoutConfig>(Base+"/Data/Layout.asset");if(!cfg){cfg=ScriptableObject.CreateInstance<BiogearLayoutConfig>();AssetDatabase.CreateAsset(cfg,Base+"/Data/Layout.asset");}
            var previous=SceneManager.GetActiveScene();if(previous.path==ScenePath && previous.isDirty)throw new Exception("Save your scene before rebuilding. Builder replaces its own generated scene.");
            if(File.Exists(ScenePath)) {string backup="Backups/BiogearLab/"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity";Directory.CreateDirectory(Path.GetDirectoryName(backup));File.Copy(ScenePath,backup,true);}
            if(previous.path==ScenePath) previous=EditorSceneManager.OpenScene(EnvironmentTools.Hub);
            var state=UnityEngine.Random.state;UnityEngine.Random.InitState(cfg.seed);meshId=0;meshCache.Clear();materialCache.Clear();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
            bool editing=false;
            try
            {
                AssetDatabase.StartAssetEditing();editing=true;
                root=Group("BiogearLab_Root",null);Materials();boxMesh=BoxMesh();deckMesh=BoxMesh(true);Arena();UpperDisk();Tower();Dome();Corridor();Nature();Lighting();Cameras();
                AssetDatabase.StopAssetEditing();editing=false;
                PrefabUtility.SaveAsPrefabAsset(root.gameObject,Base+"/Prefabs/BiogearLab_Environment.prefab");
                EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
            }
            finally {if(editing)AssetDatabase.StopAssetEditing();UnityEngine.Random.state=state;EditorSceneManager.CloseScene(scene,true);if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);}
            AssetDatabase.Refresh();Register();Debug.Log("BIOGEAR_ENVIRONMENT_BUILT: "+ScenePath);
        }
        public static void Register()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath);if(!catalog)throw new Exception("Environment catalog missing.");
            var definition=catalog.Find("biogear-lab");if(!definition){definition=ScriptableObject.CreateInstance<EnvironmentDefinition>();definition.id="biogear-lab";AssetDatabase.CreateAsset(definition,Base+"/Data/BiogearDefinition.asset");catalog.environments=catalog.environments.Concat(new[]{definition}).ToArray();EditorUtility.SetDirty(catalog);}
            definition.displayName="BIOGEAR LAB";definition.description="Đảo nghiên cứu · tháp, đấu trường, nhà vòm · camera tự do.";definition.scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);definition.battleVariant=-1;definition.available=true;definition.SyncScene();EditorUtility.SetDirty(definition);EnvironmentTools.RegisterScenes();AssetDatabase.SaveAssets();
        }
        [MenuItem("Tools/Biogear Lab/Open Environment")]
        public static void Open(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(ScenePath);}
        [MenuItem("Tools/Biogear Lab/Capture Review Views")]
        public static void Capture()
        {
            var camera=UnityEngine.Object.FindAnyObjectByType<Camera>();var explorer=camera?camera.GetComponent<BiogearExplorer>():null;if(!explorer)throw new Exception("Open Biogear Lab scene first.");
            Directory.CreateDirectory(Base+"/Documentation/Review");var oldPos=camera.transform.position;var oldRot=camera.transform.rotation;var target=camera.targetTexture;var active=RenderTexture.active;float aspect=camera.aspect;float fov=camera.fieldOfView;
            try
            {
                for(int i=0;i<explorer.viewpoints.Length;i++)
                {
                    explorer.View(i);var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);
                    camera.targetTexture=rt;camera.aspect=16f/9;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(Base+"/Documentation/Review/"+explorer.viewpoints[i].name+".png",tex.EncodeToPNG());camera.targetTexture=target;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
                }
            }
            finally {camera.transform.SetPositionAndRotation(oldPos,oldRot);camera.targetTexture=target;camera.aspect=aspect;camera.fieldOfView=fov;RenderTexture.active=active;}
            AssetDatabase.Refresh();var d=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath).Find("biogear-lab");d.thumbnail=AssetDatabase.LoadAssetAtPath<Texture2D>(Base+"/Documentation/Review/01_Overview.png");EditorUtility.SetDirty(d);AssetDatabase.SaveAssets();Debug.Log("BIOGEAR_REVIEW_READY");
        }
    }
}
