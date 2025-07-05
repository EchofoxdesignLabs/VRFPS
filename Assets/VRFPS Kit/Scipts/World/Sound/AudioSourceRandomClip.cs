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
    public class AudioSourceRandomClip : MonoBehaviour
    {
        public AudioClip[] randomAudioClips;
        public bool alwaysUnique = true;
        
        private AudioSource _audio;
        private bool _wasPlayingLastFrame;
        private float _audioTimeLastFrame;
        
        void Awake()
        {
            _audio = GetComponent<AudioSource>();
            RandomizeAudioClip();
        }
        
        // Update is called once per frame
        void Update()
        {
            if(randomAudioClips == null || randomAudioClips.Length == 0){Debug.LogWarning("AudioSourceRandomClip has no clips to randomize from"); return;}
            
            //If just stopped playing
            if (_wasPlayingLastFrame && !_audio.isPlaying)
                RandomizeAudioClip();
            
            //If sound just restarted playing
            if (_audio.time < _audioTimeLastFrame && _audio.isPlaying)
            {
                RandomizeAudioClip();
                _audio.Play(); //Changing audio clip cancels play, restart it
            }
            
            _wasPlayingLastFrame = _audio.isPlaying;
            _audioTimeLastFrame = _audio.time;
        }

        private void RandomizeAudioClip()
        {
            List<AudioClip> clips = new List<AudioClip>(randomAudioClips);
            if (alwaysUnique) clips.Remove(_audio.clip);
            
            _audio.clip = clips[Random.Range(0, clips.Count)];
        }
    }
}
