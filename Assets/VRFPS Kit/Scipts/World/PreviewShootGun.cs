using UnityEngine;

namespace VRFPSKit
{
    public class PreviewShootGun : MonoBehaviour
    {
        public Firearm previewFirearm;

        public void Shoot()
        {
            previewFirearm.chamberCartridge = new Cartridge(Caliber.Cal_9x19, BulletType.Tracer);
            previewFirearm.TryShoot();
        }
    }
}
