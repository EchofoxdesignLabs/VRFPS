using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using VRFPSKit;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRFPSKit.HandPoses
{
    [ExecuteInEditMode]
    public class HandManager : MonoBehaviour
    {
        // The hand prefabs we're using
        [SerializeField] private bool hideHands = true;
        [SerializeField] private GameObject leftHandPrefab = null;
        [SerializeField] private GameObject rightHandPrefab = null;

        // The references to the hands being manipulated
        public PreviewHand LeftHand { get; private set; } = null;
        public PreviewHand RightHand { get; private set; } = null;
        public bool HandsExist => LeftHand && RightHand;

        private void OnEnable()
        {
            CreateHandPreviews();
        }

        private void OnDisable()
        {
            DestroyHandPreviews();
        }

        private void CreateHandPreviews()
        {
            // Create both hands
            LeftHand = CreateHand(leftHandPrefab);
            RightHand = CreateHand(rightHandPrefab);
        }

        public void UpdateActiveHand(bool rightHandEnabled)
        {
            SetHandActive(LeftHand, !rightHandEnabled);
            SetHandActive(RightHand, rightHandEnabled);
        }

        private void SetHandActive(PreviewHand hand, bool setActive)
        {
            Undo.RecordObject(hand.gameObject, "Toggle Hand");
            hand.gameObject.SetActive(setActive);
            if(setActive)
                Selection.activeGameObject=hand.gameObject;
        }

        private PreviewHand CreateHand(GameObject prefab)
        {
            // Create the hand
            GameObject handObject = Instantiate(prefab, transform);

            handObject.hideFlags = HideFlags.DontSave;
            // If we want to hide the hand, this prevents accidental manual deletion
            if (hideHands)
                handObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;

            // Get the preview to save
            return handObject.GetComponent<PreviewHand>();
        }

        private void DestroyHandPreviews()
        {
            // Make sure to destroy the gameobjects
#if UNITY_EDITOR
            if(LeftHand) DestroyImmediate(LeftHand.gameObject);
            if(RightHand) DestroyImmediate(RightHand.gameObject);
#endif
        }

        public void UpdateHands(Pose pose, Transform parentTransform, XRHandPoseContainer poseContainer)
        {
            // Child the hands to the object we're working with, simplifies everything
            LeftHand.transform.parent = parentTransform;
            RightHand.transform.parent = parentTransform;

            LeftHand.attachTransform = poseContainer.GetAttachPoint();
            RightHand.attachTransform = poseContainer.GetAttachPoint();

            // Pose 'em!
            LeftHand.ApplyPose(pose);
            RightHand.ApplyPose(pose);
        }

        public void SavePose(Pose pose, XRHandPoseContainer poseContainer)
        {
            // Mark object as dirty for saving
#if UNITY_EDITOR
            EditorUtility.SetDirty(pose);
#endif

            // Copy the hand info into
            pose.leftHandInfo.Save(LeftHand, poseContainer);
            pose.rightHandInfo.Save(RightHand, poseContainer);
        }
    }
}
