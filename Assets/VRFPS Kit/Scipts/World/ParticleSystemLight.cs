using UnityEngine;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    [RequireComponent(typeof(ParticleSystem), typeof(Light))]
    public class ParticleSystemLight : MonoBehaviour
    {
        public AnimationCurve lightIntensityCurve = AnimationCurve.EaseInOut(0, .5f, 1, 0);
        
        private ParticleSystem _particle;
        private Light _light;
        
        // Update is called once per frame
        void Update()
        {
            
            UpdateParticleLight();
        }
        
        private void UpdateParticleLight()
        {
            //Light is only enabled when particle system is playing
            bool effectIsPlaying = _particle.time > 0 && _particle.time < lightIntensityCurve.keys[^1].time;
            _light.enabled = effectIsPlaying;

            if (effectIsPlaying)
                _light.intensity = lightIntensityCurve.Evaluate(_particle.time);
            else
                _light.intensity = lightIntensityCurve.Evaluate(0);
        }
        
        void Awake()
        {
            _particle = GetComponent<ParticleSystem>();
            _light = GetComponent<Light>();
        }
    }
}
