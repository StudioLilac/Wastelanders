Shader "UI/TutorialCutoutFeathered"
{
    Properties
    {
        _Color ("Tint", Color) = (0,0,0,0.8)
        _CutoutRect ("Cutout Rect", Vector) = (0,0,0,0)
        _FeatherPixels ("Feather (Pixels)", Range(0, 100)) = 20.0
        _HoleAlpha ("Hole Alpha", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            float4 _CutoutRect;
            float _FeatherPixels;
            float _HoleAlpha;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            // Shader creates a feather around the focus
            fixed4 frag (v2f i) : SV_Target
            {
                float2 pixelPos = i.texcoord * _ScreenParams.xy;
                float2 minPx = _CutoutRect.xy * _ScreenParams.xy;
                float2 maxPx = _CutoutRect.zw * _ScreenParams.xy;

                float2 center = (minPx + maxPx) * 0.5;
                float2 halfSize = (maxPx - minPx) * 0.5;

                float2 d = abs(pixelPos - center) - halfSize;
                float dist = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);

                float alphaMultiplier = smoothstep(0.0, _FeatherPixels, dist);

                float finalMultiplier = lerp(_HoleAlpha, 1.0, alphaMultiplier);

                fixed4 finalColor = _Color;
                finalColor.a *= finalMultiplier;
                
                return finalColor;
            }
            ENDCG
        }
    }
}