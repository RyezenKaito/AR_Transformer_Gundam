using System;
using System.Collections.Generic;
using UnityEngine;

namespace MechaDemo
{
    [DisallowMultipleComponent]
    public sealed class MechaHealth : MonoBehaviour
    {
        public static readonly List<MechaHealth> Active = new List<MechaHealth>();
        public string displayName = "Training unit";
        public bool playerFaction;
        public bool trainingDummy;
        public float maxHealth = 1000f;
        public float currentHealth = 1000f;
        public Vector3 aimOffset = new Vector3(0f, 1.35f, 0f);
        public bool showWorldHealth = true;
        public int receivedHits;
        public float lastDamage;
        public event Action<float, Vector3, Vector3> Damaged;
        public event Action Died;
        public event Action ResetPerformed;
        float stopRemaining;
        public float HP { get { return Mathf.Max(0f, currentHealth); } }
        public bool IsDead { get { return currentHealth <= 0f; } }
        public bool IsHitStopped { get { return stopRemaining > 0f; } }
        public Vector3 AimPosition { get { return transform.TransformPoint(aimOffset); } }

        void Awake() { currentHealth = maxHealth; }
        void OnEnable() { if (!Active.Contains(this)) Active.Add(this); }
        void OnDisable() { Active.Remove(this); stopRemaining = 0f; }
        void Update()
        {
            if (Time.timeScale > 0f) stopRemaining = Mathf.Max(0f, stopRemaining - Time.unscaledDeltaTime);
        }
        public void AddHitStop(float seconds) { stopRemaining = Mathf.Max(stopRemaining, Mathf.Max(0f, seconds)); }
        public bool TakeDamage(float amount, Vector3 point, Vector3 direction)
        {
            if (IsDead || amount <= 0f || float.IsNaN(amount)) return false;
            lastDamage = amount;
            receivedHits++;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            if (Damaged != null) Damaged(amount, point, direction);
            if (IsDead && Died != null) Died();
            return true;
        }
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            receivedHits = 0;
            lastDamage = 0f;
            stopRemaining = 0f;
            if (ResetPerformed != null) ResetPerformed();
        }
        void OnGUI()
        {
            if (!showWorldHealth || playerFaction) return;
            Camera cam = Camera.main;
            if (cam == null) return;
            Vector3 screen = cam.WorldToScreenPoint(AimPosition + Vector3.up * 1.1f);
            if (screen.z <= 0f) return;
            Rect r = new Rect(screen.x - 70f, Screen.height - screen.y, 140f, 8f);
            Color old = GUI.color;
            GUI.color = new Color(.03f, .06f, .10f, .9f); GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = playerFaction ? Color.cyan : new Color(1f, .34f, .24f);
            GUI.DrawTexture(new Rect(r.x, r.y, r.width * Mathf.Clamp01(currentHealth / Mathf.Max(1f, maxHealth)), r.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(r.x - 15f, r.y - 24f, 200f, 25f), displayName + (IsDead ? "  DISABLED" : "  " + Mathf.CeilToInt(HP)));
            GUI.color = old;
        }
    }
}
