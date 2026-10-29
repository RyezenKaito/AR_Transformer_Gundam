using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using static BlueFlame.Editor.BFG;

namespace BlueFlame.Editor
{
    public static partial class BlueFlameBuilder
    {
        static Transform Energy(Transform model)
        {
            var root=G("12 / Optional blue flame energy",model);
            var v=new List<Vector3>();var uv=new List<Vector2>();var phase=new List<Vector2>();var ids=new List<int>();
            var random=new System.Random(301);
            System.Action<Vector3,float,float> plume=(origin,height,width)=>
            {
                float seed=(float)random.NextDouble(),seed2=(float)random.NextDouble();
                for(int plane=0;plane<2;plane++)
                {
                    int start=v.Count;Vector3 across=plane==0?Vector3.right:Vector3.forward;
                    for(int i=0;i<=14;i++)
                    {
                        float t=i/14f;float w=width*Mathf.Pow(1-t,.8f)*(.6f+.4f*Mathf.Sin(t*Mathf.PI));
                        Vector3 c=origin+V(Mathf.Sin(t*6+seed*4)*t*height*.16f,t*height,Mathf.Sin(t*4+seed2*5)*t*height*.08f);
                        v.Add(c-across*w);v.Add(c+across*w);uv.Add(new Vector2(0,t));uv.Add(new Vector2(1,t));phase.Add(new Vector2(seed,seed2));phase.Add(new Vector2(seed,seed2));
                        if(i<14){int k=start+i*2;ids.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}
                    }
                }
            };
            var flight=Child(model,"09 /");
            for(int s=-1;s<=1;s+=2)
            {
                var wing=Child(flight,s<0?"L wing":"R wing");
                for(int i=0;i<28;i++)
                {
                    float x=1.10f+i*.16f+((float)random.NextDouble()-.5f)*.075f;
                    float y=x<1.48f?Mathf.Lerp(.99f,1.08f,(x-1.10f)/.38f):x<2.05f?Mathf.Lerp(1.08f,.89f,(x-1.48f)/.57f):x<2.89f?Mathf.Lerp(.89f,.30f,(x-2.05f)/.84f):Mathf.Lerp(.30f,-1.03f,(x-2.89f)/2.94f);
                    Vector3 origin=root.InverseTransformPoint(wing.TransformPoint(V(s*x,y,.10f)));
                    plume(origin,.18f+(float)random.NextDouble()*.65f,.13f+(float)random.NextDouble()*.09f);
                }
            }
            var mesh=new Mesh{name="BF_BlueFlame_energy_ribbons",indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetUVs(1,phase);mesh.SetTriangles(ids,0);mesh.RecalculateBounds();Meshes.Add(mesh);
            string path=Root+"/Materials/14_Procedural_BlueFlame.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("BlueFlame/ProceduralEnergy"));AssetDatabase.CreateAsset(mat,path);}mat.SetFloat("_Intensity",2.2f);EditorUtility.SetDirty(mat);
            var renderer=BFG.Object("Wing energy tongues",root,mesh,mat).GetComponent<MeshRenderer>();renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            // Keep the inspectable kit visible by default. F enables the alternate effect presentation.
            root.gameObject.SetActive(false);return root;
        }
    }
}
