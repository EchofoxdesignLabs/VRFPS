#if UNITY_EDITOR
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace VRFPSKit
{
    [CustomEditor(typeof(Grenade))]
    public class GrenadeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Reference to the target script
            Grenade grenade = (Grenade)target;
            
            // Disable buttons if not in Play mode
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            
            // Add buttons
            if(grenade.safetyPin)
                if (GUILayout.Button("Detach Safety Pin", GUILayout.Height(30)))
                    grenade.DetachSafetyPin();

            // Re-enable the GUI
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            
            // Draw the default inspector
            DrawDefaultInspector();
        }
    }
}
#endif