using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
namespace GundamLobby
{
 [RequireComponent(typeof(CanvasRenderer))] public sealed class ItemArtwork:MaskableGraphic
 {
  public string itemId="fcore";Sprite icon;
  public override Texture mainTexture=>icon?icon.texture:Texture2D.whiteTexture;
  public void Show(string id){itemId=id;icon=MechaItemVisuals.Icon(id);raycastTarget=false;SetAllDirty();}
  protected override void OnEnable(){base.OnEnable();icon=MechaItemVisuals.Icon(itemId);SetAllDirty();}
  protected override void OnPopulateMesh(VertexHelper vh)
  {
   vh.Clear();var r=rectTransform.rect;Vector4 uv=icon?DataUtility.GetOuterUV(icon):new Vector4(0,0,1,1);
   if(icon){float ratio=icon.rect.width/icon.rect.height;if(r.width/r.height>ratio){float w=r.height*ratio;r.x+=(r.width-w)*.5f;r.width=w;}else{float h=r.width/ratio;r.y+=(r.height-h)*.5f;r.height=h;}}
   Color tint=icon?color:new Color(.1f,.25f,.32f,.5f);vh.AddVert(new Vector3(r.xMin,r.yMin),tint,new Vector2(uv.x,uv.y));vh.AddVert(new Vector3(r.xMin,r.yMax),tint,new Vector2(uv.x,uv.w));vh.AddVert(new Vector3(r.xMax,r.yMax),tint,new Vector2(uv.z,uv.w));vh.AddVert(new Vector3(r.xMax,r.yMin),tint,new Vector2(uv.z,uv.y));vh.AddTriangle(0,1,2);vh.AddTriangle(2,3,0);
  }
 }
}