Shader "TurnBattle/EnergyRibbon"
{
    Properties
    {
        _BaseMap("Mask",2D)="white"{}
        [HDR]_BaseColor("Energy",Color)=(2,1.6,.2,1)
        _Opacity("Opacity",Range(0,1))=1
        _Dissolve("Dissolve",Range(0,1))=0
        _Phase("Manual time",Float)=0
        _Scroll("UV velocity",Vector)=(0,0,0,0)
    }
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST, _BaseColor, _Scroll;
                float _Opacity, _Dissolve, _Phase;
            CBUFFER_END
            struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            V vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv*_BaseMap_ST.xy+_BaseMap_ST.zw+_Scroll.xy*_Phase;o.color=i.color;return o;}
            half4 frag(V i):SV_Target
            {
                half4 mask=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);
                half a=mask.a*smoothstep(_Dissolve-.025,_Dissolve+.10,mask.r)*_Opacity*i.color.a*_BaseColor.a;
                return half4(_BaseColor.rgb*i.color.rgb,a);
            }
            ENDHLSL
        }
    }
}
