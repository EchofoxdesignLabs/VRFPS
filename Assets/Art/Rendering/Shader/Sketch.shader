// VRDefender/Sketch.shader

Shader "VRDefender/Sketch"
{
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        #define E 2.71828f

        // This struct and helper function correctly prepare UVs for stereo rendering.
        struct StereoVaryings
        {
            float2 uv;
            uint eyeIndex;
        };

        StereoVaryings GetStereoVaryings(Varyings input)
        {
            StereoVaryings output;
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            output.uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
            output.eyeIndex = unity_StereoEyeIndex;
            return output;
        }
        
        // Declare variables used by multiple passes here
        float4 _CustomTexelSize;
        uint _KernelSize;
        float _Spread;
        uint _BlurStepSize;
        
        float gaussian(int x) 
        {
            float sigmaSqu = _Spread * _Spread;
            return (1 / sqrt(TWO_PI * sigmaSqu)) * pow(E, -(x * x) / (2 * sigmaSqu));
        }

        ENDHLSL

        Pass
        {
            Name "Sketch Main"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            TEXTURE2D(_SketchTexture);
            TEXTURE2D_X(_ShadowmapTexture);

            float4 _SketchColor;
            float2 _SketchThresholds;
            float2 _SketchTiling;
            float _CrossHatching;

            // FIX: Restored the full function body for triplanar sampling.
            float4 triplanarSample(Texture2D tex, SamplerState texSampler, float2x2 rotation, float3 uv, float3 normals, float blend)
            {
                float2 uvX = mul(rotation, uv.zy * _SketchTiling);
                float2 uvY = mul(rotation, uv.xz * _SketchTiling);
                float2 uvZ = mul(rotation, uv.xy * _SketchTiling);

                if (normals.x < 0) { uvX.x = -uvX.x; }
                if (normals.y < 0) { uvY.x = -uvY.x; }
                if (normals.z >= 0){ uvZ.x = -uvZ.x; }

                float4 colX = SAMPLE_TEXTURE2D(tex, texSampler, uvX);
                float4 colY = SAMPLE_TEXTURE2D(tex, texSampler, uvY);
                float4 colZ = SAMPLE_TEXTURE2D(tex, texSampler, uvZ);

                float3 blending = pow(abs(normals), blend);
                blending /= dot(blending, 1.0f);

                return (colX * blending.x + colY * blending.y + colZ * blending.z);
            }

            float4 frag (Varyings i) : SV_Target
            {
                StereoVaryings stereo = GetStereoVaryings(i);
                float3 uv = float3(stereo.uv, stereo.eyeIndex);
                
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                
                float depth = SampleSceneDepth(stereo.uv);
                float3 worldPos = ComputeWorldSpacePosition(stereo.uv, depth, UNITY_MATRIX_I_VP);
                float3 worldNormal = SampleSceneNormals(stereo.uv);
                
                float shadows = 1.0f - SAMPLE_TEXTURE2D_X(_ShadowmapTexture, sampler_LinearClamp, uv).r;
                float sketchVisibility = smoothstep(_SketchThresholds.x, _SketchThresholds.y, shadows);

                float2x2 rotationMatrix = float2x2(1, 0, 0, 1);
                float4 sketchTexture = saturate(triplanarSample(_SketchTexture, sampler_LinearRepeat, rotationMatrix, worldPos, worldNormal, 10.0f));

                if(_CrossHatching > 0.5f)
                {
                    rotationMatrix = float2x2(0.707, -0.707, 0.707, 0.707);
                    float4 sketchTexture2 = saturate(triplanarSample(_SketchTexture, sampler_LinearRepeat, rotationMatrix, worldPos, worldNormal, 10.0f));
                    sketchTexture.rgb = saturate(sketchTexture.rgb + sketchTexture2.rgb);
                    sketchTexture.a = max(sketchTexture.a, sketchTexture2.a);
                }
                sketchTexture *= _SketchColor;
                
                return lerp(col, sketchTexture, sketchVisibility * sketchTexture.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Horizontal Blur"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_horizontal
            float4 frag_horizontal (Varyings i) : SV_Target
            {
                StereoVaryings stereo = GetStereoVaryings(i);
                float3 col = 0.0f;
                float kernelSum = 0.0f;
                int upper = ((_KernelSize - 1) / 2);
                int lower = -upper;

                for (int x = lower; x <= upper; x += _BlurStepSize)
                {
                    float2 offset_uv = stereo.uv + float2(_CustomTexelSize.x * x, 0.0f);
                    float gauss = gaussian(x);
                    kernelSum += gauss;
                    
                    col += gauss * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, float3(offset_uv, stereo.eyeIndex)).r;
                }

                if (kernelSum > 0.0f)
                    col /= kernelSum;

                return float4(col, 1.0);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "Vertical Blur"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_vertical
            float4 frag_vertical (Varyings i) : SV_Target
            {
                StereoVaryings stereo = GetStereoVaryings(i);
                float3 col = 0.0f;
                float kernelSum = 0.0f;
                int upper = ((_KernelSize - 1) / 2);
                int lower = -upper;

                for (int y = lower; y <= upper; y += _BlurStepSize)
                {
                    float2 offset_uv = stereo.uv + float2(0.0f, _CustomTexelSize.y * y);
                    float gauss = gaussian(y);
                    kernelSum += gauss;
                    
                    col += gauss * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, float3(offset_uv, stereo.eyeIndex)).r;
                }

                if (kernelSum > 0.0f)
                    col /= kernelSum;

                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
