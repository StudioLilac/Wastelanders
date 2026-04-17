Shader "Unlit/CutoutScrim"
{
    Properties
    {
        _Color ("Tint", Color) = (0,0,0,0.8)
        _CutoutRect ("Cutout Rect", Vector) = (0,0,0,0)
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float4 _CutoutRect;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                if (i.uv.x >= _CutoutRect.x && i.uv.x <= _CutoutRect.z &&
                    i.uv.y >= _CutoutRect.y && i.uv.y <= _CutoutRect.w)
                {
                    return fixed4(0, 0, 0, 0);
                }
                
                return _Color;
            }
            ENDCG
        }
    }
}
