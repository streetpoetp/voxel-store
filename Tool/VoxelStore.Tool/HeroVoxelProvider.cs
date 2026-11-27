using System.Security.Cryptography;
using Hero.Voxel.Tool;

namespace VoxelStore.Tool;

public sealed class HeroVoxelProvider
{
    public sealed record Resolution(string Root, HeroVoxelLock Lock);
    public sealed record CompiledAsset(
        VoxelCompilation Compilation,
        string SpecPath,
        string PalettePath,
        string GeneratedVoxPath,
        string? GeneratedAnimationPath);

    private readonly string _repositoryRoot;
    private readonly string? _providerRootOverride;
    private Resolution? _resolution;

    public HeroVoxelProvider(string repositoryRoot, string? providerRoot = null)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _providerRootOverride = string.IsNullOrWhiteSpace(providerRoot)
            ? null
            : Path.GetFullPath(providerRoot);
    }
    public Resolution Resolve()
    {
        if (_resolution != null)
            return _resolution;

        string lockPath = Path.Combine(_repositoryRoot, "toolchain", "hero-voxel.lock.yaml");
        HeroVoxelLock lockFile = ContractYaml.Load<HeroVoxelLock>(lockPath);
        if (lockFile.Version != 1)
            throw new InvalidDataException("Unsupported Hero.Voxel lock version.");
        if (Path.IsPathRooted(lockFile.ToolProject) || Path.IsPathRooted(lockFile.Workflow))
            throw new InvalidDataException("Hero.Voxel lock paths must be portable relative paths.");

        string root = _providerRootOverride ??
            Path.Combine(_repositoryRoot, "Dependencies", "hero-studio");
        root = Path.GetFullPath(root);
        if (!Directory.Exists(root))
            throw new InvalidOperationException(
                "Hero Studio provider is missing at Dependencies/hero-studio.");
        string project = Path.Combine(root, Normalize(lockFile.ToolProject));
        string workflow = Path.Combine(root, Normalize(lockFile.Workflow));
        if (!File.Exists(project) || !File.Exists(workflow))
            throw new InvalidOperationException("Resolved Hero Studio root does not contain the pinned Hero.Voxel contract files.");

        string workflowHash = HashFile(workflow);
        if (!string.Equals(workflowHash, lockFile.WorkflowSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Hero.Voxel workflow fingerprint mismatch. expected={lockFile.WorkflowSha256} actual={workflowHash}");

        _resolution = new Resolution(root, lockFile);
        return _resolution;
    }

    public CompiledAsset Compile(AssetRecord asset, bool writeGenerated)
    {
        Resolve();
        string specPath = asset.SourceBase + ".voxel.yaml";
        if (!File.Exists(specPath))
            throw new FileNotFoundException("VoxelSpec source missing.", specPath);
        VoxelSpecDocument spec = VoxelSpecYaml.LoadSpec(specPath);
        PaletteSpec palette = VoxelSpecYaml.LoadPalette(asset.PalettePath);
        VoxelCompilation compilation = VoxelSpecCompiler.Compile(spec, palette);
        byte[] vox = MagicaVoxelWriter.WriteToBytes(compilation);
        string generatedVox = asset.SourceBase + ".generated.vox";
        string animationPath = asset.SourceBase + GeneratedAnimationManifestWriter.Suffix;
        string? generatedAnimation = compilation.Animations.Count == 0 ? null : animationPath;

        if (writeGenerated)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(generatedVox)!);
            File.WriteAllBytes(generatedVox, vox);
            if (generatedAnimation != null)
                File.WriteAllText(generatedAnimation, GeneratedAnimationManifestWriter.Serialize(compilation));
            else if (File.Exists(animationPath))
                File.Delete(animationPath);
        }

        return new CompiledAsset(compilation, specPath, asset.PalettePath, generatedVox, generatedAnimation);
    }

    public void GenerateProof(AssetRecord asset)
    {
        Resolve();
        string specPath = asset.SourceBase + ".voxel.yaml";
        string standalone = Path.Combine(asset.ProofDirectory, "Standalone");
        string parts = Path.Combine(asset.ProofDirectory, "Parts");
        ResetDirectory(standalone);
        ResetDirectory(parts);
        VoxelPreviewCommand.Run(specPath, asset.PalettePath, standalone, 256, 256);
        VoxelPreviewCommand.Run(specPath, asset.PalettePath, parts, 192, 192, partsOnly: true);
    }

    public static string HashFile(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        Directory.CreateDirectory(path);
    }

    private static string Normalize(string relative) =>
        relative.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

}
