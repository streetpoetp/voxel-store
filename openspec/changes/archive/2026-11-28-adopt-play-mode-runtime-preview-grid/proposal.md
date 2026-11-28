# Proposal

## Why

Voxel-store's canonical interactive preview grid still realizes baked content through Hero.Voxel Edit Mode Scene Debug hierarchies and manually composes carried equipment in the scene hierarchy. That duplicates a presentation path beside the real VoxelRenderWorld and VoxelActorInstance path already used by the store's graphical capture proof, and it prevents the new provider-owned Play Mode runtime Inspector from being the single interactive animation-inspection surface.

Hero Studio now provides component-free Play Mode runtime inspection over an existing VoxelRenderInstance, canonical VoxelModelAnimationTrack, and optional VoxelActorInstance. Voxel-store should adopt that provider capability rather than maintaining an Edit Mode grid-specific playback/composition path.

## What Changes

- Advance the repository-owned Hero Studio gitlink to the provider revision that contains Play Mode runtime inspection. Preserve the existing semantic Hero.Voxel lock identity and workflow fingerprint because this Editor-only capability does not change Tool/workflow semantics.
- Keep preview-plan discovery, baking, baked asset paths, deterministic asset/equipment selection, grid layout, capture scenarios, and visual acceptance store-owned.
- Change Voxel Store/Refresh Preview Grid from building a persistent Edit Mode Scene Debug grid to bake current models, open the disposable preview shell, enter Play Mode, realize the grid through one provider VoxelRenderWorld, compose avatar equipment through VoxelActorInstance, and register plain hierarchy selection hosts with Hero Studio's shared runtime Inspector.
- Give avatar root, carried-equipment, and standalone-equipment selection hosts access to the exact canonical model track they represent without adding voxel-store preview/player MonoBehaviour state.
- Remove interactive-grid reliance on VoxelSceneDebugBuilder and VoxelSceneDebugController; retain Hero.Voxel Scene Debug only as the provider's separate direct baked-asset/Edit Mode inspection path.
- Remove or replace obsolete SceneView-grid batch helpers that assume the canonical grid is an Edit Mode mesh hierarchy.
- Preserve the existing provider-attested avatar-motion-gallery capture path as a separate graphical content proof; do not merge its checkpoint semantics into the interactive grid.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- unity-baked-preview: make the canonical interactive grid a Play Mode realization of current baked content through the pinned Hero.Voxel runtime and shared provider Inspector while preserving Editor-only ownership and no voxel-store runtime surface.

## Impact

- Dependencies/hero-studio advances to the published Hero Studio revision containing the Actor Editor Play Mode runtime inspection capability.
- toolchain/hero-voxel.lock.yaml remains unchanged after verification: the repository already permits the Hero Studio gitlink to advance beyond the semantic lock identity when Tool/workflow semantics are unchanged.
- Preview/Unity/Assets/Preview/Editor/VoxelStorePreview.cs changes interactive grid creation from Scene Debug hierarchy creation to Play Mode/runtime orchestration.
- Preview/Unity/Assets/Preview/Editor/VoxelStore.Preview.Editor.asmdef consumes the provider Actor runtime and Actor Editor inspection assemblies directly.
- Preview/Unity/Assets/Preview/Editor.Tests/VoxelStorePreviewTests.cs gains Play Mode runtime-grid/inspection proof and drops assumptions that the canonical grid is a Scene Debug mesh hierarchy.
- WORKFLOW.md documents Play Mode as the canonical interactive grid path and keeps Scene Debug as direct baked-asset/Edit Mode tooling only.
- No asset source, identity, publication, gameplay semantics, reusable runtime API, RPC/protocol, or APK/Player build is introduced.