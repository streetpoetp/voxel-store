# Design

## Context

See `proposal.md` for motivation. voxel-store currently owns source/reference/publication authority and a standalone .NET Tool, but deliberately owns no Unity runtime. Hero.Voxel owns the canonical VoxelSpec compiler, `VoxelAssemblyBaker`, `VoxelAnimationBaker`, baked asset types, Inspector preview, and Scene Debug construction.

The store is pinned to Hero Studio revision `56d2997250a8d49846a5246b52de5a03bc44b88e`. That revision already contains the required Editor APIs, and its Unity project uses Unity `6000.3.24f1`.

The generic Hero.Voxel folder importer is not sufficient as voxel-store's authoritative integration because it optionally discovers semantic identity from `.unitref` sidecars, while voxel-store owns identity and source locations in `asset.yaml` and derives publication through `StoreEngine.Scan`. The preview therefore needs a thin store-to-Hero.Voxel adapter without creating a second manifest parser in Unity.

## Goals / Non-Goals

**Goals:**
- Provide a standalone Unity Editor workspace under `Preview/Unity/`.
- Reuse the exact pinned Hero.Voxel Unity package and bake/preview APIs.
- Derive preview membership from current store publication evaluation.
- Bake authoritative voxel-store sources in place into disposable Unity assets.
- Open one canonical scene containing every currently published avatar and equipment unit in a deterministic grid.
- Preserve authored world scale so the scene can be used to judge relative size as well as silhouette.
- Keep refresh idempotent and remove stale preview output safely.

**Non-Goals:**
- No game runtime, player build, Addressables/content-delivery layer, or production scene.
- No VoxelSpec/YAML parser, MagicaVoxel parser, mesh builder, assembly transform evaluator, animation evaluator, or renderer owned by voxel-store.
- No copied Unity-side authoring source or `.unitref` compatibility sidecars.
- No Unity bake output used as publication/proof/compatibility authority.
- No modification to Hero.Voxel itself as part of this change.

## Decisions

### 1. Add a standalone Editor-only Unity project at `Preview/Unity/`

The preview project is a development workspace, not a consumer game. Its tracked content is limited to Unity project configuration, the canonical preview scene shell, voxel-store Editor integration, and tests.

The project uses Unity `6000.3.24f1`, matching the pinned Hero Studio revision. Standard Unity generated directories (`Library`, `Temp`, `Logs`, `obj`, user settings as appropriate) remain ignored.

**Alternative considered:** put preview code into the Hero Studio development Unity project. Rejected because voxel-store would then have no self-contained workspace and preview behavior would depend on another repository's development scene/layout.

### 2. Resolve Hero Studio locally from the existing pin instead of committing or cloning another provider

A preview bootstrap command reads `toolchain/hero-voxel.lock.yaml`, resolves `HERO_STUDIO_ROOT`, and verifies the exact pinned revision/workflow contract. It then creates an ignored local package projection at `Preview/Unity/Packages/hero.studio` containing junctions to the pinned `hero.voxel` module and `hero.mathematics/Runtime` dependency plus a minimal package descriptor.

The projection keeps the package name `hero.studio`, so Hero.Voxel's own `PackageInfo.FindForAssembly` resolution and `Tool~` lookup continue to operate unchanged. It includes only `hero.voxel`, `hero.mathematics/Runtime`, and the pinned package-level `NLog` / `YamlDotNet` binaries directly referenced by Hero.Voxel Editor code; unrelated Hero Studio modules and third-party dependencies remain excluded.

The projection is derived and gitignored. No absolute provider path is committed. `Packages/manifest.json` references only the portable `file:Packages/hero.studio` projection and the Unity Test Framework needed by the preview project's own EditMode tests; Burst/Collections/Mathematics are declared by the projected package.

On Windows the bootstrap uses directory junctions for the pinned module directories. The checked-in repository stores only the provider contract and bootstrap logic, never resolved machine paths or provider source bytes.

