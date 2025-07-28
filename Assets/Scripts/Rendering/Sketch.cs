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
                if (m_grabmaterial == null)
                {
                    var shader = Shader.Find("Hidden/VRDefender/GrabShadows");
                    if (shader != null) m_grabmaterial = new Material(shader);
                }
            }

            public void Dispose()
            {
                tempTexHandle?.Release();
                shadowmapHandle1?.Release();
                shadowmapHandle2?.Release();
                CoreUtils.Destroy(m_sketchmaterial);
                CoreUtils.Destroy(m_grabmaterial);
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

            private static void ExecuteCopyPass(RasterCommandBuffer cmd, RTHandle source)
            {
                Blitter.BlitTexture(cmd, source, new Vector4(1, 1, 0, 0), 0.0f, false);
            }

            private static void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle source, Material material)
            {
                // Set Sketch effect properties.
                
                // material.SetTexture("_SketchTexture", settings.sketchTexture.value);
                // material.SetColor("_SketchColor", settings.sketchColor.value);
                // material.SetVector("_SketchTiling", settings.sketchTiling.value);
                // material.SetVector("_SketchThresholds", settings.sketchThresholds.value);
                // material.SetFloat("_DepthSensitivity", settings.extendDepthSensitivity.value);
                // material.SetFloat("_CrossHatching", settings.crossHatching.value ? 1 : 0);

                // material.SetInt("_KernelSize", settings.blurAmount.value);
                // material.SetFloat("_Spread", settings.blurAmount.value / 6.0f);
                // material.SetInt("_BlurStepSize", settings.blurStepSize.value);

                Blitter.BlitTexture(cmd, source, new Vector4(1, 1, 0, 0), material, 0);
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if(m_sketchmaterial == null)
                {
                    CreateMaterial();
                }
                m_Settings = VolumeManager.instance.stack.GetComponent<SketchSettings>();
                if (m_sketchmaterial == null || m_grabmaterial == null || m_Settings == null || !m_Settings.IsActive()) return;
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
                // RenderGraphUtils.BlitMaterialParameters para = new(resourceData.cameraDepthTexture, resourceData.activeColorTexture, m_Material, 0);
                    // // para.sourceTexturePropertyID = Shader.PropertyToID(m_TextureName);
                    // renderGraph.AddBlitPass(para, passName: "Blit Selected Resource");   

                    // --- Step 1: The "Grab" Pass ---
                    // Create a texture handle that our grab pass will write into.
                    TextureHandle grabbedShadowMap = UniversalRenderer.CreateRenderGraphTexture(renderGraph, shadowDescriptor, "_ShadowMap_Copy", false);
                using (var builder = renderGraph.AddRasterRenderPass<GrabPassData>("Grab Shadow Map", out var passData))
                {
                    passData.material = m_grabmaterial;
                    builder.SetRenderAttachment(grabbedShadowMap, 0);
                    
                    // This pass will read the global _ScreenSpaceShadowmapTexture and output it to our 'grabbedShadowMap' handle.
                    builder.SetRenderFunc((GrabPassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, (Texture)null, Vector2.one, data.material, 0));
                }

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
                        passData.source = grabbedShadowMap;
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
                        builder.SetRenderAttachment(grabbedShadowMap, 0);
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
                    passData.sourceShadow = grabbedShadowMap; // This is now our final, blurred shadow map

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