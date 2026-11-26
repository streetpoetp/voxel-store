using System.Diagnostics;
using System.Buffers.Binary;
using System.Text.Json;
using VoxelStore.Tool;

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}


static void ExpectFailure(Action action, string contains)
{
    try
    {
        action();
    }
    catch (Exception ex)
    {
        if (ex.ToString().Contains(contains, StringComparison.OrdinalIgnoreCase))
            return;
        throw new InvalidOperationException(
            $"Expected failure containing '{contains}', got: {ex}", ex);
    }

    throw new InvalidOperationException(
        $"Expected failure containing '{contains}', but action succeeded.");
}

static string TempRoot()
{
    string path = Path.Combine(
        Path.GetTempPath(),
        "voxel-store-tests",
        Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    return path;
}

static string HeroStudioSubmodule(string repositoryRoot) =>
    Path.Combine(repositoryRoot, "Dependencies", "hero-studio");
static void CopyFile(string sourceRoot, string targetRoot, string relative)
{
    string source = Path.Combine(
        sourceRoot,
        relative.Replace('/', Path.DirectorySeparatorChar));
    string target = Path.Combine(
        targetRoot,
        relative.Replace('/', Path.DirectorySeparatorChar));
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
    File.Copy(source, target, true);
}

static void CopyDirectory(string source, string target)
{
    Directory.CreateDirectory(target);
    foreach (string file in Directory.GetFiles(source))
        File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
    foreach (string directory in Directory.GetDirectories(source))
        CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
}

static void DeleteTree(string root)
{
    if (!Directory.Exists(root))
        return;
    foreach (string file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        File.SetAttributes(file, FileAttributes.Normal);
    Directory.Delete(root, true);
}

static void RunGit(string root, params string[] args)
{
    var start = new ProcessStartInfo("git")
    {
        WorkingDirectory = root,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    foreach (string arg in args)
        start.ArgumentList.Add(arg);
    using Process process =
        Process.Start(start) ?? throw new InvalidOperationException("Failed to start git.");
    string stdout = process.StandardOutput.ReadToEnd();
    string stderr = process.StandardError.ReadToEnd();
    process.WaitForExit();
    if (process.ExitCode != 0)
        throw new InvalidOperationException(
            $"git {string.Join(' ', args)} failed: {stdout}{stderr}");
}

static void WritePngHeader(string path, int width, int height, bool validSignature = true)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    byte[] bytes = new byte[24];
    byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
    if (validSignature)
        signature.CopyTo(bytes, 0);
    BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(8, 4), 13);
    "IHDR"u8.CopyTo(bytes.AsSpan(12, 4));
    BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(16, 4), (uint)width);
    BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(20, 4), (uint)height);
    File.WriteAllBytes(path, bytes);
}


static void RunReferenceDensitySelectorTests()
{
    var reference = new ReferenceContract
    {
        Version = 2,
        Composition = "composition.png",
        RequiredParts = new List<string> { "Staff" },
        PartSheets = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Staff"] = "Parts/Staff.png"
        }
    };

    var ruler = new VpuRulerPolicy
    {
        Version = 1,
        Scope = "per-unit",
        CandidateVpu = new List<int> { 2, 4, 6, 8, 10, 12, 16, 20, 24, 32, 40, 48 },
        Priors = new Dictionary<string, VpuRulerPrior>(StringComparer.Ordinal)
        {
            ["equipment"] = new() { Typical = new[] { 8, 24 } }
        },
        FeatureReadability = new VpuRulerFeatureReadability
        {
            AllowIntentionalOneVoxelFeature = true,
            UniversalMinimumFeatureVoxels = null
        },
        Selection = new VpuRulerSelection
        {
            Mode = "lowest-sufficient",
            PriorsAreHardBounds = false,
            PreserveWorldExtent = true,
            PreserveReferenceCadence = true
        }
    };
    ReferenceDensitySelector.ValidateRuler(ruler);

    var input = new ReferenceDensityInput
    {
        Version = 2,
        AssetClass = "equipment",
        TargetWorldLongAxis = 1.5,
        Parts = new Dictionary<string, ReferenceDensityPartInput>(StringComparer.Ordinal)
        {
            ["Staff"] = new()
            {
                PartSheet = "Parts/Staff.png",
                ObjectLongAxisPixels = 520,
                ApparentVoxelCount = 24,
                Features = new List<ReferenceDensityFeatureInput>
                {
                    new() { Id = "one-voxel-edge", Importance = "primary" },
                    new() { Id = "cage-gap", Importance = "secondary" }
                }
            }
        }
    };

    ReferenceDensitySelector.ValidateInput(input, reference);
    Require(
        Math.Abs(ReferenceDensitySelector.EstimateVpu(input) - 16.0) < 1e-9,
        "cadence estimate must derive per-unit VPU from apparent voxel count / world extent");

    int[] candidates = ReferenceDensitySelector.BuildCandidateSequence(input, ruler).ToArray();
    int[] candidatesAgain = ReferenceDensitySelector.BuildCandidateSequence(input, ruler).ToArray();
    Require(
        candidates.SequenceEqual(new[] { 8, 10, 12, 16, 20, 24 }) &&
        candidates.SequenceEqual(candidatesAgain),
        "v2 candidate neighborhood must be deterministic and use canonical low ruler values");

    var coarserUnit = new ReferenceDensityInput
    {
        Version = 2,
        AssetClass = "equipment",
        TargetWorldLongAxis = 1.5,
        Parts = new Dictionary<string, ReferenceDensityPartInput>(StringComparer.Ordinal)
        {
            ["Staff"] = new()
            {
                PartSheet = "Parts/Staff.png",
                ObjectLongAxisPixels = 520,
                ApparentVoxelCount = 12,
                Features = input.Parts["Staff"].Features
            }
        }
    };
    ReferenceDensitySelector.ValidateInput(coarserUnit, reference);
    Require(
        Math.Abs(ReferenceDensitySelector.EstimateVpu(coarserUnit) - 8.0) < 1e-9 &&
        !ReferenceDensitySelector.BuildCandidateSequence(coarserUnit, ruler).SequenceEqual(candidates),
        "two units with different apparent cadence must derive different per-unit estimates/search neighborhoods");

    var wrongSheet = new ReferenceDensityInput
    {
        Version = 2,
        AssetClass = "equipment",
        TargetWorldLongAxis = 1.5,
        Parts = new Dictionary<string, ReferenceDensityPartInput>(StringComparer.Ordinal)
        {
            ["Staff"] = new()
            {
                PartSheet = "Parts/Stale.png",
                ObjectLongAxisPixels = 520,
                ApparentVoxelCount = 24,
                Features = input.Parts["Staff"].Features
            }
        }
    };
    ExpectFailure(
        () => ReferenceDensitySelector.ValidateInput(wrongSheet, reference),
        "must match accepted reference authority");

    var invalidV2Feature = new ReferenceDensityInput
    {
        Version = 2,
        AssetClass = "equipment",
        TargetWorldLongAxis = 1.5,
        Parts = new Dictionary<string, ReferenceDensityPartInput>(StringComparer.Ordinal)
        {
            ["Staff"] = new()
            {
                PartSheet = "Parts/Staff.png",
                ObjectLongAxisPixels = 520,
                ApparentVoxelCount = 24,
                Features = new List<ReferenceDensityFeatureInput>
                {
                    new()
                    {
                        Id = "legacy-span",
                        Importance = "primary",
                        RequiredVoxelSpan = 2
                    }
                }
            }
        }
    };
    ExpectFailure(
        () => ReferenceDensitySelector.ValidateInput(invalidV2Feature, reference),
        "must not declare requiredVoxelSpan");

    // Legacy v1 remains readable during migration.
    var legacy = new ReferenceDensityInput
    {
        Version = 1,
        TargetWorldLongAxis = 1.5,
        Parts = new Dictionary<string, ReferenceDensityPartInput>(StringComparer.Ordinal)
        {
            ["Staff"] = new()
            {
                PartSheet = "Parts/Staff.png",
                ObjectLongAxisPixels = 520,
                Features = new List<ReferenceDensityFeatureInput>
                {
                    new()
                    {
                        Id = "legacy-bar",
                        PixelSpan = 10,
                        Importance = "secondary",
                        RequiredVoxelSpan = 3
                    }
                }
            }
        }
    };
    ReferenceDensitySelector.ValidateInput(legacy, reference);
    Require(
        Math.Abs(ReferenceDensitySelector.EstimateVpu(legacy) - 104.0) < 1e-9 &&
        ReferenceDensitySelector.BuildCandidateSequence(legacy).Take(2).SequenceEqual(new[] { 96, 112 }),
        "legacy v1 density behavior must remain readable until published assets migrate");

    var budget = new StoreBudget { MaxVoxelsPerAsset = 65536, MaxPartsPerAsset = 16 };
    ReferenceDensityCandidateAudit Candidate(
        int vpu,
        bool edge,
        bool gap,
        bool cadence = true,
        int edgeSpan = 1,
        int occupied = 8000) =>
        new()
        {
            Version = 1,
            Vpu = vpu,
            Features = new Dictionary<string, bool>(StringComparer.Ordinal)
            {
                ["one-voxel-edge"] = edge,
                ["cage-gap"] = gap
            },
            FeatureVoxelSpans = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["one-voxel-edge"] = edgeSpan,
                ["cage-gap"] = 1
            },
            CadencePreserved = cadence,
            NormalizedWorldSize = new[] { 0.4, 0.4, 1.5 },
            IntegerBounds = new[] { 4, 4, Math.Max(1, (int)Math.Round(1.5 * vpu)) },
            OccupiedVoxels = occupied,
            AxisCompliant = true,
            GreedyMergedQuads = 120,
            Vertices = 480,
            Triangles = 240,
            SourceSha256 = new string('a', 64),
            PreviewSha256 = new string('b', 64)
        };

    ReferenceDensityCandidateEvidence oneVoxelPass =
        ReferenceDensitySelector.ValidateCandidate(
            input,
            Candidate(8, true, true, edgeSpan: 1),
            budget,
            new string('c', 64));
    Require(
        oneVoxelPass.Passed &&
        oneVoxelPass.FeatureVoxelSpans["one-voxel-edge"] == 1,
        "intentional one-voxel feature must be allowed when preserved and readable");

    ReferenceDensityCandidateEvidence lostFeature =
        ReferenceDensitySelector.ValidateCandidate(
            input,
            Candidate(10, false, true),
            budget,
            new string('d', 64));
    Require(
        !lostFeature.Passed &&
        lostFeature.LostFeatures.SequenceEqual(new[] { "one-voxel-edge" }),
        "candidate losing a meaningful feature must fail");

    ReferenceDensityCandidateEvidence cadenceMismatch =
        ReferenceDensitySelector.ValidateCandidate(
            input,
            Candidate(12, true, true, cadence: false),
            budget,
            new string('e', 64));
    Require(!cadenceMismatch.Passed, "candidate that materially changes reference cadence must fail");

    ReferenceDensityCandidateEvidence pass16 =
        ReferenceDensitySelector.ValidateCandidate(
            input,
            Candidate(16, true, true, edgeSpan: 1),
            budget,
            new string('f', 64));
    Require(
        ReferenceDensitySelector.SelectLowestSufficient(
            new[] { 8, 10, 12, 16 },
            new[] { lostFeature, cadenceMismatch, pass16, oneVoxelPass }) == 8,
        "selector must choose the lowest sufficient evaluated ruler candidate");

    ReferenceDensityCandidateEvidence coarserPass6 =
        ReferenceDensitySelector.ValidateCandidate(
            coarserUnit,
            Candidate(6, true, true, edgeSpan: 1),
            budget,
            new string('2', 64));
    int? coarserSelected = ReferenceDensitySelector.SelectLowestSufficient(
        ReferenceDensitySelector.BuildCandidateSequence(coarserUnit, ruler),
        new[] { coarserPass6 });
    Require(
        coarserSelected == 6,
        $"different per-unit cadence must be able to select a different VPU; expected 6 got {coarserSelected}");

    ReferenceDensityCandidateEvidence overBudget =
        ReferenceDensitySelector.ValidateCandidate(
            input,
            Candidate(20, true, true, occupied: 65537),
            budget,
            new string('1', 64));
    Require(!overBudget.Passed, "over-budget density candidate must be rejected");
}


static (string Root, string AssetRoot) PrepareDensityEquipmentFixture(string repositoryRoot)
{
    string root = TempRoot();
    try
    {
        CopyDirectory(Path.Combine(repositoryRoot, "Store"), Path.Combine(root, "Store"));
        CopyDirectory(Path.Combine(repositoryRoot, "Collections"), Path.Combine(root, "Collections"));
        CopyFile(repositoryRoot, root, "toolchain/hero-voxel.lock.yaml");
        CopyDirectory(
            Path.Combine(repositoryRoot, "Styles", "low-resolution-voxel.v1"),
            Path.Combine(root, "Styles", "low-resolution-voxel.v1"));

        string assetRoot = Path.Combine(
            root, "Assets", "Equipment", "FixtureStaff");
        Directory.CreateDirectory(Path.Combine(assetRoot, "Reference", "Parts"));
        Directory.CreateDirectory(Path.Combine(assetRoot, "Source"));

        File.WriteAllText(
            Path.Combine(assetRoot, "asset.yaml"),
            """
            version: 3
            id: fixture.equipment.staff
            kind: equipment
            unitRef: fixture/equipment/staff
            form: staff
            attachment:
              capability: Hand
              side: Any
              anchor: Mount
            source:
              base: ./Source/FixtureStaff
              palette: ./Source/FixtureStaff.palette.yaml
            style:
              family: low-resolution-voxel.v1
            """);
        File.WriteAllText(
            Path.Combine(assetRoot, "Reference", "reference.yaml"),
            """
            version: 2
            composition: composition.png
            requiredParts:
              - Staff
            partSheets:
              Staff: Parts/Staff.png
            """);
        WritePngHeader(Path.Combine(assetRoot, "Reference", "composition.png"), 1200, 800);
        WritePngHeader(Path.Combine(assetRoot, "Reference", "Parts", "Staff.png"), 800, 800);
        File.WriteAllText(
            Path.Combine(assetRoot, "Source", "FixtureStaff.palette.yaml"),
            """
            version: 1
            colors:
              staff: "#75462BFF"
            """);
        File.WriteAllText(
            Path.Combine(assetRoot, "Source", "FixtureStaff.voxel.yaml"),
            """
            name: FixtureStaff
            voxelsPerUnit: 64
            assemblyOrigin:
              part: Staff
            parts:
              - part: Staff
                pivot: [2, 2, 48]
                voxels:
                  - box: { min: [0, 0, 0], size: [4, 4, 96], color: staff }
            sockets:
              - socket: Mount
                parent: Staff
                transform:
                  translate: [2, 2, 48]
            animations:
              - clip: Idle
                duration: 1
                loop: true
                tracks:
                  - part: Staff
                    rotation:
                      keys:
                        - time: 0
                          value: [0, 0, 0]
              - clip: WalkForward
                duration: 1
                loop: true
                tracks:
                  - part: Staff
                    rotation:
                      keys:
                        - time: 0
                          value: [0, 0, 0]
              - clip: Attack
                duration: 1
                loop: false
                tracks:
                  - part: Staff
                    rotation:
                      keys:
                        - time: 0
                          value: [0, 0, 0]
              - clip: Hit
                duration: 1
                loop: false
                tracks:
                  - part: Staff
                    rotation:
                      keys:
                        - time: 0
                          value: [0, 0, 0]
              - clip: Death
                duration: 1
                loop: false
                tracks:
                  - part: Staff
                    rotation:
                      keys:
                        - time: 0
                          value: [0, 0, 0]
              - clip: Idle.V1
                duration: 0.9
                loop: true
                tracks:
                  - part: Staff
                    rotation:
                      keys:
                        - time: 0
                          value: [0, 0, 2]
              - clip: WalkForward.V1
                duration: 0.9
                loop: true
                tracks:
                  - part: Staff
                    rotation:
                      keys:
                        - time: 0
                          value: [0, 0, 3]
              - clip: Attack.V1
                duration: 0.9
                loop: false
                tracks:
                  - part: Staff
                    rotation:
                      keys:
                        - time: 0
                          value: [0, 0, 8]
              - clip: Hit.V1
                duration: 0.9
                loop: false
                tracks:
                  - part: Staff
                    rotation:
                      keys:
                        - time: 0
                          value: [0, 0, -4]
              - clip: Death.V1
                duration: 0.9
                loop: false
                tracks:
                  - part: Staff
                    rotation:
                      keys:
                        - time: 0
                          value: [0, 0, 12]
            """);

        var engine = new StoreEngine(root, HeroStudioSubmodule(repositoryRoot));
        engine.Generate("fixture.equipment.staff");
        engine.Seal("fixture.equipment.staff");
        return (root, assetRoot);
    }
    catch
    {
        DeleteTree(root);
        throw;
    }
}


