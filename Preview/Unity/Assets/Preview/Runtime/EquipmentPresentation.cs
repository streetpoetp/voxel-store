using System;
using Hero.Voxel.Actor.Runtime.Unity;
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

        private VoxelActorBindingId bindingId;

        public Transform PlacementRoot => transform;
        public VoxelRenderPresentation Model => model;
        public string SocketName => socketName;
        public EquipmentSlotSide Side => side;
        public VoxelActorBindingId BindingId =>
            bindingId;
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
            bindingId = default;
        }

        internal void SetBindingId(
            VoxelActorBindingId id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentException(
                    "A valid voxel actor binding id is required.",
                    nameof(id));
            }

            bindingId = id;
        }

        internal void ClearBindingId()
        {
            bindingId = default;
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
