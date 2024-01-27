Shader "Gamba/Sprite"
{
	Properties
	{
		[HideInInspector] _MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			Tags
			{
				"Queue" = "Transparent"
			}

			Cull Off
			ZWrite Off
			Blend One OneMinusSrcAlpha
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			// --------------------------------------------------------------------------------------------

			struct AppData
			{
				float4 objPos  : POSITION;
				float2 uv	   : TEXCOORD0;
				float4 color   : COLOR;
			};

			struct VertData
			{
				float4 clipPos : SV_POSITION;
				float2 uv	   : TEXCOORD0;
				float4 color   : COLOR;
			};

			// --------------------------------------------------------------------------------------------

			VertData vert(AppData data)
			{
				VertData output;

				output.clipPos = UnityObjectToClipPos(data.objPos);
				output.uv = TRANSFORM_TEX(data.uv, _MainTex);
				output.color = data.color;

				return output;
			}

			float4 frag(VertData data) : SV_Target
			{
				float4 col = tex2D(_MainTex, data.uv) * data.color;
				col.rgb *= col.a;

				return col;
			}

			ENDCG
		}
	}
}