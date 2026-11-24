# Proposal

## Why

Voxel Store still realizes and synchronizes voxel models through local preview MonoBehaviours even though Hero Studio now provides the reusable actor and render mechanisms. Adoption will make the gallery exercise the same technology as downstream games and remove competing presentation implementations.

## What Changes

- Upgrade the repository-pinned complete Hero Studio package to a published revision containing the shared actor foundation; synchronize the gitlink, lock revision and workflow hash.
- Replace runtime gallery per-part GameObject realization and local pose/attachment evaluation with VoxelRenderWorld, VoxelRenderInstance and VoxelActorInstance.
- Keep catalog selection, semantic clip candidates, driver selection, authored model frame, gallery layout and interactive preview controls consumer-owned.
- Preserve deterministic capture, loop/speed controls, reset and repeated equipment behavior; resolve provider contract gaps at their owning boundary before adoption is accepted.
- Reconcile contradictory legacy Body-ratio/procedural and authored socket/clip preview requirements with the approved bind-anchor actor contract. Do not weaken visual acceptance or silently accept anchor drift.
- Preserve existing asset content, imported catalogs, publication authority and unrelated working-tree changes.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `unity-baked-preview`: canonical provider-backed runtime gallery, authored socket/animation composition, playback controls and proof through the shared render/actor path.

## Impact

Preview runtime, gallery builder, editor controls, capture runner, preview tests and assembly references under Preview/Unity/Assets/Preview; dependency gitlink and toolchain/hero-voxel.lock.yaml; unity-baked-preview specification. Derived gallery output may be regenerated. Hero Studio publication and any necessary generic provider fixes are prerequisites owned by the authoritative H:/projects/hero-studio repository, not consumer patches. No production game runtime, asset authoring changes, APK build, UI automation or provider source edits are in scope.
