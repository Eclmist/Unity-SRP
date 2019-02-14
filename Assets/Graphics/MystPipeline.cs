﻿using UnityEngine;
using UnityEngine.Rendering;

public class MystPipeline : RenderPipeline
{
    const string MYST_DEFAULT_UNLIT = "SRPDefaultUnlit";

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

        // Sends culling instructions to context
        CullingResults cullingResults = context.Cull(ref cullingParams);

        // Sets up camera specific global shader params
        context.SetupCameraProperties(camera);

        // Explicitly clear the render target with command buffers
        CameraClearFlags clearFlags = camera.clearFlags;
        CommandBuffer buffer = new CommandBuffer
        { 
            name = camera.name
        };

        buffer.ClearRenderTarget(
            (clearFlags & CameraClearFlags.Depth) != 0,
            (clearFlags & CameraClearFlags.Color) != 0,
            camera.backgroundColor
        );
        context.ExecuteCommandBuffer(buffer);
        buffer.Release();

        // Setup default shaders for drawing
        DrawingSettings drawingSettings = new DrawingSettings();
        drawingSettings.SetShaderPassName(0, new ShaderTagId(MYST_DEFAULT_UNLIT));

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

        // Submit render loop for execution
        context.Submit();
    }
}
