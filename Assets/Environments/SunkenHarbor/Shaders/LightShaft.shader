Shader "SunkenHarbor/Soft Light Shaft"
{
 Properties {_BaseColor("Color",Color)=(.18,.65,.68,.035)}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Back
 Pass {HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 float4 _BaseColor;struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;};
 V Vert(A a){V o;o.world=TransformObjectToWorld(a.p.xyz);o.p=TransformWorldToHClip(o.world);o.n=TransformObjectToWorldNormal(a.n);o.uv=a.uv;return o;}
 half4 Frag(V i):SV_Target{float edge=pow(saturate(dot(normalize(i.n),GetWorldSpaceNormalizeViewDir(i.world))),2);float ends=smoothstep(0,.2,i.uv.y)*(1-smoothstep(.75,1,i.uv.y));float noise=.7+.3*sin(i.world.y*.37+_Time.y*.15+i.world.x*.3);return half4(_BaseColor.rgb,_BaseColor.a*edge*ends*noise);}
 ENDHLSL}
 }
}
