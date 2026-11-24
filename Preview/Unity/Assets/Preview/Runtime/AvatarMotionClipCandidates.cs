using System;
using System.Collections.Generic;
using System.Globalization;

namespace VoxelStore.Preview.Runtime
{
    public static class AvatarMotionClipCandidates
    {
        public static string NormalizeVariant(string variant)
        {
            if (string.IsNullOrWhiteSpace(variant))
                return string.Empty;

            if (variant.Length < 2 ||
                variant[0] != 'V' ||
                !int.TryParse(
                    variant.Substring(1),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out int number) ||
                number <= 0 ||
                !string.Equals(
                    variant,
                    "V" + number.ToString(CultureInfo.InvariantCulture),
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Animation variant must match canonical V<n> with n > 0.",
                    nameof(variant));
            }

            return variant;
        }

        public static string[] Build(
            AvatarMotion motion,
            EquipmentSlotSide? side,
            string variant)
        {
            string baseName = motion.ToString();
            string normalizedVariant = NormalizeVariant(variant);
            var candidates = new List<string>(4);

            if (side.HasValue && normalizedVariant.Length > 0)
            {
                candidates.Add(
                    baseName + "." + side.Value + "." + normalizedVariant);
            }

            if (normalizedVariant.Length > 0)
                candidates.Add(baseName + "." + normalizedVariant);

            if (side.HasValue)
                candidates.Add(baseName + "." + side.Value);

            candidates.Add(baseName);
            return candidates.ToArray();
        }
    }
}
