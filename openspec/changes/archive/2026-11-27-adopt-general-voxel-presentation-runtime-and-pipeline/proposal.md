# Proposal

## Why

Voxel-store currently duplicates generic voxel presentation behavior that is now shipped by the pinned Hero Studio provider: direct attachment reference-influence composition, streamed attachment visual state, and low-level actor lifetime glue live inside the preview even though voxel-store explicitly does not own a voxel runtime. Its authoring skills also expose overlapping end-to-end flows instead of one durable pipeline that coordinates existing stage authorities.

## What Changes

- Advance the repository-owned Hero Studio submodule and provider lock to a revision that contains `hero.voxel.presentation` and `hero.voxel.presentation.streaming`, preserving the pinned-submodule dependency contract.
- Migrate preview avatar composition from direct `VoxelActorInstance` plus store-owned reference-influence solving to provider-owned `VoxelPresentation`; keep voxel-store semantic choices such as `Body`, equipment side/socket selection, `Mount`, and motion vocabulary local.
- Migrate the mutable streamed proof from `VoxelActorPresenter` plus a store-owned handle-to-resolver dictionary to `VoxelStreamedPresentation`, preserving observable attach/rebind/detach/replace/reacquire, independent playback, release/recovery, bounds, and capture evidence while removing assertions that depend on hidden actor topology revision or consumer-owned resolver state.
- Remove `AttachmentReferenceInfluenceResolver` and any preview/runtime code whose only purpose was duplicating provider presentation composition.
- Update preview assemblies, Editor/PlayMode tests, capture runner evidence, runtime-gallery documentation, and provider compatibility evidence to verify the new presentation boundary without weakening visual or lifecycle proof.
- Add a top-level `voxel-store-pipeline` skill that coordinates one asset through `intent -> Product Target -> construction -> density when applicable -> source -> generate -> runtime presentation proof -> compatibility -> seal -> publish`, delegating each stage to existing Tool/spec/skill authorities.
- Converge `voxel-store-create-model`, `voxel-store-maintain`, and `WORKFLOW.md` onto that pipeline so direct creation requests retain a compatibility entrypoint but no second competing end-to-end stage definition exists.

## Capabilities

### New Capabilities
- `asset-authoring-pipeline`: Defines the single one-asset authoring pipeline, stage delegation, gate re-evaluation, applicability/no-op behavior, and compatibility entrypoints for creation and maintenance.

### Modified Capabilities
- `unity-baked-preview`: Requires runtime gallery and mutable streamed proof to consume provider-owned general presentation composition instead of a voxel-store attachment solver or direct actor-topology proof surface.
- `store-maintenance`: Routes new production creation through the shared asset-authoring pipeline while retaining deterministic Tool gate priority, one-asset ownership, and fail-closed maintenance behavior.

## Impact

- Dependency state: `Dependencies/hero-studio`, `toolchain/hero-voxel.lock.yaml`.
- Preview runtime: `AvatarPresentation`, `EquipmentPresentation`, `MutableStreamedActorPreview`, runtime assembly references, and removal of the local reference-influence resolver.
- Evidence: Avatar presentation tests, PlayMode tests, mutable streamed capture assertions/checkpoint logging, compatibility evidence, and runtime-gallery documentation.
- Agent workflow: new `.agents/skills/voxel-store-pipeline/SKILL.md`, plus focused updates to create-model, maintain, `WORKFLOW.md`, and any skill/workflow validation tests.
- No production game runtime, gameplay semantics, or APK/Player build is introduced.
