using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace MechaDemo
{
 [DisallowMultipleComponent, RequireComponent(typeof(CharacterController)), RequireComponent(typeof(MechaHealth))]
 public sealed class MechaPlayer : MonoBehaviour
 {
  public Animator animator;
  public Transform bladeBase, bladeTip, muzzle;
  public Camera viewCamera;
  public Transform[] thrusters;
  public MechaEffects effects;
  public MechaTuning tuning;
  public LayerMask environmentMask = 1 << 8;
  public LayerMask hurtboxMask = 1 << 10;
  public bool drawDebug = true, showHelp = true;
  public float mouseSensitivity = .12f;
  public MechaHealth Target { get { return lockedTarget != null && !lockedTarget.IsDead ? lockedTarget : null; } }
  public Transform TargetTransform { get { return Target != null ? Target.transform : null; } }
  public Vector3 TargetPoint { get { return lockedTarget != null ? lockedTarget.AimPosition : transform.position + transform.forward * 8f + Vector3.up; } }
  public string ActionName { get { return actionName; } }
  public string LocomotionTransition { get { return locomotionTransition; } }
  public float ActionTime { get { return actionTime; } }
  public bool IsBoosting { get { return boosting; } }
  public bool IsHovering { get { return hovering; } }
  public bool IsPaused { get { return paused; } }
  public bool IsDead { get { return health != null && health.IsDead; } }
  public bool Grounded { get { return grounded; } }
  public float Energy { get { return energy; } }
  public float MaxEnergy { get { return tuning != null ? tuning.maxEnergy : 100f; } }
  public float HP { get { return health != null ? health.HP : 0f; } }
  public Vector3 PlanarVelocity { get { return actualPlanarVelocity; } }
  public float VerticalSpeed { get { return verticalVelocity; } }
  public float QCooldown { get { return qCooldown; } }
  public float ECooldown { get { return eCooldown; } }
  public float RCooldown { get { return rCooldown; } }
  public float DashCooldown { get { return dashCooldown; } }
  public int ConfirmedHits { get; private set; }
  public string LastMessage { get; private set; }
  CharacterController motor;
  MechaHealth health, lockedTarget;
  MechaActionConfig action;
  readonly HashSet<MechaHealth> hitTargets = new HashSet<MechaHealth>();
  readonly RaycastHit[] castHits = new RaycastHit[64];
  readonly Collider[] overlapHits = new Collider[64];
  string actionName = "", animationName = "", locomotionTransition = "";
  float locomotionTransitionRemaining, turnTransitionCooldown;
  bool priorMoveIntent, priorBoostMode;
  Vector3 priorMoveDirection = Vector3.forward;
  float actionTime, previousActionTime, simpleDuration, energy, regenDelay;
  float dashCooldown, qCooldown, eCooldown, rCooldown, fireCooldown, clock;
  float attackBufferUntil = -1f, dashBufferUntil = -1f, jumpBufferUntil = -1f;
  float groundedGrace, airTime, verticalVelocity;
  Vector3 planarVelocity, actualPlanarVelocity, moveDirection, actionDirection;
  Vector2 moveInput;
  float actionTravel, turnAllowance;
  bool actionPlanned, jumpLaunched, beamFired, comboQueued, airDashUsed, finishAfterMotor;
  bool grounded, hovering, boosting, paused, ownTuning, sampleActive, previousBladeValid;
  bool wantedBoost, wantedWalk, wantedHover, wantedDescend, wantedFire;
  float upperWeight, recoilUntil;
  Vector3 oldBladeBase, oldBladeTip, spawnPosition;
  Quaternion spawnRotation;
  float pauseScale = 1f;
  bool audioPausedBefore;
  GUIStyle titleStyle, bodyStyle, smallStyle;
  bool initialized;
  void Awake()
  {
   motor = GetComponent<CharacterController>(); health = GetComponent<MechaHealth>();
   if (animator == null) animator = GetComponentInChildren<Animator>();
   if (effects == null) effects = GetComponent<MechaEffects>();
   if (viewCamera == null) viewCamera = Camera.main;
   if (tuning == null) { tuning = ScriptableObject.CreateInstance<MechaTuning>(); tuning.hideFlags = HideFlags.DontSave; ownTuning = true; }
   energy = tuning.maxEnergy; motor.slopeLimit = tuning.slopeLimit;
   health.playerFaction = true; health.showWorldHealth = false;
   health.Damaged += OnDamaged; health.Died += OnDied;
   spawnPosition = transform.position; spawnRotation = transform.rotation;
   if (animator != null) animator.applyRootMotion = false;
   initialized = true;
  }
  void Start()
  {
   grounded = ProbeGround(); Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; SetAnimation("Idle", .01f);
  }
  void OnDestroy()
  {
   if (health != null) { health.Damaged -= OnDamaged; health.Died -= OnDied; }
   if (ownTuning && tuning != null) Destroy(tuning);
  }
  void OnDisable()
  {
   if (!initialized) return;
   if (paused) { AudioListener.pause = audioPausedBefore; Time.timeScale = pauseScale; paused = false; }
   if (animator != null) animator.speed = 1f;
   if (effects != null) effects.Cleanup();
   Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
  }
  void Update()
  {
   Keyboard k = Keyboard.current;
   if (k != null && k.escapeKey.wasPressedThisFrame) TogglePause();
   if (k != null && k.f1Key.wasPressedThisFrame) showHelp = !showHelp;
   if (k != null && k.f5Key.wasPressedThisFrame) { ResetDemo(); return; }
   if (paused) return;
   float dt = Mathf.Min(.1f, Time.deltaTime); if (dt <= 0f) return;
   clock += dt; TickCooldowns(dt);
   if (viewCamera == null) viewCamera = Camera.main;
   if (lockedTarget != null && (lockedTarget.IsDead || !lockedTarget.isActiveAndEnabled || Vector3.Distance(transform.position, lockedTarget.transform.position) > tuning.targetRange * 1.5f))
   {
    if (actionName == "R" && actionTime < 1.15f) AbortAction(false);
    lockedTarget = null;
   }
   grounded = verticalVelocity <= .5f && (motor.isGrounded || ProbeGround());
   if (grounded) groundedGrace = .10f; else groundedGrace = Mathf.Max(0f, groundedGrace - dt);
   ReadInput(k); sampleActive = false;
   if (IsDead) { if (animator != null) animator.speed = 1f; MoveVertical(dt); return; }
   if (health.IsHitStopped) { if (animator != null) animator.speed = 0f; return; }
   if (animator != null) animator.speed = 1f;
   ProcessInputActions(); previousActionTime = actionTime; finishAfterMotor = false;
   if (actionName.Length > 0) AdvanceAction(dt);
   UpdateMotor(dt, actionName.Length > 0);
   if (finishAfterMotor) EndAction();
   UpdateResources(dt); UpdateAnimation(dt); UpdateEffects();
  }
  void ReadInput(Keyboard k)
  {
   moveInput = Vector2.zero; wantedBoost = wantedWalk = wantedHover = wantedDescend = wantedFire = false;
   Mouse m = Mouse.current;
   if (k != null)
   {
    moveInput.x = (k.dKey.isPressed ? 1f : 0f) - (k.aKey.isPressed ? 1f : 0f);
    moveInput.y = (k.wKey.isPressed ? 1f : 0f) - (k.sKey.isPressed ? 1f : 0f);
    moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    wantedBoost = k.leftShiftKey.isPressed || k.rightShiftKey.isPressed;
    wantedWalk = k.leftAltKey.isPressed || k.rightAltKey.isPressed;
    wantedHover = k.spaceKey.isPressed; wantedDescend = k.cKey.isPressed;
    if (k.spaceKey.wasPressedThisFrame) jumpBufferUntil = clock + .10f;
    if (k.leftCtrlKey.wasPressedThisFrame || k.rightCtrlKey.wasPressedThisFrame) dashBufferUntil = clock + tuning.inputBuffer;
    if (!health.IsHitStopped)
    {
     if (k.qKey.wasPressedThisFrame) TryAction("Q");
     if (k.eKey.wasPressedThisFrame) TryAction("E");
     if (k.rKey.wasPressedThisFrame) TryAction("R");
    }
   }
   if (m != null)
   {
    if (m.middleButton.wasPressedThisFrame) ToggleTarget();
    if (m.leftButton.wasPressedThisFrame)
    {
     attackBufferUntil = clock + tuning.inputBuffer;
     if (IsMelee() && action != null && action.queueClose > 0f && actionTime >= action.queueOpen && actionTime <= action.queueClose) comboQueued = true;
    }
    wantedFire = m.rightButton.isPressed;
   }
   Vector3 f = viewCamera != null ? Vector3.ProjectOnPlane(viewCamera.transform.forward, Vector3.up).normalized : transform.forward;
   if (f.sqrMagnitude < .01f) f = transform.forward;
   moveDirection = Vector3.ClampMagnitude(f * moveInput.y + Vector3.Cross(Vector3.up, f) * moveInput.x, 1f);
  }
  void ProcessInputActions()
  {
   if (dashBufferUntil >= clock && CanDashCancel())
   {
    if (TryAction("Dash")) { dashBufferUntil = -1f; attackBufferUntil = -1f; comboQueued = false; return; }
   }
   if (actionName.Length == 0 && jumpBufferUntil >= clock && groundedGrace > 0f)
   { BeginSimple("JumpStart", .18f); jumpLaunched = false; jumpBufferUntil = -1f; }
   if (IsMelee() && action != null && attackBufferUntil >= clock && action.queueClose > 0f && actionTime >= action.queueOpen && actionTime <= action.queueClose)
   { comboQueued = true; attackBufferUntil = -1f; }
   if (actionName.Length == 0 && attackBufferUntil >= clock && grounded)
   { TryAction("Attack1"); attackBufferUntil = -1f; }
   if (wantedFire && actionName.Length == 0 && fireCooldown <= 0f) FireGun();
  }
  public bool TestStartAction(string id)
  {
   if (!initialized || paused || IsDead || health.IsHitStopped) return false;
   if (id == "Fire") { if (actionName.Length > 0 || fireCooldown > 0f) return false; FireGun(); return true; }
   if (id == "Jump" || id == "JumpStart") { if (actionName.Length > 0 || !grounded) return false; BeginSimple("JumpStart", .18f); jumpLaunched = false; return true; }
   return TryAction(id);
  }
  public void LockTarget(MechaHealth target) { lockedTarget = target != null && !target.IsDead && !target.playerFaction ? target : null; }
  public bool TryAction(string id)
  {
   if (!initialized || paused || IsDead || health.IsHitStopped) return false;
   bool isDash = id == "Dash";
   float availableEnergy = energy + (isDash && actionName == "E" && actionTime < .45f ? 24f : 0f);
   if (isDash)
   {
    if (!CanDashCancel() || dashCooldown > 0f || availableEnergy < tuning.dash.cost || (!grounded && airDashUsed)) return false;
   }
   else if (actionName.Length > 0) return false;
   if (!isDash && !grounded) return false;
   MechaActionConfig next = tuning.Get(id); if (next == null) return false;
   if (availableEnergy + .001f < next.cost) { Message("Not enough energy"); return false; }
   if ((id == "Q" && qCooldown > 0f) || (id == "E" && eCooldown > 0f) || (id == "R" && rCooldown > 0f)) return false;
   if (id == "R")
   {
    if (lockedTarget == null) lockedTarget = FindTarget(10f);
    if (lockedTarget == null || lockedTarget.IsDead || Vector3.Distance(transform.position, lockedTarget.transform.position) > 10f || !Visible(lockedTarget))
    { Message("R needs a visible target within 10 m"); return false; }
   }
   if (actionName.Length > 0) AbortAction(true);
   ClearLocomotionTransition(); action = next; actionName = id; actionTime = previousActionTime = 0f;
   comboQueued = beamFired = actionPlanned = finishAfterMotor = false;
   hitTargets.Clear(); previousBladeValid = false;
   turnAllowance = id == "R" ? 70f : 40f;
   actionDirection = moveDirection.sqrMagnitude > .01f && isDash ? moveDirection.normalized : transform.forward;
   actionTravel = next.advance; energy = Mathf.Max(0f, energy - next.cost);
   if (next.cost > 0f) regenDelay = tuning.regenerationDelay;
   if (id == "Q") qCooldown = next.cooldown;
   if (id == "E") eCooldown = next.cooldown;
   if (id == "R") rCooldown = next.cooldown;
   if (isDash)
   {
    dashCooldown = next.cooldown; if (!grounded) airDashUsed = true;
    Vector3 local = transform.InverseTransformDirection(actionDirection);
    string state = Mathf.Abs(local.x) > Mathf.Abs(local.z) ? (local.x > 0f ? "DashR" : "DashL") : (local.z >= 0f ? "DashF" : "DashB");
    SetAnimation(state, .035f); if (effects != null) effects.DashBurst(actionDirection);
   }
   else SetAnimation(id, .055f);
   if (effects != null) effects.PlayCue(id == "E" ? "charge" : id == "R" ? "ultimate" : id == "Dash" ? "dash" : "servo");
   return true;
  }
  bool CanDashCancel()
  {
   if (actionName.Length == 0) return true;
   if (actionName == "E" && actionTime < .45f) return true;
   return action != null && actionName != "Dash" && actionTime >= action.dashCancel;
  }
  void BeginSimple(string id, float duration)
  {
   ClearLocomotionTransition(); action = null; actionName = id; actionTime = previousActionTime = 0f; simpleDuration = duration;
   comboQueued = false; previousBladeValid = false; SetAnimation(id, .06f);
  }
  void AdvanceAction(float dt)
  {
   actionTime += dt;
   if (actionName == "JumpStart" && !jumpLaunched && actionTime >= .14f)
   {
    jumpLaunched = true; grounded = false; groundedGrace = 0f; airTime = 0f;
    verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(tuning.gravity) * tuning.jumpHeight);
    if (effects != null) { effects.DashBurst(Vector3.up); effects.PlayCue("jump"); }
   }
   if (action == null) { if (actionTime >= simpleDuration) finishAfterMotor = true; return; }
   if (actionName == "R" && actionTime < 1.15f && (lockedTarget == null || lockedTarget.IsDead)) { AbortAction(false); return; }
   if ((actionName == "Q" && Crossed(.18f)) || (actionName == "R" && Crossed(1.15f))) PlanApproach();
   if (actionName == "E" && !beamFired && actionTime >= .65f) { beamFired = true; FireBeam(); }
   float windup = IsMelee() ? action.activeStart : actionName == "Q" ? .18f : actionName == "E" ? .65f : actionName == "R" ? 1.15f : 0f;
   if (actionTime <= windup && actionName != "Dash") TurnForAction(dt);
   sampleActive = actionName != "E" && actionName != "Dash" && previousActionTime < action.activeEnd && actionTime > action.activeStart;
   if (IsMelee() && comboQueued && action.linkTime > 0f && actionTime >= action.linkTime)
   {
    string nextId = actionName == "Attack1" ? "Attack2" : "Attack3"; EndAction(); StartLinked(nextId); return;
   }
   if (actionTime >= action.duration) finishAfterMotor = true;
  }
  void StartLinked(string id)
  {
   ClearLocomotionTransition(); action = tuning.Get(id); actionName = id; actionTime = previousActionTime = 0f;
   comboQueued = beamFired = actionPlanned = false; hitTargets.Clear(); previousBladeValid = false;
   turnAllowance = 40f; actionDirection = transform.forward; actionTravel = action.advance;
   SetAnimation(id, .04f); if (effects != null) effects.PlayCue("servo");
  }
  bool Crossed(float marker) { return previousActionTime < marker && actionTime >= marker; }
  bool IsMelee() { return actionName == "Attack1" || actionName == "Attack2" || actionName == "Attack3"; }
  void TurnForAction(float dt)
  {
   Vector3 desired = actionName == "E" ? AimPoint(tuning.beamRange) - transform.position : lockedTarget != null ? lockedTarget.AimPosition - transform.position : transform.forward;
   desired.y = 0f; if (desired.sqrMagnitude < .01f) return;
   Quaternion old = transform.rotation;
   transform.rotation = Quaternion.RotateTowards(old, Quaternion.LookRotation(desired), Mathf.Min(turnAllowance, (actionName == "E" ? 60f : 240f) * dt));
   turnAllowance = Mathf.Max(0f, turnAllowance - Quaternion.Angle(old, transform.rotation));
  }
  void PlanApproach()
  {
   actionPlanned = true;
   Vector3 delta = lockedTarget != null && !lockedTarget.IsDead ? lockedTarget.transform.position - transform.position : transform.forward * action.advance;
   delta.y = 0f;
   // Approach commits the final WINDUP heading; no extra snap or homing past the turn budget.
   actionDirection = transform.forward;
   float projectedDistance = Vector3.Dot(delta, actionDirection);
   float stop = lockedTarget != null && !lockedTarget.IsDead ? 1.65f : 0f;
   actionTravel = Mathf.Min(action.advance, Mathf.Max(0f, projectedDistance - stop));
   Vector3 bottom = transform.position + Vector3.up * (motor.radius + .05f);
   Vector3 top = transform.position + Vector3.up * Mathf.Max(motor.radius + .1f, motor.height - motor.radius);
   RaycastHit obstruction;
   if (actionTravel > 0f && Physics.CapsuleCast(bottom, top, motor.radius * .95f, actionDirection, out obstruction, actionTravel + .05f, environmentMask, QueryTriggerInteraction.Ignore)) actionTravel = Mathf.Max(0f, obstruction.distance - .10f);
   if (effects != null) effects.DashBurst(actionDirection);
  }
  float MotionFraction(float t)
  {
   if (action == null) return 0f;
   float start = actionName == "Attack1" ? .08f : actionName == "Attack2" ? .10f : actionName == "Attack3" ? .20f : .02f, end = action.activeEnd;
   if (actionName == "Dash") { start = .02f; end = .20f; }
   else if (actionName == "Q") { start = .18f; end = .38f; }
   else if (actionName == "R") { start = 1.15f; end = 1.65f; }
   else if (!IsMelee()) return 0f;
   float u = Mathf.Clamp01((t - start) / Mathf.Max(.01f, end - start)); return u * u * (3f - 2f * u);
  }
  void UpdateMotor(float dt, bool hasFullAction)
  {
   bool wasGrounded = grounded; float oldVertical = verticalVelocity;
   boosting = !hasFullAction && wantedBoost && moveInput.sqrMagnitude > .01f && energy > .02f;
   hovering = !hasFullAction && !grounded && airTime > .22f && wantedHover && !wantedDescend && energy > .02f;
   float speed = boosting ? tuning.boostSpeed : wantedWalk ? tuning.walkSpeed : tuning.runSpeed;
   if (!boosting && lockedTarget != null)
   {
    Vector3 localInput = transform.InverseTransformDirection(moveDirection);
    if (Mathf.Abs(localInput.x) > Mathf.Abs(localInput.z)) speed = Mathf.Min(speed, wantedWalk ? 1.8f : 3f);
    else if (localInput.z < 0f) speed = Mathf.Min(speed, wantedWalk ? 2.5f : 5f);
   }
   Vector3 desired = hasFullAction ? Vector3.zero : moveDirection * speed;
   Vector3 priorPlanarVelocity = planarVelocity;
   planarVelocity = Vector3.MoveTowards(planarVelocity, desired, (grounded ? (desired.sqrMagnitude < .01f ? tuning.braking : tuning.acceleration) : tuning.airAcceleration) * dt);
   Vector3 move = (priorPlanarVelocity + planarVelocity) * (.5f * dt);
   if (hasFullAction)
   {
    planarVelocity = Vector3.zero;
    if (action != null && (actionName == "Dash" || IsMelee() || ((actionName == "Q" || actionName == "R") && actionPlanned))) move = actionDirection * actionTravel * Mathf.Max(0f, MotionFraction(actionTime) - MotionFraction(previousActionTime));
    else move = Vector3.zero;
   }
   else if (moveDirection.sqrMagnitude > .01f || lockedTarget != null || wantedFire)
   {
    Vector3 face = lockedTarget != null ? lockedTarget.AimPosition - transform.position : wantedFire && viewCamera != null ? viewCamera.transform.forward : moveDirection;
    face.y = 0f; if (face.sqrMagnitude > .01f) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(face), tuning.turnSpeed * dt);
   }
   if (grounded && verticalVelocity < 0f) verticalVelocity = -2f;
   float initialVertical = verticalVelocity;
   if (hovering) verticalVelocity = Mathf.MoveTowards(verticalVelocity, 0f, 30f * dt); else verticalVelocity += tuning.gravity * dt;
   if (!grounded && wantedDescend) verticalVelocity = Mathf.MoveTowards(verticalVelocity, -7f, 18f * dt);
   move += Vector3.up * (initialVertical + verticalVelocity) * (.5f * dt);
   Vector3 positionBeforeMove = transform.position;
   CollisionFlags flags = MoveSafely(move);
   actualPlanarVelocity = Vector3.ProjectOnPlane(transform.position - positionBeforeMove, Vector3.up) / Mathf.Max(.0001f, dt);
   if (!hasFullAction)
   {
    if ((flags & CollisionFlags.Sides) != 0) planarVelocity = actualPlanarVelocity;
    if (boosting && (flags & CollisionFlags.Sides) != 0 && actualPlanarVelocity.magnitude < .15f) boosting = false;
   }
   if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;
   bool onGround = verticalVelocity <= 0f && (((flags & CollisionFlags.Below) != 0) || ProbeGround());
   if (onGround)
   {
    if (!wasGrounded && airTime > .12f)
    {
     if (effects != null) { effects.Impact(transform.position + Vector3.up * .02f, Vector3.up, oldVertical < -8f ? 1.4f : .4f); effects.PlayCue("land"); }
     if (actionName.Length == 0) BeginSimple(oldVertical < -8f ? "LandHard" : "LandSoft", oldVertical < -8f ? .55f : .28f);
    }
    grounded = true; hovering = false; airDashUsed = false; airTime = 0f; verticalVelocity = -2f;
   }
   else { grounded = false; airTime += dt; }
   if (transform.position.y < -15f) ResetDemo();
  }
  void MoveVertical(float dt)
  {
   if (motor == null || !motor.enabled) return;
   if (motor.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
   verticalVelocity += tuning.gravity * dt; MoveSafely(Vector3.up * verticalVelocity * dt);
  }
  CollisionFlags MoveSafely(Vector3 displacement)
  {
   if (!motor.enabled) return CollisionFlags.None;
   int count = Mathf.Clamp(Mathf.CeilToInt(displacement.magnitude / .20f), 1, 64);
   Vector3 step = displacement / count; CollisionFlags flags = CollisionFlags.None;
   for (int i = 0; i < count; i++) flags |= motor.Move(step);
   return flags;
  }
  bool ProbeGround()
  {
   if (motor == null || !motor.enabled || verticalVelocity > .5f) return false;
   RaycastHit hit;
   if (Physics.SphereCast(transform.position + Vector3.up * .42f, Mathf.Min(.23f, motor.radius * .8f), Vector3.down, out hit, .32f, environmentMask, QueryTriggerInteraction.Ignore)) return Vector3.Angle(hit.normal, Vector3.up) <= motor.slopeLimit + 1f;
   return false;
  }
  void UpdateResources(float dt)
  {
   bool costAction = actionName == "Dash" || actionName == "Q" || actionName == "E" || actionName == "R";
   float drain = (boosting ? tuning.boostDrain : 0f) + (hovering ? tuning.hoverDrain : 0f);
   if (drain > 0f) energy = Mathf.Max(0f, energy - drain * dt);
   if (costAction || drain > 0f) regenDelay = tuning.regenerationDelay;
   else { regenDelay = Mathf.Max(0f, regenDelay - dt); if (regenDelay <= 0f) energy = Mathf.Min(tuning.maxEnergy, energy + tuning.regeneration * dt); }
  }
  void TickCooldowns(float dt)
  {
   dashCooldown = Mathf.Max(0f, dashCooldown - dt); qCooldown = Mathf.Max(0f, qCooldown - dt); eCooldown = Mathf.Max(0f, eCooldown - dt);
   rCooldown = Mathf.Max(0f, rCooldown - dt); fireCooldown = Mathf.Max(0f, fireCooldown - dt);
  }
  void UpdateAnimation(float dt)
  {
   if (actionName.Length > 0) ClearLocomotionTransition();
   else if (!grounded)
   {
    ClearLocomotionTransition();
    SetAnimation(hovering ? "Hover" : verticalVelocity > .4f ? "Ascend" : "Fall", .12f);
   }
   else UpdateGroundAnimation(dt);
   if (animator != null && animator.layerCount > 1)
   {
    upperWeight = Mathf.MoveTowards(upperWeight, actionName.Length == 0 && (wantedFire || clock < recoilUntil) ? 1f : 0f, dt * 12f);
    animator.SetLayerWeight(1, upperWeight);
    if (clock >= recoilUntil && wantedFire && upperWeight > .01f && !animator.GetCurrentAnimatorStateInfo(1).IsName("Aim")) animator.CrossFadeInFixedTime("Aim", .08f, 1);
   }
  }
  void ClearLocomotionTransition()
  {
   locomotionTransition = ""; locomotionTransitionRemaining = 0f;
   priorMoveIntent = priorBoostMode = false; turnTransitionCooldown = 0f;
  }
  void StartLocomotionTransition(string state, float duration, float dt)
  {
   locomotionTransition = state;
   locomotionTransitionRemaining = Mathf.Max(0f, duration - dt);
   SetAnimation(state, .055f);
   if (animator != null) animator.speed = 1f;
  }
  void UpdateGroundAnimation(float dt)
  {
   bool intent = moveDirection.sqrMagnitude > .01f;
   turnTransitionCooldown = Mathf.Max(0f, turnTransitionCooldown - dt);
   if (locomotionTransitionRemaining > 0f)
   {
    locomotionTransitionRemaining = Mathf.Max(0f, locomotionTransitionRemaining - dt);
    if (locomotionTransitionRemaining <= 0f) locomotionTransition = "";
   }
   if (boosting && !priorBoostMode) StartLocomotionTransition("BoostStart", .16f, dt);
   else if (!boosting && priorBoostMode) StartLocomotionTransition("BoostStop", .25f, dt);
   else if (!intent && priorMoveIntent && actualPlanarVelocity.magnitude > .15f) StartLocomotionTransition("Stop", .25f, dt);
   else if (intent && !priorMoveIntent && !boosting) StartLocomotionTransition("Start", .18f, dt);
   else if (intent && priorMoveIntent && !boosting && lockedTarget == null && !wantedFire && turnTransitionCooldown <= 0f && priorMoveDirection.sqrMagnitude > .01f && Vector3.Angle(priorMoveDirection, moveDirection) > 135f)
   {
    // These clips twist/brace the body only. The motor remains the sole source of root yaw.
    StartLocomotionTransition(Vector3.SignedAngle(priorMoveDirection, moveDirection, Vector3.up) < 0f ? "Turn180L" : "Turn180R", .35f, dt);
    turnTransitionCooldown = .35f;
   }
   if (locomotionTransition.Length > 0)
   {
    SetAnimation(locomotionTransition, .055f);
    if (animator != null) animator.speed = 1f;
   }
   else if (boosting) SetAnimation("Boost", .10f);
   else if (actualPlanarVelocity.magnitude < .1f) SetAnimation("Idle", .12f);
   else
   {
    Vector3 local = transform.InverseTransformDirection(actualPlanarVelocity);
    string suffix = Mathf.Abs(local.x) > Mathf.Abs(local.z) ? (local.x > 0f ? "R" : "L") : (local.z >= 0f ? "F" : "B");
    string gait = (wantedWalk ? "Walk" : "Run") + suffix;
    SetGaitAnimation(gait, .10f, animationName == "Start" ? .12f : -1f);
    float nominal = wantedWalk ? (suffix == "F" ? 2f : suffix == "B" ? 1.556f : 1.111f) : (suffix == "F" ? 3.939f : suffix == "B" ? 3.03f : 1.818f);
    if (animator != null) animator.speed = Mathf.Clamp(actualPlanarVelocity.magnitude / nominal, .65f, 1.65f);
   }
   priorMoveIntent = intent; priorBoostMode = boosting;
   if (intent) priorMoveDirection = moveDirection;
  }
  static bool IsGait(string state) { return state.StartsWith("Walk") || state.StartsWith("Run"); }
  void SetGaitAnimation(string state, float blend, float requestedPhase)
  {
   if (animator == null || animationName == state) return;
   float phase = requestedPhase >= 0f ? requestedPhase : IsGait(animationName) ? Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f) : 0f;
   float period = state.StartsWith("Walk") ? .90f : .66f;
   SetAnimation(state, blend, phase * period);
  }
  void SetAnimation(string state, float blend, float fixedTimeOffset = 0f)
  {
   if (animator == null || animationName == state) return;
   animationName = state; animator.CrossFadeInFixedTime(state, blend, 0, fixedTimeOffset);
  }
  void UpdateEffects()
  {
   if (effects == null) return;
   effects.Thrusters(boosting || hovering || actionName == "Dash" || (actionName == "Q" && actionTime >= .10f && actionTime < .60f) || (actionName == "R" && actionTime >= 1.05f && actionTime < 1.70f));
   effects.SwordTrail(action != null && actionName != "E" && actionName != "Dash" && actionTime >= action.activeStart - .025f && actionTime <= action.activeEnd + .04f);
   effects.Charge(actionName == "E" && actionTime < .65f ? Mathf.Clamp01(actionTime / .65f) : actionName == "R" && actionTime < 1.15f ? Mathf.Clamp01(actionTime / 1.15f) : 0f);
  }
  void LateUpdate()
  {
   if (!initialized || paused || IsDead || health.IsHitStopped || bladeBase == null || bladeTip == null) return;
   Vector3 currentBase = bladeBase.position, currentTip = bladeTip.position;
   if (sampleActive && action != null)
   {
    if (!previousBladeValid) { oldBladeBase = currentBase; oldBladeTip = currentTip; }
    float span = actionTime - previousActionTime;
    if (span > .000001f)
    {
     float first = Mathf.Clamp01((action.activeStart - previousActionTime) / span);
     float last = Mathf.Clamp01((action.activeEnd - previousActionTime) / span);
     if (last > first)
     {
      Vector3 firstBase, firstTip, lastBase, lastTip;
      InterpolateBlade(oldBladeBase, oldBladeTip, currentBase, currentTip, first, out firstBase, out firstTip);
      InterpolateBlade(oldBladeBase, oldBladeTip, currentBase, currentTip, last, out lastBase, out lastTip);
      SweepSword(firstBase, firstTip, lastBase, lastTip, action.damage, action.hitStop);
     }
    }
   }
   oldBladeBase = currentBase; oldBladeTip = currentTip; previousBladeValid = true;
  }
  static void InterpolateBlade(Vector3 fromBase, Vector3 fromTip, Vector3 toBase, Vector3 toTip, float u, out Vector3 b, out Vector3 tip)
  {
   Vector3 a = fromTip - fromBase, z = toTip - toBase;
   b = Vector3.Lerp(fromBase, toBase, u);
   Vector3 direction = a.sqrMagnitude > .000001f && z.sqrMagnitude > .000001f ? Vector3.Slerp(a.normalized, z.normalized, u) : Vector3.Lerp(a, z, u).normalized;
   tip = b + direction * Mathf.Lerp(a.magnitude, z.magnitude, u);
  }
  void SweepSword(Vector3 fromBase, Vector3 fromTip, Vector3 toBase, Vector3 toTip, float damage, float hitStop)
  {
   Vector3 oldVector = fromTip - fromBase, newVector = toTip - toBase;
   int subdivisions = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(Vector3.Angle(oldVector, newVector) / 10f, (toTip - fromTip).magnitude / .18f)), 1, 24);
   Vector3 lastBase = fromBase, lastTip = fromTip;
   for (int n = 1; n <= subdivisions; n++)
   {
    float u = n / (float)subdivisions;
    Vector3 b = Vector3.Lerp(fromBase, toBase, u);
    Vector3 dir = Vector3.Slerp(oldVector.normalized, newVector.normalized, u);
    Vector3 tip = b + dir * Mathf.Lerp(oldVector.magnitude, newVector.magnitude, u);
    for (int j = 0; j <= 8; j++)
    {
     float s = j / 8f; Vector3 start = Vector3.Lerp(lastBase, lastTip, s), end = Vector3.Lerp(b, tip, s);
     SweepPoint(start, end, damage, hitStop);
     int overlaps = Physics.OverlapSphereNonAlloc(end, tuning.swordRadius, overlapHits, hurtboxMask, QueryTriggerInteraction.Collide);
     for (int i = 0; i < overlaps; i++) TrySwordHit(overlapHits[i], end, damage, hitStop);
    }
    lastBase = b; lastTip = tip;
   }
   if (drawDebug) Debug.DrawLine(toBase, toTip, Color.cyan, .05f);
  }
  void SweepPoint(Vector3 start, Vector3 end, float damage, float stop)
  {
   Vector3 delta = end - start; if (delta.sqrMagnitude < .000001f) return;
   int count = Physics.SphereCastNonAlloc(start, tuning.swordRadius, delta.normalized, castHits, delta.magnitude, environmentMask.value | hurtboxMask.value, QueryTriggerInteraction.Collide);
   float obstacle = delta.magnitude + .001f;
   for (int i = 0; i < count; i++) if (((1 << castHits[i].collider.gameObject.layer) & environmentMask.value) != 0 && !castHits[i].collider.transform.IsChildOf(transform)) obstacle = Mathf.Min(obstacle, castHits[i].distance);
   for (int i = 0; i < count; i++) if (castHits[i].distance <= obstacle) TrySwordHit(castHits[i].collider, castHits[i].point, damage, stop);
  }
  void TrySwordHit(Collider collider, Vector3 sample, float damage, float stop)
  {
   if (collider == null) return;
   MechaHealth receiver = collider.GetComponentInParent<MechaHealth>();
   if (receiver == null || receiver == health || receiver.playerFaction == health.playerFaction || receiver.IsDead || hitTargets.Contains(receiver)) return;
   Vector3 point = collider.ClosestPoint(sample), origin = bladeBase != null ? bladeBase.position : transform.position + Vector3.up;
   RaycastHit obstruction; if (Physics.Linecast(origin, point, out obstruction, environmentMask, QueryTriggerInteraction.Ignore)) return;
   hitTargets.Add(receiver); if (!receiver.TakeDamage(damage, point, actionDirection)) return;
   ConfirmedHits++; health.AddHitStop(stop); receiver.AddHitStop(stop);
   if (effects != null) { effects.Impact(point, (origin - point).normalized, damage / 150f); effects.PlayCue("hit"); }
  }
  void FireGun()
  {
   fireCooldown = tuning.fireInterval; ResolveShot(tuning.gunRange, tuning.gunDamage, .045f, .055f); recoilUntil = clock + .16f;
   if (animator != null && animator.layerCount > 1) animator.CrossFadeInFixedTime("Fire", .025f, 1, 0f);
   if (effects != null) { effects.MuzzleFlash(); effects.PlayCue("shot"); }
  }
  void FireBeam()
  {
   ResolveShot(tuning.beamRange, tuning.e.damage, .20f, .18f);
   if (effects != null) { effects.MuzzleFlash(); effects.PlayCue("fire"); }
  }
  Vector3 AimPoint(float range)
  {
   if (lockedTarget != null && !lockedTarget.IsDead) return lockedTarget.AimPosition;
   if (viewCamera == null) return transform.position + Vector3.up * 1.5f + transform.forward * range;
   Ray ray = viewCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
   int count = Physics.RaycastNonAlloc(ray, castHits, range, environmentMask.value | hurtboxMask.value, QueryTriggerInteraction.Collide);
   float nearest = range; Vector3 aim = ray.GetPoint(range);
   for (int i = 0; i < count; i++)
   {
    if (castHits[i].collider.transform.IsChildOf(transform)) continue;
    if (castHits[i].distance < nearest) { nearest = castHits[i].distance; aim = castHits[i].point; }
   }
   return aim;
  }
  void ResolveShot(float range, float damage, float visibleTime, float radius)
  {
   Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up * 1.5f + transform.forward * .5f;
   int inside = Physics.OverlapSphereNonAlloc(origin, .05f, overlapHits, environmentMask, QueryTriggerInteraction.Ignore);
   if (inside > 0) { if (effects != null) effects.Impact(origin, -transform.forward, .4f); Message("Muzzle obstructed"); return; }
   Vector3 delta = AimPoint(range) - origin;
   Vector3 dir = delta.sqrMagnitude > .0001f ? delta.normalized : transform.forward;
   if (Vector3.Angle(Vector3.ProjectOnPlane(dir, Vector3.up), transform.forward) > 85f) dir = transform.forward;
   float distance = Mathf.Min(range, Mathf.Max(.1f, delta.magnitude));
   int count = Physics.SphereCastNonAlloc(origin, radius, dir, castHits, distance, environmentMask.value | hurtboxMask.value, QueryTriggerInteraction.Collide);
   int nearestIndex = -1; float nearest = distance;
   for (int i = 0; i < count; i++)
   {
    if (castHits[i].collider.transform.IsChildOf(transform)) continue;
    if (castHits[i].distance < nearest) { nearest = castHits[i].distance; nearestIndex = i; }
   }
   Vector3 end = origin + dir * nearest;
   if (nearestIndex >= 0)
   {
    RaycastHit hit = castHits[nearestIndex]; end = hit.point;
    MechaHealth receiver = hit.collider.GetComponentInParent<MechaHealth>();
    if (receiver != null && receiver != health && receiver.playerFaction != health.playerFaction && receiver.TakeDamage(damage, hit.point, dir))
    {
     ConfirmedHits++;
     if (actionName == "E") { health.AddHitStop(tuning.e.hitStop); receiver.AddHitStop(tuning.e.hitStop); }
    }
    if (effects != null) effects.Impact(hit.point, hit.normal, damage / 200f);
   }
   if (effects != null) effects.Beam(origin, end, visibleTime);
   if (drawDebug) Debug.DrawLine(origin, end, Color.cyan, .3f);
  }
  void ToggleTarget()
  {
   if (lockedTarget != null) { lockedTarget = null; return; }
   lockedTarget = FindTarget(tuning.targetRange); if (lockedTarget == null) Message("No visible target");
  }
  MechaHealth FindTarget(float range)
  {
   MechaHealth best = null; float bestScore = float.MaxValue;
   Vector3 facing = viewCamera != null ? viewCamera.transform.forward : transform.forward;
   for (int i = 0; i < MechaHealth.Active.Count; i++)
   {
    MechaHealth h = MechaHealth.Active[i];
    if (h == null || h == health || h.playerFaction == health.playerFaction || h.IsDead || !h.isActiveAndEnabled) continue;
    Vector3 d = h.AimPosition - (transform.position + Vector3.up * 1.3f);
    float distance = d.magnitude, angle = Vector3.Angle(facing, d);
    if (distance > range || angle > 70f || !Visible(h)) continue;
    float score = angle * .15f + distance; if (score < bestScore) { bestScore = score; best = h; }
   }
   return best;
  }
  bool Visible(MechaHealth target)
  {
   return target != null && !Physics.Linecast(transform.position + Vector3.up * 1.35f, target.AimPosition, environmentMask, QueryTriggerInteraction.Ignore);
  }
  void EndAction()
  {
   bool endedCostAction = action != null && action.cost > 0f;
   sampleActive = false; action = null; actionName = ""; actionTime = previousActionTime = 0f;
   comboQueued = beamFired = previousBladeValid = finishAfterMotor = false;
   if (endedCostAction) regenDelay = Mathf.Max(regenDelay, tuning.regenerationDelay);
   if (effects != null) { effects.SwordTrail(false); effects.Charge(0f); }
  }
  void AbortAction(bool voluntary)
  {
   if (actionName == "E" && actionTime < .45f && !IsDead) { energy = Mathf.Min(tuning.maxEnergy, energy + 24f); eCooldown = 1f; }
   EndAction(); attackBufferUntil = -1f; if (effects != null) effects.Cleanup();
  }
  void OnDamaged(float damage, Vector3 point, Vector3 direction)
  {
   if (IsDead) return;
   if (actionName == "R" && actionTime >= .45f && actionTime <= 2.35f) return;
   AbortAction(false); BeginSimple("Hit", .28f);
  }
  void OnDied()
  {
   if (paused) { AudioListener.pause = audioPausedBefore; Time.timeScale = pauseScale; paused = false; }
   AbortAction(false); actionName = "Death"; actionTime = 0f; simpleDuration = float.PositiveInfinity;
   planarVelocity = actualPlanarVelocity = Vector3.zero; boosting = hovering = false;
   if (animator != null) { animator.speed = 1f; if (animator.layerCount > 1) animator.SetLayerWeight(1, 0f); }
   SetAnimation("Death", .05f); Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
  }
  public void TogglePause()
  {
   paused = !paused;
   if (paused)
   {
    audioPausedBefore = AudioListener.pause; AudioListener.pause = true;
    pauseScale = Time.timeScale > 0f ? Time.timeScale : 1f; Time.timeScale = 0f;
    Cursor.lockState = CursorLockMode.None; Cursor.visible = true; if (animator != null) animator.speed = 0f;
   }
   else
   {
    AudioListener.pause = audioPausedBefore; Time.timeScale = pauseScale;
    Cursor.lockState = IsDead ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = IsDead;
    if (animator != null) animator.speed = health.IsHitStopped ? 0f : 1f;
    attackBufferUntil = dashBufferUntil = jumpBufferUntil = -1f;
   }
  }
  public void ResetDemo()
  {
   if (paused) { AudioListener.pause = audioPausedBefore; Time.timeScale = pauseScale; paused = false; }
   AbortAction(false);
   for (int i = 0; i < MechaHealth.Active.Count; i++) if (MechaHealth.Active[i] != null) MechaHealth.Active[i].ResetHealth();
   motor.enabled = false; transform.SetPositionAndRotation(spawnPosition, spawnRotation); motor.enabled = true;
   planarVelocity = actualPlanarVelocity = Vector3.zero; verticalVelocity = -2f; airTime = 0f; airDashUsed = false;
   actionName = ""; action = null; energy = tuning.maxEnergy;
   dashCooldown = qCooldown = eCooldown = rCooldown = fireCooldown = regenDelay = 0f;
   attackBufferUntil = dashBufferUntil = jumpBufferUntil = -1f; lockedTarget = null; ConfirmedHits = 0; animationName = ""; ClearLocomotionTransition();
   if (animator != null) { animator.speed = 1f; if (animator.layerCount > 1) animator.SetLayerWeight(1, 0f); animator.Rebind(); animator.Update(0f); }
   SetAnimation("Idle", .01f); Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; Message("Arena reset");
  }
  void Message(string text) { LastMessage = text; }
  void OnGUI()
  {
   if (!initialized) return;
   if (titleStyle == null)
   {
    titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 21, fontStyle = FontStyle.Bold };
    bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 15 };
    smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true };
   }
   Color old = GUI.color;
   GUI.color = new Color(.02f, .04f, .08f, .9f); GUI.DrawTexture(new Rect(16f, 16f, 370f, 166f), Texture2D.whiteTexture);
   GUI.color = new Color(.65f, .92f, 1f); GUI.Label(new Rect(30f, 24f, 350f, 30f), "UNICORN  /  COMBAT SYSTEM", titleStyle);
   DrawBar(new Rect(30f, 60f, 320f, 15f), health.HP / Mathf.Max(1f, health.maxHealth), new Color(.2f, .9f, .65f));
   DrawBar(new Rect(30f, 86f, 320f, 12f), energy / Mathf.Max(1f, tuning.maxEnergy), Color.cyan);
   GUI.color = Color.white;
   GUI.Label(new Rect(30f, 108f, 340f, 26f), "HP " + Mathf.CeilToInt(HP) + "    EN " + Mathf.CeilToInt(energy) + "    HITS " + ConfirmedHits, bodyStyle);
   GUI.Label(new Rect(30f, 136f, 350f, 25f), (actionName.Length > 0 ? actionName : hovering ? "HOVER" : boosting ? "BOOST" : grounded ? "GROUND" : "AIR") + "   " + (Target != null ? "LOCK: " + lockedTarget.displayName : "FREE AIM"), smallStyle);
   GUI.Label(new Rect(25f, Screen.height - 70f, 800f, 30f), "Q VECTOR SLASH " + CooldownLabel(qCooldown) + "    E CHARGED BEAM " + CooldownLabel(eCooldown) + "    R CELESTIAL BREAKER " + CooldownLabel(rCooldown), bodyStyle);
   GUI.Label(new Rect(25f, Screen.height - 39f, 800f, 25f), "F1 help  |  F5 reset  |  " + LastMessage, smallStyle);
   if (showHelp)
   {
    GUI.color = new Color(.02f, .04f, .08f, .82f); GUI.DrawTexture(new Rect(Screen.width - 300f, 20f, 280f, 192f), Texture2D.whiteTexture);
    GUI.color = Color.white;
    GUI.Label(new Rect(Screen.width - 285f, 32f, 260f, 180f), "WASD move   /   Alt walk\nShift boost   /   Ctrl dash\nSpace jump / hold to hover\nC descend   /   Mouse orbit\nLMB blade combo   /   RMB fire\nMMB target lock\nQ slash   /   E beam   /   R ultimate\nEsc pause   /   F5 reset", smallStyle);
   }
   if (!paused && !IsDead)
   {
    GUI.color = Target != null ? new Color(1f, .5f, .25f) : new Color(.65f, .95f, 1f);
    GUI.Label(new Rect(Screen.width * .5f - 6f, Screen.height * .5f - 14f, 20f, 30f), "+", titleStyle);
   }
   if (paused || IsDead)
   {
    GUI.color = new Color(0f, .02f, .05f, .72f); GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
    GUI.color = Color.white;
    GUI.Label(new Rect(Screen.width * .5f - 150f, Screen.height * .5f - 50f, 330f, 55f), IsDead ? "UNIT DISABLED" : "SYSTEM PAUSED", titleStyle);
    GUI.Label(new Rect(Screen.width * .5f - 150f, Screen.height * .5f, 330f, 55f), "F5 reset arena" + (!IsDead ? "   /   Esc resume" : ""), bodyStyle);
   }
   GUI.color = old;
  }
  static string CooldownLabel(float value) { return value <= 0f ? "[READY]" : "[" + value.ToString("0.0") + "s]"; }
  static void DrawBar(Rect rect, float value, Color color)
  {
   GUI.color = new Color(.12f, .16f, .22f); GUI.DrawTexture(rect, Texture2D.whiteTexture);
   GUI.color = color; GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(value), rect.height), Texture2D.whiteTexture);
  }
 }
}








