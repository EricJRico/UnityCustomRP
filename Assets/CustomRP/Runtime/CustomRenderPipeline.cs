using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    CameraRenderer cameraRenderer = new CameraRenderer();

    protected override void Render(ScriptableRenderContext scriptableRenderContext, Camera[] cameras)
    {
        foreach (var camera in cameras)
            cameraRenderer.Render(scriptableRenderContext, camera);
    }
}
