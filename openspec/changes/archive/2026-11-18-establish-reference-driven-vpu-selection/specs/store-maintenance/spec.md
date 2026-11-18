# Spec Delta

## ADDED Requirements

### Requirement: Maintenance resolves reference density before equipment source authoring
After a `density: reference-driven` equipment reference package is accepted, autonomous maintenance SHALL resolve current density evidence for that same asset before creating or repairing its VoxelSpec source. Density failure SHALL remain the active asset's blocking work. Legacy equipment without the opt-in keeps the previous maintenance flow until migrated.

#### Scenario: Equipment reference package becomes accepted
- **WHEN** maintenance completes visual acceptance for a `density: reference-driven` equipment Composition Sheet and every required Part Sheet
- **THEN** the next authoring step for that asset is density analysis/selection rather than immediate VoxelSpec authoring

#### Scenario: Legacy equipment has not opted into density selection
- **WHEN** maintenance encounters equipment without `density: reference-driven`
- **THEN** it preserves the previous reference-to-source maintenance sequence and does not require density evidence

#### Scenario: Density selection succeeds
- **WHEN** current density evidence selects a VPU for the active equipment asset
- **THEN** maintenance authors or repairs that asset's source using the selected VPU and continues through ordinary generation/proof/publication gates

#### Scenario: Density selection fails
- **WHEN** no candidate VPU preserves all declared meaningful features within the hard constraints
- **THEN** maintenance records density as the blocking gate for that same asset and does not silently choose a different VPU or advance to another asset

#### Scenario: Existing equipment reference changes
- **WHEN** a published equipment Part Sheet changes and invalidates its previous density evidence
- **THEN** maintenance recomputes density before repairing source even if an older VoxelSpec remains compilable
