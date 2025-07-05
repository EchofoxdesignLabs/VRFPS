using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class BulletPassSound : MonoBehaviour
    {
        public float minimumVelocity = SpeedOfSound.Value;
        [Tooltip("Can be used to make whiz sounds be played earlier")] 
        public float bulletOffset = 0;
        
        private static AudioListener _cachedListener;
        private Vector3 _lastListenerPositionLocal;
        private Vector3 _lastFramePosition;
        private Vector3 _lastFrameVelocity;
        private bool _hasPlayed;
        private bool _movingTowardsListenerAtSpawn;
        
        private Bullet _bullet;
        
        // Update is called once per frame
        void FixedUpdate()
        {
            AudioListener listener = GetListener();
            if (listener == null) return;
            if (_hasPlayed) return;

            Vector3 velocity = EstimateVelocity();
            // Get the listener position in local space
            Vector3 listenerPositionLocal = ListenerPositionLocal();
    
            if (_lastFrameVelocity.magnitude > minimumVelocity)
            {
                //Check if just passed listener
                if (listenerPositionLocal.z - bulletOffset < 0 && _lastListenerPositionLocal.z - bulletOffset > 0){
                    GetComponent<AudioSource>().PlayDelayedBySpeedOfSound();

                    //Deparent the audio source so sound doesn't keep traveling
                    transform.parent = null;
                    Destroy(gameObject, 5);
                    _hasPlayed = true;
                }

                //Check if stop moving
                if (GetRemainingVelocity01() < .2f && _movingTowardsListenerAtSpawn)
                {
                    GetComponent<AudioSource>().PlayDelayedBySpeedOfSound();
                    transform.parent = null;
                    Destroy(gameObject, 5);
                    _hasPlayed = true;
                }
            }

            _lastListenerPositionLocal = listenerPositionLocal;
            _lastFramePosition = transform.position;
            _lastFrameVelocity = velocity;
        }

        private void Start()
        {
            if(ListenerPositionLocal().z - bulletOffset > 0) _movingTowardsListenerAtSpawn = true;
        }
        
        /// <summary>
        /// We need to estimate velocity by measuring how far we've traveled since last frame as we can't
        /// Use RigidBody.velocity in multiplayer
        /// </summary>
        /// <returns></returns>
        /// TODO WARNING can be null if listener is null
        private Vector3 EstimateVelocity() => (_lastFramePosition - transform.position) / Time.fixedDeltaTime;
        
        private float GetRemainingVelocity01() => Mathf.InverseLerp(0, _bullet.ballisticProfile.startVelocity, EstimateVelocity().magnitude); 
        
        private Vector3 ListenerPositionLocal() => transform.InverseTransformPoint(GetListener().transform.position);
        
        private static AudioListener GetListener()
        {
            if(_cachedListener == null || !_cachedListener.enabled) 
                _cachedListener = Object.FindAnyObjectByType<AudioListener>(FindObjectsInactive.Exclude);

            return _cachedListener;
        }

        private void Awake()
        {
            _bullet = GetComponentInParent<Bullet>();
        }
    }
}
