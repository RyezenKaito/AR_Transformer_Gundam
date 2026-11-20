Shader "Desert/Layered Sandstone"
{
 Properties { _BaseColor("Tint",Color)=(0.6,0.3,0.15,1) _BaseMap("Detail",2D)="white"{} _Mode("0 ground 1 rock",Float)=1 _OriginX("Layout origin X",Float)=4.85 }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseColor;float4 _BaseMap_ST;float _Mode,_OriginX;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;};
   struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;float fog:TEXCOORD2;};
   V Vert(A a){V o;o.world=TransformObjectToWorld(a.p.xyz);o.p=TransformWorldToHClip(o.world);o.n=TransformObjectToWorldNormal(a.n);o.fog=ComputeFogFactor(o.p.z);return o;}
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise(float2 p){float2 f=frac(p);f=f*f*(3-2*f);float2 q=floor(p);return lerp(lerp(hash(q),hash(q+float2(1,0)),f.x),lerp(hash(q+float2(0,1)),hash(q+1),f.x),f.y);}
   half4 Frag(V i):SV_Target
   {
    float3 n=normalize(i.n),p=i.world;float3 w=pow(abs(n),4);w/=max(.001,w.x+w.y+w.z);
    float3 detail=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.zy*.24).rgb*w.x+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xz*.24).rgb*w.y+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xy*.24).rgb*w.z;
    float macro=noise(p.xz*.06),fine=noise(p.xz*19);float3 c;
    if(_Mode<.5){float road=1-smoothstep(7,12,abs(p.z+.8*sin(p.x*.05)));float tire=(1-smoothstep(.1,.4,abs(abs(p.z)-2.5)))*(.4+.6*noise(p.xz*.8));float ripples=sin(p.x*4+p.z*1.6+noise(p.xz*.3)*3);c=_BaseColor.rgb*(.84+macro*.24+fine*.10+ripples*.018);c*=1-road*.12-tire*.045;}
    else {float band=sin(p.y*3.8+noise(p.xz*.07)*3)+.35*sin(p.y*11);float cracks=pow(saturate(noise(p.xy*.8)),5);c=_BaseColor.rgb*(.82+band*.075+macro*.22)*(detail*.55+.58);c*=1-cracks*.18;}
    SurfaceData s=(SurfaceData)0;s.albedo=c;s.normalTS=float3(0,0,1);s.smoothness=.08;s.specular=.15;s.occlusion=1;s.alpha=1;
    InputData d=(InputData)0;d.positionWS=p;d.normalWS=n;d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(p);d.shadowCoord=TransformWorldToShadowCoord(p);d.bakedGI=SampleSH(n);d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);d.shadowMask=1;
    half4 col=UniversalFragmentPBR(d,s);col.rgb=MixFog(col.rgb,i.fog);return col;
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
