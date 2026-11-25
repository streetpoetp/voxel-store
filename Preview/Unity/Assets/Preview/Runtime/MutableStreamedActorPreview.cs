using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Hero.Voxel.Actor.Runtime.Unity;
using Hero.Voxel.Actor.Streaming.Runtime.Unity;
using Hero.Voxel.Render.Runtime.Unity;
using Hero.Voxel.Render.Streaming.Runtime.Unity;
using Hero.Voxel.Runtime.Assets;
using Hero.Voxel.Runtime.Data.Models;
using Hero.Voxel.Streaming.Runtime.Unity;
using UnityEngine;

namespace VoxelStore.Preview.Runtime
{
    [DisallowMultipleComponent]
    public sealed class MutableStreamedActorPreview : MonoBehaviour
    {
        public const string ObjectName = "Mutable Streamed Actor Proof";

        private const string RightSocket = "Equip.Hand.Right.0";
        private const string LeftSocket = "Equip.Hand.Left.0";
        private const string MountAnchor = "Mount";

        [SerializeField] private VoxelRenderWorld world;
        [SerializeField] private VoxelAssemblyAsset rootAssembly;
        [SerializeField] private VoxelAnimationAsset rootAnimation;
        [SerializeField] private VoxelAssemblyAsset primaryAssembly;
        [SerializeField] private VoxelAnimationAsset primaryAnimation;
        [SerializeField] private VoxelAssemblyAsset secondaryAssembly;
        [SerializeField] private VoxelAnimationAsset secondaryAnimation;
        [SerializeField] private VoxelAssemblyAsset replacementAssembly;
        [SerializeField] private VoxelAnimationAsset replacementAnimation;

        private PreviewAssetProvider provider;
        private VoxelStreamingService streaming;
        private VoxelPresenterPool presenters;
        private VoxelActorPresenterPool actors;
        private VoxelActorPresenter actor;
        private VoxelActorAttachmentHandle primaryHandle;
        private VoxelActorAttachmentHandle secondaryHandle;
        private bool hasPrimary;
        private bool hasSecondary;

        public bool IsConfigured =>
            world != null &&
            rootAssembly != null &&
            rootAnimation != null &&
            primaryAssembly != null &&
            primaryAnimation != null &&
            secondaryAssembly != null &&
            secondaryAnimation != null &&
            replacementAssembly != null &&
            replacementAnimation != null;

        public bool IsInitialized => actor != null && actor.IsActive;
        public int AttachmentCount => IsInitialized ? actor.AttachmentCount : 0;
        public VoxelUnitId RootUnitId => rootAssembly != null ? rootAssembly.UnitId : default;
        public VoxelUnitId PrimaryUnitId => primaryAssembly != null ? primaryAssembly.UnitId : default;
        public VoxelUnitId SecondaryUnitId => secondaryAssembly != null ? secondaryAssembly.UnitId : default;
        public VoxelUnitId ReplacementUnitId => replacementAssembly != null ? replacementAssembly.UnitId : default;

        public void Configure(
            VoxelRenderWorld configuredWorld,
            VoxelAssemblyAsset configuredRootAssembly,
            VoxelAnimationAsset configuredRootAnimation,
            VoxelAssemblyAsset configuredPrimaryAssembly,
            VoxelAnimationAsset configuredPrimaryAnimation,
            VoxelAssemblyAsset configuredSecondaryAssembly,
            VoxelAnimationAsset configuredSecondaryAnimation,
            VoxelAssemblyAsset configuredReplacementAssembly,
            VoxelAnimationAsset configuredReplacementAnimation)
        {
            DisposeProviderState();

            world = configuredWorld != null
                ? configuredWorld
                : throw new ArgumentNullException(nameof(configuredWorld));
            rootAssembly = RequirePair(
                configuredRootAssembly,
                configuredRootAnimation,
                nameof(configuredRootAssembly));
            rootAnimation = configuredRootAnimation;
            primaryAssembly = RequirePair(
                configuredPrimaryAssembly,
                configuredPrimaryAnimation,
                nameof(configuredPrimaryAssembly));
            primaryAnimation = configuredPrimaryAnimation;
            secondaryAssembly = RequirePair(
                configuredSecondaryAssembly,
                configuredSecondaryAnimation,
                nameof(configuredSecondaryAssembly));
            secondaryAnimation = configuredSecondaryAnimation;
            replacementAssembly = RequirePair(
                configuredReplacementAssembly,
                configuredReplacementAnimation,
                nameof(configuredReplacementAssembly));
            replacementAnimation = configuredReplacementAnimation;

            RequireDistinctUnitIds();
        }

