using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    [RequireComponent(typeof(XRSocketInteractor))]
    public class MagazineResetOnDeath : MonoBehaviour
    {
        public void Reset()
        {
            Magazine magazine = GetComponent<XRSocketInteractor>().firstInteractableSelected?.transform.GetComponent<Magazine>();
            if(magazine == null) return;
            
            magazine.FillPreset();
        }
    }
}
