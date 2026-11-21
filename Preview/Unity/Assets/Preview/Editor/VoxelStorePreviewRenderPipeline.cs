using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VoxelStore.Preview.Editor
{
    [InitializeOnLoad]
    internal static class VoxelStorePreviewRenderPipeline
    {
        internal const string AssetPath =
            "Assets/Preview/Settings/VoxelStorePreviewURP.asset";
        internal const string RendererPath =
            "Assets/Preview/Settings/VoxelStorePreviewRenderer.asset";

        static VoxelStorePreviewRenderPipeline()
        {
            EditorApplication.delayCall += () => EnsureConfigured();
        }
        internal static UniversalRenderPipelineAsset EnsureConfigured()
        {
            UniversalRenderPipelineAsset asset =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                    AssetPath);

            if (asset == null)
            {
                string settingsDirectory =
                    Path.Combine(Application.dataPath, "Preview", "Settings");
                Directory.CreateDirectory(settingsDirectory);

                asset = UniversalRenderPipelineAsset.Create();
                AssetDatabase.CreateAsset(asset, AssetPath);

                ScriptableRendererData renderer = asset.LoadBuiltinRendererData();
                string generatedRendererPath = AssetDatabase.GetAssetPath(renderer);
                if (!string.IsNullOrWhiteSpace(generatedRendererPath) &&
                    !string.Equals(
                        generatedRendererPath,
                        RendererPath,
                        StringComparison.Ordinal))
                {
                    string moveError =
                        AssetDatabase.MoveAsset(
                            generatedRendererPath,
                            RendererPath);
                    if (!string.IsNullOrWhiteSpace(moveError))
                        throw new InvalidOperationException(moveError);
                }

                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            GraphicsSettings.defaultRenderPipeline = asset;
            QualitySettings.renderPipeline = asset;
            return asset;
        }

        public static void EnsureForBatch()
        {
            EnsureConfigured();
            AssetDatabase.SaveAssets();
        }
    }
}
