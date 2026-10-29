using UnityEngine;

[AddComponentMenu("Gundam/Anim Controller")]
public class GundamAnimController : MonoBehaviour
{
    private Animator _anim;
    private static readonly string[] Clips = { "Action1", "Action2", "Appear", "Idle", "Static" };

    void Awake() { _anim = GetComponent<Animator>(); }

    void Start() { if (_anim != null) PlayAnim("Idle"); }

    void Update()
    {
        if (_anim == null) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayAnim("Action1");
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayAnim("Action2");
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayAnim("Appear");
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlayAnim("Idle");
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlayAnim("Static");
    }

    public void PlayAnim(string animName)
    {
        foreach (var c in Clips) _anim.ResetTrigger(c);
        _anim.SetTrigger(animName);
        Debug.Log("[Gundam] >>> " + animName);
    }
}
