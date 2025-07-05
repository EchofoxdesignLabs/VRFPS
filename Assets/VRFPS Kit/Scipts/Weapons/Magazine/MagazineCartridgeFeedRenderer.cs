using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRFPSKit
{
    public class MagazineCartridgeFeedRenderer : MonoBehaviour
    {
        public bool alternateSides = true;
        
        private GameObject _topCartridge;
        private XRGrabInteractable _interactable;
        private Magazine _magazine;
    
        // Update is called once per frame
        void Update()
        {
            bool flipCartridges = ShouldFlipCartridges();
            bool topCartridgeVisible = true;
            
            //Special render case if magazine is in a firearm with a cycling action
            if(TryGetFirearmCyclingAction() is FirearmCyclingAction cyclingAction)
            {
                //If action is not in "feed" position, render as if cartridges are pressed down by the bolt
                //(just a trick by flipping again and hiding the top cartridge)
                if (cyclingAction.actionPosition01 < cyclingAction.roundFeedAction01 || !cyclingAction.loadingCartridge.IsNull())
                {
                    topCartridgeVisible = false;
                    flipCartridges = !flipCartridges;
                }
            }
            
            _topCartridge.SetActive(topCartridgeVisible);
            if(alternateSides)
                transform.localScale = new Vector3(flipCartridges ? -1 : 1, 1, 1);
        }

        public bool ShouldFlipCartridges()
        {
            bool oddAmmoCount = _magazine.GetCartridges().Length % 2 == 1;
            
            return oddAmmoCount;
        }

        private FirearmCyclingAction TryGetFirearmCyclingAction()
        {
            if (!_interactable.isSelected) return null;
            if (_interactable.interactorsSelecting[0] is not SimpleMagazineInteractor magazineInteractor) return null;
            if (magazineInteractor.GetComponentInParent<FirearmCyclingAction>() is not FirearmCyclingAction cyclingAction) return null;

            return cyclingAction;
        }
        
        void Awake()
        {
            _magazine = GetComponentInParent<Magazine>();
            _interactable = GetComponentInParent<XRGrabInteractable>();
            _topCartridge = transform.GetChild(0)?.gameObject;
        }
    }
}
