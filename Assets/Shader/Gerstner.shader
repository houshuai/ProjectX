// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Gerstner"
{
	Properties
	{
		_SkyBox ("Sky Box", Cube) = "blue" {}
		_Bump ("Bump", 2D) = "bump" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv_Bump : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				//float3 worldNormal : TEXCOORD2;
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
					{ 0.70, 1.20, 1.5, float2(-0.5, 0.2), 2 },
					{ 1.20, 0.20, 1.3, float2(-0.7, 0.7), 2 },
					{ 1.520, 0.1, 1.5, float2(0.5, 0.2), 5 },
					{ 0.90, 0.5, 1.60, float2(-0.8, -0.20), 1 },
					{ 1.00, 0.3, 1.32, float2(0.2, 0.10), 2 }
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
				o.uv_Bump = TRANSFORM_TEX(v.uv, _Bump);
				//o.worldNormal = normalize(mul(unity_ObjectToWorld, normal));
				return o;
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
					{ 0.70, 1.20, 1.5, float2(-0.5, 0.2), 2 },
					{ 1.20, 0.20, 1.3, float2(-0.7, 0.7), 2 },
					{ 1.520, 0.1, 1.5, float2(0.5, 0.2), 5 },
					{ 0.90, 0.5, 1.60, float2(-0.8, -0.20), 1 },
					{ 1.00, 0.3, 1.32, float2(0.2, 0.10), 2 }
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
				float3 bump = UnpackNormal(tex2D(_Bump, i.uv_Bump + _Time.yy * 0.05)) + 
							UnpackNormal(tex2D(_Bump, i.uv_Bump * 4 + _Time.yy * 0.1));
				worldNormal = normalize(mul(normalize(bump), t2w));

				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				float3 skyUV = reflect(-viewDir, worldNormal);
				fixed4 sky = texCUBE(_SkyBox, skyUV);
				return sky;
			}
			ENDCG
		}
	}
}
