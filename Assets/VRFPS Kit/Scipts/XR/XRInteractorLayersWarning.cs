using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    public class XRInteractorLayersWarning : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            foreach(var interactor in GetComponentsInChildren<XRBaseInteractor>())
                interactor.selectEntered.AddListener(Grab);
        }

        private void Grab(SelectEnterEventArgs args)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            
            foreach(var collider in args.interactableObject.transform.GetComponentsInChildren<Collider>())
                if(!collider.isTrigger)
                    if(!Physics.GetIgnoreLayerCollision(collider.gameObject.layer, playerLayer))
                        Debug.LogWarning($"Interactable object '{args.interactableObject.transform.gameObject.name}' has a collider '{collider.gameObject.name}' on a layer that will collide with player layer. This will cause strange collision issues!");
        }
    }
}
