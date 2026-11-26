using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hero.Consumer.Editor;
using Hero.Voxel.Actor.Runtime.Unity;
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
        private const string FocusEquipmentAssemblyName = "Axe03";

        private static readonly CapturePoint[] Points =
        {
            new CapturePoint(AvatarMotion.Idle, AvatarMotion.Idle, "idle-baseline-hold", 0.50f, 0.50f),
            new CapturePoint(AvatarMotion.WalkForward, AvatarMotion.WalkForward, "walk-launch", 0.20f, 0.20f),
            new CapturePoint(AvatarMotion.WalkForward, AvatarMotion.WalkForward, "walk-apex", 0.45f, 0.45f),
            new CapturePoint(AvatarMotion.WalkForward, AvatarMotion.WalkForward, "walk-land", 0.70f, 0.70f),
            new CapturePoint(AvatarMotion.Attack, AvatarMotion.Attack, "attack-left-anticipation", 0.22f, 0.22f, EquipmentSlotSide.Left),
            new CapturePoint(AvatarMotion.Attack, AvatarMotion.Attack, "attack-left-contact", 0.50f, 0.50f, EquipmentSlotSide.Left),
            new CapturePoint(AvatarMotion.Attack, AvatarMotion.Attack, "attack-right-contact", 0.50f, 0.50f, EquipmentSlotSide.Right),
            new CapturePoint(AvatarMotion.Attack, AvatarMotion.Attack, "attack-right-follow-through", 0.70f, 0.70f, EquipmentSlotSide.Right),
            new CapturePoint(AvatarMotion.Hit, AvatarMotion.Hit, "hit-recoil", 0.28f, 0.28f),
            new CapturePoint(AvatarMotion.Hit, AvatarMotion.Hit, "hit-recover", 0.78f, 0.78f),
            new CapturePoint(AvatarMotion.Death, AvatarMotion.Death, "death-fall", 0.55f, 0.55f),
            new CapturePoint(AvatarMotion.Death, AvatarMotion.Death, "death-fallen", 0.88f, 0.88f),
            new CapturePoint(AvatarMotion.Idle, AvatarMotion.Idle, "idle-v1-breathe", 0.50f, 0.50f, null, "V1", "V1"),
            new CapturePoint(AvatarMotion.WalkForward, AvatarMotion.WalkForward, "walk-v1-apex", 0.45f, 0.45f, null, "V1", "V1"),
            new CapturePoint(AvatarMotion.Attack, AvatarMotion.Attack, "attack-left-v1-contact", 0.50f, 0.50f, EquipmentSlotSide.Left, "V1", "V1"),
            new CapturePoint(AvatarMotion.Hit, AvatarMotion.Hit, "hit-v1-recoil", 0.45f, 0.45f, null, "V1", "V1"),
            new CapturePoint(AvatarMotion.Death, AvatarMotion.Death, "death-v1-fallen", 0.88f, 0.88f, null, "V1", "V1"),
            new CapturePoint(AvatarMotion.Idle, AvatarMotion.Attack, "independent-body-idle-weapon-attack", 0.50f, 0.75f),
            new CapturePoint(AvatarMotion.WalkForward, AvatarMotion.Attack, "independent-phases", 0.25f, 0.80f),
            new CapturePoint(AvatarMotion.Death, AvatarMotion.Death, "independent-death-commands", 0.55f, 0.30f)
        };

        private static readonly CapturePoint[] FocusedAxe03Points =
        {
            new CapturePoint(AvatarMotion.Idle, AvatarMotion.Idle, "axe03-idle-hold", 0.50f, 0.50f),
            new CapturePoint(AvatarMotion.Idle, AvatarMotion.Attack, "axe03-attack-v0-contact", 0.50f, 0.50f),
            new CapturePoint(AvatarMotion.Idle, AvatarMotion.Attack, "axe03-attack-v1-contact", 0.50f, 0.50f, null, "", "V1")
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
                    AvatarPresentation[] avatars = FindAvatars();
                    if (avatars.Length == 0)
                        return;

                    SessionState.SetBool(StartedKey, true);
                    ConfigureCaptureView(avatars);
                    ValidateCaptureRenderers();
                    _ = CaptureAllAsync(avatars);
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
            AvatarPresentation[] avatars)
        {
            Exception failure = null;
            try
            {
                foreach (CapturePoint point in Points)
                {
                    foreach (AvatarPresentation avatar in avatars)
                    {
                        ApplyPoint(avatar, point);
                        ValidateAttachmentReference(avatar, point);
                        avatar.SubmitNow();
                    }

                    await GameViewCapture.CaptureAsync(
                        OutputDirectory,
                        point.Checkpoint,
                        stableFrames: 1);
                }

                AvatarPresentation axeAvatar =
                    FindAvatarWithEquipment(
                        avatars,
                        FocusEquipmentAssemblyName);
                ConfigureFocusedCaptureView(
                    axeAvatar,
                    FocusedAxe03Points);
                foreach (CapturePoint point in FocusedAxe03Points)
                {
                    ApplyFocusedEquipmentPoint(
                        axeAvatar,
                        point,
                        FocusEquipmentAssemblyName);
                    ValidateAttachmentReference(axeAvatar, point);
                    axeAvatar.SubmitNow();
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
                foreach (AvatarPresentation avatar in avatars)
                    avatar?.ResetPose();

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
            AvatarPresentation[] avatars)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                throw new InvalidOperationException(
                    "Avatar motion capture requires the runtime gallery Main Camera.");
            }

            if (avatars == null || avatars.Length == 0)
            {
                throw new InvalidOperationException(
                    "Avatar motion capture requires at least one avatar.");
            }

            bool hasBounds = false;
            Bounds bounds = default;
            try
            {
                foreach (CapturePoint point in Points)
                {
                    foreach (AvatarPresentation avatar in avatars)
                    {
                        ApplyPoint(avatar, point);
                        ValidateAttachmentReference(avatar, point);
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
                foreach (AvatarPresentation avatar in avatars)
                    avatar?.ResetPose();
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

        private static void ConfigureFocusedCaptureView(
            AvatarPresentation avatar,
            IReadOnlyList<CapturePoint> points)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                throw new InvalidOperationException(
                    "Focused equipment capture requires the runtime gallery Main Camera.");
            }
            if (avatar == null)
                throw new ArgumentNullException(nameof(avatar));
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException(
                    "Focused equipment capture requires at least one point.",
                    nameof(points));
            }

            bool hasBounds = false;
            Bounds bounds = default;
            try
            {
                foreach (CapturePoint point in points)
                {
                    ApplyFocusedEquipmentPoint(
                        avatar,
                        point,
                        FocusEquipmentAssemblyName);
                    ValidateAttachmentReference(avatar, point);
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
                avatar.ResetPose();
            }

            camera.transform.rotation =
                Quaternion.Euler(30f, 35f, 0f);
            float distance = Mathf.Max(
                8f,
                bounds.extents.magnitude * 4f);
            camera.transform.position =
                bounds.center -
                camera.transform.forward * distance;
            camera.orthographicSize =
                RequiredOrthographicSize(camera, bounds) * 1.2f;
        }


        private static void ApplyPoint(
            AvatarPresentation avatar,
            CapturePoint point)
        {
            VoxelModelMotionPlayer[] players =
                avatar.GetComponentsInChildren<VoxelModelMotionPlayer>(
                    true);
            VoxelModelMotionPlayer body =
                players.SingleOrDefault(player => player.IsBody);
            if (body == null)
            {
                throw new InvalidOperationException(
                    "Avatar motion capture requires one body model player.");
            }

            body.SampleCandidates(
                AvatarMotionClipCandidates.Build(
                    point.BodyMotion,
                    point.BodySide,
                    point.BodyVariant),
                point.BodyPhase);

            foreach (VoxelModelMotionPlayer equipmentPlayer in
                     players.Where(player => player.Equipment != null))
            {
                equipmentPlayer.SampleCandidates(
                    AvatarMotionClipCandidates.Build(
                        point.EquipmentMotion,
                        equipmentPlayer.Equipment.Side,
                        point.EquipmentVariant),
                    point.EquipmentPhase);
            }

            avatar.RefreshAttachmentReferences();
        }

        private static void ApplyFocusedEquipmentPoint(
            AvatarPresentation avatar,
            CapturePoint point,
            string equipmentAssemblyName)
        {
            VoxelModelMotionPlayer[] players =
                avatar.GetComponentsInChildren<VoxelModelMotionPlayer>(
                    true);
            VoxelModelMotionPlayer body =
                players.SingleOrDefault(player => player.IsBody);
            if (body == null)
            {
                throw new InvalidOperationException(
                    "Focused equipment capture requires one body model player.");
            }

            body.SampleCandidates(
                AvatarMotionClipCandidates.Build(
                    point.BodyMotion,
                    point.BodySide,
                    point.BodyVariant),
                point.BodyPhase);

            foreach (VoxelModelMotionPlayer equipmentPlayer in
                     players.Where(player => player.Equipment != null))
            {
                bool focused =
                    string.Equals(
                        equipmentPlayer.Model.Assembly.AssemblyName,
                        equipmentAssemblyName,
                        StringComparison.Ordinal);
                equipmentPlayer.SampleCandidates(
                    AvatarMotionClipCandidates.Build(
                        focused
                            ? point.EquipmentMotion
                            : AvatarMotion.Idle,
                        equipmentPlayer.Equipment.Side,
                        focused
                            ? point.EquipmentVariant
                            : string.Empty),
                    focused
                        ? point.EquipmentPhase
                        : 0.50f);
            }

            avatar.RefreshAttachmentReferences();
        }


        private static void ValidateAttachmentReference(
            AvatarPresentation avatar,
            CapturePoint point)
        {
            const float tolerance = 0.001f;
            foreach (EquipmentPresentation equipment in avatar.Equipment)
            {
                if (!equipment.BindingId.IsValid)
                {
                    throw new InvalidOperationException(
                        "Capture attachment has no current provider binding.");
                }

                VoxelModelAnimationTrack track =
                    equipment.Model.AnimationTrack;
                if (track == null)
                {
                    throw new InvalidOperationException(
                        "Capture attachment has no provider model track.");
                }

                Matrix4x4 reference =
                    avatar.ResolveAttachmentReferenceWorld(equipment);
                Matrix4x4 actual = track.WorldMatrix;
                if (!IsFinite(reference) || !IsFinite(actual))
                {
                    throw new InvalidOperationException(
                        "Capture attachment reference/world matrix is not finite at " +
                        point.Checkpoint + ".");
                }

                VoxelModelMotionPlayer player =
                    equipment.Model.GetComponent<VoxelModelMotionPlayer>();
                if (player == null ||
                    player.ShouldFollowAttachmentReference)
                {
                    float error = MaxMatrixDelta(reference, actual);
                    if (error > tolerance)
                    {
                        throw new InvalidOperationException(
                            "Capture attachment track does not match its current reference at " +
                            point.Checkpoint +
                            ": error=" + error.ToString("R") +
                            ", tolerance=" + tolerance.ToString("R") + ".");
                    }
                }
            }
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

        private static float MaxMatrixDelta(
            Matrix4x4 left,
            Matrix4x4 right)
        {
            float max = 0f;
            for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
            {
                max = Mathf.Max(
                    max,
                    Mathf.Abs(left[row, column] - right[row, column]));
            }

            return max;
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

        private static AvatarPresentation FindAvatarWithEquipment(
            IReadOnlyList<AvatarPresentation> avatars,
            string assemblyName)
        {
            AvatarPresentation match =
                avatars.FirstOrDefault(
                    avatar =>
                        avatar != null &&
                        avatar.Equipment.Any(
                            item =>
                                item != null &&
                                item.Model != null &&
                                item.Model.Assembly != null &&
                                string.Equals(
                                    item.Model.Assembly.AssemblyName,
                                    assemblyName,
                                    StringComparison.Ordinal)));
            if (match == null)
            {
                throw new InvalidOperationException(
                    "Avatar motion capture could not find carried equipment assembly '" +
                    assemblyName + "'.");
            }

            return match;
        }


        private static AvatarPresentation[] FindAvatars() =>
            UnityEngine.Object
                .FindObjectsByType<AvatarPresentation>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .OrderBy(
                    avatar => avatar.name,
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
                AvatarMotion bodyMotion,
                AvatarMotion equipmentMotion,
                string checkpoint,
                float bodyPhase,
                float equipmentPhase,
                EquipmentSlotSide? bodySide = null,
                string bodyVariant = "",
                string equipmentVariant = "")
            {
                BodyMotion = bodyMotion;
                EquipmentMotion = equipmentMotion;
                Checkpoint = checkpoint;
                BodyPhase = bodyPhase;
                EquipmentPhase = equipmentPhase;
                BodySide = bodySide;
                BodyVariant =
                    AvatarMotionClipCandidates.NormalizeVariant(
                        bodyVariant);
                EquipmentVariant =
                    AvatarMotionClipCandidates.NormalizeVariant(
                        equipmentVariant);
            }

            public AvatarMotion BodyMotion { get; }
            public AvatarMotion EquipmentMotion { get; }
            public string Checkpoint { get; }
            public float BodyPhase { get; }
            public float EquipmentPhase { get; }
            public EquipmentSlotSide? BodySide { get; }
            public string BodyVariant { get; }
            public string EquipmentVariant { get; }
        }
    }
}
