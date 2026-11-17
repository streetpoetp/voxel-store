# Spec Delta

## MODIFIED Requirements

### Requirement: Reference evidence follows the canonical construction contract
Agent-authored assets SHALL use visually construction-authoritative Hero.Voxel PNG sheets: one explicitly declared 3:2 Composition Sheet freezes `RequiredParts`, then exactly one explicitly declared Part Sheet PNG covers each required part before geometry authoring is accepted. Acceptance SHALL also be current against the asset's active style authority.

#### Scenario: Reference coverage is complete
- **WHEN** the accepted 3:2 Composition Sheet PNG defines `RequiredParts`, `reference.yaml` explicitly maps that Composition Sheet and exactly one accepted Part Sheet PNG for every required part, and the package is accepted against current style authority
- **THEN** the reference-package coverage gate passes and VoxelSpec geometry may qualify for later publication gates

#### Scenario: Part coverage is incomplete or ambiguous
- **WHEN** any required part has no accepted Part Sheet, multiple accepted Part Sheets, a sheet claims a part outside `RequiredParts`, or the declared mapping is missing/ambiguous
- **THEN** the reference-package coverage gate fails and the asset cannot publish

#### Scenario: Schematic placeholder satisfies filenames but not construction authority
- **WHEN** a declared Composition or Part Sheet is not a valid PNG or is only a schematic/placeholder that does not provide the canonical Hero.Voxel visual construction authority required for that sheet
- **THEN** visual reference acceptance fails even if the file exists and its hash is sealed

#### Scenario: Reference package was accepted under stale style authority
- **WHEN** the active style family or any concrete style-reference input changes after the package was accepted
- **THEN** the reference-package gate fails until the package is visually re-audited against the current style authority
