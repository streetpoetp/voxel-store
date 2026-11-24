using System;
using System.Linq;
using Hero.Voxel.Render.Runtime.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor
{
    [InitializeOnLoad]
    internal static class VoxelStoreRuntimeGalleryEditModeRestorer
    {
        static VoxelStoreRuntimeGalleryEditModeRestorer()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.delayCall += RestoreActiveGallery;
        }

        internal static void Restore(Scene scene)
        {
            if (Application.isPlaying ||
                !scene.IsValid() ||
                !scene.isLoaded ||
                !string.Equals(
                    scene.path,
                    VoxelStoreRuntimeGalleryBuilder.ScenePath,
                    StringComparison.Ordinal))
            {
                return;
            }

            GameObject root = scene.GetRootGameObjects()
                .FirstOrDefault(x =>
                    string.Equals(
                        x.name,
                        VoxelStoreRuntimeGalleryBuilder.RootName,
                        StringComparison.Ordinal));
            if (root == null)
            {
                throw new InvalidOperationException(
                    "Runtime gallery scene is missing its canonical root.");
            }

            VoxelRenderWorld world = root.GetComponent<VoxelRenderWorld>();
            if (world == null)
            {
                throw new InvalidOperationException(
                    "Runtime gallery scene is missing VoxelRenderWorld.");
            }

            world.Initialize();

            AvatarPresentation[] avatars =
                root.GetComponentsInChildren<AvatarPresentation>(true);
            foreach (AvatarPresentation avatar in avatars)
            {
                if (avatar != null && avatar.IsConfigured)
                    avatar.InitializeProvider();
            }

            VoxelRenderPresentation[] presentations =
                root.GetComponentsInChildren<VoxelRenderPresentation>(true);
            foreach (VoxelRenderPresentation presentation in presentations)
            {
                if (presentation != null &&
                    !presentation.ExternallyOwnedLifecycle)
                {
                    presentation.EnsureInstance();
                }
            }

            world.SubmitNow();
            SceneView.RepaintAll();
        }

        private static void OnSceneOpened(
            Scene scene,
            OpenSceneMode mode)
        {
            if (!string.Equals(
                    scene.path,
                    VoxelStoreRuntimeGalleryBuilder.ScenePath,
                    StringComparison.Ordinal))
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (scene.IsValid() && scene.isLoaded)
                    Restore(scene);
            };
        }

        private static void RestoreActiveGallery()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.IsValid() && scene.isLoaded)
                Restore(scene);
        }
    }
}
