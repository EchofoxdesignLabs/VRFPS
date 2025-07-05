using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    /// <summary>
    /// Simply tracks health, allowing for further behaviour extention by composition
    /// </summary>
    public class Damageable : MonoBehaviour, IDamageReciever
    {
        public float health;
        [HideInInspector] public float startHealth;
        
        #region Events
        //Events will be called both on server & clients
        public event Action<float> DamageEvent;
        public static event Action<Damageable, float> GlobalDamageEvent;
        
        public event Action DeathEvent;
        public static event Action<Damageable> GlobalDeathEvent;
        
        public event Action ResetHealthEvent;
        #endregion

        public void TakeDamage(float damage)
        {
            health -= damage;
            DamageEvent?.Invoke(damage);

            if (health <= 0)
                DeathEvent?.Invoke();
        }
        
        public void ResetHealth()
        {
            health = startHealth;
            ResetHealthEvent?.Invoke();
        }
        
        private void Awake()
        {
            startHealth = health;

            //Hook global events
            DamageEvent += (damage) => GlobalDamageEvent?.Invoke(this, damage);
            DeathEvent += () => GlobalDeathEvent?.Invoke(this);
        }
    }
}