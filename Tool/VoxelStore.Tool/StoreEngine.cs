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
    string? Form,
    bool Required);
public sealed record MaintenanceJob(string Priority, string Kind, string Target, string Reason);
public sealed record CompatibilityResult(
    string Avatar,
    string Equipment,
    string Capability,
    string Socket,
    string Side,
    string Motion,
    float Phase,
    bool Passed,
    string Detail,
    string Evidence);

internal sealed record CompatibilityPoseEvaluation(
    Matrix4x4 BodySocket,
    Matrix4x4 AttachmentReference,
    Matrix4x4 EvaluatedMount,
    float ReferenceError,
    float AnimatedMountDrift,
    bool Passed,
    string Detail);

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
    public string? AttachmentCapability { get; set; }
    public string? AttachmentSide { get; set; }
    public string? AttachmentAnchor { get; set; }
    public List<string> SlotInstances { get; } = new();
    public Dictionary<string, int> SlotCounts { get; } = new(StringComparer.Ordinal);
    public List<string> SemanticMotions { get; } = new();
    public List<SemanticMotionCoverage> SemanticMotionCoverage { get; } = new();
    public bool RequiredV1Complete { get; set; }
    public List<string> MissingRequiredV1 { get; } = new();
    public List<string> AliasedRequiredV1 { get; } = new();
    public bool BasePublished { get; set; }
    public bool Published { get; set; }
    public string? FailedGate { get; set; }
    public List<GateResult> Gates { get; } = new();
}

public sealed class StoreSnapshot
{
    public string Store { get; set; } = string.Empty;
    public string Collection { get; set; } = string.Empty;
    public List<AssetHealth> Assets { get; set; } = new();
    public List<CoverageGap> CoverageGaps { get; set; } = new();
    public List<CompatibilityResult> Compatibility { get; set; } = new();
    public MaintenanceJob? NextJob { get; set; }
    public bool Healthy => Assets.All(x => x.Published) && CoverageGaps.All(x => !x.Required);
}
public sealed record PreviewPlanUnit(
    string Id,
    string Kind,
    string UnitRef,
    string SourceBase,
    string Palette,
    string GeneratedVox,
    string? GeneratedAnimation);
public sealed class PreviewPlan
{
    public List<PreviewPlanUnit> Units { get; set; } = new();
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
    private static readonly string[] RequiredSemanticMotions =
        SemanticAnimationClipName.RequiredMotions;
    private static readonly HashSet<string> AttachmentCapabilities =
        new(StringComparer.Ordinal) { "Hand", "Back", "Head", "Hip", "Chest" };
    private static readonly HashSet<string> AttachmentSides =
        new(StringComparer.Ordinal) { "Any", "Left", "Right" };
    private static readonly HashSet<string> SlotSides =
        new(StringComparer.Ordinal) { "Left", "Right", "Center" };
    private readonly string _root;
    private readonly HeroVoxelProvider _provider;

    public StoreEngine(string root, string? providerRoot = null)
    {
        _root = Path.GetFullPath(root);
        _provider = new HeroVoxelProvider(_root, providerRoot);
    }
    public string Root => _root;

    public static string FindRoot(string? start = null)
    {
        string current = Path.GetFullPath(start ?? Environment.CurrentDirectory);
        while (true)
        {
            if (File.Exists(Path.Combine(current, "Store", "policy.yaml")))
                return current;
            DirectoryInfo? parent = Directory.GetParent(current);
            if (parent == null)
                throw new DirectoryNotFoundException("Could not locate voxel-store root (Store/policy.yaml).");
            current = parent.FullName;
        }
    }

    public IReadOnlyList<AssetRecord> DiscoverAssets()
    {
        string assetsRoot = Path.Combine(_root, "Assets");
        if (!Directory.Exists(assetsRoot))
            return Array.Empty<AssetRecord>();

        var assets = new List<AssetRecord>();
        foreach (string manifestPath in Directory.GetFiles(assetsRoot, "asset.yaml", SearchOption.AllDirectories)
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
        string path = Path.Combine(_root, "Store", "retired-assets.yaml");
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

    private StorePolicy LoadStorePolicy()
    {
        StorePolicy policy = ContractYaml.Load<StorePolicy>(Path.Combine(_root, "Store", "policy.yaml"));
        ValidatePolicy(policy);
        return policy;
    }

    private CollectionManifest LoadCollection(StorePolicy policy)
    {
        string collectionRoot = Path.Combine(_root, "Collections");
        string collectionPath = ResolveContained(
            collectionRoot,
            policy.Collection + ".yaml",
            "Collection manifest");
        CollectionManifest collection = ContractYaml.Load<CollectionManifest>(collectionPath);
        ValidateCollection(collection);
        if (!string.Equals(collection.Id, policy.Collection, StringComparison.Ordinal))
            throw new InvalidDataException(
                $"Selected collection '{policy.Collection}' does not match manifest id '{collection.Id}'.");
        return collection;
    }

    public StoreSnapshot Scan(bool writeDerived, bool writeCompatibility)
    {
        StorePolicy policy = LoadStorePolicy();
        CollectionManifest collection = LoadCollection(policy);
        IReadOnlyList<AssetRecord> assets = DiscoverAssets();
        var healthById = new Dictionary<string, AssetHealth>(StringComparer.Ordinal);

        foreach (AssetRecord asset in assets)
            healthById.Add(asset.Manifest.Id, EvaluateBaseHealth(asset, policy));

        List<CompatibilityResult> compatibility =
            EvaluateCompatibility(assets, healthById, writeCompatibility);

        foreach (AssetHealth health in healthById.Values)
        {
            bool compatPass = compatibility
                .Where(x => x.Avatar == health.Id || x.Equipment == health.Id)
                .All(x => x.Passed);
            health.Published = health.BasePublished && compatPass;
            if (health.BasePublished && !compatPass)
                health.FailedGate = "compatibility";
        }

        List<CoverageGap> gaps = EvaluateCoverage(collection, assets, healthById);
        MaintenanceJob? next = PlanNext(assets, healthById, gaps, policy);
        var snapshot = new StoreSnapshot
        {
            Store = policy.Name,
            Collection = collection.Id,
            Assets = healthById.Values.OrderBy(x => x.Id, StringComparer.Ordinal).ToList(),
            CoverageGaps = gaps,
            Compatibility = compatibility,
            NextJob = next
        };

        if (writeDerived)
            WriteDerived(snapshot);
        return snapshot;
    }
    public PreviewPlan BuildPreviewPlan()
    {
        IReadOnlyList<AssetRecord> assets = DiscoverAssets();
        StoreSnapshot snapshot = Scan(writeDerived: false, writeCompatibility: false);
        return CreatePreviewPlan(_root, assets, snapshot);
    }

    internal static PreviewPlan CreatePreviewPlan(
        string root,
        IReadOnlyList<AssetRecord> assets,
        StoreSnapshot snapshot)
    {
        var byId = assets.ToDictionary(x => x.Manifest.Id, StringComparer.Ordinal);
        return new PreviewPlan
        {
            Units = snapshot.Assets
                .Where(x => x.Published)
                .OrderBy(x => x.Kind, StringComparer.Ordinal)
                .ThenBy(x => x.Id, StringComparer.Ordinal)
                .Select(x =>
                {
                    AssetRecord asset = byId[x.Id];
                    string generatedVox = asset.SourceBase + ".generated.vox";
                    string generatedAnimation =
                        asset.SourceBase + GeneratedAnimationManifestWriter.Suffix;
                    return new PreviewPlanUnit(
                        x.Id,
                        x.Kind,
                        x.UnitRef,
                        ToRepositoryRelative(root, asset.SourceBase),
                        ToRepositoryRelative(root, asset.PalettePath),
                        ToRepositoryRelative(root, generatedVox),
                        File.Exists(generatedAnimation)
                            ? ToRepositoryRelative(root, generatedAnimation)
                            : null);
                })
                .ToList()
        };
    }

    private static string ToRepositoryRelative(string root, string absolutePath)
    {
        string fullRoot = Path.GetFullPath(root);
        string full = Path.GetFullPath(absolutePath);
        string rootPrefix = fullRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Preview source path must remain inside voxel-store: {absolutePath}");

        return Path.GetRelativePath(fullRoot, full)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
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
        WriteReferenceAudit(asset);
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

        StorePolicy policy = LoadStorePolicy();
        ValidateReference(asset);

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
        ValidateReference(asset);
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
            rulerPath = Path.Combine(_root, "Store", "vpu-ruler.yaml");
            if (!File.Exists(rulerPath))
                throw new InvalidDataException("Shared VPU ruler is missing.");
            ruler = ReferenceDensitySelector.LoadRuler(rulerPath);
            rulerSha256 = EvidenceHash.HashFile(rulerPath);
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

            string sourceHash = EvidenceHash.HashFile(sourcePath);
            string previewHash = EvidenceHash.HashFile(previewPath);
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
                EvidenceHash.HashFile(auditPath)));
        }

