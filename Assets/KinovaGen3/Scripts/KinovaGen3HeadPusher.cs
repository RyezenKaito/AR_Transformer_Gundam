using System.Collections.Generic;
using UnityEngine;

// Câu c: khi đầu (Head) của cánh tay tới gần một đối tượng có Rigidbody
// trong phạm vi <= 1 cm thì tác dụng một lực để đẩy đối tượng ra xa.
// Gắn vào đối tượng Head (con của J7_ToolRoll_Y). Vị trí của Head = đầu mút của tool.
// Đơn vị Unity: 1 unit = 1 m  =>  0.01 = 1 cm.
[RequireComponent(typeof(KinovaGen3PointCloudEffect))]
public class KinovaGen3HeadPusher : MonoBehaviour
{
    [Tooltip("Khoảng cách kích hoạt tính từ đầu mút Head tới bề mặt đối tượng (mét). 0.01 = 1 cm")]
    public float triggerDistance = 0.01f;

    [Tooltip("Lực đẩy (xung lực, ForceMode.Impulse). Tốc độ thu được = pushForce / mass")]
    public float pushForce = 0.4f;

    [Tooltip("Thời gian nghỉ giữa hai lần đẩy cùng một đối tượng (giây)")]
    public float cooldown = 0.5f;

    public LayerMask targetLayers = ~0;

    [Tooltip("Lệch đầu mút so với gốc Head (hệ toạ độ local)")]
    public Vector3 tipOffset = Vector3.zero;

    KinovaGen3PointCloudEffect effect;
    Transform robotRoot;
    readonly Dictionary<Rigidbody, float> nextPushTime = new Dictionary<Rigidbody, float>();
    readonly Collider[] buffer = new Collider[16];

    void Awake()
    {
        effect = GetComponent<KinovaGen3PointCloudEffect>();
        var rig = GetComponentInParent<KinovaGen3Rig>();
        robotRoot = rig != null ? rig.transform : transform.root;
    }

    void FixedUpdate()
    {
        Vector3 tip = transform.TransformPoint(tipOffset);

        int n = Physics.OverlapSphereNonAlloc(tip, triggerDistance, buffer, targetLayers,
                                              QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            Collider c = buffer[i];
            if (c.transform.IsChildOf(robotRoot)) continue;   // bỏ qua chính robot

            Rigidbody rb = c.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;

            float t;
            if (nextPushTime.TryGetValue(rb, out t) && Time.time < t) continue;

            Push(rb, tip);
            nextPushTime[rb] = Time.time + cooldown;
        }
    }

    void Push(Rigidbody rb, Vector3 tip)
    {
        Vector3 dir = rb.worldCenterOfMass - tip;           // hướng từ Head tới đối tượng = ra xa
        if (dir.sqrMagnitude < 1e-8f) dir = transform.up;
        dir.Normalize();

        rb.AddForce(dir * pushForce, ForceMode.Impulse);
        effect.Play(tip, dir);                              // câu d: hiệu ứng đám mây điểm
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.TransformPoint(tipOffset), triggerDistance);
    }
}
