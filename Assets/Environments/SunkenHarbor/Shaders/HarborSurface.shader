Shader "SunkenHarbor/Weathered Surface"
{
 Properties { _BaseMap("Albedo",2D)="white"{} _BumpMap("Normal",2D)="bump"{} _BaseColor("Tint",Color)=(1,1,1,1) _Smoothness("Smoothness",Range(0,1))=.2 _Metallic("Metallic",Range(0,1))=0 _Biofilm("Marine growth",Range(0,1))=.25 _WaterLevel("Water level",Float)=42 _Sway("Plant sway",Float)=0 _Cull("Cull",Float)=2 _EmissionColor("Emission",Color)=(0,0,0,1) _AlphaClip("Cutout",Float)=0 _Cutoff("Cutoff",Float)=.4 _Island("Island vegetation tint",Float)=0 }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Cull [_Cull]
  Pass
  {
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma shader_feature_local _NORMALMAP
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);TEXTURE2D(_BumpMap);SAMPLER(sampler_BumpMap);
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseMap_ST,_BaseColor,_EmissionColor;float _Smoothness,_Metallic,_Biofilm,_WaterLevel,_Sway,_Cull,_AlphaClip,_Cutoff,_Island;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;float4 tangent:TANGENT;float2 uv:TEXCOORD0;};
   struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;float4 tangent:TEXCOORD2;float2 uv:TEXCOORD3;};
   V Vert(A a){V o;float3 p=a.p.xyz;float mask=saturate(a.uv.y);p.x+=_Sway*mask*mask*sin(_Time.y*.55+p.y*.38+TransformObjectToWorld(float3(0,0,0)).x);p.z+=_Sway*.4*mask*mask*sin(_Time.y*.43+p.y*.5);o.world=TransformObjectToWorld(p);o.p=TransformWorldToHClip(o.world);o.n=TransformObjectToWorldNormal(a.n);o.tangent=float4(TransformObjectToWorldDir(a.tangent.xyz),a.tangent.w*GetOddNegativeScale());o.uv=a.uv*_BaseMap_ST.xy+_BaseMap_ST.zw;return o;}
   float H(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float N(float2 p){float2 f=frac(p),q=floor(p);f=f*f*(3-2*f);return lerp(lerp(H(q),H(q+float2(1,0)),f.x),lerp(H(q+float2(0,1)),H(q+1),f.x),f.y);}
   half4 Frag(V i):SV_Target
   {
    float3 n=normalize(i.n);float4 sample=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(lerp(1,sample.a-_Cutoff,_AlphaClip));float3 tex=sample.rgb;float3 color=tex*_BaseColor.rgb;
    #if defined(_NORMALMAP)
    float3 tangent=normalize(i.tangent.xyz),bitangent=cross(n,tangent)*i.tangent.w;float3 normalTS=UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,i.uv));n=normalize(normalTS.x*tangent+normalTS.y*bitangent+normalTS.z*n);
    #endif
    float wet=saturate((_WaterLevel-i.world.y)*.2),noise=N(i.world.xz*.55+float2(i.world.y*.13,0));float film=_Biofilm*wet*smoothstep(.32,.78,noise)*(.45+.55*saturate(n.y+.3));color=lerp(color,color*float3(.40,.61,.45),film);
    color*=lerp(1,float3(.72,.91,1),wet*.48);float moss=_Island*smoothstep(_WaterLevel+1,_WaterLevel+7,i.world.y)*smoothstep(.45,.8,n.y);color=lerp(color,color*float3(.32,.52,.23),moss*.85);
    SurfaceData s=(SurfaceData)0;s.albedo=color;s.metallic=_Metallic;s.smoothness=_Smoothness;s.specular=.2;s.normalTS=float3(0,0,1);s.occlusion=1;s.alpha=1;s.emission=_EmissionColor.rgb;
    // Soft broad caustic cells only illuminate exposed upward surfaces.
    float2 cp=i.world.xz*.32+float2(_Time.y*.07,_Time.y*.04);float ca=pow(saturate(1-abs(sin(cp.x+sin(cp.y*1.13))+sin(cp.y+sin(cp.x*.9)))*1.9),5);float depth=max(0,_WaterLevel-i.world.y);float shadow=GetMainLight(TransformWorldToShadowCoord(i.world)).shadowAttenuation;s.emission+=color*ca*.18*saturate(n.y)*exp(-depth*.035)*wet*shadow;
    InputData d=(InputData)0;d.positionWS=i.world;d.normalWS=n;d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);d.shadowCoord=TransformWorldToShadowCoord(i.world);d.bakedGI=SampleSH(n);d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);d.shadowMask=1;return UniversalFragmentPBR(d,s);
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
  UsePass "Universal Render Pipeline/Lit/DepthNormals"
 }
}
