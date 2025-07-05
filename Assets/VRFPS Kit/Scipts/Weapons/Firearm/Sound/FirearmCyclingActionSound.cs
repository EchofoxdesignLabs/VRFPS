using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class FirearmCyclingActionSound : MonoBehaviour
    {
        public static bool SilentWhenShooting = false;
        
        public Vector2 actionPositionRange = new (0, .1f);
        public AudioClip rangeActionSound;
        
        private bool _wasInRangeLastFrame;

        private FirearmCyclingAction _action;

        private void Update()
        {
            TryPlay();
            
            _wasInRangeLastFrame = IsActionInRange();
        }

        private void TryPlay()
        {
            if (!IsActionInRange()) return; //Action within sound range?
            if (_wasInRangeLastFrame) return; //If cocked state changed this frame, play sound
            if (Time.timeSinceLevelLoad < 1f) return; //Ignore sounds that happen because of spawn

            Play();
        }

        private async void Play()
        {
            await Task.Delay(100);

            if (SilentWhenShooting && Time.time - GetComponentInParent<Firearm>().lastShotTime < .3)
                return;
            
            GetComponent<AudioSource>().PlayOneShot(rangeActionSound);
        }

        private bool IsActionInRange() => 
            _action.actionPosition01 >= actionPositionRange.x &&
            _action.actionPosition01 <= actionPositionRange.y;

        private void Awake()
        {
            _action = GetComponentInParent<FirearmCyclingAction>();
            if(!_action) Debug.LogError("FirearmCyclingActionSound couldn't find FirearmCyclingAction component in parent");
        }
    }
}
