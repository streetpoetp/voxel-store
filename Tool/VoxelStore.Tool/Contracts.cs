using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VoxelStore.Tool;

public sealed class StorePolicy
{
    public int Version { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Collection { get; set; } = string.Empty;
    public string DefaultStyle { get; set; } = string.Empty;
    public bool AllowOptionalExpansion { get; set; }
    public StoreBudget Budget { get; set; } = new();
}

public sealed class StoreBudget
{
    public int MaxVoxelsPerAsset { get; set; }
    public int MaxPartsPerAsset { get; set; }
}

public sealed class CollectionManifest
{
    public int Version { get; set; }
    public string Id { get; set; } = string.Empty;
    public List<CoverageRequirement> Requirements { get; set; } = new();
}

public sealed class CoverageRequirement
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string? Form { get; set; }
    public bool Required { get; set; } = true;
}

public sealed class RetiredAssetPolicy
{
    public int Version { get; set; }
    public List<RetiredAssetIdentity> Assets { get; set; } = new();
}

public sealed class RetiredAssetIdentity
{
    public string Id { get; set; } = string.Empty;
    public string UnitRef { get; set; } = string.Empty;
}


public sealed class AssetManifest
{
    public int Version { get; set; }
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string? AnimationProfile { get; set; }
    public string UnitRef { get; set; } = string.Empty;
    public string? Form { get; set; }
    public string? Density { get; set; }
    public AssetAttachment? Attachment { get; set; }
    public AssetSource Source { get; set; } = new();
    public AssetStyle Style { get; set; } = new();
}

public sealed class AssetAttachment
{
    public string Capability { get; set; } = string.Empty;
    public string Side { get; set; } = "Any";
    public string Anchor { get; set; } = string.Empty;
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
    public Dictionary<string, List<string>> References { get; set; } =
        new(StringComparer.Ordinal);
    public Dictionary<string, List<string>> TargetReferences { get; set; } =
        new(StringComparer.Ordinal);
    public List<string> Traits { get; set; } = new();
    public Dictionary<string, List<string>> TraitsByKind { get; set; } =
        new(StringComparer.Ordinal);
}

public sealed class ReferenceTargetContract
{
    public string Image { get; set; } = string.Empty;
    public string Provenance { get; set; } = string.Empty;
}

public sealed class ReferenceContract
{
    public int Version { get; set; }
    public string Composition { get; set; } = string.Empty;
    public List<string> RequiredParts { get; set; } = new();
    public Dictionary<string, string> PartSheets { get; set; } = new(StringComparer.Ordinal);
    public ReferenceTargetContract? Target { get; set; }
    public string Fidelity { get; set; } = string.Empty;
}

public sealed class ReferenceTargetSourceProvenance
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    [JsonPropertyName("relation")]
    public string Relation { get; set; } = string.Empty;
}

public sealed class ReferenceTargetGeneratorRequestProvenance
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;
}

public sealed class ReferenceTargetGeneratorProvenance
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("request")]
    public ReferenceTargetGeneratorRequestProvenance? Request { get; set; }
}

public sealed class ReferenceTargetProvenance
{
    public const int AcquisitionNeutralVersion = 3;
    public const string RepositoryAuthoredOrigin = "repository-authored";
    public const string ImportedOrigin = "imported";

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("origin")]
    public string Origin { get; set; } = string.Empty;

    [JsonPropertyName("targetSha256")]
    public string TargetSha256 { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ReferenceTargetSourceProvenance? Source { get; set; }

    [JsonPropertyName("generator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ReferenceTargetGeneratorProvenance? Generator { get; set; }

    [JsonPropertyName("targetStyleAuthoritySha256")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TargetStyleAuthoritySha256 { get; set; }

    [JsonPropertyName("targetStyleSha256")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? TargetStyleSha256 { get; set; }
}

public sealed class ReferenceFidelityTargetEvidence
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;
}

public sealed class ReferenceFidelitySheetEvidence
{
    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    [JsonPropertyName("criteria")]
    public Dictionary<string, string> Criteria { get; set; } = new(StringComparer.Ordinal);
}

public sealed class ReferenceFidelityAudit
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("target")]
    public ReferenceFidelityTargetEvidence Target { get; set; } = new();

    [JsonPropertyName("sheets")]
    public Dictionary<string, ReferenceFidelitySheetEvidence> Sheets { get; set; } =
        new(StringComparer.Ordinal);
}

public sealed class AuditEvidence
{
    public int Version { get; set; } = 1;
    public string Kind { get; set; } = string.Empty;
    public Dictionary<string, string> Sha256 { get; set; } = new(StringComparer.Ordinal);
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReferenceStyle { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StyleKind { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StyleAuthoritySha256 { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HeroVoxelWorkflowSha256 { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? StyleSha256 { get; set; }
}

public sealed class AnimationReviewEvidence
{
    public int Version { get; set; } = 1;
    public string Outcome { get; set; } = string.Empty;
    public string AnimationProfile { get; set; } = string.Empty;
    public string ManifestSha256 { get; set; } = string.Empty;
    public string SourceSha256 { get; set; } = string.Empty;
    public string PaletteSha256 { get; set; } = string.Empty;
    public string GeneratedAnimationSha256 { get; set; } = string.Empty;
    public string AnimationProfilesSha256 { get; set; } = string.Empty;
    public string HeroStudioRevision { get; set; } = string.Empty;
    public string HeroVoxelWorkflowSha256 { get; set; } = string.Empty;
    public string CaptureReportSha256 { get; set; } = string.Empty;
    public List<string> ReviewedClips { get; set; } = new();
    public Dictionary<string, List<string>> ClipCheckpoints { get; set; } =
        new(StringComparer.Ordinal);
    public Dictionary<string, string> CaptureImageSha256 { get; set; } =
        new(StringComparer.Ordinal);
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
