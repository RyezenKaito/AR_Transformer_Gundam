Shader "Volcano/URP Basalt"
{
    Properties
    {
        _BaseMap("Basalt Albedo",2D)="gray"{}
        _BaseColor("Tint",Color)=(0.65,0.67,0.7,1)
        _Tiling("World tiling",Float)=0.22
        _Relief("Fine surface relief",Range(0,2))=0.5
        _Smoothness("Smoothness",Range(0,1))=0.14
        _Heat("Hot fractures",Range(0,4))=0
    }
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;float4 _BaseMap_ST;float _Tiling,_Relief,_Smoothness,_Heat;
            CBUFFER_END
            struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct V{float4 positionCS:SV_POSITION;float3 pos:TEXCOORD0;float3 normal:TEXCOORD1;float fog:TEXCOORD2;UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO};
            V Vert(A v){V o=(V)0;UNITY_SETUP_INSTANCE_ID(v);UNITY_TRANSFER_INSTANCE_ID(v,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.pos=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.pos);o.normal=TransformObjectToWorldNormal(v.normalOS);o.fog=ComputeFogFactor(o.positionCS.z);return o;}
            float Hash(float3 p){return frac(sin(dot(p,float3(12.9898,78.233,37.719)))*43758.5453);}
            half4 Frag(V i):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 n=normalize(i.normal),w=pow(abs(n),4);w/=max(.001,w.x+w.y+w.z);
                float3 p=i.pos*_Tiling;
                float3 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.zy).rgb*w.x+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xz).rgb*w.y+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xy).rgb*w.z;
                float h=dot(c,float3(.3,.59,.11));
                float3 dx=ddx(i.pos),dy=ddy(i.pos);float3 r1=cross(dy,n),r2=cross(n,dx);float determinant=dot(dx,r1);
                float3 surfaceGradient=sign(determinant)*(ddx(h)*r1+ddy(h)*r2);
                n=normalize(abs(determinant)*n-_Relief*surfaceGradient);
                float macro=.85+.15*sin(i.pos.x*.11+sin(i.pos.z*.16))*sin(i.pos.y*.19+i.pos.z*.12);
                SurfaceData s=(SurfaceData)0;s.albedo=c*_BaseColor.rgb*macro;s.metallic=0;s.specular=.2;s.smoothness=_Smoothness;s.normalTS=float3(0,0,1);s.occlusion=lerp(.65,1,saturate(h*4));s.alpha=1;
                float crack=1-smoothstep(.025,.07,h);s.emission=float3(1,.075,.002)*crack*_Heat;
                InputData d=(InputData)0;d.positionWS=i.pos;d.normalWS=n;d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.pos);d.shadowCoord=TransformWorldToShadowCoord(i.pos);d.bakedGI=SampleSH(n);d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);d.shadowMask=1;
                half4 col=UniversalFragmentPBR(d,s);col.rgb=MixFog(col.rgb,i.fog);return col;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
