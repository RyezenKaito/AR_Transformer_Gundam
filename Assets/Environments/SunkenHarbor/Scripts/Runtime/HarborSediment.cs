using UnityEngine;
using TurnBasedBattle;
namespace SunkenHarbor
{
 public sealed class HarborSediment:MonoBehaviour
 {
  public TurnBattleManager battle;public ParticleSystem sediment;Vector3 previous;float cooldown;bool wasAction;
  void Start(){if(battle)previous=battle.player.transform.position;}
  void Update(){if(!battle||!sediment||!battle.player)return;Vector3 p=battle.player.transform.position;cooldown-=Time.deltaTime;bool land=wasAction&&!battle.InAction;if(p.y<.5f&&cooldown<=0&&(land||(p-previous).magnitude>.07f)){sediment.transform.position=p+Vector3.up*.08f;sediment.Emit(land?12:5);cooldown=.35f;}previous=p;wasAction=battle.InAction;}
 }
}
