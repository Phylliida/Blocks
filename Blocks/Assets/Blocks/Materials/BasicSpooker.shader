Shader "Unlit/BasicSpooker"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
		{
			ZWrite On
			Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float globalLightLevel;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
                return o;
            }

			float4 frag (v2f i) : SV_Target
            {
				float lightLevel = max(i.color.r, i.color.g);
				// sample the texture
                float4 tex = tex2D(_MainTex, i.uv);
				// make it look darker according to light level
				float4 eyeScalings = float4(1.0 - 0.299, 1.0 - 0.587, 1.0 - 0.144, 1.0);
				tex = lerp(tex * eyeScalings*lightLevel, tex*lightLevel, lightLevel);
				return tex;
            }
            ENDCG
        }
    }
}
