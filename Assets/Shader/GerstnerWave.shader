Shader "Custom/GerstnerWave" 
{
	Properties 
	{
		_A ("Amplitude", Vector) = (1,2,3,4)
		_S ("Wave Speed", Vector) = (1,2,3,4)
		_Dx ("Direction x", Vector) = (1,2,3,4)
		_Dz ("Direction z", Vector) = (1,2,3,4)
		_L ("Wave Length", Vector) = (1,2,3,4)
		_Q ("Q", Vector) = (1,2,3,4)
		_SkyBox ("Sky Box", Cube) = "blue" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input 
		{
			float3 viewDir;
			float3 worldNormal;
		};
		
		float4 _A, _S, _Dx, _Dz, _L, _Q;
		samplerCUBE _SkyBox;
		half _Glossiness;
		half _Metallic;

		void vert (inout appdata_full v) 
		{
			float PI = 3.141592f;
			float G = 9.8;
			float4 w = 2 * PI * G / _L;
			float4 psi = _S * w;
			float4 phase = w * (_Dx * v.vertex.x + _Dz * v.vertex.z) + psi * _Time.x;
			float4 sinp, cosp;
			sincos(phase, sinp, cosp);
			_Q = min(1.0f / w / _A, _Q);
			v.vertex.x += dot(_Q * _A * _Dx, cosp);
			v.vertex.z += dot(_Q * _A * _Dz, cosp);
			v.vertex.y = dot(_A, sinp);

			/*phase = w * (_Dx * v.vertex.x + _Dz * v.vertex.z) + psi * _Time.x;
			sincos(phase, sinp, cosp);
			v.normal.x = -dot(w * _Dx * _A, cosp);
			v.normal.y = 1 - dot(_Q * w * _A, sinp);
			v.normal.z = -dot(w * _Dz * _A, cosp);*/
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float3 skyUV = reflect(-IN.viewDir, IN.worldNormal);
			o.Albedo = texCUBE(_SkyBox, skyUV);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
