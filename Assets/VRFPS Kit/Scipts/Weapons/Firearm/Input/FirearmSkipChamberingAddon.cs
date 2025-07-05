using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    /// <summary>
    /// Add to the gameobject containing the "FirearmCyclingAction" in order to
    /// automatically chamber a new round when possible (skip the need to chamber new rounds manually)
    /// </summary>
    [RequireComponent(typeof(Firearm))]
    public class FirearmSkipChamberingAddon : MonoBehaviour
    {
        // Update is called once per frame
        private void Update()
        {
            //Chamber new round if possible
            GetComponent<FirearmCyclingAction>().TryLoadFromMagazine();
            GetComponent<FirearmCyclingAction>().FinishLoadChamber();
        }
    }
}