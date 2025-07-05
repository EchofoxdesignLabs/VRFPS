using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRFPSKit.Input;
using VRFPSKit;

#if HAPTIC_PATTERNS
using HapticPatterns;
#endif

namespace VRFPSKit
{
    /// <summary>
    /// Handles all Firearm Trigger Behaviour, Firemodes are also implemented here
    /// </summary>
    [RequireComponent(typeof(Firearm), typeof(XRGrabInteractable))]
    public class FirearmTrigger : MonoBehaviour
    {
        #if HAPTIC_PATTERNS
        public HapticPattern triggerPressPattern;
        public HapticPattern triggerResetPattern;
        #else
        [Header("VR Haptic Patterns Isn't Installed")]
        #endif
        
        [Space]
        public XRHeldInputAction triggerPressed01Input;
        
        [Space]
        [Range(0, 1)]public float triggerPressThreshold = .5f;
        [Range(0, 1)]public float triggerResetThreshold = .5f;
        
        #region Events
        //Events will only be called on authority client
        public event Action PressEvent;
        public event Action ReleaseEvent;
        public event Action ResetEvent;
        #endregion

        public float triggerProgress01;
        private bool _triggerWaitingForReset;
        private bool _pressedLastFrame;
        
        private XRGrabInteractable _grabbable;
        private Firearm _firearm;
        
        // Update is called once per frame
        void Update()
        {
            if (!_grabbable.isSelected) return;
            
            InputAction triggerInput = triggerPressed01Input.GetActionForPrimaryHand(_grabbable);
            if (triggerInput == null) return;
            
            //Read trigger input
            triggerProgress01 = triggerInput.ReadValue<float>();

            TriggerHaptics();
            TryFire();
            
            //Trigger events when pressed state changes
            if(IsPressed() && !_pressedLastFrame) PressTrigger();
            if(!IsPressed() && _pressedLastFrame) ReleaseTrigger();
            
            //Reset trigger if it is released enough
            if (_triggerWaitingForReset && triggerProgress01 < triggerResetThreshold && _firearm.inBattery)
            {
                ResetTrigger();
                _triggerWaitingForReset = false;
            }
        }

        private void TryFire()
        {
            //Trigger press strength must exceed threshold
            if (!IsPressed()) return;
            //Can't fire if fire mode is Safe
            if (_firearm.currentFireMode == FireMode.Safe) return;
            //Cant fire if trigger hasn't reset yet
            if (_triggerWaitingForReset) return;
            //Cant fire if hammer isnt cocked
            if(!_firearm.isHammerCocked) return;
            
            _firearm.TryShoot();
            
            //If fire mode is single fire, wait for trigger reset before firing again
            if (_firearm.currentFireMode == FireMode.Single_Fire)
                _triggerWaitingForReset = true;
        }

        private bool IsPressed() => triggerProgress01 > triggerPressThreshold;

        private void TriggerHaptics()
        {
            #if HAPTIC_PATTERNS
            if(triggerResetPattern == null || triggerPressPattern == null) return;
            
            if(_triggerWaitingForReset)
                triggerResetPattern.PlayGradually(_grabbable, triggerProgress01);
            else
                triggerPressPattern.PlayGradually(_grabbable, triggerProgress01);
            #endif
        }

        private void LateUpdate()
        {
            _pressedLastFrame = IsPressed();
        }
        
        public void PressTrigger() => PressEvent?.Invoke();
        public void ReleaseTrigger() => ReleaseEvent?.Invoke();
        public void ResetTrigger() => ResetEvent?.Invoke();

        // Start is called before the first frame update
        void Awake()
        {
            _grabbable = GetComponent<XRGrabInteractable>();
            _firearm = GetComponent<Firearm>();
        }
    }
}