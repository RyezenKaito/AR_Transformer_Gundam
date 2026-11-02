Shader "SunkenHarbor/Marine Sky"
{
 Properties { _SkyTint("Sky",Color)=(.35,.55,.73,1) }
 SubShader
 {
  Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline"}
  Cull Off ZWrite Off
  Pass
  {
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   struct A {float4 p:POSITION;};struct V {float4 p:SV_POSITION;float3 dir:TEXCOORD0;};
   V Vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.dir=a.p.xyz;return o;}
   float H(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float N(float2 p){float2 f=frac(p),q=floor(p);f=f*f*(3-2*f);return lerp(lerp(H(q),H(q+float2(1,0)),f.x),lerp(H(q+float2(0,1)),H(q+1),f.x),f.y);}
   half4 Frag(V i):SV_Target{float3 d=normalize(i.dir);float t=saturate(d.y);float3 c=lerp(float3(.72,.66,.54),float3(.26,.44,.62),pow(t,.5));float2 p=d.xz/max(.12,d.y+.12)*2.2+float2(_Time.y*.0005,0);float n=N(p)*.53+N(p*2.1)*.27+N(p*4.4)*.14+N(p*9)*.06;float cloud=smoothstep(.50,.73,n)*smoothstep(.01,.16,d.y);c=lerp(c,float3(.84,.84,.79),cloud*.78);Light sun=GetMainLight();float s=pow(saturate(dot(d,sun.direction)),7000);c+=sun.color*s*2;return half4(c,1);}
   ENDHLSL
  }
 }
}
