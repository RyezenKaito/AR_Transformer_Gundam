using UnityEngine;
namespace BiogearLab
{
    [CreateAssetMenu(menuName="Biogear Lab/Layout Configuration")]
    public sealed class BiogearLayoutConfig : ScriptableObject
    {
        public int seed=72;
        public float arenaRadius=17;
        public Vector3 domePosition=new Vector3(-36,-1,0);
        public Vector3 towerPosition=new Vector3(0,5,37);
        public Vector3 overviewPosition=new Vector3(55,40,-65);
        public Vector3 overviewTarget=new Vector3(-3,17,9);
    }
}
