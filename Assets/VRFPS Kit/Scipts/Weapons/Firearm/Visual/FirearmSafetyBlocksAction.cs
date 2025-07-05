using System.Linq;
using UnityEngine;

namespace VRFPSKit
{
    /// <summary>
    /// Useful especially on the ak since putting the lever to the safe position will block the action from going back fully
    /// </summary>
    [RequireComponent(typeof(FirearmCyclingAction), typeof(Firearm))]
    public class FirearmSafetyBlocksAction : MonoBehaviour
    {
        public FireMode[] blockingFireModes = new[] { FireMode.Safe };
        public float maxActionPosition = 0.8f;
        
        private FirearmCyclingAction _action;
        private Firearm _firearm;
        
        // Update is called once per frame
        void LateUpdate()
        {
            if (!blockingFireModes.Contains(_firearm.currentFireMode)) return;
            if (_action.actionPosition01 < maxActionPosition) return;
                
            _action.actionPosition01 = maxActionPosition;
        }
        
        void Awake()
        {
            _action = GetComponent<FirearmCyclingAction>();
            _firearm = GetComponent<Firearm>();
        }
    }
}
