// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ClippedStandard" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
    _FillColor ("FillColor", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
    _SeaWidth ("SeaWidth", Range(0,50)) = 10.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
      
		Pass {
			Fog { Mode Off }
			Cull Front
			Lighting Off
   
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
               
				half _SeaWidth;
				fixed4 _FillColor;
               
				struct appdata {
					float4 vertex : POSITION;
				};
               
				struct v2f {
					float4 vertex : POSITION;
					float4 worldPos : TEXCOORD0;
				};
               
				v2f vert (appdata v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
          o.worldPos = mul(unity_ObjectToWorld, v.vertex);
					return o;
				}
               
				fixed4 frag (v2f IN) : COLOR {
					// Clip outside sea
					float2 corner = float2(_SeaWidth / 2, _SeaWidth / 2);
					clip (min(corner - IN.worldPos.xz, corner + IN.worldPos.xz));

					return _FillColor;
				}
			ENDCG
   
		}

      
    // shadow caster rendering pass, implemented manually
    // using macros from UnityCG.cginc
    Pass
    {
        Tags {"LightMode"="ShadowCaster"}

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma multi_compile_shadowcaster
        #include "UnityCG.cginc"

        struct v2f { 
            V2F_SHADOW_CASTER;
            float4 worldPos : TEXCOORD0;
        };

        half _SeaWidth;

        v2f vert(appdata_base v)
        {
            v2f o;
            o.worldPos = mul(unity_ObjectToWorld, v.vertex);
            TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
            return o;
        }

        float4 frag(v2f i) : SV_Target
        {
              // Clip outside sea
			      float2 corner = float2(_SeaWidth / 2, _SeaWidth / 2);
			      clip (min(corner - i.worldPos.xz, corner + i.worldPos.xz));

            SHADOW_CASTER_FRAGMENT(i)
        }
        ENDCG
    }

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float3 vertex;
		};

		half _Glossiness;
		half _Metallic;
		half _SeaWidth;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Clip outside sea
			float2 corner = float2(_SeaWidth / 2, _SeaWidth / 2);
			clip (min(corner - IN.worldPos.xz, corner + IN.worldPos.xz));

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}

	FallBack "Diffuse"
}
