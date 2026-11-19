using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hero.Voxel.Tool;

namespace VoxelStore.Tool;

public sealed record AssetRecord(
    AssetManifest Manifest,
    string ManifestPath,
    string RootDirectory,
    string SourceBase,
    string PalettePath,
    string ReferenceDirectory,
    string ProofDirectory);

public sealed record GateResult(string Id, bool Passed, string Message, string? Evidence = null);
public sealed record CoverageGap(
    string Id,
    string Kind,
    string? Profile,
    string? Capability,
    string? Form,
    bool Required);
public sealed record MaintenanceJob(string Priority, string Kind, string Target, string Reason);
public sealed record CompatibilityResult(
    string Avatar,
    string Equipment,
    string Profile,
    string Capability,
    string Attachment,
    string Pose,
    bool Passed,
    string Evidence);

public sealed record PartWorldFrame(Vector3 Origin, Vector3 WorldX, Vector3 WorldY, Vector3 WorldZ);
public sealed record SocketWorldFrame(Vector3 Origin, Vector3 WorldX, Vector3 WorldY, Vector3 WorldZ);
public sealed record PngInfo(int Width, int Height);
public sealed record ReferenceStyleAuthority(
    string Id,
    string Kind,
    string ManifestPath,
    IReadOnlyList<string> CommonReferencePaths,
    IReadOnlyList<string> KindReferencePaths,
    IReadOnlyList<string> Traits);

public sealed class AssetHealth
{
    public required string Id { get; init; }
    public required string Kind { get; init; }
    public required string UnitRef { get; init; }
    public string? Form { get; init; }
    public string? Profile { get; init; }
    public string? Capability { get; init; }
    public bool BasePublished { get; set; }
    public bool Published { get; set; }
    public string? FailedGate { get; set; }
    public List<GateResult> Gates { get; } = new();
}

public sealed class StoreSnapshot
{
    public string Store { get; set; } = string.Empty;
    public List<AssetHealth> Assets { get; set; } = new();
    public List<CoverageGap> CoverageGaps { get; set; } = new();
    public List<CompatibilityResult> Compatibility { get; set; } = new();
    public MaintenanceJob? NextJob { get; set; }
    public bool Healthy => Assets.All(x => x.Published) && CoverageGaps.All(x => !x.Required);
}

