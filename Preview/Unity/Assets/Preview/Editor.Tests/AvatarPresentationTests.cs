using System;
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
            string[] names = Enum.GetNames(typeof(AvatarMotion));

            CollectionAssert.AreEqual(
                new[]
                {
                    "Idle",
                    "WalkForward",
                    "Attack",
                    "Hit",
                    "Death"
                },
                names);
        }

        [Test]
        public void CanonicalDirectionUsesVisibleFront()
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
        }

        [Test]
        public void PresentationConsumesPreviewModelFrame()
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
                VoxelStoreModelFrame.Forward,
                EquipmentPresentation.CanonicalLocalForward);

            Assert.AreEqual(
                VoxelStoreModelFrame.Forward,
                VoxelStoreModelFrame.SourceToUnityVector(Vector3.down));
            Assert.AreEqual(
                Vector3.forward,
                VoxelStoreModelFrame.SourceToUnityVector(Vector3.up));
        }

        [Test]
        public void AvatarAndEquipmentShareCanonicalModelAxes()
        {
            Assert.AreEqual(
                AvatarPresentation.CanonicalLocalRight,
                EquipmentPresentation.CanonicalLocalRight);
            Assert.AreEqual(
                AvatarPresentation.CanonicalLocalUp,
                EquipmentPresentation.CanonicalLocalUp);
            Assert.AreEqual(
                AvatarPresentation.CanonicalLocalForward,
                EquipmentPresentation.CanonicalLocalForward);
            Assert.AreEqual(
                Vector3.right,
                EquipmentPresentation.CanonicalLocalRight);
            Assert.AreEqual(
                Vector3.up,
                EquipmentPresentation.CanonicalLocalUp);
            Assert.AreEqual(
                Vector3.back,
                EquipmentPresentation.CanonicalLocalForward);
        }

        [Test]
        public void BodyRatioPlacementMapsSourceAxesToUnity()
        {
            var bounds = new Bounds(
                new Vector3(1f, 2f, 3f),
                new Vector3(4f, 6f, 8f));

            Vector3 resolved =
                AvatarPresentation.ResolveBodyRatioPosition(
                    bounds,
                    new Vector3(0.75f, 0f, 0.25f));

            Vector3 expected =
                bounds.center +
                Vector3.Scale(
                    bounds.extents,
                    new Vector3(0.75f, 0.25f, 0f));

            Assert.That(
                Vector3.Distance(
                    expected,
                    resolved),
                Is.LessThan(0.0001f));
        }

        [Test]
        public void EquipmentMotionResetDoesNotChangePlacement()
        {
            GameObject unit = null;
            try
            {
                AvatarPresentation avatar =
                    CreateAvatar(
                        out unit,
                        out EquipmentPresentation equipment);

                Vector3 placement =
                    equipment.PlacementRoot.localPosition;
                Quaternion placementRotation =
                    equipment.PlacementRoot.localRotation;

                equipment.MotionRoot.localPosition =
                    new Vector3(0.2f, 0.3f, -0.4f);
                equipment.MotionRoot.localRotation =
                    Quaternion.Euler(20f, 30f, 40f);

                equipment.ResetMotion();

                Assert.That(
                    Vector3.Distance(
                        placement,
                        equipment.PlacementRoot.localPosition),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        placementRotation,
                        equipment.PlacementRoot.localRotation),
                    Is.LessThan(0.0001f));
                Assert.That(
                    equipment.MotionRoot.localPosition.sqrMagnitude,
                    Is.LessThan(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        equipment.MotionRoot.localRotation),
                    Is.LessThan(0.0001f));

                Assert.AreEqual(
                    1,
                    avatar.Equipment.Count);
            }
            finally
            {
                if (unit != null)
                    UnityEngine.Object.DestroyImmediate(unit);
            }
        }

        [Test]
        public void IdleRepeatedEntryExitDoesNotDriftPlacement()
        {
            GameObject unit = null;
            try
            {
                AvatarPresentation avatar =
                    CreateAvatar(
                        out unit,
                        out EquipmentPresentation equipment);
                AvatarMotionPlayer player =
                    unit.AddComponent<AvatarMotionPlayer>();
                player.Configure(
                    avatar,
                    AvatarMotion.Idle,
                    false,
                    1f);

                Vector3 placementPosition =
                    equipment.PlacementRoot.localPosition;
                Quaternion placementRotation =
                    equipment.PlacementRoot.localRotation;

                for (int i = 0; i < 3; i++)
                {
                    player.SampleMotion(
                        AvatarMotion.Idle,
                        0.5f);
                    Assert.That(
                        Vector3.Distance(
                            placementPosition,
                            equipment.PlacementRoot.localPosition),
                        Is.LessThan(0.0001f));
                    Assert.That(
                        Quaternion.Angle(
                            placementRotation,
                            equipment.PlacementRoot.localRotation),
                        Is.LessThan(0.0001f));

                    player.ResetMotion();
                    AssertNeutral(
                        avatar,
                        equipment);
                    Assert.That(
                        Vector3.Distance(
                            placementPosition,
                            equipment.PlacementRoot.localPosition),
                        Is.LessThan(0.0001f));
                    Assert.That(
                        Quaternion.Angle(
                            placementRotation,
                            equipment.PlacementRoot.localRotation),
                        Is.LessThan(0.0001f));
                }
            }
            finally
            {
                if (unit != null)
                    UnityEngine.Object.DestroyImmediate(unit);
            }
        }

        [Test]
        public void MultipleEquipmentUseIndependentPlacementRoots()
        {
            GameObject unit = null;
            try
            {
                unit = new GameObject("UnitRoot");
                var presentation =
                    new GameObject("PresentationRoot");
                presentation.transform.SetParent(
                    unit.transform,
                    false);

                GameObject bodyObject =
                    GameObject.CreatePrimitive(
                        PrimitiveType.Cube);
                bodyObject.name = "Body";
                bodyObject.transform.SetParent(
                    presentation.transform,
                    false);
                MeshFilter bodyMesh =
                    bodyObject.GetComponent<MeshFilter>();

                EquipmentPresentation left =
                    CreateEquipment(
                        presentation.transform,
                        new Vector3(-0.75f, 0f, 0.2f),
                        "Left");
                EquipmentPresentation right =
                    CreateEquipment(
                        presentation.transform,
                        new Vector3(0.75f, 0f, 0.2f),
                        "Right");

                AvatarPresentation avatar =
                    unit.AddComponent<AvatarPresentation>();
                avatar.Configure(
                    unit.transform,
                    presentation.transform,
                    bodyMesh,
                    new[] { left, right });

                Assert.AreEqual(
                    2,
                    avatar.Equipment.Count);
                Assert.AreNotEqual(
                    left.PlacementRoot.localPosition,
                    right.PlacementRoot.localPosition);
                Assert.AreSame(
                    presentation.transform,
                    left.PlacementRoot.parent);
                Assert.AreSame(
                    presentation.transform,
                    right.PlacementRoot.parent);
                Assert.AreSame(
                    left.PlacementRoot,
                    left.MotionRoot.parent);
                Assert.AreSame(
                    right.PlacementRoot,
                    right.MotionRoot.parent);
            }
            finally
            {
                if (unit != null)
                    UnityEngine.Object.DestroyImmediate(unit);
            }
        }

        [Test]
        public void AttackSampleMovesForwardWhileEquipmentKeepsSemanticForward()
        {
            GameObject unit = null;
            try
            {
                AvatarPresentation avatar =
                    CreateAvatar(
                        out unit,
                        out EquipmentPresentation equipment);
                AvatarMotionPlayer player =
                    unit.AddComponent<AvatarMotionPlayer>();
                player.Configure(
                    avatar,
                    AvatarMotion.Attack,
                    false,
                    1f);

                Vector3 unitPosition =
                    unit.transform.position;
                Quaternion unitRotation =
                    unit.transform.rotation;
                Vector3 placementPosition =
                    equipment.PlacementRoot.localPosition;
                Quaternion placementRotation =
                    equipment.PlacementRoot.localRotation;

                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        equipment.MotionRoot.localRotation),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Vector3.Dot(
                        equipment.MotionRoot
                            .TransformDirection(equipment.LocalForward)
                            .normalized,
                        avatar.WorldForward),
                    Is.GreaterThan(0.9999f));

                player.SampleMotion(
                    AvatarMotion.Attack,
                    0.50f);

                float forwardDistance =
                    Vector3.Dot(
                        avatar.PresentationRoot.localPosition,
                        avatar.LocalForward);

                Assert.That(
                    forwardDistance,
                    Is.GreaterThan(0.12f));
                Assert.That(
                    equipment.MotionRoot.localPosition.magnitude,
                    Is.GreaterThan(0.03f));
                Assert.That(
                    Vector3.Dot(
                        equipment.MotionRoot.localPosition,
                        equipment.LocalForward),
                    Is.GreaterThan(0.03f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        avatar.PresentationRoot.localRotation),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        equipment.MotionRoot.localRotation),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Vector3.Dot(
                        equipment.MotionRoot
                            .TransformDirection(equipment.LocalForward)
                            .normalized,
                        avatar.WorldForward),
                    Is.GreaterThan(0.9999f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        equipment.PlacementRoot.localRotation),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Vector3.Distance(
                        placementPosition,
                        equipment.PlacementRoot.localPosition),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        placementRotation,
                        equipment.PlacementRoot.localRotation),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Vector3.Distance(
                        unitPosition,
                        unit.transform.position),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        unitRotation,
                        unit.transform.rotation),
                    Is.LessThan(0.0001f));

                player.ResetMotion();
                AssertNeutral(
                    avatar,
                    equipment);
            }
            finally
            {
                if (unit != null)
                    UnityEngine.Object.DestroyImmediate(unit);
            }
        }

        [Test]
        public void WalkHitAndDeathSamplesRespectSemanticAxes()
        {
            GameObject unit = null;
            try
            {
                AvatarPresentation avatar =
                    CreateAvatar(
                        out unit,
                        out EquipmentPresentation equipment);
                AvatarMotionPlayer player =
                    unit.AddComponent<AvatarMotionPlayer>();
                player.Configure(
                    avatar,
                    AvatarMotion.Idle,
                    false,
                    1f);

                player.SampleMotion(
                    AvatarMotion.WalkForward,
                    0.45f);
                Assert.That(
                    avatar.PresentationRoot.localPosition.y,
                    Is.GreaterThan(0.10f));
                Assert.That(
                    Vector3.Dot(
                        avatar.PresentationRoot.localPosition,
                        avatar.LocalForward),
                    Is.GreaterThan(0.05f));
                Assert.That(
                    equipment.MotionRoot.localPosition.magnitude,
                    Is.GreaterThan(0.01f));

                player.ResetMotion();

                player.SampleMotion(
                    AvatarMotion.Hit,
                    0.25f);
                Assert.That(
                    Vector3.Dot(
                        avatar.PresentationRoot.localPosition,
                        avatar.LocalForward),
                    Is.LessThan(-0.05f));

                player.ResetMotion();

                player.SampleMotion(
                    AvatarMotion.Death,
                    0.88f);
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        avatar.PresentationRoot.localRotation),
                    Is.GreaterThan(30f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        equipment.MotionRoot.localRotation),
                    Is.GreaterThan(15f));

                player.ResetMotion();
                AssertNeutral(
                    avatar,
                    equipment);
            }
            finally
            {
                if (unit != null)
                    UnityEngine.Object.DestroyImmediate(unit);
            }
        }

        private static void AssertNeutral(
            AvatarPresentation avatar,
            EquipmentPresentation equipment)
        {
            Assert.That(
                avatar.PresentationRoot.localPosition.sqrMagnitude,
                Is.LessThan(0.0001f));
            Assert.That(
                Quaternion.Angle(
                    Quaternion.identity,
                    avatar.PresentationRoot.localRotation),
                Is.LessThan(0.0001f));
            Assert.That(
                Vector3.Distance(
                    Vector3.one,
                    avatar.PresentationRoot.localScale),
                Is.LessThan(0.0001f));
            Assert.That(
                equipment.MotionRoot.localPosition.sqrMagnitude,
                Is.LessThan(0.0001f));
            Assert.That(
                Quaternion.Angle(
                    Quaternion.identity,
                    equipment.MotionRoot.localRotation),
                Is.LessThan(0.0001f));
        }

        private static AvatarPresentation CreateAvatar(
            out GameObject unit,
            out EquipmentPresentation equipment)
        {
            unit = new GameObject("UnitRoot");
            var presentation =
                new GameObject("PresentationRoot");
            presentation.transform.SetParent(
                unit.transform,
                false);

            GameObject bodyObject =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube);
            bodyObject.name = "Body";
            bodyObject.transform.SetParent(
                presentation.transform,
                false);
            MeshFilter bodyMesh =
                bodyObject.GetComponent<MeshFilter>();

            equipment = CreateEquipment(
                presentation.transform,
                new Vector3(0.75f, 0f, 0.2f),
                "Equipment");

            AvatarPresentation avatar =
                unit.AddComponent<AvatarPresentation>();
            avatar.Configure(
                unit.transform,
                presentation.transform,
                bodyMesh,
                new[] { equipment });
            return avatar;
        }

        private static EquipmentPresentation CreateEquipment(
            Transform presentationRoot,
            Vector3 ratio,
            string name)
        {
            var placement =
                new GameObject(name + " Placement");
            placement.transform.SetParent(
                presentationRoot,
                false);

            var motion =
                new GameObject("MotionRoot");
            motion.transform.SetParent(
                placement.transform,
                false);

            EquipmentPresentation equipment =
                placement.AddComponent<EquipmentPresentation>();
            equipment.Configure(
                placement.transform,
                motion.transform,
                ratio);
            return equipment;
        }
    }
}
