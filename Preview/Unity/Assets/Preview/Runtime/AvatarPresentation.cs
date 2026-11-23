using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private MeshFilter bodyMesh;
        [SerializeField] private VoxelAnimatedAssembly bodyAnimation;
        [SerializeField] private EquipmentPresentation[] equipment =
            Array.Empty<EquipmentPresentation>();

        public Transform UnitRoot => unitRoot;
        public Transform PresentationRoot => presentationRoot;
        public MeshFilter BodyMesh => bodyMesh;
        public VoxelAnimatedAssembly BodyAnimation => bodyAnimation;
        public Vector3 LocalRight => CanonicalLocalRight;
        public Vector3 LocalUp => CanonicalLocalUp;
        public Vector3 LocalForward => CanonicalLocalForward;
        public IReadOnlyList<EquipmentPresentation> Equipment => equipment;

        public Vector3 WorldForward =>
            unitRoot != null
                ? unitRoot.TransformDirection(LocalForward).normalized
                : LocalForward;

        private void Awake()
        {
            if (unitRoot == null ||
                presentationRoot == null ||
                bodyMesh == null ||
                bodyMesh.sharedMesh == null)
            {
                return;
            }

            ApplyEquipmentPlacement();
        }

        public void Configure(
            Transform authoritativeUnitRoot,
            Transform presentation,
            MeshFilter body,
            VoxelAnimatedAssembly animatedBody,
            IReadOnlyList<EquipmentPresentation> attachedEquipment)
        {
            if (authoritativeUnitRoot == null)
                throw new ArgumentNullException(nameof(authoritativeUnitRoot));
            if (presentation == null)
                throw new ArgumentNullException(nameof(presentation));
            if (body == null || body.sharedMesh == null)
            {
                throw new ArgumentException(
                    "AvatarPresentation requires a baked Body MeshFilter.",
                    nameof(body));
            }
            if (animatedBody == null)
                throw new ArgumentNullException(nameof(animatedBody));

            unitRoot = authoritativeUnitRoot;
            presentationRoot = presentation;
            bodyMesh = body;
            bodyAnimation = animatedBody;
            equipment = attachedEquipment == null
                ? Array.Empty<EquipmentPresentation>()
                : attachedEquipment
                    .Where(x => x != null)
                    .ToArray();

            ApplyEquipmentPlacement();
        }

        public void ApplyEquipmentPlacement()
        {
            RequireConfigured();

            foreach (EquipmentPresentation item in equipment)
            {
                if (item.PlacementRoot.parent != presentationRoot)
                {
                    item.PlacementRoot.SetParent(
                        presentationRoot,
                        false);
                }

                item.ResetMotion();
                item.ApplyAttachment(
                    ResolveBodySocketInPresentation(
                        item.SocketName));
            }
        }

        public void SampleMotion(
            AvatarMotion motion,
            EquipmentSlotSide driverSide,
            float normalizedTime)
        {
            RequireConfigured();
            bodyAnimation.Sample(
                motion,
                driverSide,
                normalizedTime);

            foreach (EquipmentPresentation item in equipment)
            {
                item.ApplyAttachment(
                    ResolveBodySocketInPresentation(
                        item.SocketName));
                item.Sample(
                    motion,
                    normalizedTime);
            }
        }

        public void ResetPose()
        {
            RequireConfigured();
            bodyAnimation.ResetPose();
            ApplyEquipmentPlacement();
        }

        public void ResetEquipmentMotion()
        {
            foreach (EquipmentPresentation item in equipment)
                item?.ResetMotion();
        }

        public Matrix4x4 ResolveBodySocketInPresentation(
            string socketName)
        {
            RequireConfigured();
            Matrix4x4 socketInModel =
                bodyAnimation.GetSocketLocalMatrix(socketName);
            Matrix4x4 socketWorld =
                bodyAnimation.ModelRoot.localToWorldMatrix *
                socketInModel;
            return presentationRoot.worldToLocalMatrix *
                   socketWorld;
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

            if (bodyMesh == null || bodyMesh.sharedMesh == null)
            {
                throw new InvalidOperationException(
                    "AvatarPresentation is missing its Body mesh.");
            }
            if (bodyAnimation == null)
            {
                throw new InvalidOperationException(
                    "AvatarPresentation is missing authored Body animation.");
            }
        }

    }
}
