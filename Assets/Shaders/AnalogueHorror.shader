// =====================================================================
//  Hidden/FX/Analogue Horror  -  VHS / broken-signal camera effect
//  BUILT-IN RENDER PIPELINE  (OnRenderImage post process)
//
//  Drive it with AnalogueHorrorEffect.cs on your camera.
//  Everything scales off _Intensity (0 = clean image, 1 = signal loss),
//  so you can ramp the whole stack from one value in your VN script.
//
//  Stack, in the order it is applied:
//    tracking bar -> scanline jitter -> block corruption -> wave warp
//    -> RGB separation -> desaturation + colour fringes -> tape bleed
//    -> static -> speckle -> scanlines -> dropout -> flicker -> vignette
// =====================================================================
Shader "Hidden/FX/Analogue Horror"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}

        [Header(Master)]
        _Intensity     ("Intensity",            Range(0, 1)) = 1
        _StepRate      ("Time Step (fps feel)", Float) = 18

        [Header(Colour Separation)]
        _Separation    ("RGB Separation",       Range(0, 0.1)) = 0.012
        _FringeBoost   ("Fringe Strength",      Range(0, 4)) = 1.8
        _Desaturate    ("Desaturate To Gray",   Range(0, 1)) = 0.85
        _Bleed         ("Tape Bleed (smear)",   Range(0, 0.05)) = 0.008

        [Header(Distortion)]
        _LineDensity   ("Scanline Bands",       Float) = 180
        _LineJitter    ("Line Jitter",          Range(0, 0.2)) = 0.035
        _JitterChance  ("Line Jitter Chance",   Range(0, 1)) = 0.35
        _Blocks        ("Block Grid (XY)",      Vector) = (14, 40, 0, 0)
        _BlockAmount   ("Block Corruption",     Range(0, 1)) = 0.25
        _BlockShift    ("Block Shift",          Range(0, 0.3)) = 0.06
        _BlockRate     ("Block Rate",           Float) = 8
        _WaveAmp       ("Wave Warp Amount",     Range(0, 0.1)) = 0.006
        _WaveFreq      ("Wave Warp Frequency",  Float) = 9
        _WaveSpeed     ("Wave Warp Speed",      Float) = 1.6

        [Header(Tracking Bar)]
        _BarHeight     ("Bar Height",           Range(0, 0.5)) = 0.07
        _BarSpeed      ("Bar Roll Speed",       Float) = 0.18
        _BarDisplace   ("Bar Displacement",     Range(0, 0.3)) = 0.05
        _BarBrightness ("Bar Brightness",       Range(-1, 1)) = 0.12

        [Header(Noise)]
        _Static        ("Static Grain",         Range(0, 1)) = 0.22
        _Speckle       ("Colour Speckle",       Range(0, 0.3)) = 0.03
        _Snow          ("Signal Loss (snow)",   Range(0, 1)) = 0
        _Scanline      ("Scanline Darkening",   Range(0, 1)) = 0.25

        [Header(Signal)]
        _DropoutChance ("Dropout Chance",       Range(0, 1)) = 0.12
        _Flicker       ("Exposure Flicker",     Range(0, 1)) = 0.18
        _InvertChance  ("Invert Flash Chance",  Range(0, 1)) = 0.02
        _Vignette      ("Vignette",             Range(0, 3)) = 0.9
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "AnalogueHorror"
            CGPROGRAM
            #pragma vertex   vert_img
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float  _Intensity, _StepRate;
            float  _Separation, _FringeBoost, _Desaturate, _Bleed;
            float  _LineDensity, _LineJitter, _JitterChance;
            float4 _Blocks;
            float  _BlockAmount, _BlockShift, _BlockRate;
            float  _WaveAmp, _WaveFreq, _WaveSpeed;
            float  _BarHeight, _BarSpeed, _BarDisplace, _BarBrightness;
            float  _Static, _Speckle, _Snow, _Scanline;
            float  _DropoutChance, _Flicker, _InvertChance, _Vignette;

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

            float luma(float3 c) { return dot(c, float3(0.2126, 0.7152, 0.0722)); }

            // ---------------------------------------------------------
            fixed4 frag(v2f_img i) : SV_Target
            {
                float I = saturate(_Intensity);
                float t = _Time.y;

                // Stepped time: analogue artefacts read better when they
                // update on "frames" rather than continuously.
                float ts = floor(t * max(_StepRate, 1.0)) / max(_StepRate, 1.0);

                float2 uv = i.uv;

                // --- rolling tracking bar -----------------------------
                float barPos = frac(t * _BarSpeed);
                float barD   = abs(frac(uv.y - barPos + 0.5) - 0.5);
                float bar    = 1.0 - smoothstep(0.0, max(_BarHeight, 1e-4), barD);
                uv.x += bar * _BarDisplace * I * (hash11(floor(t * 6.0)) * 2.0 - 1.0);

                // --- per-scanline horizontal jitter -------------------
                float band   = floor(uv.y * _LineDensity);
                float pick   = hash11(band * 1.7 + ts * 11.0);
                float active = step(1.0 - _JitterChance * I, pick);
                float jit    = (hash11(band + ts * 37.0) * 2.0 - 1.0) * _LineJitter * I;
                uv.x += jit * active;

                // --- block corruption ---------------------------------
                float2 blockId = floor(uv * _Blocks.xy);
                float  bRand   = hash21(blockId + floor(ts * _BlockRate) * 13.7);
                float  bOn     = step(1.0 - _BlockAmount * I, bRand);
                float2 bShift  = (hash23(blockId + 7.7).xy * 2.0 - 1.0) * _BlockShift * I;
                uv += bShift * bOn * float2(1.0, 0.25);

                // --- slow wave warp (the picture "breathing") ---------
                uv.x += sin(uv.y * _WaveFreq + t * _WaveSpeed) * _WaveAmp * I;
                uv.y += sin(uv.x * _WaveFreq * 0.6 - t * _WaveSpeed * 0.7) * _WaveAmp * 0.35 * I;

                uv = clamp(uv, 0.001, 0.999);

                // --- RGB separation -----------------------------------
                // widened inside the tracking bar and on corrupt blocks
                float sepAmt = _Separation * I * (1.0 + bar * 2.0 + bOn * 2.5);
                float2 off   = float2(sepAmt, sepAmt * 0.12);

                float3 c0 = tex2D(_MainTex, uv).rgb;
                float3 cr = tex2D(_MainTex, clamp(uv + off, 0.001, 0.999)).rgb;
                float3 cb = tex2D(_MainTex, clamp(uv - off, 0.001, 0.999)).rgb;

                float3 col = float3(cr.r, c0.g, cb.b);

                // --- crush to gray, then re-add colour fringes ---------
                float g = luma(c0);
                col = lerp(col, g.xxx, _Desaturate * I);
                float3 fringe = float3(cr.r - c0.r, 0.0, cb.b - c0.b);
                col += fringe * _FringeBoost * I;

                // --- horizontal tape bleed (cheap 3-tap smear) --------
                if (_Bleed > 0.0001)
                {
                    float3 s1 = tex2D(_MainTex, clamp(uv - float2(_Bleed * 0.5, 0), 0.001, 0.999)).rgb;
                    float3 s2 = tex2D(_MainTex, clamp(uv - float2(_Bleed, 0), 0.001, 0.999)).rgb;
                    float3 smear = (s1 * 0.6 + s2 * 0.4);
                    col = lerp(col, max(col, lerp(col, smear, 0.6)), I);
                }

                // --- tracking bar brightening -------------------------
                col += bar * _BarBrightness * I;

                // --- static grain -------------------------------------
                float n = hash21(i.uv * _ScreenParams.xy * 0.7 + ts * 137.0);
                col += (n - 0.5) * 2.0 * _Static * I;

                // --- colour speckle (dead pixels / dropout dots) -------
                float sp = step(1.0 - _Speckle * I, hash21(i.uv * _ScreenParams.xy + ts * 71.0));
                col = lerp(col, hash23(i.uv * _ScreenParams.xy + ts * 3.1), sp);

                // --- full signal loss (snow) --------------------------
                float snowN = hash21(i.uv * _ScreenParams.xy * 1.3 + ts * 411.0);
                col = lerp(col, snowN.xxx, saturate(_Snow * I));

                // --- scanlines ----------------------------------------
                float sl = 0.5 + 0.5 * sin(i.uv.y * _ScreenParams.y * 3.14159);
                col *= 1.0 - _Scanline * I * sl;

                // --- horizontal dropout bands + frame dropout ---------
                float dropBand = step(1.0 - _DropoutChance * I,
                                      hash11(floor(i.uv.y * 40.0) + floor(t * 9.0) * 31.0));
                col *= 1.0 - dropBand * 0.75;

                float frameDrop = step(1.0 - _DropoutChance * 0.35 * I, hash11(floor(t * 7.0)));
                col *= 1.0 - frameDrop * 0.6;

                // --- rare invert flash --------------------------------
                float inv = step(1.0 - _InvertChance * I, hash11(floor(t * 12.0) + 5.5));
                col = lerp(col, 1.0 - col, inv);

                // --- exposure flicker ---------------------------------
                col *= 1.0 + (hash11(floor(t * 24.0)) - 0.5) * _Flicker * I;

                // --- vignette -----------------------------------------
                float2 cuv = i.uv - 0.5;
                col *= 1.0 - saturate(dot(cuv, cuv) * _Vignette * I);

                return fixed4(saturate(col), 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
