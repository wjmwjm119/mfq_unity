// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

//2017.6.27
//2017.6.17

Shader "@Moblie_WJM_Water" 
{
	Properties 
	{
		_SpecColor2("Specular Color", Color) = (0,0,0,1)
		_Shininess("Shininess", Range(0.001,5) ) = 0.8			
		_Color ("Main Color", Color) = (0,0,0,1)
		_MainTex ("DiffuseMap(RGBA)", 2D) = "white" {}
		_shadowIntensity("Shadow Intensity",Range(0,1)) = 0
		_lightmap_color("Lighting Color", Color) = (0,0,0,1)
		_LightMap ("SecondMap (CompleteMap or LightMap)", 2D) = "gray" {}
		_BumpMap("Normalmap", 2D) = "bump" {}

		_Smooth("Reflection Smooth ", Range(0,6)) = 0
		_Reflect("Reflect Blender", range(0,1)) = 0.5
		_FresnelPower("Fresnel Power", Range(0.1, 2)) = 0.1
		_FresnelBias("Fresnel Bias", Range(0.015, 1)) = 0.015

		_normalDis("Water Distortion", Range(0,50)) = 0.05
		_RefractionTex("Refraction Texture", 2D) = "black" {}

		_ReflectionTex("Reflection Texture", 2D) = "black" {}

		_WaveSpeed("WaveSpeed(Four_Direct)", Vector) = (-0.2,-0.2,0.1,0.1)

	}


	SubShader
	{		
		Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
		LOD 200
			
//		UsePass "Shader/Name"	
		pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }



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

			half4  _MainTex_ST;
			half4  _BumpMap_ST;

			half _shadowIntensity;
			half _Shininess;
			half4 _Color;
			half4 _SpecColor2;		
			half4 _lightmap_color;

			sampler2D _MainTex;
			sampler2D _LightMap;		
			sampler2D _BumpMap;

			half _Smooth;
			half _Reflect;
			half _FresnelPower;
			half _FresnelBias;

			sampler2D _ReflectionTex;

			sampler2D _RefractionTex;

//			half _RefractionDis;

//			half _ReflectionDis;

			half4 _WaveSpeed;

			half _normalDis;


			struct appdata 
			{
				float4 vertex:POSITION;
				half4 color:COLOR;
				half3 normal:NORMAL;
				half4 tangent:TANGENT;
				half4 texcoord:TEXCOORD0;
				half4 texcoord1:TEXCOORD1;
			};
		
			struct v2f
			{
				float4 pos :SV_POSITION;
//				half4 color:COLOR;
				half3 binormal :TEXCOORD7;
				half4 uvgrab : COLOR;
				half3 normal:NORMAL;
				half4 uv1And2:TEXCOORD0;
				half3 tangent:TEXCOORD1;
				half4 unityLightMapUVAndBumpUV:TEXCOORD2;
				float4 viewDir : TEXCOORD3;
				half3 lightDir : TEXCOORD4;
//				fixed fog : TEXCOORD1;

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
				o.uv1And2.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.unityLightMapUVAndBumpUV.zw= TRANSFORM_TEX(v.texcoord, _BumpMap);

				float3 eyePos = UnityObjectToViewPos(float4(v.vertex.xyz, 1)).xyz;
				half3 wNormal = mul((half3x3)unity_ObjectToWorld, v.normal.xyz);
				half3 wbinormal = mul((half3x3)unity_ObjectToWorld, cross(v.normal, v.tangent.xyz) * v.tangent.w);
				v.tangent.xyz = mul((half3x3)unity_ObjectToWorld, v.tangent.xyz);
				o.binormal = wbinormal;



   				o.lightDir= WorldSpaceLightDir(v.vertex);	

   				o.pos = UnityObjectToClipPos( v.vertex ); 			

				o.viewDir.xyz =_WorldSpaceCameraPos.xyz -mul( unity_ObjectToWorld, v.vertex ).xyz;
				o.tangent.xyz = v.tangent.xyz;
   				o.normal=mul( unity_ObjectToWorld,half4(v.normal,0));

				o.unityLightMapUVAndBumpUV.xy = 0;
				#ifndef LIGHTMAP_OFF
				o.unityLightMapUVAndBumpUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				o.uv1And2.zw = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;

				o.uvgrab = ComputeScreenPos(o.pos);

#if USING_FOG
				float fogCoord = length(eyePos.xyz); // radial fog distance
				UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
				o.viewDir.a = saturate(unityFogFactor);
#endif


 				TRANSFER_VERTEX_TO_FRAGMENT(o);			



  		  		return o;
			}
		
		half4 fragBase (v2f i):COLOR
		{

			float fog = i.viewDir.a;
			half4 final = 0;
			half3 bump0 = UnpackNormal(tex2D(_BumpMap,i.unityLightMapUVAndBumpUV.zw*(1+0.01*sin(0.3*_Time)*0.2) +0.1*_WaveSpeed.xy*_Time));
			half3 bump1 = UnpackNormal(tex2D(_BumpMap,i.unityLightMapUVAndBumpUV.zw*(1+0.07*sin(0.1*_Time)*0.2)+0.1*_WaveSpeed.zw*_Time));
			half3 bump = (bump0 + bump1) * 0.5;
			bump.x = -bump.x;
			bump.y = -bump.y;
//			bump.xy*=

			float2 reflUV = ((i.uvgrab.xy+ bump.xy*_normalDis) / i.uvgrab.w).xy;

			i.normal = normalize(i.normal);
			i.binormal = normalize(i.binormal);	
			i.tangent = normalize(i.tangent);

			float3x3 tangentSpaceToWordRotation = float3x3(i.tangent, i.binormal, i.normal);

			i.normal = normalize(mul(bump, tangentSpaceToWordRotation));

//			i.normal.z = -i.normal.z;

			i.viewDir=normalize(i.viewDir); 	

			half4 tex0 = tex2D(_MainTex, i.uv1And2.xy*(1 + 0.01*sin(0.1*_Time)*0.2) + 0.1*_WaveSpeed.xy*_Time);
			half4 tex1 = tex2D(_MainTex, i.uv1And2.xy*(1 + 0.07*sin(0.3*_Time)*0.2) + 0.1*_WaveSpeed.zw*_Time);
			half4 diffuseColor = (tex0 + tex1) * 0.5;

//			half4 diffuseColor=tex2D(_MainTex,i.uv1And2.xy);
			half lightDiff =max (0, dot (i.normal,_WorldSpaceLightPos0.xyz));						
			half atten=LIGHT_ATTENUATION(i);		

			half3 h = normalize (_WorldSpaceLightPos0 + i.viewDir);
			half vh = dot(i.viewDir,h);		
			half nh = max (0, dot (i.normal, h));
			
			half fresnel = 1.0f - saturate(dot(i.viewDir, i.normal));
			fresnel = pow(fresnel, _FresnelPower);
			fresnel = min(fresnel + _FresnelBias, 1.0f);
//			half3 worldRefl = normalize(-reflect(i.viewDir, i.normal));

			half4	resultRefr = 0;			
			resultRefr = tex2Dlod(_RefractionTex,half4( reflUV ,0, _Smooth));

			half4	resultRefl = 0;
			resultRefl = tex2Dlod(_ReflectionTex, half4(reflUV ,0 ,_Smooth));

			diffuseColor =lerp(diffuseColor, lerp(resultRefr, resultRefl, fresnel),_Reflect);


			half4 lightStrength = 0;
			#if !defined(LIGHTMAP_ON)
			lightStrength = 0.3*_LightColor0*lightDiff*atten;
			#endif

			half4 selfLight=_lightmap_color*half4(DecodeLightmap2(tex2D(_LightMap,i.uv1And2.zw)),1);

			#ifndef LIGHTMAP_OFF
			  selfLight += _Color*half4(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.unityLightMapUVAndBumpUV.xy)), 1);
			#else

			#endif



  			final.rgb=diffuseColor*(_Color);
	  	    final.rgb+=diffuseColor.rgb*selfLight.rgb;
  			
			half spec = pow(nh, _Shininess*2048);
			final.rgb +=10* spec*_SpecColor2;
     		final.rgb*= lerp(1, atten, _shadowIntensity);

//			final.rgb = lightDiff.xxx;
//			final.rgb = i.normal;

     		final.a=1;

#if USING_FOG
			final.rgb = lerp(unity_FogColor.rgb, final.rgb, fog);
#endif

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
