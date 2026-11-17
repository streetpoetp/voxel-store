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

static string PrepareReferenceFixture(string sourceRoot, string referenceYaml)
{
    string root = TempRoot();
    Directory.CreateDirectory(Path.Combine(root, "policy", "families"));
    File.WriteAllText(
        Path.Combine(root, "policy", "store.yaml"),
        """
        version: 1
        name: ReferenceFixture
        styleFamily: combat-foundation.v1
        referenceStyle: low-resolution-voxel.v1
        allowOptionalExpansion: false
        budget:
          maxVoxelsPerAsset: 4096
          maxPartsPerAsset: 16
        """);
    File.WriteAllText(
        Path.Combine(root, "policy", "coverage.yaml"),
        """
        version: 1
        foundation: ReferenceFixture
        requirements: []
        """);
    CopyFile(
        sourceRoot,
        root,
        "policy/families/humanoid.medium.v1.yaml");
    CopyFile(
        sourceRoot,
        root,
        "toolchain/hero-voxel.lock.yaml");

    string styleRoot = Path.Combine(root, "Art", "Style", "low-resolution-voxel.v1");
    Directory.CreateDirectory(styleRoot);
    File.WriteAllText(
        Path.Combine(styleRoot, "style.yaml"),
        """
        version: 1
        id: low-resolution-voxel.v1
        references:
          - reference-01.png
          - reference-02.png
          - reference-03.png
        traits:
          - chunky low-resolution voxel forms
          - chibi proportion language
        """);
    WritePngHeader(Path.Combine(styleRoot, "reference-01.png"), 1200, 900);
    WritePngHeader(Path.Combine(styleRoot, "reference-02.png"), 1200, 900);
    WritePngHeader(Path.Combine(styleRoot, "reference-03.png"), 1200, 900);

    string assetRoot = Path.Combine(
        root, "Art", "CombatFoundation", "Units", "Avatars", "FixtureAvatar");
    Directory.CreateDirectory(assetRoot);
    File.WriteAllText(
        Path.Combine(assetRoot, "asset.yaml"),
        """
        version: 1
        id: fixture.avatar
        kind: avatar
        unitRef: fixture/avatar
        family: humanoid.medium.v1
        source:
          base: ./Source/FixtureAvatar
          palette: ../../../Palette.yaml
        style:
          family: combat-foundation.v1
        """);
    string referenceRoot = Path.Combine(assetRoot, "Reference");
    Directory.CreateDirectory(referenceRoot);
    File.WriteAllText(Path.Combine(referenceRoot, "reference.yaml"), referenceYaml);
    return root;
}

