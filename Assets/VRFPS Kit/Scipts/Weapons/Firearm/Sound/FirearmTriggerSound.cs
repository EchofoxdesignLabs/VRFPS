using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class FirearmTriggerSound : MonoBehaviour
    {
        public AudioClip pressSound;
        public AudioClip releaseSound;
        public AudioClip resetSound;

        private void OnTriggerPress()
        {
            if(pressSound == null) return;
            GetComponent<AudioSource>().PlayOneShot(pressSound);
        }
        
        private void OnTriggerRelease()
        {
            if(releaseSound == null) return;
            GetComponent<AudioSource>().PlayOneShot(releaseSound);
        }
        
        private void OnTriggerReset()
        {
            if(resetSound == null) return;
            GetComponent<AudioSource>().PlayOneShot(resetSound);
        }

        private void Awake()
        {
            FirearmTrigger trigger = GetComponentInParent<FirearmTrigger>();
            if(!trigger) {Debug.LogError("FireModeSwitchSound couldn't find FirearmTrigger component in parent"); return; }

            trigger.PressEvent += OnTriggerPress;
            trigger.ReleaseEvent += OnTriggerRelease;
            trigger.ResetEvent += OnTriggerReset;
        }
    }
}
