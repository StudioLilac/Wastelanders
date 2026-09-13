// Fullscreen composite. Reads the single-channel mask produced by Impact/Mask
// and paints the screen: masked pixels get _FgColor, everything else _BgColor.
//
// Rendered on a camera-parented quad at queue Overlay+100 with ZTest Always, so
// it lands on top of all world-space and screen-space-camera content.
// It CANNOT cover Screen Space - Overlay canvases; hide those instead.
Shader "Impact/ImpactFrame"
{
    Properties
    {
        _MaskTex   ("Mask", 2D)                = "black" {}
        _FgColor   ("Silhouette Color", Color) = (0,0,0,1)
        _BgColor   ("Background Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,1))   = 0
        _Dilate    ("Dilate (px)", Range(0,10))= 1.5
        _Threshold ("Threshold", Range(0.001,1)) = 0.2
        _Invert    ("Invert", Range(0,1))      = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"       = "Overlay+100"
            "RenderType"  = "Overlay"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float4 screen : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MaskTex;
            float4 _MaskTex_TexelSize;

            fixed4 _FgColor;
            fixed4 _BgColor;
            float  _Intensity;
            float  _Dilate;
            float  _Threshold;
            float  _Invert;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos    = UnityObjectToClipPos(v.vertex);
                // derive screen UVs from clip pos so the quad's own UVs don't matter
                o.screen = ComputeScreenPos(o.pos);
                return o;
            }

            // 9-tap max filter. Fattens thin sprite details (crown prongs, particle
            // wisps) so they stay readable as silhouette at a glance.
            float SampleMask (float2 uv)
            {
                float m = tex2D(_MaskTex, uv).r;

                if (_Dilate <= 0.001)
                    return m;

                float2 o = _MaskTex_TexelSize.xy * _Dilate;

                m = max(m, tex2D(_MaskTex, uv + float2( o.x,  0.0 )).r);
                m = max(m, tex2D(_MaskTex, uv + float2(-o.x,  0.0 )).r);
                m = max(m, tex2D(_MaskTex, uv + float2( 0.0,  o.y )).r);
                m = max(m, tex2D(_MaskTex, uv + float2( 0.0, -o.y )).r);
                m = max(m, tex2D(_MaskTex, uv + float2( o.x,  o.y )).r);
                m = max(m, tex2D(_MaskTex, uv + float2(-o.x,  o.y )).r);
                m = max(m, tex2D(_MaskTex, uv + float2( o.x, -o.y )).r);
                m = max(m, tex2D(_MaskTex, uv + float2(-o.x, -o.y )).r);

                return m;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.screen.xy / i.screen.w;

                float m = SampleMask(uv);
                float s = step(_Threshold, m);            // hard edge -- no AA, this is the look

                s = lerp(s, 1.0 - s, saturate(_Invert));  // flicker frames

                fixed3 col = lerp(_BgColor.rgb, _FgColor.rgb, s);
                return fixed4(col, saturate(_Intensity));
            }
            ENDCG
        }
    }

    Fallback Off
}
