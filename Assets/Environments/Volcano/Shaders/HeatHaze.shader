Shader "Volcano/URP Local Heat Haze"
{
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-10"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            struct V{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
            V Vert(float4 p:POSITION,float2 uv:TEXCOORD0){V o;o.pos=TransformObjectToHClip(p.xyz);o.uv=uv;return o;}
            half4 Frag(V i):SV_Target
            {
                float2 uv=GetNormalizedScreenSpaceUV(i.pos);float2 q=i.uv;float edge=pow(saturate(1-abs(q.x*2-1)),2)*saturate(sin(q.y*3.14159));
                float2 distortion=float2(sin(q.y*24-_Time.y*1.7+sin(q.x*13)),cos(q.x*17+q.y*9-_Time.y*1.1))*.0014*edge;
                return half4(SampleSceneColor(uv+distortion),edge*.22);
            }
            ENDHLSL
        }
    }
}
