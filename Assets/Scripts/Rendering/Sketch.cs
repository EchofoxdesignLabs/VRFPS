namespace VRDefender.Rendering
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
    using UnityEngine.Rendering.RenderGraphModule;
    using System.Reflection;
    using System.Text;
    using UnityEngine.Rendering.RenderGraphModule.Util;
#endif

    public class Sketch : ScriptableRendererFeature
    {
        SketchRenderPass sketchPass;
        private FieldInfo _screenSpaceShadowmapTextureField;
        private const string FIELD_NAME = "m_ScreenSpaceShadowmapTexture";
        
        

        public override void Create()
        {
            sketchPass = new SketchRenderPass();
            name = "Sketch";
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Preview
            || renderingData.cameraData.cameraType == CameraType.Reflection
            || UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
            return;
            sketchPass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Color);
            var settings = VolumeManager.instance.stack.GetComponent<SketchSettings>();

            if (settings != null && settings.IsActive())
            {
                renderer.EnqueuePass(sketchPass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            sketchPass.Dispose();
            base.Dispose(disposing);
        }

        class SketchRenderPass : ScriptableRenderPass
        {
            private Material m_sketchmaterial;
            private Material m_grabmaterial;
            private SketchSettings m_Settings;
            private RTHandle tempTexHandle;
            private RTHandle sourceShadowmap;
            private RTHandle shadowmapHandle1;
            private RTHandle shadowmapHandle2;
            private Material m_shadowMaterial;
            // Get a static Property ID for the global texture we need to read
            private static readonly int s_ScreenSpaceShadowmapTextureID = Shader.PropertyToID("_ScreenSpaceShadowmapTexture");

            public SketchRenderPass()
            {
                profilingSampler = new ProfilingSampler("SketchEffect");
                renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

#if UNITY_6000_0_OR_NEWER
                requiresIntermediateTexture = true;
#endif
            }
            // Method to receive the shadowmap handle from the feature
            public void SetSource(RTHandle source)
            {
                this.sourceShadowmap = source;
            }

            private void CreateMaterial()
            {
                if (m_sketchmaterial == null)
                {
                    var shader = Shader.Find("VRDefender/Sketch");
                    if (shader != null) m_sketchmaterial = new Material(shader);
                }
                // if (m_grabmaterial == null)
                // {
                //     var shader = Shader.Find("Hidden/VRDefender/GrabShadows");
                //     if (shader != null) m_grabmaterial = new Material(shader);
                // }
                if (m_shadowMaterial == null)
                {
                    // Load our new, VR-compatible screen space shadow shader
                    var shader = Shader.Find("Hidden/VR_ScreenSpaceShadows");
                    if (shader != null) m_shadowMaterial = new Material(shader);
                }
            }

            public void Dispose()
            {
                tempTexHandle?.Release();
                shadowmapHandle1?.Release();
                shadowmapHandle2?.Release();
                CoreUtils.Destroy(m_sketchmaterial);
                CoreUtils.Destroy(m_grabmaterial);
                CoreUtils.Destroy(m_shadowMaterial);
            }

#if UNITY_6000_0_OR_NEWER

            // private class CopyPassData
            // {
            //     public TextureHandle inputTexture;
            // }

            // private class MainPassData
            // {
            //     public Material material;
            //     public TextureHandle inputTexture;
            // }
            private class BlurPassData { public Material material; public TextureHandle source; }
            private class MainPassData { public Material material; public TextureHandle sourceColor; public TextureHandle sourceShadow; }
            private class GrabPassData { public Material material; }
            private class CopyPassData { public TextureHandle source; }
            private class PassData { public Material material; public TextureHandle source; }
            private void UpdateMaterialProperties()
            {
                if (m_sketchmaterial == null || m_Settings == null) return;
                m_sketchmaterial.SetTexture("_SketchTexture", m_Settings.sketchTexture.value);
                m_sketchmaterial.SetColor("_SketchColor", m_Settings.sketchColor.value);
                m_sketchmaterial.SetVector("_SketchTiling", m_Settings.sketchTiling.value);
                m_sketchmaterial.SetVector("_SketchThresholds", m_Settings.sketchThresholds.value);
                m_sketchmaterial.SetFloat("_DepthSensitivity", m_Settings.extendDepthSensitivity.value);
                m_sketchmaterial.SetFloat("_CrossHatching", m_Settings.crossHatching.value ? 1.0f : 0.0f);
                m_sketchmaterial.SetInt("_KernelSize", m_Settings.blurAmount.value);
                m_sketchmaterial.SetFloat("_Spread", m_Settings.blurAmount.value / 6.0f);
                m_sketchmaterial.SetInt("_BlurStepSize", m_Settings.blurStepSize.value);
            }


            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if(m_sketchmaterial == null)
                {
                    CreateMaterial();
                }
                m_Settings = VolumeManager.instance.stack.GetComponent<SketchSettings>();
                if (m_sketchmaterial == null || m_shadowMaterial == null || m_Settings == null || !m_Settings.IsActive()) return;
                UpdateMaterialProperties();

                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                
                var shadowDescriptor = cameraData.cameraTargetDescriptor;
                shadowDescriptor.colorFormat = RenderTextureFormat.ARGB32;
                shadowDescriptor.depthBufferBits = (int)DepthBits.None;
                if (cameraData.xr.enabled)
                {
                    shadowDescriptor.dimension = TextureDimension.Tex2DArray;
                    shadowDescriptor.volumeDepth = 2; // 2 slices for 2 eyes
                }
                // --- FIX: Calculate and set the texel size for the blur passes ---
                Vector4 texelSize = new Vector4(1.0f / shadowDescriptor.width, 1.0f / shadowDescriptor.height, shadowDescriptor.width, shadowDescriptor.height);
                m_sketchmaterial.SetVector("_CustomTexelSize", texelSize);
                // --- END FIX ---
                var calculatedShadowMap = UniversalRenderer.CreateRenderGraphTexture(renderGraph, shadowDescriptor, "_CalculatedShadowMap", false);
                // --- 1. Calculate the shadow map using our custom VR shader ---
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Calculate VR Shadows", out var passData))
                {
                    passData.material = m_shadowMaterial;
                    builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                    builder.SetRenderAttachment(calculatedShadowMap, 0);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, (Texture)null, Vector2.one, data.material, 0));
                }

                    // --- Step 1: The "Grab" Pass ---
                // Create a texture handle that our grab pass will write into.
                // TextureHandle grabbedShadowMap = UniversalRenderer.CreateRenderGraphTexture(renderGraph, shadowDescriptor, "_ShadowMap_Copy", false);
                // using (var builder = renderGraph.AddRasterRenderPass<GrabPassData>("Grab Shadow Map", out var passData))
                // {
                //     passData.material = m_grabmaterial;
                //     // FIX: Explicitly declare that this pass reads the global screen space shadow map.
                //     // This forces the Render Graph to wait until the shadow map is generated before running this pass.
                //     builder.UseGlobalTexture(s_ScreenSpaceShadowmapTextureID, AccessFlags.Read);
                //     builder.SetRenderAttachment(grabbedShadowMap, 0);
                    
                //     // This pass will read the global _ScreenSpaceShadowmapTexture and output it to our 'grabbedShadowMap' handle.
                //     builder.SetRenderFunc((GrabPassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, (Texture)null, Vector2.one, data.material, 0));
                // }

                // --- From here, we use our 'grabbedShadowMap' handle ---
                var colorCopyDescriptor = cameraData.cameraTargetDescriptor;
                // FIX: The camera descriptor includes depth format. We must remove it for a color texture.
                colorCopyDescriptor.depthBufferBits = (int)DepthBits.None; 
                if (cameraData.xr.enabled)
                {
                    colorCopyDescriptor.dimension = TextureDimension.Tex2DArray;
                    colorCopyDescriptor.volumeDepth = 2;
                }
                TextureHandle blurredShadowMap2 = UniversalRenderer.CreateRenderGraphTexture(renderGraph, shadowDescriptor, "_blurredShadowMap2", false);
                TextureHandle copiedColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, colorCopyDescriptor, "_SketchColorCopy", false);

                // --- Step 2: Blur the shadow map ---
                if (m_Settings.blurAmount.value > m_Settings.blurStepSize.value * 2)
                {
                    // Horizontal Blur (reads from grabbed, writes to blur2)
                    using (var builder = renderGraph.AddRasterRenderPass<BlurPassData>("Horizontal Shadow Blur", out var passData))
                    {
                        passData.material = m_sketchmaterial;
                        passData.source = calculatedShadowMap;
                        builder.UseTexture(passData.source, AccessFlags.Read);
                        // FIX: Declare that this pass needs to read the depth texture.
                        builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                        builder.SetRenderAttachment(blurredShadowMap2, 0);
                        builder.SetRenderFunc((BlurPassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, data.source, Vector2.one, data.material, 1));
                    }
                    // Vertical Blur (reads from blur2, writes back to grabbed)
                    using (var builder = renderGraph.AddRasterRenderPass<BlurPassData>("Vertical Shadow Blur", out var passData))
                    {
                        passData.material = m_sketchmaterial;
                        passData.source = blurredShadowMap2;
                        builder.UseTexture(passData.source, AccessFlags.Read);
                        builder.SetRenderAttachment(calculatedShadowMap, 0);
                        builder.SetRenderFunc((BlurPassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, data.source, Vector2.one, data.material, 2));
                    }
                }
                // --- Step 3: Composite the final image ---
                // --- 3. Copy the Camera Color (manual pass to avoid API errors) ---
                using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("Copy Color", out var passData))
                {
                    passData.source = resourceData.activeColorTexture;
                    builder.UseTexture(passData.source, AccessFlags.Read); // Corrected enum
                    builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                    builder.SetRenderAttachment(copiedColor, 0);
                    builder.SetRenderFunc((CopyPassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0.0f, false));
                }
                //renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(resourceData.activeColorTexture, copiedColor));
                using (var builder = renderGraph.AddRasterRenderPass<MainPassData>("Sketch Main Pass", out var passData))
                {
                    passData.material = m_sketchmaterial;
                    passData.sourceColor = copiedColor;
                    passData.sourceShadow = calculatedShadowMap; // This is now our final, blurred shadow map

                    builder.UseTexture(passData.sourceColor, AccessFlags.Read);
                    builder.UseTexture(passData.sourceShadow, AccessFlags.Read);
                    // FIX: The final pass also needs depth and normals for triplanar mapping.
                    builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                    builder.UseTexture(resourceData.cameraNormalsTexture, AccessFlags.Read);
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                    builder.SetRenderFunc((MainPassData data, RasterGraphContext context) =>
                    {
                        data.material.SetTexture("_ShadowmapTexture", data.sourceShadow);
                        Blitter.BlitTexture(context.cmd, data.sourceColor, Vector2.one, data.material, 0);
                    });
                }
            }

#endif
        }
    }
}