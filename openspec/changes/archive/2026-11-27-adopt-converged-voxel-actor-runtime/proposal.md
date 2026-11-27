# Proposal

## Why

Hero Studio has converged the public composite voxel runtime from the redundant Presentation layers into `hero.voxel.actor` and `hero.voxel.actor.streaming`, while voxel-store is still pinned to and documented against `VoxelPresentation` / `VoxelStreamedPresentation`. The preview consumer must adopt the published Actor boundary now so voxel-store remains compatible with its pinned provider and does not preserve a retired runtime abstraction.

## What Changes

- Upgrade the repository-owned Hero Studio submodule pin and `toolchain/hero-voxel.lock.yaml` to a published revision that contains the Presentation-to-Actor convergence.
- **BREAKING**: migrate direct composed preview ownership from `VoxelPresentation` / `VoxelPresentationAttachmentHandle` to the provider's `hero.voxel.actor` public surface and actor binding identity.
- **BREAKING**: migrate mutable streamed proof from `VoxelStreamedPresentation` / `VoxelStreamedPresentationAttachmentHandle` to `hero.voxel.actor.streaming` and its activation-scoped attachment handle.
- Remove dependencies, imports, terminology, and shadow assumptions tied to the retired Presentation packages while preserving voxel-store's existing preview semantics, authored animation tracks, reference-influence behavior, release/recovery proof, mutation proof, and capture scenarios.
- Update workflow/spec documentation so the provider boundary is explicit: Render owns one realized model, Actor owns one composed visual actor, and Actor.Streaming owns pooled/streamed composite lifecycle.
- Re-run provider pin verification, Tool/tests, runtime-gallery proof, mutable-streamed-actor proof, strict OpenSpec validation, and `git diff --check`. Do not build a Player or APK.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `unity-baked-preview`: replace the retired provider Presentation contract with the converged Actor / Actor.Streaming contract while preserving the preview's externally observable composition, animation, mutation, reset, and graphical-proof behavior.

## Impact

- `Dependencies/hero-studio` and `toolchain/hero-voxel.lock.yaml` provider revision/fingerprint.
- `Preview/Unity/Assets/Preview/Runtime/**`, especially `AvatarPresentation`, `EquipmentPresentation`, `MutableStreamedActorPreview`, and runtime assembly references.
- Preview tests/capture integration that assert direct and streamed composite behavior.
- `WORKFLOW.md` and the `unity-baked-preview` OpenSpec contract.
- No voxel-store asset-authoring authority, gameplay semantics, production game runtime, or APK/Player build scope changes.
