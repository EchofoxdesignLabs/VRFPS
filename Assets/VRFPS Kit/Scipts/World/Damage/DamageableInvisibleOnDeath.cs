using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    public class DamageableInvisibleOnDeath : MonoBehaviour
    {
        private Damageable _damageable;
        private Quaternion _defaultRotation;

        // Start is called before the first frame update
        private void Start()
        {
            _damageable = GetComponentInParent<Damageable>();
            _damageable.DeathEvent += () => SetVisible(false);
            _damageable.ResetHealthEvent += () => SetVisible(true);
        }

        // Update is called once per frame
        void SetVisible(bool visible)
        {
            foreach(var renderer in GetComponentsInChildren<MeshRenderer>())
                //Set visibility on clients based on whether damageable is destroyed
                renderer.enabled = visible;
        }
    }
}