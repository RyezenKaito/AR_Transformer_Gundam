using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GundamLobby
{
    /// <summary>Plays source-rig presentation clips without combat input or root motion.</summary>
    [DisallowMultipleComponent]
    public sealed class LobbyPreviewAnimation : MonoBehaviour
    {
        public string unitId;
        public Animator rig;
        public AnimationClip clip;
        [Range(.1f,2f)] public float speed=1;
        public float phaseOffset=2;
        public float PlaybackTime { get; private set; }
        public bool IsPlaying=>graph.IsValid()&&graph.IsPlaying();
        PlayableGraph graph;
        AnimationClipPlayable playable;
        AnimationClip ownedClip;
        float clock;
        void OnEnable()
        {
            if(rig==null||clip==null)return;
            rig.enabled=true;rig.applyRootMotion=false;rig.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            graph=PlayableGraph.Create("Lobby preview / "+unitId);
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            // Some downloaded rigs expose Legacy clips. Convert a private runtime copy;
            // never change the imported model or its read-only source clip.
            var playableClip=clip;
            if(clip.legacy){ownedClip=Instantiate(clip);ownedClip.name=clip.name+" / preview";ownedClip.legacy=false;playableClip=ownedClip;}
            playable=AnimationClipPlayable.Create(graph,playableClip);playable.SetSpeed(0);playable.SetApplyFootIK(false);playable.SetApplyPlayableIK(false);
            var output=AnimationPlayableOutput.Create(graph,"Source rig",rig);output.SetSourcePlayable(playable);
            graph.Play();Evaluate(0);
        }
        void LateUpdate(){Evaluate(Time.deltaTime);}
        void Evaluate(float delta)
        {
            if(!graph.IsValid())return;
            clock+=Mathf.Max(0,delta)*speed;
            PlaybackTime=Mathf.Repeat(phaseOffset+clock,Mathf.Max(.001f,clip.length));
            playable.SetTime(PlaybackTime);graph.Evaluate(0);
        }
        void Release()
        {
            if(graph.IsValid())graph.Destroy();
            if(ownedClip!=null){Destroy(ownedClip);ownedClip=null;}
        }
        void OnDisable(){Release();}
        void OnDestroy(){Release();}
    }
}
