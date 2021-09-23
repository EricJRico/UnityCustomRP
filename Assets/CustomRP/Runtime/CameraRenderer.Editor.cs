using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

partial class CameraRenderer
{
    partial void drawUnsupportedShaders();
    partial void drawGizmos();
    partial void prepareForSceneWindow();
    partial void prepareBuffer();

#if UNITY_EDITOR
    static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    static Material errorMaterial;
    string sampleName { get; set; }

    partial void drawUnsupportedShaders()
    {
        if (errorMaterial == null)
        {
            errorMaterial =
                new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawingSettings = new DrawingSettings(
            legacyShaderTagIds[0], new SortingSettings(camera)
        )
        {
            overrideMaterial = errorMaterial
        };

        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }

        var filteringSettings = FilteringSettings.defaultValue;
        scriptableRenderContext.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }

    partial void drawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            scriptableRenderContext.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            scriptableRenderContext.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }

    partial void prepareForSceneWindow()
    {
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
    }

    partial void prepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        commandBuffer.name = sampleName = camera.name;
        Profiler.EndSample();
    }

#else

	string sampleName => bufferName;
#endif
}
