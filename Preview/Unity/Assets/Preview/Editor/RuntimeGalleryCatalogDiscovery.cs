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
                if (!TryDescribeAssetPath(
                        assetPath,
                        out string catalog,
                        out string modelName))
                {
                    continue;
                }

                string identity = catalog + "/" + modelName;
                if (!identities.Add(identity))
                {
                    throw new InvalidOperationException(
                        "Runtime gallery contains duplicate imported model identity: " +
                        identity);
                }

                models.Add(
                    new RuntimeGalleryModel
                    {
                        Catalog = catalog,
                        Name = modelName,
                        AssetPath = assetPath
                    });
            }

            return models
                .OrderBy(x => x.Catalog, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
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
