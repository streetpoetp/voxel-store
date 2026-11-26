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
    public static class MutableStreamedActorCaptureRunner
    {
        private const string RunKey =
            "VoxelStore.MutableStreamedActorCapture.Run";
        private const string StartedKey =
            "VoxelStore.MutableStreamedActorCapture.Started";
        private const string ResultKey =
            "VoxelStore.MutableStreamedActorCapture.Result";
        private const string ErrorKey =
            "VoxelStore.MutableStreamedActorCapture.Error";
        private const string OutputDirectory =
            "Library/Capture/MutableStreamedActorGallery";

        static MutableStreamedActorCaptureRunner()
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
                        "Mutable streamed actor capture must start outside Play Mode.");
                }

                string projectRoot =
                    Path.GetDirectoryName(Application.dataPath) ??
                    throw new InvalidOperationException(
                        "Unity project root could not be resolved.");
                string outputRoot = Path.Combine(
                    projectRoot,
                    OutputDirectory.Replace(
                        '/',
                        Path.DirectorySeparatorChar));

                if (Directory.Exists(outputRoot))
                    Directory.Delete(outputRoot, recursive: true);
                Directory.CreateDirectory(outputRoot);

                VoxelStorePreviewCommand.BakeAllForBatch();
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
                    MutableStreamedActorPreview demo = FindDemo();
                    if (demo == null)
                        return;

                    SessionState.SetBool(StartedKey, true);
                    _ = CaptureAllAsync(demo);
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

        private static async Task CaptureAllAsync(
            MutableStreamedActorPreview demo)
        {
            Exception failure = null;
            try
            {
                HideOtherGalleryEntries(demo);
                demo.transform.position = Vector3.zero;
                demo.transform.rotation = Quaternion.identity;
                demo.transform.localScale = Vector3.one;

                await demo.InitializeRootAsync();

                await demo.AttachInitialAsync(
                    rootPhase: 0.20f,
                    primaryPhase: 0.75f,
                    secondaryPhase: 0.40f);
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-attached",
                    validateGrip: true);

                demo.DetachSecondaryPreservingPlayback();
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-detached",
                    validateGrip: true);

                demo.RebindPrimaryPreservingPlayback();
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-rebound",
                    validateGrip: true);

                await demo.ReplacePrimaryAsync();
                demo.SampleCurrentTopology(
                    rootPhase: 0.20f,
                    primaryPhase: 0.65f,
                    secondaryPhase: 0f);
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-replaced",
                    validateGrip: true);

                await demo.ReleaseAndReacquireRootOnlyAsync(0.50f);
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-reacquired-root-only",
                    validateGrip: false);

                await demo.ReattachAfterReacquireAsync(
                    rootPhase: 0.35f,
                    primaryPhase: 0.80f);
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-reattached",
                    validateGrip: true);
            }
            catch (Exception exception)
            {
                failure = exception;
                Debug.LogException(exception);
            }
            finally
            {
                demo?.DisposeProviderState();

                SessionState.SetString(
                    ResultKey,
                    failure == null ? "success" : "failure");
                SessionState.SetString(
                    ErrorKey,
                    failure?.ToString() ?? string.Empty);

                EditorApplication.isPlaying = false;
            }
        }

        private static async Task CaptureCheckpointAsync(
            MutableStreamedActorPreview demo,
            string checkpoint,
            bool validateGrip)
        {
            if (validateGrip)
                demo.ValidateAttachmentGrip();

            Debug.Log(
                "Mutable streamed checkpoint " +
                checkpoint +
                ": topologyRevision=" +
                demo.TopologyRevision +
                ", attachments=" +
                demo.AttachmentCount +
                ", mutableAttachments=" +
                demo.MutableAttachmentCount + ".");

            Bounds bounds = demo.GetWorldBounds();
            ConfigureCaptureView(bounds);
            demo.SubmitNow();

            await GameViewCapture.CaptureAsync(
                OutputDirectory,
                checkpoint,
                stableFrames: 1);
        }

        private static MutableStreamedActorPreview FindDemo()
        {
            return Resources
                .FindObjectsOfTypeAll<MutableStreamedActorPreview>()
                .SingleOrDefault(x =>
                    x != null &&
                    x.gameObject.scene.IsValid() &&
                    x.gameObject.scene.isLoaded);
        }

        private static void HideOtherGalleryEntries(
            MutableStreamedActorPreview demo)
        {
            GameObject root = GameObject.Find(
                VoxelStoreRuntimeGalleryBuilder.RootName);
            if (root == null)
            {
                throw new InvalidOperationException(
                    "Mutable streamed actor capture requires the runtime gallery root.");
            }

            for (int i = 0; i < root.transform.childCount; i++)
            {
                Transform child = root.transform.GetChild(i);
                if (ReferenceEquals(child, demo.transform))
                    continue;
                child.gameObject.SetActive(false);
            }
        }

        private static void ConfigureCaptureView(Bounds bounds)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                throw new InvalidOperationException(
                    "Mutable streamed actor capture requires the runtime gallery Main Camera.");
            }

            camera.orthographic = true;
            camera.transform.rotation =
                Quaternion.Euler(24f, 35f, 0f);

            float distance = Mathf.Max(
                8f,
                bounds.extents.magnitude * 3f);
            camera.transform.position =
                bounds.center -
                camera.transform.forward * distance;
            camera.orthographicSize =
                RequiredOrthographicSize(camera, bounds) * 1.18f;
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
                        Vector3 corner =
                            c + Vector3.Scale(
                                e,
                                new Vector3(x, y, z));
                        Vector3 local =
                            camera.transform.InverseTransformPoint(
                                corner);
                        maxX = Mathf.Max(
                            maxX,
                            Mathf.Abs(local.x));
                        maxY = Mathf.Max(
                            maxY,
                            Mathf.Abs(local.y));
                    }
                }
            }

            float aspect = Mathf.Max(0.01f, camera.aspect);
            return Mathf.Max(maxY, maxX / aspect);
        }

        private static void ClearSessionState()
        {
            SessionState.EraseBool(RunKey);
            SessionState.EraseBool(StartedKey);
            SessionState.EraseString(ResultKey);
            SessionState.EraseString(ErrorKey);
        }
    }
}
