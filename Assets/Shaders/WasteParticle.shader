// =====================================================================
//  Impact/WasteParticlePixelRamp  -  grayscale chunk art, coloured in engine
//
//  Built-in renderer. For art authored as GRAYSCALE, where each grey level
//  is a palette slot rather than a final colour.
//
//  The grey value indexes a 256x1 ramp texture. Point-filtered, so every
//  distinct grey in your art maps to exactly one ramp texel: you get out
//  precisely as many colours as you drew shades, with no interpolation
//  inventing values your palette never had.
//
//  A flat Tint multiply (the other pixel shader) can only darken toward
//  black and can't shift hue, so highlights and shadows stay the same hue
//  at different brightness. Pixel art palettes almost always ramp hue as
//  well as value -- shadows toward blue, highlights toward pink. That's
//  what this buys you.
//
//  IMPORT SETTINGS -- these two differ from each other on purpose:
//    _MainTex : sRGB OFF. The grey is an INDEX, not a displayed colour.
//               With sRGB on in a Linear project the sampler gamma-decodes
//               it and grey 128 would look up ramp texel 55 instead of 128.
//    _Palette : sRGB ON. That one really is colour.
//
//  Age comes from vertex alpha, so Colour over Lifetime with an alpha ramp
//  of 1 -> 0 is REQUIRED. It drives the crumble, not the output alpha.
// =====================================================================
Shader "Impact/WasteParticlePixelRamp"
{
    Properties
    {
        _MainTex ("Chunk Sheet (grayscale index, sRGB OFF)", 2D) = "white" {}
        _Palette ("Palette Ramp 256x1 (sRGB ON)", 2D) = "white" {}
        _Cutoff  ("Alpha Cutoff", Range(0,1)) = 0.5

        [Header(Colour)]
        _Tint     ("Tint", Color) = (1,1,1,1)
        _Emission ("Emission Boost", Range(0,4)) = 1.0

        [Header(Crumble)]
        _CrumbleCluster   ("Cluster (source px)", Range(1,8)) = 1
        _CrumbleEdgeWidth ("Edge Width", Range(0.001,0.5)) = 0.18
        _CrumbleEdgeColor ("Edge Colour", Color) = (1.0, 0.62, 1.0, 1)
        _CrumbleEdgeBoost ("Edge Boost", Range(0,4)) = 1.5

        _Opacity ("Opacity", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "RenderType"      = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType"     = "Plane"
        }

        Cull Off
        ZWrite Off
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                fixed4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;   // zw = sheet size in pixels

            sampler2D _Palette;

            float  _Cutoff;
            fixed4 _Tint;
            float  _Emission;
            float  _CrumbleCluster, _CrumbleEdgeWidth, _CrumbleEdgeBoost;
            fixed4 _CrumbleEdgeColor;
            float  _Opacity;

            float hash21(float2 p)
            {
                float3 p3 = frac(p.xyx * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                clip(tex.a - _Cutoff);

                // Colour over Lifetime alpha 1 -> 0 becomes age 0 -> 1
                float age = saturate(1.0 - i.color.a);

                // Snap to whole source texels so pixels leave the chunk intact
                // instead of eroding into a sub-pixel fringe.
                float2 texel = floor(i.uv * _MainTex_TexelSize.zw
                                     / max(_CrumbleCluster, 1.0));

                float n = hash21(texel) * 0.93 + 0.07;

                float alive = n - age;
                clip(alive);

                // Constant within a texel, so whole pixels light up before they go.
                float edge = 1.0 - saturate(alive / max(_CrumbleEdgeWidth, 1e-4));

                // --- palette lookup -----------------------------------
                // Grayscale source, so r == g == b. Using .r avoids a luma
                // weighting that would shift the index off your authored greys.
                float idx = saturate(tex.r);
                float3 col = tex2D(_Palette, float2(idx, 0.5)).rgb;

                col *= _Tint.rgb * i.color.rgb * _Emission;
                col = lerp(col, _CrumbleEdgeColor.rgb * _CrumbleEdgeBoost, edge);

                // Solid. The chunk erodes away, it never fades away.
                return fixed4(col, _Opacity);
            }
            ENDCG
        }
    }

    Fallback Off
}
