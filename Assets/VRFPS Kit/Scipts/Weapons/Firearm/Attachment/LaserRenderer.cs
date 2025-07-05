using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRFPSKit;

namespace VFFPSKit
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserRenderer : MonoBehaviour
    {
        //"Default" layer
        public LayerMask collisionMask = 1 << 0;
        public Light rayLight;
        
        private Magazine _magazineLastFrame;

        private LineRenderer _line;
        private void Update()
        {
            UpdateRayLength();
        }

        private void UpdateRayLength()
        {
            float lineLength = 1000;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit,Mathf.Infinity, collisionMask))
                lineLength = hit.distance;

            Vector3 lineEndPosition = new Vector3(0, 0, lineLength);
            _line.SetPosition(1, lineEndPosition);
            rayLight.transform.localPosition = lineEndPosition - new Vector3(0, 0, .1f);
        }
        
        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
        }
    }
}