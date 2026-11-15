Shader "CG/S17_Mission2"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _BaseMap;

            void vert(float3 positionOS : POSITION,
                      float2 uv : TEXCOORD0,
                      out float2 uvOut : TEXCOORD0,
                      out float4 positionCS : SV_POSITION)
            {
                uvOut = uv;
                positionCS = TransformObjectToHClip(positionOS);
            }

            float4 frag(float2 uv : TEXCOORD0) : SV_Target
            {
                // 미션 2: 그림 전체를 표면의 왼쪽 아래 1/4에, 범위 밖은 회색
                float2 p = uv * 2;
                if (p.x > 1 || p.y > 1) return float4(0.5, 0.5, 0.5, 1);

                return tex2D(_BaseMap, p);
            }
            ENDHLSL
        }
    }
}