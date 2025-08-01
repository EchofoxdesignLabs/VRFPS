// BenDayBloom.cs

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Experimental.Rendering;

public class BenDayBloom : ScriptableRendererFeature
{
    [System.Serializable]
    public class BenDayBloomSettings
    {
        [Header("Bloom Settings")]
        [Tooltip("Filters out pixels under this brightness threshold.")]
        [Min(0f)] public float threshold = 0.9f;
        [Tooltip("Strength of the bloom glow.")]
        [Min(0f)] public float intensity = 0.75f;
        [Tooltip("How much the bloom spreads. 0 = sharp, 1 = soft.")]
        [Range(0f, 1f)] public float scatter = 0.7f;
        [Tooltip("A color to tint the bloom and the dots.")]
        public Color bloomTint = Color.white; // New property

        [Header("Ben Day Dot Settings")]
        [Tooltip("Controls how many dots appear on screen. Higher values mean smaller, denser dots.")]
        [Min(1.0f)] public float dotDensity = 150f; // Replaces Dot Scale

        [Tooltip("The angle of the dot grid. 45 degrees is a classic look.")]
        [Range(0f, 90f)] public float patternAngle = 45f; // New property
        [Tooltip("Controls the power of the vignette falloff.")]
        [Range(0.1f, 4f)] public float vignettePower = 1.0f; //
        [Tooltip("Controls the falloff of the dots. Higher values make the dots sharper.")]
        [Range(0.01f, 2.0f)] public float dotHardness = 0.5f;

        [Tooltip("The minimum dot size for the darkest areas of the bloom.")]
        [Range(0.0f, 1.0f)] public float minDotSize = 0.1f;
    }
    [SerializeField] private BenDayBloomSettings settings = new BenDayBloomSettings();
    private BenDayBloomPass m_BenDayPass;

    public override void Create()
    {
        m_BenDayPass = new BenDayBloomPass();
        name = "Ben Day Bloom";
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Preview || renderingData.cameraData.cameraType == CameraType.Reflection)
            return;

