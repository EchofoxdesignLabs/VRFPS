using System;
using UnityEngine;

namespace VRFPSKit
{
    public class FirearmActionLockedHint : FirearmHint 
    {
        private FirearmCyclingAction _action;
        
        protected override bool HintConditionMet()
        {
            if (!_action.isLockedBack) return false;
            if (_firearm.magazine == null) return false;
            if (_firearm.magazine.GetTopCartridge().IsNull()) return false;

            return true;
        }

        protected override void Awake()
        {
            base.Awake();
            
            _action = _firearm.GetComponent<FirearmCyclingAction>();
        }
    }
}
