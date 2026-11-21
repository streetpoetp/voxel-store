using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                SourceBase = "Assets/Fixture/Source/Fixture",
                Palette = "Assets/Fixture/Source/Fixture.palette.yaml",
                GeneratedVox = "Assets/Fixture/Source/Fixture.generated.vox",
                GeneratedAnimation = "Assets/Fixture/Source/Fixture.animation.generated.yaml"
            };
            PreviewBakePaths paths =
                PreviewBake.CreatePaths(unit, VoxelStorePreviewPaths.StoreRoot);
            StringAssert.EndsWith(
                Path.Combine("Assets", "Fixture", "Source", "Fixture.generated.vox"),
                paths.SourceVox);
            StringAssert.EndsWith(
                Path.Combine("Assets", "Fixture", "Source", "Fixture.animation.generated.yaml"),
                paths.AnimationManifest);
            Assert.AreEqual(
                "Assets/__VoxelStorePreview/Baked/Avatars/fixture.avatar",
                paths.OutputDirectory);
            StringAssert.EndsWith("fixture.avatar.assembly.asset", paths.AssemblyAsset);
            StringAssert.EndsWith("fixture.avatar.animation.asset", paths.AnimationAsset);
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
                    new PreviewUnitContract { Id = "a", Kind = "avatar" },
                    new PreviewUnitContract { Id = "b", Kind = "equipment" }
                }
            };
            string[] stale = PreviewBake.PlanStaleDirectories(
                plan,
                new[]
                {
                    "Assets/__VoxelStorePreview/Baked/Avatars/a",
                    "Assets/__VoxelStorePreview/Baked/Avatars/stale",
                    "Assets/__VoxelStorePreview/Baked/Equipment/b"
                });
            CollectionAssert.AreEqual(
                new[] { "Assets/__VoxelStorePreview/Baked/Avatars/stale" },
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
        public void SafeUnitDirectorySeparatesKinds()
        {
            Assert.AreEqual(
                "Assets/__VoxelStorePreview/Baked/Avatars/a",
                VoxelStorePreviewPaths.SafeUnitDirectory(new PreviewUnitContract
                {
                    Id = "a",
                    Kind = "avatar"
                }));
            Assert.AreEqual(
                "Assets/__VoxelStorePreview/Baked/Equipment/b",
                VoxelStorePreviewPaths.SafeUnitDirectory(new PreviewUnitContract
                {
                    Id = "b",
                    Kind = "equipment"
                }));
        }
    }
}
