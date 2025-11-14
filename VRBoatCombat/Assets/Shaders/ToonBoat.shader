Shader "VRBoatCombat/ToonBoat"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.01

        [Header(Toon Shading)]
        _ToonRamp ("Toon Ramp (RGB)", 2D) = "white" {}
        _RampThreshold ("Ramp Threshold", Range(0, 1)) = 0.5
        _RampSmooth ("Ramp Smoothness", Range(0, 1)) = 0.1

        [Header(Rim Lighting)]
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.1, 10)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 1)) = 0.5

        [Header(Emission)]
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        LOD 200

        // Outline pass
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            half _OutlineWidth;
            half4 _OutlineColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;

                // Expand vertices along normals for outline
                float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float2 offset = TransformViewToProjection(norm.xy);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.pos.xy += offset * o.pos.z * _OutlineWidth;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        // Main toon shading pass
        CGPROGRAM
        #pragma surface surf Toon fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _ToonRamp;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
        };

        half4 _Color;
        half4 _RimColor;
        half4 _EmissionColor;
        half _RampThreshold;
        half _RampSmooth;
        half _RimPower;
        half _RimIntensity;

        // Custom toon lighting model
        half4 LightingToon(SurfaceOutput s, half3 lightDir, half atten)
        {
            // Diffuse lighting
            half NdotL = dot(s.Normal, lightDir);

            // Toon ramp with smooth step
            half ramp = smoothstep(_RampThreshold - _RampSmooth, _RampThreshold + _RampSmooth, NdotL);

            // Apply ramp texture if available
            half3 rampColor = tex2D(_ToonRamp, float2(ramp, 0.5)).rgb;

            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * rampColor * atten;
            c.a = s.Alpha;

            return c;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            // Albedo
            half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            // Rim lighting
            half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            rim = pow(rim, _RimPower) * _RimIntensity;
            o.Emission = _RimColor.rgb * rim + _EmissionColor.rgb;
        }
        ENDCG
    }

    FallBack "Mobile/Diffuse"
}
