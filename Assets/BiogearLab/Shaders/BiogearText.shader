Shader "BiogearLab/Depth Tested Text"
{
 Properties { _MainTex("Font",2D)="white" {} }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest LEqual Cull Off
 Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
 struct A{float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};struct V{float4 position:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 V vert(A v){V o;o.position=TransformObjectToHClip(v.vertex.xyz);o.uv=v.uv;o.color=v.color;return o;}
 half4 frag(V i):SV_Target{half a=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a;clip(a-.05);return half4(i.color.rgb,i.color.a*a);}
 ENDHLSL
 } }
}
