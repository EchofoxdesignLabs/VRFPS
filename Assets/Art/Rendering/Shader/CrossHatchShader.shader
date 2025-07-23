// Gemini: Cel/Toon Shader with Procedural Cross-Hatch Shadows & Texture
// Target Pipeline: Universal Render Pipeline (URP)
// Version: 1.2 - Adapted to use a custom lighting function.
//
// Date: 2024-07-21
//
// Features:
// - Uses your custom lighting.hlsl for main light data.
// - Main Texture: Apply a standard albedo texture.
// - Cel Shading: Hard-edged lighting for a cartoonish look.
// - Procedural Hatching: Shadows are rendered as a cross-hatch pattern.

Shader "Gemini/URP/CelHatchVR_CustomLit"
{
    Properties
    {
        [Header(Color and Texture Settings)]
        _Color("Main Color Tint", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _MainTex("Main Texture (Albedo)", 2D) = "white" {}
        _ShadowBaseColor("Shadow Base Color", Color) = (0.4, 0.4, 0.6, 1)
        _HatchLineColor("Hatch Line Color", Color) = (0.1, 0.1, 0.2, 1)

        [Header(Cel Shading)]
        _LightThreshold("Light Threshold", Range(0, 1)) = 0.5

        [Header(Cross Hatching)]
        _HatchFrequency("Hatch Frequency", Float) = 50
        _HatchLineThickness("Hatch Line Thickness", Range(0, 0.2)) = 0.05
        _HatchAngle1("Hatch Angle 1", Range(0, 3.14159)) = 0.785
        _HatchAngle2("Hatch Angle 2", Range(0, 3.14159)) = 2.356
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            
            // FIXED: Explicitly enable writing to the depth buffer.
            // This prevents objects behind from rendering in front.
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            // ... (The rest of the code is exactly the same)
            
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Art/Rendering/Shader/lighting.hlsl"

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
                float3 normalWS     : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                float4 screenPos    : TEXCOORD2;
                float2 uv           : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _ShadowBaseColor;
                half4 _HatchLineColor;
                half _LightThreshold;
                float _HatchFrequency;
                float _HatchLineThickness;
                float _HatchAngle1;
                float _HatchAngle2;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.screenPos = ComputeScreenPos(o.positionCS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                half3 normal = normalize(i.normalWS);
                
                float3 lightDir, lightColor;
                float distanceAtten, shadowAtten;

                MainLight_float(i.positionWS, lightDir, lightColor, distanceAtten, shadowAtten);

                half NdotL = saturate(dot(normal, lightDir));
                half cel = step(_LightThreshold, NdotL);

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                
                float s1 = sin(_HatchAngle1), c1 = cos(_HatchAngle1);
                float2x2 rotMatrix1 = float2x2(c1, -s1, s1, c1);
                float s2 = sin(_HatchAngle2), c2 = cos(_HatchAngle2);
                float2x2 rotMatrix2 = float2x2(c2, -s2, s2, c2);

                float2 rotatedUV1 = mul(rotMatrix1, screenUV * _HatchFrequency);
                float2 rotatedUV2 = mul(rotMatrix2, screenUV * _HatchFrequency);

                float fw = fwidth(rotatedUV1.x) * 2.0;
                float line1 = smoothstep(1.0 - _HatchLineThickness, 1.0 - _HatchLineThickness + fw, frac(rotatedUV1.x));
                float line2 = smoothstep(1.0 - _HatchLineThickness, 1.0 - _HatchLineThickness + fw, frac(rotatedUV2.x));
                float crossHatch = max(line1, line2);

                half3 litColor = _Color.rgb * texColor.rgb;
                half3 shadowBaseWithTex = _ShadowBaseColor.rgb * texColor.rgb;
                half3 shadowColor = lerp(shadowBaseWithTex, _HatchLineColor.rgb, crossHatch);
                half3 finalColor = lerp(shadowColor, litColor, cel);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}