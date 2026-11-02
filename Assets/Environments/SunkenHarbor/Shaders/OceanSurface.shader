Shader "SunkenHarbor/Ocean Surface"
{
 Properties {_WaterLevel("Water level",Float)=42 _DeepColor("Deep water",Color)=(.025,.18,.21,1)}
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off
  Pass
  {
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
   float _WaterLevel;float4 _DeepColor;
   struct A {float4 p:POSITION;};struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;};
   V Vert(A a){V o;o.world=TransformObjectToWorld(a.p.xyz);o.world.y+=sin(o.world.x*.09+_Time.y*.43)*.19+sin(o.world.z*.12+o.world.x*.04-_Time.y*.36)*.14;o.p=TransformWorldToHClip(o.world);return o;}
   half4 Frag(V i,FRONT_FACE_TYPE front:FRONT_FACE_SEMANTIC):SV_Target
   {
    float2 uv=GetNormalizedScreenSpaceUV(i.p);float3 view=GetWorldSpaceNormalizeViewDir(i.world);float2 p=i.world.xz;
    float footprint=max(length(ddx(p)),length(ddy(p)));float detailFade=exp(-distance(i.world,_WorldSpaceCameraPos)*.018)*saturate(1-footprint*.8);
    float nx=cos(p.x*.09+_Time.y*.43)*.017+cos(p.x*.85+p.y*.4+_Time.y*1.2)*.045*detailFade;
    float nz=cos(p.y*.12+p.x*.04-_Time.y*.36)*.017+sin(p.y*1.2-p.x*.35-_Time.y)*.035*detailFade;
    float3 normal=normalize(float3(-nx,1,-nz));float3 reflected=reflect(-view,normal);float fresnel=.035+.965*pow(1-saturate(abs(dot(view,normal))),5);
    float3 sky=lerp(float3(.64,.73,.78),float3(.22,.43,.63),saturate(reflected.y));Light sun=GetMainLight();float spec=pow(saturate(dot(reflect(-sun.direction,normal),view)),280)*1.5;
    float2 refractedUV=clamp(uv+float2(nx,nz)*.012,.002,.998);float3 refraction=SampleSceneColor(refractedUV);float3 color;
    float cloud=.9+.1*sin(p.x*.04+sin(p.y*.05));float3 below=lerp(sky*.85+float3(.02,.10,.12),refraction*float3(.60,.88,.96),.18)*cloud+spec*.15;
    float raw=SampleSceneDepth(uv);float behind=LinearEyeDepth(raw,_ZBufferParams);float surface=-TransformWorldToView(i.world).z;float diff=max(0,behind-surface);float foam=(1-smoothstep(.05,1.15,diff))*(.65+.35*sin(p.x*2+sin(p.y*2)+_Time.y*1.7));float3 above=lerp(lerp(refraction,_DeepColor.rgb,.60),sky,fresnel)+spec*sun.color;above=lerp(above,float3(.82,.9,.89),foam*.7);
    color=lerp(below,above,smoothstep(_WaterLevel-.4,_WaterLevel+.4,_WorldSpaceCameraPos.y));
    return half4(color,1);
   }
   ENDHLSL
  }
 }
}
