using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VoxelStore.Tool;

public sealed class ReferenceDensityInput
{
    public int Version { get; set; } = 1;
    public double TargetWorldLongAxis { get; set; }
    public Dictionary<string, ReferenceDensityPartInput> Parts { get; set; } =
        new(StringComparer.Ordinal);
}

public sealed class ReferenceDensityPartInput
{
    public string PartSheet { get; set; } = string.Empty;
    public int ObjectLongAxisPixels { get; set; }
    public List<ReferenceDensityFeatureInput> Features { get; set; } = new();
}

public sealed class ReferenceDensityFeatureInput
{
    public string Id { get; set; } = string.Empty;
    public int PixelSpan { get; set; }
    public string Importance { get; set; } = string.Empty;
    public int RequiredVoxelSpan { get; set; }
}

public sealed class ReferenceDensityCandidateAudit
{
    public int Version { get; set; } = 1;
    public int Vpu { get; set; }
    public Dictionary<string, bool> Features { get; set; } = new(StringComparer.Ordinal);
    public double[] NormalizedWorldSize { get; set; } = Array.Empty<double>();
    public int[] IntegerBounds { get; set; } = Array.Empty<int>();
    public int OccupiedVoxels { get; set; }
    public bool AxisCompliant { get; set; }
    public int GreedyMergedQuads { get; set; }
    public int Vertices { get; set; }
    public int Triangles { get; set; }
    public string SourceSha256 { get; set; } = string.Empty;
    public string PreviewSha256 { get; set; } = string.Empty;
}

public sealed class ReferenceDensityEvidence
{
    public int Version { get; set; } = 1;
    public string ReferenceAuditSha256 { get; set; } = string.Empty;
    public string DensityInputSha256 { get; set; } = string.Empty;
    public double TargetWorldLongAxis { get; set; }
    public double EstimatedVpu { get; set; }
    public List<int> CandidateSequence { get; set; } = new();
    public List<ReferenceDensityCandidateEvidence> Candidates { get; set; } = new();
    public int? SelectedVpu { get; set; }
    public string SelectionReason { get; set; } = string.Empty;
}

public sealed class ReferenceDensityCandidateEvidence
{
    public int Vpu { get; set; }
    public bool Passed { get; set; }
    public List<string> LostFeatures { get; set; } = new();
    public double[] NormalizedWorldSize { get; set; } = Array.Empty<double>();
    public int[] IntegerBounds { get; set; } = Array.Empty<int>();
    public int OccupiedVoxels { get; set; }
    public bool AxisCompliant { get; set; }
    public int GreedyMergedQuads { get; set; }
    public int Vertices { get; set; }
    public int Triangles { get; set; }
    public string SourceSha256 { get; set; } = string.Empty;
    public string PreviewSha256 { get; set; } = string.Empty;
    public string AuditSha256 { get; set; } = string.Empty;
}

public static class ReferenceDensitySelector
{
    public const int VpuQuantum = 16;
    public const int HeroVoxelAxisLimit = 256;

    private static readonly HashSet<string> SupportedImportance =
        new(StringComparer.Ordinal) { "primary", "secondary", "tertiary" };

    public static ReferenceDensityInput LoadInput(string path) =>
        ContractYaml.Load<ReferenceDensityInput>(path);

