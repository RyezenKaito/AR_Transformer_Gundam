using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GundamGame.CombatDemo.Editor
{
    [InitializeOnLoad]
    public static class RougeSkillSetup
    {
        public const string ModelPath = "Assets/Models/MBF-02-Strike-Rouge-Ootori/source/785154d4d67f435a99b5365aece0ea2c.glb";
        const string Request = "Temp/RougeSkillUpgrade.request";
        public const string ReportFolder = "Docs/Reports/StrikeRougeSkills";
        static RougeSkillSetup() { EditorApplication.update += CheckRequest; }
        static void CheckRequest()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(Request)) return;
            string mode;
            try { mode = File.ReadAllText(Request).Trim(); File.Delete(Request); }
            catch (IOException) { return; }
            try
            {
                if (mode == "inspect") Inspect();
                else if (mode == "upgrade") RougeCombatAuthoring.Upgrade();
                else if (mode == "camera") RougeCombatAuthoring.UpgradeCameras();
                else Debug.LogWarning("Unknown Rouge setup request: " + mode);
            }
            catch (Exception e)
            {
                Directory.CreateDirectory(ReportFolder);
                File.WriteAllText(ReportFolder + "/SetupError.txt", e.ToString());
                Debug.LogException(e);
            }
        }
        [MenuItem("Gundam/Strike Rouge/Inspect Imported Animations")]
        public static void Inspect()
        {
            Directory.CreateDirectory(ReportFolder);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var clips = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__")).ToArray();
            var lines = new List<string> { "Model: " + ModelPath };
            foreach (var clip in clips)
            {
                lines.Add("CLIP " + clip.name + " seconds=" + clip.length + " bindings=" + AnimationUtility.GetCurveBindings(clip).Length);
                foreach (var binding in AnimationUtility.GetCurveBindings(clip).Take(3)) lines.Add("  " + binding.path + " : " + binding.propertyName);
            }
            var frames = new List<Texture2D>();
            foreach (var spec in new[] { "O-02-IDLE:2", "O-03-LAUNCHER:0", "O-03-LAUNCHER:2", "O-03-LAUNCHER:4", "O-03-LAUNCHER:6", "O-03-LAUNCHER:8", "O-04-SEPARATION:0", "O-04-SEPARATION:3", "O-04-SEPARATION:6", "O-01-APPER:2", "O-01-APPER:6", "O-01-APPER:9" })
            {
                var parts = spec.Split(':'); var clip = clips.First(c => c.name == parts[0]);
                float time = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                var utility = new PreviewRenderUtility();
                try
                {
                    var go = UnityEngine.Object.Instantiate(asset);
                    foreach (var a in go.GetComponentsInChildren<Animator>(true)) a.enabled = false;
                    foreach (var s in go.GetComponentsInChildren<SkinnedMeshRenderer>(true)) s.updateWhenOffscreen = true;
                    utility.AddSingleGO(go); clip.SampleAnimation(go, time);
                    var nodes = go.GetComponentsInChildren<Transform>(true);
                    var hips = nodes.FirstOrDefault(t => t.name.StartsWith("hips"));
                    Vector3 target = hips != null ? hips.position : Vector3.up;
                    utility.camera.transform.position = target + new Vector3(3, 1.6f, 5.2f);
                    utility.camera.transform.LookAt(target);
                    utility.camera.fieldOfView = 32; utility.camera.nearClipPlane = .01f; utility.camera.farClipPlane = 100;
                    utility.camera.clearFlags = CameraClearFlags.SolidColor; utility.camera.backgroundColor = new Color(.025f,.04f,.065f);
                    utility.lights[0].intensity = 1.5f; utility.lights[0].transform.rotation = Quaternion.Euler(40,20,0);
                    utility.lights[1].intensity = .9f;
                    utility.BeginPreview(new Rect(0,0,600,600), GUIStyle.none); utility.Render(true);
                    var texture = utility.EndPreview();
                    var previous = RenderTexture.active; var rt = RenderTexture.GetTemporary(600,600,0);
                    Graphics.Blit(texture,rt); RenderTexture.active=rt;
                    var image = new Texture2D(600,600,TextureFormat.RGB24,false); image.ReadPixels(new Rect(0,0,600,600),0,0); image.Apply();
                    RenderTexture.active=previous; RenderTexture.ReleaseTemporary(rt);
                    File.WriteAllBytes(ReportFolder + "/" + parts[0] + "_" + parts[1] + ".png", image.EncodeToPNG()); frames.Add(image);
                    lines.Add("SAMPLE " + spec);
                    if (spec == "O-03-LAUNCHER:2")
                    {
                        foreach (var renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                        {
                            var mesh = new Mesh(); renderer.BakeMesh(mesh);
                            var vertices = mesh.vertices; var weights = renderer.sharedMesh.boneWeights;
                            for (int b=0;b<renderer.bones.Length;b++)
                            {
                                var bone=renderer.bones[b]; if (bone==null) continue;
                                bool found=false; Bounds bounds=new Bounds();
                                for(int v=0;v<weights.Length;v++) if(weights[v].boneIndex0==b && weights[v].weight0>.5f)
                                { var p=bone.InverseTransformPoint(renderer.transform.TransformPoint(vertices[v])); if(!found){bounds=new Bounds(p,Vector3.zero);found=true;}else bounds.Encapsulate(p); }
                                if(found) lines.Add("BONE-GEOMETRY " + bone.name + " min="+bounds.min.ToString("F3")+" max="+bounds.max.ToString("F3"));
                            }
                            UnityEngine.Object.DestroyImmediate(mesh);
                        }
                    }
                    foreach (var node in nodes.Where(t => t.name.StartsWith("hand.") || t.name.EndsWith("_031") || t.name.EndsWith("_046") || t.name.EndsWith("_048") || t.name.EndsWith("_044") || t.name.EndsWith("_045") || t.name.StartsWith("upper_arm")))
                        lines.Add("  " + node.name + " p=" + node.position.ToString("F3") + " e=" + node.localEulerAngles.ToString("F1") + " right=" + node.right.ToString("F2") + " up=" + node.up.ToString("F2") + " fwd=" + node.forward.ToString("F2"));
                }
                finally { utility.Cleanup(); }
            }
            var sheet = new Texture2D(2400,1800,TextureFormat.RGB24,false);
            for (int i=0;i<frames.Count;i++) { sheet.SetPixels((i%4)*600,(2-i/4)*600,600,600,frames[i].GetPixels()); UnityEngine.Object.DestroyImmediate(frames[i]); }
            sheet.Apply(); File.WriteAllBytes(ReportFolder + "/AnimationContactSheet.png", sheet.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(sheet);
            File.WriteAllLines(ReportFolder + "/AnimationAudit.txt", lines);
            Debug.Log("Rouge animation audit complete: " + ReportFolder);
        }
    }
}
