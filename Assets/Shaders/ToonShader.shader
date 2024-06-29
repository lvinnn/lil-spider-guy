Shader "Unlit/TestShader"
{
    Properties
    {
        _Color("Test Color", color) = (1,1,1,1)
        _Threshold1("Threshold1", Range(0, 1)) = 0.5
        _Threshold2("Threshold2", Range(0, 1)) = 0
        _Threshold3("Threshold3", Range(0, 1)) = 0
        _Threshold4("Threshold4", Range(0, 1)) = 0
        _FresnelPower("Fresnel Power", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            float4 _Color;
            float _Threshold1;
            float _Threshold2;
            float _Threshold3;
            float _Threshold4;
            float _FresnelPower;

            float remap (float val, float2 from, float2 to)
            {
                return to.x + (val - from.x) * (to.y - to.x) / (from.y - from.x);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate lighting
                float3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 NdotL = dot(normalize(i.worldNormal), worldLightDir);

                float scaledThreshold1 = remap(_Threshold1, float2(0,1), float2(-1,1));
                float scaledThreshold2 = remap(_Threshold2, float2(0,1), float2(scaledThreshold1,1));
                float scaledThreshold3 = remap(_Threshold3, float2(0,1), float2(scaledThreshold2,1));
                float scaledThreshold4 = remap(_Threshold4, float2(0,1), float2(scaledThreshold3,1));
                
                float lightLevel = NdotL<scaledThreshold1 ? .01 : (
                    NdotL<scaledThreshold2 ? .1 : (
                        NdotL < scaledThreshold3 ? .3 : (
                            NdotL < scaledThreshold4 ? .6 : 1)));
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1.0 - dot(viewDir, normalize(i.worldNormal)), _FresnelPower);
                float3 col = _Color.rgb*lightLevel;// + fresnel;                
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}