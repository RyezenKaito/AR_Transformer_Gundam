Shader "OrbitalFrontier/Scenic Wake"
{
 Properties{_BaseColor("Emission",Color)=(.25,.6,1,1)}
 SubShader
 {
  Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
  Pass
  {
   Tags{"LightMode"="UniversalForward"}
   Blend SrcAlpha One
   Cull Off
   ZWrite Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseColor;
   CBUFFER_END
   struct A{float4 positionOS:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   struct V{float4 positionCS:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.color=a.color*_BaseColor;o.uv=a.uv;return o;}
   half4 frag(V i):SV_Target{float edge=pow(saturate(1-abs(i.uv.y*2-1)),1.5);return half4(i.color.rgb*2.1,i.color.a*edge);}
   ENDHLSL
  }
 }
}
