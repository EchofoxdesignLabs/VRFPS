// VRFPS/BloomFX.shader
// A simplified, high-quality bloom shader with proper downsampling and upsampling.

Shader "VRFPS/BloomFX"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        // Uniforms
        float4 _Params; // x: scatter, y: unused, z: threshold (linear), w: threshold knee
        TEXTURE2D_X(_SourceTexLowMip);

        // Macros for easy access
        #define Scatter         _Params.x
        #define Threshold       _Params.z
        #define ThresholdKnee   _Params.w

        // HDR encoding/decoding is required for bloom
        half4 EncodeHDR(half3 color)
        {
            return half4(max(0, color), 1.0);
        }

        half3 DecodeHDR(half4 data)
        {
            return data.xyz;
        }

        // Pass 0: Prefilter
        // Isolates bright pixels from the source image.
        half4 FragPrefilter(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
            half3 color = DecodeHDR(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv));

            half brightness = Max3(color.r, color.g, color.b);
            half softness = clamp(brightness - Threshold + ThresholdKnee, 0.0, 2.0 * ThresholdKnee);
            softness = (softness * softness) / (4.0 * ThresholdKnee + 1e-4);
            half multiplier = max(brightness - Threshold, softness) / max(brightness, 1e-4);
            color *= multiplier;
            
            return EncodeHDR(color);
        }

        // Pass 1: Downsample
        // Blurs and shrinks the image using a 4-tap tent filter for good quality.
        half4 FragDownsample(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
            float2 texelSize = _BlitTexture_TexelSize.xy;

            half3 c = 0;
            c += DecodeHDR(SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - texelSize, 0.0));
            c += DecodeHDR(SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + texelSize, 0.0));
            c += DecodeHDR(SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize.x, -texelSize.y), 0.0));
            c += DecodeHDR(SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(-texelSize.x, texelSize.y), 0.0));
            
            return EncodeHDR(c * 0.25);
        }

        // Pass 2: Upsample
        // Blends the lower-resolution blurred texture with the higher-resolution one.
        half4 FragUpsample(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
            
            // Texture from the current (higher-res) mip level
            half3 highMip = DecodeHDR(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv));
            
            // Texture from the previous (lower-res) mip level
            half3 lowMip = DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTexLowMip, sampler_LinearClamp, uv));

            // Blend based on the Scatter setting
            half3 color = lerp(highMip, lowMip, Scatter);
            return EncodeHDR(color);
        }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off

        // Pass 0
        Pass
        {
            Name "Bloom Prefilter"
            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragPrefilter
            ENDHLSL
        }

        // Pass 1
        Pass
        {
            Name "Bloom Downsample"
            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragDownsample
            ENDHLSL
        }

        // Pass 2
        Pass
        {
            Name "Bloom Upsample"
            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragUpsample
            ENDHLSL
        }
    }
}