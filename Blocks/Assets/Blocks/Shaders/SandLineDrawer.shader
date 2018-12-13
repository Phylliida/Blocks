// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/SandLineDrawer" {
	Properties
	{
		//_PtCloudTex("Texture", 3D) = "" {}
	}
		SubShader{

		Tags{ "RenderType" = "Opaque"}
		Pass{
		//Lighting Off
		//Cull Off
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
		float4 col : COLOR0;
	};

	StructuredBuffer<uint4> DrawingThings;
	StructuredBuffer<float4> lineOffsets;
	//sampler3D _PtCloudTex;
	float4x4 localToWorld;
	//StructuredBuffer<float4> PixelData;

#define totalSize 96

	v2f vertex_shader(uint ida : SV_VertexID)
	{
		uint id = ida / 24;
		uint idQ = (ida % 24);
		//uint a = id / ptCloudWidth;
		uint idI = DrawingThings[id].w;
		uint3 idPt = DrawingThings[id].xyz;
		//uint a = id / totalSize;
		//uint3 idPt = uint3(id % totalSize, a % totalSize, a / totalSize);
		//uint3 idPt = uint3(id % ptCloudWidth, a % ptCloudWidth, a / ptCloudWidth);
		//float4 cola = ;//(_PtCloudTex, float4(uvPt / 2, 0));
		//int isOn = sign();
		//float4 cola = PixelData[idI];
		float4 cola = float4(1.0, 1.0, 1.0, 1.0);
		//int isOff = sign(max(0, -sign(cola.w - 0.05)));
		float3 offset = lineOffsets[idQ].xyz;
		/*
		float3 pos = ((idPt + ) * ptCloudScale)*isOn;
		float4 col = isOn*float4(1, 1, 1, 1);
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, mul(localToWorld, float4(pos + ptCloudOffset.xyz, isOn)));
		o.col = col;
		return o;
		*/

		int isSource = sign(max(0, 0.12-abs(cola.w - 0.2)));
		float3 pos = (idPt + offset) * ptCloudScale;
		float4 col = isSource*float4(1, 1, 1, 1) + (1 - isSource)*float4(0, 0, 0, 1);
		v2f o;
		o.pos = UnityObjectToClipPos(mul(localToWorld, float4(pos + ptCloudOffset.xyz, 1)));
		o.col = col;
		return o;
	}


	float4 fragment_shader(v2f i) : SV_Target
	{
		//return tex3D(_PtCloudTex, i.uv);
		return i.col;
	}


		ENDCG
	}
	}
}