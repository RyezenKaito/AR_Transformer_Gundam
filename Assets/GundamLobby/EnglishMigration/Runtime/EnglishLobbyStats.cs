using TMPro;
using UnityEngine;

namespace GundamLobby
{
    [DefaultExecutionOrder(300)]
    public sealed class EnglishLobbyStats : MonoBehaviour
    {
        public HomeLobbyDashboard dashboard;
        public TMP_Text unitName, role, grade;
        public TMP_Text[] values, radarLabels;
        public LobbyShape radar;
        StrikerDefinition shown;int version=-1;
        void OnEnable(){Refresh();}
        void LateUpdate(){if(shown!=dashboard.ActiveUnit||version!=PlayerProgress.Version)Refresh();}
        public static string[] StatNames={"MAX HP","ATTACK","DEFENSE","SPEED","BREAK DMG","ARMOR"};
        public static int[] Numbers(StrikerDefinition u)=>new[]{PlayerProgress.Scaled(u.id,u.hp,"hp"),PlayerProgress.Scaled(u.id,u.attack,"attack"),PlayerProgress.Scaled(u.id,u.defense,"defense"),PlayerProgress.Scaled(u.id,u.speed,"speed"),PlayerProgress.Scaled(u.id,u.breakDamage,"break"),PlayerProgress.Scaled(u.id,u.armor,"armor")};
        public static float[] Scores(StrikerDefinition u){var n=Numbers(u);return new[]{Mathf.InverseLerp(300,1000,n[0]),Mathf.InverseLerp(50,200,n[1]),Mathf.InverseLerp(50,180,n[2]),Mathf.InverseLerp(120,180,n[3]),Mathf.InverseLerp(20,130,n[4]),Mathf.InverseLerp(60,500,n[5])};}
        public static string Grade(float score)=>score>=.90f?"S":score>=.75f?"A":score>=.60f?"B":score>=.40f?"C":"D";
        public void Refresh()
        {
            if(!dashboard)return;
            version=PlayerProgress.Version;shown=dashboard.ActiveUnit;if(shown==null)return;
            unitName.text=shown.displayName;role.text=shown.role+" / LV. "+PlayerProgress.UnitLevel(shown.id);
            grade.text=shown.gradeBadge;
            var numbers=Numbers(shown);var scores=Scores(shown);
            for(int i=0;i<6;i++)
            {
                values[i].text=numbers[i].ToString("N0");
                radarLabels[i].text=StatNames[i]+" <color=#82DDF8><b>"+Grade(scores[i])+"</b></color>";
            }
            radar.radarValues=scores;radar.SetVerticesDirty();
        }
        public static string[] Specification(StrikerDefinition u)
        {
            var v=new System.Collections.Generic.List<string>();
            foreach(var n in Numbers(u))v.Add(n.ToString("N0"));
            for(int i=0;i<4;i++)v.Add(u.skillNames[i]+"  /  "+u.burstCosts[i]+" BURST");
            v.Add("5  /  START 2");v.Add("0 / 1 / 2 BURST");v.Add("+1 BURST / TURN");v.Add("5 BURST / FULL BURST");
            return v.ToArray();
        }
    }
}
