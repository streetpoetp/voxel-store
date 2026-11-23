using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Hero.Voxel.Editor.Baking;
using Hero.Voxel.Editor.Preview.Scene;
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
        public int AnimationCount;
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


        internal static string SafeUnitDirectory(PreviewUnitContract unit)
        {
            string kind = string.Equals(unit.Kind, "avatar", StringComparison.Ordinal)
                ? "Avatars"
                : string.Equals(unit.Kind, "equipment", StringComparison.Ordinal)
                    ? "Equipment"
                    : throw new InvalidDataException("Unsupported preview unit kind: " + unit.Kind);
            string safeId = new string(unit.Id.Select(c =>
                char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_' ? c : '_').ToArray());
            return BakedRoot + "/" + kind + "/" + safeId;
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
                    string.IsNullOrWhiteSpace(unit.GeneratedVox))
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
            string storeRoot) =>
            "run --project " + Quote(toolProject) +
            " -- preview-plan --root " + Quote(storeRoot);

        internal static PreviewPlanContract Load()
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
                Arguments = BuildToolArguments(toolProject, storeRoot)
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
            string output = VoxelStorePreviewPaths.SafeUnitDirectory(unit);
            string safeId = Path.GetFileName(output);            return new PreviewBakePaths
            {
                SourceVox = VoxelStorePreviewPaths.ToAbsoluteStorePath(
                    unit.GeneratedVox, storeRoot),
                AnimationManifest = string.IsNullOrWhiteSpace(unit.GeneratedAnimation)
                    ? null
                    : VoxelStorePreviewPaths.ToAbsoluteStorePath(
                        unit.GeneratedAnimation, storeRoot),
                OutputDirectory = output,
                AssemblyAsset = output + "/" + safeId + ".assembly.asset",
                AnimationAsset = output + "/" + safeId + ".animation.asset"
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
                plan.Units.Select(VoxelStorePreviewPaths.SafeUnitDirectory),
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

        internal static void CleanupStale(PreviewPlanContract plan)
        {
            var existing = new List<string>();
            foreach (string kindRoot in new[]
                     {
                         VoxelStorePreviewPaths.BakedRoot + "/Avatars",
                         VoxelStorePreviewPaths.BakedRoot + "/Equipment"
                     })
            {
                string absoluteKindRoot = Path.Combine(
                    VoxelStorePreviewPaths.ProjectRoot,
                    kindRoot.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(absoluteKindRoot))
                    continue;

                foreach (string directory in Directory.GetDirectories(absoluteKindRoot))
                {
                    if (!LooksLikeGeneratedUnitDirectory(directory))
                        continue;

                    existing.Add(
                        "Assets/" + Path.GetRelativePath(
                            Application.dataPath,
                            directory).Replace('\\', '/'));
                }
            }
            foreach (string stale in PlanStaleDirectories(plan, existing))
                AssetDatabase.DeleteAsset(stale);
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

    internal static class PreviewGridBuilder
    {
        internal static void ConfigureGenerated(GameObject value)
        {
            value.tag = "EditorOnly";
            value.hideFlags = HideFlags.DontSaveInEditor;
        }

        internal static PreviewGridBuildResult Build(IReadOnlyList<BakedPreviewUnit> units)
        {
            EditorSceneManager.OpenScene(
                VoxelStorePreviewPaths.ScenePath,
                OpenSceneMode.Single);

            GameObject previous = GameObject.Find(VoxelStorePreviewPaths.GeneratedGridName);
            if (previous != null)
                UnityEngine.Object.DestroyImmediate(previous);

            var grid = new GameObject(VoxelStorePreviewPaths.GeneratedGridName);
            ConfigureGenerated(grid);

            var entries = new List<GameObject>(units.Count);
            var measured = new List<Bounds>(units.Count);
            int animationCount = 0;
            foreach (BakedPreviewUnit baked in units)
            {
                var entry = new GameObject(baked.Unit.Id);
                ConfigureGenerated(entry);
                entry.transform.SetParent(grid.transform, false);

                PreviewBakePaths paths = PreviewBake.CreatePaths(
                    baked.Unit,
                    VoxelStorePreviewPaths.StoreRoot);
                VoxelAssemblyAsset assembly =
                    AssetDatabase.LoadAssetAtPath<VoxelAssemblyAsset>(paths.AssemblyAsset);
                if (assembly == null)
                    throw new InvalidOperationException(
                        "Baked assembly could not be reloaded after scene open: " + baked.Unit.Id);

                VoxelAnimationAsset animation = null;
                if (!string.IsNullOrWhiteSpace(baked.Unit.GeneratedAnimation))
                    animation = AssetDatabase.LoadAssetAtPath<VoxelAnimationAsset>(
                        paths.AnimationAsset);

                GameObject debug = VoxelSceneDebugBuilder.Create(
                    assembly,
                    Vector3.zero);
                debug.transform.SetParent(entry.transform, true);
                VoxelSceneDebugController.Register(
                    debug,
                    assembly,
                    animation);
                if (animation != null)
                    animationCount++;
                Renderer[] renderers = debug.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                    throw new InvalidOperationException(
                        "Baked preview has no renderers: " + baked.Unit.Id);

                Bounds combined = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    combined.Encapsulate(renderers[i].bounds);
                entries.Add(entry);
                measured.Add(combined);
            }

            Vector3[] offsets = PreviewGridLayout.Calculate(
                measured,
                VoxelStorePreviewPaths.GridMargin);
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].transform.position += offsets[i];
                entries[i].transform.localScale = Vector3.one;
            }

            var finalBounds = new Bounds[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                Renderer[] renderers = entries[i].GetComponentsInChildren<Renderer>(true);
                Bounds combined = renderers[0].bounds;
                for (int j = 1; j < renderers.Length; j++)
                    combined.Encapsulate(renderers[j].bounds);
                finalBounds[i] = combined;
            }

            Selection.activeGameObject = grid;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected(false);
            SceneView.RepaintAll();

            return new PreviewGridBuildResult
            {
                Grid = grid,
                EntryIds = units.Select(x => x.Unit.Id).ToArray(),
                WorldBounds = finalBounds,
                AnimationCount = animationCount
            };
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

        public static void RefreshAllAndOpenGrid()
        {
            try
            {
                PreviewGridBuildResult result = PreviewRefreshOrchestrator.Run(
                    PreviewPlanClient.Load,
                    PreviewBake.BakeAll,
                    PreviewGridBuilder.Build);
                Debug.Log(
                    $"Voxel Store preview grid refreshed: {result.EntryIds.Length} published units.");
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

        public static void RefreshAllAndOpenGridForBatch()
        {
            PreviewPlanContract plan = PreviewPlanClient.Load();
            List<BakedPreviewUnit> baked = PreviewBake.BakeAll(plan);
            PreviewGridBuildResult result = PreviewGridBuilder.Build(baked);

            string reportDir = Path.Combine(
                VoxelStorePreviewPaths.ProjectRoot,
                "Library",
                "VoxelStorePreview");
            Directory.CreateDirectory(reportDir);
            var report = new PreviewGridReport
            {
                PlanCount = plan.Units.Length,
                GridCount = result.EntryIds.Length,
                UnitIds = result.EntryIds,
                AnimationCount = result.AnimationCount,
                OverlapCount = PreviewGridLayout.CountXZOverlaps(result.WorldBounds),
                NonUnitScaleCount = result.Grid.transform
                    .Cast<Transform>()
                    .Count(x => x.localScale != Vector3.one)
            };
            File.WriteAllText(
                Path.Combine(reportDir, "grid-report.json"),
                JsonUtility.ToJson(report, true));

            if (report.PlanCount != report.GridCount)
                throw new InvalidOperationException(
                    $"Preview grid count mismatch. plan={report.PlanCount} grid={report.GridCount}");
            if (!plan.Units.Select(x => x.Id).SequenceEqual(
                    result.EntryIds,
                    StringComparer.Ordinal))
                throw new InvalidOperationException(
                    "Preview grid identity/order differs from preview-plan.");
            if (report.OverlapCount != 0)
                throw new InvalidOperationException(
                    "Preview grid contains overlapping unit bounds: " + report.OverlapCount);
            if (report.NonUnitScaleCount != 0)
                throw new InvalidOperationException(
                    "Preview grid normalized one or more unit scales.");
        }

        public static void CaptureCurrentGridSceneViewForBatch()
        {
            PreviewPlanContract plan = PreviewPlanClient.Load();
            List<BakedPreviewUnit> baked = PreviewBake.BakeAll(plan);
            PreviewGridBuildResult result = PreviewGridBuilder.Build(baked);

            string reportDir = Path.Combine(
                VoxelStorePreviewPaths.ProjectRoot,
                "Library",
                "VoxelStorePreview");
            Directory.CreateDirectory(reportDir);
            CaptureSceneView(
                result,
                Path.Combine(reportDir, "grid-scene-view.png"));
        }

        private static void CaptureSceneView(
            PreviewGridBuildResult result,
            string outputPath)
        {
            if (result.WorldBounds == null || result.WorldBounds.Length == 0)
                throw new InvalidOperationException(
                    "Cannot capture an empty preview grid.");

            Bounds all = result.WorldBounds[0];
            for (int i = 1; i < result.WorldBounds.Length; i++)
                all.Encapsulate(result.WorldBounds[i]);

            CaptureSceneView(all, outputPath);
        }

        private static void CaptureSceneView(
            Bounds all,
            string outputPath)
        {
            CaptureSceneView(
                all,
                outputPath,
                Quaternion.Euler(35f, 45f, 0f));
        }

        private static void CaptureSceneView(
            Bounds all,
            string outputPath,
            Quaternion viewRotation)
        {
            SceneView sceneView = EditorWindow.GetWindow<SceneView>();
            sceneView.position = new Rect(40f, 40f, 1600f, 1000f);
            sceneView.pivot = all.center;
            sceneView.rotation = viewRotation;
            sceneView.orthographic = true;
            sceneView.size = Mathf.Max(
                all.size.y,
                Mathf.Max(all.size.x, all.size.z) * 0.72f);

            Camera camera = sceneView.camera;
            if (camera == null)
                throw new InvalidOperationException(
                    "SceneView camera is unavailable for preview proof capture.");

            float distance = Mathf.Max(20f, all.extents.magnitude * 3f);
            camera.transform.rotation = viewRotation;
            camera.transform.position =
                all.center - camera.transform.forward * distance;
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(
                all.extents.y + 1f,
                Mathf.Max(all.extents.x / 1.6f, all.extents.z) * 1.12f);
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = distance * 3f + all.extents.magnitude;
            camera.cullingMask = ~0;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.14f, 0.16f, 1f);
            const int width = 1600;
            const int height = 1000;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                RenderTexture.active = target;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply(false, false);
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(texture);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        public static void CreateShellSceneForBatch()
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Preview"));
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, VoxelStorePreviewPaths.ScenePath);
        }

        [Serializable]
        private sealed class PreviewGridReport
        {
            public int PlanCount;
            public int GridCount;
            public string[] UnitIds;
            public int AnimationCount;
            public int OverlapCount;
            public int NonUnitScaleCount;
        }
    }
}
