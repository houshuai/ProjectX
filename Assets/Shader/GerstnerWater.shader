Shader "Custom/GerstnerWater" 
{
	Properties 
	{
		_SkyBox ("Sky Box", Cube) = "blue" {}
		_Bump ("Bump", 2D) = "bump" {}
		_ReflectTex ("Reflect Texture", 2D) = "white" {}
		_RefractTex ("Refract Texture", 2D) = "white" {}
		_Para ("Parameter", float) = 1
		_Smoothness("Specular", Range(0, 1)) = 0.5
	}
	SubShader 
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
		LOD 200
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0           
			#pragma multi_compile REFLECTION _
			#pragma multi_compile REFRACTION _
			#include "UnityCG.cginc"
			#include "UnityPBSLighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				//float3 worldNormal : TEXCOORD1;
				float2 uv_Bump : TEXCOORD2;
				float3 screenPos : TEXCOORD3;
			};

			struct Wave 
			{
			  float freq;  // 2*PI / wavelength
			  float amp;   // amplitude
			  float phase; // speed * 2*PI / wavelength
			  float2 dir;
			  float q;
			};
			
			samplerCUBE _SkyBox;
			sampler2D _Bump;
			float4 _Bump_ST;
#ifdef REFLECTION
			sampler2D _ReflectTex;
#endif
#ifdef REFRACTION
			sampler2D _RefractTex;
