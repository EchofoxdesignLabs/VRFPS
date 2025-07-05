#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VRFPSKit.HandPoses
{
    [CustomEditor(typeof(XRHandPoseContainer))]
    public class PoseContainerEditor : Editor
    {
        private XRHandPoseContainer poseContainer = null;

        private void OnEnable()
        {
            poseContainer = (XRHandPoseContainer)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Open Pose Editor"))
                PoseWindow.Open(poseContainer.pose, poseContainer);
        }
    }
}
#endif
