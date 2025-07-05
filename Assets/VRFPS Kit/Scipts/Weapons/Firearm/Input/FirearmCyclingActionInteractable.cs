using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

#if HAPTIC_PATTERNS
using HapticPatterns;
#endif

namespace VRFPSKit
{
    /// <summary>
    /// Creates a proxy interactable for FirearmCyclingAction, allowing you to pull back the action.
    /// Also handles behaviour like chamber loading and ejecting.
    /// </summary>
    [RequireComponent(typeof(XRBaseInteractable))]
    public class FirearmCyclingActionInteractable : MonoBehaviour, IXRSimulatedWeight
    {
        #if HAPTIC_PATTERNS
        public HapticPattern actionPullPattern;
        #else
        [Header("VR Haptic Patterns Isn't Installed")]
        #endif
        
        public Vector3 handMovementDirectionSensitivity;
        public float springWeight = 6;
        
        
        [HideInInspector] public XRBaseInteractable actionInteractable;
        [HideInInspector] public FirearmCyclingAction _action;

        private Vector3 _handRelativePositionLastFrame;
        private float _actionPosition01LastFrame;
        private float _actionPosition01TwoFrames;

        // Update is called once per frame
        private void LateUpdate()
        {
            //Only run when holding action, can't use actionInteractable.isSelected since FirearmChargingGripActionInteractable uses the second hand
            if(GetHand() == null) return;
            
            //Unlock action if it has been grabbed
            _action.TryUnlockAction();
            
            _action.actionPosition01 = ActionMovementFromHand();
            
            #if HAPTIC_PATTERNS
            if(actionPullPattern != null)
                actionPullPattern.PlayGradually(actionInteractable, _action.actionPosition01);
            #endif
            
            _handRelativePositionLastFrame = GetHandRelativePosition();
            _actionPosition01TwoFrames = _actionPosition01LastFrame;
            _actionPosition01LastFrame = _action.actionPosition01;
        }

        protected virtual float ActionMovementFromHand()
        {
            //return if _handPositionLastFrame isn't initialized
            if (_handRelativePositionLastFrame == Vector3.zero) return _action.actionPosition01;
            
            //Calculate hand movement change since last frame
            Vector3 deltaHandMovement = GetHandRelativePosition() - _handRelativePositionLastFrame;
            //Translate hand movement to bolt position change
            Vector3 trackedDirectionDeltaMovement = Vector3.Scale(
                -deltaHandMovement,//TODO this should not be negated with - (requires changing weapon values though)
                handMovementDirectionSensitivity);
            
            float movementMagnitude = trackedDirectionDeltaMovement.x + trackedDirectionDeltaMovement.y +
                                      trackedDirectionDeltaMovement.z;
            
            //Apply bolt position change
            return Mathf.Clamp01(_action.actionPosition01 + movementMagnitude);
        }

        public void ForceDetachSelectors()
        {
            IXRSelectInteractor hand = GetHand();
            if(hand == null) return;
            
            actionInteractable.interactionManager.CancelInteractorSelection(hand);
        }

        private Vector3 GetHandRelativePosition()
        {
            IXRSelectInteractor hand = GetHand();
            if(hand == null) return Vector3.zero;
            
            return transform.InverseTransformPoint(hand.transform.position);
        }

        protected virtual IXRSelectInteractor GetHand()
        {
            if (!actionInteractable.isSelected) return null;
            
            return actionInteractable.interactorsSelecting[0];
        }

        private float ActionMovementSpeed() => (_actionPosition01TwoFrames - _actionPosition01LastFrame) / Time.deltaTime;

        public float GetSimulatedMass() => (ActionMovementSpeed() < 0 && actionInteractable.isSelected) ? springWeight : 0;

        // Start is called before the first frame update
        void Awake()
        {
            //Fetch components necessary
            _action = GetComponentInParent<FirearmCyclingAction>();
            actionInteractable = GetComponent<XRBaseInteractable>();

            if (_action == null)
                Debug.LogError("Action interfacer cycler was not found in parent!");

            if (GetComponent<Collider>() && !GetComponent<Collider>().isTrigger)
                Debug.LogWarning("FirearmCyclingActionInteractable collider must be a trigger!");
        }
    }
}