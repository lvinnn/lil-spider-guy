Shader "Unlit/TestShader"
{
	Properties
    {
        _Color("Base Color", Color) = (1,1,1,1)
        _ColorTint("Color Tint", Color) = (1,1,1,1)
    	_Smoothness("Smoothness", Float) = 0
    }
    SubShader
    {
        Tags {"RenderPipeline" = "UnivesalPipeline"}
        
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

			};

			float4 _ColorTint;
			float4 _Color;
			float _Smoothness;
			
			Interpolators Vertex(Attributes input) {
				Interpolators output;

				VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
				VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);
				output.positionCS = posnInputs.positionCS;
				output.normalWS = normInputs.normalWS;
				output.positionWS = posnInputs.positionWS;
				output.uv = input.uv;
				return output;
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
				half4 output;
				#if UNITY_VERSION >= 202120
					output = UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
				#else
					output = UniversalFragmentBlinnPhong(lightingInput, surfaceInput.albedo, half4(surfaceInput.specular,1), surfaceInput.smoothness, 0, surfaceInput.alpha);
				#endif

				output = output[0] < .5 ? half4(0,0,0,0) : half4(1,1,1,1);
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
				#if	UNITY_REVERSED_Z
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