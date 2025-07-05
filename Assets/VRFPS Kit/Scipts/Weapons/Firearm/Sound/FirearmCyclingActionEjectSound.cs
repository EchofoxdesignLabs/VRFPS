using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class FirearmCyclingActionEjectSound : MonoBehaviour
    {
        public AudioClip ejectSound;
        
        private CartridgeEjector _ejector;

        private async void OnEject(Cartridge obj)
        {
            await Task.Delay(50);
            
            if (FirearmCyclingActionSound.SilentWhenShooting && Time.time - GetComponentInParent<Firearm>().lastShotTime < .3)
                return;
            
            GetComponent<AudioSource>().PlayOneShot(ejectSound);
        }

        private void Awake()
        {
            _ejector = GetComponentInParent<CartridgeEjector>();
            if (!_ejector)
            {
                Debug.LogError("FirearmCyclingActionEjectSound couldn't find CartridgeEjector component in parent"); 
                return;
            }
            
            _ejector.EjectEvent += OnEject;
        }
    }
}
