using UnityEngine;

namespace VoxelStore.Preview.Runtime
{
    /// <summary>
    /// Preview-owned semantic model frame and Hero.Voxel source-axis mapping.
    /// The provider owns bake behavior; this consumer contract keeps preview
    /// composition deterministic without requiring a dirty provider package.
    /// </summary>
    public static class VoxelStoreModelFrame
    {
        public static readonly Vector3 Right = Vector3.right;
        public static readonly Vector3 Up = Vector3.up;
        public static readonly Vector3 Forward = Vector3.back;

        public static Vector3 SourceToUnityVector(Vector3 source) =>
            new Vector3(
                source.x,
                source.z,
                source.y);

        public static Vector3 UnityToSourceVector(Vector3 unity) =>
            new Vector3(
                unity.x,
                unity.z,
                unity.y);
    }
}
