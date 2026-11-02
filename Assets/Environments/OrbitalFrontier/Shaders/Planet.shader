Shader "OrbitalFrontier/Planet"
{
 Properties { _BaseMap("Surface",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1) _Moon("Procedural lunar surface",Float)=0 _GasGiant("Gas giant: 1 Jupiter, 2 Saturn",Float)=0 }
 SubShader
 {
  Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  Pass
  {
   Tags{"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseColor;float4 _BaseMap_ST;float _Moon;float _GasGiant;
   CBUFFER_END
   struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
   struct V{float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;float2 uv:TEXCOORD1;float3 positionWS:TEXCOORD2;};
   V vert(A a){V o;o.positionWS=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(a.normalOS);o.uv=a.uv;return o;}
   float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
   float noise(float3 p){float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(a),hash(a+float3(1,0,0)),f.x),lerp(hash(a+float3(0,1,0)),hash(a+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(a+float3(0,0,1)),hash(a+float3(1,0,1)),f.x),lerp(hash(a+float3(0,1,1)),hash(a+1),f.x),f.y),f.z);}
   half4 frag(V i):SV_Target
   {
    float3 n=normalize(i.normalWS);Light sun=GetMainLight();float d=dot(n,sun.direction);float day=smoothstep(-.12,.20,d);float diffuse=max(0,d);
    float3 albedo=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb*_BaseColor.rgb;
    if(_Moon<.5&&_GasGiant<.5){float ocean=1-smoothstep(.012,.075,max(albedo.r,max(albedo.g,albedo.b)));albedo+=float3(.012,.04,.085)*ocean;}
    if(_GasGiant>.5)
    {
     float3 p=normalize(TransformWorldToObject(i.positionWS));float latitude=asin(clamp(p.y,-1,1));float longitude=atan2(p.z,p.x);float turbulence=noise(p*14)*2+sin(longitude*9+latitude*12)*.3;
     float bands=sin(latitude*43+turbulence)*.65+.24*sin(latitude*106+noise(p*44)*1.6);float stripe=smoothstep(-1.25,1.25,bands);float fine=noise(p*140)*.045;
     float3 pale=_GasGiant>1.5?float3(.74,.64,.40):float3(.78,.72,.60);float3 brown=_GasGiant>1.5?float3(.48,.39,.23):float3(.43,.31,.22);albedo=lerp(brown,pale,stripe)+fine;
     if(_GasGiant<1.5){float dx=frac(i.uv.x-.53+.5)-.5;float dy=i.uv.y-.43;float spot=length(float2(dx/.055,dy/.026));float rim=1-smoothstep(.8,1.2,spot);float swirl=.1*sin(atan2(dy,dx)*5+spot*8);albedo=lerp(albedo,float3(.49,.21,.12)+swirl,rim*.87);}
    }
    if(_Moon>.5){float3 p=normalize(TransformWorldToObject(i.positionWS));float g=noise(p*8)*.25+noise(p*35)*.12+noise(p*140)*.035;float craters=1;for(int k=0;k<3;k++){float scale=12+k*19;float3 cell=floor(p*scale);float3 f=frac(p*scale);float nearest=2;for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)for(int z=-1;z<=1;z++){float3 v=float3(x,y,z);float3 seed=float3(hash(cell+v),hash(cell+v+17),hash(cell+v+37));nearest=min(nearest,length(v+seed-f));}craters*=.82+.2*smoothstep(.13,.34,nearest)+.13*exp(-pow((nearest-.32)*18,2));}albedo=_BaseColor.rgb*(.5+g)*craters;}
    float3 c=albedo*(.012+day*(.16+diffuse*.9));
    float3 view=normalize(_WorldSpaceCameraPos-i.positionWS);float rim=pow(1-saturate(dot(n,view)),4);c+=(_GasGiant>.5?float3(.12,.10,.06):float3(.06,.22,.43))*rim*day*(1-_Moon)*.45;
    return half4(c,1);
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
  UsePass "Universal Render Pipeline/Lit/DepthNormals"
 }
}
