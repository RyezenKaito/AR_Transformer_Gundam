using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class ShopArtwork:MaskableGraphic
    {
        public enum Kind { Frame, Auction }
        public Kind kind;
        Vector2 P(float x,float y){var r=rectTransform.rect;return new Vector2(r.xMin+x*r.width/100,r.yMax-y*r.height/140);}
        void Path(VertexHelper vh,Color tint,float width,params float[] xy)
        {
            for(int n=2;n<xy.Length;n+=2){var a=P(xy[n-2],xy[n-1]);var b=P(xy[n],xy[n+1]);var d=new Vector2(a.y-b.y,b.x-a.x).normalized*width*.5f;int i=vh.currentVertCount;vh.AddVert(a-d,tint,Vector2.zero);vh.AddVert(a+d,tint,Vector2.zero);vh.AddVert(b+d,tint,Vector2.zero);vh.AddVert(b-d,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
        }
        void Fill(VertexHelper vh,Color tint,params float[] xy){int start=vh.currentVertCount;for(int i=0;i<xy.Length;i+=2)vh.AddVert(P(xy[i],xy[i+1]),tint,Vector2.zero);for(int i=1;i<xy.Length/2-1;i++)vh.AddTriangle(start,start+i,start+i+1);}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(kind==Kind.Auction){Path(vh,color,2.5f,24,20,54,20,67,44,37,44,24,20);Path(vh,color,2.5f,48,44,77,100);Path(vh,color,2.5f,23,108,77,108);Path(vh,color,2.5f,30,100,70,100);return;}
            Fill(vh,new Color(.018f,.061f,.10f,.98f),8,3,92,3,97,12,97,128,90,137,8,137,3,128,3,13);
            var dim=new Color(color.r*.45f,color.g*.45f,color.b*.45f,color.a);
            Path(vh,color,1.8f,8,3,92,3,97,12,97,128,90,137,8,137,3,128,3,13,8,3);
            Path(vh,dim,1,13,12,87,12,87,128,13,128,13,12);
            Path(vh,color,2.6f,4,25,12,17,12,4);Path(vh,color,2.6f,96,25,88,17,88,4);Path(vh,color,2.6f,4,116,12,124,12,136);Path(vh,color,2.6f,96,116,88,124,88,136);
            var blue=new Color(.10f,.40f,.54f,.5f);Path(vh,blue,1.2f,24,55,76,55,50,99,24,55);Path(vh,blue,1.2f,34,63,66,63,50,88,34,63);
            Fill(vh,new Color(.10f,.40f,.54f,.15f),34,63,66,63,50,88);
            Path(vh,blue,1,24,111,40,111);Path(vh,blue,1,24,116,61,116);Path(vh,blue,1,24,121,52,121);
            Path(vh,dim,1,18,22,27,22);Path(vh,dim,1,73,22,82,22);
        }
    }
}
