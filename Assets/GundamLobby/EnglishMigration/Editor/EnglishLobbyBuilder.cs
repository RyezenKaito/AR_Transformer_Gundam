using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby.Editor
{
    public static class EnglishLobbyBuilder
    {
        const string Root="Assets/GundamLobby/EnglishMigration";
        static TMP_FontAsset font;
        static List<(string vi,string en)> words;
        static Color C(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
        static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var t=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            t.SetParent(parent,false);t.anchorMin=t.anchorMax=t.pivot=new Vector2(0,1);t.anchoredPosition=new Vector2(x,-y);t.sizeDelta=new Vector2(w,h);return t;
        }
        static TMP_Text Text(string name,string value,Transform p,float x,float y,float w,float h,float size,string color="E7F0F4")
        {
            var t=Rect(name,p,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=C(color);t.raycastTarget=false;t.margin=Vector4.zero;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Ellipsis;return t;
        }
        static LobbyShape Shape(string name,Transform p,float x,float y,float w,float h,string color,string border="00000000")
        {
            var t=Rect(name,p,x,y,w,h);t.gameObject.AddComponent<CanvasRenderer>();var g=t.gameObject.AddComponent<LobbyShape>();g.geometry=LobbyShape.Geometry.Panel;g.color=C(color);g.border=C(border);g.cut=6;g.thickness=1;g.raycastTarget=false;return g;
        }
        static Button Button(string name,string title,Transform p,float x,float y,float w,float h)
        {
            var g=Shape(name,p,x,y,w,h,"0B202DF5","285164");g.raycastTarget=true;var b=g.gameObject.AddComponent<Button>();b.targetGraphic=g;b.transition=Selectable.Transition.None;
            var hover=g.gameObject.AddComponent<LobbyHover>();hover.surface=g;hover.normal=g.color;hover.hover=C("12506A");hover.pressed=C("177CA2");hover.duration=.18f;
            var text=Text("Label",title,g.transform,16,0,w-32,h,22);text.alignment=TextAlignmentOptions.MidlineLeft;return b;
        }
        static string English(string value)
        {
            if(string.IsNullOrEmpty(value))return value;
            foreach(var pair in words)value=value.Replace(pair.vi,pair.en);
            return value;
        }
        static bool Translate(UnityEngine.Object obj)
        {
            if(!obj)return false;
            var so=new SerializedObject(obj);var p=so.GetIterator();bool changed=false;
            while(p.Next(true))
            {
                if(p.propertyType!=SerializedPropertyType.String||p.name=="m_Name"||p.name=="m_EditorClassIdentifier")continue;
                string old=p.stringValue;string next=English(old);
                if(old!=next){p.stringValue=next;changed=true;}
            }
            if(changed){so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(obj);}return changed;
        }
        public static string Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first.");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!="Assets/GundamLobby/Scenes/HomeLobby.unity")throw new InvalidOperationException("Open My project HomeLobby.");
            words=File.ReadAllLines(Root+"/english.tsv").Where(l=>l.Contains("|")).Select(l=>{int n=l.IndexOf('|');return(l.Substring(0,n),l.Substring(n+1));}).OrderByDescending(p=>p.Item1.Length).ToList();
            var dash=UnityEngine.Object.FindFirstObjectByType<HomeLobbyDashboard>();
            font=dash.unitLabel.font;
            var striker=dash.striker;
            string[] ids={"Unicorn","StrikeRouge"};
            var roster=ScriptableObject.CreateInstance<StrikerCatalog>();
            roster.units=ids.Select(id=>JsonUtility.FromJson<StrikerDefinition>(JsonUtility.ToJson(striker.catalog.units.First(u=>u.id==id)))).ToArray();
            for(int i=0;i<2;i++)
            {
                var u=roster.units[i];u.role=i==0?"BALANCED ATTACKER":"RANGED ASSAULT";
                u.prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GundamLobby/ARPresentation/Game/Features/MainMenu/Presentation/Prefabs/"+ids[i]+"_AnimatedPreview.prefab");
                if(!u.prefab)throw new InvalidOperationException("Missing imported preview "+ids[i]);
                u.gradeBadge=i==0?"PERFECT GRADE [PG]":"REAL GRADE [RG]";u.hp=i==0?650:450;u.armor=i==0?190:145;u.attack=i==0?110:100;u.defense=i==0?80:68;u.speed=i==0?146:157;u.breakDamage=i==0?50:90;
                u.burstCosts=new[]{0,1,2,5};u.skillNames=i==0?new[]{"Twin Vulcan","Psycho-Frame Awakening","Beam Magnum","Psycho-Frame Full Burst"}:new[]{"V-Fin Pulse","Anti-Ship","Ootori Missiles","Full Burst"};
            }
            var oldCards=striker.cards;
            var selectedCards=ids.Select(id=>oldCards.First(c=>striker.catalog.units[c.index].id==id)).ToArray();
            foreach(var card in oldCards)card.gameObject.SetActive(selectedCards.Contains(card));
            for(int i=0;i<selectedCards.Length;i++){selectedCards[i].index=i;selectedCards[i].transform.SetAsLastSibling();selectedCards[i].Bind(roster.units[i],i==0,i==0);}
            striker.cards=selectedCards;striker.defaultIndex=0;
            var existing=AssetDatabase.LoadAssetAtPath<StrikerCatalog>(Root+"/PlayableRoster.asset");
            if(existing){EditorUtility.CopySerialized(roster,existing);UnityEngine.Object.DestroyImmediate(roster);roster=existing;}else AssetDatabase.CreateAsset(roster,Root+"/PlayableRoster.asset");
            striker.catalog=roster;
            // Preserve the existing hangar, camera, header and every other page.
            if(dash.lobby.placeholderModel)dash.lobby.placeholderModel.SetActive(false);
            var preview=(GameObject)PrefabUtility.InstantiatePrefab(roster.units[0].prefab,dash.lobby.modelPivot);
            preview.name="Playable Gundam Preview";preview.transform.localPosition=Vector3.zero;preview.transform.localRotation=Quaternion.identity;
            dash.lobby.placeholderModel=preview;dash.lobby.gundamPrefab=roster.units[0].prefab;dash.lobby.gundamName="UNICORN";dash.lobby.role=roster.units[0].role;
            var left=dash.eventTitleLabel.transform.parent;left.gameObject.SetActive(false);
            var root=left.parent;
            var oldPanel=root.Find("Gundam Core Statistics");if(oldPanel)UnityEngine.Object.DestroyImmediate(oldPanel.gameObject);
            var panel=Rect("Gundam Core Statistics",root,40,218,410,780);
            var stats=panel.gameObject.AddComponent<EnglishLobbyStats>();stats.dashboard=dash;
            var change=Button("Switch Gundam","SWITCH GUNDAM    >",panel,12,0,350,53);UnityEventTools.AddPersistentListener(change.onClick,dash.OpenStrikers);
            stats.unitName=Text("Unit Name","UNICORN",panel,12,78,360,51,40);stats.unitName.enableAutoSizing=true;stats.unitName.fontSizeMin=27;stats.unitName.fontSizeMax=40;
            stats.role=Text("Unit Role","",panel,14,134,360,27,19,"A9C3D1");
            stats.grade=Text("Grade Badge","REAL GRADE [RG]",panel,14,171,360,28,18,"82DDF8");
            Shape("Core stats rule",panel,14,212,350,1,"285164");
            stats.values=new TMP_Text[6];
            for(int i=0;i<6;i++)
            {
                float x=14+(i%2)*184,y=231+(i/2)*63;
                Text("Stat Name "+i,EnglishLobbyStats.StatNames[i],panel,x,y,170,22,17,"829EAC");
                stats.values[i]=Text("Stat Value "+i,"--",panel,x,y+22,170,34,27);
            }
            var radar=Shape("Core Stat Radar",panel,75,472,245,245,"20C8F62E","20C8F66B");radar.geometry=LobbyShape.Geometry.Radar;radar.radarRings=5;stats.radar=radar;
            stats.radarLabels=new TMP_Text[6];
            Vector2[] positions={new(124,438),new(250,490),new(250,656),new(124,732),new(-10,656),new(-10,490)};
            for(int i=0;i<6;i++){stats.radarLabels[i]=Text("Radar Label "+i,"",panel,positions[i].x,positions[i].y,145,28,17);stats.radarLabels[i].alignment=TextAlignmentOptions.Center;}
            foreach(var nav in UnityEngine.Object.FindObjectsByType<LobbyNavigationBar>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                var dropdown=nav.GetComponent<LaboratoryDropdown>()??nav.gameObject.AddComponent<LaboratoryDropdown>();dropdown.dashboard=dash;
                if(dropdown.menuRoot)UnityEngine.Object.DestroyImmediate(dropdown.menuRoot);
                var menu=Rect("Laboratory Dropdown",nav.transform,0,0,1920,1080);
                var canvas=menu.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=100;menu.gameObject.AddComponent<GraphicRaycaster>();
                dropdown.menuRoot=menu.gameObject;dropdown.group=menu.gameObject.AddComponent<CanvasGroup>();
                var blocker=Rect("Dismiss menu",menu,0,0,1920,1080).gameObject.AddComponent<Image>();blocker.color=Color.clear;dropdown.dismiss=blocker.gameObject.AddComponent<Button>();
                var bg=Shape("Dropdown Panel",menu,420,84,252,224,"071721FF","20C8F687");
                Button[] items=new Button[4];string[] names={"LOBBY","WEAPONRY","STRIKE DESIGN","UPDATE"};
                for(int i=0;i<4;i++)items[i]=Button(names[i],names[i],bg.transform,5,5+i*53,242,51);
                dropdown.lobby=items[0];dropdown.weaponry=items[1];dropdown.strikeDesign=items[2];dropdown.update=items[3];
                var label=nav.laboratory.GetComponentInChildren<TMP_Text>();label.text="LABORATORY  ▾";
                menu.gameObject.SetActive(false);
            }
            int translated=0;
            foreach(var go in scene.GetRootGameObjects())foreach(var comp in go.GetComponentsInChildren<MonoBehaviour>(true))if(Translate(comp))translated++;
            foreach(var guid in AssetDatabase.FindAssets("t:ScriptableObject",new[]{"Assets/GundamLobby"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);if(path.Contains("Font"))continue;
                var obj=AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);if(obj&&Translate(obj))translated++;
            }
            foreach(var folder in new[]{"Inventory","Weaponry","Striker","Shop","Battle","Prefabs"})
            foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/GundamLobby/"+folder}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);if(!path.Contains("/Prefabs/"))continue;
                if(path.Contains("/SourceModels/")||path.Contains("/ARPresentation/"))continue;
                var contents=PrefabUtility.LoadPrefabContents(path);bool changed=false;
                try{foreach(var comp in contents.GetComponentsInChildren<MonoBehaviour>(true))changed|=Translate(comp);if(changed)PrefabUtility.SaveAsPrefabAsset(contents,path);}finally{PrefabUtility.UnloadPrefabContents(contents);}
            }
            dash.Refresh();stats.Refresh();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            return "English UI applied; 5 Lab dropdowns; Unicorn + Strike Rouge animated previews; source core stats and radar. Translated objects: "+translated;
        }
        public static string Polish()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
            var d=UnityEngine.Object.FindFirstObjectByType<HomeLobbyDashboard>();var s=d.striker;font=d.unitLabel.font;
            words=File.ReadAllLines(Root+"/english.tsv").Where(l=>l.Contains("|")).Select(l=>{int n=l.IndexOf('|');return(l.Substring(0,n),l.Substring(n+1));}).OrderByDescending(p=>p.Item1.Length).ToList();
            for(int i=0;i<s.cards.Length;i++)
            {
                var c=s.cards[i];c.index=i;c.Bind(s.catalog.units[i],i==0,i==0);
                foreach(var t in c.GetComponentsInChildren<Transform>(true))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);
                    foreach(var comp in t.GetComponents<Component>())if(comp)PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
                }
            }
            s.catalog.units[0].gradeBadge="PERFECT GRADE [PG]";EditorUtility.SetDirty(s.catalog);
            var screen=s.powerLabel.transform.parent;
            foreach(Transform child in screen)
                if(child.name.StartsWith("Heading ")||child.name.StartsWith("Stat ")||child.name=="Power panel"||child.name=="Power label"||child.name=="Power")child.gameObject.SetActive(false);
            var old=screen.Find("Playable Unit Specification");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var specs=Rect("Playable Unit Specification",screen,1520,220,362,680);
            string[] names={"MAX HP","ATTACK","DEFENSE","SPEED","BREAK DAMAGE","ARMOR","SKILL 1","SKILL 2","SKILL 3","FULL BURST","MAX BURST","SKILL COSTS","TURN END GAIN","ULTIMATE COST"};
            s.statValues=new TMP_Text[14];int row=0;float y=0;
            string[] sections={"CORE STATS","SKILLS","TURN ECONOMY"};int[] counts={6,4,4};
            for(int group=0;group<3;group++)
            {
                var heading=Text("Heading",sections[group],specs,0,y,362,32,21,"20C8F6");heading.alignment=TextAlignmentOptions.Center;y+=42;
                for(int j=0;j<counts[group];j++,row++)
                {
                    var item=Rect("Specification "+row,specs,0,y,362,31);
                    Text("Label",names[row],item,0,0,row>=6&&row<=9?95:175,30,17,"829EAC");
                    s.statValues[row]=Text("Value","",item,row>=6&&row<=9?98:178,0,row>=6&&row<=9?264:184,30,21);
                    s.statValues[row].alignment=TextAlignmentOptions.Right;s.statValues[row].enableAutoSizing=true;s.statValues[row].fontSizeMin=12;s.statValues[row].fontSizeMax=21;
                    Shape("Rule",item,0,30,362,1,"24465487");y+=33;
                }
                y+=19;
            }
            foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            foreach(var comp in root.GetComponentsInChildren<MonoBehaviour>(true))if(comp){Translate(comp);PrefabUtility.RecordPrefabInstancePropertyModifications(comp);}
            foreach(var nav in UnityEngine.Object.FindObjectsByType<LobbyNavigationBar>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                var label=nav.laboratory.GetComponentInChildren<TMP_Text>();label.text="LABORATORY  v";PrefabUtility.RecordPrefabInstancePropertyModifications(label);
            }
            var values=EnglishLobbyStats.Specification(s.catalog.units[0]);for(int i=0;i<14;i++)s.statValues[i].text=values[i];
            d.Refresh();UnityEngine.Object.FindFirstObjectByType<EnglishLobbyStats>().Refresh();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();return "Recorded roster overrides; aligned 6 core + 4 skills + 4 turn rows; completed English labels";
        }
    }
}
