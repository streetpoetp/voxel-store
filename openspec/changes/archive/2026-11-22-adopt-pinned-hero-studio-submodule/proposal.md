# Proposal

## Why

The current Hero Studio integration has two competing provider paths: Tool commands depend on a machine-local `HERO_STUDIO_ROOT`, while the Unity preview builds a selective local projection of Hero.Voxel. That split has already caused provider drift, fixture-import errors, and render-pipeline mismatch. Voxel-store should consume one exact Hero Studio checkout for both CLI and Unity.

## What Changes

- Add `Dependencies/hero-studio` as a Git submodule pinned to latest `origin/main` commit `363e590268ec28a32a09e1805d0772d79277aecb`.
- **BREAKING** Remove production dependence on `HERO_STUDIO_ROOT` and selective Hero.Voxel/provider projections.
- Make the Tool resolve Hero.Voxel from the repository submodule and keep `toolchain/hero-voxel.lock.yaml` aligned to the same Hero Studio revision/workflow fingerprint.
- Make `Preview/Unity` consume the complete `hero.studio` Unity package directly from the submodule.
- Configure the preview workspace for the render pipeline required by the full Hero Studio package instead of patching Hero.Voxel shaders locally.
- Keep temp-store tests deterministic through explicit test-only provider-root injection rather than environment variables.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `store-maintenance`: tool dependency resolution becomes repository-local through the pinned Hero Studio submodule rather than a machine-local provider root.
- `unity-baked-preview`: preview provider resolution consumes the complete pinned Hero Studio package from the repository submodule instead of a selective generated projection.

## Impact

- Git dependency metadata: `.gitmodules`, `Dependencies/hero-studio`.
- Tool provider/build wiring: `Tool/Directory.Build.props`, `VoxelStore.Tool.csproj`, `HeroVoxelProvider`, tests.
- Provider lock: `toolchain/hero-voxel.lock.yaml`.
- Unity preview package/render-pipeline configuration and Editor launch path.
- Repository workflow documentation and existing preview bootstrap logic.
