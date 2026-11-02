Shader "TurnBattle/SoftDust"
{
    Properties{_BaseMap("Mask",2D)="white"{} _BaseColor("Color",Color)=(1,1,1,1)}
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial) float4 _BaseColor; CBUFFER_END
            struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.c=i.c;return o;}
            half4 frag(V i):SV_Target{half4 mask=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);return half4(i.c.rgb*_BaseColor.rgb*(.75+mask.r*.25),mask.a*i.c.a*_BaseColor.a);}
            ENDHLSL
        }
    }
}
