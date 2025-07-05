using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    [RequireComponent(typeof(Damageable))]
    public class DamageParticleEffect : MonoBehaviour
    {
        public ParticleSystem particle;
        public bool onlyPlayOnDeath;
        
        private Damageable _damageable;
        
        // Update is called once per frame
        private void OnDamage(float damage)
        {
            //Wait for death event if onlyPlayOnDeath is enabled
            if (onlyPlayOnDeath) return;
            
            particle.Play();
        }
        
        // Update is called once per frame
        private void OnDeath()
        {
            if (!onlyPlayOnDeath) return;
            
            particle.Play();
        }
        
        private void Awake()
        {
            _damageable = GetComponent<Damageable>();

            _damageable.DamageEvent += OnDamage;
            _damageable.DeathEvent += OnDeath;
        }
    }
}