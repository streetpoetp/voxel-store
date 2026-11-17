using System.Diagnostics;
using System.Globalization;
using System.Numerics;
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
public sealed record CoverageGap(string Id, string Kind, string? Profile, string? Mount, bool Required);
public sealed record MaintenanceJob(string Priority, string Kind, string Target, string Reason);
public sealed record CompatibilityResult(
    string Avatar,
    string Equipment,
    string Profile,
    string Mount,
    string Pose,
    bool Passed,
    string Evidence);

public sealed record PartWorldFrame(Vector3 Origin, Vector3 WorldX, Vector3 WorldY, Vector3 WorldZ);
public sealed record SocketWorldFrame(Vector3 Origin, Vector3 WorldX, Vector3 WorldY, Vector3 WorldZ);

public sealed class AssetHealth
{
    public required string Id { get; init; }
    public required string Kind { get; init; }
    public required string UnitRef { get; init; }
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
        foreach (AssetRecord asset in assets)
            ValidateHistoricalIdentity(asset);
        return assets;
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

    public void Seal(string assetId)
    {
        AssetRecord asset = RequireAsset(assetId);
        WriteReferenceAudit(asset);
        WriteProofAudit(asset);
    }

    public HeroVoxelProvider.Resolution ResolveProvider() => _provider.Resolve();

