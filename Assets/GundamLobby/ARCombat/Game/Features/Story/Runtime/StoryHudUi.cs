using System;
using GundamGame.CombatDemo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GundamGame.Story
{
    internal static class StoryHudUi
    {
        public static readonly Color Ink=new(.023f,.048f,.082f), Cyan=new(.28f,.90f,1), Amber=new(1,.68f,.22f),
            Red=new(1,.30f,.35f), White=new(.92f,.97f,1), Muted=new(.61f,.76f,.83f), Line=new(.22f,.41f,.50f),
            Green=new(.55f,1,.78f), RedOrange=new(1,.42f,.25f);

        public static RectTransform Area(Transform parent,string name,float x0,float y0,float x1,float y1)
        {
            var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;
            rect.SetParent(parent,false);
            rect.anchorMin=new Vector2(x0,y0);rect.anchorMax=new Vector2(x1,y1);
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            return rect;
        }

        public static Image Image(Transform parent,string name,float x0,float y0,float x1,float y1,Color tint)
        {
            var image=Area(parent,name,x0,y0,x1,y1).gameObject.AddComponent<Image>();
            image.color=tint;image.raycastTarget=false;
            return image;
        }

        public static Image Art(Transform parent,string name,float x0,float y0,float x1,float y1,Sprite icon)
        {
            var image=Image(parent,name,x0,y0,x1,y1,Color.white);
            image.sprite=icon;image.preserveAspect=true;image.enabled=icon;
            return image;
        }

        public static MechaPlateGraphic Plate(Transform parent,string name,float x0,float y0,float x1,float y1,
            Color edge,float cut=16)
        {
            var plate=Area(parent,name,x0,y0,x1,y1).gameObject.AddComponent<MechaPlateGraphic>();
            plate.top=new Color(.058f,.108f,.152f,.95f);
            plate.bottom=new Color(.013f,.026f,.040f,.97f);
            plate.TintEdge(edge);plate.cut=cut;plate.raycastTarget=false;
            return plate;
        }

        public static TMP_Text Text(Transform parent,string name,string value,TMP_FontAsset font,
            float x0,float y0,float x1,float y1,int size,Color color,TextAlignmentOptions alignment=TextAlignmentOptions.MidlineLeft,
            bool bold=false)
        {
            var text=Area(parent,name,x0,y0,x1,y1).gameObject.AddComponent<TextMeshProUGUI>();
            text.font=font?font:TMP_Settings.defaultFontAsset;
            text.color=color;text.alignment=alignment;text.text=value;text.raycastTarget=false;
            text.enableAutoSizing=true;text.fontSizeMin=size*.54f;text.fontSizeMax=size;text.fontSize=size;
            text.textWrappingMode=TextWrappingModes.NoWrap;text.overflowMode=TextOverflowModes.Ellipsis;
            if(bold)text.fontStyle=FontStyles.Bold;
            return text;
        }

        public static Button Button(Graphic surface,Action callback)
        {
            surface.raycastTarget=true;
            var button=surface.gameObject.AddComponent<Button>();
            button.targetGraphic=surface;
            button.navigation=new Navigation { mode=Navigation.Mode.None };
            var color=button.colors;
            color.highlightedColor=new Color(.87f,1,1);
            color.pressedColor=new Color(.65f,.84f,.91f);
            color.selectedColor=Color.white;
            color.disabledColor=new Color(.55f,.64f,.70f);
            button.colors=color;
            button.onClick.AddListener(()=>callback());
            return button;
        }

        public static StoryHudGraphic Mark(Transform parent,string name,float x0,float y0,float x1,float y1,
            StoryHudGraphic.Mark mark,Color tint,Color edge,float width=2)
        {
            var graphic=Area(parent,name,x0,y0,x1,y1).gameObject.AddComponent<StoryHudGraphic>();
            graphic.shape=mark;graphic.color=tint;graphic.SetEdge(edge);graphic.width=width;graphic.raycastTarget=false;
            return graphic;
        }

        public static Image Meter(Transform parent,string name,float x0,float y0,float x1,float y1,Color color)
        {
            var track=Image(parent,name+" Track",x0,y0,x1,y1,new Color(.11f,.19f,.25f));
            return Image(track.transform,name,0,0,1,1,color);
        }

        public static void Fill(Image image,float ratio)
            => image.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(ratio),1);
    }
}