        public async Task InitializeRootAsync()
        {
            RequireConfigured();
            DisposeProviderState();

            world.Initialize();
            provider = new PreviewAssetProvider(
                new VoxelUnitAssets(rootAssembly, rootAnimation),
                new VoxelUnitAssets(primaryAssembly, primaryAnimation),
                new VoxelUnitAssets(secondaryAssembly, secondaryAnimation),
                new VoxelUnitAssets(replacementAssembly, replacementAnimation));
            streaming = new VoxelStreamingService(
                provider,
                new VoxelStreamingOptions
                {
                    IdleTtl = TimeSpan.FromMinutes(5),
                    CleanupInterval = TimeSpan.FromMinutes(1),
                    LoadTimeout = TimeSpan.FromSeconds(15),
                    MaxConcurrentLoadsPerKind = 4
                });
            presenters = new VoxelPresenterPool(
                world,
                streaming,
                maxPresenters: 8,
                maxIdlePresenters: 8);
            actors = new VoxelActorPresenterPool(
                presenters,
                new VoxelActorPresentationDescriptor(rootAssembly.UnitId),
                maxPresenters: 1,
                maxIdlePresenters: 1);

            actor = await actors.AcquireAsync().AsTask();
            hasPrimary = false;
            hasSecondary = false;
            ApplyTransformAndVisibility();
            SampleRootOnly(0.5f);
        }

        public async Task AttachInitialAsync(
            float rootPhase,
            float primaryPhase,
            float secondaryPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(rootPhase);
            RequireNormalizedPhase(primaryPhase);
            RequireNormalizedPhase(secondaryPhase);
            if (actor.AttachmentCount != 0)
            {
                throw new InvalidOperationException(
                    "Initial streamed proof requires a root-only actor.");
            }

            primaryHandle = await actor.AttachAsync(
                primaryAssembly.UnitId,
                RightSocket,
                MountAnchor).AsTask();
            hasPrimary = true;

            secondaryHandle = await actor.AttachAsync(
                secondaryAssembly.UnitId,
                LeftSocket,
                MountAnchor).AsTask();
            hasSecondary = true;

            if (primaryHandle == secondaryHandle)
            {
                throw new InvalidOperationException(
                    "Distinct mutable attachments must receive distinct handles.");
            }

            SampleCurrentTopology(
                rootPhase,
                primaryPhase,
                secondaryPhase);
        }

        public void DetachSecondaryPreservingPlayback()
        {
            RequireInitialized();
            if (!hasSecondary)
                throw new InvalidOperationException("Secondary attachment is not active.");

            VoxelModelAnimationTrack rootTrack = actor.RootTrack;
            VoxelModelAnimationTrack primaryTrack =
                actor.GetAttachmentTrack(primaryHandle);
            float rootPhase = rootTrack.NormalizedPhase;
            float primaryPhase = primaryTrack.NormalizedPhase;
            VoxelActorAttachmentHandle stale = secondaryHandle;
            actor.Detach(secondaryHandle);
            hasSecondary = false;
            secondaryHandle = default;
            RequireStaleHandle(stale, "detached secondary");
            if (!ReferenceEquals(rootTrack, actor.RootTrack) ||
                !ReferenceEquals(
                    primaryTrack,
                    actor.GetAttachmentTrack(primaryHandle)) ||
                !Mathf.Approximately(
                    rootPhase,
                    actor.RootTrack.NormalizedPhase) ||
                !Mathf.Approximately(
                    primaryPhase,
                    actor.GetAttachmentTrack(primaryHandle)
                        .NormalizedPhase))
            {
                throw new InvalidOperationException(
                    "Detaching one streamed attachment changed unaffected model playback.");
            }
            ApplyTransformAndVisibility();
            actor.RefreshBindings();
        }

