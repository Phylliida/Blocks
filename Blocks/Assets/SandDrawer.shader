// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/SandDrawer" {
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader{

		Tags { "RenderType" = "Opaque" }
		Pass{

		ZWrite On
		Cull Front
		CGPROGRAM
#include "UnityCG.cginc"
#pragma target 5.0  
#pragma vertex vertex_shader 
#pragma fragment fragment_shader

		int ptCloudWidth;
	float ptCloudScale;
	float4 ptCloudOffset;

	struct v2f {
		float4 pos : SV_POSITION;
		//float4 col : COLOR0;
		float2 uv : TEXCOORD0;
	};


	StructuredBuffer<float4> cubeOffsets;

	StructuredBuffer<float4> uvOffsets;
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	float4 _MainTex_ST;
	StructuredBuffer<int4> DrawingThings;
	//StructuredBuffer<float4> PixelData;
	float4x4 localToWorld;
	v2f vertex_shader(uint ida : SV_VertexID)
	{
		uint id = ida / 36;
		uint idQ = ida % 36;
		int idI = abs(DrawingThings[id].w);
		//uint idI = 1;
		int3 idPt = DrawingThings[id].xyz;
		//float4 col = PixelData[idI];
		//float4 col = float4(0.5, 0.5, 0.9, 1);
		float3 offset = cubeOffsets[idQ].xyz;
		float2 uvOffset = uvOffsets[idQ].xy;
		// from https://answers.unity.com/questions/678193/is-it-possible-to-access-the-dimensions-of-a-textu.html
		//_MainTex_TexelSize.z //contains width
		//_MainTex_TexelSize.w //contains height
		int numBlocks = 64;
		uvOffset.y /= 3.0f;
		uvOffset.y += (idI - 1) / (float)numBlocks;
		//float3 pos = (idPt + offset*0.98 + 0.01) * ptCloudScale;
		float3 pos = (idPt + offset) * ptCloudScale;
		v2f o;
		//o.col = col;
		o.pos = UnityObjectToClipPos(mul(localToWorld, float4(pos + ptCloudOffset.xyz, 1)));
		o.uv = uvOffset;
		return o;
	}


	float4 fragment_shader(v2f i) : SV_Target
	{
		fixed4 col = tex2D(_MainTex, i.uv);
		return col;
	}


		ENDCG
	}
	}
}