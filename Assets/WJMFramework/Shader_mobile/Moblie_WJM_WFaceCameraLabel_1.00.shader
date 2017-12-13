// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

//2017.6.27
//2017.6.17

Shader "@Moblie_WJM_WFaceCameraLabel"
{
	Properties
	{
		_Color("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex("DiffuseMap(RGBA)", 2D) = "white" {}
		_Width("Width",float) = 1
		_Height("Height",float) = 1
		_sizeBlend("Object&Screen",Range(0,1)) = 0
		_scale("Scale",Range(0,2)) = 0.8
		_PviotOffsetX("Pviot OffsetX",Range(-1,1)) = 0
		_PviotOffsetY("Pviot OffsetY",Range(-1,1)) = 0
		_aniImageCount("Ani Image Count",int)=1
	}

		SubShader
	{
			Tags{ "Queue" = "Transparent+2000" "RenderType" = "Transparent" }
			LOD 200
			Cull Off
		//		UsePass "Shader/Name"	
		pass
	{
		Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha



			Cull Off
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

		float4 _MainTex_ST;
		float4 _Color;
		sampler2D _MainTex;

		float	_Width;
		float   _Height;
		float _sizeBlend;

		float _PviotOffsetX;
		float _PviotOffsetY;
		float _scale;
		float _aniImageCount;

		struct appdata
		{
			float4 vertex:POSITION;
			float4 texcoord:TEXCOORD0;
		};

		struct v2f
		{
			float4 pos :SV_POSITION;
			float4 uv1And2:TEXCOORD0;
			float3 worldPos:TEXCOORD1;
			fixed fog : TEXCOORD7;
			float3 viewDir : TEXCOORD3;
			LIGHTING_COORDS(5, 6)
		};


		v2f vertBase(appdata v)
		{
			v2f o = (v2f)0;
			o.uv1And2.xy = TRANSFORM_TEX(v.texcoord, _MainTex);


			float3 eyePos = UnityObjectToViewPos(float4(v.vertex.xyz, 1)).xyz;


			float3 scale = mul(float4(1,0,0, 0), UNITY_MATRIX_M);
			float scaleLenth = length(scale);

			float4 ori = mul(UNITY_MATRIX_MV, float4(0, 0, 0, 1));

			scaleLenth *= lerp(0.5, 0.001, _sizeBlend);
			//相机在70度的前提下乘以1.7187,能使图片大小等于实际大小
			scaleLenth *= 1.7187*_scale;

			v.vertex.x *= _Width*scaleLenth;
			v.vertex.z *= _Height*scaleLenth;
			v.vertex.x += _PviotOffsetX*_Width*scaleLenth;
			v.vertex.z += _PviotOffsetY*_Height*scaleLenth;



			float4 vt = v.vertex*lerp(1,-ori.z*5, _sizeBlend);
		
			vt.y = vt.z;
			vt.xy *= lerp(1, 0.2, _sizeBlend);
			vt.z = ori.z;
			vt.xy += ori.xy;
		
			o.pos = mul(UNITY_MATRIX_P, vt);


			o.uv1And2.x=1- (o.uv1And2.x*1/_aniImageCount+floor(_Time.y*_aniImageCount % _aniImageCount)* 1 / _aniImageCount);


			TRANSFER_VERTEX_TO_FRAGMENT(o);

			return o;
		}

		half4 fragBase(v2f i) :COLOR
		{
			float4 final = 0;
			float4 diffuseColor = tex2D(_MainTex,1-i.uv1And2.xy);


			final.rgb =2.0*diffuseColor*_Color;

			clip(diffuseColor.a - 0.1);

			final.a = diffuseColor.a*_Color.a;

#if USING_FOG
			final.rgb = lerp(unity_FogColor.rgb, final.rgb, i.fog);
#endif


			return 	final;
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
