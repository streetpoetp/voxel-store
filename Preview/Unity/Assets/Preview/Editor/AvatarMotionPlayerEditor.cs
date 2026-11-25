using System;
using System.Collections.Generic;
using Hero.Voxel.Runtime.Assets;
using UnityEditor;
using UnityEngine;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor
{
    internal readonly struct AvatarMotionInspectorSelection
    {
        public AvatarMotionInspectorSelection(
            AvatarMotion motion,
            EquipmentSlotSide? side,
            string variant)
        {
            Motion = motion;
            Side = side;
            Variant = variant ?? string.Empty;
        }

        public AvatarMotion Motion { get; }
        public EquipmentSlotSide? Side { get; }
        public string Variant { get; }
    }

    internal static class AvatarMotionInspectorClips
    {
        internal static string[] GetClipNames(
            VoxelAnimationAsset animation)
        {
            if (animation == null || animation.ClipCount == 0)
                return Array.Empty<string>();

            var names = new string[animation.ClipCount];
            for (int i = 0; i < names.Length; i++)
                names[i] = animation.GetClip(i).Name;
            return names;
        }

        internal static string ResolveCurrent(
            IReadOnlyList<string> clipNames,
            AvatarMotion motion,
            EquipmentSlotSide side,
            string variant)
        {
            if (clipNames == null || clipNames.Count == 0)
                return string.Empty;

            IReadOnlyList<string> candidates =
                AvatarMotionClipCandidates.Build(
                    motion,
                    side,
                    variant);
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                string candidate = candidates[candidateIndex];
                for (int clipIndex = 0;
                     clipIndex < clipNames.Count;
                     clipIndex++)
                {
                    if (string.Equals(
                            clipNames[clipIndex],
                            candidate,
                            StringComparison.Ordinal))
                    {
                        return candidate;
                    }
                }
            }

            return string.Empty;
        }

        internal static bool TryParse(
            string clipName,
            out AvatarMotionInspectorSelection selection)
        {
            selection = default;
            if (string.IsNullOrWhiteSpace(clipName))
                return false;

            string[] parts = clipName.Split('.');
            if (parts.Length is < 1 or > 3 ||
                !Enum.TryParse(
                    parts[0],
                    false,
                    out AvatarMotion motion))
            {
                return false;
            }

            EquipmentSlotSide? side = null;
            string variant = string.Empty;
            int index = 1;

            if (index < parts.Length &&
                Enum.TryParse(
                    parts[index],
                    false,
                    out EquipmentSlotSide parsedSide))
            {
                side = parsedSide;
                index++;
            }

            if (index < parts.Length)
            {
                try
                {
                    variant =
                        AvatarMotionClipCandidates.NormalizeVariant(
                            parts[index]);
                }
                catch (ArgumentException)
                {
                    return false;
                }

                index++;
            }

            if (index != parts.Length)
                return false;

            selection = new AvatarMotionInspectorSelection(
                motion,
                side,
                variant);
            return true;
        }
    }

    [CustomEditor(typeof(AvatarMotionPlayer))]
    public sealed class AvatarMotionPlayerEditor : UnityEditor.Editor
    {
        private SerializedProperty avatarProperty;
        private SerializedProperty motionProperty;
        private SerializedProperty driverSideProperty;
        private SerializedProperty variantProperty;
        private SerializedProperty loopProperty;
        private SerializedProperty speedProperty;

        private void OnEnable()
        {
            avatarProperty =
                serializedObject.FindProperty("avatar");
            motionProperty =
                serializedObject.FindProperty("motion");
            driverSideProperty =
                serializedObject.FindProperty("driverSide");
            variantProperty =
                serializedObject.FindProperty("variant");
            loopProperty =
                serializedObject.FindProperty("loop");
            speedProperty =
                serializedObject.FindProperty("speed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(
                avatarProperty,
                new GUIContent("Avatar"));

            AvatarPresentation presentation =
                avatarProperty.objectReferenceValue as
                    AvatarPresentation;
            VoxelAnimationAsset animation =
                presentation != null
                    ? presentation.BodyAnimation
                    : null;

            if (animation != null &&
                animation.ClipCount > 0)
            {
                DrawMotionClipPopup(animation);
            }
            else
            {
                EditorGUILayout.PropertyField(
                    motionProperty,
                    new GUIContent("Semantic Motion"));
                EditorGUILayout.PropertyField(
                    driverSideProperty,
                    new GUIContent("Driver Side"));
                EditorGUILayout.PropertyField(
                    variantProperty,
                    new GUIContent("Variant"));
                EditorGUILayout.HelpBox(
                    "No baked body animation is available, so exact motion clips cannot be discovered.",
                    MessageType.Info);
            }

            EditorGUILayout.PropertyField(
                loopProperty,
                new GUIContent("Loop"));
            EditorGUILayout.PropertyField(
                speedProperty,
                new GUIContent("Speed"));

            bool changed = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();

            if (!changed || !Application.isPlaying)
                return;

            var player = (AvatarMotionPlayer)target;
            if (!player.isActiveAndEnabled ||
                player.Avatar == null)
            {
                return;
            }

            player.Play(
                player.Motion,
                player.Loop,
                player.Speed);
        }

        private void DrawMotionClipPopup(
            VoxelAnimationAsset animation)
        {
            string[] clipNames =
                AvatarMotionInspectorClips.GetClipNames(
                    animation);
            AvatarMotion motion =
                (AvatarMotion)motionProperty.intValue;
            EquipmentSlotSide side =
                (EquipmentSlotSide)driverSideProperty.intValue;
            string variant = variantProperty.stringValue;

            string current =
                AvatarMotionInspectorClips.ResolveCurrent(
                    clipNames,
                    motion,
                    side,
                    variant);
            int currentIndex =
                Array.IndexOf(clipNames, current);
            if (currentIndex < 0)
                currentIndex = 0;

            int selectedIndex = EditorGUILayout.Popup(
                "Motion Clip",
                currentIndex,
                clipNames);

            if (selectedIndex >= 0 &&
                selectedIndex < clipNames.Length &&
                selectedIndex != currentIndex &&
                AvatarMotionInspectorClips.TryParse(
                    clipNames[selectedIndex],
                    out AvatarMotionInspectorSelection
                        selection))
            {
                motionProperty.intValue =
                    (int)selection.Motion;
                if (selection.Side.HasValue)
                {
                    driverSideProperty.intValue =
                        (int)selection.Side.Value;
                }

                variantProperty.stringValue =
                    selection.Variant;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    motionProperty,
                    new GUIContent("Semantic Motion"));
                EditorGUILayout.PropertyField(
                    driverSideProperty,
                    new GUIContent("Driver Side"));
                EditorGUILayout.TextField(
                    "Variant",
                    string.IsNullOrEmpty(
                        variantProperty.stringValue)
                        ? "Base"
                        : variantProperty.stringValue);
            }
        }
    }
}
