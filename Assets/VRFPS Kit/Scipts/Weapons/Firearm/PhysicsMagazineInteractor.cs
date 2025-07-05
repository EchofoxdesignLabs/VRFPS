using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace VRFPSKit
{
    /// <summary>
    /// Extends SocketInteractor, handling Magazines that attach
    /// </summary>
    [RequireComponent(typeof(ConfigurableJoint), typeof(Rigidbody))]
    public class PhysicsMagazineInteractor : SimpleMagazineInteractor
    {
        const float MinimmuHoverTime = .02f;
        [Header("Adjust 'Limit' values of either 'Linear Limit', \n"  +
                "'High Angular X Limit' & 'Low Angular X' on \n" +
                "Configurable Joint to adjust the range of motion. \n" +
                "\n" +
                "Limits are configured in distance from 'Insert Point'\n" +
                "transform. configure Axis & Secondary axis so X pos/rot is main.")]
        [Space]
        [Space]
        [Space]
        [Header("Magazine Insertion Settings")]
        [Tooltip("'Insert Point' is where the magazine starts to be inserted to the weapon")]
        public Transform insertPoint;
        [Header("Configure 'Attach point' as for where the magazine to be attached to the weapon")]
        [Tooltip("Is the hover interaction automatically ended when magazine is inserted?")]
        public bool endHoverWhenInserted = false;
        [Tooltip("When is magazine considered inserted?")]
        public float insert01Threshold = .95f;
        [Space]
        [Tooltip("What value between 0 - 1 is considered too much for interaction to begin? Useful to force users completing the whole 'Insertion Motion'")]
        public float insertionBeginMaxThreshold = .5f;
        [Space] 
        [Space] 
        [Space] 
        [Space] 

        private float _hoverStartTime;
        private float _hoverEndTime;
        
        private float _forceHoverTime; //Will force hover state again until time is reached
        private XRGrabInteractable _forceHoverInteractable;

        private ConfigurableJoint _joint;
        
        #region Interaction Events
        /// <summary>
        /// Called when a new magazine is attached
        /// </summary>
        /// <param name="args"></param>
        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            
            //If magazine is already being selected, don't do anything
            if(IsSelecting(args.interactableObject)) return;
            
            _hoverStartTime = Time.time;
            Rigidbody magRigidbody = args.interactableObject.transform.GetComponent<Rigidbody>();
            
            //If magazine is already attached, don't do anything (maybe mag wasn't allowed to be detached yet)
            if (_joint.connectedBody == magRigidbody) return;
                
            //Disable all colliders on magazine
            MagazineToggleCollisions(false, args.interactableObject.transform);
            
            float progress = JointProgress(args.interactableObject);
            
            //Prepare achor point
            args.interactableObject.transform.position = insertPoint.position;
            args.interactableObject.transform.rotation = insertPoint.rotation;
            
            //Connect magazine with joint
            _joint.connectedBody = magRigidbody;
            
            //Move magazine to insert point
            Transform jointStartTransform = progress > .5f ? attachTransform : insertPoint;
            args.interactableObject.transform.position = jointStartTransform.position;
            args.interactableObject.transform.rotation = jointStartTransform.rotation;
        }

        /// <summary>
        /// Called when magazine is detached
        /// </summary>
        /// <param name="args"></param>
        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);

            if (JointProgress(args.interactableObject) > 0 && JointProgress(args.interactableObject) <= 1)
            {
                ForceHover(args.interactableObject);
                return;
            }
            
            _hoverEndTime = Time.time;
            
            _joint.connectedBody = null;
            
            //Only reenable collisions if magazine isnt being selected now
            if(interactablesSelected.Count == 0 || interactablesSelected[0] != args.interactableObject)
                MagazineToggleCollisions(true, args.interactableObject.transform);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
                
            _joint.connectedBody = null;
            MagazineToggleCollisions(false, args.interactableObject.transform);
            _forceHoverTime = -1;
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            
            //await Task.Delay(10);//Wait so selection has ended
            //Disable all colliders on magazine
            MagazineToggleCollisions(false, args.interactableObject.transform);
            
            //Prepare joint anchor (only so joint is relative to insert point)
            args.interactableObject.transform.position = insertPoint.position;
            args.interactableObject.transform.rotation = insertPoint.rotation;
            
            //Connect magazine with joint
            _joint.connectedBody = args.interactableObject.transform.GetComponent<Rigidbody>();
            
            //Move magazine to attach point
            args.interactableObject.transform.position = attachTransform.position;
            args.interactableObject.transform.rotation = attachTransform.rotation;
            
            ForceHover(args.interactableObject);
        }
        #endregion

        private void Update()
        {
            if (_forceHoverTime > Time.time && !hasHover && !hasSelection)
            {
                interactionManager.HoverEnter(this, (IXRHoverInteractable) _forceHoverInteractable);
            }
            
            //Make trigger huge if a magazine is hovered, so it wont be unhovered because magazine is outside trigger
            TryEndInteraction();
        }

        private void TryEndInteraction()
        {
            if (interactablesHovered.Count == 0) return;
            
            if (endHoverWhenInserted && JointProgress() >= insert01Threshold && Time.time - _hoverStartTime > 0.2f) {
                interactionManager.SelectEnter(this, (IXRSelectInteractable)interactablesHovered[0]);
                return;
            }
            
            //If magazine is close enough to insert point, detach it
            if (JointProgress() <= 0 && Time.time - _hoverStartTime > MinimmuHoverTime)
            {
                interactionManager.HoverExit(this, interactablesHovered[0]);
            }
        }

        #region Helper Methods
        public float JointProgress(IXRHoverInteractable interactable = null)
        {
            if (interactable == null)
            {
                if(interactablesHovered.Count > 0) 
                    interactable = interactablesHovered[0];
                else
                    return -1; //If no interactable is provided, and can't use a hovered one, return -1 for invalid parameters
                
            }
            if(IsSelecting(interactable)) return 1; //If selecting, return 1
            
            Transform magazine = interactable.transform;
            float totalProgress01 = 0;
            if (UsingRotationAxis())
            {
                totalProgress01 = InverseLerp(insertPoint.rotation, attachTransform.rotation, magazine.rotation);
            } else {
                totalProgress01 = InverseLerp(insertPoint.position, attachTransform.position, magazine.transform.position);
            }
            
            return totalProgress01;
        }
        
        private static float InverseLerp(Quaternion a, Quaternion b, Quaternion value)
        {
            float totalProgress01 = 0;
            
            //All rotations are relative to insert point
            Vector3 bRelativeToA = (Quaternion.Inverse(a) * b).eulerAngles;
            Vector3 valueRelativeToA = (Quaternion.Inverse(a) * value).eulerAngles;
            
            // Apply Mathf.DeltaAngle to each component
            Vector3 aDeltaAngles = new Vector3(
                Mathf.DeltaAngle(0, bRelativeToA.x),
                Mathf.DeltaAngle(0, bRelativeToA.y),
                Mathf.DeltaAngle(0, bRelativeToA.z));
            Vector3 valueaDeltaAngles = new Vector3(
                Mathf.DeltaAngle(0, valueRelativeToA.x),
                Mathf.DeltaAngle(0, valueRelativeToA.y),
                Mathf.DeltaAngle(0, valueRelativeToA.z));
            
            //TODO lerp in local space axis
            int axisCount = 0;
            if (aDeltaAngles.x is < -5 or > 5)
            {
                //TODO we need to account for 0-360 wrap
                float xRot01 = Mathf.InverseLerp(0, aDeltaAngles.x, valueaDeltaAngles.x);
                totalProgress01 += xRot01;
                axisCount++;
            }
            if (aDeltaAngles.y is < -5 or > 5)
            {
                float yRot01 = Mathf.InverseLerp(0, aDeltaAngles.y, valueaDeltaAngles.y);
                totalProgress01 += yRot01;
                axisCount++;
            }
            if (aDeltaAngles.z is < -5 or > 5)
            {
                float zRot01 = Mathf.InverseLerp(0, aDeltaAngles.z, valueaDeltaAngles.z);
                totalProgress01 += zRot01;
                axisCount++;
            }
            
            if(axisCount > 0) totalProgress01 /= axisCount;

            return totalProgress01;
        }
        
        private static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            // Calculate the direction vector and its length
            Vector3 ab = b - a;
            float abMagnitude = ab.magnitude;

            // If the points are coincident, return 0 to avoid division by zero
            if (abMagnitude == 0f) return 0f;

            // Project the vector from `a` to `value` onto the direction vector
            Vector3 av = value - a;
            float projection = Vector3.Dot(av, ab.normalized);

            // Return the normalized value (clamped between 0 and 1 for safety)
            return projection / abMagnitude;
        }
        
        /// <summary>
        /// Toggles collisions between magazine and weapon
        /// </summary>
        private void MagazineToggleCollisions(bool collide, Transform magazine)
        {
            foreach(Collider magCol in magazine.GetComponentsInChildren<Collider>())
                foreach (Collider weaponCol in GetComponentInParent<Firearm>().GetComponentsInChildren<Collider>())
                {
                    //Dont ignore trigger on interactor
                    if(weaponCol.gameObject == gameObject) continue;
                    
                    Physics.IgnoreCollision(magCol, weaponCol, !collide);
                }
        }
        
        private bool UsingRotationAxis() => 
            _joint.angularXMotion != ConfigurableJointMotion.Locked || 
            _joint.angularYMotion != ConfigurableJointMotion.Locked || 
            _joint.angularZMotion != ConfigurableJointMotion.Locked;
        
        private void ForceHover(IXRInteractable interactable)
        {
            _forceHoverTime = Time.time + MinimmuHoverTime;
            _forceHoverInteractable = (XRGrabInteractable)interactable;
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();

            _joint = GetComponent<ConfigurableJoint>();
            selectFilters.Add(new XRSelectFilterDelegate(ReattachCooldownFilter));
            hoverFilters.Add(new XRHoverFilterDelegate(ReattachCooldownFilter));
            hoverFilters.Add(new XRHoverFilterDelegate(OnlyHoverOneFilter));
            hoverFilters.Add(new XRHoverFilterDelegate(MagazineProgressHoverFilter));

            var reattachFilter = new XRReattachCooldownFilter(reattachCooldownDelay: .4f, cooldownAfterHovered: true); //Best if both hover and select use same filter
            selectFilters.Add(reattachFilter);
            selectFilters.Add(reattachFilter);
        }
        
        private bool ReattachCooldownFilter(IXRInteractor interactor, IXRInteractable interactable)
        {
            if (IsSelecting(interactable)) return true; //If already selected, keep it selected
            if (IsHovering(interactable)) return true; //If interactable already hovered, keep it hovered
            
            return (Time.time - _hoverEndTime > 0.3f);
        }
        
        private bool OnlyHoverOneFilter(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            if(IsHovering((IXRSelectInteractable)interactable)) return true; //If interactable already hovered, keep it hovered
            if(IsSelecting((IXRSelectInteractable)interactable)) return true; //Allow selected interactable to be hovered
            
            if(interactablesHovered.Count == 0 && interactablesSelected.Count == 0) return true; //If socket is free, we can start a new hover

            return false;
        }
        
        private bool MagazineProgressHoverFilter(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            if(IsHovering((IXRSelectInteractable)interactable)) return true; //If interactable already hovered, keep it hovered
            if(IsSelecting((IXRSelectInteractable)interactable)) return true; //Allow selected interactable to be hovered
            
            //Check if magazine progress is below threshold & not below 0
            if(JointProgress(interactable) > insertionBeginMaxThreshold) return false; 
            if(JointProgress(interactable) <= 0) return false; 
            
            return true;
        }
    }
}