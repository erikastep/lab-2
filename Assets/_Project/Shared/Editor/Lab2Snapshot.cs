using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Lab2.EditorTools
{
    /// <summary>Renders the main camera of the Lab 2 scene to a PNG (for the report).</summary>
    public static class Lab2Snapshot
    {
        [MenuItem("Lab2/Save Scene Snapshot")]
        public static void Snap()
        {
            EditorSceneManager.OpenScene(Lab2SceneBuilder.ScenePath);
            EditorSettings.asyncShaderCompilation = false;
            ShaderUtil.allowAsyncCompilation = false;
            var cam = Camera.main;
            cam.Render();
            var rt = new RenderTexture(1600, 1000, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            System.IO.File.WriteAllBytes("Lab2_scene_snapshot.png", tex.EncodeToPNG());
            Debug.Log("[Lab2Snapshot] saved Lab2_scene_snapshot.png");
        }

        public static void SnapBatch() { Snap(); EditorApplication.Exit(0); }
    }
}
