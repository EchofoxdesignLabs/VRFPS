
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    [RequireComponent(typeof(XRSocketInteractor))]
    public class UniversalLimitedMagazineSocket : UniversalUnlimitedMagazineSocket
    {
        public int startingMagazineCount = 5;
        public bool resetOnDeath;
        [Space]
        public int magazineCount = 0;

        public TextMeshProUGUI countText;

        public void Start()
        {
            GetComponentInParent<Damageable>().DeathEvent += OnDeath;
            
            //Decrement magazine count when magazine is removed from socket
            _socket.selectExited.AddListener((_) => {magazineCount--;});
            
            magazineCount = startingMagazineCount;
        }

        public void OnDeath()
        {
            if (!resetOnDeath) return;

            magazineCount = startingMagazineCount;
        }
        
        protected override void Update()
        {
            base.Update();
            
            if(countText != null) countText.text = magazineCount.ToString();
        }

        protected override Magazine GetCompatibleMagazinePrefab()
        {
            if(magazineCount <= 0) return null;
            
            return base.GetCompatibleMagazinePrefab();
        }
        
        protected override void DestroyedOldMagazineEvent()
        {
            //Destroying mag triggers selectExited event which unintentionally decrements count
            //So we need to increment count here to counteract that
            magazineCount++;
        }
    }
}
