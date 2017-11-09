// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BrunetonsAtmosphere/PostEffectAtmosphericScattering"
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Transmittance("Transmittance", 2D) = "black" {}
		_Inscatter("Inscatter", 3D) = "black" {}
		_Scale("Scale", float) = 2

		_Start("Start",float)=50
		_End("End",float)=500


	}
	SubShader 
	{
	    Pass 
	    {
	    	ZTest Always
	    	ZWrite off
	    	Fog { Mode Off }
	    	Cull front 
	    
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma target 3.0
		#include "Lighting.cginc"

		sampler2D _MainTex;
		sampler2D _CameraDepthTexture;
		float4x4 _FrustumCorners;
		float4 _MainTex_TexelSize;

		//The scattering tables are based on real world sizes.
		//Your game world will probably be smaller. This will scale up
		//the size and has the effect of making the scattering stronger.
		float _Scale;

		float _Start;
		float _End;



		sampler2D _Transmittance;
		sampler3D _Inscatter;

		#define EARTH_POS  float3(0.0f, 6360010.0f, 0.0f)

//		float3 SUN_DIR;
//		float SUN_INTENSITY;

		#define betaR  float3(0.0058f, 0.0135f, 0.0331f)

		#define mieG 0.75f

		#define M_PI 3.141592
		#define Rg 6360000.0
		#define Rt 6420000.0
		#define RL 6421000.0
		#define RES_R 32.0
		#define RES_MU 64.0
		#define RES_MU_S 32.0
		#define RES_NU 8.0


		float3 hdr(float3 L)
		{
			L = L * 0.4;
			L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
			L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
			L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);
			return L;
		}

		float4 Texture4D(sampler3D table, float r, float mu, float muS, float nu)
		{
			float H = sqrt(Rt * Rt - Rg * Rg);
			float rho = sqrt(r * r - Rg * Rg);

			float rmu = r * mu;
			float delta = rmu * rmu - r * r + Rg * Rg;
			float4 cst = rmu < 0.0 && delta > 0.0 ? float4(1.0, 0.0, 0.0, 0.5 - 0.5 / RES_MU) : float4(-1.0, H * H, H, 0.5 + 0.5 / RES_MU);
			float uR = 0.5 / RES_R + rho / H * (1.0 - 1.0 / RES_R);
			float uMu = cst.w + (rmu * cst.x + sqrt(delta + cst.y)) / (rho + cst.z) * (0.5 - 1.0 / float(RES_MU));
			// paper formula
			//float uMuS = 0.5 / RES_MU_S + max((1.0 - exp(-3.0 * muS - 0.6)) / (1.0 - exp(-3.6)), 0.0) * (1.0 - 1.0 / RES_MU_S);
			// better formula
			float uMuS = 0.5 / RES_MU_S + (atan(max(muS, -0.1975) * tan(1.26 * 1.1)) / 1.1 + (1.0 - 0.26)) * 0.5 * (1.0 - 1.0 / RES_MU_S);

			float lep = (nu + 1.0) / 2.0 * (RES_NU - 1.0);
			float uNu = floor(lep);
			lep = lep - uNu;

			return tex3D(table, float3((uNu + uMuS) / RES_NU, uMu, uR)) * (1.0 - lep) + tex3D(table, float3((uNu + uMuS + 1.0) / RES_NU, uMu, uR)) * lep;
		}

		float3 GetMie(float4 rayMie)
		{
			// approximated single Mie scattering (cf. approximate Cm in paragraph "Angular precision")
			// rayMie.rgb=C*, rayMie.w=Cm,r
			return rayMie.rgb * rayMie.w / max(rayMie.r, 1e-4) * (betaR.r / betaR);
		}

		float PhaseFunctionR(float mu)
		{
			// Rayleigh phase function
			return (3.0 / (16.0 * M_PI)) * (1.0 + mu * mu);
		}

		float PhaseFunctionM(float mu)
		{
			// Mie phase function
			return 1.5 * 1.0 / (4.0 * M_PI) * (1.0 - mieG*mieG) * pow(1.0 + (mieG*mieG) - 2.0*mieG*mu, -3.0 / 2.0) * (1.0 + mu * mu) / (2.0 + mieG*mieG);
		}

		float3 Transmittance(float r, float mu)
		{
			// transmittance(=transparency) of atmosphere for infinite ray (r,mu)
			// (mu=cos(view zenith angle)), intersections with ground ignored
			float uR, uMu;
			uR = sqrt((r - Rg) / (Rt - Rg));
			uMu = atan((mu + 0.15) / (1.0 + 0.15) * tan(1.5)) / 1.5;

			return tex2D(_Transmittance, float2(uMu, uR)).rgb;
		}

		float3 TransmittanceWithShadow(float r, float mu)
		{
			// transmittance(=transparency) of atmosphere for infinite ray (r,mu)
			// (mu=cos(view zenith angle)), or zero if ray intersects ground

			return mu < -sqrt(1.0 - (Rg / r) * (Rg / r)) ? float3(0, 0, 0) : Transmittance(r, mu);
		}


		float3 SunRadiance(float3 worldPos, float3 SUN_DIR,float3 SUN_INTENSITY)
		{
			float3 worldV = normalize(worldPos + EARTH_POS); // vertical vector
			float r = length(worldPos + EARTH_POS);
			float muS = dot(worldV, SUN_DIR);

			return TransmittanceWithShadow(r, muS) * SUN_INTENSITY;
		}



		float3 SkyRadiance(float3 camera, float3 viewdir,out float3 extinction,float3 SUN_DIR,float3 SUN_INTENSITY)
		{
			// scattered sunlight between two points
			// camera=observer
			// viewdir=unit vector towards observed point
			// sundir=unit vector towards the sun
			// return scattered light

			camera += EARTH_POS;

			float3 result = float3(0, 0, 0);
			float r = length(camera);
			float rMu = dot(camera, viewdir);
			float mu = rMu / r;
			float r0 = r;
			float mu0 = mu;

			float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);
			float din = max(-rMu - deltaSq, 0.0);
			if (din > 0.0)
			{
				camera += din * viewdir;
				rMu += din;
				mu = rMu / Rt;
				r = Rt;
			}

			float nu = dot(viewdir, SUN_DIR);
			float muS = dot(camera, SUN_DIR) / r;

			float4 inScatter = Texture4D(_Inscatter, r, rMu / r, muS, nu);
			extinction = Transmittance(r, mu);

			if (r <= Rt)
			{
				float3 inScatterM = GetMie(inScatter);
				float phase = PhaseFunctionR(nu);
				float phaseM = PhaseFunctionM(nu);
				result = inScatter.rgb * phase + inScatterM * phaseM;
			}
			else
			{
				result = float3(0, 0, 0);
				extinction = float3(1, 1, 1);
			}

			return result * SUN_INTENSITY;
		}

		float3 InScattering(float3 camera, float3 _point, out float3 extinction, float shaftWidth, float3 SUN_DIR,float3 SUN_INTENSITY)
		{

			// single scattered sunlight between two points
			// camera=observer
			// point=point on the ground
			// sundir=unit vector towards the sun
			// return scattered light and extinction coefficient

			float3 result = float3(0, 0, 0);
			extinction = float3(1, 1, 1);

			float3 viewdir = _point - camera;
			float d = length(viewdir);
			viewdir = viewdir / d;
			float r = length(camera);

			if (r < 0.9 * Rg)
			{
				camera.y += Rg;
				_point.y += Rg;
				r = length(camera);
			}
			float rMu = dot(camera, viewdir);
			float mu = rMu / r;
			float r0 = r;
			float mu0 = mu;
			_point -= viewdir * clamp(shaftWidth, 0.0, d);

			float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);
			float din = max(-rMu - deltaSq, 0.0);

			if (din > 0.0 && din < d)
			{
				camera += din * viewdir;
				rMu += din;
				mu = rMu / Rt;
				r = Rt;
				d -= din;
			}

			if (r <= Rt)
			{
				float nu = dot(viewdir, SUN_DIR);
				float muS = dot(camera, SUN_DIR) / r;

				float4 inScatter;

				if (r < Rg + 600.0)
				{
					// avoids imprecision problems in aerial perspective near ground
					float f = (Rg + 600.0) / r;
					r = r * f;
					rMu = rMu * f;
					_point = _point * f;
				}

				float r1 = length(_point);
				float rMu1 = dot(_point, viewdir);
				float mu1 = rMu1 / r1;
				float muS1 = dot(_point, SUN_DIR) / r1;

				if (mu > 0.0)
					extinction = min(Transmittance(r, mu) / Transmittance(r1, mu1), 1.0);
				else
					extinction = min(Transmittance(r1, -mu1) / Transmittance(r, -mu), 1.0);

				const float EPS = 0.004;
				float lim = -sqrt(1.0 - (Rg / r) * (Rg / r));

				if (abs(mu - lim) < EPS)
				{
					float a = ((mu - lim) + EPS) / (2.0 * EPS);

					mu = lim - EPS;
					r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
					mu1 = (r * mu + d) / r1;

					float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
					float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
					float4 inScatterA = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);

					mu = lim + EPS;
					r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
					mu1 = (r * mu + d) / r1;

					inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
					inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
					float4 inScatterB = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);

					inScatter = lerp(inScatterA, inScatterB, a);
				}
				else
				{
					float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
					float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
					inScatter = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
				}

				// avoids imprecision problems in Mie scattering when sun is below horizon
				inScatter.w *= smoothstep(0.00, 0.02, muS);

				float3 inScatterM = GetMie(inScatter);
				float phase = PhaseFunctionR(nu);
				float phaseM = PhaseFunctionM(nu);
				result = inScatter.rgb * phase + inScatterM * phaseM;
			}

			return result * SUN_INTENSITY;

		}



						
			struct v2f 
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv_depth : TEXCOORD1;
				float4 interpolatedRay : TEXCOORD2;
			};
			
			v2f vert( appdata_img v )
			{
				v2f o;
				half index = v.vertex.z;
				v.vertex.z = 0.1;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				o.uv_depth = v.texcoord.xy;
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1-o.uv.y;
				#endif				
				
				o.interpolatedRay = _FrustumCorners[(int)index];
				o.interpolatedRay.w = index;
				
				return o;
			}
			
			half4 frag(v2f i) : COLOR 
			{
				float4 col = tex2D(_MainTex, i.uv);

				float depthSample = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv_depth));

				float depth = Linear01Depth(depthSample);
				float depthDis = LinearEyeDepth(depthSample);

				float3 worldPos = (_WorldSpaceCameraPos + depth * i.interpolatedRay);
				
				//If the depth buffer has not been written into (ie depth is 1) this must be the sky.
				//This acts as a mask so we can tell what areas is sky and what is not.
				//The sky already has scattering applied to it so needs to be skipped.
				//This is not the best method but its easy.
				if (depth == 1.0) return col;


				float3 dir = normalize(worldPos-_WorldSpaceCameraPos);
			    
				float3 extinction = float3(0,0,0);

//				_Scale *= lerp(0, 2, depth);
//				smoothstep(500, 2000, x)

				float fogFactor = smoothstep(_Start, _End, depthDis);

				float3 inscatter = InScattering(_WorldSpaceCameraPos*_Scale, worldPos*_Scale, extinction, 1.0, _WorldSpaceLightPos0.xyz, _LightColor0.xyz*100);
				

//				float3 scatter = col.rgb * extinction + inscatter;
				float3 scatter = col.rgb * extinction + inscatter;
//				scatter = hdr(scatter);

				scatter = lerp(col.rgb, scatter, fogFactor);
				
				return float4(scatter, 1.0);
//				return float4(hdr(extinction), 1.0);
//				return float4(hdr(scatter), 1.0);
			    
			}
			ENDCG
	    }
	}
}