static void RunReferenceDensityEvidenceTests(string repositoryRoot)
{
    const string densityFixtureId = "fixture.equipment.staff";
    (string root, string assetRoot) = PrepareDensityEquipmentFixture(repositoryRoot);
    try
    {
        string densityRoot = Path.Combine(assetRoot, "Density");
        DeleteTree(densityRoot);
        string manifestPath = Path.Combine(assetRoot, "asset.yaml");
        string legacyManifest = File.ReadAllText(manifestPath);
        var legacyEngine = new StoreEngine(root, HeroStudioSubmodule(repositoryRoot));
        legacyEngine.Seal(densityFixtureId);
        var legacySnapshot = legacyEngine.Scan(false, false);
        Require(
            legacySnapshot.Assets.Single().Published &&
            legacySnapshot.Assets.Single().Gates.All(x => x.Id != "density"),
            "legacy equipment must remain publishable without density evidence during migration");

        File.WriteAllText(
            manifestPath,
            legacyManifest.Replace(
                "form: staff",
                "form: staff\ndensity: reference-driven",
                StringComparison.Ordinal));

        Directory.CreateDirectory(densityRoot);
        string inputPath = Path.Combine(densityRoot, "input.yaml");
        string originalInput =
            """
            version: 2
            assetClass: equipment
            targetWorldLongAxis: 1.5
            parts:
              Staff:
                partSheet: Parts/Staff.png
                objectLongAxisPixels: 520
                apparentVoxelCount: 18
                features:
                  - id: one-voxel-edge
                    importance: primary
                  - id: cage-gap
                    importance: secondary
            """;
        File.WriteAllText(inputPath, originalInput);

        void WriteCandidate(
            int vpu,
            bool edge,
            bool gap,
            bool cadencePreserved,
            int edgeSpan = 1)
        {
            string candidateRoot = Path.Combine(
                densityRoot, "Candidates", $"vpu-{vpu:D3}");
            string sourcePath = Path.Combine(candidateRoot, "Source", "candidate.voxel.yaml");
            string previewPath = Path.Combine(candidateRoot, "Proof", "hero.png");
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            File.WriteAllText(sourcePath, $"name: candidate-{vpu}\nvoxelsPerUnit: {vpu}\n");
            WritePngHeader(previewPath, 512, 512);

            var audit = new ReferenceDensityCandidateAudit
            {
                Version = 1,
                Vpu = vpu,
                Features = new Dictionary<string, bool>(StringComparer.Ordinal)
                {
                    ["one-voxel-edge"] = edge,
                    ["cage-gap"] = gap
                },
                FeatureVoxelSpans = new Dictionary<string, int>(StringComparer.Ordinal)
                {
                    ["one-voxel-edge"] = edgeSpan,
                    ["cage-gap"] = 1
                },
                CadencePreserved = cadencePreserved,
                NormalizedWorldSize = new[] { 0.4, 0.4, 1.5 },
                IntegerBounds = new[]
                {
                    4,
                    4,
                    (int)Math.Round(1.5 * vpu)
                },
                OccupiedVoxels = 600 + vpu,
                AxisCompliant = true,
                GreedyMergedQuads = 90 + vpu,
                Vertices = (90 + vpu) * 4,
                Triangles = (90 + vpu) * 2,
                SourceSha256 = HeroVoxelProvider.HashFile(sourcePath),
                PreviewSha256 = HeroVoxelProvider.HashFile(previewPath)
            };
            File.WriteAllText(
                Path.Combine(candidateRoot, "audit.json"),
                JsonSerializer.Serialize(audit, new JsonSerializerOptions { WriteIndented = true }));
        }

        WriteCandidate(8, edge: true, gap: false, cadencePreserved: true, edgeSpan: 1);
        WriteCandidate(12, edge: true, gap: true, cadencePreserved: true, edgeSpan: 1);

        StoreSnapshot missingDensity = new StoreEngine(root, HeroStudioSubmodule(repositoryRoot)).Scan(false, false);
        AssetHealth missingDensityStaff = missingDensity.Assets.Single();
        Require(
            !missingDensityStaff.Published &&
            missingDensityStaff.FailedGate == "density" &&
            missingDensity.NextJob?.Target == densityFixtureId &&
            missingDensity.NextJob.Reason.Contains("density", StringComparison.Ordinal),
            "opted-in equipment missing density evidence must block at density on the same maintenance asset");

        var engine = new StoreEngine(root, HeroStudioSubmodule(repositoryRoot));
        ReferenceDensityEvidence first = engine.SelectDensity(
            densityFixtureId,
            write: false);
        ReferenceDensityEvidence second = engine.SelectDensity(
            densityFixtureId,
            write: false);
        string firstJson = JsonSerializer.Serialize(first);
        string secondJson = JsonSerializer.Serialize(second);
        Require(
            first.Version == 2 &&
            first.RulerVersion == 1 &&
            !string.IsNullOrWhiteSpace(first.RulerSha256) &&
            Math.Abs(first.EstimatedVpu - 12.0) < 1e-9 &&
            first.ApparentVoxelCounts["Staff"] == 18 &&
            first.CandidateSequence.SequenceEqual(new[] { 8, 10, 12, 16, 20, 24 }) &&
            first.SelectedVpu == 12 &&
            first.Candidates.Single(x => x.Vpu == 12).FeatureVoxelSpans["one-voxel-edge"] == 1 &&
            firstJson == secondJson,
            "v2 density operation must bind ruler/cadence evidence and select the lowest passing low-VPU candidate deterministically");

        engine.SelectDensity(densityFixtureId, write: true);
        AssetRecord staff = engine.DiscoverAssets().Single();
        StoreSnapshot sourceMismatch = new StoreEngine(root, HeroStudioSubmodule(repositoryRoot)).Scan(false, false);
        Require(
            sourceMismatch.Assets.Single().FailedGate == "source" &&
            sourceMismatch.Assets.Single().Gates.Single(x => x.Id == "source").Message.Contains(
                "does not match selected density VPU",
                StringComparison.Ordinal),
            "current density evidence with mismatched canonical VPU must fail at source");

        string canonicalSpec = Path.Combine(assetRoot, "Source", "FixtureStaff.voxel.yaml");
        string originalCanonicalSpec = File.ReadAllText(canonicalSpec);
        File.WriteAllText(
            canonicalSpec,
            System.Text.RegularExpressions.Regex.Replace(
                originalCanonicalSpec,
                @"(?m)^voxelsPerUnit:\s*\d+\s*$",
                "voxelsPerUnit: 12"));
        StoreSnapshot densityAndSourceCurrent = new StoreEngine(root, HeroStudioSubmodule(repositoryRoot)).Scan(false, false);
        Require(
            densityAndSourceCurrent.Assets.Single().Gates.Single(x => x.Id == "density").Passed &&
            densityAndSourceCurrent.Assets.Single().Gates.Single(x => x.Id == "source").Passed &&
            densityAndSourceCurrent.Assets.Single().FailedGate == "generation",
            "matching per-unit density/source VPU must advance beyond the source gate");
        File.WriteAllText(canonicalSpec, originalCanonicalSpec);

        StorePolicy storePolicy = ContractYaml.Load<StorePolicy>(
            Path.Combine(root, "Store", "policy.yaml"));
        engine.ValidateDensityEvidence(staff, storePolicy);

        (int ExitCode, string StdOut, string StdErr) RunDensityCli(bool write)
        {
            var start = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            start.ArgumentList.Add(typeof(StoreEngine).Assembly.Location);
            start.ArgumentList.Add("density");
            start.ArgumentList.Add(densityFixtureId);
            start.ArgumentList.Add("--root");
            start.ArgumentList.Add(root);
            if (write)
                start.ArgumentList.Add("--write");
            using Process process =
                Process.Start(start) ?? throw new InvalidOperationException("Failed to start density CLI.");
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (process.ExitCode, stdout, stderr);
        }

        var cli1 = RunDensityCli(write: false);
        var cli2 = RunDensityCli(write: false);
        Require(
            cli1.ExitCode == 0 &&
            cli2.ExitCode == 0 &&
            cli1.StdOut == cli2.StdOut,
            $"density CLI must produce deterministic output: {cli1.StdErr}{cli2.StdErr}");

        File.WriteAllText(
            inputPath,
            originalInput.Replace(
                "targetWorldLongAxis: 1.5",
                "targetWorldLongAxis: 1.6",
                StringComparison.Ordinal));
        ExpectFailure(
            () => engine.ValidateDensityEvidence(staff, storePolicy),
            "stale");
        StoreSnapshot staleDensity = new StoreEngine(root, HeroStudioSubmodule(repositoryRoot)).Scan(false, false);
        Require(
            staleDensity.Assets.Single().FailedGate == "density",
            "changed target extent must stale density at the density gate");
        File.WriteAllText(inputPath, originalInput);
        engine.SelectDensity(densityFixtureId, write: true);

        File.WriteAllText(
            inputPath,
            originalInput.Replace(
                "apparentVoxelCount: 18",
                "apparentVoxelCount: 20",
                StringComparison.Ordinal));
        ExpectFailure(
            () => engine.ValidateDensityEvidence(staff, storePolicy),
            "stale");
        File.WriteAllText(inputPath, originalInput);
        engine.SelectDensity(densityFixtureId, write: true);

        string rulerPath = Path.Combine(root, "Store", "vpu-ruler.yaml");
        string originalRuler = File.ReadAllText(rulerPath);
        File.WriteAllText(rulerPath, originalRuler + Environment.NewLine + "# ruler-revision-test");
        ExpectFailure(
            () => engine.ValidateDensityEvidence(staff, storePolicy),
            "stale");
        File.WriteAllText(rulerPath, originalRuler);
        engine.SelectDensity(densityFixtureId, write: true);

        string partSheet = Path.Combine(assetRoot, "Reference", "Parts", "Staff.png");
        byte[] partSheetBytes = File.ReadAllBytes(partSheet);
        WritePngHeader(partSheet, 999, 999);
        ExpectFailure(
            () => engine.ValidateDensityEvidence(staff, storePolicy),
            "stale");
        File.WriteAllBytes(partSheet, partSheetBytes);
        engine.SelectDensity(densityFixtureId, write: true);

        string candidatePreview = Path.Combine(
            densityRoot, "Candidates", "vpu-012", "Proof", "hero.png");
        byte[] previewBytes = File.ReadAllBytes(candidatePreview);
        WritePngHeader(candidatePreview, 513, 512);
        ExpectFailure(
            () => engine.ValidateDensityEvidence(staff, storePolicy),
            "preview evidence is stale");
        File.WriteAllBytes(candidatePreview, previewBytes);
        engine.ValidateDensityEvidence(staff, storePolicy);
    }
    finally
    {
        DeleteTree(root);
    }
}

static string PrepareReferenceFixture(string sourceRoot, string referenceYaml)
{
    string root = TempRoot();
    Directory.CreateDirectory(Path.Combine(root, "Store"));
    Directory.CreateDirectory(Path.Combine(root, "Collections"));
    File.WriteAllText(
        Path.Combine(root, "Store", "policy.yaml"),
        """
        version: 2
        name: ReferenceFixture
        collection: combat-foundation.v1
        defaultStyle: low-resolution-voxel.v1
        allowOptionalExpansion: false
        budget:
          maxVoxelsPerAsset: 4096
          maxPartsPerAsset: 16
        """);
    File.WriteAllText(
        Path.Combine(root, "Collections", "combat-foundation.v1.yaml"),
        """
        version: 1
        id: combat-foundation.v1
        requirements: []
        """);
    CopyFile(
        sourceRoot,
        root,
        "toolchain/hero-voxel.lock.yaml");

    string styleRoot = Path.Combine(root, "Styles", "low-resolution-voxel.v1");
    Directory.CreateDirectory(styleRoot);
    File.WriteAllText(
        Path.Combine(styleRoot, "style.yaml"),
        """
        version: 2
        id: low-resolution-voxel.v1
        references:
          avatar:
            - reference-01.png
            - reference-02.png
            - reference-03.png
          equipment:
            - equipment-bow-source.png
        traits:
          - chunky low-resolution voxel forms
          - chibi proportion language
        traitsByKind:
          equipment:
            - purposeful secondary construction detail
        """);
    WritePngHeader(Path.Combine(styleRoot, "reference-01.png"), 1200, 900);
    WritePngHeader(Path.Combine(styleRoot, "reference-02.png"), 1200, 900);
    WritePngHeader(Path.Combine(styleRoot, "reference-03.png"), 1200, 900);
    WritePngHeader(Path.Combine(styleRoot, "equipment-bow-source.png"), 1200, 900);

    string assetRoot = Path.Combine(
        root, "Assets", "Avatars", "FixtureAvatar");
    Directory.CreateDirectory(assetRoot);
    File.WriteAllText(
        Path.Combine(assetRoot, "asset.yaml"),
        """
        version: 2
        id: fixture.avatar
        kind: avatar
        unitRef: fixture/avatar
        source:
          base: ./Source/FixtureAvatar
          palette: ./Source/FixtureAvatar.palette.yaml
        style:
          family: low-resolution-voxel.v1
        """);
    string referenceRoot = Path.Combine(assetRoot, "Reference");
    Directory.CreateDirectory(referenceRoot);
    File.WriteAllText(Path.Combine(referenceRoot, "reference.yaml"), referenceYaml);
    return root;
}

static GateResult ReferenceGate(StoreSnapshot snapshot) =>
    snapshot.Assets.Single().Gates.Single(x => x.Id == "reference");


static void RunTargetStyleAuthorityTests(string sourceRoot)
{
    string root = PrepareReferenceFixture(
        sourceRoot,
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        """);
    try
    {
        var engine = new StoreEngine(root);
        ReferenceStyleAuthority legacyConstruction = engine.ResolveReferenceStyle("equipment");
        string legacyConstructionHash = engine.ReferenceStyleAuthoritySha256(legacyConstruction);
        Require(
            legacyConstruction.CommonReferencePaths.Count == 0 &&
            legacyConstruction.KindReferencePaths.Count == 1 &&
            Path.GetFileName(legacyConstruction.KindReferencePaths[0]) == "equipment-bow-source.png",
            "v2 construction style resolution must remain unchanged");
        ExpectFailure(
            () => engine.ResolveTargetReferenceStyle("equipment"),
            "version 3");

        string stylePath = Path.Combine(
            root, "Styles", "low-resolution-voxel.v1", "style.yaml");
        string v2 = File.ReadAllText(stylePath);
        string v3 = v2
            .Replace("version: 2", "version: 3", StringComparison.Ordinal)
            .Replace(
                "traits:",
                """
                targetReferences:
                  common:
                    - reference-02.png
                  equipment:
                    - equipment-bow-source.png
                  avatar:
                    - reference-01.png
                traits:
                """,
                StringComparison.Ordinal);
        File.WriteAllText(stylePath, v3);

        var v3Engine = new StoreEngine(root);
        ReferenceStyleAuthority v3Construction = v3Engine.ResolveReferenceStyle("equipment");
        string v3ConstructionHash = v3Engine.ReferenceStyleAuthoritySha256(v3Construction);
        Require(
            string.Equals(legacyConstructionHash, v3ConstructionHash, StringComparison.Ordinal),
            "adding targetReferences must not change legacy construction-style authority hash");

        ReferenceStyleAuthority target = v3Engine.ResolveTargetReferenceStyle("equipment");
        Require(
            target.CommonReferencePaths.Count == 1 &&
            Path.GetFileName(target.CommonReferencePaths[0]) == "reference-02.png" &&
            target.KindReferencePaths.Count == 1 &&
            Path.GetFileName(target.KindReferencePaths[0]) == "equipment-bow-source.png",
            "v3 target style must resolve target common plus matching-kind references");

        File.WriteAllText(
            stylePath,
            v3.Replace(
                "  equipment:\n    - equipment-bow-source.png\n  avatar:",
                "  avatar:",
                StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(root).ResolveTargetReferenceStyle("equipment"),
            "target scope 'equipment' must be non-empty");
    }
    finally
    {
        DeleteTree(root);
    }
    var activeEngine = new StoreEngine(sourceRoot);
    ReferenceStyleAuthority activeTarget =
        activeEngine.ResolveTargetReferenceStyle("equipment");
    Require(
        activeTarget.CommonReferencePaths.Count == 1 &&
        Path.GetFileName(activeTarget.CommonReferencePaths[0]) == "reference-02.png" &&
        activeTarget.KindReferencePaths.Select(Path.GetFileName).SequenceEqual(
            new[] { "equipment-bow-source.png", "equipment-general-reference.png" },
            StringComparer.Ordinal),
        "active equipment target authority must include polished common plus existing equipment guidance");

    foreach (AssetRecord asset in activeEngine.DiscoverAssets())
    {
        string auditPath = Path.Combine(asset.ReferenceDirectory, "audit.generated.json");
        AuditEvidence audit =
            JsonSerializer.Deserialize<AuditEvidence>(File.ReadAllText(auditPath))
            ?? throw new InvalidDataException($"Invalid reference audit: {auditPath}");
        ReferenceStyleAuthority construction =
            activeEngine.ResolveReferenceStyle(asset.Manifest.Kind);
        Require(
            string.Equals(
                audit.StyleAuthoritySha256,
                activeEngine.ReferenceStyleAuthoritySha256(construction),
                StringComparison.OrdinalIgnoreCase),
            $"legacy construction-style authority must remain current for {asset.Manifest.Id}");
    }
}


static void RunReferenceV3StructureTests(string sourceRoot)
{
    static string ReferenceRoot(string root) => Path.Combine(
        root,
        "Assets",
        "Avatars",
        "FixtureAvatar",
        "Reference");

    static void WriteBaseVisuals(string referenceRoot)
    {
        Directory.CreateDirectory(Path.Combine(referenceRoot, "Parts"));
        WritePngHeader(Path.Combine(referenceRoot, "composition.png"), 1200, 800);
        WritePngHeader(Path.Combine(referenceRoot, "Parts", "Body.png"), 800, 800);
        WritePngHeader(Path.Combine(referenceRoot, "target.png"), 1200, 900);
        File.WriteAllText(Path.Combine(referenceRoot, "target.provenance.json"), "{}");
        File.WriteAllText(Path.Combine(referenceRoot, "fidelity.audit.json"), "{}");
    }

    string v2Root = PrepareReferenceFixture(
        sourceRoot,
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        """);
    try
    {
        string referenceRoot = ReferenceRoot(v2Root);
        Directory.CreateDirectory(Path.Combine(referenceRoot, "Parts"));
        WritePngHeader(Path.Combine(referenceRoot, "composition.png"), 1200, 800);
        WritePngHeader(Path.Combine(referenceRoot, "Parts", "Body.png"), 800, 800);
        StoreEngine engine = new(v2Root);
        ReferenceContract contract =
            engine.ValidateReferenceContractStructure(engine.DiscoverAssets().Single());
        Require(contract.Version == 2, "valid legacy v2 reference contract must remain accepted");
    }
    finally
    {
        DeleteTree(v2Root);
    }

    const string validV3 =
        """
        version: 3
        target:
          image: target.png
          provenance: target.provenance.json
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        fidelity: fidelity.audit.json
        """;

    string validRoot = PrepareReferenceFixture(sourceRoot, validV3);
    try
    {
        string referenceRoot = ReferenceRoot(validRoot);
        WriteBaseVisuals(referenceRoot);
        StoreEngine engine = new(validRoot);
        ReferenceContract contract =
            engine.ValidateReferenceContractStructure(engine.DiscoverAssets().Single());
        Require(
            contract.Version == 3 &&
            contract.Target?.Image == "target.png" &&
            contract.Target.Provenance == "target.provenance.json" &&
            contract.Fidelity == "fidelity.audit.json",
            "valid target-driven v3 mappings must be preserved");
    }
    finally
    {
        DeleteTree(validRoot);
    }

    void ExpectV3StructureFailure(
        string yaml,
        string expected,
        Action<string>? arrange = null)
    {
        string root = PrepareReferenceFixture(sourceRoot, yaml);
        try
        {
            string referenceRoot = ReferenceRoot(root);
            WriteBaseVisuals(referenceRoot);
            arrange?.Invoke(referenceRoot);
            StoreEngine engine = new(root);
            AssetRecord asset = engine.DiscoverAssets().Single();
            ExpectFailure(
                () => engine.ValidateReferenceContractStructure(asset),
                expected);
        }
        finally
        {
            DeleteTree(root);
        }
    }

    ExpectV3StructureFailure(
        """
        version: 3
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        fidelity: fidelity.audit.json
        """,
        "requires a Product Target");

    ExpectV3StructureFailure(
        """
        version: 3
        target:
          image: target.png
          provenance: target.provenance.json
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        """,
        "requires target image, target provenance, and fidelity");

    ExpectV3StructureFailure(
        """
        version: 3
        target:
          image: target.png
          provenance: target.provenance.json
        composition: composition.png
        requiredParts:
          - Body
          - Head
        partSheets:
          Body: Parts/Body.png
        fidelity: fidelity.audit.json
        """,
        "match RequiredParts exactly");

    ExpectV3StructureFailure(
        """
        version: 3
        target:
          image: target.svg
          provenance: target.provenance.json
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        fidelity: fidelity.audit.json
        """,
        "Product Target must be a PNG",
        referenceRoot =>
            WritePngHeader(Path.Combine(referenceRoot, "target.svg"), 1200, 900));

    ExpectV3StructureFailure(
        validV3,
        "Invalid PNG signature",
        referenceRoot =>
            WritePngHeader(
                Path.Combine(referenceRoot, "target.png"),
                1200,
                900,
                validSignature: false));

    ExpectV3StructureFailure(
        """
        version: 3
        target:
          image: target.png
          provenance: target.provenance.json
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        fidelity: fidelity.audit.json
        vpu: 32
        """,
        "vpu");

    ExpectV3StructureFailure(
        """
        version: 3
        target:
          image: target.png
          provenance: target.provenance.json
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        fidelity: fidelity.audit.json
        targetBounds: [6, 3, 10]
        """,
        "targetBounds");

    ExpectV3StructureFailure(
        validV3,
        "Product Target provenance is missing",
        referenceRoot =>
            File.Delete(Path.Combine(referenceRoot, "target.provenance.json")));

    ExpectV3StructureFailure(
        validV3,
        "Target fidelity evidence is missing",
        referenceRoot =>
            File.Delete(Path.Combine(referenceRoot, "fidelity.audit.json")));
}


