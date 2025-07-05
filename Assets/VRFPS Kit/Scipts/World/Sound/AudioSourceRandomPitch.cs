using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRFPSKit
{
    /// <summary>
    /// Attach to an AudioSource to randomize the audio clip every time the source plays
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceRandomPitch : MonoBehaviour
    {
        [Range(0, 2)] public float minPitch = .9f;
        [Range(0, 2)] public float maxPitch = 1.1f;
        
        private AudioSource _audio;
        private bool _wasPlayingLastFrame;
        private float _audioTimeLastFrame;
        
        void Awake()
        {
            _audio = GetComponent<AudioSource>();
            RandomizePitch();
        }
        
        // Update is called once per frame
        void Update()
        {
            //If just stopped playing
            if (_wasPlayingLastFrame && !_audio.isPlaying)
                RandomizePitch();
            
            //If sound just restarted playing
            if (_audio.time < _audioTimeLastFrame && _audio.isPlaying)
                RandomizePitch();
            
            _wasPlayingLastFrame = _audio.isPlaying;
            _audioTimeLastFrame = _audio.time;
        }

        private void RandomizePitch()
        {
            _audio.pitch = Random.Range(minPitch, maxPitch);
        }
    }
}
