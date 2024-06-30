using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
 
// Make sure script name matches the name of the class,
// I am using "MyCustomRenderPassFeature" instead of "CustomRenderPassFeature" here
public class MyCustomRenderPassFeature : ScriptableRendererFeature
{
  [System.Serializable]
  public class Settings
  {
    public Material material = null;
  }
  public Settings settings = new Settings();
 
  MyCustomRenderPass m_ScriptablePass;
 
  public override void Create()
  {
    m_ScriptablePass = new MyCustomRenderPass(settings)
    {
      renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
    };
  }
 
  public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
  {
    // we dont wan't to run the pass if there is no material
    if (settings.material == null)
    {
      return;
    }
    // this will keep the preview cameras (Materials, Prefabs, ect) box in inspectors from being affected
    if (renderingData.cameraData.isPreviewCamera)
    {
      return;
    }
 
    renderer.EnqueuePass(m_ScriptablePass);
  }
 
  class MyCustomRenderPass : ScriptableRenderPass
  {
    // name this what you want, it will be used to name the profile in frame debugger
    const string profilingName = "My Custom Renderer Pass";
 
    // name this whatever you want, it will just be used to make your temp id
    const string destinationName = "_MyCustomTemp";
 
    // store settings instead of material itself
    Settings settings;
 
    // use int as id instead of RenderTargetHandle
    int destinationID;
 
    public RenderTargetIdentifier source;
 
    public MyCustomRenderPass(Settings settings)
    {
      // storing the settings allows you to add more features faster without having to boiler plate code,
      // also ensures that any changes made in the render feature reflect in the pass
      this.settings = settings;
 
      // well get a shader id instead of creating target handle
      this.destinationID = Shader.PropertyToID(destinationName);
 
      // create a new profiling sampler with are chosen name,
      // else you get just a generic "ScriptableRendererPass" name
      this.profilingSampler = new ProfilingSampler(profilingName);
    }
 
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
      // retrieve a temporary RT right before Execute using the destinationID
      cmd.GetTemporaryRT(destinationID, cameraTextureDescriptor);
    }
 
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
      // get the source target from rendering data every frame
      source = renderingData.cameraData.renderer.cameraColorTarget;
    }
 
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
      CommandBuffer cmd = CommandBufferPool.Get();
   
      // create new profiling scope
      // not needed but makes things nice in frame debugger
      using( var profilingScope= new ProfilingScope(cmd, profilingSampler))
      {
        // uncomment the set target portions if you still have issues
        Blit(cmd, source, destinationID, settings.material);
        Blit(cmd, destinationID, source);
 
        // if you still have issues possibly try the code below instead
        // of the blit calls above
        /*
        cmd.SetRenderTarget(destinationID);
        cmd.Blit(source, destinationID, settings.material);
        cmd.SetRenderTarget(source);
        cmd.Blit(destinationID, source);
        */
      }
      // execute CommandBuffer then release it
      context.ExecuteCommandBuffer(cmd);
      CommandBufferPool.Release(cmd);
    }
 
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
      // very important to release temporary RT's after use, all sorts of things can go wrong if you don't
      cmd.ReleaseTemporaryRT(destinationID);
    }
  }
}