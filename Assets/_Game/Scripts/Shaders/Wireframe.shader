Shader "Gamba/Wireframe"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}

        _Color ("Color", Color) = (1, 1, 1, 1)
        _Threshold ("Threshold", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
        }

        Cull Off

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Structures ------------------------------------
            struct AppData
            {
                float4 objectPos : POSITION;
                float2 uv : TEXCOORD0;
                float3 objectNormal : NORMAL;
            };

            struct VertData
            {
                float4 clipPos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            struct GeomData
            {
                float4 clipPos : SV_POSITION;
                float2 uv : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 coord : TEXCOORD3;
                float3 viewDir : TEXCOORD4;
            };
            
            // Variables ------------------------------------
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _Color;
            float _Threshold;

            // Functions ------------------------------------
            VertData vert(AppData data)
            {
                VertData output;

                output.clipPos = UnityObjectToClipPos(data.objectPos);
                output.uv = TRANSFORM_TEX(data.uv, _MainTex);
                output.worldNormal = UnityObjectToWorldNormal(data.objectNormal);
                output.viewDir = ObjSpaceViewDir(data.objectPos);

                return output;
            }

            [maxvertexcount(3)]
            void geom(triangle VertData vertices[3], inout TriangleStream<GeomData> stream)
            {
                GeomData output;

                output.clipPos = vertices[0].clipPos;
                output.uv = vertices[0].uv;
                output.worldNormal = vertices[0].worldNormal;
                output.coord = float3(1, 0, 0);
                output.viewDir = vertices[0].viewDir;
                
                stream.Append(output);
                
                output.clipPos = vertices[1].clipPos;
                output.uv = vertices[1].uv;
                output.worldNormal = vertices[1].worldNormal;
                output.coord = float3(0, 0, 1);
                output.viewDir = vertices[1].viewDir;
                
                stream.Append(output);
                
                output.clipPos = vertices[2].clipPos;
                output.uv = vertices[2].uv;
                output.worldNormal = vertices[2].worldNormal;
                output.coord = float3(0, 1, 0);
                output.viewDir = vertices[2].viewDir;

                stream.Append(output);
            }

            float3 frag(GeomData data) : SV_Target
            {
                float threshold = _Threshold * length(data.viewDir);
                if (data.coord.x > threshold && data.coord.y > threshold && data.coord.z > threshold) discard;

                float3 col = _Color;

                return col;
            }

            ENDCG
        }
    }
}