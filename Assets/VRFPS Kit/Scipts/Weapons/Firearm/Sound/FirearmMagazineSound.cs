using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class FirearmMagazineSound : MonoBehaviour
    {
        private Magazine _magazineLastFrame;

        private Firearm _firearm;

        private void Update()
        {
            //If magazine changed this frame, play sound
            if(_magazineLastFrame != _firearm.magazine)
                GetComponent<AudioSource>().Play();
            
            _magazineLastFrame = _firearm.magazine;
        }
        
        private void Awake()
        {
            _firearm = GetComponentInParent<Firearm>();
            if(!_firearm) Debug.LogError("FirearmMagazineSound couldn't find Firearm component in parent");
        }
    }
}
