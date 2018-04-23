// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//This shader goes on the objects themselves. It just draws the object as white, and has the "Outline" tag.

Shader "Custom/ClippedSimple"
{
	Properties{
		_SeaWidth("SeaWidth", Range(0,50)) = 20.0
	}
	SubShader
	{
		ZWrite Off
		ZTest Always
		Lighting Off
		Pass
	{
		CGPROGRAM
#pragma vertex VShader
#pragma fragment FShader

		struct VertexToFragment
	{
		float4 pos:SV_POSITION;
		float4 worldPos:TEXCOORD0;
	};

	//just get the position correct
	VertexToFragment VShader(VertexToFragment i)
	{
		VertexToFragment o;
		o.pos = UnityObjectToClipPos(i.pos);
		o.worldPos = mul(unity_ObjectToWorld, i.pos);
		return o;
	}

	half _SeaWidth;

	//return white
	half4 FShader(in VertexToFragment i) :COLOR0
	{
		// Clip outside sea
		float2 corner = float2(_SeaWidth / 2, _SeaWidth / 2);
		clip(min(corner - i.worldPos.xz, corner + i.worldPos.xz));
		return half4(1,1,1,1);
	}

		ENDCG
	}
	}
}