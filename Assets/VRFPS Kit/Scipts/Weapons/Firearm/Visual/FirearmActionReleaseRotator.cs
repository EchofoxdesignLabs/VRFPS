using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    public class FirearmActionReleaseRotator : MonoBehaviour
    {
        public Vector3 activeRotation;
        
        private Quaternion _defaultRotation;
        
        private FirearmCyclingAction _action;
        
        // Update is called once per frame
        void Update()
        {
            // Interpolate between the rotations
            transform.localRotation = _action.isLockedBack ? Quaternion.Euler(activeRotation) : _defaultRotation;
        }
        
        void Awake()
        {
            _action = GetComponentInParent<FirearmCyclingAction>();
            _defaultRotation = transform.localRotation;
        }
    }
}
