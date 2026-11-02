using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GundamGame.CombatDemo.Editor
{
    public static class RougeAntennaAuthoring
    {
        public const string IconPath="Assets/Game/Features/Combat/UI/StrikeRouge/Art/Icon_AntennaPulse.png";
        public const string ClipPath="Assets/Game/Features/Combat/Animations/StrikeRouge/Rouge_AntennaPulse.anim";
        public const string ReportFolder="Docs/Reports/StrikeRougeAntenna";
        static Transform Find(Transform root,string token)=>root.GetComponentsInChildren<Transform>(true).First(t=>t.name.StartsWith(token,StringComparison.Ordinal));

        [MenuItem("Gundam/Strike Rouge/Use V-Fin Pulse for Skill One")]
        public static void Upgrade()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before changing skill one.");
            var scene=EditorSceneManager.GetActiveScene();
            if(scene.path!=RougeCombatSceneBuilder.ScenePath)throw new InvalidOperationException("Open the Strike Rouge combat demo first.");
            Directory.CreateDirectory(ReportFolder);
            if(!EditorSceneManager.SaveScene(scene,ReportFolder+"/BeforeAntenna_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true))throw new IOException("Cannot preserve current scene.");
            var battle=UnityEngine.Object.FindAnyObjectByType<RougeBattleDemo>();
            Configure(battle);
            var art=AssetDatabase.LoadAssetAtPath<RougeHudArt>(RougeHudAuthoring.Folder+"/StrikeRougeHudArt.asset");
            UseAntennaIcon(art);EditorUtility.SetDirty(art);
            string path=RougeHudAuthoring.Folder+"/Prefabs/StrikeRougeMechaHUD.prefab";
            var prefab=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var hud=prefab.GetComponent<MechaBattleHUD>();hud.art=art;
                var first=hud.skillButtons[0];
                var picture=first.GetComponentsInChildren<Image>(true).First(i=>i.sprite!=null);
                picture.sprite=art.weaponIcons[0];
                first.GetComponentsInChildren<Text>(true).First(t=>t.name=="Skill Name").text="V-FIN PULSE";
                PrefabUtility.SaveAsPrefabAsset(prefab,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(prefab);}
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText(ReportFolder+"/Setup.txt","Skill 1: V-Fin Pulse\nThree single-target pulses; unchanged damage, armor and Burst rules.\nSockets parented to head_06 and fitted to weighted V-fin mesh vertices.\nAnimation: "+ClipPath+"\nIcon: "+IconPath+"\n");
            Debug.Log("Rouge V-Fin Pulse ready: head sockets, neutral arms, dedicated animation and HUD icon.");
        }

        public static void Configure(RougeBattleDemo battle)
        {
            if(battle==null || battle.presentation==null)throw new InvalidOperationException("Rouge presentation missing.");
            var presentation=battle.presentation;
            var model=battle.player.visual.GetChild(0);
            var head=Find(model,"head_");
            // Fit using a separate sampled model, so authoring never changes the live source pose.
            var source=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(RougeSkillSetup.ModelPath));
            source.hideFlags=HideFlags.HideAndDontSave;
            Vector3 left,right,center;
            try
            {
                foreach(var a in source.GetComponentsInChildren<Animator>(true))a.enabled=false;
                presentation.animationRig.idle.SampleAnimation(source,2);
                var sampleHead=Find(source.transform,"head_");
                FitFin(source,sampleHead,out left,out right,out center);
            }
            finally{UnityEngine.Object.DestroyImmediate(source);}
            presentation.antennaLeftTip=Socket(head,"V-Fin Left Tip",left);
            presentation.antennaRightTip=Socket(head,"V-Fin Right Tip",right);
            presentation.antennaMuzzle=Socket(head,"V-Fin Pulse Muzzle",center);
            battle.player.muzzle=presentation.antennaMuzzle;
            presentation.animationRig.antenna=GenerateClip(presentation.animationRig.idle);
            EditorUtility.SetDirty(presentation.animationRig);EditorUtility.SetDirty(presentation);EditorUtility.SetDirty(battle.player);
        }
        static Transform Socket(Transform head,string name,Vector3 local)
        {
            var socket=head.Find(name);
            if(socket==null){socket=new GameObject(name).transform;socket.SetParent(head,false);}
            socket.localPosition=local;socket.localRotation=Quaternion.identity;return socket;
        }
        static void FitFin(GameObject source,Transform head,out Vector3 left,out Vector3 right,out Vector3 center)
        {
            var vertices=new List<Vector3>();
            foreach(var renderer in source.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                int joint=Array.IndexOf(renderer.bones,head);if(joint<0)continue;
                var baked=new Mesh();renderer.BakeMesh(baked);
                try
                {
                    var positions=baked.vertices;var weights=renderer.sharedMesh.boneWeights;
                    for(int i=0;i<positions.Length;i++)
                    {
                        var w=weights[i];float weight=(w.boneIndex0==joint?w.weight0:0)+(w.boneIndex1==joint?w.weight1:0)+(w.boneIndex2==joint?w.weight2:0)+(w.boneIndex3==joint?w.weight3:0);
                        if(weight>.5f)vertices.Add(head.InverseTransformPoint(renderer.transform.TransformPoint(positions[i])));
                    }
                }
                finally{UnityEngine.Object.DestroyImmediate(baked);}
            }
            if(vertices.Count<4)throw new InvalidOperationException("Cannot locate Rouge head geometry.");
            // Imported bone axes are rotated. Select high, lateral vertices in model space,
            // then retain their head-local coordinates so sockets follow the animation.
            Vector3 headCenter=source.transform.InverseTransformPoint(head.position);
            Vector3 ModelOffset(Vector3 p)=>source.transform.InverseTransformPoint(head.TransformPoint(p))-headCenter;
            left=vertices.Where(p=>ModelOffset(p).x<-.035f&&ModelOffset(p).y>.05f).OrderByDescending(p=>ModelOffset(p).y+Mathf.Abs(ModelOffset(p).x)*.8f).First();
            right=vertices.Where(p=>ModelOffset(p).x>.035f&&ModelOffset(p).y>.05f).OrderByDescending(p=>ModelOffset(p).y+Mathf.Abs(ModelOffset(p).x)*.8f).First();
            Vector3 forward=head.InverseTransformDirection(source.transform.forward).normalized;
            center=(left+right)*.5f+forward*.025f;
            Directory.CreateDirectory(ReportFolder);
            File.WriteAllText(ReportFolder+"/HeadSockets.txt","Head: "+head.name+"\nLeft: "+left.ToString("F5")+"\nRight: "+right.ToString("F5")+"\nMuzzle: "+center.ToString("F5")+"\n");
        }

        public static void UseAntennaIcon(RougeHudArt art)
        {
            if(art==null || art.weaponIcons.Length!=4)throw new InvalidOperationException("HUD artwork missing.");
            AssetDatabase.ImportAsset(IconPath,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(IconPath);
            if(importer==null)throw new InvalidOperationException("Antenna icon PNG missing.");
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=1024;
            importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Compressed;
            importer.SaveAndReimport();art.weaponIcons[0]=AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
        }

        struct Pose{public Transform t;public Vector3 p;public Quaternion q;}
        static AnimationClip GenerateClip(AnimationClip idle)
        {
            var source=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(RougeSkillSetup.ModelPath));source.hideFlags=HideFlags.HideAndDontSave;
            try
            {
                foreach(var a in source.GetComponentsInChildren<Animator>(true))a.enabled=false;
                idle.SampleAnimation(source,2);
                var nodes=source.GetComponentsInChildren<Transform>(true).Where(t=>t!=source.transform).ToArray();
                var poses=nodes.Select(t=>new Pose{t=t,p=t.localPosition,q=t.localRotation}).ToArray();
                var head=Find(source.transform,"head_");var chest=Find(source.transform,"chest_");
                var samples=nodes.ToDictionary(t=>t,t=>Enumerable.Range(0,7).Select(_=>new List<Keyframe>()).ToArray());
                const float length=1.5f;
                for(int frame=0;frame<=45;frame++)
                {
                    float time=frame/30f;
                    foreach(var pose in poses){pose.t.localPosition=pose.p;pose.t.localRotation=pose.q;}
                    float envelope=Mathf.SmoothStep(0,1,Mathf.Clamp01(time/.2f))*Mathf.SmoothStep(0,1,Mathf.Clamp01((length-time)/.22f));
                    float recoil=0;foreach(float shot in new[]{.38f,.69f,1f})recoil+=Mathf.Max(0,1-Mathf.Abs(time-shot)/.08f);
                    // Keep both arms in the original idle; only the head and torso brace for firing.
                    chest.localRotation*=Quaternion.Euler((-2+recoil*1.4f)*envelope,0,0);
                    head.localRotation*=Quaternion.Euler((4-recoil*3)*envelope,0,0);
                    foreach(var node in nodes)
                    {
                        var p=node.localPosition;var q=node.localRotation;float[] values={p.x,p.y,p.z,q.x,q.y,q.z,q.w};
                        for(int i=0;i<7;i++)samples[node][i].Add(new Keyframe(time,values[i]));
                    }
                }
                var clip=new AnimationClip{name="Rouge_AntennaPulse",frameRate=30};
                string[] properties={"m_LocalPosition.x","m_LocalPosition.y","m_LocalPosition.z","m_LocalRotation.x","m_LocalRotation.y","m_LocalRotation.z","m_LocalRotation.w"};
                foreach(var pair in samples)
                {
                    string path=AnimationUtility.CalculateTransformPath(pair.Key,source.transform);
                    for(int i=0;i<7;i++)
                    {
                        var keys=pair.Value[i];
                        var curve=keys.All(k=>Mathf.Abs(k.value-keys[0].value)<.00001f)?new AnimationCurve(new Keyframe(0,keys[0].value),new Keyframe(length,keys[0].value)):new AnimationCurve(keys.ToArray());
                        for(int k=0;k<curve.length;k++){AnimationUtility.SetKeyLeftTangentMode(curve,k,AnimationUtility.TangentMode.Linear);AnimationUtility.SetKeyRightTangentMode(curve,k,AnimationUtility.TangentMode.Linear);}
                        AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(path,typeof(Transform),properties[i]),curve);
                    }
                }
                clip.EnsureQuaternionContinuity();var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
                if(existing==null){AssetDatabase.CreateAsset(clip,ClipPath);return clip;}
                EditorUtility.CopySerialized(clip,existing);EditorUtility.SetDirty(existing);UnityEngine.Object.DestroyImmediate(clip);return existing;
            }
            finally{UnityEngine.Object.DestroyImmediate(source);}
        }
    }
}
