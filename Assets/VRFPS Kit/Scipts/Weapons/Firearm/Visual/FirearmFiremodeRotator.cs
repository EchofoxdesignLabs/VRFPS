using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    public class FirearmFiremodeRotator : MonoBehaviour
    {
        public float transitionSpeed = 700;
        [FormerlySerializedAs("FireModeRotations")] [Space]
        public FireModeRotation[] fireModeRotations;// We can't use a dictionary here because Unity doesn't serialize them, won't show up in inspector
        
        private Firearm _firearm;
        
        // Update is called once per frame
        void Update()
        {
            // Interpolate between the rotations
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(GetRotationForFireMode(_firearm.currentFireMode)), transitionSpeed * Time.deltaTime);
        }

        private Vector3 GetRotationForFireMode(FireMode fireMode) => fireModeRotations.First(x => x.fireMode == fireMode).rotation;
        
        void Awake()
        {
            _firearm = GetComponentInParent<Firearm>();
        }
    }
    
    [Serializable]
    public struct FireModeRotation {
        public FireMode fireMode;
        public Vector3 rotation;
    }
}
