using System.Text.Json;

namespace VoxelStore.Tool;

public sealed class ReferenceDensityInput
{
    public int Version { get; set; } = 1;
    public string AssetClass { get; set; } = string.Empty;
    public double TargetWorldLongAxis { get; set; }
    public Dictionary<string, ReferenceDensityPartInput> Parts { get; set; } =
        new(StringComparer.Ordinal);
}

public sealed class ReferenceDensityPartInput
{
    public string PartSheet { get; set; } = string.Empty;
    public int ObjectLongAxisPixels { get; set; }
    public double ApparentVoxelCount { get; set; }
    public List<ReferenceDensityFeatureInput> Features { get; set; } = new();
}

public sealed class ReferenceDensityFeatureInput
{
    public string Id { get; set; } = string.Empty;
    public int PixelSpan { get; set; }
    public string Importance { get; set; } = string.Empty;
    public int? RequiredVoxelSpan { get; set; }
}

public sealed class VpuRulerPolicy
{
    public int Version { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public List<int> CandidateVpu { get; set; } = new();
    public Dictionary<string, VpuRulerPrior> Priors { get; set; } = new(StringComparer.Ordinal);
    public VpuRulerCadence Cadence { get; set; } = new();
    public VpuRulerFeatureReadability FeatureReadability { get; set; } = new();
    public VpuRulerSelection Selection { get; set; } = new();
}

public sealed class VpuRulerPrior
{
    public int[] Typical { get; set; } = Array.Empty<int>();
}

public sealed class VpuRulerCadence
{
    public string Estimate { get; set; } = string.Empty;
    public bool UseAcceptedOrthographicEvidence { get; set; }
    public bool WholeImagePixelSimilarityIsAuthority { get; set; }
}

public sealed class VpuRulerFeatureReadability
{
    public bool AllowIntentionalOneVoxelFeature { get; set; }
    public int? UniversalMinimumFeatureVoxels { get; set; }
    public List<string> RejectWhen { get; set; } = new();
}

public sealed class VpuRulerSelection
{
    public string Mode { get; set; } = string.Empty;
    public bool PriorsAreHardBounds { get; set; }
    public bool PreserveWorldExtent { get; set; }
    public bool PreserveReferenceCadence { get; set; }
}

public sealed class ReferenceDensityCandidateAudit
{
    public int Version { get; set; } = 1;
    public int Vpu { get; set; }
    public Dictionary<string, bool> Features { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, int> FeatureVoxelSpans { get; set; } = new(StringComparer.Ordinal);
    public bool CadencePreserved { get; set; } = true;
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
    public int? RulerVersion { get; set; }
    public string? RulerSha256 { get; set; }
    public double TargetWorldLongAxis { get; set; }
    public Dictionary<string, double> ApparentVoxelCounts { get; set; } = new(StringComparer.Ordinal);
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
    public bool CadencePreserved { get; set; }
    public List<string> LostFeatures { get; set; } = new();
    public Dictionary<string, int> FeatureVoxelSpans { get; set; } = new(StringComparer.Ordinal);
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
    public const int LegacyVpuQuantum = 16;
    public const int HeroVoxelAxisLimit = 256;

    private static readonly HashSet<string> SupportedImportance =
        new(StringComparer.Ordinal) { "primary", "secondary", "tertiary" };

    public static ReferenceDensityInput LoadInput(string path) =>
        ContractYaml.Load<ReferenceDensityInput>(path);

    public static VpuRulerPolicy LoadRuler(string path)
    {
        VpuRulerPolicy ruler = ContractYaml.Load<VpuRulerPolicy>(path);
        ValidateRuler(ruler);
        return ruler;
    }

    public static ReferenceDensityCandidateAudit LoadCandidateAudit(string path)
    {
        return JsonSerializer.Deserialize<ReferenceDensityCandidateAudit>(
                   File.ReadAllText(path),
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new InvalidDataException($"Density candidate audit is invalid: {path}");
    }

    public static void ValidateRuler(VpuRulerPolicy ruler)
    {
        if (ruler.Version <= 0)
            throw new InvalidDataException("VPU ruler version must be positive.");
        if (!string.Equals(ruler.Scope, "per-unit", StringComparison.Ordinal))
            throw new InvalidDataException("VPU ruler scope must be 'per-unit'.");
        if (ruler.CandidateVpu.Count == 0 ||
            ruler.CandidateVpu.Any(x => x <= 0) ||
            !ruler.CandidateVpu.SequenceEqual(ruler.CandidateVpu.OrderBy(x => x).Distinct()))
            throw new InvalidDataException("VPU ruler candidateVpu must be positive, unique, and ascending.");
        if (ruler.FeatureReadability.UniversalMinimumFeatureVoxels.HasValue)
            throw new InvalidDataException("VPU ruler must not impose a universal minimum feature voxel span.");
        if (!ruler.FeatureReadability.AllowIntentionalOneVoxelFeature)
            throw new InvalidDataException("VPU ruler must allow intentional one-voxel features.");
        if (!string.Equals(ruler.Selection.Mode, "lowest-sufficient", StringComparison.Ordinal) ||
            ruler.Selection.PriorsAreHardBounds)
            throw new InvalidDataException("VPU ruler selection must be lowest-sufficient with non-binding priors.");

        foreach ((string id, VpuRulerPrior prior) in ruler.Priors)
        {
            if (string.IsNullOrWhiteSpace(id) ||
                prior.Typical.Length != 2 ||
                prior.Typical[0] <= 0 ||
                prior.Typical[1] < prior.Typical[0])
                throw new InvalidDataException($"VPU ruler prior '{id}' must declare a positive ascending [min,max] typical range.");
        }
    }

    public static void ValidateInput(ReferenceDensityInput input, ReferenceContract reference)
    {
        if (input.Version is not (1 or 2))
            throw new InvalidDataException("Density input must use version 1 or 2.");
        if (!double.IsFinite(input.TargetWorldLongAxis) || input.TargetWorldLongAxis <= 0)
            throw new InvalidDataException("Density targetWorldLongAxis must be positive and finite.");
        if (input.Version == 2 && string.IsNullOrWhiteSpace(input.AssetClass))
            throw new InvalidDataException("Density input v2 requires assetClass.");

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
            if (input.Version == 2 &&
                (!double.IsFinite(part.ApparentVoxelCount) || part.ApparentVoxelCount <= 0))
                throw new InvalidDataException(
                    $"Density apparentVoxelCount must be positive and finite for '{partId}'.");
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
                if (!SupportedImportance.Contains(feature.Importance))
                    throw new InvalidDataException(
                        $"Density feature '{feature.Id}' has unsupported importance '{feature.Importance}'.");

                if (input.Version == 1)
                {
                    if (feature.PixelSpan <= 0 || !feature.RequiredVoxelSpan.HasValue ||
                        feature.RequiredVoxelSpan.Value <= 0)
                        throw new InvalidDataException(
                            $"Legacy density feature '{feature.Id}' spans must be positive.");
                }
                else if (feature.RequiredVoxelSpan.HasValue)
                {
                    throw new InvalidDataException(
                        $"Density input v2 feature '{feature.Id}' must not declare requiredVoxelSpan.");
                }
            }
        }
    }

