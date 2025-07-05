using UnityEngine;

namespace VRFPSKit
{
    public class AudioDistancePassFilter : MonoBehaviour
    {
        [Header("Far away cutoff frequencies")]
        [Range(0, 22000)]public float lowPassCutoffFrequency = 22000;
        [Range(0, 22000)]public float highPassCutoffFrequency = 1400;
        [Space]
        public float farDistance = 200;
        public bool continueCuttingPastDistance = false;
        
        private static AudioListener _cachedListener;
        private AudioHighPassFilter _highFilter;
        private AudioLowPassFilter _lowFilter;
        //TODO Resonance?
        private void Update()
        {
            _highFilter.cutoffFrequency = CalculateHighCutoffFrequency();
            _lowFilter.cutoffFrequency = CalculateLowCutoffFrequency();
        }

        private float CalculateHighCutoffFrequency()
        {
            float farDistance01 = Mathf.InverseLerp(0, farDistance, CalculateDistance());
            float freq = Mathf.Lerp(0, highPassCutoffFrequency, farDistance01);

            if (!continueCuttingPastDistance) freq = Mathf.Clamp(freq, 50, highPassCutoffFrequency);

            return freq;
        }

        private float CalculateLowCutoffFrequency()
        {
            float farDistance01 = Mathf.InverseLerp(0, farDistance, CalculateDistance());
            float freq = Mathf.Lerp(22000, lowPassCutoffFrequency, farDistance01);

            if (!continueCuttingPastDistance) freq = Mathf.Clamp(freq, lowPassCutoffFrequency, 22000);

            return freq;
        }
        
        private float CalculateDistance() 
        {
            AudioListener listener = GetListener();
            if(listener == null) return 0;
            
            return Vector3.Distance(transform.position, listener.transform.position);
        }

        private static AudioListener GetListener()
        {
            if(_cachedListener == null) _cachedListener = Object.FindAnyObjectByType<AudioListener>();

            return _cachedListener;
        }

        private void Awake()
        {
            _highFilter = GetComponent<AudioHighPassFilter>();
            if(!_highFilter) _highFilter = gameObject.AddComponent<AudioHighPassFilter>();
            _lowFilter = GetComponent<AudioLowPassFilter>();
            if(!_lowFilter) _lowFilter = gameObject.AddComponent<AudioLowPassFilter>();
        }
    }
}