static void RunTargetProvenanceTests(string sourceRoot)
{
    const string validV3 =
        """
        version: 3
        target:
          image: target.png
          provenance: target.provenance.json
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        fidelity: fidelity.audit.json
        """;

    static string ReferenceRoot(string root) => Path.Combine(
        root,
        "Assets",
        "Avatars",
        "FixtureAvatar",
        "Reference");

    static void WriteV3Visuals(string referenceRoot)
    {
        Directory.CreateDirectory(Path.Combine(referenceRoot, "Parts"));
        WritePngHeader(Path.Combine(referenceRoot, "composition.png"), 1200, 800);
        WritePngHeader(Path.Combine(referenceRoot, "Parts", "Body.png"), 800, 800);
        WritePngHeader(Path.Combine(referenceRoot, "target.png"), 1200, 900);
        File.WriteAllText(Path.Combine(referenceRoot, "fidelity.audit.json"), "{}");
    }

    static void UpgradeTargetStyle(string root)
    {
        string stylePath = Path.Combine(
            root, "Styles", "low-resolution-voxel.v1", "style.yaml");
        string style = File.ReadAllText(stylePath);
        style = style
            .Replace("version: 2", "version: 3", StringComparison.Ordinal)
            .Replace(
                "traits:",
                """
                targetReferences:
                  common:
                    - reference-02.png
                  avatar:
                    - reference-01.png
                  equipment:
                    - equipment-bow-source.png
                traits:
                """,
                StringComparison.Ordinal);
        File.WriteAllText(stylePath, style);
    }

    static Dictionary<string, string> StyleHashes(
        string root,
        ReferenceStyleAuthority style) =>
        style.CommonReferencePaths
            .Concat(style.KindReferencePaths)
            .ToDictionary(
                path => Path.GetRelativePath(root, path).Replace('\\', '/'),
                HeroVoxelProvider.HashFile,
                StringComparer.Ordinal);

    string generatedRoot = PrepareReferenceFixture(sourceRoot, validV3);
    try
    {
        string referenceRoot = ReferenceRoot(generatedRoot);
        WriteV3Visuals(referenceRoot);
        UpgradeTargetStyle(generatedRoot);

        StoreEngine engine = new(generatedRoot);
        AssetRecord asset = engine.DiscoverAssets().Single();
        ReferenceStyleAuthority targetStyle =
            engine.ResolveTargetReferenceStyle(asset.Manifest.Kind);
        var provenance = new ReferenceTargetProvenance
        {
            Version = 1,
            Origin = "generated",
            TargetSha256 = HeroVoxelProvider.HashFile(
                Path.Combine(referenceRoot, "target.png")),
            TargetStyleAuthoritySha256 =
                engine.ReferenceStyleAuthoritySha256(targetStyle),
            TargetStyleSha256 = StyleHashes(generatedRoot, targetStyle)
        };
        string provenancePath = Path.Combine(referenceRoot, "target.provenance.json");
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));

        string first = JsonSerializer.Serialize(engine.ValidateTargetProvenance(asset));
        string second = JsonSerializer.Serialize(engine.ValidateTargetProvenance(asset));
        Require(
            first == second,
            "unchanged generated Product Target provenance must validate deterministically");

        string targetPath = Path.Combine(referenceRoot, "target.png");
        byte[] originalTarget = File.ReadAllBytes(targetPath);
        WritePngHeader(targetPath, 1201, 900);
        ExpectFailure(
            () => engine.ValidateTargetProvenance(asset),
            "stale for target.png");
        File.WriteAllBytes(targetPath, originalTarget);

        string styleInput = Path.Combine(
            generatedRoot,
            "Styles",
            "low-resolution-voxel.v1",
            "reference-02.png");
        byte[] originalStyle = File.ReadAllBytes(styleInput);
        WritePngHeader(styleInput, 1201, 900);
        ExpectFailure(
            () => new StoreEngine(generatedRoot).ValidateTargetProvenance(asset),
            "generated style authority is stale");
        File.WriteAllBytes(styleInput, originalStyle);
        engine.ValidateTargetProvenance(asset);

        string requestPath = Path.Combine(referenceRoot, "target.request.json");
        File.WriteAllText(
            requestPath,
            """
            {
              "version": 1,
              "intent": "Generate a polished voxel avatar Product Target from current style authority."
            }
            """);
        provenance.Version = 2;
        provenance.Generator = new ReferenceTargetGeneratorProvenance
        {
            Kind = "image-model",
            Provider = "fixture-provider",
            Model = "fixture-image-model",
            Request = new ReferenceTargetGeneratorRequestProvenance
            {
                Path = "target.request.json",
                Sha256 = HeroVoxelProvider.HashFile(requestPath)
            }
        };
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));
        engine.ValidateTargetProvenance(asset);

        byte[] originalRequest = File.ReadAllBytes(requestPath);
        File.AppendAllText(requestPath, Environment.NewLine);
        ExpectFailure(
            () => engine.ValidateTargetProvenance(asset),
            "generation request provenance is stale");
        File.WriteAllBytes(requestPath, originalRequest);

        provenance.Generator.Kind = "procedural";
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));
        ExpectFailure(
            () => engine.ValidateTargetProvenance(asset),
            "generator kind must be 'image-model'");

        provenance.Generator.Kind = "image-model";
        provenance.Generator = null;
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));
        ExpectFailure(
            () => engine.ValidateTargetProvenance(asset),
            "requires image-model generator evidence");

        provenance.Generator = new ReferenceTargetGeneratorProvenance
        {
            Kind = "image-model",
            Provider = "fixture-provider",
            Model = "fixture-image-model",
            Request = new ReferenceTargetGeneratorRequestProvenance
            {
                Path = "target.request.json",
                Sha256 = HeroVoxelProvider.HashFile(requestPath)
            }
        };
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));
        engine.ValidateTargetProvenance(asset);
        provenance.Version = ReferenceTargetProvenance.AcquisitionNeutralVersion;
        provenance.Origin = ReferenceTargetProvenance.RepositoryAuthoredOrigin;
        provenance.Generator = null;
        provenance.Source = null;
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));

        ReferenceTargetProvenance roundTripped =
            JsonSerializer.Deserialize<ReferenceTargetProvenance>(
                File.ReadAllText(provenancePath))!;
        Require(
            roundTripped.Version == ReferenceTargetProvenance.AcquisitionNeutralVersion &&
            roundTripped.Origin == ReferenceTargetProvenance.RepositoryAuthoredOrigin &&
            roundTripped.Generator == null &&
            roundTripped.Source == null,
            "v3 repository-authored Product Target provenance must round-trip without generator/source metadata");
        engine.ValidateTargetProvenance(asset);

        string? repositoryAuthoredStyleAuthority = provenance.TargetStyleAuthoritySha256;
        provenance.TargetStyleAuthoritySha256 = null;
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));
        ExpectFailure(
            () => engine.ValidateTargetProvenance(asset),
            "repository-authored style authority is stale");
        provenance.TargetStyleAuthoritySha256 = repositoryAuthoredStyleAuthority;

        provenance.Source = new ReferenceTargetSourceProvenance
        {
            Path = "target.png",
            Sha256 = provenance.TargetSha256,
            Relation = "exact"
        };
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));
        ExpectFailure(
            () => engine.ValidateTargetProvenance(asset),
            "cannot declare imported source evidence");
        provenance.Source = null;
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));
        engine.ValidateTargetProvenance(asset);
    }
    finally
    {
        DeleteTree(generatedRoot);
    }

    string importedRoot = PrepareReferenceFixture(sourceRoot, validV3);
    try
    {
        string referenceRoot = ReferenceRoot(importedRoot);
        WriteV3Visuals(referenceRoot);
        string sourceDir = Path.Combine(referenceRoot, "Source");
        Directory.CreateDirectory(sourceDir);
        string sourcePath = Path.Combine(sourceDir, "imported.png");
        WritePngHeader(sourcePath, 1200, 900);
        string targetPath = Path.Combine(referenceRoot, "target.png");
        File.Copy(sourcePath, targetPath, true);

        var provenance = new ReferenceTargetProvenance
        {
            Version = 1,
            Origin = "imported",
            TargetSha256 = HeroVoxelProvider.HashFile(targetPath),
            Source = new ReferenceTargetSourceProvenance
            {
                Path = "Source/imported.png",
                Sha256 = HeroVoxelProvider.HashFile(sourcePath),
                Relation = "exact"
            }
        };
        string provenancePath = Path.Combine(referenceRoot, "target.provenance.json");
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));

        StoreEngine engine = new(importedRoot);
        AssetRecord asset = engine.DiscoverAssets().Single();
        engine.ValidateTargetProvenance(asset);

        byte[] originalSource = File.ReadAllBytes(sourcePath);
        provenance.Version = ReferenceTargetProvenance.AcquisitionNeutralVersion;
        provenance.Generator = new ReferenceTargetGeneratorProvenance
        {
            Kind = "optional-trace",
            Provider = string.Empty,
            Model = string.Empty
        };
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));
        ReferenceTargetProvenance importedRoundTripped =
            JsonSerializer.Deserialize<ReferenceTargetProvenance>(
                File.ReadAllText(provenancePath))!;
        Require(
            importedRoundTripped.Version == ReferenceTargetProvenance.AcquisitionNeutralVersion &&
            importedRoundTripped.Origin == ReferenceTargetProvenance.ImportedOrigin &&
            importedRoundTripped.Source != null,
            "v3 imported Product Target provenance must round-trip without required provider/model evidence");
        engine.ValidateTargetProvenance(asset);


        WritePngHeader(sourcePath, 1201, 900);
        ExpectFailure(
            () => engine.ValidateTargetProvenance(asset),
            "imported source provenance is stale");
        File.WriteAllBytes(sourcePath, originalSource);

        WritePngHeader(sourcePath, 1000, 900);
        provenance.Source.Sha256 = HeroVoxelProvider.HashFile(sourcePath);
        provenance.Source.Relation = "canonical-derivative";
        provenance.TargetSha256 = HeroVoxelProvider.HashFile(targetPath);
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));
        engine.ValidateTargetProvenance(asset);

        provenance.Source.Relation = "unknown";
        File.WriteAllText(
            provenancePath,
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));
        ExpectFailure(
            () => engine.ValidateTargetProvenance(asset),
            "exact or canonical-derivative");
    }
    finally
    {
        DeleteTree(importedRoot);
    }
}


static void RunTargetFidelityTests(string sourceRoot)
{
    const string validV3 =
        """
        version: 3
        target:
          image: target.png
          provenance: target.provenance.json
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        fidelity: fidelity.audit.json
        """;

    static string ReferenceRoot(string root) => Path.Combine(
        root,
        "Assets",
        "Avatars",
        "FixtureAvatar",
        "Reference");

    static Dictionary<string, string> PassingCriteria() => new(StringComparer.Ordinal)
    {
        ["silhouette"] = "pass",
        ["proportions"] = "pass",
        ["constructionRichness"] = "pass",
        ["materialColorHierarchy"] = "pass",
        ["meaningfulDetailDensity"] = "pass",
        ["visualEvidenceDominance"] = "pass",
        ["implementationNeutral"] = "pass"
    };

    static ReferenceFidelityAudit ValidAudit(string referenceRoot)
    {
        string target = Path.Combine(referenceRoot, "target.png");
        string composition = Path.Combine(referenceRoot, "composition.png");
        string part = Path.Combine(referenceRoot, "Parts", "Body.png");
        return new ReferenceFidelityAudit
        {
            Version = 1,
            Target = new ReferenceFidelityTargetEvidence
            {
                Path = "target.png",
                Sha256 = HeroVoxelProvider.HashFile(target)
            },
            Sheets = new Dictionary<string, ReferenceFidelitySheetEvidence>(StringComparer.Ordinal)
            {
                ["composition.png"] = new()
                {
                    Sha256 = HeroVoxelProvider.HashFile(composition),
                    Criteria = PassingCriteria()
                },
                ["Parts/Body.png"] = new()
                {
                    Sha256 = HeroVoxelProvider.HashFile(part),
                    Criteria = PassingCriteria()
                }
            }
        };
    }

    static void WriteAudit(string referenceRoot, ReferenceFidelityAudit audit) =>
        File.WriteAllText(
            Path.Combine(referenceRoot, "fidelity.audit.json"),
            JsonSerializer.Serialize(
                audit,
                new JsonSerializerOptions { WriteIndented = true }));

    string root = PrepareReferenceFixture(sourceRoot, validV3);
    try
    {
        string referenceRoot = ReferenceRoot(root);
        Directory.CreateDirectory(Path.Combine(referenceRoot, "Parts"));
        WritePngHeader(Path.Combine(referenceRoot, "target.png"), 1200, 900);
        WritePngHeader(Path.Combine(referenceRoot, "composition.png"), 1200, 800);
        WritePngHeader(Path.Combine(referenceRoot, "Parts", "Body.png"), 800, 800);
        File.WriteAllText(Path.Combine(referenceRoot, "target.provenance.json"), "{}");

        StoreEngine engine = new(root);
        AssetRecord asset = engine.DiscoverAssets().Single();

        ReferenceFidelityAudit valid = ValidAudit(referenceRoot);
        WriteAudit(referenceRoot, valid);
        string first = JsonSerializer.Serialize(engine.ValidateFidelityAudit(asset));
        string second = JsonSerializer.Serialize(engine.ValidateFidelityAudit(asset));
        Require(first == second, "unchanged target fidelity evidence must validate deterministically");

        ReferenceFidelityAudit missingCriterion = ValidAudit(referenceRoot);
        missingCriterion.Sheets["composition.png"].Criteria.Remove("constructionRichness");
        WriteAudit(referenceRoot, missingCriterion);
        ExpectFailure(
            () => engine.ValidateFidelityAudit(asset),
            "required criteria exactly");

        ReferenceFidelityAudit failedCriterion = ValidAudit(referenceRoot);
        failedCriterion.Sheets["composition.png"].Criteria["constructionRichness"] = "fail";
        WriteAudit(referenceRoot, failedCriterion);
        ExpectFailure(
            () => engine.ValidateFidelityAudit(asset),
            "constructionRichness");

        ReferenceFidelityAudit missingSheet = ValidAudit(referenceRoot);
        missingSheet.Sheets.Remove("Parts/Body.png");
        WriteAudit(referenceRoot, missingSheet);
        ExpectFailure(
            () => engine.ValidateFidelityAudit(asset),
            "cover exactly");

        ReferenceFidelityAudit unknownSheet = ValidAudit(referenceRoot);
        unknownSheet.Sheets["Parts/Unknown.png"] = new()
        {
            Sha256 = new string('0', 64),
            Criteria = PassingCriteria()
        };
        WriteAudit(referenceRoot, unknownSheet);
        ExpectFailure(
            () => engine.ValidateFidelityAudit(asset),
            "cover exactly");

        ReferenceFidelityAudit staleSheet = ValidAudit(referenceRoot);
        WriteAudit(referenceRoot, staleSheet);
        string compositionPath = Path.Combine(referenceRoot, "composition.png");
        byte[] compositionBytes = File.ReadAllBytes(compositionPath);
        File.WriteAllBytes(
            compositionPath,
            compositionBytes.Concat(new byte[] { 0 }).ToArray());
        ExpectFailure(
            () => engine.ValidateFidelityAudit(asset),
            "stale for 'composition.png'");
        File.WriteAllBytes(compositionPath, compositionBytes);

        ReferenceFidelityAudit staleTarget = ValidAudit(referenceRoot);
        WriteAudit(referenceRoot, staleTarget);
        string targetPath = Path.Combine(referenceRoot, "target.png");
        byte[] targetBytes = File.ReadAllBytes(targetPath);
        WritePngHeader(targetPath, 1201, 900);
        ExpectFailure(
            () => engine.ValidateFidelityAudit(asset),
            "Product Target evidence is stale");
        File.WriteAllBytes(targetPath, targetBytes);

        WriteAudit(referenceRoot, ValidAudit(referenceRoot));
        engine.ValidateFidelityAudit(asset);
    }
    finally
    {
        DeleteTree(root);
    }
}


