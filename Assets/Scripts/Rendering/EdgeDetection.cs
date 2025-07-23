using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class EdgeDetection : ScriptableRendererFeature
{
    private class EdgeDetectionPass : ScriptableRenderPass
    {
        private Material material;

        private static readonly int OutlineThicknessProperty = Shader.PropertyToID("_OutlineThickness");
        private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");
        private static readonly int MaxDistanceProperty = Shader.PropertyToID("_MaxThicknessDistance");
        private static readonly int MinMultiplierProperty = Shader.PropertyToID("_MinThicknessMultiplier");
        private static readonly int NoiseTexProperty = Shader.PropertyToID("_NoiseTexture");
        private static readonly int NoiseScaleProperty = Shader.PropertyToID("_NoiseScale");
        private static readonly int NoiseStrengthProperty = Shader.PropertyToID("_NoiseStrength");

        public EdgeDetectionPass()
        {
            profilingSampler = new ProfilingSampler(nameof(EdgeDetectionPass));
        }

        public void Setup(ref EdgeDetectionSettings settings, ref Material edgeDetectionMaterial)
        {
            material = edgeDetectionMaterial;
            renderPassEvent = settings.renderPassEvent;

            material.SetFloat(OutlineThicknessProperty, settings.outlineThickness);
            material.SetColor(OutlineColorProperty, settings.outlineColor);
            material.SetFloat(MaxDistanceProperty, settings.maxThicknessDistance);
            material.SetFloat(MinMultiplierProperty, settings.minThicknessMultiplier);
            material.SetTexture(NoiseTexProperty, settings.noiseTexture);
            material.SetFloat(NoiseScaleProperty, settings.noiseScale);
            material.SetFloat(NoiseStrengthProperty, settings.noiseStrength);
        }

        private class PassData
        {
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            using var builder = renderGraph.AddRasterRenderPass<PassData>("Edge Detection", out _);

            builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
            builder.UseAllGlobalTextures(true);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc((PassData _, RasterGraphContext context) => { Blitter.BlitTexture(context.cmd, Vector2.one, material, 0); });
        }
    }

    [Serializable]
    public class EdgeDetectionSettings
    {
        [Header("General Settings")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Color outlineColor = Color.black;

        [Header("Thickness Settings")]
        [Range(0, 15)] public float outlineThickness = 3f;
        [Tooltip("Distance at which lines begin to get thinner.")]
        [Min(0)] public float maxThicknessDistance = 50f;
        [Tooltip("The minimum thickness multiplier for distant lines (e.g., 0.1 means 10% of original thickness).")]
        [Range(0.0f, 1.0f)] public float minThicknessMultiplier = 0.1f;

        [Header("Noise Settings")]
        [Tooltip("A seamless noise texture (like Perlin noise) for line distortion.")]
        public Texture2D noiseTexture;
        [Tooltip("Controls the tiling of the noise texture.")]
        [Min(0)] public float noiseScale = 10f;
        [Tooltip("Controls the intensity of the noise effect (line wobble and breakup).")]
        [Range(0.0f, 1.0f)] public float noiseStrength = 0.1f;
    }

    [SerializeField] private EdgeDetectionSettings settings;
    private Material edgeDetectionMaterial;
    private EdgeDetectionPass edgeDetectionPass;

    /// <summary>
    /// Called
    /// - When the Scriptable Renderer Feature loads the first time.
    /// - When you enable or disable the Scriptable Renderer Feature.
    /// - When you change a property in the Inspector window of the Renderer Feature.
    /// </summary>
    public override void Create()
    {
        edgeDetectionPass ??= new EdgeDetectionPass();
    }

    /// <summary>
    /// Called
    /// - Every frame, once for each camera.
    /// </summary>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Don't render for some views.
        if (renderingData.cameraData.cameraType == CameraType.Preview
            || renderingData.cameraData.cameraType == CameraType.Reflection
            || UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
            return;
        
        if (edgeDetectionMaterial == null)
        {
            edgeDetectionMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Edge Detection"));
            if (edgeDetectionMaterial == null)
            {
                Debug.LogWarning("Not all required materials could be created. Edge Detection will not render.");
                return;
            }
        }

        edgeDetectionPass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Color);
        edgeDetectionPass.requiresIntermediateTexture = true;
        edgeDetectionPass.Setup(ref settings, ref edgeDetectionMaterial);

        renderer.EnqueuePass(edgeDetectionPass);
    }

    /// <summary>
    /// Clean up resources allocated to the Scriptable Renderer Feature such as materials.
    /// </summary>
    override protected void Dispose(bool disposing)
    {
        edgeDetectionPass = null;
        CoreUtils.Destroy(edgeDetectionMaterial);
    }
}