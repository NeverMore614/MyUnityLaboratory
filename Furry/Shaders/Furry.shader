Shader "Study/Furry"
{
    Properties
    {
        _ColorTex ("主帖图", 2D) = "white" {}
        _NoiseTex ("噪点", 2D) = "white" {}
        _FurryLength("毛发长度", float) = 1
        _FurryDensity("毛发密度", Range(1, 10)) = 1
        _UVOffest("毛发偏移)",Vector) = (0,0.01,0)
        _BrightCol("亮部色", Color) = (1,1,1,1)
        _DarkCol("暗部色", Color) = (0,0,0,1)
        _Color("毛发颜色", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalRenderPipeline"
            "RenderType" = "Transparent"
            "Queue" = "AlphaTest"
        }
        LOD 100

        Pass
        {

            ZWrite On
            ZTest On
            Cull back
            Blend SrcAlpha OneMinusSrcAlpha


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
                float3 uv_Fur     : TEXCOORD3;
            };

            sampler2D _NoiseTex;
            float4 _ColorTex_ST;
            //定义采样器
            TEXTURE2D(_ColorTex);
            SAMPLER(sampler_ColorTex);


            //本地到世界坐标
            float4x4 _LocalToWorldMatri;
            //pass数量（越高越精细）
            float _PassCount;
            //毛发长度
            float _FurryLength;
            //毛发密度
            float _FurryDensity;
            //毛发偏移
            float3 _UVOffest;
            //颜色
            float3 _BrightCol;
            float3 _DarkCol;
            float4 _Color;

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                //层数
                float layer = saturate(instanceID / _PassCount);

                //朝法线延申
                float normalLength = (layer) * _FurryLength;
                v.positionOS.xyz = v.positionOS.xyz + v.normalOS * normalLength;
                float3 normalOS = v.normalOS;
                //坐标转换
                o.positionWS = mul(_LocalToWorldMatri, v.positionOS);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = mul(_LocalToWorldMatri, normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _ColorTex)+ _UVOffest.xy * layer * 0.1;
                o.uv_Fur.xy = o.uv * _FurryDensity;
                o.uv_Fur.z = layer;

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {   

                float layer = i.uv_Fur.z;
                float4 col = SAMPLE_TEXTURE2D(_ColorTex, sampler_ColorTex, i.uv);
                col.rgb = col.rgb * lerp(0.2, 1, layer);
                //col.rgb = col.rgb * lerp(0, 1, layer);
                float4 noise = tex2D(_NoiseTex, i.uv_Fur.xy);
                float alpha = noise.a - layer;
                if (alpha > 0)
                {
                    alpha = 1 - layer;
                }
                if (alpha < 0)
                {
                    discard;
                }
                col.a = alpha;
                //光照
                float3 normalWS = i.normalWS + noise.xyz;
                float4 shadowCoords = TransformWorldToShadowCoord(i.positionWS);
                Light mainLight = GetMainLight(shadowCoords);
                float lambert = saturate(dot(normalWS, mainLight.direction));
                col.xyz = col.xyz * lerp(_DarkCol, _BrightCol, lambert) * mainLight.color ;
                return col;
            }
            ENDHLSL
        }
    }
}
