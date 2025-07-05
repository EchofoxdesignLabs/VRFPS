using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    [RequireComponent(typeof(Renderer))]
    public class DeadMaterial : MonoBehaviour
    {
        public Material deadMaterial;
        
        private Damageable _damageable;
        private Material _defaultMaterial;
        private Renderer _renderer;

        // Start is called before the first frame update
        private void Start()
        {
            _damageable = GetComponentInParent<Damageable>();
            _renderer = GetComponent<Renderer>();
            _defaultMaterial = _renderer.material;
        }

        // Update is called once per frame
        void Update()
        {
            _renderer.material = (_damageable.health > 0) ? _defaultMaterial : deadMaterial;
        }
    }
}