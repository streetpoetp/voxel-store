using System;
using System.Collections.Generic;
using System.Linq;
using Hero.Voxel.Actor.Runtime.Unity;
using Hero.Voxel.Render.Runtime.Unity;
using Hero.Voxel.Runtime.Assets;
using UnityEngine;

namespace VoxelStore.Preview.Runtime
{
    [DisallowMultipleComponent]
    public sealed class AvatarPresentation : MonoBehaviour
    {
        public static readonly Vector3 CanonicalLocalRight =
            VoxelStoreModelFrame.Right;
        public static readonly Vector3 CanonicalLocalUp =
            VoxelStoreModelFrame.Up;
        public static readonly Vector3 CanonicalLocalForward =
            VoxelStoreModelFrame.Forward;

        [SerializeField] private Transform unitRoot;
        [SerializeField] private Transform presentationRoot;
        [SerializeField] private VoxelRenderPresentation body;
        [SerializeField] private EquipmentPresentation[] equipment =
            Array.Empty<EquipmentPresentation>();

        private VoxelActorInstance actor;
        private VoxelActorAnimationPlan currentPlan;

        public Transform UnitRoot => unitRoot;
        public Transform PresentationRoot => presentationRoot;
        public VoxelRenderPresentation Body => body;
        public VoxelAssemblyAsset BodyAssembly => body != null ? body.Assembly : null;
        public VoxelAnimationAsset BodyAnimation => body != null ? body.Animation : null;
        public Vector3 LocalRight => CanonicalLocalRight;
        public Vector3 LocalUp => CanonicalLocalUp;
        public Vector3 LocalForward => CanonicalLocalForward;
        public IReadOnlyList<EquipmentPresentation> Equipment => equipment;
        public bool IsConfigured =>
            unitRoot != null &&
            presentationRoot != null &&
            body != null;
        public bool IsProviderInitialized =>
            actor != null &&
            body != null &&
            body.IsInitialized;
        public bool IsPlaying =>
            actor != null && actor.IsPlaying;
        public float CurrentNormalizedPhase =>
            actor != null ? actor.CurrentNormalizedPhase : 0f;
        public float CurrentDuration =>
            currentPlan != null ? currentPlan.Duration : 0f;
        public VoxelRenderWorld World =>
            body != null ? body.World : null;

        public Vector3 WorldForward =>
            unitRoot != null
                ? unitRoot.TransformDirection(LocalForward).normalized
                : LocalForward;

        public void Configure(
            Transform authoritativeUnitRoot,
            Transform presentation,
            VoxelRenderPresentation bodyPresentation,
            IReadOnlyList<EquipmentPresentation> attachedEquipment)
        {
            ReleaseProvider();

            unitRoot = authoritativeUnitRoot != null
                ? authoritativeUnitRoot
                : throw new ArgumentNullException(nameof(authoritativeUnitRoot));
            presentationRoot = presentation != null
                ? presentation
                : throw new ArgumentNullException(nameof(presentation));
            body = bodyPresentation != null
                ? bodyPresentation
                : throw new ArgumentNullException(nameof(bodyPresentation));
            equipment = attachedEquipment == null
                ? Array.Empty<EquipmentPresentation>()
                : attachedEquipment.Where(x => x != null).ToArray();

            body.SetExternallyOwnedLifecycle(true);
            foreach (EquipmentPresentation item in equipment)
            {
                item.RequireConfigured();
                item.Model.SetExternallyOwnedLifecycle(true);
                if (!ReferenceEquals(item.Model.World, body.World))
                {
                    throw new InvalidOperationException(
                        "Avatar body and equipment must use the same VoxelRenderWorld.");
                }
            }

            InitializeProvider();
            ResetPose();
        }

        public void InitializeProvider()
        {
            RequireConfigured();

            if (IsProviderInitialized &&
                equipment.All(x => x.Model.IsInitialized && x.BindingId.IsValid))
            {
                SyncWorldTransform();
                return;
            }

            ReleaseProvider();
            body.SetExternallyOwnedLifecycle(true);
            VoxelRenderInstance rootInstance = body.EnsureInstance();
            rootInstance.RequirePart("Body");
            actor = new VoxelActorInstance(rootInstance);

            foreach (EquipmentPresentation item in equipment)
            {
                item.RequireConfigured();
                item.Model.SetExternallyOwnedLifecycle(true);
                VoxelRenderInstance child = item.Model.EnsureInstance();
                VoxelActorBindingId binding = actor.Attach(
                    child,
                    item.SocketName,
                    "Mount");
                item.SetBinding(binding);
            }

            currentPlan = null;
            body.SyncWorldMatrix();
            actor.Reset();
        }

        public void StartMotion(
            AvatarMotion motion,
            EquipmentSlotSide driverSide,
            bool shouldLoop)
        {
            StartMotion(
                motion,
                driverSide,
                shouldLoop,
                string.Empty);
        }

        public void StartMotion(
            AvatarMotion motion,
            EquipmentSlotSide driverSide,
            bool shouldLoop,
            string rootVariant)
        {
            InitializeProvider();
            body.SyncWorldMatrix();
            currentPlan = BuildPlan(
                motion,
                driverSide,
                rootVariant);
            actor.Start(currentPlan, shouldLoop);
        }

        public void StartMotionAtPhase(
            AvatarMotion motion,
            EquipmentSlotSide driverSide,
            bool shouldLoop,
            float normalizedPhase)
        {
            StartMotionAtPhase(
                motion,
                driverSide,
                shouldLoop,
                normalizedPhase,
                string.Empty);
        }

