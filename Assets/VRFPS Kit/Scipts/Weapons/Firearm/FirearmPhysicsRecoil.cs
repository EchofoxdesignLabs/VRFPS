using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Random = UnityEngine.Random;

namespace VRFPSKit
{
    /// <summary>
    /// TODO
    /// </summary>
    [RequireComponent(typeof(Firearm), typeof(Rigidbody))]
    public class FirearmPhysicsRecoil : MonoBehaviour
    {
        public Vector3 recoil;
        public Vector3 angularRecoil;
        [Space]
        public Vector3 randomRecoil;
        public Vector3 randomAngularRecoil;
        [Space]
        public float resetFactor = 3f;
        public float angularResetFactor = 4f;
        public float oneHandedRecoilMultiplier = 2.5f;
        [Space]
        public AnimationCurve recoilMultiplierOverCurrentMagnitude = AnimationCurve.EaseInOut(0, 1, 3, .1f);
        public AnimationCurve randomRecoilMultiplierOverCurrentMagnitude = AnimationCurve.EaseInOut(0, 1, 3, .2f);
        
        private Vector3 _currentRecoilForce;
        private Vector3 _currentAngularRecoil;

        private void ShootEvent(Cartridge cartridge)
        {
            ApplyRecoil(recoil + RandomVector3(randomRecoil) * randomRecoilMultiplierOverCurrentMagnitude.Evaluate(_currentRecoilForce.magnitude), 
                angularRecoil + RandomVector3(randomAngularRecoil) * randomRecoilMultiplierOverCurrentMagnitude.Evaluate(_currentRecoilForce.magnitude));
        }

        private void ApplyRecoil(Vector3 newLinearRecoil, Vector3 newAngularRecoil)
        {
            //Multiply recoil by recoil curve
            newLinearRecoil *= recoilMultiplierOverCurrentMagnitude.Evaluate(_currentRecoilForce.magnitude);
            newAngularRecoil *= recoilMultiplierOverCurrentMagnitude.Evaluate(_currentRecoilForce.magnitude);
            
            //If held by one hand, multiply by one handed constant
            if (GetAmountOfHolders() == 1)
            {
                newLinearRecoil *= oneHandedRecoilMultiplier;
                newAngularRecoil *= oneHandedRecoilMultiplier;
            }
            
            _currentRecoilForce += newLinearRecoil;
            _currentAngularRecoil += newAngularRecoil;
        }

        private void FixedUpdate()
        {
            Vector3 localRecoilForce = transform.TransformDirection(_currentRecoilForce);
            Vector3 localAngularRecoil = transform.TransformDirection(_currentAngularRecoil);
            
            //If firearm is held, recoil needs to be applied  
            if (GetComponent<XRGrabInteractable>().isSelected)
            {
                if (localRecoilForce.magnitude > 0.05f)
                    GetComponent<Rigidbody>().AddForce(localRecoilForce / 0.015f, ForceMode.Acceleration); //Divide by / 0.015f for old values to still work (was using wrong ForceMode before)
            
                if (localAngularRecoil.magnitude > 0.05f) 
                    GetComponent<Rigidbody>().AddTorque(localAngularRecoil / 0.015f, ForceMode.Acceleration); //Divide by / 0.015f for old values to still work (was using wrong ForceMode before)

                _currentRecoilForce *= Mathf.Max(1f - (resetFactor * Time.fixedDeltaTime), 0);
                _currentAngularRecoil *= Mathf.Max(1f - (angularResetFactor * Time.fixedDeltaTime), 0);
            }
            else
            {
                GetComponent<Rigidbody>().AddForce(localRecoilForce, ForceMode.VelocityChange); //Divide by / 0.015f for old values to still work (was using wrong ForceMode before)
                GetComponent<Rigidbody>().AddTorque(localAngularRecoil, ForceMode.VelocityChange); //Divide by / 0.015f for old values to still work (was using wrong ForceMode before)

                _currentRecoilForce = Vector3.zero;
                _currentAngularRecoil = Vector3.zero;
            }
            //TODO action recoil
            //TODO comments
            //Recoil forces need to be applied continuously as "XRGrabInteractable" velocity tracking will override it
            

            //TODO breaks after dropping weapon and grabbing again
            //TODO apply normal force based on relative hand positions (are attached? where are holding?)
        }

        // Start is called before the first frame update
        void Awake()
        {
            GetComponent<Firearm>().ShootEvent += ShootEvent;
        }
        
        private int GetAmountOfHolders()
        {
            int count = 0;
            foreach (var nestedInteractable in GetComponentsInChildren<XRBaseInteractable>())
                foreach (var selectInteractor in nestedInteractable.interactorsSelecting)
                    count++;

            return count;
        }
        
        private Vector3 RandomVector3(Vector3 bounds) => new (Random.Range(-bounds.x, bounds.x), Random.Range(-bounds.y, bounds.y), Random.Range(-bounds.z, bounds.z));
    }
}