        m_BenDayPass.Setup(settings);
        renderer.EnqueuePass(m_BenDayPass);
    }
    protected override void Dispose(bool disposing)
    {
        m_BenDayPass.Dispose();
    }
    private class BenDayBloomPass : ScriptableRenderPass
    {
        private BenDayBloomSettings m_Settings;
        private Material m_BloomMaterial;
        private Material m_BenDayMaterial;

        // Property IDs for shader variables
        private static readonly int BloomParamsID = Shader.PropertyToID("_Params");
        private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
        private static readonly int BloomTintID = Shader.PropertyToID("_BloomTint");
        private static readonly int VignettePowerID = Shader.PropertyToID("_VignettePower");
        private static readonly int DotDensityID = Shader.PropertyToID("_DotDensity");
        private static readonly int PatternAngleID = Shader.PropertyToID("_PatternAngle");
        private static readonly int DotHardnessID = Shader.PropertyToID("_DotHardness");
        private static readonly int MinDotSizeID = Shader.PropertyToID("_MinDotSize");
        private static readonly int LowMipTexID = Shader.PropertyToID("_SourceTexLowMip");
        // FIX: Add a property ID for our custom texel size
        private static readonly int CustomTexelSizeID = Shader.PropertyToID("_CustomTexelSize");
        public BenDayBloomPass()
        {
            profilingSampler = new ProfilingSampler("BenDayBloomEffect");
        }
        public void Setup(BenDayBloomSettings settings)
        {
            m_Settings = settings;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }
        public void Dispose()
        {
            CoreUtils.Destroy(m_BloomMaterial);
            CoreUtils.Destroy(m_BenDayMaterial);
        }
        private void CreateMaterials()
        {
            if (m_BloomMaterial == null)
            {
                var shader = Shader.Find("VRFPS/BloomFX");
                if (shader != null) m_BloomMaterial = new Material(shader);
            }
            if (m_BenDayMaterial == null)
            {
                var shader = Shader.Find("Hidden/BenDayDots");
                if (shader != null) m_BenDayMaterial = new Material(shader);
            }
        }
        private void UpdateMaterialProperties()
        {
            if (m_BloomMaterial != null)
            {
                float threshold = Mathf.GammaToLinearSpace(m_Settings.threshold);
                float thresholdKnee = threshold * 0.5f;
                m_BloomMaterial.SetVector(BloomParamsID, new Vector4(m_Settings.scatter, 0, threshold, thresholdKnee));
            }
            if (m_BenDayMaterial != null)
            {
                m_BenDayMaterial.SetFloat(IntensityID, m_Settings.intensity);
                m_BenDayMaterial.SetColor(BloomTintID, m_Settings.bloomTint);
                m_BenDayMaterial.SetFloat(VignettePowerID, m_Settings.vignettePower);
                m_BenDayMaterial.SetFloat(DotDensityID, m_Settings.dotDensity);
                m_BenDayMaterial.SetFloat(PatternAngleID, m_Settings.patternAngle);
                m_BenDayMaterial.SetFloat(DotHardnessID, m_Settings.dotHardness);
                m_BenDayMaterial.SetFloat(MinDotSizeID, m_Settings.minDotSize);

            }
        }
        private class BlitPassData { public Material material; public TextureHandle source; }
        private class UpsamplePassData { public Material material; public TextureHandle source; public TextureHandle lowMip; }
        private class CopyPassData { public TextureHandle source; }
        private class CompositePassData
        {
            public Material material;
            public TextureHandle sourceColor;
            public TextureHandle sourceBloom;
            public float intensity;
        }
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            CreateMaterials();
            if (m_BloomMaterial == null || m_BenDayMaterial == null || m_Settings == null) return;
            UpdateMaterialProperties();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // --- 1. Prefilter Pass ---
            var prefilterDescriptor = cameraData.cameraTargetDescriptor;
            prefilterDescriptor.width /= 2;
            prefilterDescriptor.height /= 2;
            prefilterDescriptor.graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
            prefilterDescriptor.depthBufferBits = (int)DepthBits.None;
            var sourceDescriptor = cameraData.cameraTargetDescriptor;
            Vector4 texelSize = new Vector4(1.0f / sourceDescriptor.width, 1.0f / sourceDescriptor.height, sourceDescriptor.width, sourceDescriptor.height);
            m_BloomMaterial.SetVector(CustomTexelSizeID, texelSize);
            TextureHandle prefilterTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, prefilterDescriptor, "_prefilter", false);
            using (var builder = renderGraph.AddRasterRenderPass<BlitPassData>("Bloom Prefilter", out var passData))
            {
                passData.material = m_BloomMaterial;
                passData.source = resourceData.activeColorTexture;
                builder.UseTexture(passData.source, AccessFlags.Read);
                builder.SetRenderAttachment(prefilterTexture, 0);
                builder.SetRenderFunc((BlitPassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, data.source, Vector2.one, data.material, 0));
            }
            // --- 2. Downsample Passes ---
            const int kMaxIterations = 5;
            var downsampleTextures = new TextureHandle[kMaxIterations];
            downsampleTextures[0] = prefilterTexture;
            var desc = prefilterDescriptor;
            for (int i = 1; i < kMaxIterations; i++)
            {

                desc.width = Mathf.Max(1, desc.width / 2);
                desc.height = Mathf.Max(1, desc.height / 2);
                downsampleTextures[i] = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, $"_prefilter{i}", false);
                using (var builder = renderGraph.AddRasterRenderPass<BlitPassData>($"Bloom Downsample {i}", out var passData))
                {
                    passData.material = m_BloomMaterial;
                    passData.source = downsampleTextures[i - 1];
                    builder.UseTexture(passData.source, AccessFlags.Read);
                    builder.SetRenderAttachment(downsampleTextures[i], 0);
                    builder.SetRenderFunc((BlitPassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, data.source, Vector2.one, data.material, 1));
                }
            }
            // --- 3. Upsample Passes ---
            for (int i = kMaxIterations - 2; i >= 0; i--)
            {
                using (var builder = renderGraph.AddRasterRenderPass<UpsamplePassData>($"Bloom Upsample {i}", out var passData))
                {
                    passData.material = m_BloomMaterial;
                    passData.source = downsampleTextures[i]; // This is the destination (high mip)
                    passData.lowMip = downsampleTextures[i + 1];
                    builder.UseTexture(passData.lowMip, AccessFlags.Read);
                    builder.SetRenderAttachment(passData.source, 0);
                    // FIX: Allow this pass to modify global shader properties.
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((UpsamplePassData data, RasterGraphContext context) =>
                    {
                        context.cmd.SetGlobalTexture(LowMipTexID, data.lowMip);
                        Blitter.BlitTexture(context.cmd, data.source, Vector2.one, data.material, 2);
                    });
                }
            }
            // --- 4. Final Composite Pass ---
            var finalBloomTexture = downsampleTextures[0];
            var colorCopyDescriptor = cameraData.cameraTargetDescriptor;
            colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;
            TextureHandle copiedColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, colorCopyDescriptor, "_copiedColor", false);
            using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("Copy Color", out var passData))
            {
                passData.source = resourceData.activeColorTexture;
                builder.UseTexture(passData.source, AccessFlags.Read);
                builder.SetRenderAttachment(copiedColor, 0);
                builder.SetRenderFunc((CopyPassData data, RasterGraphContext context) => Blitter.BlitTexture(context.cmd, data.source, Vector2.one, 0.0f, false));
            }
            using (var builder = renderGraph.AddRasterRenderPass<CompositePassData>("Ben Day Bloom Composite", out var passData))
            {
                passData.material = m_BenDayMaterial;
                passData.sourceColor = copiedColor;
                passData.sourceBloom = finalBloomTexture;
                passData.intensity = m_Settings.intensity;

                builder.UseTexture(passData.sourceColor, AccessFlags.Read);
                builder.UseTexture(passData.sourceBloom, AccessFlags.Read);
                // FIX: Allow this pass to modify global shader properties as well.
                builder.AllowGlobalStateModification(true);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                builder.SetRenderFunc((CompositePassData data, RasterGraphContext context) =>
                {
                    context.cmd.SetGlobalTexture("_BloomTexture", data.sourceBloom);
                    data.material.SetFloat("_Intensity", data.intensity);
                    Blitter.BlitTexture(context.cmd, data.sourceColor, Vector2.one, data.material, 0);
                });
            }
        }
    }

    
}