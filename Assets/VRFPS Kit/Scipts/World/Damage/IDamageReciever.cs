using UnityEngine;

namespace VRFPSKit
{
    public interface IDamageReciever
    {
        void TakeDamage(float damage);
        
        Transform transform { get; } 
    }
}
