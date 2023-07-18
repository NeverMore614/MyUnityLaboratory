using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FurFeature : ScriptableRendererFeature
{
    [Header("【渲染时机】")]
    [Space(20)]
    public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingSkybox;//毛发渲染发生的时机

    [Header("【过滤】")]
    [Space(20)]
    public RenderQueueType queue = RenderQueueType.Opaque;//场景物体的渲染队列在该范围内时渲染
    public enum RenderQueueType
    {
        Opaque,
        Transparent,
    };
    public LayerMask layerMask;//只对此层内的物体附加毛发渲染
    public string[] lightModeTags;//只对Shader Tag在该列表中的材质渲染

    [Header("【材质】")]
    [Space(20)]
    public Material furMat;
    public int passId;//使用材质对应shader中的第passId个Pass进行渲染

    [Header("【材质参数】")]
    [Range(2, 100)] public int step = 10;//层数

    //
    class FurPass : ScriptableRenderPass
    {
        public int step;
        public FilteringSettings filteringSettings;
        private RenderQueueType renderQueueType;
        private List<ShaderTagId> tagIds;
        private Material overrideMaterial;
        private int overrideMaterialPassIndex;

        //构造函数
        public FurPass(int step, RenderPassEvent renderPassEvent, RenderQueueType renderQueueType, int layerMask, string[] lightModeTags, Material overrideMaterial, int overrideMaterialPassIndex)
        {
            this.step = step;
            this.renderPassEvent = renderPassEvent;
            this.renderQueueType = renderQueueType;
            this.overrideMaterial = overrideMaterial;
            this.overrideMaterialPassIndex = overrideMaterialPassIndex;

            //渲染队列过滤配置
            RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
                ? RenderQueueRange.transparent
                : RenderQueueRange.opaque;
            filteringSettings = new FilteringSettings(renderQueueRange, layerMask);

            //Shader Tag过滤配置
            tagIds = new List<ShaderTagId>();
            if (lightModeTags != null && lightModeTags.Length > 0)
            {
                for (int i = 0; i < lightModeTags.Length; i++)
                {
                    tagIds.Add(new ShaderTagId(lightModeTags[i]));
                }
            }
            else
            {
                //管线默认的Tag
                tagIds.Add(new ShaderTagId("SRPDefaultUnlit"));
                tagIds.Add(new ShaderTagId("UniversalForward"));
                tagIds.Add(new ShaderTagId("UniversalForwardOnly"));
                tagIds.Add(new ShaderTagId("LightweightForward"));
            }
        }

        ////——2
        //public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        //{
        //}

        //核心渲染逻辑——3
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //排序层配置
            SortingCriteria sortingCriteria = (renderQueueType == RenderQueueType.Transparent)
                ? SortingCriteria.CommonTransparent
                : renderingData.cameraData.defaultOpaqueSortFlags;

            //材质配置
            DrawingSettings drawingSettings = CreateDrawingSettings(tagIds, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = overrideMaterial;
            drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;

            //绘制
            CommandBuffer cmd = CommandBufferPool.Get();
            for (int i = 0; i < step; i++)
            {
                cmd.SetGlobalFloat("_Fur_Height01_GLB", i / (step - 1.0f));
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            }
            CommandBufferPool.Release(cmd);
        }


        ////——4
        //public override void OnCameraCleanup(CommandBuffer cmd)
        //{
        //}
    }

    FurPass furPass;

    //配置相关参数——0
    public override void Create()
    {
        furPass = new FurPass(step, passEvent, queue, layerMask, lightModeTags, furMat, passId);
    }

    //将自定义效果加入渲染队列——1
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(furPass);
    }
}