using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.PlayMode.Tests
{
    public sealed class RuntimeGalleryPlayModeTests
    {
        [UnityTest]
        public IEnumerator AttackMovesForwardAndKeepsEquipmentForward()
        {
            Fixture fixture = CreateFixture();
            try
            {
                Vector3 unitPosition =
                    fixture.Unit.transform.position;
                Quaternion unitRotation =
                    fixture.Unit.transform.rotation;

                fixture.Player.Play(
                    AvatarMotion.Attack,
                    false,
                    1f);

                float maxForward = 0f;
                float minForward = 0f;
                float maxEquipmentMotion = 0f;
                float minEquipmentForwardAlignment = 1f;
                for (int i = 0; i < 30; i++)
                {
                    yield return new WaitForSeconds(0.02f);

                    float forward =
                        Vector3.Dot(
                            fixture.Presentation.localPosition,
                            fixture.Avatar.LocalForward);
                    maxForward =
                        Mathf.Max(maxForward, forward);
                    minForward =
                        Mathf.Min(minForward, forward);

                    maxEquipmentMotion = Mathf.Max(
                        maxEquipmentMotion,
                        fixture.Equipment.MotionRoot.localPosition.magnitude);

                    Vector3 equipmentForward =
                        fixture.Equipment.MotionRoot
                            .TransformDirection(
                                fixture.Equipment.LocalForward)
                            .normalized;
                    minEquipmentForwardAlignment = Mathf.Min(
                        minEquipmentForwardAlignment,
                        Vector3.Dot(
                            equipmentForward,
                            fixture.Avatar.WorldForward));
                }

                Assert.That(maxForward, Is.GreaterThan(0.15f));
                Assert.That(minForward, Is.LessThan(-0.03f));
                Assert.That(
                    maxEquipmentMotion,
                    Is.GreaterThan(0.05f));
                Assert.That(
                    minEquipmentForwardAlignment,
                    Is.GreaterThan(0.999f));
                AssertUnitRootUnchanged(
                    fixture,
                    unitPosition,
                    unitRotation);

                fixture.Player.Stop();
                AssertNeutral(fixture);
            }
            finally
            {
                Object.Destroy(fixture.Unit);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator WalkForwardReadsAsHopWithoutOwningUnitRoot()
        {
            Fixture fixture = CreateFixture();
            try
            {
                Vector3 unitPosition =
                    fixture.Unit.transform.position;
                Quaternion unitRotation =
                    fixture.Unit.transform.rotation;

                fixture.Player.Play(
                    AvatarMotion.WalkForward,
                    false,
                    1f);

                float maxHeight = 0f;
                float maxForward = 0f;
                float maxEquipmentMotion = 0f;
                for (int i = 0; i < 30; i++)
                {
                    yield return new WaitForSeconds(0.02f);

                    maxHeight = Mathf.Max(
                        maxHeight,
                        fixture.Presentation.localPosition.y);
                    maxForward = Mathf.Max(
                        maxForward,
                        Vector3.Dot(
                            fixture.Presentation.localPosition,
                            fixture.Avatar.LocalForward));
                    maxEquipmentMotion = Mathf.Max(
                        maxEquipmentMotion,
                        fixture.Equipment.MotionRoot.localPosition.magnitude);
                }

                Assert.That(maxHeight, Is.GreaterThan(0.15f));
                Assert.That(maxForward, Is.GreaterThan(0.10f));
                Assert.That(
                    maxEquipmentMotion,
                    Is.GreaterThan(0.02f));
                AssertUnitRootUnchanged(
                    fixture,
                    unitPosition,
                    unitRotation);

                fixture.Player.Stop();
                AssertNeutral(fixture);
            }
            finally
            {
                Object.Destroy(fixture.Unit);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator HitRecoilsBackwardAndDeathIsTerminal()
        {
            Fixture fixture = CreateFixture();
            try
            {
                fixture.Player.Play(
                    AvatarMotion.Hit,
                    false,
                    1f);

                float minForward = 0f;
                for (int i = 0; i < 18; i++)
                {
                    yield return new WaitForSeconds(0.02f);
                    minForward = Mathf.Min(
                        minForward,
                        Vector3.Dot(
                            fixture.Presentation.localPosition,
                            fixture.Avatar.LocalForward));
                }

                Assert.That(
                    minForward,
                    Is.LessThan(-0.10f));

                fixture.Player.Stop();
                AssertNeutral(fixture);

                fixture.Player.Play(
                    AvatarMotion.Death,
                    false,
                    1f);
                yield return new WaitForSeconds(0.52f);

                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.Presentation.localRotation),
                    Is.GreaterThan(35f));
                Assert.That(
                    Quaternion.Angle(
                        Quaternion.identity,
                        fixture.Equipment.MotionRoot.localRotation),
                    Is.GreaterThan(20f));

                fixture.Player.ResetMotion();
                AssertNeutral(fixture);
            }
            finally
            {
                Object.Destroy(fixture.Unit);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SwitchDisableAndStopResetOwnedPresentation()
        {
            Fixture fixture = CreateFixture();
            try
            {
                Vector3 placementPosition =
                    fixture.Equipment.PlacementRoot.localPosition;
                Quaternion placementRotation =
                    fixture.Equipment.PlacementRoot.localRotation;

                fixture.Player.Play(
                    AvatarMotion.Attack,
                    false,
                    1f);
                yield return new WaitForSeconds(0.12f);

                Assert.That(
                    fixture.Presentation.localPosition.sqrMagnitude,
                    Is.GreaterThan(0.0001f));

                fixture.Player.Play(
                    AvatarMotion.Hit,
                    false,
                    1f);
                Assert.AreEqual(
                    AvatarMotion.Hit,
                    fixture.Player.CurrentMotion);
                yield return new WaitForSeconds(0.06f);

                fixture.Player.enabled = false;
                yield return null;
                AssertNeutral(fixture);
                Assert.That(
                    Vector3.Distance(
                        placementPosition,
                        fixture.Equipment.PlacementRoot.localPosition),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        placementRotation,
                        fixture.Equipment.PlacementRoot.localRotation),
                    Is.LessThan(0.0001f));

                fixture.Player.enabled = true;
                yield return new WaitForSeconds(0.06f);
                fixture.Player.Stop();
                AssertNeutral(fixture);
            }
            finally
            {
                Object.Destroy(fixture.Unit);
            }

            yield return null;
        }

        private static Fixture CreateFixture()
        {
            var unit =
                new GameObject("UnitRoot");
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

            var placement =
                new GameObject("Equipment Placement");
            placement.transform.SetParent(
                presentation.transform,
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
                new Vector3(0.75f, 0f, 0.2f));

            AvatarPresentation avatar =
                unit.AddComponent<AvatarPresentation>();
            avatar.Configure(
                unit.transform,
                presentation.transform,
                bodyMesh,
                new[] { equipment });

            AvatarMotionPlayer player =
                unit.AddComponent<AvatarMotionPlayer>();
            player.Configure(
                avatar,
                AvatarMotion.Idle,
                false,
                1f);

            return new Fixture
            {
                Unit = unit,
                Presentation = presentation.transform,
                Avatar = avatar,
                Equipment = equipment,
                Player = player
            };
        }

        private static void AssertUnitRootUnchanged(
            Fixture fixture,
            Vector3 position,
            Quaternion rotation)
        {
            Assert.That(
                Vector3.Distance(
                    position,
                    fixture.Unit.transform.position),
                Is.LessThan(0.0001f));
            Assert.That(
                Quaternion.Angle(
                    rotation,
                    fixture.Unit.transform.rotation),
                Is.LessThan(0.0001f));
        }

        private static void AssertNeutral(Fixture fixture)
        {
            Assert.That(
                fixture.Presentation.localPosition.sqrMagnitude,
                Is.LessThan(0.0001f));
            Assert.That(
                Quaternion.Angle(
                    Quaternion.identity,
                    fixture.Presentation.localRotation),
                Is.LessThan(0.0001f));
            Assert.That(
                Vector3.Distance(
                    Vector3.one,
                    fixture.Presentation.localScale),
                Is.LessThan(0.0001f));
            Assert.That(
                fixture.Equipment.MotionRoot.localPosition.sqrMagnitude,
                Is.LessThan(0.0001f));
            Assert.That(
                Quaternion.Angle(
                    Quaternion.identity,
                    fixture.Equipment.MotionRoot.localRotation),
                Is.LessThan(0.0001f));
        }

        private sealed class Fixture
        {
            public GameObject Unit;
            public Transform Presentation;
            public AvatarPresentation Avatar;
            public EquipmentPresentation Equipment;
            public AvatarMotionPlayer Player;
        }
    }
}
