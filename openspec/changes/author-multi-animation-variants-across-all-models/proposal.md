# Proposal

## Why

The generalized animation-variant architecture is available, but the published store is not actually multi-animation: only 2 of 25 published models currently author V1 takes, while the other 23 still expose only their baseline semantic motions. The store needs a publication-level minimum variant contract so every published avatar and equipment model provides real authored motion diversity and the Unity Inspector reflects the clips owned by that model.

## What Changes

- Require every published avatar to author a second take for each reusable motion: `Idle.V1`, `WalkForward.V1`, `Hit.V1`, `Death.V1`, plus `Attack.Left.V1` and `Attack.Right.V1` alongside the existing baseline and side-reactive Attack clips.
- Require every published equipment model to author `Idle.V1`, `WalkForward.V1`, `Attack.V1`, `Hit.V1`, and `Death.V1` alongside its required baseline clips.
- Treat the V1 set as real authored animation coverage: a required V1 clip must not be an identical alias/copy of the corresponding baseline or side-only clip.
- Upgrade the current published inventory to the new minimum. The audited migration is 126 new clips: 61 avatar clips and 65 equipment clips across the 25-model baseline.
- Keep model-local variant semantics, loop inheritance, side/variant fallback, shared normalized phase, socket/Mount authority, and Hero.Voxel.Actor APIs unchanged.
- Strengthen generated catalog/health validation so a published reusable model cannot silently regress to baseline-only animation coverage.
- Make `AvatarMotionPlayer` Inspector motion choices come directly from the selected avatar's baked `VoxelAnimationAsset`; the Inspector does not maintain a hard-coded motion/variant option list.
- Regenerate generated animation manifests, proof/audit artifacts, catalog/health state, and Unity baked preview assets after authoring.
- Verify every model's minimum coverage mechanically and verify authored differences through generated animation proof, while retaining provider-attested Game View capture as the runtime integration proof path.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-publication`: reusable avatar and equipment publication now requires a minimum authored V1 variant set, non-identical authored takes, and generated coverage that proves the minimum for every published model.
- `unity-baked-preview`: the component Inspector discovers exact motion clips from each model's baked animation asset, and preview verification must expose the store-wide required V1 coverage without a hard-coded option list.

## Impact

The change affects authoritative `Assets/Avatars/**/Source/*.voxel.yaml` and `Assets/Equipment/**/Source/*.voxel.yaml` animation authoring for the current published baseline, StoreEngine validation/catalog projection and tests, generated animation/proof/catalog artifacts, Unity baked preview assets, `AvatarMotionPlayerEditor`, preview Editor tests, and animation proof workflow. It does not add gameplay semantics, random variant selection, new Hero Studio APIs, new avatar parts, attachment correction, or a production animation state machine.
