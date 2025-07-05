using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    public class XRReattachCooldownFilter : IXRSelectFilter, IXRHoverFilter
    {
        private static readonly Dictionary<IXRInteractor, float> LastInteractorExitTime = new();
        
        private float _reattachCooldownDelay;
        private bool _cooldownAfterHovered;

        public bool canProcess => true;
        public XRReattachCooldownFilter(float reattachCooldownDelay = .5f, bool cooldownAfterHovered = false)
        {
            _reattachCooldownDelay = reattachCooldownDelay;
            _cooldownAfterHovered = cooldownAfterHovered;
        }
        
        /// <summary>
        /// Filter processing for selection interactions
        /// </summary>
        /// <param name="interactor"></param>
        /// <param name="interactable"></param>
        /// <returns></returns>
        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            //Return if already attached (we only want cooldown to be applied when interaction is started)
            if (interactor.IsSelecting(interactable)) return true;
            
            //Listen to select exit event, and store exit time in LastSocketExitTime
            interactor.selectExited.AddListener(StoreCooldownExitTime);
            
            LastInteractorExitTime.TryGetValue(interactor, out float exitTime);
            return (Time.time - exitTime > _reattachCooldownDelay);
        }
        
        /// <summary>
        /// Filter processing for hover interactions
        /// </summary>
        /// <param name="interactor"></param>
        /// <param name="interactable"></param>
        /// <returns></returns>
        public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            //Will only be processed if added as a hover filter, so wont block hovers otherwise
            
            //Return if already attached (we only want cooldown to be applied when interaction is started)
            if (interactor.IsHovering(interactable)) return true;
            
            if(_cooldownAfterHovered)
                interactor.hoverExited.AddListener(StoreCooldownExitTime);
            
            LastInteractorExitTime.TryGetValue(interactor, out float exitTime);
            return (Time.time - exitTime > _reattachCooldownDelay);
        }


        private void StoreCooldownExitTime(SelectExitEventArgs args)
        {
            XRSocketInteractor socket = (XRSocketInteractor)args.interactorObject;
            
            LastInteractorExitTime[socket] = Time.time; 
            
            //Only need to call this once
            socket.selectExited.RemoveListener(StoreCooldownExitTime);
        }


        private void StoreCooldownExitTime(HoverExitEventArgs args)
        {
            XRSocketInteractor socket = (XRSocketInteractor)args.interactorObject;
            
            LastInteractorExitTime[socket] = Time.time; 
            
            //Only need to call this once
            socket.hoverExited.RemoveListener(StoreCooldownExitTime);
        }
    }
}
