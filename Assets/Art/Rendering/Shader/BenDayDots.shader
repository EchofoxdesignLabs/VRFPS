// Shaders/Hidden/BenDayDots.shader

Shader "Hidden/BenDayDots"
{
    Properties
    {
        _DotScale ("Dot Scale", Float) = 500
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

            TEXTURE2D(_BloomTexture);
            SAMPLER(sampler_BloomTexture);

            float _DotScale;
            float _DotHardness;
            float _MinDotSize;

            // --- Voronoi Noise Functions ---
            float2 hash(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            // FIX: Rewritten voronoi function to be more explicit and avoid compiler issues.
            float voronoi(float2 uv)
            {
                float2 grid_uv = floor(uv);
                float2 frac_uv = frac(uv);

                // We use squared distance to avoid using sqrt() inside the loop.
                float min_dist_sq = 10.0;

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 neighbor_offset = float2(x, y);
                        float2 cell_coord = grid_uv + neighbor_offset;
                        
                        // Get the random point for the neighboring cell.
                        float2 point_in_cell = hash(cell_coord);
                        
                        // Calculate the vector from the pixel to the random point.
                        float2 vec_to_point = neighbor_offset + point_in_cell - frac_uv;
                        
                        // Calculate squared distance.
                        float dist_sq = dot(vec_to_point, vec_to_point);
                        
                        // Keep the minimum squared distance.
                        min_dist_sq = min(min_dist_sq, dist_sq);
                    }
                }
                // Return the actual distance.
                return sqrt(min_dist_sq);
            }
            // --- End Voronoi ---

            float4 Frag(Varyings i) : SV_Target
            {
                // FIX: Replaced the undeclared 'sampler_BlitTexture' with the universally available 'sampler_LinearClamp'.
                float4 originalColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);
                float3 bloomColor = SAMPLE_TEXTURE2D(_BloomTexture, sampler_BloomTexture, i.texcoord).rgb;
                float bloomLuminance = Luminance(bloomColor);

                float2 scaledUV = i.texcoord * _DotScale;
                float voronoiDist = voronoi(scaledUV);

                float dotRadius = bloomLuminance + _MinDotSize;
                float dot = 1.0 - smoothstep(dotRadius, dotRadius + _DotHardness, voronoiDist);
                
                float3 benDayEffect = dot * bloomColor;

                return float4(originalColor.rgb + benDayEffect, 1.0);
            }
            ENDHLSL
        }
    }
}