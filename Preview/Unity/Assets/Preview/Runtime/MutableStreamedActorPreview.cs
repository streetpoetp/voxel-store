using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Hero.Voxel.Actor.Runtime.Unity;
using Hero.Voxel.Actor.Streaming.Runtime.Unity;
using Hero.Voxel.Presentation.Streaming.Runtime.Unity;
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
        private VoxelStreamedPresentation presentation;
        private VoxelStreamedPresentationAttachmentHandle primaryHandle;
        private VoxelStreamedPresentationAttachmentHandle secondaryHandle;

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

        public bool IsInitialized => presentation != null;
        public int AttachmentCount =>
            IsInitialized ? presentation.AttachmentCount : 0;
        public bool HasPrimaryAttachment =>
            IsCurrentAttachment(primaryHandle);
        public bool HasSecondaryAttachment =>
            IsCurrentAttachment(secondaryHandle);
        public VoxelUnitId RootUnitId =>
            rootAssembly != null ? rootAssembly.UnitId : default;
        public VoxelUnitId PrimaryUnitId =>
            primaryAssembly != null ? primaryAssembly.UnitId : default;
        public VoxelUnitId SecondaryUnitId =>
            secondaryAssembly != null ? secondaryAssembly.UnitId : default;
        public VoxelUnitId ReplacementUnitId =>
            replacementAssembly != null ? replacementAssembly.UnitId : default;

        public float PrimaryReferenceInfluence
        {
            get
            {
                RequireCurrentAttachment(
                    primaryHandle,
                    "primary influence");
                return presentation.GetAttachmentTrack(primaryHandle)
                    .ReferenceInfluence;
            }
        }

        public Vector3 PrimaryWorldPosition
        {
            get
            {
                RequireCurrentAttachment(
                    primaryHandle,
                    "primary world position");
                return presentation.GetAttachmentTrack(primaryHandle)
                    .WorldMatrix.MultiplyPoint3x4(Vector3.zero);
            }
        }

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

            presentation =
                await VoxelStreamedPresentation
                    .AcquireCompositeAsync(actors)
                    .AsTask();
            primaryHandle = default;
            secondaryHandle = default;
            if (!presentation.IsComposite ||
                presentation.AttachmentCount != 0)
            {
                throw new InvalidOperationException(
                    "Fresh streamed proof must be composite-capable and root-only.");
            }

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
            if (presentation.AttachmentCount != 0)
            {
                throw new InvalidOperationException(
                    "Initial streamed proof requires a root-only presentation.");
            }

            primaryHandle = await presentation.AttachAsync(
                AttachmentDescriptor(
                    primaryAssembly.UnitId,
                    RightSocket)).AsTask();
            RequireCurrentAttachment(
                primaryHandle,
                "primary after attach");
            if (presentation.AttachmentCount != 1)
            {
                throw new InvalidOperationException(
                    "Primary attach did not produce exactly one attachment.");
            }

            secondaryHandle = await presentation.AttachAsync(
                AttachmentDescriptor(
                    secondaryAssembly.UnitId,
                    LeftSocket)).AsTask();
            RequireCurrentAttachment(
                secondaryHandle,
                "secondary after attach");
            if (presentation.AttachmentCount != 2)
            {
                throw new InvalidOperationException(
                    "Secondary attach did not produce exactly two attachments.");
            }
            if (primaryHandle == secondaryHandle)
            {
                throw new InvalidOperationException(
                    "Distinct mutable attachments must receive distinct handles.");
            }

            SampleCurrentPresentation(
                rootPhase,
                primaryPhase,
                secondaryPhase);
        }

        public void DetachSecondaryPreservingPlayback()
        {
            RequireInitialized();
            RequireCurrentAttachment(
                primaryHandle,
                "primary before secondary detach");
            RequireCurrentAttachment(
                secondaryHandle,
                "secondary before detach");

            VoxelModelAnimationTrack rootTrack = presentation.RootTrack;
            VoxelModelAnimationTrack primaryTrack =
                presentation.GetAttachmentTrack(primaryHandle);
            float rootPhase = rootTrack.NormalizedPhase;
            float primaryPhase = primaryTrack.NormalizedPhase;
            VoxelStreamedPresentationAttachmentHandle stale =
                secondaryHandle;

            presentation.Detach(secondaryHandle);
            secondaryHandle = default;
            RequireStaleHandle(stale, "detached secondary");
            if (presentation.AttachmentCount != 1)
            {
                throw new InvalidOperationException(
                    "Secondary detach did not leave exactly one attachment.");
            }
            RequireUnchangedTrack(
                rootTrack,
                presentation.RootTrack,
                rootPhase,
                "root during secondary detach");
            RequireUnchangedTrack(
                primaryTrack,
                presentation.GetAttachmentTrack(primaryHandle),
                primaryPhase,
                "primary during secondary detach");

            ApplyTransformAndVisibility();
        }

        public void RebindPrimaryPreservingPlayback()
        {
            RequireInitialized();
            RequireCurrentAttachment(
                primaryHandle,
                "primary before rebind");

            VoxelStreamedPresentationAttachmentHandle preservedHandle =
                primaryHandle;
            VoxelModelAnimationTrack rootTrack = presentation.RootTrack;
            VoxelModelAnimationTrack primaryTrack =
                presentation.GetAttachmentTrack(primaryHandle);
            VoxelModelAnimationTrack secondaryTrack =
                IsCurrentAttachment(secondaryHandle)
                    ? presentation.GetAttachmentTrack(secondaryHandle)
                    : null;
            float rootPhase = rootTrack.NormalizedPhase;
            float primaryPhase = primaryTrack.NormalizedPhase;
            float secondaryPhase =
                secondaryTrack != null
                    ? secondaryTrack.NormalizedPhase
                    : 0f;

            presentation.Rebind(
                primaryHandle,
                LeftSocket,
                MountAnchor);
            RequireCurrentAttachment(
                primaryHandle,
                "primary after left rebind");
            if (primaryHandle != preservedHandle)
                throw new InvalidOperationException(
                    "Rebind changed the high-level attachment handle.");
            RequireUnchangedTrack(
                rootTrack,
                presentation.RootTrack,
                rootPhase,
                "root during primary rebind");
            RequireUnchangedTrack(
                primaryTrack,
                presentation.GetAttachmentTrack(primaryHandle),
                primaryPhase,
                "primary during left rebind");
            if (secondaryTrack != null)
            {
                RequireUnchangedTrack(
                    secondaryTrack,
                    presentation.GetAttachmentTrack(secondaryHandle),
                    secondaryPhase,
                    "secondary during primary rebind");
            }

            presentation.Rebind(
                primaryHandle,
                RightSocket,
                MountAnchor);
            RequireCurrentAttachment(
                primaryHandle,
                "primary after right rebind");
            if (primaryHandle != preservedHandle)
                throw new InvalidOperationException(
                    "Second rebind changed the high-level attachment handle.");
            RequireUnchangedTrack(
                rootTrack,
                presentation.RootTrack,
                rootPhase,
                "root after primary rebind");
            RequireUnchangedTrack(
                primaryTrack,
                presentation.GetAttachmentTrack(primaryHandle),
                primaryPhase,
                "primary after rebind");
        }

        public async Task ReplacePrimaryAsync()
        {
            RequireInitialized();
            RequireCurrentAttachment(
                primaryHandle,
                "primary before replacement");

            VoxelStreamedPresentationAttachmentHandle preservedHandle =
                primaryHandle;
            VoxelModelAnimationTrack rootTrack = presentation.RootTrack;
            VoxelModelAnimationTrack oldPrimaryTrack =
                presentation.GetAttachmentTrack(primaryHandle);
            VoxelModelAnimationTrack secondaryTrack =
                IsCurrentAttachment(secondaryHandle)
                    ? presentation.GetAttachmentTrack(secondaryHandle)
                    : null;
            float rootPhase = rootTrack.NormalizedPhase;
            float secondaryPhase =
                secondaryTrack != null
                    ? secondaryTrack.NormalizedPhase
                    : 0f;

            await presentation.ReplaceAsync(
                primaryHandle,
                AttachmentDescriptor(
                    replacementAssembly.UnitId,
                    RightSocket)).AsTask();

            if (primaryHandle != preservedHandle ||
                !presentation.ContainsAttachment(primaryHandle))
            {
                throw new InvalidOperationException(
                    "Successful replacement changed or invalidated the public attachment handle.");
            }

            VoxelModelAnimationTrack replacementTrack =
                presentation.GetAttachmentTrack(primaryHandle);
            if (ReferenceEquals(oldPrimaryTrack, replacementTrack) ||
                replacementTrack.HasSelection)
            {
                throw new InvalidOperationException(
                    "Replacement did not reset only the replacement model track.");
            }

            RequireUnchangedTrack(
                rootTrack,
                presentation.RootTrack,
                rootPhase,
                "root during primary replacement");
            if (secondaryTrack != null)
            {
                RequireUnchangedTrack(
                    secondaryTrack,
                    presentation.GetAttachmentTrack(secondaryHandle),
                    secondaryPhase,
                    "secondary during primary replacement");
            }

            ApplyTransformAndVisibility();
        }

        public async Task ReleaseAndReacquireRootOnlyAsync(
            float normalizedPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(normalizedPhase);

            VoxelStreamedPresentationAttachmentHandle stalePrimary =
                primaryHandle;
            VoxelStreamedPresentationAttachmentHandle staleSecondary =
                secondaryHandle;

            presentation.Dispose();
            presentation =
                await VoxelStreamedPresentation
                    .AcquireCompositeAsync(actors)
                    .AsTask();

            if (!presentation.IsComposite ||
                presentation.AttachmentCount != 0)
            {
                throw new InvalidOperationException(
                    "Warm reacquisition inherited runtime-added attachments.");
            }
            if (presentation.RootTrack.HasSelection ||
                presentation.RootTrack.IsPlaying)
            {
                throw new InvalidOperationException(
                    "Warm reacquisition inherited root playback state.");
            }

            RequireStaleHandle(
                stalePrimary,
                "pre-release primary");
            if (staleSecondary.IsInitialized)
            {
                RequireStaleHandle(
                    staleSecondary,
                    "pre-release secondary");
            }

            primaryHandle = default;
            secondaryHandle = default;
            SampleRootOnly(normalizedPhase);
        }

        public async Task ReattachAfterReacquireAsync(
            float rootPhase,
            float primaryPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(rootPhase);
            RequireNormalizedPhase(primaryPhase);
            if (presentation.AttachmentCount != 0)
            {
                throw new InvalidOperationException(
                    "Post-reacquire proof expected a root-only presentation.");
            }

            primaryHandle = await presentation.AttachAsync(
                AttachmentDescriptor(
                    replacementAssembly.UnitId,
                    RightSocket)).AsTask();
            RequireCurrentAttachment(
                primaryHandle,
                "primary after reacquire attach");
            SampleCurrentPresentation(
                rootPhase,
                primaryPhase,
                secondaryPhase: 0f);
        }

        public void ValidatePresentationState()
        {
            RequireInitialized();

            int expectedCount = 0;
            if (IsCurrentAttachment(primaryHandle))
            {
                ValidateAttachmentTrack(
                    primaryHandle,
                    "primary");
                expectedCount++;
            }
            if (IsCurrentAttachment(secondaryHandle))
            {
                ValidateAttachmentTrack(
                    secondaryHandle,
                    "secondary");
                expectedCount++;
            }

            if (presentation.AttachmentCount != expectedCount)
            {
                throw new InvalidOperationException(
                    "Mutable streamed presentation membership is inconsistent with current high-level handles.");
            }
        }

        public Bounds GetWorldBounds()
        {
            RequireInitialized();
            return presentation.GetWorldBounds();
        }

        public void SubmitNow()
        {
            RequireInitialized();
            world.SubmitNow();
        }

        public void DisposeProviderState()
        {
            presentation?.Dispose();
            presentation = null;
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

        public void SampleCurrentPresentation(
            float rootPhase,
            float primaryPhase,
            float secondaryPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(rootPhase);
            RequireNormalizedPhase(primaryPhase);
            RequireNormalizedPhase(secondaryPhase);

            VoxelModelAnimationTrack rootTrack = presentation.RootTrack;
            rootTrack.Select(
                AttackCandidates(EquipmentSlotSide.Right));
            rootTrack.SampleNormalized(rootPhase);

            if (IsCurrentAttachment(primaryHandle))
            {
                VoxelModelAnimationTrack primaryTrack =
                    presentation.GetAttachmentTrack(primaryHandle);
                primaryTrack.Select(
                    AttackCandidates(EquipmentSlotSide.Right));
                primaryTrack.SampleNormalized(primaryPhase);
            }

            if (IsCurrentAttachment(secondaryHandle))
            {
                VoxelModelAnimationTrack secondaryTrack =
                    presentation.GetAttachmentTrack(secondaryHandle);
                secondaryTrack.Select(
                    AttackCandidates(EquipmentSlotSide.Left));
                secondaryTrack.SampleNormalized(secondaryPhase);
            }

            ApplyTransformAndVisibility();
            presentation.Refresh();
        }

        private void SampleRootOnly(float normalizedPhase)
        {
            VoxelModelAnimationTrack rootTrack = presentation.RootTrack;
            rootTrack.Select(
                AttackCandidates(EquipmentSlotSide.Right));
            rootTrack.SampleNormalized(normalizedPhase);
            ApplyTransformAndVisibility();
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
            presentation.SetWorldTransform(
                transform.position,
                transform.rotation,
                transform.lossyScale);
            presentation.SetVisible(true);
        }

        private static VoxelActorAttachmentDescriptor
            AttachmentDescriptor(
                VoxelUnitId unitId,
                string parentSocketName)
        {
            return new VoxelActorAttachmentDescriptor(
                unitId,
                parentSocketName,
                MountAnchor);
        }

        private void ValidateAttachmentTrack(
            VoxelStreamedPresentationAttachmentHandle handle,
            string label)
        {
            RequireCurrentAttachment(handle, label);
            VoxelModelAnimationTrack track =
                presentation.GetAttachmentTrack(handle);
            Matrix4x4 worldMatrix = track.WorldMatrix;
            float influence = track.ReferenceInfluence;
            if (!IsFinite(worldMatrix) ||
                !float.IsFinite(influence) ||
                influence < 0f ||
                influence > 1f)
            {
                throw new InvalidOperationException(
                    "Mutable streamed presentation state is not finite or valid: " +
                    label + ".");
            }
        }

        private static bool IsFinite(Matrix4x4 value)
        {
            for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
            {
                if (!float.IsFinite(value[row, column]))
                    return false;
            }

            return true;
        }

        private void RequireStaleHandle(
            VoxelStreamedPresentationAttachmentHandle handle,
            string label)
        {
            if (presentation.ContainsAttachment(handle))
            {
                throw new InvalidOperationException(
                    "Expected stale streamed presentation handle to fail closed: " +
                    label + ".");
            }
        }

        private bool IsCurrentAttachment(
            VoxelStreamedPresentationAttachmentHandle handle)
        {
            return IsInitialized &&
                handle.IsInitialized &&
                presentation.ContainsAttachment(handle);
        }

        private void RequireCurrentAttachment(
            VoxelStreamedPresentationAttachmentHandle handle,
            string label)
        {
            if (!IsCurrentAttachment(handle))
            {
                throw new InvalidOperationException(
                    "Expected current streamed presentation handle: " +
                    label + ".");
            }
        }

        private static void RequireUnchangedTrack(
            VoxelModelAnimationTrack expectedTrack,
            VoxelModelAnimationTrack actualTrack,
            float expectedPhase,
            string label)
        {
            if (!ReferenceEquals(expectedTrack, actualTrack) ||
                !Mathf.Approximately(
                    expectedPhase,
                    actualTrack.NormalizedPhase))
            {
                throw new InvalidOperationException(
                    "Mutable streamed presentation changed unaffected playback: " +
                    label + ".");
            }
        }

        private void RequireConfigured()
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException(
                    "Mutable streamed actor preview is not configured.");
            }
            RequireDistinctUnitIds();
        }

        private void RequireInitialized()
        {
            RequireConfigured();
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    "Mutable streamed actor preview is not initialized.");
            }
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
                    if (unit.Assembly == null ||
                        !unit.Assembly.UnitId.IsValid)
                    {
                        throw new InvalidOperationException(
                            "Preview streamed provider requires stable assembly identity.");
                    }

                    if (unit.Animation == null ||
                        !ReferenceEquals(
                            unit.Animation.Assembly,
                            unit.Assembly))
                    {
                        throw new InvalidOperationException(
                            "Preview streamed provider requires matching animation assets.");
                    }

                    units.Add(unit.Assembly.UnitId, unit);
                }
            }

            public UniTask<IVoxelResourceHandle<VoxelUnitAssets>>
                LoadUnitAsync(
                    VoxelUnitId id,
                    CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!units.TryGetValue(
                        id,
                        out VoxelUnitAssets assets))
                {
                    throw new InvalidOperationException(
                        "Preview streamed provider does not contain unit id " +
                        id.Value + ".");
                }

                return UniTask.FromResult<
                    IVoxelResourceHandle<VoxelUnitAssets>>(
                        new StaticHandle<VoxelUnitAssets>(assets));
            }

            public UniTask<IVoxelResourceHandle<VoxelThemeAsset>>
                LoadThemeAsync(
                    VoxelThemeId id,
                    CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new NotSupportedException(
                    "Mutable streamed actor preview does not load voxel themes.");
            }
        }

        private sealed class StaticHandle<T> :
            IVoxelResourceHandle<T>
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
