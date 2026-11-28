using Hero.Voxel.Tool;

namespace VoxelStore.Tool;

internal sealed record ProfileClipRequirement(string Name, bool Loop);

internal sealed class ProfileAnimationCoverage
{
    internal List<ProfileClipRequirement> RequiredClips { get; } = new();
    internal List<string> MissingClips { get; } = new();
    internal List<string> InvalidClips { get; } = new();
    internal bool Complete => MissingClips.Count == 0 && InvalidClips.Count == 0;

    internal void RequireComplete(string assetId)
    {
        if (!Complete)
            throw new InvalidDataException($"Animation profile coverage is incomplete for '{assetId}': " +
                string.Join("; ", MissingClips.Select(name => "missing " + name).Concat(InvalidClips)));
    }

    internal static ProfileAnimationCoverage Evaluate(
        AnimationProfilePolicy policy, AssetManifest asset, IReadOnlyList<CompiledAnimationClip> clips)
    {
        var profile = policy.Resolve(asset);
        var result = new ProfileAnimationCoverage();
        var names = new HashSet<string>(StringComparer.Ordinal);
        var families = new SortedSet<string>(profile.Required, StringComparer.Ordinal);
        var familyLoops = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (string family in profile.Required)
            familyLoops.Add(family, policy.LoopFor(family));

        foreach (var clip in clips.OrderBy(c => c.Name, StringComparer.Ordinal))
        {
            if (!names.Add(clip.Name))
            {
                result.InvalidClips.Add($"duplicate clip '{clip.Name}'");
                continue;
            }
            SemanticAnimationClipIdentity identity;
            try
            {
                identity = SemanticAnimationClipName.Parse(clip.Name, policy.Families.Keys);
            }
            catch (InvalidDataException ex)
            {
                result.InvalidClips.Add(ex.Message);
                continue;
            }
            if (!profile.Allows(identity.Motion))
            {
                result.InvalidClips.Add($"clip '{clip.Name}' is not allowed by profile '{asset.AnimationProfile}'");
                continue;
            }
            string family = identity.Motion + (identity.Side == null ? "" : "." + identity.Side);
            families.Add(identity.Motion);
            families.Add(family);
            bool loop = policy.LoopFor(identity.Motion);
            familyLoops[identity.Motion] = loop;
            familyLoops[family] = loop;
            if (clip.Loop != loop)
                result.InvalidClips.Add($"clip '{clip.Name}' has incorrect loop policy");
            if (!float.IsFinite(clip.Duration) || clip.Duration <= 0)
                result.InvalidClips.Add($"clip '{clip.Name}' has invalid duration");
        }

        // A side-qualified or optional take activates its own complete family;
        // side-qualified takes never substitute for the generic family.
        foreach (string family in families)
        {
            for (int variant = 0; variant <= 3; variant++)
            {
                string name = variant == 0 ? family : family + ".V" + variant;
                result.RequiredClips.Add(new ProfileClipRequirement(name, familyLoops[family]));
                if (!names.Contains(name))
                    result.MissingClips.Add(name);
            }
        }

        return result;
    }
}
