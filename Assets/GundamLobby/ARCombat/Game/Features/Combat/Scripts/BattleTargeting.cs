using UnityEngine;

namespace GundamGame.CombatDemo
{
    internal static class BattleTargeting
    {
        public static int Pick(Camera camera,Vector2 screenPoint,DemoFighter[] enemies)
        {
            if(!camera || !camera.pixelRect.Contains(screenPoint))return -1;
            Ray ray=camera.ScreenPointToRay(screenPoint);
            int selected=-1;
            float nearest=float.PositiveInfinity;
            for(int i=0;i<enemies.Length;i++)
            {
                var enemy=enemies[i];
                if(!enemy || !enemy.IsAlive || !enemy.gameObject.activeInHierarchy)continue;
                float height=Mathf.Max(.1f,enemy.height);
                var body=new Bounds(Vector3.up*height*.55f,new Vector3(height*.8f,height*1.15f,height*.8f));
                var localRay=new Ray(enemy.transform.InverseTransformPoint(ray.origin),enemy.transform.InverseTransformVector(ray.direction));
                if(!body.IntersectRay(localRay,out float distance))continue;
                float depth=(enemy.transform.TransformPoint(localRay.GetPoint(distance))-ray.origin).sqrMagnitude;
                if(depth>=nearest)continue;
                nearest=depth;
                selected=i;
            }
            return selected;
        }
    }
}
