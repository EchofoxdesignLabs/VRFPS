using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    public class FirearmDustCover : MonoBehaviour
    {
        private  const float Action01OpenThreshold = 0.15f;
        
        public Vector3 closedRotation;
        public bool dustCoverOpen;

        public Action DustCoverOpenEvent;
        
        private Quaternion _openRotation;
        private FirearmCyclingAction _action;
        
        // Update is called once per frame
        void Update()
        {
            if(!dustCoverOpen && _action.actionPosition01 > Action01OpenThreshold)
            {
                dustCoverOpen = true;
                DustCoverOpenEvent?.Invoke();
            }
            
            // Interpolate between the rotations
            transform.localRotation = dustCoverOpen ? _openRotation : Quaternion.Euler(closedRotation);
        }
        
        void Awake()
        {
            _action = GetComponentInParent<FirearmCyclingAction>();
            _openRotation = transform.localRotation;
        }
    }
}
