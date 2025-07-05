using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    /// <summary>
    /// Prevents Interactables from lagging behind one frame per every nested socket.
    /// Essentially emulates making the interactable transform a child.
    /// </summary>
    [RequireComponent(typeof(XRSocketInteractor))]
    public class XRSocketDelayedTrackingFix : MonoBehaviour
    {
        //TODO could in theory be applicable for other types of interactors
        private XRSocketInteractor _socket;

        private void OnBeforeRender() => UpdateInteractor();

        private void UpdateInteractor()
        {
            /*TODO doesn't move before render (not working at all)
             
            //Try to Update any parent XRSocketTrackingDelayFixes first
            foreach (var selectedInteractable in _socket.interactablesSelected)
                foreach(var delayFixesInChildren in selectedInteractable.transform.GetComponentsInChildren<XRSocketDelayedTrackingFix>())
                    delayFixesInChildren.UpdateInteractor();
            
            //Lastly update our own interactor
            foreach (var selectedInteractable in _socket.interactablesSelected)
                selectedInteractable.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender);
                */
        }
        
        void Awake()
        {
            _socket = GetComponent<XRSocketInteractor>();
            Application.onBeforeRender += OnBeforeRender;
        }
    }
}