**Alternative considered:** link the entire `Packages/hero.studio` package. Rejected after integration testing showed that it unnecessarily resolves unrelated Hero Studio UI/config/third-party dependencies for a workspace that only consumes Hero.Voxel.

**Alternative considered:** a UPM Git dependency pinned directly to the Hero Studio commit. Rejected because it introduces a second remote checkout/download path even when the repository already has an explicitly configured local provider.

**Alternative considered:** copy Hero.Voxel code into voxel-store. Rejected because that forks provider code and creates drift from `toolchain/hero-voxel.lock.yaml`.
### 3. Add a read-only store Tool command that emits the preview plan

Add a deterministic Tool command such as:

```
dotnet run --project Tool/VoxelStore.Tool -- preview-plan --root <store-root>
```

The command evaluates current store state without writing catalog/health outputs and emits only the published units needed by the Unity adapter. Each entry contains at least:

```
id
kind
unitRef
sourceBase
palette
```

Paths are repository-relative and come from the store's already-parsed authoritative asset records. Units are sorted deterministically by kind then stable asset id.

This command is the only bridge from store publication semantics into Unity. The Unity project does not scan `asset.yaml`, interpret publication gates, or use existing baked assets to infer membership.

**Alternative considered:** parse `Catalog/catalog.generated.json` from Unity. Rejected because generated Catalog is explicitly non-authoritative and does not provide the complete source mapping required for baking.

**Alternative considered:** teach Hero.Voxel's generic folder importer about voxel-store manifests. Rejected because that moves product-store semantics into a reusable provider.

### 4. Use one primary Editor command: Refresh All & Open Grid

Add a voxel-store Editor command, conceptually:

`Voxel Store/Refresh All & Open Preview Grid`

The command performs this orchestration:

1. Resolve the repository root.
2. Execute the Tool's read-only preview plan.
3. Fail before mutation if the preview plan or pinned Hero.Voxel provider cannot be resolved.
4. For each planned unit, consume the publication-validated generated paths emitted by `preview-plan` and call the pinned Hero.Voxel `VoxelAssemblyBaker` / `VoxelAnimationBaker` directly using:
   - the current `.generated.vox` beside authoritative source,
   - the current generated animation manifest when present,
   - semantic `UnitRef`,
   - a derived output directory under the preview project's `Assets/`.
5. Delete only stale unit directories below the dedicated derived baked root.
6. Open the tracked canonical scene shell.
7. Reconstruct the transient grid from the newly baked assembly/animation assets.
8. Select/frame the generated grid in Scene View.

The preview intentionally does **not** invoke `VoxelArtPipeline.Run`: publication already verifies deterministic generated geometry/animation are current, while that higher-level pipeline may recompile and rewrite those store-side generated artifacts on a cache miss. Direct Hero.Voxel bakers preserve the read-only preview boundary.
Per-unit Hero.Voxel source/Inspector/Scene Debug tools remain available for focused inspection after the all-units refresh.

**Alternative considered:** automatically bake whenever Unity opens. Rejected because opening an Editor project should not unexpectedly perform a potentially expensive multi-asset mutation.

### 5. Keep baked assets under one disposable Unity root

Use a dedicated derived root such as:

```
Preview/Unity/Assets/__VoxelStorePreview/Baked/
  Avatars/<asset-id>/
  Equipment/<asset-id>/
```

and captures/reports under:

```
Preview/Unity/Library/VoxelStorePreview/
```

The baked root and its `.meta` files are ignored. Refresh may delete/recreate content only below that root. The tracked scene and Editor integration must never depend on stable GUIDs from these ignored outputs.

This prevents regenerated Unity assets from becoming a second source of truth and avoids repository churn when model source changes.

### 6. Make the canonical scene a tracked shell with a transient generated grid

Track one scene:

`Preview/Unity/Assets/Preview/VoxelStorePreview.unity`

The scene contains no serialized references to ignored baked assets. The refresh command creates the grid after opening the scene and marks the generated hierarchy as Editor-only/transient so it is not the durable scene contract.

