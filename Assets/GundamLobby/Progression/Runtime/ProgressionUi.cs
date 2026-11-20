using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace GundamLobby
{
 public static class ProgressionUi
 {
  public static readonly Color Cyan=new Color(.125f,.785f,.965f),Muted=new Color(.51f,.65f,.72f);
  public static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
  {
   var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
   r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
  }
  public static LobbyShape Shape(string name,Transform parent,float x,float y,float w,float h,Color fill,Color border)
  {
   var s=Rect(name,parent,x,y,w,h).gameObject.AddComponent<LobbyShape>();s.color=fill;s.border=border;s.cut=12;s.raycastTarget=false;return s;
  }
  public static TMP_Text Text(string name,string value,Transform parent,TMP_FontAsset font,float x,float y,float w,float h,float size,Color color)
  {
   var t=Rect(name,parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.raycastTarget=false;
   t.enableAutoSizing=true;t.fontSizeMax=size;t.fontSizeMin=12;t.textWrappingMode=TextWrappingModes.NoWrap;return t;
  }
  public static Button Button(string name,string value,Transform parent,TMP_FontAsset font,float x,float y,float w,float h,UnityAction action,bool primary=false)
  {
   var s=Shape(name,parent,x,y,w,h,primary?new Color(.03f,.42f,.59f):new Color(.025f,.07f,.10f,.96f),primary?Cyan:new Color(.13f,.28f,.34f));s.raycastTarget=true;
   var b=s.gameObject.AddComponent<Button>();b.targetGraphic=s;b.transition=Selectable.Transition.None;b.onClick.AddListener(action);
   var hover=s.gameObject.AddComponent<LobbyHover>();hover.surface=s;hover.normal=s.color;hover.hover=new Color(.08f,.37f,.47f);hover.pressed=new Color(.10f,.46f,.60f);
   var label=Text("Label",value,s.transform,font,10,0,w-20,h,24,Color.white);label.alignment=TextAlignmentOptions.Center;return b;
  }
  public static void Lock(Transform parent,TMP_FontAsset font,float x,float y)
  {
   var ring=Shape("Lock shackle",parent,x+10,y,23,25,Color.clear,Muted);ring.geometry=LobbyShape.Geometry.Ring;ring.thickness=3;
   Shape("Lock body",parent,x,y+15,43,32,Muted,Color.clear).geometry=LobbyShape.Geometry.Box;
   Text("Keyhole","·",parent,font,x+13,y+17,17,25,24,new Color(.03f,.08f,.12f));
  }
 }
}
