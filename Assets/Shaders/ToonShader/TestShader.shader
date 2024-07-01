Shader "Unlit/TestShader"
{
    Properties
    {
        _Color("Base Color", Color) = (1,1,1,1)
        _ColorTint("Color Tint", Color) = (1,1,1,1)
        _Smoothness("Smoothness", Float) = 0
        _VoronoiScale("Voronoi Scale", Float) = 10.0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (.002, 0.1)) = .005
    }
    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline"}
        
		Pass 
		{
            Tags { "RenderType"="Opaque" }
		    
		    Cull Front
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha
		
			Name "OUTLINE"
			
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
            CBUFFER_START(UnityPerMaterial)
            float _Outline;
            float4 _OutlineColor;
            CBUFFER_END
			
            struct Attributes 
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
        
            struct Varyings 
            {
                float4 positionCS : SV_POSITION;
                half fogCoord : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            Varyings vert(Attributes input) 
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
	
                input.positionOS.xyz += input.normalOS.xyz * _Outline;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                
                output.color = _OutlineColor;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }
			
			half4 frag(Varyings i) : SV_Target
			{
				i.color.rgb = MixFog(i.color.rgb, i.fogCoord);
				return i.color;
			}
            ENDHLSL
		}
        
        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}
            
            HLSLPROGRAM

            #define _SPECULAR_COLOR
            #if UNITY_VERSION >= 202120
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #else
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #endif
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            #pragma vertex Vertex
            #pragma fragment Fragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
            };

            float4 _ColorTint;
            float4 _Color;
            float _Smoothness;
            float _VoronoiScale;
            
            Interpolators Vertex(Attributes input) {
                Interpolators output;

                VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);
                output.positionCS = posnInputs.positionCS;
                output.normalWS = normInputs.normalWS;
                output.positionWS = posnInputs.positionWS;
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(posnInputs.positionCS);
                
                return output;
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

            float remap(float value, float inputMin, float inputMax, float outputMin, float outputMax) {
                return outputMin + (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);
            }
            
            float4 Fragment(Interpolators input) : SV_TARGET {
                InputData lightingInput = (InputData)0;
                SurfaceData surfaceInput = (SurfaceData)0;

                lightingInput.normalWS = (input.normalWS);
                lightingInput.positionWS = input.positionWS;
                lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                
                surfaceInput.albedo = _Color.rgb * _ColorTint.rgb;
                surfaceInput.alpha = _Color.a * _ColorTint.a;
                surfaceInput.specular = 1;    
                surfaceInput.smoothness = _Smoothness;

                half output;
                #if UNITY_VERSION >= 202120
                    output = UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
                #else
                    output = UniversalFragmentBlinnPhong(lightingInput, surfaceInput.albedo, half4(surfaceInput.specular,1), surfaceInput.smoothness, 0, surfaceInput.alpha);
                #endif
                output *= 2;
                output = floor(output) / 2;
                output = clamp(output, .05f, 1);
                return output;
            }
            
            
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags {"lightMode" = "ShadowCaster"}
            
            ColorMask 0
            
            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float3 positionsOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Interpolators
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            float4 GetShadowCasterPositionCS(float3 positionWS, float3 normalWS)
            {
                float3 lightDirectionWS = _LightDirection;
                float4 positionCS  = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                #if    UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return positionCS;
            }
            
            Interpolators Vertex(Attributes input)
            {
                Interpolators output;
                VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionsOS);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);
                output.positionCS = GetShadowCasterPositionCS(posnInputs.positionWS, normInputs.normalWS);
                return output;
            }

            float4 Fragment(Interpolators input) : SV_TARGET
            {
                return 0;
            }
            
            ENDHLSL
        }
    }
}
