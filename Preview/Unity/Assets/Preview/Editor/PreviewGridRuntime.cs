using System;
using System.Collections.Generic;
using System.Linq;
using Hero.Voxel.Actor.Editor;
using Hero.Voxel.Actor.Runtime.Unity;
using Hero.Voxel.Render.Runtime.Unity;
using Hero.Voxel.Runtime.Assets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace VoxelStore.Preview.Editor
{
    internal sealed class PreviewRuntimeGridSession : IDisposable
    {
        private readonly GameObject _runtimeRoot;
        private readonly VoxelRenderWorld _world;
        private readonly List<VoxelActorInstance> _actors;
        private readonly List<VoxelRenderInstance> _instances;
        private readonly List<GameObject> _inspectionHosts;

        internal PreviewRuntimeGridSession(
            GameObject runtimeRoot,
            VoxelRenderWorld world,
            List<VoxelActorInstance> actors,
            List<VoxelRenderInstance> instances,
            List<GameObject> inspectionHosts,
            PreviewGridBuildResult result)
        {
            _runtimeRoot = runtimeRoot;
            _world = world;
            _actors = actors;
            _instances = instances;
            _inspectionHosts = inspectionHosts;
            Result = result;
        }

        internal PreviewGridBuildResult Result { get; }

        public void Dispose()
        {
            foreach (GameObject host in _inspectionHosts)
                VoxelRuntimeInspectionController.Unregister(host);

            foreach (VoxelActorInstance actor in _actors)
                actor.Dispose();
            _actors.Clear();

            bool worldAlive = _world != null;
            if (worldAlive)
            {
                foreach (VoxelRenderInstance instance in _instances)
                    instance.Dispose();
            }
            _instances.Clear();

            if (worldAlive)
                _world.Clear();
            if (_runtimeRoot != null)
                UnityEngine.Object.DestroyImmediate(_runtimeRoot);
        }
    }

    internal sealed class PreviewRuntimeEntry
    {
        internal PreviewRuntimeEntry(
            GameObject host,
            VoxelRenderInstance instance,
            VoxelActorInstance actor)
        {
            Host = host;
            Instance = instance;
            Actor = actor;
        }

        internal GameObject Host { get; }
        internal VoxelRenderInstance Instance { get; }
        internal VoxelActorInstance Actor { get; }

        internal Bounds GetWorldBounds() =>
            Actor != null ? Actor.GetWorldBounds() : Instance.GetWorldBounds();

        internal void SetWorldPosition(Vector3 position)
        {
            if (Actor != null)
            {
                Actor.SetWorldTransform(
                    position,
                    Quaternion.identity,
                    Vector3.one);
            }
            else
            {
                Instance.SetWorldTransform(
                    position,
                    Quaternion.identity,
                    Vector3.one);
            }
        }
    }

    internal static class PreviewGridRuntimeBuilder
    {
        private static PreviewRuntimeGridSession _active;

        internal static PreviewGridBuildResult ActiveResult =>
            _active?.Result;

        internal static void ConfigureGenerated(GameObject value)
        {
            value.tag = "EditorOnly";
            value.hideFlags =
                HideFlags.DontSaveInEditor |
                HideFlags.DontSaveInBuild;
        }

        internal static PreviewGridBuildResult Build(
            PreviewPlanContract plan)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Voxel Store runtime preview grid can be realized only in Play Mode.");
            }
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));
            DisposeActive();

            var runtimeRoot =
                new GameObject("[Voxel Store Preview Runtime]");
            ConfigureGenerated(runtimeRoot);

            VoxelRenderWorld world =
                runtimeRoot.AddComponent<VoxelRenderWorld>();
            world.Initialize();

            var grid =
                new GameObject(VoxelStorePreviewPaths.GeneratedGridName);
            ConfigureGenerated(grid);
            grid.transform.SetParent(runtimeRoot.transform, false);

            var actors = new List<VoxelActorInstance>();
            var instances = new List<VoxelRenderInstance>();
            var inspectionHosts = new List<GameObject>();
            List<PreviewRuntimeEntry> entries = null;
            List<Bounds> measured = null;
            PreviewUnitContract[] equipmentUnits = null;
            Dictionary<string, int> avatarOrder = null;
            int animationCount = 0;
            int carriedEquipmentCount = 0;

            try
            {
                entries = new List<PreviewRuntimeEntry>(plan.Units.Length);
                measured = new List<Bounds>(plan.Units.Length);
                equipmentUnits = plan.Units
                    .Where(x => IsKind(x, "equipment"))
                    .OrderBy(ModelName, StringComparer.Ordinal)
                    .ToArray();
                avatarOrder = plan.Units
                    .Where(x => IsKind(x, "avatar"))
                    .OrderBy(ModelName, StringComparer.Ordinal)
                    .Select((x, i) => new { x.Id, Index = i })
                    .ToDictionary(x => x.Id, x => x.Index, StringComparer.Ordinal);

                foreach (PreviewUnitContract unit in plan.Units)
                {
                    BakedPreviewUnit baked = LoadBaked(unit);
                    var entry = new GameObject(unit.Id);
                    ConfigureGenerated(entry);
                    entry.transform.SetParent(grid.transform, false);
                    inspectionHosts.Add(entry);

                    if (baked.Animation != null)
                        animationCount++;

                    if (IsKind(unit, "avatar"))
                    {
                        VoxelRenderInstance rootInstance =
                            world.Spawn(baked.Assembly, baked.Animation);
                        instances.Add(rootInstance);
                        var actor = new VoxelActorInstance(rootInstance);
                        actors.Add(actor);
                        VoxelRuntimeInspectionController.Register(
                            entry,
                            rootInstance,
                            actor.RootTrack,
                            actor);

                        if (equipmentUnits.Length > 0)
                        {
                            int avatarIndex = avatarOrder[unit.Id];
                            int primaryIndex =
                                avatarIndex % equipmentUnits.Length;
                            AttachEquipment(
                                entry,
                                world,
                                actor,
                                equipmentUnits[primaryIndex],
                                "Equip.Hand.Right.0",
                                instances,
                                inspectionHosts);
                            carriedEquipmentCount++;

                            if (equipmentUnits.Length > 1)
                            {
                                int secondaryIndex =
                                    (avatarIndex +
                                     Math.Max(1, equipmentUnits.Length / 2)) %
                                    equipmentUnits.Length;
                                if (secondaryIndex == primaryIndex)
                                {
                                    secondaryIndex =
                                        (primaryIndex + 1) %
                                        equipmentUnits.Length;
                                }

                                AttachEquipment(
                                    entry,
                                    world,
                                    actor,
                                    equipmentUnits[secondaryIndex],
                                    "Equip.Hand.Left.0",
                                    instances,
                                    inspectionHosts);
                                carriedEquipmentCount++;
                            }
                        }

                        entries.Add(
                            new PreviewRuntimeEntry(
                                entry,
                                rootInstance,
                                actor));
                        measured.Add(actor.GetWorldBounds());
                    }
                    else if (IsKind(unit, "equipment"))
                    {
                        VoxelRenderInstance instance =
                            world.Spawn(baked.Assembly, baked.Animation);
                        instances.Add(instance);
                        var track =
                            new VoxelModelAnimationTrack(instance);
                        VoxelRuntimeInspectionController.Register(
                            entry,
                            instance,
                            track);
                        entries.Add(
                            new PreviewRuntimeEntry(
                                entry,
                                instance,
                                null));
                        measured.Add(instance.GetWorldBounds());
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Unsupported preview unit kind: " + unit.Kind);
                    }
                }

                Vector3[] offsets = PreviewGridLayout.Calculate(
                    measured,
                    VoxelStorePreviewPaths.GridMargin);
                for (int i = 0; i < entries.Count; i++)
                    entries[i].SetWorldPosition(offsets[i]);

                var finalBounds = new Bounds[entries.Count];
                for (int i = 0; i < entries.Count; i++)
                {
                    finalBounds[i] = entries[i].GetWorldBounds();
                    RequireFiniteBounds(finalBounds[i], entries[i].Host.name);
                }

                world.SubmitNow();
                if (plan.Units.Length > 0 && world.SubmittedPartCount <= 0)
                {
                    throw new InvalidOperationException(
                        "Voxel Store runtime preview submitted no voxel parts.");
                }

                var result = new PreviewGridBuildResult
                {
                    Grid = grid,
                    EntryIds = plan.Units.Select(x => x.Id).ToArray(),
                    WorldBounds = finalBounds,
                    AnimationCount = animationCount,
                    CarriedEquipmentCount = carriedEquipmentCount,
                    InspectionHosts = inspectionHosts.ToArray(),
                    SubmittedPartCount = world.SubmittedPartCount
                };

                _active = new PreviewRuntimeGridSession(
                    runtimeRoot,
                    world,
                    actors,
                    instances,
                    inspectionHosts,
                    result);

                Selection.activeGameObject = grid;
                FocusSceneView(finalBounds);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                foreach (GameObject host in inspectionHosts)
                    VoxelRuntimeInspectionController.Unregister(host);
                foreach (VoxelActorInstance actor in actors)
                    actor.Dispose();

                bool worldAlive = world != null;
                if (worldAlive)
                {
                    foreach (VoxelRenderInstance instance in instances)
                        instance.Dispose();
                    world.Clear();
                }

                if (runtimeRoot != null)
                    UnityEngine.Object.DestroyImmediate(runtimeRoot);
                throw;
            }
        }

        internal static void DisposeActive()
        {
            _active?.Dispose();
            _active = null;
        }

        private static BakedPreviewUnit LoadBaked(
            PreviewUnitContract unit)
        {
            PreviewBakePaths paths = PreviewBake.CreatePaths(
                unit,
                VoxelStorePreviewPaths.StoreRoot);
            VoxelAssemblyAsset assembly =
                AssetDatabase.LoadAssetAtPath<VoxelAssemblyAsset>(
                    paths.AssemblyAsset);
            if (assembly == null)
            {
                throw new InvalidOperationException(
                    "Baked assembly is unavailable for runtime preview: " +
                    unit.Id);
            }

            VoxelAnimationAsset animation = null;
            if (!string.IsNullOrWhiteSpace(unit.GeneratedAnimation))
            {
                animation =
                    AssetDatabase.LoadAssetAtPath<VoxelAnimationAsset>(
                        paths.AnimationAsset);
                if (animation == null)
                {
                    throw new InvalidOperationException(
                        "Baked animation is unavailable for runtime preview: " +
                        unit.Id);
                }
            }

            return new BakedPreviewUnit
            {
                Unit = unit,
                Assembly = assembly,
                Animation = animation
            };
        }

        private static void AttachEquipment(
            GameObject avatarHost,
            VoxelRenderWorld world,
            VoxelActorInstance actor,
            PreviewUnitContract equipmentUnit,
            string avatarSocketName,
            ICollection<VoxelRenderInstance> instances,
            ICollection<GameObject> inspectionHosts)
        {
            BakedPreviewUnit baked = LoadBaked(equipmentUnit);
            VoxelRenderInstance instance =
                world.Spawn(baked.Assembly, baked.Animation);
            instances.Add(instance);

            VoxelActorBindingId binding =
                actor.Attach(instance, avatarSocketName, "Mount");
            VoxelModelAnimationTrack track =
                actor.GetAttachmentTrack(binding);

            var host = new GameObject(
                equipmentUnit.Id + " @ " + avatarSocketName);
            ConfigureGenerated(host);
            host.transform.SetParent(avatarHost.transform, false);
            inspectionHosts.Add(host);

            VoxelRuntimeInspectionController.Register(
                host,
                instance,
                track,
                actor);
        }

        private static bool IsKind(
            PreviewUnitContract unit,
            string kind) =>
            unit != null &&
            string.Equals(
                unit.Kind,
                kind,
                StringComparison.Ordinal);

        private static string ModelName(PreviewUnitContract unit)
        {
            string sourceBase = unit?.SourceBase;
            if (string.IsNullOrWhiteSpace(sourceBase))
                return unit?.Id ?? string.Empty;

            string normalized =
                sourceBase.Replace('\\', '/').TrimEnd('/');
            int slash = normalized.LastIndexOf('/');
            return slash >= 0 && slash + 1 < normalized.Length
                ? normalized[(slash + 1)..]
                : normalized;
        }

        private static void RequireFiniteBounds(
            Bounds bounds,
            string id)
        {
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;
            if (!float.IsFinite(center.x) ||
                !float.IsFinite(center.y) ||
                !float.IsFinite(center.z) ||
                !float.IsFinite(size.x) ||
                !float.IsFinite(size.y) ||
                !float.IsFinite(size.z))
            {
                throw new InvalidOperationException(
                    "Runtime preview bounds are not finite: " + id);
            }
        }

        private static void FocusSceneView(
            IReadOnlyList<Bounds> bounds)
        {
            if (bounds == null ||
                bounds.Count == 0 ||
                SceneView.lastActiveSceneView == null)
            {
                return;
            }

            Bounds all = bounds[0];
            for (int i = 1; i < bounds.Count; i++)
                all.Encapsulate(bounds[i]);

            SceneView view = SceneView.lastActiveSceneView;
            view.pivot = all.center;
            view.rotation = Quaternion.Euler(35f, 45f, 0f);
            view.orthographic = true;
            view.size = Mathf.Max(
                all.size.y,
                Mathf.Max(all.size.x, all.size.z) * 0.72f);
            view.Repaint();
        }
    }

    [InitializeOnLoad]
    internal static class PreviewGridPlayModeOrchestrator
    {
        private const string PendingKey =
            "VoxelStore.Preview.RuntimeGrid.Pending";

        static PreviewGridPlayModeOrchestrator()
        {
            EditorApplication.playModeStateChanged -=
                OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged +=
                OnPlayModeStateChanged;

            if (EditorApplication.isPlaying &&
                SessionState.GetBool(PendingKey, false))
            {
                EditorApplication.delayCall +=
                    RealizePendingIfNeeded;
            }
        }

        internal static bool PendingForTesting =>
            SessionState.GetBool(PendingKey, false);

        internal static void PrepareAndEnterPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "Exit Play Mode before refreshing the canonical Voxel Store preview grid.");
            }

            EditorSceneManager.OpenScene(
                VoxelStorePreviewPaths.ScenePath,
                OpenSceneMode.Single);

            GameObject stale =
                GameObject.Find(VoxelStorePreviewPaths.GeneratedGridName);
            if (stale != null)
                UnityEngine.Object.DestroyImmediate(stale);

            SessionState.SetBool(PendingKey, true);
            EditorApplication.isPlaying = true;
        }

        internal static void SetPendingForTesting()
        {
            SessionState.SetBool(PendingKey, true);
        }

        internal static void ClearPendingForTesting()
        {
            SessionState.EraseBool(PendingKey);
        }

        internal static void RealizePendingForTesting()
        {
            RealizePendingIfNeeded();
        }

        private static void OnPlayModeStateChanged(
            PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.delayCall +=
                    RealizePendingIfNeeded;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode ||
                     state == PlayModeStateChange.EnteredEditMode)
            {
                PreviewGridRuntimeBuilder.DisposeActive();
                if (state == PlayModeStateChange.EnteredEditMode)
                    SessionState.EraseBool(PendingKey);
            }
        }

        private static void RealizePendingIfNeeded()
        {
            if (!EditorApplication.isPlaying ||
                !SessionState.GetBool(PendingKey, false))
            {
                return;
            }

            SessionState.EraseBool(PendingKey);
            try
            {
                PreviewPlanContract plan =
                    PreviewPlanClient.Load();
                PreviewGridBuildResult result =
                    PreviewGridRuntimeBuilder.Build(plan);
                Debug.Log(
                    "Voxel Store Play Mode preview grid ready: " +
                    result.EntryIds.Length +
                    " published units, " +
                    result.CarriedEquipmentCount +
                    " carried equipment.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.isPlaying = false;
            }
        }
    }
}