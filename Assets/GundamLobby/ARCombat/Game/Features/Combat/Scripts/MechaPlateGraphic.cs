using UnityEngine;
using UnityEngine.UI;

namespace GundamGame.CombatDemo
{
    [AddComponentMenu("Gundam UI/Mecha Plate")]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MechaPlateGraphic : MaskableGraphic
    {
        public Color top=new Color(.12f,.17f,.21f,.98f),bottom=new Color(.025f,.04f,.06f,.98f);
        public Color edge=new Color(.4f,.55f,.62f,1);
        [Min(0)] public float cut=16,edgeWidth=2;
        public void TintEdge(Color value){if(edge==value)return;edge=value;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();Rect r=GetPixelAdjustedRect();if(r.width<=0||r.height<=0)return;
            float width=Mathf.Clamp(edgeWidth,0,Mathf.Min(r.width,r.height)*.15f);
            var outer=Points(r,cut);var inner=Points(new Rect(r.x+width,r.y+width,r.width-width*2,r.height-width*2),Mathf.Max(0,cut-width*.6f));
            for(int i=0;i<8;i++){vh.AddVert(outer[i],edge*color,Vector2.zero);vh.AddVert(inner[i],edge*color,Vector2.zero);}
            for(int i=0;i<8;i++){int a=i*2,b=((i+1)%8)*2;vh.AddTriangle(a,b,a+1);vh.AddTriangle(b,b+1,a+1);}
            int start=vh.currentVertCount;
            vh.AddVert(r.center,Color.Lerp(bottom,top,.5f)*color,Vector2.zero);
            for(int i=0;i<8;i++)vh.AddVert(inner[i],Color.Lerp(bottom,top,Mathf.InverseLerp(r.yMin,r.yMax,inner[i].y))*color,Vector2.zero);
            for(int i=0;i<8;i++)vh.AddTriangle(start,start+i+1,start+(i+1)%8+1);
        }
        static Vector2[] Points(Rect r,float corner)
        {
            float c=Mathf.Clamp(corner,0,Mathf.Min(r.width,r.height)*.45f);
            return new[]{new Vector2(r.xMin+c,r.yMin),new Vector2(r.xMax-c,r.yMin),new Vector2(r.xMax,r.yMin+c),new Vector2(r.xMax,r.yMax-c),new Vector2(r.xMax-c,r.yMax),new Vector2(r.xMin+c,r.yMax),new Vector2(r.xMin,r.yMax-c),new Vector2(r.xMin,r.yMin+c)};
        }
    }
}
