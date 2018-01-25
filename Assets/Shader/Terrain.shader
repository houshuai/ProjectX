// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/Terrain" {
	Properties {
		_FirstTex ("First", 2D) = "white" {}
		_FirstNormal ("First Normal", 2D) = "bump" {}
		_SecondTex ("Second", 2D) = "white" {}
		_SecondNormal ("Second Normal", 2D) = "bump" {}
		_ThirdTex ("Third", 2D) = "white" {}
		_ThirdNormal ("Third Normal", 2D) = "bump" {}
		_FourthTex ("Fourth", 2D) = "white" {}
		_FourthNormal ("Fourth Normal", 2D) = "bump" {}
		_Mask ("Mask", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}

	SubShader {
		Tags { "RenderType" = "Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 4.0 target, to get nicer looking lighting
		#pragma target 4.0

		sampler2D _FirstTex;
		sampler2D _FirstNormal;
		sampler2D _SecondTex;
		sampler2D _SecondNormal;
		sampler2D _ThirdTex;
		sampler2D _ThirdNormal;
		sampler2D _FourthTex;
		sampler2D _FourthNormal;
		sampler2D _Mask;

		struct Input {
			float2 uv_FirstTex;
			float2 uv_FirstNormal;
			float2 uv_SecondTex;
			float2 uv_SecondNormal;
			float2 uv_ThirdTex;
			float2 uv_ThirdNormal;
			float2 uv_FourthTex;
			float2 uv_FourthNormal;
			float2 uv_Mask;
		};

		half _Glossiness;
		half _Metallic;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c1 = tex2D(_FirstTex, IN.uv_FirstTex);
			fixed4 c2 = tex2D(_SecondTex, IN.uv_SecondTex);
			fixed4 c3 = tex2D(_ThirdTex, IN.uv_ThirdTex);
			fixed4 c4 = tex2D(_FourthTex, IN.uv_FourthTex);
			fixed4 n1 = tex2D(_FirstNormal, IN.uv_FirstNormal);
			fixed4 n2 = tex2D(_SecondNormal, IN.uv_SecondNormal);
			fixed4 n3 = tex2D(_ThirdNormal, IN.uv_ThirdNormal);
			fixed4 n4 = tex2D(_FourthNormal, IN.uv_FourthNormal);
			fixed4 mask = tex2D(_Mask, IN.uv_Mask);

			o.Albedo = c1.rgb * mask.r + c2.rgb * mask.g + c3.rgb * mask.b + c4.rgb * mask.a;
			o.Normal = UnpackNormal(n1 * mask.r + n2 * mask.g + n3 * mask.b + n4 * mask.a);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}
		ENDCG
	}

	SubShader{
		Tags{"RenderType" = "Opaque"}
		LOD 100

		CGPROGRAM
		#pragma surface surf Lambert
		#pragma target 4.0

		sampler2D _FirstTex;
		sampler2D _SecondTex;
		sampler2D _ThirdTex;
		sampler2D _FourthTex;
		sampler2D _Mask;

		struct Input{
			float2 uv_FirstTex;
			float2 uv_SecondTex;
			float2 uv_ThirdTex;
			float2 uv_FourthTex;
			float2 uv_Mask;
		};
		
		half _Glossiness;
		half _Metallic;

		void surf (Input IN, inout SurfaceOutput o){
			fixed4 c1 = tex2D(_FirstTex, IN.uv_FirstTex);
			fixed4 c2 = tex2D(_SecondTex, IN.uv_SecondTex);
			fixed4 c3 = tex2D(_ThirdTex, IN.uv_ThirdTex);
			fixed4 c4 = tex2D(_FourthTex, IN.uv_FourthTex);
			fixed4 mask = tex2D(_Mask, IN.uv_Mask);

			o.Albedo = c1.rgb * mask.r + c2.rgb * mask.g + c3.rgb * mask.b + c4.rgb * mask.a;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
