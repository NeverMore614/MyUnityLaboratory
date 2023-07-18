Shader "Effect/Fur_RendererFeature"
{
    Properties
    {
        [Header(Texture)]
        _MainTex("主帖图", 2D) = "white" {}
        [NoScaleOffset]_FurMask("毛发遮罩", 2D) = "white" {}
        _FurScale("毛发密度", float) = 10
        _NormalInt("法线强度", Range(-5, 5)) = 1

        [Space(20)]
        [Header(Color)]
        _BrightCol("亮部色", Color) = (1,1,1,1)
        _DarkCol("暗部色", Color) = (0,0,0,1)
        _AOInt("AO强度", Range(0, 1)) = 0.5
        _OpacityPow("高度-透明度衰减", Range(0, 4)) = 1

        [Space(20)]
        [Header(Shape)]
        _ConePow("锥化度", Range(0, 4)) = 1
        _Thick("总膨胀高度", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalRenderPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            Cull back
            ZWrite on

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 

            sampler2D _MainTex, _FurMask;
            CBUFFER_START(UnityPerMaterial)
            //全局
            float _Fur_Height01_GLB;

            //贴图
            float4 _MainTex_ST;
            float _FurScale;
            float _NormalInt;

            //颜色
            float3 _BrightCol;
            float3 _DarkCol;
            float _AOInt;
            float _OpacityPow;

            //形状
            float _ConePow;
            float _Thick;
            CBUFFER_END

            struct a2v
            {
                float4 posOS	: POSITION;
                float2 uv0      : TEXCOORD0;
                float3 nDirOS   : NORMAL;
                float4 tDirOS   : TANGENT;
            };

            struct v2f
            {
                float4 posCS	    : SV_POSITION;
                float2 uv_Main     : TEXCOORD0;
                float2 uv_Fur     : TEXCOORD1;
                float3 nDirWS   : TEXCOORD2;
                float3 tDirWS   : TEXCOORD3;
                float3 bDirWS   : TEXCOORD4;
            };

            v2f vert(a2v i)
            {
                v2f o;

                //坐标
                float3 posWS = TransformObjectToWorld(i.posOS.xyz);
                o.nDirWS = TransformObjectToWorldNormal(i.nDirOS);
                posWS += o.nDirWS * _Fur_Height01_GLB * _Thick;
                o.posCS = TransformWorldToHClip(posWS);

                //向量
                o.tDirWS = TransformObjectToWorldDir(i.tDirOS.xyz);
                o.bDirWS = cross(o.nDirWS, o.tDirWS) * i.tDirOS.w;

                //UV
                o.uv_Main = TRANSFORM_TEX(i.uv0, _MainTex);
                o.uv_Fur = _FurScale * i.uv0;

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                //采样
                float4 var_MainTex = tex2D(_MainTex, i.uv_Main);
                float4 var_FurMask = tex2D(_FurMask, i.uv_Fur);

                //法线转换
                float3 nDirTS = 2 * var_FurMask.xyz - 1;
                nDirTS.xy *= _NormalInt;
                float3x3 TBN = { normalize(i.tDirWS),normalize(i.bDirWS) ,normalize(i.nDirWS) };

                //向量
                float3 nDirWS = normalize(mul(nDirTS, TBN));
                float3 lDirWS = GetMainLight().direction;

                //光照
                float lambert = saturate(dot(nDirWS, lDirWS));
                float3 diffuseCol = var_MainTex.rgb * lerp(_DarkCol, _BrightCol, lambert);

                //A0
                float height = var_FurMask.a;
                float ao = lerp(1, height, _AOInt);

                //锥化
                float coneMask = step(_Fur_Height01_GLB, pow(height, _ConePow));

                //高度-透明度衰减
                float heightOpacityAtten = pow(saturate(1.001 - _Fur_Height01_GLB), _OpacityPow);

                //混合
                float3 finalcCol = diffuseCol * ao;
                float opacity = coneMask * var_MainTex.a * heightOpacityAtten;
                clip(opacity - 0.001);

                return float4(finalcCol, opacity);
            }
            ENDHLSL
        }
    }
}