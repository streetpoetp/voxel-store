using UnityEditor;
using UnityEngine;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor
{
    [CustomEditor(typeof(AvatarMotionPlayer))]
    public sealed class AvatarMotionPlayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            bool changed = DrawDefaultInspector();
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
    }
}
