using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Hero.Voxel.Render.Runtime.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor.Tests
{
    public sealed class VoxelStorePreviewTests
    {
        private const string ValidUnitJson =
            "{\"Id\":\"a\",\"Kind\":\"avatar\",\"UnitRef\":\"u/a\"," +
            "\"SourceBase\":\"Assets/A/Source/A\",\"Palette\":\"Assets/A/Source/A.palette.yaml\"," +
            "\"GeneratedVox\":\"Assets/A/Source/A.generated.vox\"," +
            "\"GeneratedAnimation\":\"Assets/A/Source/A.animation.generated.yaml\"}";

        [Test]
        public void MotionVariantCandidatesPreserveSpecificityAndBaseFallback()
        {
            CollectionAssert.AreEqual(
                new[]
                {
                    "Attack.Left.V2",
                    "Attack.V2",
                    "Attack.Left",
                    "Attack"
                },
                AvatarMotionClipCandidates.Build(
                    AvatarMotion.Attack,
                    EquipmentSlotSide.Left,
                    "V2"));
            CollectionAssert.AreEqual(
                new[] { "WalkForward.V2", "WalkForward" },
                AvatarMotionClipCandidates.Build(
                    AvatarMotion.WalkForward,
                    null,
                    "V2"));
            CollectionAssert.AreEqual(
                new[] { "Attack.Right", "Attack" },
                AvatarMotionClipCandidates.Build(
                    AvatarMotion.Attack,
                    EquipmentSlotSide.Right,
                    string.Empty));
        }

        [Test]
        public void MotionVariantStateIsOptionalCanonicalAndModelLocal()
        {
            Assert.AreEqual(
                string.Empty,
                AvatarMotionClipCandidates.NormalizeVariant(null));
            Assert.Throws<ArgumentException>(
                () => AvatarMotionClipCandidates.NormalizeVariant("V0"));
            Assert.Throws<ArgumentException>(
                () => AvatarMotionClipCandidates.NormalizeVariant("Heavy"));

            var firstObject = new GameObject("equipment-v1");
            var secondObject = new GameObject("equipment-v2");
            try
            {
                EquipmentPresentation first =
                    firstObject.AddComponent<EquipmentPresentation>();
                EquipmentPresentation second =
                    secondObject.AddComponent<EquipmentPresentation>();
                Assert.IsTrue(first.SetVariant("V1"));
                Assert.IsTrue(second.SetVariant("V2"));
                Assert.AreEqual("V1", first.Variant);
                Assert.AreEqual("V2", second.Variant);
                Assert.IsTrue(first.SetVariant(string.Empty));
                Assert.AreEqual(string.Empty, first.Variant);
                Assert.AreEqual("V2", second.Variant);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(firstObject);
                UnityEngine.Object.DestroyImmediate(secondObject);
            }
        }

        [Test]
        public void ParsePlanAcceptsValidAndEmptyPlans()
        {
            PreviewPlanContract valid = PreviewPlanClient.Parse(
                "{\"Units\":[" + ValidUnitJson + "]}");
            Assert.AreEqual(1, valid.Units.Length);
            Assert.AreEqual(0, PreviewPlanClient.Parse("{\"Units\":[]}").Units.Length);
        }

        [Test]
        public void ParsePlanRejectsMalformedIncompleteAndFailedProcess()
        {
            Assert.Throws<InvalidDataException>(() => PreviewPlanClient.Parse(""));
            Assert.Throws<InvalidDataException>(() => PreviewPlanClient.Parse("{not-json"));
            Assert.Throws<InvalidDataException>(() =>
                PreviewPlanClient.Parse("{\"Units\":[{\"Id\":\"a\"}]}"));
            InvalidOperationException failure = Assert.Throws<InvalidOperationException>(() =>
                PreviewPlanClient.CompleteProcess(2, string.Empty, "provider failed"));
            StringAssert.Contains("provider failed", failure.Message);
        }

        [Test]
        public void ToolArgumentsUseRepositorySubmoduleWithoutProviderOverride()
        {
            string arguments = PreviewPlanClient.BuildToolArguments(
                "H:/repo/Tool/VoxelStore.Tool/VoxelStore.Tool.csproj",
                "H:/repo");
            StringAssert.Contains("preview-plan", arguments);
            StringAssert.DoesNotContain("HeroStudioRoot", arguments);
            StringAssert.DoesNotContain("HERO_STUDIO_ROOT", arguments);
        }
        [Test]
        public void BakePathsUsePublishedGeneratedArtifactsAndDerivedOutput()
        {
            var unit = new PreviewUnitContract
            {
                Id = "fixture.avatar",
                Kind = "avatar",
                UnitRef = "fixture/avatar",
                SourceBase = "Assets/Avatars/Fixture/Source/Fixture",
                Palette = "Assets/Avatars/Fixture/Source/Fixture.palette.yaml",
                GeneratedVox = "Assets/Avatars/Fixture/Source/Fixture.generated.vox",
                GeneratedAnimation = "Assets/Avatars/Fixture/Source/Fixture.animation.generated.yaml"
            };
            PreviewBakePaths paths =
                PreviewBake.CreatePaths(unit, VoxelStorePreviewPaths.StoreRoot);
            StringAssert.EndsWith(
                Path.Combine(
                    "Assets",
                    "Avatars",
                    "Fixture",
                    "Source",
                    "Fixture.generated.vox"),
                paths.SourceVox);
            StringAssert.EndsWith(
                Path.Combine(
                    "Assets",
                    "Avatars",
                    "Fixture",
                    "Source",
                    "Fixture.animation.generated.yaml"),
                paths.AnimationManifest);
            Assert.AreEqual(
                "Assets/__VoxelStorePreview/Baked/Avatars/Fixture",
                paths.OutputDirectory);
            StringAssert.EndsWith(
                "Fixture.assembly.asset",
                paths.AssemblyAsset);
            StringAssert.EndsWith(
                "Fixture.animation.asset",
                paths.AnimationAsset);
        }

        [Test]
        public void DerivedPathGuardRejectsEscapes()
        {
            Assert.AreEqual(
                "Assets/__VoxelStorePreview/Baked/Avatars/a",
                VoxelStorePreviewPaths.RequireDerivedAssetPath(
                    "Assets/__VoxelStorePreview/Baked/Avatars/a"));
            Assert.Throws<InvalidOperationException>(() =>
                VoxelStorePreviewPaths.RequireDerivedAssetPath("Assets/Preview"));
            Assert.Throws<InvalidOperationException>(() =>
                VoxelStorePreviewPaths.RequireDerivedAssetPath(
                    "Assets/__VoxelStorePreview/Baked/../Preview"));
        }

        [Test]
        public void StalePlannerKeepsCurrentAndRemovesOnlyStaleDerivedDirectories()
        {
            var plan = new PreviewPlanContract
            {
                Units = new[]
                {
                    new PreviewUnitContract
                    {
                        Id = "dungeon.avatar.a",
                        Kind = "avatar",
                        SourceBase =
                            "Assets/Avatars/A/Source/A"
                    },
                    new PreviewUnitContract
                    {
                        Id = "foundation.equipment.b",
                        Kind = "equipment",
                        SourceBase =
                            "Assets/Equipment/B/Source/B"
                    }
                }
            };
            string[] stale = PreviewBake.PlanStaleDirectories(
                plan,
                new[]
                {
                    "Assets/__VoxelStorePreview/Baked/Avatars/A",
                    "Assets/__VoxelStorePreview/Baked/Avatars/dungeon.avatar.a",
                    "Assets/__VoxelStorePreview/Baked/Avatars/Stale",
                    "Assets/__VoxelStorePreview/Baked/Equipment/B"
                });
            CollectionAssert.AreEqual(
                new[]
                {
                    "Assets/__VoxelStorePreview/Baked/Avatars/Stale",
                    "Assets/__VoxelStorePreview/Baked/Avatars/dungeon.avatar.a"
                },
                stale);
            Assert.Throws<InvalidOperationException>(() =>
                PreviewBake.PlanStaleDirectories(
                    plan,
                    new[] { "Assets/Outside/unsafe" }));
        }

        [Test]
        public void BakeFailurePreventsGridBuild()
        {
            bool built = false;
            Assert.Throws<InvalidOperationException>(() =>
                PreviewRefreshOrchestrator.Run(
                    () => new PreviewPlanContract
                    {
                        Units = new[]
                        {
                            new PreviewUnitContract { Id = "broken", Kind = "avatar" }
                        }
                    },
                    plan => PreviewBake.BakeAll(
                        plan,
                        _ => { },
                        unit => throw new IOException("bake exploded")),
                    baked =>
                    {
                        built = true;
                        return new PreviewGridBuildResult();
                    }));
            Assert.IsFalse(built);
        }

        [Test]
        public void EmptyPlanBakesNoUnits()
        {
            int calls = 0;
            List<BakedPreviewUnit> baked = PreviewBake.BakeAll(
                new PreviewPlanContract { Units = Array.Empty<PreviewUnitContract>() },
                _ => { },
                _ =>
                {
                    calls++;
                    return null;
                });
            Assert.AreEqual(0, calls);
            Assert.AreEqual(0, baked.Count);
        }

        [Test]
        public void RuntimeGallerySavedSceneRestoresEditModeProviderInstances()
        {
            Assert.IsFalse(
                EditorApplication.isPlaying,
                "regression must prove the manual Edit Mode scene path");

            try
            {
                RuntimeGalleryBuildResult built =
                    VoxelStoreRuntimeGalleryBuilder.BuildAndSave();
                Assert.That(
                    built.World.InstanceCount,
                    Is.GreaterThan(0),
                    "fresh gallery build must create provider instances");

                UnityEngine.SceneManagement.Scene reopened =
                    EditorSceneManager.OpenScene(
                        VoxelStoreRuntimeGalleryBuilder.ScenePath,
                        OpenSceneMode.Single);

                VoxelStoreRuntimeGalleryEditModeRestorer.Restore(
                    reopened);

                GameObject root = reopened.GetRootGameObjects()
                    .Single(x =>
                        x.name ==
                        VoxelStoreRuntimeGalleryBuilder.RootName);
                VoxelRenderWorld world =
                    root.GetComponent<VoxelRenderWorld>();
                VoxelRenderPresentation[] presentations =
                    root.GetComponentsInChildren<VoxelRenderPresentation>(
                        true);
                AvatarPresentation[] avatars =
                    root.GetComponentsInChildren<AvatarPresentation>(
                        true);

                Assert.That(presentations.Length, Is.GreaterThan(0));
                Assert.AreEqual(
                    presentations.Length,
                    world.InstanceCount,
                    "save/reopen must restore every serialized voxel presentation into the transient render world");
                Assert.That(
                    world.SubmittedPartCount,
                    Is.GreaterThan(0),
                    "restored Edit Mode gallery must submit visible voxel parts");
                Assert.That(
                    avatars.All(x => x.IsProviderInitialized),
                    Is.True,
                    "reopened avatar presentations must restore their actor-owned provider instances");
            }
            finally
            {
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);
            }
        }

        [Test]
        public void RuntimeGalleryCatalogComesFromImportedFolderStructure()
        {
            Assert.IsTrue(
                RuntimeGalleryCatalogDiscovery.TryDescribeAssetPath(
                    "Assets/__VoxelStorePreview/Baked/HumanImport/Avatars/ZombieBrute01/Source/ZombieBrute01.asset",
                    out string avatarCatalog,
                    out string avatarName));
            Assert.AreEqual("Avatars", avatarCatalog);
            Assert.AreEqual("ZombieBrute01", avatarName);

            Assert.IsTrue(
                RuntimeGalleryCatalogDiscovery.TryDescribeAssetPath(
                    "Assets/__VoxelStorePreview/Baked/Anything/Weapons/Bow02/Source/Bow02.asset",
                    out string weaponCatalog,
                    out string weaponName));
            Assert.AreEqual("Weapons", weaponCatalog);
            Assert.AreEqual("Bow02", weaponName);

            Assert.IsFalse(
                RuntimeGalleryCatalogDiscovery.TryDescribeAssetPath(
                    "Assets/__VoxelStorePreview/Baked/Weapons/Bow02/Bow02.asset",
                    out _,
                    out _));
        }

        [Test]
        public void ProviderBakedGalleryPathResolvesMatchingAnimationAsset()
        {
            Assert.IsTrue(
                RuntimeGalleryCatalogDiscovery.TryDescribeProviderAssetPath(
                    "Assets/__VoxelStorePreview/Baked",
                    "Assets/__VoxelStorePreview/Baked/Avatars/ZombieBrute01/ZombieBrute01.assembly.asset",
                    out string catalog,
                    out string modelName,
                    out string animationAssetPath));
            Assert.AreEqual("Avatars", catalog);
            Assert.AreEqual("ZombieBrute01", modelName);
            Assert.AreEqual(
                "Assets/__VoxelStorePreview/Baked/Avatars/ZombieBrute01/ZombieBrute01.animation.asset",
                animationAssetPath);

            Assert.IsFalse(
                RuntimeGalleryCatalogDiscovery.TryDescribeProviderAssetPath(
                    "Assets/__VoxelStorePreview/Baked",
                    "Assets/__VoxelStorePreview/Baked/Avatars/Avatars/Foo/Source/Foo.asset",
                    out _,
                    out _,
                    out _));
        }


        [Test]
        public void MissingBakedAnimationFailsSynchronizedGalleryLoad()
        {
            var model = new RuntimeGalleryModel
            {
                Catalog = "Avatars",
                Name = "MissingAnimation",
                AssetPath =
                    "Assets/__VoxelStorePreview/Baked/Avatars/MissingAnimation/MissingAnimation.assembly.asset",
                AnimationAssetPath =
                    "Assets/__VoxelStorePreview/Baked/Avatars/MissingAnimation/MissingAnimation.animation.asset"
            };

            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(
                    () => RuntimeGalleryCatalogDiscovery.LoadAnimation(model));
            StringAssert.Contains(
                "requires the baked authored animation asset",
                exception.Message);
        }

        [Test]
        public void MismatchedBakedAnimationFailsSynchronizedGalleryLoad()
        {
            RuntimeGalleryModel[] models =
                RuntimeGalleryCatalogDiscovery.Discover();
            Assert.That(models.Length, Is.GreaterThanOrEqualTo(2));

            RuntimeGalleryModel first = models[0];
            RuntimeGalleryModel second = models
                .Skip(1)
                .First(x =>
                    !string.Equals(
                        x.AssetPath,
                        first.AssetPath,
                        StringComparison.Ordinal));

            var stale = new RuntimeGalleryModel
            {
                Catalog = first.Catalog,
                Name = first.Name,
                AssetPath = first.AssetPath,
                AnimationAssetPath =
                    second.AnimationAssetPath
            };

            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(
                    () => RuntimeGalleryCatalogDiscovery.LoadAnimation(stale));
            StringAssert.Contains(
                "targets a different assembly",
                exception.Message);
        }

        [Test]
        public void GridMathUsesFiveColumnsForTwentyFiveWithoutOverlap()
        {
            Assert.AreEqual(5, PreviewGridLayout.ColumnsForCount(25));
            var source = Enumerable.Range(0, 25)
                .Select(i => new Bounds(
                    new Vector3(i == 0 ? 1f : 0f, 1f, 0f),
                    new Vector3(i == 0 ? 4f : 1f, 2f, i == 1 ? 3f : 1f)))
                .ToArray();
            Vector3[] positions = PreviewGridLayout.Calculate(source, 1f);
            Assert.AreEqual(25, positions.Length);

            var placed = new Bounds[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                placed[i] = source[i];
                placed[i].center += positions[i];
                Assert.AreEqual(0f, placed[i].min.y, 0.0001f);
                Assert.AreEqual(source[i].size, placed[i].size);
            }
            Assert.AreEqual(0, PreviewGridLayout.CountXZOverlaps(placed));
        }

        [Test]
        public void GeneratedObjectsRemainEditorOnlyAndTransient()
        {
            var go = new GameObject("generated-test");
            try
            {
                PreviewGridBuilder.ConfigureGenerated(go);
                Assert.AreEqual("EditorOnly", go.tag);
                Assert.AreNotEqual(0, go.hideFlags & HideFlags.DontSaveInEditor);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BakedModelDirectoryMirrorsCanonicalModelPath()
        {
            Assert.AreEqual(
                "Assets/__VoxelStorePreview/Baked/Avatars/ZombieBrute01",
                VoxelStorePreviewPaths.BakedModelDirectory(
                    new PreviewUnitContract
                    {
                        Id = "dungeon.avatar.zombie-brute-01",
                        Kind = "avatar",
                        SourceBase =
                            "Assets/Avatars/ZombieBrute01/Source/ZombieBrute01"
                    }));
            Assert.AreEqual(
                "Assets/__VoxelStorePreview/Baked/Equipment/WarPick01",
                VoxelStorePreviewPaths.BakedModelDirectory(
                    new PreviewUnitContract
                    {
                        Id = "foundation.equipment.war-pick.01",
                        Kind = "equipment",
                        SourceBase =
                            "Assets/Equipment/WarPick01/Source/WarPick01"
                    }));
            Assert.Throws<InvalidDataException>(
                () => VoxelStorePreviewPaths.BakedModelDirectory(
                    new PreviewUnitContract
                    {
                        Id = "bad",
                        Kind = "avatar",
                        SourceBase =
                            "Assets/Avatars/Wrong/Source/Other"
                    }));
        }
    }
}
