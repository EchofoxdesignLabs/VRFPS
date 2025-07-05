using UnityEngine;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class ButtonSound : MonoBehaviour
    {
        public AudioSource pressSound;
        public AudioSource releaseSound;
        
        private bool _pressedLastFrame;
        
        private PhysicsButton _button;
        
        void Update()
        {
            if(_button.pressed && !_pressedLastFrame) pressSound.Play();
            if(!_button.pressed && _pressedLastFrame) releaseSound.Play();
            
            _pressedLastFrame = _button.pressed;
        }
        
        void Awake()
        {
            _button = GetComponent<PhysicsButton>();
        }
    }
}
