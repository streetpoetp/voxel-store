# Tasks

## 1. Store Preview Plan

- [x] 1.1 Add a read-only `preview-plan` command and DTOs to `Tool/VoxelStore.Tool` that derive every and only currently published asset from authoritative store evaluation, include id/kind/UnitRef/source-base/palette, and sort by kind then stable asset id; verify focused Tool tests cover published inclusion, unpublished exclusion, both asset kinds, and deterministic ordering.
- [x] 1.2 Keep preview planning non-mutating and source-authoritative by returning repository-relative paths from parsed asset records without reading generated Catalog as authority; verify a Tool test snapshots catalog/health files before and after `preview-plan` and proves they are unchanged while source/identity mappings match the manifests used by the scan.

## 2. Unity Preview Workspace and Provider Bootstrap

- [x] 2.1 Add the minimal `Preview/Unity/` project shell using Unity `6000.3.24f1`, a package manifest compatible with the pinned Hero Studio package, and repository ignore rules for Unity-generated state plus `Assets/__VoxelStorePreview/Baked/**`; verify the tracked project contains no baked preview assets or machine-local absolute paths.
- [x] 2.2 Add a preview bootstrap command that reads `toolchain/hero-voxel.lock.yaml`, resolves `HERO_STUDIO_ROOT`, verifies the exact pinned Hero Studio revision/workflow contract, and creates an ignored local `Packages/hero.studio` provider projection containing only pinned Hero.Voxel, required Hero.Mathematics runtime, and provider-owned precompiled dependencies; verify the correct provider bootstraps successfully and a missing/mismatched provider fails without changing authoritative store files.
- [x] 2.3 Document the preview bootstrap/open workflow and the non-authoritative Unity boundary in `WORKFLOW.md`; verify every documented command/path matches the implemented project and no documentation instructs users to copy source assets or commit local provider paths.

## 3. Editor Refresh and Bake Orchestration

- [x] 3.1 Add Editor integration that locates the voxel-store root, executes `preview-plan`, parses its JSON contract, and reports Tool/provider errors before any bake mutation; verify Unity EditMode tests cover valid plans, empty plans, malformed results, and failed process/provider resolution.
- [x] 3.2 Implement per-unit refresh by consuming `preview-plan`'s publication-validated `.generated.vox` / generated-animation paths and calling pinned Hero.Voxel `VoxelAssemblyBaker` / `VoxelAnimationBaker` into the derived Unity output directory; verify Editor tests prove path mapping and an integration run produces canonical `VoxelAssemblyAsset` / `VoxelAnimationAsset` outputs without copying or rewriting voxel-store source.
- [x] 3.3 Implement exact-set cleanup limited to `Preview/Unity/Assets/__VoxelStorePreview/Baked/**`, removing stale unit directories only after a valid preview plan and refusing paths outside that root; verify EditMode tests cover stale removal, current-output preservation, and path-escape rejection.
- [x] 3.4 Make the primary `Voxel Store/Refresh All & Open Preview Grid` command fail closed when any planned unit fails to bake and avoid presenting a newly rebuilt grid as complete; verify an Editor test injects one failed unit and proves the command reports the asset/stage while withholding successful-grid completion.

## 4. Canonical All-Units Scene Grid

- [x] 4.1 Add the tracked shell scene `Preview/Unity/Assets/Preview/VoxelStorePreview.unity` and open it from the primary refresh command without serializing references to ignored baked assets; verify a fresh derived-output deletion leaves the tracked scene valid and free of broken baked GUID dependencies.
- [x] 4.2 Build each grid entry through Hero.Voxel `VoxelSceneDebugBuilder`, measure the instantiated child `Renderer.bounds`, and lay out all entries row-major in deterministic avatar-then-equipment/id order with `ceil(sqrt(count))` columns, shared non-overlap spacing, ground alignment, and scale 1; verify EditMode tests cover ordering, current 25-unit 5x5 layout math, non-overlap, and preservation of unequal authored sizes.
- [x] 4.3 Register available baked animations through Hero.Voxel's Scene Debug controller, keep generated grid/debug objects Editor-only and transient, and frame/select the completed grid in Scene View; verify EditMode tests prove animation association remains Hero.Voxel-owned and generated entries are not durable runtime/scene authority.
- [x] 4.4 Ensure each successful refresh produces exactly one generated scene entry for every preview-plan unit and zero stale entries, including the empty-store case; verify tests compare generated identity/order against synthetic preview plans containing avatars, equipment, removals, additions, and no units.

## 5. End-to-End Proof

- [x] 5.1 Run the Tool test suite and Unity EditMode suite, then bootstrap the preview workspace against the pinned Hero Studio revision and execute the primary refresh against the current voxel-store; verify all tests pass and the actual Scene grid entry count exactly equals the current `preview-plan` published-unit count.
- [x] 5.2 Inspect the resulting current-store grid in Scene View and verify all published avatars and equipment are visible together without overlap at authored relative scale, focused Hero.Voxel Inspector/Scene Debug preview still works, no unpublished asset appears, and `git diff --check` plus workspace hygiene show no generated Unity/provider artifacts accidentally tracked.