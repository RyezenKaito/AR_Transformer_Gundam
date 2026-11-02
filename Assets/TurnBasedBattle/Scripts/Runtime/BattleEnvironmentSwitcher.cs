using System;
using UnityEngine;
using UnityEngine.UI;

namespace TurnBasedBattle
{
    /// <summary>Changes scenery only; battle state and camera orbit remain owned by their controllers.</summary>
    public sealed class BattleEnvironmentSwitcher : MonoBehaviour
    {
        [Serializable] public class EnvironmentOption
        {
            public string title;
            public GameObject root;
            public Color ambient = new Color(.4f,.46f,.52f);
            public Color background = new Color(.035f,.05f,.075f);
        }
        public TurnBattleManager battle;
        public BattleOrbitCamera desktopOrbit;
        public EnvironmentOption[] environments;
        public MeshRenderer[] trainingRenderers;
        public Button nextButton;
        public Text buttonLabel;
        [SerializeField] int selectedIndex;
        public int SelectedIndex => selectedIndex;
        public int Count => environments == null ? 0 : environments.Length;
        public string CurrentName => Count > 0 ? environments[selectedIndex].title : "";
        void OnEnable()
        {
            if(nextButton)nextButton.onClick.AddListener(Next);
            Apply(selectedIndex);
        }
        void OnDisable(){if(nextButton)nextButton.onClick.RemoveListener(Next);}
        public void Next(){if(Count > 0)Apply((selectedIndex+1)%Count);}
        public void Apply(int index)
        {
            if(index<0 || index>=Count)return;
            selectedIndex=index;
            for(int i=0;i<Count;i++)if(environments[i].root)environments[i].root.SetActive(i==index);
            // Training meshes share the persistent arena floor collider and target rings.
            if(trainingRenderers!=null)foreach(var r in trainingRenderers)if(r)r.enabled=index==0;
            var option=environments[index];
            if(!desktopOrbit || !desktopOrbit.ExternallyDriven)
            {
                RenderSettings.ambientLight=option.ambient;
                RenderSettings.ambientIntensity=1;
                if(battle && battle.battleCamera)battle.battleCamera.backgroundColor=option.background;
            }
            if(buttonLabel)buttonLabel.text="ĐỔI MÔI TRƯỜNG  ›\n"+(index+1)+" / "+Count+"   "+option.title;
            if(nextButton)nextButton.interactable=Count>1;
        }
    }
}
