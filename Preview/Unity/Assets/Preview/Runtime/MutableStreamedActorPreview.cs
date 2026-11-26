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
        private readonly Dictionary<
            VoxelActorAttachmentHandle,
            AttachmentReferenceInfluenceResolver> attachmentResolvers =
                new Dictionary<
                    VoxelActorAttachmentHandle,
                    AttachmentReferenceInfluenceResolver>();

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
        public int MutableAttachmentCount =>
            IsInitialized ? actor.MutableAttachmentCount : 0;
        public int TopologyRevision =>
            IsInitialized ? actor.TopologyRevision : 0;
        public bool HasPrimaryAttachment =>
            IsCurrentAttachment(primaryHandle);
        public bool HasSecondaryAttachment =>
            IsCurrentAttachment(secondaryHandle);
        public VoxelUnitId RootUnitId => rootAssembly != null ? rootAssembly.UnitId : default;
        public VoxelUnitId PrimaryUnitId => primaryAssembly != null ? primaryAssembly.UnitId : default;
        public VoxelUnitId SecondaryUnitId => secondaryAssembly != null ? secondaryAssembly.UnitId : default;
        public VoxelUnitId ReplacementUnitId => replacementAssembly != null ? replacementAssembly.UnitId : default;

        public float PrimaryReferenceInfluence
        {
            get
            {
                RequireCurrentAttachment(
                    primaryHandle,
                    "primary influence");
                return actor.GetAttachmentTrack(primaryHandle)
                    .ReferenceInfluence;
            }
        }

        public float PrimaryReferencePositionDistance
        {
            get
            {
                RequireCurrentAttachment(
                    primaryHandle,
                    "primary reference distance");
                Matrix4x4 reference =
                    actor.GetAttachmentReferenceWorldMatrix(primaryHandle);
                Matrix4x4 actual =
                    actor.GetAttachmentTrack(primaryHandle).WorldMatrix;
                return Vector3.Distance(
                    reference.MultiplyPoint3x4(Vector3.zero),
                    actual.MultiplyPoint3x4(Vector3.zero));
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

            actor = await actors.AcquireAsync().AsTask();
            primaryHandle = default;
            secondaryHandle = default;
            if (actor.MutableAttachmentCount != 0)
            {
                throw new InvalidOperationException(
                    "Fresh streamed proof actor must not contain mutable attachments.");
            }
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

            int beforePrimaryAttach = actor.TopologyRevision;
            primaryHandle = await actor.AttachAsync(
                primaryAssembly.UnitId,
                RightSocket,
                MountAnchor).AsTask();
            RequireTopologyAdvanced(
                beforePrimaryAttach,
                "attach primary");
            RequireCurrentAttachment(
                primaryHandle,
                "primary after attach");
            if (actor.MutableAttachmentCount != 1)
            {
                throw new InvalidOperationException(
                    "Primary attach did not produce exactly one mutable attachment.");
            }

            int beforeSecondaryAttach = actor.TopologyRevision;
            secondaryHandle = await actor.AttachAsync(
                secondaryAssembly.UnitId,
                LeftSocket,
                MountAnchor).AsTask();
            RequireTopologyAdvanced(
                beforeSecondaryAttach,
                "attach secondary");
            RequireCurrentAttachment(
                secondaryHandle,
                "secondary after attach");
            if (actor.MutableAttachmentCount != 2)
            {
                throw new InvalidOperationException(
                    "Secondary attach did not produce exactly two mutable attachments.");
            }

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
            RequireCurrentAttachment(
                primaryHandle,
                "primary before secondary detach");
            RequireCurrentAttachment(
                secondaryHandle,
                "secondary before detach");

            VoxelModelAnimationTrack rootTrack = actor.RootTrack;
            VoxelModelAnimationTrack primaryTrack =
                actor.GetAttachmentTrack(primaryHandle);
            float rootPhase = rootTrack.NormalizedPhase;
            float primaryPhase = primaryTrack.NormalizedPhase;
            VoxelActorAttachmentHandle stale = secondaryHandle;
            int beforeDetach = actor.TopologyRevision;
            actor.Detach(secondaryHandle);
            RequireTopologyAdvanced(
                beforeDetach,
                "detach secondary");
            attachmentResolvers.Remove(stale);
            if (attachmentResolvers.ContainsKey(stale))
            {
                throw new InvalidOperationException(
                    "Detached streamed attachment retained resolver state.");
            }
            RequireStaleHandle(stale, "detached secondary");
            if (actor.MutableAttachmentCount != 1)
            {
                throw new InvalidOperationException(
                    "Secondary detach did not leave exactly one mutable attachment.");
            }
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
            ApplyCurrentAttachmentReferences();
        }

        public void RebindPrimaryPreservingPlayback()
        {
            RequireInitialized();
            RequireCurrentAttachment(
                primaryHandle,
                "primary before rebind");

            VoxelModelAnimationTrack rootTrack = actor.RootTrack;
            VoxelModelAnimationTrack primaryTrack =
                actor.GetAttachmentTrack(primaryHandle);
            VoxelModelAnimationTrack secondaryTrack =
                IsCurrentAttachment(secondaryHandle)
                    ? actor.GetAttachmentTrack(secondaryHandle)
                    : null;
            float rootPhase = rootTrack.NormalizedPhase;
            float primaryPhase = primaryTrack.NormalizedPhase;
            float secondaryPhase =
                secondaryTrack != null
                    ? secondaryTrack.NormalizedPhase
                    : 0f;

            int beforeLeftRebind = actor.TopologyRevision;
            actor.Rebind(
                primaryHandle,
                LeftSocket,
                MountAnchor);
            RequireTopologyAdvanced(
                beforeLeftRebind,
                "rebind primary left");
            RequireCurrentAttachment(
                primaryHandle,
                "primary after left rebind");
            ApplyAttachmentReference(primaryHandle);
            ValidateReference(primaryHandle, 0.001f);
            RequireUnchangedTrack(
                rootTrack,
                actor.RootTrack,
                rootPhase,
                "root during primary rebind");
            RequireUnchangedTrack(
                primaryTrack,
                actor.GetAttachmentTrack(primaryHandle),
                primaryPhase,
                "primary during rebind");
            if (secondaryTrack != null)
            {
                RequireUnchangedTrack(
                    secondaryTrack,
                    actor.GetAttachmentTrack(secondaryHandle),
                    secondaryPhase,
                    "secondary during primary rebind");
            }

            int beforeRightRebind = actor.TopologyRevision;
            actor.Rebind(
                primaryHandle,
                RightSocket,
                MountAnchor);
            RequireTopologyAdvanced(
                beforeRightRebind,
                "rebind primary right");
            ApplyAttachmentReference(primaryHandle);
            ValidateReference(primaryHandle, 0.001f);
            RequireUnchangedTrack(
                rootTrack,
                actor.RootTrack,
                rootPhase,
                "root after primary rebind");
            RequireUnchangedTrack(
                primaryTrack,
                actor.GetAttachmentTrack(primaryHandle),
                primaryPhase,
                "primary after rebind");
        }

        public async Task ReplacePrimaryAsync()
        {
            RequireInitialized();
            RequireCurrentAttachment(
                primaryHandle,
                "primary before replacement");

            VoxelActorAttachmentHandle preservedHandle = primaryHandle;
            VoxelModelAnimationTrack rootTrack = actor.RootTrack;
            VoxelModelAnimationTrack oldPrimaryTrack =
                actor.GetAttachmentTrack(primaryHandle);
            VoxelModelAnimationTrack secondaryTrack =
                IsCurrentAttachment(secondaryHandle)
                    ? actor.GetAttachmentTrack(secondaryHandle)
                    : null;
            float rootPhase = rootTrack.NormalizedPhase;
            float secondaryPhase =
                secondaryTrack != null
                    ? secondaryTrack.NormalizedPhase
                    : 0f;
            if (!actor.TryGetAttachmentInfo(
                    primaryHandle,
                    out VoxelActorAttachmentInfo before))
            {
                throw new InvalidOperationException(
                    "Primary attachment handle is not current before replacement.");
            }

            VoxelUnitId beforeUnit = before.UnitId;

            AttachmentReferenceInfluenceResolver previousResolver = null;
            attachmentResolvers.TryGetValue(
                primaryHandle,
                out previousResolver);

            int beforeReplace = actor.TopologyRevision;
            await actor.ReplaceAsync(
                primaryHandle,
                replacementAssembly.UnitId,
                RightSocket,
                MountAnchor).AsTask();
            RequireTopologyAdvanced(
                beforeReplace,
                "replace primary");

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
            attachmentResolvers.Remove(primaryHandle);
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
            if (secondaryTrack != null)
            {
                RequireUnchangedTrack(
                    secondaryTrack,
                    actor.GetAttachmentTrack(secondaryHandle),
                    secondaryPhase,
                    "secondary during primary replacement");
            }
            ApplyTransformAndVisibility();
            ApplyCurrentAttachmentReferences();
            if (previousResolver != null &&
                ReferenceEquals(
                    previousResolver,
                    RequireAttachmentResolver(primaryHandle)))
            {
                throw new InvalidOperationException(
                    "Replacement reused stale attachment resolver state.");
            }
        }

        public async Task ReleaseAndReacquireRootOnlyAsync(float normalizedPhase)
        {
            RequireInitialized();
            RequireNormalizedPhase(normalizedPhase);

            VoxelActorAttachmentHandle stalePrimary = primaryHandle;
            VoxelActorAttachmentHandle staleSecondary = secondaryHandle;
            actors.Release(actor);
            actor = await actors.AcquireAsync().AsTask();
            attachmentResolvers.Clear();
            if (attachmentResolvers.Count != 0)
            {
                throw new InvalidOperationException(
                    "Warm streamed actor release retained attachment resolver state.");
            }

            if (actor.MutableAttachmentCount != 0 ||
                actor.AttachmentCount != 0)
            {
                throw new InvalidOperationException(
                    "Warm reacquisition inherited runtime-added attachments.");
            }
            if (actor.RootTrack.HasSelection ||
                actor.RootTrack.IsPlaying)
            {
                throw new InvalidOperationException(
                    "Warm reacquisition inherited root playback state.");
            }

            RequireStaleHandle(stalePrimary, "pre-release primary");
            if (staleSecondary.IsInitialized)
                RequireStaleHandle(
                    staleSecondary,
                    "pre-release secondary");
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

            int beforeAttach = actor.TopologyRevision;
            primaryHandle = await actor.AttachAsync(
                replacementAssembly.UnitId,
                RightSocket,
                MountAnchor).AsTask();
            RequireTopologyAdvanced(
                beforeAttach,
                "reattach primary after reacquire");
            RequireCurrentAttachment(
                primaryHandle,
                "primary after reacquire attach");
            SampleCurrentTopology(
                rootPhase,
                primaryPhase,
                secondaryPhase: 0f);
        }

        public void ValidateAttachmentReferences(float tolerance = 0.001f)
        {
            RequireInitialized();
            if (tolerance < 0f || float.IsNaN(tolerance) || float.IsInfinity(tolerance))
                throw new ArgumentOutOfRangeException(nameof(tolerance));

            if (IsCurrentAttachment(primaryHandle))
                ValidateReference(primaryHandle, tolerance);
            if (IsCurrentAttachment(secondaryHandle))
                ValidateReference(secondaryHandle, tolerance);
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
            primaryHandle = default;
            secondaryHandle = default;
            attachmentResolvers.Clear();

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

            if (IsCurrentAttachment(primaryHandle))
            {
                VoxelModelAnimationTrack primaryTrack =
                    actor.GetAttachmentTrack(primaryHandle);
                primaryTrack.Select(
                    AttackCandidates(EquipmentSlotSide.Right));
                primaryTrack.SampleNormalized(primaryPhase);
            }

            if (IsCurrentAttachment(secondaryHandle))
            {
                VoxelModelAnimationTrack secondaryTrack =
                    actor.GetAttachmentTrack(secondaryHandle);
                secondaryTrack.Select(
                    AttackCandidates(EquipmentSlotSide.Left));
                secondaryTrack.SampleNormalized(secondaryPhase);
            }

            ApplyCurrentAttachmentReferences();
        }

        private void SampleRootOnly(float normalizedPhase)
        {
            ApplyTransformAndVisibility();
            VoxelModelAnimationTrack rootTrack = actor.RootTrack;
            rootTrack.Select(
                AttackCandidates(EquipmentSlotSide.Right));
            rootTrack.SampleNormalized(normalizedPhase);
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

        private void ApplyCurrentAttachmentReferences()
        {
            if (IsCurrentAttachment(primaryHandle))
                ApplyAttachmentReference(primaryHandle);
            if (IsCurrentAttachment(secondaryHandle))
                ApplyAttachmentReference(secondaryHandle);
        }

        private void ApplyAttachmentReference(
            VoxelActorAttachmentHandle handle)
        {
            RequireCurrentAttachment(
                handle,
                "apply attachment reference");
            VoxelModelAnimationTrack track =
                actor.GetAttachmentTrack(handle);
            Matrix4x4 reference =
                actor.GetAttachmentReferenceWorldMatrix(handle);
            AttachmentReferenceInfluenceResolver resolver =
                RequireAttachmentResolver(handle);
            track.SetWorldMatrix(
                resolver.Resolve(
                    reference,
                    track.ReferenceInfluence));
        }

        private AttachmentReferenceInfluenceResolver
            RequireAttachmentResolver(
                VoxelActorAttachmentHandle handle)
        {
            if (!attachmentResolvers.TryGetValue(
                    handle,
                    out AttachmentReferenceInfluenceResolver resolver))
            {
                resolver =
                    new AttachmentReferenceInfluenceResolver();
                attachmentResolvers.Add(handle, resolver);
            }

            return resolver;
        }

        private void ValidateReference(
            VoxelActorAttachmentHandle handle,
            float tolerance)
        {
            RequireCurrentAttachment(
                handle,
                "validate attachment reference");
            Matrix4x4 reference =
                actor.GetAttachmentReferenceWorldMatrix(handle);
            VoxelModelAnimationTrack track =
                actor.GetAttachmentTrack(handle);
            Matrix4x4 actual = track.WorldMatrix;
            float influence = track.ReferenceInfluence;
            if (!IsFinite(reference) ||
                !IsFinite(actual) ||
                !float.IsFinite(influence) ||
                influence < 0f ||
                influence > 1f)
            {
                throw new InvalidOperationException(
                    "Mutable streamed attachment reference state is not finite or valid.");
            }

            Matrix4x4 expected =
                RequireAttachmentResolver(handle)
                    .Resolve(reference, influence);
            float error = MaxMatrixDelta(expected, actual);
            if (error > tolerance)
            {
                throw new InvalidOperationException(
                    "Mutable streamed attachment track does not match resolved presentation: " +
                    "influence=" + influence.ToString("R") +
                    ", error=" + error.ToString("R") +
                    ", tolerance=" + tolerance.ToString("R") + ".");
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

        private static float MaxMatrixDelta(
            Matrix4x4 left,
            Matrix4x4 right)
        {
            float max = 0f;
            for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
            {
                max = Mathf.Max(
                    max,
                    Mathf.Abs(left[row, column] - right[row, column]));
            }

            return max;
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

        private bool IsCurrentAttachment(
            VoxelActorAttachmentHandle handle)
        {
            return IsInitialized &&
                handle.IsInitialized &&
                actor.ContainsAttachment(handle);
        }

        private void RequireCurrentAttachment(
            VoxelActorAttachmentHandle handle,
            string label)
        {
            if (!handle.IsInitialized ||
                !actor.ContainsAttachment(handle) ||
                !actor.TryGetAttachmentInfo(handle, out _))
            {
                throw new InvalidOperationException(
                    "Expected current mutable attachment handle: " +
                    label + ".");
            }
        }

        private void RequireTopologyAdvanced(
            int beforeRevision,
            string label)
        {
            if (actor.TopologyRevision <= beforeRevision)
            {
                throw new InvalidOperationException(
                    "Mutable streamed actor topology revision did not advance for " +
                    label +
                    ". before=" + beforeRevision +
                    ", after=" + actor.TopologyRevision + ".");
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
                    "Mutable streamed actor changed unaffected playback: " +
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
