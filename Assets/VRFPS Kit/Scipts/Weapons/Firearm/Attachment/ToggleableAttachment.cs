using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRFPSKit.Input;

namespace VRFPSKit
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ToggleableAttachment : MonoBehaviour
    {
        public bool activated = true;
        [Space]
        public GameObject toggledObject;
        public XRHeldInputAction toggleInput;
        public XRHeldInputAction attachedToggleInput;
        [Space] 
        public AudioSource toggleAudioSource;
        public AudioClip activateClip;
        public AudioClip deactivateClip;
        
        private XRGrabInteractable _grabbable;
        private bool _activatedLastFrame;
        
        // Update is called once per frame
        void Update()
        {
            TryToggleInput();

            toggledObject.SetActive(activated);
            
            //If activated state just changed, play sound
            if (activated != _activatedLastFrame)
            {
                toggleAudioSource.clip = activated ? activateClip : deactivateClip;
                toggleAudioSource.Play();
            }
        }

        private void TryToggleInput()
        {
            if (!_grabbable.isSelected) return;
            
            InputAction input = toggleInput.GetActionForPrimaryHand(_grabbable);

            if (TryGetSelectingFirearm() is XRGrabInteractable firearm)
            {
                input = attachedToggleInput.GetActionForSecondaryHand(firearm);
                
                //Special case for pistol support hand, since it has a separate interactable
                if(firearm.GetComponentInChildren<PistolSupportHand>() is PistolSupportHand supportHand)
                    input = attachedToggleInput.GetActionForPrimaryHand(supportHand.GetComponent<XRBaseInteractable>());
            }
            
            //If no input action is active, return
            if (input == null) return;
            if (!input.WasPressedThisFrame()) return;

            activated = !activated;
        }
        
        private XRGrabInteractable TryGetSelectingFirearm()
        {
            const int MAX_ITERATION = 10;

            // Iterates through selections of this grabbable and it's parents to try and find a Firearm that is selecting
            // Iteration is neccessary since this object might not be directly attached to the firearm and instead through
            // nested selection like following situation:
            //
            //          (ToggleableAttachment) - (Some sort of Mount) - (Firearm)
            
            XRGrabInteractable childInteractable = _grabbable;
            for (int i = 0; i < MAX_ITERATION; i++)
            {
                if (!childInteractable.isSelected) break;
                XRBaseInteractor selectingInteractor = (XRBaseInteractor)childInteractable.interactorsSelecting[0];
                
                //Find XRGrabInteractable in parent of the interactor that is selecting "childInteractable"
                if (selectingInteractor.GetComponentInParent<XRGrabInteractable>() is not XRGrabInteractable parentGrabbable) break;
                
                //Try to find a firearm component on "parentGrabbable"
                if (parentGrabbable.GetComponent<Firearm>() != null)
                    return parentGrabbable;
                
                //Otherwise continue iterating upwards on "parentGrabbable"
                childInteractable = parentGrabbable;
            }
            
            return null;
        }
        
        void Awake()
        {
            _grabbable = GetComponent<XRGrabInteractable>();
        }

        private void LateUpdate()
        {
            _activatedLastFrame = activated;
        }
    }
}