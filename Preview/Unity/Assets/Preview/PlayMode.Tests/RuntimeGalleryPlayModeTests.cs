using System;
using System.Collections;
using Hero.Voxel.Runtime.Assets;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.PlayMode.Tests
{
    public sealed class RuntimeGalleryPlayModeTests
    {
        [UnityTest]
        public IEnumerator LeftAndRightAttackProduceDifferentWholeBodyResponse()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.Player.SampleMotion(
                    AvatarMotion.Attack,
                    0.5f,
                    EquipmentSlotSide.Left);
                Quaternion left = fixture.BodyPart.localRotation;
                Vector3 leftMount =
                    fixture.Equipment.PlacementRoot.localPosition;

                fixture.Player.SampleMotion(
                    AvatarMotion.Attack,
                    0.5f,
                    EquipmentSlotSide.Right);
                Quaternion right = fixture.BodyPart.localRotation;
                Vector3 rightMount =
                    fixture.Equipment.PlacementRoot.localPosition;

                Assert.That(
                    Quaternion.Angle(left, right),
                    Is.GreaterThan(20f));
                Assert.That(
                    Vector3.Distance(leftMount, rightMount),
                    Is.GreaterThan(0.1f));
                yield return null;
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator DifferentAuthoredDurationsStayOnSameNormalizedPhase()
        {
            Fixture fixture = CreateFixture();
            try
            {
                Assert.That(
                    fixture.Avatar.BodyAnimation.GetClipDuration(
                        AvatarMotion.Attack,
                        EquipmentSlotSide.Right),
                    Is.EqualTo(0.4f).Within(0.0001f));
                Assert.That(
                    fixture.Equipment.AnimatedModel.GetClipDuration(
                        AvatarMotion.Attack,
                        EquipmentSlotSide.Right),
                    Is.EqualTo(1.2f).Within(0.0001f));

                fixture.Player.SampleMotion(
                    AvatarMotion.Attack,
                    0.5f,
                    EquipmentSlotSide.Right);

                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.EquipmentPart.localRotation),
                    Is.InRange(25f, 45f));
                AssertMountLocked(fixture);
                yield return null;
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator AllSemanticMotionsUseAuthoredPoseAndReset()
        {
            Fixture fixture = CreateFixture();
            try
            {
                foreach (AvatarMotion motion in Enum.GetValues(typeof(AvatarMotion)))
                {
                    fixture.Player.SampleMotion(
                        motion,
                        0.65f,
                        EquipmentSlotSide.Right);
                    AssertMountLocked(fixture);
                }

                fixture.Player.ResetMotion();
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.BodyPart.localRotation),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.EquipmentPart.localRotation),
                    Is.LessThan(0.0001f));
                AssertMountLocked(fixture);
                yield return null;
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator SwitchDisableAndStopResetAuthoredPresentation()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.Player.Play(
                    AvatarMotion.Attack,
                    false,
                    1f);
                yield return new WaitForSeconds(0.12f);
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.EquipmentPart.localRotation),
                    Is.GreaterThan(1f));

                fixture.Player.Play(
                    AvatarMotion.Hit,
                    false,
                    1f);
                Assert.AreEqual(
                    AvatarMotion.Hit,
                    fixture.Player.CurrentMotion);

                fixture.Player.enabled = false;
                yield return null;
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.BodyPart.localRotation),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.EquipmentPart.localRotation),
                    Is.LessThan(0.0001f));

                fixture.Player.enabled = true;
                yield return null;
                fixture.Player.Stop();
                AssertMountLocked(fixture);
            }
            finally
            {
                fixture.Dispose();
            }
        }

        private static void AssertMountLocked(Fixture fixture)
        {
            Matrix4x4 bodySocket =
                fixture.Avatar.ResolveBodySocketInPresentation(
                    "Equip.Hand.Right.0");
            Vector3 expected =
                bodySocket.MultiplyPoint3x4(Vector3.zero);
            Assert.That(
                Vector3.Distance(
                    expected,
                    fixture.Equipment.PlacementRoot.localPosition),
                Is.LessThan(0.0001f));
        }

        private static Fixture CreateFixture()
        {
            var fixture = new Fixture();
            fixture.Mesh = CreateMesh();

            fixture.BodyAssembly = VoxelAssemblyAsset.Create(
                "BodyFixture",
                8,
                new[]
                {
                    new VoxelAssemblyPart(
                        "Body",
                        fixture.Mesh,
                        -1,
                        Vector3.zero,
                        Quaternion.identity,
                        Vector3.one)
                },
                new[]
                {
                    new VoxelAssemblySocket(
                        "Equip.Hand.Left.0",
                        0,
                        new Vector3(-0.6f, 0f, 0f),
                        Quaternion.identity),
                    new VoxelAssemblySocket(
                        "Equip.Hand.Right.0",
                        0,
                        new Vector3(0.6f, 0f, 0f),
                        Quaternion.identity)
                });
            fixture.BodyAnimation =
                VoxelAnimationAsset.Create(
                    fixture.BodyAssembly,
                    new[]
                    {
                        PositionClip("Idle", true, Vector3.zero, 1f),
                        PositionClip("WalkForward", true, new Vector3(0f, 0.2f, -0.2f), 0.6f),
                        PositionClip("Attack", false, new Vector3(0f, 0f, -0.25f), 0.4f),
                        RotationClip("Attack.Left", false, Quaternion.Euler(0f, 0f, -25f), 0.4f),
                        RotationClip("Attack.Right", false, Quaternion.Euler(0f, 0f, 25f), 0.4f),
                        PositionClip("Hit", false, new Vector3(0f, 0f, 0.2f), 0.3f),
                        RotationClip("Death", false, Quaternion.Euler(0f, 0f, 80f), 0.7f)
                    });

            fixture.EquipmentAssembly = VoxelAssemblyAsset.Create(
                "EquipmentFixture",
                8,
                new[]
                {
                    new VoxelAssemblyPart(
                        "Weapon",
                        fixture.Mesh,
                        -1,
                        Vector3.zero,
                        Quaternion.identity,
                        Vector3.one)
                },
                new[]
                {
                    new VoxelAssemblySocket(
                        "Mount",
                        0,
                        Vector3.zero,
                        Quaternion.identity)
                });
            fixture.EquipmentAnimation =
                VoxelAnimationAsset.Create(
                    fixture.EquipmentAssembly,
                    new[]
                    {
                        RotationClip("Idle", true, Quaternion.identity, 1.1f),
                        RotationClip("WalkForward", true, Quaternion.Euler(0f, 0f, 5f), 0.9f),
                        RotationClip("Attack", false, Quaternion.Euler(0f, 0f, 70f), 1.2f),
                        RotationClip("Hit", false, Quaternion.Euler(0f, 0f, 12f), 0.8f),
                        RotationClip("Death", false, Quaternion.Euler(0f, 0f, 82f), 1.4f)
                    });

            fixture.Unit = new GameObject("UnitRoot");
            GameObject presentation =
                new GameObject("PresentationRoot");
            presentation.transform.SetParent(
                fixture.Unit.transform,
                false);

            GameObject bodyRoot =
                new GameObject("BodyModel");
            bodyRoot.transform.SetParent(
                presentation.transform,
                false);
            GameObject bodyPart =
                new GameObject("Body");
            bodyPart.transform.SetParent(
                bodyRoot.transform,
                false);
            MeshFilter bodyFilter =
                bodyPart.AddComponent<MeshFilter>();
            bodyFilter.sharedMesh = fixture.Mesh;
            fixture.BodyPart = bodyPart.transform;

            VoxelAnimatedAssembly animatedBody =
                bodyRoot.AddComponent<VoxelAnimatedAssembly>();
            animatedBody.Configure(
                bodyRoot.transform,
                fixture.BodyAssembly,
                fixture.BodyAnimation,
                new[] { bodyPart.transform });

            GameObject placement =
                new GameObject("Equipment");
            placement.transform.SetParent(
                presentation.transform,
                false);
            GameObject equipmentRoot =
                new GameObject("Model");
            equipmentRoot.transform.SetParent(
                placement.transform,
                false);
            GameObject equipmentPart =
                new GameObject("Weapon");
            equipmentPart.transform.SetParent(
                equipmentRoot.transform,
                false);
            equipmentPart.AddComponent<MeshFilter>().sharedMesh =
                fixture.Mesh;
            fixture.EquipmentPart =
                equipmentPart.transform;

            VoxelAnimatedAssembly animatedEquipment =
                equipmentRoot.AddComponent<VoxelAnimatedAssembly>();
            animatedEquipment.Configure(
                equipmentRoot.transform,
                fixture.EquipmentAssembly,
                fixture.EquipmentAnimation,
                new[] { equipmentPart.transform });

            fixture.Equipment =
                placement.AddComponent<EquipmentPresentation>();
            fixture.Equipment.Configure(
                placement.transform,
                animatedEquipment,
                "Equip.Hand.Right.0",
                EquipmentSlotSide.Right);

            fixture.Avatar =
                fixture.Unit.AddComponent<AvatarPresentation>();
            fixture.Avatar.Configure(
                fixture.Unit.transform,
                presentation.transform,
                bodyFilter,
                animatedBody,
                new[] { fixture.Equipment });

            fixture.Player =
                fixture.Unit.AddComponent<AvatarMotionPlayer>();
            fixture.Player.Configure(
                fixture.Avatar,
                AvatarMotion.Idle,
                false,
                1f);

            return fixture;
        }

        private static VoxelAnimationClip PositionClip(
            string name,
            bool loop,
            Vector3 end,
            float duration)
        {
            return new VoxelAnimationClip(
                name,
                duration,
                loop,
                new[]
                {
                    new VoxelAnimationTrack(
                        0,
                        new VoxelVectorChannel(
                            VoxelAnimationInterpolation.SmoothStep,
                            new[]
                            {
                                new VoxelVectorKey(0f, Vector3.zero),
                                new VoxelVectorKey(1f, end)
                            }),
                        null,
                        null)
                });
        }

        private static VoxelAnimationClip RotationClip(
            string name,
            bool loop,
            Quaternion end,
            float duration)
        {
            return new VoxelAnimationClip(
                name,
                duration,
                loop,
                new[]
                {
                    new VoxelAnimationTrack(
                        0,
                        null,
                        new VoxelQuaternionChannel(
                            VoxelAnimationInterpolation.SmoothStep,
                            new[]
                            {
                                new VoxelQuaternionKey(0f, Quaternion.identity),
                                new VoxelQuaternionKey(1f, end)
                            }),
                        null)
                });
        }

        private static Mesh CreateMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(0f, 1f, 0.5f)
            };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.RecalculateBounds();
            return mesh;
        }

        private sealed class Fixture : IDisposable
        {
            public GameObject Unit;
            public Mesh Mesh;
            public VoxelAssemblyAsset BodyAssembly;
            public VoxelAnimationAsset BodyAnimation;
            public VoxelAssemblyAsset EquipmentAssembly;
            public VoxelAnimationAsset EquipmentAnimation;
            public AvatarPresentation Avatar;
            public AvatarMotionPlayer Player;
            public EquipmentPresentation Equipment;
            public Transform BodyPart;
            public Transform EquipmentPart;

            public void Dispose()
            {
                if (Unit != null)
                    UnityEngine.Object.Destroy(Unit);
                if (BodyAnimation != null)
                    UnityEngine.Object.Destroy(BodyAnimation);
                if (EquipmentAnimation != null)
                    UnityEngine.Object.Destroy(EquipmentAnimation);
                if (BodyAssembly != null)
                    UnityEngine.Object.Destroy(BodyAssembly);
                if (EquipmentAssembly != null)
                    UnityEngine.Object.Destroy(EquipmentAssembly);
                if (Mesh != null)
                    UnityEngine.Object.Destroy(Mesh);
            }
        }
    }
}
