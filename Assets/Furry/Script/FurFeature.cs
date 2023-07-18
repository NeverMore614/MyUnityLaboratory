using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FurFeature : ScriptableRendererFeature
{
    [Header("����Ⱦʱ����")]
    [Space(20)]
    public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingSkybox;//ë����Ⱦ������ʱ��

    [Header("�����ˡ�")]
    [Space(20)]
    public RenderQueueType queue = RenderQueueType.Opaque;//�����������Ⱦ�����ڸ÷�Χ��ʱ��Ⱦ
    public enum RenderQueueType
    {
        Opaque,
        Transparent,
    };
    public LayerMask layerMask;//ֻ�Դ˲��ڵ����帽��ë����Ⱦ
    public string[] lightModeTags;//ֻ��Shader Tag�ڸ��б��еĲ�����Ⱦ

    [Header("�����ʡ�")]
    [Space(20)]
    public Material furMat;
    public int passId;//ʹ�ò��ʶ�Ӧshader�еĵ�passId��Pass������Ⱦ

    [Header("�����ʲ�����")]
    [Range(2, 100)] public int step = 10;//����

    //
    class FurPass : ScriptableRenderPass
    {
        public int step;
        public FilteringSettings filteringSettings;
        private RenderQueueType renderQueueType;
        private List<ShaderTagId> tagIds;
        private Material overrideMaterial;
        private int overrideMaterialPassIndex;

        //���캯��
        public FurPass(int step, RenderPassEvent renderPassEvent, RenderQueueType renderQueueType, int layerMask, string[] lightModeTags, Material overrideMaterial, int overrideMaterialPassIndex)
        {
            this.step = step;
            this.renderPassEvent = renderPassEvent;
            this.renderQueueType = renderQueueType;
            this.overrideMaterial = overrideMaterial;
            this.overrideMaterialPassIndex = overrideMaterialPassIndex;

            //��Ⱦ���й�������
            RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
                ? RenderQueueRange.transparent
                : RenderQueueRange.opaque;
            filteringSettings = new FilteringSettings(renderQueueRange, layerMask);

            //Shader Tag��������
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
                //����Ĭ�ϵ�Tag
                tagIds.Add(new ShaderTagId("SRPDefaultUnlit"));
                tagIds.Add(new ShaderTagId("UniversalForward"));
                tagIds.Add(new ShaderTagId("UniversalForwardOnly"));
                tagIds.Add(new ShaderTagId("LightweightForward"));
            }
        }

        ////����2
        //public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        //{
        //}

        //������Ⱦ�߼�����3
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //���������
            SortingCriteria sortingCriteria = (renderQueueType == RenderQueueType.Transparent)
                ? SortingCriteria.CommonTransparent
                : renderingData.cameraData.defaultOpaqueSortFlags;

            //��������
            DrawingSettings drawingSettings = CreateDrawingSettings(tagIds, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = overrideMaterial;
            drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;

            //����
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


        ////����4
        //public override void OnCameraCleanup(CommandBuffer cmd)
        //{
        //}
    }

    FurPass furPass;

    //������ز�������0
    public override void Create()
    {
        furPass = new FurPass(step, passEvent, queue, layerMask, lightModeTags, furMat, passId);
    }

    //���Զ���Ч��������Ⱦ���С���1
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(furPass);
    }
}