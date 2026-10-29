using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;

namespace UnicornDemo
{
    public sealed class UnicornDemoController : MonoBehaviour
    {
        public GameObject unicornPrefab;
        public Material effectTemplate, floorTemplate;
        public UnicornMotion Robot { get; private set; }
        public bool UsingAR { get; private set; }
        public string Status { get; private set; }
        public Vector2 TestMoveInput { get; set; }
        public int SpawnCount { get; private set; }
        Transform arena, floor;
        Camera orbitCamera, arCamera;
        GameObject arRig, arSessionObject, reticle;
        ARSession arSession;
        ARRaycastManager raycasts;
        ARPlaneManager planes;
        ARAnchorManager anchors;
        ARAnchor anchor;
        DemoJoystick joystick;
        Text statusText, modeText, actionText, fpsText;
        Font font;
        Material floorMaterial, lineMaterial, reticleMaterial;
        readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
        bool arStarting, placed, sawTracking;
        Pose candidate;
        float yaw = 18f, pitch = 17f, distance = 5.5f, fpsTime, fpsValue;
        int fpsFrames;
        Vector2 previousPointer;
        bool orbiting;
        bool wasMoving;
        bool wasLanding;
        Vector3 spawnPosition;
        float arenaRadius = 3.8f;
        static readonly Color Cyan = new Color(.24f, .85f, 1f);

