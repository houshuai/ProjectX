Shader "Unlit/Grass"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white"{}
		_AlphaTex("AlphaTex", 2D) = "white"{}
		_Height("Height", float) = 1
		_Width("Width", range(0, 0.2)) = 0.1
		_WindAmplitude("Wind Amplitude", float) = 2
		_WindFrequency("Wind Frequency", float) = 3
		_PlayerPos("Player Position", Vector) = (0, 0, 0, 0)
	}
	SubShader
	{
		LOD 100

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" "Queue" = "AlphaTest" }
			Cull Off
			AlphaToMask On

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2g
			{
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 vertex : POSITION;
			};

			struct g2f 
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _AlphaTex;
			float4 _AlphaTex_ST;
			float _Height;
			float _Width;
			float _WindAmplitude;
			float _WindFrequency;
			float3 _PlayerPos;
			
			v2g vert (appdata_base v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.normal = v.normal;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			[maxvertexcount(30)]
			void geom(point v2g points[1], inout TriangleStream<g2f> triStream) 
			{
				float4 root = points[0].vertex;

				float random = sin(UNITY_HALF_PI * (frac(root.x) + frac(root.z)));

				_Height = _Height * random;
				_Width = _Width * random;

				const int vertCount = 12;

				float currV = 0;
				float offsetV = 1.0f / ((vertCount / 2) - 1);

				float currHeight = 0;
				float x = _Width * random;
				float z = sqrt(_Width*_Width - x*x);

				float xe=0, ze=0;
				if (distance(root, _PlayerPos)<0.5)
				{
					/*xe = root.x - _PlayerPos.x;
					ze = root.z - _PlayerPos.z;
					xe = clamp(1.0f / xe, -0.4, 0.4);
					ze = clamp(1.0f / ze, -0.4, 0.4);*/
					if (root.x < _PlayerPos.x) {
						xe = -0.4;
					}
					else {
						xe = 0.4;
					}
					if (root.z < _PlayerPos.z) {
						ze = -0.4;
					}
					else {
						ze = 0.4;
					}
				}
				
				g2f v[vertCount];

				for (int i = 0; i < vertCount; i++)
				{
					g2f vi;
					vi.normal = float3(0, 0, 1);

					if (fmod(i, 2) == 0)
					{
						vi.vertex = float4(root.x - x, root.y + currHeight, root.z - z, 1);
						vi.vertex.z += sin(_Time.x * _WindFrequency) * currHeight * currHeight * _WindAmplitude;
						vi.vertex.x += xe*currHeight;
						vi.vertex.z += ze*currHeight;
						vi.uv = float2(0, currV);
					}
					else 
					{
						vi.vertex = float4(root.x + x, root.y + currHeight, root.z + z, 1);
						vi.vertex.z += sin(_Time.x * _WindFrequency) * currHeight * currHeight * _WindAmplitude;
						vi.vertex.x += xe*currHeight;
						vi.vertex.z += ze*currHeight;
						vi.uv = float2(1, currV);

						currV += offsetV;
						currHeight = _Height * currV;
					}


					vi.vertex = UnityObjectToClipPos(vi.vertex);
					v[i] = vi;
				}

				for (int j = 0; j < vertCount - 2; j++)
				{
					triStream.Append(v[j]);
					triStream.Append(v[j + 1]);
					triStream.Append(v[j + 2]);
				}
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				float3 albedo = tex2D(_MainTex, i.uv).rgb;
				float3 alpha = tex2D(_AlphaTex, i.uv);

				return float4(albedo, alpha.g);
			}
			ENDCG
		}

	}
}
