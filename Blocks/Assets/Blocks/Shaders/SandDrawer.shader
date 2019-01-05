// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Blocks/SandDrawer" {
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
			 //_FogColor("Fog Color (RGB)", Color) = (0.5, 0.5, 0.5, 1.0)
		//_FogStart("Fog Start", Float) = 0.0
		//_FogEnd("Fog End", Float) = 10.0
	}
		SubShader{

		//Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase"}
		Tags { "RenderType" = "Opaque"}
		Pass{
		//Lighting On
		ZWrite On
		Cull Front
		CGPROGRAM
#include "UnityCG.cginc"
		/*
			#include "UnityLightingCommon.cginc"
			#include "UnityShaderVariables.cginc"
			#include "AutoLight.cginc"
			#include "UnityDeferredLibrary.cginc"
			*/
#pragma target 5.0  
#pragma vertex vertex_shader 
#pragma fragment fragment_shader
		//#pragma multi_compile_fog

		int ptCloudWidth;
	float ptCloudScale;
	float4 ptCloudOffset;


	//unity defined variables
	//float4 _FogColor;
	//float _FogStart;
	//float _FogEnd;

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		//float4 normal : TEXCOORD1;
		float2 lightLevel : TEXCOORD1;
		//float fog : TEXCOORD1;
	};

	StructuredBuffer<float4> cubeOffsets;
	StructuredBuffer<float4> cubeNormals;

	StructuredBuffer<float4> uvOffsets;
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	float4 _MainTex_ST;


	struct DrawingData {
		int4 data1;
		int4 data2;
	};

	StructuredBuffer<DrawingData> DrawingThings;
	float globalLightLevel;

	//StructuredBuffer<float4> PixelData;
	float4x4 localToWorld;
	v2f vertex_shader(uint ida : SV_VertexID)
	{
		uint id = ida / 36;
		uint idQ = ida % 36;
		int idI = abs(DrawingThings[id].data1.w);
		//uint idI = 1;
		int3 idPt = DrawingThings[id].data1.xyz;
		int animFrame = DrawingThings[id].data2.w;
		float lightLevel = DrawingThings[id].data2.z / 255.0;

		animFrame = 0;
		lightLevel = 1.0f;
		//lightLevel = globalLightLevel;
		//float4 col = PixelData[idI];
		//float4 col = float4(0.5, 0.5, 0.9, 1);
		float3 offset = cubeOffsets[idQ].xyz;
		float2 uvOffset = uvOffsets[idQ].xy;
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
		float4 curPos = mul(localToWorld, float4(pos + ptCloudOffset.xyz, 1));
		o.pos = UnityObjectToClipPos(curPos);
		//float4 worldLightPos = mul(localToWorld, float4(unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x, 1.0));
		//float fogz = mul(UNITY_MATRIX_MV, curPos).z;
		//o.fog = clamp((fogz + _FogStart) / (_FogStart - _FogEnd), 0.0, 1.0);
		o.uv = uvOffset;
		//o.normal = cubeNormals[idQ];
		// I made this 2d so v2f would be a total of 8 values in size (7 is poorly aligned so it might hurt performance? idk)
		o.lightLevel = float2(lightLevel, lightLevel);
		//o.lightColor.rgba = float4(1.0, 1.0, 1.0, 1.0) * 1 / distance(worldLightPos.xyz, curPos.xyz);
		return o;
	}


	float4 fragment_shader(v2f i) : SV_Target
	{	
		float lightLevel = i.lightLevel.x;
		 float4 tex = tex2D(_MainTex, i.uv);
		 float4 eyeScalings = float4(1.0 - 0.299, 1.0-0.587, 1.0 - 0.144, 1.0);
		 tex = lerp(tex * eyeScalings*lightLevel, tex*lightLevel, lightLevel);
		 return tex;
		 //return float4(tex.xyz+i.lightColor.rgb, 1.0);
		 /*
		float3 normalDirection = i.normal.xyz;
		 float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.pos.xyz);
		 float3 lightDirection;
		 float atten;
		 //Texture Maps

		 if (_WorldSpaceLightPos0.w == 0.0) { //directional light
		   atten = 1.0;
		   lightDirection = normalize(_WorldSpaceLightPos0.xyz);
		 }
		 else {
		   float3 fragmentToLightSource = _WorldSpaceLightPos0.xyz - i.pos.xyz;
		   float distance = length(fragmentToLightSource);
		   atten = 1.0 / distance;
		   lightDirection = normalize(fragmentToLightSource);
		 }

		 //Lighting
		 float3 diffuseReflection = atten * _LightColor0.xyz * saturate(dot(normalDirection, lightDirection));
		 float3 specularReflection = diffuseReflection * _SpecColor.xyz * pow(saturate(dot(reflect(-lightDirection, normalDirection), viewDirection)) , _Shininess);

		 float3 lightFinal = UNITY_LIGHTMODEL_AMBIENT.xyz + diffuseReflection + specularReflection;// + rimLighting;


		 return float4(tex.xyz * lightFinal, 1.0);

		 */

		//fixed4 col = tex2D(_MainTex, i.uv);
		//return col;
		//return lerp(col, _FogColor, i.fog);
	}


		ENDCG
	}
	}
}