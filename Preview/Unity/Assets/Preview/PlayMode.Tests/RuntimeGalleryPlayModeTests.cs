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
        public IEnumerator WeaponAttackDoesNotDriveBodyResponse()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.BodyPlayer.SampleExact(
                    "Attack.Right",
                    0.5f);
                Quaternion bodyBefore =
                    fixture.Avatar.ResolveBodyPartWorld("Body").rotation;
                float bodyPhase =
                    fixture.BodyPlayer.CurrentNormalizedPhase;

                fixture.EquipmentPlayer.SampleExact(
                    "Attack",
                    0.75f);
                Quaternion bodyAfter =
                    fixture.Avatar.ResolveBodyPartWorld("Body").rotation;

                Assert.That(
                    Quaternion.Angle(bodyBefore, bodyAfter),
                    Is.LessThan(0.0001f));
                Assert.That(
                    fixture.BodyPlayer.CurrentNormalizedPhase,
                    Is.EqualTo(bodyPhase).Within(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.Equipment.Model.Instance
                            .GetPartWorldMatrix(0)
                            .rotation),
                    Is.GreaterThan(20f));
                AssertMountBound(fixture);
                yield return null;
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator DifferentAuthoredDurationsKeepIndependentPhases()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.BodyPlayer.SampleExact(
                    "Attack.Right",
                    0.25f);
                fixture.EquipmentPlayer.SampleExact(
                    "Attack",
                    0.80f);
                Assert.That(
                    fixture.BodyPlayer.CurrentNormalizedPhase,
                    Is.EqualTo(0.25f).Within(0.0001f));
                Assert.That(
                    fixture.EquipmentPlayer.CurrentNormalizedPhase,
                    Is.EqualTo(0.80f).Within(0.0001f));
                AssertMountBound(fixture);
                yield return null;
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator BodyAndEquipmentResetOnlyWhenExplicitlyCommanded()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.BodyPlayer.SampleExact(
                    "Death",
                    0.65f);
                fixture.EquipmentPlayer.SampleExact(
                    "Attack",
                    0.65f);
                Quaternion bodyDeath =
                    fixture.Avatar
                        .ResolveBodyPartWorld("Body")
                        .rotation;
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        bodyDeath),
                    Is.GreaterThan(10f));

                fixture.EquipmentPlayer.ResetMotion();
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.Avatar
                            .ResolveBodyPartWorld("Body")
                            .rotation),
                    Is.GreaterThan(10f));
                Assert.That(
                    fixture.EquipmentPlayer.CurrentClipName,
                    Is.EqualTo(string.Empty));
                Assert.IsFalse(
                    fixture.Equipment.Model.AnimationTrack.HasSelection);
                AssertMountBound(fixture);

                fixture.BodyPlayer.ResetMotion();
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.Avatar
                            .ResolveBodyPartWorld("Body")
                            .rotation),
                    Is.LessThan(0.0001f));
                yield return null;
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator DisablingWeaponPlayerDoesNotResetBodyTrack()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.BodyPlayer.SampleExact(
                    "Attack.Right",
                    0.55f);
                Quaternion bodyBefore =
                    fixture.Avatar
                        .ResolveBodyPartWorld("Body")
                        .rotation;
                float bodyPhase =
                    fixture.BodyPlayer.CurrentNormalizedPhase;

                fixture.EquipmentPlayer.PlayExact(
                    "Attack",
                    false,
                    1f);
                yield return new WaitForSeconds(0.12f);
                fixture.EquipmentPlayer.enabled = false;
                yield return null;

                Assert.That(
                    Quaternion.Angle(
                        bodyBefore,
                        fixture.Avatar
                            .ResolveBodyPartWorld("Body")
                            .rotation),
                    Is.LessThan(0.0001f));
                Assert.That(
                    fixture.BodyPlayer.CurrentNormalizedPhase,
                    Is.EqualTo(bodyPhase).Within(0.0001f));
                AssertMountBound(fixture);
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator LoopOverrideIsLocalToOneModelTrack()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.EquipmentPlayer.SampleExact(
                    "Idle",
                    0.7f);
                float equipmentPhase =
                    fixture.EquipmentPlayer.CurrentNormalizedPhase;

                fixture.BodyPlayer.PlayExact(
                    "Attack.Right",
                    shouldLoop: true,
                    playbackSpeed: 1f);
                float attackDuration =
                    fixture.BodyPlayer.CurrentDuration;
                Assert.That(
                    fixture.BodyModel.EnsureAnimationTrack()
                        .Advance(attackDuration),
                    Is.EqualTo(0f).Within(0.0001f));
                Assert.IsTrue(
                    fixture.BodyModel.AnimationTrack.IsPlaying);
                Assert.That(
                    fixture.EquipmentPlayer.CurrentNormalizedPhase,
                    Is.EqualTo(equipmentPhase).Within(0.0001f));

                fixture.BodyPlayer.PlayExact(
                    "Idle",
                    shouldLoop: false,
                    playbackSpeed: 1f);
                float idleDuration =
                    fixture.BodyPlayer.CurrentDuration;
                Assert.That(
                    fixture.BodyModel.EnsureAnimationTrack()
                        .Advance(idleDuration * 1.5f),
                    Is.EqualTo(1f).Within(0.0001f));
                Assert.IsFalse(
                    fixture.BodyModel.AnimationTrack.IsPlaying);
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

            fixture.BodyPlayer =
                fixture.BodyModel.gameObject
                    .AddComponent<VoxelModelMotionPlayer>();
            fixture.BodyPlayer.Configure(
                fixture.BodyModel,
                fixture.Avatar,
                "Idle",
                false,
                1f);
            fixture.EquipmentPlayer =
                fixture.Equipment.Model.gameObject
                    .AddComponent<VoxelModelMotionPlayer>();
            fixture.EquipmentPlayer.Configure(
                fixture.Equipment.Model,
                fixture.Avatar,
                "Idle",
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
            public VoxelModelMotionPlayer BodyPlayer;
            public VoxelModelMotionPlayer EquipmentPlayer;
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
