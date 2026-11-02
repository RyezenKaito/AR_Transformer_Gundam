Shader "Volcano/URP Flowing Lava"
{
    Properties
    {
        _CrustMap("Cooled crust",2D)="gray"{}
        _Emission("Thermal radiance",Range(0,10))=3.5
        _Crust("Crust coverage",Range(0,1))=.58
        _Speed("Flow speed",Float)=.075
        _Motion("Rolling flow waves",Range(0,1))=.45
    }
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
        Pass
        {
            Tags{"LightMode"="UniversalForward"}
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_CrustMap);SAMPLER(sampler_CrustMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _CrustMap_ST;float _Emission,_Crust,_Speed,_Motion;
            CBUFFER_END
            struct A{float4 pos:POSITION;float2 uv:TEXCOORD0;};
            struct V{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;float fog:TEXCOORD2;};
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float Noise(float2 p){float2 f=frac(p);float2 b=f*f*(3-2*f);p=floor(p);return lerp(lerp(Hash(p),Hash(p+float2(1,0)),b.x),lerp(Hash(p+float2(0,1)),Hash(p+1),b.x),b.y);}
            float Fbm(float2 p){return .55*Noise(p)+.27*Noise(p*2.03)+.12*Noise(p*4.07)+.06*Noise(p*8.11);}
            float Cracks(float2 p)
            {
                float2 cell=floor(p),f=frac(p);float first=10,second=10;
                for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++){float2 g=float2(x,y);float2 h=float2(Hash(cell+g),Hash(cell+g+17.3));float d=length(g+h-f);if(d<first){second=first;first=d;}else second=min(second,d);}
                return second-first;
            }
            V Vert(A a){V o;o.world=TransformObjectToWorld(a.pos.xyz);o.pos=TransformWorldToHClip(o.world);o.uv=a.uv;o.fog=ComputeFogFactor(o.pos.z);return o;}
            float3 Lava(float2 q,float t)
            {
                q.x+=(.12+_Motion*.32)*sin(q.y*2.4-t*_Speed*2.5)+.07*sin(q.y*6.5-t*_Speed*3);
                float warp=Fbm(q*.7+13);float n=Fbm(q*2+warp*1.7);
                float ridges=abs(sin(q.y*17+warp*14+sin(q.x*6)*2));
                float2 folded=q*1.7+float2(Fbm(q*2.3+8),Fbm(q*2.1+32))*1.7;
                float gap=Cracks(folded);
                float hot=1-smoothstep(.012,.06+(1-_Crust)*(.13+n*.14),gap);
                hot=max(hot*.88,smoothstep(.6,.82,n)*.8);
                hot*=.82+.18*ridges;
                float rollingFront=pow(saturate(.5+.5*sin(q.y*7-t*_Speed*4+warp*5)),6);
                hot=saturate(hot+rollingFront*_Motion*.14);
                float3 crust=SAMPLE_TEXTURE2D(_CrustMap,sampler_CrustMap,q*.9).rgb*.5;
                float3 core=lerp(float3(.38,.005,.0005),float3(1,.09,.002),smoothstep(.04,.65,hot));
                core=lerp(core,float3(1,.38,.035),smoothstep(.76,1,hot)*.7);
                float pulse=.96+.04*sin(t*.48+q.x*.9+q.y*.3);
                return crust+core*hot*_Emission*pulse;
            }
            half4 Frag(V i):SV_Target
            {
                float t=_Time.y;float phase=frac(t*_Speed*.08);float blend=abs(phase*2-1);
                float2 q=i.uv*float2(2.5,1);float2 a=q-float2(0,phase*5);float2 b=q-float2(0,frac(phase+.5)*5);
                float3 col=lerp(Lava(a,t),Lava(b,t),blend);
                return half4(MixFog(col,i.fog),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
