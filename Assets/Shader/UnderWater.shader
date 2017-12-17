// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/UnderWater"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_WaterAltitude ("Water Altitude", float) = -0.1
		_WaterAttenuation ("Water Attenuation", Vector) = (1,2,3,4)
		_WaterRadiance ("Water Radiance", Vector) = (1,2,3,4)
		_WaterKD ("Water KD", Vector) = (1,2,3,4)
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
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 outScatter : TEXCOORD1;
				float3 inScatter : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _WaterAltitude;
			float4 _WaterAttenuation;
			float4 _WaterRadiance;
			float4 _WaterKD;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 viewDir = WorldSpaceViewDir(v.vertex);
				float l = length(viewDir);
				float d = _WorldSpaceCameraPos.y - worldPos.y;
				float h = max(0, _WaterAltitude - worldPos.y);
				float length = l * h / d;
				o.outScatter = exp(-_WaterAttenuation*length);
				o.inScatter = _WaterRadiance*(1 - o.outScatter*exp(-h*_WaterKD));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed3 diffuse = col*_LightColor0.rgb*DotClamped(_WorldSpaceLightPos0.xyz, i.worldNormal);
				diffuse.xyz = diffuse.xyz*i.outScatter + i.inScatter;
				return fixed4(diffuse, col.a);
			}
			ENDCG
		}
	}
}
