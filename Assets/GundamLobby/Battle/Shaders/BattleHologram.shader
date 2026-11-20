Shader "GundamLobby/BattleHologram"
{
    Properties{_BaseColor("Tint",Color)=(0.1,0.7,1,1)}
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
        Pass
        {
            Tags{"LightMode"="SRPDefaultUnlit"}
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            struct Attributes{float4 positionOS:POSITION;half4 color:COLOR;};
            struct Varyings{float4 positionCS:SV_POSITION;half4 color:COLOR;float3 world:TEXCOORD0;};
            Varyings Vert(Attributes input){Varyings o;o.world=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.color=input.color*_BaseColor;return o;}
            half4 Frag(Varyings input):SV_Target{half pulse=.94h+.06h*sin(_Time.y*1.2h+input.world.z*.8h);return half4(input.color.rgb,input.color.a*pulse);}
            ENDHLSL
        }
    }
}
