using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    public class FirearmTriggerRotator : MonoBehaviour
    {
        [Space]
        public Vector3 pressedRotation;
        
        private Quaternion _unpressedRotation;
        
        private FirearmTrigger _trigger;
        
        // Update is called once per frame
        void Update()
        {
            // Interpolate between the rotations
            transform.localRotation = Quaternion.Lerp(_unpressedRotation, Quaternion.Euler(pressedRotation), _trigger.triggerProgress01);
        }
        
        void Awake()
        {
            _trigger = GetComponentInParent<FirearmTrigger>();
            _unpressedRotation = transform.localRotation;
        }
    }
}
