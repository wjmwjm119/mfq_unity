// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

//2017.6.27
//2017.6.17

Shader "@Moblie_WJM_360" 
{
	Properties 
	{
		_Color ("RGB_A(BlendCube)", Color) = (1,1,1,1)
		_CubeMap("Cubemap", Cube) = "" {  }
		_CubeMap2("Cubemap2", Cube) = "" {  }

		_lightmap_color("Lighting Color", Color) = (1,1,1,1)
		_LightMap ("SecondMap (CompleteMap or LightMap)", 2D) = "gray" {}

	}


	SubShader
	{		
			Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 200
			Cull back
//		UsePass "Shader/Name"	
		pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }


			CGPROGRAM
			#pragma vertex vertBase				
			#pragma target 2.0 
			#pragma fragment fragBase
			#pragma multi_compile_fwdbase 
			#define UNITY_PASS_FORWARDBASE		

			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			half4 _MainTex_ST;
			half4 _Color;	
			half4 _lightmap_color;

			sampler2D _LightMap;		
			samplerCUBE _CubeMap;
			samplerCUBE _CubeMap2;

			struct appdata 
			{
				float4 vertex:POSITION;
				half4 color:COLOR;
				float3 normal:NORMAL;
				half4 texcoord:TEXCOORD0;
				half4 texcoord1:TEXCOORD1;
			};
		
			struct v2f
			{
				float4 pos :SV_POSITION;
				half4 color:COLOR;
				float3 normal:NORMAL;
				half4 uv1And2:TEXCOORD0;
				float3 worldPos :TEXCOORD1;
				half2 unityLightMapUV:TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				LIGHTING_COORDS(5, 6)


			};
			
			//移动版就用以下参数
			inline half3 DecodeLightmap2( half4 color )
			{
				return 2.0 * color.rgb;
			}	   	

			v2f vertBase (appdata v) 
			{		
   				v2f o=(v2f)0;		   			 
   				o.uv1And2.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

   				o.pos = UnityObjectToClipPos( v.vertex ); 	
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				o.normal = mul(unity_ObjectToWorld, float4(-v.normal, 0));
				o.normal = mul(unity_WorldToObject, float4(-v.normal, 0));

				o.viewDir =_WorldSpaceCameraPos.xyz - o.worldPos;

				o.unityLightMapUV.xy = 0;
				#ifndef LIGHTMAP_OFF
				o.unityLightMapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				o.uv1And2.zw = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;


 				TRANSFER_VERTEX_TO_FRAGMENT(o);			

  		  		return o;
			}
		
		half4 fragBase (v2f i):COLOR
		{
			half4 final = 0;

			i.normal=normalize(i.normal);		
//			i.viewDir=normalize(i.viewDir); 	
			
			half4 diffuseColor=texCUBE(_CubeMap,normalize(i.normal));
			half4 diffuseColor2=texCUBE(_CubeMap2,normalize(i.normal));
			diffuseColor = lerp(diffuseColor2,diffuseColor,  _Color.a);

			half4 selfLight=_lightmap_color*half4(DecodeLightmap2(tex2D(_LightMap,i.uv1And2.zw)),1);

			#ifndef LIGHTMAP_OFF
			  selfLight += _Color*half4(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.unityLightMapUV.xy)), 1);
			#else

			#endif

	  	    final.rgb=diffuseColor.rgb*selfLight.rgb;
  			
			return final;
		}
		
						
			ENDCG
		}//pass
		

		//1

		Pass{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 2.0

			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON 
			#pragma multi_compile_shadowcaster

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"

			ENDCG
		}

	
	} //subshader
	
//	FallBack "VertexLit"
}//shader
