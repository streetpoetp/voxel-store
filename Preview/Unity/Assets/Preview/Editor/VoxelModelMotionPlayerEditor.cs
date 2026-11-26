using System;
using Hero.Voxel.Runtime.Assets;
using UnityEditor;
using UnityEngine;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor
{
    internal static class VoxelModelMotionInspectorClips
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

        internal static int IndexOf(
            string[] clipNames,
            string exactClipName)
        {
            if (clipNames == null ||
                string.IsNullOrWhiteSpace(exactClipName))
            {
                return -1;
            }

            for (int i = 0; i < clipNames.Length; i++)
            {
                if (string.Equals(
                        clipNames[i],
                        exactClipName,
                        StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }
    }

    [CustomEditor(typeof(VoxelModelMotionPlayer))]
    public sealed class VoxelModelMotionPlayerEditor :
        UnityEditor.Editor
    {
        private SerializedProperty modelProperty;
        private SerializedProperty compositionProperty;
        private SerializedProperty clipNameProperty;
        private SerializedProperty loopProperty;
        private SerializedProperty speedProperty;

        private void OnEnable()
        {
            modelProperty =
                serializedObject.FindProperty("model");
            compositionProperty =
                serializedObject.FindProperty("composition");
            clipNameProperty =
                serializedObject.FindProperty("clipName");
            loopProperty =
                serializedObject.FindProperty("loop");
            speedProperty =
                serializedObject.FindProperty("speed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                modelProperty,
                new GUIContent("Model"));
            EditorGUILayout.PropertyField(
                compositionProperty,
                new GUIContent("Composition"));

            VoxelRenderPresentation model =
                modelProperty.objectReferenceValue as
                    VoxelRenderPresentation;
            DrawExactClip(model != null ? model.Animation : null);

            EditorGUILayout.PropertyField(
                loopProperty,
                new GUIContent("Loop"));
            EditorGUILayout.PropertyField(
                speedProperty,
                new GUIContent("Speed"));

            bool changed = serializedObject.ApplyModifiedProperties();
            VoxelModelMotionPlayer player =
                (VoxelModelMotionPlayer)target;

            if (changed &&
                Application.isPlaying &&
                model != null &&
                !string.IsNullOrWhiteSpace(
                    clipNameProperty.stringValue))
            {
                player.SetPlaybackSettings(
                    clipNameProperty.stringValue,
                    loopProperty.boolValue,
                    speedProperty.floatValue);
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       model == null))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Play"))
                    player.PlayConfigured();
                if (GUILayout.Button("Stop"))
                    player.Stop();
                if (GUILayout.Button("Reset"))
                    player.ResetMotion();
                EditorGUILayout.EndHorizontal();

                float phase = player.CurrentNormalizedPhase;
                float sampled = EditorGUILayout.Slider(
                    "Phase",
                    phase,
                    0f,
                    1f);
                if (!Mathf.Approximately(sampled, phase) &&
                    !string.IsNullOrWhiteSpace(player.ClipName))
                {
                    player.SampleExact(
                        player.ClipName,
                        sampled);
                }
            }

            if (model != null)
            {
                EditorGUILayout.LabelField(
                    "Runtime Clip",
                    string.IsNullOrEmpty(player.CurrentClipName)
                        ? "(none)"
                        : player.CurrentClipName);
                EditorGUILayout.LabelField(
                    "Runtime State",
                    player.IsPlaying
                        ? "Playing"
                        : player.CurrentClipName.Length > 0
                            ? "Selected / Stopped"
                            : "Unselected");
                EditorGUILayout.LabelField(
                    "Role",
                    player.IsBody
                        ? "Body"
                        : player.Equipment != null
                            ? "Equipment"
                            : "Standalone");
            }
        }

        private void DrawExactClip(
            VoxelAnimationAsset animation)
        {
            string[] clipNames =
                VoxelModelMotionInspectorClips.GetClipNames(
                    animation);
            if (clipNames.Length == 0)
            {
                EditorGUILayout.PropertyField(
                    clipNameProperty,
                    new GUIContent("Exact Clip"));
                EditorGUILayout.HelpBox(
                    "No baked animation clips are available on the selected model.",
                    MessageType.Info);
                return;
            }

            int currentIndex =
                VoxelModelMotionInspectorClips.IndexOf(
                    clipNames,
                    clipNameProperty.stringValue);
            if (currentIndex < 0)
                currentIndex = 0;

            int selectedIndex = EditorGUILayout.Popup(
                "Exact Clip",
                currentIndex,
                clipNames);
            clipNameProperty.stringValue =
                clipNames[selectedIndex];
        }
    }
}
