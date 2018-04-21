
Shader "2D/Water"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			
			#define WATER_HEIGHT_PERCENT 0.7
#define WATER_BELOW_HEIGHT_PERCENT 0.2
#define WATER_SURFACE_THICKNESS 0.01
#define WATER_AMP 0.025
#define WATER_FREQ 20.0
#define WATER_SPEED 45.0
#define WATER_REFR_STRENGTH 0.1
#define nTIME _Time.x
#define TIME speed*_Time.x

#define WATER_COLOR_HOT fixed3(0.4, 1.2, 2.0)
#define WATER_COLOR_ICE fixed3(4.0, 4.0, 4.0)

			fixed2 rot(fixed2 X, float a)
			{
				float s = sin(a); float c = cos(a);
				return mul(float2x2(c, -s, s, c), X);
			}

			float hash(fixed2 uv)
			{
				fixed2 suv = sin(uv);
				suv = rot(suv, uv.x);
				return frac(lerp(suv.x*13.13032942, suv.y*12.01293203924, dot(uv, suv)));
			}

			fixed2 hash2D(fixed2 uv)
			{
				return fixed2(
					sin(hash(uv.xy)*12.32042304),
					cos(hash(uv.yx)*12.302849234832)
				);
			}

			float voronoi(fixed2 uv) {
				fixed2 fl = floor(uv);
				fixed2 fr = frac(uv);
				float res = 1.0;
				for (int j = -1; j <= 1; j++) {
					for (int i = -1; i <= 1; i++) {
						fixed2 p = fixed2(i, j);
						float h = hash(fl + p);
						//h += 0.5-0.5*sin(iTime*0.8+h);
						fixed2 vp = p - fr + h;
						float d = dot(vp, vp);

						//res = min(res, d);
						res += 1.0 / pow(d, 8.0);
					}
				}
				//return sqrt(res);
				return pow(1.0 / res, 1.0 / 16.0);
			}

			float SchlickFresnel(float n1, float n2, float cosO)
			{
				float R0 = (n1 - n2) / (n1 + n2);
				float cosO5 = cosO*cosO*cosO*cosO*cosO;
				return R0 + (1.0 - R0)*cosO5;
			}

			float surface(float _x, float _a, float _f, fixed2 _t)
			{
				float F = _f;
				float A = _a;
				float iSine = 0.0;
				for (int i = 0; i < 6; i++)
				{
					float s = sin(_x*F + _t.x);
					iSine += 1.0*A*pow(abs(s), 1.00) + A*abs(s)
						// ebullition
						+ 4.0*A*cos(_x*F*0.5 + _t.y);
					F *= 1.9;
					A *= 0.8;
				}
				return max(0.0, 1.0 - iSine) - 1.0;
			}

			fixed3 mainWater(in fixed2 uv, float FTemp)
			{
				float amp = min(0.02, WATER_AMP*FTemp*0.5);
				float freq = WATER_FREQ*FTemp;
				float speed = WATER_SPEED*FTemp;

				// bubbles
				fixed2 vdir = fixed2(TIME*speed, 0.0);
				fixed2 vuv = uv*freq + (vdir + fixed2(sin(uv.x + nTIME), -TIME*speed))*FTemp;
				float v = voronoi(vuv);
				fixed2 nearestBP = fixed2(0.0, 0.0) + (max(0.0, (1.0 - pow(v + 0.9, 8.0))));

				float surf = surface(uv.x, amp, freq, fixed2(TIME, TIME*speed));
				float surfx = surface(uv.x + 0.0001, amp, freq, fixed2(TIME, TIME*speed));

				fixed2 N = normalize(fixed2(-(surfx - surf), 0.0001));

				float HPF = max(0.0, uv.y + surf - WATER_HEIGHT_PERCENT);
				HPF /= max(WATER_SURFACE_THICKNESS, HPF);
				HPF = min(1.0, HPF + max(0.0, uv.y) / WATER_HEIGHT_PERCENT*WATER_BELOW_HEIGHT_PERCENT);

				float height = lerp(0.0, 1.0, HPF);

				float ebullition = max(0.0, FTemp - 0.1);

				height = max(height, ebullition*clamp(nearestBP.y, 0.0, WATER_HEIGHT_PERCENT));

				fixed4 noise = tex2D(_MainTex, uv + TIME*0.01);
				fixed2 offset = noise.rg*(1.0 - HPF)*WATER_REFR_STRENGTH*ebullition;

				// colouring

				fixed3 bgcolor = fixed3(0.0, 0.0, 0.0);
				fixed3 wcolor = lerp(WATER_COLOR_ICE,
					WATER_COLOR_HOT,
					1.0 - (pow(1.0 - FTemp, 5.0)));


				// blending depth
				wcolor = lerp(fixed3(0.0, 0.0, 0.0), wcolor, height);

				wcolor = lerp(wcolor*0.6, wcolor, min(1.0, 2.0*(1.0 - ebullition)));

				return lerp(wcolor, bgcolor, height);
			}

			float Fresnel_Schlick(float n1, float n2, float NdotV)
			{
				float R0 = (n1 - n2) / (n1 + n2);
				R0 *= R0;
				return R0 + (1.0 - R0)*(1.0 - NdotV)*(1.0 - NdotV)*(1.0 - NdotV)*(1.0 - NdotV)*(1.0 - NdotV);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			float temp = 20.0;
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				float FTemp = min(1.0, max(0.0, temp+20.0)/100.0);
				fixed3 color = mainWater(i.uv, FTemp);

				col = fixed4(color / (1.0 + color), 1.0);

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