    public static ReferenceDensityCandidateAudit LoadCandidateAudit(string path)
    {
        return JsonSerializer.Deserialize<ReferenceDensityCandidateAudit>(
                   File.ReadAllText(path),
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new InvalidDataException($"Density candidate audit is invalid: {path}");
    }

    public static void ValidateInput(ReferenceDensityInput input, ReferenceContract reference)
    {
        if (input.Version != 1)
            throw new InvalidDataException("Density input must use version 1.");
        if (!double.IsFinite(input.TargetWorldLongAxis) || input.TargetWorldLongAxis <= 0)
            throw new InvalidDataException("Density targetWorldLongAxis must be positive and finite.");

        string[] expectedParts = reference.RequiredParts
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();
        string[] actualParts = input.Parts.Keys
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();
        if (!expectedParts.SequenceEqual(actualParts, StringComparer.Ordinal))
            throw new InvalidDataException(
                "Density input parts must exactly match accepted RequiredParts.");

        var globalFeatureIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (string partId in expectedParts)
        {
            ReferenceDensityPartInput part = input.Parts[partId];
            if (!reference.PartSheets.TryGetValue(partId, out string? acceptedSheet) ||
                !string.Equals(part.PartSheet, acceptedSheet, StringComparison.Ordinal))
                throw new InvalidDataException(
                    $"Density Part Sheet mapping for '{partId}' must match accepted reference authority.");
            if (part.ObjectLongAxisPixels <= 0)
                throw new InvalidDataException(
                    $"Density objectLongAxisPixels must be positive for '{partId}'.");
            if (part.Features.Count == 0)
                throw new InvalidDataException(
                    $"Density input requires meaningful features for '{partId}'.");

            var featureIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (ReferenceDensityFeatureInput feature in part.Features)
            {
                if (string.IsNullOrWhiteSpace(feature.Id) ||
                    !featureIds.Add(feature.Id) ||
                    !globalFeatureIds.Add(feature.Id))
                    throw new InvalidDataException(
                        "Density feature ids must be non-empty and globally unique.");
                if (feature.PixelSpan <= 0 || feature.RequiredVoxelSpan <= 0)
                    throw new InvalidDataException(
                        $"Density feature '{feature.Id}' spans must be positive.");
                if (!SupportedImportance.Contains(feature.Importance))
                    throw new InvalidDataException(
                        $"Density feature '{feature.Id}' has unsupported importance '{feature.Importance}'.");
            }
        }
    }

    public static double EstimateVpu(ReferenceDensityInput input)
    {
        double estimate = 0;
        foreach (ReferenceDensityPartInput part in input.Parts.Values)
        {
            foreach (ReferenceDensityFeatureInput feature in part.Features)
            {
                double required =
                    part.ObjectLongAxisPixels * (double)feature.RequiredVoxelSpan /
                    (feature.PixelSpan * input.TargetWorldLongAxis);
                estimate = Math.Max(estimate, required);
            }
        }

        if (!double.IsFinite(estimate) || estimate <= 0)
            throw new InvalidDataException("Density VPU estimate is invalid.");
        return estimate;
    }

    public static IReadOnlyList<int> BuildCandidateSequence(ReferenceDensityInput input)
    {
        double estimate = EstimateVpu(input);
        int roundedUp = (int)Math.Ceiling(estimate / VpuQuantum) * VpuQuantum;
        int start = Math.Max(VpuQuantum, roundedUp - VpuQuantum);
        int maxByLongAxis = (int)Math.Floor(HeroVoxelAxisLimit / input.TargetWorldLongAxis);
        int maxCandidate = maxByLongAxis / VpuQuantum * VpuQuantum;

        if (maxCandidate < roundedUp)
            throw new InvalidDataException(
                "Estimated density cannot fit Hero.Voxel's 256-voxel long-axis limit.");

        var result = new List<int>();
        for (int vpu = start; vpu <= maxCandidate; vpu += VpuQuantum)
            result.Add(vpu);
        return result;
    }

    public static ReferenceDensityCandidateEvidence ValidateCandidate(
        ReferenceDensityInput input,
        ReferenceDensityCandidateAudit audit,
        StoreBudget budget,
        string auditSha256)
    {
        if (audit.Version != 1 || audit.Vpu <= 0)
            throw new InvalidDataException("Density candidate audit has invalid version/VPU.");
        if (audit.NormalizedWorldSize.Length != 3 ||
            audit.NormalizedWorldSize.Any(x => !double.IsFinite(x) || x <= 0))
            throw new InvalidDataException("Density candidate normalizedWorldSize must contain three positive finite values.");
        if (audit.IntegerBounds.Length != 3 || audit.IntegerBounds.Any(x => x <= 0))
            throw new InvalidDataException("Density candidate integerBounds must contain three positive values.");
        if (audit.OccupiedVoxels <= 0)
            throw new InvalidDataException("Density candidate occupiedVoxels must be positive.");
        if (audit.GreedyMergedQuads <= 0 || audit.Vertices <= 0 || audit.Triangles <= 0)
            throw new InvalidDataException("Density candidate greedy mesh metrics must be positive.");
        if (string.IsNullOrWhiteSpace(audit.SourceSha256) ||
            string.IsNullOrWhiteSpace(audit.PreviewSha256) ||
            string.IsNullOrWhiteSpace(auditSha256))
            throw new InvalidDataException("Density candidate source/preview/audit hashes are required.");

        string[] requiredFeatures = input.Parts.Values
            .SelectMany(x => x.Features)
            .Select(x => x.Id)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
        string[] actualFeatures = audit.Features.Keys
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();
        if (!requiredFeatures.SequenceEqual(actualFeatures, StringComparer.Ordinal))
            throw new InvalidDataException(
                "Density candidate feature results must exactly cover declared meaningful features.");

        var lost = requiredFeatures.Where(id => !audit.Features[id]).ToList();
        bool withinAxis = audit.AxisCompliant && audit.IntegerBounds.All(x => x <= HeroVoxelAxisLimit);
        bool withinBudget = audit.OccupiedVoxels <= budget.MaxVoxelsPerAsset;
        double longAxis = audit.NormalizedWorldSize.Max();
        bool normalizedSizeMatches =
            Math.Abs(longAxis - input.TargetWorldLongAxis) <= 1e-6;

        return new ReferenceDensityCandidateEvidence
        {
            Vpu = audit.Vpu,
            Passed = lost.Count == 0 && withinAxis && withinBudget && normalizedSizeMatches,
            LostFeatures = lost,
            NormalizedWorldSize = audit.NormalizedWorldSize,
            IntegerBounds = audit.IntegerBounds,
            OccupiedVoxels = audit.OccupiedVoxels,
            AxisCompliant = withinAxis,
            GreedyMergedQuads = audit.GreedyMergedQuads,
            Vertices = audit.Vertices,
            Triangles = audit.Triangles,
            SourceSha256 = audit.SourceSha256,
            PreviewSha256 = audit.PreviewSha256,
            AuditSha256 = auditSha256
        };
    }

    public static int? SelectLowestSufficient(
        IReadOnlyList<int> candidateSequence,
        IReadOnlyList<ReferenceDensityCandidateEvidence> candidates)
    {
        var byVpu = candidates.ToDictionary(x => x.Vpu);
        foreach (int vpu in candidateSequence)
            if (byVpu.TryGetValue(vpu, out ReferenceDensityCandidateEvidence? candidate) &&
                candidate.Passed)
                return vpu;
        return null;
    }
}
