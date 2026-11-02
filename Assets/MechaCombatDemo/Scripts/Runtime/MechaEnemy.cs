using UnityEngine;
namespace MechaDemo
{
 [DisallowMultipleComponent, RequireComponent(typeof(MechaHealth)), RequireComponent(typeof(CharacterController))]
 public sealed class MechaEnemy : MonoBehaviour
 {
  public MechaPlayer player;
  public Animator animator;
  public MechaEffects effects;
  public float moveSpeed = 2.4f;
  public float detectionRange = 20f;
  public float attackRange = 2.7f;
  public float damage = 80f;
  public LayerMask environmentMask = 1 << 8;
  public bool aggressive = true;
  CharacterController motor;
  MechaHealth health;
  Vector3 spawn;
  Quaternion rotation;
  float vertical, attackTime = -1f, recovery, hitReaction;
  bool hitSent;
  string state = "";
  public string State { get { return health != null && health.IsDead ? "Death" : hitReaction > 0f ? "Hit" : attackTime >= 0f ? "Attack" : state; } }
  void Awake()
  {
   health = GetComponent<MechaHealth>(); motor = GetComponent<CharacterController>();
   if (animator == null) animator = GetComponentInChildren<Animator>();
   if (effects == null) effects = GetComponent<MechaEffects>();
   spawn = transform.position; rotation = transform.rotation;
   health.Damaged += OnDamaged; health.Died += OnDied; health.ResetPerformed += ResetActor;
  }
  void OnDestroy()
  {
   if (health != null) { health.Damaged -= OnDamaged; health.Died -= OnDied; health.ResetPerformed -= ResetActor; }
  }
  void Update()
  {
   if (Time.timeScale <= 0f || (player != null && player.IsPaused)) return;
   float dt = Mathf.Min(.1f, Time.deltaTime);
   if (health.IsHitStopped) { if (animator != null) animator.speed = 0f; return; }
   if (animator != null) animator.speed = 1f;
   Vector3 movement = Vector3.zero;
   if (health.IsDead) { Play("Death"); ApplyGravity(movement, dt); return; }
   if (hitReaction > 0f) { hitReaction -= dt; Play("Hit"); ApplyGravity(movement, dt); return; }
   recovery = Mathf.Max(0f, recovery - dt);
   if (player == null)
   {
    for (int i = 0; i < MechaHealth.Active.Count; i++) if (MechaHealth.Active[i] != null && MechaHealth.Active[i].playerFaction) { player = MechaHealth.Active[i].GetComponent<MechaPlayer>(); break; }
   }
   if (!aggressive || player == null || player.IsDead) { Play("Idle"); ApplyGravity(movement, dt); return; }
   Vector3 delta = player.transform.position - transform.position; delta.y = 0f;
   float distance = delta.magnitude;
   if (attackTime >= 0f)
   {
    if (animator != null) animator.speed = .56f;
    float old = attackTime; attackTime += dt;
    if (attackTime < .35f && distance > .01f) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(delta), 180f * dt);
    if (!hitSent && old <= .58f && attackTime >= .50f)
    {
     hitSent = true;
     bool inArc = distance < attackRange + .25f && Vector3.Angle(transform.forward, delta) < 65f;
     bool clear = !Physics.Linecast(transform.position + Vector3.up * 1.3f, player.transform.position + Vector3.up * 1.3f, environmentMask, QueryTriggerInteraction.Ignore);
     if (inArc && clear)
     {
      MechaHealth target = player.GetComponent<MechaHealth>();
      if (target != null && target.TakeDamage(damage, target.AimPosition, transform.forward))
      {
       health.AddHitStop(.045f); target.AddHitStop(.045f);
       if (effects != null) { effects.Impact(target.AimPosition, -transform.forward, .7f); effects.PlayCue("hit"); }
      }
     }
    }
    if (effects != null) effects.SwordTrail(attackTime >= .46f && attackTime <= .81f);
    if (attackTime >= 1.61f) { attackTime = -1f; recovery = .70f; }
   }
   else if (distance <= detectionRange)
   {
    if (distance > .01f) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(delta), 240f * dt);
    if (distance > attackRange)
    {
     Vector3 desired = delta.normalized;
     RaycastHit wall;
     if (Physics.SphereCast(transform.position + Vector3.up, .36f, desired, out wall, 1f, environmentMask, QueryTriggerInteraction.Ignore))
     {
      Vector3 slide = Vector3.ProjectOnPlane(desired, wall.normal); slide.y = 0f;
      desired = slide.sqrMagnitude > .01f ? slide.normalized : Quaternion.Euler(0, 90, 0) * desired;
     }
     movement = desired * moveSpeed; Play("RunF");
     if (animator != null) animator.speed = Mathf.Clamp(moveSpeed / 3.939f, .65f, 1.2f);
    }
    else if (recovery <= 0f)
    {
     attackTime = 0f; hitSent = false; Play("Attack3"); if (animator != null) animator.speed = .56f;
     if (effects != null) effects.PlayCue("servo");
    }
    else Play("Idle");
   }
   else Play("Idle");
   ApplyGravity(movement, dt);
  }
  void ApplyGravity(Vector3 movement, float dt)
  {
   if (!motor.enabled) return;
   if (motor.isGrounded && vertical < 0f) vertical = -2f;
   vertical -= 22f * dt; motor.Move((movement + Vector3.up * vertical) * dt);
  }
  void Play(string id)
  {
   if (state == id) return; state = id;
   if (animator != null) animator.CrossFadeInFixedTime(id, .10f, 0, 0f);
  }
  void OnDamaged(float amount, Vector3 point, Vector3 dir)
  {
   if (health.IsDead) return;
   attackTime = -1f; hitSent = false; recovery = .5f; hitReaction = .25f; state = ""; Play("Hit");
   if (effects != null) effects.Cleanup();
  }
  void OnDied()
  {
   attackTime = -1f; hitReaction = 0f; recovery = 0f; state = "";
   if (animator != null) animator.speed = 1f; Play("Death"); if (effects != null) effects.Cleanup();
  }
  public void ResetActor()
  {
   motor.enabled = false; transform.SetPositionAndRotation(spawn, rotation); motor.enabled = true;
   vertical = -2f; attackTime = -1f; hitReaction = recovery = 0f; hitSent = false; state = "";
   if (animator != null) { animator.speed = 1f; animator.Rebind(); animator.Update(0f); }
   Play("Idle"); if (effects != null) effects.Cleanup();
  }
 }
}

