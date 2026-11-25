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
        private VoxelActorAnimationPlan currentPlan;

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
            currentPlan = null;
            ApplyTransformAndVisibility();
            SampleRootOnly(0.5f);
        }

        public async Task AttachInitialAsync(float normalizedPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(normalizedPhase);
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

            SampleCurrentTopology(normalizedPhase);
        }

        public void DetachSecondary(float normalizedPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(normalizedPhase);
            if (!hasSecondary)
                throw new InvalidOperationException("Secondary attachment is not active.");

            VoxelActorAttachmentHandle stale = secondaryHandle;
            actor.Detach(secondaryHandle);
            hasSecondary = false;
            secondaryHandle = default;
            RequireStaleHandle(stale, "detached secondary");
            SampleCurrentTopology(normalizedPhase);
        }

        public async Task ReplacePrimaryAsync(float normalizedPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(normalizedPhase);
            if (!hasPrimary)
                throw new InvalidOperationException("Primary attachment is not active.");

            VoxelActorAttachmentHandle preservedHandle = primaryHandle;
            VoxelActorAnimationPlan stalePlan = currentPlan;
            VoxelRenderInstance before =
                actor.GetAttachmentRenderInstance(primaryHandle);
            VoxelUnitId beforeUnit = before.Assembly.UnitId;

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

            VoxelRenderInstance after =
                actor.GetAttachmentRenderInstance(primaryHandle);
            if (after.Assembly.UnitId != replacementAssembly.UnitId ||
                after.Assembly.UnitId == beforeUnit)
            {
                throw new InvalidOperationException(
                    "Replacement handle did not resolve the replacement equipment unit.");
            }

            bool stalePlanRejected = false;
            try
            {
                actor.SampleNormalized(stalePlan, normalizedPhase);
            }
            catch (InvalidOperationException)
            {
                stalePlanRejected = true;
            }

            if (!stalePlanRejected)
            {
                throw new InvalidOperationException(
                    "Pre-replacement animation plan remained valid after topology mutation.");
            }

            SampleCurrentTopology(normalizedPhase);
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
            currentPlan = null;

            if (actor.AttachmentCount != 0)
            {
                throw new InvalidOperationException(
                    "Warm reacquisition inherited runtime-added attachments.");
            }

            RequireStaleHandle(stalePrimary, "pre-release primary");
            ApplyTransformAndVisibility();
            SampleRootOnly(normalizedPhase);
        }

        public async Task ReattachAfterReacquireAsync(float normalizedPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(normalizedPhase);
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
            SampleCurrentTopology(normalizedPhase);
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
            Bounds result =
                VoxelRenderPresentation.CalculateWorldBounds(
                    actor.RootRenderInstance);

            if (hasPrimary)
            {
                result.Encapsulate(
                    VoxelRenderPresentation.CalculateWorldBounds(
                        actor.GetAttachmentRenderInstance(primaryHandle)));
            }

            if (hasSecondary)
            {
                result.Encapsulate(
                    VoxelRenderPresentation.CalculateWorldBounds(
                        actor.GetAttachmentRenderInstance(secondaryHandle)));
            }

            return result;
        }

        public void SubmitNow()
        {
            RequireInitialized();
            world.SubmitNow();
        }

        public void DisposeProviderState()
        {
            currentPlan = null;
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

        private void SampleCurrentTopology(float normalizedPhase)
        {
            ApplyTransformAndVisibility();

            var requests =
                new List<VoxelActorPresenterAttachmentClipRequest>(
                    actor.AttachmentCount);

            // Deliberately submit mutable requests in non-binding order when both
            // handles are active. The provider maps by opaque handle identity.
            if (hasSecondary)
            {
                requests.Add(
                    new VoxelActorPresenterAttachmentClipRequest(
                        secondaryHandle,
                        AttackCandidates(EquipmentSlotSide.Left)));
            }

            if (hasPrimary)
            {
                requests.Add(
                    new VoxelActorPresenterAttachmentClipRequest(
                        primaryHandle,
                        AttackCandidates(EquipmentSlotSide.Right)));
            }

            currentPlan = actor.CreatePlan(
                AttackCandidates(EquipmentSlotSide.Right),
                requests);
            actor.SampleNormalized(currentPlan, normalizedPhase);
        }

        private void SampleRootOnly(float normalizedPhase)
        {
            ApplyTransformAndVisibility();
            currentPlan = actor.CreatePlan(
                AttackCandidates(EquipmentSlotSide.Right),
                Array.Empty<VoxelActorPresenterAttachmentClipRequest>());
            actor.SampleNormalized(currentPlan, normalizedPhase);
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
            VoxelRenderInstance root = actor.RootRenderInstance;
            VoxelRenderInstance child =
                actor.GetAttachmentRenderInstance(handle);

            int rootSocket = root.RequireSocket(socketName);
            int childMount = child.RequireSocket(MountAnchor);
            Vector3 expected =
                root.GetSocketWorldMatrix(rootSocket)
                    .MultiplyPoint3x4(Vector3.zero);
            Vector3 actual =
                child.GetSocketWorldMatrix(childMount)
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
            bool rejected = false;
            try
            {
                actor.GetAttachmentRenderInstance(handle);
            }
            catch (ArgumentException)
            {
                rejected = true;
            }

            if (!rejected)
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
