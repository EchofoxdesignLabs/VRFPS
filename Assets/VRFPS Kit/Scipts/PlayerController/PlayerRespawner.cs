using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    /// <summary>
    /// Teleports player once Damageable health reaches 0
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    public class PlayerRespawner : MonoBehaviour
    {
        public ItemDropsOnDeath itemDropsOnDeath = ItemDropsOnDeath.DropAllItems;
        public enum ItemDropsOnDeath
        {
            KeepItems,
            DropHeldItems,
            DropAllItems
        }
        
        private void Death()
        {
            Respawn();
            
            // Drop items on death depending on configuration
            if(itemDropsOnDeath == ItemDropsOnDeath.DropHeldItems)
                foreach(var handInteractor in GetComponentsInChildren<XRDirectInteractor>())
                    handInteractor.interactionManager.CancelInteractorSelection((IXRSelectInteractor)handInteractor);
            
            if(itemDropsOnDeath == ItemDropsOnDeath.DropAllItems)
                foreach(var playerInteractor in GetComponentsInChildren<XRBaseInteractor>())
                    playerInteractor.interactionManager.CancelInteractorSelection((IXRSelectInteractor)playerInteractor);
        }

        private void Respawn()
        {
            Vector3 spawnPosition = Vector3.zero;
            
            transform.position = spawnPosition;
            GetComponent<Damageable>().ResetHealth();
        }
        
        private void Awake()
        {
            GetComponent<Damageable>().DeathEvent += Death;
        }
    }
}