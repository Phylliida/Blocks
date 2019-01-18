// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Blocks/SandDrawerSimple" {
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
		float lightLevel : TEXCOORD1;
		//float fog : TEXCOORD1;
	};

	StructuredBuffer<float4> cubeOffsets;
	StructuredBuffer<float4> cubeNormals;

	StructuredBuffer<float4> uvOffsets;
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	float4 _MainTex_ST;


	struct DrawingData {
		int4 data;
		float4 vertexPos[3];
		float2 texOffset[3];
	};

	StructuredBuffer<DrawingData> DrawingThings;
	float globalLightLevel;

	float4x4 localToWorld;
	v2f vertex_shader(uint ida : SV_VertexID)
	{
		uint i1 = ida / 3;
		uint i2 = ida % 3;
		int lightLevelI = DrawingThings[i1].data.z;
		float skyLightLevel = ((lightLevelI & 0xF0) >> 4) / 15.0f*globalLightLevel;
		float blockLightLevel = (lightLevelI & 0xF)/15.0f;
		float lightLevel = max(skyLightLevel, blockLightLevel);
		//float lightLevel = 1.0;
		v2f o;
		o.pos = UnityObjectToClipPos(DrawingThings[i1].vertexPos[i2]);
		o.uv = DrawingThings[i1].texOffset[i2];
		//o.uv = DrawingThings[ida].texOffset;
		o.lightLevel = lightLevel;
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