Shader "Wastelanders/MoonGlow"
{
    Properties
    {
        [HDR] _GlowColor      ("Glow Colour", Color) = (0.72, 0.78, 1.0, 1.0)
        _Intensity            ("Intensity", Range(0, 4)) = 1.0

        [Header(Shape)]
        _CoreRadius           ("Core Radius", Range(0.05, 1)) = 0.35
        _CorePower            ("Core Falloff", Range(0.5, 8)) = 2.5
        _CoreStrength         ("Core Strength", Range(0, 2)) = 0.6
        _HaloPower            ("Halo Falloff", Range(0.5, 8)) = 3.0
        _HaloStrength         ("Halo Strength", Range(0, 2)) = 0.5

        [Header(Pixel Art)]
        _GlowPixels           ("Glow Pixels Across Quad", Range(8, 512)) = 64
        _Steps                ("Brightness Steps (0 = smooth)", Range(0, 32)) = 8

        [Header(Pulse)]
        _PulseAmount          ("Pulse Amount", Range(0, 0.5)) = 0.12
        _Period1              ("Pulse Period 1 (s)", Float) = 4.3
        _Period2              ("Pulse Period 2 (s)", Float) = 7.1
        _Period3              ("Pulse Period 3 (s)", Float) = 11.7

        [Header(Ripple)]
        _RippleAmount         ("Ripple Amount", Range(0, 1)) = 0.0
        _RippleFreq           ("Ripple Frequency", Range(2, 48)) = 18
        _RippleSpeed          ("Ripple Speed", Range(0, 4)) = 0.6

        [Header(Tear Flare)]
        _TearMajorReach       ("Major Reach", Range(0, 1)) = 0.90
        _TearMinorReach       ("Minor Reach", Range(0, 1)) = 0.50
        _TearThickness        ("Prong Thickness", Range(0.01, 0.5)) = 0.16
        _TearFalloff          ("Prong Falloff", Range(0.5, 6)) = 1.2
        _TearMinorRatio       ("Minor Brightness", Range(0, 1)) = 0.35
        _TearMajorBias        ("Major Lopsidedness", Range(-0.8, 0.8)) = 0.35
        _TearMinorBias        ("Minor Lopsidedness", Range(-0.8, 0.8)) = -0.15
        _TearMinorAngle       ("Minor Angle (deg)", Range(0, 180)) = 45
        _TearFlareStrength    ("Flare Brightness", Range(0, 10)) = 2.5
        _TearHaze             ("Veiling Haze", Range(0, 2)) = 0.35
        _TearLocal            ("Local Tear Strength (editor preview)", Range(0, 1)) = 0
        _DebugMode            ("DEBUG (0 off, 1 flare, 2 raw, 3 uv, 4 strength)", Range(0, 4)) = 0

        [Header(Time)]
        [Toggle] _UseCustomTime ("Driven By Script", Float) = 0
        _GlowTime             ("Glow Time (set by script)", Float) = 0

        // Present so SpriteRenderer's material binding does not warn. Unused.
        [HideInInspector] _MainTex ("Sprite", 2D) = "white" {}
        [HideInInspector] _Color   ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "RenderType"        = "Transparent"
            "IgnoreProjector"   = "True"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        // Additive. Alpha is ignored, so brightness lives entirely in rgb.
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "TearSpike.cginc"

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

            fixed4 _GlowColor;
            float  _Intensity;

            float _CoreRadius, _CorePower, _CoreStrength;
            float _HaloPower,  _HaloStrength;

            float _GlowPixels, _Steps;

            float _PulseAmount, _Period1, _Period2, _Period3;
            float _RippleAmount, _RippleFreq, _RippleSpeed;

            float _TearMajorReach, _TearMinorReach, _TearThickness, _TearFalloff;
            float _TearMinorRatio, _TearMajorBias, _TearMinorBias, _TearMinorAngle;
            float _TearFlareStrength, _TearHaze, _TearLocal, _DebugMode;

            float _UseCustomTime, _GlowTime;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Quantise to the quad's own pixel grid so the falloff is built
                // from square pixels that scale with the sprite. Because this is
                // in UV space rather than screen space, the glow stays locked to
                // the moon in world space and shrinks correctly as the camera
                // zooms out.
                float2 uv = i.uv;
                if (_GlowPixels > 0)
                    uv = (floor(uv * _GlowPixels) + 0.5) / _GlowPixels;

                float2 p = (uv - 0.5) * 2.0;
                float  d = length(p);

                // Two lobes: a tight bloom hugging the disc, and a wide ambient
                // halo that fills the gap out to the painted rings.
                float core = pow(saturate(1.0 - d / max(_CoreRadius, 1e-4)), _CorePower);
                float halo = pow(saturate(1.0 - d), _HaloPower);
                float g    = core * _CoreStrength + halo * _HaloStrength;

                // The disc stays round. The flare radiates past it, which is what
                // a bright source actually does through a broken tear film.
                //
                // Kept in its own accumulator all the way through posterisation.
                // Folded into g first, a prong only ever nudges the halo's radial
                // bands outward a step, which reads as the aura growing rather than
                // as a prong appearing.
                // Computed unconditionally. The old early-out saved nothing worth
                // measuring and made the flare impossible to inspect when the
                // global was zero, which is exactly when you need to look at it.
                float flareRaw = TearFlare(
                    p,
                    _TearMajorReach,
                    _TearMinorReach,
                    _TearThickness,
                    _TearFalloff,
                    _TearMinorRatio,
                    _TearMajorBias,
                    _TearMinorBias,
                    radians(_TearMinorAngle));

                // The global is only written by a running TearFilm, so it is
                // always zero in the material preview. _TearLocal lets you dial
                // the effect up there to tune prong shape without playing the
                // scene. Leave it at 0 in the scene material.
                float tear = max(_TearStrength, _TearLocal);

                float flare = flareRaw * _TearFlareStrength * tear;

                // Veiling glare lifts the floor across the halo rather than
                // brightening the centre. This one does belong to the halo.
                g += pow(saturate(1.0 - d), 1.2) * _TearHaze * tear * 0.35;

                // Diagnostics, bypassing pulse, posterisation and the corner mask
                // so nothing downstream can swallow the signal.
                if (_DebugMode > 0.5)
                {
                    float3 dbg;
                    if (_DebugMode < 1.5)
                        dbg = _GlowColor.rgb * flare;                       // after multipliers
                    else if (_DebugMode < 2.5)
                        dbg = flareRaw.xxx;                                 // shape only
                    else if (_DebugMode < 3.5)
                        dbg = float3(saturate(p.x * 0.5 + 0.5),
                                     saturate(p.y * 0.5 + 0.5), 0);         // quad coordinates
                    else
                        dbg = tear.xxx;                            // the global
                    return fixed4(dbg, 0);
                }

                float t = lerp(_Time.y, _GlowTime, _UseCustomTime);

                // Three incommensurate periods. A single sine over a 130 second
                // hold reads as a machine; these never line up, so it breathes.
                const float TAU = 6.28318530718;
                float pulse = 1.0 + _PulseAmount * (
                      0.55 * sin(t * TAU / max(_Period1, 1e-3))
                    + 0.30 * sin(t * TAU / max(_Period2, 1e-3) + 1.7)
                    + 0.15 * sin(t * TAU / max(_Period3, 1e-3) + 4.1));

                g *= pulse;
                flare *= pulse;

                // Optional outward-travelling ring, modulated by the glow itself
                // so it only ever appears where there is light to disturb.
                if (_RippleAmount > 0.0)
                    g += g * sin(d * _RippleFreq - t * _RippleSpeed * TAU) * _RippleAmount;

                g     = max(g, 0.0);
                flare = max(flare, 0.0);

                // Posterise last, so the band edges migrate outward and inward with
                // the pulse. On pixel art that reads as deliberate shimmer rather
                // than as a gradient being dimmed. Each term bands on its own so the
                // prong keeps a shape instead of dissolving into the halo's rings.
                if (_Steps > 0.5)
                {
                    g     = floor(g * _Steps) / _Steps;
                    flare = floor(flare * _Steps) / _Steps;
                }

                g += flare;

                // Soft kill before the quad's corners. Prong reach is in the same
                // units, so keep it under 1.0 or the tips get clipped here.
                g *= 1.0 - smoothstep(0.94, 1.0, d);

                fixed3 rgb = _GlowColor.rgb * _GlowColor.a * g * _Intensity * i.color.rgb * i.color.a;
                return fixed4(rgb, 0);
            }
            ENDCG
        }
    }

    Fallback Off
}
