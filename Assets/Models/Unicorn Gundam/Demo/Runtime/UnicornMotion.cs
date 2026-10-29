using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnicornDemo
{
    // Rigid mechanical joints preserve the authored geometry without a humanoid rig.
    public sealed class UnicornMotion : MonoBehaviour
    {
        public Transform poseRoot, head, armL, armR, forearmL, forearmR, legL, legR, shinL, shinR, backpack;
        public bool IsLanding { get; private set; }
        public bool IsExploded { get; private set; }
        public bool IsAttacking => attackRemaining > 0f;
        public float MoveBlend { get; private set; }
        readonly Dictionary<Transform, Vector3> bindPositions = new Dictionary<Transform, Vector3>();
        readonly Dictionary<Transform, Quaternion> bindRotations = new Dictionary<Transform, Quaternion>();
        readonly List<Transform> joints = new List<Transform>();
        float phase, attackRemaining, burstRemaining, explosion;
        float desiredMove;
        Coroutine landing;
        Material ringMaterial;
        Transform landingRing, attackRing;
        LineRenderer[] jets;

        void Awake()
        {
            CacheBindings();
        }
        void CacheBindings()
        {
            bindPositions.Clear();bindRotations.Clear();joints.Clear();
            foreach (var t in new[] { head, armL, armR, forearmL, forearmR, legL, legR, shinL, shinR, backpack })
                if (t && !bindPositions.ContainsKey(t))
                { joints.Add(t); bindPositions[t] = t.localPosition; bindRotations[t] = t.localRotation; }
            ringMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            ringMaterial.color = new Color(.1f, .85f, 1f);
            landingRing = CreateRing("Landing shockwave", 100);
            attackRing = CreateRing("Attack pulse", 55);
            landingRing.gameObject.SetActive(false);
            attackRing.gameObject.SetActive(false);
            jets = new LineRenderer[2];
            for(int i=0;i<2;i++)
            {
                var jet=new GameObject("Thruster glow").AddComponent<LineRenderer>();
                jet.transform.SetParent(poseRoot,false);jet.useWorldSpace=false;jet.positionCount=2;
                jet.sharedMaterial=ringMaterial;jet.startWidth=.06f;jet.endWidth=.008f;
                jets[i]=jet;
            }
        }
        Transform CreateRing(string label, int points)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = ringMaterial;
            line.useWorldSpace = false; line.loop = true;
            line.widthMultiplier = .018f; line.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float a = Mathf.PI * 2 * i / points;
                line.SetPosition(i, new Vector3(Mathf.Cos(a), .018f, Mathf.Sin(a)));
            }
            return go.transform;
        }
        public void SetMovement(float blend) { desiredMove = Mathf.Clamp01(blend); }
        public void SetExploded(bool value)
        {
            if (IsLanding) return;
            IsExploded = value; desiredMove = 0; attackRemaining = 0;
        }
        public void Attack()
        {
            if (!IsLanding && !IsExploded && attackRemaining <= 0) {attackRemaining = .85f;Debug.Log("UNICORN_DEMO ATTACK");}
        }
        public void Burst()
        {
            if (!IsLanding && !IsExploded) burstRemaining = 3f;
        }
        public void Spawn()
        {
            if (landing != null) StopCoroutine(landing);
            IsExploded = false; explosion = 0; attackRemaining = 0; burstRemaining = 0; desiredMove = 0;
            landing = StartCoroutine(Land());
        }
        IEnumerator Land()
        {
            IsLanding = true;
            landingRing.gameObject.SetActive(false);
            const float descent = 1.65f;
            for (float t = 0; t < descent; t += Time.deltaTime)
            {
                float u = Mathf.Clamp01(t / descent);
                poseRoot.localPosition = Vector3.up * Mathf.Lerp(4.2f, 0f, u * u * (3f - 2f * u));
                yield return null;
            }
            landingRing.gameObject.SetActive(true);
            for (float t = 0; t < .65f; t += Time.deltaTime)
            {
                float u = t / .65f;
                landingRing.localScale = Vector3.one * Mathf.Lerp(.12f, 1.6f, u);
                poseRoot.localPosition = Vector3.down * (.1f * Mathf.Sin(u * Mathf.PI));
                yield return null;
            }
            poseRoot.localPosition = Vector3.zero;
            landingRing.gameObject.SetActive(false);
            IsLanding = false; landing = null;
            Debug.Log("UNICORN_DEMO LANDING_COMPLETE");
        }
        void LateUpdate()
        {
            if(bindRotations.Count==0) CacheBindings();
            float dt = Time.deltaTime;
            MoveBlend = Mathf.MoveTowards(MoveBlend, IsLanding || IsExploded ? 0 : desiredMove, dt * 5);
            phase += dt * Mathf.Lerp(1.2f, 9f, MoveBlend);
            explosion = Mathf.MoveTowards(explosion, IsExploded ? 1 : 0, dt * 2.5f);
            foreach (var t in joints) { t.localPosition = bindPositions[t]; t.localRotation = bindRotations[t]; }
            float gait = Mathf.Sin(phase) * MoveBlend;
            Rotate(head, new Vector3(0, Mathf.Sin(Time.time * .7f) * 6 * (1 - MoveBlend), 0));
            Rotate(armL, new Vector3(gait * 24, 0, -3));
            Rotate(armR, new Vector3(-gait * 24, 0, 3));
            Rotate(forearmL, new Vector3(-8 - Mathf.Max(0, -gait) * 18, 0, 0));
            Rotate(forearmR, new Vector3(-8 - Mathf.Max(0, gait) * 18, 0, 0));
            Rotate(legL, new Vector3(-gait * 20, 0, 0));
            Rotate(legR, new Vector3(gait * 20, 0, 0));
            Rotate(shinL, new Vector3(Mathf.Max(0, gait) * 30, 0, 0));
            Rotate(shinR, new Vector3(Mathf.Max(0, -gait) * 30, 0, 0));
            if (!IsLanding) poseRoot.localPosition = Vector3.up * (Mathf.Sin(Time.time * 1.8f) * .008f + Mathf.Abs(gait) * .025f);
            if (attackRemaining > 0)
            {
                attackRemaining -= dt;
                float u = 1 - Mathf.Max(0, attackRemaining) / .85f;
                float punch = Mathf.Sin(Mathf.Clamp01(u / .7f) * Mathf.PI);
                Rotate(armR, new Vector3(-85 * punch, -15 * punch, 8));
                Rotate(forearmR, new Vector3(-50 * (1 - punch), 0, 0));
                attackRing.gameObject.SetActive(u > .25f && u < .85f);
                attackRing.localPosition = new Vector3(.2f, .02f, .65f + u * .5f);
                attackRing.localScale = Vector3.one * (.15f + u * .4f);
            }
            else attackRing.gameObject.SetActive(false);
            if (burstRemaining > 0)
            {
                burstRemaining -= dt;
                landingRing.gameObject.SetActive(true);
                landingRing.localScale = Vector3.one * (1 + Mathf.Sin(Time.time * 12) * .08f);
            }
            else if (!IsLanding) landingRing.gameObject.SetActive(false);
            for(int i=0;i<jets.Length;i++)
            {
                bool firing=IsLanding||MoveBlend>.1f||burstRemaining>0;
                jets[i].enabled=firing&&!IsExploded;
                Vector3 start=new Vector3(i==0?-.14f:.14f,1.32f,-.27f);
                jets[i].SetPosition(0,start);
                Vector3 direction=IsLanding?Vector3.down:Vector3.back;
                jets[i].SetPosition(1,start+direction*(.24f+Mathf.Sin(Time.time*39)*.04f+MoveBlend*.15f));
            }
            if (explosion > 0)
            {
                Explode(head, Vector3.up * .45f);
                Explode(armL, Vector3.left * .55f); Explode(armR, Vector3.right * .55f);
                Explode(legL, new Vector3(-.35f, 0, .25f)); Explode(legR, new Vector3(.35f, 0, .25f));
                Explode(backpack, Vector3.back * .55f);
            }
        }
        void Rotate(Transform t, Vector3 angle) { if (t && bindRotations.TryGetValue(t,out var rotation)) t.localRotation = rotation * Quaternion.Euler(angle); }
        void Explode(Transform t, Vector3 offset) { if (t) t.localPosition = bindPositions[t] + offset * Mathf.SmoothStep(0, 1, explosion); }
        void OnDestroy() { if (ringMaterial) Destroy(ringMaterial); }
    }
}
