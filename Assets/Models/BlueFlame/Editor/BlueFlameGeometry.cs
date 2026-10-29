using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BlueFlame.Editor
{
    // Units: one model unit = approximately 30 mm on the display kit. Front is -Z.
    // Armor panels use separate cap, bevel, wall and rear lip surfaces, not a triangle fan ridge.
    public static class BFG
    {
        public static readonly List<Mesh> Meshes = new List<Mesh>();
        static int serial;
        public static Vector3 V(float x,float y,float z=0) => new Vector3(x,y,z);
        public static Transform Group(string name,Transform parent,Vector3 position=default,Vector3 euler=default)
        { var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;t.localEulerAngles=euler;return t; }
        public sealed class Geo
        {
            readonly List<Vector3> vertices=new List<Vector3>();
            readonly List<Vector2> uv=new List<Vector2>();
            readonly List<Vector3> normals=new List<Vector3>();
            readonly List<int> indices=new List<int>();
            public void Face(params Vector3[] p)
            {
                int k=vertices.Count;Vector3 n=Vector3.Cross(p[1]-p[0],p[2]-p[0]).normalized;
                Vector3 u=Mathf.Abs(n.y)>.8f?Vector3.right:Vector3.Cross(Vector3.up,n).normalized;
                Vector3 v=Vector3.Cross(n,u);
                foreach(var point in p){vertices.Add(point);normals.Add(n);uv.Add(new Vector2(Vector3.Dot(point,u),Vector3.Dot(point,v)));}
                for(int i=1;i<p.Length-1;i++)indices.AddRange(new[]{k,k+i,k+i+1});
            }
            public void SmoothFace(Vector3[] p,Vector3[] n)
            {
                int start=vertices.Count;Face(p);for(int i=0;i<p.Length;i++)normals[start+i]=n[i];
            }
            public Mesh Finish(string name)
            {
                var m=new Mesh{name="BF_"+(serial++).ToString("D5")+"_"+name,indexFormat=IndexFormat.UInt32};
                m.SetVertices(vertices);m.SetUVs(0,uv);m.SetTriangles(indices,0);m.SetNormals(normals);m.RecalculateTangents();m.RecalculateBounds();Meshes.Add(m);return m;
            }
        }
        public static GameObject Object(string name,Transform parent,Mesh mesh,Material mat)
        {var o=Group(name,parent).gameObject;o.AddComponent<MeshFilter>().sharedMesh=mesh;var r=o.AddComponent<MeshRenderer>();r.sharedMaterial=mat;return o;}
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        // Ear clipping retains concave notches and access cutouts in the manufactured outlines.
        static void Cap(Geo g,Vector3[] p,bool reverse)
        {
            var ids=new List<int>();for(int i=0;i<p.Length;i++)ids.Add(i);
            int guard=0;
            while(ids.Count>2 && guard++<p.Length*p.Length)
            {
                bool clipped=false;
                for(int j=0;j<ids.Count;j++)
                {
                    int a=ids[(j+ids.Count-1)%ids.Count],b=ids[j],c=ids[(j+1)%ids.Count];
                    Vector2 aa=p[a],bb=p[b],cc=p[c];if(Cross(bb-aa,cc-bb)>=-.0000001f)continue;
                    bool inside=false;
                    foreach(int q in ids){if(q==a||q==b||q==c)continue;Vector2 pt=p[q];if(Cross(bb-aa,pt-aa)<-.000001f&&Cross(cc-bb,pt-bb)<-.000001f&&Cross(aa-cc,pt-cc)<-.000001f){inside=true;break;}}
                    if(inside)continue;
                    if(reverse)g.Face(p[a],p[c],p[b]);else g.Face(p[a],p[b],p[c]);
                    ids.RemoveAt(j);clipped=true;break;
                }
                if(!clipped)break;
            }
        }
        static Vector3[] Inset(Vector3[] p,float amount,float dz)
        {
            var q=new Vector3[p.Length];
            for(int i=0;i<p.Length;i++)
            {
                Vector2 a=p[(i+p.Length-1)%p.Length],b=p[i],c=p[(i+1)%p.Length];
                Vector2 u=(b-a).normalized,v=(c-b).normalized;
                Vector2 n1=new Vector2(u.y,-u.x),n2=new Vector2(v.y,-v.x),bis=(n1+n2).normalized;
                float d=amount/Mathf.Max(.3f,Vector2.Dot(bis,n1));
                q[i]=p[i]+V(bis.x*d,bis.y*d,dz);
            }return q;
        }
        public static GameObject Panel(string name,Transform parent,Material mat,float thickness,float bevel,params Vector3[] outline)
        {
            var p=(Vector3[])outline.Clone();float area=0;for(int i=0;i<p.Length;i++)area+=Cross(p[i],p[(i+1)%p.Length]);if(area>0)Array.Reverse(p);
            // Fit a plane to the authored perimeter. This prevents accidental diagonal folds
            // across a single stamped panel; intentional ridges are separate physical panels.
            Vector3 normal=Vector3.zero,center=Vector3.zero;
            for(int i=0;i<p.Length;i++){var a=p[i];var b=p[(i+1)%p.Length];normal+=new Vector3((a.y-b.y)*(a.z+b.z),(a.z-b.z)*(a.x+b.x),(a.x-b.x)*(a.y+b.y));center+=a;}
            center/=p.Length;normal.Normalize();
            if(Mathf.Abs(normal.z)>.12f)for(int i=0;i<p.Length;i++)p[i].z=center.z-(normal.x*(p[i].x-center.x)+normal.y*(p[i].y-center.y))/normal.z;
            bevel=Mathf.Min(bevel,thickness*.35f);
            var g=new Geo();var front=Inset(p,bevel,0);var lip=Inset(p,bevel*.22f,bevel*.28f);var edge=Inset(p,0,bevel);
            var backEdge=Inset(p,0,thickness-bevel);var back=Inset(p,bevel,thickness);
            Cap(g,front,false);Cap(g,back,true);
            var loops=new[]{front,lip,edge,backEdge,back};
            for(int k=0;k<loops.Length-1;k++)for(int i=0;i<p.Length;i++){int j=(i+1)%p.Length;g.Face(loops[k][i],loops[k+1][i],loops[k+1][j],loops[k][j]);}
            return Object(name,parent,g.Finish(name),mat);
        }
        public static GameObject Box(string name,Transform p,Vector3 c,Vector3 size,Material m,float bevel=.025f,Vector3 e=default)
        {
            float x=size.x/2,y=size.y/2,b=Mathf.Min(bevel,Mathf.Min(x,y)*.35f);
            var o=Panel(name,p,m,size.z,b,V(-x+b,-y,-size.z/2),V(-x,-y+b,-size.z/2),V(-x,y-b,-size.z/2),V(-x+b,y,-size.z/2),V(x-b,y,-size.z/2),V(x,y-b,-size.z/2),V(x,-y+b,-size.z/2),V(x-b,-y,-size.z/2));
            o.transform.localPosition=c;o.transform.localEulerAngles=e;return o;
        }
        // Section = y, half-width, half-depth, z center. A 12-sided profile wraps around the frame.
        public static GameObject Hull(string name,Transform parent,Material mat,params Vector4[] sections)
        {
            var g=new Geo();var rings=new List<Vector3[]>();
            foreach(var s in sections)
            {
                float x=s.y,z=s.z;var r=new[]{V(-x*.72f,s.x,-z+s.w),V(x*.72f,s.x,-z+s.w),V(x*.94f,s.x,-z*.78f+s.w),V(x,s.x,-z*.48f+s.w),V(x,s.x,z*.48f+s.w),V(x*.78f,s.x,z+s.w),V(x*.4f,s.x,z+s.w),V(-x*.4f,s.x,z+s.w),V(-x*.78f,s.x,z+s.w),V(-x,s.x,z*.48f+s.w),V(-x,s.x,-z*.48f+s.w),V(-x*.94f,s.x,-z*.78f+s.w)};rings.Add(r);
            }
            for(int j=0;j<rings.Count-1;j++)for(int i=0;i<12;i++){int k=(i+1)%12;g.Face(rings[j][i],rings[j+1][i],rings[j+1][k],rings[j][k]);}
            for(int i=0;i<12;i++){int k=(i+1)%12;g.Face(V(0,sections[0].x,sections[0].w),rings[0][i],rings[0][k]);int j=rings.Count-1;g.Face(V(0,sections[j].x,sections[j].w),rings[j][k],rings[j][i]);}
            return Object(name,parent,g.Finish(name),mat);
        }
        // Lathed machined part. Section = axial distance, outer radius. Axis initially +Z.
        public static GameObject Lathe(string name,Transform parent,Vector3 pos,Material mat,Vector2[] sections,int steps=48,Vector3 e=default,bool cap=true)
        {
            var g=new Geo();Func<int,int,Vector3> p=(j,i)=>{float a=i*Mathf.PI*2/steps;return V(Mathf.Cos(a)*sections[j].y,Mathf.Sin(a)*sections[j].y,sections[j].x);};
            for(int j=0;j<sections.Length-1;j++)for(int i=0;i<steps;i++)
            {
                Vector2 delta=sections[j+1]-sections[j];
                Func<int,Vector3> normal=k=>{float a=k*Mathf.PI*2/steps;return V(Mathf.Cos(a)*delta.x,Mathf.Sin(a)*delta.x,-delta.y).normalized;};
                g.SmoothFace(new[]{p(j,i),p(j,i+1),p(j+1,i+1),p(j+1,i)},new[]{normal(i),normal(i+1),normal(i+1),normal(i)});
            }
            if(cap)for(int i=0;i<steps;i++){g.Face(V(0,0,sections[0].x),p(0,i+1),p(0,i));int k=sections.Length-1;g.Face(V(0,0,sections[k].x),p(k,i),p(k,i+1));}
            var o=Object(name,parent,g.Finish(name),mat);o.transform.localPosition=pos;o.transform.localEulerAngles=e;return o;
        }
        public static GameObject Tube(string name,Transform p,Vector3 pos,float outer,float inner,float depth,Material mat,Vector3 e=default)
        {return Lathe(name,p,pos,mat,new[]{new Vector2(-depth/2,inner),new Vector2(-depth/2,outer*.95f),new Vector2(-depth/2+.012f,outer),new Vector2(depth/2-.012f,outer),new Vector2(depth/2,outer*.95f),new Vector2(depth/2,inner),new Vector2(-depth/2,inner)},48,e,false);}
        public static GameObject Rod(string name,Transform parent,Vector3 a,Vector3 b,float radius,Material mat,int sides=32)
        {
            float d=Vector3.Distance(a,b);var o=Lathe(name,parent,a,mat,new[]{new Vector2(0,radius*.9f),new Vector2(.005f,radius),new Vector2(d-.005f,radius),new Vector2(d,radius*.9f)},sides);o.transform.localRotation=Quaternion.FromToRotation(Vector3.forward,b-a);return o;
        }
        public static void Line(string name,Transform parent,Material mat,float width,params Vector3[] path)
        {
            for(int i=0;i<path.Length-1;i++)
            {
                var d=path[i+1]-path[i];var side=new Vector3(-d.y,d.x,0).normalized*width*.5f;
                Panel(name,parent,mat,.003f,.0005f,path[i]-side,path[i]+side,path[i+1]+side,path[i+1]-side);
            }
        }
        public static Vector4 S(float y,float halfWidth,float halfDepth,float z=0)=>new Vector4(y,halfWidth,halfDepth,z);
    }
}