        public async Task ReplacePrimaryAsync()
        {
            RequireInitialized();
            if (!hasPrimary)
                throw new InvalidOperationException("Primary attachment is not active.");

            VoxelActorAttachmentHandle preservedHandle = primaryHandle;
            VoxelModelAnimationTrack rootTrack = actor.RootTrack;
            VoxelModelAnimationTrack oldPrimaryTrack =
                actor.GetAttachmentTrack(primaryHandle);
            float rootPhase = rootTrack.NormalizedPhase;
            if (!actor.TryGetAttachmentInfo(
                    primaryHandle,
                    out VoxelActorAttachmentInfo before))
            {
                throw new InvalidOperationException(
                    "Primary attachment handle is not current before replacement.");
            }

            VoxelUnitId beforeUnit = before.UnitId;

            await actor.ReplaceAsync(
                primaryHandle,
                replacementAssembly.UnitId,
                RightSocket,
                MountAnchor).AsTask();

            if (primaryHandle != preservedHandle)
            {
                throw new InvalidOperationException(
                    "Successful replacement changed the public attachment handle.");
            }

            if (!actor.TryGetAttachmentInfo(
                    primaryHandle,
                    out VoxelActorAttachmentInfo after) ||
                after.UnitId != replacementAssembly.UnitId ||
                after.UnitId == beforeUnit)
            {
                throw new InvalidOperationException(
                    "Replacement handle did not resolve the replacement equipment unit.");
            }

            VoxelModelAnimationTrack replacementTrack =
                actor.GetAttachmentTrack(primaryHandle);
            if (ReferenceEquals(oldPrimaryTrack, replacementTrack) ||
                replacementTrack.HasSelection)
            {
                throw new InvalidOperationException(
                    "Replacement did not reset only the replacement model track.");
            }

            if (!ReferenceEquals(rootTrack, actor.RootTrack) ||
                !Mathf.Approximately(
                    rootPhase,
                    actor.RootTrack.NormalizedPhase))
            {
                throw new InvalidOperationException(
                    "Replacing one attachment changed root playback.");
            }
            ApplyTransformAndVisibility();
            actor.RefreshBindings();
        }

        public async Task ReleaseAndReacquireRootOnlyAsync(float normalizedPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(normalizedPhase);

            VoxelActorAttachmentHandle stalePrimary = primaryHandle;
            actors.Release(actor);
            actor = await actors.AcquireAsync().AsTask();

            hasPrimary = false;
            hasSecondary = false;
            primaryHandle = default;
            secondaryHandle = default;
            if (actor.AttachmentCount != 0)
            {
                throw new InvalidOperationException(
                    "Warm reacquisition inherited runtime-added attachments.");
            }

            RequireStaleHandle(stalePrimary, "pre-release primary");
            ApplyTransformAndVisibility();
            SampleRootOnly(normalizedPhase);
        }

        public async Task ReattachAfterReacquireAsync(
            float rootPhase,
            float primaryPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(rootPhase);
            RequireNormalizedPhase(primaryPhase);
            if (actor.AttachmentCount != 0)
            {
                throw new InvalidOperationException(
                    "Post-reacquire proof expected a root-only actor.");
            }

            primaryHandle = await actor.AttachAsync(
                replacementAssembly.UnitId,
                RightSocket,
                MountAnchor).AsTask();
            hasPrimary = true;
            SampleCurrentTopology(
                rootPhase,
                primaryPhase,
                secondaryPhase: 0f);
        }

