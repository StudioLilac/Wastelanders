// =====================================================================
//  Impact/SpriteDissolvePixel  -  dematerialise a pixel-art sprite
//
//  Built-in renderer. Drop-in replacement for Sprites/Default on any
//  SpriteRenderer, so atlas UVs, sorting and tint all behave normally.
//
//  Shares the waste chunks' vocabulary on purpose: the threshold is
//  SNAPPED TO THE SOURCE TEXEL GRID, so the sprite sheds whole art
//  pixels. A smooth noise mask over pixel art reads as a rendering bug --
//  fine dithered fringes in colours the palette never had.
//
//  _Direction sweeps the dissolve across the sprite so it can travel
//  along the blow rather than eating the whole body at once.
//  _Extents must be the sprite's local half-size; SpriteDissolver pushes
//  it automatically.
// =====================================================================
Shader "Impact/SpriteDissolvePixel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Dissolve)]
        _Progress ("Progress", Range(0,1)) = 0
        _Cluster  ("Cluster (source px)", Range(1,8)) = 1

        [Header(Sweep)]
        _Direction       ("Direction (xy)", Vector) = (1,0,0,0)
        _DirectionalBias ("Directional Bias", Range(0,1)) = 0.45
        _Extents         ("Sprite Extents (xy)", Vector) = (0.5,0.5,0,0)

        [Header(Edge)]
        _EdgeWidth ("Edge Width", Range(0.001,0.5)) = 0.14
        _EdgeColor ("Edge Colour", Color) = (0.72,0.35,1,1)
        _EdgeBoost ("Edge Boost", Range(0,4)) = 1.6
    }

    SubShader
    {
        Tags
        {
            "Queue"            = "Transparent"
            "IgnoreProjector"  = "True"
            "RenderType"       = "Transparent"
            "PreviewType"      = "Plane"
            "CanUseSpriteAtlas"= "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha   // premultiplied, matches Sprites/Default

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
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float2 objPos : TEXCOORD1;
                fixed4 color  : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;   // zw = texture size in pixels
            fixed4 _Color;

            float  _Progress, _Cluster;
            float4 _Direction, _Extents;
            float  _DirectionalBias;
            float  _EdgeWidth, _EdgeBoost;
            fixed4 _EdgeColor;

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

                o.pos    = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                o.objPos = v.vertex.xy;   // local space -- atlas UVs can't give a sweep
                o.color  = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv) * i.color;

                // Snap to whole source texels so the sprite loses entire art
                // pixels rather than eroding into a fringe.
                float2 texel = floor(i.uv * _MainTex_TexelSize.zw / max(_Cluster, 1.0));
                float n = hash21(texel);

                // Sweep: low threshold dies first, so the side the direction
                // points away from goes first.
                float2 p = i.objPos / max(abs(_Extents.xy), 1e-4);
                float2 dir = normalize(_Direction.xy + 1e-6);
                float sweep = saturate(dot(dir, p) * 0.5 + 0.5);

                float threshold = lerp(n, sweep, saturate(_DirectionalBias));
                threshold = threshold * 0.93 + 0.07;   // nothing dies at progress 0

                float alive = threshold - _Progress;
                clip(alive);

                // Constant within a texel, so whole pixels light before they go.
                float edge = 1.0 - saturate(alive / max(_EdgeWidth, 1e-4));
                tex.rgb = lerp(tex.rgb, _EdgeColor.rgb * _EdgeBoost, edge * tex.a);

                tex.rgb *= tex.a;   // premultiply for the blend mode above
                return tex;
            }
            ENDCG
        }
    }

    Fallback Off
}
