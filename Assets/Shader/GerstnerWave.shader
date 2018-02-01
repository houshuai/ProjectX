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
		_Para ("Parameter", float) = 1
		_SkyBox ("Sky Box", Cube) = "blue" {}
		_ReflectTex ("Reflect Texture", 2D) = "white" {}
		_RefractTex ("Refract Texture", 2D) = "white" {}
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
		#pragma target 4.0           
		#pragma multi_compile REFLECTION _
		#pragma multi_compile REFRACTION _


		struct Input 
		{
			float3 viewDir;
			float3 worldNormal;
			float4 screenPos;
			float2 uv_MainTex;
		};
		
		float4 _A, _S, _Dx, _Dz, _L, _Q;
		float _Para;
		samplerCUBE _SkyBox;
#ifdef REFLECTION
		sampler2D _ReflectTex;
#endif
#ifdef REFRACTION
		sampler2D _RefractTex;
#endif
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
		
		inline float FastFresnel(float3 i, float3 h, float f0) 
		{
			float cosin = saturate(1 - dot(i, h));
			float i2 = cosin * cosin, i4 = i2 * i2;
			return f0 + (1 - f0) * (cosin * i4);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float3 skyUV = reflect(-IN.viewDir, IN.worldNormal);
			fixed4 sky = texCUBE(_SkyBox, skyUV);
			float2 offsets = IN.worldNormal.xz * IN.viewDir.y;
#ifdef REFLECTION
			fixed4 reflectCol = tex2D(_ReflectTex, IN.screenPos.xy / IN.screenPos.w + offsets * _Para);
			reflectCol.xyz = lerp(sky.xyz, reflectCol.xyz, reflectCol.a);
	
#else
			fixed4 reflectCol = sky;
#endif

#ifdef REFRACTION
			fixed4 refractCol = tex2D(_RefractTex, IN.screenPos.xy / IN.screenPos.w + offsets * _Para);

			//float fresnel = FastFresnel(i.viewDir, i.worldNormal, 0.02f);
			float fresnel = 0.55 + 0.5*(pow(clamp(1 - dot(IN.viewDir, IN.worldNormal), 0.0f, 1.0f), 2));

			fixed3 col = fixed3(1,1,1);
			col.xyz = lerp(refractCol.xyz, reflectCol.xyz, fresnel);
			o.Albedo = col;
#else
			o.Albedo = reflectCol;
#endif
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
