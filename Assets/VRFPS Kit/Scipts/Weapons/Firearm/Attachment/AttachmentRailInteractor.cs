using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    public class AttachmentRailInteractor : XRSocketInteractor
    {
        protected void Update()
        {
            foreach (var hoveredInteractable in interactablesHovered)
            {
                Vector3 position = attachTransform.position;
                hoveredInteractable.transform.position = attachTransform.position;
            }
        }

        /*TODO multiple
        /// <inheritdoc />
        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return base.CanSelect(interactable) &&
                   ((!hasSelection && !interactable.isSelected) ||
                    (IsSelecting(interactable) && interactable.interactorsSelecting.Count == 1));
        }
*/
        
    }
}
