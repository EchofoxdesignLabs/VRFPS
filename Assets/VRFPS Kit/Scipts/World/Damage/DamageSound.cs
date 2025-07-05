using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class DamageSound : MonoBehaviour
    {
        public bool onlyPlayOnDeath;
        
        private Damageable _damageable;
        
        private void OnDamage(float damage)
        {
            //Wait for death event if onlyPlayOnDeath is enabled
            if (onlyPlayOnDeath) return;
            
            GetComponent<AudioSource>().PlayDelayedBySpeedOfSound();
        }
        
        // Update is called once per frame
        private void OnDeath()
        {
            if (!onlyPlayOnDeath) return;
            
            GetComponent<AudioSource>().PlayDelayedBySpeedOfSound();
        }
        
        private void Awake()
        {
            _damageable = GetComponentInParent<Damageable>();

            _damageable.DamageEvent += OnDamage;
            _damageable.DeathEvent += OnDeath;
        }
    }
}