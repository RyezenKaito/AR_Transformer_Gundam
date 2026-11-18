Shader "Desert/Eroded Sandstone"
{
 Properties { _BaseColor("Sandstone tint",Color)=(1,1,1,1) _BaseMap("Weathered rock albedo",2D)="gray"{} _WorldScale("Texture repeats per metre",Float)=.22 _Relief("Surface relief metres",Range(0,.3))=.10 }
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
   float4 _BaseColor;float4 _BaseMap_ST;float _WorldScale,_Relief;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;};
   struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;float fog:TEXCOORD2;};
   V Vert(A a){V o;o.world=TransformObjectToWorld(a.p.xyz);o.p=TransformWorldToHClip(o.world);o.n=TransformObjectToWorldNormal(a.n);o.fog=ComputeFogFactor(o.p.z);return o;}
   float H(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float N(float2 p){float2 q=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(H(q),H(q+float2(1,0)),f.x),lerp(H(q+float2(0,1)),H(q+1),f.x),f.y);}
   float3 Rock(float3 p,float3 w,float scale)
   {return SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.zy*scale).rgb*w.x+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xz*scale).rgb*w.y+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xy*scale).rgb*w.z;}
   float RockHeight(float3 p,float3 w,float scale)
   {float3 c=SAMPLE_TEXTURE2D_BIAS(_BaseMap,sampler_BaseMap,p.zy*scale,2).rgb*w.x+SAMPLE_TEXTURE2D_BIAS(_BaseMap,sampler_BaseMap,p.xz*scale,2).rgb*w.y+SAMPLE_TEXTURE2D_BIAS(_BaseMap,sampler_BaseMap,p.xy*scale,2).rgb*w.z;return dot(c,float3(.3,.59,.11));}
   half4 Frag(V i):SV_Target
   {
    float3 p=i.world,n=normalize(i.n),w=pow(abs(n),6);w/=max(.001,w.x+w.y+w.z);
    float3 tex=Rock(p,w,_WorldScale);
    float macro=N(p.xz*.018+float2(p.y*.013,0));
    // The scan-like albedo carries the mineral detail; no periodic bands or stretched mountain atlas.
    float3 broad=Rock(p+float3(17.3,9.1,31.7),w,_WorldScale*.37);
    float3 c=lerp(tex,tex*(.78+broad*.65),.22)*_BaseColor.rgb*(.94+macro*.12);
    float grain=dot(tex,float3(.3,.59,.11));float height=RockHeight(p,w,_WorldScale)*_Relief;
    float3 dx=ddx(p),dy=ddy(p),r1=cross(dy,n),r2=cross(n,dx);float determinant=dot(dx,r1);float3 gradient=sign(determinant)*(ddx(height)*r1+ddy(height)*r2)/max(abs(determinant),1e-7);
    gradient*=min(1,.18/(length(gradient)+1e-5))*(1-smoothstep(35,120,distance(p,_WorldSpaceCameraPos)));n=normalize(n-gradient);
    SurfaceData s=(SurfaceData)0;s.albedo=c;s.normalTS=float3(0,0,1);s.specular=.12;s.smoothness=lerp(.035,.10,grain);s.occlusion=1;s.alpha=1;
    InputData d=(InputData)0;d.positionWS=p;d.normalWS=n;d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(p);d.shadowCoord=TransformWorldToShadowCoord(p);d.bakedGI=SampleSH(n)+float3(.10,.065,.035)*saturate(-n.y);d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);d.shadowMask=1;
    half4 col=UniversalFragmentPBR(d,s);col.rgb=MixFog(col.rgb,i.fog);return col;
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
