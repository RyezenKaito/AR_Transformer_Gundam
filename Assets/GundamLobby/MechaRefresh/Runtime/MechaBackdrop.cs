using UnityEngine;
using UnityEngine.UI;
namespace GundamLobby
{
 [RequireComponent(typeof(CanvasRenderer))]public sealed class MechaBackdrop:MaskableGraphic
 {
  public bool showroom;VertexHelper mesh;Rect r;
  void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color ca,Color cb,Color cc,Color cd){int n=mesh.currentVertCount;mesh.AddVert(a,ca,Vector2.zero);mesh.AddVert(b,cb,Vector2.zero);mesh.AddVert(c,cc,Vector2.zero);mesh.AddVert(d,cd,Vector2.zero);mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);}
  Vector2 P(float x,float y)=>new(r.x+x*r.width,r.y+y*r.height);
  protected override void OnPopulateMesh(VertexHelper vh)
  {
   vh.Clear();mesh=vh;r=rectTransform.rect;Quad(P(0,0),P(0,1),P(1,1),P(1,0),new(.009f,.018f,.027f),new(.018f,.039f,.058f),new(.025f,.053f,.075f),new(.012f,.024f,.036f));
   Vector2 center=P(showroom?.64f:.66f,.57f);float radius=r.width*.66f;int n=mesh.currentVertCount;mesh.AddVert(center,new Color(.10f,.23f,.30f,.35f),Vector2.zero);for(int i=0;i<=40;i++){float a=i*Mathf.PI*2/40;mesh.AddVert(center+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.8f),new Color(.025f,.07f,.1f,0),Vector2.zero);if(i>0)mesh.AddTriangle(n,n+i,n+i+1);}
   if(showroom)for(int i=0;i<9;i++){float y=.18f+i*.022f;Quad(P(.08f,y),P(.08f,y+.001f),P(.94f,y+.001f),P(.94f,y),new(.12f,.27f,.33f,.13f),new(.12f,.27f,.33f,.13f),new(.12f,.27f,.33f,.13f),new(.12f,.27f,.33f,.13f));}
  }
 }
}