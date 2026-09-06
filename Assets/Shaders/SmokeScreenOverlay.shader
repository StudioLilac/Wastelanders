// =====================================================================
//  UI/Smoke Screen Overlay  -  procedural screen-space smoke for UGUI
//  BUILT-IN RENDER PIPELINE
//
//  Designed for a full-screen Image sitting on top of your VN UI.
//  Nothing is sampled from the world: the density field is 3D value
//  noise in UV space, animated continuously so it keeps rolling.
//
//  _Amount (0..1) drives the roll-in. It grows outward from _Origin,
//  so you can spawn it wherever the canister lands on screen.
//  Animate _Amount from script (see SmokeScreenOverlay.cs).
// =====================================================================
Shader "UI/Smoke Screen Overlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Mask Texture (optional)", 2D) = "white" {}

        [Header(Reveal)]
        _Amount        ("Amount (0-1)",        Range(0, 1)) = 0
        _Origin        ("Origin (UV xy)",      Vector) = (0.5, 0.35, 0, 0)
        _Feather       ("Reveal Feather",      Range(0.01, 1.5)) = 0.55
        _FullCover     ("Solidify At Full",    Range(0, 1)) = 0
        _Opacity       ("Max Opacity",         Range(0, 1)) = 1

        [Header(Color)]
        _Color         ("Smoke Tint",          Color) = (1, 1, 1, 1)
        _ShadowColor   ("Shadow Tint",         Color) = (0.62, 0.65, 0.70, 1)
        _LightDir      ("Light Dir (XY)",      Vector) = (0.4, 0.85, 0.2, 0)
        _Ambient       ("Ambient",             Range(0, 1)) = 0.45
        _LightBoost    ("Light Contrast",      Range(0.1, 4)) = 1.5
        _Absorption    ("Self Shadowing",      Range(0, 3)) = 1.0

        [Header(Shape)]
        _Density       ("Density",             Range(0, 4)) = 1.8
        _NoiseScale    ("Noise Scale",         Float) = 3.5
        _Detail        ("Detail (octave gain)", Range(0.2, 0.75)) = 0.5
        _Warp          ("Domain Warp",         Range(0, 2)) = 0.75
        _EdgeSoftness  ("Edge Softness",       Range(0.01, 1)) = 0.35
        _Aspect        ("Aspect Override (0 = auto)", Float) = 0

        [Header(Motion)]
        _Swirl         ("Boil Speed",          Float) = 0.16
        _Rise          ("Rise Speed",          Float) = 0.05
        _Drift         ("Drift (XY)",          Vector) = (0.02, 0, 0, 0)

        // --- standard UGUI plumbing, leave alone ---
        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID",         Float) = 0
        _StencilOp        ("Stencil Operation",  Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask        ("Color Mask",         Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "SmokeOverlay"
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ClipRect;

            float4 _Color;
            float4 _ShadowColor;
            float4 _LightDir;
            float4 _Origin;
            float4 _Drift;
            float  _Amount;
            float  _Feather;
            float  _FullCover;
            float  _Opacity;
            float  _Ambient;
            float  _LightBoost;
            float  _Absorption;
            float  _Density;
            float  _NoiseScale;
            float  _Detail;
            float  _Warp;
            float  _EdgeSoftness;
            float  _Aspect;
            float  _Swirl;
            float  _Rise;

            // ---------------------------------------------------------
            //  Noise
            // ---------------------------------------------------------
            #define NOISE_ROT float3x3( 0.00,  0.80,  0.60, \
                                       -0.80,  0.36, -0.48, \
                                       -0.60, -0.48,  0.64)

            float hash13(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.zyx + 31.32);
                return frac((p.x + p.y) * p.z);
            }

            float valueNoise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);

                float n000 = hash13(i + float3(0, 0, 0));
                float n100 = hash13(i + float3(1, 0, 0));
                float n010 = hash13(i + float3(0, 1, 0));
                float n110 = hash13(i + float3(1, 1, 0));
                float n001 = hash13(i + float3(0, 0, 1));
                float n101 = hash13(i + float3(1, 0, 1));
                float n011 = hash13(i + float3(0, 1, 1));
                float n111 = hash13(i + float3(1, 1, 1));

                return lerp(lerp(lerp(n000, n100, f.x), lerp(n010, n110, f.x), f.y),
                            lerp(lerp(n001, n101, f.x), lerp(n011, n111, f.x), f.y), f.z);
            }

            float fbm5(float3 p, float gain)      // main density field
            {
                float amp = 0.5, sum = 0.0, norm = 0.0;
                [unroll]
                for (int k = 0; k < 5; k++)
                {
                    sum  += amp * valueNoise(p);
                    norm += amp;
                    p     = mul(NOISE_ROT, p) * 2.02;
                    amp  *= gain;
                }
                return sum / max(norm, 1e-4);
            }

            float fbm3(float3 p, float gain)      // cheaper, for the shadow probe
            {
                float amp = 0.5, sum = 0.0, norm = 0.0;
                [unroll]
                for (int k = 0; k < 3; k++)
                {
                    sum  += amp * valueNoise(p);
                    norm += amp;
                    p     = mul(NOISE_ROT, p) * 2.02;
                    amp  *= gain;
                }
                return sum / max(norm, 1e-4);
            }

            float fbm2(float3 p)                  // domain warp field
            {
                float n = valueNoise(p) * 0.65;
                n += valueNoise(mul(NOISE_ROT, p) * 2.03) * 0.35;
                return n;
            }

            // ---------------------------------------------------------
            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                float4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.worldPosition = IN.vertex;
                OUT.vertex        = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord      = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color         = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float t = _Time.y;

                // keep puffs circular regardless of screen shape
                float asp = (_Aspect > 0.001) ? _Aspect : (_ScreenParams.x / max(_ScreenParams.y, 1.0));
                float2 uv  = IN.texcoord;
                float2 auv = float2(uv.x * asp, uv.y);

                // --- animated noise domain (pure UV space) ---
                float3 p = float3(auv * _NoiseScale, t * _Swirl);
                p.xy += _Drift.xy * _NoiseScale * t;
                p.y  -= _Rise * _NoiseScale * t;

                float3 warp = float3(fbm2(p + 13.1),
                                     fbm2(p +  7.3),
                                     fbm2(p +  3.7)) - 0.5;
                float3 pw = p + warp * _Warp;

                float n = fbm5(pw, _Detail);

                // optionally go fully solid at the end of the roll-in
                n = lerp(n, 1.0, saturate((_Amount - 0.75) * 4.0) * _FullCover);

                // --- reveal: expands outward from _Origin as _Amount rises ---
                float2 org  = float2(_Origin.x * asp, _Origin.y);
                float  dist = length(auv - org) / max(length(float2(asp, 1.0)), 1e-4);
                float  reveal = saturate((_Amount * (1.0 + _Feather) - dist) / max(_Feather, 1e-3));

                // --- density, eroded by the reveal front so edges are wispy ---
                float density = saturate(n * _Density);
                density = (density - (1.0 - reveal)) / max(_EdgeSoftness, 1e-3);
                float alpha = smoothstep(0.0, 1.0, saturate(density)) * _Opacity;

                // --- fake self shadowing for volume ---
                float3 L    = normalize(_LightDir.xyz + 1e-5);
                float  occl = saturate(fbm3(pw + L * 0.5, _Detail) * _Absorption);
                float  lit  = saturate(_Ambient + pow(1.0 - occl, _LightBoost));

                float3 col = lerp(_ShadowColor.rgb, _Color.rgb, lit);

                fixed4 outCol = fixed4(col, alpha) * IN.color;
                outCol.a *= tex2D(_MainTex, uv).a;   // optional soft mask sprite

                #ifdef UNITY_UI_CLIP_RECT
                outCol.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(outCol.a - 0.001);
                #endif

                return outCol;
            }
            ENDCG
        }
    }
    Fallback Off
}
