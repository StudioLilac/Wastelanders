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

        // NEW
        _NoiseOffset ("Noise Offset (XYZ)", Vector) = (0,0,0,0)
        _WarpScale ("Warp Scale", Float) = 0.6
    }

    SubShader
    {
        Tags
        {
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
            float4 _NoiseOffset;
            float _WarpScale;

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
                float time = _Time.y;

                // Base UV with directional wind
                float2 uv = i.uv;
                uv += _Speed.xy * time;

                // --- WARP NOISE (turbulence) ---
                float2 warpUV = uv * _WarpScale + _NoiseOffset.xy;
                float2 warp =
                    tex2D(_MainTex, warpUV).rg - 0.5;

                uv += warp * _DistortStrength;

                // --- FAKE 3D NOISE ---
                float nA = tex2D(_MainTex, uv + _NoiseOffset.xy).r;
                float nB = tex2D(_MainTex, uv * 0.5 + _NoiseOffset.zw).r;

                float density = lerp(nA, nB, 0.5);

                // Shape fog density
                density = smoothstep(_DensityMin, _DensityMax, density);
                density *= density; // soften

                return fixed4(_Color.rgb, density * _Color.a);
            }
            ENDCG
        }
    }
}
