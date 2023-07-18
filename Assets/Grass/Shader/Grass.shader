Shader "Unlit/Grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

    }
    SubShader
    {
        Tags {"Queue" = "AlphaTest"  "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"}
        LOD 100

        Pass
        {

            Cull Off

            Tags { "LightMode" = "Meta" }


            HLSLPROGRAM
            //第一步： sharder 增加变体使用shader可以支持instance  
            #pragma multi_compile_instancing
            #pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma shader_feature _RECEIVE_SHADOWS_OFF
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE


            struct appdata
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float4 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            float4 _MainTex_ST;
            float4x4 _LocalToWorldMatri;

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);

            StructuredBuffer<float4x4> _GrassInfos;



            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f o;
                float4x4 localToTerrian = _GrassInfos[instanceID];
                float4 positionOS = v.positionOS;
                float3 normalOS = v.normalOS;

                //将顶点和法线从Quad本地空间转换到Terrian本地空间
                positionOS = mul(localToTerrian, positionOS);
                normalOS = mul(localToTerrian, normalOS);
                o.normalWS = mul(unity_ObjectToWorld, normalOS);

                float4 positionWS = mul(_LocalToWorldMatri, positionOS);
                o.positionWS = positionWS;

                o.positionCS = TransformWorldToHClip(positionWS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                //光影
                float4 shadowCoords = TransformWorldToShadowCoord(i.positionWS);
                Light mainLight = GetMainLight(shadowCoords);
                // sample the texture
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                if (col.a < 0.5f)
                {
                    discard;
                    return 0;
                }
                float3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - i.positionWS); // safe防止分母为0
                float dotValue1 = (dot(viewDirectionWS, i.normalWS));
                float dotValue = (dot(mainLight.direction, i.normalWS));
                //col.rgb = col.rgb * mainLight.color.rgb * clamp(mainLight.shadowAttenuation, 0.2f, 1);
                col.rgb = col.rgb * mainLight.color.rgb * ((dotValue * dotValue1 + 1) / 2) * clamp(mainLight.shadowAttenuation, 0.3f, 1);
                return col;
            }
            ENDHLSL
        }
    }
}
