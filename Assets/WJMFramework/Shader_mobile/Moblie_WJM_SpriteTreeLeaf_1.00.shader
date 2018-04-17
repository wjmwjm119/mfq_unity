// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

//2017.6.27
//2017.6.17

//使用float精度，如果使用half精度ios下显示错位


Shader "@Moblie_WJM_SpriteTreeLeaf"
{
	Properties
	{

		_SpecColor2("Specular Color", Color) = (0,0,0,1)
		_Shininess("Shininess", Range(0.001,5)) = 0.8
		_Color("Main Color", Color) = (0,0,0,1)
		_MainTex("DiffuseMap(RGBA)", 2D) = "white" {}
	    _shadowIntensity("Shadow Intensity",Range(0,1)) = 0
		_lightmap_color("Lighting Color", Color) = (0,0,0,1)
		_LightMap("SecondMap (CompleteMap or LightMap)", 2D) = "gray" {}
	    _Cutoff("Alpha Cutoff", Range(0,1)) = 0.333

	}

		SubShader
	{
		LOD 200
		//		UsePass "Shader/Name"	
		pass
	{
		Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" "Queue" = "AlphaTest"  "RenderType" = "TransparentCutout" }
			Cull Off
			CGPROGRAM
#pragma vertex vertBase				
#pragma target 3.0 
#pragma fragment fragBase
#pragma multi_compile_fwdbase 
//#define UNITY_PASS_FORWARDBASE		
#pragma multi_compile_fog
#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

			float4 _MainTex_ST;
		float  _shadowIntensity;
		float  _Shininess;
		float4 _Color;
		float4 _SpecColor2;
		float4 _lightmap_color;
		float _Cutoff;

		sampler2D _MainTex;
		sampler2D _LightMap;

		float2 GetAngleDegree(float3 viewDir,float random)
		{
			float a;
			float b;

			float temp = sqrt(viewDir.y*viewDir.y + viewDir.x*viewDir.x);
			float mZconut = length(viewDir);
			float mXconut = acos(temp / mZconut);

			float i = 0;
			float j = 0;
			float k = 0;

			mXconut = mXconut / (0.5 *3.1415926);

			//用不到b,先注销
			/*
			if (viewDir.x>0 && viewDir.y >= 0)
			{
			b = atan(viewDir.y / viewDir.x) / 3.1415926;
			}
			if (viewDir.x<0 && viewDir.y >= 0)
			{
			b = 1 - atan(viewDir.y / -viewDir.x) / 3.1415926;
			}
			else if (viewDir.x<0 && viewDir.y >= 0)
			{
			b = 1 - atan(viewDir.y / -viewDir.x) / 3.1415926;
			}
			else if (viewDir.x<0 && viewDir.y <= 0)
			{
			b = 1 + atan(-viewDir.y / -viewDir.x) / 3.1415926;
			}
			else if (viewDir.x>0 && viewDir.y <= 0)
			{
			b = 2 - atan(-viewDir.y / viewDir.x) / 3.1415926;
			}

			//				b = 2 - b;
			//得到0～1
			//				b = 0.5*b;
			*/
			a = 1.0*mXconut;

			//				random = abs(random);

			//得到0～1，k没用
			//				b = modf(b + random, k);

			//使用1行为一圈
			//				b = b * 1;

			float eachSize = 1.0 / 16;
			//1除16
			//				i = floor(modf(b, k) / eachSize);
			j = floor(a / (eachSize * 1)) * 1 + k;

			return float2(i, j);
		}

		struct appdata
		{
			float4 vertex:POSITION;
			//				float4 color:COLOR;
			float3 normal:NORMAL;
			float4 texcoord:TEXCOORD0;
			float4 texcoord1:TEXCOORD1;
			float4 worldPosAndScale:COLOR;

		};

		struct v2f
		{
			float4 pos :SV_POSITION;
			//				float4 color:COLOR;
			float3 normal:NORMAL;
			float4 uv1And2:TEXCOORD0;
			float3 worldPos:TEXCOORD1;
			fixed fog : TEXCOORD7;
			float2 unityLightMapUV:TEXCOORD2;
			float3 viewDir : TEXCOORD3;
			float3 lightDir : TEXCOORD4;
			LIGHTING_COORDS(5, 6)

		};

		//移动版就用以下参数
		inline float3 DecodeLightmap2(float4 color)
		{
			return 2.0 * color.rgb;
		}

		v2f vertBase(appdata v)
		{
			v2f o = (v2f)0;
			//默认UV为（0.875，0.875）～（1，1），为了还原uv1到0～1空间，进行以下操作
			v.texcoord.xy= float2(v.texcoord.x - 0.875, v.texcoord.y - 0.875) * 8;
			o.uv1And2.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
//			o.uv1And2.xy = float2(o.uv1And2.x - 0.875, o.uv1And2.y - 0.875) * 8;
			o.lightDir = WorldSpaceLightDir(v.vertex);

			o.viewDir = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
			float3 eyePos = UnityObjectToViewPos(float4(v.vertex.xyz, 1)).xyz;
			o.normal = mul(unity_ObjectToWorld,float4(v.normal,0));

			o.unityLightMapUV.xy = 0;
#ifndef LIGHTMAP_OFF
			o.unityLightMapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif
			o.uv1And2.zw = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;

			float3 viewDir = float3(0, 0, 1);
			viewDir = mul(float4(viewDir, 1), UNITY_MATRIX_V);

			float3 viewX = float3(1,0,0);
			float3 viewY = float3(0,1,0);

			//centerPos
			//mul(unity_ObjectToWorld, float4(0, 0, 0, 1)


			o.worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)) + v.worldPosAndScale;


			//view to world Space
			viewX = mul(float4(viewX, 1), UNITY_MATRIX_V);
			viewY = mul(float4(viewY, 1), UNITY_MATRIX_V);

			viewX = normalize(viewX);
			viewY = normalize(viewY);

			float width = v.texcoord.x - 0.5;
			float height = v.texcoord.y - 0.5;

			float3 scale = mul(float4(1,0,0, 0), UNITY_MATRIX_M);
			float scaleLenth = length(scale);

			//				//放大了两倍
			float3 spriteFace = viewX*width * 1.2 * scaleLenth *v.worldPosAndScale.a + viewY*height * 1.2 * scaleLenth *v.worldPosAndScale.a + o.worldPos;



			v.vertex.xyz = spriteFace;


			//				v.vertex= mul(unity_WorldToObject, v.vertex);
			//				v.vertex = mul(unity_ObjectToWorld, v.vertex);


			//				float colCount = 8;
			float random = 0;
			float oneStep = 1.0 / 8;

			random = o.worldPos.x*0.3 + o.worldPos.z*0.3;

			//得到视角的XY0～15值整数值，不使用水平切换，只使用垂直切换
			float2 viewXYid = GetAngleDegree(viewDir.xzy,random);


			float colID = 2 * floor(random * 4);

			float needAddCol = floor(viewXYid.y / 8);
			viewXYid.y = viewXYid.y % 8;

			o.uv1And2.xy = float2(oneStep*(colID + needAddCol),oneStep*viewXYid.y) + o.uv1And2.xy*float2(oneStep,oneStep);

			o.pos = mul(UNITY_MATRIX_VP, v.vertex);

#if USING_FOG
			float fogCoord = length(eyePos.xyz); // radial fog distance
			UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
			o.fog = saturate(unityFogFactor);
#endif

			TRANSFER_VERTEX_TO_FRAGMENT(o);

			return o;
		}

		float4 fragBase(v2f i) :COLOR
		{
			float4 final = 0;

			i.normal = normalize(i.normal);
			i.viewDir = normalize(i.viewDir);

			float4 diffuseColor = tex2D(_MainTex,i.uv1And2.xy);

			clip(diffuseColor.a - _Cutoff);

			float lightDiff = max(0, dot(i.normal,_WorldSpaceLightPos0.xyz));
			float atten = LIGHT_ATTENUATION(i);

			float3 h = normalize(_WorldSpaceLightPos0 + i.viewDir);
			float vh = dot(i.viewDir,h);
			float nh = max(0, dot(i.normal, h));

			float4 lightStrength = 0;
#if !defined(LIGHTMAP_ON)
			lightStrength = _LightColor0*lightDiff*atten;
#endif

			float4 selfLight = _lightmap_color*float4(DecodeLightmap2(tex2D(_LightMap,i.uv1And2.zw)),1);

#ifndef LIGHTMAP_OFF
			selfLight += _Color*float4(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.unityLightMapUV.xy)), 1);