        public void ValidateAttachmentGrip(float tolerance = 0.001f)
        {
            RequireInitialized();
            if (tolerance < 0f || float.IsNaN(tolerance) || float.IsInfinity(tolerance))
                throw new ArgumentOutOfRangeException(nameof(tolerance));

            if (hasPrimary)
                ValidateGrip(primaryHandle, RightSocket, tolerance);
            if (hasSecondary)
                ValidateGrip(secondaryHandle, LeftSocket, tolerance);
        }

        public Bounds GetWorldBounds()
        {
            RequireInitialized();
            return actor.GetWorldBounds();
        }

        public void SubmitNow()
        {
            RequireInitialized();
            world.SubmitNow();
        }

        public void DisposeProviderState()
        {
            actor = null;
            hasPrimary = false;
            hasSecondary = false;
            primaryHandle = default;
            secondaryHandle = default;

            actors?.Dispose();
            actors = null;
            presenters?.Dispose();
            presenters = null;
            streaming?.Dispose();
            streaming = null;
            provider = null;
        }

        public void SampleCurrentTopology(
            float rootPhase,
            float primaryPhase,
            float secondaryPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(rootPhase);
            RequireNormalizedPhase(primaryPhase);
            RequireNormalizedPhase(secondaryPhase);
            ApplyTransformAndVisibility();
            VoxelModelAnimationTrack rootTrack = actor.RootTrack;
            rootTrack.Select(
                AttackCandidates(EquipmentSlotSide.Right));
            rootTrack.SampleNormalized(rootPhase);
            actor.RefreshBindings();

            if (hasPrimary)
            {
                VoxelModelAnimationTrack primaryTrack =
                    actor.GetAttachmentTrack(primaryHandle);
                primaryTrack.Select(
                    AttackCandidates(EquipmentSlotSide.Right));
                primaryTrack.SampleNormalized(primaryPhase);
            }

            if (hasSecondary)
            {
                VoxelModelAnimationTrack secondaryTrack =
                    actor.GetAttachmentTrack(secondaryHandle);
                secondaryTrack.Select(
                    AttackCandidates(EquipmentSlotSide.Left));
                secondaryTrack.SampleNormalized(secondaryPhase);
            }
        }

        private void SampleRootOnly(float normalizedPhase)
        {
            ApplyTransformAndVisibility();
            VoxelModelAnimationTrack rootTrack = actor.RootTrack;
            rootTrack.Select(
                AttackCandidates(EquipmentSlotSide.Right));
            rootTrack.SampleNormalized(normalizedPhase);
            actor.RefreshBindings();
        }

        private static IReadOnlyList<string> AttackCandidates(
            EquipmentSlotSide side)
        {
            return AvatarMotionClipCandidates.Build(
                AvatarMotion.Attack,
                side,
                "V1");
        }

        private void ApplyTransformAndVisibility()
        {
            actor.SetWorldTransform(
                transform.position,
                transform.rotation,
                transform.lossyScale);
            actor.SetVisible(true);
        }

        private void ValidateGrip(
            VoxelActorAttachmentHandle handle,
            string socketName,
            float tolerance)
        {
            Vector3 expected =
                actor.GetRootSocketWorldMatrix(socketName)
                    .MultiplyPoint3x4(Vector3.zero);
            Vector3 actual =
                actor.GetAttachmentSocketWorldMatrix(
                        handle,
                        MountAnchor)
                    .MultiplyPoint3x4(Vector3.zero);
            float drift = Vector3.Distance(expected, actual);
            if (drift > tolerance)
            {
                throw new InvalidOperationException(
                    "Mutable streamed attachment grip drift exceeds tolerance: " +
                    "socket=" + socketName +
                    ", drift=" + drift.ToString("R") +
                    ", tolerance=" + tolerance.ToString("R") + ".");
            }
        }

        private void RequireStaleHandle(
            VoxelActorAttachmentHandle handle,
            string label)
        {
            if (actor.ContainsAttachment(handle) ||
                actor.TryGetAttachmentInfo(handle, out _))
            {
                throw new InvalidOperationException(
                    "Expected stale mutable attachment handle to fail closed: " +
                    label + ".");
            }
        }

