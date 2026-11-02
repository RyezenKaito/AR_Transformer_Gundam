Shader "Volcano/Ash Sky"
{
    Properties{_Top("Zenith",Color)=(.08,.12,.18,1) _Horizon("Horizon",Color)=(.42,.32,.26,1) _AshCoverage("Ash cloud cover",Range(0,1))=.82}
    SubShader
    {
        Tags{"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float4 _Top,_Horizon;
            float _AshCoverage;
            struct V{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
            V Vert(float4 p:POSITION){V o;o.pos=TransformObjectToHClip(p.xyz);o.dir=p.xyz;return o;}
            float Hash(float3 p){return frac(sin(dot(p,float3(17.1,71.7,123.4)))*43758.5);}
            float Noise(float3 p){float3 f=frac(p);f=f*f*(3-2*f);p=floor(p);return lerp(lerp(lerp(Hash(p),Hash(p+float3(1,0,0)),f.x),lerp(Hash(p+float3(0,1,0)),Hash(p+float3(1,1,0)),f.x),f.y),lerp(lerp(Hash(p+float3(0,0,1)),Hash(p+float3(1,0,1)),f.x),lerp(Hash(p+float3(0,1,1)),Hash(p+1),f.x),f.y),f.z);}
            float Fbm(float3 p){return Noise(p)*.5+Noise(p*2.03)*.26+Noise(p*4.07)*.14+Noise(p*8.1)*.07+Noise(p*16.13)*.03;}
            half4 Frag(V i):SV_Target
            {
                float3 d=normalize(i.dir);float h=saturate(d.y);float3 col=lerp(_Horizon.rgb,_Top.rgb,pow(h,.45));
                float3 wind=float3(_Time.y*.008,0,_Time.y*.002);
                float broad=Fbm(d*5+wind);float rolling=Fbm(d*13-wind*.45+float3(broad*2,0,broad));
                float cover=smoothstep(.58-_AshCoverage*.34,.78-_AshCoverage*.25,broad*.65+rolling*.35);
                float heightMask=smoothstep(-.06,.24,d.y);
                float3 ash=lerp(float3(.075,.07,.073),float3(.3,.29,.28),saturate(rolling*1.6-.2));
                col=lerp(col,ash,cover*heightMask*.95);
                float veils=Fbm(d*3.5-wind*.2+32);col=lerp(col,float3(.26,.225,.2),smoothstep(.35,.7,veils)*_AshCoverage*.3*heightMask);
                float sun=pow(saturate(dot(d,normalize(float3(-.35,.3,-.8)))),600);col+=sun*float3(1.2,.7,.3)*(1-cover*.85);return half4(col,1);
            }
            ENDHLSL
        }
    }
}
