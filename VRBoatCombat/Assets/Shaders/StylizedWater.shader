Shader "VRBoatCombat/StylizedWater"
{
    Properties
    {
        [MainColor] _Color ("Water Color", Color) = (0.0, 0.5, 1.0, 0.8)
        _DeepColor ("Deep Water Color", Color) = (0.0, 0.2, 0.5, 1.0)
        _FresnelColor ("Fresnel Color", Color) = (0.8, 0.9, 1.0, 1.0)

        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveHeight ("Wave Height", Float) = 0.5
        _WaveFrequency ("Wave Frequency", Float) = 1.0

        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamAmount ("Foam Amount", Range(0, 1)) = 0.3
        _FoamCutoff ("Foam Cutoff", Range(0, 1)) = 0.5

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _DepthFade ("Depth Fade", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }

        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _DeepColor;
                half4 _FresnelColor;
                half4 _FoamColor;

                half _WaveSpeed;
                half _WaveHeight;
                half _WaveFrequency;
                half _FoamAmount;
                half _FoamCutoff;
                half _Glossiness;
                half _Metallic;
                half _DepthFade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                float eyeDepth : TEXCOORD4;
                float fogFactor : TEXCOORD5;
            };

            // Simple noise function
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Wave calculation
            float wave(float2 pos, float time)
            {
                float w = 0.0;

                // Multiple wave layers for complexity
                w += sin(pos.x * _WaveFrequency + time * _WaveSpeed) * _WaveHeight;
                w += sin(pos.y * _WaveFrequency * 0.7 + time * _WaveSpeed * 1.2) * _WaveHeight * 0.5;
                w += sin((pos.x + pos.y) * _WaveFrequency * 0.5 + time * _WaveSpeed * 0.8) * _WaveHeight * 0.3;

                return w * 0.1;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                float time = _Time.y;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

                // Calculate wave displacement
                float displacement = wave(positionWS.xz, time);
                positionWS.y += displacement;

                // Calculate normal for waves
                float delta = 0.1;
                float dX = wave(positionWS.xz + float2(delta, 0), time) - displacement;
                float dZ = wave(positionWS.xz + float2(0, delta), time) - displacement;
                float3 normalWS = normalize(float3(-dX, delta, -dZ));

                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                output.uv = input.uv;

                // Store eye depth for foam
                float4 positionVS = TransformWorldToView(positionWS);
                output.eyeDepth = -positionVS.z;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;

                // Base water color
                half4 waterColor = _Color;

                // Depth-based color blending
                float depthFactor = saturate(input.eyeDepth / _DepthFade);
                waterColor = lerp(_Color, _DeepColor, depthFactor);

                // Fresnel effect
                float3 viewDirWS = normalize(input.viewDirWS);
                float3 normalWS = normalize(input.normalWS);
                float fresnel = 1.0 - saturate(dot(viewDirWS, normalWS));
                fresnel = pow(fresnel, 3.0);
                waterColor = lerp(waterColor, _FresnelColor, fresnel * 0.5);

                // Foam (using noise)
                float foamNoise = noise(input.positionWS.xz * 5.0 + time * 0.5);
                float foam = step(_FoamCutoff, foamNoise) * _FoamAmount;
                waterColor = lerp(waterColor, _FoamColor, foam);

                // Apply lighting from main light
                Light mainLight = GetMainLight();
                half3 lighting = mainLight.color * saturate(dot(normalWS, mainLight.direction));
                waterColor.rgb *= 0.5 + lighting * 0.5;

                // Specular highlight
                half3 halfDir = normalize(mainLight.direction + viewDirWS);
                half spec = pow(saturate(dot(normalWS, halfDir)), 32.0) * _Glossiness;
                waterColor.rgb += spec * mainLight.color;

                // Apply fog
                waterColor.rgb = MixFog(waterColor.rgb, input.fogFactor);

                return waterColor;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
