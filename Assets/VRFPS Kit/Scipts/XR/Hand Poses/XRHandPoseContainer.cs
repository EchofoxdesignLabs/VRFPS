using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit.HandPoses
{
    [RequireComponent(typeof(XRBaseInteractable))]
    public class XRHandPoseContainer : MonoBehaviour
    {
        public Pose pose;
        [Space] [Tooltip("Useful to give non Grab-interactables hand pose attach points")]
        public Transform customAttachPoint;
        [Tooltip("Index of the interator to use for this pose. 0 is the first interactor, 1 is the second")] 
        public int interactorIndex = 0;
        
        public Transform GetAttachPoint()
        {
            //Always return custom customAttachPoint if it's set
            if (customAttachPoint) return customAttachPoint;
            if (GetComponent<XRBaseInteractable>() is not XRGrabInteractable grabInteractable) return transform;

            Transform attachTransform = grabInteractable.attachTransform;
            if (interactorIndex == 1) attachTransform = grabInteractable.secondaryAttachTransform;
            if (attachTransform == null) attachTransform = transform;
            
            return attachTransform;
        }
    }
}
