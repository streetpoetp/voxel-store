# Proposal

## Why

`voxel-store` is defined as a source-authoritative voxel content library that does not own a voxel runtime, but its Unity preview currently ships a second runtime/model layer around Hero Studio for render instances, actor composition, animation playback, streamed topology, and model conventions. This duplicates provider responsibilities, expands maintenance surface, and makes store-specific proof code look like reusable runtime architecture.

## What Changes

- **BREAKING** Remove the store-owned `Preview/Runtime` layer, including store wrappers for render presentation, avatar/equipment composition, animation playback, streamed actor proof state, motion vocabulary, and model-frame conventions.
- Replace store-owned runtime wrappers with direct use of the pinned Hero Studio public APIs from Editor-only preview and capture orchestration.
- Keep voxel-store ownership limited to authored content, metadata/catalog/publication, Editor asset discovery, proof-case selection, deterministic capture checkpoints, and visual acceptance evidence.
- Remove store tests whose only purpose is to re-prove Hero Studio runtime mechanics such as animation-track lifecycle, streamed attachment topology, handle staleness, warm reacquire, and pool behavior.
- Retain integration smoke coverage that proves published store assets can be baked, discovered, composed through Hero Studio, and captured successfully.
- Simplify or remove store-owned motion preview UI where Hero Studio already provides animation inspection, clip selection, scrubbing, and scene-debug preview.
- Do not modify Hero Studio as part of this change. Any future provider change must be justified independently as a generic Hero Studio requirement, not as a destination for deleted voxel-store helpers.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `unity-baked-preview`: Replace runtime-gallery/runtime-controller requirements with Editor-only baked-content inspection, store-wide clip/socket validation, direct Hero Studio integration proof, and deterministic graphical evidence. Remove requirements that make voxel-store responsible for reusable runtime playback or streamed topology behavior.

## Impact

- Deletes `Preview/Unity/Assets/Preview/Runtime/**` and its runtime assembly.
- Refactors `Preview/Unity/Assets/Preview/Editor/**`, `Capture.Editor/**`, and related asmdefs to consume Hero Studio directly.
- Removes or rewrites `AvatarPresentationTests`, runtime-wrapper tests, and PlayMode tests that duplicate provider contracts; keeps store/provider integration validation.
- Updates the generated runtime-gallery workflow and `unity-baked-preview` specification to describe Editor proof orchestration rather than a store-owned runtime surface.
- No Hero Studio source, API, package, or submodule revision change is in scope.
