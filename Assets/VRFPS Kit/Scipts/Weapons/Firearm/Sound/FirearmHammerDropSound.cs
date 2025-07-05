using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class FirearmHammerDropSound : MonoBehaviour
    {
        private bool _cockedLastFrame;

        private Firearm _firearm;

        private void Update()
        {
            //If cocked state changed this frame, play sound
            if(!_firearm.isHammerCocked && _cockedLastFrame)
                GetComponent<AudioSource>().Play();
            
            _cockedLastFrame = _firearm.isHammerCocked;
        }

        private void Awake()
        {
            _firearm = GetComponentInParent<Firearm>();
            if(!_firearm) Debug.LogError("FireModeSwitchSound couldn't find Firearm component in parent");
        }
    }
}
