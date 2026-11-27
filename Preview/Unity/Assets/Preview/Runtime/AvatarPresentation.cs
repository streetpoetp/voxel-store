using System;
using System.Collections.Generic;
using System.Linq;
using Hero.Voxel.Presentation.Runtime.Unity;
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

        private VoxelPresentation presentation;

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
            presentation != null &&
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
            Transform presentationTransform,
            VoxelRenderPresentation bodyPresentation,
            IReadOnlyList<EquipmentPresentation> attachedEquipment)
        {
            ReleaseProvider();

            unitRoot = authoritativeUnitRoot != null
                ? authoritativeUnitRoot
                : throw new ArgumentNullException(nameof(authoritativeUnitRoot));
            presentationRoot = presentationTransform != null
                ? presentationTransform
                : throw new ArgumentNullException(nameof(presentationTransform));
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
                equipment.All(x =>
                    x.Model.IsInitialized &&
                    x.AttachmentHandle.IsInitialized &&
                    presentation.ContainsAttachment(x.AttachmentHandle)))
            {
                SyncWorldTransform();
                return;
            }

            ReleaseProvider();
            body.SetExternallyOwnedLifecycle(true);
            VoxelRenderInstance rootInstance = body.EnsureInstance();
            rootInstance.RequirePart("Body");
            presentation = new VoxelPresentation(rootInstance);
            body.AdoptAnimationTrack(presentation.RootTrack);

            foreach (EquipmentPresentation item in equipment)
            {
                item.RequireConfigured();
                item.Model.SetExternallyOwnedLifecycle(true);
                VoxelRenderInstance child = item.Model.EnsureInstance();
                VoxelPresentationAttachmentHandle handle =
                    presentation.Attach(
                        child,
                        item.SocketName,
                        "Mount");
                item.SetAttachmentHandle(handle);
                item.Model.AdoptAnimationTrack(
                    presentation.GetAttachmentTrack(handle));
            }

            SyncWorldTransform();
        }

        public void ResetPose()
        {
            if (!IsConfigured)
                return;

            InitializeProvider();
            presentation.Reset();
            SyncWorldTransform();
        }

        public void RefreshAttachmentReferences()
        {
            InitializeProvider();
            SyncWorldTransform();
        }

        public bool ContainsEquipment(EquipmentPresentation item)
        {
            if (item == null ||
                presentation == null ||
                equipment == null ||
                !equipment.Contains(item))
            {
                return false;
            }

            VoxelPresentationAttachmentHandle handle =
                item.AttachmentHandle;
            return handle.IsInitialized &&
                presentation.ContainsAttachment(handle);
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
            return presentation.GetWorldBounds();
        }

        public void SubmitNow()
        {
            InitializeProvider();
            SyncWorldTransform();
            body.World.SubmitNow();
        }

        public void ReleaseProvider()
        {
            if (equipment != null)
            {
                foreach (EquipmentPresentation item in equipment)
                {
                    if (item == null)
                        continue;
                    item.Model?.ClearAdoptedAnimationTrack();
                }
            }
            body?.ClearAdoptedAnimationTrack();

            if (presentation != null)
            {
                presentation.Dispose();
                presentation = null;
            }

            if (equipment != null)
            {
                foreach (EquipmentPresentation item in equipment)
                {
                    if (item == null)
                        continue;
                    item.ClearAttachmentHandle();
                    item.Model?.ReleaseInstance();
                }
            }

            body?.ReleaseInstance();
        }

        private void SyncWorldTransform()
        {
            if (presentation == null)
                return;

            presentation.RootTrack.SetWorldMatrix(
                body.ModelRoot.localToWorldMatrix);
            presentation.Refresh();
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
