using QuickOutline;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    [RequireComponent(typeof(XRBaseInteractable))]
    public class XRHoverOutline : MonoBehaviour
    {
        private const float FadeInTime = .5f;
        private const float OutlineAlpha01 = .3f;
        
        public GameObject outlinedObject;
        
        private float _hoverTime = -1;
        
        private Outline _outline;
        private XRBaseInteractable _interactable;

        // Update is called once per frame
        private void Update()
        {
            if (_interactable.isHovered && //Hovered
                _interactable.interactorsHovering[0] is XRDirectInteractor hoveringInteractor && //Hovered by hand
                //Is highest priority hover target
                hoveringInteractor.targetsForSelection.Capacity > 0 && ReferenceEquals(hoveringInteractor.targetsForSelection[0], _interactable)) 
            {
                if(_hoverTime < 0) //Reset time if just hovered
                    _hoverTime = Time.time;
            }
            else _hoverTime = -1;
            //TODO fix fade
            
            float fade01 = Mathf.Clamp01(Mathf.InverseLerp(0, FadeInTime, Time.time - _hoverTime));
            if (!HoveredByHand()) fade01 = 0;
            
            Color fadeColor = _outline.OutlineColor;
            fadeColor.a = Mathf.SmoothStep(0, OutlineAlpha01, fade01);
            _outline.OutlineColor = fadeColor;
            
            
            _outline.enabled = HoveredByHand(); 
        }

        //Value will be set to -1 when not hovered, doing .hasHovered doesn't work since weapon selection will trigger hover too
        private bool HoveredByHand() => _hoverTime > 0;
        
        private void Awake()
        {
            if(outlinedObject == null) outlinedObject = gameObject;
            
            //Add and configure outline component
            _outline = outlinedObject.AddComponent<Outline>();
            _outline.OutlineMode = Outline.Mode.OutlineAll;
            _outline.OutlineColor = Color.white;
            _outline.OutlineWidth = 12f;
            _outline.enabled = false;
            
            _interactable = GetComponent<XRBaseInteractable>();
            _interactable.hoverExited.AddListener(_ => _hoverTime = -1);
        }
        
        private void OnDestroy()
        {
            Destroy(_outline);
        }
    }
}