static GateResult ReferenceGate(StoreSnapshot snapshot) =>
    snapshot.Assets.Single().Gates.Single(x => x.Id == "reference");

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
        string styleDir = Path.Combine(styleRoot, "Art", "Style", "low-resolution-voxel.v1");
        ReferenceStyleAuthority style = new StoreEngine(styleRoot).ResolveReferenceStyle();
        Require(style.Id == "low-resolution-voxel.v1", "style id must resolve");
        Require(style.ReferencePaths.Count == 3, "style must resolve exactly three PNG references");

        string styleManifest = Path.Combine(styleDir, "style.yaml");
        string originalStyle = File.ReadAllText(styleManifest);
        File.WriteAllText(
            styleManifest,
            originalStyle.Replace(
                "  - reference-03.png",
                "  - reference-02.png",
                StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(styleRoot).ResolveReferenceStyle(),
            "non-empty and unique");
        File.WriteAllText(styleManifest, originalStyle);

        File.WriteAllText(
            styleManifest,
            originalStyle.Replace(
                "reference-03.png",
                "reference-03.svg",
                StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(styleRoot).ResolveReferenceStyle(),
            "must be PNG");
        File.WriteAllText(styleManifest, originalStyle);

        File.Delete(Path.Combine(styleDir, "reference-03.png"));
        ExpectFailure(
            () => new StoreEngine(styleRoot).ResolveReferenceStyle(),
            "Required PNG is missing");
        WritePngHeader(Path.Combine(styleDir, "reference-03.png"), 1200, 900);

        string policyPath = Path.Combine(styleRoot, "policy", "store.yaml");
        string originalPolicy = File.ReadAllText(policyPath);
        File.WriteAllText(
            policyPath,
            originalPolicy.Replace(
                "referenceStyle: low-resolution-voxel.v1",
                "referenceStyle:",
                StringComparison.Ordinal));
        ExpectFailure(
            () => new StoreEngine(styleRoot).ResolveReferenceStyle(),
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
                "Art",
                "CombatFoundation",
                "Units",
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
            "Art",
            "CombatFoundation",
            "Units",
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
            audit.RootElement.GetProperty("StyleSha256").EnumerateObject().Count() == 4,
            "reference audit must hash style manifest plus three style PNGs");

        string stylePng = Path.Combine(
            validRoot,
            "Art",
            "Style",
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
                    "style evidence is stale",
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
            "Art",
            "Style",
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
                    "style evidence is stale",
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

string repositoryRoot = StoreEngine.FindRoot(AppContext.BaseDirectory);
if (args.Contains("--reference-authority-only", StringComparer.Ordinal))
{
    RunReferenceAuthorityContractTests(repositoryRoot);
    Console.WriteLine("PASS: reference-style and ReferenceContract v2 authority tests");
    return;
}

var engine = new StoreEngine(repositoryRoot);

// Provider resolution and public Hero.Voxel integration.
HeroVoxelProvider.Resolution resolution = engine.ResolveProvider();
Require(
    File.Exists(Path.Combine(
        resolution.Root,
        resolution.Lock.Workflow.Replace('/', Path.DirectorySeparatorChar))),
    "provider workflow did not resolve");

string? originalProviderRoot = Environment.GetEnvironmentVariable("HERO_STUDIO_ROOT");
try
{
    Environment.SetEnvironmentVariable("HERO_STUDIO_ROOT", null);
    ExpectFailure(
        () => new HeroVoxelProvider(repositoryRoot).Resolve(),
        "HERO_STUDIO_ROOT");
}
finally
{
    Environment.SetEnvironmentVariable("HERO_STUDIO_ROOT", originalProviderRoot);
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
        () => new HeroVoxelProvider(mismatchRoot).Resolve(),
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
        () => new HeroVoxelProvider(absoluteLockRoot).Resolve(),
        "portable relative paths");
}
finally
{
    Directory.Delete(absoluteLockRoot, true);
}

// Store YAML is strict: gameplay and pair-correction fields are not accepted.
ExpectFailure(
    () => ContractYaml.Parse<AssetManifest>(
        """
        version: 1
        id: invalid.gameplay
        kind: equipment
        unitRef: invalid/gameplay
        damage: 10
        mount:
          profile: humanoid.medium.v1
          socket: Hand.R
        source:
          base: ./Source/Item
          palette: ./Palette.yaml
        style:
          family: combat-foundation.v1
        """),
    "damage");

ExpectFailure(
    () => ContractYaml.Parse<AssetManifest>(
        """
        version: 1
        id: invalid.correction
        kind: equipment
        unitRef: invalid/correction
        mount:
          profile: humanoid.medium.v1
          socket: Hand.R
          correction: [1, 2, 3]
        source:
          base: ./Source/Item
          palette: ./Palette.yaml
        style:
          family: combat-foundation.v1
        """),
    "correction");

// Repository containment and identity uniqueness.
string invalidPathRoot = TempRoot();
try
{
    string assetDir = Path.Combine(invalidPathRoot, "Art", "Bad");
    Directory.CreateDirectory(assetDir);
    File.WriteAllText(
        Path.Combine(assetDir, "asset.yaml"),
        """
        version: 1
        id: bad.path
        kind: avatar
        unitRef: bad/path
        family: humanoid.medium.v1
        source:
          base: ../../../../outside
          palette: ./Palette.yaml
        style:
          family: combat-foundation.v1
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
    string assetDir = Path.Combine(invalidKindRoot, "Art", "BadKind");
    Directory.CreateDirectory(assetDir);
    File.WriteAllText(
        Path.Combine(assetDir, "asset.yaml"),
        """
        version: 1
        id: bad.kind
        kind: weapon
        unitRef: bad/kind
        source:
          base: ./Source/Item
          palette: ./Palette.yaml
        style:
          family: combat-foundation.v1
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
        string assetDir = Path.Combine(duplicateRoot, "Art", "A" + i);
        Directory.CreateDirectory(assetDir);
        File.WriteAllText(
            Path.Combine(assetDir, "asset.yaml"),
            $"""
             version: 1
             id: duplicate.asset
             kind: avatar
             unitRef: duplicate/unit-{i}
             family: humanoid.medium.v1
             source:
               base: ./Source/Item
               palette: ./Palette.yaml
             style:
               family: combat-foundation.v1
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
        string assetDir = Path.Combine(duplicateUnitRefRoot, "Art", "U" + i);
        Directory.CreateDirectory(assetDir);
        File.WriteAllText(
            Path.Combine(assetDir, "asset.yaml"),
            $"""
             version: 1
             id: duplicate.unitref-{i}
             kind: avatar
             unitRef: duplicate/shared-unit
             family: humanoid.medium.v1
             source:
               base: ./Source/Item
               palette: ./Palette.yaml
             style:
               family: combat-foundation.v1
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
    string originalDir = Path.Combine(identityHistoryRoot, "Art", "Original");
    Directory.CreateDirectory(originalDir);
    string originalManifest = Path.Combine(originalDir, "asset.yaml");
    const string originalIdentity =
        """
        version: 1
        id: history.original
        kind: avatar
        unitRef: history/original
        family: humanoid.medium.v1
        source:
          base: ./Source/Item
          palette: ./Palette.yaml
        style:
          family: combat-foundation.v1
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
    string newDir = Path.Combine(identityHistoryRoot, "Art", "New");
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

string profileHistoryRoot = TempRoot();
try
{
    CopyFile(repositoryRoot, profileHistoryRoot, "policy/store.yaml");
    Directory.CreateDirectory(Path.Combine(profileHistoryRoot, "policy"));
    File.WriteAllText(
        Path.Combine(profileHistoryRoot, "policy", "coverage.yaml"),
        """
        version: 1
        foundation: history
        requirements: []
        """);
    CopyFile(
        repositoryRoot,
        profileHistoryRoot,
        "policy/families/humanoid.medium.v1.yaml");
    RunGit(profileHistoryRoot, "init");
    RunGit(profileHistoryRoot, "config", "user.email", "test@example.invalid");
    RunGit(profileHistoryRoot, "config", "user.name", "Voxel Store Test");
    RunGit(profileHistoryRoot, "add", ".");
    RunGit(profileHistoryRoot, "commit", "-m", "baseline profile");

    string profilePath = Path.Combine(
        profileHistoryRoot,
        "policy",
        "families",
        "humanoid.medium.v1.yaml");
    string originalProfile = File.ReadAllText(profilePath);
    File.WriteAllText(
        profilePath,
        originalProfile.Replace(
            "envelope: [4, 4, 12]",
            "envelope: [5, 4, 12]",
            StringComparison.Ordinal));
    ExpectFailure(
        () => new StoreEngine(profileHistoryRoot).Scan(false, false),
        "changed in-place");

    File.WriteAllText(profilePath, originalProfile);
    File.WriteAllText(
        Path.Combine(
            profileHistoryRoot,
            "policy",
            "families",
            "humanoid.medium.v2.yaml"),
        originalProfile.Replace(
            "id: humanoid.medium.v1",
            "id: humanoid.medium.v2",
            StringComparison.Ordinal));
    new StoreEngine(profileHistoryRoot).Scan(false, false);
}
finally
{
    DeleteTree(profileHistoryRoot);
}

IReadOnlyList<AssetRecord> assets = engine.DiscoverAssets();
Require(
    assets.Count == 4,
    "CombatFoundation.v1 must contain exactly four V1 assets");
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
    engine.Scan(writeDerived: true, writeCompatibility: true);
StoreSnapshot second =
    engine.Scan(writeDerived: true, writeCompatibility: true);
Require(first.Healthy && second.Healthy, "completed V1 store must be healthy");
Require(
    first.CoverageGaps.Count == 0,
    "completed V1 store must have no required coverage gaps");
Require(
    first.NextJob == null && second.NextJob == null,
    "healthy store must plan no creative job");
Require(
    first.Assets.Count == second.Assets.Count,
    "scan must be deterministic");
Require(
    first.Compatibility.Count == 6 &&
    first.Compatibility.All(x => x.Passed),
    "three equipment pairs across two required pose samples must pass");

string compatibilityEvidence = Path.Combine(
    assets.Single(x => x.Manifest.Kind == "avatar").ProofDirectory,
    "Compatibility",
    "foundation.equipment.blade.right-01.json");
using (JsonDocument evidence =
       JsonDocument.Parse(File.ReadAllText(compatibilityEvidence)))
{
    JsonElement samples = evidence.RootElement.GetProperty("samples");
    Require(
        samples.GetArrayLength() == 2,
        "compatibility evidence must contain required pose samples");
    JsonElement frame = samples[0].GetProperty("frame");
    Require(
        frame.GetProperty("origin").GetArrayLength() == 3 &&
        frame.GetProperty("worldX").GetArrayLength() == 3 &&
        frame.GetProperty("worldY").GetArrayLength() == 3 &&
        frame.GetProperty("worldZ").GetArrayLength() == 3,
        "compatibility evidence must persist the animated socket world frame");
}

// Derived catalog/health can be deleted and regenerated byte-identically.
string catalogPath =
    Path.Combine(repositoryRoot, "catalog", "catalog.generated.json");
string healthPath =
    Path.Combine(repositoryRoot, "catalog", "health.generated.json");
string catalogBefore = File.ReadAllText(catalogPath);
string healthBefore = File.ReadAllText(healthPath);
File.Delete(catalogPath);
File.Delete(healthPath);
engine.Scan(writeDerived: true, writeCompatibility: true);
Require(
    File.ReadAllText(catalogPath) == catalogBefore,
    "catalog regeneration must be byte deterministic");
Require(
    File.ReadAllText(healthPath) == healthBefore,
    "health regeneration must be byte deterministic");

// A stale generated sibling is diagnosed before expansion work.
AssetRecord blade = assets.Single(
    x => x.Manifest.Id == "foundation.equipment.blade.right-01");
string bladeGenerated = blade.SourceBase + ".generated.vox";
byte[] originalGenerated = File.ReadAllBytes(bladeGenerated);
string coveragePath = Path.Combine(repositoryRoot, "policy", "coverage.yaml");
string originalCoverage = File.ReadAllText(coveragePath);
try
{
    File.WriteAllBytes(
        bladeGenerated,
        originalGenerated.Concat(new byte[] { 0x7f }).ToArray());
    File.WriteAllText(
        coveragePath,
        originalCoverage +
        """

          - id: simultaneous.coverage-gap
            kind: avatar
            profile: another.physical.family.v1
            required: true
        """);
    StoreSnapshot stale =
        engine.Scan(writeDerived: false, writeCompatibility: false);
    AssetHealth staleBlade =
        stale.Assets.Single(x => x.Id == blade.Manifest.Id);
    Require(
        staleBlade.FailedGate == "generation",
        "stale generated VOX must fail generation gate");
    Require(
        stale.NextJob?.Priority == "stale-reproducible" &&
        stale.NextJob.Target == blade.Manifest.Id,
        "stale generated VOX must be the failure-first maintenance job");
    Require(
        stale.CoverageGaps.Any(x => x.Id == "simultaneous.coverage-gap"),
        "priority test must contain a simultaneous coverage gap");
}
finally
{
    File.WriteAllBytes(bladeGenerated, originalGenerated);
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
        engine.Scan(writeDerived: false, writeCompatibility: false);
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
            engine.Scan(writeDerived: false, writeCompatibility: false);
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
        engine.Scan(writeDerived: false, writeCompatibility: false);
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

// Initial bootstrap with only the canonical avatar exposes exactly three equipment gaps.
string bootstrapRoot = TempRoot();
try
{
    foreach (string relative in new[]
             {
                 "policy/store.yaml",
                 "policy/coverage.yaml",
                 "policy/families/humanoid.medium.v1.yaml",
                 "toolchain/hero-voxel.lock.yaml",
                 "Art/CombatFoundation/Palette.yaml"
             })
    {
        CopyFile(repositoryRoot, bootstrapRoot, relative);
    }

    CopyDirectory(
        Path.Combine(
            repositoryRoot,
            "Art",
            "Style",
            "low-resolution-voxel.v1"),
        Path.Combine(
            bootstrapRoot,
            "Art",
            "Style",
            "low-resolution-voxel.v1"));

    CopyDirectory(
        Path.Combine(
            repositoryRoot,
            "Art",
            "CombatFoundation",
            "Units",
            "Avatars",
            "FoundationGuard01"),
        Path.Combine(
            bootstrapRoot,
            "Art",
            "CombatFoundation",
            "Units",
            "Avatars",
            "FoundationGuard01"));

    StoreSnapshot bootstrap =
        new StoreEngine(bootstrapRoot).Scan(false, false);
    Require(
        bootstrap.Assets.Count == 1 && bootstrap.Assets[0].Published,
        "bootstrap avatar must publish");
    Require(
        bootstrap.CoverageGaps.Count == 3 &&
        bootstrap.CoverageGaps.All(x => x.Kind == "equipment"),
        "avatar-only bootstrap must expose exactly three equipment mount gaps");
    Require(
        bootstrap.NextJob?.Priority == "required-coverage",
        "bootstrap must select required coverage after healthy avatar");
}
finally
{
    DeleteTree(bootstrapRoot);
}

string incompatibleRoot = TempRoot();
try
{
    CopyDirectory(
        Path.Combine(repositoryRoot, "policy"),
        Path.Combine(incompatibleRoot, "policy"));
    CopyDirectory(
        Path.Combine(repositoryRoot, "toolchain"),
        Path.Combine(incompatibleRoot, "toolchain"));
    CopyDirectory(
        Path.Combine(repositoryRoot, "Art"),
        Path.Combine(incompatibleRoot, "Art"));

    string profilePath = Path.Combine(
        incompatibleRoot,
        "policy",
        "families",
        "humanoid.medium.v1.yaml");
    string compatibleProfile = File.ReadAllText(profilePath);
    File.WriteAllText(
        profilePath,
        compatibleProfile.Replace(
            "voxelsPerUnit: 8",
            "voxelsPerUnit: 9",
            StringComparison.Ordinal));
    StoreSnapshot incompatible =
        new StoreEngine(incompatibleRoot).Scan(false, false);
    CompatibilityResult failedPair = incompatible.Compatibility.First(
        x => !x.Passed && !string.IsNullOrEmpty(x.Equipment));
    Require(
        failedPair.Avatar == "foundation.avatar.humanoid.guard-01" &&
        !string.IsNullOrWhiteSpace(failedPair.Equipment) &&
        failedPair.Profile == "humanoid.medium.v1" &&
        !string.IsNullOrWhiteSpace(failedPair.Mount) &&
        !string.IsNullOrWhiteSpace(failedPair.Pose) &&
        !string.IsNullOrWhiteSpace(failedPair.Evidence),
        "failed pair must identify avatar, equipment, profile, mount, pose, and evidence");
    Require(
        incompatible.Assets.Any(x => !x.Published),
        "physical incompatibility must prevent affected assets from publishing healthy");

    File.WriteAllText(
        profilePath,
        compatibleProfile.Replace(
            "optionalMounts: []",
            """
              - socket: Missing
                forward: [0, 0, 1]
                up: [0, 1, 0]
                envelope: [4, 4, 4]
            optionalMounts: []
            """,
            StringComparison.Ordinal));
    StoreSnapshot missingSocket =
        new StoreEngine(incompatibleRoot).Scan(false, false);
    Require(
        missingSocket.Compatibility.Any(
            x => !x.Passed &&
                 x.Equipment == "" &&
                 x.Mount == "Missing"),
        "missing required avatar socket must produce profile conformance failure");
    Require(
        !missingSocket.Assets.Single(
            x => x.Id == "foundation.avatar.humanoid.guard-01").Published,
        "avatar missing a required profile socket must not publish healthy");
}
finally
{
    DeleteTree(incompatibleRoot);
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
        "only the first returned job",
        StringComparison.OrdinalIgnoreCase) &&
    skill.Contains(
        "Never select a second asset",
        StringComparison.OrdinalIgnoreCase),
    "maintenance skill must enforce one claimed job per invocation");

Console.WriteLine(
    $"PASS: {assets.Count} assets, healthy={first.Healthy}, " +
    $"compatibility={first.Compatibility.Count}, gaps={first.CoverageGaps.Count}");
