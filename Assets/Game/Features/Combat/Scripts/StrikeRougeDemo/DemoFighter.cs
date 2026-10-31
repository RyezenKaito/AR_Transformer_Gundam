using UnityEngine;

namespace GundamGame.CombatDemo
{
    /// <summary>Demo actor. Keep the imported model inside visual; the outer transform is its battlefield slot.</summary>
    public sealed class DemoFighter : MonoBehaviour
    {
        public string displayName = "Gundam";
        public Transform visual;
        public Transform rightArm;
        public Transform leftArm;
        public Transform torso;
        public Transform muzzle;
        public bool lowerImportedArmsAtRest = true;
        public int maxHealth = 180;
        [Header("Combat attributes")]
        public int maxArmor = 300;
        public int attack = 100;
        public int defense = 80;
        [Range(120,160)] public int speed = 144;
        public int breakDamage = 60;
        [SerializeField] float armor;
        public float height = 1.65f;
        public Color accent = Color.cyan;
        [SerializeField] int health;
        Vector3 homeVisualPosition;
        Quaternion homeVisualRotation, rightRest, leftRest, torsoRest;
        bool captured;
        float rightMotionSign = 1, leftMotionSign = 1;

        public int Health => health;
        public float Armor => armor;
        public bool ArmorBroken => armor <= .001f;
        public int ArmorBreakCount { get; private set; }
        public bool IsAlive => health > 0;
        public Vector3 AimPoint => transform.TransformPoint(Vector3.up * height * 0.62f);
        public Vector3 MuzzlePoint => muzzle != null ? muzzle.position : transform.TransformPoint(new Vector3(0.25f, height * 0.68f, 0.38f));

        public void CaptureRestPose()
        {
            if (captured) return;
            if (visual == null && transform.childCount > 0) visual = transform.GetChild(0);
            if (visual == null) visual = transform;
            foreach (Transform joint in visual.GetComponentsInChildren<Transform>(true))
            {
                if (rightArm == null && joint.name.StartsWith("upper_arm.R")) rightArm = joint;
                if (leftArm == null && joint.name.StartsWith("upper_arm.L")) leftArm = joint;
                if (torso == null && joint.name.StartsWith("chest_")) torso = joint;
            }
            homeVisualPosition = visual.localPosition;
            homeVisualRotation = visual.localRotation;
            if (rightArm != null) rightRest = rightArm.localRotation;
            if (leftArm != null) leftRest = leftArm.localRotation;
            if (lowerImportedArmsAtRest && rightArm != null && rightArm.name.StartsWith("upper_arm.R"))
            {
                rightRest *= Quaternion.Euler(-65, 0, 0);
                rightMotionSign = -1;
            }
            if (lowerImportedArmsAtRest && leftArm != null && leftArm.name.StartsWith("upper_arm.L"))
            {
                leftRest *= Quaternion.Euler(-65, 0, 0);
                leftMotionSign = -1;
            }
            if (torso != null) torsoRest = torso.localRotation;
            captured = true;
        }

        public void ResetFighter()
        {
            CaptureRestPose();
            health = maxHealth;
            armor = Mathf.Max(0,maxArmor); ArmorBreakCount=0;
            RestorePose();
            visual.gameObject.SetActive(true);
        }

        public int TakeDamage(int amount)
        {
            int dealt = Mathf.Clamp(amount, 0, health);
            health -= dealt;
            if (health <= 0) Pose(Vector3.zero, new Vector3(0, 0, -63), 0, 0);
            return dealt;
        }

        // Demo rule: the hit that breaks armor still gets its initial armor reduction.
        // Existing per-skill HP totals remain tuning data until the HP/Def formula is decided.
        public int ReceiveCombatHit(int hpDamage,float armorDamage,float defenseMultiplier=1)
        {
            if(!IsAlive)return 0;
            float protection=ArmorBroken?1:.5f;
            bool wasArmored=!ArmorBroken;
            float multiplier=Mathf.Clamp01(defenseMultiplier);
            armor=Mathf.Max(0,armor-Mathf.Max(0,armorDamage)*multiplier);
            if(wasArmored && ArmorBroken)ArmorBreakCount++;
            return TakeDamage(Mathf.Max(0,Mathf.RoundToInt(hpDamage*protection*multiplier)));
        }

        /// <summary>Offsets use battlefield units and local visual rotations, preserving the source model's orientation.</summary>
        public void Pose(Vector3 offset, Vector3 euler, float armAngle = 0, float torsoAngle = 0)
        {
            CaptureRestPose();
            visual.localPosition = homeVisualPosition + offset;
            visual.localRotation = homeVisualRotation * Quaternion.Euler(euler);
            if (rightArm != null) rightArm.localRotation = rightRest * Quaternion.Euler(armAngle * rightMotionSign, 0, 0);
            if (leftArm != null) leftArm.localRotation = leftRest * Quaternion.Euler(armAngle * 0.25f * leftMotionSign, 0, 0);
            if (torso != null) torso.localRotation = torsoRest * Quaternion.Euler(0, torsoAngle, 0);
        }

        public void RestorePose()
        {
            CaptureRestPose();
            if (visual != null) { visual.localPosition = homeVisualPosition; visual.localRotation = homeVisualRotation; }
            if (rightArm != null) rightArm.localRotation = rightRest;
            if (leftArm != null) leftArm.localRotation = leftRest;
            if (torso != null) torso.localRotation = torsoRest;
        }

        public void MoveVisualToWorld(Vector3 worldPosition)
        {
            CaptureRestPose();
            visual.position = worldPosition;
        }

        public Vector3 RestVisualWorldPosition
        {
            get { CaptureRestPose(); return visual.parent != null ? visual.parent.TransformPoint(homeVisualPosition) : homeVisualPosition; }
        }
    }
}
