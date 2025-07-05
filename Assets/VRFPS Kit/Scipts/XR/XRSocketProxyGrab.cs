using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    /// <summary>
    /// When this proxy object is grabbed, interaction will be passed to the item of the target socket.
    /// </summary>
    public class XRSocketProxyGrab : XRSimpleInteractable
    {
        public XRSocketInteractor targetSocket;
        
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            
            //Cancel selection of socket
            interactionManager.CancelInteractableSelection((IXRSelectInteractable)this);

            IXRSelectInteractable proxyGrabbable = GetProxyGrabbable();
            if (proxyGrabbable == null) return;
            
            interactionManager.CancelInteractableSelection(proxyGrabbable);
            
            //Create new proxy selection between hand & socket item
            interactionManager.SelectEnter(args.interactorObject, proxyGrabbable);
        }

        private IXRSelectInteractable GetProxyGrabbable() => targetSocket.firstInteractableSelected;
        
        protected override void Awake()
        {
            base.Awake();
            
            if(targetSocket == null)
                Debug.LogError("XRSocketProxyGrab: Target socket is not assigned. Please assign a target socket in the inspector.", this);
        }
    }
}
