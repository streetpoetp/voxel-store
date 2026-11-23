using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public bool Loop;
        public float Speed;
        public string Screenshot;
    }

    internal sealed class RuntimeGalleryBuildResult
    {
        public Scene Scene;
        public GameObject Root;
        public Camera Camera;
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
        internal const string RuntimeGeneratedRoot =
            "Assets/__VoxelStorePreview/Baked/RuntimeGallery";
        internal const string RuntimeMaterialPath =
            RuntimeGeneratedRoot + "/VoxelAssetPreview.mat";


        [MenuItem("Voxel Store/Refresh Runtime Gallery Scene")]
        public static void RefreshRuntimeGalleryScene()
        {
            RuntimeGalleryBuildResult result = BuildAndSave();
            WriteReportAndCapture(result);

            Selection.activeGameObject = result.Root;
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

            Material voxelMaterial = RequireRuntimeMaterial();

            var root = new GameObject(RootName);
            var entries = new List<GameObject>(models.Length);
            var initialBounds = new List<Bounds>(models.Length);

            int carriedEquipmentCount = 0;
            for (int i = 0; i < avatarModels.Length; i++)
            {
                RuntimeGalleryModel avatarModel = avatarModels[i];

                var unitRoot = new GameObject(
                    "Avatars | " + avatarModel.Name);
                unitRoot.transform.SetParent(root.transform, false);

                var presentation = new GameObject("PresentationRoot");
                presentation.transform.SetParent(unitRoot.transform, false);

                VoxelAssemblyAsset avatarAssembly =
                    RuntimeGalleryCatalogDiscovery.LoadAssembly(
                        avatarModel);
                VoxelAnimationAsset avatarAnimation =
                    RuntimeGalleryCatalogDiscovery.LoadAnimation(
                        avatarModel);
                AssemblyVisual body = CreateAssemblyVisual(
                    avatarAssembly,
                    avatarAnimation,
                    presentation.transform,
                    voxelMaterial,
                    "Body");
                if (body.BodyMeshFilter == null)
                {
                    throw new InvalidOperationException(
                        "Avatar catalog model must contain a Body part: " +
                        avatarModel.AssetPath);
                }

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
                            RuntimeGalleryCatalogDiscovery.LoadAssembly(
                                primary),
                            RuntimeGalleryCatalogDiscovery.LoadAnimation(
                                primary),
                            presentation.transform,
                            voxelMaterial,
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
                                RuntimeGalleryCatalogDiscovery.LoadAssembly(
                                    secondary),
                                RuntimeGalleryCatalogDiscovery.LoadAnimation(
                                    secondary),
                                presentation.transform,
                                voxelMaterial,
                                secondary.Name,
                                "Equip.Hand.Left.0",
                                EquipmentSlotSide.Left));
                        carriedEquipmentCount++;
                    }
                }

                AvatarPresentation avatarPresentation =
                    unitRoot.AddComponent<AvatarPresentation>();
                avatarPresentation.Configure(
                    unitRoot.transform,
                    presentation.transform,
                    body.BodyMeshFilter,
                    body.Animated,
                    equipmentPresentations);

                AvatarMotionPlayer motionPlayer =
                    unitRoot.AddComponent<AvatarMotionPlayer>();
                motionPlayer.Configure(
                    avatarPresentation,
                    AvatarMotionPreviewPreferences.Motion,
                    AvatarMotionPreviewPreferences.Loop,
                    AvatarMotionPreviewPreferences.Speed);

                entries.Add(unitRoot);
                initialBounds.Add(MeasureRenderBounds(unitRoot));
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
                presentation.transform.SetParent(unitRoot.transform, false);
                CreateAssemblyVisual(
                    RuntimeGalleryCatalogDiscovery.LoadAssembly(
                        model),
                    RuntimeGalleryCatalogDiscovery.LoadAnimation(
                        model),
                    presentation.transform,
                    voxelMaterial,
                    "Model");

                entries.Add(unitRoot);
                initialBounds.Add(MeasureRenderBounds(unitRoot));
            }

            Vector3[] offsets = PreviewGridLayout.Calculate(
                initialBounds,
                margin: 1.75f);
            for (int i = 0; i < entries.Count; i++)
                entries[i].transform.position = offsets[i];

            var worldBounds = entries
                .Select(MeasureRenderBounds)
                .ToArray();
            if (PreviewGridLayout.CountXZOverlaps(worldBounds) != 0)
                throw new InvalidOperationException(
                    "Runtime gallery grid contains overlapping model bounds.");

            Bounds galleryBounds = Combine(worldBounds);
            Camera camera = CreateCamera(galleryBounds);
            CreateDirectionalLight();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new RuntimeGalleryBuildResult
            {
                Scene = scene,
                Root = root,
                Camera = camera,
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

        private static Material RequireRuntimeMaterial()
        {
            string absoluteDirectory = Path.Combine(
                VoxelStorePreviewPaths.ProjectRoot,
                RuntimeGeneratedRoot.Replace(
                    '/',
                    Path.DirectorySeparatorChar));
            Directory.CreateDirectory(absoluteDirectory);
            AssetDatabase.Refresh();

            Material existing =
                AssetDatabase.LoadAssetAtPath<Material>(
                    RuntimeMaterialPath);
            Shader shader = Shader.Find(
                "Hidden/Hero/VoxelAssetPreview");
            if (shader == null)
            {
                throw new InvalidOperationException(
                    "Hero.Voxel URP preview shader is unavailable: " +
                    "Hidden/Hero/VoxelAssetPreview");
            }

            if (existing != null)
            {
                if (existing.shader != shader)
                {
                    existing.shader = shader;
                    EditorUtility.SetDirty(existing);
                }
                return existing;
            }

            var material = new Material(shader)
            {
                name = "Voxel Store Runtime Gallery Material",
                enableInstancing = true
            };
            AssetDatabase.CreateAsset(
                material,
                RuntimeMaterialPath);
            return material;
        }

        private static AssemblyVisual CreateAssemblyVisual(
            VoxelAssemblyAsset assembly,
            VoxelAnimationAsset animation,
            Transform parent,
            Material material,
            string objectName)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (animation == null)
                throw new ArgumentNullException(nameof(animation));
            if (animation.Assembly != assembly)
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

            var modelRoot = new GameObject(objectName);
            modelRoot.transform.SetParent(parent, false);

            var partObjects = new GameObject[assembly.PartCount];
            var filters = new MeshFilter[assembly.PartCount];
            MeshFilter bodyFilter = null;

            for (int i = 0; i < assembly.PartCount; i++)
            {
                VoxelAssemblyPart part = assembly.GetPart(i);
                if (part.Mesh == null)
                {
                    throw new InvalidOperationException(
                        "Runtime gallery part is missing its mesh: " +
                        AssemblyLabel(assembly) +
                        "/" +
                        part.Name);
                }

                var partObject = new GameObject(part.Name);
                partObjects[i] = partObject;

                MeshFilter filter =
                    partObject.AddComponent<MeshFilter>();
                filter.sharedMesh = part.Mesh;
                filters[i] = filter;

                MeshRenderer renderer =
                    partObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode =
                    UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage =
                    UnityEngine.Rendering.LightProbeUsage.Off;
                renderer.reflectionProbeUsage =
                    UnityEngine.Rendering.ReflectionProbeUsage.Off;

                if (string.Equals(
                        part.Name,
                        "Body",
                        StringComparison.Ordinal))
                {
                    bodyFilter = filter;
                }
            }

            for (int i = 0; i < assembly.PartCount; i++)
            {
                VoxelAssemblyPart part = assembly.GetPart(i);
                if (part.ParentIndex < -1 ||
                    part.ParentIndex >= assembly.PartCount ||
                    part.ParentIndex == i)
                {
                    throw new InvalidOperationException(
                        "Runtime gallery part has an invalid parent index: " +
                        AssemblyLabel(assembly) +
                        "/" +
                        part.Name);
                }

                Transform partParent =
                    part.ParentIndex >= 0
                        ? partObjects[part.ParentIndex].transform
                        : modelRoot.transform;
                Transform partTransform = partObjects[i].transform;
                partTransform.SetParent(partParent, false);
                partTransform.localPosition = part.BindLocalPosition;
                partTransform.localRotation = part.BindLocalRotation;
                partTransform.localScale = part.BindLocalScale;
            }

            VoxelAnimatedAssembly animated =
                modelRoot.AddComponent<VoxelAnimatedAssembly>();
            animated.Configure(
                modelRoot.transform,
                assembly,
                animation,
                partObjects.Select(x => x.transform).ToArray());

            return new AssemblyVisual
            {
                Root = modelRoot,
                MeshFilter = filters[0],
                BodyMeshFilter = bodyFilter,
                Animated = animated
            };
        }

        private static EquipmentPresentation CreateCarriedEquipment(
            VoxelAssemblyAsset assembly,
            VoxelAnimationAsset animation,
            Transform presentationRoot,
            Material material,
            string equipmentId,
            string socketName,
            EquipmentSlotSide side)
        {
            var placementObject = new GameObject(
                "Equipment | " + ShortName(equipmentId));
            placementObject.transform.SetParent(
                presentationRoot,
                false);

            AssemblyVisual visual = CreateAssemblyVisual(
                assembly,
                animation,
                placementObject.transform,
                material,
                "Model");

            EquipmentPresentation presentation =
                placementObject.AddComponent<EquipmentPresentation>();
            presentation.Configure(
                placementObject.transform,
                visual.Animated,
                socketName,
                side);
            return presentation;
        }

        private static Camera CreateCamera(Bounds bounds)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.10f, 0.12f, 1f);
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

            string screenshotPath = Path.Combine(
                reportDir,
                "runtime-gallery.png");
            CaptureCamera(
                result.Camera,
                screenshotPath);

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
                Loop = AvatarMotionPreviewPreferences.Loop,
                Speed = AvatarMotionPreviewPreferences.Speed,
                Screenshot = "runtime-gallery.png"
            };
            File.WriteAllText(
                Path.Combine(
                    reportDir,
                    "runtime-gallery-report.json"),
                JsonUtility.ToJson(report, true));
        }

        internal static void CaptureCamera(
            Camera camera,
            string outputPath)
        {
            const int width = 1600;
            const int height = 900;
            var target = new RenderTexture(
                width,
                height,
                24,
                RenderTextureFormat.ARGB32);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            var texture = new Texture2D(
                width,
                height,
                TextureFormat.RGB24,
                false);
            try
            {
                camera.targetTexture = target;
                RenderTexture.active = target;
                camera.Render();
                texture.ReadPixels(
                    new Rect(0, 0, width, height),
                    0,
                    0);
                texture.Apply(false, false);
                File.WriteAllBytes(
                    outputPath,
                    texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(texture);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static Bounds MeasureRenderBounds(GameObject root)
        {
            Renderer[] renderers =
                root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new InvalidOperationException(
                    "Runtime gallery entry contains no renderer: " +
                    root.name);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static Bounds Combine(IReadOnlyList<Bounds> values)
        {
            if (values == null || values.Count == 0)
                throw new InvalidOperationException(
                    "Runtime gallery contains no model bounds.");

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


        private sealed class AssemblyVisual
        {
            public GameObject Root;
            public MeshFilter MeshFilter;
            public MeshFilter BodyMeshFilter;
            public VoxelAnimatedAssembly Animated;
        }
    }
}
