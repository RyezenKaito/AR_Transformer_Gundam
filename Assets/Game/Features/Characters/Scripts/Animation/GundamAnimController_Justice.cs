using UnityEngine;

[AddComponentMenu("Gundam/Anim Controller")]
public class NewAnimatorController : MonoBehaviour
{
    private Animator _anim;
    private static readonly string[] Clips = { "00-IDLE", "01-SABER", "02-RIFLE", "03-SHILD", "04-BOOMERANG", "05-LIFTER" };

    void Awake() { _anim = GetComponent<Animator>(); }

    void Start() { if (_anim != null) PlayAnim("00-IDLE"); }

    void Update()
    {
        if (_anim == null) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayAnim("01-SABER");
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayAnim("02-RIFLE");
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayAnim("03-SHILD");
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlayAnim("04-BOOMERANG");
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlayAnim("05-LIFTER");
    }

    public void PlayAnim(string animName)
    {
        foreach (var c in Clips) _anim.ResetTrigger(c);
        _anim.SetTrigger(animName);
        Debug.Log("[Gundam] >>> " + animName);
    }
}
