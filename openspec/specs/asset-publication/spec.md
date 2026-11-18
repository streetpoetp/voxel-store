# asset-publication Specification

## Purpose

Defines how reusable voxel avatars and equipment become source-authoritative, verifiable, and publishable assets without turning generated outputs or runtime artifacts into authoring authority.

## Requirements

### Requirement: Asset records own stable semantic identity
Each authored asset SHALL have one repository-owned manifest that declares a unique stable asset id, asset kind, immutable semantic `UnitRef`, compatibility classification, canonical source/reference locations, and any applicable reusable physical-form classification. Published identity SHALL NOT be derived from display names, folder paths, generated filenames, Unity asset paths, or a particular physical hand instance.

#### Scenario: New asset declares a valid identity
- **WHEN** an avatar or equipment asset is added with a unique asset id and `UnitRef` plus any required compatibility and physical-form classification
- **THEN** the store accepts those values as the asset's stable authoring identity and includes the reusable classification in derived publication data

#### Scenario: Hand-mounted equipment declares side-neutral identity
- **WHEN** equipment mounts through a logical hand capability that can resolve to multiple physical hand attachment instances
- **THEN** its stable id, `UnitRef`, and mount declaration remain independent of which physical hand instance a consumer chooses

#### Scenario: Published semantic identity changes
- **WHEN** a previously published asset changes its stable asset id or `UnitRef` without an explicit migration
- **THEN** validation fails instead of silently treating the changed value as the same published asset

#### Scenario: Published equipment changes physical form
- **WHEN** previously published equipment changes its reusable physical-form classification without an explicit migration
- **THEN** validation fails instead of silently moving the same published identity between coverage forms

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
Agent-authored assets SHALL use visually construction-authoritative Hero.Voxel PNG sheets. Legacy reference-contract v2 packages MAY remain valid unchanged. New or intentionally redesigned assets entering the target-driven workflow SHALL use reference-contract v3 and SHALL have current target-fidelity and style acceptance before geometry authoring is accepted.

#### Scenario: Legacy v2 reference coverage remains complete
- **WHEN** an unchanged published asset still uses a valid v2 Composition/Part reference package with current style acceptance
- **THEN** its reference-package coverage gate may continue to pass without a Product Target until that asset is intentionally migrated or redesigned

#### Scenario: Reference coverage is complete
- **WHEN** a reference package satisfies all requirements of its declared contract version, including target/fidelity evidence for v3 and current style acceptance
- **THEN** the reference-package coverage gate passes and VoxelSpec geometry may qualify for later publication gates

#### Scenario: Target-driven v3 reference coverage is complete
- **WHEN** a v3 package declares a current Product Target, accepted 3:2 Composition Sheet, exactly one accepted Part Sheet for every `RequiredParts` id, current target-fidelity evidence, and current style authority
- **THEN** the reference-package coverage gate passes and VoxelSpec geometry may qualify for later publication gates

#### Scenario: Target-driven package omits Product Target or fidelity evidence
- **WHEN** a v3 package has Composition/Part Sheets but no current declared Product Target or target-fidelity evidence
- **THEN** the reference-package coverage gate fails before source authority is accepted

#### Scenario: Part coverage is incomplete or ambiguous
- **WHEN** any required part has no accepted Part Sheet, multiple accepted Part Sheets, a sheet claims a part outside `RequiredParts`, or the declared mapping is missing/ambiguous
- **THEN** the reference-package coverage gate fails and the asset cannot publish

#### Scenario: Schematic placeholder satisfies filenames but not construction authority
- **WHEN** a declared Product Target, Composition Sheet, or Part Sheet is not valid visual authority or the construction sheets materially fail the applicable Hero.Voxel/target-fidelity audit
- **THEN** visual reference acceptance fails even if the files exist and their hashes are sealed

#### Scenario: Reference package was accepted under stale style authority
- **WHEN** the active style family or any concrete style-reference input applicable to the accepted package changes after acceptance
- **THEN** the package fails until its Product Target and construction evidence are revalidated as required by its contract version

