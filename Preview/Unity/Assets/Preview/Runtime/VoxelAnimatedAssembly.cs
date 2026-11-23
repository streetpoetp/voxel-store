using System;
using System.Collections.Generic;
using Hero.Voxel.Runtime.Animation;
using Hero.Voxel.Runtime.Assets;
using UnityEngine;

namespace VoxelStore.Preview.Runtime
{
    [DisallowMultipleComponent]
    public sealed class VoxelAnimatedAssembly : MonoBehaviour
    {
        [SerializeField] private Transform modelRoot;
        [SerializeField] private VoxelAssemblyAsset assembly;
        [SerializeField] private VoxelAnimationAsset animationAsset;
        [SerializeField] private Transform[] partTransforms =
            Array.Empty<Transform>();

        private VoxelAssemblyPose pose;
        private VoxelAssemblyPoseEvaluator evaluator;
        private Matrix4x4[] bindSocketMatrices =
            Array.Empty<Matrix4x4>();

        public Transform ModelRoot => modelRoot;
        public VoxelAssemblyAsset Assembly => assembly;
        public VoxelAnimationAsset Animation => animationAsset;

        public void Configure(
            Transform root,
            VoxelAssemblyAsset assemblyAsset,
            VoxelAnimationAsset animation,
            IReadOnlyList<Transform> parts)
        {
            modelRoot = root != null
                ? root
                : throw new ArgumentNullException(nameof(root));
            assembly = assemblyAsset != null
                ? assemblyAsset
                : throw new ArgumentNullException(nameof(assemblyAsset));
            animationAsset = animation != null
                ? animation
                : throw new ArgumentNullException(nameof(animation));

            if (parts == null ||
                parts.Count != assembly.PartCount)
            {
                throw new ArgumentException(
                    "Part transform count must match the baked assembly.",
                    nameof(parts));
            }

            partTransforms =
                new Transform[parts.Count];
            for (int i = 0; i < parts.Count; i++)
            {
                partTransforms[i] = parts[i] != null
                    ? parts[i]
                    : throw new ArgumentException(
                        "Part transforms cannot contain null entries.",
                        nameof(parts));
            }

            RebuildRuntimeState();
        }

        private void Awake()
        {
            RebuildRuntimeStateIfSerialized();
        }

        private void OnEnable()
        {
            RebuildRuntimeStateIfSerialized();
        }

        public void ResetPose()
        {
            RequireConfigured();
            EvaluateBindPose();
            ApplyEvaluatedParts();
        }

        public void Sample(
            AvatarMotion motion,
            EquipmentSlotSide side,
            float normalizedTime)
        {
            RequireConfigured();
            VoxelAnimationClip clip =
                ResolveClip(motion, side);

            pose.Reset();
            VoxelAnimationSampler.SampleNormalized(
                clip,
                Mathf.Clamp01(normalizedTime),
                pose);
            evaluator.Evaluate(
                Matrix4x4.identity,
                pose);
            ApplyEvaluatedParts();
        }

        public float GetClipDuration(
            AvatarMotion motion,
            EquipmentSlotSide side)
        {
            RequireConfigured();
            return ResolveClip(motion, side).Duration;
        }

        public Matrix4x4 GetSocketLocalMatrix(
            string socketName)
        {
            RequireConfigured();
            int index =
                RequireSocketIndex(socketName);
            return evaluator.GetSocketWorldMatrix(index);
        }

        public Matrix4x4 GetBindSocketLocalMatrix(
            string socketName)
        {
            RequireConfigured();
            return bindSocketMatrices[
                RequireSocketIndex(socketName)];
        }

        private VoxelAnimationClip ResolveClip(
            AvatarMotion motion,
            EquipmentSlotSide side)
        {
            string baseName = motion.ToString();
            string sideName =
                baseName + "." + side;

            if (animationAsset.TryGetClipIndex(
                    sideName,
                    out int sideIndex))
            {
                return animationAsset.GetClip(sideIndex);
            }

            if (animationAsset.TryGetClipIndex(
                    baseName,
                    out int baseIndex))
            {
                return animationAsset.GetClip(baseIndex);
            }

            throw new InvalidOperationException(
                "Required semantic animation clip is missing: " +
                baseName +
                " on " +
                assembly.AssemblyName);
        }

        private void EvaluateBindPose()
        {
            pose.Reset();
            evaluator.Evaluate(
                Matrix4x4.identity,
                pose);
        }

        private void ApplyEvaluatedParts()
        {
            for (int i = 0;
                 i < assembly.PartCount;
                 i++)
            {
                VoxelAssemblyPart part =
                    assembly.GetPart(i);
                Matrix4x4 matrix =
                    evaluator.GetPartWorldMatrix(i);

                if (part.ParentIndex >= 0)
                {
                    matrix =
                        evaluator
                            .GetPartWorldMatrix(
                                part.ParentIndex)
                            .inverse *
                        matrix;
                }

                ApplyLocalMatrix(
                    partTransforms[i],
                    matrix);
            }
        }

        private int RequireSocketIndex(
            string socketName)
        {
            if (!assembly.TryGetSocketIndex(
                    socketName,
                    out int index))
            {
                throw new InvalidOperationException(
                    "Required socket is missing: " +
                    socketName +
                    " on " +
                    assembly.AssemblyName);
            }

            return index;
        }

        private void RequireConfigured()
        {
            if (pose == null ||
                evaluator == null ||
                bindSocketMatrices == null ||
                bindSocketMatrices.Length !=
                (assembly != null
                    ? assembly.SocketCount
                    : 0))
            {
                RebuildRuntimeStateIfSerialized();
            }

            if (modelRoot == null ||
                assembly == null ||
                animationAsset == null ||
                pose == null ||
                evaluator == null ||
                partTransforms == null ||
                partTransforms.Length !=
                assembly.PartCount)
            {
                throw new InvalidOperationException(
                    "VoxelAnimatedAssembly is not configured.");
            }
        }

        private void RebuildRuntimeStateIfSerialized()
        {
            if (modelRoot == null ||
                assembly == null ||
                animationAsset == null ||
                partTransforms == null ||
                partTransforms.Length !=
                assembly.PartCount)
            {
                return;
            }

            for (int i = 0;
                 i < partTransforms.Length;
                 i++)
            {
                if (partTransforms[i] == null)
                    return;
            }

            RebuildRuntimeState();
        }

        private void RebuildRuntimeState()
        {
            if (animationAsset.Assembly != assembly)
            {
                throw new InvalidOperationException(
                    "Voxel animation asset must target the configured assembly.");
            }

            pose = new VoxelAssemblyPose(assembly);
            evaluator =
                new VoxelAssemblyPoseEvaluator(assembly);
            bindSocketMatrices =
                new Matrix4x4[assembly.SocketCount];

            EvaluateBindPose();
            for (int i = 0;
                 i < bindSocketMatrices.Length;
                 i++)
            {
                bindSocketMatrices[i] =
                    evaluator.GetSocketWorldMatrix(i);
            }

            ApplyEvaluatedParts();
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
