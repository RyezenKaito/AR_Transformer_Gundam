using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HaoLing.Editor
{
    // Authored mesh primitives. All armor is closed geometry, with planar hard-surface normals.
    public static class HLM
    {
        public static Material White, Pearl, Navy, Frame, Gold, Red, Ruby, Teal, Black;
        public static readonly List<Mesh> Meshes = new List<Mesh>();
        static int serial;
        public static Transform Group(string name, Transform parent, Vector3 pos=default(Vector3), Vector3 euler=default(Vector3))
        {
            var t=new GameObject(name).transform; t.SetParent(parent,false); t.localPosition=pos; t.localEulerAngles=euler; return t;
        }
        sealed class Geo
        {
            public readonly List<Vector3> v=new List<Vector3>();
            public readonly List<Vector2> uv=new List<Vector2>();
            public readonly List<int> tri=new List<int>();
            public void Face(params Vector3[] p)
            {
                int start=v.Count; Vector3 normal=Vector3.Cross(p[1]-p[0],p[2]-p[0]).normalized;
                Vector3 u=(p[1]-p[0]).normalized; Vector3 w=Vector3.Cross(normal,u);
                for(int i=0;i<p.Length;i++){v.Add(p[i]); uv.Add(new Vector2(Vector3.Dot(p[i],u),Vector3.Dot(p[i],w))*2);}
                for(int i=1;i<p.Length-1;i++){tri.Add(start);tri.Add(start+i);tri.Add(start+i+1);}
            }
            public Mesh Finish(string name)
            {
                var m=new Mesh{name="HL_"+(serial++).ToString("D5")+"_"+name,indexFormat=v.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
                m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(tri,0);m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();
                Meshes.Add(m);return m;
            }
        }
        static GameObject Object(string name,Transform parent,Mesh mesh,Material material)
        {
            var go=Group(name,parent).gameObject;go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;
            return go;
        }
        public static GameObject Plate(string name,Transform parent,Vector3[] outline,float depth,Material material,float ridge=.02f)
        {
            var g=new Geo();Vector3 center=Vector3.zero;foreach(var p in outline)center+=p;center/=outline.Length;
            // Ensure a front-facing (-Z) winding, independently of author order.
            float area=0; for(int i=0;i<outline.Length;i++){var a=outline[i];var b=outline[(i+1)%outline.Length];area+=a.x*b.y-b.x*a.y;}
            var pts=(Vector3[])outline.Clone();if(area>0)Array.Reverse(pts);
            Vector3 front=center-Vector3.forward*ridge,back=center+Vector3.forward*depth;
            var inner=new Vector3[pts.Length];
            for(int i=0;i<pts.Length;i++)
            {
                Vector3 delta=center-pts[i];delta.z=0;
                float lip=Mathf.Min(depth*.16f,delta.magnitude*.07f);
                inner[i]=pts[i]+delta.normalized*lip-Vector3.forward*Mathf.Min(depth*.10f,.008f);
            }
            for(int i=0;i<pts.Length;i++)
            {
                Vector3 a=pts[i],b=pts[(i+1)%pts.Length],aa=a+Vector3.forward*depth,bb=b+Vector3.forward*depth;
                g.Face(front,inner[i],inner[(i+1)%pts.Length]);g.Face(inner[i],a,b,inner[(i+1)%pts.Length]);
                g.Face(back,bb,aa);g.Face(a,aa,bb,b);
            }
            return Object(name,parent,g.Finish(name),material);
        }
        public static GameObject Blade(string name,Transform parent,Vector3 a,Vector3 b,float width,float depth,Material material,float sweep=0)
        {
            var d=b-a;var side=Vector3.Cross(d.normalized,Vector3.forward).normalized;
            if(side.sqrMagnitude<.1f)side=Vector3.right;
            return Plate(name,parent,new[]{a-side*width*.16f,a+d*.14f-side*width*.46f,a+d*.60f-side*width*.34f+side*sweep,b,a+d*.66f+side*width*.32f+side*sweep,a+d*.22f+side*width*.50f,a+side*width*.14f},depth,material,Mathf.Min(width*.12f,depth*.45f));
        }
        public static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material material,Vector3 euler=default(Vector3),float bevel=.12f)
        {
            var g=new Geo();float x=size.x*.5f,y=size.y*.5f,z=size.z*.5f;
            float b=Mathf.Min(x,Mathf.Min(y,z))*Mathf.Clamp(bevel,.01f,.45f);
            var ring=new[]{new Vector2(-x+b,-y),new Vector2(x-b,-y),new Vector2(x,-y+b),new Vector2(x,y-b),new Vector2(x-b,y),new Vector2(-x+b,y),new Vector2(-x,y-b),new Vector2(-x,-y+b)};
            var f=new Vector3[8];var fb=new Vector3[8];var bk=new Vector3[8];var bkb=new Vector3[8];
            for(int i=0;i<8;i++)
            {
                var p=ring[i];fb[i]=new Vector3(p.x,p.y,-z+b);bkb[i]=new Vector3(p.x,p.y,z-b);
                f[i]=new Vector3(p.x*(x-b)/x,p.y*(y-b)/y,-z);bk[i]=new Vector3(f[i].x,f[i].y,z);
            }
            for(int i=0;i<8;i++){int j=(i+1)%8;g.Face(Vector3.back*z,f[j],f[i]);g.Face(Vector3.forward*z,bk[i],bk[j]);g.Face(f[i],f[j],fb[j],fb[i]);g.Face(fb[i],fb[j],bkb[j],bkb[i]);g.Face(bkb[i],bkb[j],bk[j],bk[i]);}
            var go=Object(name,parent,g.Finish(name),material);go.transform.localPosition=pos;go.transform.localEulerAngles=euler;return go;
        }
        public static GameObject Beam(string name,Transform parent,Vector3 a,Vector3 b,float radius,Material material,int sides=10)
        {
            var g=new Geo();float len=(b-a).magnitude;
            for(int i=0;i<sides;i++)
            {
                float t=i*Mathf.PI*2/sides,u=(i+1)*Mathf.PI*2/sides;
                var p=new Vector3(Mathf.Cos(t)*radius,0,Mathf.Sin(t)*radius);var q=new Vector3(Mathf.Cos(u)*radius,0,Mathf.Sin(u)*radius);
                g.Face(p,p+Vector3.up*len,q+Vector3.up*len,q);g.Face(Vector3.zero,p,q);g.Face(Vector3.up*len,q+Vector3.up*len,p+Vector3.up*len);
            }
            var go=Object(name,parent,g.Finish(name),material);go.transform.localPosition=a;go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);return go;
        }
        public static GameObject Orb(string name,Transform parent,Vector3 pos,Vector3 size,Material material)
        {
            var g=new Geo();int rings=8,segs=12;
            Func<int,int,Vector3> p=(r,s)=>{float a=Mathf.PI*r/rings,b=Mathf.PI*2*s/segs;return Vector3.Scale(new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b)),size*.5f);};
            for(int j=0;j<rings;j++)for(int i=0;i<segs;i++)
            {
                if(j==0)g.Face(p(j,i),p(j+1,i+1),p(j+1,i));
                else if(j==rings-1)g.Face(p(j,i),p(j,i+1),p(j+1,i));
                else g.Face(p(j,i),p(j,i+1),p(j+1,i+1),p(j+1,i));
            }
            var go=Object(name,parent,g.Finish(name),material);go.transform.localPosition=pos;return go;
        }
        public static GameObject Ring(string name,Transform parent,Vector3 pos,float radius,float tube,Material material,Vector3 euler=default(Vector3))
        {
            var g=new Geo();int n=32,k=6;
            Func<int,int,Vector3> p=(i,j)=>{float a=i*Mathf.PI*2/n,b=j*Mathf.PI*2/k;return new Vector3(Mathf.Cos(a)*(radius+Mathf.Cos(b)*tube),Mathf.Sin(a)*(radius+Mathf.Cos(b)*tube),Mathf.Sin(b)*tube);};
            for(int i=0;i<n;i++)for(int j=0;j<k;j++)g.Face(p(i,j),p(i+1,j),p(i+1,j+1),p(i,j+1));
            var go=Object(name,parent,g.Finish(name),material);go.transform.localPosition=pos;go.transform.localEulerAngles=euler;return go;
        }
    }
}
