using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRFPSKit.HandPoses
{
    [Serializable]
    public class HandInfo
    {
        public Vector3 attachPosition = Vector3.zero;
        public Quaternion attachRotation = Quaternion.identity;
        public List<Quaternion> fingerRotations = new();

        public static HandInfo Empty => new HandInfo();

        public void Save(PreviewHand hand, XRHandPoseContainer poseContainer)
        {
            attachPosition = hand.GetAttachPositionOffset();
            attachRotation = hand.GetAttachRotationOffset();

            // Save rotations from the hand's current joints
            fingerRotations = hand.GetJointRotations();
        }
    }
}