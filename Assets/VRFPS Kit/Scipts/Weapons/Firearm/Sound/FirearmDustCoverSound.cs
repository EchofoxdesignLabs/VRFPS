using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class FirearmDustCoverSound : MonoBehaviour
    {
        public AudioClip feedSound;
        
        private FirearmDustCover _dustCover;

        private void OnCoverOpen()
        {
            if (FirearmCyclingActionSound.SilentWhenShooting && Time.time - GetComponentInParent<Firearm>().lastShotTime < .3)
                return;
            
            GetComponent<AudioSource>().PlayOneShot(feedSound);
        }

        private void Awake()
        {
            _dustCover = GetComponentInParent<Firearm>().GetComponentInChildren<FirearmDustCover>();
            if (!_dustCover)
            {
                Debug.LogError("FirearmDustCoverSound couldn't find FirearmDustCover component"); 
                return;
            }
            
            _dustCover.DustCoverOpenEvent += OnCoverOpen;
        }
    }
}