    private AssetHealth EvaluateBaseHealth(
        AssetRecord asset,
        StorePolicy policy,
        IReadOnlyDictionary<string, CompatibilityProfile> profiles)
    {
        var health = new AssetHealth
        {
            Id = asset.Manifest.Id,
            Kind = asset.Manifest.Kind,
            UnitRef = asset.Manifest.UnitRef
        };
        AddGate(health, "manifest", () => ValidateManifest(asset, policy, profiles));
        AddGate(health, "reference", () => ValidateReference(asset));
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
            if (voxels > policy.Budget.MaxVoxelsPerAsset)
                throw new InvalidDataException($"Voxel budget exceeded: {voxels}>{policy.Budget.MaxVoxelsPerAsset}.");
            if (compiled.Compilation.Parts.Count > policy.Budget.MaxPartsPerAsset)
                throw new InvalidDataException($"Part budget exceeded: {compiled.Compilation.Parts.Count}>{policy.Budget.MaxPartsPerAsset}.");
            return $"{voxels} voxels, {compiled.Compilation.Parts.Count} parts";
        });

        health.BasePublished = health.Gates.All(x => x.Passed);
        health.Published = health.BasePublished;
        health.FailedGate = health.Gates.FirstOrDefault(x => !x.Passed)?.Id;
        return health;
    }

    private static void AddGate(AssetHealth health, string id, Func<string?> action)
    {
        if (health.Gates.Any(x => !x.Passed))
        {
            health.Gates.Add(new GateResult(id, false, "Blocked by earlier gate."));
            return;
        }
        try
        {
            health.Gates.Add(new GateResult(id, true, "PASS", action()));
        }
        catch (Exception ex)
        {
            health.Gates.Add(new GateResult(id, false, ex.Message));
        }
    }

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
            if (!profile.RequiredMounts.Concat(profile.OptionalMounts).Any(x => x.Socket == m.Mount.Socket))
                throw new InvalidDataException($"Equipment mount '{m.Mount.Socket}' is not declared by profile '{profile.Id}'.");
            if (!string.IsNullOrWhiteSpace(m.Family))
                throw new InvalidDataException("Equipment compatibility belongs in mount.profile, not family.");
        }
        return asset.ManifestPath;
    }

    private static string ValidateReference(AssetRecord asset)
    {
        string contractPath = Path.Combine(asset.ReferenceDirectory, "reference.yaml");
        string auditPath = Path.Combine(asset.ReferenceDirectory, "audit.generated.json");
        if (!File.Exists(contractPath) || !File.Exists(auditPath))
            throw new InvalidDataException("Reference contract/audit evidence is missing.");
        ReferenceContract contract = ContractYaml.Load<ReferenceContract>(contractPath);
        if (contract.RequiredParts.Count == 0 ||
            contract.RequiredParts.Distinct(StringComparer.Ordinal).Count() != contract.RequiredParts.Count)
            throw new InvalidDataException("RequiredParts must be non-empty and unique.");
        string composition = Path.Combine(asset.ReferenceDirectory, contract.Composition);
        if (!File.Exists(composition))
            throw new InvalidDataException("Composition Sheet is missing.");
        foreach (string part in contract.RequiredParts)
        {
            string sheet = Path.Combine(asset.ReferenceDirectory, "Parts", Sanitize(part) + ".svg");
            if (!File.Exists(sheet))
                throw new InvalidDataException($"Part Sheet missing for '{part}'.");
        }
        VerifyAudit(
            asset.ReferenceDirectory,
            auditPath,
            "reference",
            EnumerateReferenceFiles(asset));
        return auditPath;
    }

    private static string ValidateSource(AssetRecord asset)
    {
        string spec = asset.SourceBase + ".voxel.yaml";
        if (!File.Exists(spec) || !File.Exists(asset.PalettePath))
            throw new InvalidDataException("Authoritative VoxelSpec or palette is missing.");
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
            foreach (MountContract required in profile.RequiredMounts)
            {
                CompiledSocket? socket = avatarCompiled.Compilation.Sockets.SingleOrDefault(x => x.Name == required.Socket);
                if (socket == null)
                {
                    result.Add(new CompatibilityResult(
                        avatar.Manifest.Id, "", profile.Id, required.Socket, "bind", false, "required socket missing"));
                    continue;
                }
                if (avatarCompiled.Compilation.VoxelsPerUnit != profile.VoxelsPerUnit)
                {
                    result.Add(new CompatibilityResult(
                        avatar.Manifest.Id, "", profile.Id, required.Socket, "bind", false, "voxelsPerUnit mismatch"));
                }
            }

            foreach (AssetRecord equipment in assets.Where(x =>
                         x.Manifest.Kind == "equipment" &&
                         x.Manifest.Mount?.Profile == profile.Id &&
                         health[x.Manifest.Id].BasePublished))
            {
                string mount = equipment.Manifest.Mount!.Socket;
                CompiledSocket? socket = avatarCompiled.Compilation.Sockets.SingleOrDefault(x => x.Name == mount);
                HeroVoxelProvider.CompiledAsset equipmentCompiled = _provider.Compile(equipment, false);
                bool equipmentFrameValid =
                    equipmentCompiled.Compilation.VoxelsPerUnit == profile.VoxelsPerUnit &&
                    equipmentCompiled.Compilation.AssemblyOrigin != null;
                string evidencePath = Path.Combine(
                    avatar.ProofDirectory, "Compatibility", Sanitize(equipment.Manifest.Id) + ".json");
                var sampleRows = new List<object>();
                if (profile.RepresentativePoseSamples.Count == 0)
                {
                    bool pass = socket != null && equipmentFrameValid;
                    string detail = pass
                        ? "PASS"
                        : socket == null
                            ? "required socket missing"
                            : "equipment mount frame/VPU is incompatible";
                    result.Add(new CompatibilityResult(
                        avatar.Manifest.Id, equipment.Manifest.Id, profile.Id, mount, "bind", pass, evidencePath));
                    sampleRows.Add(new { pose = "bind", passed = pass, detail });
                }
                else
                {
                    foreach (PoseSampleContract sample in profile.RepresentativePoseSamples)
                    {
                        string key = PoseKey(sample.Clip, sample.NormalizedTime);
                        SocketWorldFrame? frame = socket == null ||
                                                  !poseSamples.TryGetValue(key, out Dictionary<string, PartWorldFrame>? parts)
                            ? null
                            : ResolveSocketWorldFrame(avatarCompiled.Compilation, socket, parts);
                        bool pass = socket != null && equipmentFrameValid && frame != null;
                        string detail = pass
                            ? "PASS"
                            : socket == null
                                ? "required socket missing"
                                : !equipmentFrameValid
                                    ? "equipment mount frame/VPU is incompatible"
                                    : $"pose/socket frame unavailable: {key}";
                        result.Add(new CompatibilityResult(
                            avatar.Manifest.Id, equipment.Manifest.Id, profile.Id, mount, key, pass, evidencePath));
                        sampleRows.Add(new
                        {
                            pose = key,
                            passed = pass,
                            detail,
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
                        mount,
                        socketParent = socket?.ParentName,
                        socketTranslation = socket == null ? null : new[]
                        {
                            socket.Transform.Translation.X,
                            socket.Transform.Translation.Y,
                            socket.Transform.Translation.Z
                        },
                        samples = sampleRows
                    }, JsonOptions));
                }
            }
        }
        return result;
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
                (requirement.Mount == null || asset.Manifest.Mount?.Socket == requirement.Mount));
            if (!satisfied)
                gaps.Add(new CoverageGap(
                    requirement.Id, requirement.Kind, requirement.Profile, requirement.Mount, requirement.Required));
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
                "reference" or "source" or "proof" or "budget" => "regressed-evidence",
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
                x.UnitRef
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
        yield return Path.Combine(asset.ReferenceDirectory, contract.Composition);
        foreach (string part in contract.RequiredParts)
            yield return Path.Combine(asset.ReferenceDirectory, "Parts", Sanitize(part) + ".svg");
    }

    private void WriteReferenceAudit(AssetRecord asset)
    {
        var audit = new AuditEvidence { Kind = "reference" };
        foreach (string file in EnumerateReferenceFiles(asset))
        {
            if (!File.Exists(file))
                throw new FileNotFoundException("Cannot seal missing reference evidence.", file);
            audit.Sha256[Path.GetRelativePath(asset.ReferenceDirectory, file).Replace('\\', '/')] =
                HeroVoxelProvider.HashFile(file);
        }
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

    private AssetRecord RequireAsset(string assetId) =>
        DiscoverAssets().SingleOrDefault(x => x.Manifest.Id == assetId)
        ?? throw new InvalidOperationException($"Unknown asset id '{assetId}'.");

    private static void ValidateManifestShape(AssetManifest m, string path)
    {
        if (m.Version != 1)
            throw new InvalidDataException($"Unsupported asset manifest version in {path}.");
        if (string.IsNullOrWhiteSpace(m.Id) || string.IsNullOrWhiteSpace(m.UnitRef))
            throw new InvalidDataException($"Asset id and UnitRef are required in {path}.");
        if (m.Kind is not ("avatar" or "equipment"))
            throw new InvalidDataException($"Unsupported asset kind '{m.Kind}' in {path}.");
        if (string.IsNullOrWhiteSpace(m.Source.Base) || string.IsNullOrWhiteSpace(m.Source.Palette))
            throw new InvalidDataException($"Asset source base/palette are required in {path}.");
        if (Path.IsPathRooted(m.Source.Base) || Path.IsPathRooted(m.Source.Palette))
            throw new InvalidDataException($"Asset source paths must be repository-portable relative paths in {path}.");
    }

    private static void ValidatePolicy(StorePolicy policy)
    {
        if (policy.Version != 1 || string.IsNullOrWhiteSpace(policy.Name) || string.IsNullOrWhiteSpace(policy.StyleFamily))
            throw new InvalidDataException("Store policy is incomplete.");
        if (policy.Budget.MaxVoxelsPerAsset <= 0 || policy.Budget.MaxPartsPerAsset <= 0)
            throw new InvalidDataException("Store budgets must be positive.");
    }

    private static void ValidateCoverage(CoveragePolicy policy)
    {
        if (policy.Version != 1 || string.IsNullOrWhiteSpace(policy.Foundation))
            throw new InvalidDataException("Coverage policy is incomplete.");
        if (policy.Requirements.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != policy.Requirements.Count)
            throw new InvalidDataException("Coverage requirement ids must be unique.");
        foreach (CoverageRequirement requirement in policy.Requirements)
            if (requirement.Kind is not ("avatar" or "equipment"))
                throw new InvalidDataException($"Unsupported coverage kind '{requirement.Kind}'.");
    }

    private static void ValidateProfile(CompatibilityProfile profile, string path)
    {
        if (profile.Version != 1 || string.IsNullOrWhiteSpace(profile.Id) || profile.VoxelsPerUnit <= 0)
            throw new InvalidDataException($"Compatibility profile is incomplete: {path}");
        if (profile.RequiredMounts.Count == 0)
            throw new InvalidDataException($"Compatibility profile requires at least one mount: {path}");
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (MountContract mount in profile.RequiredMounts.Concat(profile.OptionalMounts))
        {
            if (!names.Add(mount.Socket))
                throw new InvalidDataException($"Duplicate mount '{mount.Socket}' in {path}.");
            if (mount.Forward.Length != 3 || mount.Up.Length != 3 || mount.Envelope.Length != 3)
                throw new InvalidDataException($"Mount '{mount.Socket}' must define 3D forward/up/envelope vectors.");
            if (mount.Envelope.Any(x => x <= 0))
                throw new InvalidDataException($"Mount '{mount.Socket}' envelope components must be positive.");
        }
    }

    private void ValidateHistoricalIdentity(AssetRecord asset)
    {
        string relative = Path.GetRelativePath(_root, asset.ManifestPath).Replace('\\', '/');
        string? previous = GitShow(relative);
        if (previous == null)
            return;
        AssetManifest old = ContractYaml.Parse<AssetManifest>(previous);
        if (!string.Equals(old.Id, asset.Manifest.Id, StringComparison.Ordinal) ||
            !string.Equals(old.UnitRef, asset.Manifest.UnitRef, StringComparison.Ordinal))
            throw new InvalidDataException(
                $"Published semantic identity changed in-place at {relative}; create a new asset identity or explicit migration.");
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
