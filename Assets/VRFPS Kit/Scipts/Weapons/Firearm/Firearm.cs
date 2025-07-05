using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

#if HAPTIC_PATTERNS
using HapticPatterns;
#endif

namespace VRFPSKit
{
    /// <summary>
    /// Main script that handles behaviour which is in common for all firearms,
    /// the rest of the components are compositional and added when needed
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Firearm : MonoBehaviour
    {
        #if HAPTIC_PATTERNS
        public HapticPattern shootHaptic;
        #else
        [Header("VR Haptic Patterns Isn't Installed")]
        #endif
        
        [Space] 
        public FireMode[] availableFireModes;
        [Space] 
        
        [Header("Current State")]
        public Cartridge chamberCartridge;
        public Magazine magazine;
        public FireMode currentFireMode;
        public bool isHammerCocked = true;

        [HideInInspector] public bool inBattery = true;
        [HideInInspector] public string weaponName;
        [HideInInspector] public float lastShotTime;
        
        //Events will be called both on server & clients
        public event Action<Cartridge> ShootEvent;

        public void TryShoot()
        {
            //Can't shoot unless action is in battery
            if (!inBattery) return;
            
            //Drop hammer
            isHammerCocked = false;
            
            //Make sure the cartridge can fire
            if (!chamberCartridge.CanFire()) return;
            
            Cartridge unspentCartridge = chamberCartridge;
            chamberCartridge.Consume();
            
            //Fire if all checks pass
            ShootEvent?.Invoke(unspentCartridge);
            
            //If haptic patterns is installed, play shoot haptic
            #if HAPTIC_PATTERNS 
            if (shootHaptic != null)
                shootHaptic.PlayOverTime(GetComponent<XRGrabInteractable>());
            #endif
        }

        /// <summary>
        /// If a cartridge is in the chamber, eject it
        /// </summary>
        public void TryEjectChamber()
        {
            //Cant eject empty chamber
            if (chamberCartridge.IsNull())
                return;

            //If an ejector exists, visually eject the round
            GetComponent<CartridgeEjector>()?.EjectCartridge(chamberCartridge);

            //Chamber is emptied
            chamberCartridge = Cartridge.Empty;
        }
        
        private void Awake()
        {
            ShootEvent += _ => lastShotTime = Time.time;
        } 

        /// <summary>
        /// Called when saving / building in the editor, only used to retrieve weapon name in editor
        /// </summary>
        protected void OnValidate()
        {
            if(GetComponent<XRGrabInteractable>().colliders.Count == 0) 
                Debug.LogWarning($"Recommended to assign colliders to XRGrabInteractable on firearm {gameObject.name}, otherwise it might use colliders from weapon controlls etc & two handed grab gets harder");
            
            //PrefabUtility.GetCorrespondingObjectFromOriginalSource() is only available in the editor
            #if UNITY_EDITOR
                //Store weapon name based on the original prefab name
                GameObject prefabObj = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
                if(prefabObj)
                    weaponName = prefabObj.name;
            #else
                //Throw error if weaponName for some reason is empty
                if(weaponName == String.Empty)
                    Debug.LogError($"Weapon.weaponName has not been assigned in '{gameObject.name}', this is automatically set when you manually save the prefab");
            #endif
        }
    }
}