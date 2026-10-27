Shader "PocketWreckshop/ScreenCracks"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float3 positionOS:POSITION; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; half4 color:COLOR; };
            Varyings Vertex(Attributes input)
            {
                Varyings output;output.positionCS=TransformObjectToHClip(input.positionOS);output.color=input.color;return output;
            }
            half4 Fragment(Varyings input):SV_Target { return input.color; }
            ENDHLSL
        }
    }
}
