using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class FirearmFireModeSound : MonoBehaviour
    {
        private FireMode _fireModeLastFrame;

        private Firearm _firearm;

        private void Update()
        {
            //If fire mode changed this frame, play sound
            if(_fireModeLastFrame != _firearm.currentFireMode)
                GetComponent<AudioSource>().Play();
            
            _fireModeLastFrame = _firearm.currentFireMode;
        }

        private void Awake()
        {
            _firearm = GetComponentInParent<Firearm>();
            if(!_firearm) Debug.LogError("FireModeSwitchSound couldn't find Firearm component in parent");
        }
    }
}
