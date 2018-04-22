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
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#include "UnityStandardBRDF.cginc"


		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _DFFTex;
		sampler2D _NormTex;

		struct Input {
			float2 uv_DFFTex;
			float3	N;
			float3 viewDir;
			float3 worldPos;
			float debug;
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

		void vert(inout appdata_full v, out Input data)
		{
			UNITY_INITIALIZE_OUTPUT(Input, data);
			if (dot(v.normal, float3(0.0, 1.0, 0.0)) < 1.0f)
			{
				data.uv_DFFTex = float2(0.0, 0.0);
			}
			data.N = v.normal;
			data.debug = v.normal.y>0.2f;
			data.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			data.viewDir = normalize(v.vertex.xyz);
		}
		
		float3 BoxProjection(
			float3 direction, float3 position,
			float4 cubemapPosition, float3 boxMin, float3 boxMax
		) {
			if (cubemapPosition.w > 0) {
				float3 factors =
					((direction > 0 ? boxMax : boxMin) - position) / direction;
				float scalar = min(min(factors.x, factors.y), factors.z);
				direction = direction * scalar + (position - cubemapPosition.xyz);
			}
			return direction;
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = _Color;
			float3 normal = IN.N;
			//if (dot(IN.N, float3(0.0, 1.0, 0.0)) > 0.2)
			{
				float2 speedBoat = max(float2(0.001, 0.001), _SpeedBoatWind.xy);
				float2 speedWind = max(float2(0.001, 0.001), _SpeedBoatWind.zw);
				float2 uv = _BoatPosition.xy + _UVTiling * (IN.uv_DFFTex + (_Time.xx*speedBoat.xy*speedWind.xy));
				c = tex2D(_DFFTex, uv) * _Color;
				normal = UnpackNormal(tex2D(_NormTex, uv));
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
			}

			/*Unity_GlossyEnvironmentData envData;
			envData.roughness = 1 - 0.0f;
			envData.reflUVW = BoxProjection(
				reflect(IN.viewDir , normal), IN.worldPos,
				unity_SpecCube0_ProbePosition,
				unity_SpecCube0_BoxMin.xyz, unity_SpecCube0_BoxMax.xyz
			);
			sampleCUBE cube = UNITY_PASS_TEXCUBE(unity_SpecCube0);
			float4 probe0 = Unity_GlossyEnvironment(
				cube, unity_SpecCube0_HDR, envData
			);
			float3 probe0NoHDR = DecodeHDR(probe0, unity_SpecCube0_HDR);*/

			o.Albedo = c.rgb;
			o.Normal = normal;
			// Metallic and smoothness come from slider variables
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
