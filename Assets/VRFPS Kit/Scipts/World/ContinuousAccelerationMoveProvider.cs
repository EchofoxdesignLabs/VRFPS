using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

namespace VRFPSKit
{
    [RequireComponent(typeof(CharacterController))]
    public class ContinuousAccelerationMoveProvider : ContinuousMoveProvider
    {
        [Space]
        public float accelerationRate = 5f; // Higher values = faster acceleration
        
        private CharacterController _controller;

        protected override Vector3 ComputeDesiredMove(Vector2 input)
        {
            // Compute the base movement from the parent class or base logic
            Vector3 baseMovement = base.ComputeDesiredMove(input); 

            // Get the current velocity and its magnitude
            Vector3 currentVelocity = _controller.velocity;
            float currentSpeed = currentVelocity.magnitude;

            // Normalize current velocity and base movement direction
            Vector3 currentDirection = currentVelocity.normalized;
            Vector3 desiredDirection = baseMovement.normalized;

            // Calculate the dot product to determine direction similarity (-1 to 1 range)
            float directionFactor = Vector3.Dot(currentDirection, desiredDirection);

            // Adjust acceleration based on the dot product
            float accelerationFactor = Mathf.Clamp01((1f + directionFactor) / 2f); // Normalize to [0, 1]

            // Reduce acceleration further based on current speed
            float speedFactor = Mathf.Clamp01(currentSpeed / moveSpeed);

            // Combine acceleration factors
            float finalAcceleration = (1f - speedFactor) * accelerationFactor;

            // Scale the desired movement direction by the final acceleration
            Vector3 targetMove = baseMovement * finalAcceleration;

            // Gradually adjust current velocity towards the target move direction
            Vector3 adjustedMove = Vector3.Lerp(currentVelocity, targetMove, Time.deltaTime * accelerationRate);

            // Print for debugging (optional)
            print($"Current Speed: {currentSpeed}, Direction Factor: {directionFactor}, Final Acceleration: {finalAcceleration}");

            return adjustedMove;
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            _controller = GetComponent<CharacterController>();
        }
    }
}
