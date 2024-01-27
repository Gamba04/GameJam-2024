Shader "Gamba/Image"
{
    Properties
    {
        [HideInInspector] [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask("Color Mask", Float) = 15
        [HideInInspector] [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Pass
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest[unity_GUIZTestMode]
            ColorMask[_ColorMask]

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TextureSampleAdd;
            float4 _ClipRect;

			// --------------------------------------------------------------------------------------------

            struct AppData
            {
                float4 objPos  : POSITION;
                float2 uv      : TEXCOORD0;
                float4 color   : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertData
            {
                float4 clipPos : SV_POSITION;
                float2 uv      : TEXCOORD0;
                float4 objPos  : TEXCOORD1;
                float4 color   : COLOR;

                UNITY_VERTEX_OUTPUT_STEREO
            };

			// --------------------------------------------------------------------------------------------

            VertData vert(AppData data)
            {
                VertData output;

                UNITY_SETUP_INSTANCE_ID(data);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.objPos = data.objPos;
                output.clipPos = UnityObjectToClipPos(output.objPos);
                output.uv = TRANSFORM_TEX(data.uv, _MainTex);
                output.color = data.color;

                return output;
            }

            float4 frag(VertData data) : SV_Target
            {
                float4 col = (tex2D(_MainTex, data.uv) + _TextureSampleAdd) * data.color;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(data.objPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }

            ENDCG

        }
    }
}