### Requirement: Equipment reference authority precedes equipment source authority
For agent-authored equipment design or creative repair, accepted visual authority SHALL exist before new or revised VoxelSpec/VOX geometry can qualify as authoritative source. New/redesigned target-driven equipment SHALL accept Product Target first, then Composition and Part Sheets with fidelity evidence, before density/source authoring. Existing model/proof output SHALL NOT become the normal upstream design authority.

#### Scenario: New equipment is authored
- **WHEN** maintenance creates a new equipment asset from visual intent and scoped style authority
- **THEN** the asset follows the applicable reference contract, using Product Target first for target-driven v3, before equipment geometry is authored or accepted for publication

#### Scenario: New target-driven equipment is authored
- **WHEN** maintenance creates new equipment from form intent and scoped style authority
- **THEN** it establishes and accepts the Product Target, then accepts Composition and Part Sheets against that target before density/source geometry is authored or accepted for publication

#### Scenario: Equipment visual design changes
- **WHEN** a published equipment asset receives an intentional visual redesign under the target-driven workflow
- **THEN** the revised Product Target and construction package are accepted first and source/proof are subsequently brought into agreement with that authority

#### Scenario: Existing model is used during explicit migration or recovery
- **WHEN** an explicit migration/recovery task uses existing VoxelSpec or proof output to reconstruct missing reference evidence
- **THEN** that output is treated as reconstruction evidence only and the reconstructed Product Target/reference package still requires independent visual acceptance under current authority

### Requirement: Equipment reference authority remains implementation-neutral
Equipment Product Targets and construction sheets SHALL define visual identity, rigid-part ownership, proportions, material/color hierarchy, meaningful construction detail, and visual attachment intent only. They SHALL NOT own exact pivots, assembly origin, runtime socket encoding, mount transforms, pair-specific corrective transforms, exact VPU, exact voxel dimensions, or source voxel budgets.

#### Scenario: Handheld equipment shows a grip region
- **WHEN** a target/reference sheet visually indicates where a hand should hold an equipment item
- **THEN** the sheet may communicate the grip/attachment region without defining an exact Hero.Voxel pivot, assembly origin, socket, or transform

#### Scenario: Reference image attempts to freeze VPU or voxel bounds
- **WHEN** a Product Target, Composition Sheet, or Part Sheet declares exact implementation density such as VPU or fixed voxel W x H x D dimensions
- **THEN** reference acceptance fails and the density/bounds decision remains downstream of accepted visual authority

#### Scenario: Equipment is compatible with an avatar mount
- **WHEN** later compatibility verification composes equipment with a declared avatar socket/profile
- **THEN** the compatibility implementation remains downstream evidence and does not become Product Target or construction-reference authority

### Requirement: Generated catalog is a projection, not authority
The repository SHALL generate catalog and health outputs from authoritative manifests, sources, policies, and verification evidence. Generated catalog/health files SHALL be safely regenerable and SHALL NOT be accepted as the sole source for identity, compatibility, publication, or coverage decisions.

#### Scenario: Catalog is deleted
- **WHEN** generated catalog and health outputs are removed while authoritative store inputs remain intact
- **THEN** a store scan can reconstruct equivalent catalog/health state from authoritative inputs and current verification evidence

### Requirement: Asset metadata remains gameplay-neutral
Store-level asset contracts SHALL describe visual identity, construction, physical compatibility, reusable physical form, palette/style family, bounds, and authoring evidence only. They SHALL NOT own handed gameplay rules, two-hand occupancy, damage, rarity, class, AI, progression, economy, balance, attack behavior, or other consuming-game semantics.

#### Scenario: Gameplay metadata is placed in an asset manifest
- **WHEN** an asset manifest attempts to declare a gameplay-only field such as damage, rarity, attack behavior, or required hand occupancy as store authority
- **THEN** store validation rejects that field or treats it as outside the supported asset contract rather than publishing it as shared voxel metadata

