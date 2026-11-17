using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VoxelStore.Tool;

public sealed class StorePolicy
{
    public int Version { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StyleFamily { get; set; } = string.Empty;
    public string ReferenceStyle { get; set; } = string.Empty;
    public bool AllowOptionalExpansion { get; set; }
    public StoreBudget Budget { get; set; } = new();
}

public sealed class StoreBudget
{
    public int MaxVoxelsPerAsset { get; set; }
    public int MaxPartsPerAsset { get; set; }
}

public sealed class CoveragePolicy
{
    public int Version { get; set; }
    public string Foundation { get; set; } = string.Empty;
    public List<CoverageRequirement> Requirements { get; set; } = new();
}

public sealed class CoverageRequirement
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string? Profile { get; set; }
    public string? Mount { get; set; }
    public bool Required { get; set; } = true;
}

public sealed class CompatibilityProfile
{
    public int Version { get; set; }
    public string Id { get; set; } = string.Empty;
    public int VoxelsPerUnit { get; set; }
    public List<MountContract> RequiredMounts { get; set; } = new();
    public List<MountContract> OptionalMounts { get; set; } = new();
    public List<PoseSampleContract> RepresentativePoseSamples { get; set; } = new();
}

public sealed class MountContract
{
    public string Socket { get; set; } = string.Empty;
    public float[] Forward { get; set; } = Array.Empty<float>();
    public float[] Up { get; set; } = Array.Empty<float>();
    public float[] Envelope { get; set; } = Array.Empty<float>();
}

public sealed class PoseSampleContract
{
    public string Clip { get; set; } = string.Empty;
    public float NormalizedTime { get; set; }
}

public sealed class AssetManifest
{
    public int Version { get; set; }
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string UnitRef { get; set; } = string.Empty;
    public string? Family { get; set; }
    public AssetMount? Mount { get; set; }
    public AssetSource Source { get; set; } = new();
    public AssetStyle Style { get; set; } = new();
}

public sealed class AssetMount
{
    public string Profile { get; set; } = string.Empty;
    public string Socket { get; set; } = string.Empty;
}

public sealed class AssetSource
{
    public string Base { get; set; } = string.Empty;
    public string Palette { get; set; } = string.Empty;
}

public sealed class AssetStyle
{
    public string Family { get; set; } = string.Empty;
}

public sealed class HeroVoxelLock
{
    public int Version { get; set; }
    public string HeroStudioRevision { get; set; } = string.Empty;
    public string WorkflowSha256 { get; set; } = string.Empty;
    public string ToolProject { get; set; } = string.Empty;
    public string Workflow { get; set; } = string.Empty;
}

public sealed class ReferenceStyleManifest
{
    public int Version { get; set; }
    public string Id { get; set; } = string.Empty;
    public List<string> References { get; set; } = new();
    public List<string> Traits { get; set; } = new();
}

public sealed class ReferenceContract
{
    public int Version { get; set; }
    public string Composition { get; set; } = string.Empty;
    public List<string> RequiredParts { get; set; } = new();
    public Dictionary<string, string> PartSheets { get; set; } = new(StringComparer.Ordinal);
}

public sealed class AuditEvidence
{
    public int Version { get; set; } = 1;
    public string Kind { get; set; } = string.Empty;
    public Dictionary<string, string> Sha256 { get; set; } = new(StringComparer.Ordinal);
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReferenceStyle { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HeroVoxelWorkflowSha256 { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? StyleSha256 { get; set; }
}

public static class ContractYaml
{
    private static readonly IDeserializer Deserializer =
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

    public static T Load<T>(string path) where T : class =>
        Deserializer.Deserialize<T>(File.ReadAllText(path))
        ?? throw new InvalidDataException($"YAML document is empty: {path}");

    public static T Parse<T>(string text) where T : class =>
        Deserializer.Deserialize<T>(text)
        ?? throw new InvalidDataException("YAML document is empty.");
}
