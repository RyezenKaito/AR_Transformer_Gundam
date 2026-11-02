using UnityEngine;

namespace SunkenHarbor
{
 public sealed class HarborLife : MonoBehaviour
 {
  public Transform[] fish;
  public Vector3 center;
  public float radius=15,speed=.12f;
  void Update()
  {
   if(fish==null)return;
   for(int i=0;i<fish.Length;i++)if(fish[i]){float a=Time.time*speed+i*.16f;Vector3 position=center+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a*1.7f+i)*.7f,Mathf.Sin(a)*radius*.45f);fish[i].localPosition=position;fish[i].localRotation=Quaternion.LookRotation(new Vector3(-Mathf.Sin(a),0,Mathf.Cos(a)*.45f));}
  }
 }
}
