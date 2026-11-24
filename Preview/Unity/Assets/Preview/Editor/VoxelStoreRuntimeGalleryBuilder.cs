using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hero.Voxel.Render.Runtime.Unity;
using Hero.Voxel.Runtime.Assets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor
{
    [Serializable]
    internal sealed class RuntimeGalleryReport
    {
        public string ScenePath;
        public int PublishedModelCount;
        public int CatalogCount;
        public int ModelCount;
        public int AvatarCount;
        public int EquipmentCount;
        public int AnimatedAvatarCount;
        public int CarriedEquipmentCount;
        public int StandaloneEquipmentCount;
        public int GridOverlapCount;
        public string Motion;
        public string RootVariant;
        public string EquipmentVariant;
        public bool Loop;
        public float Speed;
        public string Screenshot;
    }

    internal sealed class RuntimeGalleryBuildResult
    {
        public Scene Scene;
        public GameObject Root;
        public Camera Camera;
        public VoxelRenderWorld World;
        public Bounds[] WorldBounds = Array.Empty<Bounds>();
        public int PublishedModelCount;
        public int CatalogCount;
        public int ModelCount;
        public int AvatarCount;
        public int EquipmentCount;
        public int AnimatedAvatarCount;
        public int CarriedEquipmentCount;
        public int StandaloneEquipmentCount;
    }

    public static class VoxelStoreRuntimeGalleryBuilder
    {
        internal const string ScenePath =
            "Assets/Preview/VoxelStoreRuntimeGallery.generated.unity";
        public const string RootName =
            "[Voxel Store Runtime Gallery]";

        [MenuItem("Voxel Store/Refresh Runtime Gallery Scene")]
        public static void RefreshRuntimeGalleryScene()
        {
            // Manual refresh owns the complete derived-preview refresh.
            VoxelStorePreviewCommand.BakeAllForBatch();
            RuntimeGalleryBuildResult result = BuildAndSave();
            WriteReportAndCapture(result);

            VoxelStoreRuntimeGalleryEditModeRestorer.Restore(
                result.Scene);
            Selection.activeGameObject = result.Root;
            FrameSceneView(result.WorldBounds);
            SceneView.RepaintAll();
            Debug.Log(
                $"Voxel Store runtime gallery ready: " +
                $"{result.ModelCount} imported models across " +
                $"{result.CatalogCount} catalogs; " +
                $"{result.AvatarCount} avatars, " +
                $"{result.EquipmentCount} equipment models, " +
                $"{result.CarriedEquipmentCount} carried equipment instances. " +
                "Press Play and watch the Game view.");
        }

        public static void RefreshRuntimeGallerySceneForBatch()
        {
            RuntimeGalleryBuildResult result = BuildAndSave();
            WriteReportAndCapture(result);
        }

        internal static RuntimeGalleryBuildResult BuildAndSave()
        {
            RuntimeGalleryModel[] models =
                RuntimeGalleryCatalogDiscovery.Discover();
            if (models.Length == 0)
            {
                throw new InvalidOperationException(
                    "Runtime gallery found no imported voxel models under " +
                    VoxelStorePreviewPaths.BakedRoot +
                    ". Expected <Catalog>/<Model>/Source/<Model>.asset.");
            }

            RuntimeGalleryModel[] avatarModels = models
                .Where(x =>
                    RuntimeGalleryCatalogDiscovery.IsCatalog(
                        x,
                        "Avatars"))
                .ToArray();
            RuntimeGalleryModel[] equipmentModels = models
                .Where(x =>
                    RuntimeGalleryCatalogDiscovery.IsCatalog(
                        x,
                        "Equipment"))
                .ToArray();

            Directory.CreateDirectory(
                Path.Combine(
                    VoxelStorePreviewPaths.ProjectRoot,
                    "Assets",
                    "Preview"));
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            var root = new GameObject(RootName);
            VoxelRenderWorld world =
                root.AddComponent<VoxelRenderWorld>();
            world.Initialize();

            var entries =
                new List<RuntimeGalleryEntry>(models.Length);
            int carriedEquipmentCount = 0;

            for (int i = 0; i < avatarModels.Length; i++)
            {
                RuntimeGalleryModel avatarModel = avatarModels[i];
                var unitRoot = new GameObject(
                    "Avatars | " + avatarModel.Name);
                unitRoot.transform.SetParent(root.transform, false);

                var presentation = new GameObject("PresentationRoot");
                presentation.transform.SetParent(
                    unitRoot.transform,
                    false);

                VoxelAssemblyAsset avatarAssembly =
                    RuntimeGalleryCatalogDiscovery.LoadAssembly(
                        avatarModel);
                VoxelAnimationAsset avatarAnimation =
                    RuntimeGalleryCatalogDiscovery.LoadAnimation(
                        avatarModel);
                RequirePart(
                    avatarAssembly,
                    "Body",
                    avatarModel.AssetPath);

                VoxelRenderPresentation body =
                    CreateRenderPresentation(
                        world,
                        avatarAssembly,
                        avatarAnimation,
                        presentation.transform,
                        "Body",
                        externalLifecycle: true);

                var equipmentPresentations =
                    new List<EquipmentPresentation>(2);

                if (equipmentModels.Length > 0)
                {
                    int primaryIndex = i % equipmentModels.Length;
                    int secondaryIndex =
                        (i + Math.Max(1, equipmentModels.Length / 2)) %
                        equipmentModels.Length;
                    if (equipmentModels.Length > 1 &&
                        secondaryIndex == primaryIndex)
                    {
                        secondaryIndex =
                            (primaryIndex + 1) %
                            equipmentModels.Length;
                    }

                    RuntimeGalleryModel primary =
                        equipmentModels[primaryIndex];
                    equipmentPresentations.Add(
                        CreateCarriedEquipment(
                            world,
                            RuntimeGalleryCatalogDiscovery.LoadAssembly(
                                primary),
                            RuntimeGalleryCatalogDiscovery.LoadAnimation(
                                primary),
                            presentation.transform,
                            primary.Name,
                            "Equip.Hand.Right.0",
                            EquipmentSlotSide.Right));
                    carriedEquipmentCount++;

                    if (equipmentModels.Length > 1)
                    {
                        RuntimeGalleryModel secondary =
                            equipmentModels[secondaryIndex];
                        equipmentPresentations.Add(
                            CreateCarriedEquipment(
                                world,
                                RuntimeGalleryCatalogDiscovery.LoadAssembly(
                                    secondary),
                                RuntimeGalleryCatalogDiscovery.LoadAnimation(
                                    secondary),
                                presentation.transform,
                                secondary.Name,
                                "Equip.Hand.Left.0",
                                EquipmentSlotSide.Left));
                        carriedEquipmentCount++;
                    }
                }

                AvatarPresentation avatar =
                    unitRoot.AddComponent<AvatarPresentation>();
                avatar.Configure(
                    unitRoot.transform,
                    presentation.transform,
                    body,
                    equipmentPresentations);

                AvatarMotionPlayer motionPlayer =
                    unitRoot.AddComponent<AvatarMotionPlayer>();
                motionPlayer.Configure(
                    avatar,
                    AvatarMotionPreviewPreferences.Motion,
                    AvatarMotionPreviewPreferences.Loop,
                    AvatarMotionPreviewPreferences.Speed,
                    AvatarMotionPreviewPreferences.RootVariant);
                motionPlayer.SetEquipmentVariant(
                    AvatarMotionPreviewPreferences.EquipmentVariant);

                entries.Add(new RuntimeGalleryEntry(
                    unitRoot,
                    avatar,
                    null));
            }

            foreach (RuntimeGalleryModel model in models)
            {
                if (RuntimeGalleryCatalogDiscovery.IsCatalog(
                        model,
                        "Avatars"))
                {
                    continue;
                }

                var unitRoot = new GameObject(
                    model.Catalog + " | " + model.Name);
                unitRoot.transform.SetParent(root.transform, false);

                var presentation = new GameObject("PresentationRoot");
                presentation.transform.SetParent(
                    unitRoot.transform,
                    false);

                VoxelRenderPresentation renderPresentation =
                    CreateRenderPresentation(
                        world,
                        RuntimeGalleryCatalogDiscovery.LoadAssembly(
                            model),
                        RuntimeGalleryCatalogDiscovery.LoadAnimation(
                            model),
                        presentation.transform,
                        "Model",
                        externalLifecycle: false);
                renderPresentation.EnsureInstance();

                entries.Add(new RuntimeGalleryEntry(
                    unitRoot,
                    null,
                    renderPresentation));
            }

            Bounds[] initialBounds =
                entries.Select(x => x.GetBounds()).ToArray();
            Vector3[] offsets = PreviewGridLayout.Calculate(
                initialBounds,
                margin: 1.75f);
            for (int i = 0; i < entries.Count; i++)
                entries[i].Root.transform.position = offsets[i];

            Bounds[] worldBounds =
                entries.Select(x => x.GetBounds()).ToArray();
            if (PreviewGridLayout.CountXZOverlaps(worldBounds) != 0)
            {
                throw new InvalidOperationException(
                    "Runtime gallery grid contains overlapping model bounds.");
            }

            Bounds galleryBounds = Combine(worldBounds);
            Camera camera = CreateCamera(galleryBounds);
            CreateDirectionalLight();

            world.SubmitNow();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new RuntimeGalleryBuildResult
            {
                Scene = scene,
                Root = root,
                Camera = camera,
                World = world,
                WorldBounds = worldBounds,
                PublishedModelCount = models.Length,
                CatalogCount = models
                    .Select(x => x.Catalog)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                ModelCount = models.Length,
                AvatarCount = avatarModels.Length,
                EquipmentCount = equipmentModels.Length,
                AnimatedAvatarCount =
                    root.GetComponentsInChildren<AvatarMotionPlayer>(true)
                        .Length,
                CarriedEquipmentCount = carriedEquipmentCount,
                StandaloneEquipmentCount = equipmentModels.Length
            };
        }

        private static void FrameSceneView(
            IReadOnlyList<Bounds> worldBounds)
        {
            if (worldBounds == null || worldBounds.Count == 0)
                return;

            Bounds combined = worldBounds[0];
            for (int i = 1; i < worldBounds.Count; i++)
                combined.Encapsulate(worldBounds[i]);

            SceneView view = SceneView.lastActiveSceneView;
            if (view == null)
                return;

            float size = Mathf.Max(
                1f,
                combined.extents.magnitude * 1.35f);
            view.LookAt(
                combined.center,
                Quaternion.Euler(30f, -135f, 0f),
                size);
            view.Repaint();
        }

        private static VoxelRenderPresentation CreateRenderPresentation(
            VoxelRenderWorld world,
            VoxelAssemblyAsset assembly,
            VoxelAnimationAsset animation,
            Transform parent,
            string objectName,
            bool externalLifecycle)
        {
            ValidateAssemblyAndAnimation(assembly, animation);

            var modelRoot = new GameObject(objectName);
            modelRoot.transform.SetParent(parent, false);
            VoxelRenderPresentation presentation =
                modelRoot.AddComponent<VoxelRenderPresentation>();
            presentation.Configure(
                world,
                modelRoot.transform,
                assembly,
                animation,
                externalLifecycle);
            return presentation;
        }

        private static EquipmentPresentation CreateCarriedEquipment(
            VoxelRenderWorld world,
            VoxelAssemblyAsset assembly,
            VoxelAnimationAsset animation,
            Transform presentationRoot,
            string equipmentId,
            string socketName,
            EquipmentSlotSide side)
        {
            var placementObject = new GameObject(
                "Equipment | " + ShortName(equipmentId));
            placementObject.transform.SetParent(
                presentationRoot,
                false);

            VoxelRenderPresentation model =
                placementObject.AddComponent<VoxelRenderPresentation>();
            model.Configure(
                world,
                placementObject.transform,
                assembly,
                animation,
                externalLifecycle: true);

            EquipmentPresentation presentation =
                placementObject.AddComponent<EquipmentPresentation>();
            presentation.Configure(
                model,
                socketName,
                side);
            return presentation;
        }

        private static void ValidateAssemblyAndAnimation(
            VoxelAssemblyAsset assembly,
            VoxelAnimationAsset animation)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (animation == null)
                throw new ArgumentNullException(nameof(animation));
            if (!ReferenceEquals(animation.Assembly, assembly))
            {
                throw new InvalidOperationException(
                    "Runtime gallery animation targets a different assembly: " +
                    AssemblyLabel(assembly));
            }
            if (assembly.PartCount == 0)
            {
                throw new InvalidOperationException(
                    "Runtime gallery model contains no parts: " +
                    AssemblyLabel(assembly));
            }

            for (int i = 0; i < assembly.PartCount; i++)
            {
                if (assembly.GetPart(i).Mesh == null)
                {
                    throw new InvalidOperationException(
                        "Runtime gallery part is missing its mesh: " +
                        AssemblyLabel(assembly) + "/" +
                        assembly.GetPart(i).Name);
                }
            }
        }

        private static void RequirePart(
            VoxelAssemblyAsset assembly,
            string partName,
            string assetPath)
        {
            for (int i = 0; i < assembly.PartCount; i++)
            {
                if (string.Equals(
                        assembly.GetPart(i).Name,
                        partName,
                        StringComparison.Ordinal))
                {
                    return;
                }
            }

            throw new InvalidOperationException(
                "Avatar catalog model must contain part '" +
                partName + "': " + assetPath);
        }

        private static Camera CreateCamera(Bounds bounds)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor =
                new Color(0.08f, 0.10f, 0.12f, 1f);
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane =
                Math.Max(100f, bounds.extents.magnitude * 8f);

            Quaternion rotation = Quaternion.Euler(35f, 0f, 0f);
            camera.transform.rotation = rotation;
            float distance = Math.Max(
                20f,
                bounds.extents.magnitude * 3f);
            camera.transform.position =
                bounds.center -
                camera.transform.forward * distance;

            const float targetAspect = 16f / 9f;
            float verticalForWidth =
                bounds.extents.x / targetAspect;
            float verticalForDepth =
                bounds.extents.z * 0.75f +
                bounds.extents.y * 0.65f;
            camera.orthographicSize =
                Math.Max(
                    Math.Max(
                        verticalForWidth,
                        verticalForDepth),
                    bounds.extents.y) +
                1.5f;

            return camera;
        }

        private static void CreateDirectionalLight()
        {
            var lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            lightObject.transform.rotation =
                Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.fog = false;
        }

        private static void WriteReportAndCapture(
            RuntimeGalleryBuildResult result)
        {
            string reportDir = Path.Combine(
                VoxelStorePreviewPaths.ProjectRoot,
                "Library",
                "VoxelStorePreview");
            Directory.CreateDirectory(reportDir);

            string legacyDiagnosticScreenshot = Path.Combine(
                reportDir,
                "runtime-gallery.png");
            if (File.Exists(legacyDiagnosticScreenshot))
                File.Delete(legacyDiagnosticScreenshot);

            var report = new RuntimeGalleryReport
            {
                ScenePath = ScenePath,
                PublishedModelCount = result.PublishedModelCount,
                CatalogCount = result.CatalogCount,
                ModelCount = result.ModelCount,
                AvatarCount = result.AvatarCount,
                EquipmentCount = result.EquipmentCount,
                AnimatedAvatarCount = result.AnimatedAvatarCount,
                CarriedEquipmentCount = result.CarriedEquipmentCount,
                StandaloneEquipmentCount =
                    result.StandaloneEquipmentCount,
                GridOverlapCount =
                    PreviewGridLayout.CountXZOverlaps(
                        result.WorldBounds),
                Motion = AvatarMotionPreviewPreferences.Motion.ToString(),
                RootVariant = AvatarMotionPreviewPreferences.RootVariant,
                EquipmentVariant =
                    AvatarMotionPreviewPreferences.EquipmentVariant,
                Loop = AvatarMotionPreviewPreferences.Loop,
                Speed = AvatarMotionPreviewPreferences.Speed,
                Screenshot = string.Empty
            };
            File.WriteAllText(
                Path.Combine(
                    reportDir,
                    "runtime-gallery-report.json"),
                JsonUtility.ToJson(report, true));
        }

        private static Bounds Combine(
            IReadOnlyList<Bounds> values)
        {
            if (values == null || values.Count == 0)
            {
                throw new InvalidOperationException(
                    "Runtime gallery contains no model bounds.");
            }

            Bounds result = values[0];
            for (int i = 1; i < values.Count; i++)
                result.Encapsulate(values[i]);
            return result;
        }

        private static string AssemblyLabel(
            VoxelAssemblyAsset assembly)
        {
            if (!string.IsNullOrWhiteSpace(assembly.UnitRef))
                return assembly.UnitRef;
            if (!string.IsNullOrWhiteSpace(assembly.AssemblyName))
                return assembly.AssemblyName;
            return "<unnamed voxel assembly>";
        }

        private static string ShortName(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "model";
            int lastDot = id.LastIndexOf('.');
            return lastDot >= 0 && lastDot + 1 < id.Length
                ? id[(lastDot + 1)..]
                : id;
        }

        private sealed class RuntimeGalleryEntry
        {
            public RuntimeGalleryEntry(
                GameObject root,
                AvatarPresentation avatar,
                VoxelRenderPresentation model)
            {
                Root = root;
                Avatar = avatar;
                Model = model;
            }

            public GameObject Root { get; }
            public AvatarPresentation Avatar { get; }
            public VoxelRenderPresentation Model { get; }

            public Bounds GetBounds()
            {
                if (Avatar != null)
                    return Avatar.GetWorldBounds();
                if (Model != null)
                    return Model.GetWorldBounds();
                throw new InvalidOperationException(
                    "Runtime gallery entry has no provider presentation.");
            }
        }
    }
}
