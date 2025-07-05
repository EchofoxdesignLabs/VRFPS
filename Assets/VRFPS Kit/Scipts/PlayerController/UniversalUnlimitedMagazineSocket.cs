using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    [RequireComponent(typeof(XRSocketInteractor))]
    public class UniversalUnlimitedMagazineSocket : MonoBehaviour
    {
        public Magazine[] spawnableMagazines;

        protected Magazine defaultMagazine => spawnableMagazines[0];
        
        private Magazine _magazineLastFrame;
        
        protected XRSocketInteractor _socket;
        
        protected virtual void Update()
        {
            Magazine compatibleMag = GetCompatibleMagazinePrefab();
            
            //Update Socket Magazine if:
            if (!_socket.hasSelection || // Socket isn't selecting an item
                _socket.firstInteractableSelected.transform.GetComponent<Magazine>()?.magazineType != compatibleMag?.magazineType) //Or a different magazine type is needed
                UpdateSocketMagazine(compatibleMag); 
        }

        private void UpdateSocketMagazine(Magazine magazinePrefab)
        {
            if(magazinePrefab == null) return;
            
            //Despawn previous magazine
            if (_socket.hasSelection)
            {
                DestroyedOldMagazineEvent();
                Destroy(_socket.firstInteractableSelected.transform.gameObject);
            }

            GameObject magObj = Instantiate(magazinePrefab.gameObject);
            
            _socket.interactionManager.SelectEnter(_socket, magObj.GetComponent<IXRSelectInteractable>());
        }
        
        protected virtual void DestroyedOldMagazineEvent() { }

        private Player GetTargetPlayer()
        {
            Player player = GetComponentInParent<Player>();
            
            if(player == null)
                Debug.LogError("XRSocketUniversalMagazineSpawner couldn't find Player component in parent");

            return player;
        }
        
        private Firearm GetPlayerFirearm(Player player)
        {
            XRDirectInteractor[] playerHandInteractors = player.GetComponentsInChildren<XRDirectInteractor>();

            foreach (XRDirectInteractor handInteractor in playerHandInteractors)
                foreach (IXRSelectInteractable interactable in handInteractor.interactablesSelected)
                    if(interactable.transform.GetComponent<Firearm>() != null)
                        return interactable.transform.GetComponent<Firearm>();
            
            return null;
        }
        
        protected virtual Magazine GetCompatibleMagazinePrefab()
        {
            Firearm firearm = GetPlayerFirearm(GetTargetPlayer());
            if(firearm == null) return defaultMagazine;
            SimpleMagazineInteractor magazineInteractor = GetPlayerFirearm(GetTargetPlayer()).GetComponentInChildren<SimpleMagazineInteractor>();
            if(magazineInteractor == null) return defaultMagazine;

            foreach (Magazine magazineToTest in spawnableMagazines)
                if (magazineInteractor.compatibleMagazineTypes.Contains(magazineToTest.magazineType))
                    return magazineToTest;
            
            return defaultMagazine;
        }

        protected void Awake()
        {
            _socket = GetComponent<XRSocketInteractor>();
            
            //Disallow hovering items => only script can select items
            _socket.hoverFilters.Add(new XRHoverFilterDelegate((interactor, interactable) =>
            {
                return false;
            }));
        }
    }
}
