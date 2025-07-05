using UnityEngine;

namespace VRFPSKit
{
    public class ExplosionSound : MonoBehaviour
    {
        void Start()//Explodes as soon as spawned
        {
            GetComponent<AudioSource>().PlayDelayedBySpeedOfSound();
        }
    }
}
