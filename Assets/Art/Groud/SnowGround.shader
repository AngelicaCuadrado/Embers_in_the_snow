Shader "Custom/SnowGroundVR"
{
    Properties
    {
        _MainTex ("Snow Texture", 2D) = "white" {}
        _Color ("Snow Color", Color) = (1,1,1,1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _TextureScale ("Texture Tiling (Triplanar)", Float) = 1.0
        _SparkleScale ("Sparkle Scale", Float) = 100
        _SparkleCutoff ("Sparkle Intensity", Range(0, 1)) = 0.5
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Required for point lights and spot lights in URP
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 normalWS : NORMAL_WS;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _TextureScale;
            float _SparkleScale;
            float _SparkleCutoff;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            float pseudo_noise(float3 p)
            {
                return frac(sin(dot(p, float3(12.9898, 78.233, 45.164))) * 43758.5453);
            }

            // Triplanar blending logic for snow texture
            float3 GetTriplanarWeight(float3 normal)
            {
                float3 weight = abs(normal);
                weight /= (weight.x + weight.y + weight.z);
                return weight;
            }

            // Function to calculate sparkle for a single light
            float3 CalculateSnowLight(Light light, float3 normalWS, float3 viewDir, float3 worldPos)
            {
                float3 lightDir = light.direction;
                float3 halfDir = normalize(lightDir + viewDir);
                
                // Diffuse
                float NdotL = saturate(dot(normalWS, lightDir));
                float3 diffuse = light.color * (NdotL * light.distanceAttenuation * light.shadowAttenuation);

                // Sparkle logic (Crystalline glints)
                float sparkleNoise = pseudo_noise(floor(worldPos * _SparkleScale));
                float sparkle = pow(sparkleNoise, 10.0) * _SparkleCutoff;
                float sparkleMask = saturate(dot(normalWS, halfDir));
                float3 sparkleEffect = sparkle * pow(sparkleMask, 80.0) * light.color * light.distanceAttenuation;

                return diffuse + sparkleEffect;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDir = normalize(_WorldSpaceCameraPos - input.worldPos);
                
                // --- Triplanar Mapping for Base Texture ---
                float3 weight = GetTriplanarWeight(normalWS);
                float2 uvX = input.worldPos.zy * _TextureScale;
                float2 uvY = input.worldPos.xz * _TextureScale;
                float2 uvZ = input.worldPos.xy * _TextureScale;
                
                float4 colX = tex2D(_MainTex, uvX);
                float4 colY = tex2D(_MainTex, uvY);
                float4 colZ = tex2D(_MainTex, uvZ);
                float4 texColor = (colX * weight.x + colY * weight.y + colZ * weight.z) * _Color;

                // 1. Primary Light (Sun)
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.worldPos));
                float3 lighting = CalculateSnowLight(mainLight, normalWS, viewDir, input.worldPos);

                // 2. Additional Lights (Campfire, Torches)
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint i = 0; i < pixelLightCount; ++i)
                {
                    Light addLight = GetAdditionalLight(i, input.worldPos);
                    lighting += CalculateSnowLight(addLight, normalWS, viewDir, input.worldPos);
                }

                return half4(lighting * texColor.rgb, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}