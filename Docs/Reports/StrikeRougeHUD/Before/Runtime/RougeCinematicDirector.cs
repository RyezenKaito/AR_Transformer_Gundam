using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace GundamGame.CombatDemo
{
    /// <summary>A full-screen character cut-in, rendered without moving the player's or tracked camera.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class RougeCinematicDirector : MonoBehaviour
    {
        public enum Shot { CasterFocus, Salvo, Impact }
        public bool Active { get; private set; }
        public Shot CurrentShot { get; private set; }
        public Camera ShotCamera => shotCamera;
        public bool GameplayHudHidden => battle!=null && battle.GameplayCanvas!=null && !battle.GameplayCanvas.enabled;
        public bool AllEnemiesHidden
        {
            get
            {
                if(battle==null)return false;
                foreach(var enemy in battle.enemies)
                    if(enemy!=null)foreach(var renderer in enemy.visual.GetComponentsInChildren<Renderer>(true))
                        if(renderer.enabled && renderer.gameObject.activeInHierarchy && !renderer.forceRenderingOff)return false;
                return true;
            }
        }
        readonly Dictionary<Renderer,bool> rendererStates=new Dictionary<Renderer,bool>();
        readonly List<CanvasState> canvasStates=new List<CanvasState>();
        struct CanvasState { public Canvas canvas;public bool enabled;public GraphicRaycaster raycaster;public bool raycasts; }
        RougeBattleDemo battle;
        RougeArenaPlacement placement;
        DemoFighter target;
        Camera shotCamera;
        RenderTexture texture;
        RawImage image;
        float elapsed;
        int width,height;
        static readonly Color Background=new Color(.016f,.008f,.035f,1);

        public void Initialize(RougeBattleDemo controller,Camera source,RougeArenaPlacement ar,Transform cinemaRoot)
        {
            if(shotCamera!=null)return;
            battle=controller;placement=ar;
            var cameraObject=new GameObject("Rouge Character Cinematic Camera",typeof(Camera));cameraObject.transform.SetParent(transform,false);
            shotCamera=cameraObject.GetComponent<Camera>();
            if(source!=null)shotCamera.CopyFrom(source);
            shotCamera.enabled=false;shotCamera.tag="Untagged";shotCamera.clearFlags=CameraClearFlags.SolidColor;shotCamera.backgroundColor=Background;
            shotCamera.allowHDR=false;shotCamera.allowMSAA=false;shotCamera.depth=30;shotCamera.nearClipPlane=.005f;shotCamera.farClipPlane=300;
            cameraObject.AddComponent<UniversalAdditionalCameraData>().renderShadows=true;
            var view=new GameObject("Character Focus View",typeof(RectTransform),typeof(RawImage));view.transform.SetParent(cinemaRoot,false);view.transform.SetAsFirstSibling();
            image=view.GetComponent<RawImage>();image.raycastTarget=false;
            var rect=view.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            image.gameObject.SetActive(false);
        }
        public void Begin(DemoFighter victim)
        {
            if(shotCamera==null || battle==null)return;
            if(Active)End();
            target=victim;elapsed=0;CurrentShot=Shot.CasterFocus;
            rendererStates.Clear();canvasStates.Clear();
            // Snapshot only objects that existed before charging VFX are spawned.
            foreach(var renderer in battle.arenaRoot.GetComponentsInChildren<Renderer>(true))rendererStates[renderer]=renderer.forceRenderingOff;
            HideCanvas(battle.GameplayCanvas);
            if(placement!=null)HideCanvas(placement.GetComponentInChildren<Canvas>(true));
            EnsureTexture();image.texture=texture;image.gameObject.SetActive(true);shotCamera.enabled=true;Active=true;
            FocusVisibility();ComposeShot();
        }
        public void SetShot(Shot shot)
        {
            if(!Active || CurrentShot==shot)return;
            CurrentShot=shot;RestoreRenderers();
            if(shot==Shot.CasterFocus)FocusVisibility();
            ComposeShot();
        }
        public void SetClock(float time){elapsed=time;}
        void LateUpdate()
        {
            if(!Active)return;
            EnsureTexture();ComposeShot();
        }
        void EnsureTexture()
        {
            float aspect=Mathf.Max(.2f,Screen.width/(float)Mathf.Max(1,Screen.height));
            int w=aspect>=1?Mathf.Min(1280,Mathf.Max(320,Screen.width)):Mathf.RoundToInt(Mathf.Min(1280,Mathf.Max(320,Screen.height))*aspect);
            int h=aspect>=1?Mathf.RoundToInt(w/aspect):Mathf.Min(1280,Mathf.Max(320,Screen.height));
            w=Mathf.Max(128,w);h=Mathf.Max(128,h);
            if(texture!=null && width==w && height==h)return;
            if(texture!=null){shotCamera.targetTexture=null;texture.Release();Destroy(texture);}
            texture=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){name="Rouge Cinematic Frame",antiAliasing=1};texture.Create();width=w;height=h;
            shotCamera.targetTexture=texture;shotCamera.aspect=w/(float)h;image.texture=texture;
        }
        void FocusVisibility()
        {
            foreach(var state in rendererStates)
                if(state.Key!=null && !state.Key.transform.IsChildOf(battle.player.visual))state.Key.forceRenderingOff=true;
        }
        void RestoreRenderers(){foreach(var state in rendererStates)if(state.Key!=null)state.Key.forceRenderingOff=state.Value;}
        void HideCanvas(Canvas canvas)
        {
            if(canvas==null)return;
            var raycaster=canvas.GetComponent<GraphicRaycaster>();
            canvasStates.Add(new CanvasState{canvas=canvas,enabled=canvas.enabled,raycaster=raycaster,raycasts=raycaster!=null && raycaster.enabled});
            canvas.enabled=false;if(raycaster!=null)raycaster.enabled=false;
        }
        void ComposeShot()
        {
            var arena=battle.arenaRoot;var caster=battle.player;
            Vector3 up=arena.up;
            Vector3 direction=target!=null?Vector3.ProjectOnPlane(target.transform.position-caster.transform.position,up).normalized:caster.visual.forward;
            if(direction.sqrMagnitude<.01f)direction=arena.forward;
            Vector3 right=Vector3.Cross(up,direction).normalized;
            Bounds bounds=VisualBounds(caster);
            Vector3 eyeDirection;
            float fov;
            if(CurrentShot==Shot.CasterFocus)
            {
                float arc=Mathf.Lerp(-.32f,-.12f,Mathf.Clamp01(elapsed/1.55f));
                eyeDirection=(direction+right*arc+up*.18f).normalized;fov=38;
            }
            else if(CurrentShot==Shot.Salvo)
            {
                if(target!=null)bounds.Encapsulate(VisualBounds(target));
                eyeDirection=(right*1.05f-direction*.4f+up*.45f).normalized;fov=43;
            }
            else
            {
                foreach(var enemy in battle.enemies)if(enemy!=null)bounds.Encapsulate(VisualBounds(enemy));
                eyeDirection=(right*.85f-direction*.65f+up*.65f).normalized;fov=43;
            }
            FitBounds(bounds,eyeDirection,up,fov,CurrentShot==Shot.CasterFocus?1.35f:1.28f);
            if(CurrentShot==Shot.Salvo)HideOccluders(caster);
        }
        public Bounds CasterBounds => battle!=null?VisualBounds(battle.player):new Bounds();
        static Bounds VisualBounds(DemoFighter actor)
        {
            Bounds bounds=new Bounds(actor.AimPoint,Vector3.zero);bool found=false;
            foreach(var renderer in actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if(!renderer.enabled || !renderer.gameObject.activeInHierarchy)continue;
                if(!found){bounds=renderer.bounds;found=true;}else bounds.Encapsulate(renderer.bounds);
            }
            if(!found)bounds=new Bounds(actor.AimPoint,Vector3.one*actor.height*Mathf.Abs(actor.transform.lossyScale.x));
            return bounds;
        }
        void FitBounds(Bounds bounds,Vector3 eyeDirection,Vector3 up,float fov,float margin)
        {
            Quaternion rotation=Quaternion.LookRotation(-eyeDirection,up);
            Vector3 right=rotation*Vector3.right,vertical=rotation*Vector3.up;
            float tangent=Mathf.Tan(fov*.5f*Mathf.Deg2Rad),horizontal=tangent*shotCamera.aspect;
            float distance=.01f;
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
            {
                Vector3 offset=Vector3.Scale(bounds.extents,new Vector3(x,y,z));
                float depth=Vector3.Dot(offset,eyeDirection);
                distance=Mathf.Max(distance,depth+Mathf.Abs(Vector3.Dot(offset,right))/horizontal*margin,depth+Mathf.Abs(Vector3.Dot(offset,vertical))/tangent*margin);
            }
            distance+=Mathf.Max(.01f,battle.player.height*Mathf.Abs(battle.arenaRoot.lossyScale.x)*.1f);
            shotCamera.fieldOfView=fov;
            shotCamera.transform.SetPositionAndRotation(bounds.center+eyeDirection*distance,rotation);
        }
        void HideOccluders(DemoFighter caster)
        {
            RestoreRenderers();
            Vector3 aim=VisualBounds(caster).center,rayDirection=aim-shotCamera.transform.position;
            float length=rayDirection.magnitude;var ray=new Ray(shotCamera.transform.position,rayDirection.normalized);
            foreach(var enemy in battle.enemies)
                if(enemy!=null)foreach(var renderer in enemy.visual.GetComponentsInChildren<Renderer>(true))
                    if(renderer.enabled && renderer.bounds.IntersectRay(ray,out float hit) && hit<length-.05f*Mathf.Abs(battle.arenaRoot.lossyScale.x))renderer.forceRenderingOff=true;
        }
        public void End()
        {
            RestoreRenderers();rendererStates.Clear();
            foreach(var state in canvasStates)
            {if(state.canvas!=null)state.canvas.enabled=state.enabled;if(state.raycaster!=null)state.raycaster.enabled=state.raycasts;}
            canvasStates.Clear();
            if(shotCamera!=null)shotCamera.enabled=false;
            if(image!=null)image.gameObject.SetActive(false);
            Active=false;
        }
        void OnDisable(){End();}
        void OnDestroy(){End();if(texture!=null){texture.Release();Destroy(texture);}}
    }
}
