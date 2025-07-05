#if UNITY_EDITOR
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace VRFPSKit
{
    [CustomEditor(typeof(FirearmTrigger))]
    public class FirearmTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Reference to the target script
            FirearmTrigger trigger = (FirearmTrigger)target;

            if (trigger == null)
            {
                DrawDefaultInspector();
                return;
            }
            
            // Disable buttons if not in Play mode
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            
            // Add buttons
            if (GUILayout.Button("Pull Trigger (0.5s)", GUILayout.Height(30)))
                PullTrigger(trigger);
            if (GUILayout.Button("Shoot FMJ", GUILayout.Height(30)))
            {
                LoadRound(trigger, new Cartridge(Caliber.Cal_9x19, BulletType.FMJ));
                PullTrigger(trigger);
            }
            if (GUILayout.Button("Shoot Tracer", GUILayout.Height(30)))
            {
                LoadRound(trigger, new Cartridge(Caliber.Cal_9x19, BulletType.Tracer));
                PullTrigger(trigger);
            }

            // Re-enable the GUI
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            
            // Draw the default inspector
            DrawDefaultInspector();
        }

        private void LoadRound(FirearmTrigger trigger, Cartridge cartridge)
        {
            Firearm firearm = trigger.GetComponent<Firearm>();

            firearm.chamberCartridge = cartridge;
        }

        private async void PullTrigger(FirearmTrigger trigger)
        {
            trigger.PressTrigger();
            trigger.GetComponent<Firearm>().TryShoot();
            
            await Task.Delay(500);
            trigger.ReleaseTrigger();
            trigger.ResetTrigger();
        }
    }
}
#endif