using System;
using UnityEngine;

namespace VRFPSKit
{
    public class FirearmSafetyHint : FirearmHint 
    {
        protected override bool HintConditionMet()
        {
            if (!_firearm.inBattery) return false;
            if (_firearm.currentFireMode != FireMode.Safe) return false;

            return true;
        }
    }
}
