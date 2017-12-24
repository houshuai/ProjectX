Shader "Unlit/WaterNoRef"
{
	Properties
	{
		_SkyBox ("Sky Box", Cube) = "blue"
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
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 viewDir : TEXCOORD1;
			};

			samplerCUBE _SkyBox;
			float _Smooth;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 skyUV = reflect(-i.viewDir, i.worldNormal);
				fixed4 sky = texCUBE(_SkyBox, skyUV);

				float3 halfVec = normalize(_WorldSpaceLightPos0.xyz + i.viewDir);
				fixed3 specular = _LightColor0.rgb * pow(DotClamped(halfVec, i.worldNormal), _Smooth * 100);
				sky.rgb += specular;
				return sky;
			}
			ENDCG
		}
	}
}
