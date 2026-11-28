using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Hero.Voxel.Editor.Baking;
using Hero.Voxel.Actor.Runtime.Unity;
using Hero.Voxel.Render.Runtime.Unity;
using Hero.Voxel.Runtime.Assets;
using Hero.Voxel.Runtime.Data.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace VoxelStore.Preview.Editor
{
    [Serializable]
    internal sealed class PreviewPlanContract
    {
        public PreviewUnitContract[] Units = Array.Empty<PreviewUnitContract>();
    }

    [Serializable]
    internal sealed class PreviewUnitContract
    {
        public string Id;
        public string Kind;
        public string UnitRef;
        public string SourceBase;
        public string Palette;
        public string GeneratedVox;
        public string GeneratedAnimation;
        public string AnimationProfile;
        public PreviewClipContract[] RequiredClips =
            Array.Empty<PreviewClipContract>();
        public bool Candidate;
    }

    [Serializable]
    internal sealed class PreviewClipContract
    {
        public string Name;
        public bool Loop;
    }

    internal sealed class BakedPreviewUnit
    {
        public PreviewUnitContract Unit;
        public VoxelAssemblyAsset Assembly;
        public VoxelAnimationAsset Animation;
    }

    internal sealed class PreviewBakePaths
    {
        public string SourceVox;
        public string AnimationManifest;
        public string OutputDirectory;
        public string AssemblyAsset;
        public string AnimationAsset;
    }

    internal sealed class PreviewGridBuildResult
    {
        public GameObject Grid;
        public string[] EntryIds = Array.Empty<string>();
        public Bounds[] WorldBounds = Array.Empty<Bounds>();
        public VoxelRenderWorld World;
        public VoxelRenderInstance[] Instances = Array.Empty<VoxelRenderInstance>();
        public VoxelModelAnimationTrack[] Tracks = Array.Empty<VoxelModelAnimationTrack>();
        public int AnimationCount;
        public int CarriedEquipmentCount;
        public int SubmittedPartCount;
    }

    internal static class VoxelStorePreviewPaths
    {
        internal const string BakedRoot = "Assets/__VoxelStorePreview/Baked";
        internal const string ScenePath = "Assets/Preview/VoxelStorePreview.unity";
        internal const string GeneratedGridName = "[Voxel Store Preview Grid]";
        internal const float GridMargin = 1.0f;

        internal static string ProjectRoot =>
            Directory.GetParent(Application.dataPath)?.FullName ??
            throw new InvalidOperationException("Unable to resolve Unity project root.");

        internal static string StoreRoot
        {
            get
            {
                string current = ProjectRoot;
                while (true)
                {
                    if (File.Exists(Path.Combine(current, "Store", "policy.yaml")))
                        return current;
                    DirectoryInfo parent = Directory.GetParent(current);
                    if (parent == null)
                        throw new DirectoryNotFoundException(
                            "Could not locate voxel-store root from Unity preview project.");
                    current = parent.FullName;
                }
            }
        }


        internal static string BakedModelDirectory(
            PreviewUnitContract unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            string expectedCatalog =
                string.Equals(
                    unit.Kind,
                    "avatar",
                    StringComparison.Ordinal)
                    ? "Avatars"
                    : string.Equals(
                        unit.Kind,
                        "equipment",
                        StringComparison.Ordinal)
                        ? "Equipment"
                        : throw new InvalidDataException(
                            "Unsupported preview unit kind: " +
                            unit.Kind);

            if (string.IsNullOrWhiteSpace(unit.SourceBase))
            {
                throw new InvalidDataException(
                    "Preview unit SourceBase is required.");
            }

            string normalized = unit.SourceBase
                .Replace('\\', '/')
                .Trim('/');
            string[] segments = normalized.Split(
                new[] { '/' },
                StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length != 5 ||
                !string.Equals(
                    segments[0],
                    "Assets",
                    StringComparison.Ordinal) ||
                !string.Equals(
                    segments[1],
                    expectedCatalog,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    segments[3],
                    "Source",
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Preview SourceBase must match " +
                    $"Assets/{expectedCatalog}/<Model>/Source/<Model>: " +
                    unit.SourceBase);
            }

            string modelName = segments[4];
            string modelDirectory = segments[2];
            if (!string.Equals(
                    modelName,
                    modelDirectory,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Preview SourceBase model folder and source name " +
                    "must match: " +
                    unit.SourceBase);
            }

            return BakedRoot +
                   "/" +
                   expectedCatalog +
                   "/" +
                   modelName;
        }

        internal static string ToAbsoluteStorePath(string relative, string storeRoot = null)
        {
            string root = storeRoot ?? StoreRoot;
            string combined = Path.GetFullPath(Path.Combine(
                root,
                relative.Replace('/', Path.DirectorySeparatorChar)));
            string prefix = Path.GetFullPath(root)
                                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                            + Path.DirectorySeparatorChar;
            if (!combined.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Preview source escaped voxel-store root: " + relative);
            return combined;
        }


        internal static string RequireDerivedAssetPath(string assetPath)
        {
            string normalized = assetPath.Replace('\\', '/').TrimEnd('/');
            string root = BakedRoot + "/";
            if (!normalized.StartsWith(root, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Refusing preview mutation outside derived baked root: " + assetPath);
            if (normalized.Contains("/../", StringComparison.Ordinal) ||
                normalized.EndsWith("/..", StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Refusing preview path traversal: " + assetPath);            return normalized;
        }
    }

    internal static class PreviewPlanClient
    {
        internal static PreviewPlanContract Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidDataException("preview-plan returned empty output.");

            PreviewPlanContract plan;
            try
            {
                plan = JsonUtility.FromJson<PreviewPlanContract>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("preview-plan returned invalid JSON.", ex);
            }

            if (plan == null || plan.Units == null)
                throw new InvalidDataException("preview-plan is missing Units.");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (PreviewUnitContract unit in plan.Units)
            {
                if (unit == null ||
                    string.IsNullOrWhiteSpace(unit.Id) ||
                    string.IsNullOrWhiteSpace(unit.Kind) ||
                    string.IsNullOrWhiteSpace(unit.UnitRef) ||
                    string.IsNullOrWhiteSpace(unit.SourceBase) ||
                    string.IsNullOrWhiteSpace(unit.Palette) ||
                    string.IsNullOrWhiteSpace(unit.GeneratedVox) ||
                    string.IsNullOrWhiteSpace(unit.AnimationProfile) ||
                    unit.RequiredClips == null ||
                    unit.RequiredClips.Length == 0 ||
                    unit.RequiredClips.Any(clip =>
                        clip == null ||
                        string.IsNullOrWhiteSpace(clip.Name)))
                    throw new InvalidDataException("preview-plan contains an incomplete unit.");
                if (!ids.Add(unit.Id))
                    throw new InvalidDataException("preview-plan contains duplicate asset id: " + unit.Id);
            }

            return plan;
        }

        internal static PreviewPlanContract CompleteProcess(
            int exitCode,
            string stdout,
            string stderr)
        {
            if (exitCode != 0)
                throw new InvalidOperationException(
                    "voxel-store preview-plan failed: " + (stderr ?? string.Empty).Trim());
            return Parse(stdout);
        }

        internal static string BuildToolArguments(
            string toolProject,
            string storeRoot,
            IEnumerable<string> candidates = null)
        {
            string arguments =
                "run --project " + Quote(toolProject) +
                " -- preview-plan --root " + Quote(storeRoot);
            if (candidates != null)
                foreach (string candidate in candidates)
                    if (!string.IsNullOrWhiteSpace(candidate))
                        arguments +=
                            " --candidate " +
                            Quote(candidate);
            return arguments;
        }

        internal static PreviewPlanContract Load(
            IEnumerable<string> candidates = null)
        {
            string storeRoot = VoxelStorePreviewPaths.StoreRoot;
            string toolProject = Path.Combine(
                storeRoot, "Tool", "VoxelStore.Tool", "VoxelStore.Tool.csproj");

            var start = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = storeRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = BuildToolArguments(
                    toolProject,
                    storeRoot,
                    candidates)
            };

            using Process process = Process.Start(start) ??
                throw new InvalidOperationException("Failed to start voxel-store preview-plan.");
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return CompleteProcess(process.ExitCode, stdout, stderr);
        }
        private static string Quote(string value) =>
            "\"" + value.Replace("\"", "\\\"") + "\"";
    }

    internal static class PreviewBake
    {
        internal static PreviewBakePaths CreatePaths(
            PreviewUnitContract unit,
            string storeRoot)
        {
            string output =
                VoxelStorePreviewPaths.BakedModelDirectory(unit);
            string modelName = Path.GetFileName(
                unit.SourceBase
                    .Replace('\\', '/')
                    .TrimEnd('/'));
            return new PreviewBakePaths
            {
                SourceVox = VoxelStorePreviewPaths.ToAbsoluteStorePath(
                    unit.GeneratedVox, storeRoot),
                AnimationManifest = string.IsNullOrWhiteSpace(unit.GeneratedAnimation)
                    ? null
                    : VoxelStorePreviewPaths.ToAbsoluteStorePath(
                        unit.GeneratedAnimation, storeRoot),
                OutputDirectory = output,
                AssemblyAsset =
                    output + "/" + modelName + ".assembly.asset",
                AnimationAsset =
                    output + "/" + modelName + ".animation.asset"
            };
        }

        internal static List<BakedPreviewUnit> BakeAll(PreviewPlanContract plan) =>
            BakeAll(plan, CleanupStale, BakeUnit);

        internal static List<BakedPreviewUnit> BakeAll(
            PreviewPlanContract plan,
            Action<PreviewPlanContract> cleanup,
            Func<PreviewUnitContract, BakedPreviewUnit> bakeOne)
        {
            cleanup(plan);
            var baked = new List<BakedPreviewUnit>(plan.Units.Length);
            foreach (PreviewUnitContract unit in plan.Units)
            {
                try
                {
                    baked.Add(bakeOne(unit));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        "Preview bake failed for " + unit.Id + ": " + ex.Message,
                        ex);
                }
            }
            return baked;
        }

        internal static BakedPreviewUnit BakeUnit(PreviewUnitContract unit)
        {
            PreviewBakePaths paths = CreatePaths(unit, VoxelStorePreviewPaths.StoreRoot);
            if (!File.Exists(paths.SourceVox))
                throw new FileNotFoundException(
                    "Published generated VOX is missing.", paths.SourceVox);
            if (!string.IsNullOrWhiteSpace(paths.AnimationManifest) &&
                !File.Exists(paths.AnimationManifest))
                throw new FileNotFoundException(
                    "Published generated animation manifest is missing.",
                    paths.AnimationManifest);

            VoxelStorePreviewPaths.RequireDerivedAssetPath(paths.OutputDirectory);
            if (AssetDatabase.IsValidFolder(paths.OutputDirectory))
                AssetDatabase.DeleteAsset(paths.OutputDirectory);

            string absoluteOutput = Path.Combine(
                VoxelStorePreviewPaths.ProjectRoot,
                paths.OutputDirectory.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(absoluteOutput);
            AssetDatabase.Refresh();

            VoxelAssemblyAsset assembly = VoxelAssemblyBaker.Bake(
                paths.SourceVox,
                paths.AssemblyAsset,
                VoxelUnitId.FromRef(unit.UnitRef),
                unit.UnitRef);

            VoxelAnimationAsset animation = null;
            if (!string.IsNullOrWhiteSpace(paths.AnimationManifest))
            {
                animation = VoxelAnimationBaker.Bake(
                    paths.AnimationManifest,
                    assembly,
                    paths.AnimationAsset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return new BakedPreviewUnit
            {
                Unit = unit,
                Assembly = assembly,
                Animation = animation
            };
        }

        internal static string[] PlanStaleDirectories(
            PreviewPlanContract plan,
            IEnumerable<string> existingDirectories)
        {
            var desired = new HashSet<string>(
                plan.Units.Select(VoxelStorePreviewPaths.BakedModelDirectory),
                StringComparer.Ordinal);
            return existingDirectories
                .Select(VoxelStorePreviewPaths.RequireDerivedAssetPath)
                .Where(x => !desired.Contains(x))
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
        }

        internal static bool LooksLikeGeneratedUnitDirectory(
            string absoluteDirectory)
        {
            if (string.IsNullOrWhiteSpace(absoluteDirectory) ||
                !Directory.Exists(absoluteDirectory))
            {
                return false;
            }

            return Directory
                .GetFiles(
                    absoluteDirectory,
                    "*.assembly.asset",
                    SearchOption.TopDirectoryOnly)
                .Length > 0;
        }

        internal static void CleanupStale(
            PreviewPlanContract plan)
        {
            string absoluteBakedRoot = Path.Combine(
                VoxelStorePreviewPaths.ProjectRoot,
                VoxelStorePreviewPaths.BakedRoot.Replace(
                    '/',
                    Path.DirectorySeparatorChar));
            if (!Directory.Exists(absoluteBakedRoot))
                return;

            string[] existing = Directory
                .GetDirectories(
                    absoluteBakedRoot,
                    "*",
                    SearchOption.AllDirectories)
                .Where(LooksLikeGeneratedUnitDirectory)
                .Select(directory =>
                    "Assets/" + Path.GetRelativePath(
                        Application.dataPath,
                        directory).Replace('\\', '/'))
                .ToArray();

            foreach (string stale in
                     PlanStaleDirectories(plan, existing))
            {
                AssetDatabase.DeleteAsset(stale);
            }
        }
    }

    internal static class PreviewGridLayout
    {
        internal static int ColumnsForCount(int count) =>
            count <= 0 ? 0 : Mathf.CeilToInt(Mathf.Sqrt(count));

        internal static Vector3[] Calculate(IReadOnlyList<Bounds> bounds, float margin)
        {
            if (bounds == null || bounds.Count == 0)
                return Array.Empty<Vector3>();

            float cellWidth = bounds.Max(x => x.size.x) + margin;
            float cellDepth = bounds.Max(x => x.size.z) + margin;
            int columns = ColumnsForCount(bounds.Count);
            var positions = new Vector3[bounds.Count];

            for (int i = 0; i < bounds.Count; i++)
            {
                int row = i / columns;
                int column = i % columns;
                Bounds current = bounds[i];
                positions[i] = new Vector3(
                    column * cellWidth - current.center.x,
                    -current.min.y,
                    -row * cellDepth - current.center.z);
            }
            return positions;
        }

        internal static int CountXZOverlaps(IReadOnlyList<Bounds> bounds)
        {
            int overlaps = 0;
            for (int i = 0; i < bounds.Count; i++)
            for (int j = i + 1; j < bounds.Count; j++)
            {
                bool x = bounds[i].min.x < bounds[j].max.x &&
                         bounds[i].max.x > bounds[j].min.x;
                bool z = bounds[i].min.z < bounds[j].max.z &&
                         bounds[i].max.z > bounds[j].min.z;
                if (x && z)
                    overlaps++;
            }
            return overlaps;
        }
    }

    internal static class PreviewRefreshOrchestrator
    {
        internal static PreviewGridBuildResult Run(
            Func<PreviewPlanContract> load,
            Func<PreviewPlanContract, List<BakedPreviewUnit>> bake,
            Func<IReadOnlyList<BakedPreviewUnit>, PreviewGridBuildResult> build)
        {
            PreviewPlanContract plan = load();
            List<BakedPreviewUnit> baked = bake(plan);
            return build(baked);
        }
    }


    public static class VoxelStorePreviewCommand
    {

        [MenuItem("Voxel Store/Refresh Preview Grid")]
        public static void RefreshAllAndOpenGrid()
        {
            try
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    throw new InvalidOperationException(
                        "Exit Play Mode before refreshing the canonical Voxel Store preview grid.");
                }

                PreviewPlanContract plan = PreviewPlanClient.Load();
                List<BakedPreviewUnit> baked = PreviewBake.BakeAll(plan);
                PreviewVariantCoverage.ValidateAll(baked);
                if (baked.Count != plan.Units.Length)
                {
                    throw new InvalidOperationException(
                        "Preview bake count mismatch. plan=" +
                        plan.Units.Length +
                        " baked=" +
                        baked.Count);
                }

                PreviewGridPlayModeOrchestrator.PrepareAndEnterPlayMode();
                Debug.Log(
                    "Voxel Store preview bake complete; entering Play Mode for runtime grid inspection.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog(
                    "Voxel Store Preview Failed",
                    ex.Message,
                    "OK");
                throw;
            }
        }

        public static void BakeAllForBatch()
        {
            PreviewPlanContract plan =
                PreviewPlanClient.Load();
            List<BakedPreviewUnit> baked =
                PreviewBake.BakeAll(plan);
            PreviewVariantCoverage.ValidateAll(baked);
            if (baked.Count != plan.Units.Length)
            {
                throw new InvalidOperationException(
                    "Preview bake count mismatch. plan=" +
                    plan.Units.Length +
                    " baked=" +
                    baked.Count);
            }

            Debug.Log(
                "Voxel Store preview bake ready: " +
                baked.Count +
                " published units.");
        }

        internal static List<BakedPreviewUnit> BakeCandidatesForBatch(
            IEnumerable<string> candidateIds)
        {
            PreviewPlanContract plan =
                PreviewPlanClient.Load(candidateIds);
            List<BakedPreviewUnit> baked =
                PreviewBake.BakeAll(plan);
            PreviewVariantCoverage.ValidateAll(baked);
            return baked;
        }

        public static void CreateShellSceneForBatch()
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Preview"));
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, VoxelStorePreviewPaths.ScenePath);
        }

    }
}
