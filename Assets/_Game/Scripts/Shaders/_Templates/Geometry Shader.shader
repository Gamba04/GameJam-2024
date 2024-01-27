Shader "Gamba/Geometry"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Geometry"
        }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _Color;

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

            struct GeomData
            {
                float4 clipPos     : SV_POSITION;
                float2 uv          : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 viewDir     : TEXCOORD3;
                float3 coord       : TEXCOORD4;
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

            [maxvertexcount(3)]
            void geom(triangle VertData vertices[3], inout TriangleStream<GeomData> stream)
            {
                GeomData output;
                VertData vert;

                vert = vertices[0];

                output.clipPos = vert.clipPos;
                output.uv = vert.uv;
                output.worldNormal = vert.worldNormal;
                output.viewDir = vert.viewDir;
                output.coord = float3(1, 0, 0);
                
                stream.Append(output);
                
                vert = vertices[1];

                output.clipPos = vert.clipPos;
                output.uv = vert.uv;
                output.worldNormal = vert.worldNormal;
                output.viewDir = vert.viewDir;
                output.coord = float3(0, 0, 1);
                
                stream.Append(output);
                
                vert = vertices[2];

                output.clipPos = vert.clipPos;
                output.uv = vert.uv;
                output.worldNormal = vert.worldNormal;
                output.viewDir = vert.viewDir;
                output.coord = float3(0, 1, 0);

                stream.Append(output);
            }

            float4 frag(GeomData data) : SV_Target
            {
                float4 col = tex2D(_MainTex, data.uv) * _Color;

                return col;
            }

            ENDCG
        }
    }
}