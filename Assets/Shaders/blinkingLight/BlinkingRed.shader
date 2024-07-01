Shader "Unlit/Blinking Red"
{
    Properties
    {
        _BlinkSpeed("blink speed", Float) = 200000
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float _BlinkSpeed;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(1, 0, 0, _Time[1] % _BlinkSpeed < _BlinkSpeed/2 ? 0 : 1);
            }
            ENDCG
        }
    }
}
