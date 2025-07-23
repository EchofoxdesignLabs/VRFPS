Shader "Hidden/Edge Detection"
{
    Properties
    {
        _OutlineThickness ("Outline Thickness", Float) = 1
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        // NEW PROPERTIES
        _MaxThicknessDistance ("Max Thickness Distance", Float) = 50
        _MinThicknessMultiplier ("Min Thickness Multiplier", Range(0.0, 1.0)) = 0.1
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 10
        _NoiseStrength ("Noise Strength", Range(0.0, 1.0)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque"
        }

        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass 
        {
            Name "EDGE DETECTION OUTLINE"
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl" // needed to sample scene depth
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl" // needed to sample scene normals
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl" // needed to sample scene color/luminance

            float _OutlineThickness;
            float4 _OutlineColor;
            float _MaxThicknessDistance;
            float _MinThicknessMultiplier;
            TEXTURE2D(_NoiseTexture);
            SAMPLER(sampler_NoiseTexture);
            float _NoiseScale;
            float _NoiseStrength;

            #pragma vertex Vert // vertex shader is provided by the Blit.hlsl include
            #pragma fragment frag

            // Edge detection kernel that works by taking the sum of the squares of the differences between diagonally adjacent pixels (Roberts Cross).
            float RobertsCross(float3 samples[4])
            {
                const float3 difference_1 = samples[1] - samples[2];
                const float3 difference_2 = samples[0] - samples[3];
                return sqrt(dot(difference_1, difference_1) + dot(difference_2, difference_2));
            }

            // The same kernel logic as above, but for a single-value instead of a vector3.
            float RobertsCross(float samples[4])
            {
                const float difference_1 = samples[1] - samples[2];
                const float difference_2 = samples[0] - samples[3];
                return sqrt(difference_1 * difference_1 + difference_2 * difference_2);
            }
            
            // Helper function to sample scene normals remapped from [-1, 1] range to [0, 1].
            float3 SampleSceneNormalsRemapped(float2 uv)
            {
                return SampleSceneNormals(uv) * 0.5 + 0.5;
            }

            // Helper function to sample scene luminance.
            float SampleSceneLuminance(float2 uv)
            {
                float3 color = SampleSceneColor(uv);
                return color.r * 0.3 + color.g * 0.59 + color.b * 0.11;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                // Screen-space coordinates which we will use to sample.
                float2 uv = IN.texcoord;
                float2 texel_size = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
                // --- 1. Get Depth and Calculate Distance-based Thickness ---
                float centerDepth = SampleSceneDepth(uv);
                float linearEyeDepth = LinearEyeDepth(centerDepth, _ZBufferParams);
                // Calculate thickness falloff
                float distanceFalloff = 1.0 - smoothstep(0, _MaxThicknessDistance, linearEyeDepth);
                float thicknessMultiplier = lerp(_MinThicknessMultiplier, 2.0, distanceFalloff);
                float finalThickness = _OutlineThickness * thicknessMultiplier;
                // Don't render if thickness is effectively zero
                if (finalThickness < 0.1)
                {
                    return 0;
                }

                // --- 2. Add Noise for Distortion ---
                // Sample noise texture
                //float2 noiseUV = uv * _ScreenParams.xy / _NoiseScale;
                //float noise = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV).r;
                // Create a random offset vector from the noise
                //float2 distortionOffset = (noise - 0.5) * 2.0 * _NoiseStrength * texel_size * 100.0;
                
                // --- 3. Sample Scene Buffers with Distortion ---
                const float half_width_f = floor(finalThickness  * 0.5);
                const float half_width_c = ceil(finalThickness  * 0.5);

                float2 uvs[4];
                uvs[0] = uv + texel_size * float2(half_width_f, half_width_c) * float2(-1, 1) ;
                uvs[1] = uv + texel_size * float2(half_width_c, half_width_c) * float2(1, 1) ;
                uvs[2] = uv + texel_size * float2(half_width_f, half_width_f) * float2(-1, -1) ;
                uvs[3] = uv + texel_size * float2(half_width_c, half_width_f) * float2(1, -1) ;
                
                float3 normal_samples[4];
                float depth_samples[4], luminance_samples[4];
                float totalNoise = 0; // We'll average the noise for the line breakup effect
                
                for (int i = 0; i < 4; i++) {
                    float2 base_uvs = uvs[i];
                    float2 noiseUV = base_uvs * _ScreenParams.xy / _NoiseScale;
                    float noise = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV).r;
                    totalNoise += noise;

                    // Create a unique offset for this point
                    float2 distortionOffset = (noise - 0.5) * 2.0 * _NoiseStrength * texel_size * 100.0;
                    float2 final_uv = base_uvs + distortionOffset;
                    // Sample scene buffers using the final, distorted UV
                    depth_samples[i] = SampleSceneDepth(uvs[i]);
                    normal_samples[i] = SampleSceneNormalsRemapped(uvs[i]);
                    luminance_samples[i] = SampleSceneLuminance(uvs[i]);
                }
                //
                // Apply edge detection kernel on the samples to compute edges.
                float edge_depth = RobertsCross(depth_samples);
                float edge_normal = RobertsCross(normal_samples);
                float edge_luminance = RobertsCross(luminance_samples);
                
                // Threshold the edges (discontinuity must be above certain threshold to be counted as an edge). The sensitivities are hardcoded here.
                float depth_threshold = linearEyeDepth / 500.0f; // Scale threshold with distance
                edge_depth = edge_depth > depth_threshold ? 1 : 0;
                
                float normal_threshold = 1 / 4.0f;
                edge_normal = edge_normal > normal_threshold ? 1 : 0;
                
                float luminance_threshold = 1 / 0.5f;
                edge_luminance = edge_luminance > luminance_threshold ? 1 : 0;
                
                // Combine the edges from depth/normals/luminance using the max operator.
                float edge = max(edge_depth, max(edge_normal, edge_luminance));
                // --- 5. Apply Noise to Break up the Line ---
                // --- 4. Apply Averaged Noise to Break up the Line ---
                float averageNoise = totalNoise / 4.0;
                edge *= saturate(averageNoise / (1.0 - _NoiseStrength));
                
                // Color the edge with a custom color.
                return edge * _OutlineColor;
            }
            ENDHLSL
        }
    }
}