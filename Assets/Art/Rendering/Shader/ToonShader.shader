// Roystan - Toon Complete (URP/VR)
// Updated to Universal Render Pipeline and optimized for VR.
// Now uses a screen-space filter for edge lines (Sobel-like effect).
Shader "Roystan/Toon Complete URP"
{
    Properties
    {
        [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
        [MainColor] _Color("Color", Color) = (1,1,1,1)
        
        [Header(Lighting)]
        [HDR] _AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
        [HDR] _SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
        _Glossiness("Glossiness", Float) = 32

        [Header(Rim Light)]
        [HDR] _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimAmount("Rim Amount", Range(0, 1)) = 0.716
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.1

        [Header(Edge Lines)]
        _LineColor("Line Color", Color) = (0,0,0,1)
        _LineNormalSensitivity("Normal Line Sensitivity", Range(0, 2)) = 0.5
        _LineDepthSensitivity("Depth Line Sensitivity", Range(0, 2)) = 0.5
    }
    SubShader
    {
        // URP specific tags. "RenderPipeline" must be "UniversalPipeline".
        // "RenderType" is used for various rendering effects like transparency.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        // --- Main Toon Shading Pass ---
        // This pass renders the object with the toon lighting model and edge lines.
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 normalWS     : NORMAL;
                float3 viewDirWS    : TEXCOORD0;
                float2 uv           : TEXCOORD1;
                float4 shadowCoord  : TEXCOORD2;
                // We need view-space position for depth-based edge detection.
                float3 positionVS   : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _AmbientColor;
                half4 _SpecularColor;
                float _Glossiness;
                half4 _RimColor;
                half _RimAmount;
                half _RimThreshold;
                // Edge line properties
                half4 _LineColor;
                half _LineNormalSensitivity;
                half _LineDepthSensitivity;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = positionInputs.positionCS;
                o.positionVS = positionInputs.positionVS;
                
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS);
                o.normalWS = normalInputs.normalWS;

                o.viewDirWS = GetCameraPositionWS() - positionInputs.positionWS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.shadowCoord = GetShadowCoord(positionInputs);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                half3 normal = normalize(i.normalWS);
                half3 viewDir = normalize(i.viewDirWS);
                Light mainLight = GetMainLight(i.shadowCoord);
                half shadow = mainLight.shadowAttenuation;

                // --- Toon Lighting ---
                half NdotL = dot(normal, mainLight.direction);
                half lightIntensity = smoothstep(0, 0.01, NdotL * shadow);
                half3 lightColor = lightIntensity * mainLight.color;

                half3 halfVector = normalize(mainLight.direction + viewDir);
                half NdotH = dot(normal, halfVector);
                half specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
                half specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
                half3 specular = specularIntensitySmooth * _SpecularColor.rgb;

                half rimDot = 1 - dot(viewDir, normal);
                half rimIntensity = rimDot * pow(NdotL, _RimThreshold);
                rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
                half3 rim = rimIntensity * _RimColor.rgb;

                half3 ambient = SampleSH(normal) * _AmbientColor.rgb;
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half3 finalColor = (ambient + lightColor + specular + rim) * _Color.rgb * texColor.rgb;

                // --- Edge Lines (Screen-Space Filter) ---
                // We use fwidth(), which calculates screen-space derivatives, to find edges.
                // This tells us how fast a value changes between adjacent pixels.
                
                // 1. Normal-based edges (creases on the model)
                half normal_delta = fwidth(i.normalWS);
                half normal_line = smoothstep(0.0, _LineNormalSensitivity, normal_delta);

                // 2. Depth-based edges (silhouettes)
                // We use the Z-component of the view-space position for depth.
                half depth_delta = fwidth(i.positionVS.z);
                half depth_line = smoothstep(0.0, _LineDepthSensitivity, depth_delta);

                // Combine the lines and clamp between 0 and 1.
                half line_factor = saturate(normal_line + depth_line);

                // Blend the final shaded color with the line color.
                finalColor = lerp(finalColor, _LineColor.rgb, line_factor);
                
                return half4(finalColor, texColor.a * _Color.a);
            }
            ENDHLSL
        }

        // --- Shadow Caster Pass ---
        // This pass is necessary for the object to cast shadows correctly in URP.
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings ShadowPassVertex(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                float3 lightDirection = _MainLightPosition.xyz;
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                
                o.positionCS = positionCS;
                return o;
            }

            half4 ShadowPassFragment(Varyings i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
