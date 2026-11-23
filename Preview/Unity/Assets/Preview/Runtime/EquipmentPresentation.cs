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
        [SerializeField] private Transform motionRoot;
        [SerializeField] private Vector3 sourceRatio;

        public Transform PlacementRoot =>
            placementRoot != null ? placementRoot : transform;
        public Transform MotionRoot => motionRoot;
        public Vector3 SourceRatio => sourceRatio;
        public Vector3 LocalRight => CanonicalLocalRight;
        public Vector3 LocalUp => CanonicalLocalUp;
        public Vector3 LocalForward => CanonicalLocalForward;

        public void Configure(
            Transform placement,
            Transform motion,
            Vector3 ratio)
        {
            if (placement == null)
                throw new ArgumentNullException(nameof(placement));
            if (motion == null)
                throw new ArgumentNullException(nameof(motion));

            AvatarPresentation.ValidateRatio(ratio);

            placementRoot = placement;
            motionRoot = motion;
            sourceRatio = ratio;

            if (motionRoot.parent != placementRoot)
                motionRoot.SetParent(placementRoot, false);

            ResetMotion();
        }

        public void ApplyPlacement(Bounds bodyBounds)
        {
            RequireConfigured();

            placementRoot.localPosition =
                AvatarPresentation.ResolveBodyRatioPosition(
                    bodyBounds,
                    sourceRatio);
            placementRoot.localRotation = Quaternion.identity;
            placementRoot.localScale = Vector3.one;

            ResetMotion();
        }

        public void ResetMotion()
        {
            if (motionRoot == null)
                return;

            motionRoot.localPosition = Vector3.zero;
            motionRoot.localRotation = Quaternion.identity;
            motionRoot.localScale = Vector3.one;
        }

        private void RequireConfigured()
        {
            if (placementRoot == null)
            {
                throw new InvalidOperationException(
                    "EquipmentPresentation is missing PlacementRoot.");
            }

            if (motionRoot == null)
            {
                throw new InvalidOperationException(
                    "EquipmentPresentation is missing MotionRoot.");
            }
        }
    }
}
