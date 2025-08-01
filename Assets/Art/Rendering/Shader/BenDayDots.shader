// Shaders/Hidden/BenDayDots.shader
// Final version with intensity, tint, and a power-based vignette control.

Shader "Hidden/BenDayDots"
{
    Properties
    {
        _Intensity ("Intensity", Float) = 1
        _BloomTint ("Bloom Tint", Color) = (1,1,1,1)
        _VignettePower ("Vignette Power", Range(0.1, 4)) = 1.0
        _DotDensity ("Dot Density", Float) = 100
        _PatternAngle ("Pattern Angle", Range(0, 90)) = 45
        _DotHardness ("Dot Hardness", Range(0.01, 2.0)) = 0.5
        _MinDotSize ("Min Dot Size", Range(0.0, 1.0)) = 0.1
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "BenDayDotsComposite"
            ZTest Always ZWrite Off Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Textures from the render pass
            TEXTURE2D(_BloomTexture);
            SAMPLER(sampler_BloomTexture);

            // Uniforms from the C# script
            float _Intensity;
            half4 _BloomTint;
            float _VignettePower;
            float _DotDensity;
            float _PatternAngle;
            float _DotHardness;
            float _MinDotSize;

            // Helper function for 2D rotation
            float2 rotate(float2 uv, float angle_rad)
            {
                float s = sin(angle_rad);
                float c = cos(angle_rad);
                return mul(float2x2(c, -s, s, c), uv);
            }

            float4 Frag(Varyings i) : SV_Target
            {
                // Sample the original scene and the generated bloom texture
                float4 originalColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);
                float3 bloomColor = SAMPLE_TEXTURE2D(_BloomTexture, sampler_BloomTexture, i.texcoord).rgb;

                // --- 1. Apply Tint and Intensity ---
                bloomColor *= _BloomTint.rgb;
                float bloomLuminance = Luminance(bloomColor * _Intensity);

                // --- 2. Calculate Screen-Space Vignette ---
                // Normalize distance from screen center to a 0-1 range
                float distFromScreenCenter = distance(i.texcoord, 0.5) * 1.41421f;
                // Use pow() for a curved falloff controlled by Vignette Power
                float vignette = 1.0 - pow(distFromScreenCenter, _VignettePower);
                vignette = saturate(vignette); // Clamp between 0 and 1

                // --- 3. Halftone Logic ---
                float angle_rad = _PatternAngle * (3.14159 / 180.0);
                float2 rotatedUV = rotate(i.texcoord, angle_rad);
                float2 scaledUV = rotatedUV * _DotDensity;
                float2 cellPos = frac(scaledUV);
                float distFromCellCenter = distance(cellPos, 0.5);

                // Modify the dot radius with the calculated vignette
                float dotRadius = (bloomLuminance * vignette + _MinDotSize) * 0.7;
                float dot = 1.0 - smoothstep(dotRadius, dotRadius + _DotHardness, distFromCellCenter);

                // --- 4. Final Composite ---
                // Apply intensity to the final dot color as well
                float3 benDayEffect = dot * bloomColor * _Intensity;
                return float4(originalColor.rgb + benDayEffect, 1.0);
            }
            ENDHLSL
        }
    }
}