        int? selected = ReferenceDensitySelector.SelectLowestSufficient(sequence, candidates);
        string referenceAuditPath = Path.Combine(asset.ReferenceDirectory, "audit.generated.json");
        return new ReferenceDensityEvidence
        {
            Version = input.Version,
            ReferenceAuditSha256 = EvidenceHash.HashFile(referenceAuditPath),
            DensityInputSha256 = EvidenceHash.HashFile(inputPath),
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
        StorePolicy policy = LoadStorePolicy();
        return ResolveReferenceStyle(policy.DefaultStyle, assetKind);
    }

    public ReferenceStyleAuthority ResolveTargetReferenceStyle(string assetKind)
    {
        StorePolicy policy = LoadStorePolicy();
        return ResolveReferenceStyle(policy.DefaultStyle, assetKind, targetAuthority: true);
    }

    internal string ReferenceStyleAuthoritySha256(ReferenceStyleAuthority style) =>
        BuildStyleAuthoritySha256(style);

    private AssetHealth EvaluateBaseHealth(
        AssetRecord asset,
        StorePolicy policy)
    {
        var health = new AssetHealth
        {
            Id = asset.Manifest.Id,
            Kind = asset.Manifest.Kind,
            UnitRef = asset.Manifest.UnitRef,
            Form = asset.Manifest.Form,
            AttachmentCapability = asset.Manifest.Attachment?.Capability,
            AttachmentSide = asset.Manifest.Attachment?.Side,
            AttachmentAnchor = asset.Manifest.Attachment?.Anchor
        };
        AddGate(health, "manifest", () => ValidateManifest(asset));
        AddGate(health, "reference", () => ValidateReference(asset));
        if (IsReferenceDrivenDensity(asset.Manifest))
            AddGate(health, "density", () => ValidateDensityEvidence(asset, policy));
        AddGate(health, "source", () => ValidateSource(asset));
        HeroVoxelProvider.CompiledAsset? compiled = null;
        AddGate(health, "generation", () =>
        {
            compiled = _provider.Compile(asset, writeGenerated: false);
            if (asset.Manifest.Kind == "avatar")
                ValidateRigidBodyAvatar(compiled.Compilation);
            else
                ValidateEquipmentCompositionFrame(
                    compiled.Compilation,
                    asset.Manifest.Attachment ?? throw new InvalidDataException("Equipment attachment metadata is missing."));
            ValidateRequiredV1Coverage(
                compiled.Compilation,
                asset.Manifest.Kind);
            PopulateAssetProjection(health, compiled.Compilation);

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
                if (!EvidenceHash.TextFileEquals(
                        compiled.GeneratedAnimationPath,
                        expectedAnimation))
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

        return Path.GetRelativePath(_root, full).Replace('\\', '/');
    }

    private string ValidateManifest(AssetRecord asset)
    {
        AssetManifest m = asset.Manifest;
        ResolveReferenceStyle(m.Style.Family, m.Kind);
        return asset.ManifestPath;
    }

    private string ValidateReference(AssetRecord asset)
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
            asset.Manifest.Style.Family,
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

        if (provenance.Version is not (1 or 2 or ReferenceTargetProvenance.AcquisitionNeutralVersion))
            throw new InvalidDataException(
                "Product Target provenance must use version 1, 2, or 3.");
        if (!IsSha256(provenance.TargetSha256))
            throw new InvalidDataException("Product Target provenance targetSha256 is invalid.");
        string targetHash = EvidenceHash.HashFile(targetPath);
        if (!string.Equals(
                targetHash,
                provenance.TargetSha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Product Target provenance is stale for target.png.");

        if (provenance.Version == ReferenceTargetProvenance.AcquisitionNeutralVersion)
        {
            if (string.Equals(
                    provenance.Origin,
                    ReferenceTargetProvenance.RepositoryAuthoredOrigin,
                    StringComparison.Ordinal))
            {
                if (provenance.Source != null)
                    throw new InvalidDataException(
                        "Repository-authored Product Target provenance cannot declare imported source evidence.");

                ValidateTargetStyleProvenance(
                    asset,
                    provenance,
                    "repository-authored");
                return provenance;
            }

            if (string.Equals(
                    provenance.Origin,
                    ReferenceTargetProvenance.ImportedOrigin,
                    StringComparison.Ordinal))
            {
                if (!string.IsNullOrWhiteSpace(provenance.TargetStyleAuthoritySha256) ||
                    provenance.TargetStyleSha256 is { Count: > 0 })
                    throw new InvalidDataException(
                        "Imported Product Target provenance cannot declare repository-authored style authority.");

                ValidateImportedTargetSourceProvenance(asset, provenance, targetHash);
                return provenance;
            }

            throw new InvalidDataException(
                "Product Target provenance v3 origin must be 'repository-authored' or 'imported'.");
        }

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
                string requestHash = EvidenceHash.HashFile(requestPath);
                if (!string.Equals(
                        requestHash,
                        generator.Request.Sha256,
                        StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException(
                        "Product Target generation request provenance is stale.");
            }

            ValidateTargetStyleProvenance(asset, provenance, "generated");
            return provenance;
        }

        if (string.Equals(
                provenance.Origin,
                ReferenceTargetProvenance.ImportedOrigin,
                StringComparison.Ordinal))
        {
            if (provenance.Version != 1)
                throw new InvalidDataException(
                    "Imported Product Target provenance must use version 1 unless it uses acquisition-neutral version 3.");
            if (provenance.Generator != null)
                throw new InvalidDataException(
                    "Imported Product Target provenance v1 cannot declare generator evidence.");
            if (!string.IsNullOrWhiteSpace(provenance.TargetStyleAuthoritySha256) ||
                provenance.TargetStyleSha256 is { Count: > 0 })
                throw new InvalidDataException(
                    "Imported Product Target provenance cannot declare generated style authority.");

            ValidateImportedTargetSourceProvenance(asset, provenance, targetHash);
            return provenance;
        }

        throw new InvalidDataException(
            "Product Target provenance origin must be 'generated', 'repository-authored', or 'imported'.");
    }

    private void ValidateTargetStyleProvenance(
        AssetRecord asset,
        ReferenceTargetProvenance provenance,
        string originLabel)
    {
        ReferenceStyleAuthority targetStyle =
            ResolveReferenceStyle(
                asset.Manifest.Style.Family,
                asset.Manifest.Kind,
                targetAuthority: true);
        string expectedAuthority = BuildStyleAuthoritySha256(targetStyle);
        if (!IsSha256(provenance.TargetStyleAuthoritySha256) ||
            !string.Equals(
                provenance.TargetStyleAuthoritySha256,
                expectedAuthority,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Product Target {originLabel} style authority is stale.");

        Dictionary<string, string> expectedHashes = BuildStyleHashes(targetStyle);
        if (provenance.TargetStyleSha256 == null ||
            provenance.TargetStyleSha256.Count != expectedHashes.Count ||
            expectedHashes.Any(pair =>
                !provenance.TargetStyleSha256.TryGetValue(pair.Key, out string? actual) ||
                !string.Equals(actual, pair.Value, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException(
                $"Product Target {originLabel} style evidence is stale.");
    }

    private void ValidateImportedTargetSourceProvenance(
        AssetRecord asset,
        ReferenceTargetProvenance provenance,
        string targetHash)
    {
        if (provenance.Source == null ||
            string.IsNullOrWhiteSpace(provenance.Source.Path) ||
            !IsSha256(provenance.Source.Sha256) ||
            provenance.Source.Relation is not ("exact" or "canonical-derivative"))
            throw new InvalidDataException(
                "Imported Product Target provenance requires source path/hash and exact or canonical-derivative relation.");

        string sourcePath = ResolveContained(
            asset.ReferenceDirectory,
            provenance.Source.Path,
            "Product Target imported source");
        if (!File.Exists(sourcePath))
            throw new InvalidDataException("Product Target imported source is missing.");
        string sourceHash = EvidenceHash.HashFile(sourcePath);
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
                EvidenceHash.HashFile(targetPath),
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
                    EvidenceHash.HashFile(path),
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

    private sealed record CompatibilitySample(string Motion, float Phase);
    private sealed record SlotAddress(
        string Capability,
        string Side,
        int Instance,
        CompiledSocket Socket);
    private static readonly CompatibilitySample[] CanonicalCompatibilitySamples =
    {
        new("Idle", 0f),
        new("Idle", 0.5f),
        new("WalkForward", 0.5f),
        new("Attack", 0f),
        new("Attack", 0.5f),
        new("Attack", 1f),
        new("Hit", 0.5f),
        new("Death", 1f)
    };

    private List<CompatibilityResult> EvaluateCompatibility(
        IReadOnlyList<AssetRecord> assets,
        IReadOnlyDictionary<string, AssetHealth> health,
        bool write)
    {
        var result = new List<CompatibilityResult>();
        AssetRecord[] equipmentAssets = assets
            .Where(x => x.Manifest.Kind == "equipment" && health[x.Manifest.Id].BasePublished)
            .OrderBy(x => x.Manifest.Id, StringComparer.Ordinal)
            .ToArray();

        foreach (AssetRecord avatar in assets
                     .Where(x => x.Manifest.Kind == "avatar" && health[x.Manifest.Id].BasePublished)
                     .OrderBy(x => x.Manifest.Id, StringComparer.Ordinal))
        {
            VoxelCompilation avatarCompilation = _provider.Compile(avatar, false).Compilation;
            ValidateRigidBodyAvatar(avatarCompilation);

            foreach (AssetRecord equipment in equipmentAssets)
            {
                VoxelCompilation equipmentCompilation = _provider.Compile(equipment, false).Compilation;
                AssetAttachment attachment =
                    equipment.Manifest.Attachment
                    ?? throw new InvalidDataException("Equipment attachment metadata is missing.");
                ValidateEquipmentCompositionFrame(
                    equipmentCompilation,
                    attachment);
                string evidencePath = Path.Combine(
                    avatar.ProofDirectory,
                    "Compatibility",
                    Sanitize(equipment.Manifest.Id) + ".json");
                string evidenceRef = NormalizeEvidence(evidencePath)!;
                var samples = new List<object>();
                SlotAddress[] compatibleSlots =
                    GetCompatibleSlots(
                        avatarCompilation,
                        attachment)
                    .ToArray();

                if (compatibleSlots.Length == 0)
                {
                    string detail =
                        $"Avatar does not expose a compatible '{attachment.Capability}' " +
                        $"slot for side policy '{attachment.Side}'.";
                    result.Add(new CompatibilityResult(
                        avatar.Manifest.Id,
                        equipment.Manifest.Id,
                        attachment.Capability,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        0f,
                        false,
                        detail,
                        evidenceRef));
                    samples.Add(new
                    {
                        passed = false,
                        detail
                    });
                }
                else
                {
                    int mountSocketIndex =
                        RequireSocketIndex(
                            equipmentCompilation,
                            attachment.Anchor);
                    Matrix4x4 mountBind =
                        VoxelCompiledPoseEvaluator.Evaluate(
                                equipmentCompilation,
                                null,
                                0f)
                            .GetSocketModelMatrix(mountSocketIndex);
                    if (!Matrix4x4.Invert(
                            mountBind,
                            out Matrix4x4 inverseMountBind))
                    {
                        throw new InvalidDataException(
                            "Equipment Mount bind frame is singular.");
                    }

                    foreach (SlotAddress slot in compatibleSlots)
                    foreach (CompatibilitySample sample in CanonicalCompatibilitySamples)
                    {
                        string bodyClipName =
                            ResolveSemanticClipName(
                                avatarCompilation,
                                sample.Motion,
                                slot.Side);
                        string equipmentClipName =
                            ResolveSemanticClipName(
                                equipmentCompilation,
                                sample.Motion,
                                slot.Side);
                        CompiledAnimationClip bodyClip =
                            avatarCompilation.Animations.Single(
                                x => string.Equals(
                                    x.Name,
                                    bodyClipName,
                                    StringComparison.Ordinal));
                        CompiledAnimationClip equipmentClip =
                            equipmentCompilation.Animations.Single(
                                x => string.Equals(
                                    x.Name,
                                    equipmentClipName,
                                    StringComparison.Ordinal));

                        int bodySocketIndex =
                            RequireSocketIndex(
                                avatarCompilation,
                                slot.Socket.Name);
                        CompatibilityPoseEvaluation pose =
                            EvaluateCompatibilityPose(
                                avatarCompilation,
                                bodyClip,
                                bodySocketIndex,
                                equipmentCompilation,
                                equipmentClip,
                                mountSocketIndex,
                                sample.Phase,
                                mountBind,
                                inverseMountBind);

                        result.Add(new CompatibilityResult(
                            avatar.Manifest.Id,
                            equipment.Manifest.Id,
                            attachment.Capability,
                            slot.Socket.Name,
                            slot.Side,
                            sample.Motion,
                            sample.Phase,
                            pose.Passed,
                            pose.Detail,
                            evidenceRef));
                        samples.Add(new
                        {
                            capability = attachment.Capability,
                            socket = slot.Socket.Name,
                            side = slot.Side,
                            motion = sample.Motion,
                            phase = sample.Phase,
                            bodyClip = bodyClipName,
                            equipmentClip = equipmentClipName,
                            bodySocketOrigin = Vector(MatrixOrigin(pose.BodySocket)),
                            attachmentReferenceOrigin =
                                Vector(MatrixOrigin(pose.AttachmentReference)),
                            evaluatedMountOrigin = Vector(MatrixOrigin(pose.EvaluatedMount)),
                            referenceError = pose.ReferenceError,
                            animatedMountDrift = pose.AnimatedMountDrift,
                            passed = pose.Passed,
                            detail = pose.Detail
                        });
                    }
                }

                if (write)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(evidencePath)!);
                    File.WriteAllText(evidencePath, JsonSerializer.Serialize(new
                    {
                        convention = "model-socket-reference-semantic-phase.v2",
                        avatar = avatar.Manifest.Id,
                        equipment = equipment.Manifest.Id,
                        attachment = new
                        {
                            capability = attachment.Capability,
                            side = attachment.Side,
                            anchor = attachment.Anchor
                        },
                        samples
                    }, JsonOptions));
                }
            }
        }

        return result;
    }

    internal static void ValidateRigidBodyAvatar(VoxelCompilation compilation)
    {
        if (compilation.VoxelsPerUnit <= 0)
            throw new InvalidDataException("Rigid-body avatar voxelsPerUnit must be positive.");
        if (compilation.Parts.Count != 1 ||
            !string.Equals(compilation.Parts[0].Name, "Body", StringComparison.Ordinal))
            throw new InvalidDataException("Rigid-body avatar must contain exactly one authored part named 'Body'.");

        CompiledPart body = compilation.Parts[0];
        if (body.ParentName != null ||
            body.Transform.Translation != default ||
            body.Transform.Rotation != default)
            throw new InvalidDataException("Rigid-body avatar Body must be an untransformed root part.");
        if (body.Voxels.Count == 0)
            throw new InvalidDataException("Rigid-body avatar Body must contain voxel geometry.");
        if (compilation.AssemblyOrigin == null ||
            !string.Equals(compilation.AssemblyOrigin.PartName, "Body", StringComparison.Ordinal))
            throw new InvalidDataException("Rigid-body avatar assemblyOrigin must be Body.");

        SlotAddress[] slots = compilation.Sockets
            .Select(ParseSlotAddress)
            .ToArray();
        if (slots.Select(x => x.Socket.Name)
                .Distinct(StringComparer.Ordinal).Count() !=
            slots.Length)
        {
            throw new InvalidDataException(
                "Rigid-body avatar equipment socket names must be unique.");
        }
        if (slots.Any(x =>
                !string.Equals(
                    x.Socket.ParentName,
                    "Body",
                    StringComparison.Ordinal)))
        {
            throw new InvalidDataException(
                "Rigid-body avatar equipment sockets must be parented to Body.");
        }
        if (!slots.Any(x => x.Capability == "Hand" && x.Side == "Left") ||
            !slots.Any(x => x.Capability == "Hand" && x.Side == "Right"))
        {
            throw new InvalidDataException(
                "Rigid-body avatar must expose both Left and Right Hand slot instances.");
        }

        ValidateSemanticAnimationSet(
            compilation,
            requireSideAttackVariants: true);
    }


    internal static void ValidateEquipmentCompositionFrame(
        VoxelCompilation compilation,
        AssetAttachment attachment)
    {
        if (compilation.VoxelsPerUnit <= 0 ||
            compilation.Parts.Count != 1 ||
            compilation.AssemblyOrigin == null)
        {
            throw new InvalidDataException(
                "Equipment must contain one positive-density root part and assembly origin.");
        }

        CompiledPart part = compilation.Parts[0];
        if (part.Voxels.Count == 0 ||
            part.ParentName != null ||
            part.Transform.Translation != default ||
            part.Transform.Rotation != default ||
            !string.Equals(
                compilation.AssemblyOrigin.PartName,
                part.Name,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                "Equipment root part must remain the untransformed assembly origin.");
        }

        if (compilation.Sockets.Count != 1)
            throw new InvalidDataException("Equipment must declare exactly one Mount socket.");
        CompiledSocket mount = compilation.Sockets[0];
        if (!string.Equals(mount.Name, attachment.Anchor, StringComparison.Ordinal) ||
            !string.Equals(mount.Name, "Mount", StringComparison.Ordinal) ||
            !string.Equals(mount.ParentName, part.Name, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                "Equipment Mount socket must match attachment.anchor and be parented to the root part.");
        }
        if (!Near(mount.Transform.Translation, part.Pivot))
        {
            throw new InvalidDataException(
                "Current single-part equipment Mount position must coincide with the authored part pivot.");
        }

        ValidateSemanticAnimationSet(
            compilation,
            requireSideAttackVariants: false);
    }

    internal static CompatibilityPoseEvaluation EvaluateCompatibilityPose(
        VoxelCompilation avatarCompilation,
        CompiledAnimationClip bodyClip,
        int bodySocketIndex,
        VoxelCompilation equipmentCompilation,
        CompiledAnimationClip equipmentClip,
        int mountSocketIndex,
        float phase)
    {
        Matrix4x4 mountBind =
            VoxelCompiledPoseEvaluator.Evaluate(
                    equipmentCompilation,
                    null,
                    0f)
                .GetSocketModelMatrix(mountSocketIndex);
        if (!Matrix4x4.Invert(mountBind, out Matrix4x4 inverseMountBind))
        {
            throw new InvalidDataException(
                "Equipment Mount bind frame is singular.");
        }

        return EvaluateCompatibilityPose(
            avatarCompilation,
            bodyClip,
            bodySocketIndex,
            equipmentCompilation,
            equipmentClip,
            mountSocketIndex,
            phase,
            mountBind,
            inverseMountBind);
    }

    private static CompatibilityPoseEvaluation EvaluateCompatibilityPose(
        VoxelCompilation avatarCompilation,
        CompiledAnimationClip bodyClip,
        int bodySocketIndex,
        VoxelCompilation equipmentCompilation,
        CompiledAnimationClip equipmentClip,
        int mountSocketIndex,
        float phase,
        Matrix4x4 mountBind,
        Matrix4x4 inverseMountBind)
    {
        Matrix4x4 bodySocket =
            VoxelCompiledPoseEvaluator.Evaluate(
                    avatarCompilation,
                    bodyClip,
                    phase)
                .GetSocketModelMatrix(bodySocketIndex);
        Matrix4x4 attachmentReference =
            inverseMountBind * bodySocket;
        Matrix4x4 evaluatedMount =
            VoxelCompiledPoseEvaluator.Evaluate(
                    equipmentCompilation,
                    equipmentClip,
                    phase)
                .GetSocketModelMatrix(mountSocketIndex) *
            attachmentReference;
        Matrix4x4 resolvedBindMount =
            mountBind * attachmentReference;

        bool finite =
            IsFinite(bodySocket) &&
            IsFinite(attachmentReference) &&
            IsFinite(evaluatedMount) &&
            IsFinite(resolvedBindMount);
        float referenceError = finite
            ? MaxMatrixDelta(resolvedBindMount, bodySocket)
            : float.PositiveInfinity;
        float animatedMountDrift = finite
            ? Vector3.Distance(
                MatrixOrigin(bodySocket),
                MatrixOrigin(evaluatedMount))
            : float.PositiveInfinity;
        bool passed =
            finite &&
            referenceError <= 0.0001f;
        string detail = !finite
            ? "Compatibility evaluation produced a non-finite attachment transform."
            : passed
                ? "PASS"
                : "Equipment bind Mount did not resolve to the current body socket reference.";

        return new CompatibilityPoseEvaluation(
            bodySocket,
            attachmentReference,
            evaluatedMount,
            referenceError,
            animatedMountDrift,
            passed,
            detail);
    }

    internal static bool IsEquipmentCompositionFrameValid(VoxelCompilation compilation)
    {
        try
        {
            ValidateEquipmentCompositionFrame(
                compilation,
                new AssetAttachment
                {
                    Capability = "Hand",
                    Side = "Any",
                    Anchor = "Mount"
                });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void PopulateAssetProjection(
        AssetHealth health,
        VoxelCompilation compilation)
    {
        health.SlotInstances.Clear();
        health.SlotCounts.Clear();
        health.SemanticMotions.Clear();
        health.SemanticMotionCoverage.Clear();
        health.MissingRequiredV1.Clear();
        health.AliasedRequiredV1.Clear();

        string[] clipNames = compilation.Animations
            .Select(x => x.Name)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        // Preserve the legacy flat clip-name projection during migration.
        health.SemanticMotions.AddRange(clipNames);

        IReadOnlyList<SemanticMotionCoverage> coverage =
            SemanticAnimationClipName.BuildCoverage(clipNames);
        health.SemanticMotionCoverage.AddRange(coverage);

        health.MissingRequiredV1.AddRange(
            GetMissingRequiredV1Clips(
                compilation,
                health.Kind));
        health.AliasedRequiredV1.AddRange(
            GetAliasedRequiredV1Clips(
                compilation,
                health.Kind));
        health.RequiredV1Complete =
            health.MissingRequiredV1.Count == 0 &&
            health.AliasedRequiredV1.Count == 0;

        if (health.Kind != "avatar")
            return;

        foreach (SlotAddress slot in compilation.Sockets
                     .Select(ParseSlotAddress)
                     .OrderBy(x => x.Capability, StringComparer.Ordinal)
                     .ThenBy(x => SlotSideOrder(x.Side))
                     .ThenBy(x => x.Instance))
        {
            health.SlotInstances.Add(slot.Socket.Name);
            string key = slot.Capability + "." + slot.Side;
            health.SlotCounts[key] =
                health.SlotCounts.TryGetValue(key, out int count)
                    ? count + 1
                    : 1;
        }
    }

    internal static IReadOnlyList<string> ResolveCompatibleSlotNames(
        VoxelCompilation compilation,
        AssetAttachment attachment,
        int requestedCount)
    {
        if (requestedCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedCount),
                "Requested equipment slot count must be positive.");
        }

        SlotAddress[] available =
            GetCompatibleSlots(
                compilation,
                attachment)
            .ToArray();
        if (available.Length < requestedCount)
        {
            throw new InvalidDataException(
                "Insufficient body slot capacity for capability '" +
                attachment.Capability +
                "' and side policy '" +
                attachment.Side +
                "'. requested=" +
                requestedCount +
                " available=" +
                available.Length +
                ".");
        }

        return available
            .Take(requestedCount)
            .Select(x => x.Socket.Name)
            .ToArray();
    }

    private static IEnumerable<SlotAddress> GetCompatibleSlots(
        VoxelCompilation compilation,
        AssetAttachment attachment) =>
        compilation.Sockets
            .Select(ParseSlotAddress)
            .Where(x =>
                string.Equals(
                    x.Capability,
                    attachment.Capability,
                    StringComparison.Ordinal) &&
                (string.Equals(attachment.Side, "Any", StringComparison.Ordinal) ||
                 string.Equals(x.Side, attachment.Side, StringComparison.Ordinal)))
            .OrderBy(x => SlotSideOrder(x.Side))
            .ThenBy(x => x.Instance);

    private static SlotAddress ParseSlotAddress(CompiledSocket socket)
    {
        string[] parts = socket.Name.Split('.');
        if (parts.Length != 4 ||
            !string.Equals(parts[0], "Equip", StringComparison.Ordinal) ||
            !AttachmentCapabilities.Contains(parts[1]) ||
            !SlotSides.Contains(parts[2]) ||
            !int.TryParse(
                parts[3],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int instance) ||
            instance < 0)
        {
            throw new InvalidDataException(
                $"Equipment slot '{socket.Name}' must match Equip.<Capability>.<Side>.<Instance>.");
        }
        return new SlotAddress(
            parts[1],
            parts[2],
            instance,
            socket);
    }

    private static int SlotSideOrder(string side) => side switch
    {
        "Left" => 0,
        "Right" => 1,
        "Center" => 2,
        _ => 3
    };

    private static void ValidateSemanticAnimationSet(
        VoxelCompilation compilation,
        bool requireSideAttackVariants)
    {
        foreach (CompiledAnimationClip clip in compilation.Animations)
        {
            SemanticAnimationClipIdentity identity =
                SemanticAnimationClipName.Parse(clip.Name);
            bool shouldLoop =
                SemanticAnimationClipName.ShouldLoop(identity.Motion);
            if (clip.Loop != shouldLoop)
            {
                throw new InvalidDataException(
                    $"Semantic animation clip '{clip.Name}' loop setting is invalid.");
            }
        }

        foreach (string motion in RequiredSemanticMotions)
        {
            _ = compilation.Animations.SingleOrDefault(
                    x => string.Equals(
                        x.Name,
                        motion,
                        StringComparison.Ordinal))
                ?? throw new InvalidDataException(
                    $"Required semantic animation clip '{motion}' is missing.");
        }

        if (!requireSideAttackVariants)
            return;

        foreach (string side in new[] { "Left", "Right" })
        {
            string name = "Attack." + side;
            _ = compilation.Animations.SingleOrDefault(
                    x => string.Equals(
                        x.Name,
                        name,
                        StringComparison.Ordinal))
                ?? throw new InvalidDataException(
                    $"Side-reactive avatar animation clip '{name}' is missing.");
        }
    }

    internal static IReadOnlyList<string> RequiredV1Clips(
        string kind) =>
        kind switch
        {
            "avatar" => new[]
            {
                "Idle.V1",
                "WalkForward.V1",
                "Attack.Left.V1",
                "Attack.Right.V1",
                "Hit.V1",
                "Death.V1"
            },
            "equipment" => new[]
            {
                "Idle.V1",
                "WalkForward.V1",
                "Attack.V1",
                "Hit.V1",
                "Death.V1"
            },
            _ => throw new InvalidDataException(
                "Unsupported reusable model kind for required V1 coverage: " +
                kind)
        };

    internal static IReadOnlyList<string> GetMissingRequiredV1Clips(
        VoxelCompilation compilation,
        string kind)
    {
        var names = compilation.Animations
            .Select(x => x.Name)
            .ToHashSet(StringComparer.Ordinal);
        return RequiredV1Clips(kind)
            .Where(x => !names.Contains(x))
            .ToArray();
    }

    internal static IReadOnlyList<string> GetAliasedRequiredV1Clips(
        VoxelCompilation compilation,
        string kind)
    {
        var clips = compilation.Animations.ToDictionary(
            x => x.Name,
            StringComparer.Ordinal);
        var aliased = new List<string>();
        foreach (string variantName in RequiredV1Clips(kind))
        {
            if (!clips.TryGetValue(
                    variantName,
                    out CompiledAnimationClip? variant))
            {
                continue;
            }

            string fallbackName =
                variantName[..^".V1".Length];
            if (!clips.TryGetValue(
                    fallbackName,
                    out CompiledAnimationClip? fallback))
            {
                continue;
            }

            if (AnimationContentEquals(variant, fallback))
                aliased.Add(variantName);
        }

        return aliased;
    }

    internal static void ValidateRequiredV1Coverage(
        VoxelCompilation compilation,
        string kind)
    {
        IReadOnlyList<string> missing =
            GetMissingRequiredV1Clips(compilation, kind);
        if (missing.Count > 0)
        {
            throw new InvalidDataException(
                "Required V1 animation coverage is incomplete for " +
                kind +
                ": " +
                string.Join(", ", missing));
        }

        IReadOnlyList<string> aliased =
            GetAliasedRequiredV1Clips(compilation, kind);
        if (aliased.Count > 0)
        {
            throw new InvalidDataException(
                "Required V1 animation clips must contain authored motion " +
                "different from their fallback clips: " +
                string.Join(", ", aliased));
        }
    }

    private static bool AnimationContentEquals(
        CompiledAnimationClip left,
        CompiledAnimationClip right)
    {
        if (left.Duration != right.Duration ||
            left.Loop != right.Loop ||
            left.Tracks.Count != right.Tracks.Count)
        {
            return false;
        }

        for (int i = 0; i < left.Tracks.Count; i++)
        {
            CompiledAnimationTrack a = left.Tracks[i];
            CompiledAnimationTrack b = right.Tracks[i];
            if (!string.Equals(
                    a.PartName,
                    b.PartName,
                    StringComparison.Ordinal) ||
                !AnimationChannelEquals(a.Position, b.Position) ||
                !AnimationChannelEquals(a.Rotation, b.Rotation) ||
                !AnimationChannelEquals(a.Scale, b.Scale))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AnimationChannelEquals(
        CompiledAnimationVectorChannel? left,
        CompiledAnimationVectorChannel? right)
    {
        if (left == null || right == null)
            return left == right;
        if (left.Interpolation != right.Interpolation ||
            left.Keys.Count != right.Keys.Count)
        {
            return false;
        }

        for (int i = 0; i < left.Keys.Count; i++)
        {
            CompiledAnimationVectorKey a = left.Keys[i];
            CompiledAnimationVectorKey b = right.Keys[i];
            if (a.Time != b.Time ||
                a.Value != b.Value)
            {
                return false;
            }
        }

        return true;
    }

    internal static string ResolveSemanticClipName(
        VoxelCompilation compilation,
        string motion,
        string? side,
        string? variant = null)
    {
        foreach (string candidate in
                 SemanticAnimationClipName.BuildCandidates(
                     motion,
                     side,
                     variant))
        {
            if (compilation.Animations.Any(
                    x => string.Equals(
                        x.Name,
                        candidate,
                        StringComparison.Ordinal)))
            {
                return candidate;
            }
        }

        throw new InvalidDataException(
            $"Semantic animation clip '{motion}' is unavailable.");
    }

    private static int RequireSocketIndex(
        VoxelCompilation compilation,
        string socketName)
    {
        for (int i = 0; i < compilation.Sockets.Count; i++)
        {
            if (string.Equals(
                    compilation.Sockets[i].Name,
                    socketName,
                    StringComparison.Ordinal))
            {
                return i;
            }
        }

        throw new InvalidDataException(
            $"Compiled socket '{socketName}' is unavailable.");
    }

    private static bool Near(Int3 value, Float3 other) =>
        MathF.Abs(value.X - other.X) <= 0.0001f &&
        MathF.Abs(value.Y - other.Y) <= 0.0001f &&
        MathF.Abs(value.Z - other.Z) <= 0.0001f;

    private static Vector3 MatrixOrigin(Matrix4x4 matrix) =>
        Vector3.Transform(Vector3.Zero, matrix);

    private static bool IsFinite(Matrix4x4 matrix) =>
        float.IsFinite(matrix.M11) &&
        float.IsFinite(matrix.M12) &&
        float.IsFinite(matrix.M13) &&
        float.IsFinite(matrix.M14) &&
        float.IsFinite(matrix.M21) &&
        float.IsFinite(matrix.M22) &&
        float.IsFinite(matrix.M23) &&
        float.IsFinite(matrix.M24) &&
        float.IsFinite(matrix.M31) &&
        float.IsFinite(matrix.M32) &&
        float.IsFinite(matrix.M33) &&
        float.IsFinite(matrix.M34) &&
        float.IsFinite(matrix.M41) &&
        float.IsFinite(matrix.M42) &&
        float.IsFinite(matrix.M43) &&
        float.IsFinite(matrix.M44);

    private static float MaxMatrixDelta(Matrix4x4 left, Matrix4x4 right) =>
        new[]
        {
            MathF.Abs(left.M11 - right.M11),
            MathF.Abs(left.M12 - right.M12),
            MathF.Abs(left.M13 - right.M13),
            MathF.Abs(left.M14 - right.M14),
            MathF.Abs(left.M21 - right.M21),
            MathF.Abs(left.M22 - right.M22),
            MathF.Abs(left.M23 - right.M23),
            MathF.Abs(left.M24 - right.M24),
            MathF.Abs(left.M31 - right.M31),
            MathF.Abs(left.M32 - right.M32),
            MathF.Abs(left.M33 - right.M33),
            MathF.Abs(left.M34 - right.M34),
            MathF.Abs(left.M41 - right.M41),
            MathF.Abs(left.M42 - right.M42),
            MathF.Abs(left.M43 - right.M43),
            MathF.Abs(left.M44 - right.M44)
        }.Max();

    internal static void ValidatePublicationBudget(int voxels, int parts, StoreBudget budget)
    {
        if (voxels > budget.MaxVoxelsPerAsset)
            throw new InvalidDataException($"Voxel budget exceeded: {voxels}>{budget.MaxVoxelsPerAsset}.");
        if (parts > budget.MaxPartsPerAsset)
            throw new InvalidDataException($"Part budget exceeded: {parts}>{budget.MaxPartsPerAsset}.");
    }

    private static float[] Vector(Vector3 value) => new[] { value.X, value.Y, value.Z };


    private static List<CoverageGap> EvaluateCoverage(
        CollectionManifest coverage,
        IReadOnlyList<AssetRecord> assets,
        IReadOnlyDictionary<string, AssetHealth> health)
    {
        var gaps = new List<CoverageGap>();
        foreach (CoverageRequirement requirement in coverage.Requirements)
        {
            bool satisfied = assets.Any(asset =>
                health[asset.Manifest.Id].Published &&
                asset.Manifest.Kind == requirement.Kind &&
                (requirement.Form == null || asset.Manifest.Form == requirement.Form));
            if (!satisfied)
                gaps.Add(new CoverageGap(
                    requirement.Id,
                    requirement.Kind,
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
        string catalogDir = Path.Combine(_root, "Catalog");
        Directory.CreateDirectory(catalogDir);
        File.WriteAllText(Path.Combine(catalogDir, "catalog.generated.json"), JsonSerializer.Serialize(new
        {
            store = snapshot.Store,
            collection = snapshot.Collection,
            assets = snapshot.Assets.Where(x => x.Published).Select(x => new
            {
                x.Id,
                x.Kind,
                x.UnitRef,
                x.Form,
                attachment = x.Kind == "equipment"
                    ? new
                    {
                        capability = x.AttachmentCapability,
                        side = x.AttachmentSide,
                        anchor = x.AttachmentAnchor
                    }
                    : null,
                slotInstances = x.Kind == "avatar" ? x.SlotInstances : null,
                slotCounts = x.Kind == "avatar" ? x.SlotCounts : null,
                semanticMotions = x.SemanticMotions,
                semanticMotionCoverage = x.SemanticMotionCoverage,
                requiredV1Complete = x.RequiredV1Complete,
                missingRequiredV1 = x.MissingRequiredV1,
                aliasedRequiredV1 = x.AliasedRequiredV1
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
    private void WriteReferenceAudit(AssetRecord asset)
    {
        ReferenceContract contract = ValidateReferenceContractStructure(asset);
        if (contract.Version == 3)
        {
            ValidateTargetProvenance(asset);
            ValidateFidelityAudit(asset);
        }

        ReferenceStyleAuthority style = ResolveReferenceStyle(
            asset.Manifest.Style.Family,
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
                EvidenceHash.HashFile(file);
        }
        foreach (string file in EnumerateApplicableStyleReferences(style))
            audit.StyleSha256![Path.GetRelativePath(_root, file).Replace('\\', '/')] =
                EvidenceHash.HashFile(file);
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
                EvidenceHash.HashFile(file);
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
            string actual = EvidenceHash.HashFile(path);
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
                EvidenceHash.HashFile(file);
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
                .Append(EvidenceHash.HashFile(file)).Append('\n');
        }
        foreach (string file in style.KindReferencePaths)
        {
            string relative = Path.GetRelativePath(manifestDirectory, file).Replace('\\', '/');
            builder.Append("kindref=").Append(relative).Append('|')
                .Append(EvidenceHash.HashFile(file)).Append('\n');
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
            throw new InvalidDataException("Asset style family is required.");
        if (assetKind is not ("avatar" or "equipment"))
            throw new InvalidDataException(
                $"Reference style does not support asset kind '{assetKind}'.");

        string styleBase = Path.Combine(_root, "Styles");
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

    internal static void ValidateManifestShape(AssetManifest m, string path)
    {
        if (m.Version != 3)
            throw new InvalidDataException($"Active asset manifests must use version 3 in {path}.");
        if (string.IsNullOrWhiteSpace(m.Id) || string.IsNullOrWhiteSpace(m.UnitRef))
            throw new InvalidDataException($"Asset id and UnitRef are required in {path}.");
        if (m.Kind is not ("avatar" or "equipment"))
            throw new InvalidDataException($"Unsupported asset kind '{m.Kind}' in {path}.");
        if (string.IsNullOrWhiteSpace(m.Source.Base) || string.IsNullOrWhiteSpace(m.Source.Palette))
            throw new InvalidDataException($"Asset source base/palette are required in {path}.");
        if (string.IsNullOrWhiteSpace(m.Style.Family))
            throw new InvalidDataException($"Asset style family is required in {path}.");
        if (Path.IsPathRooted(m.Source.Base) || Path.IsPathRooted(m.Source.Palette))
            throw new InvalidDataException($"Asset source paths must be repository-portable relative paths in {path}.");

        if (m.Kind == "avatar")
        {
            if (!string.IsNullOrWhiteSpace(m.Form) ||
                !string.IsNullOrWhiteSpace(m.Density) ||
                m.Attachment != null)
            {
                throw new InvalidDataException(
                    $"Avatar manifest cannot declare equipment form/density/attachment metadata in {path}.");
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(m.Form))
            throw new InvalidDataException($"Equipment must declare a reusable physical form in {path}.");
        if (!string.IsNullOrWhiteSpace(m.Density) && !IsReferenceDrivenDensity(m))
            throw new InvalidDataException($"Equipment density must be 'reference-driven' when declared in {path}.");
        if (m.Attachment == null)
            throw new InvalidDataException($"Equipment must declare attachment metadata in {path}.");
        if (!AttachmentCapabilities.Contains(m.Attachment.Capability))
            throw new InvalidDataException(
                $"Equipment attachment capability '{m.Attachment.Capability}' is unsupported in {path}.");
        if (!AttachmentSides.Contains(m.Attachment.Side))
            throw new InvalidDataException(
                $"Equipment attachment side '{m.Attachment.Side}' is unsupported in {path}.");
        if (!string.Equals(m.Attachment.Anchor, "Mount", StringComparison.Ordinal))
            throw new InvalidDataException(
                $"Equipment attachment anchor must be 'Mount' in {path}.");
    }

    private static void ValidatePolicy(StorePolicy policy)
    {
        if (policy.Version != 2 ||
            string.IsNullOrWhiteSpace(policy.Name) ||
            string.IsNullOrWhiteSpace(policy.Collection) ||
            string.IsNullOrWhiteSpace(policy.DefaultStyle))
            throw new InvalidDataException("Store policy is incomplete.");
        if (policy.Budget.MaxVoxelsPerAsset <= 0 || policy.Budget.MaxPartsPerAsset <= 0)
            throw new InvalidDataException("Store budgets must be positive.");
    }

    private static void ValidateCollection(CollectionManifest collection)
    {
        if (collection.Version != 1 || string.IsNullOrWhiteSpace(collection.Id))
            throw new InvalidDataException("Active collection manifest must use version 1.");
        if (collection.Requirements.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != collection.Requirements.Count)
            throw new InvalidDataException("Collection requirement ids must be unique.");

        foreach (CoverageRequirement requirement in collection.Requirements)
        {
            if (requirement.Kind is not ("avatar" or "equipment"))
                throw new InvalidDataException($"Unsupported collection requirement kind '{requirement.Kind}'.");
            if (requirement.Kind == "equipment")
            {
                if (string.IsNullOrWhiteSpace(requirement.Form))
                    throw new InvalidDataException("Equipment collection requirements require a form.");
            }
            else if (!string.IsNullOrWhiteSpace(requirement.Form))
            {
                throw new InvalidDataException("Avatar collection requirements cannot declare equipment form.");
            }
        }
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
