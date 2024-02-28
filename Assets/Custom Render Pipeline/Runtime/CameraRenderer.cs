using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private ScriptableRenderContext context;
    private Camera camera;
    private CullingResults cullingResults;

    private const string BUFFER_NAME = "Render Camera";
    private CommandBuffer m_CommandBuffer = new CommandBuffer()
    {
        name = BUFFER_NAME
    };

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    public void Render(ScriptableRenderContext scriptableRenderContext, Camera camera)
    {
        this.context = scriptableRenderContext;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();

        if (!Cull())
            return;

        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }

    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters parameters))
        {
            cullingResults = context.Cull(ref parameters);
            return true;
        }
        return false;
    }

    private void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        m_CommandBuffer.ClearRenderTarget(
                    flags <= CameraClearFlags.Depth,
                    flags == CameraClearFlags.Color,
                    flags == CameraClearFlags.Color ?
                        camera.backgroundColor.linear : Color.clear
                );
        // Setup allows nesting commands under "Render Camera" in frame debugger
        m_CommandBuffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(m_CommandBuffer);
        m_CommandBuffer.Clear();
    }

    private void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        var drawingSettings = new DrawingSettings(
            // Only support unlit at this time
            unlitShaderTagId, sortingSettings
        );

        // if filtering by opaque, transparent won't be drawn
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        // RenderLoop.Draw
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );

        context.DrawSkybox(camera);

        // Set up setting for transparent
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(
        cullingResults, ref drawingSettings, ref filteringSettings
    );
    }

    private void Submit()
    {
        m_CommandBuffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }
}
