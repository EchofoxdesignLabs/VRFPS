using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CustomPostProcessPass : ScriptableRenderPass
{
    private Material m_bloomMaterial;
    private Material m_compositeMaterial;
    private BendayBloomEffectComponent m_BloomEffect;
    private RTHandle m_CameraColorTarget;
    private RTHandle m_CameraDepthTarget;
    //private RenderTextureDescriptor m_Descriptor;
    RenderTextureDescriptor m_Descriptor;

    const int k_MaxPyramidSize = 16;
    private int[] _BloomMipUp;
    private int[] _BloomMipDown;
    private RTHandle[] m_BloomMipDown;
    private RTHandle[] m_BloomMipUp;
    private GraphicsFormat hdrFormat;
    private const string k_PassName = "BenDayDotsEffect";


    public CustomPostProcessPass(Material bloomMaterial, Material compositeMaterial)
    {
        m_bloomMaterial = bloomMaterial;
        m_compositeMaterial = compositeMaterial;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        requiresIntermediateTexture = true;

        _BloomMipUp = new int[k_MaxPyramidSize];
        _BloomMipDown = new int[k_MaxPyramidSize];
        m_BloomMipUp = new RTHandle[k_MaxPyramidSize];
        m_BloomMipDown = new RTHandle[k_MaxPyramidSize];

        for (int i = 0; i < k_MaxPyramidSize; i++)
        {
            _BloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
            _BloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
            // Get name, will get Allocated with descriptor later
            m_BloomMipUp[i] = RTHandles.Alloc(_BloomMipUp[i], name: "_BloomMipUp" + i);
            m_BloomMipDown[i] = RTHandles.Alloc(_BloomMipDown[i], name: "_BloomMipDown" + i);
        }



        // Texture format pre-lookup
        // UUM-41070: We require `Linear | Render` but with the deprecated FormatUsage this was checking `Blend`
        // For now, we keep checking for `Blend` until the performance hit of doing the correct checks is evaluated

        const GraphicsFormatUsage usage = GraphicsFormatUsage.Linear | GraphicsFormatUsage.Render;
        if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage))    // Typically, RGBA16Float.
        {
            hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        }
        else if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage)) // HDR fallback
        {
            // NOTE: Technically request format can be with alpha, however if it's not supported and we fall back here
            // , we assume no alpha. Post-process default format follows the back buffer format.
            // If support failed, it must have failed for back buffer too.
            hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
            //m_DefaultColorFormatIsAlpha = false;
        }
        else
        {
            hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                ? GraphicsFormat.R8G8B8A8_SRGB
                : GraphicsFormat.R8G8B8A8_UNorm;
        }
    }
    public override void RecordRenderGraph(RenderGraph renderGraph,
    ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        // The following line ensures that the render pass doesn't blit
        // from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
            return;
        VolumeStack stack = VolumeManager.instance.stack;
        m_BloomEffect = stack.GetComponent<BendayBloomEffectComponent>();
        TextureHandle source = resourceData.activeColorTexture;
        m_Descriptor = cameraData.cameraTargetDescriptor;
        var destinationDesc = renderGraph.GetTextureDesc(source);
        destinationDesc.name = $"CameraColor-{k_PassName}";
        destinationDesc.clearBuffer = false;
        destinationDesc.colorFormat = hdrFormat;

        // Compute mip count
        int width = destinationDesc.width >> 1;
        int height = destinationDesc.height >> 1;
        int maxSize = Mathf.Max(width, height);
        int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f));
        int mipCount = Mathf.Clamp(iterations, 1, m_BloomEffect.maxIterations.value);

        // Create mip textures
        TextureHandle[] mipDown = new TextureHandle[mipCount];
        for (int i = 0; i < mipCount; i++)
        {
            var mipDesc = destinationDesc;
            mipDesc.width = Mathf.Max(1, width >> i);
            mipDesc.height = Mathf.Max(1, height >> i);
            mipDesc.name = "BloomMip" + i;
            mipDown[i] = renderGraph.CreateTexture(mipDesc);
        }

        // Prefilter + downsample cascade
        // Setup material parameters
        float threshold = Mathf.GammaToLinearSpace(m_BloomEffect.threshold.value);
        float knee = threshold * 0.5f;
        float scatter = Mathf.Lerp(0.05f, 0.95f, m_BloomEffect.scatter.value);
        m_bloomMaterial.SetVector("_Params", new Vector4(scatter, m_BloomEffect.clamp.value, threshold, knee));
        // m_bloomMaterial.SetInt("_DotsDensity", m_BloomEffect.dotsDensity.value);
        // m_bloomMaterial.SetFloat("_DotsCutoff", m_BloomEffect.dotsCutoff.value);
        // m_bloomMaterial.SetVector("_ScrollDir", m_BloomEffect.scrollDirection.value);
        // Prefilter into first mip
        var prefilterParams = new RenderGraphUtils.BlitMaterialParameters(source, mipDown[0], m_bloomMaterial, 0);
        renderGraph.AddBlitPass(prefilterParams, k_PassName + " Prefilter");

        // Blur down
        for (int i = 1; i < mipCount; i++)
        {
            var downParams = new RenderGraphUtils.BlitMaterialParameters(mipDown[i - 1], mipDown[i], m_bloomMaterial, 1);
            renderGraph.AddBlitPass(downParams, k_PassName + $" BlurDown {i}");
            var blurVParams = new RenderGraphUtils.BlitMaterialParameters(mipDown[i], mipDown[i], m_bloomMaterial, 2);
            renderGraph.AddBlitPass(blurVParams, k_PassName + $" BlurVertical {i}");
        }

        // Upsample + combine
        TextureHandle last = mipDown[mipCount - 1];
        for (int i = mipCount - 2; i >= 0; i--)
        {
            m_bloomMaterial.SetTexture("_SourceTexLowMip", last);
            var upParams = new RenderGraphUtils.BlitMaterialParameters(mipDown[i], mipDown[i], m_bloomMaterial, 3);
            renderGraph.AddBlitPass(upParams, k_PassName + $" Upsample {i}");
            last = mipDown[i];
        }
        // Composite back
        // m_compositeMaterial.SetFloat("_BloomIntensity", m_BloomEffect.intensity.value);
        // var compParams = new RenderGraphUtils.BlitMaterialParameters(last, source, m_compositeMaterial, 0);
        // renderGraph.AddBlitPass(compParams, k_PassName + " Composite");
        resourceData.cameraColor = last;
    }
    


    // private void SetupBloom(CommandBuffer cmd, RTHandle source)
    // {
    //     int downres = 1;
    //     int tw = m_Descriptor.width >> downres;
    //     int th = m_Descriptor.height >> downres;

    //     // Determine the iteration count
    //     int maxSize = Mathf.Max(tw, th);
    //     int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1);
    //     int mipCount = Mathf.Clamp(iterations, 1, m_BloomEffect.maxIterations.value);

    //     // Pre-filtering parameters
    //     float clamp = m_BloomEffect.clamp.value;
    //     float threshold = Mathf.GammaToLinearSpace(m_BloomEffect.threshold.value);
    //     float thresholdKnee = threshold * 0.5f; // Hardcoded soft knee

    //     // Material setup
    //     float scatter = Mathf.Lerp(0.05f, 0.95f, m_BloomEffect.scatter.value);
    //     var bloomMaterial = m_bloomMaterial;
    //     bloomMaterial.SetVector("_Params", new Vector4(scatter, clamp, threshold, thresholdKnee));

    //     // Prefilter
    //     var desc = GetCompatibleDescriptor(tw, th, hdrFormat);
    //     for (int i = 0; i < mipCount; i++)
    //     {
    //         RenderingUtils.ReAllocateHandleIfNeeded(ref m_BloomMipUp[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: m_BloomMipUp[i].name);
    //         RenderingUtils.ReAllocateHandleIfNeeded(ref m_BloomMipDown[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: m_BloomMipDown[i].name);
    //         desc.width = Mathf.Max(1, desc.width >> 1);
    //         desc.height = Mathf.Max(1, desc.height >> 1);
    //     }
    //     //Blitter.BlitCameraTexture(cmd, source, m_BloomMipDown[0], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 0);
    //     // Downsample - gaussian pyramid
    //     var lastDown = m_BloomMipDown[0];

    //     for (int i = 1; i < mipCount; i++)
    //     {

    //         // Classic two pass gaussian blur - use mipUp as a temporary target
    //         //   First pass does 2x downsampling + 9-tap gaussian
    //         //   Second pass does 9-tap gaussian using a 5-tap filter + bilinear filtering

    //         Blitter.BlitCameraTexture(cmd, lastDown, m_BloomMipUp[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 1);
    //         Blitter.BlitCameraTexture(cmd, m_BloomMipUp[i], m_BloomMipDown[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 2);

    //         lastDown = m_BloomMipDown[i];
    //     }

    //     // Upsample (bilinear by default, HQ filtering does bicubic instead
    //     for (int i = mipCount - 2; i >= 0; i--)
    //     {
    //         var lowMip = (i == mipCount - 2) ? m_BloomMipDown[i + 1] : m_BloomMipUp[i + 1];
    //         var highMip = m_BloomMipDown[i];
    //         var dst = m_BloomMipUp[i];

    //         cmd.SetGlobalTexture("_SourceTexLowMip", lowMip);
    //         Blitter.BlitCameraTexture(cmd, highMip, dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 3);
    //     }

    //     cmd.SetGlobalTexture("_Bloom_Texture", m_BloomMipUp[0]);
    //     cmd.SetGlobalFloat("_BloomIntensity", m_BloomEffect.intensity.value);
    // }

    // RenderTextureDescriptor GetCompatibleDescriptor()
    //         => GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);

    // RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format, GraphicsFormat depthStencilFormat = GraphicsFormat.None)
    //     => GetCompatibleDescriptor(m_Descriptor, width, height, format, depthStencilFormat);
    // internal static RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor desc, int width, int height, GraphicsFormat format, GraphicsFormat depthStencilFormat = GraphicsFormat.None)
    // {
    //     desc.depthStencilFormat = depthStencilFormat;
    //     desc.msaaSamples = 1;
    //     desc.width = width;
    //     desc.height = height;
    //     desc.graphicsFormat = format;
    //     return desc;
    // }
}
