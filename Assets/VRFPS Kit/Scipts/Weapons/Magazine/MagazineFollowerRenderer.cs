using UnityEngine;
using VRFPSKit;

namespace VRFPSKit
{
    public class MagazineFollowerRenderer : MonoBehaviour
    {
        public Vector3 offsetFromLowestCartridge;
        public bool ignoreXPosition;
        [Space]
        public Transform cartridgeRendererContainer;
        
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        
        // Update is called once per frame
        void Update()
        {
            Transform lowestCartridge = GetLastRenderedCartridgeChild(cartridgeRendererContainer);

            if (lowestCartridge == null)
            {
                transform.localPosition = _startPosition;
                transform.localRotation = _startRotation;
            }
            else
            {
                transform.rotation = lowestCartridge.rotation;
                
                transform.position = lowestCartridge.position;
                transform.localPosition += offsetFromLowestCartridge;
                if (ignoreXPosition) transform.localPosition = new Vector3(_startPosition.x, transform.localPosition.y, transform.localPosition.z);
            }
        }
        
        private Transform GetLastRenderedCartridgeChild(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (!child.gameObject.activeSelf) continue;
                var cartridgeRenderer = child.GetComponent<CartridgeRenderer>();
                if (cartridgeRenderer == null) continue;
                if (cartridgeRenderer.cartridgeToRender.IsNull()) continue;
                
                return child;
            }
            return null; // Return null if no enabled child is found
        }
        
        void Awake()
        {
            _startPosition = transform.localPosition;
            _startRotation = transform.localRotation;
        }
    }
}
