// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

//2017.6.27
//2017.6.17

Shader "@Moblie_WJM_Glass_Middle" 
{
	Properties 
	{
		_SpecColor2("Specular Color", Color) = (0,0,0,1)
		_Shininess("Shininess", Range(0.001,5) ) = 0.8	
		_Color ("RGB_Alpha (A)", Color) = (0,0,0,1)
		_MainTex ("DiffuseMap(RGBA)", 2D) = "white" {}
		_shadowIntensity("Shadow Intensity",Range(0,1)) = 0
		_lightmap_color("Lighting Color", Color) = (0,0,0,1)
		_LightMap ("SecondMap (CompleteMap or LightMap)", 2D) = "gray" {}

		_Smooth("Reflection Smooth ", Range(0,6)) = 0
		_Reflect("Reflect Blender", range(0,1)) = 0.5
		_FresnelPower("Fresnel Power", Range(0.1, 2)) = 0.1
		_FresnelBias("Fresnel Bias", Range(0.015, 1)) = 0.015

	}

	SubShader
	{		
		Tags{ "Queue" = "Transparent"  "RenderType" = "Transparent" }
		LOD 200
			
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
			#pragma multi_compile_fog
			#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			half4	_MainTex_ST;
			half _shadowIntensity;
			half _Shininess;
			half4 _Color;
			half4 _SpecColor2;		
			half4 _lightmap_color;
			half _alphaSin;

			sampler2D _MainTex;
			sampler2D _LightMap;

			half _Smooth;
			half _Reflect;
			half _FresnelPower;
			half _FresnelBias;

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
				float3 worldPos:TEXCOORD1;
				half2 unityLightMapUV:TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				half3 lightDir : TEXCOORD4;
				fixed fog : TEXCOORD7;
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
				float3 eyePos = UnityObjectToViewPos(float4(v.vertex.xyz, 1)).xyz;
   				o.pos = UnityObjectToClipPos( v.vertex ); 	
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.viewDir =_WorldSpaceCameraPos.xyz -mul( unity_ObjectToWorld, v.vertex ).xyz;
	  			o.color.xyz=v.color;
   				o.normal=mul( unity_ObjectToWorld,half4(v.normal,0));

				o.unityLightMapUV.xy = 0;
				#ifndef LIGHTMAP_OFF
				o.unityLightMapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				o.uv1And2.zw = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;

#if USING_FOG
				float fogCoord = length(eyePos.xyz); // radial fog distance
				UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
				o.fog = saturate(unityFogFactor);
#endif
 				TRANSFER_VERTEX_TO_FRAGMENT(o);			

  		  		return o;
			}
		
		half4 fragBase (v2f i):COLOR
		{
			half4 final = 0;

			i.normal=normalize(i.normal);		
			i.viewDir=normalize(i.viewDir); 	

			half4 diffuseColor=tex2D(_MainTex,i.uv1And2.xy);
			half lightDiff =max (0, dot (i.normal,_WorldSpaceLightPos0.xyz));						
			half atten=LIGHT_ATTENUATION(i);		

			half3 h = normalize (_WorldSpaceLightPos0 + i.viewDir);
			half vh = dot(i.viewDir,h);		
			half nh = max (0, dot (i.normal, h));
			

			half fresnel = 1.0f - saturate(dot(i.viewDir, i.normal));
			fresnel = pow(fresnel, _FresnelPower);
			fresnel = min(fresnel + _FresnelBias, 1.0f);
			half3 worldRefl = normalize(-reflect(i.viewDir, i.normal));

			#if UNITY_SPECCUBE_BOX_PROJECTION		
			worldRefl = BoxProjectedCubemapDirection(worldRefl, i.worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
			#endif
			half4 refl = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, worldRefl, _Smooth);

			fixed4 resultRefl = 0;
			resultRefl.rgb = DecodeHDR_NoLinearSupportInSM2(refl, unity_SpecCube0_HDR).xyz;

			resultRefl = resultRefl*_Reflect*fresnel;

			diffuseColor = diffuseColor*(1 - _Reflect + _Reflect*diffuseColor.a) + resultRefl*(1 - diffuseColor.a);

			//diffuseColor = lerp(diffuseColor, resultRefl, _Reflect*fresnel*diffuseColor.a);


			half4 lightStrength = 0;
			#if !defined(LIGHTMAP_ON)
			lightStrength = _LightColor0*lightDiff*atten;
			#endif

			half4 selfLight=_lightmap_color*half4(DecodeLightmap2(tex2D(_LightMap,i.uv1And2.zw)),1);

			#ifndef LIGHTMAP_OFF
			  selfLight += _Color* half4(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.unityLightMapUV.xy)), 1);
			#else

			#endif

  			final.rgb=diffuseColor*lightStrength.rgb*_Color;
	  	    final.rgb+=diffuseColor.rgb*selfLight.rgb;
  			
			half spec = pow(nh, _Shininess*2048);
			final.rgb +=5* spec*_SpecColor2*diffuseColor;
     		final.rgb*= lerp(1, atten, _shadowIntensity);

			final.a = diffuseColor.a + (1 - diffuseColor.a)*_Color.a;
//			final.rgb = cubeColor.rgb;

#if USING_FOG
			final.rgb = lerp(unity_FogColor.rgb, final.rgb, i.fog);
#endif


			return final;
		}
		
						
			ENDCG
		}//pass
		

		//1

		//  Shadow rendering pass
		Pass{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
#pragma target 2.0

#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON 
#pragma shader_feature _METALLICGLOSSMAP
#pragma skip_variants SHADOWS_SOFT
#pragma multi_compile_shadowcaster

#pragma vertex vertShadowCaster
#pragma fragment fragShadowCaster

#include "UnityStandardShadow.cginc"

			ENDCG
		}

	
	} //subshader
	
//	FallBack "VertexLit"
}//shader
