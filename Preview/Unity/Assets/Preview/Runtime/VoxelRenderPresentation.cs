using System;
using Hero.Voxel.Render.Runtime.Unity;
using Hero.Voxel.Runtime.Assets;
using UnityEngine;

namespace VoxelStore.Preview.Runtime
{
    [DisallowMultipleComponent]
    public sealed class VoxelRenderPresentation : MonoBehaviour
    {
        [SerializeField] private VoxelRenderWorld world;
        [SerializeField] private Transform modelRoot;
        [SerializeField] private VoxelAssemblyAsset assembly;
        [SerializeField] private VoxelAnimationAsset animationAsset;
        [SerializeField] private bool externallyOwnedLifecycle;

        private VoxelRenderInstance instance;

        public VoxelRenderWorld World => world;
        public Transform ModelRoot => modelRoot;
        public VoxelAssemblyAsset Assembly => assembly;
        public VoxelAnimationAsset Animation => animationAsset;
        public VoxelRenderInstance Instance =>
            instance != null && instance.IsValid ? instance : null;
        public bool IsInitialized => Instance != null;
        public bool ExternallyOwnedLifecycle => externallyOwnedLifecycle;

        public void Configure(
            VoxelRenderWorld renderWorld,
            Transform root,
            VoxelAssemblyAsset assemblyAsset,
            VoxelAnimationAsset animation,
            bool externalLifecycle = false)
        {
            ReleaseInstance();

            world = renderWorld != null
                ? renderWorld
                : throw new ArgumentNullException(nameof(renderWorld));
            modelRoot = root != null
                ? root
                : throw new ArgumentNullException(nameof(root));
            assembly = assemblyAsset != null
                ? assemblyAsset
                : throw new ArgumentNullException(nameof(assemblyAsset));
            animationAsset = animation != null
                ? animation
                : throw new ArgumentNullException(nameof(animation));

            if (!ReferenceEquals(animationAsset.Assembly, assembly))
            {
                throw new ArgumentException(
                    "Voxel animation asset must target the configured assembly.",
                    nameof(animation));
            }

            externallyOwnedLifecycle = externalLifecycle;
            if (Application.isPlaying && !externallyOwnedLifecycle)
                EnsureInstance();
        }

        public void SetExternallyOwnedLifecycle(bool value)
        {
            externallyOwnedLifecycle = value;
            if (!value &&
                Application.isPlaying &&
                isActiveAndEnabled)
            {
                EnsureInstance();
            }
        }

        public VoxelRenderInstance EnsureInstance()
        {
            RequireConfigured();
            if (instance == null || !instance.IsValid)
                instance = world.Spawn(assembly, animationAsset);

            SyncWorldMatrix();
            return instance;
        }

        public void SyncWorldMatrix()
        {
            if (instance == null || !instance.IsValid)
                return;

            instance.SetWorldMatrix(modelRoot.localToWorldMatrix);
        }

        public Bounds GetWorldBounds()
        {
            VoxelRenderInstance current = EnsureInstance();
            return CalculateWorldBounds(current);
        }

        public void ReleaseInstance()
        {
            if (instance == null)
                return;

            instance.Dispose();
            instance = null;
        }

        public static Bounds CalculateWorldBounds(
            VoxelRenderInstance renderInstance)
        {
            if (renderInstance == null || !renderInstance.IsValid)
                throw new ArgumentNullException(nameof(renderInstance));

            VoxelAssemblyAsset source = renderInstance.Assembly;
            if (source.PartCount == 0)
            {
                throw new InvalidOperationException(
                    "Voxel render presentation requires at least one part.");
            }

            bool initialized = false;
            Bounds result = default;
            for (int i = 0; i < source.PartCount; i++)
            {
                VoxelAssemblyPart part = source.GetPart(i);
                if (part.Mesh == null)
                {
                    throw new InvalidOperationException(
                        "Voxel render presentation part is missing its baked mesh: " +
                        source.AssemblyName + "/" + part.Name);
                }

                Bounds transformed = TransformBounds(
                    part.Mesh.bounds,
                    renderInstance.GetPartWorldMatrix(i));
                if (!initialized)
                {
                    result = transformed;
                    initialized = true;
                }
                else
                {
                    result.Encapsulate(transformed);
                }
            }

            return result;
        }

        private void OnEnable()
        {
            if (Application.isPlaying &&
                !externallyOwnedLifecycle &&
                world != null &&
                modelRoot != null &&
                assembly != null &&
                animationAsset != null)
            {
                EnsureInstance();
            }
        }

        private void LateUpdate()
        {
            if (!externallyOwnedLifecycle)
                SyncWorldMatrix();
        }

        private void OnDisable()
        {
            if (!externallyOwnedLifecycle)
                ReleaseInstance();
        }

        private void OnDestroy()
        {
            ReleaseInstance();
        }

        private void RequireConfigured()
        {
            if (world == null ||
                modelRoot == null ||
                assembly == null ||
                animationAsset == null)
            {
                throw new InvalidOperationException(
                    "VoxelRenderPresentation is not configured.");
            }

            if (!ReferenceEquals(animationAsset.Assembly, assembly))
            {
                throw new InvalidOperationException(
                    "Voxel render presentation animation targets a different assembly.");
            }
        }

        private static Bounds TransformBounds(
            Bounds local,
            Matrix4x4 matrix)
        {
            Vector3 center = matrix.MultiplyPoint3x4(local.center);
            Vector3 extents = local.extents;
            Vector3 worldExtents = new Vector3(
                Mathf.Abs(matrix.m00) * extents.x +
                Mathf.Abs(matrix.m01) * extents.y +
                Mathf.Abs(matrix.m02) * extents.z,
                Mathf.Abs(matrix.m10) * extents.x +
                Mathf.Abs(matrix.m11) * extents.y +
                Mathf.Abs(matrix.m12) * extents.z,
                Mathf.Abs(matrix.m20) * extents.x +
                Mathf.Abs(matrix.m21) * extents.y +
                Mathf.Abs(matrix.m22) * extents.z);
            return new Bounds(center, worldExtents * 2f);
        }
    }
}
