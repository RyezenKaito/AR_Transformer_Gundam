Shader "GundamLobby/AtmosphericShaft" {
Properties { _Tint("Tint",Color)=(0.4,0.6,0.8,0.025) }
SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
Pass { Blend SrcAlpha One ZWrite Off Cull Off
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
struct V {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
CBUFFER_START(UnityPerMaterial)
float4 _Tint;
CBUFFER_END
V vert(A input){V o;o.pos=TransformObjectToHClip(input.positionOS.xyz);o.uv=input.uv;return o;}
half4 frag(V i):SV_Target {float edge=pow(saturate(sin(i.uv.x*3.14159)),2.5);float lengthFade=smoothstep(0,.12,i.uv.y)*pow(saturate(1-i.uv.y),.65);return half4(_Tint.rgb,_Tint.a*edge*lengthFade);}
ENDHLSL
} } }