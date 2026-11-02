Shader "OrbitalFrontier/Planet Rings"
{
 Properties{_BaseColor("Dust color",Color)=(.6,.5,.35,.7)}
 SubShader
 {
  Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
  Pass
  {
   Tags{"LightMode"="UniversalForward"}
   Blend SrcAlpha OneMinusSrcAlpha
   Cull Off
   ZWrite Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseColor;
   CBUFFER_END
   struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float3 p:TEXCOORD1;};
   V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.uv=a.uv;o.p=a.positionOS.xyz;return o;}
   half4 frag(V i):SV_Target
   {Light sun=GetMainLight();float3 l=normalize(TransformWorldToObjectDir(sun.direction));float t=max(0,-dot(i.p,l));float shadow=length(i.p+l*t)<1?.12:1;float grain=.73+.15*sin(i.uv.x*350)+.1*sin(i.uv.x*1390);float gap=1-.92*(smoothstep(.59,.60,i.uv.x)-smoothstep(.65,.66,i.uv.x));float edge=smoothstep(0,.035,i.uv.x)*(1-smoothstep(.96,1,i.uv.x));return half4(_BaseColor.rgb*grain*(.2+.8*abs(l.y))*shadow,_BaseColor.a*grain*gap*edge);}
   ENDHLSL
  }
 }
}
