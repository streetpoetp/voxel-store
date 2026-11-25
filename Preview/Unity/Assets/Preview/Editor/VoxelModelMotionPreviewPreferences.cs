namespace VoxelStore.Preview.Editor
{
    internal static class VoxelModelMotionPreviewPreferences
    {
        private const string Prefix =
            "VoxelStore.Preview.ModelMotion.";
        private const string LoopKey =
            Prefix + "Loop";
        private const string SpeedKey =
            Prefix + "Speed";

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