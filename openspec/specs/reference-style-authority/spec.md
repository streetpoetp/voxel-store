# reference-style-authority Specification

## Purpose

Defines how the voxel asset store owns concrete visual style references so generated construction sheets preserve an approved reusable art direction without replacing Hero.Voxel construction semantics.

## Requirements

### Requirement: Active style families resolve to concrete repository-owned references
The store SHALL require each active reference-image style family to resolve to a non-empty ordered set of repository-owned PNG image assets rather than a string-only style label.

#### Scenario: Active style authority resolves successfully
- **WHEN** an asset uses an active style family and every declared style-reference image exists
- **THEN** the store exposes that concrete image set as the visual style authority for reference generation and audit

#### Scenario: Active style authority is incomplete
- **WHEN** an active style family has no declared image set or any declared style-reference image is missing
- **THEN** reference generation/publication fails closed instead of falling back to an implicit or invented style

### Requirement: Style authority and construction authority remain separate
Style-reference images SHALL control visual art direction only; canonical Hero.Voxel reference-package rules SHALL remain the authority for Composition Sheet regions, Part Sheet views, part coverage, construction orientation, and implementation-semantic exclusions.

#### Scenario: Style example uses a different sheet layout
- **WHEN** a style-reference image presents characters or objects in a layout that differs from the canonical Hero.Voxel sheet contract
- **THEN** agents preserve its visual style while generating sheets with Hero.Voxel's canonical construction structure

### Requirement: Low-resolution voxel style is the V1 reference-image authority
The `low-resolution-voxel.v1` style family SHALL use the approved character reference set to define chunky low-resolution voxel forms, simplified readable geometry, chibi proportion language, restrained detail density, clear color blocking, and high silhouette readability.

#### Scenario: Agent generates a new V1 reference sheet
- **WHEN** the active store policy selects `low-resolution-voxel.v1`
- **THEN** the generated visual reference follows the approved low-resolution voxel character style while construction-authority regions remain compliant with Hero.Voxel's flat/unlit rules

### Requirement: Style authority is included in visual evidence provenance
Accepted reference-package evidence SHALL identify the active style family and concrete style-reference inputs used for generation/audit so a style-authority change invalidates stale visual acceptance.

#### Scenario: Style-reference input changes after acceptance
- **WHEN** a style-reference image or the active style-family mapping changes after a reference package was accepted
- **THEN** the previous reference acceptance is stale and the asset cannot remain healthy until visual reference evidence is re-audited
