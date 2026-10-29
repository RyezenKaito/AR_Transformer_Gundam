using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace BlueFlame.Editor
{
    public static partial class BlueFlameBuilder
    {
        [MenuItem("BlueFlame/Validate prefabs and export review views")]
        public static void ValidateAndCapture()
        {
            var report=new StringBuilder("BlueFlame asset validation\n");int issues=0;
            foreach(var file in Directory.GetFiles(Root+"/Prefabs","*.prefab"))
            {
                var path=file.Replace('\\','/');var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if(!prefab){report.AppendLine("MISSING PREFAB "+path);issues++;continue;}
                var mesh=prefab.GetComponentsInChildren<MeshFilter>(true);var renderers=prefab.GetComponentsInChildren<MeshRenderer>(true);
                int missing=mesh.Count(m=>!m.sharedMesh)+renderers.Count(r=>r.sharedMaterials.Any(m=>!m||!m.shader||!m.shader.isSupported));
                int scripts=prefab.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
                int nonpersistent=mesh.Count(m=>m.sharedMesh&&!EditorUtility.IsPersistent(m.sharedMesh));
                issues+=missing+scripts+nonpersistent;
                report.AppendLine(Path.GetFileName(path)+": meshes="+mesh.Length+", triangles="+mesh.Where(m=>m.sharedMesh).Sum(m=>(long)m.sharedMesh.triangles.Length/3)+", missing="+missing+", missingScripts="+scripts+", transientMeshes="+nonpersistent);
            }
            var ar=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/BlueFlame_AR_25cm.prefab");
            var clone=UnityEngine.Object.Instantiate(ar);try{var rs=clone.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);report.AppendLine("AR bounds height="+b.size.y+", ground="+b.min.y);if(Mathf.Abs(b.size.y-.25f)>.001f||Mathf.Abs(b.min.y)>.001f)issues++;}finally{UnityEngine.Object.DestroyImmediate(clone);}
            var volume=UnityEngine.Object.FindAnyObjectByType<Volume>();report.AppendLine("Volume components persistent="+volume.sharedProfile.components.All(c=>EditorUtility.IsPersistent(c)));
            foreach(string n in new[]{"10_Cyan_OpticalEmission","11_Reactor_SapphireLens"}){var m=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/"+n+".mat");report.AppendLine(n+" emission="+m.IsKeywordEnabled("_EMISSION"));if(!m.IsKeywordEnabled("_EMISSION"))issues++;}
            var viewer=UnityEngine.Object.FindAnyObjectByType<BlueFlame.BlueFlameViewer>();
            viewer.energy.gameObject.SetActive(false);viewer.SetView(23,6,19.5f,new Vector3(0,3.6f,0));Capture("BlueFlame_Hero",2400,1800);
            viewer.SetView(0,3,23,new Vector3(0,3.6f,0));Capture("BlueFlame_Front",2200,1600);
            viewer.SetView(157,8,22,new Vector3(0,3.6f,0));Capture("BlueFlame_Rear",2200,1600);
            viewer.SetView(12,3,5.7f,new Vector3(0,5.94f,0));Capture("BlueFlame_HeadChest",1600,1600);
            viewer.SetView(22,4,6.3f,new Vector3(.4f,1.72f,0));Capture("BlueFlame_LegMechanics",1400,1600);
            viewer.SetView(23,6,19.5f,new Vector3(0,3.6f,0));viewer.energy.gameObject.SetActive(true);Capture("BlueFlame_Energy",2200,1600);viewer.energy.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            report.AppendLine("Issues="+issues);File.WriteAllText("Docs/BlueFlame/ValidationReport.txt",report.ToString());Debug.Log(report.ToString());
            if(issues>0)throw new InvalidOperationException("BlueFlame validation issues: "+issues);
        }
    }
}
