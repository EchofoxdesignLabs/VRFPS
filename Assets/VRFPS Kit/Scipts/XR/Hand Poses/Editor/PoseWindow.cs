#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRFPSKit.HandPoses
{
    public class PoseWindow : EditorWindow
    {
        // The pose we're editing
        private Pose activePose = null;
        private XRHandPoseContainer activePoseContainer = null;

        // Root object
        private GameObject poseHelper = null;

        // Functionality
        private HandManager handManager = null;
        private SelectionHandler selectionHandler = null;
        private static bool _rightHand = true;

        private bool _initializedHandSelection = false;
        
        private void OnEnable()
        {
            CreatePoseHelper();
            EditorApplication.playModeStateChanged += CloseWindow;
            EditorSceneManager.sceneClosing += CloseWindow;
        }

        private void OnDisable()
        {
            DestroyPoseHelper();
            EditorApplication.playModeStateChanged -= CloseWindow;
            EditorSceneManager.sceneClosing -= CloseWindow;
        }

        void CreatePoseHelper()
        {
            if (!poseHelper)
            {
                // Get the manager from resources
                Object helperPrefab = Resources.Load("PoseHelper");

                // Instantiate it into the scene, mark as not to save
                poseHelper = (GameObject)PrefabUtility.InstantiatePrefab(helperPrefab);
                poseHelper.hideFlags = HideFlags.DontSave;

                // Get functionality
                selectionHandler = poseHelper.GetComponent<SelectionHandler>();
                handManager = poseHelper.GetComponent<HandManager>();
            }
        }

        private void DestroyPoseHelper()
        {
            DestroyImmediate(poseHelper);
        }

        private void CloseWindow(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.ExitingEditMode)
                Close();
        }

        private void CloseWindow(Scene scene, bool removingScene)
        {
            if (removingScene)
                Close();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            if (handManager.LeftHand == null || handManager.RightHand == null)
            {
                Close();
                return;
            }

            if (!_initializedHandSelection)
            {
                handManager.UpdateActiveHand(_rightHand);
                _initializedHandSelection = true;
            }
            
            GUIStyle labelStyle = EditorStyles.label;
            labelStyle.alignment = TextAnchor.MiddleCenter;

            string poseName = activePose ? activePose.name : "No Pose";
            GUILayout.Label(poseName, labelStyle);

            using (new EditorGUI.DisabledScope(activePose))
            {
                if (GUILayout.Button("Create Pose"))
                    CreatePose();

                if (GUILayout.Button("Refresh Pose"))
                    RefreshPose();
            }

            using (new EditorGUI.DisabledScope(!activePose))
            {
                if (GUILayout.Button("Clear Pose"))
                    ClearPose();
            }

            using (new EditorGUI.DisabledScope(!handManager.HandsExist))
            {
                PreviewHand leftHand = handManager.LeftHand;
                PreviewHand rightHand = handManager.RightHand;
                float objectWidth = EditorGUIUtility.currentViewWidth * 0.5f;
                GUILayout.Space(20);

                // Toggle buttons
                using (new GUILayout.HorizontalScope())
                {
                    if (_rightHand)
                    {
                        if (GUILayout.Button("Left Hand", GUILayout.Width(objectWidth)))
                        {
                            _rightHand = false;
                            handManager.UpdateActiveHand(_rightHand);
                        }
                        
                        GUILayout.Label("Right Hand", labelStyle, GUILayout.Width(objectWidth));
                    }
                    else
                    {
                        GUILayout.Label("Left Hand", labelStyle, GUILayout.Width(objectWidth));
                        if (GUILayout.Button("Right Hand", GUILayout.Width(objectWidth)))
                        {
                            _rightHand = true;
                            handManager.UpdateActiveHand(_rightHand);
                        }
                    }
                }

                // Buttons that require a pose
                using (new EditorGUI.DisabledScope(!activePose))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Mirror L > R", GUILayout.Width(objectWidth)))
                            MirrorPose(leftHand, rightHand);

                        if (GUILayout.Button("Mirror R > L", GUILayout.Width(objectWidth)))
                            MirrorPose(rightHand, leftHand);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Undo Changes", GUILayout.Width(objectWidth)))
                            UndoChanges(leftHand);

                        if (GUILayout.Button("Undo Changes", GUILayout.Width(objectWidth)))
                            UndoChanges(rightHand);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Reset Default", GUILayout.Width(objectWidth)))
                            ResetPose(leftHand);

                        if (GUILayout.Button("Reset Default", GUILayout.Width(objectWidth)))
                            ResetPose(rightHand);
                    }
                }
            }

            using (new EditorGUI.DisabledScope(!activePose))
            {
                GUILayout.Label("Remember to Save!", labelStyle);

                if (GUILayout.Button("Save Pose"))
                    handManager.SavePose(activePose, activePoseContainer);
            }
            
            GUILayout.Space(40);
            GUILayout.Label("Press dots to rotate easily", labelStyle);
        }

        private void CreatePose()
        {
            // Create the new asset
            activePose = CreatePoseAsset();

            // Give the pose to the object we have selected
            GameObject targetObject = selectionHandler.SetObjectPose(activePose);

            // Update the hands
            handManager.UpdateHands(activePose, targetObject.transform, activePoseContainer);
        }

        Pose CreatePoseAsset()
        {
            // Create new scriptable object
            Pose pose = CreateInstance<Pose>();

            // Store the asset
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/New Hand Pose.asset");
            AssetDatabase.CreateAsset(pose, path);

            return pose;
        }

        private void UpdateSelection()
        {
            UpdateActivePose(Selection.activeGameObject);
        }

        private void RefreshPose()
        {
            // Get the object we have selected
            GameObject currentObject = selectionHandler.CurretInteractable.gameObject;
            UpdateActivePose(currentObject);
        }

        private void UpdateActivePose(GameObject targetObject)
        {
            // Get pose from container, update hands
            activePose = activePoseContainer.pose;
            
            // If we have a pose, update the hands
            if(activePose)
                handManager.UpdateHands(activePose, targetObject.transform, activePoseContainer);
        }

        private void ClearPose()
        {
            selectionHandler.SetObjectPose(null);
            activePose = null;
        }

        private void ResetPose(PreviewHand hand)
        {
            Undo.RecordObject(hand.transform, "Reset Pose");
            hand.ApplyDefaultPose();
        }

        private void UndoChanges(PreviewHand hand)
        {
            Undo.RecordObject(hand.transform, "Undo Changes");
            hand.ApplyPose(activePose);
        }

        private void MirrorPose(PreviewHand sourceHand, PreviewHand targetHand)
        {
            Undo.RecordObject(targetHand.transform, "Mirror Pose");
            targetHand.MirrorAndApplyPose(sourceHand);
        }

        public static void Open(Pose pose, XRHandPoseContainer poseContainer)
        {
            PoseWindow window = GetWindow<PoseWindow>("Hand Poser");
            window.activePose = pose;
            window.activePoseContainer = poseContainer;
            window.UpdateSelection();
        }
    }
}
#endif