# Spec Delta

## MODIFIED Requirements

### Requirement: One maintenance run owns one bounded asset job
An autonomous maintenance invocation SHALL select at most one asset-scoped design or repair job and delegate that same job into the shared asset-authoring pipeline at its authoritative current stage. It SHALL carry only that asset through applicable downstream stages and SHALL stop with explicit evidence when the selected job cannot safely advance.

#### Scenario: Healthy store has one highest-priority gap
- **WHEN** a scan finds no higher-priority failures and selects one uncovered required asset slot
- **THEN** the maintenance run establishes that one asset job and delegates it to the shared pipeline while leaving other uncovered slots for later runs

#### Scenario: Selected job fails a gate
- **WHEN** the active asset fails a reference, generation, visual, budget, compatibility, or publication gate
- **THEN** the run records the failing evidence and stops or repairs that same selected asset through the shared pipeline rather than silently skipping to another asset

#### Scenario: Maintenance selects an existing repair
- **WHEN** deterministic Tool state selects an existing asset and its earliest unresolved/current gate
- **THEN** maintenance enters the shared pipeline at the corresponding owning stage for that same asset without replaying already-current upstream authority

### Requirement: Human-facing lifecycle groups gates without replacing them
The repository MAY present asset execution through the shared lifecycle `Intent -> Product Target -> Construction -> Density -> Source -> Generate -> Runtime Presentation Proof -> Compatibility -> Seal -> Publish`, with Density or Runtime Presentation Proof explicitly not applicable when their owning contracts allow. Maintenance Plan remains the deterministic job-selection preface. These labels SHALL NOT redefine or bypass Tool, OpenSpec, policy, preview, or Hero.Voxel validation contracts.

#### Scenario: Workflow groups Product Target and construction sheets under Reference
- **WHEN** human-facing documentation describes Product Target, Composition, and Part work through the shared Product Target and Construction stages
- **THEN** the underlying target-fidelity, style-authority, provenance, and Hero.Voxel construction requirements remain independently authoritative

#### Scenario: Workflow groups density and VoxelSpec under Source
- **WHEN** human-facing documentation describes density selection before Source and VoxelSpec authoring within Source
- **THEN** existing reference-density-selection and source validation contracts remain unchanged and determine whether that work may advance

#### Scenario: Workflow reaches runtime presentation proof
- **WHEN** current proof or compatibility contracts require provider-backed runtime presentation evidence after generated source is current
- **THEN** Runtime Presentation Proof runs as derived downstream evidence and does not become source, compatibility, or publication authority

#### Scenario: A pipeline stage is not applicable
- **WHEN** the owning contract explicitly does not require Density or Runtime Presentation Proof for the active asset
- **THEN** the stage is recorded as not applicable and the pipeline advances without inventing new evidence requirements
