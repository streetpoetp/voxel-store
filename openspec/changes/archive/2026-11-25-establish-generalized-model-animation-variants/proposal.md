# Proposal

## Why

Reusable voxel models currently support one baseline clip per semantic motion plus limited side-specific attack variants, but they cannot publish multiple authored takes for general motions such as Idle, WalkForward, Hit, or Death. Generalized model-local animation variants let consumer games choose deterministic presentation diversity without moving gameplay state, randomness, or animation vocabulary into Hero Studio.

## What Changes

- Extend reusable model animation naming so every semantic motion can optionally publish model-local authored variants such as `Idle.V1`, `WalkForward.V2`, `Hit.V1`, and `Death.V3`.
- Preserve physical side context as an independent dimension, allowing qualified clips such as `Attack.Left.V2` without turning side or variant identifiers into new semantic motions.
- Define ordered fallback for a requested motion, side, and variant: `<Motion>.<Side>.<Variant>` -> `<Motion>.<Variant>` -> `<Motion>.<Side>` -> `<Motion>`.
- Keep baseline semantic clips mandatory and retain their loop policy; optional variants inherit the loop/non-loop policy of their semantic motion.
- Treat variant identifiers as model-local authored take keys with no cross-model gameplay meaning or required aligned counts.
- Allow consumers to select variants explicitly per model instance; voxel-store and Hero Studio do not randomly or semantically choose variants.
- Project structured variant coverage in generated catalog/health output instead of treating qualified clip names as independent semantic motions.
- Extend Unity preview and command-driven proof so generalized variants can be selected and visually verified while reusing the existing Hero.Voxel.Actor opaque candidate-resolution API.
- Keep Hero Studio / `hero.voxel.actor` unchanged unless implementation proves a missing generic foundation capability.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-publication`: reusable animation publication gains optional generalized model-local variants, validation rules, loop-policy inheritance, and structured variant coverage projection.
- `asset-compatibility`: animation resolution gains an explicit variant dimension independent from side, with deterministic ordered fallback and per-model-instance selection.
- `unity-baked-preview`: preview selection, playback, and graphical proof gain variant-aware motion requests and coverage for generalized semantic-motion variants.

## Impact

Primary impact is in voxel-store validation/catalog code, animation-facing tests, preview request/candidate construction, preview controls, and capture scenarios. Existing baseline clip names and side-specific clips remain valid. Existing Hero.Voxel.Actor APIs already accept ordered opaque clip candidates, so no shared foundation API change is planned. Consumer games gain the ability to request a specific model-local variant but remain responsible for deciding when and why to choose it.
