using System.Text.RegularExpressions;

namespace VoxelStore.Tool;

/// <summary>Store-owned visual motion policy; never a runtime controller.</summary>
internal sealed class AnimationProfilePolicy
{
    public int Version { get; set; }
    public Dictionary<string, AnimationFamilyPolicy> Families { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, AnimationProfileDefinition> Profiles { get; set; } = new(StringComparer.Ordinal);

    internal static AnimationProfilePolicy Load(string root)
    {
        var policy = ContractYaml.Load<AnimationProfilePolicy>(Path.Combine(root, "Store", "animation-profiles.yaml"));
        policy.Validate();
        return policy;
    }

    internal void Validate()
    {
        if (Version != 1 || Families.Count == 0 || Profiles.Count == 0)
            throw new InvalidDataException("Animation profile policy must use version 1 with nonempty families and profiles.");
        foreach (var (name, family) in Families)
            if (!Regex.IsMatch(name, "^[A-Z][A-Za-z0-9]*$") || family == null || family.Loop == null)
                throw new InvalidDataException($"Invalid animation family policy '{name}': explicit loop rule required.");

        foreach (var (id, profile) in Profiles)
        {
            if (profile == null || profile.Kind is not ("avatar" or "equipment") ||
                !id.StartsWith(profile.Kind + ".", StringComparison.Ordinal) ||
                !Regex.IsMatch(id, "^[a-z]+\\.[a-z][a-z0-9-]*$") ||
                string.IsNullOrWhiteSpace(profile.Brief))
                throw new InvalidDataException($"Invalid animation profile '{id}'.");
            if ((profile.Kind == "equipment" && profile.Forms.Count == 0) ||
                (profile.Kind == "avatar" && profile.Forms.Count != 0) ||
                profile.Forms.Any(string.IsNullOrWhiteSpace) ||
                profile.Forms.Distinct(StringComparer.Ordinal).Count() != profile.Forms.Count)
                throw new InvalidDataException($"Invalid animation profile forms for '{id}'.");
            var motions = profile.Required.Concat(profile.Optional).ToArray();
            if (profile.Required.Count == 0 || motions.Any(m => !Families.ContainsKey(m)) ||
                motions.Distinct(StringComparer.Ordinal).Count() != motions.Length)
                throw new InvalidDataException($"Invalid animation profile families for '{id}'.");
        }
    }

    internal AnimationProfileDefinition Resolve(AssetManifest asset)
    {
        if (string.IsNullOrWhiteSpace(asset.AnimationProfile))
            throw new InvalidDataException($"Asset '{asset.Id}' requires an explicit animationProfile.");
        if (!Profiles.TryGetValue(asset.AnimationProfile, out var profile))
            throw new InvalidDataException($"Unknown animation profile '{asset.AnimationProfile}' for '{asset.Id}'.");
        if (!string.Equals(profile.Kind, asset.Kind, StringComparison.Ordinal) ||
            (profile.Kind == "equipment" && !profile.Forms.Contains(asset.Form ?? "", StringComparer.Ordinal)))
            throw new InvalidDataException($"Animation profile '{asset.AnimationProfile}' is incompatible with kind/form '{asset.Kind}/{asset.Form}'.");
        return profile;
    }

    internal bool LoopFor(string family)
        => Families.TryGetValue(family, out var policy) && policy.Loop.HasValue
            ? policy.Loop.Value
            : throw new InvalidDataException($"Unknown animation family '{family}'.");
}

internal sealed class AnimationFamilyPolicy
{
    public bool? Loop { get; set; }
}

internal sealed class AnimationProfileDefinition
{
    public string Kind { get; set; } = string.Empty;
    public List<string> Forms { get; set; } = new();
    public List<string> Required { get; set; } = new();
    public List<string> Optional { get; set; } = new();
    public string Brief { get; set; } = string.Empty;

    internal bool Allows(string motion) => Required.Contains(motion, StringComparer.Ordinal) ||
        Optional.Contains(motion, StringComparer.Ordinal);
}
