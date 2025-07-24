// Shaders/Hidden/CustomBloom.shader

Shader "Hidden/CustomBloom"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        // This texture will be used for upsampling
        TEXTURE2D_X(_SourceTexLowMip);
        float4 _SourceTexLowMip_TexelSize;

        // x: scatter, y: clamp, z: threshold (linear), w: threshold knee
        float4 _Params;
        #define Scatter         _Params.x
        #define ClampMax        _Params.y
        #define Threshold       _Params.z
        #define ThresholdKnee   _Params.w

        half4 EncodeHDR(half3 color)
        {
        #if UNITY_COLORSPACE_GAMMA
            color = sqrt(color); // linear to γ
        #endif
            return half4(color, 1.0);
        }

        half3 DecodeHDR(half4 data)
        {
            half3 color = data.xyz;
        #if UNITY_COLORSPACE_GAMMA
            color *= color; // γ to linear
        #endif
            return color;
        }

        // --- Pass 0: Prefilter ---
        half4 FragPrefilter(Varyings input) : SV_Target
        {
            float2 uv = input.texcoord;
            half3 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;

            // Clamp to limit crazy high broken spec
            color = min(ClampMax, color);

            // Thresholding
            half brightness = Max3(color.r, color.g, color.b);
            half softness = clamp(brightness - Threshold + ThresholdKnee, 0.0, 2.0 * ThresholdKnee);
            softness = (softness * softness) / (4.0 * ThresholdKnee + 1e-4);
            half multiplier = max(brightness - Threshold, softness) / max(brightness, 1e-4);
            color *= multiplier;

            color = max(color, 0);
            return EncodeHDR(color);
        }

        // --- Pass 1: Downsample/Blur ---
        // This single fragment shader can be used for both horizontal and vertical blurs
        half4 FragBlur(Varyings input) : SV_Target
        {
            float2 texelSize = _BlitTexture_TexelSize.xy;
            float2 uv = input.texcoord;

            // 5-tap bilinear gaussian blur
            half3 c0 = DecodeHDR(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - texelSize * 2.0));
            half3 c1 = DecodeHDR(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - texelSize * 1.0));
            half3 c2 = DecodeHDR(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv));
            half3 c3 = DecodeHDR(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + texelSize * 1.0));
            half3 c4 = DecodeHDR(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + texelSize * 2.0));

            half3 color = c0 * 0.054 + c1 * 0.242 + c2 * 0.399 + c3 * 0.242 + c4 * 0.054;
            return EncodeHDR(color);
        }

        // --- Pass 2: Upsample ---
        half3 Upsample(float2 uv)
        {
            half3 highMip = DecodeHDR(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv));
            half3 lowMip = DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTexLowMip, sampler_LinearClamp, uv));
            return lerp(highMip, lowMip, Scatter);
        }

        half4 FragUpsample(Varyings input) : SV_Target
        {
            half3 color = Upsample(input.texcoord);
            return EncodeHDR(color);
        }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZTest Always ZWrite Off Cull Off

        // Pass 0
        Pass { Name "Bloom Prefilter" HLSLPROGRAM #pragma vertex Vert #pragma fragment FragPrefilter ENDHLSL }
        // Pass 1
        Pass { Name "Bloom Blur" HLSLPROGRAM #pragma vertex Vert #pragma fragment FragBlur ENDHLSL }
        // Pass 2
        Pass { Name "Bloom Upsample" HLSLPROGRAM #pragma vertex Vert #pragma fragment FragUpsample ENDHLSL }
    }
}