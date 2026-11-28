# Proposal

## Why

Voxel-store's reusable content structure is generic (`Assets/Avatars/Wraith01`, `Assets/Equipment/Rifle02`), but published asset identities still embed legacy collection/origin namespaces such as `dungeon` and `foundation` and use inconsistent numeric separators. This makes model identity harder to predict, leaks obsolete ownership concepts into reusable content, and allows folder/model/id/UnitRef naming to drift independently.

## What Changes

- Define one mechanical identity convention from the canonical model directory name:
  - `Assets/<Catalog>/<ModelName>/`
  - `source.base = ./Source/<ModelName>`
  - deterministic lowercase kebab `ModelKey` derived from `<ModelName>`
  - `id = <kind>.<ModelKey>`
  - `unitRef = voxel-store/<kind>/<ModelKey>`
- Require active avatar/equipment manifests to satisfy that convention at validation/publication time instead of accepting arbitrary non-empty ids and UnitRefs.
- **BREAKING**: perform one explicit identity migration for all 25 currently published assets, removing legacy `dungeon`, `foundation`, `humanoid`, dotted numeric suffixes, and other non-structural identity segments.
- Reserve every migrated legacy id/UnitRef in `Store/retired-assets.yaml` so old published identities cannot be reused; do not keep runtime/store aliases.
- Regenerate or migrate identity-bearing compatibility evidence, catalog/health state, preview-plan output, and other derived projections from the new canonical identities rather than treating generated files as authority.
- Preserve model content, model directory names, Hero.Voxel source names, reference/proof visual content, forms, animations, sockets, and publication semantics except where identity-bearing evidence must be regenerated.
- Preserve the existing rule that later repository reorganization does not silently change an already-published identity; structural derivation is the canonical creation/migration convention, not permission for automatic renames after arbitrary path moves.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-publication`: define the canonical ModelName/ModelKey/id/UnitRef relationship, require active manifests to conform, and define the explicit migration/reservation behavior for the existing published identities.

## Impact

- `Assets/**/asset.yaml`: all 25 active ids and UnitRefs migrate while folder/source model names remain unchanged.
- `Store/retired-assets.yaml`: gains the 25 legacy identity pairs as reserved history in addition to existing retired identities.
- `Tool/VoxelStore.Tool`: manifest validation gains deterministic path/model/identity checks and a shared ModelName-to-ModelKey rule.
- `Tool/VoxelStore.Tool.Tests`: gains positive/negative convention coverage and migration/retired-identity tests.
- Identity-bearing compatibility proof filenames/payloads and generated catalog/health outputs must be regenerated under the new ids; visual/model source bytes should remain unchanged.
- Consumer repositories or scripts that explicitly refer to old voxel-store asset ids/UnitRefs will need to update to the new identities; no compatibility alias layer is introduced.
