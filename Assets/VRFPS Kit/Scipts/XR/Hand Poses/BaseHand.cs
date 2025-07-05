using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit.HandPoses
{
    public abstract class BaseHand : MonoBehaviour
    {
        // Neutral pose for the hand
        [SerializeField] protected Pose defaultPose = null;

        // Serialized so it can be used in editor by the preview hand
        [SerializeField] protected List<Transform> fingerRoots = new List<Transform>();

        // What kind of hand is this?
        [SerializeField] protected InteractorHandedness handType = InteractorHandedness.None;
        public InteractorHandedness HandType => handType;
        [HideInInspector] public Transform attachTransform;

        public List<Transform> Joints { get; protected set; } = new List<Transform>();

        protected virtual void Awake()
        {
            Joints = CollectJoints();
        }

        protected List<Transform> CollectJoints()
        {
            List<Transform> joints = new List<Transform>();

            foreach (Transform root in fingerRoots)
                joints.AddRange(root.GetComponentsInChildren<Transform>());

            return joints;
        }

        public List<Quaternion> GetJointRotations()
        {
            List<Quaternion> rotations = new List<Quaternion>();

            foreach (Transform joint in Joints)
                rotations.Add(joint.localRotation);

            return rotations;
        }

        public void ApplyDefaultPose()
        {
            ApplyPose(defaultPose);
        }

        public void ApplyPose(Pose pose)
        {
            // Get the proper info using hand's type
            HandInfo handInfo = pose.GetHandInfo(handType);

            // Apply rotations 
            ApplyFingerRotations(handInfo.fingerRotations);

            // Position, and rotate, this differs on the type of hand
            ApplyOffset(handInfo.attachPosition, handInfo.attachRotation);
        }

        public void ApplyFingerRotations(List<Quaternion> rotations)
        {
            // Make sure we have the rotations for all the joints
            if (HasProperCount(rotations))
            {
                // Set the local rotation of each joint
                for (int i = 0; i < Joints.Count; i++)
                    Joints[i].localRotation = rotations[i];
            }else Debug.LogWarning("Can't apply hand pose! Wrong number of digits");
        }

        private bool HasProperCount(List<Quaternion> rotations)
        {
            return Joints.Count == rotations.Count;
        }


        public void ApplyOffset(Vector3 positionOffset, Quaternion rotationOffset)
        {
            transform.rotation = attachTransform.rotation * rotationOffset;
            transform.position = TransformPointUnscaled(attachTransform, positionOffset);
        }

        public Vector3 GetAttachPositionOffset()
        {
            if (!attachTransform) return transform.localPosition;

            return InverseTransformPointUnscaled(attachTransform, transform.position);
        }

        public Quaternion GetAttachRotationOffset()
        {
            if (!attachTransform) return transform.localRotation;
            
            return Quaternion.Inverse(attachTransform.rotation) * transform.rotation;
        }
        
        public static Vector3 TransformPointUnscaled(Transform transform, Vector3 position)
        {
            var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            return localToWorldMatrix.MultiplyPoint3x4(position);
        }
        
        public static Vector3 InverseTransformPointUnscaled(Transform transform, Vector3 position)
        {
            var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
            return worldToLocalMatrix.MultiplyPoint3x4(position);
        }
    }
}
