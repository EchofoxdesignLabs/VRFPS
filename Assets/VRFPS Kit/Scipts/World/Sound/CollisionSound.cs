
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRFPSKit
{
    public class CollisionSound : MonoBehaviour
    {
        private const float soundCooldown = .2f;
        
        //By default, we only want to play collision sounds when colliding with the environment ("Default" layer)
        public LayerMask collisionMask = 1 << 0;
        public AudioSource source;

        private float _lastPlayTime;

        private void OnCollisionEnter(Collision collision)
        {
            if(Time.timeSinceLevelLoad < 1f) return; //Ignore collisions that happen because of spawn
            //Only play sound if collisionMask contains collider layer
            if (!DoesMaskContainLayer(collision.gameObject.layer, collisionMask)) return;
            if (!source) return;
            float collisionSpeed = collision.impulse.magnitude / GetComponent<Rigidbody>().mass;
            if (collisionSpeed < .003f) return;
            if (Time.time - _lastPlayTime < soundCooldown) return;
            
            //TODO are collisions triggered on clients? 
            source.Play();
            _lastPlayTime = Time.time;
        }

        bool DoesMaskContainLayer(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
    }
}