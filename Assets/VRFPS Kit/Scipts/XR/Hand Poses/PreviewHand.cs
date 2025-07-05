using System.Collections.Generic;
using UnityEngine;

namespace VRFPSKit.HandPoses
{
    [SelectionBase]
    [ExecuteInEditMode]
    public class PreviewHand : BaseHand
    {
        public void MirrorAndApplyPose(PreviewHand sourceHand)
        {
            // Mirror and apply the joint values
            List<Quaternion> mirroredRotations = MirrorJoints(sourceHand.Joints);
            ApplyFingerRotations(mirroredRotations);

            // Mirror and apply the position and rotation
            Vector3 mirroredPosition = MirrorPosition(sourceHand);
            Quaternion mirroredRotation = MirrorRotation(sourceHand);
            ApplyOffset(mirroredPosition, mirroredRotation);
        }

        private List<Quaternion> MirrorJoints(List<Transform> joints)
        {
            List<Quaternion> mirroredJoints = new List<Quaternion>();

            foreach (Transform joint in joints)
            {
                Quaternion inversedRotation = MirrorJoint(joint);
                mirroredJoints.Add(inversedRotation);
            }

            return mirroredJoints;
        }

        private Quaternion MirrorJoint(Transform sourceTransform)
        {
            Quaternion mirrorRotation = new Quaternion(sourceTransform.localRotation.x, sourceTransform.localRotation.y,
                sourceTransform.localRotation.z, sourceTransform.localRotation.w);
            //mirrorRotation.x *= -1.0f;

            return mirrorRotation;
        }

        private Quaternion MirrorRotation(PreviewHand sourceHand)
        {
            Quaternion mirrorRotation = sourceHand.GetAttachRotationOffset();
            mirrorRotation.y *= -1.0f;
            mirrorRotation.z *= -1.0f;
            return mirrorRotation;
        }

        private Vector3 MirrorPosition(PreviewHand sourceHand)
        {
            Vector3 mirroredPosition = sourceHand.GetAttachPositionOffset();
            mirroredPosition.x *= -1.0f;
            return mirroredPosition;
        }
    }
}