using System;
using UnityEngine;

namespace MechaDemo
{
    [DisallowMultipleComponent]
    public sealed class MechaRigData : MonoBehaviour
    {
        [Serializable]
        public sealed class PartBinding
        {
            public string sourceName;
            public Transform part;
            public Transform driver;
            public string mechanicalRole;
            public string confidence;
            public Vector3 restLocalPosition;
            public Quaternion restLocalRotation;
            public Vector3 restLocalScale;
        }

        public Transform visualRoot;
        public Transform skeletonRoot;
        public Transform[] joints = Array.Empty<Transform>();
        public Vector3[] restLocalPositions = Array.Empty<Vector3>();
        public Quaternion[] restLocalRotations = Array.Empty<Quaternion>();
        public PartBinding[] parts = Array.Empty<PartBinding>();
        public Transform swordSocket;
        public Transform gunSocket;
        public Transform[] thrusterSockets = Array.Empty<Transform>();
        public float sourceToMeters;
        public Vector3 sourceOffset;
        public float referenceHeight = 2.6f;
        public float maximumRestPoseError;
        [TextArea(3, 8)] public string rigNotes;
        public bool drawJointGizmos;

        public Transform Joint(string jointName)
        {
            for (int i = 0; i < joints.Length; i++)
                if (joints[i] != null && joints[i].name == jointName) return joints[i];
            return null;
        }

        public Transform FindJoint(string jointName) => Joint(jointName);

        [ContextMenu("Restore mechanical rest pose")]
        public void ResetPose()
        {
            for (int i = 0; i < joints.Length; i++)
            {
                if (joints[i] == null) continue;
                if (i < restLocalPositions.Length) joints[i].localPosition = restLocalPositions[i];
                if (i < restLocalRotations.Length) joints[i].localRotation = restLocalRotations[i];
            }
        }

        [ContextMenu("Preview single arm and leg")]
        public void PreviewArmLeg()
        {
            ResetPose();
            SetPose("UpperArm_L", new Vector3(-25, 0, -5));
            SetPose("LowerArm_L", new Vector3(-50, 0, 0));
            SetPose("UpperLeg_L", new Vector3(-15, 0, 0));
            SetPose("LowerLeg_L", new Vector3(35, 0, 0));
            SetPose("Foot_L", new Vector3(-15, 0, 0));
            SetPose("FrontSkirt_L", new Vector3(-20, 0, 0));
            SetPose("Shoulder_L", new Vector3(-8, 0, -5));
        }

        public void SetPose(string jointName, Vector3 euler)
        {
            Transform joint = Joint(jointName);
            if (joint != null) joint.localRotation = Quaternion.Euler(euler);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawJointGizmos) return;
            foreach (Transform joint in joints)
            {
                if (joint == null) continue;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(joint.position, .018f);
                if (joint.parent != null && joint.parent != skeletonRoot && joint.parent != visualRoot)
                { Gizmos.color = new Color(.3f, .9f, .7f); Gizmos.DrawLine(joint.position, joint.parent.position); }
                Gizmos.color = Color.red; Gizmos.DrawRay(joint.position, joint.right * .10f);
                Gizmos.color = Color.green; Gizmos.DrawRay(joint.position, joint.up * .10f);
                Gizmos.color = Color.blue; Gizmos.DrawRay(joint.position, joint.forward * .10f);
            }
        }
    }
}
