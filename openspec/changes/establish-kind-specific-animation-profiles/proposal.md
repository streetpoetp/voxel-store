# Proposal

## Why

The universal Idle/WalkForward/Attack/Hit/Death contract forces equipment to perform meaningless motions and produces repetitive, physically implausible animation across the dungeon models. Replace it with reusable kind/form-specific motion profiles and four genuinely distinct authored takes per applicable family, and migrate the current catalog and future authoring workflow together.

## What Changes

- Introduce a single store-owned animation profile policy, explicit per-model profile selection, profile-defined motion families and loop rules.
- Require every authored motion family to have a default and V1, V2, V3; reject placeholder, irrelevant and duplicate choreography. Preserve model-local optional side qualifiers without making side attacks universal.
- **BREAKING**: equipment uses Idle, Attack and Drop instead of mandatory WalkForward, Hit and Death; floating/winged avatars use Glide/Fly instead of WalkForward. Remove obsolete clips rather than synthesizing aliases. Existing consumers must select applicable exact clip names.
- Replace shared-motion compatibility sampling with independent body/equipment clip and phase scenarios, covering every authored take, including release, return and terminal poses.
- Project resolved coverage and proof cases from Tool to Unity; keep normal preview published-only and support explicit unpublished authoring candidates before sealing.
- Synchronize specs, AGENTS, WORKFLOW and existing skills; upgrade all 11 avatars and 14 equipment models through the existing one-asset pipeline, retaining current geometry, identity, palette, VPU, sockets and Mount.
- Regenerate provider outputs and exact-input evidence; require actual visual acceptance and healthy publication of all 25 models. No new runtime, physics, skeleton, gameplay API or APK.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-publication`: profile-specific families, coverage, choreography and sealed animation evidence.
- `asset-compatibility`: independent motion requests and compatible sample pairs.
- `unity-baked-preview`: profile-driven coverage, candidate review, all-take visual capture.
- `asset-authoring-pipeline`: profile authority and animation-only migration with current evidence.
- `store-maintenance`: store-owned policy and bounded catalog migration.

## Impact

Store policy, manifests, authored VoxelSpecs/effective animation sources, Tool contracts/validation/health/preview-plan/compatibility/tests, preview Editor/capture/tests, workflow/skills and generated evidence/catalog. Reuse pinned Hero Studio compiler, exact-name clip resolution, Actor and Inspector unchanged. The optional typed VoxelStandardMotion convenience API does not enumerate all new names; extending it is out of scope.
