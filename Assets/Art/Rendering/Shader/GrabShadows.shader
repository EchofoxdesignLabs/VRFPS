// Shaders/GrabShadows.shader

Shader "Hidden/VRDefender/GrabShadows"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "GrabShadows"
            ZTest Always ZWrite Off Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // VR FIX: Use TEXTURE2D_X for the texture declaration.
            TEXTURE2D_X(_ScreenSpaceShadowmapTexture);
            // FIX: Use the standard SAMPLER macro for the sampler. SAMPLER_X is not a valid macro.
            SAMPLER(sampler_ScreenSpaceShadowmapTexture);

            float4 Frag(Varyings i) : SV_Target
            {
                // Set up the stereo eye index.
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                // Create a 3D texture coordinate (uv.xy for position, eyeIndex for the slice).
                float3 uv = float3(i.texcoord.xy, unity_StereoEyeIndex);

                // Sample from the texture array using the correct macro and 3D coordinate.
                return SAMPLE_TEXTURE2D_X(_ScreenSpaceShadowmapTexture, sampler_ScreenSpaceShadowmapTexture, uv);
            }
            ENDHLSL
        }
    }
}