#### Scenario: Equipment declares a physical form
- **WHEN** equipment declares a form such as sword, bow, spear, or rifle solely to distinguish reusable visual and physical asset coverage
- **THEN** the store accepts that classification without assigning combat behavior, balance, or handed gameplay meaning to it

### Requirement: Published assets can be retired without identity reuse
The store SHALL allow a previously published asset to be removed from the active baseline while preserving its historical asset id and UnitRef as reserved identities that cannot be reassigned to a different asset.

#### Scenario: Published equipment is intentionally retired
- **WHEN** an approved baseline migration removes a previously published equipment asset and its active authoring/evidence files
- **THEN** subsequent scans omit that asset from active publication/catalog state without treating its absence as an in-place identity mutation

#### Scenario: Replacement equipment attempts to reuse a retired identity
- **WHEN** a new replacement asset declares an asset id or UnitRef previously published by a retired asset
- **THEN** validation fails and requires a new semantic identity

### Requirement: Replacement assets pass normal publication gates
A replacement asset SHALL be treated as a new independently authored asset and SHALL pass the same reference-first, source, deterministic generation, proof, compatibility, and budget gates as any other new asset.

#### Scenario: Replacement bow is added after retiring prior equipment
- **WHEN** the new bow has a unique id/UnitRef and its current evidence satisfies every required publication gate
- **THEN** the bow may publish independently of the retired equipment history

#### Scenario: Replacement bow bypasses reference-first authority
- **WHEN** bow source or generated proof exists without an accepted canonical reference package
- **THEN** publication fails at the reference/source ordering boundary

### Requirement: High-resolution source authoring remains explicitly bounded
The store SHALL permit equipment to use high-resolution voxel source geometry when it remains within the declared source-voxel and part budgets, is deterministically generated, and satisfies all ordinary reference, proof, compatibility, and publication gates.

#### Scenario: High-resolution equipment is within the declared budget
- **WHEN** an equipment asset uses a higher authoring density but its occupied source voxels and rigid-part count remain within the active store budget
- **THEN** publication does not reject the asset solely because its source voxel density is higher than the avatar's

#### Scenario: High-resolution equipment exceeds the declared source budget
- **WHEN** an equipment asset's occupied source voxels exceed the active store budget
- **THEN** the publication budget gate fails with the measured occupied-voxel count even if runtime greedy meshing would reduce mesh complexity

### Requirement: Reference-driven equipment requires current density evidence
Equipment that explicitly opts into `density: reference-driven` SHALL pass a density gate after reference acceptance and before source authority. The density gate SHALL require current reference-density evidence, and the subsequent source gate SHALL require authored source to use the selected VPU. Equipment without the opt-in remains valid legacy content during incremental migration and is not subject to the density gate.

#### Scenario: Current density evidence matches source
- **WHEN** equipment declares `density: reference-driven`, has current accepted reference evidence, current density-selection evidence, and source authored at the selected VPU
- **THEN** the density gate passes and the source gate verifies the authored VPU before later generation/proof gates run

#### Scenario: Legacy equipment has not opted into density selection
- **WHEN** equipment does not declare `density: reference-driven`
- **THEN** publication follows the pre-density gate sequence and does not require density evidence during incremental migration

#### Scenario: Density evidence is missing
- **WHEN** equipment declares `density: reference-driven` and has accepted Part Sheets but no density-selection evidence
- **THEN** publication fails at the density gate before source authority is accepted

#### Scenario: Density evidence is stale
- **WHEN** any accepted Part Sheet or density input changes after VPU selection
- **THEN** publication fails at the density gate until density selection is recomputed

#### Scenario: Source VPU disagrees with selected VPU
- **WHEN** current density evidence selects one VPU but the authoritative equipment source declares another VPU
- **THEN** publication fails at the source gate instead of silently accepting the unselected density
