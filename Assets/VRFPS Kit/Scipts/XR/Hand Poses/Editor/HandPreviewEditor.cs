#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace VRFPSKit.HandPoses
{
    [CustomEditor(typeof(PreviewHand))]
    public class HandPreviewEditor : Editor
    {
        private PreviewHand previewHand = null;
        private Transform activeJoint = null;
        
        private bool _disableGizmosWhenDone;

        private void OnEnable()
        {
            previewHand = target as PreviewHand;
            
            if (!GetSceneViewGizmosEnabled())
            {
                Debug.Log("HandPreviewEditor requires Gizmos enabled, enabling while editing...");
                SetSceneViewGizmos(true);
                _disableGizmosWhenDone = true;
            }
        }

        private void OnDisable()
        {
            if(_disableGizmosWhenDone)
                SetSceneViewGizmos(false);
        }

        private void OnSceneGUI()
        {
            DrawJointButtons();
            DrawJointHandle();
        }

        private void DrawJointButtons()
        {
            // Draw a button for each joint
            foreach (Transform joint in previewHand.Joints)
            {
                // Were one of the buttons pressed?
                bool pressed = Handles.Button(joint.position, joint.rotation, 0.01f, 0.005f, Handles.SphereHandleCap);

                // Did we select the same joint?
                if (pressed)
                    activeJoint = IsSelected(joint) ? null : joint;
            }
        }

        private bool IsSelected(Transform joint)
        {
            return joint == activeJoint;
        }

        private void DrawJointHandle()
        {
            // If a joint is selected
            if (HasActiveJoint())
            {
                // Draw handle
                Quaternion currentRotation = activeJoint.rotation;
                Quaternion newRotation = Handles.RotationHandle(currentRotation, activeJoint.position);

                // Detect if handle has rotated
                if (HandleRotated(currentRotation, newRotation))
                {
                    Undo.RecordObject(activeJoint, "Joint Rotated");
                    activeJoint.rotation = newRotation;
                }
            }
        }

        private bool HasActiveJoint()
        {
            return activeJoint;
        }

        private bool HandleRotated(Quaternion currentRotation, Quaternion newRotation)
        {
            return currentRotation != newRotation;
        }
        
        public static void SetSceneViewGizmos(bool gizmosOn)
        {
            UnityEditor.SceneView sv =
                UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>(null, false);
            sv.drawGizmos = gizmosOn;
        }

        public static bool GetSceneViewGizmosEnabled()
        {
            UnityEditor.SceneView sv =
                UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>(null, false);
            return sv.drawGizmos;
        }
    }
}
#endif