public sealed class StoreEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string[] RequiredFidelityCriteria =
    {
        "silhouette",
        "proportions",
        "constructionRichness",
        "materialColorHierarchy",
        "meaningfulDetailDensity",
        "visualEvidenceDominance",
        "implementationNeutral"
    };
    private readonly string _root;
    private readonly HeroVoxelProvider _provider;

    public StoreEngine(string root)
    {
        _root = Path.GetFullPath(root);
        _provider = new HeroVoxelProvider(_root);
    }

    public string Root => _root;

    public static string FindRoot(string? start = null)
    {
        string current = Path.GetFullPath(start ?? Environment.CurrentDirectory);
        while (true)
        {
            if (File.Exists(Path.Combine(current, "policy", "store.yaml")))
                return current;
            DirectoryInfo? parent = Directory.GetParent(current);
            if (parent == null)
                throw new DirectoryNotFoundException("Could not locate voxel-store root (policy/store.yaml).");
            current = parent.FullName;
        }
    }

    public IReadOnlyList<AssetRecord> DiscoverAssets()
    {
        string art = Path.Combine(_root, "Art");
        if (!Directory.Exists(art))
            return Array.Empty<AssetRecord>();

        var assets = new List<AssetRecord>();
        foreach (string manifestPath in Directory.GetFiles(art, "asset.yaml", SearchOption.AllDirectories)
                     .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            AssetManifest manifest = ContractYaml.Load<AssetManifest>(manifestPath);
            ValidateManifestShape(manifest, manifestPath);
            string assetRoot = Path.GetDirectoryName(manifestPath)!;
            string sourceBase = ResolveRelative(assetRoot, manifest.Source.Base);
            string palette = ResolveRelative(assetRoot, manifest.Source.Palette);
            assets.Add(new AssetRecord(
                manifest,
                manifestPath,
                assetRoot,
                sourceBase,
                palette,
                Path.Combine(assetRoot, "Reference"),
                Path.Combine(assetRoot, "Proof")));
        }

        EnsureUnique(assets, x => x.Manifest.Id, "asset id");
        EnsureUnique(assets, x => x.Manifest.UnitRef, "UnitRef");
        IReadOnlyList<RetiredAssetIdentity> retired = LoadRetiredIdentities();
        foreach (AssetRecord asset in assets)
        {
            if (retired.Any(x => string.Equals(x.Id, asset.Manifest.Id, StringComparison.Ordinal)))
                throw new InvalidDataException(
                    $"Active asset id '{asset.Manifest.Id}' reuses a retired published identity.");
            if (retired.Any(x => string.Equals(x.UnitRef, asset.Manifest.UnitRef, StringComparison.Ordinal)))
                throw new InvalidDataException(
                    $"Active UnitRef '{asset.Manifest.UnitRef}' reuses a retired published identity.");
        }
        foreach (AssetRecord asset in assets)
            ValidateHistoricalIdentity(asset);
        return assets;
    }

    private IReadOnlyList<RetiredAssetIdentity> LoadRetiredIdentities()
    {
        string path = Path.Combine(_root, "policy", "retired-assets.yaml");
        if (!File.Exists(path))
            return Array.Empty<RetiredAssetIdentity>();

        RetiredAssetPolicy policy = ContractYaml.Load<RetiredAssetPolicy>(path);
        if (policy.Version != 1)
            throw new InvalidDataException("Unsupported retired-assets policy version.");
        if (policy.Assets.Any(x =>
                string.IsNullOrWhiteSpace(x.Id) ||
                string.IsNullOrWhiteSpace(x.UnitRef)))
            throw new InvalidDataException("Retired asset id and UnitRef are required.");
        if (policy.Assets.Select(x => x.Id)
                .Distinct(StringComparer.Ordinal).Count() != policy.Assets.Count)
            throw new InvalidDataException("Retired asset ids must be unique.");
        if (policy.Assets.Select(x => x.UnitRef)
                .Distinct(StringComparer.Ordinal).Count() != policy.Assets.Count)
            throw new InvalidDataException("Retired UnitRefs must be unique.");
        return policy.Assets;
    }

    public StoreSnapshot Scan(bool writeDerived, bool writeCompatibility)
    {
        StorePolicy policy = ContractYaml.Load<StorePolicy>(Path.Combine(_root, "policy", "store.yaml"));
        ValidatePolicy(policy);
        CoveragePolicy coverage = ContractYaml.Load<CoveragePolicy>(Path.Combine(_root, "policy", "coverage.yaml"));
        ValidateCoverage(coverage);
        IReadOnlyList<AssetRecord> assets = DiscoverAssets();
        Dictionary<string, CompatibilityProfile> profiles = LoadProfiles();
        var healthById = new Dictionary<string, AssetHealth>(StringComparer.Ordinal);

        foreach (AssetRecord asset in assets)
            healthById.Add(asset.Manifest.Id, EvaluateBaseHealth(asset, policy, profiles));

        List<CompatibilityResult> compatibility =
            EvaluateCompatibility(assets, healthById, profiles, writeCompatibility);

        foreach (AssetHealth health in healthById.Values)
        {
            bool compatPass = compatibility
                .Where(x => x.Avatar == health.Id || x.Equipment == health.Id)
                .All(x => x.Passed);
            health.Published = health.BasePublished && compatPass;
            if (health.BasePublished && !compatPass)
                health.FailedGate = "compatibility";
        }

        List<CoverageGap> gaps = EvaluateCoverage(coverage, assets, healthById);
        MaintenanceJob? next = PlanNext(assets, healthById, gaps, policy);
        var snapshot = new StoreSnapshot
        {
            Store = policy.Name,
            Assets = healthById.Values.OrderBy(x => x.Id, StringComparer.Ordinal).ToList(),
            CoverageGaps = gaps,
            Compatibility = compatibility,
            NextJob = next
        };

        if (writeDerived)
            WriteDerived(snapshot);
        return snapshot;
    }

    public void Generate(string assetId)
    {
        AssetRecord asset = RequireAsset(assetId);
        _provider.Compile(asset, writeGenerated: true);
        _provider.GenerateProof(asset);
    }

    public void SealReference(string assetId)
    {
        AssetRecord asset = RequireAsset(assetId);
        StorePolicy policy = ContractYaml.Load<StorePolicy>(Path.Combine(_root, "policy", "store.yaml"));
        ValidatePolicy(policy);
        WriteReferenceAudit(asset, policy);
    }

    public void Seal(string assetId)
    {
        SealReference(assetId);
        AssetRecord asset = RequireAsset(assetId);
        WriteProofAudit(asset);
    }

    public HeroVoxelProvider.Resolution ResolveProvider() => _provider.Resolve();

    public ReferenceDensityEvidence SelectDensity(string assetId, bool write)
    {
        AssetRecord asset = RequireAsset(assetId);
        if (!string.Equals(asset.Manifest.Kind, "equipment", StringComparison.Ordinal))
            throw new InvalidOperationException("Reference-density selection applies only to equipment.");

        StorePolicy policy = ContractYaml.Load<StorePolicy>(Path.Combine(_root, "policy", "store.yaml"));
        ValidatePolicy(policy);
        ValidateReference(asset, policy);

        ReferenceDensityEvidence evidence = BuildDensityEvidence(asset, policy);
        if (write)
        {
            string densityDirectory = Path.Combine(asset.RootDirectory, "Density");
            Directory.CreateDirectory(densityDirectory);
            File.WriteAllText(
                Path.Combine(densityDirectory, "evidence.generated.json"),
                JsonSerializer.Serialize(evidence, JsonOptions));
        }

        return evidence;
    }

    internal string ValidateDensityEvidence(AssetRecord asset, StorePolicy policy)
    {
        ValidateReference(asset, policy);
        string evidencePath = Path.Combine(asset.RootDirectory, "Density", "evidence.generated.json");
        if (!File.Exists(evidencePath))
            throw new InvalidDataException("Density evidence is missing.");

        ReferenceDensityEvidence actual =
            JsonSerializer.Deserialize<ReferenceDensityEvidence>(
                File.ReadAllText(evidencePath),
                JsonOptions)
            ?? throw new InvalidDataException("Density evidence is invalid.");
        ReferenceDensityEvidence expected = BuildDensityEvidence(asset, policy);

        string actualJson = JsonSerializer.Serialize(actual, JsonOptions);
        string expectedJson = JsonSerializer.Serialize(expected, JsonOptions);
        if (!string.Equals(actualJson, expectedJson, StringComparison.Ordinal))
            throw new InvalidDataException("Density evidence is stale.");
        if (!actual.SelectedVpu.HasValue)
            throw new InvalidDataException("Density evidence has no selected VPU.");
        return evidencePath;
    }

    private ReferenceDensityEvidence BuildDensityEvidence(AssetRecord asset, StorePolicy policy)
    {
        string densityDirectory = Path.Combine(asset.RootDirectory, "Density");
        string inputPath = Path.Combine(densityDirectory, "input.yaml");
        if (!File.Exists(inputPath))
            throw new InvalidDataException("Density input is missing.");

        ReferenceContract reference = ContractYaml.Load<ReferenceContract>(
            Path.Combine(asset.ReferenceDirectory, "reference.yaml"));
        ReferenceDensityInput input = ReferenceDensitySelector.LoadInput(inputPath);
        ReferenceDensitySelector.ValidateInput(input, reference);

        VpuRulerPolicy? ruler = null;
        string? rulerPath = null;
        string? rulerSha256 = null;
        if (input.Version == 2)
        {
            if (!string.Equals(input.AssetClass, asset.Manifest.Kind, StringComparison.Ordinal))
                throw new InvalidDataException(
                    $"Density input assetClass '{input.AssetClass}' must match asset kind '{asset.Manifest.Kind}'.");
            rulerPath = Path.Combine(_root, "policy", "vpu-ruler.yaml");
            if (!File.Exists(rulerPath))
                throw new InvalidDataException("Shared VPU ruler is missing.");
            ruler = ReferenceDensitySelector.LoadRuler(rulerPath);
            rulerSha256 = HeroVoxelProvider.HashFile(rulerPath);
        }

        IReadOnlyList<int> sequence = ReferenceDensitySelector.BuildCandidateSequence(input, ruler);
        var candidates = new List<ReferenceDensityCandidateEvidence>();
        foreach (int vpu in sequence)
        {
            string candidateDirectory = Path.Combine(
                densityDirectory,
                "Candidates",
                $"vpu-{vpu:D3}");
            string auditPath = Path.Combine(candidateDirectory, "audit.json");
            if (!File.Exists(auditPath))
                continue;

            ReferenceDensityCandidateAudit audit =
                ReferenceDensitySelector.LoadCandidateAudit(auditPath);
            if (audit.Vpu != vpu)
                throw new InvalidDataException(
                    $"Density candidate directory VPU {vpu} disagrees with audit VPU {audit.Vpu}.");

            string sourcePath = Path.Combine(candidateDirectory, "Source", "candidate.voxel.yaml");
            string previewPath = Path.Combine(candidateDirectory, "Proof", "hero.png");
            if (!File.Exists(sourcePath) || !File.Exists(previewPath))
                throw new InvalidDataException(
                    $"Density candidate VPU {vpu} is missing source or preview evidence.");

            string sourceHash = HeroVoxelProvider.HashFile(sourcePath);
            string previewHash = HeroVoxelProvider.HashFile(previewPath);
            if (!string.Equals(sourceHash, audit.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Density candidate VPU {vpu} source evidence is stale.");
            if (!string.Equals(previewHash, audit.PreviewSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Density candidate VPU {vpu} preview evidence is stale.");

            candidates.Add(ReferenceDensitySelector.ValidateCandidate(
                input,
                audit,
                policy.Budget,
                HeroVoxelProvider.HashFile(auditPath)));
        }

        int? selected = ReferenceDensitySelector.SelectLowestSufficient(sequence, candidates);
        string referenceAuditPath = Path.Combine(asset.ReferenceDirectory, "audit.generated.json");
        return new ReferenceDensityEvidence
        {
            Version = input.Version,
            ReferenceAuditSha256 = HeroVoxelProvider.HashFile(referenceAuditPath),
            DensityInputSha256 = HeroVoxelProvider.HashFile(inputPath),
            RulerVersion = ruler?.Version,
            RulerSha256 = rulerSha256,
            TargetWorldLongAxis = input.TargetWorldLongAxis,
            ApparentVoxelCounts = input.Version == 2
                ? input.Parts.ToDictionary(
                    x => x.Key,
                    x => x.Value.ApparentVoxelCount,
                    StringComparer.Ordinal)
                : new Dictionary<string, double>(StringComparer.Ordinal),
            EstimatedVpu = ReferenceDensitySelector.EstimateVpu(input),
            CandidateSequence = sequence.ToList(),
            Candidates = candidates.OrderBy(x => x.Vpu).ToList(),
            SelectedVpu = selected,
            SelectionReason = selected.HasValue
                ? input.Version == 2
                    ? "lowest evaluated ruler candidate preserving reference cadence, meaningful construction, world extent, and hard constraints"
                    : "lowest evaluated candidate preserving all declared features and satisfying hard constraints"
                : input.Version == 2
                    ? "no evaluated ruler candidate preserves reference cadence and meaningful construction within hard constraints"
                    : "no evaluated candidate preserves all declared features within hard constraints"
        };
    }


    public ReferenceStyleAuthority ResolveReferenceStyle(string assetKind)
    {
        StorePolicy policy = ContractYaml.Load<StorePolicy>(Path.Combine(_root, "policy", "store.yaml"));
        ValidatePolicy(policy);
        return ResolveReferenceStyle(policy.ReferenceStyle, assetKind);
    }

    public ReferenceStyleAuthority ResolveTargetReferenceStyle(string assetKind)
    {
        StorePolicy policy = ContractYaml.Load<StorePolicy>(Path.Combine(_root, "policy", "store.yaml"));
        ValidatePolicy(policy);
        return ResolveReferenceStyle(policy.ReferenceStyle, assetKind, targetAuthority: true);
    }

    internal string ReferenceStyleAuthoritySha256(ReferenceStyleAuthority style) =>
        BuildStyleAuthoritySha256(style);

    private AssetHealth EvaluateBaseHealth(
        AssetRecord asset,
        StorePolicy policy,
        IReadOnlyDictionary<string, CompatibilityProfile> profiles)
    {
        var health = new AssetHealth
        {
            Id = asset.Manifest.Id,
            Kind = asset.Manifest.Kind,
            UnitRef = asset.Manifest.UnitRef,
            Form = asset.Manifest.Form,
            Profile = asset.Manifest.Family ?? asset.Manifest.Mount?.Profile,
            Capability = asset.Manifest.Mount?.Capability
        };
        AddGate(health, "manifest", () => ValidateManifest(asset, policy, profiles));
        AddGate(health, "reference", () => ValidateReference(asset, policy));
        if (IsReferenceDrivenDensity(asset.Manifest))
            AddGate(health, "density", () => ValidateDensityEvidence(asset, policy));
        AddGate(health, "source", () => ValidateSource(asset));
        HeroVoxelProvider.CompiledAsset? compiled = null;
        AddGate(health, "generation", () =>
        {
            compiled = _provider.Compile(asset, writeGenerated: false);
            ReferenceContract reference = ContractYaml.Load<ReferenceContract>(
                Path.Combine(asset.ReferenceDirectory, "reference.yaml"));
            string[] expectedParts = reference.RequiredParts.OrderBy(x => x, StringComparer.Ordinal).ToArray();
            string[] compiledParts = compiled.Compilation.Parts.Select(x => x.Name)
                .OrderBy(x => x, StringComparer.Ordinal).ToArray();
            if (!expectedParts.SequenceEqual(compiledParts, StringComparer.Ordinal))
                throw new InvalidDataException(
                    "Compiled rigid parts do not exactly match the accepted RequiredParts reference contract.");
            byte[] expected = MagicaVoxelWriter.WriteToBytes(compiled.Compilation);
            if (!File.Exists(compiled.GeneratedVoxPath))
                throw new InvalidDataException("Generated VOX is missing.");
            if (!File.ReadAllBytes(compiled.GeneratedVoxPath).SequenceEqual(expected))
                throw new InvalidDataException("Generated VOX is stale or manually modified.");
            if (compiled.GeneratedAnimationPath != null)
            {
                string expectedAnimation = GeneratedAnimationManifestWriter.Serialize(compiled.Compilation);
                if (!File.Exists(compiled.GeneratedAnimationPath) ||
                    File.ReadAllText(compiled.GeneratedAnimationPath) != expectedAnimation)
                    throw new InvalidDataException("Generated animation manifest is stale or missing.");
            }
            return compiled.GeneratedVoxPath;
        });
        AddGate(health, "proof", () => ValidateProof(asset));
        AddGate(health, "budget", () =>
        {
            compiled ??= _provider.Compile(asset, writeGenerated: false);
            int voxels = compiled.Compilation.Parts.Sum(x => x.Voxels.Count);
            ValidatePublicationBudget(voxels, compiled.Compilation.Parts.Count, policy.Budget);
            return $"{voxels} voxels, {compiled.Compilation.Parts.Count} parts";
        });

        health.BasePublished = health.Gates.All(x => x.Passed);
        health.Published = health.BasePublished;
        health.FailedGate = health.Gates.FirstOrDefault(x => !x.Passed)?.Id;
        return health;
    }

    private void AddGate(AssetHealth health, string id, Func<string?> action)
    {
        if (health.Gates.Any(x => !x.Passed))
        {
            health.Gates.Add(new GateResult(id, false, "Blocked by earlier gate."));
            return;
        }
        try
        {
            health.Gates.Add(new GateResult(id, true, "PASS", NormalizeEvidence(action())));
        }
        catch (Exception ex)
        {
            health.Gates.Add(new GateResult(id, false, ex.Message));
        }
    }

    private string? NormalizeEvidence(string? evidence)
    {
        if (string.IsNullOrWhiteSpace(evidence) || !Path.IsPathRooted(evidence))
            return evidence;

        string full = Path.GetFullPath(evidence);
        string rootPrefix = _root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            return evidence;

        return Path.GetRelativePath(_root, full).Replace('\\', '/');    }

    private static string ValidateManifest(
        AssetRecord asset,
        StorePolicy policy,
        IReadOnlyDictionary<string, CompatibilityProfile> profiles)
    {
        AssetManifest m = asset.Manifest;
        if (!string.Equals(m.Style.Family, policy.StyleFamily, StringComparison.Ordinal))
            throw new InvalidDataException($"Style family '{m.Style.Family}' does not match store policy '{policy.StyleFamily}'.");
        if (m.Kind == "avatar")
        {
            if (string.IsNullOrWhiteSpace(m.Family) || !profiles.ContainsKey(m.Family))
                throw new InvalidDataException("Avatar must declare an existing compatibility family.");
            if (m.Mount != null)
                throw new InvalidDataException("Avatar cannot declare equipment mount metadata.");
        }
        else
        {
            if (m.Mount == null || !profiles.TryGetValue(m.Mount.Profile, out CompatibilityProfile? profile))
                throw new InvalidDataException("Equipment must declare an existing compatibility profile.");
            if (profile.Version != 2 ||
                !profile.RequiredMountCapabilities.Concat(profile.OptionalMountCapabilities)
                    .Any(x => x.Capability == m.Mount.Capability))
                throw new InvalidDataException($"Equipment capability '{m.Mount.Capability}' is not declared by profile '{profile.Id}'.");
            if (!string.IsNullOrWhiteSpace(m.Family))
                throw new InvalidDataException("Equipment compatibility belongs in mount.profile, not family.");
        }
        return asset.ManifestPath;
    }

    private string ValidateReference(AssetRecord asset, StorePolicy policy)
    {
        string auditPath = Path.Combine(asset.ReferenceDirectory, "audit.generated.json");
        if (!File.Exists(auditPath))
            throw new InvalidDataException("Reference audit evidence is missing.");
        ReferenceContract contract = ValidateReferenceContractStructure(asset);
        if (contract.Version == 3)
        {
            ValidateTargetProvenance(asset);
            ValidateFidelityAudit(asset);
        }

        ReferenceStyleAuthority style = ResolveReferenceStyle(
            policy.ReferenceStyle,
            asset.Manifest.Kind);
        VerifyAudit(
            asset.ReferenceDirectory,
            auditPath,
            "reference",
            EnumerateReferenceFiles(asset));
        VerifyReferenceProvenance(auditPath, style);
        return auditPath;
    }

    internal ReferenceContract ValidateReferenceContractStructure(AssetRecord asset)
    {
        string contractPath = Path.Combine(asset.ReferenceDirectory, "reference.yaml");
        if (!File.Exists(contractPath))
            throw new InvalidDataException("Reference contract is missing.");

        ReferenceContract contract = ContractYaml.Load<ReferenceContract>(contractPath);
        if (contract.Version is not (2 or 3))
            throw new InvalidDataException(
                "Reference contract must use version 2 legacy or version 3 target-driven authority.");
        if (contract.RequiredParts.Count == 0 ||
            contract.RequiredParts.Distinct(StringComparer.Ordinal).Count() != contract.RequiredParts.Count)
            throw new InvalidDataException("RequiredParts must be non-empty and unique.");

        ValidateReferenceMapping(asset, contract);
        string composition = ResolveContained(
            asset.ReferenceDirectory,
            contract.Composition,
            "Composition Sheet");
        PngInfo compositionInfo = ReadPngInfo(composition);
        if ((long)compositionInfo.Width * 2 != (long)compositionInfo.Height * 3)
            throw new InvalidDataException("Composition Sheet PNG must use exact 3:2 aspect ratio.");
        foreach (string part in contract.RequiredParts)
            ReadPngInfo(ResolveContained(
                asset.ReferenceDirectory,
                contract.PartSheets[part],
                $"Part Sheet '{part}'"));

        if (contract.Version == 2)
        {
            if (contract.Target != null || !string.IsNullOrWhiteSpace(contract.Fidelity))
                throw new InvalidDataException(
                    "Reference contract version 2 cannot declare target-driven fields.");
            return contract;
        }

        if (contract.Target == null)
            throw new InvalidDataException(
                "Reference contract version 3 requires a Product Target mapping.");
        if (string.IsNullOrWhiteSpace(contract.Target.Image) ||
            string.IsNullOrWhiteSpace(contract.Target.Provenance) ||
            string.IsNullOrWhiteSpace(contract.Fidelity))
            throw new InvalidDataException(
                "Reference contract version 3 requires target image, target provenance, and fidelity mappings.");

        string targetImage = ResolveContained(
            asset.ReferenceDirectory,
            contract.Target.Image,
            "Product Target");
        if (!string.Equals(Path.GetExtension(targetImage), ".png", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Product Target must be a PNG file.");
        ReadPngInfo(targetImage);

        string provenance = ResolveContained(
            asset.ReferenceDirectory,
            contract.Target.Provenance,
            "Product Target provenance");
        string fidelity = ResolveContained(
            asset.ReferenceDirectory,
            contract.Fidelity,
            "Target fidelity evidence");
        if (!string.Equals(Path.GetExtension(provenance), ".json", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(Path.GetExtension(fidelity), ".json", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                "Product Target provenance and fidelity evidence must be JSON files.");
        if (!File.Exists(provenance))
            throw new InvalidDataException("Product Target provenance is missing.");
        if (!File.Exists(fidelity))
            throw new InvalidDataException("Target fidelity evidence is missing.");

        string[] allAuthorityPaths = new[]
        {
            targetImage,
            provenance,
            fidelity,
            composition
        }.Concat(contract.RequiredParts.Select(part =>
            ResolveContained(
                asset.ReferenceDirectory,
                contract.PartSheets[part],
                $"Part Sheet '{part}'"))).ToArray();
        if (allAuthorityPaths.Distinct(StringComparer.OrdinalIgnoreCase).Count() != allAuthorityPaths.Length)
            throw new InvalidDataException(
                "Target, provenance, fidelity, Composition, and Part mappings must resolve to unique files.");

        return contract;
    }

    internal ReferenceTargetProvenance ValidateTargetProvenance(AssetRecord asset)
    {
        ReferenceContract contract = ValidateReferenceContractStructure(asset);
        if (contract.Version != 3 || contract.Target == null)
            throw new InvalidDataException(
                "Product Target provenance applies only to reference contract version 3.");

        string targetPath = ResolveContained(
            asset.ReferenceDirectory,
            contract.Target.Image,
            "Product Target");
        string provenancePath = ResolveContained(
            asset.ReferenceDirectory,
            contract.Target.Provenance,
            "Product Target provenance");
        ReferenceTargetProvenance provenance =
            JsonSerializer.Deserialize<ReferenceTargetProvenance>(
                File.ReadAllText(provenancePath),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("Product Target provenance is invalid.");

        if (provenance.Version is not (1 or 2))
            throw new InvalidDataException("Product Target provenance must use version 1 or 2.");
        if (!IsSha256(provenance.TargetSha256))
            throw new InvalidDataException("Product Target provenance targetSha256 is invalid.");
        string targetHash = HeroVoxelProvider.HashFile(targetPath);
        if (!string.Equals(
                targetHash,
                provenance.TargetSha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Product Target provenance is stale for target.png.");

        if (string.Equals(provenance.Origin, "generated", StringComparison.Ordinal))
        {
            if (provenance.Source != null)
                throw new InvalidDataException(
                    "Generated Product Target provenance cannot declare imported source evidence.");
            if (provenance.Version == 1 && provenance.Generator != null)
                throw new InvalidDataException(
                    "Generated Product Target provenance v1 cannot declare generator evidence.");
            if (provenance.Version == 2)
            {
                ReferenceTargetGeneratorProvenance generator = provenance.Generator
                    ?? throw new InvalidDataException(
                        "Generated Product Target provenance v2 requires image-model generator evidence.");
                if (!string.Equals(generator.Kind, "image-model", StringComparison.Ordinal))
                    throw new InvalidDataException(
                        "Generated Product Target provenance v2 generator kind must be 'image-model'.");
                if (string.IsNullOrWhiteSpace(generator.Provider) ||
                    string.IsNullOrWhiteSpace(generator.Model))
                    throw new InvalidDataException(
                        "Generated Product Target provenance v2 requires nonempty provider and model.");
                if (generator.Request == null ||
                    string.IsNullOrWhiteSpace(generator.Request.Path) ||
                    !IsSha256(generator.Request.Sha256))
                    throw new InvalidDataException(
                        "Generated Product Target provenance v2 requires request path/hash evidence.");

                string requestPath = ResolveContained(
                    asset.ReferenceDirectory,
                    generator.Request.Path,
                    "Product Target generation request");
                if (!File.Exists(requestPath))
                    throw new InvalidDataException(
                        "Product Target generation request is missing.");
                string requestHash = HeroVoxelProvider.HashFile(requestPath);
                if (!string.Equals(
                        requestHash,
                        generator.Request.Sha256,
                        StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException(
                        "Product Target generation request provenance is stale.");
            }
            ReferenceStyleAuthority targetStyle =
                ResolveTargetReferenceStyle(asset.Manifest.Kind);
            string expectedAuthority = BuildStyleAuthoritySha256(targetStyle);
            if (!IsSha256(provenance.TargetStyleAuthoritySha256) ||
                !string.Equals(
                    provenance.TargetStyleAuthoritySha256,
                    expectedAuthority,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    "Product Target generated style authority is stale.");

            Dictionary<string, string> expectedHashes = BuildStyleHashes(targetStyle);
            if (provenance.TargetStyleSha256 == null ||
                provenance.TargetStyleSha256.Count != expectedHashes.Count ||
                expectedHashes.Any(pair =>
                    !provenance.TargetStyleSha256.TryGetValue(pair.Key, out string? actual) ||
                    !string.Equals(actual, pair.Value, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidDataException(
                    "Product Target generated style evidence is stale.");
            return provenance;
        }

        if (string.Equals(provenance.Origin, "imported", StringComparison.Ordinal))
        {
            if (provenance.Version != 1)
                throw new InvalidDataException(
                    "Imported Product Target provenance must use version 1.");
            if (provenance.Generator != null)
                throw new InvalidDataException(
                    "Imported Product Target provenance cannot declare generator evidence.");
            if (provenance.Source == null ||
                string.IsNullOrWhiteSpace(provenance.Source.Path) ||
                !IsSha256(provenance.Source.Sha256) ||
                provenance.Source.Relation is not ("exact" or "canonical-derivative"))
                throw new InvalidDataException(
                    "Imported Product Target provenance requires source path/hash and exact or canonical-derivative relation.");
            if (!string.IsNullOrWhiteSpace(provenance.TargetStyleAuthoritySha256) ||
                provenance.TargetStyleSha256 is { Count: > 0 })
                throw new InvalidDataException(
                    "Imported Product Target provenance cannot declare generated style authority.");

            string sourcePath = ResolveContained(
                asset.ReferenceDirectory,
                provenance.Source.Path,
                "Product Target imported source");
            if (!File.Exists(sourcePath))
                throw new InvalidDataException("Product Target imported source is missing.");
            string sourceHash = HeroVoxelProvider.HashFile(sourcePath);
            if (!string.Equals(
                    sourceHash,
                    provenance.Source.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    "Product Target imported source provenance is stale.");
            if (string.Equals(provenance.Source.Relation, "exact", StringComparison.Ordinal) &&
                !string.Equals(sourceHash, targetHash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    "Exact imported Product Target must match the source bytes.");
            return provenance;
        }

        throw new InvalidDataException(
            "Product Target provenance origin must be 'generated' or 'imported'.");
    }

    internal ReferenceFidelityAudit ValidateFidelityAudit(AssetRecord asset)
    {
        ReferenceContract contract = ValidateReferenceContractStructure(asset);
        if (contract.Version != 3 || contract.Target == null)
            throw new InvalidDataException(
                "Target fidelity evidence applies only to reference contract version 3.");

        string fidelityPath = ResolveContained(
            asset.ReferenceDirectory,
            contract.Fidelity,
            "Target fidelity evidence");
        ReferenceFidelityAudit audit =
            JsonSerializer.Deserialize<ReferenceFidelityAudit>(
                File.ReadAllText(fidelityPath),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("Target fidelity evidence is invalid.");

        if (audit.Version != 1)
            throw new InvalidDataException("Target fidelity evidence must use version 1.");

        string expectedTargetPath = contract.Target.Image.Replace('\\', '/');
        if (!string.Equals(audit.Target.Path, expectedTargetPath, StringComparison.Ordinal))
            throw new InvalidDataException(
                "Target fidelity evidence must reference the declared Product Target exactly.");
        if (!IsSha256(audit.Target.Sha256))
            throw new InvalidDataException(
                "Target fidelity Product Target hash is invalid.");
        string targetPath = ResolveContained(
            asset.ReferenceDirectory,
            contract.Target.Image,
            "Product Target");
        if (!string.Equals(
                HeroVoxelProvider.HashFile(targetPath),
                audit.Target.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                "Target fidelity Product Target evidence is stale.");

        string[] expectedSheets = new[] { contract.Composition }
            .Concat(contract.RequiredParts.Select(part => contract.PartSheets[part]))
            .Select(path => path.Replace('\\', '/'))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        string[] actualSheets = audit.Sheets.Keys
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        if (!expectedSheets.SequenceEqual(actualSheets, StringComparer.Ordinal))
            throw new InvalidDataException(
                "Target fidelity evidence must cover exactly the declared Composition and Part Sheets.");

        string[] requiredCriteria = RequiredFidelityCriteria
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
        foreach (string relative in expectedSheets)
        {
            ReferenceFidelitySheetEvidence evidence = audit.Sheets[relative];
            if (!IsSha256(evidence.Sha256))
                throw new InvalidDataException(
                    $"Target fidelity sheet hash is invalid for '{relative}'.");

            string path = ResolveContained(
                asset.ReferenceDirectory,
                relative,
                "Target fidelity audited sheet");
            if (!string.Equals(
                    HeroVoxelProvider.HashFile(path),
                    evidence.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Target fidelity evidence is stale for '{relative}'.");

            string[] actualCriteria = evidence.Criteria.Keys
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
            if (!requiredCriteria.SequenceEqual(actualCriteria, StringComparer.Ordinal))
                throw new InvalidDataException(
                    $"Target fidelity criteria for '{relative}' must match the required criteria exactly.");
            foreach (string criterion in RequiredFidelityCriteria)
                if (!string.Equals(
                        evidence.Criteria[criterion],
                        "pass",
                        StringComparison.Ordinal))
                    throw new InvalidDataException(
                        $"Target fidelity criterion '{criterion}' failed for '{relative}'.");
        }

        return audit;
    }

    private static bool IsSha256(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 64)
            return false;
        foreach (char c in value)
            if (!Uri.IsHexDigit(c))
                return false;
        return true;
    }

    private static string ValidateSource(AssetRecord asset)
    {
        string spec = asset.SourceBase + ".voxel.yaml";
        if (!File.Exists(spec) || !File.Exists(asset.PalettePath))
            throw new InvalidDataException("Authoritative VoxelSpec or palette is missing.");
        if (IsReferenceDrivenDensity(asset.Manifest))
        {
            string evidencePath = Path.Combine(asset.RootDirectory, "Density", "evidence.generated.json");
            ReferenceDensityEvidence evidence =
                JsonSerializer.Deserialize<ReferenceDensityEvidence>(File.ReadAllText(evidencePath), JsonOptions)
                ?? throw new InvalidDataException("Density evidence is invalid.");
            if (!evidence.SelectedVpu.HasValue)
                throw new InvalidDataException("Density evidence has no selected VPU.");
            string? vpuText = ReadTopLevelYamlScalar(File.ReadAllText(spec), "voxelsPerUnit");
            if (!int.TryParse(vpuText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int sourceVpu) || sourceVpu <= 0)
                throw new InvalidDataException("Authoritative VoxelSpec voxelsPerUnit is invalid.");
            if (sourceVpu != evidence.SelectedVpu.Value)
                throw new InvalidDataException($"Authoritative VoxelSpec VPU {sourceVpu} does not match selected density VPU {evidence.SelectedVpu.Value}.");
        }
        return spec;
    }

    private static string ValidateProof(AssetRecord asset)
    {
        string standalone = Path.Combine(asset.ProofDirectory, "Standalone");
        string parts = Path.Combine(asset.ProofDirectory, "Parts", "parts", "manifest.json");
        string audit = Path.Combine(asset.ProofDirectory, "audit.generated.json");
        foreach (string file in new[]
                 {
                     Path.Combine(standalone, "hero.png"),
                     Path.Combine(standalone, "front.png"),
                     Path.Combine(standalone, "preview-poses.json"),
                     parts,
                     audit
                 })
            if (!File.Exists(file))
                throw new InvalidDataException($"Required proof missing: {Path.GetRelativePath(asset.RootDirectory, file)}");
        VerifyAudit(
            asset.RootDirectory,
            audit,
            "proof",
            EnumerateProofInputs(asset));
        return audit;
    }

    private Dictionary<string, CompatibilityProfile> LoadProfiles()
    {
        string root = Path.Combine(_root, "policy", "families");
        var profiles = new Dictionary<string, CompatibilityProfile>(StringComparer.Ordinal);
        if (!Directory.Exists(root))
            return profiles;
        foreach (string path in Directory.GetFiles(root, "*.yaml").OrderBy(x => x, StringComparer.Ordinal))
        {
            CompatibilityProfile profile = ContractYaml.Load<CompatibilityProfile>(path);
            ValidateProfile(profile, path);
            if (!profiles.TryAdd(profile.Id, profile))
                throw new InvalidDataException($"Duplicate compatibility profile '{profile.Id}'.");
            ValidateHistoricalProfile(profile, path);
        }
        return profiles;
    }

    private List<CompatibilityResult> EvaluateCompatibility(
        IReadOnlyList<AssetRecord> assets,
        IReadOnlyDictionary<string, AssetHealth> health,
        IReadOnlyDictionary<string, CompatibilityProfile> profiles,
        bool write)
    {
        var result = new List<CompatibilityResult>();
        foreach (AssetRecord avatar in assets.Where(x => x.Manifest.Kind == "avatar" && health[x.Manifest.Id].BasePublished))
        {
            CompatibilityProfile profile = profiles[avatar.Manifest.Family!];
            HeroVoxelProvider.CompiledAsset avatarCompiled = _provider.Compile(avatar, false);
            Dictionary<string, Dictionary<string, PartWorldFrame>> poseSamples = LoadPoseSamples(avatar);

            foreach (MountCapabilityContract required in profile.RequiredMountCapabilities)
            {
                foreach (string attachment in required.Sockets)
                {
                    CompiledSocket? socket = avatarCompiled.Compilation.Sockets.SingleOrDefault(x => x.Name == attachment);
                    string? failure = socket == null
                        ? "required attachment missing"
                        : avatarCompiled.Compilation.VoxelsPerUnit != profile.VoxelsPerUnit
                            ? "voxelsPerUnit mismatch"
                            : !SocketMatchesContract(socket, required.Forward, required.Up)
                                ? "attachment frame mismatch"
                                : null;
                    if (failure != null)
                    {
                        result.Add(new CompatibilityResult(
                            avatar.Manifest.Id, "", profile.Id, required.Capability, attachment, "bind", false, failure));
                    }
                }
            }

            foreach (AssetRecord equipment in assets.Where(x =>
                         x.Manifest.Kind == "equipment" &&
                         x.Manifest.Mount?.Profile == profile.Id &&
                         health[x.Manifest.Id].BasePublished))
            {
                HeroVoxelProvider.CompiledAsset equipmentCompiled = _provider.Compile(equipment, false);
                bool equipmentFrameValid =
                    IsEquipmentMountFrameValid(equipmentCompiled.Compilation);
                string evidencePath = Path.Combine(
                    avatar.ProofDirectory, "Compatibility", Sanitize(equipment.Manifest.Id) + ".json");
                string evidenceRef = NormalizeEvidence(evidencePath)!;
                var sampleRows = new List<object>();

                string capability = equipment.Manifest.Mount!.Capability!;
                MountCapabilityContract contract = profile.RequiredMountCapabilities
                    .Concat(profile.OptionalMountCapabilities)
                    .Single(x => x.Capability == capability);
                IEnumerable<(string Attachment, float[] Forward, float[] Up)> attachments =
                    contract.Sockets.Select(x => (x, contract.Forward, contract.Up)).ToArray();

                foreach ((string attachment, float[] forward, float[] up) in attachments)
                {
                    CompiledSocket? socket = avatarCompiled.Compilation.Sockets.SingleOrDefault(x => x.Name == attachment);
                    bool socketFrameValid = socket != null && SocketMatchesContract(socket, forward, up);

                    if (profile.RepresentativePoseSamples.Count == 0)
                    {
                        bool pass = socket != null && socketFrameValid && equipmentFrameValid;
                        string detail = CompatibilityDetail(socket, socketFrameValid, equipmentFrameValid, null);
                        result.Add(new CompatibilityResult(
                            avatar.Manifest.Id, equipment.Manifest.Id, profile.Id, capability, attachment, "bind", pass, evidenceRef));
                        sampleRows.Add(new
                        {
                            capability,
                            attachment,
                            pose = "bind",
                            passed = pass,
                            detail,
                            socketParent = socket?.ParentName,
                            socketTranslation = SocketTranslation(socket)
                        });
                        continue;
                    }

                    foreach (PoseSampleContract sample in profile.RepresentativePoseSamples)
                    {
                        string key = PoseKey(sample.Clip, sample.NormalizedTime);
                        SocketWorldFrame? frame = socket == null ||
                                                  !poseSamples.TryGetValue(key, out Dictionary<string, PartWorldFrame>? parts)
                            ? null
                            : ResolveSocketWorldFrame(avatarCompiled.Compilation, socket, parts);
                        bool pass = socket != null && socketFrameValid && equipmentFrameValid && frame != null;
                        string detail = CompatibilityDetail(socket, socketFrameValid, equipmentFrameValid, frame == null ? key : null);
                        result.Add(new CompatibilityResult(
                            avatar.Manifest.Id, equipment.Manifest.Id, profile.Id, capability, attachment, key, pass, evidenceRef));
                        sampleRows.Add(new
                        {
                            capability,
                            attachment,
                            pose = key,
                            passed = pass,
                            detail,
                            socketParent = socket?.ParentName,
                            socketTranslation = SocketTranslation(socket),
                            frame = frame == null ? null : new
                            {
                                origin = Vector(frame.Origin),
                                worldX = Vector(frame.WorldX),
                                worldY = Vector(frame.WorldY),
                                worldZ = Vector(frame.WorldZ)
                            }
                        });
                    }
                }

                if (write)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(evidencePath)!);
                    File.WriteAllText(evidencePath, JsonSerializer.Serialize(new
                    {
                        avatar = avatar.Manifest.Id,
                        equipment = equipment.Manifest.Id,
                        profile = profile.Id,
                        capability,
                        samples = sampleRows
                    }, JsonOptions));
                }
            }
        }
        return result;
    }

    internal static bool IsEquipmentMountFrameValid(VoxelCompilation compilation) =>
        compilation.VoxelsPerUnit > 0 && compilation.AssemblyOrigin != null;

    internal static void ValidatePublicationBudget(int voxels, int parts, StoreBudget budget)
    {
        if (voxels > budget.MaxVoxelsPerAsset)
            throw new InvalidDataException($"Voxel budget exceeded: {voxels}>{budget.MaxVoxelsPerAsset}.");
        if (parts > budget.MaxPartsPerAsset)
            throw new InvalidDataException($"Part budget exceeded: {parts}>{budget.MaxPartsPerAsset}.");
    }

    private static string CompatibilityDetail(
        CompiledSocket? socket,
        bool socketFrameValid,
        bool equipmentFrameValid,
        string? unavailablePose)
    {
        if (socket == null)
            return "required attachment missing";
        if (!socketFrameValid)
            return "attachment frame mismatch";
        if (!equipmentFrameValid)
            return "equipment mount frame is incompatible";
        if (unavailablePose != null)
            return $"pose/socket frame unavailable: {unavailablePose}";
        return "PASS";
    }

    private static float[]? SocketTranslation(CompiledSocket? socket) =>
        socket == null
            ? null
            : new float[]
            {
                socket.Transform.Translation.X,
                socket.Transform.Translation.Y,
                socket.Transform.Translation.Z
            };

    private static bool SocketMatchesContract(CompiledSocket socket, float[] forward, float[] up)
    {
        Quaternion rotation = Euler(socket.Transform.Rotation);
        Vector3 actualForward = Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, rotation));
        Vector3 actualUp = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, rotation));
        Vector3 expectedForward = Vector3.Normalize(new Vector3(forward[0], forward[1], forward[2]));
        Vector3 expectedUp = Vector3.Normalize(new Vector3(up[0], up[1], up[2]));
        return Vector3.Distance(actualForward, expectedForward) < 0.001f &&
               Vector3.Distance(actualUp, expectedUp) < 0.001f;
    }


    private static Dictionary<string, Dictionary<string, PartWorldFrame>> LoadPoseSamples(AssetRecord avatar)
    {
        string path = Path.Combine(avatar.ProofDirectory, "Standalone", "preview-poses.json");
        var samples = new Dictionary<string, Dictionary<string, PartWorldFrame>>(StringComparer.Ordinal);
        if (!File.Exists(path))
            return samples;
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        if (!document.RootElement.TryGetProperty("Samples", out JsonElement sampleArray))
            return samples;
        foreach (JsonElement sample in sampleArray.EnumerateArray())
        {
            string clip = sample.GetProperty("Clip").GetString() ?? "";
            float time = sample.GetProperty("NormalizedTime").GetSingle();
            var parts = new Dictionary<string, PartWorldFrame>(StringComparer.Ordinal);
            foreach (JsonElement part in sample.GetProperty("Parts").EnumerateArray())
            {
                string name = part.GetProperty("Part").GetString() ?? "";
                parts.Add(name, new PartWorldFrame(
                    ReadVector(part, "WorldOrigin"),
                    ReadVector(part, "WorldX"),
                    ReadVector(part, "WorldY"),
                    ReadVector(part, "WorldZ")));
            }
            samples.Add(PoseKey(clip, time), parts);
        }
        return samples;
    }

    private static SocketWorldFrame? ResolveSocketWorldFrame(
        VoxelCompilation compilation,
        CompiledSocket socket,
        IReadOnlyDictionary<string, PartWorldFrame> sampledParts)
    {
        if (!sampledParts.TryGetValue(socket.ParentName, out PartWorldFrame? parentFrame))
            return null;
        CompiledPart? parentPart = compilation.Parts.SingleOrDefault(x => x.Name == socket.ParentName);
        if (parentPart == null)
            return null;

        Vector3 basisX = parentFrame.WorldX - parentFrame.Origin;
        Vector3 basisY = parentFrame.WorldY - parentFrame.Origin;
        Vector3 basisZ = parentFrame.WorldZ - parentFrame.Origin;
        var socketFromPivot = new Vector3(
            socket.Transform.Translation.X - parentPart.Pivot.X,
            socket.Transform.Translation.Y - parentPart.Pivot.Y,
            socket.Transform.Translation.Z - parentPart.Pivot.Z);
        Vector3 origin = parentFrame.Origin + TransformDirection(socketFromPivot, basisX, basisY, basisZ);

        Quaternion rotation = Euler(socket.Transform.Rotation);
        Vector3 localX = Vector3.Transform(Vector3.UnitX, rotation);
        Vector3 localY = Vector3.Transform(Vector3.UnitY, rotation);
        Vector3 localZ = Vector3.Transform(Vector3.UnitZ, rotation);
        return new SocketWorldFrame(
            origin,
            origin + TransformDirection(localX, basisX, basisY, basisZ),
            origin + TransformDirection(localY, basisX, basisY, basisZ),
            origin + TransformDirection(localZ, basisX, basisY, basisZ));
    }

    private static Vector3 TransformDirection(
        Vector3 local,
        Vector3 basisX,
        Vector3 basisY,
        Vector3 basisZ) =>
        (basisX * local.X) + (basisY * local.Y) + (basisZ * local.Z);

    private static Quaternion Euler(Float3 degrees)
    {
        float x = degrees.X * (MathF.PI / 180f);
        float y = degrees.Y * (MathF.PI / 180f);
        float z = degrees.Z * (MathF.PI / 180f);
        Quaternion qx = Quaternion.CreateFromAxisAngle(Vector3.UnitX, x);
        Quaternion qy = Quaternion.CreateFromAxisAngle(Vector3.UnitY, y);
        Quaternion qz = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, z);
        return Quaternion.Normalize(qy * qx * qz);
    }

    private static Vector3 ReadVector(JsonElement owner, string property)
    {
        float[] value = owner.GetProperty(property).EnumerateArray().Select(x => x.GetSingle()).ToArray();
        if (value.Length != 3 || value.Any(x => !float.IsFinite(x)))
            throw new InvalidDataException($"Pose report property '{property}' is not a finite 3D vector.");
        return new Vector3(value[0], value[1], value[2]);
    }

    private static float[] Vector(Vector3 value) => new[] { value.X, value.Y, value.Z };

    private static List<CoverageGap> EvaluateCoverage(
        CoveragePolicy coverage,
        IReadOnlyList<AssetRecord> assets,
        IReadOnlyDictionary<string, AssetHealth> health)
    {
        var gaps = new List<CoverageGap>();
        foreach (CoverageRequirement requirement in coverage.Requirements)
        {
            bool satisfied = assets.Any(asset =>
                health[asset.Manifest.Id].Published &&
                asset.Manifest.Kind == requirement.Kind &&
                (requirement.Profile == null ||
                 asset.Manifest.Family == requirement.Profile ||
                 asset.Manifest.Mount?.Profile == requirement.Profile) &&
                (requirement.Capability == null || asset.Manifest.Mount?.Capability == requirement.Capability) &&
                (requirement.Form == null || asset.Manifest.Form == requirement.Form));
            if (!satisfied)
                gaps.Add(new CoverageGap(
                    requirement.Id,
                    requirement.Kind,
                    requirement.Profile,
                    requirement.Capability,
                    requirement.Form,
                    requirement.Required));
        }
        return gaps;
    }

    private static MaintenanceJob? PlanNext(
        IReadOnlyList<AssetRecord> assets,
        IReadOnlyDictionary<string, AssetHealth> health,
        IReadOnlyList<CoverageGap> gaps,
        StorePolicy policy)
    {
        foreach (AssetRecord asset in assets.OrderBy(x => x.Manifest.Id, StringComparer.Ordinal))
        {
            AssetHealth h = health[asset.Manifest.Id];
            if (h.Published)
                continue;
            string gate = h.FailedGate ?? "unknown";
            string priority = gate switch
            {
                "manifest" => "invalid-authority",
                "reference" or "density" or "source" or "proof" or "budget" => "regressed-evidence",
                "compatibility" => "compatibility-failure",
                "generation" => "stale-reproducible",
                _ => "regressed-evidence"
            };
            return new MaintenanceJob(priority, "repair", asset.Manifest.Id, $"failed gate: {gate}");
        }

        CoverageGap? required = gaps.FirstOrDefault(x => x.Required);
        if (required != null)
            return new MaintenanceJob("required-coverage", "design", required.Id, $"missing required {required.Kind} coverage");
        CoverageGap? optional = gaps.FirstOrDefault(x => !x.Required);
        if (optional != null && policy.AllowOptionalExpansion)
            return new MaintenanceJob("optional-expansion", "design", optional.Id, $"missing optional {optional.Kind} coverage");
        return null;
    }

    private void WriteDerived(StoreSnapshot snapshot)
    {
        string catalogDir = Path.Combine(_root, "catalog");
        Directory.CreateDirectory(catalogDir);
        File.WriteAllText(Path.Combine(catalogDir, "catalog.generated.json"), JsonSerializer.Serialize(new
        {
            store = snapshot.Store,
            assets = snapshot.Assets.Where(x => x.Published).Select(x => new
            {
                x.Id,
                x.Kind,
                x.UnitRef,
                x.Form,
                x.Profile,
                x.Capability
            }).OrderBy(x => x.Id).ToArray()
        }, JsonOptions));
        File.WriteAllText(
            Path.Combine(catalogDir, "health.generated.json"),
            JsonSerializer.Serialize(snapshot, JsonOptions));
    }

    private static IEnumerable<string> EnumerateReferenceFiles(AssetRecord asset)
    {
        string contractPath = Path.Combine(asset.ReferenceDirectory, "reference.yaml");
        ReferenceContract contract = ContractYaml.Load<ReferenceContract>(contractPath);
        yield return contractPath;
        if (contract.Version == 3 && contract.Target != null)
        {
            yield return ResolveContained(
                asset.ReferenceDirectory,
                contract.Target.Image,
                "Product Target");
            yield return ResolveContained(
                asset.ReferenceDirectory,
                contract.Target.Provenance,
                "Product Target provenance");
        }
        yield return ResolveContained(asset.ReferenceDirectory, contract.Composition, "Composition Sheet");
        foreach (string part in contract.RequiredParts)
            yield return ResolveContained(asset.ReferenceDirectory, contract.PartSheets[part], $"Part Sheet '{part}'");
        if (contract.Version == 3)
            yield return ResolveContained(
                asset.ReferenceDirectory,
                contract.Fidelity,
                "Target fidelity evidence");
    }
    private void WriteReferenceAudit(AssetRecord asset, StorePolicy policy)
    {
        ReferenceContract contract = ValidateReferenceContractStructure(asset);
        if (contract.Version == 3)
        {
            ValidateTargetProvenance(asset);
            ValidateFidelityAudit(asset);
        }

        ReferenceStyleAuthority style = ResolveReferenceStyle(
            policy.ReferenceStyle,
            asset.Manifest.Kind);
        HeroVoxelLock lockFile = ContractYaml.Load<HeroVoxelLock>(
            Path.Combine(_root, "toolchain", "hero-voxel.lock.yaml"));
        var audit = new AuditEvidence
        {
            Kind = "reference",
            ReferenceStyle = style.Id,
            StyleKind = style.Kind,
            StyleAuthoritySha256 = BuildStyleAuthoritySha256(style),
            HeroVoxelWorkflowSha256 = lockFile.WorkflowSha256,
            StyleSha256 = new Dictionary<string, string>(StringComparer.Ordinal)
        };
        foreach (string file in EnumerateReferenceFiles(asset))
        {
            if (!File.Exists(file))
                throw new FileNotFoundException("Cannot seal missing reference evidence.", file);
            audit.Sha256[Path.GetRelativePath(asset.ReferenceDirectory, file).Replace('\\', '/')] =
                HeroVoxelProvider.HashFile(file);
        }
        foreach (string file in EnumerateApplicableStyleReferences(style))
            audit.StyleSha256![Path.GetRelativePath(_root, file).Replace('\\', '/')] =
                HeroVoxelProvider.HashFile(file);
        Directory.CreateDirectory(asset.ReferenceDirectory);
        File.WriteAllText(
            Path.Combine(asset.ReferenceDirectory, "audit.generated.json"),
            JsonSerializer.Serialize(audit, JsonOptions));
    }

    private void WriteProofAudit(AssetRecord asset)
    {
        var audit = new AuditEvidence { Kind = "proof" };
        foreach (string file in EnumerateProofInputs(asset))
        {
            if (!File.Exists(file))
                throw new FileNotFoundException("Cannot seal missing proof input.", file);
            audit.Sha256[Path.GetRelativePath(asset.RootDirectory, file).Replace('\\', '/')] =
                HeroVoxelProvider.HashFile(file);
        }
        Directory.CreateDirectory(asset.ProofDirectory);
        File.WriteAllText(
            Path.Combine(asset.ProofDirectory, "audit.generated.json"),
            JsonSerializer.Serialize(audit, JsonOptions));
    }

    private static IEnumerable<string> EnumerateProofInputs(AssetRecord asset)
    {
        yield return asset.ManifestPath;
        yield return asset.SourceBase + ".voxel.yaml";
        yield return asset.PalettePath;
        yield return asset.SourceBase + ".generated.vox";
        string animation = asset.SourceBase + GeneratedAnimationManifestWriter.Suffix;
        if (File.Exists(animation))
            yield return animation;
        string standalone = Path.Combine(asset.ProofDirectory, "Standalone");
        if (Directory.Exists(standalone))
            foreach (string file in Directory.GetFiles(standalone, "*", SearchOption.TopDirectoryOnly)
                         .OrderBy(x => x, StringComparer.Ordinal))
                yield return file;
        string parts = Path.Combine(asset.ProofDirectory, "Parts");
        if (Directory.Exists(parts))
            foreach (string file in Directory.GetFiles(parts, "*", SearchOption.AllDirectories)
                         .OrderBy(x => x, StringComparer.Ordinal))
                yield return file;
    }

    private static void VerifyAudit(
        string baseDirectory,
        string auditPath,
        string expectedKind,
        IEnumerable<string> expectedFiles)
    {
        AuditEvidence audit = JsonSerializer.Deserialize<AuditEvidence>(File.ReadAllText(auditPath), JsonOptions)
                              ?? throw new InvalidDataException("Audit evidence is invalid.");
        if (!string.Equals(audit.Kind, expectedKind, StringComparison.Ordinal))
            throw new InvalidDataException($"Unexpected audit kind '{audit.Kind}'.");
        if (audit.Sha256.Count == 0)
            throw new InvalidDataException("Audit evidence contains no file hashes.");
        string[] expectedKeys = expectedFiles
            .Select(path => Path.GetRelativePath(baseDirectory, path).Replace('\\', '/'))
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
        string[] actualKeys = audit.Sha256.Keys
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
        if (!expectedKeys.SequenceEqual(actualKeys, StringComparer.Ordinal))
            throw new InvalidDataException(
                "Audit evidence does not cover the exact required input set.");
        foreach ((string relative, string expected) in audit.Sha256)
        {
            string path = Path.Combine(baseDirectory, relative.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
                throw new InvalidDataException($"Audited file is missing: {relative}");
            string actual = HeroVoxelProvider.HashFile(path);
            if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Audit evidence is stale for '{relative}'.");
        }
    }

    private void VerifyReferenceProvenance(
        string auditPath,
        ReferenceStyleAuthority style)
    {
        AuditEvidence audit =
            JsonSerializer.Deserialize<AuditEvidence>(File.ReadAllText(auditPath), JsonOptions)
            ?? throw new InvalidDataException("Reference audit evidence is invalid.");
        if (!string.Equals(audit.ReferenceStyle, style.Id, StringComparison.Ordinal))
            throw new InvalidDataException("Reference audit style authority is stale.");
        if (!string.Equals(audit.StyleKind, style.Kind, StringComparison.Ordinal))
            throw new InvalidDataException("Reference audit style kind is stale.");
        string expectedAuthority = BuildStyleAuthoritySha256(style);
        if (!string.Equals(
                audit.StyleAuthoritySha256,
                expectedAuthority,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Reference audit style authority digest is stale.");

        HeroVoxelLock lockFile = ContractYaml.Load<HeroVoxelLock>(
            Path.Combine(_root, "toolchain", "hero-voxel.lock.yaml"));
        if (!string.Equals(
                audit.HeroVoxelWorkflowSha256,
                lockFile.WorkflowSha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Reference audit Hero.Voxel workflow authority is stale.");

        Dictionary<string, string> expected = BuildStyleHashes(style);
        if (audit.StyleSha256 == null ||
            audit.StyleSha256.Count != expected.Count ||
            expected.Any(pair =>
                !audit.StyleSha256.TryGetValue(pair.Key, out string? actual) ||
                !string.Equals(actual, pair.Value, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("Reference audit style evidence is stale.");
    }

    private Dictionary<string, string> BuildStyleHashes(ReferenceStyleAuthority style)
    {
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string file in EnumerateApplicableStyleReferences(style))
            hashes[Path.GetRelativePath(_root, file).Replace('\\', '/')] =
                HeroVoxelProvider.HashFile(file);
        return hashes;
    }

    private string BuildStyleAuthoritySha256(ReferenceStyleAuthority style)
    {
        string manifestDirectory = Path.GetDirectoryName(style.ManifestPath)!;
        var builder = new StringBuilder();
        builder.Append("style=").Append(style.Id).Append('\n');
        builder.Append("kind=").Append(style.Kind).Append('\n');
        foreach (string trait in style.Traits)
            builder.Append("trait=").Append(trait).Append('\n');
        foreach (string file in style.CommonReferencePaths)
        {
            string relative = Path.GetRelativePath(manifestDirectory, file).Replace('\\', '/');
            builder.Append("common=").Append(relative).Append('|')
                .Append(HeroVoxelProvider.HashFile(file)).Append('\n');
        }
        foreach (string file in style.KindReferencePaths)
        {
            string relative = Path.GetRelativePath(manifestDirectory, file).Replace('\\', '/');
            builder.Append("kindref=").Append(relative).Append('|')
                .Append(HeroVoxelProvider.HashFile(file)).Append('\n');
        }
        byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static IEnumerable<string> EnumerateApplicableStyleReferences(
        ReferenceStyleAuthority style)
    {
        foreach (string file in style.CommonReferencePaths)
            yield return file;
        foreach (string file in style.KindReferencePaths)
            yield return file;
    }

    private ReferenceStyleAuthority ResolveReferenceStyle(
        string styleId,
        string assetKind) =>
        ResolveReferenceStyle(styleId, assetKind, targetAuthority: false);

    private ReferenceStyleAuthority ResolveReferenceStyle(
        string styleId,
        string assetKind,
        bool targetAuthority)
    {
        if (string.IsNullOrWhiteSpace(styleId))
            throw new InvalidDataException("Store referenceStyle is required.");
        if (assetKind is not ("avatar" or "equipment"))
            throw new InvalidDataException(
                $"Reference style does not support asset kind '{assetKind}'.");

        string styleBase = Path.Combine(_root, "Art", "Style");
        string manifestPath = ResolveContained(
            styleBase,
            styleId.Replace('\\', '/') + "/style.yaml",
            "Reference style manifest");
        if (!File.Exists(manifestPath))
            throw new InvalidDataException($"Reference style manifest is missing for '{styleId}'.");

        ReferenceStyleManifest manifest = ContractYaml.Load<ReferenceStyleManifest>(manifestPath);
        if (manifest.Version is not (2 or 3) ||
            !string.Equals(manifest.Id, styleId, StringComparison.Ordinal))
            throw new InvalidDataException($"Reference style manifest '{styleId}' has invalid identity/version.");
        if (manifest.Traits.Count == 0 || manifest.Traits.Any(string.IsNullOrWhiteSpace))
            throw new InvalidDataException("Reference style traits must be non-empty.");
        if (manifest.TraitsByKind.Values.SelectMany(x => x).Any(string.IsNullOrWhiteSpace))
            throw new InvalidDataException("Scoped reference style traits must not contain empty values.");

        Dictionary<string, List<string>> scopes = targetAuthority
            ? manifest.TargetReferences
            : manifest.References;
        if (targetAuthority && manifest.Version != 3)
            throw new InvalidDataException(
                $"Reference style manifest '{styleId}' must use version 3 for targetReferences.");
        if (!scopes.TryGetValue(assetKind, out List<string>? kindReferences) ||
            kindReferences.Count == 0)
            throw new InvalidDataException(
                $"Reference style {(targetAuthority ? "target " : string.Empty)}scope '{assetKind}' must be non-empty.");
        scopes.TryGetValue("common", out List<string>? commonReferences);
        commonReferences ??= new List<string>();

        string manifestDirectory = Path.GetDirectoryName(manifestPath)!;
        string scopePrefix = targetAuthority ? "target " : string.Empty;
        List<string> common = ResolveStyleScope(
            manifestDirectory,
            scopePrefix + "common",
            commonReferences);
        List<string> kind = ResolveStyleScope(
            manifestDirectory,
            scopePrefix + assetKind,
            kindReferences);
        if (common.Concat(kind).Distinct(StringComparer.OrdinalIgnoreCase).Count() !=
            common.Count + kind.Count)
            throw new InvalidDataException(
                "Applicable reference style image paths must be unique across scopes.");

        manifest.TraitsByKind.TryGetValue(assetKind, out List<string>? scopedTraits);
        scopedTraits ??= new List<string>();
        List<string> applicableTraits = manifest.Traits.Concat(scopedTraits).ToList();
        if (applicableTraits.Distinct(StringComparer.Ordinal).Count() != applicableTraits.Count)
            throw new InvalidDataException("Applicable reference style traits must be unique across shared and kind scopes.");

        return new ReferenceStyleAuthority(
            manifest.Id,
            assetKind,
            manifestPath,
            common,
            kind,
            applicableTraits);
    }
    private static List<string> ResolveStyleScope(
        string manifestDirectory,
        string scope,
        IEnumerable<string> references)
    {
        var resolved = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string relative in references)
        {
            if (!string.Equals(Path.GetExtension(relative), ".png", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Reference style scope '{scope}' images must be PNG files.");
            string path = ResolveContained(manifestDirectory, relative, "Reference style image");
            if (!seen.Add(path))
                throw new InvalidDataException(
                    $"Reference style scope '{scope}' image paths must be unique.");
            ReadPngInfo(path);
            resolved.Add(path);
        }
        return resolved;
    }

    private static void ValidateReferenceMapping(AssetRecord asset, ReferenceContract contract)
    {
        if (string.IsNullOrWhiteSpace(contract.Composition))
            throw new InvalidDataException("Composition Sheet mapping is required.");
        string[] expectedParts =
            contract.RequiredParts.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        string[] actualParts =
            contract.PartSheets.Keys.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        if (!expectedParts.SequenceEqual(actualParts, StringComparer.Ordinal))
            throw new InvalidDataException("Part Sheet mapping must match RequiredParts exactly.");

        string composition =
            ResolveContained(asset.ReferenceDirectory, contract.Composition, "Composition Sheet");
        if (!string.Equals(Path.GetExtension(composition), ".png", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Composition Sheet must be a PNG file.");

        var uniquePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            composition
        };
        foreach (string part in contract.RequiredParts)
        {
            string relative = contract.PartSheets[part];
            if (string.IsNullOrWhiteSpace(relative))
                throw new InvalidDataException($"Part Sheet mapping is empty for '{part}'.");
            string sheet =
                ResolveContained(asset.ReferenceDirectory, relative, $"Part Sheet '{part}'");
            if (!string.Equals(Path.GetExtension(sheet), ".png", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Part Sheet '{part}' must be a PNG file.");
            if (!uniquePaths.Add(sheet))
                throw new InvalidDataException(
                    "Composition and Part Sheet mappings must resolve to unique files.");
        }
    }

    private static PngInfo ReadPngInfo(string path)
    {
        if (!File.Exists(path))
            throw new InvalidDataException($"Required PNG is missing: {path}");
        byte[] header = new byte[24];
        using FileStream stream = File.OpenRead(path);
        if (stream.Read(header, 0, header.Length) != header.Length)
            throw new InvalidDataException($"PNG is truncated: {path}");
        ReadOnlySpan<byte> signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        if (!header.AsSpan(0, 8).SequenceEqual(signature) ||
            BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(8, 4)) != 13 ||
            !header.AsSpan(12, 4).SequenceEqual("IHDR"u8))
            throw new InvalidDataException($"Invalid PNG signature/IHDR: {path}");
        uint width = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(16, 4));
        uint height = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(20, 4));
        if (width == 0 || height == 0 || width > int.MaxValue || height > int.MaxValue)
            throw new InvalidDataException($"PNG dimensions are invalid: {path}");
        return new PngInfo((int)width, (int)height);
    }

    private static string ResolveContained(string root, string relative, string label)
    {
        if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative))
            throw new InvalidDataException($"{label} path must be a non-empty relative path.");
        string fullRoot =
            Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
            Path.DirectorySeparatorChar;
        string full = Path.GetFullPath(
            Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"{label} path escapes its authority root: {relative}");
        return full;
    }

    private AssetRecord RequireAsset(string assetId) =>
        DiscoverAssets().SingleOrDefault(x => x.Manifest.Id == assetId)
        ?? throw new InvalidOperationException($"Unknown asset id '{assetId}'.");

    private static bool IsReferenceDrivenDensity(AssetManifest manifest) =>
        string.Equals(manifest.Density, "reference-driven", StringComparison.Ordinal);

    private static void ValidateManifestShape(AssetManifest m, string path)
    {
        if (m.Version != 2)
            throw new InvalidDataException($"Active asset manifests must use version 2 in {path}.");
        if (string.IsNullOrWhiteSpace(m.Id) || string.IsNullOrWhiteSpace(m.UnitRef))
            throw new InvalidDataException($"Asset id and UnitRef are required in {path}.");
        if (m.Kind is not ("avatar" or "equipment"))
            throw new InvalidDataException($"Unsupported asset kind '{m.Kind}' in {path}.");
        if (string.IsNullOrWhiteSpace(m.Source.Base) || string.IsNullOrWhiteSpace(m.Source.Palette))
            throw new InvalidDataException($"Asset source base/palette are required in {path}.");
        if (Path.IsPathRooted(m.Source.Base) || Path.IsPathRooted(m.Source.Palette))
            throw new InvalidDataException($"Asset source paths must be repository-portable relative paths in {path}.");

        if (m.Kind == "avatar")
        {
            if (m.Mount != null || !string.IsNullOrWhiteSpace(m.Form) || !string.IsNullOrWhiteSpace(m.Density))
                throw new InvalidDataException($"Avatar manifest cannot declare equipment form/mount/density metadata in {path}.");
            return;
        }

        if (m.Mount == null || string.IsNullOrWhiteSpace(m.Mount.Profile))
            throw new InvalidDataException($"Equipment mount profile is required in {path}.");
        if (string.IsNullOrWhiteSpace(m.Mount.Capability) ||
            string.IsNullOrWhiteSpace(m.Form))
            throw new InvalidDataException($"Equipment must declare form plus logical capability in {path}.");
        if (!string.IsNullOrWhiteSpace(m.Density) && !IsReferenceDrivenDensity(m))
            throw new InvalidDataException($"Equipment density must be 'reference-driven' when declared in {path}.");
    }

    private static void ValidatePolicy(StorePolicy policy)
    {
        if (policy.Version != 1 ||
            string.IsNullOrWhiteSpace(policy.Name) ||
            string.IsNullOrWhiteSpace(policy.StyleFamily) ||
            string.IsNullOrWhiteSpace(policy.ReferenceStyle))
            throw new InvalidDataException("Store policy is incomplete.");
        if (policy.Budget.MaxVoxelsPerAsset <= 0 || policy.Budget.MaxPartsPerAsset <= 0)
            throw new InvalidDataException("Store budgets must be positive.");
    }

    private static void ValidateCoverage(CoveragePolicy policy)
    {
        if (policy.Version != 2 || string.IsNullOrWhiteSpace(policy.Foundation))
            throw new InvalidDataException("Active coverage policy must use version 2.");
        if (policy.Requirements.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != policy.Requirements.Count)
            throw new InvalidDataException("Coverage requirement ids must be unique.");

        foreach (CoverageRequirement requirement in policy.Requirements)
        {
            if (requirement.Kind is not ("avatar" or "equipment"))
                throw new InvalidDataException($"Unsupported coverage kind '{requirement.Kind}'.");

            if (string.IsNullOrWhiteSpace(requirement.Profile))
                throw new InvalidDataException("V2 coverage requires a profile.");
            if (requirement.Kind == "equipment")
            {
                if (string.IsNullOrWhiteSpace(requirement.Capability) || string.IsNullOrWhiteSpace(requirement.Form))
                    throw new InvalidDataException("V2 equipment coverage requires capability and form.");
            }
            else if (!string.IsNullOrWhiteSpace(requirement.Capability) || !string.IsNullOrWhiteSpace(requirement.Form))
            {
                throw new InvalidDataException("V2 avatar coverage cannot declare equipment capability/form.");
            }
        }
    }

    private static void ValidateProfile(CompatibilityProfile profile, string path)
    {
        if (profile.Version != 2 || string.IsNullOrWhiteSpace(profile.Id) || profile.VoxelsPerUnit <= 0)
            throw new InvalidDataException($"Active compatibility profile must use version 2: {path}");

        if (profile.RequiredMountCapabilities.Count == 0)
            throw new InvalidDataException($"Compatibility profile requires at least one mount capability: {path}");
        var capabilities = new HashSet<string>(StringComparer.Ordinal);
        var sockets = new HashSet<string>(StringComparer.Ordinal);
        foreach (MountCapabilityContract mount in profile.RequiredMountCapabilities.Concat(profile.OptionalMountCapabilities))
        {
            if (string.IsNullOrWhiteSpace(mount.Capability) || !capabilities.Add(mount.Capability))
                throw new InvalidDataException($"Duplicate/empty mount capability '{mount.Capability}' in {path}.");
            if (mount.Sockets.Count == 0 || mount.Sockets.Any(string.IsNullOrWhiteSpace))
                throw new InvalidDataException($"Mount capability '{mount.Capability}' requires physical socket instances in {path}.");
            if (mount.Sockets.Distinct(StringComparer.Ordinal).Count() != mount.Sockets.Count)
                throw new InvalidDataException($"Mount capability '{mount.Capability}' contains duplicate socket instances in {path}.");
            foreach (string socket in mount.Sockets)
                if (!sockets.Add(socket))
                    throw new InvalidDataException($"Physical socket instance '{socket}' is assigned more than once in {path}.");
            ValidateFrameContract(mount.Capability, mount.Forward, mount.Up, mount.Envelope, path);
        }
    }

    private static void ValidateFrameContract(
        string label,
        float[] forward,
        float[] up,
        float[] envelope,
        string path)
    {
        if (forward.Length != 3 || up.Length != 3 || envelope.Length != 3 ||
            forward.Any(x => !float.IsFinite(x)) || up.Any(x => !float.IsFinite(x)) ||
            envelope.Any(x => !float.IsFinite(x)))
            throw new InvalidDataException($"Mount '{label}' must define finite 3D forward/up/envelope vectors in {path}.");
        if (envelope.Any(x => x <= 0))
            throw new InvalidDataException($"Mount '{label}' envelope components must be positive in {path}.");

        var forwardVector = new Vector3(forward[0], forward[1], forward[2]);
        var upVector = new Vector3(up[0], up[1], up[2]);
        if (MathF.Abs(forwardVector.Length() - 1f) > 0.001f ||
            MathF.Abs(upVector.Length() - 1f) > 0.001f ||
            MathF.Abs(Vector3.Dot(forwardVector, upVector)) > 0.001f)
            throw new InvalidDataException($"Mount '{label}' forward/up vectors must be normalized and orthogonal in {path}.");
    }

    private void ValidateHistoricalIdentity(AssetRecord asset)
    {
        string relative = Path.GetRelativePath(_root, asset.ManifestPath).Replace('\\', '/');        string? previous = GitShow(relative);
        if (previous == null)
            return;

        HistoricalAssetIdentity old = ParseHistoricalAssetIdentity(previous);
        bool identityChanged =
            !string.Equals(old.Id, asset.Manifest.Id, StringComparison.Ordinal) ||
            !string.Equals(old.UnitRef, asset.Manifest.UnitRef, StringComparison.Ordinal);
        if (identityChanged)
        {
            bool explicitlyRetired = LoadRetiredIdentities().Any(x =>
                string.Equals(x.Id, old.Id, StringComparison.Ordinal) &&
                string.Equals(x.UnitRef, old.UnitRef, StringComparison.Ordinal));
            if (!explicitlyRetired)
                throw new InvalidDataException(
                    $"Published semantic identity changed in-place at {relative}; create a new asset identity or explicit migration.");
            return;
        }

        if (old.Version == 2 &&
            !string.Equals(old.Form, asset.Manifest.Form, StringComparison.Ordinal))
            throw new InvalidDataException(
                $"Published equipment form changed in-place at {relative}; create an explicit migration.");
    }

    private sealed record HistoricalAssetIdentity(int Version, string Id, string UnitRef, string? Form);

    private static HistoricalAssetIdentity ParseHistoricalAssetIdentity(string yaml)
    {
        string versionText = ReadTopLevelYamlScalar(yaml, "version")
            ?? throw new InvalidDataException("Historical asset manifest is missing version.");
        if (!int.TryParse(versionText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int version))
            throw new InvalidDataException("Historical asset manifest version is invalid.");

        string id = ReadTopLevelYamlScalar(yaml, "id")
            ?? throw new InvalidDataException("Historical asset manifest is missing id.");
        string unitRef = ReadTopLevelYamlScalar(yaml, "unitRef")
            ?? throw new InvalidDataException("Historical asset manifest is missing UnitRef.");

        return new HistoricalAssetIdentity(
            version,
            id,
            unitRef,
            ReadTopLevelYamlScalar(yaml, "form"));
    }

    private static string? ReadTopLevelYamlScalar(string yaml, string key)
    {
        string prefix = key + ":";
        foreach (string raw in yaml.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (raw.Length == 0 || char.IsWhiteSpace(raw[0]) ||
                !raw.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            string value = raw[prefix.Length..].Trim();
            if (value.Length == 0)
                return null;
            return value.Trim('"', '\'');
        }
        return null;
    }
    private void ValidateHistoricalProfile(CompatibilityProfile profile, string path)
    {
        string relative = Path.GetRelativePath(_root, path).Replace('\\', '/');
        string? previous = GitShow(relative);
        if (previous == null)
            return;
        CompatibilityProfile old = ContractYaml.Parse<CompatibilityProfile>(previous);
        string oldJson = JsonSerializer.Serialize(old);
        string currentJson = JsonSerializer.Serialize(profile);
        if (!string.Equals(oldJson, currentJson, StringComparison.Ordinal))
            throw new InvalidDataException(
                $"Compatibility profile '{profile.Id}' changed in-place; publish a new profile version.");
    }

    private string? GitShow(string relative)
    {
        var psi = new ProcessStartInfo("git")
        {
            WorkingDirectory = _root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        psi.ArgumentList.Add("show");
        psi.ArgumentList.Add($"HEAD:{relative}");
        using Process process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start git.");
        string stdout = process.StandardOutput.ReadToEnd();
        process.StandardError.ReadToEnd();
        process.WaitForExit();
        return process.ExitCode == 0 ? stdout : null;
    }

    private static void EnsureUnique(
        IEnumerable<AssetRecord> assets,
        Func<AssetRecord, string> selector,
        string label)
    {
        string? duplicate = assets
            .GroupBy(selector, StringComparer.Ordinal)
            .FirstOrDefault(x => x.Count() > 1)?.Key;
        if (duplicate != null)
            throw new InvalidDataException($"Duplicate {label}: {duplicate}");
    }

    private string ResolveRelative(string root, string relative)
    {
        string fullRoot = Path.GetFullPath(_root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string full = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Relative path escapes repository root: {relative}");
        return full;
    }

    private static string Sanitize(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (char c in value)
            sb.Append(char.IsLetterOrDigit(c) || c is '.' or '-' or '_' ? c : '_');
        return sb.ToString();
    }

    private static string PoseKey(string clip, float time) =>
        clip + "@" + time.ToString("0.###", CultureInfo.InvariantCulture);
}
