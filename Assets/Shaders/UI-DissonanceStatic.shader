// =====================================================================
//  UI/Dissonance Static
//  Corruption creeping along a health bar.
//
//  Used by TWO Images driven from the same component:
//
//   1. BODY  (_Spill = 0) - inside the fill mask, exactly the fill rect.
//      Draws the corruption itself.
//   2. SPILL (_Spill = 1) - OUTSIDE the mask, a padded rect around the
//      bar. Draws only detached blocks and tear streaks beyond the bar
//      edges. _PadX/_PadY tell it where the bar sits inside its own rect
//      so both agree on where the front is; the component computes them.
//
//    _Fill       = stacks / maxHealth       (position of the front)
//    _Proximity  = stacks / currentHealth   (how close to lethal)
// =====================================================================
Shader "UI/Dissonance Static"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Header(Drive)]
        _Fill          ("Fill (0-1)",           Range(0, 1)) = 0
        _Proximity     ("Proximity To Lethal",  Range(0, 1)) = 0
        _Invert        ("Fill From Right",      Range(0, 1)) = 0

        [Header(Colour)]
        _Color         ("Static Tint",          Color) = (1, 1, 1, 1)
        _HotColor      ("Lethal Tint",          Color) = (1, 0.35, 0.4, 1)
        _EdgeColor     ("Leading Edge",         Color) = (1, 1, 1, 1)
        _Chroma        ("RGB Chroma",           Range(0, 1)) = 0.65
        _Subpixel      ("Pure RGB Cell Chance", Range(0, 0.5)) = 0.12
        _Whites        ("White Bias",           Range(0.2, 4)) = 1.7
        _Floor         ("Brightness Floor",     Range(0, 1)) = 0.25
        _DarkHoles     ("Dark Cells As Holes",  Range(0, 1)) = 0.85
        _Separation    ("RGB Separation",       Range(0, 0.2)) = 0.05
        _Contrast      ("Static Contrast",      Range(0.5, 4)) = 1.5

        [Header(Static)]
        _NoiseScale    ("Noise Cells (XY)",     Vector) = (90, 7, 0, 0)
        _StepRate      ("Time Step (fps feel)", Float) = 16
        _Sparse        ("Sparseness",           Range(0, 1)) = 0.25
        _Crunch        ("Cell Hardness",        Range(0, 1)) = 0.8
        _Scanline      ("Scanline Darkening",   Range(0, 1)) = 0.2

        [Header(Edge)]
        _EdgeWidth     ("Edge Raggedness",      Range(0, 0.2)) = 0.045
        _EdgeRoughness ("Edge Roughness",       Float) = 7
        _EdgeSpeed     ("Edge Churn Speed",     Float) = 1.4
        _Feather       ("Edge Feather",         Range(0.001, 0.1)) = 0.012
        _WispReach     ("Wisp Reach",           Range(0, 0.25)) = 0.06
        _EdgeGlow      ("Leading Edge Glow",    Range(0, 0.08)) = 0.018

        [Header(Spill Outside Bar)]
        [Toggle] _Spill ("Spill Mode",          Float) = 0
        _PadX          ("Pad X (fraction)",     Range(0, 0.5)) = 0
        _PadY          ("Pad Y (fraction)",     Range(0, 0.5)) = 0
        _SpillAmount   ("Spill Density",        Range(0, 1)) = 0.35
        _SpillReach    ("Spill Reach",          Range(0.05, 3)) = 1
        _SpillFocus    ("Focus Near Front",     Range(0.02, 1)) = 0.25
        _StreakChance  ("Tear Streak Chance",   Range(0, 1)) = 0.3
        _StreakLength  ("Tear Streak Length",   Range(0, 1)) = 0.45

        [Header(Aggression)]
        _Tear          ("Row Tearing",          Range(0, 0.2)) = 0.05
        _Rows          ("Tear Rows",            Float) = 9
        _Pulse         ("Lethal Pulse",         Range(0, 1)) = 0.35
        _PulseSpeed    ("Lethal Pulse Speed",   Float) = 5
        _FlashChance   ("Flash Chance",         Range(0, 1)) = 0.15

        // --- UGUI plumbing ---
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
            Name "DissonanceStatic"
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

            float4 _Color, _HotColor, _EdgeColor, _NoiseScale;
            float  _Fill, _Proximity, _Invert;
            float  _Chroma, _Subpixel, _Whites, _Floor, _DarkHoles;
            float  _Separation, _Contrast;
            float  _StepRate, _Sparse, _Crunch, _Scanline;
            float  _EdgeWidth, _EdgeRoughness, _EdgeSpeed, _Feather, _WispReach, _EdgeGlow;
            float  _Spill, _PadX, _PadY, _SpillAmount, _SpillReach, _SpillFocus;
            float  _StreakChance, _StreakLength;
            float  _Tear, _Rows, _Pulse, _PulseSpeed, _FlashChance;

            // ---------------------------------------------------------
            float hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float hash21(float2 p)
            {
                float3 p3 = frac(p.xyx * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float3 hash23(float2 p)
            {
                float3 p3 = frac(p.xyx * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yxz + 33.33);
                return frac((p3.xxy + p3.yzz) * p3.zyx);
            }

            float vnoise2(float2 x)
            {
                float2 i = floor(x);
                float2 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i + float2(0, 0));
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // ---------------------------------------------------------
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex        = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord      = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color         = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float t    = _Time.y;
                float ts   = floor(t * max(_StepRate, 1.0)) / max(_StepRate, 1.0);
                float prox = saturate(_Proximity);

                // --- map this Image's UV into BAR space ------------------
                // For the body image pad is 0 so this is a no-op. For the
                // spill image it undoes the padding, so both images agree
                // on where the front sits.
                float2 uv = IN.texcoord;
                float2 pad = float2(_PadX, _PadY);
                float2 barUV = (uv - pad) / max(1.0 - 2.0 * pad, 1e-4);

                barUV.x = lerp(barUV.x, 1.0 - barUV.x, saturate(_Invert));

                float insideBar = step(0.0, barUV.x) * step(barUV.x, 1.0)
                                * step(0.0, barUV.y) * step(barUV.y, 1.0);

                // --- row tearing -----------------------------------------
                float row  = floor(barUV.y * _Rows);
                float tear = (hash11(row + ts * 17.0) * 2.0 - 1.0) * _Tear * prox;
                tear *= step(0.55, hash11(row * 3.1 + ts * 5.0));
                barUV.x += tear;

                // --- ragged leading edge ---------------------------------
                float en = vnoise2(float2(barUV.y * _EdgeRoughness, ts * _EdgeSpeed)) - 0.5;
                float front = _Fill + en * _EdgeWidth * (0.6 + prox * 0.8);

                // --- cells (shared grid across both images) --------------
                float2 cell = floor(barUV * _NoiseScale.xy);

                // --- TV snow colour --------------------------------------
                // Independent per-channel noise, so cells are genuinely
                // coloured rather than a tinted greyscale.
                float3 rgb  = hash23(cell + ts * 91.0);
                float  grey = dot(rgb, float3(0.333, 0.334, 0.333));
                float3 lum  = lerp(grey.xxx, rgb, _Chroma);

                // push toward white: real static is mostly bright
                lum = 1.0 - pow(saturate(1.0 - lum), _Whites);
                lum = saturate((lum - 0.5) * _Contrast + 0.5);
                lum = max(lum, _Floor);

                // occasional pure R / G / B cells (dead subpixels)
                float  pr = hash21(cell * 3.37 + ts * 7.0);
                float3 pureCol = float3(step(pr, 0.333),
                                        step(0.333, pr) * step(pr, 0.666),
                                        step(0.666, pr));
                float  pure = step(1.0 - _Subpixel * (0.5 + prox), hash21(cell + ts * 23.0));
                lum = lerp(lum, pureCol, pure);

                // separation fringe near the front
                float sep = _Separation * (0.35 + prox);
                float nr = hash21(floor((barUV + float2(sep, 0)) * _NoiseScale.xy) + ts * 91.0);
                float nb = hash21(floor((barUV - float2(sep, 0)) * _NoiseScale.xy) + ts * 91.0);
                lum.r = lerp(lum.r, max(lum.r, nr), 0.5);
                lum.b = lerp(lum.b, max(lum.b, nb), 0.5);

                float cellBright = dot(lum, float3(0.333, 0.334, 0.333));

                // --- coverage --------------------------------------------
                float coverSoft = smoothstep(front + _Feather, front - _Feather, barUV.x);
                float coverHard = step(barUV.x, front);
                float cover = lerp(coverSoft, coverHard, _Crunch);

                float hole = step(_Sparse * (1.0 - prox * 0.6), hash21(cell + ts * 13.0));
                cover *= lerp(1.0, hole, 0.55);

                // wisps ahead of the front
                float ahead    = saturate((barUV.x - front) / max(_WispReach, 1e-4));
                float wispRand = hash21(cell * 1.7 + ts * 41.0);
                float wisp     = step(0.82 - prox * 0.25, wispRand) * (1.0 - ahead) * step(0.0, ahead);
                cover = saturate(max(cover, wisp * 0.85));

                float bodyAlpha = cover * insideBar;

                // --- spill: detached blocks + tear streaks outside the bar
                float spillAlpha = 0.0;
                if (_Spill > 0.5)
                {
                    // how far outside the bar we are, in bar-UV units
                    float2 outDist = max(max(-barUV, barUV - 1.0), 0.0);
                    float  d       = max(outDist.x, outDist.y);
                    float  falloff = 1.0 - saturate(d / max(_SpillReach, 1e-4));

                    // artefacts cluster around the advancing front
                    float nearFront = 1.0 - saturate(abs(barUV.x - front) / max(_SpillFocus, 1e-4));

                    float chance = _SpillAmount * (0.25 + prox * 1.35)
                                 * falloff * lerp(0.25, 1.0, nearFront);

                    float blockRand = hash21(cell * 2.1 + floor(ts * 9.0) * 31.0);
                    float blocks = step(1.0 - chance, blockRand);

                    // horizontal tear streaks shooting out of the bar edges
                    float srow   = floor(barUV.y * _Rows);
                    float sPick  = hash11(srow * 5.7 + floor(ts * 6.0) * 13.0);
                    float sOn    = step(1.0 - _StreakChance * prox, sPick);
                    float sLen   = _StreakLength * hash11(srow + ts * 3.0);
                    float inRowY = step(0.0, barUV.y) * step(barUV.y, 1.0);
                    float streak = sOn * inRowY
                                 * step(outDist.x, sLen) * step(0.0001, outDist.x)
                                 * step(0.5, hash21(cell + ts * 55.0));

                    spillAlpha = saturate(max(blocks, streak)) * (1.0 - insideBar);
                }

                float alpha = max(bodyAlpha * (1.0 - _Spill), spillAlpha);

                // --- dark cells punch through as holes, not black paint ---
                alpha *= lerp(1.0, cellBright, _DarkHoles);

                // --- colour ------------------------------------------------
                float3 tint = lerp(_Color.rgb, _HotColor.rgb, prox * prox);
                float3 col  = lum * tint;

                // leading edge highlight (body only)
                float glow = (1.0 - smoothstep(0.0, max(_EdgeGlow, 1e-4), abs(barUV.x - front)))
                           * insideBar * (1.0 - _Spill);
                col = lerp(col, _EdgeColor.rgb, glow * 0.85);
                alpha = saturate(alpha + glow * 0.9 * step(0.001, _Fill));

                float sl = 0.5 + 0.5 * sin(barUV.y * _NoiseScale.y * 3.14159 * 2.0);
                col *= 1.0 - _Scanline * sl;

                col *= 1.0 + sin(t * _PulseSpeed) * _Pulse * prox;
                float flash = step(1.0 - _FlashChance * prox, hash11(floor(t * 11.0)));
                col = lerp(col, _EdgeColor.rgb, flash * 0.5 * prox);

                alpha *= _Color.a * IN.color.a;
                alpha *= step(0.0005, _Fill + _WispReach);

                float4 outCol = float4(col * IN.color.rgb, saturate(alpha));

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
