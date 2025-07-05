using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VRFPSKit
{
    public class Bird : MonoBehaviour
    {
        public float speed;
        public float lifetime = 20;

        private float _flapEndTime = 0;
        private float _floatEndTime = 0;
        
        private Animator _animator;
        
        // Update is called once per frame
        void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            
            _animator.SetBool("Flap", Time.time < _flapEndTime);
            
            //Are we done flapping & floating? Start flapping again
            if (_flapEndTime < Time.time && _floatEndTime < Time.time)
            {
                //Start flapping again for 3 - 5s
                _flapEndTime = Time.time + Random.Range(1, 5);
                _floatEndTime = _flapEndTime + Random.Range(4, 7);
            }
        }

        private void Start()
        {
            _flapEndTime = Time.time + 3;
            
            Invoke(nameof(Despawn), lifetime);
        }
        
        private void Despawn() =>  Destroy(gameObject);

        void Awake()
        {
            _animator = GetComponent<Animator>();
        }
    }
}
