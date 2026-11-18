# Spec Delta

## MODIFIED Requirements

### Requirement: Style authority and construction authority remain separate
Style-reference images SHALL control reusable art direction. For target-driven assets, the Product Target SHALL convert that art direction plus asset intent into asset-specific visual-fidelity authority. Canonical Hero.Voxel Composition/Part Sheets SHALL remain construction authority for part coverage, orientation, view requirements, and implementation-semantic exclusions. None of these layers SHALL silently replace another.

#### Scenario: Style example uses a different sheet layout
- **WHEN** a style-reference image presents characters or objects in a layout that differs from the canonical Hero.Voxel sheet contract
- **THEN** the Product Target preserves its visual language while Composition/Part Sheets use Hero.Voxel's canonical construction structure

#### Scenario: Generated Product Target is grounded in style authority
- **WHEN** an asset has no direct approved product image and a Product Target is generated from active style references plus asset form intent
- **THEN** the target remains traceable to those style inputs and becomes asset-specific visual authority without changing the global style family

#### Scenario: Product Target does not replace construction authority
- **WHEN** a Product Target is visually rich but does not provide canonical orthographic construction coverage
- **THEN** it cannot substitute for Composition/Part Sheets; those sheets must be derived and audited separately

#### Scenario: Construction sheet is structurally correct but visually off-style
- **WHEN** a Composition or Part Sheet follows Hero.Voxel layout rules but materially diverges from the Product Target and applicable style grammar
- **THEN** visual acceptance fails rather than treating structural compliance as sufficient

### Requirement: Low-resolution voxel style is the V1 reference-image authority
The `low-resolution-voxel.v1` family SHALL preserve shared high-fidelity voxel-art language through applicable `common` references while retaining avatar- and equipment-specific construction examples. Product Target generation SHALL consume the applicable common plus matching-kind authority so simplified kind-specific boards cannot silently reduce the visual richness established by shared style references.

#### Scenario: Agent generates a new V1 reference sheet
- **WHEN** the active store policy selects `low-resolution-voxel.v1` for a supported authored asset kind
- **THEN** generation uses the shared grammar plus applicable style authority while canonical Hero.Voxel construction rules remain authoritative

#### Scenario: Agent generates a new V1 Product Target
- **WHEN** a target-driven asset uses `low-resolution-voxel.v1`
- **THEN** Product Target generation uses applicable common high-fidelity references together with the matching asset-kind examples

#### Scenario: Equipment-specific board is simpler than common style authority
- **WHEN** an equipment construction board communicates form clearly but is materially less visually rich than an applicable common style reference
- **THEN** the board remains supplemental construction guidance and does not cap Product Target silhouette richness, material hierarchy, or meaningful detail density

#### Scenario: Agent generates a V1 avatar reference
- **WHEN** the active store policy selects `low-resolution-voxel.v1` for an avatar
- **THEN** generation uses the shared grammar plus applicable common and avatar-scoped examples while canonical Hero.Voxel construction rules remain authoritative

#### Scenario: Agent generates a V1 equipment reference
- **WHEN** the active store policy selects `low-resolution-voxel.v1` for equipment
- **THEN** generation uses the shared grammar plus applicable common and equipment-scoped examples while canonical Hero.Voxel construction rules remain authoritative

#### Scenario: Equipment reference stops at a primitive primary silhouette
- **WHEN** an equipment reference has a recognizable overall form but omits visually useful secondary construction such as taper, thickness variation, shaped grip regions, stepped contours, structural transitions, or material separation that the approved style examples support
- **THEN** the equipment reference does not satisfy the V1 style authority until purposeful secondary construction detail is restored

#### Scenario: Equipment reference becomes realistically or noisily over-detailed
- **WHEN** added equipment detail reduces chunky voxel readability through realistic micro-detail, dense surface noise, texture-like decoration, or accents that do not clarify construction
- **THEN** the equipment reference does not satisfy the V1 style authority until detail is simplified back to the approved low-resolution grammar

#### Scenario: Equipment uses detail-count as a substitute for visual quality
- **WHEN** an equipment asset reaches an arbitrary box, voxel, or accent count without improving readable construction or reference fidelity
- **THEN** that count alone does not satisfy the V1 style authority
