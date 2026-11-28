using System;
using System.Collections.Generic;
using System.Linq;
using Hero.Voxel.Runtime.Assets;

namespace VoxelStore.Preview.Editor
{
    internal static class PreviewVariantCoverage
    {
        internal static void ValidateAll(
            IEnumerable<BakedPreviewUnit> bakedUnits)
        {
            foreach (BakedPreviewUnit baked in
                     bakedUnits ?? Array.Empty<BakedPreviewUnit>())
            {
                if (baked?.Unit == null || baked.Animation == null)
                    throw new InvalidOperationException(
                        "Preview baked animation validation received an incomplete unit.");

                var actual = Enumerable
                    .Range(0, baked.Animation.ClipCount)
                    .Select(i => baked.Animation.GetClip(i))
                    .Select(clip => (clip.Name, clip.Loop))
                    .ToArray();
                ValidateInventory(baked.Unit, actual);
            }
        }

        internal static void ValidateInventory(
            PreviewUnitContract unit,
            IEnumerable<(string Name, bool Loop)> actualClips)
        {
            if (unit == null || unit.RequiredClips == null)
                throw new ArgumentNullException(nameof(unit));

            var actual = (actualClips ??
                          Array.Empty<(string Name, bool Loop)>())
                .ToDictionary(
                    clip => clip.Name,
                    clip => clip.Loop,
                    StringComparer.Ordinal);
            var expected = unit.RequiredClips
                .ToDictionary(
                    clip => clip.Name,
                    clip => clip.Loop,
                    StringComparer.Ordinal);

            string[] missing = expected.Keys
                .Where(name => !actual.ContainsKey(name))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            string[] unexpected = actual.Keys
                .Where(name => !expected.ContainsKey(name))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            string[] wrongLoop = expected
                .Where(pair =>
                    actual.TryGetValue(
                        pair.Key,
                        out bool loop) &&
                    loop != pair.Value)
                .Select(pair => pair.Key)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            if (missing.Length == 0 &&
                unexpected.Length == 0 &&
                wrongLoop.Length == 0)
                return;

            throw new InvalidOperationException(
                "Preview baked animation inventory differs from the " +
                "Tool-resolved profile for " +
                unit.Id +
                ": missing=[" +
                string.Join(", ", missing) +
                "] unexpected=[" +
                string.Join(", ", unexpected) +
                "] wrongLoop=[" +
                string.Join(", ", wrongLoop) +
                "]");
        }
    }
}
