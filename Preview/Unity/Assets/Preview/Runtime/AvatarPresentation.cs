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
        [SerializeField] private EquipmentPresentation[] equipment =
            Array.Empty<EquipmentPresentation>();

        public Transform UnitRoot => unitRoot;
        public Transform PresentationRoot => presentationRoot;
        public MeshFilter BodyMesh => bodyMesh;
        public Vector3 LocalRight => CanonicalLocalRight;
        public Vector3 LocalUp => CanonicalLocalUp;
        public Vector3 LocalForward => CanonicalLocalForward;
        public IReadOnlyList<EquipmentPresentation> Equipment => equipment;
        public Bounds BodyBounds =>
            ResolveBodyBounds(presentationRoot, bodyMesh);

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

            unitRoot = authoritativeUnitRoot;
            presentationRoot = presentation;
            bodyMesh = body;
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

            Bounds bounds = BodyBounds;
            foreach (EquipmentPresentation item in equipment)
            {
                if (item.PlacementRoot.parent != presentationRoot)
                {
                    item.PlacementRoot.SetParent(
                        presentationRoot,
                        false);
                }

                item.ApplyPlacement(bounds);
            }
        }

        public void ResetEquipmentMotion()
        {
            foreach (EquipmentPresentation item in equipment)
                item?.ResetMotion();
        }

        public static Vector3 ResolveBodyRatioPosition(
            Bounds bodyBounds,
            Vector3 sourceRatio)
        {
            ValidateRatio(sourceRatio);

            Vector3 unityRatio =
                VoxelStoreModelFrame.SourceToUnityVector(sourceRatio);
            return bodyBounds.center +
                   Vector3.Scale(bodyBounds.extents, unityRatio);
        }

        public static Bounds ResolveBodyBounds(
            Transform presentation,
            MeshFilter body)
        {
            if (presentation == null)
                throw new ArgumentNullException(nameof(presentation));
            if (body == null || body.sharedMesh == null)
            {
                throw new InvalidOperationException(
                    "AvatarPresentation requires a baked Body MeshFilter.");
            }

            Matrix4x4 bodyToPresentation =
                presentation.worldToLocalMatrix *
                body.transform.localToWorldMatrix;
            return TransformBounds(
                body.sharedMesh.bounds,
                bodyToPresentation);
        }

        public static void ValidateRatio(Vector3 value)
        {
            if (Invalid(value.x) ||
                Invalid(value.y) ||
                Invalid(value.z))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Body-ratio components must be finite values in [-1,+1].");
            }
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
        }

        private static Bounds TransformBounds(
            Bounds bounds,
            Matrix4x4 matrix)
        {
            Vector3 center = matrix.MultiplyPoint3x4(bounds.center);
            Vector3 extents = bounds.extents;

            Vector3 axisX =
                matrix.MultiplyVector(
                    new Vector3(extents.x, 0f, 0f));
            Vector3 axisY =
                matrix.MultiplyVector(
                    new Vector3(0f, extents.y, 0f));
            Vector3 axisZ =
                matrix.MultiplyVector(
                    new Vector3(0f, 0f, extents.z));
            Vector3 transformedExtents = new Vector3(
                Mathf.Abs(axisX.x) +
                Mathf.Abs(axisY.x) +
                Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) +
                Mathf.Abs(axisY.y) +
                Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) +
                Mathf.Abs(axisY.z) +
                Mathf.Abs(axisZ.z));

            return new Bounds(
                center,
                transformedExtents * 2f);
        }

        private static bool Invalid(float value) =>
            float.IsNaN(value) ||
            float.IsInfinity(value) ||
            Mathf.Abs(value) > 1f;
    }
}
