using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Myst")]
public class MystPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    protected bool dynamicBatching;

    [SerializeField]
    protected bool instancing;

    protected override RenderPipeline CreatePipeline()
    {
        MystPipeline.PipelineFlags flags = MystPipeline.PipelineFlags.None;

        if (dynamicBatching)
            flags |= MystPipeline.PipelineFlags.DynamicBatching;

        if (instancing)
            flags |= MystPipeline.PipelineFlags.Instancing;

        return new MystPipeline(flags);
    }
}