static void RunReferenceSealVersionTests(string sourceRoot)
{
    static string ReferenceRoot(string root) => Path.Combine(
        root,
        "Assets",
        "Avatars",
        "FixtureAvatar",
        "Reference");

    string v2Root = PrepareReferenceFixture(
        sourceRoot,
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        """);
    try
    {
        string referenceRoot = ReferenceRoot(v2Root);
        Directory.CreateDirectory(Path.Combine(referenceRoot, "Parts"));
        WritePngHeader(Path.Combine(referenceRoot, "composition.png"), 1200, 800);
        WritePngHeader(Path.Combine(referenceRoot, "Parts", "Body.png"), 800, 800);
        StoreEngine engine = new(v2Root);
        engine.SealReference("fixture.avatar");
        string auditPath = Path.Combine(referenceRoot, "audit.generated.json");
        byte[] first = File.ReadAllBytes(auditPath);
        engine.SealReference("fixture.avatar");
        byte[] second = File.ReadAllBytes(auditPath);
        Require(
            first.SequenceEqual(second),
            "legacy v2 reference resealing must remain byte-identical");
    }
    finally
    {
        DeleteTree(v2Root);
    }

    string v3Root = PrepareReferenceFixture(
        sourceRoot,
        """
        version: 3
        target:
          image: target.png
          provenance: target.provenance.json
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        fidelity: fidelity.audit.json
        """);
    try
    {
        string referenceRoot = ReferenceRoot(v3Root);
        Directory.CreateDirectory(Path.Combine(referenceRoot, "Parts"));
        Directory.CreateDirectory(Path.Combine(referenceRoot, "Source"));
        WritePngHeader(Path.Combine(referenceRoot, "composition.png"), 1200, 800);
        WritePngHeader(Path.Combine(referenceRoot, "Parts", "Body.png"), 800, 800);
        string imported = Path.Combine(referenceRoot, "Source", "imported.png");
        string target = Path.Combine(referenceRoot, "target.png");
        WritePngHeader(imported, 1200, 900);
        File.Copy(imported, target, true);

        var provenance = new ReferenceTargetProvenance
        {
            Version = 1,
            Origin = "imported",
            TargetSha256 = HeroVoxelProvider.HashFile(target),
            Source = new ReferenceTargetSourceProvenance
            {
                Path = "Source/imported.png",
                Sha256 = HeroVoxelProvider.HashFile(imported),
                Relation = "exact"
            }
        };
        File.WriteAllText(
            Path.Combine(referenceRoot, "target.provenance.json"),
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));

        Dictionary<string, string> criteria = new(StringComparer.Ordinal)
        {
            ["silhouette"] = "pass",
            ["proportions"] = "pass",
            ["constructionRichness"] = "pass",
            ["materialColorHierarchy"] = "pass",
            ["meaningfulDetailDensity"] = "pass",
            ["visualEvidenceDominance"] = "pass",
            ["implementationNeutral"] = "pass"
        };
        var fidelity = new ReferenceFidelityAudit
        {
            Version = 1,
            Target = new ReferenceFidelityTargetEvidence
            {
                Path = "target.png",
                Sha256 = HeroVoxelProvider.HashFile(target)
            },
            Sheets = new Dictionary<string, ReferenceFidelitySheetEvidence>(StringComparer.Ordinal)
            {
                ["composition.png"] = new()
                {
                    Sha256 = HeroVoxelProvider.HashFile(
                        Path.Combine(referenceRoot, "composition.png")),
                    Criteria = new Dictionary<string, string>(criteria, StringComparer.Ordinal)
                },
                ["Parts/Body.png"] = new()
                {
                    Sha256 = HeroVoxelProvider.HashFile(
                        Path.Combine(referenceRoot, "Parts", "Body.png")),
                    Criteria = new Dictionary<string, string>(criteria, StringComparer.Ordinal)
                }
            }
        };
        File.WriteAllText(
            Path.Combine(referenceRoot, "fidelity.audit.json"),
            JsonSerializer.Serialize(
                fidelity,
                new JsonSerializerOptions { WriteIndented = true }));

        StoreEngine engine = new(v3Root);
        engine.SealReference("fixture.avatar");
        string auditPath = Path.Combine(referenceRoot, "audit.generated.json");
        byte[] first = File.ReadAllBytes(auditPath);
        using (JsonDocument audit = JsonDocument.Parse(first))
        {
            string[] keys = audit.RootElement
                .GetProperty("Sha256")
                .EnumerateObject()
                .Select(x => x.Name)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
            string[] expected =
            {
                "Parts/Body.png",
                "composition.png",
                "fidelity.audit.json",
                "reference.yaml",
                "target.png",
                "target.provenance.json"
            };
            Require(
                keys.SequenceEqual(expected.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal),
                "v3 reference audit must cover exactly target/provenance/Composition/Parts/fidelity plus contract");
            Require(
                !keys.Contains("Source/imported.png", StringComparer.Ordinal),
                "import source provenance must not become construction reference audit authority");
        }

        engine.SealReference("fixture.avatar");
        byte[] second = File.ReadAllBytes(auditPath);
        Require(
            first.SequenceEqual(second),
            "target-driven v3 reference resealing must be byte-identical");
    }
    finally
    {
        DeleteTree(v3Root);
    }
}

static void RunEvidenceHashPortabilityTests()
{
    string root = TempRoot();
    try
    {
        string textPath = Path.Combine(root, "evidence.yaml");
        const string logicalText = "version: 1\nname: portable\n";
        File.WriteAllText(textPath, logicalText);
        string lfHash = EvidenceHash.HashFile(textPath);

        File.WriteAllText(
            textPath,
            logicalText.Replace("\n", "\r\n", StringComparison.Ordinal));
        string crlfHash = EvidenceHash.HashFile(textPath);
        Require(
            string.Equals(lfHash, crlfHash, StringComparison.Ordinal),
            "repository text evidence hash must be invariant to LF/CRLF checkout representation");
        Require(
            EvidenceHash.TextFileEquals(textPath, logicalText),
            "generated text comparison must accept CRLF checkout representation for canonical LF output");

        File.WriteAllText(
            textPath,
            logicalText.Replace("\n", "\r", StringComparison.Ordinal));
        string crHash = EvidenceHash.HashFile(textPath);
        Require(
            string.Equals(lfHash, crHash, StringComparison.Ordinal),
            "repository text evidence hash must normalize lone CR line endings to LF");

        File.WriteAllText(textPath, "version: 2\nname: portable\n");
        Require(
            !string.Equals(
                lfHash,
                EvidenceHash.HashFile(textPath),
                StringComparison.Ordinal),
            "substantive repository text changes must change evidence hash");
        Require(
            !EvidenceHash.TextFileEquals(textPath, logicalText),
            "generated text comparison must reject substantive text changes");

        string binaryPath = Path.Combine(root, "evidence.png");
        File.WriteAllBytes(binaryPath, new byte[] { 1, 13, 10, 2 });
        string binaryCrLf = EvidenceHash.HashFile(binaryPath);
        File.WriteAllBytes(binaryPath, new byte[] { 1, 10, 2 });
        string binaryLf = EvidenceHash.HashFile(binaryPath);
        Require(
            !string.Equals(binaryCrLf, binaryLf, StringComparison.Ordinal),
            "binary evidence must remain byte-exact and must not normalize CRLF");
    }
    finally
    {
        DeleteTree(root);
    }
}


static void RunReferenceV3PublicationTests(string sourceRoot)
{
    static string ReferenceRoot(string root) => Path.Combine(
        root, "Assets", "Avatars", "FixtureAvatar", "Reference");

    static Dictionary<string, string> PassingCriteria() => new(StringComparer.Ordinal)
    {
        ["silhouette"] = "pass",
        ["proportions"] = "pass",
        ["constructionRichness"] = "pass",
        ["materialColorHierarchy"] = "pass",
        ["meaningfulDetailDensity"] = "pass",
        ["visualEvidenceDominance"] = "pass",
        ["implementationNeutral"] = "pass"
    };

    static void UpgradeTargetStyle(string root)
    {
        string stylePath = Path.Combine(
            root, "Styles", "low-resolution-voxel.v1", "style.yaml");
        string style = File.ReadAllText(stylePath);
        style = style
            .Replace("version: 2", "version: 3", StringComparison.Ordinal)
            .Replace(
                "traits:",
                """
                targetReferences:
                  common:
                    - reference-02.png
                  avatar:
                    - reference-01.png
                  equipment:
                    - equipment-bow-source.png
                traits:
                """,
                StringComparison.Ordinal);
        File.WriteAllText(stylePath, style);
    }

    static Dictionary<string, string> StyleHashes(
        string root,
        ReferenceStyleAuthority style) =>
        style.CommonReferencePaths
            .Concat(style.KindReferencePaths)
            .ToDictionary(
                path => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'),
                HeroVoxelProvider.HashFile,
                StringComparer.Ordinal);

    static void WriteV3Package(string root, string referenceRoot)
    {
        Directory.CreateDirectory(Path.Combine(referenceRoot, "Parts"));
        WritePngHeader(Path.Combine(referenceRoot, "composition.png"), 1200, 800);
        WritePngHeader(Path.Combine(referenceRoot, "Parts", "Body.png"), 800, 800);
        string target = Path.Combine(referenceRoot, "target.png");
        WritePngHeader(target, 1200, 900);

        StoreEngine provenanceEngine = new(root);
        ReferenceStyleAuthority targetStyle =
            provenanceEngine.ResolveTargetReferenceStyle("avatar");
        var provenance = new ReferenceTargetProvenance
        {
            Version = ReferenceTargetProvenance.AcquisitionNeutralVersion,
            Origin = ReferenceTargetProvenance.RepositoryAuthoredOrigin,
            TargetSha256 = HeroVoxelProvider.HashFile(target),
            TargetStyleAuthoritySha256 =
                provenanceEngine.ReferenceStyleAuthoritySha256(targetStyle),
            TargetStyleSha256 = StyleHashes(root, targetStyle)
        };
        File.WriteAllText(
            Path.Combine(referenceRoot, "target.provenance.json"),
            JsonSerializer.Serialize(provenance, new JsonSerializerOptions { WriteIndented = true }));

        var fidelity = new ReferenceFidelityAudit
        {
            Version = 1,
            Target = new ReferenceFidelityTargetEvidence
            {
                Path = "target.png",
                Sha256 = HeroVoxelProvider.HashFile(target)
            },
            Sheets = new Dictionary<string, ReferenceFidelitySheetEvidence>(StringComparer.Ordinal)
            {
                ["composition.png"] = new()
                {
                    Sha256 = HeroVoxelProvider.HashFile(Path.Combine(referenceRoot, "composition.png")),
                    Criteria = PassingCriteria()
                },
                ["Parts/Body.png"] = new()
                {
                    Sha256 = HeroVoxelProvider.HashFile(Path.Combine(referenceRoot, "Parts", "Body.png")),
                    Criteria = PassingCriteria()
                }
            }
        };
        File.WriteAllText(
            Path.Combine(referenceRoot, "fidelity.audit.json"),
            JsonSerializer.Serialize(fidelity, new JsonSerializerOptions { WriteIndented = true }));
    }

    string v3Root = PrepareReferenceFixture(
        sourceRoot,
        """
        version: 3
        target:
          image: target.png
          provenance: target.provenance.json
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        fidelity: fidelity.audit.json
        """);
    try
    {
        string referenceRoot = ReferenceRoot(v3Root);
        UpgradeTargetStyle(v3Root);
        WriteV3Package(v3Root, referenceRoot);
        StoreEngine engine = new(v3Root);
        engine.SealReference("fixture.avatar");
        Require(
            ReferenceGate(engine.Scan(false, false)).Passed,
            "current v3 Product Target/provenance/fidelity package must pass reference gate");

        string provenancePath = Path.Combine(referenceRoot, "target.provenance.json");
        byte[] provenanceBytes = File.ReadAllBytes(provenancePath);
        File.Delete(provenancePath);
        StoreSnapshot missingProvenanceSnapshot = engine.Scan(false, false);
        GateResult missingProvenance = ReferenceGate(missingProvenanceSnapshot);
        Require(
            !missingProvenance.Passed &&
            missingProvenance.Message.Contains("provenance is missing", StringComparison.OrdinalIgnoreCase) &&
            missingProvenanceSnapshot.Assets.Single().Gates
                .Single(x => x.Id == "source").Message.Contains("Blocked", StringComparison.OrdinalIgnoreCase),
            "missing v3 target provenance must fail at reference and block source");
        File.WriteAllBytes(provenancePath, provenanceBytes);

        string targetPath = Path.Combine(referenceRoot, "target.png");
        byte[] targetBytes = File.ReadAllBytes(targetPath);
        File.WriteAllBytes(targetPath, targetBytes.Concat(new byte[] { 0 }).ToArray());
        StoreSnapshot staleTargetSnapshot = engine.Scan(false, false);
        GateResult staleTarget = ReferenceGate(staleTargetSnapshot);
        Require(
            !staleTarget.Passed &&
            staleTarget.Message.Contains("stale for target.png", StringComparison.OrdinalIgnoreCase) &&
            staleTargetSnapshot.Assets.Single().Gates
                .Single(x => x.Id == "source").Message.Contains("Blocked", StringComparison.OrdinalIgnoreCase),
            "changed Product Target must fail reference provenance and block source before outer seal validation");
        File.WriteAllBytes(targetPath, targetBytes);

        string fidelityPath = Path.Combine(referenceRoot, "fidelity.audit.json");
        byte[] fidelityBytes = File.ReadAllBytes(fidelityPath);
        ReferenceFidelityAudit failed =
            JsonSerializer.Deserialize<ReferenceFidelityAudit>(File.ReadAllText(fidelityPath))
            ?? throw new InvalidDataException("fixture fidelity missing");
        failed.Sheets["composition.png"].Criteria["constructionRichness"] = "fail";
        File.WriteAllText(
            fidelityPath,
            JsonSerializer.Serialize(failed, new JsonSerializerOptions { WriteIndented = true }));
        GateResult failedFidelity = ReferenceGate(engine.Scan(false, false));
        Require(
            !failedFidelity.Passed &&
            failedFidelity.Message.Contains("constructionRichness", StringComparison.Ordinal),
            "failed v3 fidelity criterion must fail at reference");
        File.WriteAllBytes(fidelityPath, fidelityBytes);

        ReferenceFidelityAudit implementationOwned =
            JsonSerializer.Deserialize<ReferenceFidelityAudit>(File.ReadAllText(fidelityPath))
            ?? throw new InvalidDataException("fixture fidelity missing");
        implementationOwned.Sheets["composition.png"].Criteria["implementationNeutral"] = "fail";
        File.WriteAllText(
            fidelityPath,
            JsonSerializer.Serialize(
                implementationOwned,
                new JsonSerializerOptions { WriteIndented = true }));
        StoreSnapshot implementationBlocked = engine.Scan(false, false);
        GateResult implementationGate = ReferenceGate(implementationBlocked);
        Require(
            !implementationGate.Passed &&
            implementationGate.Message.Contains("implementationNeutral", StringComparison.Ordinal) &&
            implementationBlocked.Assets.Single().Gates
                .Single(x => x.Id == "source").Message.Contains("Blocked", StringComparison.OrdinalIgnoreCase),
            "implementation-density authority must block at reference before source");
        File.WriteAllBytes(fidelityPath, fidelityBytes);
        File.WriteAllBytes(fidelityPath, fidelityBytes);
        Require(
            ReferenceGate(engine.Scan(false, false)).Passed,
            "restored byte-identical v3 package must pass reference again");
    }
    finally
    {
        DeleteTree(v3Root);
    }

    string v2Root = PrepareReferenceFixture(
        sourceRoot,
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        """);
    try
    {
        string referenceRoot = ReferenceRoot(v2Root);
        Directory.CreateDirectory(Path.Combine(referenceRoot, "Parts"));
        WritePngHeader(Path.Combine(referenceRoot, "composition.png"), 1200, 800);
        WritePngHeader(Path.Combine(referenceRoot, "Parts", "Body.png"), 800, 800);
        StoreEngine engine = new(v2Root);
        engine.SealReference("fixture.avatar");
        Require(
            ReferenceGate(engine.Scan(false, false)).Passed,
            "unchanged v2 asset must continue to pass reference without target migration");
    }
    finally
    {
        DeleteTree(v2Root);
    }
}


static void RunV3DensityBoundaryTests(string sourceRoot)
{
    const string densityFixtureId = "fixture.equipment.staff";
    (string root, string assetRoot) = PrepareDensityEquipmentFixture(sourceRoot);
    try
    {
        string manifestPath = Path.Combine(assetRoot, "asset.yaml");
        File.WriteAllText(
            manifestPath,
            File.ReadAllText(manifestPath).Replace(
                "form: staff",
                "form: staff\ndensity: reference-driven",
                StringComparison.Ordinal));

        string densityRoot = Path.Combine(assetRoot, "Density");
        Directory.CreateDirectory(densityRoot);
        File.WriteAllText(
            Path.Combine(densityRoot, "input.yaml"),
            """
            version: 2
            assetClass: equipment
            targetWorldLongAxis: 1.5
            parts:
              Staff:
                partSheet: Parts/Staff.png
                objectLongAxisPixels: 600
                apparentVoxelCount: 18
                features:
                  - id: staff-band
                    importance: secondary
            """);

        void WriteCandidate(int vpu, bool featurePass, bool cadencePreserved)
        {
            string candidateRoot = Path.Combine(
                densityRoot, "Candidates", $"vpu-{vpu:D3}");
            string sourcePath = Path.Combine(candidateRoot, "Source", "candidate.voxel.yaml");
            string previewPath = Path.Combine(candidateRoot, "Proof", "hero.png");
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            File.WriteAllText(sourcePath, $"name: candidate-{vpu}\nvoxelsPerUnit: {vpu}\n");
            WritePngHeader(previewPath, 512, 512);

            var audit = new ReferenceDensityCandidateAudit
            {
                Version = 1,
                Vpu = vpu,
                Features = new Dictionary<string, bool>(StringComparer.Ordinal)
                {
                    ["staff-band"] = featurePass
                },
                FeatureVoxelSpans = new Dictionary<string, int>(StringComparer.Ordinal)
                {
                    ["staff-band"] = 1
                },
                CadencePreserved = cadencePreserved,
                NormalizedWorldSize = new[] { 0.25, 0.25, 1.5 },
                IntegerBounds = new[] { 4, 4, (int)Math.Round(1.5 * vpu) },
                OccupiedVoxels = 400 + vpu,
                AxisCompliant = true,
                GreedyMergedQuads = 40 + vpu,
                Vertices = (40 + vpu) * 4,
                Triangles = (40 + vpu) * 2,
                SourceSha256 = HeroVoxelProvider.HashFile(sourcePath),
                PreviewSha256 = HeroVoxelProvider.HashFile(previewPath)
            };
            File.WriteAllText(
                Path.Combine(candidateRoot, "audit.json"),
                JsonSerializer.Serialize(audit, new JsonSerializerOptions { WriteIndented = true }));
        }

        WriteCandidate(8, featurePass: true, cadencePreserved: false);
        WriteCandidate(10, featurePass: true, cadencePreserved: true);        string referenceRoot = Path.Combine(assetRoot, "Reference");
        string targetPath = Path.Combine(referenceRoot, "target.png");
        string importedSource = Path.Combine(referenceRoot, "Source", "target-source.png");
        Directory.CreateDirectory(Path.GetDirectoryName(importedSource)!);
        File.Copy(Path.Combine(referenceRoot, "composition.png"), targetPath, true);
        File.Copy(targetPath, importedSource, true);

        var provenance = new ReferenceTargetProvenance
        {
            Version = 1,
            Origin = "imported",
            TargetSha256 = HeroVoxelProvider.HashFile(targetPath),
            Source = new ReferenceTargetSourceProvenance
            {
                Path = "Source/target-source.png",
                Sha256 = HeroVoxelProvider.HashFile(importedSource),
                Relation = "exact"
            }
        };
        File.WriteAllText(
            Path.Combine(referenceRoot, "target.provenance.json"),
            JsonSerializer.Serialize(provenance, new JsonSerializerOptions { WriteIndented = true }));

        static Dictionary<string, string> PassingCriteria() => new(StringComparer.Ordinal)
        {
            ["silhouette"] = "pass",
            ["proportions"] = "pass",
            ["constructionRichness"] = "pass",
            ["materialColorHierarchy"] = "pass",
            ["meaningfulDetailDensity"] = "pass",
            ["visualEvidenceDominance"] = "pass",
            ["implementationNeutral"] = "pass"
        };

        var fidelity = new ReferenceFidelityAudit
        {
            Version = 1,
            Target = new ReferenceFidelityTargetEvidence
            {
                Path = "target.png",
                Sha256 = HeroVoxelProvider.HashFile(targetPath)
            },
            Sheets = new Dictionary<string, ReferenceFidelitySheetEvidence>(StringComparer.Ordinal)
            {
                ["composition.png"] = new()
                {
                    Sha256 = HeroVoxelProvider.HashFile(
                        Path.Combine(referenceRoot, "composition.png")),
                    Criteria = PassingCriteria()
                },
                ["Parts/Staff.png"] = new()
                {
                    Sha256 = HeroVoxelProvider.HashFile(
                        Path.Combine(referenceRoot, "Parts", "Staff.png")),
                    Criteria = PassingCriteria()
                }
            }
        };
        File.WriteAllText(
            Path.Combine(referenceRoot, "fidelity.audit.json"),
            JsonSerializer.Serialize(fidelity, new JsonSerializerOptions { WriteIndented = true }));
        File.WriteAllText(
            Path.Combine(referenceRoot, "reference.yaml"),
            """
            version: 3
            target:
              image: target.png
              provenance: target.provenance.json
            composition: composition.png
            requiredParts:
              - Staff
            partSheets:
              Staff: Parts/Staff.png
            fidelity: fidelity.audit.json
            """);

        StoreEngine engine = new(root);
        engine.SealReference(densityFixtureId);
        AssetRecord staff = engine.DiscoverAssets().Single();
        Require(
            engine.ValidateReferenceContractStructure(staff).Version == 3,
            "temporary Staff reference must be valid v3");
        engine.ValidateTargetProvenance(staff);
        engine.ValidateFidelityAudit(staff);

        ReferenceDensityEvidence evidence =
            engine.SelectDensity(densityFixtureId, write: false);
        Require(
            evidence.SelectedVpu == 10 &&
            evidence.Candidates.Single(x => x.Vpu == 10).FeatureVoxelSpans["staff-band"] == 1,
            $"valid v3 reference must feed per-unit low-VPU density selection through reference-only seal; got {evidence.SelectedVpu}");
    }
    finally
    {
        DeleteTree(root);
    }
}

static void RunTargetDrivenSampleTests(string sourceRoot)
{
    string sampleRoot = Path.Combine(
        sourceRoot, "Samples", "TargetDrivenReferenceProof");
    string referenceRoot = Path.Combine(sampleRoot, "Reference");
    Require(
        Directory.Exists(referenceRoot) &&
        !File.Exists(Path.Combine(sampleRoot, "asset.yaml")),
        "target-driven reference proof must remain non-published and have no asset.yaml");

    var asset = new AssetRecord(
        new AssetManifest
        {
            Version = 2,
            Id = "sample.target-driven-reference-proof",
            Kind = "equipment",
            UnitRef = "Sample.TargetDrivenReferenceProof"
        },
        Path.Combine(sampleRoot, "__nonpublished__.asset.yaml"),
        sampleRoot,
        Path.Combine(sampleRoot, "Source", "TargetDrivenBowProof"),
        Path.Combine(sampleRoot, "Source", "TargetDrivenBowProof.palette.yaml"),
        referenceRoot,
        Path.Combine(sampleRoot, "Proof"));

    StoreEngine engine = new(sourceRoot);
    ReferenceContract contract = engine.ValidateReferenceContractStructure(asset);
    Require(
        contract.Version == 3 &&
        contract.RequiredParts.SequenceEqual(new[] { "Bow" }, StringComparer.Ordinal),
        "target-driven sample must expose one valid v3 Bow construction package");

    string firstProvenance =
        JsonSerializer.Serialize(engine.ValidateTargetProvenance(asset));
    string secondProvenance =
        JsonSerializer.Serialize(engine.ValidateTargetProvenance(asset));
    string firstFidelity =
        JsonSerializer.Serialize(engine.ValidateFidelityAudit(asset));
    string secondFidelity =
        JsonSerializer.Serialize(engine.ValidateFidelityAudit(asset));
    Require(
        firstProvenance == secondProvenance &&
        firstFidelity == secondFidelity,
        "target-driven sample v3 validation must be deterministic");

    Hero.Voxel.Tool.VoxelSpecDocument sampleSpec =
        Hero.Voxel.Tool.VoxelSpecYaml.LoadSpec(asset.SourceBase + ".voxel.yaml");
    Hero.Voxel.Tool.PaletteSpec samplePalette =
        Hero.Voxel.Tool.VoxelSpecYaml.LoadPalette(asset.PalettePath);
    Hero.Voxel.Tool.VoxelCompilation sampleCompilation =
        Hero.Voxel.Tool.VoxelSpecCompiler.Compile(sampleSpec, samplePalette);
    Require(
        sampleCompilation.Parts.Select(x => x.Name).SequenceEqual(new[] { "Bow" }, StringComparer.Ordinal),
        "target-driven sample model must compile to exactly the accepted Bow rigid part");

    string generatedVox = Path.Combine(
        sampleRoot, "Generated", "TargetDrivenBowProof.generated.vox");
    Require(
        File.Exists(generatedVox) &&
        File.ReadAllBytes(generatedVox).SequenceEqual(
            Hero.Voxel.Tool.MagicaVoxelWriter.WriteToBytes(sampleCompilation)),
        "target-driven sample generated VOX must match deterministic compilation");

    string[] partViews = { "hero.png", "front.png", "back.png", "left.png", "right.png", "top.png", "bottom.png" };
    string[] assembledViews =
    {
        "hero.png", "front.png", "back.png", "left.png", "right.png", "top.png", "bottom.png",
        "front-left-3q.png", "front-right-3q.png", "back-left-3q.png", "back-right-3q.png"
    };
    Require(
        partViews.All(x => File.Exists(Path.Combine(sampleRoot, "Proof", "Part", "parts", "000", x))) &&
        assembledViews.All(x => File.Exists(Path.Combine(sampleRoot, "Proof", "Standalone", x))) &&
        File.Exists(Path.Combine(sampleRoot, "Proof", "model-fidelity.audit.json")),
        "target-driven sample must include individual-part, assembled, and target-to-model proof evidence");
}


