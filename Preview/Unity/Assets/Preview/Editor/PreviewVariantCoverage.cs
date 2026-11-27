using System;
using System.Collections.Generic;
using System.Linq;
using Hero.Voxel.Runtime.Assets;

namespace VoxelStore.Preview.Editor
{
    internal static class PreviewVariantCoverage
    {
        private static readonly string[] AvatarRequired =
        {
            "Idle.V1",
            "WalkForward.V1",
            "Attack.Left.V1",
            "Attack.Right.V1",
            "Hit.V1",
            "Death.V1"
        };

        private static readonly string[] EquipmentRequired =
        {
            "Idle.V1",
            "WalkForward.V1",
            "Attack.V1",
            "Hit.V1",
            "Death.V1"
        };

        internal static IReadOnlyList<string> RequiredClips(
            string catalog)
        {
            if (string.Equals(
                    catalog,
                    "Avatars",
                    StringComparison.OrdinalIgnoreCase))
            {
                return AvatarRequired;
            }

            if (string.Equals(
                    catalog,
                    "Equipment",
                    StringComparison.OrdinalIgnoreCase))
            {
                return EquipmentRequired;
            }

            return Array.Empty<string>();
        }

        internal static string[] MissingClips(
            string catalog,
            IEnumerable<string> clipNames)
        {
            var names = new HashSet<string>(
                clipNames ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            return RequiredClips(catalog)
                .Where(x => !names.Contains(x))
                .ToArray();
        }

        internal static void ValidateAll(
            IEnumerable<PreviewCatalogModel> models)
        {
            foreach (PreviewCatalogModel model in models)
            {
                VoxelAnimationAsset animation =
                    PreviewCatalogDiscovery.LoadAnimation(
                        model);
                string[] clipNames = Enumerable
                    .Range(0, animation.ClipCount)
                    .Select(i => animation.GetClip(i).Name)
                    .ToArray();
                string[] missing =
                    MissingClips(model.Catalog, clipNames);
                if (missing.Length == 0)
                    continue;

                throw new InvalidOperationException(
                    "Preview baked animation coverage is incomplete for " +
                    model.Catalog +
                    "/" +
                    model.Name +
                    ": missing " +
                    string.Join(", ", missing));
            }
        }
    }
}
