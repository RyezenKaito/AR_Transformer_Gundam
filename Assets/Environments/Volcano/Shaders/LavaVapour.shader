Shader "Volcano/URP Thin Lava Vapour"
{
    Properties
    {
        _BaseMap("Soft density",2D)="white"{}
        _Opacity("Vapour opacity",Range(0,1))=.65
    }
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;float _Opacity;
            CBUFFER_END
            struct A{float4 pos:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
            struct V{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;float3 world:TEXCOORD1;float eye:TEXCOORD2;float fog:TEXCOORD3;};
            V Vert(A a){V o;o.world=TransformObjectToWorld(a.pos.xyz);o.pos=TransformWorldToHClip(o.world);o.eye=-TransformWorldToView(o.world).z;o.uv=a.uv;o.color=a.color;o.fog=ComputeFogFactor(o.pos.z);return o;}
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float Noise(float2 p){float2 f=frac(p);f=f*f*(3-2*f);p=floor(p);return lerp(lerp(Hash(p),Hash(p+float2(1,0)),f.x),lerp(Hash(p+float2(0,1)),Hash(p+1),f.x),f.y);}
            half4 Frag(V i):SV_Target
            {
                float2 uv=i.uv;float t=_Time.y;
                float n=Noise(uv*4+i.world.xz*.07+float2(t*.06,-t*.14));
                uv+=float2(sin(uv.y*8+t*.5),cos(uv.x*7-t*.4))*.025;
                float density=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).a*(.45+.55*n);
                float depth=LinearEyeDepth(SampleSceneDepth(GetNormalizedScreenSpaceUV(i.pos)),_ZBufferParams);
                float soft=saturate((depth-i.eye)/1.2)*saturate((i.eye-.3)/1.5);
                half alpha=saturate(density*i.color.a*_Opacity*soft);
                half3 col=lerp(i.color.rgb*.8,i.color.rgb*1.1,n);
                return half4(MixFog(col,i.fog),alpha);
            }
            ENDHLSL
        }
    }
}
