using UnityEngine;

namespace GundamGame.CombatDemo
{
    [CreateAssetMenu(menuName="Gundam/Combat HUD Art")]
    public sealed class RougeHudArt : ScriptableObject
    {
        public Sprite playerPortrait;
        public Sprite[] enemyPortraits=new Sprite[3];
        public Sprite[] weaponIcons=new Sprite[4];
    }
}
