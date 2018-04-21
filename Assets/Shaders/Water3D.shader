Shader "3D/Water"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SpeedBoatWind("SpeedBoatWind", Vector) = (1, 0, 1, 0)
		_UVTiling("UVTiling", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _SpeedBoatWind;
			float _UVTiling;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float2 speedBoat = max(0.001, _SpeedBoatWind.xy);
				float2 speedWind = max(0.001, _SpeedBoatWind.zw);
				fixed4 col = tex2D(_MainTex, _UVTiling*(i.uv+(_Time.xx*speedBoat.xy*speedWind.xy)));
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