#else

#endif

			final.rgb = diffuseColor*lightStrength.rgb*_Color;
			final.rgb += diffuseColor.rgb*selfLight.rgb;

			float spec = pow(nh, _Shininess * 2048);
			final.rgb += 5 * spec*_SpecColor2*diffuseColor;
			final.rgb *= lerp(1, atten, _shadowIntensity);

			final.a = 1;

#if USING_FOG
			final.rgb = lerp(unity_FogColor.rgb, final.rgb, i.fog);
#endif


			return 	final;
		}


			ENDCG
	}//pass


	 //1

	 // Pass to render object as a shadow caster
	Pass{
		Name "Caster"
		Tags{ "LightMode" = "ShadowCaster" }

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile_shadowcaster
#pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders
#include "UnityCG.cginc"

		struct v2f {
		V2F_SHADOW_CASTER;
		float2  uv : TEXCOORD1;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	uniform float4 _MainTex_ST;

	v2f vert(appdata_full v)
	{
		v2f o;

		//			float3 scale = mul(float4(1, 0, 0, 0), UNITY_MATRIX_M);
		//  		float scaleLenth = length(scale);


//		float2 scale2 = (v.texcoord.xy - float2(0.5, 0.5));

		//放大了两倍
//		v.vertex.xyz = v.vertex.xyz - v.color.a*(3 - 1)*float3(scale2.x,0,scale2.y);

		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		return o;
	}

	uniform sampler2D _MainTex;
	uniform fixed _Cutoff;
	uniform fixed4 _Color;

	float4 frag(v2f i) : SV_Target
	{
		fixed4 texcol = tex2D(_MainTex, i.uv*0.125 + 0.125 * 6);
	clip(texcol.a*_Color.a - _Cutoff);

	SHADOW_CASTER_FRAGMENT(i)
	}
		ENDCG

	}


	} //subshader

//	FallBack "VertexLit"
}//shader
