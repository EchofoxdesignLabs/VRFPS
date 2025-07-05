using System.Threading.Tasks;
using UnityEngine;

namespace VRFPSKit
{
    public static class SpeedOfSound
    { 
        public const float Value = 343; // m/s 

        private static AudioListener _cachedListener;

        public static async void PlayDelayedBySpeedOfSound(this AudioSource source)
        {
            if (source == null) return;
            if (source.transform == null) return;
            await Task.Delay((int)(CalculateDelay(source.transform) * 1000 / Time.timeScale));
            
            if (source == null) return;
            source.Play();
        }
        
        public static float CalculateDelay(Transform transform) => CalculateDelay(Vector3.Distance(transform.position, GetListener()?.transform?.position ?? Vector3.zero));
        
        public static float CalculateDelay(float distance) => distance / Value;

        private static AudioListener GetListener()
        {
            if(_cachedListener == null) _cachedListener = Object.FindAnyObjectByType<AudioListener>();

            return _cachedListener;
        }
    }
}
