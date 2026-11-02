using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TurnBasedBattle.Editor
{
    public static class BattleAssetUtility
    {
        public const string Root="Assets/TurnBasedBattle";
        public static T Save<T>(T obj,string path) where T:UnityEngine.Object
        {
            MonoScript script=obj is ScriptableObject so?MonoScript.FromScriptableObject(so):null;
            var previous=AssetDatabase.LoadAssetAtPath<T>(path);
            if(previous==null)AssetDatabase.CreateAsset(obj,path);
            else{EditorUtility.CopySerialized(obj,previous);UnityEngine.Object.DestroyImmediate(obj);obj=previous;}
            if(script!=null){var serialized=new SerializedObject(obj);var binding=serialized.FindProperty("m_Script");if(binding!=null){binding.objectReferenceValue=script;serialized.ApplyModifiedPropertiesWithoutUndo();}}
            EditorUtility.SetDirty(obj);return obj;
        }
        public static GameObject Node(string name,Transform parent=null,Vector3 position=default)
        {var g=new GameObject(name);if(parent!=null)g.transform.SetParent(parent,false);g.transform.localPosition=position;return g;}
        public static Material Lit(string name,Color color,Color emission=default,float metallic=.2f)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",.35f);
            if(emission.maxColorComponent>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission);}
            return Save(m,Root+"/Materials/"+name+".mat");
        }
        public static Material Energy(string name,Color color,Texture texture,Vector2 scroll=default)
        {
            var m=new Material(Shader.Find("TurnBattle/EnergyRibbon")){name=name};m.SetColor("_BaseColor",color);m.SetTexture("_BaseMap",texture);m.SetVector("_Scroll",new Vector4(scroll.x,scroll.y,0,0));
            return Save(m,Root+"/Materials/"+name+".mat");
        }
        public static GameObject Shape(string name,PrimitiveType type,Transform parent,Vector3 pos,Vector3 scale,Material mat,Quaternion rotation=default)
        {
            var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=scale;
            if(rotation!=default)g.transform.localRotation=rotation;
            UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat;return g;
        }
        public static GameObject MeshObject(string name,Transform parent,Mesh mesh,Material material,Vector3 position=default)
        {
            var g=Node(name,parent,position);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;return g;
        }
        public static LineRenderer Line(string name,Transform parent,Material material,int count=33)
        {
            var line=Node(name,parent).AddComponent<LineRenderer>();line.sharedMaterial=material;line.positionCount=count;line.useWorldSpace=true;line.startWidth=line.endWidth=1;line.widthMultiplier=.03f;
            line.numCapVertices=2;line.numCornerVertices=1;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.startColor=line.endColor=Color.white;return line;
        }
        public static ParticleSystem Particles(string name,Transform parent,Material material,Color color,float lifetime,float size,float speed,int max=128,Mesh mesh=null)
        {
            var p=Node(name,parent).AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var m=p.main;m.playOnAwake=false;m.loop=false;m.duration=1;m.startLifetime=new ParticleSystem.MinMaxCurve(lifetime*.65f,lifetime);m.startSize=new ParticleSystem.MinMaxCurve(size*.4f,size);m.startSpeed=new ParticleSystem.MinMaxCurve(speed*.4f,speed);m.startColor=color;m.simulationSpace=ParticleSystemSimulationSpace.World;m.maxParticles=max;
            var e=p.emission;e.rateOverTime=0;var sh=p.shape;sh.shapeType=ParticleSystemShapeType.Cone;sh.angle=55;sh.radius=.25f;
            p.transform.localRotation=Quaternion.Euler(-90,0,0);
            var c=p.colorOverLifetime;c.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.85f,.08f),new GradientAlphaKey(.55f,.4f),new GradientAlphaKey(0,1)});c.color=gradient;
            var s=p.sizeOverLifetime;s.enabled=true;s.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,.5f,1,mesh==null?1.6f:.6f));
            var r=p.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
            if(mesh!=null){r.renderMode=ParticleSystemRenderMode.Mesh;r.mesh=mesh;var rotation=p.rotationOverLifetime;rotation.enabled=true;rotation.separateAxes=true;rotation.x=2;rotation.y=3;rotation.z=1; m.gravityModifier=.8f;}
            return p;
        }
        public static Texture2D Texture(string name,int width,int height,Func<float,float,Color> sample,bool repeat=false)
        {
            string path=Root+"/Textures/"+name+".png";var t=new Texture2D(width,height,TextureFormat.RGBA32,false);var pixels=new Color[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)pixels[y*width+x]=sample(x/(float)(width-1),y/(float)(height-1));
            t.SetPixels(pixels);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;importer.wrapMode=repeat?TextureWrapMode.Repeat:TextureWrapMode.Clamp;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        public static Mesh Rock(string name,int seed,bool pillar)
        {
            var random=new System.Random(seed);int sides=7,rings=pillar?6:4;var vertices=new List<Vector3>();var colors=new List<Color>();var uv=new List<Vector2>();var tris=new List<int>();
            var points=new Vector3[rings,sides];
            for(int j=0;j<rings;j++)for(int i=0;i<sides;i++)
            {
                float t=j/(float)(rings-1),a=i*Mathf.PI*2/sides;float radial=(pillar?Mathf.Lerp(.53f,.18f,t):Mathf.Sin(t*Mathf.PI)*.65f+.07f)*(.78f+(float)random.NextDouble()*.44f);
                points[j,i]=new Vector3(Mathf.Cos(a)*radial+(pillar?t*.13f:0),t+(j==rings-1?(float)random.NextDouble()*.10f:0),Mathf.Sin(a)*radial);
            }
            Action<Vector3,Vector3,Vector3> face=(a,b,c)=>{int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);float shade=.78f+(float)random.NextDouble()*.35f;for(int q=0;q<3;q++){colors.Add(new Color(shade,shade,shade,1));uv.Add(Vector2.zero);}tris.Add(n);tris.Add(n+1);tris.Add(n+2);};
            for(int j=0;j<rings-1;j++)for(int i=0;i<sides;i++){int k=(i+1)%sides;face(points[j,i],points[j+1,i],points[j,k]);face(points[j,k],points[j+1,i],points[j+1,k]);}
            for(int i=0;i<sides;i++){int k=(i+1)%sides;face(Vector3.zero,points[0,k],points[0,i]);face(Vector3.up*1.02f,points[rings-1,i],points[rings-1,k]);}
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();return Save(mesh,Root+"/Meshes/"+name+".asset");
        }
        public static Mesh Dome(string name,int variant)
        {
            const int segments=128;var v=new Vector3[(segments+1)*2];var uv=new Vector2[v.Length];var colors=new Color[v.Length];var tris=new int[segments*6];
            for(int i=0;i<=segments;i++)
            {
                float u=i/(float)segments,a=u*Mathf.PI*2;
                float y=.10f+variant*.22f+.12f*Mathf.Sin(a*2+variant);float radius=1-y*.16f;
                v[i*2]=new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius);
                v[i*2+1]=new Vector3(Mathf.Cos(a)*(radius-.04f),y+.24f,Mathf.Sin(a)*(radius-.04f));
                uv[i*2]=new Vector2(u*2,0);uv[i*2+1]=new Vector2(u*2,1);colors[i*2]=colors[i*2+1]=Color.white;
                if(i<segments){int t=i*6,k=i*2;tris[t]=k;tris[t+1]=k+1;tris[t+2]=k+2;tris[t+3]=k+2;tris[t+4]=k+1;tris[t+5]=k+3;}
            }
            var mesh=new Mesh{name=name,vertices=v,uv=uv,colors=colors,triangles=tris};mesh.RecalculateNormals();mesh.RecalculateBounds();return Save(mesh,Root+"/Meshes/"+name+".asset");
        }
        public static AudioClip Sound(string name,float duration,float frequency,float endFrequency,float noiseAmount,bool loop=false)
        {
            const int rate=22050;int count=(int)(duration*rate);var random=new System.Random(name.GetHashCode());double phase=0;string path=Root+"/Audio/"+name+".wav";
            using(var w=new BinaryWriter(File.Open(path,FileMode.Create)))
            {
                w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+count*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(count*2);
                for(int i=0;i<count;i++){double t=i/(double)count;phase+=2*Math.PI*Mathf.Lerp(frequency,endFrequency,(float)t)/rate;double env=loop?Math.Sin(Math.PI*t)*.6:Math.Sin(Math.PI*Math.Min(1,t*10))*Math.Pow(1-t,1.5);double s=(Math.Sin(phase)*.45+Math.Sin(phase*1.97)*.15+(random.NextDouble()*2-1)*noiseAmount)*env;w.Write((short)(Math.Max(-1,Math.Min(1,s))*.55*short.MaxValue));}
            }
            AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
