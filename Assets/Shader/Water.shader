Shader "Unlit/Water"
{
	Properties
	{
		_ReflectTex ("Reflect Texture", 2D) = "white" {}
		_SkyBox ("Sky Box", Cube) = "blue"
		_RefractTex ("Refract Texture", 2D) = "white" {}
		_Smooth ("Specular Smooth", float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityPBSLighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float3 worldNormal : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
			};

			sampler2D _ReflectTex;
			samplerCUBE _SkyBox;
			sampler2D _RefractTex;
			float _Smooth;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.screenPos = ComputeScreenPos(o.vertex);
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
				return o;
			}

			inline float FastFresnel(float3 i, float3 h, float f0) 
			{
				float cosin = saturate(1 - dot(i, h));
				float i2 = cosin * cosin, i4 = i2 * i2;
				return f0 + (1 - f0) * (cosin * i4);
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = fixed4(1,1,1,1);
				float2 offsets = i.worldNormal.xz * i.viewDir.y;
				fixed4 reflectCol = tex2D(_ReflectTex, i.screenPos.xy / i.screenPos.w + offsets);
				float3 reflectUV = reflect(-i.viewDir, i.worldNormal);
				fixed4 sky = texCUBE(_SkyBox, reflectUV);
				reflectCol.xyz = lerp(sky.xyz, reflectCol.xyz, reflectCol.a);
				fixed4 refractCol = tex2D(_RefractTex, i.screenPos.xy / i.screenPos.w + offsets);

				//float fresnel = FastFresnel(i.viewDir, i.worldNormal, 0.02f);
				float fresnel = 0.55 + 0.5*(pow(clamp(1 - dot(i.viewDir, i.worldNormal), 0.0f, 1.0f), 2));
				col.xyz = lerp(refractCol.xyz, reflectCol.xyz, fresnel);

				float3 halfVec = normalize(_WorldSpaceLightPos0.xyz + i.viewDir);
				fixed3 specular = _LightColor0 * pow(DotClamped(halfVec, i.worldNormal), _Smooth*100);
				col.xyz = col.xyz + specular;
				return col;
			}
			ENDCG
		}

	}
}
