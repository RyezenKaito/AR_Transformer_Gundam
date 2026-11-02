Shader "OrbitalFrontier/Planet Layers"
{
 Properties { _BaseMap("Cloud mask",2D)="white"{} _BaseColor("Color",Color)=(.2,.5,1,.3) _Atmosphere("Atmosphere mode",Float)=0 }
 SubShader
 {
  Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
  Pass
  {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseColor;float4 _BaseMap_ST;float _Atmosphere;
   CBUFFER_END
   struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
   struct V{float4 positionCS:SV_POSITION;float3 n:TEXCOORD0;float2 uv:TEXCOORD1;float3 p:TEXCOORD2;};
   V vert(A a){V o;o.p=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.p);o.n=TransformObjectToWorldNormal(a.normalOS);o.uv=a.uv;return o;}
   float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
   float noise(float3 p){float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(a),hash(a+float3(1,0,0)),f.x),lerp(hash(a+float3(0,1,0)),hash(a+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(a+float3(0,0,1)),hash(a+float3(1,0,1)),f.x),lerp(hash(a+float3(0,1,1)),hash(a+1),f.x),f.y),f.z);}
   half4 frag(V i):SV_Target
   {
    float3 n=normalize(i.n),v=normalize(_WorldSpaceCameraPos-i.p);Light sun=GetMainLight();float day=smoothstep(-.18,.25,dot(n,sun.direction));float edge=1-saturate(dot(n,v));
    if(_Atmosphere>.5)return half4(_BaseColor.rgb, pow(edge,5)*_BaseColor.a*day);
    float3 p=normalize(TransformWorldToObject(i.p));p+=.18*sin(p.zxy*7+2);float cloud=noise(p*8)*.5+noise(p*23)*.3+noise(p*71)*.14+noise(p*190)*.06;float a=smoothstep(.48,.67,cloud);return half4(_BaseColor.rgb*(.02+day*(.3+.7*saturate(dot(n,sun.direction)))),a*_BaseColor.a);
   }
   ENDHLSL
  }
 }
}
