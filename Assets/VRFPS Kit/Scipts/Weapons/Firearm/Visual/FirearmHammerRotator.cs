using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    public class FirearmHammerRotator : MonoBehaviour
    {
        [Header("Leave hammer in uncocked position, assign cocked rotation below")]
        public Vector3 cockedRotation;
        
        [Tooltip("How long ahead of FirearmCyclingAction.hammerResetAction01 should uncocked hammer start moving back?")] [Range(0, 1)] 
        public float cockingLerpActionSpan = .2f;

        private Quaternion _uncockedRotation;
        
        private FirearmCyclingAction _action;
        private Firearm _firearm;
        
        // Update is called once per frame
        void Update()
        {
            float hammerCocked01 = _firearm.isHammerCocked ? 1 : 0;
            
            //If hammer is not cocked, and action is within transition span, start moving hammer back
            if(hammerCocked01 == 0 && _action.actionPosition01 > _action.hammerResetAction01 - cockingLerpActionSpan)
                hammerCocked01 = Mathf.InverseLerp(_action.hammerResetAction01 - cockingLerpActionSpan, _action.hammerResetAction01, _action.actionPosition01);
            
            // Interpolate between the rotations
            transform.localRotation = Quaternion.Lerp(_uncockedRotation, Quaternion.Euler(cockedRotation), hammerCocked01);
        }
        void Awake()
        {
            _action = GetComponentInParent<FirearmCyclingAction>();
            _firearm = GetComponentInParent<Firearm>();
            if(_action == null || _firearm == null) Debug.LogError("FirearmHammerRotator couldn't find required components in parents");
            
            _uncockedRotation = transform.localRotation;
            
        }
    }
}
