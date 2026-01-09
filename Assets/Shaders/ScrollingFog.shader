Shader "Blizzard/ScrollingFog2D"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        _Color ("Fog Color", Color) = (0.9,0.95,1,0.12)

        _Speed ("Scroll Speed", Vector) = (0.15, 0.05, 0, 0)
        _Tiling ("Tiling", Float) = 1

        _DensityMin ("Density Min", Range(0,1)) = 0.35
        _DensityMax ("Density Max", Range(0,1)) = 0.7

        _DistortStrength ("Distortion Strength", Range(0,0.5)) = 0.15
    }

    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float4 _Speed;
            float _Tiling;
            float _DensityMin;
            float _DensityMax;
            float _DistortStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _Tiling;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Time-driven offsets (very small)
                float t = _Time.y * 0.1;

                // Sample noise to create local distortion
                float n1 = tex2D(_MainTex, uv + float2(t, 0)).r;
                float n2 = tex2D(_MainTex, uv + float2(0, t)).r;

                // Distort UVs locally instead of translating them
                float2 warpedUV = uv + (float2(n1, n2) - 0.5) * 0.15;

                // Final noise sample
                float density = tex2D(_MainTex, warpedUV).r;

                // Shape into fog
                density = smoothstep(0.4, 0.7, density);
                density *= density;

                return fixed4(_Color.rgb, density * _Color.a);
            }
            ENDCG
        }
    }
}
