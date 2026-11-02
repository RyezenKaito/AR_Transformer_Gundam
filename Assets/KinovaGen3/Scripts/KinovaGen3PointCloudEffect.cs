using UnityEngine;

// Câu d: hiệu ứng đám mây điểm tại Head để thấy trực quan lực tác động.
// Mỗi lần đẩy, hàng trăm điểm nhỏ bắn ra từ đầu mút Head theo hướng của lực
// (trong một hình nón), nhỏ dần rồi biến mất.
public class KinovaGen3PointCloudEffect : MonoBehaviour
{
    [Header("Đám mây điểm")]
    public int pointCount = 150;
    [Tooltip("Kích thước mỗi điểm (mét)")]
    public float pointSize = 0.008f;
    public float lifetime = 1.2f;
    public Vector2 speedRange = new Vector2(0.15f, 0.9f);
    [Range(5f, 90f)] public float coneAngle = 40f;
    public float startRadius = 0.01f;
    public Color nearColor = new Color(0.2f, 0.9f, 1f);
    public Color farColor = Color.white;

    ParticleSystem ps;

    void Awake()
    {
        Build();
    }

    void Build()
    {
        GameObject go = new GameObject("PointCloud");
        go.transform.SetParent(transform, false);
        ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;                       // giữ hệ hạt luôn sống, ta tự Emit bằng code
        main.playOnAwake = false;
        main.startLifetime = lifetime;
        main.startSize = pointSize;
        main.startSpeed = 0f;
        main.maxParticles = 3000;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission; em.enabled = false;
        var sh = ps.shape; sh.enabled = false;

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)));

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        Shader s = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (s == null) s = Shader.Find("Particles/Standard Unlit");
        if (s == null) s = Shader.Find("Sprites/Default");
        r.material = new Material(s);

        ps.Play();
    }

    // pos: vị trí phát (đầu mút Head), dir: hướng của lực, strength: hệ số nhân số điểm
    public void Play(Vector3 pos, Vector3 dir, float strength = 1f)
    {
        if (ps == null) Build();
        if (!ps.isPlaying) ps.Play();

        dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : transform.up;
        Quaternion look = Quaternion.LookRotation(dir);
        float tanCone = Mathf.Tan(coneAngle * Mathf.Deg2Rad);
        int n = Mathf.Max(1, Mathf.RoundToInt(pointCount * strength));

        var p = new ParticleSystem.EmitParams();
        for (int i = 0; i < n; i++)
        {
            Vector2 c = Random.insideUnitCircle * tanCone;
            Vector3 v = (look * new Vector3(c.x, c.y, 1f)).normalized;
            float t = Random.value;

            p.position = pos + Random.insideUnitSphere * startRadius;
            p.velocity = v * Mathf.Lerp(speedRange.x, speedRange.y, t);
            p.startLifetime = lifetime * Random.Range(0.7f, 1f);
            p.startSize = pointSize * Random.Range(0.6f, 1.2f);
            p.startColor = Color.Lerp(nearColor, farColor, t);
            ps.Emit(p, 1);
        }
    }

    // Chuột phải vào component trong lúc Play để xem thử hiệu ứng
    [ContextMenu("Test burst")]
    void TestBurst()
    {
        if (Application.isPlaying) Play(transform.position, transform.up);
    }
}
