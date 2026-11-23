using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hero.Consumer.Editor;
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
            new CapturePoint(AvatarMotion.WalkForward, "walk-launch", 0.20f),
            new CapturePoint(AvatarMotion.WalkForward, "walk-apex", 0.45f),
            new CapturePoint(AvatarMotion.WalkForward, "walk-land", 0.70f),
            new CapturePoint(AvatarMotion.Attack, "attack-anticipation", 0.22f),
            new CapturePoint(AvatarMotion.Attack, "attack-contact", 0.50f),
            new CapturePoint(AvatarMotion.Attack, "attack-follow-through", 0.70f),
            new CapturePoint(AvatarMotion.Hit, "hit-recoil", 0.28f),
            new CapturePoint(AvatarMotion.Hit, "hit-recover", 0.78f),
            new CapturePoint(AvatarMotion.Death, "death-fall", 0.55f),
            new CapturePoint(AvatarMotion.Death, "death-fallen", 0.88f)
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
                    ConfigureCaptureView();
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
                        player.SampleMotion(
                            point.Motion,
                            point.NormalizedTime);
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

        private static void ConfigureCaptureView()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                throw new InvalidOperationException(
                    "Avatar motion capture requires the runtime gallery Main Camera.");
            }

            Renderer[] renderers =
                UnityEngine.Object.FindObjectsByType<Renderer>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException(
                    "Avatar motion capture requires visible gallery renderers.");
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            float distance = Mathf.Max(
                20f,
                Vector3.Distance(
                    camera.transform.position,
                    bounds.center));
            camera.transform.rotation =
                Quaternion.Euler(30f, 35f, 0f);
            camera.transform.position =
                bounds.center -
                camera.transform.forward * distance;

            // The oblique proof view gives semantic Forward (-Z) a visible
            // screen-space component, so attack travel can be judged from
            // pixels instead of being hidden along the camera axis.
            camera.orthographicSize =
                Mathf.Max(
                    camera.orthographicSize * 1.08f,
                    bounds.extents.magnitude * 0.72f);
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
                float normalizedTime)
            {
                Motion = motion;
                Checkpoint = checkpoint;
                NormalizedTime = normalizedTime;
            }

            public AvatarMotion Motion { get; }
            public string Checkpoint { get; }
            public float NormalizedTime { get; }
        }
    }
}
