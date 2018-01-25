Shader "Custom/GerstnerWave" 
{
	Properties 
	{
		_A ("Amplitude", Vector) = (1,2,3,4)
		_S ("Wave Speed", Vector) = (1,2,3,4)
		_Dx ("Direction x", Vector) = (1,2,3,4)
		_Dz ("Direction z", Vector) = (1,2,3,4)
		_L ("Wave Length", Vector) = (1,2,3,4)
		_Q ("Steepness", Vector) = (0.1, 0.2, 0.3, 0.4)
		_Tess ("Tessellation", float) = 12
		//_SkyBox ("Sky Box", Cube) = "blue" {}
		_MainTex ("Tex", 2D) = "blue" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader 
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model
		#pragma surface surf Standard vertex:vert 

		// Use shader model 5.0 target, to get nicer looking lighting
		#pragma target 5.0           
		#include "Tessellation.cginc"


		struct Input 
		{
			//float3 viewDir;
			//float3 worldNormal;
			float2 uv_MainTex;
		};
		
		float4 _A, _S, _Dx, _Dz, _L, _Q;
		float _Tess;
		//samplerCUBE _SkyBox;
		sampler2D _MainTex;
		half _Glossiness;
		half _Metallic;

		void vert (inout appdata_full v) 
		{
			float PI = 3.141592f;
			float G = 9.8;      //gravity
			float4 w = 2 * PI * G / _L;
			float4 psi = _S * w;
			float x=0,y=0,z=0;
			//float4 q = float4(1.5f / w[0] / _A[0],1.5f / w[1] / _A[1],1.5f / w[2] / _A[2],1.5f / w[3] / _A[3]);
			for(int i = 0; i < 4; i++)
			{
				float2 D = float2(_Dx[i], _Dz[i]);
				D = normalize(D);
				float phase = w[i] * dot(D, v.vertex.xz) + psi[i] * _Time.x;
				float sinp, cosp;
				sincos(phase, sinp, cosp);
				//float qq = 1.0f / w[i] / _A[i];
				x += _Q[i] * _A[i] * D.x * cosp;
				z += _Q[i] * _A[i] * D.y * cosp;
				y += _A[i] * sinp;
			}
			
			v.vertex.x += x;
			v.vertex.z += z;
			v.vertex.y = y;

			x=0; y=0; z=0;
			for(i = 0; i < 4; i++)
			{
				float2 D = float2(_Dx[i], _Dz[i]);
				D = normalize(D);
				float phase = w[i] * dot(D, v.vertex.xz) + psi[i] * _Time.x;
				float sinp, cosp;
				sincos(phase, sinp, cosp);
				x -= w[i] * D.x * _A[i] * cosp;
				z -= w[i] * D.y * _A[i] * cosp;
				y -= _Q[i] * w[i] * _A[i] * sinp;
			}

			v.normal.x = x;
			v.normal.y = 1 + y;
			v.normal.z = z;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			//float3 skyUV = reflect(-IN.viewDir, IN.worldNormal);
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex); //texCUBE(_SkyBox, skyUV);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
