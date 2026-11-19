# reference-style-authority Specification

## Purpose

Defines how the voxel asset store owns concrete visual style references so generated construction sheets preserve an approved reusable art direction without replacing Hero.Voxel construction semantics.

## Requirements

### Requirement: Active style families resolve to concrete repository-owned references
The store SHALL allow an active reference-image style family to declare scoped repository-owned PNG references. For an asset, applicable style authority SHALL be the ordered union of any optional `common` references and the required references for that asset kind; unrelated kind scopes SHALL NOT be required for that asset.

#### Scenario: Active style authority resolves successfully
- **WHEN** an asset uses an active style family whose matching asset-kind scope is complete and any declared common references are available
- **THEN** the store exposes only the applicable common plus matching-kind references as visual style authority for generation and audit

#### Scenario: Active style authority is incomplete
- **WHEN** an active style family has no usable matching asset-kind reference scope or any applicable common/kind-scoped image is missing
- **THEN** reference generation/publication for that asset fails closed instead of falling back to another scope or an invented style

#### Scenario: Avatar style authority resolves
- **WHEN** an avatar uses an active style family with a complete `avatar` reference scope and any declared `common` references are available
- **THEN** the store exposes only those applicable references as the avatar's visual style authority for generation and audit

#### Scenario: Equipment style authority resolves
- **WHEN** equipment uses an active style family with a complete `equipment` reference scope and any declared `common` references are available
- **THEN** the store exposes only those applicable references as the equipment's visual style authority for generation and audit

#### Scenario: Applicable scoped style authority is incomplete
- **WHEN** an asset's matching asset-kind scope is missing/empty, or any applicable common/kind-scoped reference image is missing
- **THEN** reference generation/publication for that asset fails closed instead of falling back to another asset-kind scope or an invented style

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

### Requirement: Shared style grammar does not implicitly own asset-specific visual identity
Shared style authority SHALL define reusable visual grammar and the family quality bar. Unless active style authority explicitly declares a palette, material hierarchy, accent family, decorative motif, or equivalent visual choice as a family-wide invariant, those choices SHALL remain asset-specific and SHALL be established by asset intent and the accepted Product Target. An agent SHALL NOT turn incidental choices from one or more style exemplars into undeclared global requirements.

#### Scenario: Two equipment assets share grammar but use different palettes
- **WHEN** two equipment Product Targets both preserve the active style's chunky low-resolution geometry, readable silhouette, detail restraint, and construction-quality language but use different coherent palettes and material hierarchies
- **THEN** both may satisfy the same shared style authority if no active style rule explicitly requires one common palette or material recipe

#### Scenario: Equipment exemplar contains recurring green wraps and brown wood
- **WHEN** an equipment style exemplar uses green wraps, brown wood, and gold accents but those choices are not declared family-wide invariants
- **THEN** a new equipment asset may use a different coherent grip material, palette, accent color, and motif while preserving the shared voxel grammar and quality bar

#### Scenario: Style authority explicitly fixes a family-wide visual invariant
- **WHEN** active style authority explicitly declares a visual property as global rather than merely depicting it in an exemplar
- **THEN** generated Product Targets preserve that declared invariant while remaining free to establish other asset-specific visual identity choices

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

### Requirement: Style authority is included in visual evidence provenance
Accepted reference evidence SHALL identify the active style family and only the concrete style inputs applicable to the asset. A change to applicable common/kind-scoped authority SHALL stale that asset's visual acceptance; a change only to an unrelated asset-kind scope SHALL NOT stale it.

#### Scenario: Style-reference input changes after acceptance
- **WHEN** any style input applicable to an accepted asset changes after reference acceptance
- **THEN** that asset's previous visual acceptance becomes stale until it is re-audited against current applicable style authority

#### Scenario: Common style authority changes
- **WHEN** a common style reference or common style mapping changes after avatar or equipment reference acceptance
- **THEN** previously accepted assets using that style family become stale until visually re-audited

#### Scenario: Equipment-only style authority changes
- **WHEN** only an equipment-scoped style reference or mapping changes
- **THEN** accepted equipment using that style becomes stale while avatar acceptance remains current

#### Scenario: Avatar-only style authority changes
- **WHEN** only an avatar-scoped style reference or mapping changes
- **THEN** accepted avatars using that style become stale while equipment acceptance remains current

### Requirement: Active style authority remains independent of retired asset outputs
An active style-reference scope SHALL resolve to repository-owned visual inputs that remain valid independently of any retired asset's generated source, proof, or construction-reference outputs.

#### Scenario: Retired asset-derived style board remains referenced
- **WHEN** an active style scope points only to an image derived from an asset that has been retired from the baseline
- **THEN** style authority is invalid until that dependency is removed or replaced by independent visual authority

#### Scenario: Supplied source image becomes equipment style authority
- **WHEN** an approved external source image is imported as a repository-owned style input and listed in the equipment scope
- **THEN** equipment reference generation may use that image without depending on any currently published equipment model

### Requirement: Style-source identity is preserved through import
A repository-owned copy of an approved supplied style image SHALL preserve the source image bytes or an explicitly recorded canonical derivative so visual provenance can be audited independently of generated asset evidence.

#### Scenario: Exact supplied image is imported
- **WHEN** the approved source image can be stored directly in the repository
- **THEN** its hash becomes the concrete equipment-style provenance input

#### Scenario: Canonical derivative is required
- **WHEN** repository constraints require a derivative rather than the exact source image
- **THEN** the derivative relationship and source hash are recorded so the style authority remains traceable to the supplied image