        public void StartMotionAtPhase(
            AvatarMotion motion,
            EquipmentSlotSide driverSide,
            bool shouldLoop,
            float normalizedPhase,
            string rootVariant)
        {
            ValidateNormalizedTime(normalizedPhase);
            StartMotion(
                motion,
                driverSide,
                shouldLoop,
                rootVariant);
            if (normalizedPhase > 0f)
                actor.Advance(currentPlan.Duration * normalizedPhase);
        }

        public float Advance(float deltaSeconds)
        {
            InitializeProvider();
            if (currentPlan == null)
            {
                throw new InvalidOperationException(
                    "Avatar presentation has no started animation plan.");
            }

            body.SyncWorldMatrix();
            return actor.Advance(deltaSeconds);
        }

        public void SampleMotion(
            AvatarMotion motion,
            EquipmentSlotSide driverSide,
            float normalizedTime)
        {
            SampleMotion(
                motion,
                driverSide,
                normalizedTime,
                string.Empty);
        }

        public void SampleMotion(
            AvatarMotion motion,
            EquipmentSlotSide driverSide,
            float normalizedTime,
            string rootVariant)
        {
            ValidateNormalizedTime(normalizedTime);
            InitializeProvider();
            actor.Reset();
            body.SyncWorldMatrix();
            currentPlan = BuildPlan(
                motion,
                driverSide,
                rootVariant);
            actor.SampleNormalized(currentPlan, normalizedTime);
        }

        public void ResetPose()
        {
            if (!IsConfigured)
                return;

            InitializeProvider();
            body.SyncWorldMatrix();
            actor.Reset();
            currentPlan = null;
        }

        public Matrix4x4 ResolveBodySocketWorld(string socketName)
        {
            InitializeProvider();
            SyncWorldTransform();
            int index = body.Instance.RequireSocket(socketName);
            return body.Instance.GetSocketWorldMatrix(index);
        }

        public Matrix4x4 ResolveBodyPartWorld(string partName)
        {
            InitializeProvider();
            SyncWorldTransform();
            int index = body.Instance.RequirePart(partName);
            return body.Instance.GetPartWorldMatrix(index);
        }

        public Bounds GetWorldBounds()
        {
            InitializeProvider();
            SyncWorldTransform();

            Bounds bounds = body.GetWorldBounds();
            foreach (EquipmentPresentation item in equipment)
                bounds.Encapsulate(item.Model.GetWorldBounds());
            return bounds;
        }

        public void SubmitNow()
        {
            InitializeProvider();
            SyncWorldTransform();
            body.World.SubmitNow();
        }

        public void ReleaseProvider()
        {
            currentPlan = null;

            if (actor != null)
            {
                actor.Dispose();
                actor = null;
            }

            if (equipment != null)
            {
                foreach (EquipmentPresentation item in equipment)
                {
                    if (item == null)
                        continue;
                    item.ClearBinding();
                    item.Model?.ReleaseInstance();
                }
            }

            body?.ReleaseInstance();
        }

        private VoxelActorAnimationPlan BuildPlan(
            AvatarMotion motion,
            EquipmentSlotSide driverSide,
            string rootVariant)
        {
            var requests =
                new VoxelActorAttachmentClipRequest[equipment.Length];
            for (int i = 0; i < equipment.Length; i++)
            {
                EquipmentPresentation item = equipment[i];
                requests[i] = new VoxelActorAttachmentClipRequest(
                    item.BindingId,
                    AvatarMotionClipCandidates.Build(
                        motion,
                        item.Side,
                        item.Variant));
            }

            return actor.CreatePlan(
                AvatarMotionClipCandidates.Build(
                    motion,
                    driverSide,
                    rootVariant),
                requests);
        }

        private void SyncWorldTransform()
        {
            if (actor == null)
                return;

            body.SyncWorldMatrix();
            if (currentPlan != null)
            {
                actor.SampleNormalized(
                    currentPlan,
                    actor.CurrentNormalizedPhase);
            }
            else
            {
                actor.Reset();
            }
        }

        private void Awake()
        {
            if (Application.isPlaying && IsConfigured)
                InitializeProvider();
        }

        private void OnEnable()
        {
            if (Application.isPlaying && IsConfigured)
                InitializeProvider();
        }

        private void OnDisable()
        {
            ReleaseProvider();
        }

        private void OnDestroy()
        {
            ReleaseProvider();
        }

        private void RequireConfigured()
        {
            if (unitRoot == null)
            {
                throw new InvalidOperationException(
                    "AvatarPresentation is missing UnitRoot.");
            }

            if (presentationRoot == null)
            {
                throw new InvalidOperationException(
                    "AvatarPresentation is missing PresentationRoot.");
            }

            if (body == null)
            {
                throw new InvalidOperationException(
                    "AvatarPresentation is missing its provider-backed Body.");
            }

            if (body.World == null ||
                body.Assembly == null ||
                body.Animation == null)
            {
                throw new InvalidOperationException(
                    "AvatarPresentation Body is not configured.");
            }
        }

        private static void ValidateNormalizedTime(float normalizedTime)
        {
            if (float.IsNaN(normalizedTime) ||
                float.IsInfinity(normalizedTime) ||
                normalizedTime < 0f ||
                normalizedTime > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(normalizedTime),
                    "Normalized sample time must be in [0,1].");
            }
        }
    }
}
