using System;
using UnityEngine;

namespace VoxelStore.Preview.Runtime
{
    internal sealed class AttachmentReferenceInfluenceResolver
    {
        private const float MatrixTolerance = 0.001f;
        private const float AxisEpsilon = 0.000001f;

        private bool initialized;
        private Matrix4x4 releaseBasisWorld = Matrix4x4.identity;

        public bool IsInitialized => initialized;

        public void Reset()
        {
            initialized = false;
            releaseBasisWorld = Matrix4x4.identity;
        }

        public Matrix4x4 Resolve(
            Matrix4x4 currentReferenceWorld,
            float referenceInfluence)
        {
            ValidateInfluence(referenceInfluence);
            RequireFinite(currentReferenceWorld, nameof(currentReferenceWorld));

            if (!initialized)
            {
                releaseBasisWorld = currentReferenceWorld;
                initialized = true;
            }

            if (referenceInfluence >= 1f)
            {
                releaseBasisWorld = currentReferenceWorld;
                return currentReferenceWorld;
            }

            if (referenceInfluence <= 0f)
                return releaseBasisWorld;

            Decompose(
                releaseBasisWorld,
                out Vector3 releasePosition,
                out Quaternion releaseRotation,
                out Vector3 releaseScale);
            Decompose(
                currentReferenceWorld,
                out Vector3 referencePosition,
                out Quaternion referenceRotation,
                out Vector3 referenceScale);

            Matrix4x4 resolved = Matrix4x4.TRS(
                Vector3.LerpUnclamped(
                    releasePosition,
                    referencePosition,
                    referenceInfluence),
                Quaternion.SlerpUnclamped(
                    releaseRotation,
                    referenceRotation,
                    referenceInfluence),
                Vector3.LerpUnclamped(
                    releaseScale,
                    referenceScale,
                    referenceInfluence));
            RequireFinite(resolved, "resolved attachment reference");
            return resolved;
        }

        private static void ValidateInfluence(float value)
        {
            if (!float.IsFinite(value) ||
                value < 0f ||
                value > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Reference influence must be finite and within [0,1].");
            }
        }

        private static void Decompose(
            Matrix4x4 value,
            out Vector3 position,
            out Quaternion rotation,
            out Vector3 scale)
        {
            RequireFinite(value, "attachment reference");
            if (Mathf.Abs(value.m30) > MatrixTolerance ||
                Mathf.Abs(value.m31) > MatrixTolerance ||
                Mathf.Abs(value.m32) > MatrixTolerance ||
                Mathf.Abs(value.m33 - 1f) > MatrixTolerance)
            {
                throw new InvalidOperationException(
                    "Attachment reference must be an affine TRS matrix.");
            }

            position = new Vector3(
                value.m03,
                value.m13,
                value.m23);
            Vector3 x = new Vector3(
                value.m00,
                value.m10,
                value.m20);
            Vector3 y = new Vector3(
                value.m01,
                value.m11,
                value.m21);
            Vector3 z = new Vector3(
                value.m02,
                value.m12,
                value.m22);

            float xLength = x.magnitude;
            float yLength = y.magnitude;
            float zLength = z.magnitude;
            if (xLength <= AxisEpsilon ||
                yLength <= AxisEpsilon ||
                zLength <= AxisEpsilon)
            {
                throw new InvalidOperationException(
                    "Attachment reference contains a singular scale.");
            }

            Vector3 xAxis = x / xLength;
            Vector3 yAxis = y / yLength;
            Vector3 zAxis = z / zLength;
            if (Mathf.Abs(Vector3.Dot(xAxis, yAxis)) > MatrixTolerance ||
                Mathf.Abs(Vector3.Dot(xAxis, zAxis)) > MatrixTolerance ||
                Mathf.Abs(Vector3.Dot(yAxis, zAxis)) > MatrixTolerance ||
                Vector3.Dot(Vector3.Cross(xAxis, yAxis), zAxis) <
                1f - MatrixTolerance)
            {
                throw new InvalidOperationException(
                    "Attachment reference contains shear or mirrored axes.");
            }

            rotation = Quaternion.LookRotation(zAxis, yAxis);
            scale = new Vector3(
                xLength,
                yLength,
                zLength);

            Matrix4x4 reconstructed =
                Matrix4x4.TRS(position, rotation, scale);
            if (MaxMatrixDelta(value, reconstructed) > MatrixTolerance)
            {
                throw new InvalidOperationException(
                    "Attachment reference cannot be decomposed as stable TRS.");
            }
        }

        private static void RequireFinite(
            Matrix4x4 value,
            string label)
        {
            for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
            {
                if (!float.IsFinite(value[row, column]))
                {
                    throw new InvalidOperationException(
                        label + " contains non-finite values.");
                }
            }
        }

        private static float MaxMatrixDelta(
            Matrix4x4 left,
            Matrix4x4 right)
        {
            float max = 0f;
            for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
            {
                max = Mathf.Max(
                    max,
                    Mathf.Abs(left[row, column] - right[row, column]));
            }

            return max;
        }
    }
}
