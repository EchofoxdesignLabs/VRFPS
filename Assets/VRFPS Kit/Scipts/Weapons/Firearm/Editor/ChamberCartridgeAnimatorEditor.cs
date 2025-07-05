#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRFPSKit;

[CustomEditor(typeof(ChamberCartridgeAnimator))]
public class ChamberCartridgeAnimatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();
        
        // Reference to the target script
        ChamberCartridgeAnimator animator = (ChamberCartridgeAnimator)target;

        if (GUILayout.Button("Add Current Orientation Keyframe", GUILayout.Height(30)))
        {
            // Record the object for undo
            Undo.RecordObject(animator, "Add Keyframe");
            
            animator.keyframes = animator.keyframes.Append(new ChamberCartridgeAnimationKeyframe(.5f, animator.transform.localPosition, animator.transform.localRotation.eulerAngles)).ToArray();
            
            // Mark the object as dirty to ensure changes are saved
            EditorUtility.SetDirty(animator);
        }
        
    }
}
#endif