    public static double EstimateVpu(ReferenceDensityInput input)
    {
        if (input.Version == 1)
            return EstimateLegacyVpu(input);

        double estimate = input.Parts.Values
            .Max(part => part.ApparentVoxelCount / input.TargetWorldLongAxis);
        if (!double.IsFinite(estimate) || estimate <= 0)
            throw new InvalidDataException("Density VPU estimate is invalid.");
        return estimate;
    }

    private static double EstimateLegacyVpu(ReferenceDensityInput input)
    {
        double estimate = 0;
        foreach (ReferenceDensityPartInput part in input.Parts.Values)
        {
            foreach (ReferenceDensityFeatureInput feature in part.Features)
            {
                double required =
                    part.ObjectLongAxisPixels * (double)feature.RequiredVoxelSpan!.Value /
                    (feature.PixelSpan * input.TargetWorldLongAxis);
                estimate = Math.Max(estimate, required);
            }
        }

        if (!double.IsFinite(estimate) || estimate <= 0)
            throw new InvalidDataException("Density VPU estimate is invalid.");
        return estimate;
    }

    public static IReadOnlyList<int> BuildCandidateSequence(
        ReferenceDensityInput input,
        VpuRulerPolicy? ruler = null)
    {
        if (input.Version == 1)
            return BuildLegacyCandidateSequence(input);

        if (ruler == null)
            throw new InvalidDataException("Density input v2 requires the shared VPU ruler.");
        ValidateRuler(ruler);

        double estimate = EstimateVpu(input);
        int maxByLongAxis = (int)Math.Floor(HeroVoxelAxisLimit / input.TargetWorldLongAxis);
        int[] valid = ruler.CandidateVpu.Where(v => v <= maxByLongAxis).ToArray();
        if (valid.Length == 0)
            throw new InvalidDataException(
                "No VPU ruler candidate can fit Hero.Voxel's 256-voxel long-axis limit.");

        int insertion = Array.FindIndex(valid, v => v >= estimate);
        if (insertion < 0)
            insertion = valid.Length - 1;

        int start = Math.Max(0, insertion - 2);
        int end = Math.Min(valid.Length - 1, insertion + 2);

        if (ruler.Priors.TryGetValue(input.AssetClass, out VpuRulerPrior? prior))
        {
            int firstPrior = Array.FindIndex(valid, v => v >= prior.Typical[0]);
            int lastPrior = Array.FindLastIndex(valid, v => v <= prior.Typical[1]);
            if (firstPrior >= 0)
                start = Math.Min(start, firstPrior);
            if (lastPrior >= 0)
                end = Math.Max(end, lastPrior);
        }

        int[] result = valid.Skip(start).Take(end - start + 1).ToArray();
        if (!result.Any(v => v >= estimate) && valid.Any(v => v >= estimate))
            throw new InvalidDataException("Density candidate neighborhood did not include the estimated VPU.");
        return result;
    }

