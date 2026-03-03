Shader "Custom/BeautifulStarrySky"
{
    Properties
    {
        [Header(Sky Colors)]
        _ZenithColor ("Zenith Color (Top)", Color) = (0.02, 0.02, 0.08, 1)
        _HorizonColor ("Horizon Color (Bottom)", Color) = (0.05, 0.1, 0.15, 1)
        
        [Header(Star Settings)]
        _StarDensity ("Star Density", Range(10, 200)) = 60.0
        _StarBrightness ("Star Brightness", Range(0.1, 5.0)) = 1.5
        _TwinkleSpeed ("Twinkle Speed", Range(0, 5)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            float4 _ZenithColor;
            float4 _HorizonColor;
            float _StarDensity;
            float _StarBrightness;
            float _TwinkleSpeed;

            // High-quality 3D random hash function
            float3 Hash33(float3 p) {
                p = float3(dot(p, float3(127.1, 311.7, 74.7)),
                           dot(p, float3(269.5, 183.3, 246.1)),
                           dot(p, float3(113.5, 271.9, 124.6)));
                return frac(sin(p) * 43758.5453123);
            }

            // Cellular noise function to create organic, non-grid stars
            float StarField(float3 dir, float density, float twinkleSpeed) {
                float3 p = dir * density;
                float3 i = floor(p);
                float3 f = frac(p);
                
                float min_dist = 1.0;
                float random_val = 0.0;
                
                // Check surrounding virtual cells to find the nearest star center
                for(int z = -1; z <= 1; z++) {
                    for(int y = -1; y <= 1; y++) {
                        for(int x = -1; x <= 1; x++) {
                            float3 neighbor = float3(x, y, z);
                            float3 point_pos = Hash33(i + neighbor);
                            
                            // Calculate distance to this specific star
                            float3 diff = neighbor + point_pos - f;
                            float dist = length(diff);
                            
                            if(dist < min_dist) {
                                min_dist = dist;
                                random_val = point_pos.x; // Save a random seed for this star
                            }
                        }
                    }
                }
                
                // Invert distance so the star's center is the brightest part
                float star = 1.0 - min_dist;
                
                // Heavily sharpen the gradient so it becomes a tiny, crisp point
                star = smoothstep(0.92, 1.0, star);
                
                // Twinkle effect using time and the star's unique random seed
                float twinkle = sin(_Time.y * twinkleSpeed + random_val * 6.28) * 0.5 + 0.5;
                
                return star * random_val * twinkle;
            }

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.viewDir = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float3 dir = normalize(i.viewDir);
                
                // 1. Generate Sky Background Gradient
                // This smoothly blends from the Zenith color to the Horizon color
                float skyGradient = smoothstep(-0.1, 0.5, dir.y);
                float3 skyColor = lerp(_HorizonColor.rgb, _ZenithColor.rgb, skyGradient);
                
                // 2. Generate Two Layers of Stars for Parallax/Depth
                float layer1 = StarField(dir, _StarDensity, _TwinkleSpeed);
                float layer2 = StarField(dir, _StarDensity * 1.6, _TwinkleSpeed * 1.5) * 0.5; // Smaller and fainter
                
                float stars = (layer1 + layer2) * _StarBrightness;
                
                // 3. Fade stars out at the horizon so they don't clip into the ground
                float horizonFade = smoothstep(0.0, 0.2, dir.y);
                stars *= horizonFade;
                
                // Give stars a slight cool-white tint
                float3 finalColor = skyColor + (stars * float3(0.85, 0.95, 1.0));
                
                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}