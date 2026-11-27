using System;
using Hero.Voxel.Actor.Runtime.Unity;
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
        private VoxelModelAnimationTrack animationTrack;
        private bool animationTrackExternallyOwned;

        public VoxelRenderWorld World => world;
        public Transform ModelRoot => modelRoot;
        public VoxelAssemblyAsset Assembly => assembly;
        public VoxelAnimationAsset Animation => animationAsset;
        public VoxelRenderInstance Instance =>
            instance != null && instance.IsValid ? instance : null;
        public VoxelModelAnimationTrack AnimationTrack =>
            Instance != null ? animationTrack : null;
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
            bool created = false;
            if (instance == null || !instance.IsValid)
            {
                instance = world.Spawn(assembly, animationAsset);
                animationTrack = new VoxelModelAnimationTrack(instance);
                animationTrackExternallyOwned = false;
                created = true;
            }

            if (created || !externallyOwnedLifecycle)
                SyncWorldMatrix();
            return instance;
        }

        public VoxelModelAnimationTrack EnsureAnimationTrack()
        {
            EnsureInstance();
            if (animationTrack == null)
            {
                animationTrack = new VoxelModelAnimationTrack(instance);
                animationTrackExternallyOwned = false;
            }
            return animationTrack;
        }

        internal void AdoptAnimationTrack(VoxelModelAnimationTrack track)
        {
            EnsureInstance();
            animationTrack = track ??
                throw new ArgumentNullException(nameof(track));
            animationTrackExternallyOwned = true;
        }

        internal void ClearAdoptedAnimationTrack()
        {
            if (!animationTrackExternallyOwned)
                return;

            animationTrack = null;
            animationTrackExternallyOwned = false;
        }

        public void SyncWorldMatrix()
        {
            if (instance == null || !instance.IsValid)
                return;

            instance.SetWorldMatrix(modelRoot.localToWorldMatrix);
        }

        public Bounds GetWorldBounds()
        {
            return EnsureInstance().GetWorldBounds();
        }

        public void ReleaseInstance()
        {
            if (instance == null)
                return;

            animationTrack = null;
            animationTrackExternallyOwned = false;
            instance.Dispose();
            instance = null;
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

    }
}
