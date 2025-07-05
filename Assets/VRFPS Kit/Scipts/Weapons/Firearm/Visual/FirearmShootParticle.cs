using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    [RequireComponent(typeof(ParticleSystem))]
    public class FirearmShootParticle : MonoBehaviour
    {
        public bool playWhenUnsuppressed = true;
        public bool playWhenSuppressed = false;
        
        private Firearm _firearm;
        private XRSocketInteractor _suppressorSocket;
        
        private void ShootEffects()
        {
            bool shouldPlay = IsSuppressed() ? playWhenSuppressed : playWhenUnsuppressed;
            
            if(shouldPlay) GetComponent<ParticleSystem>().Play();
        }

        private bool IsSuppressed()
        {
            if (!_suppressorSocket) return false;
            if (_suppressorSocket.interactablesSelected.Count == 0) return false;
            
            return true;
        }

        private void Awake()
        {
            _firearm = GetComponentInParent<Firearm>();
            if(!_firearm) Debug.LogError("FirearmShootSound couldn't find Firearm component in parent");
            
            _suppressorSocket = _firearm.GetComponentInChildren<SuppressorSocket>()?.GetComponent<XRSocketInteractor>();

            _firearm.ShootEvent += _ => ShootEffects();
        }
    }
}