        void Start()
        {
            Application.targetFrameRate = 30;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            QualitySettings.vSyncCount = 0;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            floorMaterial = MakeMaterial("Universal Render Pipeline/Lit", new Color(.055f, .09f, .13f));
            lineMaterial = MakeMaterial("Universal Render Pipeline/Unlit", new Color(.1f, .27f, .33f));
            reticleMaterial = MakeMaterial("Universal Render Pipeline/Unlit", Cyan);
            BuildWorld();
            BuildUI();
            SetStatus("Sân ảo sẵn sàng. Unicorn đang hạ cánh.");
            SpawnRobot(Vector3.zero);
            Debug.Log("UNICORN_DEMO READY | " + SystemInfo.deviceModel + " | " + SystemInfo.operatingSystem + " | Virtual arena");
        }
        Material MakeMaterial(string shader, Color color)
        {
            var template=shader.EndsWith("/Unlit")?effectTemplate:floorTemplate;
            var m = template?new Material(template):new Material(Shader.Find(shader));
            m.color = color;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", .15f);
            return m;
        }
        void BuildWorld()
        {
            arena = new GameObject("ArenaRoot").transform;
            var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "Virtual test floor"; floor = slab.transform;
            floor.SetParent(arena, false); floor.localPosition = new Vector3(0, -.065f, 0); floor.localScale = new Vector3(8, .12f, 8);
            slab.GetComponent<Renderer>().sharedMaterial = floorMaterial;
            for (int i = -4; i <= 4; i++)
            {
                Line("Grid X", new Vector3(i, .002f, -4), new Vector3(i, .002f, 4), .008f, lineMaterial);
                Line("Grid Z", new Vector3(-4, .002f, i), new Vector3(4, .002f, i), .008f, lineMaterial);
            }
            var pad = new GameObject("Deployment pad").AddComponent<LineRenderer>();
            pad.transform.SetParent(arena, false); pad.useWorldSpace = false; pad.loop = true;
            pad.positionCount = 96; pad.widthMultiplier = .018f; pad.sharedMaterial = reticleMaterial;
            for (int i=0;i<96;i++) { float a=i*Mathf.PI*2/96; pad.SetPosition(i,new Vector3(Mathf.Cos(a)*1.15f,.012f,Mathf.Sin(a)*1.15f)); }
            orbitCamera = new GameObject("Demo camera").AddComponent<Camera>();
            orbitCamera.tag = "MainCamera"; orbitCamera.clearFlags = CameraClearFlags.SolidColor;
            orbitCamera.backgroundColor = new Color(.018f,.03f,.06f); orbitCamera.fieldOfView=40;
            orbitCamera.nearClipPlane=.05f; orbitCamera.farClipPlane=50;
            orbitCamera.gameObject.AddComponent<AudioListener>();
            UpdateCamera();
            if (FindAnyObjectByType<Light>() == null)
            {
                var light = new GameObject("Key light").AddComponent<Light>();
                light.type=LightType.Directional; light.intensity=1.4f; light.transform.rotation=Quaternion.Euler(45,155,0); light.shadows=LightShadows.None;
            }
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.6f,.65f,.75f);
        }
        void Line(string label, Vector3 a, Vector3 b, float width, Material material)
        {
            var l=new GameObject(label).AddComponent<LineRenderer>(); l.transform.SetParent(arena,false);
            l.useWorldSpace=false; l.positionCount=2; l.SetPosition(0,a);l.SetPosition(1,b);l.widthMultiplier=width;l.sharedMaterial=material;
        }
        public void Respawn()
        {
            if (UsingAR && !placed) { SetStatus("Quét mặt bàn rồi bấm Đặt tại đây."); return; }
            if (joystick) joystick.ResetInput();
            TestMoveInput=Vector2.zero;
            if (Robot) { Robot.transform.localPosition=spawnPosition;Robot.transform.localRotation=Quaternion.identity;Robot.Spawn();SpawnCount++; }
            else SpawnRobot(spawnPosition);
            SetStatus("Unicorn đang hạ cánh. Chờ vòng sáng kết thúc.");
        }
        void SpawnRobot(Vector3 point)
        {
            if (!unicornPrefab) { SetStatus("Thiếu prefab Unicorn trong scene.");return; }
            var go=Instantiate(unicornPrefab,arena); go.name="Unicorn Demo";
            go.transform.localPosition=point;go.transform.localRotation=Quaternion.identity;
            Robot=go.GetComponent<UnicornMotion>(); spawnPosition=point;SpawnCount++;
            Robot.Spawn();
        }
        public void ToggleInspection()
        {
            if (!Robot || Robot.IsLanding) return;
            Robot.SetExploded(!Robot.IsExploded);
            SetStatus(Robot.IsExploded?"Đang xem các cụm cơ khí. Bấm Tách / ráp để hoàn nguyên.":"Đã ráp lại. Có thể tiếp tục di chuyển.");
        }
        public void Attack() { if(Robot) Robot.Attack(); }
        public void Boost() { if(Robot) Robot.Burst(); }
        public void ResetArena()
        {
            if (joystick) joystick.ResetInput();TestMoveInput=Vector2.zero;
            if (UsingAR)
            {
                placed=false;sawTracking=false;
                if(Robot) Destroy(Robot.gameObject);
                Robot=null;
                arena.SetParent(null,true);arena.gameObject.SetActive(false);
                if(anchor) Destroy(anchor.gameObject);
                anchor=null;planes.enabled=true;
                SetStatus("Quét lại mặt bàn. Chọn vị trí rồi bấm Đặt tại đây.");
            }
            else { yaw=18;pitch=17;distance=5.5f;Respawn(); }
        }
        void Update()
        {
            if (statusText==null) return;
            fpsFrames++;fpsTime+=Time.unscaledDeltaTime;
            if(fpsTime>=.5f) {fpsValue=fpsFrames/fpsTime;fpsTime=0;fpsFrames=0;fpsText.text=Mathf.RoundToInt(fpsValue)+" FPS";}
            var keyboard=Keyboard.current;
            Vector2 input=joystick.Value+TestMoveInput;
            if(keyboard!=null)
            {
                input.x+=(keyboard.dKey.isPressed||keyboard.rightArrowKey.isPressed?1:0)-(keyboard.aKey.isPressed||keyboard.leftArrowKey.isPressed?1:0);
                input.y+=(keyboard.wKey.isPressed||keyboard.upArrowKey.isPressed?1:0)-(keyboard.sKey.isPressed||keyboard.downArrowKey.isPressed?1:0);
                if(keyboard.spaceKey.wasPressedThisFrame) Attack();
                if(keyboard.rKey.wasPressedThisFrame) Respawn();
                if(keyboard.eKey.wasPressedThisFrame) ToggleInspection();
            }
            input=Vector2.ClampMagnitude(input,1);
            bool trackingOK=!UsingAR||ARSession.state==ARSessionState.SessionTracking;
            if(UsingAR) UpdateAR(); else HandleOrbit();
            if(Robot)
            {
                if(wasLanding&&!Robot.IsLanding) SetStatus("Sẵn sàng. Dùng joystick để di chuyển; kéo vùng trống để đổi góc nhìn.");
                wasLanding=Robot.IsLanding;
                bool mayMove=trackingOK&&!Robot.IsLanding&&!Robot.IsExploded&&!Robot.IsAttacking;
                Robot.SetMovement(mayMove?input.magnitude:0);
                if(mayMove&&input.sqrMagnitude>.01f)
                {
                    var cam=UsingAR?arCamera:orbitCamera;
                    Vector3 forward=arena.InverseTransformDirection(cam.transform.forward);forward.y=0;forward.Normalize();
                    Vector3 right=new Vector3(forward.z,0,-forward.x);
                    Vector3 move=right*input.x+forward*input.y;
                    var next=Robot.transform.localPosition+move*1.05f*Time.deltaTime;
                    var flat=Vector2.ClampMagnitude(new Vector2(next.x,next.z),arenaRadius);
                    Robot.transform.localPosition=new Vector3(flat.x,0,flat.y);
                    Robot.transform.localRotation=Quaternion.Slerp(Robot.transform.localRotation,Quaternion.LookRotation(move),Time.deltaTime*10);
                }
                bool moving=mayMove&&input.sqrMagnitude>.01f;
                if(moving!=wasMoving){Debug.Log("UNICORN_DEMO "+(moving?"MOVE_START":"MOVE_STOP")+" position="+Robot.transform.localPosition);wasMoving=moving;}
                actionText.text=Robot.IsLanding?"HẠ CÁNH":Robot.IsExploded?"KIỂM TRA CƠ KHÍ":Robot.IsAttacking?"TẤN CÔNG":input.sqrMagnitude>.01f?"DI CHUYỂN":"ĐỨNG CHỜ";
            }
            else actionText.text="CHỜ ĐẶT GUNDAM";
            modeText.text=UsingAR?"CAMERA AR":"SÂN ẢO  •  KHÔNG CẦN ARCORE";
        }
        void HandleOrbit()
        {
            bool pressed=false;Vector2 point=Vector2.zero;
            if(Mouse.current!=null&&Mouse.current.rightButton.isPressed) {pressed=true;point=Mouse.current.position.ReadValue();}
            if(Touchscreen.current!=null)
            {
                var t=Touchscreen.current.primaryTouch;var p=t.position.ReadValue();
                bool onUI=EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject(t.touchId.ReadValue());
                if(t.press.isPressed&&p.x>Screen.width*.25f&&p.y>Screen.height*.25f&&!onUI){pressed=true;point=p;}
            }
            if(pressed&&orbiting) {Vector2 d=point-previousPointer;yaw+=d.x*.2f;pitch=Mathf.Clamp(pitch-d.y*.15f,5,55);}
            previousPointer=point;orbiting=pressed;
            if(Mouse.current!=null) distance=Mathf.Clamp(distance-Mouse.current.scroll.ReadValue().y*.003f,3.2f,8);
            UpdateCamera();
        }
        void UpdateCamera()
        {
            var target=(Robot?Robot.transform.position:Vector3.zero)+Vector3.up*1.05f;
            orbitCamera.transform.position=target+Quaternion.Euler(-pitch,yaw,0)*new Vector3(0,0,distance);
            orbitCamera.transform.LookAt(target);
        }
        public void BeginAR()
        {
            if(arStarting||UsingAR)return;
            if(Application.isEditor){SetStatus("Editor đang dùng sân ảo. Camera AR cần điện thoại hỗ trợ ARCore/ARKit.");return;}
            StartCoroutine(StartAR());
        }
        IEnumerator StartAR()
        {
            arStarting=true;SetStatus("Đang kiểm tra hỗ trợ AR. Có thể cần cho phép camera.");
            var manager=XRGeneralSettings.Instance!=null?XRGeneralSettings.Instance.Manager:null;
            if(manager==null) {ARFailed("Chưa có XR loader. Tiếp tục dùng sân ảo.");yield break;}
            if(manager.activeLoader==null) yield return manager.InitializeLoader();
            if(manager.activeLoader==null) {ARFailed("Máy không khởi tạo được AR. Sân ảo vẫn dùng được.");yield break;}
            manager.StartSubsystems();
            yield return ARSession.CheckAvailability();
            if(ARSession.state==ARSessionState.NeedsInstall)
            { ARFailed("Máy cần Google Play Services for AR. Bản thử tiếp tục ở sân ảo.");yield break; }
            if(ARSession.state==ARSessionState.Unsupported||ARSession.state==ARSessionState.None)
            { ARFailed("Điện thoại không hỗ trợ ARCore. Tiếp tục dùng sân ảo.");yield break; }
            BuildARRig();UsingAR=true;arStarting=false;placed=false;
            if(Robot) Destroy(Robot.gameObject);Robot=null;
            arena.gameObject.SetActive(false);orbitCamera.gameObject.SetActive(false);
            SetStatus("Quét mặt bàn. Khi có vòng ngắm, bấm Đặt tại đây.");
        }
        void ARFailed(string message)
        {
            var m=XRGeneralSettings.Instance!=null?XRGeneralSettings.Instance.Manager:null;
            if(m!=null&&m.activeLoader!=null){m.StopSubsystems();m.DeinitializeLoader();}
            arStarting=false;SetStatus(message);
        }
        void BuildARRig()
        {
            arSessionObject=new GameObject("AR Session");arSession=arSessionObject.AddComponent<ARSession>();
            arRig=new GameObject("AR Origin");var origin=arRig.AddComponent<XROrigin>();
            var offset=new GameObject("Camera Offset");offset.transform.SetParent(arRig.transform,false);origin.CameraFloorOffsetObject=offset;origin.CameraYOffset=0;
            var cameraGO=new GameObject("AR Camera");cameraGO.transform.SetParent(offset.transform,false);
            arCamera=cameraGO.AddComponent<Camera>();arCamera.tag="MainCamera";arCamera.nearClipPlane=.01f;arCamera.farClipPlane=50;
            cameraGO.AddComponent<ARCameraManager>();cameraGO.AddComponent<ARCameraBackground>();
            var driver=cameraGO.AddComponent<TrackedPoseDriver>();
            driver.positionInput=new InputActionProperty(new InputAction("AR position",binding:"<XRHMD>/centerEyePosition"));
            driver.rotationInput=new InputActionProperty(new InputAction("AR rotation",binding:"<XRHMD>/centerEyeRotation"));
            driver.trackingStateInput=new InputActionProperty(new InputAction("AR tracking",binding:"<XRHMD>/trackingState"));
            origin.Camera=arCamera;
            planes=arRig.AddComponent<ARPlaneManager>();planes.requestedDetectionMode=PlaneDetectionMode.Horizontal;
            raycasts=arRig.AddComponent<ARRaycastManager>();anchors=arRig.AddComponent<ARAnchorManager>();
            reticle=GameObject.CreatePrimitive(PrimitiveType.Cylinder);reticle.name="AR placement reticle";
            Destroy(reticle.GetComponent<Collider>());reticle.GetComponent<Renderer>().sharedMaterial=reticleMaterial;
            reticle.transform.localScale=new Vector3(.14f,.002f,.14f);reticle.SetActive(false);
        }
        void UpdateAR()
        {
            if(!placed)
            {
                bool hit=raycasts.Raycast(new Vector2(Screen.width*.5f,Screen.height*.5f),hits,TrackableType.PlaneWithinPolygon);
                reticle.SetActive(hit);
                if(hit){candidate=hits[0].pose;reticle.transform.SetPositionAndRotation(candidate.position,candidate.rotation);}
            }
            else if(ARSession.state!=ARSessionState.SessionTracking)
            {sawTracking=false;SetStatus("Tracking tạm mất. Giữ máy ổn định và quét lại mặt bàn.");}
            else if(!sawTracking){sawTracking=true;SetStatus("AR đã ổn định. Joystick di chuyển; chạm nút để thử chuyển động.");}
        }
        public async void PlaceAR()
        {
            if(!UsingAR){SetStatus("Sân ảo đã có sẵn. Bấm Gọi xuống để phát lại hạ cánh.");return;}
            if(placed||reticle==null||!reticle.activeSelf)return;
            placed=true;
            var created=await anchors.TryAddAnchorAsync(candidate);
            if(!this)return;
            if(!created.status.IsSuccess()) {placed=false;SetStatus("Chưa tạo được anchor. Thử quét và đặt lại.");return;}
            anchor=created.value;arena.SetParent(anchor.transform,false);arena.localPosition=Vector3.zero;arena.localRotation=Quaternion.identity;
            arena.localScale=Vector3.one*.12f;
            floor.gameObject.SetActive(false);
            foreach(var line in arena.GetComponentsInChildren<LineRenderer>())line.gameObject.SetActive(false);
            arena.gameObject.SetActive(true);reticle.SetActive(false);planes.enabled=false;sawTracking=true;
            SpawnRobot(Vector3.zero);SetStatus("Đã đặt sân AR. Bấm Đặt lại để quét vị trí khác.");
        }
        void SetStatus(string message){if(Status!=message)Debug.Log("UNICORN_DEMO STATUS | "+message);Status=message;if(statusText)statusText.text=message;}
        RectTransform Rect(string label, Transform parent, Vector2 anchorPoint, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var rt=new GameObject(label,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(parent,false);
            rt.anchorMin=rt.anchorMax=anchorPoint;rt.pivot=pivot;rt.anchoredPosition=position;rt.sizeDelta=size;return rt;
        }
        Text Label(string text, Transform parent, Vector2 anchorPoint, Vector2 pivot, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var rt=Rect("Label",parent,anchorPoint,pivot,position,size);var t=rt.gameObject.AddComponent<Text>();t.font=font;t.text=text;t.fontSize=fontSize;t.color=color;t.raycastTarget=false;t.alignment=TextAnchor.MiddleLeft;return t;
        }
        void Button(string title, Transform parent, Vector2 anchorPoint, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action, bool primary=false)
        {
            var rt=Rect(title,parent,anchorPoint,new Vector2(1,0),position,size);
            var image=rt.gameObject.AddComponent<Image>();image.color=primary?new Color(.12f,.59f,.7f):new Color(.075f,.13f,.2f,.96f);
            var button=rt.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=image;button.onClick.AddListener(action);
            var colors=button.colors;colors.pressedColor=new Color(.45f,.8f,.9f);colors.highlightedColor=new Color(.8f,.95f,1);button.colors=colors;
            var label=Label(title,rt,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,size,22,Color.white);label.alignment=TextAnchor.MiddleCenter;
        }
        void BuildUI()
        {
            if(!EventSystem.current){var ev=new GameObject("EventSystem");ev.AddComponent<EventSystem>();ev.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();}
            var canvasGO=new GameObject("Unicorn demo interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=canvasGO.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGO.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.6f;
            var safe=Rect("Safe area",canvasGO.transform,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero);
            var sr=Screen.safeArea;safe.anchorMin=new Vector2(sr.xMin/Screen.width,sr.yMin/Screen.height);safe.anchorMax=new Vector2(sr.xMax/Screen.width,sr.yMax/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;
            var top=Rect("Top panel",safe,new Vector2(0,1),new Vector2(0,1),new Vector2(24,-18),new Vector2(780,138));
            top.gameObject.AddComponent<Image>().color=new Color(.025f,.055f,.1f,.92f);
            Label("UNICORN",top,new Vector2(0,1),new Vector2(0,1),new Vector2(22,-12),new Vector2(520,57),42,Color.white).fontStyle=FontStyle.Bold;
            Label("GUNDAM  /  BẢN THỬ CHUYỂN ĐỘNG",top,new Vector2(0,1),new Vector2(0,1),new Vector2(24,-69),new Vector2(700,30),19,Cyan);
            modeText=Label("SÂN ẢO",top,new Vector2(0,1),new Vector2(0,1),new Vector2(24,-103),new Vector2(740,24),17,new Color(.7f,.78f,.87f));
            fpsText=Label("",safe,new Vector2(1,1),new Vector2(1,1),new Vector2(-28,-22),new Vector2(130,30),19,Cyan);fpsText.alignment=TextAnchor.MiddleRight;
            actionText=Label("",safe,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(140,-185),new Vector2(430,35),22,Cyan);actionText.alignment=TextAnchor.MiddleCenter;
            var joyRect=Rect("Joystick",safe,Vector2.zero,new Vector2(0,0),new Vector2(38,47),new Vector2(184,184));
            joyRect.gameObject.AddComponent<Image>().color=new Color(.07f,.16f,.22f,.92f);
            joystick=joyRect.gameObject.AddComponent<DemoJoystick>();
            var knob=Rect("Handle",joyRect,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(70,70));knob.gameObject.AddComponent<Image>().color=Cyan;joystick.handle=knob;
            Label("DI CHUYỂN",safe,Vector2.zero,Vector2.zero,new Vector2(41,237),new Vector2(200,30),19,Color.white);
            Button("GỌI XUỐNG",safe,new Vector2(1,0),new Vector2(-24,46),new Vector2(224,64),Respawn,true);
            Button("ĐÁNH THỬ",safe,new Vector2(1,0),new Vector2(-24,122),new Vector2(224,64),Attack);
            Button("TÁCH / RÁP",safe,new Vector2(1,0),new Vector2(-24,198),new Vector2(224,64),ToggleInspection);
            Button("HIỆU ỨNG LÕI",safe,new Vector2(1,0),new Vector2(-262,46),new Vector2(224,64),Boost);
            Button("ĐẶT LẠI",safe,new Vector2(1,0),new Vector2(-262,122),new Vector2(224,64),ResetArena);
            Button("THỬ CAMERA AR",safe,new Vector2(1,0),new Vector2(-24,274),new Vector2(224,56),BeginAR);
            Button("ĐẶT TẠI ĐÂY",safe,new Vector2(1,0),new Vector2(-262,198),new Vector2(224,64),PlaceAR);
            statusText=Label("",safe,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(-105,12),new Vector2(930,38),19,new Color(.82f,.89f,.94f));statusText.alignment=TextAnchor.MiddleCenter;
            Label("Kéo vùng trống để xoay góc nhìn  •  PC: WASD / chuột phải / Space",safe,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(-120,60),new Vector2(810,54),17,new Color(.65f,.75f,.83f)).alignment=TextAnchor.MiddleCenter;
        }
        void OnApplicationPause(bool paused){if(paused&&joystick)joystick.ResetInput();TestMoveInput=Vector2.zero;}
        void OnDestroy()
        {
            foreach(var m in new[]{floorMaterial,lineMaterial,reticleMaterial})if(m)Destroy(m);
        }
    }
}
