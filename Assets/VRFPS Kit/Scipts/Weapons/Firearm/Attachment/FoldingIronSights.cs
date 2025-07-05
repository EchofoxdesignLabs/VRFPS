using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    public class FoldingIronSights : MonoBehaviour
    {
        public Vector3 foldedRotation;
        public XRBaseInteractor[] foldWhenAttached;
        public float transitionTime = .5f;

        private Quaternion _unfoldedRotation;
        private float _stateChangeTime;
        private bool _shouldFoldLastFrame;
        
        // Update is called once per frame
        void Update()
        {
            bool shouldFold = ShouldFold();
            
            //Track when fold state changes
            if (shouldFold != _shouldFoldLastFrame) _stateChangeTime = Time.time;
            _shouldFoldLastFrame = shouldFold;
            
            // Interpolate between the rotations
            Quaternion targetRotation = shouldFold ? Quaternion.Euler(foldedRotation) : _unfoldedRotation;
            Quaternion previousRotation = shouldFold ? _unfoldedRotation : Quaternion.Euler(foldedRotation);
            
            float rotationProgress01 = Mathf.Clamp01(Mathf.InverseLerp(_stateChangeTime, _stateChangeTime + transitionTime, Time.time));
            transform.localRotation = Quaternion.Lerp(previousRotation, targetRotation, rotationProgress01);
        }

        private bool ShouldFold() => foldWhenAttached.Any(attachSocket => attachSocket.hasSelection);
        
        void Awake()
        {
            _unfoldedRotation = transform.localRotation;
        }
    }
}
