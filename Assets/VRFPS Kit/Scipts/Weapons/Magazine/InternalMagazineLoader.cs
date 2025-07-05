using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    /// <summary>
    /// Loads cartridge items that enter trigger in to magazine
    /// </summary>
    [RequireComponent(typeof(FirearmCyclingAction))]
    public class InternalMagazineLoader : MagazineLoader
    {
        [Space]
        public bool requireActionBack = true;
        public float actionBackLoadingThreshold01 = .9f;
        
        private FirearmCyclingAction _cyclingAction;
        
        protected override void TryLoadCartridges()
        {
            if(requireActionBack && _cyclingAction.actionPosition01 < actionBackLoadingThreshold01) return;
            
            base.TryLoadCartridges();
        }
        
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            _cyclingAction = GetComponent<FirearmCyclingAction>();
        }
    }
}