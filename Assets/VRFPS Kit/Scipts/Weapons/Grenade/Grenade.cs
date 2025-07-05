using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    /// <summary>
    /// Main class that handles behaviour that is in common for all grenades
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public class Grenade : MonoBehaviour, IDamageReciever
    {
        public float fuseDuration;
        public GameObject explosionPrefab;
        [Space] 
        public Rigidbody safetyPin;
        public Rigidbody lever;
        [Space] 
        public AudioSource primerSound;
        public AudioSource pinPullSound;
        public ParticleSystem primeParticle;
        public float gripLeverReleaseThreshold = .8f;

        private double _primedTime = -1;
        
        public Player lastHolder;
        
        // Update is called once per frame
        void Update()
        {
            TryDetachLever();
            
            //If is primed (_primedTime isn't = -1 anymore) & fuse duration has expired, explode
            if (_primedTime > 0 && Time.time - _primedTime > fuseDuration)
                Explode();
        }

        public void DetachSafetyPin()
        {
            DetachSafetyPinClientEffects();
        }
        
        private void TryDetachLever()
        {
            //Wait until pin is detached
            if (safetyPin) return;
            //Dont try detaching lever if it already has been done
            if (!lever) return;
            //Don't try detaching if already primed
            if (_primedTime > 0) return;
            
            if (GetComponent<XRGrabInteractable>().isSelected) //Wait until grenade is dropped
                //Or until release threshold is met
                if(((XRDirectInteractor)GetComponent<XRGrabInteractable>().interactorsSelecting[0]).selectInput.inputActionReferenceValue.action.ReadValue<float>() > gripLeverReleaseThreshold)
                    return;

            DetachLeverClientEffects();
            
            //Detach if all conditions are met
            Prime();
        }

        private void Prime()
        {
            //Schedule explosion
            _primedTime = Time.time;
        }
        
        private void Explode()
        {
            //Spawn explosion at our position
            GameObject explosionObj = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Explosion explosion = explosionObj.GetComponent<Explosion>();
            explosion.explosionOwner = lastHolder;
            
            //Destroy grenade
            Destroy(gameObject);
        }
        
        #region Client Effects
        private void DetachSafetyPinClientEffects()
        {
            //Play pin pull sound
            if(pinPullSound) pinPullSound.Play();

            //Safety pin is no longer part of grenade
            safetyPin.transform.parent = null;

            //Enable physics on the pin, and reset velocity
            safetyPin.isKinematic = false;
            safetyPin.linearVelocity = Vector3.zero;

            //Schedule safety pin object destruction
            Destroy(safetyPin.gameObject, 10);

            safetyPin = null;
        }
        
        private void DetachLeverClientEffects()
        {
            //Play pin pull sound
            if(primerSound) primerSound.PlayDelayedBySpeedOfSound();
            if(primeParticle) primeParticle.Play();

            //Lever is no longer part of grenade
            lever.transform.parent = null;

            //Enable physics on the lever, and use same velocity as grenade
            lever.isKinematic = false;
            lever.linearVelocity = GetComponent<Rigidbody>().linearVelocity * .3f;
            //TODO random angular vel

            //Schedule lever object destruction
            Destroy(lever.gameObject, 5);

            lever = null;
        }
        #endregion

        public void TakeDamage(float damage)
        {
            Prime();
            fuseDuration = .1f;
        }
    }
}