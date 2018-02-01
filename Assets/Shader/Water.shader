Shader "Custom/Water"
{
	Properties
	{
		_SkyBox ("Sky Box", Cube) = "blue"
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
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard

		// Use shader model 4.0 target, to get nicer looking lighting
		#pragma target 4.0
		#pragma multi_compile REFLECTION NOREFLECTION
		#pragma multi_compile REFRACTION NOREFRACTION
		
		struct Input
		{
			float3 viewDir;
			float3 worldNormal;
			float4 screenPos;
		};

		samplerCUBE _SkyBox;
#ifdef REFLECTION
		sampler2D _ReflectTex;
#endif
#ifdef REFRACTION
		sampler2D _RefractTex;
#endif

		half _Glossiness;
		half _Metallic;
		
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
			fixed4 reflectCol = tex2D(_ReflectTex, IN.screenPos.xy / IN.screenPos.w + offsets);
			reflectCol.xyz = lerp(sky.xyz, reflectCol.xyz, reflectCol.a);
	
#else
			fixed4 reflectCol = sky;
#endif

#ifdef REFRACTION
			fixed4 refractCol = tex2D(_RefractTex, IN.screenPos.xy / IN.screenPos.w + offsets);

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