        private void RequireConfigured()
        {
            if (!IsConfigured)
                throw new InvalidOperationException(
                    "Mutable streamed actor preview is not configured.");
            RequireDistinctUnitIds();
        }

        private void RequireInitialized()
        {
            RequireConfigured();
            if (!IsInitialized)
                throw new InvalidOperationException(
                    "Mutable streamed actor preview is not initialized.");
        }

        private void RequireDistinctUnitIds()
        {
            var ids = new HashSet<VoxelUnitId>
            {
                rootAssembly.UnitId,
                primaryAssembly.UnitId,
                secondaryAssembly.UnitId,
                replacementAssembly.UnitId
            };
            if (ids.Count != 4)
            {
                throw new InvalidOperationException(
                    "Mutable streamed actor proof requires four distinct stable unit ids.");
            }
        }

        private static VoxelAssemblyAsset RequirePair(
            VoxelAssemblyAsset assembly,
            VoxelAnimationAsset animation,
            string parameterName)
        {
            if (assembly == null)
                throw new ArgumentNullException(parameterName);
            if (animation == null)
                throw new ArgumentNullException(nameof(animation));
            if (!assembly.HasStableIdentity)
            {
                throw new InvalidOperationException(
                    "Mutable streamed actor proof requires stable baked unit identity: " +
                    assembly.AssemblyName);
            }
            if (!ReferenceEquals(animation.Assembly, assembly))
            {
                throw new InvalidOperationException(
                    "Mutable streamed actor proof animation targets a different assembly: " +
                    assembly.AssemblyName);
            }
            return assembly;
        }

        private static void RequireNormalizedPhase(float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value < 0f ||
                value > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Normalized phase must be in [0,1].");
            }
        }

        private void OnDestroy()
        {
            DisposeProviderState();
        }

        private sealed class PreviewAssetProvider : IVoxelAssetProvider
        {
            private readonly Dictionary<VoxelUnitId, VoxelUnitAssets> units =
                new Dictionary<VoxelUnitId, VoxelUnitAssets>();

            public PreviewAssetProvider(params VoxelUnitAssets[] assets)
            {
                if (assets == null)
                    throw new ArgumentNullException(nameof(assets));

                foreach (VoxelUnitAssets unit in assets)
                {
                    if (unit.Assembly == null || !unit.Assembly.UnitId.IsValid)
                    {
                        throw new InvalidOperationException(
                            "Preview streamed provider requires stable assembly identity.");
                    }

                    if (unit.Animation == null ||
                        !ReferenceEquals(unit.Animation.Assembly, unit.Assembly))
                    {
                        throw new InvalidOperationException(
                            "Preview streamed provider requires matching animation assets.");
                    }

                    units.Add(unit.Assembly.UnitId, unit);
                }
            }

            public UniTask<IVoxelResourceHandle<VoxelUnitAssets>> LoadUnitAsync(
                VoxelUnitId id,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!units.TryGetValue(id, out VoxelUnitAssets assets))
                {
                    throw new InvalidOperationException(
                        "Preview streamed provider does not contain unit id " +
                        id.Value + ".");
                }

                return UniTask.FromResult<IVoxelResourceHandle<VoxelUnitAssets>>(
                    new StaticHandle<VoxelUnitAssets>(assets));
            }

            public UniTask<IVoxelResourceHandle<VoxelThemeAsset>> LoadThemeAsync(
                VoxelThemeId id,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new NotSupportedException(
                    "Mutable streamed actor preview does not load voxel themes.");
            }
        }

        private sealed class StaticHandle<T> : IVoxelResourceHandle<T>
        {
            public StaticHandle(T value)
            {
                Value = value;
            }

            public T Value { get; }

            public void Dispose()
            {
                // Baked preview ScriptableObjects remain owned by Unity/AssetDatabase.
            }
        }
    }
}