static void RunAxeNegativeFidelityFixture(string sourceRoot)
{
    const string contract =
        """
        version: 3
        target:
          image: target.png
          provenance: target.provenance.json
        composition: composition.png
        requiredParts:
          - Axe
        partSheets:
          Axe: Parts/Axe.png
        fidelity: fidelity.audit.json
        """;

    string root = PrepareReferenceFixture(sourceRoot, contract);
    try
    {
        string fixtureRoot = Path.Combine(
            root,
            "Assets",
        "Avatars",
            "FixtureAvatar",
            "Reference");
        Directory.CreateDirectory(Path.Combine(fixtureRoot, "Parts"));
        Directory.CreateDirectory(Path.Combine(fixtureRoot, "Source"));
        WritePngHeader(Path.Combine(fixtureRoot, "target.png"), 1200, 900);
        WritePngHeader(Path.Combine(fixtureRoot, "composition.png"), 1200, 800);
        WritePngHeader(Path.Combine(fixtureRoot, "Parts", "Axe.png"), 800, 800);
        string source = Path.Combine(fixtureRoot, "Source", "target-source.png");
        File.Copy(Path.Combine(fixtureRoot, "target.png"), source, true);

        var provenance = new ReferenceTargetProvenance
        {
            Version = 1,
            Origin = "imported",
            TargetSha256 = HeroVoxelProvider.HashFile(Path.Combine(fixtureRoot, "target.png")),
            Source = new ReferenceTargetSourceProvenance
            {
                Path = "Source/target-source.png",
                Sha256 = HeroVoxelProvider.HashFile(source),
                Relation = "exact"
            }
        };
        File.WriteAllText(
            Path.Combine(fixtureRoot, "target.provenance.json"),
            JsonSerializer.Serialize(
                provenance,
                new JsonSerializerOptions { WriteIndented = true }));

        Dictionary<string, string> axeLikeCriteria = new(StringComparer.Ordinal)
        {
            ["silhouette"] = "pass",
            ["proportions"] = "pass",
            ["constructionRichness"] = "fail",
            ["materialColorHierarchy"] = "pass",
            ["meaningfulDetailDensity"] = "fail",
            ["visualEvidenceDominance"] = "fail",
            ["implementationNeutral"] = "fail"
        };
        var fidelity = new ReferenceFidelityAudit
        {
            Version = 1,
            Target = new ReferenceFidelityTargetEvidence
            {
                Path = "target.png",
                Sha256 = HeroVoxelProvider.HashFile(Path.Combine(fixtureRoot, "target.png"))
            },
            Sheets = new Dictionary<string, ReferenceFidelitySheetEvidence>(StringComparer.Ordinal)
            {
                ["composition.png"] = new()
                {
                    Sha256 = HeroVoxelProvider.HashFile(Path.Combine(fixtureRoot, "composition.png")),
                    Criteria = new Dictionary<string, string>(axeLikeCriteria, StringComparer.Ordinal)
                },
                ["Parts/Axe.png"] = new()
                {
                    Sha256 = HeroVoxelProvider.HashFile(Path.Combine(fixtureRoot, "Parts", "Axe.png")),
                    Criteria = new Dictionary<string, string>(axeLikeCriteria, StringComparer.Ordinal)
                }
            }
        };
        File.WriteAllText(
            Path.Combine(fixtureRoot, "fidelity.audit.json"),
            JsonSerializer.Serialize(
                fidelity,
                new JsonSerializerOptions { WriteIndented = true }));

        StoreEngine engine = new(root);
        AssetRecord asset = engine.DiscoverAssets().Single();
        ExpectFailure(
            () => engine.ValidateFidelityAudit(asset),
            "constructionRichness");
    }
    finally
    {
        DeleteTree(root);
    }
}

static void RunReferenceAuthorityContractTests(string sourceRoot)
{
    string styleRoot = PrepareReferenceFixture(
        sourceRoot,
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        """);
    try
    {
        string styleDir = Path.Combine(styleRoot, "Styles", "low-resolution-voxel.v1");
        ReferenceStyleAuthority style =
            new StoreEngine(styleRoot).ResolveReferenceStyle("avatar");
        Require(style.Id == "low-resolution-voxel.v1", "style id must resolve");
        Require(style.Kind == "avatar", "style kind must resolve");
        Require(style.CommonReferencePaths.Count == 0, "fixture has no common style images");
        Require(style.KindReferencePaths.Count == 3, "avatar must resolve exactly three PNG references");
        ReferenceStyleAuthority equipmentStyle =
            new StoreEngine(styleRoot).ResolveReferenceStyle("equipment");
        Require(
            equipmentStyle.KindReferencePaths.Count == 1 &&
            Path.GetFileName(equipmentStyle.KindReferencePaths[0]) == "equipment-bow-source.png",
            "equipment must resolve only the independent bow source reference");
        ExpectFailure(
            () => new StoreEngine(styleRoot).ResolveReferenceStyle("unknown"),
            "does not support asset kind");

        string styleManifest = Path.Combine(styleDir, "style.yaml");
        string originalStyle = File.ReadAllText(styleManifest);
        File.WriteAllText(
            styleManifest,
            originalStyle.Replace(
                "  avatar:",
                "  avatar_missing:",
                StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(styleRoot).ResolveReferenceStyle("avatar"),
            "scope 'avatar' must be non-empty");
        File.WriteAllText(styleManifest, originalStyle);

        File.WriteAllText(
            styleManifest,
            originalStyle.Replace(
                "  avatar:",
                "  common:\n    - reference-01.png\n  avatar:",
                StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(styleRoot).ResolveReferenceStyle("avatar"),
            "unique across scopes");
        File.WriteAllText(styleManifest, originalStyle);

        File.WriteAllText(
            styleManifest,
            originalStyle.Replace(
                "    - reference-03.png",
                "    - reference-02.png",
                StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(styleRoot).ResolveReferenceStyle("avatar"),
            "image paths must be unique");
        File.WriteAllText(styleManifest, originalStyle);

        File.WriteAllText(
            styleManifest,
            originalStyle.Replace(
                "reference-03.png",
                "reference-03.svg",
                StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(styleRoot).ResolveReferenceStyle("avatar"),
            "must be PNG");
        File.WriteAllText(styleManifest, originalStyle);

        File.Delete(Path.Combine(styleDir, "reference-03.png"));
        ExpectFailure(
            () => new StoreEngine(styleRoot).ResolveReferenceStyle("avatar"),
            "Required PNG is missing");
        WritePngHeader(Path.Combine(styleDir, "reference-03.png"), 1200, 900);

        string policyPath = Path.Combine(styleRoot, "Store", "policy.yaml");
        string originalPolicy = File.ReadAllText(policyPath);
        File.WriteAllText(
            policyPath,
            originalPolicy.Replace(
                "defaultStyle: low-resolution-voxel.v1",
                "defaultStyle:",
                StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(styleRoot).ResolveReferenceStyle("avatar"),
            "policy is incomplete");
        File.WriteAllText(policyPath, originalPolicy);
    }
    finally
    {
        DeleteTree(styleRoot);
    }

    void ExpectReferenceFailure(string yaml, string expected, Action<string>? arrange = null)
    {
        string root = PrepareReferenceFixture(sourceRoot, yaml);
        try
        {
            string referenceRoot = Path.Combine(
                root,
                "Assets",
        "Avatars",
                "FixtureAvatar",
                "Reference");
            Directory.CreateDirectory(Path.Combine(referenceRoot, "Parts"));
            WritePngHeader(Path.Combine(referenceRoot, "composition.png"), 1200, 800);
            WritePngHeader(Path.Combine(referenceRoot, "Parts", "Body.png"), 800, 800);
            WritePngHeader(Path.Combine(referenceRoot, "Parts", "Head.png"), 800, 800);
            File.WriteAllText(
                Path.Combine(referenceRoot, "audit.generated.json"),
                """{"version":1,"kind":"reference","sha256":{}}""");
            arrange?.Invoke(referenceRoot);
            StoreSnapshot snapshot = new StoreEngine(root).Scan(false, false);
            GateResult gate = ReferenceGate(snapshot);
            Require(!gate.Passed, "fixture reference gate must fail");
            Require(
                gate.Message.Contains(expected, StringComparison.OrdinalIgnoreCase),
                $"reference failure must contain '{expected}', got '{gate.Message}'");
        }
        finally
        {
            DeleteTree(root);
        }
    }

    ExpectReferenceFailure(
        """
        version: 1
        composition: composition.svg
        requiredParts:
          - Body
        """,
        "version 2");

    ExpectReferenceFailure(
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
          - Head
        partSheets:
          Body: Parts/Body.png
        """,
        "match RequiredParts exactly");

    ExpectReferenceFailure(
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
          Head: Parts/Head.png
        """,
        "match RequiredParts exactly");

    ExpectReferenceFailure(
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
          - Head
        partSheets:
          Body: Parts/Body.png
          Head: Parts/Body.png
        """,
        "unique files");

    ExpectReferenceFailure(
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: composition.png
        """,
        "unique files");

    ExpectReferenceFailure(
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: ../escape.png
        """,
        "escapes its authority root");

    ExpectReferenceFailure(
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.svg
        """,
        "must be a PNG");

    ExpectReferenceFailure(
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        """,
        "Invalid PNG signature",
        referenceRoot =>
            WritePngHeader(
                Path.Combine(referenceRoot, "Parts", "Body.png"),
                800,
                800,
                validSignature: false));

    ExpectReferenceFailure(
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        """,
        "dimensions are invalid",
        referenceRoot =>
            WritePngHeader(
                Path.Combine(referenceRoot, "composition.png"),
                0,
                800));

    ExpectReferenceFailure(
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        """,
        "exact 3:2",
        referenceRoot =>
            WritePngHeader(
                Path.Combine(referenceRoot, "composition.png"),
                1000,
                800));

    string validRoot = PrepareReferenceFixture(
        sourceRoot,
        """
        version: 2
        composition: composition.png
        requiredParts:
          - Body
        partSheets:
          Body: Parts/Body.png
        """);
    try
    {
        string referenceRoot = Path.Combine(
            validRoot,
            "Assets",
        "Avatars",
            "FixtureAvatar",
            "Reference");
        Directory.CreateDirectory(Path.Combine(referenceRoot, "Parts"));
        WritePngHeader(Path.Combine(referenceRoot, "composition.png"), 1200, 800);
        WritePngHeader(Path.Combine(referenceRoot, "Parts", "Body.png"), 700, 900);
        WritePngHeader(Path.Combine(referenceRoot, "Parts", "Unused.png"), 300, 300);

        StoreEngine fixtureEngine = new(validRoot);
        StoreSnapshot unsealed = fixtureEngine.Scan(false, false);
        Require(
            !ReferenceGate(unsealed).Passed &&
            ReferenceGate(unsealed).Message.Contains(
                "contract/audit evidence is missing",
                StringComparison.OrdinalIgnoreCase),
            "PNG files without explicit visual seal must not satisfy reference acceptance");

        ExpectFailure(
            () => fixtureEngine.Seal("fixture.avatar"),
            "Cannot seal missing proof input");
        StoreSnapshot sealedSnapshot = fixtureEngine.Scan(false, false);
        Require(ReferenceGate(sealedSnapshot).Passed, "sealed valid v2 reference must pass reference gate");
        string auditPath = Path.Combine(referenceRoot, "audit.generated.json");
        using JsonDocument audit = JsonDocument.Parse(File.ReadAllText(auditPath));
        JsonElement hashes = audit.RootElement.GetProperty("Sha256");
        Require(
            !hashes.TryGetProperty("Parts/Unused.png", out _),
            "unreferenced PNG must not become reference authority");
        Require(
            audit.RootElement.GetProperty("ReferenceStyle").GetString() ==
            "low-resolution-voxel.v1",
            "reference audit must record style authority");
        Require(
            audit.RootElement.GetProperty("StyleKind").GetString() == "avatar",
            "reference audit must record the applicable style kind");
        Require(
            audit.RootElement.GetProperty("StyleAuthoritySha256").GetString()?.Length == 64,
            "reference audit must record a semantic applicable-style digest");
        Require(
            audit.RootElement.GetProperty("StyleSha256").EnumerateObject().Count() == 3,
            "avatar reference audit must hash only three avatar-scoped style PNGs");
        Require(
            audit.RootElement.GetProperty("StyleSha256").EnumerateObject()
                .All(x => x.Name.Contains("reference-0", StringComparison.Ordinal) &&
                          !x.Name.Contains("equipment-", StringComparison.Ordinal)),
            "avatar reference audit must not include equipment-scoped style PNGs");

        string equipmentStylePng = Path.Combine(
            validRoot,
            "Styles",
            "low-resolution-voxel.v1",
            "equipment-bow-source.png");
        byte[] originalEquipmentStyle = File.ReadAllBytes(equipmentStylePng);
        try
        {
            byte[] changed = originalEquipmentStyle.ToArray();
            changed[23] ^= 1;
            File.WriteAllBytes(equipmentStylePng, changed);
            Require(
                ReferenceGate(fixtureEngine.Scan(false, false)).Passed,
                "equipment-only style image mutation must not stale avatar reference evidence");
        }
        finally
        {
            File.WriteAllBytes(equipmentStylePng, originalEquipmentStyle);
        }

        string styleManifestForScopedTraits = Path.Combine(
            validRoot,
            "Styles",
            "low-resolution-voxel.v1",
            "style.yaml");
        string originalScopedTraitManifest = File.ReadAllText(styleManifestForScopedTraits);
        try
        {
            File.WriteAllText(
                styleManifestForScopedTraits,
                originalScopedTraitManifest.Replace(
                    "purposeful secondary construction detail",
                    "purposeful secondary construction detail adjusted",
                    StringComparison.Ordinal));
            Require(
                ReferenceGate(fixtureEngine.Scan(false, false)).Passed,
                "equipment-only style trait mutation must not stale avatar reference evidence");
        }
        finally
        {
            File.WriteAllText(styleManifestForScopedTraits, originalScopedTraitManifest);
        }

        string stylePng = Path.Combine(
            validRoot,
            "Styles",
            "low-resolution-voxel.v1",
            "reference-01.png");
        byte[] original = File.ReadAllBytes(stylePng);
        try
        {
            byte[] changed = original.ToArray();
            changed[23] ^= 1;
            File.WriteAllBytes(stylePng, changed);
            StoreSnapshot stale = fixtureEngine.Scan(false, false);
            Require(
                !ReferenceGate(stale).Passed &&
                ReferenceGate(stale).Message.Contains(
                    "style authority digest is stale",
                    StringComparison.OrdinalIgnoreCase),
                "style image mutation must stale accepted reference evidence");
        }
        finally
        {
            File.WriteAllBytes(stylePng, original);
        }

        ExpectFailure(
            () => fixtureEngine.Seal("fixture.avatar"),
            "Cannot seal missing proof input");
        Require(
            ReferenceGate(fixtureEngine.Scan(false, false)).Passed,
            "resealing restored style evidence must recover reference gate");

        string styleManifest = Path.Combine(
            validRoot,
            "Styles",
            "low-resolution-voxel.v1",
            "style.yaml");
        string originalStyleManifest = File.ReadAllText(styleManifest);
        try
        {
            File.WriteAllText(
                styleManifest,
                originalStyleManifest.Replace(
                    "chibi proportion language",
                    "chibi proportion language adjusted",
                    StringComparison.Ordinal));
            StoreSnapshot stale = fixtureEngine.Scan(false, false);
            Require(
                !ReferenceGate(stale).Passed &&
                ReferenceGate(stale).Message.Contains(
                    "style authority digest is stale",
                    StringComparison.OrdinalIgnoreCase),
                "style manifest mutation must stale accepted reference evidence");
        }
        finally
        {
            File.WriteAllText(styleManifest, originalStyleManifest);
        }

        ExpectFailure(
            () => fixtureEngine.Seal("fixture.avatar"),
            "Cannot seal missing proof input");
        Require(
            ReferenceGate(fixtureEngine.Scan(false, false)).Passed,
            "resealing restored style manifest evidence must recover reference gate");

        string lockPath = Path.Combine(validRoot, "toolchain", "hero-voxel.lock.yaml");
        string originalLock = File.ReadAllText(lockPath);
        HeroVoxelLock parsedLock = ContractYaml.Load<HeroVoxelLock>(lockPath);
        try
        {
            File.WriteAllText(
                lockPath,
                originalLock.Replace(
                    parsedLock.WorkflowSha256,
                    new string('0', 64),
                    StringComparison.Ordinal));
            StoreSnapshot stale = fixtureEngine.Scan(false, false);
            Require(
                !ReferenceGate(stale).Passed &&
                ReferenceGate(stale).Message.Contains(
                    "workflow authority is stale",
                    StringComparison.OrdinalIgnoreCase),
                "Hero.Voxel workflow fingerprint mutation must stale reference evidence");
        }
        finally
        {
            File.WriteAllText(lockPath, originalLock);
        }

        ExpectFailure(
            () => fixtureEngine.Seal("fixture.avatar"),
            "Cannot seal missing proof input");
        Require(
            ReferenceGate(fixtureEngine.Scan(false, false)).Passed,
            "resealing restored workflow evidence must recover reference gate");
    }
    finally
    {
        DeleteTree(validRoot);
    }
}

