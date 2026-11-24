using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hero.Consumer.Editor;
using Hero.Voxel.Render.Runtime.Unity;
using UnityEditor;
using UnityEngine;
using VoxelStore.Preview.Editor;
using VoxelStore.Preview.Runtime;

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

        private static readonly CapturePoint[] Points =
        {
            new CapturePoint(AvatarMotion.WalkForward, "walk-launch", 0.20f, EquipmentSlotSide.Right),
            new CapturePoint(AvatarMotion.WalkForward, "walk-apex", 0.45f, EquipmentSlotSide.Right),
            new CapturePoint(AvatarMotion.WalkForward, "walk-land", 0.70f, EquipmentSlotSide.Right),
            new CapturePoint(AvatarMotion.Attack, "attack-left-anticipation", 0.22f, EquipmentSlotSide.Left),
            new CapturePoint(AvatarMotion.Attack, "attack-left-contact", 0.50f, EquipmentSlotSide.Left),
            new CapturePoint(AvatarMotion.Attack, "attack-right-contact", 0.50f, EquipmentSlotSide.Right),
            new CapturePoint(AvatarMotion.Attack, "attack-right-follow-through", 0.70f, EquipmentSlotSide.Right),
            new CapturePoint(AvatarMotion.Hit, "hit-recoil", 0.28f, EquipmentSlotSide.Right),
            new CapturePoint(AvatarMotion.Hit, "hit-recover", 0.78f, EquipmentSlotSide.Right),
            new CapturePoint(AvatarMotion.Death, "death-fall", 0.55f, EquipmentSlotSide.Right),
            new CapturePoint(AvatarMotion.Death, "death-fallen", 0.88f, EquipmentSlotSide.Right),
            new CapturePoint(AvatarMotion.Idle, "idle-v1-breathe", 0.50f, EquipmentSlotSide.Right, "V1", "V1"),
            new CapturePoint(AvatarMotion.WalkForward, "walk-v1-apex", 0.45f, EquipmentSlotSide.Right, "V1", "V1"),
            new CapturePoint(AvatarMotion.Attack, "attack-left-v1-contact", 0.50f, EquipmentSlotSide.Left, "V1", "V1"),
            new CapturePoint(AvatarMotion.Hit, "hit-v1-recoil", 0.45f, EquipmentSlotSide.Right, "V1", "V1"),
            new CapturePoint(AvatarMotion.Death, "death-v1-fallen", 0.88f, EquipmentSlotSide.Right, "V1", "V1")
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
                {
                    throw new InvalidOperationException(
                        "Avatar motion capture must start outside Play Mode.");
                }

                string projectRoot =
                    Path.GetDirectoryName(Application.dataPath) ??
                    throw new InvalidOperationException(
                        "Unity project root could not be resolved.");
                string outputRoot =
                    Path.Combine(
                        projectRoot,
                        OutputDirectory.Replace(
                            '/',
                            Path.DirectorySeparatorChar));

                if (Directory.Exists(outputRoot))
                    Directory.Delete(outputRoot, recursive: true);
                Directory.CreateDirectory(outputRoot);

                VoxelStorePreviewCommand
                    .BakeAllForBatch();

                VoxelStoreRuntimeGalleryBuilder
                    .RefreshRuntimeGallerySceneForBatch();

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
                    AvatarMotionPlayer[] players = FindPlayers();
                    if (players.Length == 0)
                        return;

                    SessionState.SetBool(StartedKey, true);
                    ConfigureCaptureView(players);
                    ValidateCaptureRenderers();
                    _ = CaptureAllAsync(players);
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

            bool success =
                string.Equals(
                    result,
                    "success",
                    StringComparison.Ordinal);
            if (!success && !string.IsNullOrWhiteSpace(error))
                Debug.LogError(error);

            EditorApplication.Exit(success ? 0 : 1);
        }

        private static async Task CaptureAllAsync(
            AvatarMotionPlayer[] players)
        {
            Exception failure = null;
            try
            {
                foreach (CapturePoint point in Points)
                {
                    foreach (AvatarMotionPlayer player in players)
                    {
                        ApplyVariants(player, point);
                        player.SampleMotion(
                            point.Motion,
                            point.NormalizedTime,
                            point.Side);
                        ValidateAttachmentGrip(player, point);
                        player.SubmitNow();
                    }

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
                foreach (AvatarMotionPlayer player in players)
                {
                    if (player != null)
                        player.ResetMotion();
                }

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
            AvatarMotionPlayer[] players)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                throw new InvalidOperationException(
                    "Avatar motion capture requires the runtime gallery Main Camera.");
            }

            if (players == null || players.Length == 0)
            {
                throw new InvalidOperationException(
                    "Avatar motion capture requires at least one avatar player.");
            }

            bool hasBounds = false;
            Bounds bounds = default;
            try
            {
                foreach (CapturePoint point in Points)
                {
                    foreach (AvatarMotionPlayer player in players)
                    {
                        ApplyVariants(player, point);
                        player.SampleMotion(
                            point.Motion,
                            point.NormalizedTime,
                            point.Side);
                        ValidateAttachmentGrip(player, point);
                    }

                    foreach (AvatarMotionPlayer player in players)
                    {
                        Bounds sampled = player.Avatar.GetWorldBounds();
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
                foreach (AvatarMotionPlayer player in players)
                    player?.ResetMotion();
            }

            if (!hasBounds)
            {
                throw new InvalidOperationException(
                    "Avatar motion capture could not measure provider bounds.");
            }

            camera.transform.rotation =
                Quaternion.Euler(30f, 35f, 0f);
            float distance = Mathf.Max(
                20f,
                bounds.extents.magnitude * 3f);
            camera.transform.position =
                bounds.center -
                camera.transform.forward * distance;
            camera.orthographicSize =
                RequiredOrthographicSize(camera, bounds) * 1.08f;
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
            {
                for (int y = -1; y <= 1; y += 2)
                {
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
                }
            }

            float aspect = Mathf.Max(0.01f, camera.aspect);
            return Mathf.Max(maxY, maxX / aspect);
        }

        private static void ApplyVariants(
            AvatarMotionPlayer player,
            CapturePoint point)
        {
            player.SetVariant(point.RootVariant);
            player.SetEquipmentVariant(point.EquipmentVariant);
        }

        private static void ValidateAttachmentGrip(
            AvatarMotionPlayer player,
            CapturePoint point)
        {
            const float tolerance = 0.001f;
            AvatarPresentation avatar = player.Avatar;
            foreach (EquipmentPresentation equipment in avatar.Equipment)
            {
                VoxelRenderInstance instance = equipment.Model.Instance;
                if (instance == null)
                {
                    throw new InvalidOperationException(
                        "Capture attachment has no provider render instance.");
                }

                Vector3 expected =
                    avatar.ResolveBodySocketWorld(equipment.SocketName)
                        .MultiplyPoint3x4(Vector3.zero);
                int mountIndex = instance.RequireSocket("Mount");
                Vector3 actual =
                    instance.GetSocketWorldMatrix(mountIndex)
                        .MultiplyPoint3x4(Vector3.zero);
                float drift = Vector3.Distance(expected, actual);
                if (drift > tolerance)
                {
                    throw new InvalidOperationException(
                        "Animated attachment grip drift exceeds tolerance at " +
                        point.Checkpoint +
                        ": socket=" + equipment.SocketName +
                        ", drift=" + drift.ToString("R") +
                        ", tolerance=" + tolerance.ToString("R") + ".");
                }
            }
        }

        private static void ValidateCaptureRenderers()
        {
            GameObject root =
                GameObject.Find(VoxelStoreRuntimeGalleryBuilder.RootName);
            if (root == null)
            {
                throw new InvalidOperationException(
                    "Runtime gallery root is missing.");
            }

            VoxelRenderWorld world =
                root.GetComponent<VoxelRenderWorld>();
            if (world == null)
            {
                throw new InvalidOperationException(
                    "Runtime gallery requires one provider VoxelRenderWorld.");
            }

            VoxelRenderPresentation[] presentations =
                root.GetComponentsInChildren<VoxelRenderPresentation>(true);
            if (presentations.Length == 0)
            {
                throw new InvalidOperationException(
                    "Runtime gallery contains no provider render presentations.");
            }

            int expectedParts = 0;
            foreach (VoxelRenderPresentation presentation in presentations)
            {
                presentation.EnsureInstance();
                expectedParts += presentation.Assembly.PartCount;

                if (Quaternion.Angle(
                        presentation.ModelRoot.localRotation,
                        Quaternion.identity) > 0.001f)
                {
                    throw new InvalidOperationException(
                        "Runtime gallery contains a model-specific rotation correction: " +
                        presentation.name);
                }
            }

            world.SubmitNow();

            if (world.InstanceCount != presentations.Length)
            {
                throw new InvalidOperationException(
                    "Provider instance count mismatch. expected=" +
                    presentations.Length +
                    ", actual=" +
                    world.InstanceCount);
            }

            if (world.SubmittedPartCount != expectedParts)
            {
                throw new InvalidOperationException(
                    "Provider submitted part count mismatch. expected=" +
                    expectedParts +
                    ", actual=" +
                    world.SubmittedPartCount);
            }

            MeshRenderer[] legacyRenderers =
                root.GetComponentsInChildren<MeshRenderer>(true);
            if (legacyRenderers.Length != 0)
            {
                throw new InvalidOperationException(
                    "Runtime gallery still contains a local MeshRenderer path.");
            }

            Debug.Log(
                "Voxel Store provider render integrity passed: " +
                presentations.Length +
                " instances, " +
                expectedParts +
                " submitted parts.");
        }

        private static AvatarMotionPlayer[] FindPlayers() =>
            UnityEngine.Object
                .FindObjectsByType<AvatarMotionPlayer>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .OrderBy(
                    player => player.name,
                    StringComparer.Ordinal)
                .ToArray();

        private static void ClearSessionState()
        {
            SessionState.EraseBool(RunKey);
            SessionState.EraseBool(StartedKey);
            SessionState.EraseString(ResultKey);
            SessionState.EraseString(ErrorKey);
        }

        private readonly struct CapturePoint
        {
            public CapturePoint(
                AvatarMotion motion,
                string checkpoint,
                float normalizedTime,
                EquipmentSlotSide side,
                string rootVariant = "",
                string equipmentVariant = "")
            {
                Motion = motion;
                Checkpoint = checkpoint;
                NormalizedTime = normalizedTime;
                Side = side;
                RootVariant =
                    AvatarMotionClipCandidates.NormalizeVariant(
                        rootVariant);
                EquipmentVariant =
                    AvatarMotionClipCandidates.NormalizeVariant(
                        equipmentVariant);
            }

            public AvatarMotion Motion { get; }
            public string Checkpoint { get; }
            public float NormalizedTime { get; }
            public EquipmentSlotSide Side { get; }
            public string RootVariant { get; }
            public string EquipmentVariant { get; }
        }
    }
}