#endif
			float _Para;
			float _Smoothness;

			
			inline float3 calcPos(Wave w, float c, float s)
			{
				float3 pos = float3(0, 0, 0);
				pos.x = w.q * w.amp * w.dir.x * c;
				pos.y = w.amp * s;
				pos.z = w.q * w.amp * w.dir.y * c;
				return  pos;
			}

			inline float3 calcNormal(Wave w, float2 c, float s)
			{
				float3 normal = float3(0, 0, 0);
				float wa = w.freq * w.amp;
				normal.x = w.dir.x * wa * c;
				normal.y = w.q * wa * s;
				normal.z = w.dir.y * wa * c;
				return normal;
			}
			
			v2f vert (appdata v)
			{
				Wave W[5] = 
				{
					{ 1.350, 0.10, 1.5, float2(-0.5, 0.2), 2 },
					{ 0.220, 0.05, 1.3, float2(-0.7, 0.7), 10 },
					{ 1.3020, 0.01, 1.5, float2(0.5, 0.2), 5 },
					{ 1.420, 0.05, 1.60, float2(0.8, -0.20), 1 },
					{ 1.220, 0.05, 1.32, float2(0.2, 0.10), 2 }
				};
				
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				//float3 normal = float3(0, 0, 0);
				for(int i=0; i<5; i++)
				{
					Wave w = W[i];
					w.q = min(w.q, 1.0/(w.freq * w.amp));
					float par = w.freq * dot(w.dir, worldPos.xz) + w.phase * _Time.y;
					float s = 0, c = 0;
					sincos(par, s, c);
					worldPos += calcPos(w, c, s);
					//normal += calcNormal(w, c, s);
				}

				//normal.x = -normal.x;
				//normal.y = 1 - normal.y;
				//normal.z = -normal.z;

				v2f o;
				o.worldPos = worldPos;
				o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
				//o.worldNormal = normalize(mul(unity_ObjectToWorld, normal));
				o.uv_Bump = TRANSFORM_TEX(v.uv, _Bump);
				o.screenPos = ComputeScreenPos(o.vertex).xyw;
				return o;
			}

			inline float FastFresnel(float3 i, float3 h, float f0) 
			{
				float cosin = saturate(1 - dot(i, h));
				float i2 = cosin * cosin, i4 = i2 * i2;
				return f0 + (1 - f0) * (cosin * i4);
			}

			inline float3 calcBinormal(Wave w, float2 c, float s)
			{
				float3 binormal = float3(0, 0, 0);
				float wa = w.freq * w.amp;
				binormal.x = w.q * w.dir.x * w.dir.x * wa * s;
				binormal.y = w.dir.x * wa * c;
				binormal.z = w.q * w.dir.x * w.dir.y * wa * s;
				return binormal;
			}

			inline float3 calcTangent(Wave w, float2 c, float s)
			{
				float3 tangent = float3(0, 0, 0);
				float wa = w.freq * w.amp;
				tangent.x = w.q * w.dir.x * w.dir.y * wa * s;
				tangent.y = w.dir.y * wa * c;
				tangent.z = w.q * w.dir.y * w.dir.y * wa * s;
				return tangent;
			}

			void calcBinormalAndTangent(float2 p, out float3 binormal, out float3 tangent)
			{
				Wave W[5] = 
				{
					{ 1.350, 0.10, 1.5, float2(-0.5, 0.2), 2 },
					{ 0.220, 0.05, 1.3, float2(-0.7, 0.7), 10 },
					{ 1.3020, 0.01, 1.5, float2(0.5, 0.2), 5 },
					{ 1.420, 0.05, 1.60, float2(0.8, -0.20), 1 },
					{ 1.220, 0.05, 1.32, float2(0.2, 0.10), 2 }		
				};
				binormal = float3(0, 0, 0);
				tangent = float3(0, 0, 0);
				for(int i=0; i<5; i++)
				{
					Wave w = W[i];
					w.q = min(w.q, 1.0/(w.freq * w.amp));
					float par = w.freq * dot(w.dir, p) + w.phase * _Time.y;
					float s = 0, c = 0;
					sincos(par, s, c);
					binormal += calcBinormal(w, c, s);
					tangent += calcTangent(w, c, s);
				}
				binormal.x = 1-binormal.x;
				binormal.z = -binormal.z;
				tangent.x = -tangent.x;
				tangent.z = 1 - tangent.z;

				binormal = normalize(binormal);
				tangent = normalize(tangent);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 binormal, tangent;
				calcBinormalAndTangent(i.worldPos.xz, binormal, tangent);
				float3 worldNormal = normalize(cross(tangent, binormal));
				float3x3 t2w = float3x3(tangent, binormal, worldNormal);
				float3 bump = UnpackNormal(tex2D(_Bump, i.uv_Bump + _Time.yy * 0.01)) + 
							UnpackNormal(tex2D(_Bump, i.uv_Bump * 4 + _Time.yy * 0.02));
				worldNormal = normalize(mul(normalize(bump), t2w));

				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				float3 skyUV = reflect(-viewDir, worldNormal);
				fixed4 sky = texCUBE(_SkyBox, skyUV);

				float2 offsets = worldNormal.xz * viewDir.y;
#ifdef REFLECTION
			fixed4 reflectCol = tex2D(_ReflectTex, i.screenPos.xy / i.screenPos.z + offsets * _Para);
			reflectCol.xyz = lerp(sky.xyz, reflectCol.xyz, reflectCol.a);
	
#else
			fixed4 reflectCol = sky;
#endif

#ifdef REFRACTION
			fixed4 refractCol = tex2D(_RefractTex, i.screenPos.xy / i.screenPos.z + offsets * _Para);

			//float fresnel = FastFresnel(i.viewDir, i.worldNormal, 0.02f);
			float fresnel = 0.55 + 0.5*(pow(clamp(1 - dot(viewDir, worldNormal), 0.0f, 1.0f), 2));

			fixed4 col = fixed4(1,1,1,1);
			col.xyz = lerp(refractCol.xyz, reflectCol.xyz, fresnel);
#else
			fixed4 col = reflectCol;
#endif

			float3 lightdir = _WorldSpaceLightPos0.xyz;
			float3 halfDir = normalize(lightdir + viewDir);
			fixed3 specular = _LightColor0.rgb * pow(DotClamped(halfDir, worldNormal), _Smoothness * 100);
			col.xyz += specular;
			return col;
			}
		
		ENDCG
		}
	}
	FallBack "Diffuse"
}
