using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit.HandPoses
{
    [SerializeField]
    [CreateAssetMenu(fileName = "NewPoseData")]
    public class Pose : ScriptableObject
    {
        // Info for each hand
        public HandInfo leftHandInfo = HandInfo.Empty;
        public HandInfo rightHandInfo = HandInfo.Empty;

        public HandInfo GetHandInfo(InteractorHandedness handType)
        {
            // Return Left or Right, you can use a dictionary or different pose appliers
            switch (handType)
            {
                case InteractorHandedness.Left:
                    return leftHandInfo;
                case InteractorHandedness.Right:
                    return rightHandInfo;
                case InteractorHandedness.None:
                    return HandInfo.Empty;
            }

            // Return an empty 
            return HandInfo.Empty;
        }
    }
}