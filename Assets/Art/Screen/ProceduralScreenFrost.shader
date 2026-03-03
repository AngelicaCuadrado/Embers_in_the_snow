Shader "Custom/ProceduralScreenFrost"
{
    Properties
    {
        [HDR] _FrostColor ("Frost Color", Color) = (0.7, 0.9, 1.0, 1.0)
        _FrostIntensity ("Frost Intensity", Range(0.0, 1.0)) = 0.0
        _NoiseScale ("Ice Crystal Size", Range(1.0, 20.0)) = 8.0
        _EdgeSoftness ("Edge Softness", Range(0.01, 1.0)) = 0.3
        
        // NEW: Controls how fast the ice evolves
        _AnimationSpeed ("Animation Speed", Range(0.0, 2.0)) = 0.2 
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _FrostColor;
                float _FrostIntensity;
                float _NoiseScale;
                float _EdgeSoftness;
                float _AnimationSpeed; // NEW: Added to buffer
            CBUFFER_END

            // 1. Pseudo-random hash function
            float2 Hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            // 2. Smooth Perlin Noise
            float PerlinNoise(float2 p)
            {
                float2 pi = floor(p);
                float2 pf = frac(p);
                float2 w = pf * pf * (3.0 - 2.0 * pf);

                float n00 = dot(Hash22(pi + float2(0.0, 0.0)), pf - float2(0.0, 0.0));
                float n10 = dot(Hash22(pi + float2(1.0, 0.0)), pf - float2(1.0, 0.0));
                float n01 = dot(Hash22(pi + float2(0.0, 1.0)), pf - float2(0.0, 1.0));
                float n11 = dot(Hash22(pi + float2(1.0, 1.0)), pf - float2(1.0, 1.0));

                return lerp(lerp(n00, n10, w.x), lerp(n01, n11, w.x), w.y);
            }

            // 3. Animated FBM
            // NEW: We now pass in a time variable to shift the noise
            float FBM(float2 p, float timeOffset)
            {
                float f = 0.0;
                float amp = 0.5;
                float freq = 1.0;
                
                for (int i = 0; i < 4; i++) 
                {
                    // By multiplying timeOffset by amp, the smaller details move 
                    // slower than the larger details, making it look like real crystallization.
                    float2 shift = float2(timeOffset * amp, timeOffset * amp * 1.2);
                    f += amp * PerlinNoise(p * freq + shift);
                    
                    freq *= 2.0;
                    amp *= 0.5;
                }
                return f; 
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 screenUV = input.uv;
                screenUV.x *= aspect;

                // NEW: Calculate the time offset based on the Animation Speed slider
                float timeOffset = _Time.y * _AnimationSpeed;

                // NEW: Pass the time offset into the FBM function
                float noise = FBM(screenUV * _NoiseScale + float2(13.5, 42.1), timeOffset); 
                noise = noise * 0.5 + 0.5;

                float2 centerUv = input.uv * 2.0 - 1.0;
                float distFromCenter = length(centerUv);

                float freezeSpread = distFromCenter + (noise * 0.6);
                float threshold = 1.5 - (_FrostIntensity * 2.0); 
                
                float alpha = smoothstep(threshold, threshold + _EdgeSoftness, freezeSpread);
                alpha *= (noise * 0.8 + 0.2); 
                alpha *= saturate(_FrostIntensity * 10.0);

                return half4(_FrostColor.rgb, alpha * _FrostColor.a);
            }
            ENDHLSL
        }
    }
}