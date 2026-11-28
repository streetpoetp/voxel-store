using System;
using System.IO;
using System.Linq;
using Hero.Voxel.Actor.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VoxelStore.Preview.Editor
{
    [InitializeOnLoad]
    public static class PreviewGridRuntimeProofRunner
    {
        private const string RunKey =
            "VoxelStore.Preview.RuntimeGridProof.Run";
        private const string StartedKey =
            "VoxelStore.Preview.RuntimeGridProof.Started";
        private const string ResultKey =
            "VoxelStore.Preview.RuntimeGridProof.Result";
        private const string ErrorKey =
            "VoxelStore.Preview.RuntimeGridProof.Error";

        static PreviewGridRuntimeProofRunner()
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
                        "Voxel Store runtime grid proof must start outside Play Mode.");
                }

                PreviewGridPlayModeOrchestrator.ClearPendingForTesting();
                VoxelStorePreviewCommand.BakeAllForBatch();
                EditorSceneManager.OpenScene(
                    VoxelStorePreviewPaths.ScenePath,
                    OpenSceneMode.Single);

                SessionState.SetBool(RunKey, true);
                SessionState.SetBool(StartedKey, false);
                SessionState.SetString(ResultKey, string.Empty);
                SessionState.SetString(ErrorKey, string.Empty);
                EditorApplication.isPlaying = true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ClearSessionState();
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
                    RunProofInPlayMode();
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

        private static void RunProofInPlayMode()
        {
            Exception failure = null;
            try
            {
                PreviewPlanContract plan = PreviewPlanClient.Load();
                PreviewGridBuildResult result =
                    PreviewGridRuntimeBuilder.Build(plan);
                Validate(plan, result);

                GameObject firstGrid = result.Grid;
                PreviewGridBuildResult rebuilt =
                    PreviewGridRuntimeBuilder.Build(plan);
                Validate(plan, rebuilt);
                if (firstGrid != null)
                {
                    throw new InvalidOperationException(
                        "Runtime grid rebuild did not dispose the previous grid.");
                }

                WriteReport(plan, rebuilt);
                SessionState.SetString(ResultKey, "success");
            }
            catch (Exception exception)
            {
                failure = exception;
                Debug.LogException(exception);
                SessionState.SetString(ResultKey, "failure");
                SessionState.SetString(
                    ErrorKey,
                    exception.ToString());
            }
            finally
            {
                PreviewGridRuntimeBuilder.DisposeActive();
                EditorApplication.isPlaying = false;
            }

            if (failure == null)
            {
                Debug.Log(
                    "VOXEL_RUNTIME_GRID_PROOF_PASS");
            }
        }

        private static void Validate(
            PreviewPlanContract plan,
            PreviewGridBuildResult result)
        {
            if (plan == null || plan.Units == null)
                throw new InvalidOperationException(
                    "Runtime grid proof plan is unavailable.");
            if (result == null)
                throw new InvalidOperationException(
                    "Runtime grid proof result is unavailable.");

            string[] expectedIds =
                plan.Units.Select(x => x.Id).ToArray();
            if (!expectedIds.SequenceEqual(
                    result.EntryIds,
                    StringComparer.Ordinal))
            {
                throw new InvalidOperationException(
                    "Runtime grid identity/order differs from preview-plan.");
            }

            int avatarCount = plan.Units.Count(x =>
                string.Equals(
                    x.Kind,
                    "avatar",
                    StringComparison.Ordinal));
            int equipmentCount = plan.Units.Count(x =>
                string.Equals(
                    x.Kind,
                    "equipment",
                    StringComparison.Ordinal));
            int expectedCarried =
                avatarCount * Math.Min(2, equipmentCount);

            if (result.CarriedEquipmentCount != expectedCarried)
            {
                throw new InvalidOperationException(
                    "Runtime grid carried-equipment count mismatch. expected=" +
                    expectedCarried +
                    " actual=" +
                    result.CarriedEquipmentCount);
            }

            if (PreviewGridLayout.CountXZOverlaps(
                    result.WorldBounds) != 0)
            {
                throw new InvalidOperationException(
                    "Runtime grid contains overlapping world bounds.");
            }

            if (result.SubmittedPartCount <= 0)
            {
                throw new InvalidOperationException(
                    "Runtime grid submitted no voxel parts.");
            }

            int expectedHosts =
                plan.Units.Length + expectedCarried;
            if (result.InspectionHosts.Length != expectedHosts)
            {
                throw new InvalidOperationException(
                    "Runtime grid inspection-host count mismatch. expected=" +
                    expectedHosts +
                    " actual=" +
                    result.InspectionHosts.Length);
            }

            foreach (GameObject host in result.InspectionHosts)
            {
                if (host == null)
                {
                    throw new InvalidOperationException(
                        "Runtime grid contains a destroyed inspection host.");
                }

                if (host.GetComponents<MonoBehaviour>().Length != 0)
                {
                    throw new InvalidOperationException(
                        "Runtime grid inspection host owns a MonoBehaviour: " +
                        host.name);
                }

                if (!VoxelRuntimeInspectionController.TryGetSession(
                        host,
                        out _))
                {
                    throw new InvalidOperationException(
                        "Runtime grid host is not registered with Hero.Voxel inspection: " +
                        host.name);
                }
            }

            GameObject carriedHost =
                result.InspectionHosts.FirstOrDefault(x =>
                    x != null &&
                    x.transform.parent != null &&
                    x.transform.parent.gameObject != result.Grid &&
                    x.transform.parent.parent != null &&
                    x.transform.parent.parent.gameObject == result.Grid);
            if (carriedHost == null)
            {
                throw new InvalidOperationException(
                    "Runtime grid has no carried-equipment inspection host.");
            }

            GameObject avatarHost =
                carriedHost.transform.parent.gameObject;
            if (!VoxelRuntimeInspectionController.TryGetSession(
                    avatarHost,
                    out VoxelRuntimeInspectionSession avatarSession) ||
                !VoxelRuntimeInspectionController.TryGetSession(
                    carriedHost,
                    out VoxelRuntimeInspectionSession equipmentSession))
            {
                throw new InvalidOperationException(
                    "Runtime grid could not resolve avatar/equipment inspection sessions.");
            }

            if (!ReferenceEquals(
                    avatarSession.Actor,
                    equipmentSession.Actor))
            {
                throw new InvalidOperationException(
                    "Carried equipment inspection is not associated with its owning Actor.");
            }

            bool avatarHadSelection =
                avatarSession.Track.HasSelection;
            float avatarPhase =
                avatarHadSelection
                    ? avatarSession.Track.NormalizedPhase
                    : 0f;

            equipmentSession.SelectClip(0);
            equipmentSession.SetNormalizedTime(0.5f);

            if (avatarSession.Track.HasSelection !=
                avatarHadSelection)
            {
                throw new InvalidOperationException(
                    "Equipment inspection changed root-track selection.");
            }

            if (avatarHadSelection &&
                Mathf.Abs(
                    avatarSession.Track.NormalizedPhase -
                    avatarPhase) > 0.0001f)
            {
                throw new InvalidOperationException(
                    "Equipment inspection changed root-track phase.");
            }
        }

        private static void WriteReport(
            PreviewPlanContract plan,
            PreviewGridBuildResult result)
        {
            string projectRoot =
                Path.GetDirectoryName(Application.dataPath) ??
                throw new InvalidOperationException(
                    "Unity project root could not be resolved.");
            string reportDirectory = Path.Combine(
                projectRoot,
                "Library",
                "VoxelStorePreview");
            Directory.CreateDirectory(reportDirectory);

            var report = new RuntimeGridProofReport
            {
                PlanCount = plan.Units.Length,
                GridCount = result.EntryIds.Length,
                InspectionHostCount =
                    result.InspectionHosts.Length,
                AnimationCount = result.AnimationCount,
                CarriedEquipmentCount =
                    result.CarriedEquipmentCount,
                OverlapCount =
                    PreviewGridLayout.CountXZOverlaps(
                        result.WorldBounds),
                SubmittedPartCount =
                    result.SubmittedPartCount,
                UnitIds = result.EntryIds
            };

            File.WriteAllText(
                Path.Combine(
                    reportDirectory,
                    "runtime-grid-report.json"),
                JsonUtility.ToJson(report, true));
        }

        private static void ClearSessionState()
        {
            SessionState.EraseBool(RunKey);
            SessionState.EraseBool(StartedKey);
            SessionState.EraseString(ResultKey);
            SessionState.EraseString(ErrorKey);
        }

        [Serializable]
        private sealed class RuntimeGridProofReport
        {
            public int PlanCount;
            public int GridCount;
            public int InspectionHostCount;
            public int AnimationCount;
            public int CarriedEquipmentCount;
            public int OverlapCount;
            public int SubmittedPartCount;
            public string[] UnitIds;
        }
    }
}