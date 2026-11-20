using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class BattleModeIcon : MaskableGraphic
    {
        public enum Kind { Campaign, Arena, Challenge, Training, CustomRoom }
        public Kind kind;
        Vector2 P(float x,float y){var r=rectTransform.rect;return new Vector2(r.xMin+x*r.width/48,r.yMax-y*r.height/48);}
        void Path(VertexHelper vh,params float[] points)
        {
            for(int n=2;n<points.Length;n+=2)
            {
                var a=P(points[n-2],points[n-1]);var b=P(points[n],points[n+1]);var offset=new Vector2(a.y-b.y,b.x-a.x).normalized*.8f;int i=vh.currentVertCount;
                vh.AddVert(a-offset,color,Vector2.zero);vh.AddVert(a+offset,color,Vector2.zero);vh.AddVert(b+offset,color,Vector2.zero);vh.AddVert(b-offset,color,Vector2.zero);
                vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
            }
        }
        void Hex(VertexHelper vh,float x,float y,float radius){Path(vh,x,y-radius,x+radius*.87f,y-radius*.5f,x+radius*.87f,y+radius*.5f,x,y+radius,x-radius*.87f,y+radius*.5f,x-radius*.87f,y-radius*.5f,x,y-radius);}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            switch(kind)
            {
                case Kind.Campaign:Hex(vh,24,10,6);Hex(vh,10,36,6);Hex(vh,38,36,6);Path(vh,21,16,13,30);Path(vh,27,16,35,30);Path(vh,16,36,32,36);break;
                case Kind.Arena:Path(vh,9,6,23,17,35,35,32,38,17,23,9,6);Path(vh,39,6,25,17,13,35,16,38,31,23,39,6);Path(vh,9,32,20,43);Path(vh,28,43,39,32);break;
                case Kind.Challenge:Path(vh,10,5,38,5,36,29,24,43,12,29,10,5);Path(vh,15,16,24,10,33,16);Path(vh,16,25,24,19,32,25);Path(vh,19,32,24,28,29,32);break;
                case Kind.Training:Hex(vh,24,24,16);Hex(vh,24,24,7);Path(vh,24,2,24,12);Path(vh,24,36,24,46);Path(vh,2,24,12,24);Path(vh,36,24,46,24);break;
                case Kind.CustomRoom:Path(vh,6,5,42,5,42,41,6,41,6,5);Path(vh,24,5,24,41);Path(vh,6,23,42,23);Path(vh,12,14,18,14);Path(vh,30,14,36,14);Path(vh,12,32,18,32);Path(vh,30,29,30,35);Path(vh,27,32,33,32);break;
            }
        }
    }
}
