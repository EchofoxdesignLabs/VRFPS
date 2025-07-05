using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    [RequireComponent(typeof(XRBaseInteractor))]
    public class XRPhysicsInteractor : MonoBehaviour
    {
        private Rigidbody _interactableRigidbody;
        private FixedJoint _joint;
        
        private Rigidbody _interactorRigidbody;
        
        private async void CreateFixedJoint(IXRInteractable interactable)
        {
            await Task.Delay(100);
            
            if (!(interactable is XRGrabInteractable grabInteractable)) return;
            if (!(interactable.transform.GetComponent<Rigidbody>() is Rigidbody rb)) return;
            if (!GetComponent<XRBaseInteractor>().IsSelecting((IXRSelectInteractable)interactable)) return;// No need to continue if interaction has ended
            
            //Resetting any previous joints
            if(_joint) Destroy(_joint);
            
            _interactableRigidbody = rb;
            _joint = _interactableRigidbody.gameObject.AddComponent<FixedJoint>();
            
            _joint.connectedBody = _interactorRigidbody;
            _joint.connectedMassScale = .001f; //Fix for Recoil breaking when connected to XRPhysicsInteractor
        }
        
        private void EndFixedJoint(IXRInteractable interactable)
        {
            if(_joint)
                Destroy(_joint);
                
            _interactableRigidbody = null;
        }

        private void Update()
        {
            //Ensure magazine is not kinematic while selected, since it makes whole firearm kinematic in combination with XRPhysicsInteractor
            if (_interactableRigidbody != null)
                _interactableRigidbody.isKinematic = false;
        }

        // Start is called before the first frame update
        private void Awake()
        {
            foreach (var interactor in GetComponents<XRBaseInteractor>())
            {
                interactor.selectEntered.AddListener(args => CreateFixedJoint(args.interactableObject));
                interactor.selectExited.AddListener(args => EndFixedJoint(args.interactableObject));
            }
            
            //We always specifically want to connect interactables to the parent XRGrabInteractable's rigidbody
            _interactorRigidbody = GetComponentInParent<XRGrabInteractable>()?.GetComponent<Rigidbody>();
            if(_interactorRigidbody == null)
                _interactorRigidbody = GetComponentInParent<Rigidbody>();//support non-XRGrabInteractable interactables
        }
    }
}
