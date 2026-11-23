using System;
using Hero.Voxel.Runtime.Assets;
using NUnit.Framework;
using UnityEngine;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor.Tests
{
    public sealed class AvatarPresentationTests
    {
        [Test]
        public void AvatarMotionSetIsSemanticAndDeterministic()
        {
            CollectionAssert.AreEqual(
                new[]
                {
                    "Idle",
                    "WalkForward",
                    "Attack",
                    "Hit",
                    "Death"
                },
                Enum.GetNames(typeof(AvatarMotion)));
        }

        [Test]
        public void PresentationConsumesCanonicalModelFrame()
        {
            Assert.AreEqual(
                Vector3.right,
                AvatarPresentation.CanonicalLocalRight);
            Assert.AreEqual(
                Vector3.up,
                AvatarPresentation.CanonicalLocalUp);
            Assert.AreEqual(
                Vector3.back,
                AvatarPresentation.CanonicalLocalForward);
            Assert.AreEqual(
                AvatarPresentation.CanonicalLocalForward,
                EquipmentPresentation.CanonicalLocalForward);
        }

        [Test]
        public void LeftAndRightSocketsArePhysicalDistinctBindings()
        {
            Fixture fixture = CreateFixture(includeSecondEquipment: true);
            try
            {
                Assert.AreEqual(2, fixture.Avatar.Equipment.Count);
                Assert.AreEqual(
                    EquipmentSlotSide.Right,
                    fixture.Primary.Side);
                Assert.AreEqual(
                    EquipmentSlotSide.Left,
                    fixture.Secondary.Side);
                Assert.That(
                    Vector3.Distance(
                        fixture.Primary.PlacementRoot.localPosition,
                        fixture.Secondary.PlacementRoot.localPosition),
                    Is.GreaterThan(0.5f));
                Assert.That(
                    fixture.Primary.PlacementRoot.localPosition.x,
                    Is.GreaterThan(0f));
                Assert.That(
                    fixture.Secondary.PlacementRoot.localPosition.x,
                    Is.LessThan(0f));
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void RepeatedEquipmentCanBindTwoLeftSlotsIndependently()
        {
            Fixture fixture = CreateFixture(
                includeSecondEquipment: true,
                primarySocketName: "Equip.Hand.Left.0",
                primarySide: EquipmentSlotSide.Left,
                secondarySocketName: "Equip.Hand.Left.1",
                secondarySide: EquipmentSlotSide.Left);
            try
            {
                Assert.AreEqual(2, fixture.Avatar.Equipment.Count);
                Assert.AreSame(
                    fixture.Primary.AnimatedModel.Assembly,
                    fixture.Secondary.AnimatedModel.Assembly);
                Assert.AreEqual(
                    EquipmentSlotSide.Left,
                    fixture.Primary.Side);
                Assert.AreEqual(
                    EquipmentSlotSide.Left,
                    fixture.Secondary.Side);
                Assert.That(
                    Vector3.Distance(
                        fixture.Primary.PlacementRoot.localPosition,
                        fixture.Secondary.PlacementRoot.localPosition),
                    Is.GreaterThan(0.1f));
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void LeftAndRightAttackSelectDifferentWholeBodyClips()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.Player.SampleMotion(
                    AvatarMotion.Attack,
                    0.5f,
                    EquipmentSlotSide.Left);
                Quaternion left =
                    fixture.BodyPart.localRotation;

                fixture.Player.SampleMotion(
                    AvatarMotion.Attack,
                    0.5f,
                    EquipmentSlotSide.Right);
                Quaternion right =
                    fixture.BodyPart.localRotation;

                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        left),
                    Is.GreaterThan(10f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        right),
                    Is.GreaterThan(10f));
                Assert.That(
                    Quaternion.Angle(left, right),
                    Is.GreaterThan(20f));

                // Current avatars really are one animated Body part.
                Assert.AreEqual(
                    1,
                    fixture.BodyAssembly.PartCount);
                Assert.AreEqual(
                    "Body",
                    fixture.BodyAssembly.GetPart(0).Name);
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void WeaponAttackUsesOwnAuthoredClipAndMountStaysBound()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.Player.SampleMotion(
                    AvatarMotion.Attack,
                    0.5f,
                    EquipmentSlotSide.Right);

                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.PrimaryPart.localRotation),
                    Is.GreaterThan(20f));

                Matrix4x4 bodySocket =
                    fixture.Avatar.ResolveBodySocketInPresentation(
                        "Equip.Hand.Right.0");
                Vector3 bodySocketOrigin =
                    bodySocket.MultiplyPoint3x4(Vector3.zero);
                Matrix4x4 mountWorldInPresentation =
                    fixture.Primary.PlacementRoot.localToWorldMatrix *
                    fixture.Primary.AnimatedModel.ModelRoot.localToWorldMatrix.inverse;
                _ = mountWorldInPresentation;

                Assert.That(
                    Vector3.Distance(
                        bodySocketOrigin,
                        fixture.Primary.PlacementRoot.localPosition),
                    Is.LessThan(0.0001f));
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void RepeatedSampleResetDoesNotDriftSocketPlacement()
        {
            Fixture fixture = CreateFixture();
            try
            {
                Vector3 neutralPlacement =
                    fixture.Primary.PlacementRoot.localPosition;

                for (int i = 0; i < 3; i++)
                {
                    fixture.Player.SampleMotion(
                        AvatarMotion.Attack,
                        0.5f,
                        EquipmentSlotSide.Right);
                    fixture.Player.ResetMotion();

                    Assert.That(
                        Vector3.Distance(
                            neutralPlacement,
                            fixture.Primary.PlacementRoot.localPosition),
                        Is.LessThan(0.0001f));
                    Assert.That(
                        Quaternion.Angle(
                            Quaternion.identity,
                            fixture.BodyPart.localRotation),
                        Is.LessThan(0.0001f));
                    Assert.That(
                        Quaternion.Angle(
                            Quaternion.identity,
                            fixture.PrimaryPart.localRotation),
                        Is.LessThan(0.0001f));
                }
            }
            finally
            {
                fixture.Dispose();
            }
        }

        private static Fixture CreateFixture(
            bool includeSecondEquipment = false,
            string primarySocketName = "Equip.Hand.Right.0",
            EquipmentSlotSide primarySide = EquipmentSlotSide.Right,
            string secondarySocketName = "Equip.Hand.Left.0",
            EquipmentSlotSide secondarySide = EquipmentSlotSide.Left)
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
                        new Vector3(-0.6f, 0.1f, 0f),
                        Quaternion.identity),
                    new VoxelAssemblySocket(
                        "Equip.Hand.Left.1",
                        0,
                        new Vector3(-0.9f, 0.1f, 0.15f),
                        Quaternion.identity),
                    new VoxelAssemblySocket(
                        "Equip.Hand.Right.0",
                        0,
                        new Vector3(0.6f, 0.1f, 0f),
                        Quaternion.identity)
                });
            fixture.BodyAnimation =
                CreateBodyAnimation(fixture.BodyAssembly);

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
                CreateEquipmentAnimation(
                    fixture.EquipmentAssembly);

            fixture.Unit = new GameObject("UnitRoot");
            GameObject presentationObject =
                new GameObject("PresentationRoot");
            presentationObject.transform.SetParent(
                fixture.Unit.transform,
                false);

            GameObject bodyRoot =
                new GameObject("BodyModel");
            bodyRoot.transform.SetParent(
                presentationObject.transform,
                false);
            GameObject bodyPartObject =
                new GameObject("Body");
            bodyPartObject.transform.SetParent(
                bodyRoot.transform,
                false);
            MeshFilter bodyFilter =
                bodyPartObject.AddComponent<MeshFilter>();
            bodyFilter.sharedMesh = fixture.Mesh;
            fixture.BodyPart = bodyPartObject.transform;

            VoxelAnimatedAssembly animatedBody =
                bodyRoot.AddComponent<VoxelAnimatedAssembly>();
            animatedBody.Configure(
                bodyRoot.transform,
                fixture.BodyAssembly,
                fixture.BodyAnimation,
                new[] { bodyPartObject.transform });

            fixture.Primary = CreateEquipment(
                presentationObject.transform,
                fixture,
                primarySocketName,
                primarySide,
                "Primary",
                out Transform primaryPart);
            fixture.PrimaryPart = primaryPart;

            EquipmentPresentation[] equipment;
            if (includeSecondEquipment)
            {
                fixture.Secondary = CreateEquipment(
                    presentationObject.transform,
                    fixture,
                    secondarySocketName,
                    secondarySide,
                    "Secondary",
                    out _);
                equipment =
                    new[] { fixture.Primary, fixture.Secondary };
            }
            else
            {
                equipment = new[] { fixture.Primary };
            }

            fixture.Avatar =
                fixture.Unit.AddComponent<AvatarPresentation>();
            fixture.Avatar.Configure(
                fixture.Unit.transform,
                presentationObject.transform,
                bodyFilter,
                animatedBody,
                equipment);

            fixture.Player =
                fixture.Unit.AddComponent<AvatarMotionPlayer>();
            fixture.Player.Configure(
                fixture.Avatar,
                AvatarMotion.Idle,
                false,
                1f);

            return fixture;
        }

        private static EquipmentPresentation CreateEquipment(
            Transform presentationRoot,
            Fixture fixture,
            string socketName,
            EquipmentSlotSide side,
            string name,
            out Transform partTransform)
        {
            GameObject placement =
                new GameObject(name + " Placement");
            placement.transform.SetParent(
                presentationRoot,
                false);

            GameObject modelRoot =
                new GameObject(name + " Model");
            modelRoot.transform.SetParent(
                placement.transform,
                false);
            GameObject part =
                new GameObject("Weapon");
            part.transform.SetParent(
                modelRoot.transform,
                false);
            MeshFilter filter =
                part.AddComponent<MeshFilter>();
            filter.sharedMesh = fixture.Mesh;
            partTransform = part.transform;

            VoxelAnimatedAssembly animated =
                modelRoot.AddComponent<VoxelAnimatedAssembly>();
            animated.Configure(
                modelRoot.transform,
                fixture.EquipmentAssembly,
                fixture.EquipmentAnimation,
                new[] { part.transform });

            EquipmentPresentation equipment =
                placement.AddComponent<EquipmentPresentation>();
            equipment.Configure(
                placement.transform,
                animated,
                socketName,
                side);
            return equipment;
        }

        private static VoxelAnimationAsset CreateBodyAnimation(
            VoxelAssemblyAsset assembly)
        {
            return VoxelAnimationAsset.Create(
                assembly,
                new[]
                {
                    PositionClip("Idle", true, Vector3.zero, Vector3.zero, 1f),
                    PositionClip(
                        "WalkForward",
                        true,
                        Vector3.zero,
                        new Vector3(0f, 0.2f, -0.2f),
                        0.6f),
                    PositionClip(
                        "Attack",
                        false,
                        Vector3.zero,
                        new Vector3(0f, 0f, -0.25f),
                        0.4f),
                    RotationClip(
                        "Attack.Left",
                        false,
                        Quaternion.Euler(0f, 0f, -25f),
                        0.4f),
                    RotationClip(
                        "Attack.Right",
                        false,
                        Quaternion.Euler(0f, 0f, 25f),
                        0.4f),
                    PositionClip(
                        "Hit",
                        false,
                        Vector3.zero,
                        new Vector3(0f, 0f, 0.2f),
                        0.3f),
                    RotationClip(
                        "Death",
                        false,
                        Quaternion.Euler(0f, 0f, 80f),
                        0.7f)
                });
        }

        private static VoxelAnimationAsset CreateEquipmentAnimation(
            VoxelAssemblyAsset assembly)
        {
            return VoxelAnimationAsset.Create(
                assembly,
                new[]
                {
                    RotationClip("Idle", true, Quaternion.identity, 1.1f),
                    RotationClip("WalkForward", true, Quaternion.Euler(0f, 0f, 5f), 0.9f),
                    RotationClip("Attack", false, Quaternion.Euler(0f, 0f, 70f), 1.2f),
                    RotationClip("Hit", false, Quaternion.Euler(0f, 0f, 12f), 0.8f),
                    RotationClip("Death", false, Quaternion.Euler(0f, 0f, 82f), 1.4f)
                });
        }

        private static VoxelAnimationClip PositionClip(
            string name,
            bool loop,
            Vector3 start,
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
                                new VoxelVectorKey(0f, start),
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
                                new VoxelQuaternionKey(
                                    0f,
                                    Quaternion.identity),
                                new VoxelQuaternionKey(
                                    1f,
                                    end)
                            }),
                        null)
                });
        }

        private static Mesh CreateMesh()
        {
            var mesh = new Mesh
            {
                name = "VoxelStore Test Mesh"
            };
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
            public EquipmentPresentation Primary;
            public EquipmentPresentation Secondary;
            public Transform BodyPart;
            public Transform PrimaryPart;

            public void Dispose()
            {
                if (Unit != null)
                    UnityEngine.Object.DestroyImmediate(Unit);
                if (BodyAnimation != null)
                    UnityEngine.Object.DestroyImmediate(BodyAnimation);
                if (EquipmentAnimation != null)
                    UnityEngine.Object.DestroyImmediate(EquipmentAnimation);
                if (BodyAssembly != null)
                    UnityEngine.Object.DestroyImmediate(BodyAssembly);
                if (EquipmentAssembly != null)
                    UnityEngine.Object.DestroyImmediate(EquipmentAssembly);
                if (Mesh != null)
                    UnityEngine.Object.DestroyImmediate(Mesh);
            }
        }
    }
}
