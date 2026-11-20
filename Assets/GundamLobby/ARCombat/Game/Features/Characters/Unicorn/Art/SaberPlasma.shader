Shader "Gundam/Unicorn Saber Plasma"
{
    Properties
    {
        [HDR] _Tint("Plasma color", Color) = (2.8, 0.025, 0.075, 1)
        _Opacity("Intensity", Range(0, 1)) = 0.64
        _EdgeWeight("Rim strength", Range(0, 1)) = 0.6
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _Tint;
            half _Opacity;
            half _EdgeWeight;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half facing = saturate(abs(dot(normalize(input.normalWS), GetWorldSpaceNormalizeViewDir(input.positionWS))));
                half rim = (1.0h - facing) * (1.0h - facing);
                float wave = sin(dot(input.positionOS, float3(13.0, 19.0, 37.0)) - _Time.y * 28.0);
                half modulation = 0.76h + 0.24h * (0.5h + 0.5h * wave);
                half intensity = lerp(1.0h, 0.21h + 0.79h * rim, _EdgeWeight) * modulation * _Opacity;
                return half4(_Tint.rgb * intensity, 0.0h);
            }
            ENDHLSL
        }
    }
}
