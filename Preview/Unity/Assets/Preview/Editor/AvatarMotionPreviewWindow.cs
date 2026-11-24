using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor
{
    public sealed class AvatarMotionPreviewWindow : EditorWindow
    {
        private enum TargetScope
        {
            AllAvatars = 0,
            SelectedAvatar = 1
        }

        private AvatarMotion motion;
        private string rootVariant;
        private string equipmentVariant;
        private string variantError;
        private bool loop;
        private float speed;
        private TargetScope scope;

        [MenuItem("Voxel Store/Avatar Motion Preview")]
        public static void Open()
        {
            GetWindow<AvatarMotionPreviewWindow>(
                false,
                "Avatar Motion Preview",
                true);
        }

        private void OnEnable()
        {
            motion = AvatarMotionPreviewPreferences.Motion;
            rootVariant = AvatarMotionPreviewPreferences.RootVariant;
            equipmentVariant =
                AvatarMotionPreviewPreferences.EquipmentVariant;
            loop = AvatarMotionPreviewPreferences.Loop;
            speed = AvatarMotionPreviewPreferences.Speed;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "Runtime Gallery Motion",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Select one semantic motion for direct comparison. " +
                "The Editor sends commands; animation itself runs only " +
                "through AvatarMotionPlayer during Play Mode.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            motion = (AvatarMotion)EditorGUILayout.EnumPopup(
                "Motion",
                motion);
            rootVariant = EditorGUILayout.DelayedTextField(
                "Body Variant",
                rootVariant);
            equipmentVariant = EditorGUILayout.DelayedTextField(
                "Equipment Variant",
                equipmentVariant);
            loop = EditorGUILayout.Toggle(
                "Loop",
                loop);
            speed = EditorGUILayout.Slider(
                "Speed",
                speed,
                0.10f,
                3f);
            scope = (TargetScope)EditorGUILayout.EnumPopup(
                "Target",
                scope);
            bool selectionChanged =
                EditorGUI.EndChangeCheck();

            if (selectionChanged)
            {
                try
                {
                    rootVariant =
                        AvatarMotionClipCandidates.NormalizeVariant(
                            rootVariant);
                    equipmentVariant =
                        AvatarMotionClipCandidates.NormalizeVariant(
                            equipmentVariant);
                    variantError = string.Empty;
                    SavePreferences();

                    if (EditorApplication.isPlaying)
                        PlayTargets();
                    else
                        ApplyDefaultsToScene();
                }
                catch (ArgumentException exception)
                {
                    variantError = exception.Message;
                }
            }

            if (!string.IsNullOrEmpty(variantError))
            {
                EditorGUILayout.HelpBox(
                    variantError,
                    MessageType.Error);
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Play"))
                    PlayTargets();
                if (GUILayout.Button("Stop"))
                    StopTargets();
                if (GUILayout.Button("Reset"))
                    ResetTargets();
            }

            EditorGUILayout.Space();
            DrawTargetSummary();
        }

        private void SavePreferences()
        {
            AvatarMotionPreviewPreferences.Motion = motion;
            AvatarMotionPreviewPreferences.RootVariant = rootVariant;
            AvatarMotionPreviewPreferences.EquipmentVariant =
                equipmentVariant;
            AvatarMotionPreviewPreferences.Loop = loop;
            AvatarMotionPreviewPreferences.Speed = speed;
        }

        private void ApplyDefaultsToScene()
        {
            AvatarMotionPlayer[] targets = FindTargets();
            foreach (AvatarMotionPlayer target in targets)
            {
                Undo.RecordObject(
                    target,
                    "Set Avatar Motion Preview");
                target.SetMotionSettings(
                    motion,
                    loop,
                    speed,
                    rootVariant);
                target.SetEquipmentVariant(equipmentVariant);
                EditorUtility.SetDirty(target);
            }
        }

        private void PlayTargets()
        {
            SavePreferences();
            AvatarMotionPlayer[] targets = FindTargets();

            if (!EditorApplication.isPlaying)
            {
                ApplyDefaultsToScene();
                return;
            }

            foreach (AvatarMotionPlayer target in targets)
            {
                target.SetMotionSettings(
                    motion,
                    loop,
                    speed,
                    rootVariant);
                target.SetEquipmentVariant(equipmentVariant);
                target.Play(
                    motion,
                    loop,
                    speed);
            }
        }

        private void StopTargets()
        {
            foreach (AvatarMotionPlayer target in FindTargets())
                target.Stop();
        }

        private void ResetTargets()
        {
            foreach (AvatarMotionPlayer target in FindTargets())
                target.ResetMotion();
        }

        private AvatarMotionPlayer[] FindTargets()
        {
            if (scope == TargetScope.SelectedAvatar)
            {
                GameObject selected = Selection.activeGameObject;
                AvatarMotionPlayer player =
                    selected != null
                        ? selected.GetComponentInParent<AvatarMotionPlayer>()
                        : null;
                return player != null
                    ? new[] { player }
                    : Array.Empty<AvatarMotionPlayer>();
            }

            return UnityEngine.Object
                .FindObjectsByType<AvatarMotionPlayer>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .OrderBy(
                    x => x.name,
                    StringComparer.Ordinal)
                .ToArray();
        }

        private void DrawTargetSummary()
        {
            AvatarMotionPlayer[] targets = FindTargets();
            EditorGUILayout.LabelField(
                "Targets",
                targets.Length.ToString());

            if (scope == TargetScope.SelectedAvatar &&
                targets.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Select an avatar or one of its children.",
                    MessageType.Warning);
            }
        }
    }
}
