using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    [RequireComponent(typeof(XRBaseInteractor))]
    public class XRSpawnWithInteractable : MonoBehaviour
    {
        public bool respawnOnDeath = true;
        public XRBaseInteractable selectedSpawnPrefab;

        private void Start()
        {
            SpawnInteractable();
        }
        
        private void CMD_Respawn() => Respawn();
        
        public void Respawn()
        {
            GameObject spawnedObject = Instantiate(selectedSpawnPrefab.transform.gameObject, transform.position, transform.rotation);
            IXRSelectInteractable spawnedInteractable = spawnedObject.GetComponent<IXRSelectInteractable>();
                
            GetComponent<XRBaseInteractor>().interactionManager.SelectEnter(GetComponent<XRBaseInteractor>(), spawnedInteractable);
            Destroy(this);
            
            if (!respawnOnDeath) return;
            if (GetComponent<XRBaseInteractor>().hasSelection) return; // Already holding something
            
            SpawnInteractable();
        }

        private async void SpawnInteractable()
        {
            GameObject spawnedObject = Instantiate(selectedSpawnPrefab.transform.gameObject, transform.position, transform.rotation);

            await Task.Delay(50);
            
            IXRSelectInteractable interactable = spawnedObject.GetComponent<IXRSelectInteractable>();
            GetComponent<XRBaseInteractor>().interactionManager.SelectEnter(GetComponent<XRBaseInteractor>(), interactable);
        }
        
        private void Awake()
        {
            GetComponentInParent<Damageable>().ResetHealthEvent += CMD_Respawn;
        }
    }
}
