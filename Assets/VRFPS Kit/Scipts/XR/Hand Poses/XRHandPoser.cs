using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit.HandPoses
{
    public class XRHandPoser : BaseHand
    {
        public XRDirectInteractor[] interactors;
        
        private XRHandPoseContainer _poseContainer;
        
        private void LateUpdate()
        {
            if(!_poseContainer) return;
            
            ApplyPose(_poseContainer.pose); //FixedUpdate()?
        }

        public void UpdatePose()
        {
            var interactor = GetActiveInteractor();
            var interactable = GetActiveInteractable();
            
            if (interactor == null || interactable == null) 
            {
                ApplyPose(defaultPose);
                return;
            }
            
            //subscribing to selectExited event on interactable is a good idea
            if (GetComponent<Animator>() is Animator animator) animator.enabled = false;
            
            //Get index of current interactor in interactable selecting list
            int interactorIndex = interactable.interactorsSelecting.FindIndex(selectingInteractor => selectingInteractor == interactor);
            
            //TODO this is a temp fix for interactorIndex returning -1, need to find a better solution
            if(interactorIndex == -1) interactorIndex = 0; //Default to 0 if index not found
             
            //Find a hand pose with a matching interactor index
            foreach (var pose in interactable.transform.GetComponents<XRHandPoseContainer>())
                if(pose.interactorIndex == interactorIndex)
                    _poseContainer = pose;

            if (!_poseContainer)
            {
                Debug.LogWarning($"No hand pose container found for interactable {interactable.transform.gameObject.name} with interactor index: {interactorIndex}");
                return;
            }
            
            attachTransform = _poseContainer.GetAttachPoint();
            ApplyPose(_poseContainer.pose);
        }
        
        private XRDirectInteractor GetActiveInteractor()
        {
            foreach (XRDirectInteractor interactor in interactors)
                if(interactor.hasSelection)
                    return interactor;
            return null;
        }

        private IXRSelectInteractable GetActiveInteractable()
        {
            XRDirectInteractor interactor = GetActiveInteractor();
            if(interactor == null) return null;
            return interactor.firstInteractableSelected;
        }

        private async void OnSelectEntered(SelectEnterEventArgs args)
        {
            GetActiveInteractable();
            //We need to cache interactable if we use Task.Delay(), will start referencing other interactables for some reason otherwise
            
            //Wait one frame for potential interactor transfer to complete before getting interactor index
            await Task.Delay(50);
            
            UpdatePose();
        }
        
        private void OnSelectExited(SelectExitEventArgs args)
        {
            attachTransform = ((XRDirectInteractor)args.interactorObject).attachTransform;
            _poseContainer = null;
            
            if (GetComponent<Animator>() is Animator animator) animator.enabled = true;

            CharacterController characterController = GetComponentInParent<CharacterController>();
            if(characterController == null){ Debug.LogError("XRHandPoser couldn't find CharacterController in parent, which is needed to find other hand posers on player"); return;}
            foreach (var poser in characterController.GetComponentsInChildren<XRHandPoser>())
                poser.UpdatePose();
            
            UpdatePose();
        }
        
        // Update is called once per frame
        protected override void Awake()
        {
            base.Awake();

            attachTransform = interactors[0].attachTransform;
            
            ApplyPose(defaultPose);
            
            foreach (var interactor in interactors)
            {
                interactor.selectEntered.AddListener(OnSelectEntered);
                interactor.selectExited.AddListener(OnSelectExited);
            }
        }
    }
}
