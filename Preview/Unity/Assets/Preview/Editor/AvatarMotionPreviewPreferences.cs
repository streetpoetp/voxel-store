using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor
{
    internal static class AvatarMotionPreviewPreferences
    {
        private const string Prefix =
            "VoxelStore.Preview.AvatarMotion.";
        private const string MotionKey =
            Prefix + "Motion";
        private const string LoopKey =
            Prefix + "Loop";
        private const string SpeedKey =
            Prefix + "Speed";

        public static AvatarMotion Motion
        {
            get => (AvatarMotion)UnityEditor.EditorPrefs.GetInt(
                MotionKey,
                (int)AvatarMotion.Attack);
            set => UnityEditor.EditorPrefs.SetInt(
                MotionKey,
                (int)value);
        }

        public static bool Loop
        {
            get => UnityEditor.EditorPrefs.GetBool(
                LoopKey,
                true);
            set => UnityEditor.EditorPrefs.SetBool(
                LoopKey,
                value);
        }

        public static float Speed
        {
            get => UnityEditor.EditorPrefs.GetFloat(
                SpeedKey,
                1f);
            set => UnityEditor.EditorPrefs.SetFloat(
                SpeedKey,
                value);
        }
    }
}
