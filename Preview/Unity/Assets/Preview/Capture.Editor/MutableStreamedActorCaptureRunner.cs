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
                    primaryPhase: 0.12f,
                    secondaryPhase: 0.40f);
                RequireInfluence(
                    demo,
                    1f,
                    "mutable-attached");
                RequireDistanceAtMost(
                    demo,
                    0.001f,
                    "mutable-attached");
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-attached",
                    validateReferences: true);

                demo.transform.position +=
                    new Vector3(1.75f, 0.25f, -0.5f);
                demo.SampleCurrentTopology(
                    rootPhase: 0.55f,
                    primaryPhase: 0.50f,
                    secondaryPhase: 0.40f);
                RequireInfluence(
                    demo,
                    0f,
                    "mutable-released-moving-actor");
                float releasedDistance =
                    RequireDistanceAtLeast(
                        demo,
                        0.05f,
                        "mutable-released-moving-actor");
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-released-moving-actor",
                    validateReferences: true);

                demo.RebindPrimaryPreservingPlayback();
                RequireInfluence(
                    demo,
                    0f,
                    "mutable-rebound-released");
                RequireDistanceAtLeast(
                    demo,
                    0.05f,
                    "mutable-rebound-released");
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-rebound-released",
                    validateReferences: true);

                demo.SampleCurrentTopology(
                    rootPhase: 0.55f,
                    primaryPhase: 0.82f,
                    secondaryPhase: 0.40f);
                RequireInfluence(
                    demo,
                    0.3f,
                    "mutable-return-early");
                float earlyReturnDistance =
                    demo.PrimaryReferencePositionDistance;
                RequireOrderedDistance(
                    releasedDistance,
                    earlyReturnDistance,
                    "released -> early return");
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-return-early",
                    validateReferences: true);

                demo.SampleCurrentTopology(
                    rootPhase: 0.55f,
                    primaryPhase: 0.92f,
                    secondaryPhase: 0.40f);
                RequireInfluence(
                    demo,
                    0.72f,
                    "mutable-return-late");
                float lateReturnDistance =
                    demo.PrimaryReferencePositionDistance;
                RequireOrderedDistance(
                    earlyReturnDistance,
                    lateReturnDistance,
                    "early -> late return");
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-return-late",
                    validateReferences: true);

                demo.SampleCurrentTopology(
                    rootPhase: 0.55f,
                    primaryPhase: 1f,
                    secondaryPhase: 0.40f);
                RequireInfluence(
                    demo,
                    1f,
                    "mutable-rejoined");
                RequireDistanceAtMost(
                    demo,
                    0.001f,
                    "mutable-rejoined");
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-rejoined",
                    validateReferences: true);

                demo.DetachSecondaryPreservingPlayback();
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-detached",
                    validateReferences: true);

                await demo.ReplacePrimaryAsync();
                demo.SampleCurrentTopology(
                    rootPhase: 0.20f,
                    primaryPhase: 0.65f,
                    secondaryPhase: 0f);
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-replaced",
                    validateReferences: true);

                await demo.ReleaseAndReacquireRootOnlyAsync(0.50f);
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-reacquired-root-only",
                    validateReferences: false);

                await demo.ReattachAfterReacquireAsync(
                    rootPhase: 0.35f,
                    primaryPhase: 0.80f);
                await CaptureCheckpointAsync(
                    demo,
                    "mutable-reattached",
                    validateReferences: true);            }
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

        private static void RequireInfluence(
            MutableStreamedActorPreview demo,
            float expected,
            string checkpoint)
        {
            float actual = demo.PrimaryReferenceInfluence;
            if (!Mathf.Approximately(actual, expected))
            {
                throw new InvalidOperationException(
                    "Mutable streamed reference influence mismatch at " +
                    checkpoint +
                    ": expected=" + expected.ToString("R") +
                    ", actual=" + actual.ToString("R") + ".");
            }
        }

        private static float RequireDistanceAtLeast(
            MutableStreamedActorPreview demo,
            float minimum,
            string checkpoint)
        {
            float distance =
                demo.PrimaryReferencePositionDistance;
            if (!(distance >= minimum))
            {
                throw new InvalidOperationException(
                    "Mutable streamed released distance is too small at " +
                    checkpoint +
                    ": distance=" + distance.ToString("R") +
                    ", minimum=" + minimum.ToString("R") + ".");
            }

            return distance;
        }

        private static void RequireDistanceAtMost(
            MutableStreamedActorPreview demo,
            float maximum,
            string checkpoint)
        {
            float distance =
                demo.PrimaryReferencePositionDistance;
            if (distance > maximum)
            {
                throw new InvalidOperationException(
                    "Mutable streamed reference distance is too large at " +
                    checkpoint +
                    ": distance=" + distance.ToString("R") +
                    ", maximum=" + maximum.ToString("R") + ".");
            }
        }

        private static void RequireOrderedDistance(
            float earlier,
            float later,
            string label)
        {
            if (!(later > 0f && later < earlier))
            {
                throw new InvalidOperationException(
                    "Mutable streamed return distance did not decrease for " +
                    label +
                    ": earlier=" + earlier.ToString("R") +
                    ", later=" + later.ToString("R") + ".");
            }
        }

        private static async Task CaptureCheckpointAsync(
            MutableStreamedActorPreview demo,
            string checkpoint,
            bool validateReferences)
        {
            if (validateReferences)
                demo.ValidateAttachmentReferences();

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
