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
                body.SyncWorldMatrix();
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

            SyncWorldTransform();
        }

        public void ResetPose()
        {
            if (!IsConfigured)
                return;

            InitializeProvider();
            body.EnsureAnimationTrack().Reset();
            foreach (EquipmentPresentation item in equipment)
                item.Model.EnsureAnimationTrack().Reset();
            SyncWorldTransform();
        }

        public void RefreshAttachmentReferences()
        {
            InitializeProvider();
            SyncWorldTransform();
        }

        public Matrix4x4 ResolveAttachmentReferenceWorld(
            EquipmentPresentation item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            InitializeProvider();
            body.SyncWorldMatrix();
            RequireCurrentEquipment(item);
            return actor.GetAttachmentReferenceWorldMatrix(item.BindingId);
        }

        public void ApplyAttachmentReference(
            EquipmentPresentation item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            InitializeProvider();
            body.SyncWorldMatrix();
            RequireCurrentEquipment(item);
            ApplyAttachmentReferenceCurrent(item);
        }

        public bool IsBodyModel(VoxelRenderPresentation model)
        {
            return model != null && ReferenceEquals(body, model);
        }

        public EquipmentPresentation FindEquipment(
            VoxelRenderPresentation model)
        {
            if (model == null)
                return null;
            return equipment.FirstOrDefault(
                item => item != null && ReferenceEquals(item.Model, model));
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

        private void SyncWorldTransform()
        {
            if (actor == null)
                return;

            body.SyncWorldMatrix();
            foreach (EquipmentPresentation item in equipment)
                ApplyAttachmentReferenceCurrent(item);
        }

        private void ApplyAttachmentReferenceCurrent(
            EquipmentPresentation item)
        {
            Matrix4x4 reference =
                actor.GetAttachmentReferenceWorldMatrix(item.BindingId);
            VoxelModelAnimationTrack track =
                item.Model.EnsureAnimationTrack();
            Matrix4x4 resolved =
                item.ResolveReferenceWorld(
                    reference,
                    track.ReferenceInfluence);
            track.SetWorldMatrix(resolved);
        }

        private void RequireCurrentEquipment(
            EquipmentPresentation item)
        {
            if (!item.BindingId.IsValid ||
                equipment == null ||
                !equipment.Contains(item))
            {
                throw new InvalidOperationException(
                    "Equipment is not a current attachment of this avatar presentation.");
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

    }
}
