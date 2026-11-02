Shader "BiogearLab/Stylized Ocean"
{
 Properties { _Shallow("Turquoise",Color)=(0.015,0.55,0.62,1) _Deep("Deep blue",Color)=(0.008,0.21,0.34,1) _Foam("Wave highlight",Color)=(0.52,0.91,0.91,1) }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
 Pass { Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A { float4 vertex:POSITION; }; struct V { float4 position:SV_POSITION; float3 world:TEXCOORD0; };
 CBUFFER_START(UnityPerMaterial) float4 _Shallow,_Deep,_Foam; CBUFFER_END
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 V vert(A v){V o;float3 w=TransformObjectToWorld(v.vertex.xyz);w.y+=.08*sin(w.x*.7+_Time.y)*cos(w.z*.6+_Time.y*.6);o.world=w;o.position=TransformWorldToHClip(w);return o;}
 half4 frag(V i):SV_Target
 {
 float2 p=i.world.xz;float t=_Time.y*.11;
 float n=noise(p*.22+float2(t,t*.3))*.55+noise(p*.63-float2(t*.4,t))*.3+noise(p*1.65+t)*.15;
 float veins=pow(saturate(1-abs(n-.53)*17),3);
 float waves=pow(saturate(sin(p.x*.8+p.y*.46+n*11+_Time.y*.6)*.5+.5),18);
 float depth=noise(p*.028);
 float3 c=lerp(_Deep.rgb,_Shallow.rgb,saturate(.42+depth*.55+n*.23));c=lerp(c,_Foam.rgb,(veins*.43+waves*.13));
 float3 view=normalize(_WorldSpaceCameraPos-i.world);float3 normal=normalize(float3(sin(p.x*.7+_Time.y)*.09,1,cos(p.y*.9+_Time.y*.8)*.08));
 float spec=pow(saturate(dot(reflect(-normalize(float3(-.5,.8,-.2)),normal),view)),100);
 return half4(c+spec*.8,1);
 }
 ENDHLSL
 } }
}
