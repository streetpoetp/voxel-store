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
Style-reference images SHALL control visual art direction only; canonical Hero.Voxel reference-package rules SHALL remain the authority for Composition Sheet regions, Part Sheet views, part coverage, construction orientation, and implementation-semantic exclusions.

#### Scenario: Style example uses a different sheet layout
- **WHEN** a style-reference image presents characters or objects in a layout that differs from the canonical Hero.Voxel sheet contract
- **THEN** agents preserve its visual style while generating sheets with Hero.Voxel's canonical construction structure

### Requirement: Low-resolution voxel style is the V1 reference-image authority
The `low-resolution-voxel.v1` family SHALL preserve one shared low-resolution voxel grammar while providing avatar- and equipment-specific examples. Equipment examples SHALL communicate equipment silhouette, thickness/proportion language, readable grip or attachment regions, restrained detail density, and clear color blocking.

#### Scenario: Agent generates a new V1 reference sheet
- **WHEN** the active store policy selects `low-resolution-voxel.v1` for a supported authored asset kind
- **THEN** generation uses the shared grammar plus that asset kind's applicable scoped examples while canonical Hero.Voxel construction rules remain authoritative

#### Scenario: Agent generates a V1 avatar reference
- **WHEN** the active store policy selects `low-resolution-voxel.v1` for an avatar
- **THEN** generation uses the shared grammar plus avatar-scoped examples while canonical Hero.Voxel construction rules remain authoritative

#### Scenario: Agent generates a V1 equipment reference
- **WHEN** the active store policy selects `low-resolution-voxel.v1` for equipment
- **THEN** generation uses the shared grammar plus equipment-scoped examples while canonical Hero.Voxel construction rules remain authoritative

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
