Shader "Study/PlaneShadow"
{
    Properties
    {
        _ShadowCol("ShadowCol", Color) = (0,0,0,0)
        _LightDir("LightDir", Vector) = (0, -1, 0)
        _Offset("Offset", Vector) = (0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Shadow"

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

            float3 _LightDirNormal;
            float3 _PlaneDirNormal;
            float _CosLP;
            float _PlaneDistance;
            float4 _ShadowColor;


            float3 _Offset;
            float4 _ShadowCol;

            v2f vert (appdata v)
            {
                v2f o;
                //计算到平面的距离
                float3 dirOS = normalize(v.positionOS.xyz);
                float disOS = length(v.positionOS.xyz);

                float cosPO = dot(dirOS, _PlaneDirNormal);
                float length = _PlaneDistance / cosPO;

                float disOP = _PlaneDistance * ((length - disOS) / length);

                float disLight = disOP / _CosLP;

                float3 postionWS = TransformObjectToWorld(v.positionOS);

                //v.positionOS.xyz += _LightDirNormal * disLight;
                postionWS += _LightDirNormal * disLight;

                o.positionHCS = TransformWorldToHClip(postionWS);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return _ShadowColor;
            }
            ENDHLSL
        }
    }
}
