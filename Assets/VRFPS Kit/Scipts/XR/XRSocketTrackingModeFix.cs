using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

//Sockets override movement mode to instantaneous but doesnt reset until item is completely dropped. This is a fix for that
[RequireComponent(typeof(XRBaseInteractor))]
public class XRSocketTrackingModeFix : MonoBehaviour
{
    private XRBaseInteractable.MovementType _previousMovementType;
    
    void Awake()
    {
        XRBaseInteractor interactor = GetComponent<XRBaseInteractor>();
        interactor.selectEntered.AddListener((args) => 
        {
            if(args.interactableObject is not XRGrabInteractable grabbable) return;
            _previousMovementType = grabbable.movementType;
        });
        interactor.selectExited.AddListener((args) =>
        {
            if(args.interactableObject is not XRGrabInteractable grabbable) return;
            grabbable.movementType = _previousMovementType;
        });

    }
}
