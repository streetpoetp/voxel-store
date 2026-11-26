using System;
using Hero.Voxel.Render.Runtime.Unity;
using Hero.Voxel.Runtime.Animation;
using Hero.Voxel.Runtime.Assets;
using NUnit.Framework;
using UnityEngine;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor.Tests
{
    public sealed class AvatarPresentationTests
    {
        [Test]
        public void CanonicalModelFrameIsSharedByAvatarAndEquipment()
        {
            Assert.AreEqual(
                VoxelStoreModelFrame.Right,
                AvatarPresentation.CanonicalLocalRight);
            Assert.AreEqual(
                VoxelStoreModelFrame.Up,
                AvatarPresentation.CanonicalLocalUp);
            Assert.AreEqual(
                VoxelStoreModelFrame.Forward,
                AvatarPresentation.CanonicalLocalForward);
            Assert.AreEqual(
                AvatarPresentation.CanonicalLocalForward,
                EquipmentPresentation.CanonicalLocalForward);
            Assert.AreEqual(Vector3.back, AvatarPresentation.CanonicalLocalForward);
        }

        [Test]
        public void DistinctBindingsCanReuseSameEquipmentAsset()
        {
            using Fixture fixture = CreateFixture(includeSecondEquipment: true);

            Assert.AreSame(
                fixture.Primary.Model.Assembly,
                fixture.Secondary.Model.Assembly);
            Assert.AreNotSame(
                fixture.Primary.Model.Instance,
                fixture.Secondary.Model.Instance);
            Assert.AreNotEqual(
                fixture.Primary.BindingId,
                fixture.Secondary.BindingId);

            Vector3 primary = SocketPosition(
                fixture.Primary.Model.Instance,
                "Mount");
            Vector3 secondary = SocketPosition(
                fixture.Secondary.Model.Instance,
                "Mount");
            Assert.That(
                Vector3.Distance(primary, secondary),
                Is.GreaterThan(0.1f));
            Assert.AreEqual(3, fixture.World.InstanceCount);
        }

        [Test]
        public void BodyAndWeaponPlaybackAreIndependent()
        {
            using Fixture fixture = CreateFixture();

            fixture.BodyPlayer.SampleExact(
                "Attack.Left",
                0.5f);
            fixture.PrimaryPlayer.SampleExact(
                "Idle",
                0.25f);
            Quaternion bodyBeforeWeaponAttack =
                fixture.Avatar.ResolveBodyPartWorld("Body").rotation;
            float bodyPhase =
                fixture.BodyPlayer.CurrentNormalizedPhase;

            fixture.PrimaryPlayer.SampleExact(
                "Attack",
                0.75f);
            Quaternion bodyAfterWeaponAttack =
                fixture.Avatar.ResolveBodyPartWorld("Body").rotation;
            Quaternion weapon =
                fixture.Primary.Model.Instance
                    .GetPartWorldMatrix(0)
                    .rotation;

            Assert.That(
                Quaternion.Angle(
                    bodyBeforeWeaponAttack,
                    bodyAfterWeaponAttack),
                Is.LessThan(0.0001f));
            Assert.That(
                fixture.BodyPlayer.CurrentNormalizedPhase,
                Is.EqualTo(bodyPhase).Within(0.0001f));
            Assert.That(
                Quaternion.Angle(Quaternion.identity, weapon),
                Is.GreaterThan(20f));
            AssertMountBound(fixture, fixture.Primary);
        }

        [Test]
        public void DifferentAuthoredDurationsUseIndependentNormalizedPhases()
        {
            using Fixture fixture = CreateFixture();

            Assert.That(
                fixture.BodyAnimation.GetClip(
                    RequireClip(fixture.BodyAnimation, "Attack.Right")).Duration,
                Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(
                fixture.EquipmentAnimation.GetClip(
                    RequireClip(fixture.EquipmentAnimation, "Attack")).Duration,
                Is.EqualTo(1.2f).Within(0.0001f));

            fixture.BodyPlayer.SampleExact(
                "Attack.Right",
                0.25f);
            fixture.PrimaryPlayer.SampleExact(
                "Attack",
                0.75f);

            Assert.That(
                fixture.BodyPlayer.CurrentNormalizedPhase,
                Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(
                fixture.PrimaryPlayer.CurrentNormalizedPhase,
                Is.EqualTo(0.75f).Within(0.0001f));
            AssertMountBound(fixture, fixture.Primary);
        }

        [Test]
        public void WeaponAttackUsesOwnAuthoredClipAndBindAnchor()
        {
            using Fixture fixture = CreateFixture();

            fixture.BodyPlayer.SampleExact(
                "Idle",
                0.5f);
            fixture.PrimaryPlayer.SampleExact(
                "Attack",
                0.5f);

            Quaternion weapon =
                fixture.Primary.Model.Instance
                    .GetPartWorldMatrix(0)
                    .rotation;
            Assert.That(
                Quaternion.Angle(
                    Quaternion.identity,
                    weapon),
                Is.GreaterThan(20f));
            AssertMountBound(fixture, fixture.Primary);
        }

        [Test]
        public void RepeatedSampleResetDoesNotDriftProviderBinding()
        {
            using Fixture fixture = CreateFixture();
            Vector3 neutral =
                SocketPosition(
                    fixture.Primary.Model.Instance,
                    "Mount");

            for (int i = 0; i < 3; i++)
            {
                fixture.BodyPlayer.SampleExact(
                    "Attack.Right",
                    0.5f);
                fixture.PrimaryPlayer.SampleExact(
                    "Attack",
                    0.7f);
                fixture.BodyPlayer.ResetMotion();
                fixture.PrimaryPlayer.ResetMotion();
                fixture.Avatar.RefreshAttachmentReferences();

                Assert.That(
                    Vector3.Distance(
                        neutral,
                        SocketPosition(
                            fixture.Primary.Model.Instance,
                            "Mount")),
                    Is.LessThan(0.0001f));
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
                        fixture.Primary.Model.Instance
                            .GetPartWorldMatrix(0)
                            .rotation),
                    Is.LessThan(0.0001f));
            }
        }

        [Test]
        public void MissingWeaponDeathFailsOnlyWeaponTrack()
        {
            using Fixture fixture =
                CreateFixture(omitEquipmentDeath: true);

            fixture.BodyPlayer.SampleExact(
                "Death",
                0.5f);
            Assert.Throws<InvalidOperationException>(() =>
                fixture.PrimaryPlayer.SampleExact(
                    "Death",
                    0.5f));
            Assert.AreEqual(
                "Death",
                fixture.BodyPlayer.CurrentClipName);
            Assert.That(
                fixture.BodyPlayer.CurrentNormalizedPhase,
                Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void ProviderBoundsNeedNoSceneMeshRendererHierarchy()
        {
            using Fixture fixture = CreateFixture(includeSecondEquipment: true);

            Bounds bounds = fixture.Avatar.GetWorldBounds();
            Assert.That(bounds.size.sqrMagnitude, Is.GreaterThan(0f));
            Assert.AreEqual(
                0,
                fixture.Unit.GetComponentsInChildren<MeshRenderer>(true).Length);
            Assert.AreEqual(3, fixture.World.InstanceCount);
        }

        [Test]
        public void ReleaseAndReinitializeOwnsProviderLifecycle()
        {
            using Fixture fixture = CreateFixture(includeSecondEquipment: true);
            Assert.AreEqual(3, fixture.World.InstanceCount);

            fixture.Avatar.ReleaseProvider();
            Assert.AreEqual(0, fixture.World.InstanceCount);
            Assert.IsFalse(fixture.Avatar.IsProviderInitialized);

            fixture.Avatar.InitializeProvider();
            Assert.AreEqual(3, fixture.World.InstanceCount);
            Assert.IsTrue(fixture.Avatar.IsProviderInitialized);
        }

        [Test]
        public void LoopPolicyIsModelLocal()
        {
            using Fixture fixture = CreateFixture();

            fixture.BodyPlayer.PlayExact(
                "Attack.Right",
                shouldLoop: true,
                playbackSpeed: 1f);
            fixture.PrimaryPlayer.SampleExact(
                "Idle",
                0.6f);
            float equipmentPhase =
                fixture.PrimaryPlayer.CurrentNormalizedPhase;
            float attackDuration =
                fixture.BodyPlayer.CurrentDuration;
            Assert.That(
                fixture.BodyModel
                    .EnsureAnimationTrack()
                    .Advance(attackDuration),
                Is.EqualTo(0f).Within(0.0001f));
            Assert.IsTrue(
                fixture.BodyModel.AnimationTrack.IsPlaying);
            Assert.That(
                fixture.PrimaryPlayer.CurrentNormalizedPhase,
                Is.EqualTo(equipmentPhase).Within(0.0001f));

            fixture.BodyPlayer.PlayExact(
                "Idle",
                shouldLoop: false,
                playbackSpeed: 1f);
            float idleDuration =
                fixture.BodyPlayer.CurrentDuration;
            Assert.That(
                fixture.BodyModel
                    .EnsureAnimationTrack()
                    .Advance(idleDuration * 1.5f),
                Is.EqualTo(1f).Within(0.0001f));
            Assert.IsFalse(
                fixture.BodyModel.AnimationTrack.IsPlaying);
        }

        [Test]
        public void AvatarWithoutEquipmentUsesRootOnlyProviderActor()
        {
            using Fixture fixture =
                CreateFixture(includePrimaryEquipment: false);

            Assert.AreEqual(0, fixture.Avatar.Equipment.Count);
            Assert.AreEqual(1, fixture.World.InstanceCount);

            fixture.BodyPlayer.SampleExact(
                "Idle",
                0.5f);

            Assert.IsTrue(fixture.Avatar.IsProviderInitialized);
            Assert.AreEqual(1, fixture.World.InstanceCount);
        }

        [Test]
        public void WeaponClipChangeDoesNotChangeBodyPhaseOrPose()
        {
            using Fixture fixture = CreateFixture();

            fixture.BodyPlayer.SampleExact(
                "Attack.Right",
                0.5f);
            Quaternion bodyBefore =
                fixture.Avatar.ResolveBodyPartWorld("Body").rotation;
            float bodyPhase =
                fixture.BodyPlayer.CurrentNormalizedPhase;

            fixture.PrimaryPlayer.SampleExact(
                "Attack",
                0.2f);
            fixture.PrimaryPlayer.SampleExact(
                "Hit",
                0.8f);

            Assert.That(
                fixture.BodyPlayer.CurrentNormalizedPhase,
                Is.EqualTo(bodyPhase).Within(0.0001f));
            Assert.That(
                Quaternion.Angle(
                    bodyBefore,
                    fixture.Avatar
                        .ResolveBodyPartWorld("Body")
                        .rotation),
                Is.LessThan(0.0001f));
            AssertMountBound(fixture, fixture.Primary);
        }

        [Test]
        public void ProviderResetClearsRuntimeStateButPreservesPreviewIntent()
        {
            using Fixture fixture = CreateFixture();

            fixture.PrimaryPlayer.SetPlaybackSettings(
                "Attack",
                shouldLoop: false,
                playbackSpeed: 2f);
            fixture.PrimaryPlayer.SampleExact(
                "Attack",
                0.4f);

            fixture.Primary.Model
                .EnsureAnimationTrack()
                .Reset();

            Assert.AreEqual("Attack", fixture.PrimaryPlayer.ClipName);
            Assert.IsFalse(fixture.PrimaryPlayer.Loop);
            Assert.That(
                fixture.PrimaryPlayer.Speed,
                Is.EqualTo(2f).Within(0.0001f));
            Assert.AreEqual(
                string.Empty,
                fixture.PrimaryPlayer.CurrentClipName);
            Assert.That(
                fixture.PrimaryPlayer.CurrentNormalizedPhase,
                Is.EqualTo(0f).Within(0.0001f));
        }

        private static Fixture CreateFixture(
            bool includeSecondEquipment = false,
            bool omitEquipmentDeath = false,
            bool includePrimaryEquipment = true)
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
                    fixture.EquipmentAssembly,
                    omitEquipmentDeath);

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

            EquipmentPresentation[] equipment;
            if (!includePrimaryEquipment)
            {
                equipment = Array.Empty<EquipmentPresentation>();
            }
            else
            {
                fixture.Primary = CreateEquipment(
                    presentation.transform,
                    fixture,
                    "Equip.Hand.Right.0",
                    EquipmentSlotSide.Right,
                    "Primary");

                if (includeSecondEquipment)
                {
                    fixture.Secondary = CreateEquipment(
                        presentation.transform,
                        fixture,
                        "Equip.Hand.Left.0",
                        EquipmentSlotSide.Left,
                        "Secondary");
                    equipment =
                        new[] { fixture.Primary, fixture.Secondary };
                }
                else
                {
                    equipment = new[] { fixture.Primary };
                }
            }

            fixture.Avatar =
                fixture.Unit.AddComponent<AvatarPresentation>();
            fixture.Avatar.Configure(
                fixture.Unit.transform,
                presentation.transform,
                fixture.BodyModel,
                equipment);

            fixture.BodyPlayer =
                fixture.BodyModel.gameObject
                    .AddComponent<VoxelModelMotionPlayer>();
            fixture.BodyPlayer.Configure(
                fixture.BodyModel,
                fixture.Avatar,
                "Idle",
                false,
                1f);

            if (fixture.Primary != null)
            {
                fixture.PrimaryPlayer =
                    fixture.Primary.Model.gameObject
                        .AddComponent<VoxelModelMotionPlayer>();
                fixture.PrimaryPlayer.Configure(
                    fixture.Primary.Model,
                    fixture.Avatar,
                    "Idle",
                    false,
                    1f);
            }

            if (fixture.Secondary != null)
            {
                fixture.SecondaryPlayer =
                    fixture.Secondary.Model.gameObject
                        .AddComponent<VoxelModelMotionPlayer>();
                fixture.SecondaryPlayer.Configure(
                    fixture.Secondary.Model,
                    fixture.Avatar,
                    "Idle",
                    false,
                    1f);
            }

            return fixture;
        }

        private static EquipmentPresentation CreateEquipment(
            Transform presentationRoot,
            Fixture fixture,
            string socketName,
            EquipmentSlotSide side,
            string name)
        {
            var placement =
                new GameObject(name + " Equipment");
            placement.transform.SetParent(
                presentationRoot,
                false);

            VoxelRenderPresentation model =
                placement.AddComponent<VoxelRenderPresentation>();
            model.Configure(
                fixture.World,
                placement.transform,
                fixture.EquipmentAssembly,
                fixture.EquipmentAnimation,
                externalLifecycle: true);

            EquipmentPresentation equipment =
                placement.AddComponent<EquipmentPresentation>();
            equipment.Configure(
                model,
                socketName,
                side);
            return equipment;
        }

        private static void AssertMountBound(
            Fixture fixture,
            EquipmentPresentation equipment)
        {
            Vector3 bodySocket =
                fixture.Avatar
                    .ResolveBodySocketWorld(equipment.SocketName)
                    .MultiplyPoint3x4(Vector3.zero);
            Vector3 mount =
                SocketPosition(
                    equipment.Model.Instance,
                    "Mount");
            Assert.That(
                Vector3.Distance(bodySocket, mount),
                Is.LessThan(0.0001f));
        }

        private static Vector3 SocketPosition(
            VoxelRenderInstance instance,
            string socketName)
        {
            int index = instance.RequireSocket(socketName);
            return instance
                .GetSocketWorldMatrix(index)
                .MultiplyPoint3x4(Vector3.zero);
        }

        private static int RequireClip(
            VoxelAnimationAsset animation,
            string name)
        {
            Assert.IsTrue(animation.TryGetClipIndex(name, out int index));
            return index;
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
            VoxelAssemblyAsset assembly,
            bool omitDeath)
        {
            var clips = new System.Collections.Generic.List<VoxelAnimationClip>
            {
                RotationClip("Idle", true, Quaternion.identity, 1.1f),
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
                    0.8f)
            };
            if (!omitDeath)
            {
                clips.Add(
                    RotationClip(
                        "Death",
                        false,
                        Quaternion.Euler(0f, 0f, 82f),
                        1.4f));
            }

            return VoxelAnimationAsset.Create(
                assembly,
                clips.ToArray());
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
            public VoxelModelMotionPlayer PrimaryPlayer;
            public VoxelModelMotionPlayer SecondaryPlayer;
            public EquipmentPresentation Primary;
            public EquipmentPresentation Secondary;

            public void Dispose()
            {
                Avatar?.ReleaseProvider();
                if (WorldRoot != null)
                    UnityEngine.Object.DestroyImmediate(WorldRoot);
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
