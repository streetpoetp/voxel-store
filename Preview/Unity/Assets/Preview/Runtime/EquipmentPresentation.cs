using System;
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

        [SerializeField] private Transform placementRoot;
        [SerializeField] private VoxelAnimatedAssembly animatedModel;
        [SerializeField] private string socketName;
        [SerializeField] private EquipmentSlotSide side;

        private Matrix4x4 inverseBindMount;
        private bool bindMountInitialized;

        public Transform PlacementRoot =>
            placementRoot != null
                ? placementRoot
                : transform;
        public VoxelAnimatedAssembly AnimatedModel =>
            animatedModel;
        public string SocketName => socketName;
        public EquipmentSlotSide Side => side;
        public Vector3 LocalRight => CanonicalLocalRight;
        public Vector3 LocalUp => CanonicalLocalUp;
        public Vector3 LocalForward => CanonicalLocalForward;

        public void Configure(
            Transform placement,
            VoxelAnimatedAssembly model,
            string avatarSocketName,
            EquipmentSlotSide slotSide)
        {
            if (placement == null)
                throw new ArgumentNullException(nameof(placement));
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(
                    avatarSocketName))
            {
                throw new ArgumentException(
                    "Avatar socket name is required.",
                    nameof(avatarSocketName));
            }

            placementRoot = placement;
            animatedModel = model;
            socketName = avatarSocketName;
            side = slotSide;
            RebuildBindMount();
            ResetMotion();
        }

        private void Awake()
        {
            RebuildBindMountIfSerialized();
        }

        private void OnEnable()
        {
            RebuildBindMountIfSerialized();
        }

        public void ApplyAttachment(
            Matrix4x4 bodySocketInPresentation)
        {
            RequireConfigured();
            ApplyLocalMatrix(
                placementRoot,
                bodySocketInPresentation *
                inverseBindMount);
        }

        public void Sample(
            AvatarMotion motion,
            float normalizedTime)
        {
            RequireConfigured();
            animatedModel.Sample(
                motion,
                side,
                normalizedTime);
        }

        public void ResetMotion()
        {
            if (animatedModel == null)
                return;

            animatedModel.ResetPose();
        }

        private void RequireConfigured()
        {
            if (placementRoot == null)
            {
                throw new InvalidOperationException(
                    "EquipmentPresentation is missing PlacementRoot.");
            }

            if (animatedModel == null)
            {
                throw new InvalidOperationException(
                    "EquipmentPresentation is missing its animated model.");
            }

            if (string.IsNullOrWhiteSpace(socketName))
            {
                throw new InvalidOperationException(
                    "EquipmentPresentation is missing its avatar socket.");
            }

            if (!bindMountInitialized)
                RebuildBindMount();
        }

        private void RebuildBindMountIfSerialized()
        {
            if (placementRoot == null ||
                animatedModel == null ||
                string.IsNullOrWhiteSpace(socketName))
            {
                return;
            }

            RebuildBindMount();
        }

        private void RebuildBindMount()
        {
            if (animatedModel == null)
            {
                throw new InvalidOperationException(
                    "EquipmentPresentation is missing its animated model.");
            }

            inverseBindMount =
                animatedModel
                    .GetBindSocketLocalMatrix("Mount")
                    .inverse;
            bindMountInitialized = true;
        }

        private static void ApplyLocalMatrix(
            Transform target,
            Matrix4x4 matrix)
        {
            target.localPosition =
                matrix.GetColumn(3);
            target.localRotation =
                matrix.rotation;
            target.localScale =
                matrix.lossyScale;
        }
    }
}
