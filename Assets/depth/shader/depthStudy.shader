Shader "Study/depthStudy"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionHCS : SV_POSITION;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 ScreenUV = i.positionHCS.xy / _ScaledScreenParams.xy;
#if UNITY_REVERSED_Z
                real depth = SampleSceneDepth(ScreenUV);
#else
                //  调整 Z 以匹配 OpenGL 的 NDC ([-1, 1])
                real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(ScreenUV));
#endif
                depth = Linear01Depth(depth, _ZBufferParams);
                float4 col = float4(depth, depth, depth, 1);

                return col;
            }
            ENDHLSL
        }
    }
}
