using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class LobbyHeaderIcon : MaskableGraphic
    {
        public enum Symbol { Energy, Credits, Crystal, Plus, Chevron }
        public Symbol symbol;
        Vector2 P(float x,float y){var r=rectTransform.rect;return new Vector2(r.xMin+x*r.width/32,r.yMax-y*r.height/32);}
        void Line(VertexHelper vh,Vector2 a,Vector2 b,Color tint,float width=1.5f)
        {
            var n=new Vector2(a.y-b.y,b.x-a.x).normalized*width*.5f;int i=vh.currentVertCount;
            vh.AddVert(a-n,tint,Vector2.zero);vh.AddVert(a+n,tint,Vector2.zero);vh.AddVert(b+n,tint,Vector2.zero);vh.AddVert(b-n,tint,Vector2.zero);
            vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
        void Path(VertexHelper vh,params float[] xy){for(int i=2;i<xy.Length;i+=2)Line(vh,P(xy[i-2],xy[i-1]),P(xy[i],xy[i+1]),color);}
        void Poly(VertexHelper vh,Color tint,params float[] xy)
        {
            int start=vh.currentVertCount;for(int i=0;i<xy.Length;i+=2)vh.AddVert(P(xy[i],xy[i+1]),tint,Vector2.zero);
            for(int i=1;i<xy.Length/2-1;i++)vh.AddTriangle(start,start+i,start+i+1);
        }
        void Ellipse(VertexHelper vh,float y)
        {
            for(int i=0;i<32;i++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;Line(vh,P(16+11*Mathf.Cos(a),y+4*Mathf.Sin(a)),P(16+11*Mathf.Cos(b),y+4*Mathf.Sin(b)),color,1.3f);}
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var dim=new Color(color.r*.55f,color.g*.55f,color.b*.55f,color.a);
            switch(symbol)
            {
                case Symbol.Energy:Poly(vh,color,18,2,6,19,14,19,12,30,27,12,18,12);break;
                case Symbol.Credits:Ellipse(vh,8);Ellipse(vh,15);Ellipse(vh,22);Path(vh,5,8,5,22);Path(vh,27,8,27,22);Path(vh,13,6,18,6,18,10,13,10);break;
                case Symbol.Crystal:Poly(vh,dim,9,3,23,3,30,13,16,30,2,13);Poly(vh,color,9,3,16,13,2,13);Poly(vh,color,23,3,30,13,16,30,16,13);Path(vh,9,3,23,3,30,13,16,30,2,13,9,3);Path(vh,2,13,30,13);Path(vh,9,3,16,13,23,3);Path(vh,16,13,16,30);break;
                case Symbol.Plus:Path(vh,16,7,16,25);Path(vh,7,16,25,16);break;
                case Symbol.Chevron:Path(vh,10,6,21,16,10,26);break;
            }
        }
    }
}
