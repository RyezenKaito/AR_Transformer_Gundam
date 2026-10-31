using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GundamGame.CombatDemo
{
    /// <summary>Evaluates imported and combat clips on the source rig, using battle time.</summary>
    public sealed class RougeAnimationDriver : MonoBehaviour
    {
        public Animator rig;
        public AnimationClip idle, arrival, launcher, separation, rifle, blade, barrage, guard, dodge;
        public string CurrentClipName => action.IsValid() && blend > .01f ? action.GetAnimationClip().name : idle != null ? idle.name : "None";
        public float ActionTime { get; private set; }
        public int ImportedClipsPlayed { get; private set; }
        PlayableGraph graph;
        AnimationMixerPlayable mixer;
        AnimationClipPlayable idlePlayable, action;
        float clock, startTime, rate=1, blend, desiredBlend;
        bool ready;

        public void Initialize()
        {
            if (ready || rig == null || idle == null) return;
            rig.enabled = true; rig.applyRootMotion = false; rig.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            graph = PlayableGraph.Create("Strike Rouge source animation rig");
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            mixer = AnimationMixerPlayable.Create(graph, 2);
            idlePlayable = AnimationClipPlayable.Create(graph, idle); idlePlayable.SetSpeed(0);
            graph.Connect(idlePlayable,0,mixer,0); mixer.SetInputWeight(0,1);
            AnimationPlayableOutput.Create(graph,"Strike Rouge",rig).SetSourcePlayable(mixer);
            graph.Play(); ready=true; Evaluate(0);
        }

        public void Play(AnimationClip clip, float sourceStart=0, float speed=1)
        {
            Initialize(); if (!ready || clip == null) return;
            if (action.IsValid()) { graph.Disconnect(mixer,1); graph.DestroyPlayable(action); }
            action = AnimationClipPlayable.Create(graph,clip); action.SetSpeed(0);
            graph.Connect(action,0,mixer,1);
            ActionTime=0; startTime=sourceStart; rate=speed; desiredBlend=1;
            if (clip == arrival || clip == launcher || clip == separation) ImportedClipsPlayed++;
            Evaluate(0);
        }

        public void Idle(bool immediate=false)
        {
            desiredBlend=0;
            if (immediate) { blend=0; clock=0; Evaluate(0); }
        }

        public void Evaluate(float delta)
        {
            if (!ready) return;
            clock+=delta; ActionTime+=delta;
            blend=Mathf.MoveTowards(blend,desiredBlend,delta*9);
            idlePlayable.SetTime(idle.length > 0 ? clock % idle.length : 0);
            if(action.IsValid()) action.SetTime(Mathf.Clamp(startTime+ActionTime*rate,0,Mathf.Max(0,action.GetAnimationClip().length-.001f)));
            mixer.SetInputWeight(0,1-blend); mixer.SetInputWeight(1,blend);
            graph.Evaluate(0);
        }
        void OnDestroy() { if(graph.IsValid())graph.Destroy(); }
    }
}
