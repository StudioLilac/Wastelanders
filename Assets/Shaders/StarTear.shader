Shader "Wastelanders/StarTear"
{
    Properties
    {
        [HDR] _StreakColor  ("Streak Colour", Color) = (0.80, 0.86, 1.0, 1.0)
        _Intensity          ("Intensity", Range(0, 4)) = 0.35

        [Header(Tear Flare)]
        _TearMajorReach     ("Major Reach", Range(0, 1)) = 0.85
        _TearMinorReach     ("Minor Reach", Range(0, 1)) = 0.42
        _TearThickness      ("Prong Thickness", Range(0.01, 0.4)) = 0.06
        _TearFalloff        ("Prong Falloff", Range(0.5, 6)) = 2.2
        _TearMinorRatio     ("Minor Brightness", Range(0, 1)) = 0.35
        _TearMajorBias      ("Major Lopsidedness", Range(-0.8, 0.8)) = 0.35
        _TearMinorBias      ("Minor Lopsidedness", Range(-0.8, 0.8)) = -0.15
        _TearMinorAngle     ("Minor Angle (deg)", Range(0, 180)) = 45
        _CoreStrength       ("Core Bloom", Range(0, 2)) = 0.5
        _TearLocal          ("Local Tear Strength (editor preview)", Range(0, 1)) = 0

        [Header(Pixel Art)]
        _GlowPixels         ("Glow Pixels Across Quad", Range(8, 256)) = 48
        _Steps              ("Brightness Steps (0 = smooth)", Range(0, 32)) = 6

        [Header(Per Star Variation)]
        _Variation          ("Reach Variation", Range(0, 0.6)) = 0.25

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
                float  seed  : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _StreakColor;
            float  _Intensity;
            float  _TearMajorReach, _TearMinorReach, _TearThickness, _TearFalloff;
            float  _TearMinorRatio, _TearMajorBias, _TearMinorBias, _TearMinorAngle;
            float  _CoreStrength, _TearLocal;
            float  _GlowPixels, _Steps, _Variation;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = v.uv;
                o.color = v.color;

                // Deterministic per-instance seed from world position, so each star
                // gets a slightly different reach without any authoring. Note this
                // varies length only. The axis stays global, because that is what
                // separates tears from glitter.
                float3 w = unity_ObjectToWorld._m03_m13_m23;
                o.seed = frac(sin(dot(w.xy, float2(12.9898, 78.233))) * 43758.5453);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Zero global in the material preview; _TearLocal makes the
                // streak previewable without running the scene.
                float tear = max(_TearStrength, _TearLocal);
                if (tear <= 0.001) return fixed4(0, 0, 0, 0);

                float2 uv = i.uv;
                if (_GlowPixels > 0)
                    uv = (floor(uv * _GlowPixels) + 0.5) / _GlowPixels;

                float2 p = (uv - 0.5) * 2.0;

                float vary = 1.0 + (i.seed - 0.5) * 2.0 * _Variation;

                float flare = TearFlare(
                    p,
                    _TearMajorReach * vary * tear,
                    _TearMinorReach * vary * tear,
                    _TearThickness,
                    _TearFalloff,
                    _TearMinorRatio,
                    _TearMajorBias,
                    _TearMinorBias,
                    radians(_TearMinorAngle));

                // Small round bloom so the star still has a body.
                float core = pow(saturate(1.0 - length(p) / max(_TearThickness * 2.5, 1e-4)), 2.0)
                             * _CoreStrength;

                float g = (flare + core) * tear;

                if (_Steps > 0.5)
                    g = floor(g * _Steps) / _Steps;

                g = max(g, 0.0);
                g *= 1.0 - smoothstep(0.94, 1.0, length(p));

                fixed3 rgb = _StreakColor.rgb * _StreakColor.a * g * _Intensity * i.color.rgb * i.color.a;
                return fixed4(rgb, 0);
            }
            ENDCG
        }
    }

    Fallback Off
}
