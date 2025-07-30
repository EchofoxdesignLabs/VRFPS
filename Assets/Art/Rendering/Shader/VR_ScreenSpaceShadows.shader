// Shaders/Hidden/VR_ScreenSpaceShadows.shader

Shader "Hidden/VR_ScreenSpaceShadows"
{
    SubShader
    {
        Tags{ "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

        // --- FIX: A custom, VR-aware vertex shader for manual DrawProcedural calls ---
        Varyings VertFullscreen(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
        {
            Varyings output;
            // Required setup for Single-Pass Instanced rendering
            UNITY_SETUP_INSTANCE_ID(instanceID);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            // Generates a fullscreen triangle
            output.texcoord = float2((vertexID << 1) & 2, vertexID & 2);
            output.positionCS = float4(output.texcoord * 2.0 - 1.0, 0.0, 1.0);

            // Flip the triangle upside down on platforms where UVs start at the top
            #if UNITY_UV_STARTS_AT_TOP
            output.positionCS.y = -output.positionCS.y;
            #endif
            
            return output;
        }
        // --- END FIX ---

        half4 Fragment(Varyings input) : SV_Target
        {
            // This macro is essential for setting up which eye (0 or 1) is currently being rendered.
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            // --- VR COMPATIBILITY FIX ---
            // This function transforms the standard texture coordinates into the correct coordinates
            // for the current eye's slice in the stereo texture array. This is the key fix.
            float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord.xy);
            // --- END FIX ---

#if UNITY_REVERSED_Z
            float deviceDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_PointClamp, uv).r;
#else
            float deviceDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_PointClamp, uv).r;
            deviceDepth = deviceDepth * 2.0 - 1.0;
#endif

            // Fetch shadow coordinates for cascade using the corrected UVs.
            float3 wpos = ComputeWorldSpacePosition(uv, deviceDepth, unity_MatrixInvVP);
            float4 coords = TransformWorldToShadowCoord(wpos);

            // Screenspace shadowmap is only used for directional lights which use orthogonal projection.
            half realtimeShadow = MainLightRealtimeShadow(coords);

            return realtimeShadow;
        }

        ENDHLSL

        Pass
        {
            Name "VR_ScreenSpaceShadows"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma multi_compile _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            // FIX: Tell the shader to use our new custom vertex shader
            #pragma vertex VertFullscreen

            #pragma vertex   Vert
            #pragma fragment Fragment
            ENDHLSL
        }
    }
}
