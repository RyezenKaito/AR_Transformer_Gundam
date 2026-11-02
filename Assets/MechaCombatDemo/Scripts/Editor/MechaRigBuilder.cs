using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MechaDemo.Editor
{
    public static class MechaRigBuilder
    {
        private sealed class SourcePart
        {
            public MeshFilter filter;
            public Renderer renderer;
            public Bounds rawBounds;
            public string lowerName;
        }

        public static GameObject Build(GameObject source, string assetRoot)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.GetComponentInChildren<MechaRigData>(true) != null)
                throw new InvalidOperationException("Source already contains a MechaRigData. Use the original unrigged prefab.");
            foreach (var data in UnityEngine.Object.FindObjectsByType<MechaRigData>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (data.gameObject.name == "Unicorn_CombatRig") return data.gameObject;

            Directory.CreateDirectory(assetRoot);
            Directory.CreateDirectory(assetRoot + "/Reports");
            var player = new GameObject("Unicorn_CombatRig");
            Undo.RegisterCreatedObjectUndo(player, "Build mechanical Gundam rig");
            var visual = Child("VisualRoot", player.transform, Vector3.zero);
            var skeleton = Child("SkeletonRoot", visual, Vector3.zero);
            Child("RigControls", visual, Vector3.zero);
            var rig = player.AddComponent<MechaRigData>();
            rig.visualRoot = visual; rig.skeletonRoot = skeleton;
            rig.referenceHeight = 2.6f;
            rig.drawJointGizmos = true;
            visual.gameObject.AddComponent<Animator>().applyRootMotion = false;

            var clone = UnityEngine.Object.Instantiate(source);
            clone.name = "SourceReference";
            if (PrefabUtility.IsPartOfPrefabInstance(clone))
                PrefabUtility.UnpackPrefabInstance(clone, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            clone.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            clone.transform.localScale = Vector3.one;
            var filters = clone.GetComponentsInChildren<MeshFilter>(true);
            if (filters.Length == 0) throw new InvalidOperationException("Source has no MeshFilter geometry.");
            var sourceParts = new List<SourcePart>(filters.Length);
            Bounds sourceBounds = new Bounds(); bool first = true;
            foreach (var filter in filters)
            {
                if (filter.sharedMesh == null) continue;
                var renderer = filter.GetComponent<Renderer>();
                if (renderer == null) continue;
                // Clone is at identity: these bounds are expressed in original model coordinates.
                Bounds raw = renderer.bounds;
                if (first) { sourceBounds = raw; first = false; } else sourceBounds.Encapsulate(raw);
                sourceParts.Add(new SourcePart { filter = filter, renderer = renderer, rawBounds = raw, lowerName = filter.name.ToLowerInvariant() });
            }
            float scale = rig.referenceHeight / sourceBounds.size.y;
            Vector3 offset = new Vector3(-sourceBounds.center.x * scale, -sourceBounds.min.y * scale, 0);
            rig.sourceToMeters = scale;
            rig.sourceOffset = offset;
            clone.transform.position = offset;
            clone.transform.localScale = Vector3.one * scale;

            var joints = new Dictionary<string, Transform>();
            var rawPositions = new Dictionary<string, Vector3>();
            Action<string, string, Vector3> joint = (name, parent, raw) =>
            {
                Transform parentTransform = string.IsNullOrEmpty(parent) ? skeleton : joints[parent];
                Vector3 world = raw * scale + offset;
                Transform t = Child(name, parentTransform, parentTransform.InverseTransformPoint(world));
                t.rotation = Quaternion.identity;
                joints.Add(name, t); rawPositions.Add(name, raw);
            };

            Vector3 pelvis = new Vector3(0, 174, 2.3f);
            joint("Pelvis", null, pelvis);
            joint("Spine", "Pelvis", new Vector3(0, 188, 2));
            joint("Chest", "Spine", new Vector3(0, 211, 0));
            Vector3 neck = Center(sourceParts, p => p.lowerName.EndsWith("_neck_port_neck_port_assy_g"), new Vector3(0, 235.48f, .48f));
            joint("Neck", "Chest", new Vector3(neck.x, 231, neck.z));
            joint("Head", "Neck", neck);
            joint("Backpack", "Chest", new Vector3(0, 219, -24));

            foreach (string side in new[] { "L", "R" })
            {
                string sl = side.ToLowerInvariant(); float sign = side == "L" ? -1 : 1;
                Vector3 shoulder = Center(sourceParts,
                    p => p.lowerName.Contains("ball_joint_shoulder_ball_joint") && (p.rawBounds.center.x * sign > 0),
                    new Vector3(sign * 41.78f, 224, -2.5f));
                Vector3 elbow = Center(sourceParts,
                    p => p.lowerName.Contains("_" + sl + "_arm_assy_") && p.lowerName.Contains("_elbow_fulcrum_"),
                    new Vector3(sign * 51, 196.2f, -6.7f));
                Vector3 wrist = Center(sourceParts,
                    p => p.lowerName.Contains("_" + sl + "_arm_assy_") && p.lowerName.EndsWith("_palm_connector"),
                    new Vector3(sign * 60, 158.2f, 7.8f));
                Vector3 hip = Center(sourceParts,
                    p => p.lowerName.Contains("_waist_ball_joint_waist_ball_joint_g_" + sl + "_leg_assy_"),
                    new Vector3(sign * 16.75f, 167.67f, 2.32f));
                Vector3 knee = Center(sourceParts,
                    p => p.lowerName.Contains("_" + sl + "_leg_assy_") && p.lowerName.EndsWith("_knee_joint"),
                    new Vector3(sign * 24.3f, 119.4f, 1.2f));
                Vector3 ankle = Center(sourceParts,
                    p => p.lowerName.Contains("_" + sl + "_leg_assy_") && p.lowerName.Contains("_ankle_joint_ankle_joint_assy_"),
                    new Vector3(sign * 38, 34.5f, 2.7f));

                Vector3 armorHinge = Center(sourceParts,
                    p => p.lowerName.Contains("shoulder_armor_assy") && p.lowerName.Contains("connector_plate_holder") && p.rawBounds.center.x * sign > 0,
                    shoulder + new Vector3(-sign * 8, 12, 0));
                joint("Shoulder_" + side, "Chest", armorHinge);
                // Independent armor hinge uses connector holder; arm uses shoulder ball joint.
                joint("UpperArm_" + side, "Chest", shoulder);
                joint("LowerArm_" + side, "UpperArm_" + side, elbow);
                joint("Hand_" + side, "LowerArm_" + side, wrist);
                joint("UpperLeg_" + side, "Pelvis", hip);
                joint("LowerLeg_" + side, "UpperLeg_" + side, knee);
                joint("Foot_" + side, "LowerLeg_" + side, ankle);
                joint("FrontSkirt_" + side, "Pelvis", new Vector3(sign * 19.5f, 181, 16));
                joint("SideSkirt_" + side, "Pelvis", new Vector3(sign * 28.0f, 182, 3));
                joint("RearSkirt_" + side, "Pelvis", new Vector3(sign * 18.0f, 181, -12));
            }

            var mappings = new List<MechaRigData.PartBinding>();
            var report = new StringBuilder("# Mechanical rig mapping\n\n");
            report.AppendLine("Source geometry is preserved, no vertex deformation or skinning. +Z front, +Y up, source L is x<0.\n");
            report.AppendLine("Source height: " + sourceBounds.size.y.ToString("F6", CultureInfo.InvariantCulture) + "; scale: " + scale.ToString("F9", CultureInfo.InvariantCulture));
            report.AppendLine("Joint centers measured from mechanical parts; pelvis/chest/skirt hinge locations are assembly-informed authored pivots and need visual clearance testing.\n");
            report.AppendLine("| Part | Driver | Evidence |\n| --- | --- | --- |");
            float maxError = 0;
            var groupCounts = new Dictionary<string, int>();
            foreach (var part in sourceParts)
            {
                string evidence;
                string role = Classify(part, out evidence);
                if (!joints.TryGetValue(role, out Transform driver)) throw new InvalidOperationException("Unknown role " + role);
                Transform t = part.filter.transform;
                Matrix4x4 before = t.localToWorldMatrix;
                t.SetParent(driver, true);
                Matrix4x4 after = t.localToWorldMatrix;
                Bounds meshBounds = part.filter.sharedMesh.bounds;
                for (int c = 0; c < 8; c++)
                {
                    Vector3 rawPoint = meshBounds.center + Vector3.Scale(meshBounds.extents, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1));
                    maxError = Mathf.Max(maxError, Vector3.Distance(before.MultiplyPoint3x4(rawPoint), after.MultiplyPoint3x4(rawPoint)));
                }
                mappings.Add(new MechaRigData.PartBinding { sourceName = t.name, part = t, driver = driver,
                    mechanicalRole = role, confidence = evidence, restLocalPosition = t.localPosition,
                    restLocalRotation = t.localRotation, restLocalScale = t.localScale });
                if (!groupCounts.ContainsKey(role)) groupCounts[role] = 0;
                groupCounts[role]++;
                report.AppendLine("| " + t.name + " | " + role + " | " + evidence + " |");
            }
            rig.maximumRestPoseError = maxError;
            if (maxError > .0005f) throw new InvalidOperationException("Rest pose preservation failed, maximum error " + maxError);
            clone.transform.SetParent(player.transform, false);
            clone.SetActive(false);
            rig.joints = joints.Values.ToArray();
            rig.restLocalPositions = rig.joints.Select(t => t.localPosition).ToArray();
            rig.restLocalRotations = rig.joints.Select(t => t.localRotation).ToArray();
            rig.parts = mappings.ToArray();

            var gripRight = PalmGrip(sourceParts, "r") * scale + offset;
            var gripLeft = PalmGrip(sourceParts, "l") * scale + offset;
            rig.swordSocket = Child("SwordSocket", joints["Hand_R"], joints["Hand_R"].InverseTransformPoint(gripRight));
            rig.swordSocket.localRotation = Quaternion.Euler(45, 0, 0); // Blade follows socket +Z; 45-degree grip supports authored overhead arc without extreme wrist flexion.
            rig.gunSocket = Child("GunSocket", joints["Hand_L"], joints["Hand_L"].InverseTransformPoint(gripLeft));
            rig.gunSocket.localRotation = Quaternion.Euler(80, 0, 0); // Authored aim upper arm -53 X plus elbow -27 X; socket +Z then faces forward.
            var thrusters = new List<Transform>();
            foreach (string side in new[] { "L", "R" })
            {
                float sign = side == "L" ? -1 : 1;
                Vector3 jetPoint = Center(sourceParts, p => p.lowerName.Contains("back_pack") && p.lowerName.Contains("flap_thruster") && p.rawBounds.center.x * sign > 0,
                    new Vector3(sign * 14, 202, -36));
                jetPoint.z -= 4;
                var nozzle = Child("Thruster_" + side, joints["Backpack"], joints["Backpack"].InverseTransformPoint(jetPoint * scale + offset));
                nozzle.localRotation = Quaternion.Euler(0, 180, 0); // local +Z is exhaust direction, towards model back.
                thrusters.Add(nozzle);
            }
            rig.thrusterSockets = thrusters.ToArray();
            rig.rigNotes = "Rigid-part mechanical rig. " + mappings.Count + " imported parts mapped by assembly tokens and geometry. Rest local rotations of joints are identity. +Z forward. Arm/hip forward flexion = negative X; knee backward flexion = positive X. Shoulder armor and skirts independent. Fingers remain rigid in their original gripping pose. Inspect extreme combat poses for armor clearance.";
            Child("CameraTarget", player.transform, new Vector3(0, 1.55f, 0));
            Child("TargetCenter", player.transform, new Vector3(0, 1.25f, 0));
            report.AppendLine("\n## Joint centers (normalized meters)\n");
            foreach (var pair in joints) report.AppendLine("- " + pair.Key + ": " + pair.Value.position.ToString("F5"));
            report.AppendLine("\n## Group counts\n");
            foreach (var count in groupCounts.OrderBy(p => p.Key)) report.AppendLine("- " + count.Key + ": " + count.Value);
            report.AppendLine("\nMapped parts: " + mappings.Count + "; rest pose max error (m): " + maxError.ToString("F8", CultureInfo.InvariantCulture));
            report.AppendLine("\nValidation status: numerical rest preservation checked. Visual range/armor/weapon grip validation still required; no claim of final animation quality.");
            File.WriteAllText(assetRoot + "/Reports/RigMapping.md", report.ToString(), new UTF8Encoding(false));
            rig.ResetPose();
            EditorUtility.SetDirty(rig);
            AssetDatabase.Refresh();
            return player;
        }

        private static Transform Child(string name, Transform parent, Vector3 position)
        {
            Transform t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = position; t.localRotation = Quaternion.identity; t.localScale = Vector3.one;
            return t;
        }

        private static Vector3 Center(List<SourcePart> parts, Predicate<SourcePart> predicate, Vector3 fallback)
        {
            SourcePart found = parts.Find(predicate);
            return found == null ? fallback : found.rawBounds.center;
        }

        private static Vector3 PalmGrip(List<SourcePart> parts, string side)
        {
            float sign = side == "l" ? -1 : 1;
            Vector3 center = Center(parts, p => p.lowerName.Contains("_" + side + "_palm_assy_") && p.lowerName.EndsWith("_palm_base"),
                new Vector3(sign * 61, 151.5f, 11));
            return center + new Vector3(-sign * 3, -5, 3);
        }

        private static string Classify(SourcePart p, out string evidence)
        {
            string n = p.lowerName;
            string side = p.rawBounds.center.x < 0 ? "L" : "R";
            evidence = "assembly token + geometry side";
            if (n.Contains("head_assy")) return "Head";
            if (n.Contains("back_pack_assy")) return "Backpack";
            if (n.Contains("_l_palm_assy_") || n.Contains("_r_palm_assy_")) return "Hand_" + side;
            if (n.Contains("_l_arm_assy_") || n.Contains("_r_arm_assy_"))
            {
                if (n.Contains("lower_arm_assy") || n.Contains("elbow_fulcrum")) return "LowerArm_" + side;
                return "UpperArm_" + side;
            }
            if (n.Contains("_l_leg_assy_") || n.Contains("_r_leg_assy_"))
            {
                if (n.Contains("waist_ball_joint") || n.Contains("upper_leg_assy") || n.Contains("upper_thigh")) return "UpperLeg_" + side;
                if (n.Contains("feet_assy") || n.Contains("feet_slide_assy") || n.Contains("feet_cap") || n.Contains("ankle_joint_ankle_joint")) return "Foot_" + side;
                return "LowerLeg_" + side;
            }
            if (n.Contains("ball_joint_shoulder_ball_joint")) return "UpperArm_" + side;
            if (n.Contains("shoulder_armor_assy") || n.Contains("connector_plate_connector_plate")) return "Shoulder_" + side;
            if (n.Contains("front_skirt_assy")) return "FrontSkirt_" + side;
            if (n.Contains("side_skirt_assy")) return "SideSkirt_" + side;
            if (n.Contains("rear_skirt_assy")) return "RearSkirt_" + side;
            if (n.Contains("lower_waist_assy") || n.Contains("abdoment_inner_frame")) return "Spine";
            if (n.Contains("waist_assy") || n.Contains("waist_ball_joint")) return "Pelvis";
            if (n.Contains("neck_port")) return "Neck";
            if (n.Contains("body_assy")) return "Chest";
            evidence = "UNRESOLVED - review required";
            throw new InvalidOperationException("Unclassified mesh: " + p.filter.name);
        }
    }
}




