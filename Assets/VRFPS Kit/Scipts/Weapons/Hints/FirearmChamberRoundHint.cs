using System;
using UnityEngine;

namespace VRFPSKit
{
    public class FirearmChamberRoundHint : FirearmHint
    {
        protected override bool HintConditionMet()
        {
            if (!_firearm.inBattery) return false;
            if (_firearm.currentFireMode == FireMode.Safe) return false;
            if (_firearm.chamberCartridge.CanFire()) return false;
            if (_firearm.magazine == null) return false;
            if (_firearm.magazine.GetTopCartridge().IsNull()) return false;

            return true;
        }
    }
}