Each unit is instantiated through Hero.Voxel's existing `VoxelSceneDebugBuilder`; voxel-store does not reconstruct part transforms or meshes itself. If a matching `VoxelAnimationAsset` exists, the instance is registered with Hero.Voxel's Scene Debug controller so selection exposes canonical animation preview controls.

This avoids broken scene GUID references on a fresh checkout and preserves Hero.Voxel as the assembly/animation semantics owner.

### 7. Compute layout from the instantiated baked bounds, not source metadata

The grid builder first creates each canonical Hero.Voxel Scene Debug instance at the origin, then measures its actual child `Renderer.bounds`. It uses those rendered bounds only for spatial layout.

Layout rules:
- deterministic row-major order: avatars first, then equipment; stable asset id within each kind;
- columns = `ceil(sqrt(unitCount))`, yielding a 5x5 grid for the current 25-unit published set;
- one shared cell width/depth based on the largest instantiated bounds plus a fixed margin;
- each unit remains at scale 1;
- each unit is translated so its bounds sit on the common ground plane and its bounds center aligns to the cell center.

Measuring the canonical instantiated hierarchy avoids duplicating Hero.Voxel bind/pivot/assembly calculations while guaranteeing non-overlap.

**Alternative considered:** normalize every model to the same preview size. Rejected because it destroys the relative-size evidence the scene is intended to provide.

### 8. Refresh is exact-set and fail-safe

A successful refresh is authoritative only for preview state:
- one planned published asset -> one baked grid entry;
- no extra stale entries;
- failures are reported with asset id and Hero.Voxel stage;
- if any bake fails, the command reports failure rather than presenting a newly rebuilt grid as complete.

Stale output cleanup occurs only after a valid preview plan is obtained and is path-guarded to the dedicated derived baked root.

The existing repository may remain globally unhealthy for an unrelated unpublished asset; that does not prevent previewing other assets whose individual `Published` state is true.

### 9. Tests separate store-plan correctness, layout correctness, and integration proof

Add .NET Tool tests for:
- preview plan includes every and only published asset;
- identity/source/palette mapping comes from authoritative asset records;
- deterministic ordering;
- no write to generated catalog/health files.

Add Unity EditMode tests for:
- plan JSON parsing;
- derived path guarding/stale cleanup;
- deterministic grid placement from synthetic bounds;
- generated entry count and identity order;
- generated objects remain Editor-only/transient.

Add one integration validation that runs against the current repository, bakes the current published set through the pinned provider, opens the preview scene, and proves grid entry count equals preview-plan count.

## Risks / Trade-offs

- **[Full refresh can be slow as the catalog grows]** → Hero.Voxel pipeline caching/reuse remains active per unit; the first implementation favors correctness over adding another incremental cache.
- **[Local provider link can become stale after a Hero Studio checkout changes]** → bootstrap verifies the exact locked revision before Unity preview use and fails closed on mismatch.
- **[Ignored baked assets receive new Unity GUIDs on another checkout]** → the tracked scene never serializes references to them; the grid is rebuilt transiently after refresh.
- **[A very large asset can make shared grid cells spacious]** → preserving real scale and non-overlap is intentional; focused Hero.Voxel Inspector/Scene Debug preview remains available for detail work.
- **[Generated Scene Debug objects could dirty the tracked scene]** → generated roots are transient/Editor-only and must not be saved as scene authority; tests cover this ownership boundary.
- **[Store-wide scan may report unrelated unhealthy assets]** → preview membership uses each asset's derived `Published` value and does not require the whole store's `Healthy` flag to be true.

## Migration Plan

1. Add the read-only Tool preview-plan contract and tests.
2. Add the Unity preview project shell and provider bootstrap.
3. Add Editor refresh/bake orchestration and derived-output cleanup.
4. Add the canonical scene shell and transient grid builder.
5. Add Unity EditMode tests and current-store integration proof.
6. Update repository workflow documentation only where needed to explain how to bootstrap and open the preview workspace.

Rollback is deletion of `Preview/Unity/` plus the preview-plan command/tests; authoritative voxel assets and publication behavior remain unchanged.
