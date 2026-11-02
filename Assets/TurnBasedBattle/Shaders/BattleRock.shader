Shader "TurnBattle/RockDissolve"
{
    Properties
    {
        _BaseColor("Rock color",Color)=(.22,.20,.17,1)
        [HDR]_EdgeColor("Dissolve edge",Color)=(3,2.3,.15,1)
        _Dissolve("Dissolve",Range(0,1))=0
        _Seed("Variation",Float)=1
        _BattleFlash("Hit flash",Range(0,1))=0
    }
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
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor, _EdgeColor;
                float _Dissolve, _Seed, _BattleFlash;
            CBUFFER_END
            struct A{float4 p:POSITION;float3 n:NORMAL;float4 c:COLOR;};
            struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float3 o:TEXCOORD2;float4 c:COLOR;};
            V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.w=TransformObjectToWorld(i.p.xyz);o.n=TransformObjectToWorldNormal(i.n);o.o=i.p.xyz;o.c=i.c;return o;}
            float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7))+_Seed)*43758.5453);}
            float noise(float3 p)
            {
                float3 k=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(hash(k),hash(k+float3(1,0,0)),f.x),lerp(hash(k+float3(0,1,0)),hash(k+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(k+float3(0,0,1)),hash(k+float3(1,0,1)),f.x),lerp(hash(k+float3(0,1,1)),hash(k+1),f.x),f.y),f.z);
            }
            half4 frag(V i):SV_Target
            {
                float n=noise(i.o*5.5)*.7+noise(i.o*17)*.3;
                clip(n-_Dissolve*1.12+.04);
                Light light=GetMainLight();float3 normal=normalize(i.n);
                float diffuse=saturate(dot(normal,light.direction));
                float variation=.62+n*.7;
                half3 color=_BaseColor.rgb*variation*(.30+diffuse*light.color*.85)*i.c.rgb;
                float edge=(_Dissolve>.001 && _Dissolve<.99)?1-smoothstep(0,.075,n-_Dissolve*1.12+.04):0;
                color+=_EdgeColor.rgb*edge;
                color=lerp(color,half3(1,.7,.25),_BattleFlash*.7);
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
