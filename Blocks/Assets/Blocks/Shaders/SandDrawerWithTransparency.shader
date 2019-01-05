// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/SandDrawerWithTransparency" {
	Properties
	{
		_MainTex("Color (RGB) Alpha (A)", 2D) = "white"
	}
		SubShader{

		 Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		Pass{
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
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
		float2 lightLevel : TEXCOORD1;
	};


	StructuredBuffer<float4> cubeOffsets;

	StructuredBuffer<float4> uvOffsets;
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	float4 _MainTex_ST;
	struct DrawingData {
		int4 data1;
		int4 data2;
	};

	StructuredBuffer<DrawingData> DrawingThings;
	//StructuredBuffer<float4> PixelData;
	float4x4 localToWorld;	
	float globalLightLevel;

	v2f vertex_shader(uint ida : SV_VertexID)
	{
		uint id = ida / 36;
		uint idQ = ida % 36;
		int idI = abs(DrawingThings[id].data1.w);
		//uint idI = 1;
		int3 idPt = DrawingThings[id].data1.xyz;
		int animFrame = DrawingThings[id].data2.w;
		animFrame = 0;
		//float4 col = PixelData[idI];
		//float4 col = float4(0.5, 0.5, 0.9, 1);
		float3 offset = cubeOffsets[idQ].xyz;
		float2 uvOffset = uvOffsets[idQ].xy;
		float lightLevel = DrawingThings[id].data2.z / 255.0;
		lightLevel = 1.0f;
		//lightLevel = globalLightLevel;
		// from https://answers.unity.com/questions/678193/is-it-possible-to-access-the-dimensions-of-a-textu.html
		//_MainTex_TexelSize.z //contains width
		//_MainTex_TexelSize.w //contains height
		int numBlocks = 64;
		uvOffset.x += animFrame;
		uvOffset.x /= 32.0f;
		uvOffset.y /= 3.0f;
		uvOffset.y += (idI - 1) / (float)numBlocks;
		//float3 pos = (idPt + offset*0.98 + 0.01) * ptCloudScale;
		float3 pos = (idPt + offset) * ptCloudScale;
		v2f o;
		//o.col = col;
		// I made this 2d so v2f would be a total of 8 values in size (7 is poorly aligned so it might hurt performance? idk)
		o.lightLevel = float2(lightLevel, lightLevel);
		o.pos = UnityObjectToClipPos(mul(localToWorld, float4(pos + ptCloudOffset.xyz, 1)));
		o.uv = uvOffset;
		return o;
	}


	float4 fragment_shader(v2f i) : SV_Target
	{
		float lightLevel = i.lightLevel.x;
		 float4 tex = tex2D(_MainTex, i.uv);
		 float4 eyeScalings = float4(1.0 - 0.299, 1.0 - 0.587, 1.0 - 0.144, 1.0);
		 tex = lerp(tex * eyeScalings*lightLevel, tex*lightLevel, lightLevel);
		 return tex;
	}


		ENDCG
	}
	}
}