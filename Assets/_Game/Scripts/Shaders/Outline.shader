Shader "Gamba/Outline"
{
    Properties
    {
        [Space(20)]
        _BackColor ("Back Color", Color) = (1, 1, 1, 1)
        _FrontColor ("Front Color", Color) = (0, 0, 0, 1)

        [Space]
        _Thickness ("Thickness", Range(0, 0.5)) = 0.04
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
        }

        ZWrite Off

        Pass // Mask Pass
        {
            Stencil
            {
                Ref 2
                Pass Replace
            }
            
            ZTest Always
            ColorMask 0
        }

        Pass // Outline Pass - Back
        {
            Stencil
            {
                Ref 2
                Comp NotEqual
            }

            Blend One OneMinusSrcAlpha
            ZTest LEqual

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _MainTex_ST;
            
            float4 _BackColor;
            float _Thickness;

			// --------------------------------------------------------------------------------------------

            struct AppData
            {
                float4 objPos         : POSITION;
                float3 objNormal      : NORMAL;
                float3 referenceUV    : TEXCOORD6;
                float3 bakedObjNormal : TEXCOORD7;
            };

            struct VertData
            {
                float4 clipPos        : SV_POSITION;
            };

			// --------------------------------------------------------------------------------------------

            float3 getWorldScale()
            {
                float x = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));
                float y = length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y));
                float z = length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z));

                return float3(x, y, z);
            }

			// --------------------------------------------------------------------------------------------

            VertData vert(AppData data)
            {
                VertData output;

                float4 objPos = data.objPos;

                float cameraDistance = length(ObjSpaceViewDir(objPos));
                float viewportCompensation = pow(cameraDistance, 0.6);

                float3 extrusionDir = data.bakedObjNormal != data.referenceUV ? data.bakedObjNormal : data.objNormal;

                float3 scaleCompensation = 1 / getWorldScale();

                float3 outlineDisplacement = normalize(extrusionDir) * _Thickness * viewportCompensation * scaleCompensation;

                output.clipPos = UnityObjectToClipPos(objPos + outlineDisplacement);

                return output;
            }

            float4 frag(VertData data) : SV_Target
            {
                if (_BackColor.a == 0) discard;

                float4 col = _BackColor;
                col.rgb *= col.a;

                return col;
            }

            ENDCG
        }

        Pass // Outline Pass - Front
        {
            Stencil
            {
                Ref 2
                Comp NotEqual
                Fail Zero
            }

            Blend One OneMinusSrcAlpha
            ZTest Greater

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _MainTex_ST;
            
            float4 _FrontColor;
            float _Thickness;

			// --------------------------------------------------------------------------------------------

            struct AppData
            {
                float4 objPos         : POSITION;
                float3 objNormal      : NORMAL;
                float3 referenceUV    : TEXCOORD6;
                float3 bakedObjNormal : TEXCOORD7;
            };

            struct VertData
            {
                float4 clipPos        : SV_POSITION;
            };

			// --------------------------------------------------------------------------------------------

            float3 getWorldScale()
            {
                float x = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));
                float y = length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y));
                float z = length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z));

                return float3(x, y, z);
            }

			// --------------------------------------------------------------------------------------------

            VertData vert(AppData data)
            {
                VertData output;

                float4 objPos = data.objPos;

                float cameraDistance = length(ObjSpaceViewDir(objPos));
                float viewportCompensation = pow(cameraDistance, 0.6);

                float3 extrusionDir = data.bakedObjNormal != data.referenceUV ? data.bakedObjNormal : data.objNormal;

                float3 scaleCompensation = 1 / getWorldScale();

                float3 outlineDisplacement = normalize(extrusionDir) * _Thickness * viewportCompensation * scaleCompensation;

                output.clipPos = UnityObjectToClipPos(objPos + outlineDisplacement);

                return output;
            }

            float4 frag(VertData data) : SV_Target
            {
                if (_FrontColor.a == 0) discard;

                float4 col = _FrontColor;
                col.rgb *= col.a;

                return col;
            }

            ENDCG
        }
    }
}