// Draws a flat white silhouette of whatever renderer it is applied to.
// Used as an override material by ImpactFrameController's command buffer,
// so it never renders through a normal camera pass.
//
// The source texture is fed via a GLOBAL named _SilhouetteTex (set per-draw).
// It is deliberately NOT declared in Properties -- a material-level texture
// property would shadow the global and every silhouette would come out as a
// solid quad.
Shader "Impact/Mask"
{
    Properties
    {
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "IgnoreProjector"="True" }

        Cull Off
        ZWrite Off
        ZTest Always        // sorting is irrelevant, every fragment writes the same value
        Blend One One       // additive: overlapping silhouettes saturate instead of punching holes
        ColorMask R         // mask RT is single channel

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

            sampler2D _SilhouetteTex;
            float _Cutoff;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = v.uv;              // sprite atlas UVs are already baked into the mesh
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // vertex alpha matters for particles fading over lifetime
                fixed a = tex2D(_SilhouetteTex, i.uv).a * i.color.a;
                clip(a - _Cutoff);
                return fixed4(1, 0, 0, 1);
            }
            ENDCG
        }
    }

    Fallback Off
}
