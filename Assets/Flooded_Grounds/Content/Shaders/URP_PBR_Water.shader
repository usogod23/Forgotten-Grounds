Shader "Flooded_Grounds/URP_PBR_Water"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Emis ("Self-Illumination", Range(0,1)) = 0.1
        _Smth ("Smoothness", Range(0,1)) = 0.9
        _Parallax ("Height", Range(0.005, 0.08)) = 0.02
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _BumpMap2 ("Normalmap2", 2D) = "bump" {}
        _BumpLerp ("Normalmap2 Blend", Range(0,1)) = 0.5
        _ParallaxMap ("Heightmap", 2D) = "black" {}
        _ScrollSpeed ("Scroll Speed", Float) = 0.2
        _WaveFreq ("Wave Frequency", Float) = 20
        _WaveHeight ("Wave Height", Float) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);      SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);      SAMPLER(sampler_BumpMap);
            TEXTURE2D(_BumpMap2);     SAMPLER(sampler_BumpMap2);
            TEXTURE2D(_ParallaxMap);  SAMPLER(sampler_ParallaxMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                float4 _BumpMap2_ST;
                float4 _ParallaxMap_ST;
                float _Emis;
                float _Smth;
                float _Parallax;
                float _ScrollSpeed;
                float _WaveFreq;
                float _WaveHeight;
                float _BumpLerp;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float4 tangentWS  : TEXCOORD3; // xyz tangent, w sign
                float3 viewDirTS  : TEXCOORD4;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                // --- vertex wave displacement (same as original) ---
                float phase = _Time.y * _WaveFreq;
                float offset = (IN.positionOS.x + (IN.positionOS.z * 2)) * 8;
                IN.positionOS.y = sin(phase + offset) * _WaveHeight;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normInputs.normalWS;
                OUT.tangentWS = float4(normInputs.tangentWS, IN.tangentOS.w * GetOddNegativeScale());
                OUT.uv = IN.uv;

                // view dir in tangent space (for parallax)
                float3 viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
                float3 bitangentWS = cross(normInputs.normalWS, normInputs.tangentWS) * OUT.tangentWS.w;
                OUT.viewDirTS = float3(
                    dot(viewDirWS, normInputs.tangentWS),
                    dot(viewDirWS, bitangentWS),
                    dot(viewDirWS, normInputs.normalWS)
                );

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float t = _Time.y;

                half scrollX  = _ScrollSpeed * t;
                half scrollY  = (_ScrollSpeed * t) * 0.5;
                half scrollX2 = (1 - _ScrollSpeed) * t;
                half scrollY2 = (1 - _ScrollSpeed * t) * 0.5;

                float2 uvParallax = IN.uv + float2(scrollX * 0.2, scrollY * 0.2);
                half h = SAMPLE_TEXTURE2D(_ParallaxMap, sampler_ParallaxMap, uvParallax).r;

                // simple parallax offset (tangent-space view dir)
                float2 parallaxOffset = ParallaxOffset1Step(h, _Parallax, IN.viewDirTS);

                float2 uvMain = IN.uv + parallaxOffset + float2(scrollX, scrollY);
                float2 uv1 = IN.uv + parallaxOffset + float2(scrollX, scrollY);
                float2 uv2 = IN.uv + parallaxOffset + float2(scrollX2, scrollY2);

                half3 nrml  = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv1));
                half3 nrml2 = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv2));
                half3 nrml3 = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap2, sampler_BumpMap2, IN.uv));

                half3 finalNormalTS = lerp(nrml + (nrml2 * half3(1,1,0)), nrml3, _BumpLerp);
                finalNormalTS = normalize(finalNormalTS);

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvMain);

                // build TBN and bring normal to world space
                float3 bitangentWS = cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w;
                float3x3 TBN = float3x3(IN.tangentWS.xyz, bitangentWS, IN.normalWS);
                float3 normalWS = normalize(mul(finalNormalTS, TBN));

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = normalize(GetWorldSpaceViewDir(IN.positionWS));
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                inputData.fogCoord = 0;
                inputData.vertexLighting = 0;
                inputData.bakedGI = SampleSH(normalWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = tex.rgb * _Color.rgb;
                surfaceData.metallic = 0;
                surfaceData.specular = 0;
                surfaceData.smoothness = _Smth;
                surfaceData.normalTS = finalNormalTS;
                surfaceData.emission = tex.rgb * _Color.rgb * _Emis;
                surfaceData.occlusion = 1;
                surfaceData.alpha = 1;

                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }

        // Needed so the water still casts shadows correctly under URP
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack "Universal Render Pipeline/Lit"
}
