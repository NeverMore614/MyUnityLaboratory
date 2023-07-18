Shader "Study/Shadow"
{
    Properties
    {
        _ShadowCol("ShadowCol", Color) = (0,0,0,0)
        _LightDir("LightDir", Vector) = (0, -1, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Shadow"
                        Stencil
	           {
	           	Ref 0
	           	Comp equal
	           	Pass incrWrap
	           	Fail keep
	           	ZFail keep
	           }
            ZWrite off

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
            };

            struct v2f
            {
                float4 positionHCS : SV_POSITION;
            };

            float3 _LightDir;
            float4 _ShadowCol;

            v2f vert (appdata v)
            {
                v2f o;

                v.positionOS.xy = v.positionOS.xy - ((_LightDir.xy * v.positionOS.z) / _LightDir.z);
                v.positionOS.z = 0;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return _ShadowCol;
            }
            ENDHLSL
        }
    }
}
