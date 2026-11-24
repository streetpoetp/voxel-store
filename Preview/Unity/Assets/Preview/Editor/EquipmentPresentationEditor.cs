using UnityEditor;
using UnityEngine;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor
{
    [CustomEditor(typeof(EquipmentPresentation))]
    public sealed class EquipmentPresentationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            bool changed = DrawDefaultInspector();
            if (!changed || !Application.isPlaying)
                return;

            var presentation = (EquipmentPresentation)target;
            AvatarMotionPlayer player =
                presentation.GetComponentInParent<AvatarMotionPlayer>();
            if (player == null || !player.isActiveAndEnabled)
                return;

            player.Play(
                player.Motion,
                player.Loop,
                player.Speed);
        }
    }
}
