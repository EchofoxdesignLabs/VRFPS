using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    /// <summary>
    /// Will be used to animate the magazine release part of the firearm.
    /// Moves both for input & insertion progress
    /// </summary>
    public class FirearmMagazineReleasePart : MonoBehaviour
    {
        public Vector3 engagedPosition;
        public Vector3 engagedRotation;
        [Space]
        public AnimationCurve insertProgress01Curve = AnimationCurve.Linear(0, 0, 1, 1);
        
        private Vector3 _disengagedPosition;
        private Quaternion _disengagedRotation;
        
        private MagazineEjectorInput _magRelease;
        private PhysicsMagazineInteractor _physicsMagazineInteractor;
        
        // Update is called once per frame
        void Update()
        {
            var input = _magRelease?.DetachInput();
            float progress01 = 0;
            if(input != null) progress01 = input.ReadValue<float>();
            
            if (_physicsMagazineInteractor)
            {
                progress01 += Mathf.Clamp01(insertProgress01Curve.Evaluate(_physicsMagazineInteractor.JointProgress()));
            }
            
            progress01 = Mathf.Clamp01(progress01);
            
            //interpolate between the positions
            if(engagedPosition != Vector3.zero)
                transform.localPosition = Vector3.Lerp(_disengagedPosition, engagedPosition, progress01);
            // Interpolate between the rotations
            if(engagedRotation != Vector3.zero)
                transform.localRotation = Quaternion.Lerp(_disengagedRotation, Quaternion.Euler(engagedRotation), progress01);
        }
        
        void Awake()
        {
            Firearm firearm = GetComponentInParent<Firearm>();
            _magRelease = firearm.GetComponentInChildren<MagazineEjectorInput>();
            _physicsMagazineInteractor = firearm.GetComponentInChildren<PhysicsMagazineInteractor>();
            
            _disengagedPosition = transform.localPosition;
            _disengagedRotation = transform.localRotation;
        }
    }
}
