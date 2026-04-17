Shader "Custom/CutoutShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (.5,.5,.5,1)
        _MainTex ("Base (RGB)", 2D) = "white" { }
        _Cutoff ("Alpha Cutoff", Range (0.0, 1.0)) = 0.5
    }

    CGINCLUDE
#include "UnityCG.cginc"
    ENDCG

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
        }
        
        LOD 100

        Pass
        {
            Name "Cutout"

            CGPROGRAM
            #pragma surface surf Lambert alpha

            fixed4 _Color;
            sampler2D _MainTex;
            fixed _Cutoff;

            struct Input
            {
                float2 uv_MainTex;
            };

            void surf(Input IN, inout SurfaceOutput o)
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Alpha = c.a;
                clip(c.a - _Cutoff);
            }
            ENDCG
        }
    }
}
