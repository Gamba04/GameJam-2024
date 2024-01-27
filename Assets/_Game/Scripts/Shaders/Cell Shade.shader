Shader "Gamba/Cell Shade"
{
    Properties
    {
        [Header(Main)]
        [Space]
        _MainTex ("Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        [Space]
        [Header(Normal)]
        [Space]
        _Normal ("Normal", 2D) = "white" {}
        _NormalAmount ("Normal Multiplier", Float) = 1

        //[Space]
        //[Header(Reflexions)]
        //[Space]
        //_Glossiness ("Smoothness", Range(0, 1)) = 0.5
        //_Metallic ("Metallic", Range(0, 1)) = 0.0

        [Space]
        [Header(Cell Shade)]
        [Space]
        _Amount ("Tones", Range(2, 32)) = 8
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Geometry"
        }

        CGPROGRAM

        #pragma surface surf CustomStandard fullforwardshadows

        sampler2D _MainTex;
        sampler2D _Normal;

        float _NormalAmount;

        float4 _Color;
        //float _Glossiness;
        //float _Metallic;

        float _Amount;

        // --------------------------------------------------------------------------------------------

        struct Input
        {
            float2 uv_MainTex;
        };

        // --------------------------------------------------------------------------------------------

        void surf (Input input, inout SurfaceOutput output)
        {
            float4 col = tex2D(_MainTex, input.uv_MainTex) * _Color;

            output.Albedo = col;
            //output.Smoothness = _Glossiness;
            //output.Metallic = _Metallic;
            output.Alpha = col.a;

            float4 normalMap = tex2D(_Normal, input.uv_MainTex);

            float3 normal = UnpackNormal(normalMap != 1 ? normalMap : 0.5);
            normal.xy *= normalMap != 1 ? _NormalAmount : 0;
            
            output.Normal = normal;
        }

        float4 LightingCustomStandard(SurfaceOutput surface, float3 viewDir, float atten)
        {
            float4 col;

            float diffuse = max(dot(surface.Normal, viewDir), 0);

            // Cell shade
            _Amount--;
            diffuse = ceil(diffuse * _Amount) / _Amount;

            col.rgb = surface.Albedo * _LightColor0 * diffuse * atten;
            col.a = surface.Alpha;

            return col;
        }

        ENDCG
    }

    FallBack "Diffuse"
}