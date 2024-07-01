Shader "Custom/ToonPostProcess"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _VoronoiScale ("Voronoi Scale", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            ZTest Always Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _VoronoiScale;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float random2d(float2 p) {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float voronoi(float2 uv, float scale) {
                uv *= scale; // Scale the UV coordinates
                float2 g = floor(uv); // Grid cell coordinates
                float2 f = frac(uv); // Fractional part within the grid cell

                float md = 1.0; // Minimum distance

                // Loop over neighboring grid cells
                for (int y = -1; y <= 1; y++) {
                    for (int x = -1; x <= 1; x++) {
                        float2 neighbor = float2(x, y); // Neighbor cell coordinates
                        float2 p = neighbor + 0.5; // Center of the neighbor cell
                        float d = distance(f, p); // Distance to the neighbor cell center
                        if (d < md) {
                            md = d;
                        }
                    }
                }

                return md;
            }
            

            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenUV = i.uv;
                screenUV.x *= _ScreenParams.x / _ScreenParams.y; // Adjust for aspect ratio

                float voronoiPattern = voronoi(screenUV, _VoronoiScale);
                float voronoiPattern2 = voronoi(screenUV+float2(1,1), _VoronoiScale);
                

                fixed4 col = tex2D(_MainTex, i.uv);

                // Apply toon shading and Voronoi pattern
                float luminance = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));

                if(luminance == 0)
                {
                    
                }
                else if(luminance < .1)
                {
                    col.rgb = voronoiPattern < .3f ? 0 : 1;
                    col.rgb *= voronoiPattern2 < .3f ? 0 : .05f;
                }
                else if(luminance < .6)
                    col.rgb = voronoiPattern < .22f ? .2 : .7;
                // else
                    // col.rgb = 1 - voronoiPattern < .65f ? 0 : 1;
                
                return col;
            }
            ENDCG
        }
    }
}
