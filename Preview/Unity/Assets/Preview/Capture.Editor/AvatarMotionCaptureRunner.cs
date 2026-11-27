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
        private const string OutputDirectory =
            "Library/Capture/AvatarMotionGallery";
        private const string FocusEquipmentAssemblyName = "Axe03";

        private static readonly CapturePoint[] Points =
        {
            new CapturePoint("Idle", "Idle", "idle-baseline-hold", 0.50f, 0.50f),
            new CapturePoint("WalkForward", "WalkForward", "walk-launch", 0.20f, 0.20f),
            new CapturePoint("WalkForward", "WalkForward", "walk-apex", 0.45f, 0.45f),
            new CapturePoint("WalkForward", "WalkForward", "walk-land", 0.70f, 0.70f),
            new CapturePoint("Attack.Left", "Attack", "attack-left-anticipation", 0.22f, 0.22f),
            new CapturePoint("Attack.Left", "Attack", "attack-left-contact", 0.50f, 0.50f),
            new CapturePoint("Attack.Right", "Attack", "attack-right-contact", 0.50f, 0.50f),
            new CapturePoint("Attack.Right", "Attack", "attack-right-follow-through", 0.70f, 0.70f),
            new CapturePoint("Hit", "Hit", "hit-recoil", 0.28f, 0.28f),
            new CapturePoint("Hit", "Hit", "hit-recover", 0.78f, 0.78f),
            new CapturePoint("Death", "Death", "death-fall", 0.55f, 0.55f),
            new CapturePoint("Death", "Death", "death-fallen", 0.88f, 0.88f),
            new CapturePoint("Idle.V1", "Idle.V1", "idle-v1-breathe", 0.50f, 0.50f),
            new CapturePoint("WalkForward.V1", "WalkForward.V1", "walk-v1-apex", 0.45f, 0.45f),
            new CapturePoint("Attack.Left.V1", "Attack.V1", "attack-left-v1-contact", 0.50f, 0.50f),
            new CapturePoint("Hit.V1", "Hit.V1", "hit-v1-recoil", 0.45f, 0.45f),
            new CapturePoint("Death.V1", "Death.V1", "death-v1-fallen", 0.88f, 0.88f),
            new CapturePoint("Idle", "Attack", "independent-body-idle-weapon-attack", 0.50f, 0.75f),
            new CapturePoint("WalkForward", "Attack", "independent-phases", 0.25f, 0.80f),
            new CapturePoint("Death", "Death", "independent-death-commands", 0.55f, 0.30f)
        };

        private static readonly CapturePoint[] FocusedAxe03Points =
        {
            new CapturePoint("Idle", "Idle", "axe03-idle-hold", 0.50f, 0.50f),
            new CapturePoint("WalkForward", "Attack", "axe03-v0-release", 0.20f, 0.50f),
            new CapturePoint("WalkForward", "Attack", "axe03-v0-return-early", 0.45f, 0.78f),
            new CapturePoint("WalkForward", "Attack", "axe03-v0-return-late", 0.70f, 0.90f),
            new CapturePoint("WalkForward", "Attack", "axe03-v0-rejoined", 0.90f, 1.00f),
            new CapturePoint("WalkForward", "Attack.V1", "axe03-v1-release", 0.20f, 0.50f),
            new CapturePoint("WalkForward", "Attack.V1", "axe03-v1-return-early", 0.45f, 0.82f),
            new CapturePoint("WalkForward", "Attack.V1", "axe03-v1-return-late", 0.70f, 0.92f),
            new CapturePoint("WalkForward", "Attack.V1", "axe03-v1-rejoined", 0.90f, 1.00f)
        };

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
                        "Avatar content capture must start outside Play Mode.");

                string projectRoot =
                    Path.GetDirectoryName(Application.dataPath) ??
                    throw new InvalidOperationException(
                        "Unity project root could not be resolved.");
                string outputRoot = Path.Combine(
                    projectRoot,
                    OutputDirectory.Replace('/', Path.DirectorySeparatorChar));

                if (Directory.Exists(outputRoot))
                    Directory.Delete(outputRoot, recursive: true);
                Directory.CreateDirectory(outputRoot);

                VoxelStorePreviewCommand.BakeAllForBatch();
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);

                SessionState.SetBool(RunKey, true);
                SessionState.SetBool(StartedKey, false);
                SessionState.SetString(ResultKey, string.Empty);
                SessionState.SetString(ErrorKey, string.Empty);
                EditorApplication.isPlaying = true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void Poll()
        {
            if (!SessionState.GetBool(RunKey, false))
                return;

            if (EditorApplication.isPlaying)
            {
                if (!SessionState.GetBool(StartedKey, false))
                {
                    SessionState.SetBool(StartedKey, true);
                    _ = CaptureAllAsync();
                }
                return;
            }

            string result =
                SessionState.GetString(ResultKey, string.Empty);
            if (string.IsNullOrWhiteSpace(result))
                return;

            string error =
                SessionState.GetString(ErrorKey, string.Empty);
            ClearSessionState();

            bool success = string.Equals(
                result,
                "success",
                StringComparison.Ordinal);
            if (!success && !string.IsNullOrWhiteSpace(error))
                Debug.LogError(error);

            EditorApplication.Exit(success ? 0 : 1);
        }

        private static async Task CaptureAllAsync()
        {
            Exception failure = null;
            ProofScene scene = null;
            try
            {
                scene = ProofScene.Create();
                ConfigureCaptureView(scene.Avatars);

                foreach (CapturePoint point in Points)
                {
                    foreach (ProofAvatar avatar in scene.Avatars)
                    {
                        avatar.Sample(point);
                        avatar.Validate(point.Checkpoint);
                    }

                    scene.World.SubmitNow();
                    await GameViewCapture.CaptureAsync(
                        OutputDirectory,
                        point.Checkpoint,
                        stableFrames: 1);
                }

                ProofAvatar axeAvatar =
                    FindAvatarWithEquipment(
                        scene.Avatars,
                        FocusEquipmentAssemblyName);
                ConfigureFocusedCaptureView(
                    axeAvatar,
                    FocusedAxe03Points);

                foreach (CapturePoint point in FocusedAxe03Points)
                {
                    axeAvatar.SampleFocused(
                        point,
                        FocusEquipmentAssemblyName);
                    axeAvatar.Validate(point.Checkpoint);
                    scene.World.SubmitNow();
                    await GameViewCapture.CaptureAsync(
                        OutputDirectory,
                        point.Checkpoint,
                        stableFrames: 1);
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
                    failure == null ? "success" : "failure");
                SessionState.SetString(
                    ErrorKey,
                    failure?.ToString() ?? string.Empty);

                EditorApplication.isPlaying = false;
            }
        }

        private static void ConfigureCaptureView(
            IReadOnlyList<ProofAvatar> avatars)
        {
            Camera camera = Camera.main;
            if (camera == null)
                throw new InvalidOperationException(
                    "Avatar content capture requires a Main Camera.");

            bool hasBounds = false;
            Bounds bounds = default;
            try
            {
                foreach (CapturePoint point in Points)
                {
                    foreach (ProofAvatar avatar in avatars)
                    {
                        avatar.Sample(point);
                        Bounds sampled = avatar.GetWorldBounds();
                        if (!hasBounds)
                        {
                            bounds = sampled;
                            hasBounds = true;
                        }
                        else
                        {
                            bounds.Encapsulate(sampled);
                        }
                    }
                }
            }
            finally
            {
                foreach (ProofAvatar avatar in avatars)
                    avatar.Reset();
            }

            if (!hasBounds)
                throw new InvalidOperationException(
                    "Avatar content capture could not measure provider bounds.");

            ConfigureCamera(camera, bounds, 20f, 1.08f);
        }

        private static void ConfigureFocusedCaptureView(
            ProofAvatar avatar,
            IReadOnlyList<CapturePoint> points)
        {
            Camera camera = Camera.main;
            if (camera == null)
                throw new InvalidOperationException(
                    "Focused equipment capture requires a Main Camera.");
            if (avatar == null)
                throw new ArgumentNullException(nameof(avatar));
            if (points == null || points.Count == 0)
                throw new ArgumentException(
                    "Focused equipment capture requires at least one point.",
                    nameof(points));

            bool hasBounds = false;
            Bounds bounds = default;
            try
            {
                foreach (CapturePoint point in points)
                {
                    avatar.SampleFocused(
                        point,
                        FocusEquipmentAssemblyName);
                    Bounds sampled = avatar.GetWorldBounds();
                    if (!hasBounds)
                    {
                        bounds = sampled;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(sampled);
                    }
                }
            }
            finally
            {
                avatar.Reset();
            }

            ConfigureCamera(camera, bounds, 8f, 1.2f);
        }

        private static void ConfigureCamera(
            Camera camera,
            Bounds bounds,
            float minimumDistance,
            float padding)
        {
            camera.transform.rotation =
                Quaternion.Euler(30f, 35f, 0f);
            float distance = Mathf.Max(
                minimumDistance,
                bounds.extents.magnitude * 4f);
            camera.transform.position =
                bounds.center - camera.transform.forward * distance;
            camera.orthographic = true;
            camera.orthographicSize =
                RequiredOrthographicSize(camera, bounds) * padding;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane =
                Mathf.Max(100f, distance * 3f + bounds.extents.magnitude);
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
                Vector3 corner = c + Vector3.Scale(
                    e,
                    new Vector3(x, y, z));
                Vector3 local =
                    camera.transform.InverseTransformPoint(corner);
                maxX = Mathf.Max(maxX, Mathf.Abs(local.x));
                maxY = Mathf.Max(maxY, Mathf.Abs(local.y));
            }

            float aspect = Mathf.Max(0.01f, camera.aspect);
            return Mathf.Max(maxY, maxX / aspect);
        }

        private static ProofAvatar FindAvatarWithEquipment(
            IReadOnlyList<ProofAvatar> avatars,
            string assemblyName)
        {
            ProofAvatar match = avatars.FirstOrDefault(
                avatar => avatar.HasEquipment(assemblyName));
            if (match == null)
            {
                throw new InvalidOperationException(
                    "Avatar content capture could not find carried equipment assembly '" +
                    assemblyName + "'.");
            }

            return match;
        }

        private static void ClearSessionState()
        {
            SessionState.EraseBool(RunKey);
            SessionState.EraseBool(StartedKey);
            SessionState.EraseString(ResultKey);
            SessionState.EraseString(ErrorKey);
        }

        private static bool IsFinite(Matrix4x4 value)
        {
            for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
            {
                if (!float.IsFinite(value[row, column]))
                    return false;
            }

            return true;
        }

        private readonly struct CapturePoint
        {
            public CapturePoint(
                string bodyClip,
                string equipmentClip,
                string checkpoint,
                float bodyPhase,
                float equipmentPhase)
            {
                BodyClip = bodyClip ??
                    throw new ArgumentNullException(nameof(bodyClip));
                EquipmentClip = equipmentClip ??
                    throw new ArgumentNullException(nameof(equipmentClip));
                Checkpoint = checkpoint ??
                    throw new ArgumentNullException(nameof(checkpoint));
                BodyPhase = bodyPhase;
                EquipmentPhase = equipmentPhase;
            }

            public string BodyClip { get; }
            public string EquipmentClip { get; }
            public string Checkpoint { get; }
            public float BodyPhase { get; }
            public float EquipmentPhase { get; }
        }

        private sealed class ProofScene : IDisposable
        {
            private readonly GameObject _root;
            private readonly List<ProofAvatar> _avatars;

            private ProofScene(
                GameObject root,
                VoxelRenderWorld world,
                List<ProofAvatar> avatars)
            {
                _root = root;
                World = world;
                _avatars = avatars;
            }

            public VoxelRenderWorld World { get; }
            public IReadOnlyList<ProofAvatar> Avatars => _avatars;

            public static ProofScene Create()
            {
                PreviewCatalogModel[] models =
                    PreviewCatalogDiscovery.Discover();
                PreviewVariantCoverage.ValidateAll(models);

                PreviewCatalogModel[] avatars = models
                    .Where(x => PreviewCatalogDiscovery.IsCatalog(x, "Avatars"))
                    .ToArray();
                PreviewCatalogModel[] equipment = models
                    .Where(x => PreviewCatalogDiscovery.IsCatalog(x, "Equipment"))
                    .ToArray();

                if (avatars.Length == 0)
                    throw new InvalidOperationException(
                        "Avatar content capture found no baked avatar models.");
                if (equipment.Length == 0)
                    throw new InvalidOperationException(
                        "Avatar content capture found no baked equipment models.");

                var root = new GameObject("[Voxel Store Content Proof]");
                VoxelRenderWorld world =
                    root.AddComponent<VoxelRenderWorld>();
                world.Initialize();

                var proofAvatars =
                    new List<ProofAvatar>(avatars.Length);
                for (int i = 0; i < avatars.Length; i++)
                {
                    PreviewCatalogModel avatarModel = avatars[i];
                    VoxelAssemblyAsset avatarAssembly =
                        PreviewCatalogDiscovery.LoadAssembly(avatarModel);
                    VoxelAnimationAsset avatarAnimation =
                        PreviewCatalogDiscovery.LoadAnimation(avatarModel);

                    VoxelRenderInstance rootInstance =
                        world.Spawn(avatarAssembly, avatarAnimation);
                    rootInstance.RequirePart("Body");
                    var actor = new VoxelActorInstance(rootInstance);
                    var attached = new List<ProofEquipment>(2);

                    int primaryIndex = i % equipment.Length;
                    Attach(
                        world,
                        actor,
                        equipment[primaryIndex],
                        "Equip.Hand.Right.0",
                        attached);

                    if (equipment.Length > 1)
                    {
                        int secondaryIndex =
                            (i + Math.Max(1, equipment.Length / 2)) %
                            equipment.Length;
                        if (secondaryIndex == primaryIndex)
                            secondaryIndex =
                                (primaryIndex + 1) % equipment.Length;
                        Attach(
                            world,
                            actor,
                            equipment[secondaryIndex],
                            "Equip.Hand.Left.0",
                            attached);
                    }

                    proofAvatars.Add(
                        new ProofAvatar(
                            avatarModel.Name,
                            world,
                            rootInstance,
                            actor,
                            attached));
                }

                Bounds[] initial = proofAvatars
                    .Select(x => x.GetWorldBounds())
                    .ToArray();
                Vector3[] positions = PreviewGridLayout.Calculate(
                    initial,
                    margin: 1.75f);
                for (int i = 0; i < proofAvatars.Count; i++)
                    proofAvatars[i].SetWorldPosition(positions[i]);

                Bounds[] placed = proofAvatars
                    .Select(x => x.GetWorldBounds())
                    .ToArray();
                if (PreviewGridLayout.CountXZOverlaps(placed) != 0)
                {
                    throw new InvalidOperationException(
                        "Avatar content proof grid contains overlapping bounds.");
                }

                Bounds all = placed[0];
                for (int i = 1; i < placed.Length; i++)
                    all.Encapsulate(placed[i]);

                CreateCamera(all);
                CreateDirectionalLight();
                world.SubmitNow();

                return new ProofScene(root, world, proofAvatars);
            }

            private static void Attach(
                VoxelRenderWorld world,
                VoxelActorInstance actor,
                PreviewCatalogModel model,
                string socketName,
                ICollection<ProofEquipment> output)
            {
                VoxelAssemblyAsset assembly =
                    PreviewCatalogDiscovery.LoadAssembly(model);
                VoxelAnimationAsset animation =
                    PreviewCatalogDiscovery.LoadAnimation(model);
                VoxelRenderInstance instance =
                    world.Spawn(assembly, animation);
                VoxelActorBindingId binding =
                    actor.Attach(instance, socketName, "Mount");
                output.Add(
                    new ProofEquipment(
                        assembly.AssemblyName,
                        instance,
                        binding,
                        actor.GetAttachmentTrack(binding)));
            }

            private static void CreateCamera(Bounds bounds)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor =
                    new Color(0.08f, 0.10f, 0.12f, 1f);
                ConfigureCamera(camera, bounds, 20f, 1.08f);
            }

            private static void CreateDirectionalLight()
            {
                var lightObject = new GameObject("Directional Light");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.25f;
                lightObject.transform.rotation =
                    Quaternion.Euler(50f, -30f, 0f);
                RenderSettings.ambientIntensity = 1f;
                RenderSettings.fog = false;
            }

            public void Dispose()
            {
                foreach (ProofAvatar avatar in _avatars)
                    avatar.Dispose();
                _avatars.Clear();

                if (World != null)
                    World.Clear();
                if (_root != null)
                    UnityEngine.Object.Destroy(_root);
            }
        }

        private sealed class ProofAvatar : IDisposable
        {
            private readonly VoxelRenderWorld _world;
            private readonly VoxelRenderInstance _rootInstance;
            private readonly VoxelActorInstance _actor;
            private readonly List<ProofEquipment> _equipment;
            private Vector3 _worldPosition;

            public ProofAvatar(
                string name,
                VoxelRenderWorld world,
                VoxelRenderInstance rootInstance,
                VoxelActorInstance actor,
                List<ProofEquipment> equipment)
            {
                Name = name;
                _world = world;
                _rootInstance = rootInstance;
                _actor = actor;
                _equipment = equipment;
                SetWorldPosition(Vector3.zero);
            }

            public string Name { get; }

            public void SetWorldPosition(Vector3 position)
            {
                _worldPosition = position;
                _actor.SetWorldTransform(
                    position,
                    Quaternion.identity,
                    Vector3.one);
            }

            public void Sample(CapturePoint point)
            {
                SampleTrack(
                    _actor.RootTrack,
                    point.BodyClip,
                    point.BodyPhase);
                foreach (ProofEquipment equipment in _equipment)
                {
                    SampleTrack(
                        equipment.Track,
                        point.EquipmentClip,
                        point.EquipmentPhase);
                }
                _actor.Refresh();
            }

            public void SampleFocused(
                CapturePoint point,
                string focusAssemblyName)
            {
                SampleTrack(
                    _actor.RootTrack,
                    point.BodyClip,
                    point.BodyPhase);
                foreach (ProofEquipment equipment in _equipment)
                {
                    bool focused = string.Equals(
                        equipment.AssemblyName,
                        focusAssemblyName,
                        StringComparison.Ordinal);
                    SampleTrack(
                        equipment.Track,
                        focused ? point.EquipmentClip : "Idle",
                        focused ? point.EquipmentPhase : 0.50f);
                }
                _actor.Refresh();
            }

            public bool HasEquipment(string assemblyName) =>
                _equipment.Any(
                    x => string.Equals(
                        x.AssemblyName,
                        assemblyName,
                        StringComparison.Ordinal));

            public Bounds GetWorldBounds() => _actor.GetWorldBounds();

            public void Validate(string checkpoint)
            {
                Bounds bounds = _actor.GetWorldBounds();
                if (!float.IsFinite(bounds.center.x) ||
                    !float.IsFinite(bounds.center.y) ||
                    !float.IsFinite(bounds.center.z))
                {
                    throw new InvalidOperationException(
                        "Avatar proof bounds are not finite at " +
                        checkpoint + ".");
                }

                foreach (ProofEquipment equipment in _equipment)
                {
                    if (!_actor.ContainsBinding(equipment.BindingId))
                    {
                        throw new InvalidOperationException(
                            "Avatar proof lost an equipment binding at " +
                            checkpoint + ".");
                    }

                    Matrix4x4 matrix = equipment.Track.WorldMatrix;
                    if (!IsFinite(matrix))
                    {
                        throw new InvalidOperationException(
                            "Equipment world matrix is not finite at " +
                            checkpoint + ".");
                    }

                    float influence =
                        equipment.Track.ReferenceInfluence;
                    if (!float.IsFinite(influence) ||
                        influence < 0f ||
                        influence > 1f)
                    {
                        throw new InvalidOperationException(
                            "Equipment reference influence is invalid at " +
                            checkpoint + ".");
                    }
                }
            }

            public void Reset()
            {
                _actor.Reset();
                _actor.SetWorldTransform(
                    _worldPosition,
                    Quaternion.identity,
                    Vector3.one);
            }

            public void Dispose()
            {
                _actor.Dispose();
                foreach (ProofEquipment equipment in _equipment)
                    equipment.Instance.Dispose();
                _rootInstance.Dispose();
            }

            private static void SampleTrack(
                VoxelModelAnimationTrack track,
                string clip,
                float phase)
            {
                track.Select(new[] { clip });
                track.SampleNormalized(phase);
            }
        }

        private sealed class ProofEquipment
        {
            public ProofEquipment(
                string assemblyName,
                VoxelRenderInstance instance,
                VoxelActorBindingId bindingId,
                VoxelModelAnimationTrack track)
            {
                AssemblyName = assemblyName;
                Instance = instance;
                BindingId = bindingId;
                Track = track;
            }

            public string AssemblyName { get; }
            public VoxelRenderInstance Instance { get; }
            public VoxelActorBindingId BindingId { get; }
            public VoxelModelAnimationTrack Track { get; }
        }
    }
}
