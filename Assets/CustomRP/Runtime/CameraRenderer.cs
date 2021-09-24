using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// https://catlikecoding.com/unity/tutorials/custom-srp/custom-render-pipeline/
/// </summary>
public partial class CameraRenderer
{
    private ScriptableRenderContext scriptableRenderContext;
    private Camera camera;
    CullingResults cullingResults;
    const string bufferName = "Render Camera";
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    CommandBuffer commandBuffer = new CommandBuffer
    {
        name = bufferName
    };

    public void Render(
        ScriptableRenderContext scriptableRenderContext,
        Camera camera,
        bool useDynamicBatching,
        bool useGPUInstancing)
    {
        this.scriptableRenderContext = scriptableRenderContext;
        this.camera = camera;

        prepareBuffer();
        prepareForSceneWindow();

        if (!isCulled())
            return;

        setup();
        drawVisibleGeometry(useDynamicBatching, useDynamicBatching);
        drawUnsupportedShaders();
        drawGizmos();
        submit();
    }

    private bool isCulled()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = scriptableRenderContext.Cull(ref p);
            return true;
        }

        return false;
    }

    private void setup()
    {
        scriptableRenderContext.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        commandBuffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
        commandBuffer.BeginSample(sampleName);
        executeBuffer();
    }

    private void drawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };

        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        scriptableRenderContext.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        scriptableRenderContext.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        scriptableRenderContext.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }

    private void submit()
    {
        commandBuffer.EndSample(sampleName);
        executeBuffer();
        scriptableRenderContext.Submit();
    }

    private void executeBuffer()
    {
        scriptableRenderContext.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Clear();
    }
}
