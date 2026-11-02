Shader "SunkenHarbor/Underwater Optics"
{
 Properties { _WaterLevel("Water level",Float)=42 _Absorption("Absorption per metre",Vector)=(.026,.011,.007,0) _Scatter("Scattered water light",Color)=(.025,.18,.23,1) _Visibility("Maximum optical distance",Float)=150 _Debug("Debug distance",Float)=0 }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline"} ZWrite Off ZTest Always Cull Off
  Pass
  {
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
   float _WaterLevel,_Visibility,_Debug;float4 _Absorption,_Scatter;
   half4 Frag(Varyings i):SV_Target
   {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float2 uv=i.texcoord;float depth=SampleSceneDepth(uv);
    #if !UNITY_REVERSED_Z
    depth=lerp(UNITY_NEAR_CLIP_VALUE,1,depth);
    #endif
    float3 end=ComputeWorldSpacePosition(uv,depth,UNITY_MATRIX_I_VP);float3 camera=_WorldSpaceCameraPos;float distanceToEnd=min(distance(camera,end),1500);float3 direction=normalize(end-camera);float optical=0;
    if(camera.y<_WaterLevel){optical=distanceToEnd;if(direction.y>.0001)optical=min(optical,(_WaterLevel-camera.y)/direction.y);}
    else if(direction.y<-.0001){float entry=(_WaterLevel-camera.y)/direction.y;optical=max(0,distanceToEnd-entry);}
    optical=clamp(optical,0,_Visibility);float3 transmit=exp(-_Absorption.rgb*optical);float3 source=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv).rgb;
    if(_Debug>.5)return half4((optical/100).xxx,1);
    return half4(source*transmit+_Scatter.rgb*(1-transmit),1);
   }
   ENDHLSL
  }
 }
}
