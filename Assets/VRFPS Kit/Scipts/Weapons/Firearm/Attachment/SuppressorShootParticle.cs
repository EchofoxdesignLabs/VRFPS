using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SuppressorShootParticle : MonoBehaviour
    {//TODO this script wouldn't have to be networked if Firearm networked the shoot event
        private Firearm _firearm;
        private XRSocketInteractor _suppressorSocket;

        private void OnFirearmShoot(Cartridge cartridge) => GetComponent<ParticleSystem>().Play();

        //Subscribe to shoot events when suppressor is attached and vice versa
        private void SuppressorAttached(Firearm firearm) => firearm.ShootEvent += OnFirearmShoot;
        private void SuppressorDetached(Firearm firearm) => firearm.ShootEvent -= OnFirearmShoot;

        private void Awake()
        {
            XRGrabInteractable grabbable = GetComponentInParent<XRGrabInteractable>();
            if(!grabbable) Debug.LogError("SuppressorShootParticle couldn't find XRGrabInteractable component in parent");
            
            //Call SuppressorAttached() if just selected by a suppressor socket
            grabbable.selectEntered.AddListener(args =>
            {
                if(args.interactorObject.transform.GetComponent<SuppressorSocket>())
                    SuppressorAttached(args.interactorObject.transform.GetComponentInParent<Firearm>());
            });
            //Call SuppressorDetached() if just selected by a suppressor socket
            grabbable.selectExited.AddListener(args =>
            {
                if(args.interactorObject.transform.GetComponent<SuppressorSocket>())
                    SuppressorDetached(args.interactorObject.transform.GetComponentInParent<Firearm>());
            });
        }
    }
}
