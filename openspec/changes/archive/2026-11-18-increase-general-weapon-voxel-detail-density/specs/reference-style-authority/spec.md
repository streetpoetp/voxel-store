# Spec Delta

## MODIFIED Requirements

### Requirement: Low-resolution voxel style is the V1 reference-image authority
The `low-resolution-voxel.v1` family SHALL preserve one shared low-resolution voxel grammar while providing avatar- and equipment-specific examples. Equipment examples SHALL communicate equipment silhouette, thickness/proportion language, readable grip or attachment regions, purposeful secondary construction detail, restrained tertiary accents, and clear color blocking without drifting into realistic or noisy detail.

#### Scenario: Agent generates a new V1 reference sheet
- **WHEN** the active store policy selects `low-resolution-voxel.v1` for a supported authored asset kind
- **THEN** generation uses the shared grammar plus that asset kind's applicable scoped examples while canonical Hero.Voxel construction rules remain authoritative

#### Scenario: Agent generates a V1 avatar reference
- **WHEN** the active store policy selects `low-resolution-voxel.v1` for an avatar
- **THEN** generation uses the shared grammar plus avatar-scoped examples while canonical Hero.Voxel construction rules remain authoritative

#### Scenario: Agent generates a V1 equipment reference
- **WHEN** the active store policy selects `low-resolution-voxel.v1` for equipment
- **THEN** generation uses the shared grammar plus equipment-scoped examples and preserves readable primary silhouette plus purposeful secondary construction detail while canonical Hero.Voxel construction rules remain authoritative

#### Scenario: Equipment reference stops at a primitive primary silhouette
- **WHEN** an equipment reference has a recognizable overall form but omits visually useful secondary construction such as taper, thickness variation, shaped grip regions, stepped contours, structural transitions, or material separation that the approved style examples support
- **THEN** the equipment reference does not satisfy the V1 style authority until purposeful secondary construction detail is restored

#### Scenario: Equipment reference becomes realistically or noisily over-detailed
- **WHEN** added equipment detail reduces chunky voxel readability through realistic micro-detail, dense surface noise, texture-like decoration, or accents that do not clarify construction
- **THEN** the equipment reference does not satisfy the V1 style authority until detail is simplified back to the approved low-resolution grammar

#### Scenario: Equipment uses detail-count as a substitute for visual quality
- **WHEN** an equipment asset reaches an arbitrary box, voxel, or accent count without improving readable construction or reference fidelity
- **THEN** that count alone does not satisfy the V1 style authority
