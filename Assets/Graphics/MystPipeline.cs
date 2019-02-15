using System;
using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute; // basically compile time #ifdef

public class MystPipeline : RenderPipeline
{
    // Shader identifiers
    protected const string MYST_SHADERID_DEFAULT_UNLIT = "SRPDefaultUnlit";
    protected const string MYST_SHADERID_DEFAULT_FORWARD = "ForwardBase";
    protected const string MYST_SHADER_ERROR = "Hidden/InternalErrorShader";

    // Materials
    protected Material errorMaterial;

    protected CommandBuffer cameraBuffer = new CommandBuffer
    {
        name = "Render Camera"
    };

    protected CullingResults cullingResults;

    protected PipelineFlags currentPipelineFlags;

    [Flags]
    public enum PipelineFlags
    {
        None = 0,
        DynamicBatching = 1,
        Instancing = 2,
    }

    public MystPipeline (PipelineFlags flags)
    {
        currentPipelineFlags = flags;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            Render(context, camera);
        }
    }

    // Temporarily only support one camera, will render the same thing for the others.
    // TODO: Add support for multi-camera setups
    protected void Render(ScriptableRenderContext context, Camera camera)
    {
        // Culling
        ScriptableCullingParameters cullingParams;

        // TryGetCullingParameters return false if it failed to create valid params
        if (!camera.TryGetCullingParameters(out cullingParams))
        {
            return;
        }

        // Inject world space UI into scene view
#if UNITY_EDITOR
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
#endif

        // Sends culling instructions to context
        cullingResults = context.Cull(ref cullingParams);

        // Sets up camera specific global shader params
        context.SetupCameraProperties(camera);

        // Explicitly clear the render target with command buffers
        CameraClearFlags clearFlags = camera.clearFlags;

        cameraBuffer.ClearRenderTarget(
            (clearFlags & CameraClearFlags.Depth) != 0,
            (clearFlags & CameraClearFlags.Color) != 0,
            camera.backgroundColor
        );
        cameraBuffer.BeginSample("Render Camera");
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();

        // Setup default shaders for drawing
        DrawingSettings drawingSettings = new DrawingSettings();
        drawingSettings.SetShaderPassName(0, new ShaderTagId(MYST_SHADERID_DEFAULT_UNLIT));

        drawingSettings.enableDynamicBatching = (currentPipelineFlags & PipelineFlags.DynamicBatching) != 0;
        drawingSettings.enableInstancing      = (currentPipelineFlags & PipelineFlags.Instancing)      != 0;

        // Setup default sort mode
        SortingSettings sortingSettings = new SortingSettings(camera);
        drawingSettings.sortingSettings = sortingSettings;

        // Filters objects to draw different stuff in each pass
        FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all);

        // Setup sort mode for opaque (front to back)
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        drawingSettings.sortingSettings = sortingSettings;

        // Render opaque pass
        filterSettings.renderQueueRange = RenderQueueRange.opaque;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);

        // Sends instructions to draw skybox
        context.DrawSkybox(camera);

        // Setup sort mode for transparent (back to front)
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;

        // Render transparent pass
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);

        // Fallback for everything else
        DrawDefaultPipeline(context, camera);

        cameraBuffer.EndSample("Render Camera");
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();

        // Submit render loop for execution
        context.Submit();
    }

    // Fallback for when something isn't supported
    [Conditional("DEVELPOMENT_BUILD"), Conditional("UNITY_EDITOR")]
    protected void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera)
    {
        if (errorMaterial == null)
        {
            Shader errorShader = Shader.Find(MYST_SHADER_ERROR);
            errorMaterial = new Material(errorShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        DrawingSettings drawingSettings = new DrawingSettings();
        drawingSettings.SetShaderPassName(0, new ShaderTagId("ForwardBase"));
        drawingSettings.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
        drawingSettings.SetShaderPassName(2, new ShaderTagId("Always"));
        drawingSettings.SetShaderPassName(3, new ShaderTagId("Vertex"));
        drawingSettings.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
        drawingSettings.SetShaderPassName(5, new ShaderTagId("VertexLM"));
        drawingSettings.overrideMaterial = errorMaterial;
        drawingSettings.overrideMaterialPassIndex = 0;

        FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all);
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
    }
}
