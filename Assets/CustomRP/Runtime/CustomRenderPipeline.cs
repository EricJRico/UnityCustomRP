using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    bool useDynamicBatching, useGPUInstancing;

    public CustomRenderPipeline(
        bool useDynamicBatching, 
        bool useGPUInstancing, 
        bool useSRPBatcher)
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }

    CameraRenderer cameraRenderer = new CameraRenderer();

    protected override void Render(ScriptableRenderContext scriptableRenderContext, Camera[] cameras)
    {
        foreach (var camera in cameras)
            cameraRenderer.Render(
                scriptableRenderContext,
                camera,
                useDynamicBatching,
                useGPUInstancing);
    }
}
