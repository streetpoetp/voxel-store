# Proposal

## Why

voxel-store can prove authored voxel geometry through generated image evidence, but it currently has no Unity workspace that lets a developer inspect the actual baked `VoxelAssemblyAsset` / `VoxelAnimationAsset` outputs that consuming games use. A thin preview workspace will close that inspection gap without turning voxel-store into a runtime owner or introducing a second voxel pipeline.

## What Changes

- Add a repository-owned Unity Editor preview workspace under `Preview/Unity/`, aligned to the Unity version and pinned Hero.Voxel contract used by the store.
- Resolve Hero.Voxel through the existing pinned provider contract; do not commit a developer-local Hero Studio path.
- Bake voxel-store sources from their existing authoritative `Assets/**/Source/**` locations into derived Unity assets under the preview workspace.
- Keep all baked Unity assets disposable/derived and excluded from source authority and publication authority.
- Add one canonical preview scene that automatically lays out **every currently published voxel unit** in a stable grid, including both avatars and equipment.
- Rebuild the grid from current published store state so additions/removals cannot leave a manually curated scene out of sync.
- Reuse Hero.Voxel's existing assembly/animation assets, Editor preview, Scene Debug construction, and animation semantics; do not add a voxel parser, mesher, runtime asset format, or alternate animation implementation.
- Provide minimal voxel-store Editor UX for rebuilding baked previews and refreshing/opening the all-units grid.

## Capabilities

### New Capabilities
- `unity-baked-preview`: Defines the non-authoritative Unity preview workspace, Hero.Voxel delegation boundary, derived bake lifecycle, and canonical all-published-units grid scene.

### Modified Capabilities

None. Asset publication and maintenance authority remain unchanged; the preview workspace consumes their published-state result without becoming a publication gate or source of truth.

## Impact

- Adds `Preview/Unity/**` project configuration, Editor-only preview integration, scene/layout code, and tests.
- Adds/updates ignore rules for Unity `Library`, transient capture data, and derived baked preview assets.
- Depends on the existing `toolchain/hero-voxel.lock.yaml` provider contract and pinned Hero.Voxel Unity Editor APIs.
- Reads voxel-store authoritative sources and current published-state evidence, but does not change asset manifests, VoxelSpec ownership, publication gates, runtime formats, or gameplay semantics.
