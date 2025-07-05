using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class FirearmShootSound : MonoBehaviour
    {
        public bool playWhenUnsuppressed = true;
        public bool playWhenSuppressed = false;
        [Space]
        public bool makeObjectCopies = true;
        public bool duckAudioBeforeShot = true;
        
        private Firearm _firearm;
        private XRSocketInteractor _suppressorSocket;
        private List<AudioSource> _soundSourceObjects = new List<AudioSource>();
        
        private async void ShootEffects()
        {
            bool shouldPlay = IsSuppressed() ? playWhenSuppressed : playWhenUnsuppressed;

            if (!shouldPlay) return;
            
            //wait for speed of sound
            await Task.Delay((int)(SpeedOfSound.CalculateDelay(transform) * 1000 / Time.timeScale));

            if (duckAudioBeforeShot)
            {
                float volumeBeforeDuck = GetComponent<AudioSource>().volume;
                //Briefly disable sound for duck effect before shooting
                foreach (AudioSource previousShootSound in _soundSourceObjects)
                    if (previousShootSound != null)
                        previousShootSound.volume = 0.1f;

                await Task.Delay(35);

                foreach (AudioSource previousShootSound in _soundSourceObjects)
                    if (previousShootSound != null)
                        previousShootSound.volume = volumeBeforeDuck;
            }

            if (makeObjectCopies)
            {
                GameObject soundCopy = Instantiate(gameObject, transform.position, transform.rotation, null);
                DestroyImmediate(soundCopy.GetComponent<FirearmShootSound>());//Remove copy of this script immediately, to prevent accessing components that don't exist in Awake()
                soundCopy.GetComponent<AudioSource>().PlayDelayedBySpeedOfSound();
                
                _soundSourceObjects.Add(soundCopy.GetComponent<AudioSource>());
                
                await Task.Delay((int)(GetComponent<AudioSource>().clip.length * 1000));//Destroy sound copy obj after delay
                
                _soundSourceObjects.Remove(soundCopy.GetComponent<AudioSource>());
                Destroy(soundCopy);
            }
            else
            {
                GetComponent<AudioSource>().PlayDelayedBySpeedOfSound();
                _soundSourceObjects.Add(GetComponent<AudioSource>());
            }
        }

        private bool IsSuppressed()
        {
            if (!_suppressorSocket) return false;
            if (_suppressorSocket.interactablesSelected.Count == 0) return false;
            
            return true;
        }
        
        private void Start()
        {
            _firearm = GetComponentInParent<Firearm>();
            
            if (!_firearm) return;
            _firearm.ShootEvent += _ => ShootEffects();
            
            if(_firearm.GetComponentInChildren<SuppressorSocket>() is SuppressorSocket suppressorSocket)
                _suppressorSocket = suppressorSocket.GetComponent<XRSocketInteractor>();
        }
    }
}
