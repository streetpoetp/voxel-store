using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor
{
    public sealed class VoxelModelMotionPreviewWindow :
        EditorWindow
    {
        private enum TargetScope
        {
            SelectedModel,
            AllModels
        }

        private TargetScope scope = TargetScope.SelectedModel;
        private bool loop;
        private float speed;

        [MenuItem("Voxel Store/Model Motion Preview")]
        public static void Open()
        {
            GetWindow<VoxelModelMotionPreviewWindow>(
                "Voxel Model Motion");
        }

        private void OnEnable()
        {
            loop = VoxelModelMotionPreviewPreferences.Loop;
            speed = VoxelModelMotionPreviewPreferences.Speed;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "Independent Model Playback",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Each body/equipment/standalone model owns its own exact clip and playback clock. This window never broadcasts one semantic motion across a composed avatar.",
                MessageType.Info);

            scope = (TargetScope)EditorGUILayout.EnumPopup(
                "Target",
                scope);

            VoxelModelMotionPlayer[] targets = FindTargets();
            DrawTargetSummary(targets);

            if (scope == TargetScope.SelectedModel &&
                targets.Length == 1)
            {
                DrawSelectedClip(targets[0]);
            }

            loop = EditorGUILayout.Toggle("Loop", loop);
            speed = EditorGUILayout.FloatField("Speed", speed);
            if (!float.IsFinite(speed) || speed < 0.05f)
                speed = 0.05f;

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(targets.Length == 0))
            {
                if (GUILayout.Button("Apply Playback Settings"))
                    ApplySettings(targets);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Play"))
                    Play(targets);
                if (GUILayout.Button("Stop"))
                    Stop(targets);
                if (GUILayout.Button("Reset"))
                    ResetTargets(targets);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawSelectedClip(
            VoxelModelMotionPlayer target)
        {
            string[] clips =
                VoxelModelMotionInspectorClips.GetClipNames(
                    target.Animation);
            if (clips.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Selected model has no baked animation clips.",
                    MessageType.Warning);
                return;
            }

            int current =
                VoxelModelMotionInspectorClips.IndexOf(
                    clips,
                    target.ClipName);
            if (current < 0)
                current = 0;

            int selected = EditorGUILayout.Popup(
                "Exact Clip",
                current,
                clips);
            if (selected != current ||
                !string.Equals(
                    target.ClipName,
                    clips[selected],
                    StringComparison.Ordinal))
            {
                Undo.RecordObject(target, "Change Voxel Model Clip");
                target.SetClip(clips[selected]);
                EditorUtility.SetDirty(target);
            }
        }

        private void ApplySettings(
            VoxelModelMotionPlayer[] targets)
        {
            SavePreferences();
            foreach (VoxelModelMotionPlayer target in targets)
            {
                Undo.RecordObject(
                    target,
                    "Apply Voxel Model Playback Settings");
                target.SetLoop(loop);
                target.SetSpeed(speed);
                EditorUtility.SetDirty(target);
            }
        }

        private void Play(VoxelModelMotionPlayer[] targets)
        {
            SavePreferences();
            foreach (VoxelModelMotionPlayer target in targets)
            {
                target.SetLoop(loop);
                target.SetSpeed(speed);
                target.PlayConfigured();
            }
        }

        private static void Stop(
            VoxelModelMotionPlayer[] targets)
        {
            foreach (VoxelModelMotionPlayer target in targets)
                target.Stop();
        }

        private static void ResetTargets(
            VoxelModelMotionPlayer[] targets)
        {
            foreach (VoxelModelMotionPlayer target in targets)
                target.ResetMotion();
        }

        private void SavePreferences()
        {
            VoxelModelMotionPreviewPreferences.Loop = loop;
            VoxelModelMotionPreviewPreferences.Speed = speed;
        }

        private VoxelModelMotionPlayer[] FindTargets()
        {
            if (scope == TargetScope.SelectedModel)
            {
                GameObject selected = Selection.activeGameObject;
                if (selected == null)
                    return Array.Empty<VoxelModelMotionPlayer>();

                VoxelModelMotionPlayer player =
                    selected.GetComponentInParent<VoxelModelMotionPlayer>(
                        true);
                return player != null
                    ? new[] { player }
                    : Array.Empty<VoxelModelMotionPlayer>();
            }

            return UnityEngine.Object
                .FindObjectsByType<VoxelModelMotionPlayer>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .OrderBy(x => x.name, StringComparer.Ordinal)
                .ThenBy(x => x.GetInstanceID())
                .ToArray();
        }

        private void DrawTargetSummary(
            VoxelModelMotionPlayer[] targets)
        {
            EditorGUILayout.LabelField(
                "Targets",
                targets.Length.ToString());

            if (scope == TargetScope.SelectedModel &&
                targets.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Select a model object that owns a VoxelModelMotionPlayer.",
                    MessageType.Warning);
                return;
            }

            if (targets.Length == 1)
            {
                VoxelModelMotionPlayer target = targets[0];
                EditorGUILayout.LabelField(
                    "Model",
                    target.Model != null
                        ? target.Model.name
                        : "(unconfigured)");
                EditorGUILayout.LabelField(
                    "Role",
                    target.IsBody
                        ? "Body"
                        : target.Equipment != null
                            ? "Equipment"
                            : "Standalone");
            }
            else if (targets.Length > 1)
            {
                EditorGUILayout.HelpBox(
                    "All Models replays each model's own configured exact clip. It does not select one shared clip.",
                    MessageType.None);
            }
        }
    }
}