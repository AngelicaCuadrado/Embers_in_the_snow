Shader "Custom/GlowingCrackedIce"
{
    Properties
    {
        [Header(Ice Surface Settings)]
        _IceColor("Ice Color (Blue)", Color) = (0.15, 0.45, 0.85, 1.0)
        _MainTex("Crack Mask Texture (R)", 2D) = "white" {}
        _IceSmoothness("Ice Smoothness", Range(0, 1)) = 0.9
        _IceMetallic("Ice Metallic", Range(0, 1)) = 0.0

        [Header(Crack (Lit) Settings)]
        _CrackColor("Crack Color (White)", Color) = (1, 1, 1, 1)
        _CrackAlbedoStrength("Crack Albedo Strength", Range(0, 2)) = 1.0
        _CrackSmoothness("Crack Smoothness", Range(0, 1)) = 0.3
        _CrackSmoothnessBlend("Crack Smoothness Influence", Range(0, 1)) = 1.0

        [Header(Crack Mask Settings)]
        _CrackThreshold("Crack Threshold", Range(0, 1)) = 0.85
        _CrackPower("Crack Sharpness", Range(1, 10)) = 2.0

        [Header(Glow (Optional))]
        [HDR]_EmissionColor("Crack Emission Color", Color) = (1, 1, 1, 1)
        _GlowStrength("Glow Strength", Range(0, 5)) = 0.0
        _PulseSpeed("Pulse Speed", Range(0, 5)) = 2.0
        _PulseMin("Minimum Glow", Range(0, 1)) = 0.4
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
            "TerrainCompatible"="True"
        }

        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _IceColor;
                float4 _CrackColor;
                float4 _EmissionColor;

                float _IceSmoothness;
                float _IceMetallic;

                float _CrackAlbedoStrength;
                float _CrackSmoothness;
                float _CrackSmoothnessBlend;

                float _CrackThreshold;
                float _CrackPower;

                float _GlowStrength;
                float _PulseSpeed;
                float _PulseMin;

                float4 _MainTex_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS    = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.uv          = IN.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return OUT;
            }

            // Simple “good enough” URP-lit shading (diffuse + spec + SH ambient)
            half3 ShadeOneLight(half3 albedo, half3 N, half3 V, Light L, half metallic, half smoothness)
            {
                half3 lightColor = L.color * (L.distanceAttenuation * L.shadowAttenuation);

                half NdotL = saturate(dot(N, L.direction));
                half3 diffuse = albedo * NdotL;

                // Spec: metallic workflow-ish approximation
                half3 F0 = lerp(half3(0.04h, 0.04h, 0.04h), albedo, metallic);
                half3 H = normalize(L.direction + V);
                half NdotH = saturate(dot(N, H));
                half specPower = lerp(8.0h, 256.0h, smoothness * smoothness);
                half3 spec = F0 * pow(NdotH, specPower) * NdotL;

                // Reduce diffuse when metallic is high
                diffuse *= (1.0h - metallic);

                return (diffuse + spec) * lightColor;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half3 N = normalize(IN.normalWS);
                half3 V = normalize(GetWorldSpaceViewDir(IN.positionWS));

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Crack mask from RED channel
                half crackMask = smoothstep(_CrackThreshold, 1.0h, tex.r);
                crackMask = pow(saturate(crackMask), _CrackPower);

                // Ice (blue) vs crack (white) ALBEDO — this is what makes cracks show under lighting
                half3 iceAlbedo   = _IceColor.rgb;
                half3 crackAlbedo = _CrackColor.rgb * _CrackAlbedoStrength;
                half3 albedo      = lerp(iceAlbedo, crackAlbedo, crackMask);

                // Smoothness difference also helps cracks “read” with light
                half smoothness = lerp(_IceSmoothness, _CrackSmoothness, crackMask * _CrackSmoothnessBlend);
                half metallic   = _IceMetallic;

                // Ambient (SH)
                half3 ambient = SampleSH(N) * albedo;

                // Main light
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half3 col = ambient + ShadeOneLight(albedo, N, V, mainLight, metallic, smoothness);

                // Additional lights
                #if defined(_ADDITIONAL_LIGHTS)
                uint lightCount = GetAdditionalLightsCount();
                for (uint i = 0; i < lightCount; i++)
                {
                    Light L = GetAdditionalLight(i, IN.positionWS);
                    col += ShadeOneLight(albedo, N, V, L, metallic, smoothness);
                }
                #endif

                // Optional emission (default OFF so cracks stay white from lighting)
                half pulse = lerp(_PulseMin, 1.0h, (sin(_Time.y * _PulseSpeed) * 0.5h + 0.5h));
                half3 emission = crackMask * _EmissionColor.rgb * (_GlowStrength * pulse);
                col += emission;

                // Fog
                half fogFactor = ComputeFogFactor(IN.positionHCS.z);
                col = MixFog(col, fogFactor);

                return half4(col, 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionHCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionHCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, 0));
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}