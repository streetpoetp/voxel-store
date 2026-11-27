using System;
using Hero.Voxel.Presentation.Runtime.Unity;
using UnityEngine;

namespace VoxelStore.Preview.Runtime
{
    [DisallowMultipleComponent]
    public sealed class EquipmentPresentation : MonoBehaviour
    {
        public static readonly Vector3 CanonicalLocalRight =
            VoxelStoreModelFrame.Right;
        public static readonly Vector3 CanonicalLocalUp =
            VoxelStoreModelFrame.Up;
        public static readonly Vector3 CanonicalLocalForward =
            VoxelStoreModelFrame.Forward;

        [SerializeField] private VoxelRenderPresentation model;
        [SerializeField] private string socketName;
        [SerializeField] private EquipmentSlotSide side;

        private VoxelPresentationAttachmentHandle attachmentHandle;

        public Transform PlacementRoot => transform;
        public VoxelRenderPresentation Model => model;
        public string SocketName => socketName;
        public EquipmentSlotSide Side => side;
        public VoxelPresentationAttachmentHandle AttachmentHandle =>
            attachmentHandle;
        public Vector3 LocalRight => CanonicalLocalRight;
        public Vector3 LocalUp => CanonicalLocalUp;
        public Vector3 LocalForward => CanonicalLocalForward;

        public void Configure(
            VoxelRenderPresentation renderModel,
            string avatarSocketName,
            EquipmentSlotSide slotSide)
        {
            if (renderModel == null)
                throw new ArgumentNullException(nameof(renderModel));
            if (string.IsNullOrWhiteSpace(avatarSocketName))
            {
                throw new ArgumentException(
                    "Avatar socket name is required.",
                    nameof(avatarSocketName));
            }

            model = renderModel;
            model.SetExternallyOwnedLifecycle(true);
            socketName = avatarSocketName;
            side = slotSide;
            attachmentHandle = default;
        }

        internal void SetAttachmentHandle(
            VoxelPresentationAttachmentHandle handle)
        {
            if (!handle.IsInitialized)
            {
                throw new ArgumentException(
                    "An initialized presentation attachment handle is required.",
                    nameof(handle));
            }

            attachmentHandle = handle;
        }

        internal void ClearAttachmentHandle()
        {
            attachmentHandle = default;
        }

        internal void RequireConfigured()
        {
            if (model == null)
            {
                throw new InvalidOperationException(
                    "EquipmentPresentation is missing its provider render presentation.");
            }

            if (string.IsNullOrWhiteSpace(socketName))
            {
                throw new InvalidOperationException(
                    "EquipmentPresentation is missing its avatar socket.");
            }
        }
    }
}
