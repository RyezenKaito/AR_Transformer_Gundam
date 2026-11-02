using UnityEngine;

// Gan vao doi tuong Head: tao dam may diem the hien luc tac dong
public class ForceEffect : MonoBehaviour
{
    public int count = 60;
    public float speed = 1.5f;
    public float spread = 0.4f;
    ParticleSystem ps;

    void Awake()
    {
        GameObject go = new GameObject("PointCloud");
        go.transform.SetParent(transform, false);
        ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 0.8f;
        main.startSize = 0.02f;
        main.startColor = Color.cyan;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission; em.enabled = false;
        var sh = ps.shape; sh.enabled = false;

        var r = go.GetComponent<ParticleSystemRenderer>();
        Shader s = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (s == null) s = Shader.Find("Particles/Standard Unlit");
        if (s == null) s = Shader.Find("Sprites/Default");
        r.material = new Material(s);
        ps.Play();
    }

    public void Play(Vector3 pos, Vector3 dir)
    {
        for (int i = 0; i < count; i++)
        {
            var p = new ParticleSystem.EmitParams();
            p.position = pos + Random.insideUnitSphere * 0.03f;
            p.velocity = (dir + Random.insideUnitSphere * spread).normalized
                         * speed * Random.Range(0.5f, 1.5f);
            ps.Emit(p, 1);
        }
    }
}
