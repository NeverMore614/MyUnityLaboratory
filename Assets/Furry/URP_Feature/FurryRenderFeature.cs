using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FurryRenderFeature : ScriptableRendererFeature
{
    public Material furryMaterial;
    class FurryRenderPass : ScriptableRenderPass
    {
        CommandBuffer m_CommandBuffer;
        Material m_furryMaterial;
        private const string m_CommandBufferName = "Furry";


        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_CommandBuffer = CommandBufferPool.Get(m_CommandBufferName);
            if (m_CommandBuffer == null) return;
            foreach (FurryRender furryRender in FurryRender.Instance.furryRenders)
            {
                furryRender.SetMaterialProperties();
                m_CommandBuffer.DrawMeshInstancedProcedural(furryRender.m_Mesh, 0, m_furryMaterial, 0, furryRender.m_PassCount, FurryRender.Instance.materialPropertyBlock);
            }
            context.ExecuteCommandBuffer(m_CommandBuffer);
            m_CommandBuffer.Release();
        }



        public void SetUp(Material furryMaterial)
        {
            m_furryMaterial = furryMaterial;
        }
    }

    FurryRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new FurryRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.SetUp(furryMaterial);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


