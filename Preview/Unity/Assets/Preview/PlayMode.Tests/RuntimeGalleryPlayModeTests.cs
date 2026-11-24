using System;
using System.Collections;
using Hero.Voxel.Render.Runtime.Unity;
using Hero.Voxel.Runtime.Animation;
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
        public IEnumerator LeftAndRightAttackProduceDifferentProviderBodyResponse()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.Player.SampleMotion(
                    AvatarMotion.Attack,
                    0.5f,
                    EquipmentSlotSide.Left);
                Quaternion left =
                    fixture.Avatar.ResolveBodyPartWorld("Body").rotation;
                Vector3 leftMount =
                    SocketPosition(fixture.Equipment.Model.Instance, "Mount");

                fixture.Player.SampleMotion(
                    AvatarMotion.Attack,
                    0.5f,
                    EquipmentSlotSide.Right);
                Quaternion right =
                    fixture.Avatar.ResolveBodyPartWorld("Body").rotation;
                Vector3 rightMount =
                    SocketPosition(fixture.Equipment.Model.Instance, "Mount");

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
        public IEnumerator DifferentAuthoredDurationsStayOnSameProviderPhase()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.Player.SampleMotion(
                    AvatarMotion.Attack,
                    0.5f,
                    EquipmentSlotSide.Right);

                Quaternion equipment =
                    fixture.Equipment.Model.Instance
                        .GetPartWorldMatrix(0)
                        .rotation;
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        equipment),
                    Is.EqualTo(47.5f).Within(0.1f));
                AssertMountBound(fixture);
                yield return null;
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator AllSemanticMotionsUseProviderAuthoredPoseAndReset()
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
                    AssertMountBound(fixture);
                }

                fixture.Player.ResetMotion();
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.Avatar
                            .ResolveBodyPartWorld("Body")
                            .rotation),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.Equipment.Model.Instance
                            .GetPartWorldMatrix(0)
                            .rotation),
                    Is.LessThan(0.0001f));
                AssertMountBound(fixture);
                yield return null;
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator SwitchDisableAndStopResetProviderPresentation()
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
                        fixture.Equipment.Model.Instance
                            .GetPartWorldMatrix(0)
                            .rotation),
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
                        fixture.Avatar
                            .ResolveBodyPartWorld("Body")
                            .rotation),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.Equipment.Model.Instance
                            .GetPartWorldMatrix(0)
                            .rotation),
                    Is.LessThan(0.0001f));

                fixture.Player.enabled = true;
                yield return null;
                fixture.Player.Stop();
                AssertMountBound(fixture);
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator LoopOverrideWorksInBothDirectionsWithoutLocalClock()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.Avatar.StartMotion(
                    AvatarMotion.Attack,
                    EquipmentSlotSide.Right,
                    shouldLoop: true);
                float attackDuration = fixture.Avatar.CurrentDuration;
                Assert.That(
                    fixture.Avatar.Advance(attackDuration),
                    Is.EqualTo(0f).Within(0.0001f));
                Assert.IsTrue(fixture.Avatar.IsPlaying);

                fixture.Avatar.ResetPose();
                fixture.Avatar.StartMotion(
                    AvatarMotion.Idle,
                    EquipmentSlotSide.Right,
                    shouldLoop: false);
                float idleDuration = fixture.Avatar.CurrentDuration;
                Assert.That(
                    fixture.Avatar.Advance(idleDuration * 1.5f),
                    Is.EqualTo(1f).Within(0.0001f));
                Assert.IsFalse(fixture.Avatar.IsPlaying);
                yield return null;
            }
            finally
            {
                fixture.Dispose();
            }
        }

        private static void AssertMountBound(Fixture fixture)
        {
            Vector3 expected =
                fixture.Avatar
                    .ResolveBodySocketWorld("Equip.Hand.Right.0")
                    .MultiplyPoint3x4(Vector3.zero);
            Vector3 actual =
                SocketPosition(
                    fixture.Equipment.Model.Instance,
                    "Mount");
            Assert.That(
                Vector3.Distance(expected, actual),
                Is.LessThan(0.0001f));
        }

        private static Vector3 SocketPosition(
            VoxelRenderInstance instance,
            string socketName)
        {
            return instance
                .GetSocketWorldMatrix(instance.RequireSocket(socketName))
                .MultiplyPoint3x4(Vector3.zero);
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
                        PositionClip(
                            "WalkForward",
                            true,
                            new Vector3(0f, 0.2f, -0.2f),
                            0.6f),
                        PositionClip(
                            "Attack",
                            false,
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
                            new Vector3(0f, 0f, 0.2f),
                            0.3f),
                        RotationClip(
                            "Death",
                            false,
                            Quaternion.Euler(0f, 0f, 80f),
                            0.7f)
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
                        RotationClip(
                            "Idle",
                            true,
                            Quaternion.identity,
                            1.1f),
                        RotationClip(
                            "WalkForward",
                            true,
                            Quaternion.Euler(0f, 0f, 5f),
                            0.9f),
                        RotationClip(
                            "Attack",
                            false,
                            Quaternion.Euler(0f, 0f, 70f),
                            1.2f),
                        RotationClip(
                            "Hit",
                            false,
                            Quaternion.Euler(0f, 0f, 12f),
                            0.8f),
                        RotationClip(
                            "Death",
                            false,
                            Quaternion.Euler(0f, 0f, 82f),
                            1.4f)
                    });

            fixture.WorldRoot = new GameObject("WorldRoot");
            fixture.World =
                fixture.WorldRoot.AddComponent<VoxelRenderWorld>();
            fixture.World.Initialize();

            fixture.Unit = new GameObject("UnitRoot");
            fixture.Unit.transform.SetParent(
                fixture.WorldRoot.transform,
                false);
            var presentation =
                new GameObject("PresentationRoot");
            presentation.transform.SetParent(
                fixture.Unit.transform,
                false);

            var bodyObject = new GameObject("Body");
            bodyObject.transform.SetParent(
                presentation.transform,
                false);
            fixture.BodyModel =
                bodyObject.AddComponent<VoxelRenderPresentation>();
            fixture.BodyModel.Configure(
                fixture.World,
                bodyObject.transform,
                fixture.BodyAssembly,
                fixture.BodyAnimation,
                externalLifecycle: true);

            var equipmentObject = new GameObject("Equipment");
            equipmentObject.transform.SetParent(
                presentation.transform,
                false);
            VoxelRenderPresentation equipmentModel =
                equipmentObject.AddComponent<VoxelRenderPresentation>();
            equipmentModel.Configure(
                fixture.World,
                equipmentObject.transform,
                fixture.EquipmentAssembly,
                fixture.EquipmentAnimation,
                externalLifecycle: true);
            fixture.Equipment =
                equipmentObject.AddComponent<EquipmentPresentation>();
            fixture.Equipment.Configure(
                equipmentModel,
                "Equip.Hand.Right.0",
                EquipmentSlotSide.Right);

            fixture.Avatar =
                fixture.Unit.AddComponent<AvatarPresentation>();
            fixture.Avatar.Configure(
                fixture.Unit.transform,
                presentation.transform,
                fixture.BodyModel,
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
            public GameObject WorldRoot;
            public GameObject Unit;
            public VoxelRenderWorld World;
            public Mesh Mesh;
            public VoxelAssemblyAsset BodyAssembly;
            public VoxelAnimationAsset BodyAnimation;
            public VoxelAssemblyAsset EquipmentAssembly;
            public VoxelAnimationAsset EquipmentAnimation;
            public VoxelRenderPresentation BodyModel;
            public AvatarPresentation Avatar;
            public AvatarMotionPlayer Player;
            public EquipmentPresentation Equipment;

            public void Dispose()
            {
                Avatar?.ReleaseProvider();
                if (WorldRoot != null)
                    UnityEngine.Object.Destroy(WorldRoot);
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
