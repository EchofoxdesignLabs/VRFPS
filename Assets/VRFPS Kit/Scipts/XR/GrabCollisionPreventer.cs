using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    [RequireComponent(typeof(XRBaseInteractor))]
    public class GrabCollisionPreventer : MonoBehaviour
    {
        private const int CollisionDelayMS = 200;
        
        private async void OnSelectEntered(SelectEnterEventArgs args)
        {
            //Only care about grabbing rigidbodies
            Rigidbody _rb = args.interactableObject.transform.GetComponent<Rigidbody>();
            if (_rb == null) return;

            LayerMask previousMask = _rb.excludeLayers;
            _rb.excludeLayers = ~0; //Ignore all collisions

            await Task.Delay(CollisionDelayMS);
            
            _rb.excludeLayers = previousMask;
        }
        
        private void Awake()
        {
            GetComponent<XRBaseInteractor>().selectEntered.AddListener(OnSelectEntered);
        }
    }
}
