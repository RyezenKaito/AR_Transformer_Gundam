using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MechaDemo.Editor
{
    public static class MechaValidation
    {
        [Serializable]
        private sealed class ValidationReport
        {
            public string title = "Mechanical rig validation";
            public string timestamp;
            public string unityVersion;
            public string root;
            public int passed;
            public int failed;
            public int warnings;
            public int mappedParts;
            public int jointCount;
            public float sourcePreservationMaxError;
            public float resetPositionError;
            public float resetRotationErrorDegrees;
            public string[] checks;
        }
        private struct PoseSnapshot
        {
            public Transform transform;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        public static string Validate(GameObject root, string assetRoot)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (string.IsNullOrEmpty(assetRoot)) throw new ArgumentException("Asset root is required", nameof(assetRoot));
            var report = new ValidationReport
            {
                timestamp = DateTimeOffset.Now.ToString("o", CultureInfo.InvariantCulture),
                unityVersion = Application.unityVersion,
                root = root.name
            };
            var checks = new List<string>();
            Action<bool, string> check = (condition, description) =>
            {
                if (condition) { report.passed++; checks.Add("PASS: " + description); }
                else { report.failed++; checks.Add("FAIL: " + description); }
            };
            Action<string> warn = description => { report.warnings++; checks.Add("WARNING: " + description); };
            var rig = root.GetComponent<MechaRigData>();
            check(rig != null, "MechaRigData exists on the player root");
            if (rig == null) return Write(report, checks, assetRoot);
            check(rig.visualRoot != null && rig.skeletonRoot != null, "VisualRoot and SkeletonRoot references are assigned");
            if (rig.visualRoot == null || rig.skeletonRoot == null) return Write(report, checks, assetRoot);
            check(rig.skeletonRoot.IsChildOf(rig.visualRoot), "SkeletonRoot is inside Animator binding hierarchy");
            check(Vector3.Distance(root.transform.localScale, Vector3.one) < .0001f, "PlayerRoot local scale is one");
            check(Vector3.Distance(rig.visualRoot.localScale, Vector3.one) < .0001f, "VisualRoot local scale is one (geometry owns imported unit conversion)");
            check(rig.sourceToMeters > 0 && IsFinite(rig.sourceToMeters), "Source-to-meters scale is finite and positive");
            check(rig.maximumRestPoseError <= .0005f && IsFinite(rig.maximumRestPoseError), "Builder rest-pose matrix error is at most 0.5 mm");
            check(rig.joints != null && rig.restLocalPositions != null && rig.restLocalRotations != null &&
                  rig.joints.Length == rig.restLocalPositions.Length && rig.joints.Length == rig.restLocalRotations.Length,
                  "Joint/rest-position/rest-rotation arrays are complete and aligned");
            check(rig.parts != null && rig.parts.Length == 375, "Expected 375 imported mesh parts are mapped");
            report.mappedParts = rig.parts == null ? 0 : rig.parts.Length;
            report.jointCount = rig.joints == null ? 0 : rig.joints.Length;

            int missingScripts = 0;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            check(missingScripts == 0, "No Missing Script component in player hierarchy (count=" + missingScripts + ")");

            string[] requiredJoints = { "Pelvis", "Spine", "Chest", "Neck", "Head", "Backpack",
                "Shoulder_L", "Shoulder_R", "UpperArm_L", "UpperArm_R", "LowerArm_L", "LowerArm_R", "Hand_L", "Hand_R",
                "UpperLeg_L", "UpperLeg_R", "LowerLeg_L", "LowerLeg_R", "Foot_L", "Foot_R",
                "FrontSkirt_L", "FrontSkirt_R", "SideSkirt_L", "SideSkirt_R", "RearSkirt_L", "RearSkirt_R" };
            foreach (string name in requiredJoints)
            {
                Transform t = rig.Joint(name);
                check(t != null && t.IsChildOf(rig.skeletonRoot), "Joint " + name + " exists inside SkeletonRoot");
            }
            var uniqueJoints = new HashSet<Transform>();
            var uniqueNames = new HashSet<string>();
            bool jointReferencesValid = rig.joints != null;
            if (rig.joints != null)
            {
                foreach (var t in rig.joints)
                {
                    if (t == null || !uniqueJoints.Add(t) || !uniqueNames.Add(t.name)) jointReferencesValid = false;
                    if (t != null && (!IsFinite(t.localPosition) || !IsFinite(t.localScale) || !IsFinite(t.localRotation))) jointReferencesValid = false;
                }
            }
            check(jointReferencesValid, "Joint references/names are unique and local transforms are finite");
            check(Under(rig.swordSocket, rig.Joint("Hand_R")), "SwordSocket follows Hand_R");
            check(Under(rig.gunSocket, rig.Joint("Hand_L")), "GunSocket follows Hand_L");
            check(rig.thrusterSockets != null && rig.thrusterSockets.Length >= 2, "At least two backpack thruster sockets exist");
            if (rig.thrusterSockets != null)
            {
                bool valid = true;
                foreach (var t in rig.thrusterSockets)
                    if (!Under(t, rig.Joint("Backpack"))) valid = false;
                check(valid, "All thruster sockets are assigned under Backpack");
            }

            Animator animator = rig.visualRoot.GetComponent<Animator>();
            check(animator != null, "Animator is on VisualRoot");
            if (animator != null)
            {
                check(!animator.applyRootMotion, "Animator root motion is disabled for motor-authoritative movement");
                if (animator.runtimeAnimatorController == null) warn("AnimatorController is not assigned yet; rig-only validation can still pass.");
                else checks.Add("INFO: Animator controller=" + AssetDatabase.GetAssetPath(animator.runtimeAnimatorController) + "; clip binding/gait validation is handled by animation report.");
            }
            CheckOptionalRuntimeReferences(root, checks, check, warn);

            var uniqueParts = new HashSet<Transform>();
            bool allPartReferences = rig.parts != null;
            int noMesh = 0, noRenderer = 0, invalidParent = 0, unresolved = 0, invalidTransforms = 0;
            if (rig.parts != null)
            {
                foreach (var p in rig.parts)
                {
                    if (p == null || p.part == null || p.driver == null)
                    { allPartReferences = false; continue; }
                    if (!uniqueParts.Add(p.part)) allPartReferences = false;
                    if (p.part.parent != p.driver || !p.driver.IsChildOf(rig.skeletonRoot)) invalidParent++;
                    var mf = p.part.GetComponent<MeshFilter>();
                    if (mf == null || mf.sharedMesh == null || mf.sharedMesh.vertexCount == 0) noMesh++;
                    if (p.part.GetComponent<MeshRenderer>() == null) noRenderer++;
                    if (string.IsNullOrEmpty(p.confidence) || p.confidence.IndexOf("unresolved", StringComparison.OrdinalIgnoreCase) >= 0) unresolved++;
                    if (!IsFinite(p.part.localPosition) || !IsFinite(p.part.localRotation) || !IsFinite(p.part.localScale)) invalidTransforms++;
                }
            }
            check(allPartReferences, "All mapped part/driver references are assigned and part transforms are unique");
            check(invalidParent == 0, "All mapped geometry is directly parented to its recorded skeleton driver");
            check(noMesh == 0 && noRenderer == 0, "Every mapped part retains a nonempty mesh and MeshRenderer");
            check(unresolved == 0, "No part is marked as unresolved");
            check(invalidTransforms == 0, "All mapped geometry transforms are finite");
            int unmappedImportedMeshes = 0;
            foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) continue;
                string meshPath = AssetDatabase.GetAssetPath(mf.sharedMesh);
                if (meshPath.IndexOf("UNICORN_GUNDAM_OBJ", StringComparison.OrdinalIgnoreCase) >= 0 && !uniqueParts.Contains(mf.transform))
                    unmappedImportedMeshes++;
            }
            check(unmappedImportedMeshes == 0, "No duplicated/unmapped source OBJ geometry remains elsewhere under player root (extra=" + unmappedImportedMeshes + ")");

            var snapshots = new List<PoseSnapshot>();
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                snapshots.Add(new PoseSnapshot { transform = t, position = t.localPosition, rotation = t.localRotation, scale = t.localScale });
            try
            {
                if (EditorApplication.isPlaying)
                {
                    warn("20-cycle rest test/source-overlay comparison skipped during Play Mode to avoid fighting runtime animation. Run in Edit Mode.");
                }
                else if (jointReferencesValid && rig.restLocalPositions.Length == rig.joints.Length && rig.restLocalRotations.Length == rig.joints.Length)
                {
                    float maxPosition = 0, maxAngle = 0;
                    bool identityRestRotations = true;
                    for (int cycle = 0; cycle < 20; cycle++)
                    {
                        rig.PreviewArmLeg();
                        rig.ResetPose();
                        for (int i = 0; i < rig.joints.Length; i++)
                        {
                            maxPosition = Mathf.Max(maxPosition, Vector3.Distance(rig.joints[i].localPosition, rig.restLocalPositions[i]));
                            maxAngle = Mathf.Max(maxAngle, Quaternion.Angle(rig.joints[i].localRotation, rig.restLocalRotations[i]));
                            if (Quaternion.Angle(rig.restLocalRotations[i], Quaternion.identity) > .01f) identityRestRotations = false;
                        }
                    }
                    report.resetPositionError = maxPosition; report.resetRotationErrorDegrees = maxAngle;
                    check(maxPosition < .00001f && maxAngle < .001f, "20 preview/reset cycles: no accumulated offset (position=" + F(maxPosition) + " m, rotation=" + F(maxAngle) + " deg)");
                    check(identityRestRotations, "All recorded skeleton rest rotations are identity");
                    ValidatePartRestLocals(rig, check);
                    ValidateAgainstSource(root, rig, report, check, warn, checks);
                    Bounds b = MappedBounds(rig);
                    check(Mathf.Abs(b.size.y - rig.referenceHeight) < .003f, "Mapped neutral geometry height matches 2.6 m within 3 mm (height=" + F(b.size.y) + ")");
                    checks.Add("INFO: Neutral mapped world bounds center=" + b.center.ToString("F5") + ", size=" + b.size.ToString("F5") + "; attachments/proxy weapons excluded.");
                }
            }
            catch (Exception exception)
            {
                check(false, "Numerical validation exception: " + exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                foreach (var snapshot in snapshots)
                {
                    if (snapshot.transform == null) continue;
                    snapshot.transform.localPosition = snapshot.position;
                    snapshot.transform.localRotation = snapshot.rotation;
                    snapshot.transform.localScale = snapshot.scale;
                }
            }
            warn("Numerical rig tests do not prove armor clearance, weapon grip, attractive key poses, crossfade quality or runtime collision behavior. Review front/side/three-quarter combat poses and play tests separately.");
            warn("Finger geometry remains rigid with Hand_L/R in the imported gripping pose; individual finger articulation is not implemented.");
            return Write(report, checks, assetRoot);
        }

        private static void ValidatePartRestLocals(MechaRigData rig, Action<bool, string> check)
        {
            float maxPosition = 0, maxAngle = 0, maxScale = 0;
            foreach (var p in rig.parts)
            {
                if (p == null || p.part == null) continue;
                maxPosition = Mathf.Max(maxPosition, Vector3.Distance(p.part.localPosition, p.restLocalPosition));
                maxAngle = Mathf.Max(maxAngle, Quaternion.Angle(p.part.localRotation, p.restLocalRotation));
                maxScale = Mathf.Max(maxScale, Vector3.Distance(p.part.localScale, p.restLocalScale));
            }
            check(maxPosition < .00001f && maxAngle < .001f && maxScale < .00001f,
                "All geometry offsets/rotations/scales retain their recorded rest transform (max local position=" + F(maxPosition) + " m)");
        }

        private static void ValidateAgainstSource(GameObject root, MechaRigData rig, ValidationReport report,
            Action<bool, string> check, Action<string> warn, List<string> checks)
        {
            const string sourcePath = "Assets/Models/Unicorn Gundam/Unicorn Gundam.prefab";
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (source == null) { warn("Original prefab not found at recorded path; source mesh/material/matrix comparison skipped."); return; }
            var sourceParts = new Dictionary<string, MeshFilter>();
            bool uniqueSourceNames = true;
            foreach (var mf in source.GetComponentsInChildren<MeshFilter>(true))
            {
                if (sourceParts.ContainsKey(mf.name)) { uniqueSourceNames = false; continue; }
                sourceParts.Add(mf.name, mf);
            }
            check(uniqueSourceNames && sourceParts.Count == rig.parts.Length, "Original prefab part names/count match mapping exactly");
            int sameMeshes = 0, sameMaterials = 0, sameVisibility = 0, matches = 0;
            float maxError = 0;
            Matrix4x4 conversion = root.transform.localToWorldMatrix * Matrix4x4.TRS(rig.sourceOffset, Quaternion.identity, Vector3.one * rig.sourceToMeters);
            foreach (var p in rig.parts)
            {
                if (p == null || p.part == null || !sourceParts.TryGetValue(p.sourceName, out MeshFilter original)) continue;
                matches++;
                MeshFilter actual = p.part.GetComponent<MeshFilter>();
                Renderer sourceRenderer = original.GetComponent<Renderer>();
                Renderer actualRenderer = p.part.GetComponent<Renderer>();
                if (actual != null && actual.sharedMesh == original.sharedMesh) sameMeshes++;
                if (sourceRenderer != null && actualRenderer != null)
                {
                    Material[] a = sourceRenderer.sharedMaterials, b = actualRenderer.sharedMaterials;
                    bool equal = a.Length == b.Length;
                    if (equal) for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) equal = false;
                    if (equal) sameMaterials++;
                    if (sourceRenderer.enabled == actualRenderer.enabled && original.gameObject.activeSelf == p.part.gameObject.activeSelf) sameVisibility++;
                }
                if (original.sharedMesh == null) continue;
                Matrix4x4 originalRelative = source.transform.worldToLocalMatrix * original.transform.localToWorldMatrix;
                Matrix4x4 expected = conversion * originalRelative;
                Matrix4x4 current = p.part.localToWorldMatrix;
                Bounds mb = original.sharedMesh.bounds;
                for (int c = 0; c < 8; c++)
                {
                    Vector3 v = mb.center + Vector3.Scale(mb.extents, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1));
                    maxError = Mathf.Max(maxError, Vector3.Distance(expected.MultiplyPoint3x4(v), current.MultiplyPoint3x4(v)));
                }
            }
            report.sourcePreservationMaxError = maxError;
            check(matches == rig.parts.Length && sameMeshes == matches, "Every mapped part retains the original shared mesh asset (" + sameMeshes + "/" + rig.parts.Length + ")");
            check(sameMaterials == matches, "Every mapped part retains original material-slot references (" + sameMaterials + "/" + matches + ")");
            check(sameVisibility == matches, "Original per-part visibility is preserved (" + sameVisibility + "/" + matches + ")");
            check(maxError < .0005f && IsFinite(maxError), "Neutral source overlay matches converted original geometry within 0.5 mm (max=" + F(maxError) + " m)");
            checks.Add("INFO: Original source comparison=" + sourcePath + "; checks mesh references, material-slot references and eight transformed bounds corners per part, not a vertex-topology rewrite.");
        }

        private static void CheckOptionalRuntimeReferences(GameObject root, List<string> checks, Action<bool, string> check, Action<string> warn)
        {
            bool foundPlayer = false;
            foreach (MonoBehaviour script in root.GetComponents<MonoBehaviour>())
            {
                if (script == null || script.GetType().Name != "MechaPlayer") continue;
                foundPlayer = true;
                foreach (string fieldName in new[] { "animator", "bladeBase", "bladeTip", "muzzle", "effects", "tuning" })
                {
                    var field = script.GetType().GetField(fieldName);
                    if (field == null) { warn("MechaPlayer schema has no public field " + fieldName + "; verify against runtime API."); continue; }
                    var value = field.GetValue(script) as UnityEngine.Object;
                    check(value != null, "MechaPlayer." + fieldName + " reference is assigned");
                }
                var cameraField = script.GetType().GetField("viewCamera");
                if (cameraField != null && (cameraField.GetValue(script) as UnityEngine.Object) == null)
                    warn("MechaPlayer.viewCamera is null in saved scene; runtime may resolve Camera.main, but explicit assignment is preferred.");
            }
            if (!foundPlayer) checks.Add("INFO: No MechaPlayer on root yet; runtime-reference validation deferred until gameplay setup.");
        }

        private static Bounds MappedBounds(MechaRigData rig)
        {
            Bounds result = new Bounds(); bool first = true;
            foreach (var p in rig.parts)
            {
                if (p == null || p.part == null) continue;
                Renderer r = p.part.GetComponent<Renderer>();
                if (r == null) continue;
                if (first) { result = r.bounds; first = false; } else result.Encapsulate(r.bounds);
            }
            return result;
        }
        private static bool Under(Transform child, Transform parent) => child != null && parent != null && child.IsChildOf(parent);
        private static bool IsFinite(float f) => !float.IsNaN(f) && !float.IsInfinity(f);
        private static bool IsFinite(Vector3 v) => IsFinite(v.x) && IsFinite(v.y) && IsFinite(v.z);
        private static bool IsFinite(Quaternion q) => IsFinite(q.x) && IsFinite(q.y) && IsFinite(q.z) && IsFinite(q.w);
        private static string F(float f) => f.ToString("F8", CultureInfo.InvariantCulture);

        private static string Write(ValidationReport report, List<string> checks, string assetRoot)
        {
            report.checks = checks.ToArray();
            string directory = assetRoot.TrimEnd(new char[] { '/', (char)92 }) + "/Reports";
            Directory.CreateDirectory(directory);
            string status = report.failed == 0 ? "NUMERICAL CHECKS PASS" : "CHECKS REQUIRE REPAIR";
            var b = new StringBuilder("# Rig validation\n\n");
            b.AppendLine(status + " — " + report.passed + " pass, " + report.failed + " fail, " + report.warnings + " warnings.\n");
            b.AppendLine("Unity " + report.unityVersion + "; root " + report.root + "; " + report.timestamp + "\n");
            b.AppendLine("These are static/numerical rig checks. They do not certify final animation, runtime behavior or commercial-game visual quality.\n");
            foreach (string line in checks) b.AppendLine("- " + line);
            File.WriteAllText(directory + "/RigValidation.md", b.ToString(), new UTF8Encoding(false));
            File.WriteAllText(directory + "/RigValidation.json", JsonUtility.ToJson(report, true), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return status + ": " + report.passed + " pass, " + report.failed + " fail, " + report.warnings + " warnings. Report: " + directory + "/RigValidation.md";
        }
    }
}


