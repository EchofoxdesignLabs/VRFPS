using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace VRFPSKit
{
    public class CartridgeEjector : MonoBehaviour
    {
        private const float MinimumActionSpeedMultiplier01 = .2f;
        private const float CasingDestructionTime = 5f;
        
        public Transform spawnPoint;
        public GameObject cartridgeItemPrefab;
        [Space] 
        public Vector3 defaultVelocity;
        public Vector3 randomVelocity = new (0.5f, 0.5f, 0.5f);
        [Space]
        public Vector3 defaultTorque = new (20, 20, 0);
        public Vector3 randomTorque = new (20, 20, 20);

        public event Action<Cartridge> EjectEvent;

        private float _actionPosition01LastFrame;
        
        private FirearmCyclingAction _action;

        private void LateUpdate()
        {
            _actionPosition01LastFrame = _action.actionPosition01;
        }
        
        private float ActionMovementSpeed01 () => (_action.actionPosition01 - _actionPosition01LastFrame); // Moving back completely over one frame is max speed

        /// <summary>
        /// Spawns an ejected cartridge
        /// </summary>
        /// <param name="cartridge">Ejected cartridge properties</param>
        public void EjectCartridge(Cartridge cartridge)
        {
            //Call events
            if (!cartridge.IsNull())
                EjectEvent?.Invoke(cartridge);
            
            if (cartridge.Equals(Cartridge.Empty))
            {
                Debug.LogWarning("Tried ejecting an empty cartridge");
                return;
            }
            if(cartridgeItemPrefab == null)
            {
                Debug.LogWarning("CartridgeEjector has no cartridgeItemPrefab assigned!");
                return;
            }

            float ejectSpeedMultiplier = ActionMovementSpeed01() / 20;
            
            //Instantiate cartridge
            GameObject obj = Instantiate(cartridgeItemPrefab, spawnPoint.position, spawnPoint.rotation);
            
            DelayCartridgeCollision(obj);
            
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            
            //Apply Random Linear Velocity
            Vector3 randomizedVelocity = defaultVelocity + RandomVector3(randomVelocity);
            Vector3 worldSpaceVelocity = transform.TransformDirection(randomizedVelocity);
            worldSpaceVelocity *= ejectSpeedMultiplier;
            rb.AddForce(worldSpaceVelocity, ForceMode.VelocityChange);
            
            //Apply random angular velocity
            rb.maxAngularVelocity = Single.PositiveInfinity;
            Vector3 eulerTourqe = defaultTorque + RandomVector3(randomTorque);
            eulerTourqe *= ejectSpeedMultiplier;
            
            Vector3 worldSpaceEulerTourqe = transform.TransformVector(eulerTourqe);
            rb.AddTorque(worldSpaceEulerTourqe,
                ForceMode.VelocityChange);

            //Apply cartridge item values
            CartridgeItem cartridgeItem = obj.GetComponent<CartridgeItem>();
            cartridgeItem.cartridge = cartridge;
            
            //If we ejected empty casing, Schedule Cartridge Destruction,
            //you can remove this if you want Casings to never disappear!a
            if(cartridge.bulletType == BulletType.Empty_Casing)
                DestroyCartridgeLater(obj);
        }

        private async void DelayCartridgeCollision(GameObject cartridge)
        {
            Array.ForEach(cartridge.GetComponentsInChildren<Collider>(),  col => col.enabled = false);
            await Task.Delay(50);
            Array.ForEach(cartridge.GetComponentsInChildren<Collider>(),  col => col.enabled = true);
        }
        
        private async void DestroyCartridgeLater(GameObject cartridge)
        {
            await Task.Delay((int)(CasingDestructionTime*1000));
            
            Destroy(cartridge);
        }
        
        private void Awake()
        {
            _action = GetComponentInParent<FirearmCyclingAction>();
        }
        
        private Vector3 RandomVector3(Vector3 bounds) => new (Random.Range(-bounds.x, bounds.x), Random.Range(-bounds.y, bounds.y), Random.Range(-bounds.z, bounds.z));
    }
}