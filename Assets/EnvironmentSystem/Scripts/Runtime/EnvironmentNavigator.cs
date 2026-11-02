using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TurnBasedBattle;

namespace ProjectEnvironments
{
    // One navigator survives scene changes. No extra Camera, AudioListener or EventSystem.
    public sealed class EnvironmentNavigator : MonoBehaviour
    {
        public const string PreferenceKey = "EnvironmentSystem.LastSelection";
        public static EnvironmentNavigator Instance { get; private set; }
        public EnvironmentCatalog Catalog { get; private set; }
        public bool Busy { get; private set; }
        public string Error { get; private set; } = "";
        public float Progress { get; private set; }
        public string LastSelectedId => PlayerPrefs.GetString(PreferenceKey, "");
        EnvironmentDefinition pending;
        Vector2 scroll;
        GUIStyle heading, body, card, button, cardTitle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatic() { Instance = null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var catalog = Resources.Load<EnvironmentCatalog>("EnvironmentCatalog");
            if(!catalog || !Recognized(catalog,SceneManager.GetActiveScene().path)) return;
            if(!Instance) new GameObject("Environment Navigator").AddComponent<EnvironmentNavigator>();
        }
        static bool Recognized(EnvironmentCatalog c,string path)
        {
            if(path==c.hubScenePath) return true;
            foreach(var d in c.environments) if(d && d.ScenePath==path) return true;
            return false;
        }
        void Awake()
        {
            if(Instance && Instance!=this) { Destroy(gameObject); return; }
            Instance=this; DontDestroyOnLoad(gameObject);
            Catalog=Resources.Load<EnvironmentCatalog>("EnvironmentCatalog");
        }
        void OnDestroy() { if(Instance==this) Instance=null; }
        public bool Select(string id)
        {
            if(Busy || !Catalog) return false;
            var d=Catalog.Find(id);
            if(!d || !d.available) { Error="Môi trường chưa sẵn sàng."; return false; }
            return Begin(d.ScenePath,d);
        }
        public bool OpenHub() => Catalog && Begin(Catalog.hubScenePath,null);
        bool Begin(string path,EnvironmentDefinition definition)
        {
            if(Busy) return false;
            if(string.IsNullOrEmpty(path) || !Application.CanStreamedLevelBeLoaded(path))
            { Error="Không mở được scene. Kiểm tra danh sách scene trong Build Profile."; return false; }
            Error=""; Busy=true; Progress=0; pending=definition;
            StartCoroutine(Load(path)); return true;
        }
        IEnumerator Load(string path)
        {
            // Let the loading overlay render before beginning the asynchronous load.
            yield return null;
            AsyncOperation operation=null;
            try { operation=SceneManager.LoadSceneAsync(path,LoadSceneMode.Single); }
            catch(Exception e) { Error="Tải môi trường thất bại: "+e.Message; }
            if(operation==null) { Busy=false; pending=null; yield break; }
            while(!operation.isDone) { Progress=Mathf.Clamp01(operation.progress/.9f); yield return null; }
            yield return null; // Allow the new scene's Awake/Start and existing scenery binding.
            if(pending)
            {
                bool valid=true;
                if(pending.battleVariant>=0)
                {
                    var switcher=FindAnyObjectByType<BattleEnvironmentSwitcher>();
                    valid=switcher && pending.battleVariant<switcher.Count;
                    if(valid) switcher.Apply(pending.battleVariant);
                    else Error="Scene đã mở, nhưng không tìm thấy biến thể môi trường tương ứng.";
                }
                if(valid) { PlayerPrefs.SetString(PreferenceKey,pending.id); PlayerPrefs.Save(); }
            }
            Progress=1; Busy=false; pending=null;
        }
        void Styles()
        {
            if(heading!=null) return;
            heading=new GUIStyle(GUI.skin.label){fontSize=32,fontStyle=FontStyle.Bold}; heading.normal.textColor=Color.white;
            body=new GUIStyle(GUI.skin.label){fontSize=17,wordWrap=true}; body.normal.textColor=new Color(.72f,.82f,.87f);
            cardTitle=new GUIStyle(body){fontSize=19,fontStyle=FontStyle.Bold}; cardTitle.normal.textColor=Color.white;
            card=new GUIStyle(GUI.skin.box){padding=new RectOffset(18,18,16,16)};
            button=new GUIStyle(GUI.skin.button){fontSize=18,fontStyle=FontStyle.Bold};
        }
        void OnGUI()
        {
            if(!Catalog || !Recognized(Catalog,SceneManager.GetActiveScene().path)) return;
            Styles(); var matrix=GUI.matrix; var originalColor=GUI.color; var originalEnabled=GUI.enabled;
            if(SceneManager.GetActiveScene().path==Catalog.hubScenePath || Busy)
            { GUI.color=new Color(.035f,.065f,.095f); GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture); GUI.color=originalColor; }
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2,0),Quaternion.identity,Vector3.one*scale);
            bool hub=SceneManager.GetActiveScene().path==Catalog.hubScenePath;
            if(hub)
            {
                GUI.color=new Color(.035f,.065f,.095f); GUI.DrawTexture(new Rect(0,0,1280,720),Texture2D.whiteTexture); GUI.color=Color.white;
                GUI.Label(new Rect(48,28,1100,48),"CHỌN MÔI TRƯỜNG",heading);
                GUI.Label(new Rect(50,82,1180,46),"Mở một bản đồ để xây dựng hoặc trải nghiệm. Chuyển scene sẽ bắt đầu lại trận đấu hiện tại.",body);
                float height=Mathf.Max(490,Mathf.Ceil(Catalog.environments.Length/3f)*230);
                scroll=GUI.BeginScrollView(new Rect(40,142,1200,505),scroll,new Rect(0,0,1170,height));
                for(int i=0;i<Catalog.environments.Length;i++)
                {
                    var d=Catalog.environments[i]; if(!d) continue;
                    float x=(i%3)*388,y=(i/3)*230; GUI.Box(new Rect(x,y,372,214),"",card);
                    GUI.color=d.accent; GUI.DrawTexture(new Rect(x+12,y+12,348,48),d.thumbnail?d.thumbnail:Texture2D.whiteTexture,ScaleMode.ScaleAndCrop); GUI.color=Color.white;
                    GUI.Label(new Rect(x+18,y+67,335,29),d.displayName,cardTitle);
                    GUI.Label(new Rect(x+18,y+100,335,58),d.description,body);
                    GUI.enabled=!Busy && d.available;
                    string label=!d.available?"CHƯA DỰNG":d.id==LastSelectedId?"MỞ LẠI":"MỞ MÔI TRƯỜNG";
                    if(GUI.Button(new Rect(x+18,y+165,335,36),label,button)) Select(d.id);
                    GUI.enabled=originalEnabled;
                }
                GUI.EndScrollView();
                GUI.Label(new Rect(50,665,1180,40),string.IsNullOrEmpty(Error)?"Thêm map mới bằng Environment Definition và Environment Catalog.":Error,body);
            }
            else
            {
                GUI.enabled=!Busy;
                if(GUI.Button(new Rect(1015,12,250,42),"CHỌN MÔI TRƯỜNG",button)) OpenHub();
                GUI.enabled=originalEnabled;
                if(!string.IsNullOrEmpty(Error)) GUI.Box(new Rect(280,64,720,65),Error,body);
            }
            if(Busy)
            {
                GUI.color=new Color(.02f,.04f,.06f,.97f); GUI.DrawTexture(new Rect(0,0,1280,720),Texture2D.whiteTexture); GUI.color=Color.white;
                GUI.Label(new Rect(390,280,600,50),"ĐANG TẢI MÔI TRƯỜNG…",heading);
                GUI.Box(new Rect(390,355,500,24),""); GUI.color=new Color(.2f,.85f,.92f);
                GUI.DrawTexture(new Rect(393,358,494*Progress,18),Texture2D.whiteTexture);
            }
            GUI.matrix=matrix; GUI.color=originalColor; GUI.enabled=originalEnabled;
        }
    }
}
