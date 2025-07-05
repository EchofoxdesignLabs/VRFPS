using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    /// <summary>
    /// Add & Remove XRHoverOutline component when socket selects / deselects an object
    /// </summary>
    [RequireComponent(typeof(XRSocketInteractor))]
    public class XRSocketHoverOutlineSelected : MonoBehaviour
    {
        void Awake()
        {
            //Add & Remove XRHoverOutline component when socket selects / deselects an object
            GetComponent<XRSocketInteractor>().selectEntered.AddListener(args => args.interactableObject.transform.gameObject.AddComponent<XRHoverOutline>());
            GetComponent<XRSocketInteractor>().selectExited.AddListener(args => Destroy(args.interactableObject.transform.GetComponent<XRHoverOutline>()));
        }
    }
}
