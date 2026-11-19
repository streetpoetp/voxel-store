# Spec Delta

## MODIFIED Requirements

### Requirement: Assets resolve reusable style authority independently of collections
Each asset SHALL declare a reusable style family that resolves from repository-owned `Styles/**` authority. Collection identity SHALL NOT be used as a substitute for visual style identity, and the store SHALL NOT require every asset to use a foundation-specific style family merely because one collection is active.

#### Scenario: Combat collection uses low-resolution voxel assets
- **WHEN** an asset selected by `combat-foundation.v1` declares `low-resolution-voxel.v1`
- **THEN** its visual provenance resolves from that reusable style authority without requiring `combat-foundation.v1` to be the style id

#### Scenario: Store default style differs from an asset style
- **WHEN** a valid asset explicitly declares a different supported style from the store's authoring default
- **THEN** validation resolves the asset-declared style rather than rejecting it solely for differing from the default
