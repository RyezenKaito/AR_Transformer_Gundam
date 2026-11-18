using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GundamGame.CombatDemo
{
    public sealed class CombatTestScene : MonoBehaviour
    {
        public CombatTestCatalog catalog;
        public GameObject opponentFixture;
        public EnemyProfile rangedEnemy, meleeEnemy;
        public int enemyPreset;
        public CharacterBattlePackage ActivePackage { get; private set; }
        public int ActiveIndex { get; private set; } = -1;
        public bool Loading { get; private set; }

        GameObject currentRoot;
        bool selectorOpen;
        string message;
        Vector2 scroll;
        Rect selectorBounds;
        const string ScenePath = "Assets/Sandbox/CombatTest/Scenes/Combat Test.unity";

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Gundam/Combat/Open Test Scene")]
        static void Open()
        {
            if(!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode &&
                UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ScenePath);
        }
#endif

        IEnumerator Start()
        {
            if(catalog && catalog.characters.Length>0)yield return LoadCharacter(0);
        }

        void Update()
        {
            if(Keyboard.current!=null && Keyboard.current.f2Key.wasPressedThisFrame &&
                (!ActivePackage || !ActivePackage.Cinematic))
                selectorOpen=!selectorOpen;
        }

        public bool SelectCharacter(int index)
        {
            if(Loading || !catalog || index<0 || index>=catalog.characters.Length || index==ActiveIndex ||
                (enemyPreset>0 && index!=0) ||
                (ActivePackage && ActivePackage.Cinematic))return false;
            StartCoroutine(LoadCharacter(index));
            return true;
        }

        public bool SelectEnemyPreset(int preset)
        {
            if(Loading || preset<0 || preset>3 || preset==enemyPreset ||
                (ActivePackage && ActivePackage.Cinematic))return false;
            enemyPreset=preset;
            StartCoroutine(LoadCharacter(preset==0?Mathf.Max(0,ActiveIndex):0));
            return true;
        }

        public bool BlocksWorldPointer(Vector2 screenPosition)
        {
            var guiPosition=new Vector2(screenPosition.x,Screen.height-screenPosition.y);
            return selectorBounds.Contains(guiPosition);
        }

        void OnDestroy()
        {
            if(currentRoot)currentRoot.SetActive(false);
        }

        IEnumerator LoadCharacter(int index)
        {
            Loading=true;
            message=null;
            var entry=catalog.characters[index];
            var request=Resources.LoadAsync<CharacterCombatProfile>(entry.resourcePath);
            yield return request;
            var profile=request.asset as CharacterCombatProfile;
            if(!profile || !profile.combatPrefab || !opponentFixture || profile.skills==null || profile.skills.Length!=4)
            {
                message="Invalid character package: "+entry.label;
                Loading=false;
                yield break;
            }
            foreach(var skill in profile.skills)
                if(!skill.animation || skill.burstCost<0 || skill.burstCost>5)
                {
                    message="Incomplete skill data: "+entry.label;
                    Loading=false;
                    yield break;
                }

            var next=new GameObject(entry.label);
            next.transform.SetParent(transform,false);
            next.SetActive(false);
            CharacterBattlePackage package;
            try
            {
                var fixture=Instantiate(opponentFixture,next.transform);
                fixture.SetActive(true);
                var opponents=fixture.GetComponentsInChildren<DemoFighter>(true);
                if(opponents.Length!=3)throw new InvalidOperationException("The shared fixture needs three opponents.");
                if(enemyPreset>0 && rangedEnemy && meleeEnemy)
                {
                    int count=enemyPreset==3?2:1;
                    var templates=opponents;
                    opponents=new DemoFighter[count];
                    for(int i=0;i<count;i++)
                    {
                        var enemy=enemyPreset==2 || (enemyPreset==3 && i%2==1)?meleeEnemy:rangedEnemy;
                        var actor=Instantiate(enemy.prefab,templates[i].transform.position,templates[i].transform.rotation,fixture.transform);
                        opponents[i]=actor.GetComponent<DemoFighter>();
                    }
                    foreach(var item in templates){item.gameObject.SetActive(false);Destroy(item.gameObject);}
                }
                package=Instantiate(profile.combatPrefab,next.transform).GetComponent<CharacterBattlePackage>();
                if(!package)throw new InvalidOperationException("Combat package component missing.");
                package.Configure(profile,opponents);
            }
            catch(Exception exception)
            {
                Destroy(next);
                message=exception.Message;
                Debug.LogException(exception,this);
                Loading=false;
                yield break;
            }

            if(currentRoot)
            {
                currentRoot.SetActive(false);
                Destroy(currentRoot);
                // Allow legacy combat UI and cinematic objects to finish teardown.
                yield return null;
                yield return null;
            }
            currentRoot=next;
            ActivePackage=package;
            ActiveIndex=index;
            selectorOpen=false;
            next.SetActive(true);
            Loading=false;
        }

        void OnGUI()
        {
            if(ActivePackage && ActivePackage.Cinematic){selectorBounds=default;return;}
            float width=Mathf.Min(320,Screen.width-24);
            var area=new Rect(Screen.width*.5f-width*.5f,12,width,selectorOpen?205:32);
            selectorBounds=area;
            GUILayout.BeginArea(area);
            string label=ActiveIndex<0?"TEST SCENE":catalog.characters[ActiveIndex].label;
            if(GUILayout.Button(Loading?"LOADING…":label+"   /   F2",GUILayout.Height(29)) && !Loading)
                selectorOpen=!selectorOpen;
            if(selectorOpen && catalog)
            {
                scroll=GUILayout.BeginScrollView(scroll);
                for(int i=0;i<catalog.characters.Length;i++)
                {
                    bool enabled=GUI.enabled;
                    GUI.enabled=!Loading && i!=ActiveIndex && (enemyPreset==0||i==0);
                    if(GUILayout.Button(catalog.characters[i].label,GUILayout.Height(30)))SelectCharacter(i);
                    GUI.enabled=enabled;
                }
                GUILayout.EndScrollView();
                if(!Loading)
                {
                    int preset=GUILayout.SelectionGrid(enemyPreset,new[]{"Fixture","Ranged","Melee","Both"},4);
                    if(preset!=enemyPreset)SelectEnemyPreset(preset);
                }
            }
            if(!string.IsNullOrEmpty(message))GUILayout.Label(message);
            GUILayout.EndArea();
        }
    }
}
