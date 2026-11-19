# Spec Delta

## ADDED Requirements

### Requirement: Deterministic Tool state routes maintenance by current gate
The maintenance workflow SHALL use deterministic scan/plan state to select one asset-scoped job and its earliest unresolved/current gate. The agent coordinator SHALL execute the owner of that gate and SHALL NOT manually advance to a later lifecycle stage while the earlier gate remains unresolved.

#### Scenario: Tool reports a reference gate
- **WHEN** deterministic maintenance state selects an asset whose earliest unresolved gate is reference
- **THEN** the coordinator executes the applicable reference owner for that same asset and does not begin density, source, proof, or publication work first

#### Scenario: Current gate passes
- **WHEN** the active asset's current gate is repaired and deterministic validation passes
- **THEN** maintenance re-derives state for that same asset and follows the newly reported next gate rather than relying on a manually maintained stage sequence

#### Scenario: Current gate remains blocked
- **WHEN** the active gate cannot safely advance because required authority, evidence, dependency, or visual acceptance is unavailable
- **THEN** maintenance reports that gate and its blocking evidence and stops without selecting another asset

### Requirement: Human-facing lifecycle groups gates without replacing them
The repository MAY present maintenance as the high-level lifecycle `Plan -> Reference -> Source -> Proof -> Publish`, but these lifecycle stages SHALL remain navigation groupings over existing deterministic gates and SHALL NOT redefine or bypass the detailed validation contracts owned by the Tool, OpenSpec specifications, policy, or Hero.Voxel.

#### Scenario: Workflow groups Product Target and construction sheets under Reference
- **WHEN** human-facing documentation describes Product Target, Composition, and Part work as the Reference stage
- **THEN** the underlying target-fidelity, style-authority, provenance, and Hero.Voxel construction requirements remain independently authoritative

#### Scenario: Workflow groups density and VoxelSpec under Source
- **WHEN** human-facing documentation describes density selection and VoxelSpec authoring as the Source stage
- **THEN** existing reference-density-selection and source validation contracts remain unchanged and determine whether that work may advance
