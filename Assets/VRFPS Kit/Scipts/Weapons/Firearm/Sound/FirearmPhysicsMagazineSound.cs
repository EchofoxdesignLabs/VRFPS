using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class FirearmPhysicsMagazineSound : MonoBehaviour
    {
        public AudioClip insertingBegin;
        public AudioClip insertingEnd;
        public AudioClip insertedBegin;
        public AudioClip insertedEnd;
        
        private bool _wasInsertingLastFrame;
        private bool _wasInsertedLastFrame;
        
        private AudioSource _audio;

        private PhysicsMagazineInteractor _interactor;

        private void Update()
        {
            float insertProgress = _interactor.JointProgress();
            bool isInserting = _interactor.hasHover && insertProgress < _interactor.insert01Threshold;
            bool isInserted = _interactor.hasSelection || insertProgress >= _interactor.insert01Threshold;
            
            if(isInserting && !_wasInsertingLastFrame)
                if(insertingBegin) //Null clip check
                    _audio.PlayOneShot(insertingBegin);
            if(!isInserting && _wasInsertingLastFrame && !isInserted)//Dont play when inserted
                if(insertingEnd) //Null clip check
                    _audio.PlayOneShot(insertingEnd);
            
            if(isInserted && !_wasInsertedLastFrame)
                if(insertedBegin) //Null clip check
                    _audio.PlayOneShot(insertedBegin);
            if(!isInserted && _wasInsertedLastFrame)
                if(insertedEnd) //Null clip check
                    _audio.PlayOneShot(insertedEnd);
            
            _wasInsertingLastFrame = isInserting;
            _wasInsertedLastFrame = isInserted;
        }
        
        private void Awake()
        {
            _interactor = GetComponentInParent<Firearm>().GetComponentInChildren<PhysicsMagazineInteractor>();
            if(!_interactor) Debug.LogError("FirearmPhysicsMagazineSound couldn't find PhysicsMagazineInteractor component on object or children");
            _audio = GetComponent<AudioSource>();
        }
    }
}
