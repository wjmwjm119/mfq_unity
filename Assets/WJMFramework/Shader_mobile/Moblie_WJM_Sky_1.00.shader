// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

//2017.6.27
//2017.6.17

Shader "@Moblie_WJM_Sky" 
{
	Properties 
	{
		_Color ("RGB_Alpha (A)", Color) = (0,0,0,1)
		_CubeMap("Sky Cubemap ", Cube) = "" {  }
//		_MainTex ("DiffuseMap(RGBA)", 2D) = "white" {}

		_lightmap_color("Lighting Color", Color) = (0,0,0,1)
		_LightMap ("SecondMap (CompleteMap or LightMap)", 2D) = "gray" {}
		_alphaSin("SanSuo ",Range(0,1)) = 0
	}


	SubShader
	{		
			Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 200
			Cull Off
//		UsePass "Shader/Name"	
		pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vertBase				
			#pragma target 3.0 
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
			half _alphaSin;

//			sampler2D _MainTex;
			sampler2D _LightMap;		
			samplerCUBE _CubeMap;

			struct appdata 
			{
				float4 vertex:POSITION;
				half4 color:COLOR;
				half3 normal:NORMAL;
				half4 texcoord:TEXCOORD0;
				half4 texcoord1:TEXCOORD1;
			};
		
			struct v2f
			{
				float4 pos :SV_POSITION;
				half4 color:COLOR;
				half3 normal:NORMAL;
				half4 uv1And2:TEXCOORD0;
				float3 worldPos :TEXCOORD1;
				half2 unityLightMapUV:TEXCOORD2;
				half3 viewDir : TEXCOORD3;
				half3 lightDir : TEXCOORD4;
				LIGHTING_COORDS(5,6)
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
   				o.lightDir= WorldSpaceLightDir(v.vertex);	

   				o.pos = UnityObjectToClipPos( v.vertex ); 	
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				o.viewDir =_WorldSpaceCameraPos.xyz - o.worldPos;
	  			o.color.xyz=v.color;
   				o.normal=mul( unity_ObjectToWorld,half4(v.normal,0));

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
			i.viewDir=normalize(i.viewDir); 	
			
			half4 diffuseColor=texCUBE(_CubeMap,normalize(i.worldPos));
			half lightDiff =max (0, dot (i.normal,_WorldSpaceLightPos0.xyz));						
			half atten=LIGHT_ATTENUATION(i);		
		
			half4 lightStrength = 0;
			#if !defined(LIGHTMAP_ON)
			lightStrength = _LightColor0*lightDiff*atten;
			#endif

			half4 selfLight=_lightmap_color*half4(DecodeLightmap2(tex2D(_LightMap,i.uv1And2.zw)),1);

			#ifndef LIGHTMAP_OFF
			  selfLight += _Color*half4(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.unityLightMapUV.xy)), 1);
			#else

			#endif

  			final.rgb=diffuseColor*lightStrength.rgb*_Color;
	  	    final.rgb+=diffuseColor.rgb*selfLight.rgb;
  			
			final.a = _Color.a*diffuseColor.a*lerp(1, 0.5*(sin(2 * _Time.y) + 1.2), _alphaSin);

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
			#pragma target 3.0

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