    private static IReadOnlyList<int> BuildLegacyCandidateSequence(ReferenceDensityInput input)
    {
        double estimate = EstimateLegacyVpu(input);
        int roundedUp = (int)Math.Ceiling(estimate / LegacyVpuQuantum) * LegacyVpuQuantum;
        int start = Math.Max(LegacyVpuQuantum, roundedUp - LegacyVpuQuantum);
        int maxByLongAxis = (int)Math.Floor(HeroVoxelAxisLimit / input.TargetWorldLongAxis);
        int maxCandidate = maxByLongAxis / LegacyVpuQuantum * LegacyVpuQuantum;

        if (maxCandidate < roundedUp)
            throw new InvalidDataException(
                "Estimated density cannot fit Hero.Voxel's 256-voxel long-axis limit.");

        var result = new List<int>();
        for (int vpu = start; vpu <= maxCandidate; vpu += LegacyVpuQuantum)
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

        if (input.Version == 2)
        {
            string[] spanFeatures = audit.FeatureVoxelSpans.Keys
                .OrderBy(x => x, StringComparer.Ordinal).ToArray();
            if (!requiredFeatures.SequenceEqual(spanFeatures, StringComparer.Ordinal) ||
                audit.FeatureVoxelSpans.Values.Any(span => span <= 0))
                throw new InvalidDataException(
                    "Density candidate v2 featureVoxelSpans must exactly cover meaningful features with positive spans.");
        }

        var lost = requiredFeatures.Where(id => !audit.Features[id]).ToList();
        bool withinAxis = audit.AxisCompliant && audit.IntegerBounds.All(x => x <= HeroVoxelAxisLimit);
        bool withinBudget = audit.OccupiedVoxels <= budget.MaxVoxelsPerAsset;
        double longAxis = audit.NormalizedWorldSize.Max();
        double tolerance = input.Version == 1 ? 1e-6 : Math.Max(1e-6, 0.5 / audit.Vpu);
        bool normalizedSizeMatches =
            Math.Abs(longAxis - input.TargetWorldLongAxis) <= tolerance;
        bool cadencePreserved = input.Version == 1 || audit.CadencePreserved;

        return new ReferenceDensityCandidateEvidence
        {
            Vpu = audit.Vpu,
            Passed = lost.Count == 0 && cadencePreserved &&
                     withinAxis && withinBudget && normalizedSizeMatches,
            CadencePreserved = cadencePreserved,
            LostFeatures = lost,
            FeatureVoxelSpans = new Dictionary<string, int>(audit.FeatureVoxelSpans, StringComparer.Ordinal),
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
        foreach (int vpu in candidateSequence.OrderBy(x => x))
            if (byVpu.TryGetValue(vpu, out ReferenceDensityCandidateEvidence? candidate) &&
                candidate.Passed)
                return vpu;
        return null;
    }
}
