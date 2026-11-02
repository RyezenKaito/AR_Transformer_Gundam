Shader "BlueFlame/ProceduralEnergy"
{
    Properties { _Intensity("Emission intensity",Range(0,8))=2.2 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Intensity;
            CBUFFER_END
            struct Attributes {float4 positionOS:POSITION;float2 uv:TEXCOORD0;float2 phase:TEXCOORD1;};
            struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float2 phase:TEXCOORD1;};
            Varyings vert(Attributes v)
            {
                Varyings o;float3 p=v.positionOS.xyz;
                p.x+=sin(_Time.y*2.9+v.phase.x*12+v.uv.y*5)*v.uv.y*v.uv.y*.055;
                o.positionCS=TransformObjectToHClip(p);o.uv=v.uv;o.phase=v.phase;return o;
            }
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float noise(float2 p)
            {
                float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);
            }
            half4 frag(Varyings i):SV_Target
            {
                float2 p=float2(i.uv.x*3+i.phase.x*17,i.uv.y*4.5-_Time.y*2.4+i.phase.y*9);
                float n=noise(p)*.65+noise(p*2.1)*.25+noise(p*4.3)*.10;
                float center=.5+sin(i.uv.y*9-_Time.y*3+i.phase.x*21)*.13*i.uv.y;
                float edge=1-abs(i.uv.x-center)*2;
                float shape=saturate((edge-(n*.42+i.uv.y*.24)) * 4);
                float base=smoothstep(0,.08,i.uv.y);float tip=pow(saturate(1-i.uv.y),.65);
                float hot=saturate((edge-.55)*2.0)*(1-i.uv.y*.8);
                float3 color=lerp(float3(.005,.12,1),float3(.16,.83,1),shape);
                color=lerp(color,float3(.65,.96,1),hot);
                return half4(color*_Intensity,shape*base*tip*.62);
            }
            ENDHLSL
        }
    }
}
