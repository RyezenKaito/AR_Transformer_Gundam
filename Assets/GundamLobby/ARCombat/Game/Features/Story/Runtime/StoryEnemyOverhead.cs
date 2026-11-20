using GundamGame.CombatDemo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GundamGame.Story.StoryHudUi;

namespace GundamGame.Story
{
    internal sealed class StoryEnemyOverhead
    {
        readonly DemoFighter fighter;
        readonly RectTransform head, lockFrame;
        readonly MechaPlateGraphic bars;
        readonly Image health, armor;
        readonly TMP_Text armorLabel;
        readonly HitNumber[] numbers=new HitNumber[6];
        int nextNumber;

        public StoryEnemyOverhead(Transform parent,DemoFighter fighter,Camera camera,TMP_FontAsset font)
        {
            this.fighter=fighter;
            head=Billboard(parent,fighter.displayName+" / Overhead vitals",camera,new Vector2(126,52));
            bars=Plate(head,"Enemy health and armor",0,0,1,1,Line,8);
            Text(bars.transform,"Enemy HP label","HP",font,.065f,.52f,.25f,.95f,16,White,bold:true);
            armorLabel=Text(bars.transform,"Enemy Armor label","ARM",font,.065f,.06f,.25f,.49f,15,Cyan,bold:true);
            health=Meter(bars.transform,"Enemy HP",.275f,.60f,.930f,.80f,Red);
            armor=Meter(bars.transform,"Enemy Armor",.275f,.21f,.930f,.38f,Cyan);
            for(int i=0;i<numbers.Length;i++)
            {
                var label=Text(head,"Floating Damage Text","",font,-.50f,1.06f,1.50f,1.86f,34,White,
                    TextAlignmentOptions.Center,true);
                label.outlineColor=new Color32(8,17,30,242);
                label.outlineWidth=.19f;
                label.gameObject.SetActive(false);
                numbers[i]=new HitNumber(label);
            }

            lockFrame=Billboard(parent,fighter.displayName+" / Target lock reticle",camera,new Vector2(153,165));
            Mark(lockFrame,"Target Lock Reticle",.02f,.02f,.98f,.98f,StoryHudGraphic.Mark.LockCorners,Red,Color.clear,3.8f);
            Mark(lockFrame,"Target lock pointer",.45f,-.065f,.55f,-.005f,StoryHudGraphic.Mark.Arrow,Red,Color.clear)
                .rectTransform.localEulerAngles=new Vector3(0,0,90);
            lockFrame.gameObject.SetActive(false);
            fighter.ImpactReceived+=OnImpact;
        }

        public void Release()
        {
            if(fighter)fighter.ImpactReceived-=OnImpact;
            if(head)Object.Destroy(head.gameObject);
            if(lockFrame)Object.Destroy(lockFrame.gameObject);
        }

        public void Refresh(Camera camera,bool visible,bool selected,float dt)
        {
            Vector3 foot=fighter.transform.position;
            Vector3 top=fighter.transform.TransformPoint(Vector3.up*fighter.height*1.01f);
            Vector3 above=fighter.transform.TransformPoint(Vector3.up*fighter.height*1.15f);
            bool inFront=visible && camera && camera.WorldToViewportPoint(above).z>camera.nearClipPlane;
            bool hasNumber=false;
            foreach(var number in numbers)
            {
                number.Refresh(dt);
                hasNumber|=number.Active;
            }
            head.gameObject.SetActive(inFront&&(fighter.IsAlive||hasNumber));
            lockFrame.gameObject.SetActive(inFront&&fighter.IsAlive&&selected);
            if(!inFront)return;
            float scale=camera.pixelHeight/1080f;
            Place(head,camera,above,scale);
            bars.gameObject.SetActive(fighter.IsAlive);
            bars.TintEdge(selected?Red:Line);
            Fill(health,fighter.Health/(float)Mathf.Max(1,fighter.maxHealth));
            Fill(armor,fighter.Armor/Mathf.Max(1,fighter.maxArmor));
            armorLabel.color=fighter.ArmorBroken?Red:Cyan;
            if(!selected || !fighter.IsAlive)return;
            Vector2 topPixel=camera.WorldToScreenPoint(top);
            Vector2 footPixel=camera.WorldToScreenPoint(foot);
            float height=Mathf.Clamp(Mathf.Abs(topPixel.y-footPixel.y)/Mathf.Max(.001f,scale),74,480);
            lockFrame.sizeDelta=new Vector2(height*.76f,height*1.06f);
            Place(lockFrame,camera,Vector3.Lerp(foot,top,.52f),scale);
        }

        void OnImpact(DemoFighter actor,int hp,bool broken)
        {
            if(hp>0)SpawnNumber("−"+hp,White,false);
            if(broken)SpawnNumber("ARMOR BREAK!",Amber,true);
        }

        void SpawnNumber(string value,Color color,bool special)
        {
            int slot=nextNumber++%numbers.Length;
            numbers[slot].Show(value,color,special,slot);
        }

        static RectTransform Billboard(Transform parent,string name,Camera camera,Vector2 size)
        {
            var node=new GameObject(name,typeof(RectTransform),typeof(Canvas));
            node.transform.SetParent(parent,false);
            var canvas=node.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.WorldSpace;
            canvas.worldCamera=camera;canvas.overrideSorting=true;canvas.sortingOrder=75;
            var rect=(RectTransform)node.transform;
            rect.sizeDelta=size;
            return rect;
        }

        static void Place(RectTransform billboard,Camera camera,Vector3 position,float referenceScale)
        {
            float depth=Vector3.Dot(position-camera.transform.position,camera.transform.forward);
            float height=camera.orthographic?camera.orthographicSize*2:2*depth*Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f);
            float unitsPerPixel=height/Mathf.Max(1,camera.pixelHeight);
            billboard.SetPositionAndRotation(position,camera.transform.rotation);
            billboard.localScale=Vector3.one*(unitsPerPixel*referenceScale);
        }

        sealed class HitNumber
        {
            readonly TMP_Text label;
            float time, duration, horizontal, baseline;
            Color color;
            public bool Active=>time>0;

            public HitNumber(TMP_Text label) { this.label=label; }

            public void Show(string value,Color tint,bool special,int slot)
            {
                label.text=value;color=tint;
                duration=time=special?1.30f:.95f;
                horizontal=special?0:((slot%3)-1)*32;
                baseline=special?40:0;
                label.rectTransform.localScale=Vector3.one*(special?1.10f:1);
                label.rectTransform.anchoredPosition=new Vector2(horizontal,baseline);
                label.color=tint;
                label.gameObject.SetActive(true);
            }

            public void Refresh(float dt)
            {
                if(!Active)return;
                time=Mathf.Max(0,time-dt);
                if(!Active){label.gameObject.SetActive(false);return;}
                float elapsed=1-time/duration;
                label.rectTransform.anchoredPosition=new Vector2(horizontal,baseline+64*Mathf.SmoothStep(0,1,elapsed));
                color.a=Mathf.Clamp01(time/.24f);label.color=color;
            }
        }
    }
}
