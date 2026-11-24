using System.Globalization;

namespace VoxelStore.Tool;

internal sealed record SemanticAnimationClipIdentity(
    string Motion,
    string? Side,
    string? Variant);

public sealed class SemanticMotionCoverage
{
    public required string Motion { get; init; }
    public bool Base { get; set; }
    public List<string> Sides { get; } = new();
    public List<string> Variants { get; } = new();
    public Dictionary<string, List<string>> SideVariants { get; } =
        new(StringComparer.Ordinal);
}

internal static class SemanticAnimationClipName
{
    internal static readonly string[] RequiredMotions =
    {
        "Idle",
        "WalkForward",
        "Attack",
        "Hit",
        "Death"
    };

    private static readonly HashSet<string> MotionSet =
        new(RequiredMotions, StringComparer.Ordinal);
    private static readonly HashSet<string> SideSet =
        new(StringComparer.Ordinal) { "Left", "Right", "Center" };

    internal static SemanticAnimationClipIdentity Parse(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidDataException("Semantic animation clip name is required.");

        string[] parts = name.Split('.');
        if (parts.Length is < 1 or > 3 || !MotionSet.Contains(parts[0]))
        {
            throw new InvalidDataException(
                $"Semantic animation clip '{name}' must match <Motion>, <Motion>.<Side>, <Motion>.V<n>, or <Motion>.<Side>.V<n>.");
        }

        string motion = parts[0];
        string? side = null;
        string? variant = null;

        if (parts.Length >= 2)
        {
            if (SideSet.Contains(parts[1]))
            {
                side = parts[1];
                if (parts.Length == 3)
                    variant = RequireVariant(parts[2], name);
            }
            else
            {
                if (parts.Length != 2)
                {
                    throw new InvalidDataException(
                        $"Semantic animation clip '{name}' has an unsupported qualifier order.");
                }

                variant = RequireVariant(parts[1], name);
            }
        }

        return new SemanticAnimationClipIdentity(motion, side, variant);
    }

    internal static string Format(
        string motion,
        string? side = null,
        string? variant = null)
    {
        RequireMotion(motion);
        side = NormalizeSide(side);
        variant = NormalizeVariant(variant);

        if (side != null && variant != null)
            return motion + "." + side + "." + variant;
        if (variant != null)
            return motion + "." + variant;
        if (side != null)
            return motion + "." + side;
        return motion;
    }

    internal static IReadOnlyList<string> BuildCandidates(
        string motion,
        string? side = null,
        string? variant = null)
    {
        RequireMotion(motion);
        side = NormalizeSide(side);
        variant = NormalizeVariant(variant);

        var candidates = new List<string>(4);
        if (side != null && variant != null)
            candidates.Add(Format(motion, side, variant));
        if (variant != null)
            candidates.Add(Format(motion, variant: variant));
        if (side != null)
            candidates.Add(Format(motion, side));
        candidates.Add(motion);
        return candidates;
    }

    internal static bool ShouldLoop(string motion)
    {
        RequireMotion(motion);
        return motion is "Idle" or "WalkForward";
    }

    internal static IReadOnlyList<SemanticMotionCoverage> BuildCoverage(
        IEnumerable<string> clipNames)
    {
        var coverage = new Dictionary<string, SemanticMotionCoverage>(
            StringComparer.Ordinal);

        foreach (string clipName in clipNames.OrderBy(x => x, StringComparer.Ordinal))
        {
            SemanticAnimationClipIdentity identity = Parse(clipName);
            if (!coverage.TryGetValue(identity.Motion, out SemanticMotionCoverage? motion))
            {
                motion = new SemanticMotionCoverage { Motion = identity.Motion };
                coverage.Add(identity.Motion, motion);
            }

            if (identity.Side == null && identity.Variant == null)
            {
                motion.Base = true;
            }
            else if (identity.Side != null && identity.Variant == null)
            {
                AddDistinct(motion.Sides, identity.Side);
            }
            else if (identity.Side == null)
            {
                AddDistinct(motion.Variants, identity.Variant!);
            }
            else
            {
                if (!motion.SideVariants.TryGetValue(
                        identity.Side,
                        out List<string>? variants))
                {
                    variants = new List<string>();
                    motion.SideVariants.Add(identity.Side, variants);
                }

                AddDistinct(variants, identity.Variant!);
            }
        }

        foreach (SemanticMotionCoverage motion in coverage.Values)
        {
            motion.Sides.Sort(SideCompare);
            motion.Variants.Sort(VariantCompare);
            foreach (List<string> variants in motion.SideVariants.Values)
                variants.Sort(VariantCompare);
        }

        return coverage.Values
            .OrderBy(x => x.Motion, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddDistinct(List<string> values, string value)
    {
        if (!values.Contains(value, StringComparer.Ordinal))
            values.Add(value);
    }

    private static int SideCompare(string left, string right) =>
        SideOrder(left).CompareTo(SideOrder(right));

    private static int SideOrder(string side) => side switch
    {
        "Left" => 0,
        "Right" => 1,
        "Center" => 2,
        _ => 3
    };

    private static int VariantCompare(string left, string right) =>
        VariantNumber(left).CompareTo(VariantNumber(right));

    private static int VariantNumber(string variant) =>
        int.Parse(variant.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture);

    private static void RequireMotion(string motion)
    {
        if (!MotionSet.Contains(motion))
        {
            throw new InvalidDataException(
                $"Unsupported semantic animation motion '{motion}'.");
        }
    }

    private static string? NormalizeSide(string? side)
    {
        if (string.IsNullOrWhiteSpace(side))
            return null;
        if (!SideSet.Contains(side))
        {
            throw new InvalidDataException(
                $"Unsupported semantic animation side '{side}'.");
        }

        return side;
    }

    private static string? NormalizeVariant(string? variant)
    {
        if (string.IsNullOrWhiteSpace(variant))
            return null;
        return RequireVariant(variant, variant);
    }

    private static string RequireVariant(string variant, string clipName)
    {
        if (variant.Length < 2 ||
            variant[0] != 'V' ||
            !int.TryParse(
                variant.AsSpan(1),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int number) ||
            number <= 0 ||
            !string.Equals(
                variant,
                "V" + number.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Semantic animation clip '{clipName}' variant must match canonical V<n> with n > 0.");
        }

        return variant;
    }
}
