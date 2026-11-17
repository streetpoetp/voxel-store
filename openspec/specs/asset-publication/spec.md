# asset-publication Specification

## Purpose

Defines how reusable voxel avatars and equipment become source-authoritative, verifiable, and publishable assets without turning generated outputs or runtime artifacts into authoring authority.

## Requirements

### Requirement: Asset records own stable semantic identity
Each authored asset SHALL have one repository-owned manifest that declares a unique stable asset id, asset kind, immutable semantic `UnitRef`, compatibility classification, and canonical source/reference locations. Published identity SHALL NOT be derived from display names, folder paths, generated filenames, or Unity asset paths.

#### Scenario: New asset declares a valid identity
- **WHEN** an avatar or equipment asset is added with a unique asset id and `UnitRef`
- **THEN** the store accepts those values as the asset's stable authoring identity and includes them in derived publication data

#### Scenario: Published semantic identity changes
- **WHEN** a previously published asset changes its stable asset id or `UnitRef` without an explicit migration
- **THEN** validation fails instead of silently treating the changed value as the same published asset

### Requirement: Hero.Voxel source ownership remains canonical
Spec-driven assets SHALL keep editable VoxelSpec sources and machine-owned generated Hero.Voxel siblings at the canonical unit source base. Generated siblings SHALL be reproducible outputs and SHALL NOT become manually edited authoring authority or be moved into a separate generated-art hierarchy.

#### Scenario: Spec-driven asset is generated
- **WHEN** the store compiles a valid VoxelSpec asset
- **THEN** generated geometry and generated animation siblings are produced beside the source base using the canonical Hero.Voxel ownership convention

#### Scenario: Generated artifact differs from reproducible output
- **WHEN** a checked generated sibling does not match deterministic generation from its authoritative source inputs
- **THEN** the asset is unhealthy and cannot satisfy publication until the generated artifact is refreshed or the authoritative source is intentionally changed

### Requirement: Publication status is evidence-derived
An asset SHALL be publishable only when its manifest, identity, reference-package coverage, authoritative source, deterministic generation, required standalone visual proof, applicable compatibility proof, and declared budget checks all pass. Publication status SHALL be derived from current evidence rather than a manually editable approval flag.

#### Scenario: All publication gates pass
- **WHEN** every required publication gate for an asset passes against current authoritative inputs
- **THEN** generated health and catalog outputs report the asset as published

#### Scenario: A required gate fails after publication
- **WHEN** current validation finds that any required publication gate for a previously published asset no longer passes
- **THEN** regenerated health/catalog state no longer reports that asset as healthy published content and identifies the failing gate

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

### Requirement: Equipment reference authority precedes equipment source authority
For agent-authored equipment design or creative repair, an accepted construction reference package SHALL exist before new or revised VoxelSpec/VOX geometry can qualify as authoritative equipment source. Existing model/proof output SHALL NOT become the normal upstream design authority.

#### Scenario: New equipment is authored
- **WHEN** maintenance creates a new equipment asset from visual intent and scoped style authority
- **THEN** its Composition and Part Sheets are visually accepted before equipment geometry is authored or accepted for publication

#### Scenario: Equipment visual design changes
- **WHEN** a published equipment asset receives an intentional visual redesign
- **THEN** the revised reference package is accepted first and source/proof are subsequently brought into agreement with that reference authority

#### Scenario: Existing model is used during explicit migration or recovery
- **WHEN** an explicit migration/recovery task uses existing VoxelSpec or proof output to reconstruct missing reference evidence
- **THEN** that output is treated as reconstruction evidence only and the reconstructed reference package still requires independent visual acceptance under current style authority

### Requirement: Equipment reference authority remains implementation-neutral
Equipment reference sheets SHALL define visual identity, rigid-part ownership, proportions, and visual attachment intent only. They SHALL NOT own exact pivots, assembly origin, runtime socket encoding, mount transforms, pair-specific corrective transforms, or compatibility implementation details.

#### Scenario: Handheld equipment shows a grip region
- **WHEN** a reference sheet visually indicates where a hand should hold an equipment item
- **THEN** the sheet may communicate the grip/attachment region without defining an exact Hero.Voxel pivot, assembly origin, socket, or transform

#### Scenario: Equipment is compatible with an avatar mount
- **WHEN** later compatibility verification composes equipment with a declared avatar socket/profile
- **THEN** the compatibility implementation remains downstream evidence and does not become construction-reference authority

### Requirement: Generated catalog is a projection, not authority
The repository SHALL generate catalog and health outputs from authoritative manifests, sources, policies, and verification evidence. Generated catalog/health files SHALL be safely regenerable and SHALL NOT be accepted as the sole source for identity, compatibility, publication, or coverage decisions.

#### Scenario: Catalog is deleted
- **WHEN** generated catalog and health outputs are removed while authoritative store inputs remain intact
- **THEN** a store scan can reconstruct equivalent catalog/health state from authoritative inputs and current verification evidence

### Requirement: Asset metadata remains gameplay-neutral
Store-level asset contracts SHALL describe visual identity, construction, physical compatibility, palette/style family, bounds, and authoring evidence only. They SHALL NOT own game-specific damage, rarity, class, AI, progression, economy, or balance semantics.

#### Scenario: Gameplay metadata is placed in an asset manifest
- **WHEN** an asset manifest attempts to declare a gameplay-only field such as damage or rarity as store authority
- **THEN** store validation rejects that field or treats it as outside the supported asset contract rather than publishing it as shared voxel metadata
