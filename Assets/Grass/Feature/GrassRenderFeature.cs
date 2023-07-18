using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrassRenderFeature : ScriptableRendererFeature
{

    public Material grassMaterial;
    class GrassRenderPass : ScriptableRenderPass
    {

        CommandBuffer m_CommandBuffer;
        Mesh m_GrassMesh;
        Material m_GrassMaterial;
        private const string m_CommandBufferName = "Grass";
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_CommandBuffer = CommandBufferPool.Get(m_CommandBufferName);
            if (m_CommandBuffer == null) return;
            foreach (GrassRender grassRender in GrassRender.Instance.grassRenders)
            {
                grassRender.SetMaterialProperties();
                m_CommandBuffer.DrawMeshInstancedProcedural(m_GrassMesh, 0, m_GrassMaterial, 0, grassRender.m_GrassCount, GrassRender.Instance.materialPropertyBlock);
            }
            context.ExecuteCommandBuffer(m_CommandBuffer);
            m_CommandBuffer.Release();
            //
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public void SetUp(Material grassMaterial)
        {
            m_GrassMesh = GrassRender.Instance.grassMesh;
            m_GrassMaterial = grassMaterial;
        }
    }

    GrassRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new GrassRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;

    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.SetUp(grassMaterial);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


