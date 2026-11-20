using UnityEngine;
using UnityEngine.UI;

namespace GundamGame.Story
{
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class StoryHudGraphic : MaskableGraphic
    {
        public enum Mark { Hexagon, Arrow, LockCorners, Shield, Evade }
        public Mark shape;
        public Color edge;
        public float width=2;

        public void SetEdge(Color value)
        {
            if(edge==value)return;
            edge=value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect=rectTransform.rect;
            if(rect.width<=0 || rect.height<=0)return;
            if(shape==Mark.Hexagon)
            {
                Polygon(mesh,rect,new[]{new Vector2(.25f,.02f),new Vector2(.75f,.02f),new Vector2(.985f,.5f),
                    new Vector2(.75f,.98f),new Vector2(.25f,.98f),new Vector2(.015f,.5f)},true);
            }
            else if(shape==Mark.Arrow)
            {
                Polygon(mesh,rect,new[]{new Vector2(.13f,.16f),new Vector2(.91f,.50f),new Vector2(.13f,.84f)},true);
            }
            else if(shape==Mark.LockCorners)
            {
                Vector2 inset=new(Mathf.Min(27,rect.width*.20f),Mathf.Min(27,rect.height*.20f));
                foreach(float x in new[]{rect.xMin,rect.xMax})
                foreach(float y in new[]{rect.yMin,rect.yMax})
                {
                    Vector2 corner=new(x,y);
                    Line(mesh,corner,corner+new Vector2(x<rect.center.x?inset.x:-inset.x,0),color,width);
                    Line(mesh,corner,corner+new Vector2(0,y<rect.center.y?inset.y:-inset.y),color,width);
                }
            }
            else if(shape==Mark.Shield)
            {
                Polygon(mesh,rect,new[]{new Vector2(.5f,.11f),new Vector2(.19f,.37f),new Vector2(.15f,.77f),
                    new Vector2(.5f,.90f),new Vector2(.85f,.77f),new Vector2(.81f,.37f)},false);
            }
            else
            {
                for(int i=0;i<2;i++)
                {
                    float shift=i*.29f;
                    Vector2 left=Point(rect,new Vector2(.18f+shift,.23f));
                    Vector2 right=Point(rect,new Vector2(.45f+shift,.50f));
                    Vector2 top=Point(rect,new Vector2(.18f+shift,.77f));
                    Line(mesh,left,right,edge,width);Line(mesh,right,top,edge,width);
                }
            }
        }

        void Polygon(VertexHelper mesh,Rect rect,Vector2[] normals,bool filled)
        {
            if(filled)
            {
                for(int i=0;i<normals.Length;i++)mesh.AddVert(Point(rect,normals[i]),color,Vector2.zero);
                for(int i=1;i<normals.Length-1;i++)mesh.AddTriangle(0,i,i+1);
            }
            if(edge.a<=0)return;
            for(int i=0;i<normals.Length;i++)
                Line(mesh,Point(rect,normals[i]),Point(rect,normals[(i+1)%normals.Length]),edge,width);
        }

        static Vector2 Point(Rect rect,Vector2 uv) => rect.position+Vector2.Scale(rect.size,uv);

        static void Line(VertexHelper mesh,Vector2 a,Vector2 b,Color tint,float thickness)
        {
            Vector2 direction=(b-a).normalized;
            var normal=new Vector2(-direction.y,direction.x)*(thickness*.5f);
            int start=mesh.currentVertCount;
            mesh.AddVert(a-normal,tint,Vector2.zero);mesh.AddVert(a+normal,tint,Vector2.zero);
            mesh.AddVert(b+normal,tint,Vector2.zero);mesh.AddVert(b-normal,tint,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
        }
    }
}
