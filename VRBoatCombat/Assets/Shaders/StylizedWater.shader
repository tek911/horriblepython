Shader "VRBoatCombat/StylizedWater"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.0, 0.5, 1.0, 0.8)
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
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }

        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard alpha vertex:vert
        #pragma target 3.0

        // Mobile optimization
        #pragma multi_compile_fog

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
            float4 screenPos;
            float eyeDepth;
        };

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

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float time = _Time.y;
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

            // Calculate wave displacement
            float displacement = wave(worldPos.xz, time);
            v.vertex.y += displacement;

            // Calculate normal for waves
            float delta = 0.1;
            float dX = wave(worldPos.xz + float2(delta, 0), time) - displacement;
            float dZ = wave(worldPos.xz + float2(0, delta), time) - displacement;

            v.normal = normalize(float3(-dX, delta, -dZ));

            // Store eye depth for foam
            o.eyeDepth = -UnityObjectToViewPos(v.vertex).z;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float time = _Time.y;

            // Base water color
            half4 waterColor = _Color;

            // Depth-based color blending
            float depthFactor = saturate(IN.eyeDepth / _DepthFade);
            waterColor = lerp(_Color, _DeepColor, depthFactor);

            // Fresnel effect
            float fresnel = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            fresnel = pow(fresnel, 3.0);
            waterColor = lerp(waterColor, _FresnelColor, fresnel * 0.5);

            // Foam (using noise)
            float foamNoise = noise(IN.worldPos.xz * 5.0 + time * 0.5);
            float foam = step(_FoamCutoff, foamNoise) * _FoamAmount;
            waterColor = lerp(waterColor, _FoamColor, foam);

            // Output
            o.Albedo = waterColor.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = waterColor.a;
            o.Normal = UnpackNormal(float4(0, 0, 1, 1)); // Use vertex normal
        }
        ENDCG
    }

    FallBack "Mobile/Diffuse"
}
