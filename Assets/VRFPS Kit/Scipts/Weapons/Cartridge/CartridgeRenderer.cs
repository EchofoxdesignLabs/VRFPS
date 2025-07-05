using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRFPSKit
{
    /// <summary>
    /// Modifies render visibility status and renderer material to match with cartridgeToRender
    /// </summary>
    public class CartridgeRenderer : MonoBehaviour
    {
        public Cartridge cartridgeToRender;
        [Space] 
        public Renderer casingRenderer;
        public Renderer bulletRenderer;
        public Renderer emptyRenderer;
        [Space] 
        public BulletTypeMaterial[] bulletTypeMaterials;

        public void UpdateRenderer()
        {
            //Hide renders if we aren't supposed to render anything
            bool visibleModel = cartridgeToRender.caliber != Caliber.None;
            bool renderBullet = cartridgeToRender.bulletType != BulletType.Empty_Casing;
            
            if(casingRenderer) casingRenderer.enabled = visibleModel;
            if(bulletRenderer) bulletRenderer.enabled = visibleModel && renderBullet;
            if(emptyRenderer) emptyRenderer.enabled = visibleModel && !renderBullet;

            Material bulletMaterial = GetBulletMaterial();
            if(bulletMaterial != null)
                bulletRenderer.material = bulletMaterial;
        }

        private Material GetBulletMaterial()
        {
            foreach (var bulletMaterial in bulletTypeMaterials)
                //Return bullet material if active bullet type matches
                if (cartridgeToRender.bulletType == bulletMaterial.bulletType)
                    return bulletMaterial.bulletMaterial;

            return null;
        }
        
        private void OnEnable()
        {
            UpdateRenderer();
        }
    }
    
    [Serializable]
    public struct BulletTypeMaterial
    {
        public BulletType bulletType;
        public Material bulletMaterial;
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Add a simple inspector button for manually updating cartridge renderer
    /// </summary>
    [CustomEditor(typeof(CartridgeRenderer))]
    public class CartridgeRendererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector layout
            DrawDefaultInspector();

            // Reference to the target script
            CartridgeRenderer rendererScript = (CartridgeRenderer)target;
            
            // Disable the button when not in Play Mode
            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("Update Renderer Manually"))
                rendererScript.UpdateRenderer();

            // Re-enable GUI to avoid affecting other elements
            GUI.enabled = true;
        }
    }
    #endif
}