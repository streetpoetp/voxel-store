using System;
using System.Collections.Generic;
using System.Linq;
using Hero.Voxel.Runtime.Assets;
using UnityEditor;

namespace VoxelStore.Preview.Editor
{
    internal sealed class RuntimeGalleryModel
    {
        public string Catalog;
        public string Name;
        public string AssetPath;
        public string AnimationAssetPath;
    }

    internal static class RuntimeGalleryCatalogDiscovery
    {
        internal static RuntimeGalleryModel[] Discover(
            string root = null)
        {
            string discoveryRoot = string.IsNullOrWhiteSpace(root)
                ? VoxelStorePreviewPaths.BakedRoot
                : root.Replace('\\', '/').TrimEnd('/');

            if (!AssetDatabase.IsValidFolder(discoveryRoot))
                return Array.Empty<RuntimeGalleryModel>();

            string[] guids = AssetDatabase.FindAssets(
                "t:VoxelAssemblyAsset",
                new[] { discoveryRoot });
            var models = new List<RuntimeGalleryModel>(guids.Length);
            var identities = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string catalog;
                string modelName;
                string animationAssetPath;

                bool described =
                    TryDescribeProviderAssetPath(
                        discoveryRoot,
                        assetPath,
                        out catalog,
                        out modelName,
                        out animationAssetPath);
                if (!described &&
                    TryDescribeAssetPath(
                        assetPath,
                        out catalog,
                        out modelName))
                {
                    animationAssetPath =
                        assetPath.Substring(
                            0,
                            assetPath.Length - ".asset".Length) +
                        ".animation.asset";
                    described = true;
                }

                if (!described ||
                    AssetDatabase.LoadAssetAtPath<VoxelAnimationAsset>(
                        animationAssetPath) == null)
                {
                    continue;
                }

                string identity = catalog + "/" + modelName;
                if (!identities.Add(identity))
                {
                    throw new InvalidOperationException(
                        "Runtime gallery contains duplicate paired model identity: " +
                        identity);
                }

                models.Add(
                    new RuntimeGalleryModel
                    {
                        Catalog = catalog,
                        Name = modelName,
                        AssetPath = assetPath,
                        AnimationAssetPath = animationAssetPath
                    });
            }

            return models
                .OrderBy(x => x.Catalog, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        internal static bool TryDescribeProviderAssetPath(
            string discoveryRoot,
            string assetPath,
            out string catalog,
            out string modelName,
            out string animationAssetPath)
        {
            catalog = null;
            modelName = null;
            animationAssetPath = null;

            if (string.IsNullOrWhiteSpace(discoveryRoot) ||
                string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            string normalizedRoot = discoveryRoot
                .Replace('\\', '/')
                .TrimEnd('/');
            string normalizedPath = assetPath
                .Replace('\\', '/');
            const string assemblySuffix = ".assembly.asset";

            if (!normalizedPath.EndsWith(
                    assemblySuffix,
                    StringComparison.OrdinalIgnoreCase) ||
                !normalizedPath.StartsWith(
                    normalizedRoot + "/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string relative =
                normalizedPath.Substring(
                    normalizedRoot.Length + 1);
            string[] segments = relative.Split(
                new[] { '/' },
                StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length != 3)
                return false;

            catalog = segments[0];
            modelName = segments[1];
            if (!string.Equals(
                    segments[2],
                    modelName + assemblySuffix,
                    StringComparison.OrdinalIgnoreCase))
            {
                catalog = null;
                modelName = null;
                return false;
            }

            animationAssetPath =
                normalizedPath.Substring(
                    0,
                    normalizedPath.Length -
                    assemblySuffix.Length) +
                ".animation.asset";
            return true;
        }

        internal static bool TryDescribeAssetPath(
            string assetPath,
            out string catalog,
            out string modelName)
        {
            catalog = null;
            modelName = null;

            if (string.IsNullOrWhiteSpace(assetPath))
                return false;

            string[] segments = assetPath
                .Replace('\\', '/')
                .Split(
                    new[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 4)
                return false;

            int fileIndex = segments.Length - 1;
            int sourceIndex = fileIndex - 1;
            int modelIndex = sourceIndex - 1;
            int catalogIndex = modelIndex - 1;

            if (catalogIndex < 0 ||
                !string.Equals(
                    segments[sourceIndex],
                    "Source",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            catalog = segments[catalogIndex];
            modelName = segments[modelIndex];
            return !string.IsNullOrWhiteSpace(catalog) &&
                   !string.IsNullOrWhiteSpace(modelName);
        }

        internal static VoxelAssemblyAsset LoadAssembly(
            RuntimeGalleryModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(model.AssetPath))
            {
                throw new InvalidOperationException(
                    "Runtime gallery model has no asset path: " +
                    model.Catalog +
                    "/" +
                    model.Name);
            }

            VoxelAssemblyAsset assembly =
                AssetDatabase.LoadAssetAtPath<VoxelAssemblyAsset>(
                    model.AssetPath);
            if (assembly == null)
            {
                throw new InvalidOperationException(
                    "Runtime gallery could not load imported voxel assembly: " +
                    model.AssetPath);
            }

            return assembly;
        }

        internal static VoxelAnimationAsset LoadAnimation(
            RuntimeGalleryModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(model.AnimationAssetPath))
            {
                throw new InvalidOperationException(
                    "Runtime gallery model has no animation asset path: " +
                    model.Catalog +
                    "/" +
                    model.Name);
            }

            VoxelAnimationAsset animation =
                AssetDatabase.LoadAssetAtPath<VoxelAnimationAsset>(
                    model.AnimationAssetPath);
            if (animation == null)
            {
                throw new InvalidOperationException(
                    "Runtime gallery requires the baked authored animation asset: " +
                    model.AnimationAssetPath);
            }

            VoxelAssemblyAsset assembly = LoadAssembly(model);
            if (animation.Assembly != assembly)
            {
                throw new InvalidOperationException(
                    "Runtime gallery animation targets a different assembly: " +
                    model.AnimationAssetPath);
            }

            return animation;
        }

        internal static bool IsCatalog(
            RuntimeGalleryModel model,
            string catalog) =>
            model != null &&
            string.Equals(
                model.Catalog,
                catalog,
                StringComparison.OrdinalIgnoreCase);
    }
}
