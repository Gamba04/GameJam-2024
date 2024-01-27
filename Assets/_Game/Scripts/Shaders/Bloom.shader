Shader "Gamba/Bloom"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

			// --------------------------------------------------------------------------------------------

            struct AppData
            {
                float4 objPos      : POSITION;
                float2 uv          : TEXCOORD0;
                float3 objNormal   : NORMAL;
            };

            struct VertData
            {
                float4 clipPos     : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir     : TEXCOORD2;
            };

			// --------------------------------------------------------------------------------------------

            VertData vert(AppData data)
            {
                VertData output;

                output.clipPos = UnityObjectToClipPos(data.objPos);
                output.uv = TRANSFORM_TEX(data.uv, _MainTex);
                output.worldNormal = UnityObjectToWorldNormal(data.objNormal);
                output.viewDir = ObjSpaceViewDir(data.objPos);

                return output;
            }

            float4 frag(VertData data) : SV_Target
            {
                float4 col = tex2D(_MainTex, data.uv);

                return col - 1;
            }

            ENDCG
        }
    }
}