static void RunRetirementContractTests()
{
    string root = TempRoot();
    try
    {
        Directory.CreateDirectory(Path.Combine(root, "Store"));
        File.WriteAllText(
            Path.Combine(root, "Store", "retired-assets.yaml"),
            """
            version: 1
            assets:
              - id: retired.asset
                unitRef: retired/unit
            """);

        string assetRoot = Path.Combine(root, "Assets", "Fixture");
        Directory.CreateDirectory(assetRoot);
        string manifestPath = Path.Combine(assetRoot, "asset.yaml");
        void WriteManifest(string id, string unitRef) =>
            File.WriteAllText(
                manifestPath,
                $"""
                version: 3
                id: {id}
                kind: equipment
                unitRef: {unitRef}
                form: fixture
                attachment:
                  capability: Hand
                  side: Any
                  anchor: Mount
                source:
                  base: ./Source/Fixture
                  palette: ./Source/Palette.yaml
                style:
                  family: low-resolution-voxel.v1
                """);

        WriteManifest("retired.asset", "fresh/unit");
        ExpectFailure(
            () => new StoreEngine(root).DiscoverAssets(),
            "reuses a retired published identity");

        WriteManifest("fresh.asset", "retired/unit");
        ExpectFailure(
            () => new StoreEngine(root).DiscoverAssets(),
            "reuses a retired published identity");

        WriteManifest("fresh.asset", "fresh/unit");
        Require(
            new StoreEngine(root).DiscoverAssets().Count == 1,
            "fresh replacement identity must remain valid");

        string validV2Manifest = File.ReadAllText(manifestPath);
        File.WriteAllText(
            manifestPath,
            validV2Manifest.Replace(
                "form: fixture\n",
                "form: fixture\nmount: legacy\n",
                StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(root).DiscoverAssets(),
            "mount");

        File.WriteAllText(
            manifestPath,
            validV2Manifest.Replace("form: fixture\n", "", StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(root).DiscoverAssets(),
            "reusable physical form");
        File.WriteAllText(manifestPath, validV2Manifest);

        File.WriteAllText(
            manifestPath,
            validV2Manifest.Replace(
                "form: fixture",
                "form: fixture\ndensity: arbitrary",
                StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(root).DiscoverAssets(),
            "density must be 'reference-driven'");
        File.WriteAllText(manifestPath, validV2Manifest);

        RunGit(root, "init");
        RunGit(root, "config", "user.email", "voxel-store-tests@example.invalid");
        RunGit(root, "config", "user.name", "Voxel Store Tests");
        RunGit(root, "add", ".");
        RunGit(root, "commit", "-m", "fixture");

        WriteManifest("changed.asset", "fresh/unit");
        ExpectFailure(
            () => new StoreEngine(root).DiscoverAssets(),
            "Published semantic identity changed in-place");

        RunGit(root, "checkout", "--", "Assets/Fixture/asset.yaml");
        string publishedManifest = File.ReadAllText(manifestPath);
        File.WriteAllText(
            manifestPath,
            publishedManifest.Replace("form: fixture", "form: changed-form", StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(root).DiscoverAssets(),
            "form changed in-place");

        RunGit(root, "checkout", "--", "Assets/Fixture/asset.yaml");
        File.Delete(manifestPath);
        Require(
            new StoreEngine(root).DiscoverAssets().Count == 0,
            "intentional deletion of a retired/removed manifest must not be treated as in-place mutation");
    }
    finally
    {
        DeleteTree(root);
    }
}

static void RunPreviewPlanTests(string repositoryRoot)
{
    string catalogPath = Path.Combine(repositoryRoot, "Catalog", "catalog.generated.json");
    string healthPath = Path.Combine(repositoryRoot, "Catalog", "health.generated.json");
    byte[]? catalogBefore = File.Exists(catalogPath) ? File.ReadAllBytes(catalogPath) : null;
    byte[]? healthBefore = File.Exists(healthPath) ? File.ReadAllBytes(healthPath) : null;

    var engine = new StoreEngine(repositoryRoot);
    IReadOnlyList<AssetRecord> records = engine.DiscoverAssets();
    StoreSnapshot snapshot = engine.Scan(writeDerived: false, writeCompatibility: false);
    PreviewPlan first = engine.BuildPreviewPlan();
    PreviewPlan second = engine.BuildPreviewPlan();

    string[] expectedIds = snapshot.Assets
        .Where(x => x.Published)
        .OrderBy(x => x.Kind, StringComparer.Ordinal)
        .ThenBy(x => x.Id, StringComparer.Ordinal)
        .Select(x => x.Id)
        .ToArray();
    Require(
        first.Units.Select(x => x.Id).SequenceEqual(expectedIds, StringComparer.Ordinal),
        "preview plan must contain every and only currently published asset in deterministic kind/id order");
    Require(
        first.Units.Select(x => x.Id).SequenceEqual(second.Units.Select(x => x.Id), StringComparer.Ordinal),
        "preview plan ordering must be deterministic");
    Require(
        snapshot.Assets.Where(x => !x.Published).All(x => first.Units.All(u => u.Id != x.Id)),
        "unpublished assets must not appear in preview plan");

    string syntheticRoot = TempRoot();
    try
    {
        AssetRecord Synthetic(string id, string kind, string unitRef) =>
            new(
                new AssetManifest
                {
                    Version = 2,
                    Id = id,
                    Kind = kind,
                    UnitRef = unitRef,
                    Source = new AssetSource
                    {
                        Base = "./Source/" + id,
                        Palette = "./Source/" + id + ".palette.yaml"
                    },
                    Style = new AssetStyle { Family = "fixture.style" }
                },
                Path.Combine(syntheticRoot, "Assets", id, "asset.yaml"),
                Path.Combine(syntheticRoot, "Assets", id),
                Path.Combine(syntheticRoot, "Assets", id, "Source", id),
                Path.Combine(syntheticRoot, "Assets", id, "Source", id + ".palette.yaml"),
                Path.Combine(syntheticRoot, "Assets", id, "Reference"),
                Path.Combine(syntheticRoot, "Assets", id, "Proof"));

        AssetRecord avatar = Synthetic("b.avatar", "avatar", "fixture/avatar/b");
        AssetRecord equipment = Synthetic("a.equipment", "equipment", "fixture/equipment/a");
        AssetRecord unpublished = Synthetic("z.unpublished", "equipment", "fixture/equipment/z");
        var syntheticSnapshot = new StoreSnapshot
        {
            Assets = new List<AssetHealth>
            {
                new() { Id = avatar.Manifest.Id, Kind = avatar.Manifest.Kind, UnitRef = avatar.Manifest.UnitRef, Published = true },
                new() { Id = equipment.Manifest.Id, Kind = equipment.Manifest.Kind, UnitRef = equipment.Manifest.UnitRef, Published = true },
                new() { Id = unpublished.Manifest.Id, Kind = unpublished.Manifest.Kind, UnitRef = unpublished.Manifest.UnitRef, Published = false }
            }
        };
        PreviewPlan synthetic = StoreEngine.CreatePreviewPlan(
            syntheticRoot,
            new[] { unpublished, avatar, equipment },
            syntheticSnapshot);
        Require(
            synthetic.Units.Select(x => x.Id).SequenceEqual(new[] { "b.avatar", "a.equipment" }, StringComparer.Ordinal),
            "synthetic preview plan must include both published kinds, exclude unpublished assets, and sort by kind then id");
    }
    finally
    {
        DeleteTree(syntheticRoot);
    }

    Dictionary<string, AssetRecord> byId = records.ToDictionary(x => x.Manifest.Id, StringComparer.Ordinal);
    foreach (PreviewPlanUnit unit in first.Units)
    {
        AssetRecord record = byId[unit.Id];
        string source = Path.GetRelativePath(repositoryRoot, record.SourceBase).Replace('\\', '/');
        string palette = Path.GetRelativePath(repositoryRoot, record.PalettePath).Replace('\\', '/');
        Require(unit.Kind == record.Manifest.Kind, $"preview kind must come from manifest for {unit.Id}");
        Require(unit.UnitRef == record.Manifest.UnitRef, $"preview UnitRef must come from manifest for {unit.Id}");
        Require(unit.SourceBase == source, $"preview source path must come from authoritative asset record for {unit.Id}");
        Require(unit.Palette == palette, $"preview palette path must come from authoritative asset record for {unit.Id}");
        string generatedVox = Path.GetRelativePath(
            repositoryRoot,
            record.SourceBase + ".generated.vox").Replace('\\', '/');
        string generatedAnimationPath =
            record.SourceBase + ".animation.generated.yaml";
        string? generatedAnimation = File.Exists(generatedAnimationPath)
            ? Path.GetRelativePath(repositoryRoot, generatedAnimationPath).Replace('\\', '/')
            : null;
        Require(unit.GeneratedVox == generatedVox,
            $"preview generated VOX path must come from current publication artifact for {unit.Id}");
        Require(unit.GeneratedAnimation == generatedAnimation,
            $"preview generated animation path must match current publication artifact for {unit.Id}");
        Require(
            !Path.IsPathRooted(unit.SourceBase) &&
            !Path.IsPathRooted(unit.Palette) &&
            !Path.IsPathRooted(unit.GeneratedVox) &&
            (unit.GeneratedAnimation == null || !Path.IsPathRooted(unit.GeneratedAnimation)),
            $"preview paths must be repository-relative for {unit.Id}");
    }

    byte[]? catalogAfter = File.Exists(catalogPath) ? File.ReadAllBytes(catalogPath) : null;
    byte[]? healthAfter = File.Exists(healthPath) ? File.ReadAllBytes(healthPath) : null;
    Require(
        (catalogBefore == null && catalogAfter == null) ||
        (catalogBefore != null && catalogAfter != null && catalogBefore.SequenceEqual(catalogAfter)),
        "preview plan must not mutate generated catalog");
    Require(
        (healthBefore == null && healthAfter == null) ||
        (healthBefore != null && healthAfter != null && healthBefore.SequenceEqual(healthAfter)),
        "preview plan must not mutate generated health");
}
string repositoryRoot = StoreEngine.FindRoot(AppContext.BaseDirectory);
if (args.Contains("--preview-plan-only", StringComparer.Ordinal))
{
    RunPreviewPlanTests(repositoryRoot);
    Console.WriteLine("PASS: non-mutating published preview-plan contract");
    return;
}
if (args.Contains("--axe-negative-fidelity-only", StringComparer.Ordinal))
{
    RunAxeNegativeFidelityFixture(repositoryRoot);
    Console.WriteLine("PASS: Axe01-equivalent negative target-fidelity fixture rejected");
    return;
}
if (args.Contains("--target-sample-only", StringComparer.Ordinal))
{
    RunTargetDrivenSampleTests(repositoryRoot);
    Console.WriteLine("PASS: non-published target-driven sample v3 validation");
    return;
}
if (args.Contains("--implementation-neutral-only", StringComparer.Ordinal))
{
    RunReferenceV3StructureTests(repositoryRoot);
    RunReferenceV3PublicationTests(repositoryRoot);
    RunV3DensityBoundaryTests(repositoryRoot);
    Console.WriteLine("PASS: implementation-neutral reference and downstream density boundary tests");
    return;
}
if (args.Contains("--reference-v3-publication-only", StringComparer.Ordinal))
{
    RunReferenceV3PublicationTests(repositoryRoot);
    Console.WriteLine("PASS: v2/v3 reference publication gate tests");
    return;
}
if (args.Contains("--reference-seal-only", StringComparer.Ordinal))
{
    RunReferenceSealVersionTests(repositoryRoot);
    Console.WriteLine("PASS: v2/v3 reference sealing and deterministic audit tests");
    return;
}
if (args.Contains("--evidence-hash-only", StringComparer.Ordinal))
{
    RunEvidenceHashPortabilityTests();
    Console.WriteLine("PASS: checkout-portable evidence hashing tests");
    return;
}
if (args.Contains("--target-fidelity-only", StringComparer.Ordinal))
{
    RunTargetFidelityTests(repositoryRoot);
    Console.WriteLine("PASS: Product Target fidelity evidence tests");
    return;
}
if (args.Contains("--target-provenance-only", StringComparer.Ordinal))
{
    RunTargetProvenanceTests(repositoryRoot);
    Console.WriteLine("PASS: Product Target provenance tests");
    return;
}
if (args.Contains("--reference-v3-structure-only", StringComparer.Ordinal))
{
    RunReferenceV3StructureTests(repositoryRoot);
    Console.WriteLine("PASS: ReferenceContract v2/v3 structural authority tests");
    return;
}
if (args.Contains("--target-style-only", StringComparer.Ordinal))
{
    RunTargetStyleAuthorityTests(repositoryRoot);
    Console.WriteLine("PASS: target-style v2 compatibility and v3 target authority tests");
    return;
}
RunEvidenceHashPortabilityTests();
RunReferenceDensitySelectorTests();
RunReferenceDensityEvidenceTests(repositoryRoot);
if (args.Contains("--reference-density-only", StringComparer.Ordinal))
{
    Console.WriteLine("PASS: per-unit reference-cadence density selector and evidence tests");
    return;
}
if (args.Contains("--reference-authority-only", StringComparer.Ordinal))
{
    RunReferenceAuthorityContractTests(repositoryRoot);
    RunRetirementContractTests();
    Console.WriteLine("PASS: reference-style, retirement, and ReferenceContract v2 authority tests");
    return;
}

var engine = new StoreEngine(repositoryRoot);

static IReadOnlyList<Hero.Voxel.Tool.CompiledAnimationClip> RequiredSemanticClips(
    string partName,
    bool includeSideAttack = false,
    params (string Name, bool Loop)[] extraClips)
{
    Hero.Voxel.Tool.CompiledAnimationVectorChannel rotation = new(
        Hero.Voxel.Tool.CompiledAnimationInterpolation.Linear,
        new[]
        {
            new Hero.Voxel.Tool.CompiledAnimationVectorKey(
                0f,
                new Hero.Voxel.Tool.Float3(0f, 0f, 0f))
        });
    Hero.Voxel.Tool.CompiledAnimationClip Clip(string name, bool loop) =>
        new(
            name,
            1f,
            loop,
            new[]
            {
                new Hero.Voxel.Tool.CompiledAnimationTrack(
                    partName,
                    null,
                    rotation,
                    null)
            });
    var clips = new List<Hero.Voxel.Tool.CompiledAnimationClip>
    {
        Clip("Idle", true),
        Clip("WalkForward", true),
        Clip("Attack", false),
        Clip("Hit", false),
        Clip("Death", false)
    };
    if (includeSideAttack)
    {
        clips.Add(Clip("Attack.Left", false));
        clips.Add(Clip("Attack.Right", false));
    }
    foreach ((string name, bool loop) in extraClips)
        clips.Add(Clip(name, loop));
    return clips;
}

static IReadOnlyList<Hero.Voxel.Tool.CompiledAnimationClip>
    RequiredV1Fixture(
        string partName,
        string kind)
{
    var clips = RequiredSemanticClips(
            partName,
            includeSideAttack:
                string.Equals(
                    kind,
                    "avatar",
                    StringComparison.Ordinal))
        .ToList();
    foreach (string variantName in
             StoreEngine.RequiredV1Clips(kind))
    {
        string fallbackName =
            variantName[..^".V1".Length];
        Hero.Voxel.Tool.CompiledAnimationClip fallback =
            clips.Single(x =>
                string.Equals(
                    x.Name,
                    fallbackName,
                    StringComparison.Ordinal));
        clips.Add(
            fallback with
            {
                Name = variantName,
                Duration = fallback.Duration + 0.1f
            });
    }

    return clips;
}

var equipmentV1Compilation =
    new Hero.Voxel.Tool.VoxelCompilation(
        "EquipmentV1Fixture",
        8,
        Array.Empty<Hero.Voxel.Tool.PaletteEntry>(),
        Array.Empty<Hero.Voxel.Tool.CompiledPart>(),
        Array.Empty<Hero.Voxel.Tool.CompiledSocket>(),
        RequiredV1Fixture("Staff", "equipment"));
StoreEngine.ValidateRequiredV1Coverage(
    equipmentV1Compilation,
    "equipment");
Require(
    StoreEngine.GetMissingRequiredV1Clips(
        equipmentV1Compilation,
        "equipment").Count == 0,
    "complete equipment V1 fixture must have no missing required clips");
Require(
    StoreEngine.GetAliasedRequiredV1Clips(
        equipmentV1Compilation,
        "equipment").Count == 0,
    "distinct equipment V1 fixture must have no aliased required clips");

ExpectFailure(
    () => StoreEngine.ValidateRequiredV1Coverage(
        equipmentV1Compilation with
        {
            Animations = equipmentV1Compilation.Animations
                .Where(x => x.Name != "Death.V1")
                .ToArray()
        },
        "equipment"),
    "Death.V1");

Hero.Voxel.Tool.CompiledAnimationClip deathBase =
    equipmentV1Compilation.Animations.Single(
        x => x.Name == "Death");
Hero.Voxel.Tool.CompiledAnimationClip[] aliasedEquipment =
    equipmentV1Compilation.Animations
        .Where(x => x.Name != "Death.V1")
        .Append(deathBase with { Name = "Death.V1" })
        .ToArray();
ExpectFailure(
    () => StoreEngine.ValidateRequiredV1Coverage(
        equipmentV1Compilation with
        {
            Animations = aliasedEquipment
        },
        "equipment"),
    "different from their fallback");

var avatarV1Compilation =
    new Hero.Voxel.Tool.VoxelCompilation(
        "AvatarV1Fixture",
        8,
        Array.Empty<Hero.Voxel.Tool.PaletteEntry>(),
        Array.Empty<Hero.Voxel.Tool.CompiledPart>(),
        Array.Empty<Hero.Voxel.Tool.CompiledSocket>(),
        RequiredV1Fixture("Body", "avatar"));
StoreEngine.ValidateRequiredV1Coverage(
    avatarV1Compilation,
    "avatar");
Require(
    StoreEngine.RequiredV1Clips("avatar")
        .SequenceEqual(
            new[]
            {
                "Idle.V1",
                "WalkForward.V1",
                "Attack.Left.V1",
                "Attack.Right.V1",
                "Hit.V1",
                "Death.V1"
            }),
    "avatar publication floor must require both side-qualified Attack V1 takes");

if (args.Contains("--required-v1-only", StringComparer.Ordinal))
{
    Console.WriteLine(
        "PASS: required V1 publication coverage and alias rejection tests");
    return;
}

SemanticAnimationClipIdentity parsedVariant =
    SemanticAnimationClipName.Parse("Attack.Left.V2");
Require(
    parsedVariant.Motion == "Attack" &&
    parsedVariant.Side == "Left" &&
    parsedVariant.Variant == "V2",
    "animation clip parser must preserve semantic motion, physical side, and authored variant");
Require(
    SemanticAnimationClipName.Format("Death", variant: "V3") == "Death.V3",
    "animation clip formatter must produce canonical model-local variants");
Require(
    SemanticAnimationClipName.BuildCandidates("Attack", "Left", "V2")
        .SequenceEqual(
            new[]
            {
                "Attack.Left.V2",
                "Attack.V2",
                "Attack.Left",
                "Attack"
            },
            StringComparer.Ordinal),
    "variant-aware candidate order must prefer side+variant, generic variant, side, then base");
Require(
    SemanticAnimationClipName.BuildCandidates("WalkForward", variant: "V2")
        .SequenceEqual(
            new[] { "WalkForward.V2", "WalkForward" },
            StringComparer.Ordinal),
    "variant-only candidate order must omit absent side context");

foreach (string invalidName in new[]
         {
             "Attack.V0",
             "Attack.V01",
             "Attack.V-1",
             "Attack.Left.Heavy",
             "Attack.V2.Left",
             "Attack.Up.V1",
             "Attack.Left.V1.Extra",
             "Unknown.V1"
         })
{
    ExpectFailure(
        () => SemanticAnimationClipName.Parse(invalidName),
        "Semantic animation clip");
}

IReadOnlyList<SemanticMotionCoverage> projectedCoverage =
    SemanticAnimationClipName.BuildCoverage(
        new[]
        {
            "Attack",
            "Attack.Left",
            "Attack.V2",
            "Attack.Left.V2"
        });
SemanticMotionCoverage projectedAttack = projectedCoverage.Single();
Require(
    projectedAttack.Motion == "Attack" &&
    projectedAttack.Base &&
    projectedAttack.Sides.SequenceEqual(new[] { "Left" }, StringComparer.Ordinal) &&
    projectedAttack.Variants.SequenceEqual(new[] { "V2" }, StringComparer.Ordinal) &&
    projectedAttack.SideVariants["Left"].SequenceEqual(
        new[] { "V2" },
        StringComparer.Ordinal),
    "structured animation coverage must keep one semantic motion and distinguish side/variant qualifiers");

static AssetManifest EquipmentManifest(
    string capability = "Hand",
    string side = "Any",
    string anchor = "Mount") =>
    new()
    {
        Version = 3,
        Id = "fixture.equipment.contract",
        Kind = "equipment",
        UnitRef = "fixture/equipment/contract",
        Form = "sword",
        Attachment = new AssetAttachment
        {
            Capability = capability,
            Side = side,
            Anchor = anchor
        },
        Source = new AssetSource
        {
            Base = "./Source/Fixture",
            Palette = "./Source/Fixture.palette.yaml"
        },
        Style = new AssetStyle
        {
            Family = "low-resolution-voxel.v1"
        }
    };

StoreEngine.ValidateManifestShape(
    EquipmentManifest(),
    "fixture/asset.yaml");
ExpectFailure(
    () => StoreEngine.ValidateManifestShape(
        EquipmentManifest(capability: "SwordSlot"),
        "fixture/asset.yaml"),
    "unsupported");
ExpectFailure(
    () => StoreEngine.ValidateManifestShape(
        EquipmentManifest(side: "Center"),
        "fixture/asset.yaml"),
    "unsupported");
ExpectFailure(
    () => StoreEngine.ValidateManifestShape(
        EquipmentManifest(anchor: "Grip"),
        "fixture/asset.yaml"),
    "must be 'Mount'");


var highDensityPart = new Hero.Voxel.Tool.CompiledPart(
    "Staff",
    null,
    "opaque",
    new Hero.Voxel.Tool.CompiledTransform(new Hero.Voxel.Tool.Int3(0, 0, 0)),
    default,
    new Hero.Voxel.Tool.Int3(1, 1, 1),
    new[] { new Hero.Voxel.Tool.VoxelCell(0, 0, 0, 0) });
var highDensityEquipment = new Hero.Voxel.Tool.VoxelCompilation(
    "HighDensityEquipment",
    64,
    Array.Empty<Hero.Voxel.Tool.PaletteEntry>(),
    new[] { highDensityPart },
    new[]
    {
        new Hero.Voxel.Tool.CompiledSocket(
            "Mount",
            "Staff",
            new Hero.Voxel.Tool.CompiledTransform(new Hero.Voxel.Tool.Int3(0, 0, 0)))
    },
    RequiredSemanticClips("Staff"))
{
    AssemblyOrigin = new Hero.Voxel.Tool.CompiledAssemblyOrigin("Staff")
};
Require(
    StoreEngine.IsEquipmentCompositionFrameValid(highDensityEquipment),
    "VPU64 one-part equipment with a canonical assembly origin must remain compatibility-eligible");
Require(
    !StoreEngine.IsEquipmentCompositionFrameValid(highDensityEquipment with { AssemblyOrigin = null }),
    "equipment without a canonical assembly origin must remain compatibility-ineligible");
Require(
    !StoreEngine.IsEquipmentCompositionFrameValid(highDensityEquipment with { VoxelsPerUnit = 0 }),
    "equipment with non-positive VPU must remain compatibility-ineligible");

ExpectFailure(
    () => StoreEngine.ValidateEquipmentCompositionFrame(
        highDensityEquipment with
        {
            Sockets = new[]
            {
                new Hero.Voxel.Tool.CompiledSocket(
                    "Mount",
                    "Staff",
                    new Hero.Voxel.Tool.CompiledTransform(
                        new Hero.Voxel.Tool.Int3(1, 0, 0)))
            }
        },
        EquipmentManifest().Attachment!),
    "must coincide");

ExpectFailure(
    () => StoreEngine.ValidateEquipmentCompositionFrame(
        highDensityEquipment with
        {
            Animations = RequiredSemanticClips("Staff")
                .Where(x => x.Name != "Death")
                .ToArray()
        },
        EquipmentManifest().Attachment!),
    "Death");

ExpectFailure(
    () => StoreEngine.ValidateEquipmentCompositionFrame(
        highDensityEquipment with
        {
            Animations = RequiredSemanticClips("Staff")
                .Select(x => x.Name == "Idle"
                    ? x with { Loop = false }
                    : x)
                .ToArray()
        },
        EquipmentManifest().Attachment!),
    "loop setting");

var variantEquipment = highDensityEquipment with
{
    Animations = RequiredSemanticClips(
        "Staff",
        extraClips: new[]
        {
            ("Idle.V1", true),
            ("WalkForward.V2", true),
            ("Attack.V1", false),
            ("Attack.V2", false),
            ("Attack.Left", false),
            ("Hit.V1", false),
            ("Death.V3", false),
            ("Attack.Left.V2", false)
        })
};
StoreEngine.ValidateEquipmentCompositionFrame(
    variantEquipment,
    EquipmentManifest().Attachment!);
Require(
    StoreEngine.ResolveSemanticClipName(
        variantEquipment,
        "Attack",
        "Left",
        "V2") == "Attack.Left.V2",
    "fully qualified side+variant clip must resolve first");
Require(
    StoreEngine.ResolveSemanticClipName(
        variantEquipment with
        {
            Animations = variantEquipment.Animations
                .Where(x => x.Name != "Attack.Left.V2")
                .ToArray()
        },
        "Attack",
        "Left",
        "V2") == "Attack.V2",
    "generic requested variant must resolve before side-only fallback");
Require(
    StoreEngine.ResolveSemanticClipName(
        highDensityEquipment,
        "Attack",
        "Left",
        "V2") == "Attack",
    "missing requested variant and side clip must fall back to the required base clip");
ExpectFailure(
    () => StoreEngine.ValidateEquipmentCompositionFrame(
        highDensityEquipment with
        {
            Animations = RequiredSemanticClips(
                "Staff",
                extraClips: new[] { ("Death.V2", true) })
        },
        EquipmentManifest().Attachment!),
    "Death.V2");
ExpectFailure(
    () => StoreEngine.ValidateEquipmentCompositionFrame(
        highDensityEquipment with
        {
            Animations = RequiredSemanticClips(
                "Staff",
                extraClips: new[] { ("Attack.Heavy", false) })
        },
        EquipmentManifest().Attachment!),
    "variant must match");

var translatedEquipmentClips =
    RequiredSemanticClips("Staff").ToArray();
var translatedAttack =
    translatedEquipmentClips.Single(x => x.Name == "Attack");
var positionChannel =
    new Hero.Voxel.Tool.CompiledAnimationVectorChannel(
        Hero.Voxel.Tool.CompiledAnimationInterpolation.Linear,
        new[]
        {
            new Hero.Voxel.Tool.CompiledAnimationVectorKey(
                0f,
                new Hero.Voxel.Tool.Float3(0f, 0f, 0f)),
            new Hero.Voxel.Tool.CompiledAnimationVectorKey(
                1f,
                new Hero.Voxel.Tool.Float3(1f, 0f, 0f))
        });
translatedEquipmentClips[
    Array.FindIndex(
        translatedEquipmentClips,
        x => x.Name == "Attack")] =
    translatedAttack with
    {
        Tracks = new[]
        {
            new Hero.Voxel.Tool.CompiledAnimationTrack(
                "Staff",
                positionChannel,
                null,
                null)
        }
    };
var translatedEquipment = highDensityEquipment with
{
    Animations = translatedEquipmentClips
};
StoreEngine.ValidateEquipmentCompositionFrame(
    translatedEquipment,
    EquipmentManifest().Attachment!);


var bodyPart = new Hero.Voxel.Tool.CompiledPart(
    "Body",
    null,
    "opaque",
    new Hero.Voxel.Tool.CompiledTransform(new Hero.Voxel.Tool.Int3(0, 0, 0)),
    new Hero.Voxel.Tool.Float3(1, 2, 3),
    new Hero.Voxel.Tool.Int3(4, 5, 6),
    new[]
    {
        new Hero.Voxel.Tool.VoxelCell(1, 2, 3, 0),
        new Hero.Voxel.Tool.VoxelCell(3, 4, 5, 0)
    });
var rigidBody = new Hero.Voxel.Tool.VoxelCompilation(
    "RigidBody",
    2,
    Array.Empty<Hero.Voxel.Tool.PaletteEntry>(),
    new[] { bodyPart },
    new[]
    {
        new Hero.Voxel.Tool.CompiledSocket(
            "Equip.Hand.Left.0",
            "Body",
            new Hero.Voxel.Tool.CompiledTransform(new Hero.Voxel.Tool.Int3(0, 2, 3))),
        new Hero.Voxel.Tool.CompiledSocket(
            "Equip.Hand.Right.0",
            "Body",
            new Hero.Voxel.Tool.CompiledTransform(new Hero.Voxel.Tool.Int3(2, 2, 3)))
    },
    RequiredSemanticClips("Body", includeSideAttack: true))
{
    AssemblyOrigin = new Hero.Voxel.Tool.CompiledAssemblyOrigin("Body")
};
StoreEngine.ValidateRigidBodyAvatar(rigidBody);
CompatibilityPoseEvaluation translatedCompatibility =
    StoreEngine.EvaluateCompatibilityPose(
        rigidBody,
        rigidBody.Animations.Single(x => x.Name == "Attack.Right"),
        1,
        translatedEquipment,
        translatedEquipment.Animations.Single(x => x.Name == "Attack"),
        0,
        0.5f);
Require(
    translatedCompatibility.Passed &&
    translatedCompatibility.ReferenceError <= 0.0001f &&
    translatedCompatibility.AnimatedMountDrift > 0.0001f,
    "compatibility must accept a valid bind reference even when authored equipment animation moves the evaluated Mount");

var variantBody = rigidBody with
{
    Animations = RequiredSemanticClips(
        "Body",
        includeSideAttack: true,
        extraClips: new[]
        {
            ("Attack.Left.V2", false),
            ("Death.V2", false)
        })
};
StoreEngine.ValidateRigidBodyAvatar(variantBody);
string bodyRequestedVariant = StoreEngine.ResolveSemanticClipName(
    variantBody,
    "Attack",
    "Left",
    "V2");
string equipmentRequestedVariant = StoreEngine.ResolveSemanticClipName(
    variantEquipment,
    "Attack",
    "Left",
    "V1");
string equipmentSideFallback = StoreEngine.ResolveSemanticClipName(
    variantEquipment,
    "Attack",
    "Left",
    "V3");
Require(
    bodyRequestedVariant == "Attack.Left.V2" &&
    equipmentRequestedVariant == "Attack.V1" &&
    equipmentSideFallback == "Attack.Left",
    "body and equipment must resolve independent model-local variants and side fallback at one caller-owned semantic phase");

ExpectFailure(
    () => StoreEngine.ValidateRigidBodyAvatar(
        rigidBody with
        {
            Sockets = new[]
            {
                new Hero.Voxel.Tool.CompiledSocket(
                    "Equip.Hand.Up.0",
                    "Body",
                    new Hero.Voxel.Tool.CompiledTransform(
                        new Hero.Voxel.Tool.Int3(0, 2, 3))),
                rigidBody.Sockets[1]
            }
        }),
    "Equip.<Capability>.<Side>.<Instance>");

ExpectFailure(
    () => StoreEngine.ValidateRigidBodyAvatar(
        rigidBody with
        {
            Sockets = new[]
            {
                rigidBody.Sockets[0],
                rigidBody.Sockets[0],
                rigidBody.Sockets[1]
            }
        }),
    "must be unique");

ExpectFailure(
    () => StoreEngine.ValidateRigidBodyAvatar(
        rigidBody with
        {
            Sockets = new[]
            {
                rigidBody.Sockets[1]
            }
        }),
    "both Left and Right");

ExpectFailure(
    () => StoreEngine.ValidateRigidBodyAvatar(
        rigidBody with
        {
            Animations = RequiredSemanticClips(
                    "Body",
                    includeSideAttack: true)
                .Where(x => x.Name != "Attack.Left")
                .ToArray()
        }),
    "Attack.Left");

ExpectFailure(
    () => StoreEngine.ValidateRigidBodyAvatar(
        rigidBody with
        {
            Animations = RequiredSemanticClips(
                    "Body",
                    includeSideAttack: true)
                .Select(x => x.Name == "Idle"
                    ? x with { Loop = false }
                    : x)
                .ToArray()
        }),
    "loop setting");

var multiSlotBody = rigidBody with
{
    Sockets = new[]
    {
        new Hero.Voxel.Tool.CompiledSocket(
            "Equip.Hand.Left.0",
            "Body",
            new Hero.Voxel.Tool.CompiledTransform(
                new Hero.Voxel.Tool.Int3(0, 2, 3))),
        new Hero.Voxel.Tool.CompiledSocket(
            "Equip.Hand.Left.1",
            "Body",
            new Hero.Voxel.Tool.CompiledTransform(
                new Hero.Voxel.Tool.Int3(0, 3, 3))),
        new Hero.Voxel.Tool.CompiledSocket(
            "Equip.Hand.Right.0",
            "Body",
            new Hero.Voxel.Tool.CompiledTransform(
                new Hero.Voxel.Tool.Int3(2, 2, 3)))
    }
};
StoreEngine.ValidateRigidBodyAvatar(multiSlotBody);

Require(
    StoreEngine.ResolveCompatibleSlotNames(
            multiSlotBody,
            EquipmentManifest().Attachment!,
            3)
        .SequenceEqual(
            new[]
            {
                "Equip.Hand.Left.0",
                "Equip.Hand.Left.1",
                "Equip.Hand.Right.0"
            },
            StringComparer.Ordinal),
    "Any-side slot resolution must be deterministic across repeated equipment instances");

Require(
    StoreEngine.ResolveCompatibleSlotNames(
            multiSlotBody,
            EquipmentManifest(side: "Left").Attachment!,
            2)
        .SequenceEqual(
            new[]
            {
                "Equip.Hand.Left.0",
                "Equip.Hand.Left.1"
            },
            StringComparer.Ordinal),
    "Left-only slot resolution must preserve same-side multiplicity");

ExpectFailure(
    () => StoreEngine.ResolveCompatibleSlotNames(
        multiSlotBody,
        EquipmentManifest(side: "Left").Attachment!,
        3),
    "Insufficient body slot capacity");

ExpectFailure(
    () => StoreEngine.ResolveCompatibleSlotNames(
        multiSlotBody,
        new AssetAttachment
        {
            Capability = "Back",
            Side = "Any",
            Anchor = "Mount"
        },
        1),
    "available=0");

ExpectFailure(
    () => StoreEngine.ResolveCompatibleSlotNames(
        multiSlotBody,
        EquipmentManifest(side: "Right").Attachment!,
        2),
    "available=1");

var articulatedBody = new Hero.Voxel.Tool.VoxelCompilation(
    "Articulated",
    2,
    Array.Empty<Hero.Voxel.Tool.PaletteEntry>(),
    new[]
    {
        bodyPart,
        bodyPart with { Name = "Head", ParentName = "Body" }
    },
    Array.Empty<Hero.Voxel.Tool.CompiledSocket>(),
    Array.Empty<Hero.Voxel.Tool.CompiledAnimationClip>())
{
    AssemblyOrigin = new Hero.Voxel.Tool.CompiledAssemblyOrigin("Body")
};
ExpectFailure(
    () => StoreEngine.ValidateRigidBodyAvatar(articulatedBody),
    "exactly one authored part named 'Body'");

StorePolicy activePolicy = ContractYaml.Load<StorePolicy>(
    Path.Combine(repositoryRoot, "Store", "policy.yaml"));
Require(
    activePolicy.Budget.MaxVoxelsPerAsset == 65536 &&
    activePolicy.Budget.MaxPartsPerAsset == 16,
    "active store budget must allow bounded high-resolution authoring without changing part capacity");
StoreEngine.ValidatePublicationBudget(8357, 1, activePolicy.Budget);
ExpectFailure(
    () => StoreEngine.ValidatePublicationBudget(65537, 1, activePolicy.Budget),
    "65537>65536");

// Provider resolution and public Hero.Voxel integration.
HeroVoxelProvider.Resolution resolution = engine.ResolveProvider();
Require(
    File.Exists(Path.Combine(
        resolution.Root,
        resolution.Lock.Workflow.Replace('/', Path.DirectorySeparatorChar))),
    "provider workflow did not resolve");

string missingProviderRoot = TempRoot();
try
{
    string missingLock = Path.Combine(
        missingProviderRoot,
        "toolchain",
        "hero-voxel.lock.yaml");
    Directory.CreateDirectory(Path.GetDirectoryName(missingLock)!);
    File.Copy(
        Path.Combine(repositoryRoot, "toolchain", "hero-voxel.lock.yaml"),
        missingLock);
    ExpectFailure(
        () => new HeroVoxelProvider(missingProviderRoot).Resolve(),
        "Dependencies/hero-studio");
}
finally
{
    Directory.Delete(missingProviderRoot, true);
}

string mismatchRoot = TempRoot();
try
{
    string sourceLock = Path.Combine(
        repositoryRoot,
        "toolchain",
        "hero-voxel.lock.yaml");
    string mismatchLock = Path.Combine(
        mismatchRoot,
        "toolchain",
        "hero-voxel.lock.yaml");
    Directory.CreateDirectory(Path.GetDirectoryName(mismatchLock)!);
    File.WriteAllText(
        mismatchLock,
        File.ReadAllText(sourceLock).Replace(
            resolution.Lock.HeroStudioRevision,
            new string('0', 40),
            StringComparison.Ordinal));
    ExpectFailure(
        () => new HeroVoxelProvider(mismatchRoot, resolution.Root).Resolve(),
        "revision mismatch");
}
finally
{
    Directory.Delete(mismatchRoot, true);
}

string absoluteLockRoot = TempRoot();
try
{
    string absoluteLock = Path.Combine(
        absoluteLockRoot,
        "toolchain",
        "hero-voxel.lock.yaml");
    Directory.CreateDirectory(Path.GetDirectoryName(absoluteLock)!);
    File.WriteAllText(
        absoluteLock,
        File.ReadAllText(Path.Combine(
            repositoryRoot,
            "toolchain",
            "hero-voxel.lock.yaml"))
        .Replace(
            resolution.Lock.ToolProject,
            Path.Combine(resolution.Root, resolution.Lock.ToolProject),
            StringComparison.Ordinal));
    ExpectFailure(
        () => new HeroVoxelProvider(absoluteLockRoot, resolution.Root).Resolve(),
        "portable relative paths");
}
finally
{
    Directory.Delete(absoluteLockRoot, true);
}

// Store YAML is strict: gameplay, mount, and pair-correction fields are not accepted.
ExpectFailure(
    () => ContractYaml.Parse<AssetManifest>(
        """
        version: 2
        id: invalid.gameplay
        kind: equipment
        unitRef: invalid/gameplay
        form: fixture
        damage: 10
        source:
          base: ./Source/Item
          palette: ./Palette.yaml
        style:
          family: low-resolution-voxel.v1
        """),
    "damage");

ExpectFailure(
    () => ContractYaml.Parse<AssetManifest>(
        """
        version: 2
        id: invalid.mount
        kind: equipment
        unitRef: invalid/mount
        form: fixture
        mount: legacy
        source:
          base: ./Source/Item
          palette: ./Palette.yaml
        style:
          family: low-resolution-voxel.v1
        """),
    "mount");

ExpectFailure(
    () => ContractYaml.Parse<AssetManifest>(
        """
        version: 2
        id: invalid.correction
        kind: equipment
        unitRef: invalid/correction
        form: fixture
        correction: [1, 2, 3]
        source:
          base: ./Source/Item
          palette: ./Palette.yaml
        style:
          family: low-resolution-voxel.v1
        """),
    "correction");

// Repository containment and identity uniqueness.
string invalidPathRoot = TempRoot();
try
{
    string assetDir = Path.Combine(invalidPathRoot, "Assets", "Bad");
    Directory.CreateDirectory(assetDir);
    File.WriteAllText(
        Path.Combine(assetDir, "asset.yaml"),
        """
        version: 3
        id: bad.path
        kind: avatar
        unitRef: bad/path
        source:
          base: ../../../../outside
          palette: ./Palette.yaml
        style:
          family: low-resolution-voxel.v1
        """);
    ExpectFailure(
        () => new StoreEngine(invalidPathRoot).DiscoverAssets(),
        "escapes repository root");
}
finally
{
    Directory.Delete(invalidPathRoot, true);
}

string invalidKindRoot = TempRoot();
try
{
    string assetDir = Path.Combine(invalidKindRoot, "Assets", "BadKind");
    Directory.CreateDirectory(assetDir);
    File.WriteAllText(
        Path.Combine(assetDir, "asset.yaml"),
        """
        version: 3
        id: bad.kind
        kind: weapon
        unitRef: bad/kind
        source:
          base: ./Source/Item
          palette: ./Palette.yaml
        style:
          family: low-resolution-voxel.v1
        """);
    ExpectFailure(
        () => new StoreEngine(invalidKindRoot).DiscoverAssets(),
        "Unsupported asset kind");
}
finally
{
    Directory.Delete(invalidKindRoot, true);
}

string duplicateRoot = TempRoot();
try
{
    for (int i = 0; i < 2; i++)
    {
        string assetDir = Path.Combine(duplicateRoot, "Assets", "A" + i);
        Directory.CreateDirectory(assetDir);
        File.WriteAllText(
            Path.Combine(assetDir, "asset.yaml"),
            $"""
             version: 3
             id: duplicate.asset
             kind: avatar
             unitRef: duplicate/unit-{i}
             source:
               base: ./Source/Item
               palette: ./Palette.yaml
             style:
               family: low-resolution-voxel.v1
             """);
    }

    ExpectFailure(
        () => new StoreEngine(duplicateRoot).DiscoverAssets(),
        "Duplicate asset id");
}
finally
{
    Directory.Delete(duplicateRoot, true);
}

string duplicateUnitRefRoot = TempRoot();
try
{
    for (int i = 0; i < 2; i++)
    {
        string assetDir = Path.Combine(duplicateUnitRefRoot, "Assets", "U" + i);
        Directory.CreateDirectory(assetDir);
        File.WriteAllText(
            Path.Combine(assetDir, "asset.yaml"),
            $"""
             version: 3
             id: duplicate.unitref-{i}
             kind: avatar
             unitRef: duplicate/shared-unit
             source:
               base: ./Source/Item
               palette: ./Palette.yaml
             style:
               family: low-resolution-voxel.v1
             """);
    }
    ExpectFailure(
        () => new StoreEngine(duplicateUnitRefRoot).DiscoverAssets(),
        "Duplicate UnitRef");
}
finally
{
    Directory.Delete(duplicateUnitRefRoot, true);
}

string identityHistoryRoot = TempRoot();
try
{
    RunGit(identityHistoryRoot, "init");
    RunGit(identityHistoryRoot, "config", "user.email", "test@example.invalid");
    RunGit(identityHistoryRoot, "config", "user.name", "Voxel Store Test");
    string originalDir = Path.Combine(identityHistoryRoot, "Assets", "Original");
    Directory.CreateDirectory(originalDir);
    string originalManifest = Path.Combine(originalDir, "asset.yaml");
    const string originalIdentity =
        """
        version: 3
        id: history.original
        kind: avatar
        unitRef: history/original
        source:
          base: ./Source/Item
          palette: ./Palette.yaml
        style:
          family: low-resolution-voxel.v1
        """;
    File.WriteAllText(originalManifest, originalIdentity);
    RunGit(identityHistoryRoot, "add", ".");
    RunGit(identityHistoryRoot, "commit", "-m", "baseline identity");

    File.WriteAllText(
        originalManifest,
        originalIdentity.Replace(
            "unitRef: history/original",
            "unitRef: history/redefined",
            StringComparison.Ordinal));
    ExpectFailure(
        () => new StoreEngine(identityHistoryRoot).DiscoverAssets(),
        "semantic identity changed");

    File.WriteAllText(originalManifest, originalIdentity);
    string newDir = Path.Combine(identityHistoryRoot, "Assets", "New");
    Directory.CreateDirectory(newDir);
    File.WriteAllText(
        Path.Combine(newDir, "asset.yaml"),
        originalIdentity
            .Replace(
                "id: history.original",
                "id: history.new",
                StringComparison.Ordinal)
            .Replace(
                "unitRef: history/original",
                "unitRef: history/new",
                StringComparison.Ordinal));
    Require(
        new StoreEngine(identityHistoryRoot).DiscoverAssets().Count == 2,
        "new identity must be allowed beside unchanged published identity");
}
finally
{
    DeleteTree(identityHistoryRoot);
}


int coldStartAssetCount = 0;
bool coldStartHealthy = false;
int coldStartCompatibilityCount = 0;
int coldStartGapCount = 0;
string coldStartRoot = TempRoot();
try
{
    CopyDirectory(
        Path.Combine(repositoryRoot, "Store"),
        Path.Combine(coldStartRoot, "Store"));
    CopyDirectory(
        Path.Combine(repositoryRoot, "Collections"),
        Path.Combine(coldStartRoot, "Collections"));
    CopyDirectory(
        Path.Combine(repositoryRoot, "toolchain"),
        Path.Combine(coldStartRoot, "toolchain"));
    CopyDirectory(
        Path.Combine(repositoryRoot, "Styles"),
        Path.Combine(coldStartRoot, "Styles"));
    CopyDirectory(
        Path.Combine(repositoryRoot, "Assets", "Avatars", "FoundationGuard01"),
        Path.Combine(coldStartRoot, "Assets", "Avatars", "FoundationGuard01"));
    CopyDirectory(
        Path.Combine(repositoryRoot, "Assets", "Equipment", "Sword02"),
        Path.Combine(coldStartRoot, "Assets", "Equipment", "Sword02"));
    CopyDirectory(
        Path.Combine(repositoryRoot, "Assets", "Equipment", "Dagger02"),
        Path.Combine(coldStartRoot, "Assets", "Equipment", "Dagger02"));

    var coldStartEngine = new StoreEngine(coldStartRoot, resolution.Root);
IReadOnlyList<AssetRecord> assets = coldStartEngine.DiscoverAssets();
Require(
    assets.Count == 3 &&
    assets.Count(x => x.Manifest.Kind == "avatar") == 1 &&
    assets.Count(x => x.Manifest.Kind == "equipment") == 2 &&
    assets.Where(x => x.Manifest.Kind == "equipment")
        .Select(x => x.Manifest.Id)
        .OrderBy(x => x, StringComparer.Ordinal)
        .SequenceEqual(
            new[]
            {
                "foundation.equipment.dagger.02",
                "foundation.equipment.sword.02"
            },
            StringComparer.Ordinal),
    "isolated cold-start proof baseline must contain one avatar plus exactly Sword02 and Dagger02");
Require(
    assets.Select(x => x.Manifest.Id)
        .Distinct(StringComparer.Ordinal).Count() == assets.Count,
    "asset ids must be unique");
Require(
    assets.Select(x => x.Manifest.UnitRef)
        .Distinct(StringComparer.Ordinal).Count() == assets.Count,
    "UnitRefs must be unique");

foreach (AssetRecord asset in assets)
{
    Hero.Voxel.Tool.VoxelSpecDocument spec =
        Hero.Voxel.Tool.VoxelSpecYaml.LoadSpec(asset.SourceBase + ".voxel.yaml");
    Hero.Voxel.Tool.PaletteSpec palette =
        Hero.Voxel.Tool.VoxelSpecYaml.LoadPalette(asset.PalettePath);
    Hero.Voxel.Tool.VoxelCompilation compiled =
        Hero.Voxel.Tool.VoxelSpecCompiler.Compile(spec, palette);
    Require(
        compiled.Parts.Count > 0,
        $"provider compile produced no parts for {asset.Manifest.Id}");
}

// Positive deterministic scan and all-pairs animated composition proof.
StoreSnapshot first =
    coldStartEngine.Scan(writeDerived: true, writeCompatibility: true);
StoreSnapshot second =
    coldStartEngine.Scan(writeDerived: true, writeCompatibility: true);
coldStartAssetCount = assets.Count;
coldStartHealthy = first.Healthy;
coldStartCompatibilityCount = first.Compatibility.Count;
coldStartGapCount = first.CoverageGaps.Count;
Require(
    !first.Healthy && !second.Healthy,
    "two-weapon cold-start proof baseline must remain intentionally incomplete");
Require(
    first.CoverageGaps.Count == 8 &&
    first.CoverageGaps.All(x => x.Required && x.Kind == "equipment"),
    "two-weapon cold-start proof baseline must expose exactly eight required equipment gaps");
Require(
    first.NextJob?.Priority == "required-coverage" &&
    first.NextJob.Target == "foundation.equipment.axe" &&
    second.NextJob?.Priority == "required-coverage" &&
    second.NextJob.Target == "foundation.equipment.axe",
    "partial baseline must deterministically continue with Axe");
Require(
    first.Assets.Count == second.Assets.Count,
    "scan must be deterministic");
Require(
    first.Compatibility.Count == 32 &&
    first.Compatibility.All(x =>
        x.Passed &&
        x.Capability == "Hand" &&
        x.Socket is "Equip.Hand.Left.0" or "Equip.Hand.Right.0" &&
        x.Side is "Left" or "Right" &&
        x.Phase >= 0f && x.Phase <= 1f),
    "Sword02 and Dagger02 must each pass both side-aware Hand slots across semantic samples");
Require(
    first.Compatibility
        .GroupBy(x => x.Equipment, StringComparer.Ordinal)
        .All(x =>
            x.Count() == 16 &&
            x.Select(row => row.Socket).Distinct(StringComparer.Ordinal).Count() == 2 &&
            x.Select(row => row.Motion).Distinct(StringComparer.Ordinal).Count() == 5),
    "each equipment asset must contribute two Hand slots across all required semantic motions");

string formCoveragePath = Path.Combine(coldStartRoot, "Collections", "combat-foundation.v1.yaml");
string formCoverageOriginal = File.ReadAllText(formCoveragePath);
try
{
    File.WriteAllText(
        formCoveragePath,
        formCoverageOriginal.Replace(
            "    form: sword",
            "    form: sword-missing",
            StringComparison.Ordinal));
    StoreSnapshot formGap = coldStartEngine.Scan(writeDerived: false, writeCompatibility: false);
    CoverageGap swordFormGap =
        formGap.CoverageGaps.Single(x => x.Id == "foundation.equipment.sword");
    Require(
        formGap.CoverageGaps.Count == 9 &&
        swordFormGap.Form == "sword-missing" &&
        formGap.NextJob?.Target == "foundation.equipment.sword",
        "coverage must match reusable physical form exactly");
}
finally
{
    File.WriteAllText(formCoveragePath, formCoverageOriginal);
}

string compatibilityEvidence = Path.Combine(
    assets.Single(x => x.Manifest.Kind == "avatar").ProofDirectory,
    "Compatibility",
    "foundation.equipment.sword.02.json");
using (JsonDocument evidence =
       JsonDocument.Parse(File.ReadAllText(compatibilityEvidence)))
{
    JsonElement samples = evidence.RootElement.GetProperty("samples");
    Require(
        evidence.RootElement.GetProperty("convention").GetString() == "model-socket-reference-semantic-phase.v2" &&
        samples.GetArrayLength() == 16,
        "compatibility evidence must use the model-socket reference semantic-phase convention for both Hand slots");
    Require(
        samples.EnumerateArray().All(x =>
            x.GetProperty("capability").GetString() == "Hand" &&
            x.GetProperty("socket").GetString()!.StartsWith("Equip.Hand.", StringComparison.Ordinal) &&
            x.GetProperty("phase").GetSingle() is >= 0f and <= 1f &&
            x.GetProperty("bodySocketOrigin").GetArrayLength() == 3 &&
            x.GetProperty("attachmentReferenceOrigin").GetArrayLength() == 3 &&
            x.GetProperty("evaluatedMountOrigin").GetArrayLength() == 3 &&
            x.GetProperty("referenceError").GetSingle() <= 0.0001f &&
            x.GetProperty("animatedMountDrift").GetSingle() >= 0f &&
            x.GetProperty("passed").GetBoolean()),
        "compatibility evidence must persist slot, phase, bind reference, evaluated Mount drift, and pass result");
}

// Derived catalog/health can be deleted and regenerated byte-identically.
string catalogPath =
    Path.Combine(coldStartRoot, "Catalog", "catalog.generated.json");
string healthPath =
    Path.Combine(coldStartRoot, "Catalog", "health.generated.json");
string catalogBefore = File.ReadAllText(catalogPath);
string healthBefore = File.ReadAllText(healthPath);
File.Delete(catalogPath);
File.Delete(healthPath);
coldStartEngine.Scan(writeDerived: true, writeCompatibility: true);
Require(
    File.ReadAllText(catalogPath) == catalogBefore,
    "catalog regeneration must be byte deterministic");
Require(
    File.ReadAllText(healthPath) == healthBefore,
    "health regeneration must be byte deterministic");

// A stale generated sibling is diagnosed before expansion work.
AssetRecord sword = assets.Single(
    x => x.Manifest.Id == "foundation.equipment.sword.02");
string swordGenerated = sword.SourceBase + ".generated.vox";
byte[] originalGenerated = File.ReadAllBytes(swordGenerated);
string coveragePath = Path.Combine(coldStartRoot, "Collections", "combat-foundation.v1.yaml");
string originalCoverage = File.ReadAllText(coveragePath);
try
{
    File.WriteAllBytes(
        swordGenerated,
        originalGenerated.Concat(new byte[] { 0x7f }).ToArray());
    File.WriteAllText(
        coveragePath,
        originalCoverage +
        """

          - id: simultaneous.coverage-gap
            kind: equipment
            form: fixture-missing
            required: true
        """);
    StoreSnapshot stale =
        coldStartEngine.Scan(writeDerived: false, writeCompatibility: false);
    AssetHealth staleSword =
        stale.Assets.Single(x => x.Id == sword.Manifest.Id);
    Require(
        staleSword.FailedGate == "generation",
        "stale generated VOX must fail generation gate");
    Require(
        stale.NextJob?.Priority == "stale-reproducible" &&
        stale.NextJob.Target == sword.Manifest.Id,
        "stale generated VOX must be the failure-first maintenance job");
    Require(
        stale.CoverageGaps.Any(x => x.Id == "simultaneous.coverage-gap"),
        "priority test must contain a simultaneous coverage gap");
}
finally
{
    File.WriteAllBytes(swordGenerated, originalGenerated);
    File.WriteAllText(coveragePath, originalCoverage);
}

// Visual/reference audit hashes fail closed when authority changes.
AssetRecord avatar =
    assets.Single(x => x.Manifest.Kind == "avatar");
string compositionPath =
    Path.Combine(avatar.ReferenceDirectory, "composition.png");
byte[] originalComposition = File.ReadAllBytes(compositionPath);
try
{
    File.WriteAllBytes(
        compositionPath,
        originalComposition.Concat(new byte[] { 0x7f }).ToArray());
    StoreSnapshot staleReference =
        coldStartEngine.Scan(writeDerived: false, writeCompatibility: false);
    AssetHealth avatarHealth =
        staleReference.Assets.Single(x => x.Id == avatar.Manifest.Id);
    Require(
        avatarHealth.FailedGate == "reference",
        "changed reference must invalidate sealed audit evidence");
    Require(
        !avatarHealth.Published,
        "stale reference evidence must remove derived publication");
}
finally
{
    File.WriteAllBytes(compositionPath, originalComposition);
}

void AssertProofAuditInvalidatesOnTextChange(string path, string label)
{
    string original = File.ReadAllText(path);
    try
    {
        File.WriteAllText(
            path,
            original + Environment.NewLine + "# audit-stale-test");
        StoreSnapshot staleProof =
            coldStartEngine.Scan(writeDerived: false, writeCompatibility: false);
        AssetHealth health =
            staleProof.Assets.Single(x => x.Id == avatar.Manifest.Id);
        Require(
            health.FailedGate == "proof",
            $"{label} change must invalidate proof audit evidence");
    }
    finally
    {
        File.WriteAllText(path, original);
    }
}

AssertProofAuditInvalidatesOnTextChange(
    avatar.SourceBase + ".voxel.yaml",
    "VoxelSpec");
AssertProofAuditInvalidatesOnTextChange(
    avatar.PalettePath,
    "palette");

string heroProof =
    Path.Combine(avatar.ProofDirectory, "Standalone", "hero.png");
byte[] originalHeroProof = File.ReadAllBytes(heroProof);
try
{
    File.WriteAllBytes(
        heroProof,
        originalHeroProof.Concat(new byte[] { 0 }).ToArray());
    StoreSnapshot staleProof =
        coldStartEngine.Scan(writeDerived: false, writeCompatibility: false);
    AssetHealth health =
        staleProof.Assets.Single(x => x.Id == avatar.Manifest.Id);
    Require(
        health.FailedGate == "proof",
        "changed required proof PNG must invalidate proof audit evidence");
}
finally
{
    File.WriteAllBytes(heroProof, originalHeroProof);
}
}
finally
{
    DeleteTree(coldStartRoot);
}

// Initial bootstrap with only the canonical avatar exposes all ten form gaps.
string bootstrapRoot = TempRoot();
try
{
    foreach (string relative in new[]
             {
                 "Store/policy.yaml",
                 "Collections/combat-foundation.v1.yaml",
                 "toolchain/hero-voxel.lock.yaml",
                 "Assets/Avatars/FoundationGuard01/Source/FoundationGuard01.palette.yaml"
             })
    {
        CopyFile(repositoryRoot, bootstrapRoot, relative);
    }

    CopyDirectory(
        Path.Combine(
            repositoryRoot,
            "Styles",
            "low-resolution-voxel.v1"),
        Path.Combine(
            bootstrapRoot,
            "Styles",
            "low-resolution-voxel.v1"));

    CopyDirectory(
        Path.Combine(
            repositoryRoot,
            "Assets",
        "Avatars",
            "FoundationGuard01"),
        Path.Combine(
            bootstrapRoot,
            "Assets",
        "Avatars",
            "FoundationGuard01"));

    StoreSnapshot bootstrap =
        new StoreEngine(bootstrapRoot, resolution.Root).Scan(false, false);
    Require(
        bootstrap.Assets.Count == 1 && bootstrap.Assets[0].Published,
        "bootstrap avatar must publish");
    Require(
        bootstrap.CoverageGaps.Count == 10 &&
        bootstrap.CoverageGaps.All(x =>
            x.Kind == "equipment" &&
            !string.IsNullOrWhiteSpace(x.Form)),
        "avatar-only bootstrap must expose exactly ten equipment form gaps");
    Require(
        bootstrap.NextJob?.Priority == "required-coverage" &&
        bootstrap.NextJob.Target == "foundation.equipment.sword",
        "bootstrap must select the first deterministic form gap after healthy avatar");
}
finally
{
    DeleteTree(bootstrapRoot);
}


// Independent full-catalog convergence fixture: isolated cold-start baseline stays partial,
// while the unchanged ten-form contract must still converge when all forms publish.
string fullCatalogRoot = TempRoot();
try
{
    CopyDirectory(
        Path.Combine(repositoryRoot, "Store"),
        Path.Combine(fullCatalogRoot, "Store"));
    CopyDirectory(
        Path.Combine(repositoryRoot, "Collections"),
        Path.Combine(fullCatalogRoot, "Collections"));
    CopyDirectory(
        Path.Combine(repositoryRoot, "toolchain"),
        Path.Combine(fullCatalogRoot, "toolchain"));
    CopyDirectory(
        Path.Combine(repositoryRoot, "Styles", "low-resolution-voxel.v1"),
        Path.Combine(fullCatalogRoot, "Styles", "low-resolution-voxel.v1"));
    CopyFile(repositoryRoot, fullCatalogRoot, "Assets/Avatars/FoundationGuard01/Source/FoundationGuard01.palette.yaml");
    CopyDirectory(
        Path.Combine(repositoryRoot, "Assets", "Avatars", "FoundationGuard01"),
        Path.Combine(fullCatalogRoot, "Assets", "Avatars", "FoundationGuard01"));

    string fixtureEquipmentRoot = Path.Combine(
        fullCatalogRoot, "Assets", "Equipment");
    Directory.CreateDirectory(fixtureEquipmentRoot);
    string sourceSwordRoot = Path.Combine(
        repositoryRoot, "Assets", "Equipment", "Sword02");
    string sourceDaggerRoot = Path.Combine(
        repositoryRoot, "Assets", "Equipment", "Dagger02");
    CopyDirectory(sourceSwordRoot, Path.Combine(fixtureEquipmentRoot, "Sword02"));
    CopyDirectory(sourceDaggerRoot, Path.Combine(fixtureEquipmentRoot, "Dagger02"));

    string[] missingForms =
    {
        "axe", "hammer", "mace", "spear", "staff", "bow", "crossbow", "rifle"
    };
    var fullEngine = new StoreEngine(fullCatalogRoot, resolution.Root);
    foreach (string form in missingForms)
    {
        string fixtureName = "Fixture" + char.ToUpperInvariant(form[0]) + form[1..];
        string fixtureRoot = Path.Combine(fixtureEquipmentRoot, fixtureName);
        CopyDirectory(sourceSwordRoot, fixtureRoot);
        string manifestPath = Path.Combine(fixtureRoot, "asset.yaml");
        string manifest = File.ReadAllText(manifestPath)
            .Replace(
                "id: foundation.equipment.sword.02",
                $"id: fixture.equipment.{form}.01",
                StringComparison.Ordinal)
            .Replace(
                "unitRef: voxel-store/equipment/sword/02",
                $"unitRef: fixture/equipment/{form}/01",
                StringComparison.Ordinal)
            .Replace(
                "form: sword",
                $"form: {form}",
                StringComparison.Ordinal);
        File.WriteAllText(manifestPath, manifest);
        fullEngine.Seal($"fixture.equipment.{form}.01");
    }

    StoreSnapshot fullCatalog =
        new StoreEngine(fullCatalogRoot, resolution.Root).Scan(false, false);
    Require(
        fullCatalog.Healthy &&
        fullCatalog.Assets.Count(x => x.Kind == "equipment") == 10 &&
        fullCatalog.CoverageGaps.Count == 0 &&
        fullCatalog.NextJob == null &&
        fullCatalog.Compatibility.Count == 160 &&
        fullCatalog.Compatibility.All(x => x.Passed),
        "isolated ten-form catalog must converge to healthy/no-gap state with 160 passing side-aware semantic-phase compatibility rows");
}
finally
{
    DeleteTree(fullCatalogRoot);
}


// Checked-in toolchain data is portable, and maintenance owns one job only.
HeroVoxelLock lockFile = ContractYaml.Load<HeroVoxelLock>(
    Path.Combine(repositoryRoot, "toolchain", "hero-voxel.lock.yaml"));
Require(
    !Path.IsPathRooted(lockFile.ToolProject) &&
    !Path.IsPathRooted(lockFile.Workflow),
    "toolchain lock paths must remain repository-portable");

string skill = File.ReadAllText(
    Path.Combine(
        repositoryRoot,
        ".agents",
        "skills",
        "voxel-store-maintain",
        "SKILL.md"));
Require(
    skill.Contains(
        "Claim only the first asset-scoped job",
        StringComparison.OrdinalIgnoreCase) &&
    skill.Contains(
        "select a second asset",
        StringComparison.OrdinalIgnoreCase),
    "maintenance skill must enforce one claimed job per invocation");

string sampleIsolationRoot = TempRoot();
try
{
    string sampleAssetRoot = Path.Combine(sampleIsolationRoot, "Samples", "IgnoredAssetLikeSample");
    Directory.CreateDirectory(sampleAssetRoot);
    File.WriteAllText(
        Path.Combine(sampleAssetRoot, "asset.yaml"),
        """
        version: 2
        id: sample.must-not-publish
        kind: equipment
        unitRef: sample/must-not-publish
        """);
    Require(
        new StoreEngine(sampleIsolationRoot).DiscoverAssets().Count == 0,
        "Samples must remain outside published asset discovery even when they contain asset-like manifests");
}
finally
{
    DeleteTree(sampleIsolationRoot);
}

Console.WriteLine(
    $"PASS: {coldStartAssetCount} cold-start assets, healthy={coldStartHealthy}, " +
    $"compatibility={coldStartCompatibilityCount}, gaps={coldStartGapCount}");
