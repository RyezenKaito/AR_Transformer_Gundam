using System;
using System.IO;
using System.Linq;
using GundamGame.CombatDemo;
using UnityEditor;
using UnityEngine;
using Cue = GundamGame.CombatDemo.EnemyProfile.Cue;

namespace GundamGame.Enemies.Editor
{
    public static class Rx78EnemyTimelines
    {
        const string Folder="Assets/Game/Features/Combat/Enemies";
        const int FrameRate=30;

        [MenuItem("Gundam/Combat/Rebuild RX-78-2 Enemy Timelines")]
        public static void Build()
        {
            var ranged=Load("Ranged");
            var melee=Load("Melee");
            Write(ranged,0,70,new[]{(0,0f),(14,2.15f),(34,3.33f),(54,3.40f),(70,3.5f)},
                (16,Cue.Warn),(35,Cue.Fire),(35,Cue.Hit),(41,Cue.Fire),(41,Cue.Hit),(47,Cue.Fire),(47,Cue.Hit),(50,Cue.Recover));
            Write(ranged,1,83,new[]{(0,0f),(16,2.12f),(34,3.29f),(63,3.43f),(83,3.50f)},
                (15,Cue.Warn),(28,Cue.Equip),(36,Cue.Fire),(47,Cue.Hit),(53,Cue.Recover));
            Write(ranged,2,65,new[]{(0,0f),(18,2.35f),(33,2.54f),(65,2.65f)},
                (12,Cue.Warn),(24,Cue.Equip),(30,Cue.Guard),(50,Cue.Recover));
            Write(melee,0,83,new[]{(0,0f),(17,2.19f),(28,3.09f),(37,3.38f),(42,3.13f),(50,3.42f),
                    (55,3.14f),(63,3.48f),(83,3.89f)},
                (12,Cue.Warn),(29,Cue.BladeOn),(37,Cue.Hit),(50,Cue.Hit),(63,Cue.Hit),(66,Cue.Recover),(76,Cue.BladeOff));
            Write(melee,1,67,new[]{(0,0f),(20,2.14f),(34,2.64f),(48,2.7f),(67,2.15f)},
                (11,Cue.Warn),(32,Cue.Fire),(32,Cue.Hit),(38,Cue.Fire),(38,Cue.Hit),(44,Cue.Fire),(44,Cue.Hit),(49,Cue.Recover));
            Write(melee,2,74,new[]{(0,0f),(18,2.2f),(29,3.15f),(39,3.25f),(46,3.46f),(74,3.9f)},
                (12,Cue.Warn),(27,Cue.BladeOn),(40,Cue.Hit),(46,Cue.Recover),(66,Cue.BladeOff));
            AssetDatabase.SaveAssets();
        }

        static EnemyProfile Load(string variant)=>AssetDatabase.LoadAssetAtPath<EnemyProfile>(Folder+"/RX-78-2 "+variant+".asset");

        static void Write(EnemyProfile profile,int slot,int frames,(int frame,float source)[] warp,params (int frame,Cue cue)[] cues)
        {
            var move=profile.moves[slot];
            if(cues.Count(c=>c.cue==Cue.Hit)!=move.hits)
                throw new InvalidOperationException(move.label+" has the wrong number of hit markers.");
            string name=move.label.Replace(" ","_");
            string path=Folder+"/"+name+".anim";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if(!clip)
            {
                clip=new AnimationClip{name=name};
                AssetDatabase.CreateAsset(clip,path);
            }
            clip.frameRate=FrameRate;
            // This timing clip contains only an inert curve to establish duration.
            // The runtime uses its events to sample the full imported rig.
            clip.SetCurve("__Timing",typeof(Transform),"localPosition.x",
                AnimationCurve.Constant(0,frames/(float)FrameRate,0));
            AnimationUtility.SetAnimationEvents(clip,cues.Select(c=>new AnimationEvent
            {
                time=c.frame/(float)FrameRate,functionName="OnEnemyCue",intParameter=(int)c.cue
            }).ToArray());
            var source=new AnimationCurve(warp.Select(k=>new Keyframe(k.frame/(float)FrameRate,k.source)).ToArray());
            for(int i=0;i<source.length;i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(source,i,AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(source,i,AnimationUtility.TangentMode.Linear);
            }
            move.timeline=clip;
            move.sourceTime=source;
            profile.moves[slot]=move;
            EditorUtility.SetDirty(clip);
            EditorUtility.SetDirty(profile);
        }
    }
}
