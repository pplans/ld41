Shader "3D/Water3DLit" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_DFFTex ("Albedo (RGB)", 2D) = "white" {}
		_NormTex("Normal", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_SpeedBoatWind("SpeedBoatWind", Vector) = (1, 0, 1, 0)
    _BoatPosition("BoatPosition", Vector) = (0, 0, 0, 0)
		_UVTiling("UVTiling", Float) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _DFFTex;
		sampler2D _NormTex;

		struct Input {
			float2 uv_DFFTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float4 _SpeedBoatWind;
    float4 _BoatPosition;
		float _UVTiling;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			float2 speedBoat = max(float2(0.001, 0.001), _SpeedBoatWind.xy);
			float2 speedWind = max(float2(0.001, 0.001), _SpeedBoatWind.zw);
			float2 uv = _BoatPosition.xy + _UVTiling * (IN.uv_DFFTex + (_Time.xx*speedBoat.xy*speedWind.xy));
			fixed4 c = tex2D(_DFFTex, uv) * _Color;
			o.Albedo = c.rgb;
			o.Normal = tex2D(_NormTex, uv);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
