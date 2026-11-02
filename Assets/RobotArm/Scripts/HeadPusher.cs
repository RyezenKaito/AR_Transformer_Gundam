using UnityEngine;

// Gan vao doi tuong Head (cung voi ForceEffect)
public class HeadPusher : MonoBehaviour
{
    public float detectRange = 0.01f;                        // 1 cm
    public float pushForce = 5f;
    public float cooldown = 0.5f;
    public Vector3 tipLocal = new Vector3(0f, 0.1f, 0f);     // dau mut cua HeadDisc

    ForceEffect effect;
    float nextTime;

    void Start() { effect = GetComponent<ForceEffect>(); }

    void Update()
    {
        if (Time.time < nextTime) return;
        Vector3 tip = transform.TransformPoint(tipLocal);
        Transform robot = transform.root;

        foreach (Collider c in Physics.OverlapSphere(tip, detectRange))
        {
            if (c.transform.IsChildOf(robot)) continue;      // bo qua chinh robot
            Rigidbody rb = c.attachedRigidbody;
            if (rb == null) continue;

            Vector3 dir = (rb.worldCenterOfMass - tip).normalized;
            rb.AddForce(dir * pushForce, ForceMode.Impulse);  // day vat ra xa
            if (effect != null) effect.Play(tip, dir);        // hieu ung cau d
            nextTime = Time.time + cooldown;
            break;
        }
    }
}
