Shader "Unlit/HealthBoard"
{
	Properties
	{
		_Width ("Width", float) = 2
		_Height ("Height", float) = 0.4
		_Health ("Health", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			#include "UnityCG.cginc"


			struct g2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD1;
			};

			float _Width;
			float _Height;
			float _Health;
			
			float4 vert (float4 vertex : POSITION) : TEXCOORD0
			{
				return mul(unity_ObjectToWorld, vertex);
			}

			[maxvertexcount(6)]
			void geom(point float4 points[1] : TEXCOORD0, inout TriangleStream<g2f> triStream)
			{
				float4 center = points[0];
				float3 cam = _WorldSpaceCameraPos;
				float3 x =  float3(center.z - cam.z, 0, cam.x - center.x);
				x = normalize(x);

				float halfHeight = _Height * 0.5;

				float3 c1 = center - _Width * 0.5 * x;
				float3 c2 = center + _Width * 0.5 * x;

				g2f f[4];
				float4 q = float4(c1.x, c1.y - halfHeight, c1.z, 1);
				f[0].vertex = mul(UNITY_MATRIX_VP, q);
				f[0].uv = float2(0, 0);

				q = float4(c1.x, c1.y + halfHeight, c1.z, 1);
				f[1].vertex = mul(UNITY_MATRIX_VP, q);
				f[1].uv = float2(0, 1);

				q = float4(c2.x, c2.y + halfHeight, c2.z, 1);
				f[2].vertex = mul(UNITY_MATRIX_VP, q);
				f[2].uv = float2(1, 1);

				q = float4(c2.x, c2.y - halfHeight, c2.z, 1);
				f[3].vertex = mul(UNITY_MATRIX_VP, q);
				f[3].uv = float2(1, 0);

				triStream.Append(f[0]);
				triStream.Append(f[1]);
				triStream.Append(f[2]);
				triStream.Append(f[3]);
				triStream.Append(f[0]);
				triStream.Append(f[2]);
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				fixed4 col = fixed4(0, 1, 0, 1);
				if(i.uv.x > _Health){
					col.g = 0;
					col.r = 1;
				}
					
				return col;
			}
			ENDCG
		}
	}
}
