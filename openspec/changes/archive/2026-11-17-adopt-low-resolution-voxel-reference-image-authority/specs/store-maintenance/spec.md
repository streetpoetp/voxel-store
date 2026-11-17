# Spec Delta

## ADDED Requirements

### Requirement: Maintenance resolves style authority before reference generation
Before generating or repairing reference sheets, maintenance SHALL resolve the asset's active concrete style-reference set and the pinned Hero.Voxel construction workflow. It SHALL generate/audit the Composition Sheet first, then exactly one uncovered Part Sheet at a time, and SHALL stop on unresolved style authority or visual audit failure.

#### Scenario: Maintenance begins reference generation
- **WHEN** the selected asset job requires a new or repaired reference package and the active style-reference set resolves successfully
- **THEN** maintenance conditions generation on that style authority while following Hero.Voxel's canonical Composition-first sequential Part Sheet workflow

#### Scenario: Style authority cannot be resolved
- **WHEN** any required style-reference image is unavailable or the asset's style family cannot resolve to a concrete reference set
- **THEN** maintenance fails closed on the current asset before generating or accepting replacement construction sheets

#### Scenario: Generated sheet fails visual audit
- **WHEN** the Composition Sheet or current Part Sheet fails either the active style audit or canonical Hero.Voxel construction audit
- **THEN** maintenance repairs/regenerates that same sheet for the current asset and does not advance to another Part Sheet or asset
