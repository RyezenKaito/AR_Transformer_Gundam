using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TurnBasedBattle;
using ProjectEnvironments;
using ProjectEnvironments.Editor;

namespace VolcanoArena.Editor
{
    public static class VolcanoBuilder
    {
        public const string Root="Assets/Environments/Volcano";
        public const string ScenePath=Root+"/Scenes/Volcano_Battle.unity";
        static VolcanoConfig config;
        static Material basalt,hotRock,lava,smokeMat,emberMat;
        static readonly List<Renderer> lavaSurfaces=new List<Renderer>();
        static readonly Vector3 ArenaCenter=new Vector3(4.85f,0,0);
        static Transform Group(string name,Transform parent=null){var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
        static float Noise(float x,float z)=>Mathf.PerlinNoise(x+82.14f,z+17.73f);
        static float Rand(float a,float b)=>UnityEngine.Random.Range(a,b);
        static Mesh SaveMesh(Mesh source,string name)
        {
            string path=Root+"/Meshes/"+name+".asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!m){source.name=name;AssetDatabase.CreateAsset(source,path);return source;}
            m.Clear();m.indexFormat=source.indexFormat;m.vertices=source.vertices;m.normals=source.normals;m.uv=source.uv;m.triangles=source.triangles;m.RecalculateBounds();m.UploadMeshData(false);EditorUtility.SetDirty(m);UnityEngine.Object.DestroyImmediate(source);return m;
        }
        static Mesh Mesh(string name,List<Vector3> v,List<Vector2> uv,List<int> indices)
        {
            if(v.Any(p=>float.IsNaN(p.x)||float.IsNaN(p.y)||float.IsNaN(p.z)||float.IsInfinity(p.x)||float.IsInfinity(p.y)||float.IsInfinity(p.z)))throw new Exception("Non-finite vertex in "+name);
            var m=new Mesh{indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(indices,0);m.RecalculateNormals();m.RecalculateBounds();return SaveMesh(m,name);
        }
        static GameObject Model(string name,Transform parent,Mesh mesh,Material material,Vector3 position,bool collision=false)
        {
            var g=Group(name,parent).gameObject;g.transform.localPosition=position;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;
            if(collision){g.layer=2;g.AddComponent<MeshCollider>().sharedMesh=mesh;}return g;
        }
        static Material Mat(string name,string shader)
        {
            var path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);var s=Shader.Find(shader);if(!s)throw new Exception("Missing shader "+shader);
            if(!m){m=new Material(s);AssetDatabase.CreateAsset(m,path);}else m.shader=s;m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
        }
        static void Materials()
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/Basalt_Albedo.png");importer.wrapMode=TextureWrapMode.Repeat;importer.mipmapEnabled=true;importer.anisoLevel=8;importer.maxTextureSize=2048;importer.SaveAndReimport();var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Basalt_Albedo.png");
            basalt=Mat("Basalt cooled rock","Volcano/URP Basalt");basalt.SetTexture("_BaseMap",texture);basalt.SetColor("_BaseColor",new Color(1.3f,1.28f,1.24f));basalt.SetFloat("_Tiling",.12f);basalt.SetFloat("_Smoothness",.12f);basalt.SetFloat("_Relief",.18f);basalt.SetFloat("_Heat",0);
            hotRock=Mat("Basalt thermal fractures","Volcano/URP Basalt");hotRock.CopyPropertiesFromMaterial(basalt);hotRock.SetFloat("_Heat",.12f);
            lava=Mat("Molten lava with drifting crust","Volcano/URP Flowing Lava");lava.SetTexture("_CrustMap",texture);lava.SetFloat("_Emission",config.lavaEmission);lava.SetFloat("_Crust",config.crustCoverage);lava.SetFloat("_Speed",config.flowSpeed);
            var puff=PuffTexture();smokeMat=Mat("Volcanic ash smoke","Universal Render Pipeline/Particles/Unlit");smokeMat.SetTexture("_BaseMap",puff);smokeMat.SetColor("_BaseColor",Color.white);smokeMat.SetFloat("_Surface",1);smokeMat.SetFloat("_Blend",0);smokeMat.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);smokeMat.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);smokeMat.SetFloat("_ZWrite",0);smokeMat.SetFloat("_Cull",0);smokeMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");smokeMat.renderQueue=3000;smokeMat.SetShaderPassEnabled("ShadowCaster",false);
            emberMat=Mat("Lava ember glow","Universal Render Pipeline/Particles/Unlit");emberMat.CopyPropertiesFromMaterial(smokeMat);emberMat.SetFloat("_DstBlend",(float)BlendMode.One);emberMat.SetFloat("_Blend",2);emberMat.SetColor("_BaseColor",new Color(4,1,.05f,1));
        }
        static Texture2D PuffTexture()
        {
            string path=Root+"/Textures/SmokeDensity.asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(texture)return texture;
            int n=128;texture=new Texture2D(n,n,TextureFormat.RGBA32,true);var colors=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){float a=(x+.5f)/n*2-1,b=(y+.5f)/n*2-1;float radial=Mathf.Clamp01(1-Mathf.Sqrt(a*a+b*b));float density=.5f+.5f*Noise(x*.045f,y*.045f);colors[y*n+x]=new Color(1,1,1,radial*radial*density);}
            texture.SetPixels(colors);texture.Apply();texture.wrapMode=TextureWrapMode.Clamp;AssetDatabase.CreateAsset(texture,path);return texture;
        }
        static Mesh Platform()
        {
            const int slices=160,rings=32;var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
            for(int j=0;j<=rings+10;j++)for(int i=0;i<=slices;i++)
            {
                float angle=i*2*Mathf.PI/slices,radial=Mathf.Min(j,rings)/(float)rings;
                float edge=1+.055f*Mathf.Sin(angle*5+.4f)+.035f*Mathf.Sin(angle*11+1.2f);
                float x=Mathf.Cos(angle)*config.arenaSize.x*.5f*edge*radial,z=Mathf.Sin(angle)*config.arenaSize.y*.5f*edge*radial;
                float y=(Noise(x*.3f,z*.3f)-.5f)*.025f*Mathf.SmoothStep(0,1,radial);
                if(j>rings){float layer=(j-rings)/10f;y=-layer*9;float outcrop=1+Mathf.Sin(layer*24+angle*7)*.026f+.03f*Noise(angle*4,layer*6);x*=outcrop;z*=outcrop;}
                v.Add(new Vector3(x,y,z));uv.Add(new Vector2(x,z)*.15f);
            }
            for(int j=0;j<rings+10;j++)for(int i=0;i<slices;i++){int a=j*(slices+1)+i,b=a+slices+1;t.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
            return Mesh("Basalt arena continuous slab",v,uv,t);
        }
        static Mesh Rock(int index)
        {
            const int slices=48,rings=24;var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();float seed=index*17.3f;
            for(int j=0;j<=rings;j++)for(int i=0;i<=slices;i++)
            {
                float a=i*2*Mathf.PI/slices,y=j/(float)rings;float shape=Mathf.Pow(Mathf.Max(0,Mathf.Sin(y*Mathf.PI)),.23f);float radius=shape*(.7f+.3f*Noise(Mathf.Cos(a)*3+seed,Mathf.Sin(a)*3+seed))*(1+.055f*Mathf.Sin(j*.93f+a*4+seed));
                v.Add(new Vector3(Mathf.Cos(a)*radius*.5f,(y-.5f)*1.2f,Mathf.Sin(a)*radius*.5f));uv.Add(new Vector2(i/(float)slices,y));
            }
            for(int j=0;j<rings;j++)for(int i=0;i<slices;i++){int a=j*(slices+1)+i,b=a+slices+1;t.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
            return Mesh("Fractured basalt "+index,v,uv,t);
        }
        static Mesh VolcanoMountain()
        {
            float[] radii={94,78,60,47,36,26,18,12,8,0};float[] heights={-3,4,15,28,42,57,69,80,72,68};int slices=180,rings=54;var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
            for(int j=0;j<=rings;j++)for(int i=0;i<=slices;i++)
            {
                float phase=j/(float)rings*9;int k=Mathf.Min(8,(int)phase);float f=phase-k;
                float r=Mathf.Lerp(radii[k],radii[k+1],f),baseY=Mathf.Lerp(heights[k],heights[k+1],f);
                float a=i*2*Mathf.PI/slices;float modulation=1+.12f*Mathf.Sin(a*3)+.07f*Mathf.Sin(a*7+1)+.04f*Mathf.Sin(a*17);
                float radialNoise=(Noise(Mathf.Cos(a)*r*.065f,Mathf.Sin(a)*r*.065f)-.5f)*5;
                float radius=r*modulation+radialNoise*Mathf.Sin(j/(float)rings*Mathf.PI);float y=baseY+(j==rings?0:(Noise(Mathf.Cos(a)*8+phase*.7f,Mathf.Sin(a)*8+phase*.7f)-.5f)*7);
                y+=Mathf.Sin(a*28+phase*1.1f)*1.2f*Mathf.Sin(j/(float)rings*Mathf.PI);
                v.Add(new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius));uv.Add(new Vector2(i/(float)slices,j*.4f));
            }
            for(int j=0;j<rings;j++)for(int i=0;i<slices;i++){int a=j*(slices+1)+i,b=a+slices+1;t.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
            return Mesh("Main volcano eroded crater",v,uv,t);
        }
        static Mesh Lake(string name,float rx,float rz)
        {
            int slices=128,rings=16;var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
            for(int j=0;j<=rings;j++)for(int i=0;i<=slices;i++){float a=i*2*Mathf.PI/slices,r=j/(float)rings;float edge=1+.07f*Mathf.Sin(a*5)+.03f*Mathf.Sin(a*9);var p=new Vector3(Mathf.Cos(a)*rx*r*edge,0,Mathf.Sin(a)*rz*r*edge);v.Add(p);uv.Add(new Vector2(p.x*.13f,p.z*.16f));}
            for(int j=0;j<rings;j++)for(int i=0;i<slices;i++){int a=j*(slices+1)+i,b=a+slices+1;t.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
            return Mesh(name,v,uv,t);
        }
        static Vector3 Catmull(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t)=>.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);
        static Mesh Ribbon(string name,Vector3[] points,float width)
        {
            int steps=(points.Length-1)*28,across=12;var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();float distance=0;Vector3 previous=points[0];
            for(int j=0;j<=steps;j++)
            {
                float k=j/(float)steps*(points.Length-1);int n=Mathf.Min(points.Length-2,(int)k);float f=k-n;var a=points[Mathf.Max(0,n-1)];var b=points[n];var c=points[n+1];var d=points[Mathf.Min(points.Length-1,n+2)];var p=Catmull(a,b,c,d,f);var tangent=Catmull(a,b,c,d,Mathf.Min(1,f+.01f))-Catmull(a,b,c,d,Mathf.Max(0,f-.01f));var side=Vector3.Cross(Vector3.up,tangent).normalized;if(side.sqrMagnitude<.1f)side=Vector3.forward;distance+=Vector3.Distance(previous,p);previous=p;
                float w=width*(.85f+.15f*Mathf.Sin(j*.09f));
                for(int i=0;i<=across;i++){float u=i/(float)across;var pos=p+side*(u-.5f)*w+Vector3.up*(.11f*Mathf.Sin(j*.7f+i*.6f));v.Add(pos);uv.Add(new Vector2(u,distance*.23f));}
            }
            for(int j=0;j<steps;j++)for(int i=0;i<across;i++){int a=j*(across+1)+i,b=a+across+1;t.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
            return Mesh(name,v,uv,t);
        }
        static GameObject LavaModel(string name,Transform root,Mesh mesh,Vector3 position)
        {var go=Model(name,root,mesh,lava,position);var r=go.GetComponent<Renderer>();r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;lavaSurfaces.Add(r);return go;}
        // Project every ribbon vertex onto its host mesh so lava follows the rock instead of floating above it.
        static void SurfaceLava(string name,Transform parent,GameObject host,float angle,float width,float startRadius=0,bool mainVolcano=false)
        {
            var collider=host.GetComponent<MeshCollider>();if(!collider)return;Physics.SyncTransforms();var bounds=collider.bounds;
            var center=host.transform.position;var direction=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var side=Vector3.Cross(Vector3.up,direction).normalized;
            float reach=1/Mathf.Sqrt(Mathf.Pow(direction.x/Mathf.Max(1,bounds.extents.x),2)+Mathf.Pow(direction.z/Mathf.Max(1,bounds.extents.z),2));
            const int steps=42,across=8;var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();Vector3 previous=Vector3.zero;float length=0;
            float floor=mainVolcano||Vector3.Distance(center,ArenaCenter)>120?-13.5f:config.lavaHeight+.03f;
            for(int j=0;j<=steps;j++)
            {
                float r=Mathf.Lerp(startRadius,reach*1.08f,j/(float)steps);var sample=center+direction*r+side*(Mathf.Sin(j*.19f+angle)*width*.22f*j/steps);
                var ray=new Ray(new Vector3(sample.x,bounds.max.y+10,sample.z),Vector3.down);
                bool hit=collider.Raycast(ray,out var central,bounds.size.y+30);
                if(!hit&&j==0)return;
                Vector3 p=hit?central.point:new Vector3(sample.x,floor,sample.z);p.y=Mathf.Max(floor,p.y);
                if(j>0)length+=Vector3.Distance(previous,p);previous=p;
                float w=width*(.85f+.15f*Mathf.Sin(j*.17f+angle));
                for(int i=0;i<=across;i++)
                {
                    float u=i/(float)across;var q=sample+side*(u-.5f)*w;var edgeRay=new Ray(new Vector3(q.x,bounds.max.y+10,q.z),Vector3.down);
                    Vector3 pos;
                    if(collider.Raycast(edgeRay,out var edge,bounds.size.y+30))pos=edge.point+edge.normal*.045f+Vector3.up*.025f;
                    else pos=new Vector3(q.x,p.y+.05f,q.z);
                    pos.y=Mathf.Max(floor,pos.y);v.Add(pos);uv.Add(new Vector2(u,length*.23f));
                }
                if(!hit&&j>1)break;
            }
            int rows=v.Count/(across+1);if(rows<2)return;
            for(int j=0;j<rows-1;j++)for(int i=0;i<across;i++){int a=j*(across+1)+i,b=a+across+1;t.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
            LavaModel(name,parent,Mesh(name,v,uv,t),Vector3.zero);
        }
        static void Terrain(Transform environment)
        {
            var platform=Group("01_BasaltArena",environment);var slab=Model("Continuous solid basalt arena",platform,Platform(),basalt,ArenaCenter,true);slab.layer=0;
            var rocks=Enumerable.Range(0,6).Select(Rock).ToArray();
            for(int i=0;i<54;i++)
            {
                float a=i*2*Mathf.PI/54;float radius=Rand(.96f,1.06f);var pos=ArenaCenter+new Vector3(Mathf.Cos(a)*config.arenaSize.x*.5f*radius,Rand(-.7f,.1f),Mathf.Sin(a)*config.arenaSize.y*.5f*radius);var go=Model("Broken slab rim "+i,platform,rocks[i%6],i%3==0?hotRock:basalt,pos,true);go.transform.localScale=new Vector3(Rand(2,5),Rand(1.3f,3.7f),Rand(2,4));go.transform.localRotation=Quaternion.Euler(Rand(-15,15),Rand(0,360),Rand(-10,10));
            }
            var banks=Group("05_CliffsAndDistantMountains",environment);
            var mountainFlows=Group("Lava on surrounding peaks",banks);
            for(int i=0;i<84;i++)
            {
                float a=i*2*Mathf.PI/84;var pos=ArenaCenter+new Vector3(Mathf.Cos(a)*Rand(66,89),Rand(-6,-3),Mathf.Sin(a)*Rand(53,70));var go=Model("Lava shore outcrop "+i,banks,rocks[i%6],i%4==0?hotRock:basalt,pos,true);go.transform.localScale=new Vector3(Rand(11,23),Rand(9,24),Rand(12,24));go.transform.localRotation=Quaternion.Euler(Rand(-12,12),Rand(0,360),Rand(-12,12));
                if(i%10<Mathf.RoundToInt(config.surroundingLavaCoverage*10))
                {
                    float facing=Mathf.Atan2(ArenaCenter.z-pos.z,ArenaCenter.x-pos.x);
                    SurfaceLava("Shore lava cascade "+i,mountainFlows,go,facing,Rand(2.5f,4.5f));
                    if(i%3==0)SurfaceLava("Shore lava branch "+i,mountainFlows,go,facing+.65f,2);
                }
            }
            for(int i=0;i<34;i++)
            {
                float a=i*2*Mathf.PI/34;var pos=new Vector3(Mathf.Cos(a)*Rand(180,260),Rand(-23,-8),Mathf.Sin(a)*Rand(165,235));if(Vector3.Distance(pos,config.mainVolcanoPosition)<100)continue;
                var go=Model("Caldera ridge "+i,banks,rocks[i%6],basalt,pos,true);go.transform.localScale=new Vector3(Rand(65,115),Rand(58,135),Rand(65,120));go.transform.localRotation=Quaternion.Euler(0,Rand(0,360),Rand(-15,15));
                if(i%10<Mathf.RoundToInt(config.surroundingLavaCoverage*10))
                {
                    float facing=Mathf.Atan2(ArenaCenter.z-pos.z,ArenaCenter.x-pos.x);
                    SurfaceLava("Ridge lava cascade "+i,mountainFlows,go,facing,Rand(6,10));
                    SurfaceLava("Ridge lava branch "+i,mountainFlows,go,facing+.7f,Rand(4,7));
                }
            }
            var bed=Model("Caldera bed below lava",banks,Lake("Caldera foundation",390,370),basalt,new Vector3(0,-14,0));
            var rivers=Group("02_LavaRiversAndLake",environment);LavaModel("Molten lake surrounding basalt",rivers,Lake("Lava lake",83,66),ArenaCenter+Vector3.up*config.lavaHeight);
            foreach(int side in new[]{-1,1})
            {
                var seam=Enumerable.Range(0,5).Select(j=>{float a=(30+j*30)*Mathf.Deg2Rad;return ArenaCenter+new Vector3(Mathf.Cos(a)*config.arenaSize.x*.41f,.025f,side*Mathf.Sin(a)*config.arenaSize.y*.41f);}).ToArray();
                LavaModel("Shallow thermal seam at arena rim "+side,rivers,Ribbon("Peripheral hot fissure "+side,seam,.65f),Vector3.zero);
            }
            LavaModel("North lava inlet",rivers,Ribbon("Lava tributary north",new[]{new Vector3(-105,config.lavaHeight+1.5f,15),new Vector3(-78,config.lavaHeight+.8f,27),new Vector3(-47,config.lavaHeight+.03f,18)},12),Vector3.zero);
            LavaModel("West lava inlet",rivers,Ribbon("Lava tributary west",new[]{new Vector3(30,config.lavaHeight+1,-108),new Vector3(13,config.lavaHeight+.6f,-85),new Vector3(22,config.lavaHeight+.03f,-52)},14),Vector3.zero);
            var mountain=Group("04_MainVolcano",environment);var mainMountain=Model("Main volcano ridged crater",mountain,VolcanoMountain(),basalt,config.mainVolcanoPosition,true);
            LavaModel("Crater molten interior",mountain,Lake("Crater lava surface",8.5f,8.5f),config.mainVolcanoPosition+Vector3.up*70);
            Vector3 m=config.mainVolcanoPosition;
            for(int i=0;i<config.mainLavaStreams;i++)
            {
                float a=(85+i*360f/config.mainLavaStreams)*Mathf.Deg2Rad;
                SurfaceLava("Volcano lava descent "+i,mountain,mainMountain,a,8+i%3*2.2f,12,true);
            }
            LavaModel("Main river from volcano",rivers,Ribbon("Source to lake channel",new[]{m+new Vector3(-102,3.8f,-20),new Vector3(61,config.lavaHeight+.5f,27),new Vector3(48,config.lavaHeight+.03f,18)},14),Vector3.zero);
            var falls=Group("03_LavaFalls",environment);
            for(int i=0;i<2;i++)
            {
                Vector3 pos=i==0?new Vector3(28,-3,49):new Vector3(-39,-4,-43);var r=Model("Lavafall basalt shelf "+i,falls,rocks[3+i],hotRock,pos,true);r.transform.localScale=new Vector3(18,24,16);
                LavaModel("Lavafall "+i,falls,Ribbon("Lava fall mesh "+i,new[]{pos+new Vector3(-5,13,2),pos+new Vector3(0,12,0),pos+new Vector3(5,8,-3),new Vector3(pos.x+6,config.lavaHeight+.03f,pos.z-10)},7),Vector3.zero);
            }
        }
        static ParticleSystem Particles(string name,Transform root,Vector3 pos,Material material,Color start,float size,float life,float speed,float rate,int max)
        {
            var p=Group(name,root).gameObject.AddComponent<ParticleSystem>();p.transform.localPosition=pos;p.transform.localRotation=Quaternion.Euler(-90,0,0);p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=p.main;main.loop=true;main.duration=12;main.startColor=start;main.startSize=new ParticleSystem.MinMaxCurve(size*.6f,size*1.4f);main.startLifetime=new ParticleSystem.MinMaxCurve(life*.7f,life*1.3f);main.startSpeed=new ParticleSystem.MinMaxCurve(speed*.7f,speed*1.3f);main.maxParticles=max;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
            var emission=p.emission;emission.rateOverTime=rate;var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=18;shape.radius=size*.3f;
            var renderer=p.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.sortMode=ParticleSystemSortMode.Distance;
            var col=p.colorOverLifetime;col.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(start,0),new GradientColorKey(start,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(start.a,.12f),new GradientAlphaKey(start.a*.7f,.65f),new GradientAlphaKey(0,1)});col.color=gradient;
            var sz=p.sizeOverLifetime;sz.enabled=true;sz.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.65f),new Keyframe(1,2)));
            var velocity=p.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=.7f;velocity.y=0;velocity.z=.25f;
            p.useAutoRandomSeed=false;p.randomSeed=(uint)UnityEngine.Random.Range(1,99999);return p;
        }
        static void AddLavaVapour(Transform effects,VolcanoAtmosphere controller)
        {
            var old=effects.Find("Lava surface wisps");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var wisps=Group("Lava surface wisps",effects);var material=Mat("Thin rising lava vapour","Volcano/URP Thin Lava Vapour");material.SetTexture("_BaseMap",PuffTexture());material.SetFloat("_Opacity",config.lavaVapourOpacity);
            var emitters=new List<ParticleSystem>();
            var positions=new List<Vector3>();
            for(int i=0;i<40;i++)
            {
                float a=i*Mathf.PI*2/40;float margin=3+1.5f*Mathf.Sin(i*2.4f);
                positions.Add(ArenaCenter+new Vector3(Mathf.Cos(a)*(config.arenaSize.x*.5f+margin),config.lavaHeight+.25f,Mathf.Sin(a)*(config.arenaSize.y*.5f+margin)));
            }
            int index=0;foreach(var r in controller.lavaSurfaces.Where(r=>r.name.StartsWith("Shore lava cascade")))
            {
                if(index++%9!=0)continue;var vertices=r.GetComponent<MeshFilter>().sharedMesh.vertices;
                var low=vertices.OrderBy(p=>p.y).First();var pos=r.transform.TransformPoint(low);pos.y=config.lavaHeight+.35f;positions.Add(pos);
            }
            positions.Add(new Vector3(34,config.lavaHeight+.3f,39));positions.Add(new Vector3(-33,config.lavaHeight+.3f,-53));
            foreach(var pos in positions)
            {
                int i=emitters.Count;var p=Particles("Lava vapour wisp "+i,wisps,pos,material,new Color(.52f,.5f,.48f,1),5.2f,9,.6f,5.4f,80);
                var main=p.main;main.prewarm=true;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;main.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
                var shape=p.shape;shape.radius=2.6f;shape.angle=10;
                var wind=p.velocityOverLifetime;wind.x=.28f;wind.z=.12f;wind.y=.06f;
                var noise=p.noise;noise.enabled=true;noise.strength=.18f;noise.frequency=.16f;noise.scrollSpeed=.12f;
                var growth=p.sizeOverLifetime;growth.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.55f),new Keyframe(.55f,1.15f),new Keyframe(1,1.6f)));
                var col=p.colorOverLifetime;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.65f,.2f),new GradientAlphaKey(.38f,.55f),new GradientAlphaKey(0,1)});col.color=gradient;
                emitters.Add(p);
            }
            controller.lavaVapour=emitters.ToArray();
        }
        static void ConfigureAirSmoke(ParticleSystem p,float size,float lifetime,float speed,int capacity,bool sky=false)
        {
            p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=p.main;main.startSize=new ParticleSystem.MinMaxCurve(size*.75f,size*1.3f);main.startLifetime=new ParticleSystem.MinMaxCurve(lifetime*.8f,lifetime*1.2f);main.startSpeed=new ParticleSystem.MinMaxCurve(speed*.8f,speed*1.2f);main.maxParticles=capacity;main.duration=lifetime*1.2f;main.prewarm=true;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
            var shape=p.shape;shape.angle=sky?35:10;shape.radius=sky?12:size*.2f;
            var wind=p.velocityOverLifetime;wind.x=1.3f;wind.z=.35f;wind.y=sky?.03f:0;
            var turbulence=p.noise;turbulence.enabled=true;turbulence.strength=sky?1:.5f;turbulence.frequency=sky?.035f:.08f;turbulence.scrollSpeed=.12f;
            var col=p.colorOverLifetime;var gradient=new Gradient();float alpha=sky?.36f:.85f;gradient.SetKeys(new[]{new GradientColorKey(new Color(.65f,.63f,.61f),0),new GradientColorKey(Color.white,.7f),new GradientColorKey(new Color(1.15f,1.12f,1.08f),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(alpha,.15f),new GradientAlphaKey(alpha*.7f,.68f),new GradientAlphaKey(0,1)});col.color=gradient;
            var growth=p.sizeOverLifetime;growth.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.6f),new Keyframe(.5f,1.15f),new Keyframe(1,1.8f)));
        }
        static void AddMountainAndSkySmoke(Transform environment,VolcanoAtmosphere controller)
        {
            var effects=environment.Find("06_SmokeAshAndHeat");var old=effects.Find("Mountain plumes and high ash");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);var root=Group("Mountain plumes and high ash",effects);
            var material=Mat("Mountain and sky ash smoke","Volcano/URP Thin Lava Vapour");material.SetTexture("_BaseMap",PuffTexture());material.SetFloat("_Opacity",.95f);
            var plumes=new List<ParticleSystem>();int shore=0,ridge=0;
            foreach(var host in environment.Find("05_CliffsAndDistantMountains").GetComponentsInChildren<MeshCollider>())
            {
                bool distant=host.name.StartsWith("Caldera ridge");if(!distant&&!host.name.StartsWith("Lava shore outcrop"))continue;
                if(distant){if(ridge++%3!=0)continue;}else if(shore++%8!=0)continue;
                Vector3 top=host.sharedMesh.vertices.Select(v=>host.transform.TransformPoint(v)).OrderByDescending(v=>v.y).First()+Vector3.up*.3f;
                var p=Particles((distant?"Distant mountain ash plume ":"Shore summit smoke ")+plumes.Count,root,top,material,new Color(.16f,.155f,.15f,1),distant?11:4.8f,distant?26:19,distant?2.8f:1.7f,3,110);
                ConfigureAirSmoke(p,distant?11:4.8f,distant?26:19,distant?2.8f:1.7f,110);plumes.Add(p);
            }
            controller.mountainPlumes=plumes.ToArray();var clouds=new List<ParticleSystem>();
            for(int i=0;i<10;i++)
            {
                float a=i*Mathf.PI*2/10;var pos=ArenaCenter+new Vector3(Mathf.Cos(a)*(165+i%3*15),78+i%4*12,Mathf.Sin(a)*(150+i%3*17));
                var p=Particles("High drifting ash bank "+i,root,pos,material,new Color(.3f,.29f,.28f,1),42,38,.06f,.34f,24);ConfigureAirSmoke(p,42,38,.06f,24,true);clouds.Add(p);
            }
            controller.skySmoke=clouds.ToArray();
            if(controller.smoke)
            {
                controller.smoke.GetComponent<ParticleSystemRenderer>().sharedMaterial=material;var main=controller.smoke.main;main.startColor=new Color(.1f,.095f,.09f,1);ConfigureAirSmoke(controller.smoke,28,34,4.2f,420);
            }
            var sky=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Ash cloud sky.mat");sky.SetFloat("_AshCoverage",config.skyAshCoverage);EditorUtility.SetDirty(sky);
        }
        static void AddAirReviewCameras(Transform environment)
        {
            var root=environment.Find("09_ReviewCameras");string[] names={"Volcano_MountainSky","Volcano_SkyCanopy"};Vector3[] positions={new Vector3(-30,25,-42),ArenaCenter+Vector3.up*3};Vector3[] targets={new Vector3(110,60,-15),ArenaCenter+new Vector3(0,100,15)};
            for(int i=0;i<2;i++)if(!root.Find(names[i]))
            {
                var c=Group(names[i],root).gameObject.AddComponent<Camera>();c.transform.position=positions[i];c.transform.LookAt(targets[i]);c.fieldOfView=i==0?62:68;c.farClipPlane=600;c.clearFlags=CameraClearFlags.Skybox;c.useOcclusionCulling=false;c.enabled=false;var data=c.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.requiresColorTexture=true;data.requiresDepthTexture=true;
            }
        }
        [MenuItem("Tools/Volcano/Update Mountain And Sky Smoke")]
        public static void UpdateMountainAndSkySmoke()
        {
            if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");var scene=SceneManager.GetActiveScene();if(scene.path!=ScenePath)throw new Exception("Open Volcano_Battle first.");if(scene.isDirty)EditorSceneManager.SaveScene(scene);Backup();
            config=AssetDatabase.LoadAssetAtPath<VolcanoConfig>(Root+"/Settings/VolcanoConfig.asset");var environment=GameObject.Find("Volcano_Battlefield").transform;var controller=environment.GetComponent<VolcanoAtmosphere>();var state=UnityEngine.Random.state;
            try{UnityEngine.Random.InitState(config.seed+1011);AddMountainAndSkySmoke(environment,controller);}finally{UnityEngine.Random.state=state;}
            foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>())c.GetUniversalAdditionalCameraData().requiresDepthTexture=true;
            AddAirReviewCameras(environment);
            controller.ApplySettings();EditorUtility.SetDirty(controller);PrefabUtility.SaveAsPrefabAsset(environment.gameObject,Root+"/Prefabs/Volcano_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("VOLCANO_MOUNTAIN_SKY_SMOKE_READY: "+controller.mountainPlumes.Length+" mountain plumes, "+controller.skySmoke.Length+" high ash banks");
        }
        [MenuItem("Tools/Volcano/Update Flow And Mist")]
        public static void UpdateFlowAndMist()
        {
            if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");var scene=SceneManager.GetActiveScene();if(scene.path!=ScenePath)throw new Exception("Open Volcano_Battle first.");
            if(scene.isDirty)EditorSceneManager.SaveScene(scene);Backup();config=AssetDatabase.LoadAssetAtPath<VolcanoConfig>(Root+"/Settings/VolcanoConfig.asset");
            var environment=GameObject.Find("Volcano_Battlefield").transform;var controller=environment.GetComponent<VolcanoAtmosphere>();var state=UnityEngine.Random.state;
            try{UnityEngine.Random.InitState(config.seed+510);AddLavaVapour(environment.Find("06_SmokeAshAndHeat"),controller);}finally{UnityEngine.Random.state=state;}
            var lavaMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Molten lava with drifting crust.mat");lavaMaterial.SetFloat("_Speed",config.flowSpeed);lavaMaterial.SetFloat("_Motion",config.flowWaves);EditorUtility.SetDirty(lavaMaterial);
            foreach(var camera in UnityEngine.Object.FindObjectsByType<Camera>())camera.GetUniversalAdditionalCameraData().requiresDepthTexture=true;
            if(!environment.Find("09_ReviewCameras/Volcano_FlowMist"))
            {
                var c=Group("Volcano_FlowMist",environment.Find("09_ReviewCameras")).gameObject.AddComponent<Camera>();c.transform.position=new Vector3(-15,3,-29);c.transform.LookAt(new Vector3(12,.6f,-28));c.fieldOfView=58;c.farClipPlane=600;c.clearFlags=CameraClearFlags.Skybox;c.useOcclusionCulling=false;c.enabled=false;var data=c.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.requiresColorTexture=true;data.requiresDepthTexture=true;
            }
            controller.ApplySettings();EditorUtility.SetDirty(controller);PrefabUtility.SaveAsPrefabAsset(environment.gameObject,Root+"/Prefabs/Volcano_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("VOLCANO_FLOW_MIST_READY: "+controller.lavaVapour.Length+" soft emitters; slopes 2.2x lake speed");
        }
        static void Effects(Transform environment)
        {
            var root=Group("06_SmokeAshAndHeat",environment);var controller=environment.gameObject.AddComponent<VolcanoAtmosphere>();controller.config=config;controller.lavaSurfaces=lavaSurfaces.ToArray();
            var smoke=Particles("Main volcanic ash plume",root,config.mainVolcanoPosition+Vector3.up*79,smokeMat,new Color(.46f,.44f,.42f,.9f),18,26,4.2f,4.8f,170);controller.smoke=smoke;
            var velocity=smoke.velocityOverLifetime;velocity.x=1.8f;velocity.z=.5f;
            var fountain=Particles("Crater incandescent ejecta",root,config.mainVolcanoPosition+Vector3.up*79,emberMat,new Color(1,.24f,.015f,1),.3f,4,15,8,160);var main=fountain.main;main.gravityModifier=.65f;var shape=fountain.shape;shape.angle=27;shape.radius=5;controller.eruption=fountain;
            controller.ash=Particles("Sparse drifting ash",root,new Vector3(4,12,0),smokeMat,new Color(.65f,.6f,.55f,.42f),.035f,9,.3f,26,500);shape=controller.ash.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(55,30,4);main=controller.ash.main;main.gravityModifier=.025f;
            for(int i=0;i<7;i++)
            {
                float a=i*Mathf.PI*2/7;var pos=ArenaCenter+new Vector3(Mathf.Cos(a)*(config.arenaSize.x*.5f+3),config.lavaHeight+.3f,Mathf.Sin(a)*(config.arenaSize.y*.5f+3));
                Particles("Thermal vent "+i,root,pos,smokeMat,new Color(.24f,.23f,.23f,.13f),1.4f,5,1.2f,2,35);
                var p=Particles("Rising embers "+i,root,pos,emberMat,new Color(1,.25f,.025f,.7f),.035f,4,1.5f,3,25);shape=p.shape;shape.radius=3;
            }
            AddLavaVapour(root,controller);
            AddMountainAndSkySmoke(environment,controller);
            var haze=Mat("Local lava heat refraction","Volcano/URP Local Heat Haze");
            foreach(int side in new[]{-1,1})
            {
                var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name="Local heat shimmer "+side;g.transform.SetParent(root,false);g.transform.localPosition=ArenaCenter+new Vector3(5,config.lavaHeight+2,side*(config.arenaSize.y*.5f+5));g.transform.localScale=new Vector3(22,7,1);g.GetComponent<Renderer>().sharedMaterial=haze;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
            }
            controller.lavaLights=environment.GetComponentsInChildren<Light>().Where(l=>l.type==LightType.Point).ToArray();
            var audio=Group("08_Ambience",environment).gameObject.AddComponent<AudioSource>();audio.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/VolcanicRumble.wav");audio.loop=true;audio.playOnAwake=true;audio.spatialBlend=0;audio.volume=config.ambienceVolume;controller.ambience=audio;controller.ApplySettings();
            foreach(var p in environment.GetComponentsInChildren<ParticleSystem>())p.Play();
        }
        static void Lighting(Transform environment)
        {
            var root=Group("07_LightingAndAtmosphere",environment);var sun=Group("Cool ash-filtered sun",root).gameObject.AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=2.1f;sun.color=new Color(.88f,.92f,1);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(48,110,0);RenderSettings.sun=sun;
            for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f+.6f;var light=Group("Lava bounce light "+i,root).gameObject.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.23f,.035f);light.intensity=1.6f;light.range=32;light.shadows=LightShadows.None;light.transform.localPosition=ArenaCenter+new Vector3(Mathf.Cos(a)*24,-1.5f,Mathf.Sin(a)*19);}
            var sky=Mat("Ash cloud sky","Volcano/Ash Sky");RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.4f,.47f,.6f);RenderSettings.ambientEquatorColor=new Color(.28f,.29f,.33f);RenderSettings.ambientGroundColor=new Color(.23f,.12f,.08f);RenderSettings.ambientIntensity=1;RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.28f,.27f,.29f);RenderSettings.fogDensity=.003f;
            string path=Root+"/Lighting/VolcanoVolume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);AssetDatabase.AddObjectToAsset(tone,profile);var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.28f);bloom.threshold.Override(1.15f);AssetDatabase.AddObjectToAsset(bloom,profile);}
            var volume=Group("Volcano global volume",root).gameObject.AddComponent<Volume>();volume.isGlobal=true;volume.priority=10;volume.sharedProfile=profile;
            var probe=Group("Basalt and lava reflection",root).gameObject.AddComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.IndividualFaces;probe.resolution=128;probe.size=new Vector3(70,35,60);probe.transform.localPosition=ArenaCenter+Vector3.up*4;probe.intensity=.65f;
        }
        static void Backup()
        {
            if(!File.Exists(ScenePath))return;string destination="Backups/Volcano/"+DateTime.Now.ToString("yyyyMMdd_HHmmss");
            foreach(string file in Directory.GetFiles(Root,"*",SearchOption.AllDirectories)){string target=destination+file.Substring(Root.Length);Directory.CreateDirectory(Path.GetDirectoryName(target));File.Copy(file,target,true);}
        }
        [MenuItem("Tools/Volcano/Build Battlefield")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");if(SceneManager.GetActiveScene().isDirty)throw new Exception("Save current scene before building.");
            foreach(string dir in new[]{"Scenes","Meshes","Materials","Textures","Prefabs","VFX","Lighting","Settings","Audio","Documentation/Review"})Directory.CreateDirectory(Root+"/"+dir);AssetDatabase.Refresh();Backup();
            string settings=Root+"/Settings/VolcanoConfig.asset";config=AssetDatabase.LoadAssetAtPath<VolcanoConfig>(settings);if(!config){config=ScriptableObject.CreateInstance<VolcanoConfig>();AssetDatabase.CreateAsset(config,settings);}
            var randomState=UnityEngine.Random.state;
            try
            {
                UnityEngine.Random.InitState(config.seed);lavaSurfaces.Clear();Materials();if(SceneManager.GetActiveScene().path==ScenePath)EditorSceneManager.OpenScene(EnvironmentTools.Hub);
                File.Copy("Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity",ScenePath,true);AssetDatabase.ImportAsset(ScenePath,ImportAssetOptions.ForceSynchronousImport);var scene=EditorSceneManager.OpenScene(ScenePath);var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var board=battle.player.transform.parent;
                foreach(string name in new[]{"MegaKitHangar","SphereCityArena"}){var go=board.Find(name);if(go)go.gameObject.SetActive(false);}var stage=board.Find("BattleArena");if(stage){foreach(var r in stage.GetComponentsInChildren<Renderer>(true))r.enabled=false;foreach(var c in stage.GetComponentsInChildren<Collider>(true))c.enabled=false;}
                foreach(string name in new[]{"Key Light","Fill Light","Battle Post Processing"}){var go=scene.GetRootGameObjects().FirstOrDefault(x=>x.name==name);if(go)go.SetActive(false);}
                board.position=new Vector3(5.4f,0,0);var environment=Group("Volcano_Battlefield");Terrain(environment);Lighting(environment);Effects(environment);
                var switcher=battle.GetComponent<BattleEnvironmentSwitcher>();switcher.environments=new[]{new BattleEnvironmentSwitcher.EnvironmentOption{title="VOLCANIC CALDERA",root=environment.gameObject,ambient=new Color(.4f,.47f,.6f),background=new Color(.28f,.27f,.29f)}};switcher.trainingRenderers=Array.Empty<MeshRenderer>();switcher.Apply(0);if(switcher.nextButton)switcher.nextButton.gameObject.SetActive(false);
                battle.initialEnemies=1;var orbit=battle.battleCamera.GetComponent<BattleOrbitCamera>();orbit.homeYaw=85;orbit.homePitch=4;orbit.homeDistance=9;orbit.minPitch=-85;orbit.maxPitch=89;orbit.maxDistance=80;orbit.ResetView(true);var camera=battle.battleCamera;camera.clearFlags=CameraClearFlags.Skybox;camera.useOcclusionCulling=false;camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;camera.GetUniversalAdditionalCameraData().requiresColorTexture=true;camera.GetUniversalAdditionalCameraData().requiresDepthTexture=true;
                var reviews=Group("09_ReviewCameras",environment);string[] names={"Volcano_Overview","Volcano_LavaDetail","Volcano_Main","Volcano_FlowMist"};Vector3[] positions={new Vector3(-42,34,-48),new Vector3(-15,1,-31),new Vector3(69,22,3),new Vector3(-15,3,-29)};Vector3[] targets={new Vector3(40,7,8),new Vector3(10,-4,-32),config.mainVolcanoPosition+Vector3.up*40,new Vector3(12,.6f,-28)};
                for(int i=0;i<names.Length;i++){var c=Group(names[i],reviews).gameObject.AddComponent<Camera>();c.transform.position=positions[i];c.transform.LookAt(targets[i]);c.fieldOfView=i==0?60:55;c.nearClipPlane=.1f;c.farClipPlane=600;c.clearFlags=CameraClearFlags.Skybox;c.enabled=false;c.useOcclusionCulling=false;c.GetUniversalAdditionalCameraData().renderPostProcessing=true;c.GetUniversalAdditionalCameraData().requiresColorTexture=true;c.GetUniversalAdditionalCameraData().requiresDepthTexture=true;}
                AddAirReviewCameras(environment);
                PrefabUtility.SaveAsPrefabAsset(environment.Find("04_MainVolcano").gameObject,Root+"/Prefabs/Volcano_Main.prefab");PrefabUtility.SaveAsPrefabAsset(environment.Find("02_LavaRiversAndLake").gameObject,Root+"/Prefabs/Lava_Flow.prefab");PrefabUtility.SaveAsPrefabAsset(environment.Find("03_LavaFalls").gameObject,Root+"/Prefabs/Lava_Fall.prefab");PrefabUtility.SaveAsPrefabAsset(environment.Find("05_CliffsAndDistantMountains").gameObject,Root+"/Prefabs/Volcanic_RockCluster.prefab");
                PrefabUtility.SaveAsPrefabAsset(environment.gameObject,Root+"/Prefabs/Volcano_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Register();Capture();Debug.Log("VOLCANO_BATTLEFIELD_READY: "+lavaSurfaces.Count+" lava meshes, "+config.arenaSize+"m basalt platform");
            }
            finally{UnityEngine.Random.state=randomState;}
        }
        static void Register()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath);var d=catalog.Find("volcanic-caldera");if(!d){d=ScriptableObject.CreateInstance<EnvironmentDefinition>();d.id="volcanic-caldera";AssetDatabase.CreateAsset(d,Root+"/VolcanoDefinition.asset");}
            d.displayName="VOLCANIC CALDERA · EMBERFALL";d.description="Gundam giao chiến trên nền basalt · hồ dung nham và núi lửa.";d.available=true;d.scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);d.SyncScene();d.battleVariant=-1;d.accent=new Color(1,.25f,.04f);EditorUtility.SetDirty(d);catalog.environments=new[]{d}.Concat(catalog.environments.Where(x=>x!=d)).ToArray();EditorUtility.SetDirty(catalog);EnvironmentTools.RegisterScenes();AssetDatabase.SaveAssets();
        }
        [MenuItem("Tools/Volcano/Open Battlefield")]
        public static void Open(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(ScenePath);}
        [MenuItem("Tools/Volcano/Capture Review")]
        public static void Capture()
        {
            foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>().Where(c=>c.name.StartsWith("Volcano_")))
            {
                var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var old=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;var image=new Texture2D(1600,900,TextureFormat.RGB24,false);
                try{c.targetTexture=rt;c.aspect=16f/9;c.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();File.WriteAllBytes(Root+"/Documentation/Review/"+c.name+".png",image.EncodeToPNG());}
                finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
            }
            AssetDatabase.Refresh();var d=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath).Find("volcanic-caldera");if(d){d.thumbnail=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Documentation/Review/Volcano_Overview.png");EditorUtility.SetDirty(d);AssetDatabase.SaveAssets();}
        }
    }
}
