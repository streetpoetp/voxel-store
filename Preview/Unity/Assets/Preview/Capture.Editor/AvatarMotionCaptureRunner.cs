using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hero.Consumer.Editor;
using Hero.Voxel.Actor.Runtime.Unity;
using Hero.Voxel.Render.Runtime.Unity;
using Hero.Voxel.Runtime.Assets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoxelStore.Preview.Editor;

namespace VoxelStore.Preview.Capture.Editor
{
    [InitializeOnLoad]
    public static class AvatarMotionCaptureRunner
    {
        private const string RunKey =
            "VoxelStore.AvatarMotionCapture.Run";
        private const string StartedKey =
            "VoxelStore.AvatarMotionCapture.Started";
        private const string ResultKey =
            "VoxelStore.AvatarMotionCapture.Result";
        private const string ErrorKey =
            "VoxelStore.AvatarMotionCapture.Error";
        private const string PlanJsonKey =
            "VoxelStore.AvatarMotionCapture.PlanJson";
        private const string OutputDirectory =
            "Library/Capture/AvatarMotionGallery";
        private const string CandidateEnvironment =
            "VOXEL_STORE_PREVIEW_CANDIDATES";

        static AvatarMotionCaptureRunner()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        public static void RunFromCommandLine()
        {
            try
            {
                if (EditorApplication.isPlaying)
                    throw new InvalidOperationException(
                        "Animation content capture must start outside Play Mode.");

                string projectRoot =
                    Path.GetDirectoryName(Application.dataPath) ??
                    throw new InvalidOperationException(
                        "Preview project root could not be resolved.");
                string outputRoot = Path.Combine(
                    projectRoot,
                    OutputDirectory.Replace(
                        '/',
                        Path.DirectorySeparatorChar));
                if (Directory.Exists(outputRoot))
                    Directory.Delete(outputRoot, recursive: true);
                Directory.CreateDirectory(outputRoot);

                string[] candidateIds = ReadCandidateIds();
                PreviewPlanContract plan =
                    PreviewPlanClient.Load(candidateIds);
                List<BakedPreviewUnit> baked =
                    PreviewBake.BakeAll(plan);
                PreviewVariantCoverage.ValidateAll(baked);
                if (baked.Count != plan.Units.Length)
                    throw new InvalidOperationException(
                        "Animation proof bake count does not match the Tool plan.");
                if (plan.Units.Length == 0)
                    throw new InvalidOperationException(
                        "Animation proof plan contains no models.");

                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);
                SessionState.SetBool(RunKey, true);
                SessionState.SetBool(StartedKey, false);
                SessionState.SetString(ResultKey, string.Empty);
                SessionState.SetString(ErrorKey, string.Empty);
                SessionState.SetString(
                    PlanJsonKey,
                    JsonUtility.ToJson(plan));
                EditorApplication.isPlaying = true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static string[] ReadCandidateIds()
        {
            string raw =
                Environment.GetEnvironmentVariable(
                    CandidateEnvironment) ??
                string.Empty;
            return raw
                .Split(
                    new[] { ';' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length != 0)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
        }

        private static void Poll()
        {
            if (!SessionState.GetBool(RunKey, false))
                return;

            if (EditorApplication.isPlaying)
            {
                if (!SessionState.GetBool(
                        StartedKey,
                        false))
                {
                    SessionState.SetBool(
                        StartedKey,
                        true);
                    _ = CaptureAllAsync();
                }
                return;
            }

            string result =
                SessionState.GetString(
                    ResultKey,
                    string.Empty);
            if (string.IsNullOrWhiteSpace(result))
                return;

            string error =
                SessionState.GetString(
                    ErrorKey,
                    string.Empty);
            ClearSessionState();
            bool success = string.Equals(
                result,
                "success",
                StringComparison.Ordinal);
            if (!success &&
                !string.IsNullOrWhiteSpace(error))
                Debug.LogError(error);
            EditorApplication.Exit(success ? 0 : 1);
        }

        private static async Task CaptureAllAsync()
        {
            Exception failure = null;
            ProofScene scene = null;
            try
            {
                string planJson =
                    SessionState.GetString(
                        PlanJsonKey,
                        string.Empty);
                PreviewPlanContract plan =
                    PreviewPlanClient.Parse(planJson);
                scene = ProofScene.Create(plan);

                foreach (PreviewUnitContract unit in
                         plan.Units.OrderBy(
                             x => x.Id,
                             StringComparer.Ordinal))
                {
                    ProofSubject subject =
                        scene.RequireSubject(unit.Id);
                    foreach (PreviewClipContract clip in
                             unit.RequiredClips.OrderBy(
                                 x => x.Name,
                                 StringComparer.Ordinal))
                    {
                        CaptureStep[] steps =
                            BuildSteps(clip.Name, clip.Loop);
                        subject.BeginClip(clip.Name);
                        Bounds clipBounds = subject.GetWorldBounds();
                        for (int i = 0; i < steps.Length; i++)
                        {
                            CaptureStep measurement = steps[i];
                            subject.Sample(
                                clip.Name,
                                measurement.Phase,
                                measurement.MoveReference);
                            clipBounds.Encapsulate(
                                subject.GetWorldBounds());
                        }

                        // Re-run from a clean clip start while keeping one
                        // trajectory-wide frame. Re-centering each checkpoint
                        // would hide authored translation.
                        subject.BeginClip(clip.Name);
                        for (int i = 0; i < steps.Length; i++)
                        {
                            CaptureStep step = steps[i];
                            subject.Sample(
                                clip.Name,
                                step.Phase,
                                step.MoveReference);
                            subject.Validate(
                                unit.Id,
                                clip.Name,
                                step.Phase);
                            scene.World.SubmitNow();
                            scene.Focus(
                                clipBounds,
                                step.AlternateView);
                            await GameViewCapture.CaptureAsync(
                                OutputDirectory,
                                Checkpoint(
                                    unit.Id,
                                    clip.Name,
                                    step),
                                stableFrames: 1);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                failure = exception;
                Debug.LogException(exception);
            }
            finally
            {
                scene?.Dispose();
                SessionState.SetString(
                    ResultKey,
                    failure == null
                        ? "success"
                        : "failure");
                SessionState.SetString(
                    ErrorKey,
                    failure?.ToString() ??
                    string.Empty);
                EditorApplication.isPlaying = false;
            }
        }

        private static CaptureStep[] BuildSteps(
            string clipName,
            bool loop)
        {
            string family =
                clipName.Split('.')[0];
            if (loop)
            {
                return new[]
                {
                    new CaptureStep(0f, false, false, "p000"),
                    new CaptureStep(0.5f, false, false, "p500"),
                    new CaptureStep(1f, false, false, "p1000"),
                    new CaptureStep(0.5f, false, true, "p500-alt")
                };
            }

            bool movingReference =
                string.Equals(
                    family,
                    "Attack",
                    StringComparison.Ordinal) ||
                string.Equals(
                    family,
                    "Drop",
                    StringComparison.Ordinal);
            return new[]
            {
                new CaptureStep(0.15f, false, false, "p150"),
                new CaptureStep(0.55f, movingReference, false, "p550"),
                new CaptureStep(1f, movingReference, false, "p1000"),
                new CaptureStep(0.55f, movingReference, true, "p550-alt")
            };
        }

        private static string Checkpoint(
            string assetId,
            string clipName,
            CaptureStep step) =>
            assetId +
            "__" +
            clipName +
            "__" +
            step.Suffix;

        private readonly struct CaptureStep
        {
            public CaptureStep(
                float phase,
                bool moveReference,
                bool alternateView,
                string suffix)
            {
                Phase = phase;
                MoveReference = moveReference;
                AlternateView = alternateView;
                Suffix = suffix;
            }

            public float Phase { get; }
            public bool MoveReference { get; }
            public bool AlternateView { get; }
            public string Suffix { get; }
        }

        private sealed class ProofScene : IDisposable
        {
            private readonly GameObject _root;
            private readonly Camera _camera;
            private readonly Dictionary<string, ProofSubject>
                _subjects;

            private ProofScene(
                GameObject root,
                Camera camera,
                VoxelRenderWorld world,
                Dictionary<string, ProofSubject> subjects)
            {
                _root = root;
                _camera = camera;
                World = world;
                _subjects = subjects;
            }

            public VoxelRenderWorld World { get; }

            public static ProofScene Create(
                PreviewPlanContract plan)
            {
                PreviewCatalogModel[] discovered =
                    PreviewCatalogDiscovery.Discover();
                var byIdentity = discovered.ToDictionary(
                    model =>
                        model.Catalog +
                        "/" +
                        model.Name,
                    StringComparer.OrdinalIgnoreCase);
                PreviewUnitContract[] avatarUnits =
                    plan.Units
                        .Where(x =>
                            string.Equals(
                                x.Kind,
                                "avatar",
                                StringComparison.Ordinal))
                        .OrderBy(
                            x => x.Id,
                            StringComparer.Ordinal)
                        .ToArray();
                if (plan.Units.Any(x =>
                        string.Equals(
                            x.Kind,
                            "equipment",
                            StringComparison.Ordinal)) &&
                    avatarUnits.Length == 0)
                {
                    throw new InvalidOperationException(
                        "Equipment animation proof requires an explicitly planned avatar peer.");
                }

                var root =
                    new GameObject(
                        "[Voxel Store Animation Proof]");
                VoxelRenderWorld world =
                    root.AddComponent<VoxelRenderWorld>();
                world.Initialize();
                Camera camera = CreateCamera();
                CreateDirectionalLight();

                var subjects =
                    new Dictionary<string, ProofSubject>(
                        StringComparer.Ordinal);
                PreviewCatalogModel holderModel =
                    avatarUnits.Length == 0
                        ? null
                        : RequireModel(
                            avatarUnits[0],
                            byIdentity);
                foreach (PreviewUnitContract unit in
                         plan.Units.OrderBy(
                             x => x.Id,
                             StringComparer.Ordinal))
                {
                    PreviewCatalogModel model =
                        RequireModel(
                            unit,
                            byIdentity);
                    ProofSubject subject =
                        string.Equals(
                            unit.Kind,
                            "avatar",
                            StringComparison.Ordinal)
                            ? CreateAvatarSubject(
                                world,
                                unit,
                                model)
                            : CreateEquipmentSubject(
                                world,
                                unit,
                                model,
                                holderModel);
                    subject.SetVisible(false);
                    subjects.Add(unit.Id, subject);
                }

                world.SubmitNow();
                return new ProofScene(
                    root,
                    camera,
                    world,
                    subjects);
            }

            private static PreviewCatalogModel RequireModel(
                PreviewUnitContract unit,
                IReadOnlyDictionary<string, PreviewCatalogModel> byIdentity)
            {
                string catalog =
                    string.Equals(
                        unit.Kind,
                        "avatar",
                        StringComparison.Ordinal)
                        ? "Avatars"
                        : "Equipment";
                string modelName =
                    Path.GetFileName(
                        unit.SourceBase
                            .Replace('\\', '/'));
                string key =
                    catalog +
                    "/" +
                    modelName;
                if (!byIdentity.TryGetValue(
                        key,
                        out PreviewCatalogModel model))
                    throw new InvalidOperationException(
                        "Baked animation proof model is missing: " +
                        key);
                return model;
            }

            private static ProofSubject CreateAvatarSubject(
                VoxelRenderWorld world,
                PreviewUnitContract unit,
                PreviewCatalogModel model)
            {
                VoxelAssemblyAsset assembly =
                    PreviewCatalogDiscovery.LoadAssembly(
                        model);
                VoxelAnimationAsset animation =
                    PreviewCatalogDiscovery.LoadAnimation(
                        model);
                VoxelRenderInstance instance =
                    world.Spawn(
                        assembly,
                        animation);
                instance.RequirePart("Body");
                return new AvatarSubject(
                    unit.Id,
                    instance,
                    new VoxelActorInstance(instance));
            }

            private static ProofSubject CreateEquipmentSubject(
                VoxelRenderWorld world,
                PreviewUnitContract unit,
                PreviewCatalogModel model,
                PreviewCatalogModel holderModel)
            {
                if (holderModel == null)
                    throw new InvalidOperationException(
                        "Equipment proof holder is missing.");

                VoxelAssemblyAsset holderAssembly =
                    PreviewCatalogDiscovery.LoadAssembly(
                        holderModel);
                VoxelAnimationAsset holderAnimation =
                    PreviewCatalogDiscovery.LoadAnimation(
                        holderModel);
                VoxelRenderInstance holderInstance =
                    world.Spawn(
                        holderAssembly,
                        holderAnimation);
                holderInstance.RequirePart("Body");
                var holder =
                    new VoxelActorInstance(
                        holderInstance);

                VoxelAssemblyAsset equipmentAssembly =
                    PreviewCatalogDiscovery.LoadAssembly(
                        model);
                VoxelAnimationAsset equipmentAnimation =
                    PreviewCatalogDiscovery.LoadAnimation(
                        model);
                VoxelRenderInstance attached =
                    world.Spawn(
                        equipmentAssembly,
                        equipmentAnimation);
                VoxelActorBindingId binding =
                    holder.Attach(
                        attached,
                        "Equip.Hand.Right.0",
                        "Mount");

                VoxelRenderInstance standalone =
                    world.Spawn(
                        equipmentAssembly,
                        equipmentAnimation);
                return new EquipmentSubject(
                    unit.Id,
                    holderInstance,
                    holder,
                    attached,
                    binding,
                    standalone,
                    new VoxelModelAnimationTrack(
                        standalone));
            }

            public ProofSubject RequireSubject(
                string assetId)
            {
                foreach (ProofSubject subject in
                         _subjects.Values)
                    subject.SetVisible(
                        string.Equals(
                            subject.AssetId,
                            assetId,
                            StringComparison.Ordinal));

                if (!_subjects.TryGetValue(
                        assetId,
                        out ProofSubject result))
                    throw new InvalidOperationException(
                        "Animation proof subject is missing: " +
                        assetId);
                return result;
            }

            public void Focus(
                Bounds bounds,
                bool alternateView)
            {
                ConfigureCamera(
                    _camera,
                    bounds,
                    alternateView);
            }

            public void Dispose()
            {
                foreach (ProofSubject subject in
                         _subjects.Values)
                    subject.Dispose();
                _subjects.Clear();
                if (World != null)
                    World.Clear();
                if (_root != null)
                    UnityEngine.Object.Destroy(_root);
            }

            private static Camera CreateCamera()
            {
                var cameraObject =
                    new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                Camera camera =
                    cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.clearFlags =
                    CameraClearFlags.SolidColor;
                camera.backgroundColor =
                    new Color(
                        0.08f,
                        0.10f,
                        0.12f,
                        1f);
                camera.nearClipPlane = 0.01f;
                return camera;
            }

            private static void ConfigureCamera(
                Camera camera,
                Bounds bounds,
                bool alternateView)
            {
                camera.transform.rotation =
                    Quaternion.Euler(
                        28f,
                        alternateView
                            ? -35f
                            : 35f,
                        0f);
                float distance =
                    Mathf.Max(
                        8f,
                        bounds.extents.magnitude *
                        4.5f);
                camera.transform.position =
                    bounds.center -
                    camera.transform.forward *
                    distance;
                camera.orthographic = true;
                camera.orthographicSize =
                    RequiredOrthographicSize(
                        camera,
                        bounds) *
                    1.18f;
                camera.farClipPlane =
                    Mathf.Max(
                        100f,
                        distance * 3f +
                        bounds.extents.magnitude);
            }

            private static float RequiredOrthographicSize(
                Camera camera,
                Bounds bounds)
            {
                Vector3 c = bounds.center;
                Vector3 e = bounds.extents;
                float maxX = 0f;
                float maxY = 0f;
                for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 corner =
                        c +
                        Vector3.Scale(
                            e,
                            new Vector3(
                                x,
                                y,
                                z));
                    Vector3 local =
                        camera.transform
                            .InverseTransformPoint(
                                corner);
                    maxX =
                        Mathf.Max(
                            maxX,
                            Mathf.Abs(local.x));
                    maxY =
                        Mathf.Max(
                            maxY,
                            Mathf.Abs(local.y));
                }
                float aspect =
                    Mathf.Max(
                        0.01f,
                        camera.aspect);
                return Mathf.Max(
                    maxY,
                    maxX / aspect);
            }

            private static void CreateDirectionalLight()
            {
                var lightObject =
                    new GameObject(
                        "Directional Light");
                Light light =
                    lightObject.AddComponent<Light>();
                light.type =
                    LightType.Directional;
                light.intensity = 1.25f;
                lightObject.transform.rotation =
                    Quaternion.Euler(
                        50f,
                        -30f,
                        0f);
                RenderSettings.ambientIntensity = 1f;
                RenderSettings.fog = false;
            }
        }

        private abstract class ProofSubject : IDisposable
        {
            protected ProofSubject(
                string assetId)
            {
                AssetId =
                    assetId ??
                    throw new ArgumentNullException(
                        nameof(assetId));
            }

            public string AssetId { get; }

            public abstract void SetVisible(
                bool visible);
            public abstract void BeginClip(
                string clipName);
            public abstract void Sample(
                string clipName,
                float phase,
                bool moveReference);
            public abstract Bounds GetWorldBounds();
            public abstract void Validate(
                string assetId,
                string clipName,
                float phase);
            public abstract void Dispose();

            protected static void SelectIfNeeded(
                VoxelModelAnimationTrack track,
                string clipName)
            {
                if (!track.HasSelection ||
                    !string.Equals(
                        track.SelectedClip.Clip.Name,
                        clipName,
                        StringComparison.Ordinal))
                    track.Select(
                        new[] { clipName });
            }

            protected static bool IsFinite(
                Matrix4x4 value)
            {
                for (int row = 0;
                     row < 4;
                     row++)
                for (int column = 0;
                     column < 4;
                     column++)
                    if (!float.IsFinite(
                            value[row, column]))
                        return false;
                return true;
            }
        }

        private sealed class AvatarSubject
            : ProofSubject
        {
            private readonly VoxelRenderInstance _instance;
            private readonly VoxelActorInstance _actor;

            public AvatarSubject(
                string assetId,
                VoxelRenderInstance instance,
                VoxelActorInstance actor)
                : base(assetId)
            {
                _instance = instance;
                _actor = actor;
            }

            public override void SetVisible(
                bool visible) =>
                _actor.SetVisible(visible);

            public override void BeginClip(
                string clipName)
            {
                _actor.Reset();
                _actor.SetWorldTransform(
                    Vector3.zero,
                    Quaternion.identity,
                    Vector3.one);
                _actor.RootTrack.Select(
                    new[] { clipName });
            }

            public override void Sample(
                string clipName,
                float phase,
                bool moveReference)
            {
                SelectIfNeeded(
                    _actor.RootTrack,
                    clipName);
                _actor.RootTrack.SampleNormalized(
                    phase);
                _actor.Refresh();
            }

            public override Bounds GetWorldBounds() =>
                _actor.GetWorldBounds();

            public override void Validate(
                string assetId,
                string clipName,
                float phase)
            {
                Bounds bounds =
                    _actor.GetWorldBounds();
                if (!float.IsFinite(
                        bounds.center.x) ||
                    !float.IsFinite(
                        bounds.center.y) ||
                    !float.IsFinite(
                        bounds.center.z))
                    throw new InvalidOperationException(
                        "Avatar animation proof has non-finite bounds: " +
                        assetId +
                        " " +
                        clipName +
                        "@" +
                        phase);
            }

            public override void Dispose()
            {
                _actor.Dispose();
                _instance.Dispose();
            }
        }

        private sealed class EquipmentSubject
            : ProofSubject
        {
            private static readonly Vector3 StandalonePosition =
                new Vector3(-1.5f, 0f, 0f);
            private static readonly Vector3 HolderPosition =
                new Vector3(1.5f, 0f, 0f);

            private readonly VoxelRenderInstance _holderInstance;
            private readonly VoxelActorInstance _holder;
            private readonly VoxelRenderInstance _attached;
            private readonly VoxelActorBindingId _binding;
            private readonly VoxelRenderInstance _standalone;
            private readonly VoxelModelAnimationTrack _standaloneTrack;
            private readonly VoxelModelAnimationTrack _attachedTrack;

            public EquipmentSubject(
                string assetId,
                VoxelRenderInstance holderInstance,
                VoxelActorInstance holder,
                VoxelRenderInstance attached,
                VoxelActorBindingId binding,
                VoxelRenderInstance standalone,
                VoxelModelAnimationTrack standaloneTrack)
                : base(assetId)
            {
                _holderInstance = holderInstance;
                _holder = holder;
                _attached = attached;
                _binding = binding;
                _standalone = standalone;
                _standaloneTrack =
                    standaloneTrack;
                _attachedTrack =
                    holder.GetAttachmentTrack(
                        binding);
            }

            public override void SetVisible(
                bool visible)
            {
                _holder.SetVisible(visible);
                _standalone.SetVisible(visible);
            }

            public override void BeginClip(
                string clipName)
            {
                _holder.Reset();
                _holder.SetWorldTransform(
                    HolderPosition,
                    Quaternion.identity,
                    Vector3.one);
                _standaloneTrack.SetWorldTransform(
                    StandalonePosition,
                    Quaternion.identity,
                    Vector3.one);
                _holder.RootTrack.Select(
                    new[] { "Idle" });
                _holder.RootTrack.SampleNormalized(
                    0.5f);
                _attachedTrack.Select(
                    new[] { clipName });
                _standaloneTrack.Select(
                    new[] { clipName });
                _holder.Refresh();
            }

            public override void Sample(
                string clipName,
                float phase,
                bool moveReference)
            {
                SelectIfNeeded(
                    _attachedTrack,
                    clipName);
                SelectIfNeeded(
                    _standaloneTrack,
                    clipName);

                if (moveReference)
                {
                    Vector3 moved =
                        HolderPosition +
                        new Vector3(
                            phase * 1.8f,
                            0f,
                            -phase * 0.9f);
                    _holder.SetWorldTransform(
                        moved,
                        Quaternion.identity,
                        Vector3.one);
                }
                _attachedTrack.SampleNormalized(
                    phase);
                _standaloneTrack.SampleNormalized(
                    phase);
                _holder.Refresh();
            }

            public override Bounds GetWorldBounds()
            {
                Bounds result =
                    _holder.GetWorldBounds();
                result.Encapsulate(
                    _standalone.GetWorldBounds());
                return result;
            }

            public override void Validate(
                string assetId,
                string clipName,
                float phase)
            {
                if (!_holder.ContainsBinding(
                        _binding))
                    throw new InvalidOperationException(
                        "Equipment proof lost logical binding: " +
                        assetId +
                        " " +
                        clipName);
                if (!IsFinite(
                        _attachedTrack.WorldMatrix) ||
                    !IsFinite(
                        _standaloneTrack.WorldMatrix))
                    throw new InvalidOperationException(
                        "Equipment proof produced a non-finite transform: " +
                        assetId +
                        " " +
                        clipName +
                        "@" +
                        phase);
                float influence =
                    _attachedTrack.ReferenceInfluence;
                if (!float.IsFinite(influence) ||
                    influence < 0f ||
                    influence > 1f)
                    throw new InvalidOperationException(
                        "Equipment proof reference influence is invalid.");
            }

            public override void Dispose()
            {
                _holder.Dispose();
                _attached.Dispose();
                _standalone.Dispose();
                _holderInstance.Dispose();
            }
        }

        private static void ClearSessionState()
        {
            SessionState.EraseBool(RunKey);
            SessionState.EraseBool(StartedKey);
            SessionState.EraseString(ResultKey);
            SessionState.EraseString(ErrorKey);
            SessionState.EraseString(PlanJsonKey);
        }
    }
}
