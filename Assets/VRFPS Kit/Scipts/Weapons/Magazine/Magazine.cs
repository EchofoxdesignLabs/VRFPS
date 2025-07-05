using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRFPSKit
{
    /// <summary>
    /// Representing a magazine that stores cartridges. Can be used on a magazine item
    /// object or as an internal magazine by attaching it to a firearm
    /// </summary>
    public class Magazine : MonoBehaviour
    {
        public Caliber caliber;
        public int capacity;
        [Space] public Cartridge[] cartridgePreset;

        public string magazineType;
        
        [Space]
        private readonly List<Cartridge> cartridges = new();

        public Action CartrigesChangedEvent;
        
        public void Start()
        {
            FillPreset();
        }
        
        public void FillPreset()
        {
            foreach (var presetCartridge in cartridgePreset.Reverse())
                AddCartridgeToTop(presetCartridge);
        }
        
        public void Reset()
        {
            cartridges.Clear();
            
            CartrigesChangedEvent?.Invoke();
        }

        public Cartridge GetTopCartridge()
        {
            if (cartridges.Count == 0)
                return Cartridge.Empty;

            return cartridges[^1];
        }

        public void AddCartridgeToTop(Cartridge cartridge, int amount = 1)
        {
            //Add cartridge to top x times
            for (int i = 0; i < amount; i++)
            {
                //Check if cartridge fits
                if (IsFull())
                    return;

                cartridges.Add(cartridge);
            }
            
            CartrigesChangedEvent?.Invoke();
        }

        public void RemoveCartridgeFromTop(int amount = 1)
        {
            //Remove cartridge from top x times
            for (int i = 0; i < amount; i++)
            {
                int index = cartridges.Count - 1;

                if (index < 0)
                    break;
                
                cartridges.RemoveAt(index);
            }
            
            CartrigesChangedEvent?.Invoke();
        }

        public Cartridge[] GetCartridges() => cartridges.ToArray();

        public bool IsEmpty() => cartridges.Count == 0;

        public bool IsFull() => cartridges.Count == capacity;
    }
}