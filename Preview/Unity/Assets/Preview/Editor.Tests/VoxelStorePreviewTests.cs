using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hero.Voxel.Actor.Runtime.Unity;
using Hero.Voxel.Editor.Preview.Scene;
using Hero.Voxel.Render.Runtime.Unity;
using NUnit.Framework;
using UnityEngine;

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
        public void PreviewHasNoStoreRuntimeAssembly()
        {
            string projectRoot =
                Path.GetDirectoryName(Application.dataPath);
            Assert.That(projectRoot, Is.Not.Null);

            string runtimeDirectory = Path.Combine(
                projectRoot,
                "Assets",
                "Preview",
                "Runtime");
            Assert.IsFalse(
                Directory.Exists(runtimeDirectory),
                "voxel-store preview must not own a Runtime assembly directory.");

            foreach (string asmdef in Directory.GetFiles(
                         Path.Combine(projectRoot, "Assets", "Preview"),
                         "*.asmdef",
                         SearchOption.AllDirectories))
            {
                StringAssert.DoesNotContain(
                    "VoxelStore.Preview." + "Runtime",
                    File.ReadAllText(asmdef),
                    asmdef);
            }
        }

        [Test]
        public void CaptureRegistryContainsOnlyStoreContentScenario()
        {
            string registryPath = Path.Combine(
                VoxelStorePreviewPaths.ProjectRoot,
                "Assets",
                "Preview",
                "Capture",
                "CaptureRegistry.json");
            string json = File.ReadAllText(registryPath);
            StringAssert.Contains(
                "\"name\": \"avatar-motion-gallery\"",
                json);
            StringAssert.DoesNotContain(
                "mutable-streamed-actor-" + "gallery",
                json);
            StringAssert.DoesNotContain(
                "MutableStreamedActorCaptureRunner",
                json);
        }

        [Test]
        public void BakedVariantCoverageIsKindSpecificAndExact()
        {
            CollectionAssert.AreEqual(
                new[] { "Attack.Right.V1" },
                PreviewVariantCoverage.MissingClips(
                    "Avatars",
                    new[]
                    {
                        "Idle.V1",
                        "WalkForward.V1",
                        "Attack.Left.V1",
                        "Hit.V1",
                        "Death.V1"
                    }));

            Assert.AreEqual(
                0,
                PreviewVariantCoverage.MissingClips(
                    "Equipment",
                    new[]
                    {
                        "Idle.V1",
                        "WalkForward.V1",
                        "Attack.V1",
                        "Hit.V1",
                        "Death.V1"
                    }).Length);
        }

        [Test]
        public void ParsePlanAcceptsValidAndEmptyPlans()
        {
            PreviewPlanContract valid = PreviewPlanClient.Parse(
                "{\"Units\":[" + ValidUnitJson + "]}");
            Assert.AreEqual(1, valid.Units.Length);
            Assert.AreEqual(
                0,
                PreviewPlanClient.Parse("{\"Units\":[]}").Units.Length);
        }

        [Test]
        public void ParsePlanRejectsMalformedIncompleteAndFailedProcess()
        {
            Assert.Throws<InvalidDataException>(
                () => PreviewPlanClient.Parse(""));
            Assert.Throws<InvalidDataException>(
                () => PreviewPlanClient.Parse("{not-json"));
            Assert.Throws<InvalidDataException>(
                () => PreviewPlanClient.Parse(
                    "{\"Units\":[{\"Id\":\"a\"}]}"));
            InvalidOperationException failure =
                Assert.Throws<InvalidOperationException>(
                    () => PreviewPlanClient.CompleteProcess(
                        2,
                        string.Empty,
                        "provider failed"));
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
                GeneratedAnimation =
                    "Assets/Avatars/Fixture/Source/Fixture.animation.generated.yaml"
            };
            PreviewBakePaths paths =
                PreviewBake.CreatePaths(
                    unit,
                    VoxelStorePreviewPaths.StoreRoot);
            StringAssert.EndsWith(
                Path.Combine(
                    "Assets",
                    "Avatars",
                    "Fixture",
                    "Source",
                    "Fixture.generated.vox"),
                paths.SourceVox);
            Assert.AreEqual(
                "Assets/__VoxelStorePreview/Baked/Avatars/Fixture",
                paths.OutputDirectory);
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
            Assert.Throws<InvalidOperationException>(
                () => VoxelStorePreviewPaths.RequireDerivedAssetPath(
                    "Assets/Preview"));
            Assert.Throws<InvalidOperationException>(
                () => VoxelStorePreviewPaths.RequireDerivedAssetPath(
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
                        SourceBase = "Assets/Avatars/A/Source/A"
                    },
                    new PreviewUnitContract
                    {
                        Id = "foundation.equipment.b",
                        Kind = "equipment",
                        SourceBase = "Assets/Equipment/B/Source/B"
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
        }

        [Test]
        public void BakeFailurePreventsGridBuild()
        {
            bool built = false;
            Assert.Throws<InvalidOperationException>(
                () => PreviewRefreshOrchestrator.Run(
                    () => new PreviewPlanContract
                    {
                        Units = new[]
                        {
                            new PreviewUnitContract
                            {
                                Id = "broken",
                                Kind = "avatar"
                            }
                        }
                    },
                    plan => PreviewBake.BakeAll(
                        plan,
                        _ => { },
                        _ => throw new IOException("bake exploded")),
                    _ =>
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
                new PreviewPlanContract
                {
                    Units = Array.Empty<PreviewUnitContract>()
                },
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
        public void PreviewCatalogComesFromImportedFolderStructure()
        {
            Assert.IsTrue(
                PreviewCatalogDiscovery.TryDescribeAssetPath(
                    "Assets/__VoxelStorePreview/Baked/HumanImport/Avatars/ZombieBrute01/Source/ZombieBrute01.asset",
                    out string avatarCatalog,
                    out string avatarName));
            Assert.AreEqual("Avatars", avatarCatalog);
            Assert.AreEqual("ZombieBrute01", avatarName);

            Assert.IsTrue(
                PreviewCatalogDiscovery.TryDescribeProviderAssetPath(
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
        }

        [Test]
        public void MissingBakedAnimationFailsPreviewCatalogLoad()
        {
            var model = new PreviewCatalogModel
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
                    () => PreviewCatalogDiscovery.LoadAnimation(model));
            StringAssert.Contains(
                "requires the baked authored animation asset",
                exception.Message);
        }

        [Test]
        public void GridMathUsesFiveColumnsForTwentyFiveWithoutOverlap()
        {
            Assert.AreEqual(5, PreviewGridLayout.ColumnsForCount(25));
            var source = Enumerable.Range(0, 25)
                .Select(i => new Bounds(
                    new Vector3(i == 0 ? 1f : 0f, 1f, 0f),
                    new Vector3(
                        i == 0 ? 4f : 1f,
                        2f,
                        i == 1 ? 3f : 1f)))
                .ToArray();
            Vector3[] positions =
                PreviewGridLayout.Calculate(source, 1f);
            var placed = new Bounds[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                placed[i] = source[i];
                placed[i].center += positions[i];
            }
            Assert.AreEqual(
                0,
                PreviewGridLayout.CountXZOverlaps(placed));
        }

        [Test]
        public void GeneratedObjectsRemainEditorOnlyAndTransient()
        {
            var go = new GameObject("generated-test");
            try
            {
                PreviewGridBuilder.ConfigureGenerated(go);
                Assert.AreEqual("EditorOnly", go.tag);
                Assert.AreNotEqual(
                    0,
                    go.hideFlags & HideFlags.DontSaveInEditor);
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

        [Test]
        public void CurrentBakedContentComposesDirectlyThroughProviderActor()
        {
            VoxelStorePreviewCommand.BakeAllForBatch();
            PreviewCatalogModel[] models =
                PreviewCatalogDiscovery.Discover();
            PreviewVariantCoverage.ValidateAll(models);

            PreviewCatalogModel avatarModel = models.First(
                x => PreviewCatalogDiscovery.IsCatalog(x, "Avatars"));
            PreviewCatalogModel equipmentModel = models.First(
                x => PreviewCatalogDiscovery.IsCatalog(x, "Equipment"));

            var root = new GameObject("provider-integration-test");
            VoxelRenderWorld world =
                root.AddComponent<VoxelRenderWorld>();
            VoxelRenderInstance body = null;
            VoxelRenderInstance equipment = null;
            VoxelActorInstance actor = null;
            try
            {
                world.Initialize();
                body = world.Spawn(
                    PreviewCatalogDiscovery.LoadAssembly(avatarModel),
                    PreviewCatalogDiscovery.LoadAnimation(avatarModel));
                body.RequirePart("Body");
                equipment = world.Spawn(
                    PreviewCatalogDiscovery.LoadAssembly(equipmentModel),
                    PreviewCatalogDiscovery.LoadAnimation(equipmentModel));

                actor = new VoxelActorInstance(body);
                VoxelActorBindingId binding = actor.Attach(
                    equipment,
                    "Equip.Hand.Right.0",
                    "Mount");
                actor.RootTrack.Select(new[] { "Idle" });
                actor.RootTrack.SampleNormalized(0.5f);
                VoxelModelAnimationTrack equipmentTrack =
                    actor.GetAttachmentTrack(binding);
                equipmentTrack.Select(new[] { "Idle" });
                equipmentTrack.SampleNormalized(0.5f);
                actor.Refresh();

                Assert.IsTrue(actor.ContainsBinding(binding));
                Assert.AreEqual(1, actor.BindingCount);
                Bounds bounds = actor.GetWorldBounds();
                Assert.IsTrue(float.IsFinite(bounds.center.x));
                world.SubmitNow();
                Assert.That(world.SubmittedPartCount, Is.GreaterThan(0));
            }
            finally
            {
                actor?.Dispose();
                equipment?.Dispose();
                body?.Dispose();
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void HeroVoxelEditorBuildsCurrentBakedModelDebugHierarchy()
        {
            VoxelStorePreviewCommand.BakeAllForBatch();
            PreviewCatalogModel model =
                PreviewCatalogDiscovery.Discover().First();
            GameObject debug = VoxelSceneDebugBuilder.Create(
                PreviewCatalogDiscovery.LoadAssembly(model),
                Vector3.zero);
            try
            {
                Assert.AreEqual("EditorOnly", debug.tag);
                Assert.That(
                    debug.GetComponentsInChildren<MeshRenderer>(true).Length,
                    Is.GreaterThan(0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(debug);
            }
        }
    }
}
