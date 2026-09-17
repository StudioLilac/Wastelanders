Shader "Wastelanders/MoonGlow"
{
    Properties
    {
        [Header(Aura)]
        [HDR] _GlowColor      ("Aura Colour", Color) = (0.72, 0.78, 1.0, 1.0)
        _Intensity            ("Aura Intensity (driven)", Range(0, 4)) = 1.0
        _CoreRadius           ("Core Radius", Range(0.05, 1)) = 0.35
        _CorePower            ("Core Falloff", Range(0.5, 8)) = 2.5
        _CoreStrength         ("Core Strength", Range(0, 2)) = 0.6
        _HaloPower            ("Halo Falloff", Range(0.5, 8)) = 3.0
        _HaloStrength         ("Halo Strength", Range(0, 2)) = 0.5
        _GlowPixels           ("Pixels Across Quad (0 = smooth)", Range(0, 512)) = 64
        _Steps                ("Aura Brightness Steps (0 = smooth)", Range(0, 32)) = 8

        [Header(Aura Pulse)]
        _PulseAmount          ("Pulse Amount", Range(0, 0.5)) = 0.12
        _Period1              ("Pulse Period 1 (s)", Float) = 4.3
        _Period2              ("Pulse Period 2 (s)", Float) = 7.1
        _Period3              ("Pulse Period 3 (s)", Float) = 11.7

        [Header(Aura Ripple)]
        _RippleAmount         ("Ripple Amount", Range(0, 1)) = 0.0
        _RippleFreq           ("Ripple Frequency", Range(2, 48)) = 18
        _RippleSpeed          ("Ripple Speed", Range(0, 4)) = 0.6

        [Header(Tear Flare)]
        [HDR] _FlareColor     ("Flare Colour", Color) = (0.85, 0.90, 1.0, 1.0)
        _TearFlareStrength    ("Flare Brightness", Range(0, 20)) = 3.0
        _TearMajorReach       ("Major Reach", Range(0, 1)) = 0.90
        _TearMinorReach       ("Minor Reach", Range(0, 1)) = 0.50
        _TearThickness        ("Prong Thickness", Range(0.005, 0.5)) = 0.06
        _TearFalloff          ("Prong Length Falloff", Range(0.3, 6)) = 1.2
        _FlareEdge            ("Prong Edge Hardness", Range(0.2, 4)) = 0.7
        _TearMinorRatio       ("Minor Brightness", Range(0, 1)) = 0.35
        _TearMajorBias        ("Major Lopsidedness", Range(-0.8, 0.8)) = 0.35
        _TearMinorBias        ("Minor Lopsidedness", Range(-0.8, 0.8)) = -0.15
        _TearMinorAngle       ("Minor Angle (deg from major)", Range(0, 180)) = 45
        _FlareSteps           ("Flare Brightness Steps (0 = smooth)", Range(0, 32)) = 0
        _FlarePulseAmount     ("Inherit Aura Pulse", Range(0, 1)) = 0
        _TearHaze             ("Veiling Haze (aura)", Range(0, 2)) = 0.35

        [Header(Tear State)]
        _TearStrength         ("Tear Strength (driven)", Range(0, 1)) = 0
        _TearAxis             ("Tear Axis (radians, driven)", Float) = 0

        [Header(Time)]
        [Toggle] _UseCustomTime ("Driven By Script", Float) = 0
        _GlowTime             ("Glow Time (driven)", Float) = 0

        _DebugMode            ("DEBUG (0 off, 1 flare, 2 raw, 3 uv, 4 strength)", Range(0, 4)) = 0

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
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
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

            fixed4 _GlowColor, _FlareColor;
            float  _Intensity;
            float  _CoreRadius, _CorePower, _CoreStrength;
            float  _HaloPower,  _HaloStrength;
            float  _GlowPixels, _Steps, _FlareSteps;
            float  _PulseAmount, _Period1, _Period2, _Period3;
            float  _RippleAmount, _RippleFreq, _RippleSpeed;
            float  _TearMajorReach, _TearMinorReach, _TearThickness, _TearFalloff;
            float  _TearMinorRatio, _TearMajorBias, _TearMinorBias, _TearMinorAngle;
            float  _TearFlareStrength, _TearHaze, _FlareEdge, _FlarePulseAmount;
            float  _TearStrength, _TearAxis;
            float  _UseCustomTime, _GlowTime;
            float  _DebugMode;

            float2 TearRotate(float2 p, float angle)
            {
                float s, c;
                sincos(angle, s, c);
                return float2(p.x * c + p.y * s, -p.x * s + p.y * c);
            }

            // One flare line running along the local x axis. At angle 0 that is
            // horizontal, which is the orientation to start from.
            //
            // bias makes it lopsided: at 0 the prong is even on both sides, at 0.5
            // the positive side reaches half again as far. Symmetric prongs look
            // synthetic, so a nonzero bias is the default rather than the exception.
            float TearProng(float2 q, float reach, float thickness, float falloff, float bias)
            {
                float side = q.x >= 0.0 ? 1.0 : -1.0;
                float r = max(reach * (1.0 + bias * side), 1e-4);

                float along  = pow(saturate(1.0 - abs(q.x) / r), falloff);
                float across = pow(saturate(1.0 - abs(q.y) / max(thickness, 1e-4)), _FlareEdge);

                return along * across;
            }

            float TearFlare(float2 p)
            {
                float2 qa = TearRotate(p, _TearAxis);
                float major = TearProng(qa, _TearMajorReach, _TearThickness, _TearFalloff, _TearMajorBias);

                float2 qb = TearRotate(p, _TearAxis + radians(_TearMinorAngle));
                float minor = TearProng(qb, _TearMinorReach, _TearThickness, _TearFalloff, _TearMinorBias)
                              * _TearMinorRatio;

                return major + minor;
            }

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
                // Quantise to the quad's own pixel grid. UV space rather than screen
                // space, so the glow stays locked to the moon in world space and
                // shrinks correctly as the camera zooms out.
                float2 uv = i.uv;
                if (_GlowPixels > 0)
                    uv = (floor(uv * _GlowPixels) + 0.5) / _GlowPixels;

                float2 p = (uv - 0.5) * 2.0;
                float  d = length(p);

                // ---- Aura --------------------------------------------------------
                float core = pow(saturate(1.0 - d / max(_CoreRadius, 1e-4)), _CorePower);
                float halo = pow(saturate(1.0 - d), _HaloPower);
                float aura = core * _CoreStrength + halo * _HaloStrength;

                // Veiling glare lifts the floor across the halo rather than
                // brightening the centre. This belongs to the aura, not the prongs.
                aura += pow(saturate(1.0 - d), 1.2) * _TearHaze * _TearStrength * 0.35;

                // ---- Flare -------------------------------------------------------
                float flareRaw = TearFlare(p);
                float flare = flareRaw * _TearFlareStrength * _TearStrength;

                if (_DebugMode > 0.5)
                {
                    float3 dbg;
                    if (_DebugMode < 1.5)      dbg = _FlareColor.rgb * flare;
                    else if (_DebugMode < 2.5) dbg = flareRaw.xxx;
                    else if (_DebugMode < 3.5) dbg = float3(saturate(p.x * 0.5 + 0.5),
                                                            saturate(p.y * 0.5 + 0.5), 0);
                    else                       dbg = _TearStrength.xxx;
                    return fixed4(dbg, 0);
                }

                // ---- Motion ------------------------------------------------------
                float t = lerp(_Time.y, _GlowTime, _UseCustomTime);

                // Three incommensurate periods. A single sine over a 130 second hold
                // reads as a machine; these never line up, so it breathes.
                const float TAU = 6.28318530718;
                float pulse = 1.0 + _PulseAmount * (
                      0.55 * sin(t * TAU / max(_Period1, 1e-3))
                    + 0.30 * sin(t * TAU / max(_Period2, 1e-3) + 1.7)
                    + 0.15 * sin(t * TAU / max(_Period3, 1e-3) + 4.1));

                aura *= pulse;

                // The moon's luminosity is a property of the moon. The flare is a
                // property of the eye looking at it, so by default it does not
                // inherit the breathing. Raise _FlarePulseAmount to couple them.
                flare *= lerp(1.0, pulse, _FlarePulseAmount);

                if (_RippleAmount > 0.0)
                    aura += aura * sin(d * _RippleFreq - t * _RippleSpeed * TAU) * _RippleAmount;

                aura  = max(aura, 0.0);
                flare = max(flare, 0.0);

                // Each term bands on its own scale. The aura wants chunky steps to
                // match the pixel art; the flare usually wants none, because banding
                // is what makes a prong look soft instead of sharp.
                if (_Steps > 0.5)      aura  = floor(aura  * _Steps)      / _Steps;
                if (_FlareSteps > 0.5) flare = floor(flare * _FlareSteps) / _FlareSteps;

                float mask = 1.0 - smoothstep(0.94, 1.0, d);

                // Separate colour and brightness paths. _Intensity is driven by
                // MoonGlowDriver's envelope and belongs to the aura alone, so dimming
                // or fading in the moon's glow never touches the flare.
                float3 auraRgb  = _GlowColor.rgb  * _GlowColor.a  * aura * _Intensity;
                float3 flareRgb = _FlareColor.rgb * _FlareColor.a * flare;

                fixed3 rgb = (auraRgb + flareRgb) * mask * i.color.rgb * i.color.a;
                return fixed4(rgb, 0);
            }
            ENDCG
        }
    }

    Fallback Off
}
