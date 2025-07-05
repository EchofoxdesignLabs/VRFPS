using System;
using UnityEngine;

namespace VRFPSKit
{
    public class Resettable: MonoBehaviour
    {
        public string groupID;
        
        private Vector3 _startPos;
        private Quaternion _startRot;

        public static void Reset(string groupID)
        {
            foreach(var resettable in FindObjectsByType<Resettable>(FindObjectsSortMode.None))
                if(resettable.groupID == groupID)
                    resettable.Reset();
        }
        
        public void Reset()
        {
            transform.position = _startPos;
            transform.rotation = _startRot;

            if (GetComponent<Rigidbody>() is Rigidbody rb)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            if (GetComponent<Damageable>() is Damageable damageable)
                damageable.ResetHealth();
            //if (GetComponent<Magazine>() is Magazine magazine)
            //    magazine.ResetHealth();
        }

        private void OnDestroy()
        {
            //TODO
        }

        private void Awake()
        {
            _startPos = transform.position;
            _startRot = transform.rotation;
        }
    }
}
