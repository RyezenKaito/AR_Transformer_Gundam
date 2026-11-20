using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class ShopBackdrop:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();const int nx=40,ny=24;var r=rectTransform.rect;
            for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++)
            {
                float u=x/(float)nx,v=y/(float)ny;
                float a=Mathf.Exp(-((u-.68f)*(u-.68f)*8+(v-.38f)*(v-.38f)*12));
                float b=Mathf.Exp(-((u-.23f)*(u-.23f)*12+(v-.05f)*(v-.05f)*20));
                float c=Mathf.Exp(-((u-.98f)*(u-.98f)*80+(v-.78f)*(v-.78f)*25));
                var tint=new Color(.019f+a*.025f+b*.027f+c*.04f,.043f+a*.043f+b*.043f+c*.064f,.063f+a*.058f+b*.047f+c*.08f,1);
                vh.AddVert(new Vector2(r.xMin+u*r.width,r.yMin+v*r.height),tint,Vector2.zero);
                if(x<nx&&y<ny){int i=y*(nx+1)+x;vh.AddTriangle(i,i+nx+1,i+1);vh.AddTriangle(i+1,i+nx+1,i+nx+2);}
            }
        }
    }
}
