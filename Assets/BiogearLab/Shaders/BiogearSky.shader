Shader "BiogearLab/Tropical Sky"
{
 Properties { _Top("Zenith",Color)=(.075,.42,.64,1) _Horizon("Horizon",Color)=(.46,.78,.85,1) _SunDir("Sun Direction",Vector)=(-.5,.65,-.3,0) }
 SubShader { Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" } Cull Off ZWrite Off
 Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A{float4 vertex:POSITION;};struct V{float4 position:SV_POSITION;float3 direction:TEXCOORD0;};
 float4 _Top,_Horizon,_SunDir;
 float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
 float n(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+1),f.x),f.y),f.z);}
 float fbm(float3 p){return n(p)*.55+n(p*2.1)*.27+n(p*4.3)*.13+n(p*8.7)*.05;}
 V vert(A v){V o;o.position=TransformObjectToHClip(v.vertex.xyz);o.direction=v.vertex.xyz;return o;}
 half4 frag(V i):SV_Target{float3 d=normalize(i.direction);float3 c=lerp(_Horizon.rgb,_Top.rgb,saturate(d.y*.9));float sun=saturate(dot(d,normalize(_SunDir.xyz)));c+=float3(1,.88,.62)*(pow(sun,750)*5+pow(sun,20)*.3);
 float3 p=d*7+float3(6,2,9);float f=fbm(p)+n(p*17)*.027;float cloud=smoothstep(.48,.56,f)*smoothstep(-.03,.14,d.y);float shade=fbm(p+float3(.12,.22,.06));float3 cc=lerp(float3(.25,.43,.57),float3(1,.99,.93),saturate((shade-.32)*3.5));c=lerp(c,cc,cloud*.94);return half4(c,1);}
 ENDHLSL
 } }
}
