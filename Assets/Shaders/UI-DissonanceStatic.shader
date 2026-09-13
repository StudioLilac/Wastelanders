// =====================================================================
//  UI/Dissonance Compact
//  Monochrome block static eating a health bar.
//
//  Rewrite of UI/Dissonance Static with the knob count cut hard and one
//  rule enforced throughout: EVERYTHING IS EVALUATED PER CELL, AND ALPHA
//  IS BINARY.
//
//  Partial alpha was where the grey came from. A cell at 45% alpha over
//  a red fill is pink-grey mud, not static. Here a cell is either fully
//  ink or fully snow or fully absent. No feather, no smoothstep, no
//  contrast curve, no greys unless you deliberately tint one in.
//
//  Evaluating per cell also makes the advancing front step in whole
//  cells, so the wave reads as blocky corruption coordinated with the
//  fill rather than a smooth curve cutting through pixels.
//
//    _Fill       = stacks / maxHealth       (front position)
//    _Proximity  = stacks / currentHealth   (how close to lethal)
//
//  Proximity automatically drives coverage, white mix, wave size and
//  ambient scatter, so there is one dial for "how bad is it" instead of
//  six. Same component drives this as the old shader; the properties it
//  writes that this one lacks are simply ignored.
// =====================================================================
Shader "UI/Dissonance Compact"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Header(Drive)]
        _Fill        ("Fill (0-1)",          Range(0, 1)) = 0
        _Proximity   ("Proximity To Lethal", Range(0, 1)) = 0
        _Invert      ("Fill From Right",     Range(0, 1)) = 0

        [Header(Palette)]
        _Ink         ("Ink (base)",          Color) = (0.03, 0.03, 0.04, 1)
        _Snow        ("Snow (grain)",        Color) = (1, 1, 1, 1)
        _WhiteMix    ("White Proportion",    Range(0, 1)) = 0.28

        [Header(Corruption)]
        _Coverage    ("Coverage",            Range(0, 1)) = 0.8
        _Ambient     ("Ambient Scatter",     Range(0, 1)) = 0.18
        _NoiseScale  ("Cells (XY)",          Vector) = (45, 9, 0, 0)

        [Header(Wave)]
        _EdgeWave    ("Wave Amount",         Range(0, 0.4)) = 0.07
        _WaveFreq    ("Wave Frequency",      Float) = 1.5
        _WaveSpeed   ("Wave Speed",          Float) = 0.9

        [Header(Timing)]
        _StepRate    ("Update Rate",         Float) = 6
        _Hold        ("Cell Hold Variance",  Range(0, 1)) = 0.7

        [Header(Spill Outside Bar)]
        [Toggle] _Spill ("Spill Mode",       Float) = 0
        _PadX        ("Pad X (fraction)",    Range(0, 0.5)) = 0
        _PadY        ("Pad Y (fraction)",    Range(0, 0.5)) = 0
        _SpillAmount ("Spill Density",       Range(0, 1)) = 0.35
        _SpillReach  ("Spill Reach",         Range(0.05, 2)) = 1

        // --- UGUI plumbing ---
        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID",         Float) = 0
        _StencilOp        ("Stencil Operation",  Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask        ("Color Mask",         Float) = 15
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
            Name "DissonanceCompact"
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ClipRect;

            float4 _Ink, _Snow, _NoiseScale;
            float  _Fill, _Proximity, _Invert;
            float  _WhiteMix, _Coverage, _Ambient;
            float  _EdgeWave, _WaveFreq, _WaveSpeed;
            float  _StepRate, _Hold;
            float  _Spill, _PadX, _PadY, _SpillAmount, _SpillReach;

            float hash21(float2 p)
            {
                float3 p3 = frac(p.xyx * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float vnoise(float2 x)
            {
                float2 i = floor(x);
                float2 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

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
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float t    = _Time.y;
                float prox = saturate(_Proximity);

                // --- into bar space (pad is 0 for the body image) ---------
                float2 pad   = float2(_PadX, _PadY);
                float2 barUV = (IN.texcoord - pad) / max(1.0 - 2.0 * pad, 1e-4);
                barUV.x = lerp(barUV.x, 1.0 - barUV.x, saturate(_Invert));

                float insideBar = step(0.0, barUV.x) * step(barUV.x, 1.0)
                                * step(0.0, barUV.y) * step(barUV.y, 1.0);

                // --- snap to the cell grid --------------------------------
                // Everything below uses the CELL CENTRE, never the raw UV.
                // That is what makes the front blocky and keeps every test
                // binary across a whole cell.
                float2 cells = max(_NoiseScale.xy, float2(2.0, 1.0));
                float2 cell  = floor(barUV * cells);
                float2 cuv   = (cell + 0.5) / cells;

                // --- per-cell clock ---------------------------------------
                float seed = hash21(cell * 1.13 + 5.7);
                float rate = lerp(1.0, lerp(0.15, 1.0, seed), _Hold);
                float ct   = floor(t * max(_StepRate, 0.25) * rate + seed * 17.0);

                float rCover = hash21(cell + ct * 91.0);
                float rTone  = hash21(cell * 3.7 + ct * 37.0);
                float rAmb   = hash21(cell * 5.3 + ct * 67.0);

                // --- the wave front ---------------------------------------
                // Sampled at the cell centre, so a whole column of the cell
                // is on the same side of the front. Grows as death nears.
                float wave = sin(cuv.y * _WaveFreq * 6.28318 + t * _WaveSpeed) * 0.5
                           + (vnoise(float2(cuv.y * _WaveFreq * 1.7, t * _WaveSpeed * 0.4)) - 0.5);
                float front = _Fill + wave * _EdgeWave * (0.5 + prox);

                float behind = step(cuv.x, front);

                // --- is this cell corrupted? ------------------------------
                float coverage = saturate(_Coverage + prox * (1.0 - _Coverage));
                float on = behind * step(1.0 - coverage, rCover);

                // scatter ahead of the front: entropy leaking forward
                float ambient = step(1.0 - _Ambient * (0.15 + prox * 0.85), rAmb);
                on = max(on, ambient * (1.0 - behind));

                // --- spill: same cells, outside the bar -------------------
                if (_Spill > 0.5)
                {
                    float2 outDist  = max(max(-barUV, barUV - 1.0), 0.0);
                    float2 bandSize = pad / max(1.0 - 2.0 * pad, 1e-4);
                    float2 outNorm  = outDist / max(bandSize, 1e-4);
                    float  falloff  = 1.0 - saturate(max(outNorm.x, outNorm.y)
                                                     / max(_SpillReach, 1e-4));

                    float chance = _SpillAmount * (0.35 + prox * 0.9) * falloff;
                    on = step(1.0 - chance, rCover) * (1.0 - insideBar);
                }
                else
                {
                    on *= insideBar;
                }

                // --- ink or snow, nothing between -------------------------
                float whiteMix = lerp(_WhiteMix, min(_WhiteMix + 0.3, 1.0), prox);
                float isSnow   = step(1.0 - whiteMix, rTone);
                float3 col     = lerp(_Ink.rgb, _Snow.rgb, isSnow);

                float alpha = on * step(0.0005, _Fill + _Ambient);

                #ifdef UNITY_UI_CLIP_RECT
                alpha *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                clip(alpha - 0.001);
                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
    Fallback Off
}
