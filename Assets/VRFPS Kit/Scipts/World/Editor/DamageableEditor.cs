#if UNITY_EDITOR
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace VRFPSKit
{
    [CustomEditor(typeof(Damageable))]
    public class DamageableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();
            
            // Reference to the target script
            Damageable damageable = (Damageable)target;

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            if (damageable == null) return;
            
            // Add buttons
            if (damageable.health > 0)
            {
                if (GUILayout.Button("Kill", GUILayout.Height(30)))
                    damageable.TakeDamage(damageable.health + 1);
            }
            else if (GUILayout.Button("Revive", GUILayout.Height(30)))
                damageable.ResetHealth();
            
            
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif