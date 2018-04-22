
Shader "Custom/Silhouetted Diffuse"
{
  	Properties {
		    _Color ("Main Color", Color) = (.5,.5,.5,1)
		    _OutlineColor ("Outline Color", Color) = (0,0,0,1)
		    _Outline ("Outline width", Range (0.0, 0.03)) = .005
  		  _MainTex ("Base (RGB)", 2D) = "white" { }
        _SeaWidth ("SeaWidth", Range(0,50)) = 10.0
  	}
 
    CGINCLUDE
        #include "UnityCG.cginc"

		    half _SeaWidth;

        struct appdata {
	        float4 vertex : POSITION;
	        float3 normal : NORMAL;
        };
 
        struct v2f {
	        float4 pos : POSITION;
	        float4 color : COLOR;
			float4 worldPos : TEXCOORD0;
			float4 posInner : TEXCOORD1;
        };
 
        uniform float _Outline;
        uniform float4 _OutlineColor;
    ENDCG

	  SubShader
	  {
		    Tags { "Queue" = "Transparent" }
			/*Pass{
				Name "BASE"
				Cull Back
				ZTest Always
				Blend Zero One

				// uncomment this to hide inner details:
				//Offset -8, -8

				SetTexture[_OutlineColor]
				{
					ConstantColor(0,0,0,0)
					Combine constant
				}
			}*/
 
		    // note that a vertex shader is specified here but its using the one above
		    Pass {
			      Name "OUTLINE"
			      Tags { "LightMode" = "Always" }
			      Cull Off
			      ZWrite Off
			      ZTest Always
			      ColorMask RGB // alpha not used

				  //Offset -8,-8
 
			      // you can choose what kind of blending mode you want for the outline
			      Blend SrcAlpha OneMinusSrcAlpha // Normal
			      //Blend One One // Additive
			      //Blend One OneMinusDstColor // Soft Additive
			      //Blend DstColor Zero // Multiplicative
			      //Blend DstColor SrcColor // 2x Multiplicative
 
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag

					v2f vert(appdata v)
					{
						// just make a copy of incoming vertex data but scaled according to normal direction
						v2f o;
						o.pos = UnityObjectToClipPos(v.vertex);

						float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
						float2 offset = TransformViewToProjection(norm.xy);

						float2 off = offset * o.pos.z * _Outline;

						o.posInner = float4(off.x, off.y, 0.0f, 0.0f);
						o.pos.xy += off;
						o.color = _OutlineColor;
						//o.color.a = abs(norm.x)>0.0f?1.0f:-1.0f;
						o.worldPos = mul(unity_ObjectToWorld, v.vertex);
						return o;
					}
 
					half4 frag(v2f i) :COLOR
					{
						// Clip outside sea
						float2 corner = float2(_SeaWidth / 2, _SeaWidth / 2);
						clip (min(corner - i.worldPos.xz, corner + i.worldPos.xz));

						return i.color * i.color.a;
					}
				ENDCG
		    }
 
		    Pass {
			      Name "DIFFUSE"
			      ZWrite On
			      ZTest LEqual
			      Blend SrcAlpha OneMinusSrcAlpha

				CGPROGRAM
					fixed4 _Color;

					#pragma vertex vert
					#pragma fragment frag

					v2f vert(appdata v)
					{
						// just make a copy of incoming vertex data but scaled according to normal direction
						v2f o;
						o.pos = UnityObjectToClipPos(v.vertex);

						float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
						float2 offset = TransformViewToProjection(norm.xy);

						//o.pos.xy += offset * o.pos.z * _Outline;
						o.color = _OutlineColor;
						o.worldPos = mul(unity_ObjectToWorld, v.vertex);
						return o;
					}
 
					half4 frag(v2f i) :COLOR
					{
						// Clip outside sea
							  float2 corner = float2(_SeaWidth / 2, _SeaWidth / 2);
							  clip (min(corner - i.worldPos.xz, corner + i.worldPos.xz));

						  return _Color;
					}
				ENDCG

			      /*Material {
				        Diffuse [_Color]
				        Ambient [_Color]
			      }
			      Lighting On
            //
			      SetTexture [_MainTex] {
				        ConstantColor [_Color]
				        Combine texture * constant
			      
            //
			      SetTexture [_MainTex] {
				        Combine previous * primary DOUBLE
			      }*/
		    }
	  }
 
  	Fallback "Diffuse"
}
