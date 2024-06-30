using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlackScreenEffect : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private RTHandle temporaryColorTexture;

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            temporaryColorTexture = RTHandles.Alloc(cameraTextureDescriptor);
            ConfigureTarget(temporaryColorTexture);
            ConfigureClear(ClearFlag.All, Color.black); // Clear to black
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("BlackScreenEffect");

            // Clear the screen to black
            cmd.ClearRenderTarget(true, true, Color.black);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (temporaryColorTexture != null)
            {
                RTHandles.Release(temporaryColorTexture);
            }
        }
    }

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        Debug.Log("black screen create");
        m_ScriptablePass = new CustomRenderPass
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing // Changed event for testing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Debug.Log("black screen add render passes");
        var src = renderer.cameraColorTargetHandle;
        m_ScriptablePass.ConfigureTarget(src); // Configure target to be the camera color target
        renderer.EnqueuePass(m_ScriptablePass);
    }
}