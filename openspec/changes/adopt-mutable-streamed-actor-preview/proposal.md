# Proposal

## Why

Hero Studio now provides production-shaped mutable streamed voxel actor composition, but voxel-store's runtime gallery still proves only the lower direct `VoxelActorInstance` composition path. The preview needs one bounded consumer proof that exercises the newly pinned streamed API with real baked store assets without replacing the existing gallery or becoming production runtime authority.

## What Changes

- Pin `Dependencies/hero-studio` / `toolchain/hero-voxel.lock.yaml` to the published mutable streamed actor provider.
- Add one preview-only streamed actor demo built from a current baked avatar plus deterministic equipment samples.
- Exercise root-only actor acquisition, two mutable attachments, handle-addressed animation plans, detach, transactional replacement preserving the handle, stale-plan rejection, and release/reacquire with no inherited runtime loadout.
- Add a registered command-driven graphical capture scenario for the mutable streamed actor checkpoints and socket/Mount grip validation.
- Preserve the existing fixed/direct runtime gallery and `avatar-motion-gallery` path unchanged as separate coverage.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `unity-baked-preview`: add bounded consumer verification for mutable streamed actor composition from real baked voxel-store assets.

## Impact

The change affects the Hero Studio submodule pin/lock, preview runtime/editor/capture code, preview assembly references, tests, registry metadata, and `WORKFLOW.md`. It does not change voxel source, asset identity, gameplay semantics, publication authority, or production build behavior, and it does not build a Player/APK.
