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

            // This is the global texture set by Unity's ScreenSpaceShadows pass
            TEXTURE2D(_ScreenSpaceShadowmapTexture);
            SAMPLER(sampler_ScreenSpaceShadowmapTexture);

            float4 Frag(Varyings i) : SV_Target
            {
                // Sample the global texture and output it
                return SAMPLE_TEXTURE2D(_ScreenSpaceShadowmapTexture, sampler_ScreenSpaceShadowmapTexture, i.texcoord);
            }
            ENDHLSL
        }
    }
}