# asset-publication Specification

## Purpose

Defines how reusable voxel avatars and equipment become source-authoritative, verifiable, and publishable assets without turning generated outputs or runtime artifacts into authoring authority.

## Requirements

### Requirement: Rigid-body avatars publish from exactly one Body part
An avatar using the rigid-body composition contract SHALL publish only when its authoritative Hero.Voxel source resolves to exactly one authored part named `Body` and required standalone evidence is current for that Body geometry.

#### Scenario: Single-Body avatar passes source structure
- **WHEN** an avatar declares the rigid-body composition contract and its authoritative source contains exactly one part named `Body`
- **THEN** the avatar may proceed to later publication gates

#### Scenario: Rigid-body avatar retains articulated parts
- **WHEN** an avatar under the rigid-body composition contract contains additional authored parts such as `Head`, `Arm`, `Hand`, or `Foot`
- **THEN** publication fails at the source/compatibility boundary until those parts are converged into `Body`

### Requirement: Asset records own stable placement-neutral semantic identity
Each authored asset SHALL have one repository-owned manifest that declares a unique stable asset id, asset kind, immutable semantic `UnitRef`, canonical source/reference locations, and any applicable reusable physical-form classification. Published identity SHALL NOT depend on Body placement ratios or consumer-selected equipment indices.

#### Scenario: New asset declares a valid identity
- **WHEN** an avatar or equipment asset is added with a unique asset id and `UnitRef` plus any required reusable physical-form classification
- **THEN** the store accepts those values as the asset's stable authoring identity and includes the reusable classification in derived publication data

#### Scenario: Ratio-composed equipment keeps placement-neutral identity
- **WHEN** equipment is composed at one or more normalized Body-bounds ratios
- **THEN** its stable id and `UnitRef` remain independent of the ratio or consumer-selected equipment index

#### Scenario: Published semantic identity changes
- **WHEN** a previously published asset changes its stable asset id or `UnitRef` without an explicit migration
- **THEN** validation fails instead of silently treating the changed value as the same published asset

#### Scenario: Published equipment changes physical form
- **WHEN** previously published equipment changes its reusable physical-form classification without an explicit migration
- **THEN** validation fails instead of silently moving the same published identity between coverage forms

### Requirement: Published identity is independent of repository collection paths
Moving a published asset between repository organization roots SHALL NOT change its published asset id or UnitRef unless an explicit identity migration is approved. Filesystem structure is organizational authority, not published identity authority.

#### Scenario: Foundation-owned asset path moves to reusable Assets root
- **WHEN** a published asset moves from a collection-specific legacy path to `Assets/**` with unchanged manifest id and UnitRef
- **THEN** the move is treated as repository-structure migration rather than retirement/republication under a new identity

### Requirement: Hero.Voxel source ownership remains canonical
Spec-driven assets SHALL keep editable VoxelSpec sources and machine-owned generated Hero.Voxel siblings at the canonical unit source base. Generated siblings SHALL be reproducible outputs and SHALL NOT become manually edited authoring authority or be moved into a separate generated-art hierarchy.

#### Scenario: Spec-driven asset is generated
- **WHEN** the store compiles a valid VoxelSpec asset
- **THEN** generated geometry and generated animation siblings are produced beside the source base using the canonical Hero.Voxel ownership convention

#### Scenario: Generated artifact differs from reproducible output
- **WHEN** a checked generated sibling does not match deterministic generation from its authoritative source inputs
- **THEN** the asset is unhealthy and cannot satisfy publication until the generated artifact is refreshed or the authoritative source is intentionally changed

### Requirement: Publication status is evidence-derived
An asset SHALL be publishable only when its manifest, identity, reference-package coverage, authoritative source, deterministic generation, required standalone visual proof, applicable rigid-body composition proof, and declared budget checks all pass. Publication status SHALL be derived from current evidence rather than a manually editable approval flag.

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

### Requirement: Equipment reference authority remains ratio-placement-neutral
Equipment Product Targets and construction sheets SHALL define visual identity, rigid-part ownership, proportions, material/color hierarchy, meaningful construction detail, and visual attachment intent only. They SHALL NOT own exact assembly origin, runtime Body-ratio offsets, consumer equipment indices, pair-specific corrective transforms, exact VPU, exact voxel dimensions, or source voxel budgets.

#### Scenario: Equipment shows an intended grip or attachment region
- **WHEN** a target/reference sheet visually indicates how an equipment item should appear relative to a character
- **THEN** the sheet may communicate that visual intent without defining exact runtime Body-ratio placement or consumer slot/index semantics

#### Scenario: Reference image attempts to freeze VPU or voxel bounds
- **WHEN** a Product Target, Composition Sheet, or Part Sheet declares exact implementation density such as VPU or fixed voxel W x H x D dimensions
- **THEN** reference acceptance fails and the density/bounds decision remains downstream of accepted visual authority

#### Scenario: Equipment is evaluated with a rigid-body avatar
- **WHEN** later compatibility verification composes equipment against canonical Body bounds using required ratio-placement evidence
- **THEN** that composition evidence remains downstream implementation proof and does not become Product Target or construction-reference authority

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

### Requirement: New target-driven publication requires accepted Product Target authority
For new target-driven authoring, reference acceptance SHALL require a current accepted Product Target with valid acquisition provenance before Composition, Part, density, or source authority may advance. Publication SHALL NOT require a specific generator, provider, model, or acquisition technology.

#### Scenario: New target has current provider-neutral provenance
- **WHEN** a new target-driven asset has an accepted Product Target whose bytes, declared upstream authority, style acceptance, and provenance are current
- **THEN** the workflow may derive and audit Composition/Part construction evidence before later density/source gates

#### Scenario: Target exists without valid provenance
- **WHEN** a new target-driven asset has a target PNG but its acquisition provenance, declared source/style authority, or target hash is missing or stale
- **THEN** publication remains blocked at Product Target/reference authority and downstream geometry does not qualify

#### Scenario: Target has no generator metadata
- **WHEN** a new target-driven asset has an accepted Product Target with valid acquisition provenance but no generator/provider/model metadata
- **THEN** publication eligibility is unaffected because producer identity is not Product Target authority

### Requirement: Published equipment declares logical attachment capability
Published reusable equipment SHALL use the current manifest contract to declare one shared logical attachment capability and one model-owned attachment anchor name.

#### Scenario: Equipment targets a hand slot
- **WHEN** equipment declares `attachment.capability: Hand`
- **THEN** publication treats `Hand` as a reusable physical compatibility capability and does not require a concrete avatar socket instance

#### Scenario: Equipment declares a form-specific slot
- **WHEN** equipment declares a capability such as `SwordSlot`, `BowSlot`, or another form-specific vocabulary outside the shared physical capability set
- **THEN** publication fails instead of coupling avatar models to equipment forms

#### Scenario: Equipment is side-reusable
- **WHEN** equipment declares capability `Hand` with compatible side `Any`
- **THEN** the same asset identity may bind to a conforming left or right Hand slot without creating left/right duplicate catalog assets

### Requirement: Published avatars own physical equipment slot sockets
A reusable avatar SHALL author its physical equipment slot instances as Hero.Voxel sockets on the avatar model using the canonical equipment-slot naming convention.

#### Scenario: Avatar publishes a hand attachment
- **WHEN** an avatar supports the `Hand` capability
- **THEN** its VoxelSpec contains one or more sockets named `Equip.Hand.<Side>.<Instance>` parented to an authored avatar part, where Side is `Left`, `Right`, or `Center`

#### Scenario: Avatar publishes multiple slots of one kind
- **WHEN** a body can physically carry multiple items requiring the same capability
- **THEN** its VoxelSpec authors multiple distinct `Equip.<Capability>.<Side>.<Instance>` sockets and publication preserves side plus multiplicity rather than collapsing the capability to one slot

#### Scenario: Avatar declares malformed equipment socket identity
- **WHEN** a purported equipment slot does not match `Equip.<Capability>.<Side>.<Instance>`, uses an unsupported side, or references a missing parent part
- **THEN** publication fails at source validation

### Requirement: Published equipment owns one Mount socket
Reusable equipment SHALL author exactly one Hero.Voxel socket whose name matches its manifest attachment anchor, with `Mount` as the baseline anchor name.

#### Scenario: Equipment has a model-owned grip
- **WHEN** equipment declares `attachment.anchor: Mount`
- **THEN** its VoxelSpec contains exactly one `Mount` socket that defines the asset-specific attachment frame

#### Scenario: Equipment relies on model origin as an implicit grip
- **WHEN** a reusable equipment asset lacks its declared Mount socket
- **THEN** publication fails rather than assuming the equipment root/pivot is the attachment point

### Requirement: Reusable models publish semantic animation clips
Every reusable avatar and equipment asset SHALL author the baseline Hero.Voxel semantic clips `Idle`, `WalkForward`, `Attack`, `Hit`, and `Death`.

#### Scenario: Semantic animation set is complete
- **WHEN** source validation compiles a reusable avatar or equipment asset
- **THEN** all five required semantic clips exist with `Idle` and `WalkForward` looping and `Attack`, `Hit`, and `Death` non-looping

#### Scenario: Equipment owns unique attack motion
- **WHEN** two equipment assets share semantic clip name `Attack`
- **THEN** each asset may author different animation tracks/curves while publication treats both as satisfying the same semantic motion contract

#### Scenario: Required semantic clip is missing
- **WHEN** a reusable model omits one required semantic clip or declares incompatible loop semantics
- **THEN** publication fails before generated proof is sealed

#### Scenario: Body declares side-reactive hand attack
- **WHEN** a body publishes both left and right Hand slots and declares side-reactive `Attack` behavior
- **THEN** publication requires distinct `Attack.Left` and `Attack.Right` authored clips in addition to the baseline semantic set

#### Scenario: Current single-part body authors side reaction
- **WHEN** a current avatar has only the authored `Body` part
- **THEN** its side-specific clips may target only `Body`; publication does not assume Torso, Shoulder, Arm, Hand, or other anatomical parts exist

#### Scenario: Current single-part equipment owns visible equipped animation
- **WHEN** an equipment model has one authored part and its Mount is parented to that part
- **THEN** publication requires Mount position to coincide with that part's pivot and rejects equipped semantic position tracks on that Mount-bearing part, allowing authored rotation around the grip without moving the grip point

### Requirement: Generated catalog projects attachment and motion coverage
Generated catalog/health output SHALL project attachment capability, anchor identity, side-aware slot addresses, counts by capability and side, compatible equipment-side policy, and structured semantic-motion coverage including base, side, variant, and side+variant availability from authoritative manifests and compiled source.

#### Scenario: Equipment is cataloged
- **WHEN** a conforming equipment asset publishes
- **THEN** generated catalog state can report its form, attachment capability, Mount anchor, semantic motions, and model-local variant coverage without becoming their authority

#### Scenario: Avatar is cataloged
- **WHEN** a conforming avatar publishes
- **THEN** generated catalog state reports each available equipment slot capability, all concrete instance names, the count for each capability, and structured semantic-motion coverage that distinguishes base clips, physical sides, variants, and side-qualified variants

#### Scenario: Qualified clip names are projected
- **WHEN** a model publishes `Attack`, `Attack.Left`, `Attack.V2`, and `Attack.Left.V2`
- **THEN** generated catalog state reports one semantic `Attack` motion with its base, Left side, V2 variant, and Left+V2 coverage instead of listing four unrelated semantic motions

### Requirement: Reusable semantic motions may publish model-local animation variants
A reusable model MAY author optional take variants for any required semantic motion using the canonical suffix `V<n>`, where `n` is a positive integer. Variant clips SHALL remain members of their base semantic motion rather than becoming new semantic motions.

#### Scenario: Idle publishes multiple authored takes
- **WHEN** a model authors `Idle.V1`, `Idle.V2`, and the required base `Idle`
- **THEN** publication treats all three clips as semantic `Idle` coverage with two optional model-local variants

#### Scenario: Death publishes a different variant count
- **WHEN** the same model authors `Death.V1`, `Death.V2`, `Death.V3`, and the required base `Death`
- **THEN** publication accepts the Death variants without requiring Idle, WalkForward, Attack, or Hit to publish matching variant counts

#### Scenario: Variant identifier is malformed
- **WHEN** a clip attempts to use a variant suffix outside the canonical `V<n>` form
- **THEN** publication rejects it as invalid generalized variant identity instead of projecting an ambiguous variant

### Requirement: Animation variants inherit semantic loop policy
Every optional variant SHALL use the loop policy of its base semantic motion: `Idle` and `WalkForward` variants loop, while `Attack`, `Hit`, and `Death` variants do not loop.

#### Scenario: Looping semantic variant is valid
- **WHEN** `WalkForward.V2` is authored as looping
- **THEN** publication accepts its loop policy when the rest of the clip is valid

#### Scenario: Terminal semantic variant has wrong loop policy
- **WHEN** `Death.V2` is authored as looping
- **THEN** publication fails source validation before generated proof is sealed

### Requirement: Physical side and authored variant are independent animation qualifiers
A reusable model MAY combine supported side context with a model-local variant using `<Motion>.<Side>.V<n>`. Side qualification SHALL retain its existing physical meaning, while the variant suffix SHALL identify only an authored take.

#### Scenario: Side-specific attack take is authored
- **WHEN** an avatar authors `Attack.Left.V2` and `Attack.Right.V2`
- **THEN** publication records V2 takes for the existing Left and Right physical contexts without creating new semantic motions or asset identities

#### Scenario: Generic take is authored without side qualification
- **WHEN** a model authors `Attack.V2`
- **THEN** publication records V2 as a side-neutral authored take that may be resolved independently of a side-specific clip

### Requirement: Animation variant identity is local to one model
Variant identifiers SHALL be interpreted only within the animation catalog of the model that owns them and SHALL NOT imply shared choreography, personality, combat meaning, or aligned numbering across different models.

#### Scenario: Body and equipment both publish V2
- **WHEN** an avatar and an attached equipment model each publish an `Attack.V2` clip
- **THEN** publication treats the two V2 identifiers as independent authored takes owned by their respective models

#### Scenario: Models publish different variant sets
- **WHEN** one model publishes `Idle.V1` through `Idle.V3` and another publishes only `Idle.V1`
- **THEN** both models may publish without requiring global variant-set alignment

### Requirement: Published avatars provide minimum V1 motion coverage
Every published reusable avatar SHALL author a V1 take for each reusable motion family while retaining the required baseline clips and side-reactive Attack clips.

#### Scenario: Avatar satisfies minimum V1 coverage
- **WHEN** a reusable avatar is validated for publication
- **THEN** it contains `Idle.V1`, `WalkForward.V1`, `Hit.V1`, `Death.V1`, `Attack.Left.V1`, and `Attack.Right.V1` in addition to its required baseline and side-reactive Attack clips

#### Scenario: Avatar is missing one required V1 take
- **WHEN** a reusable avatar omits any required V1 clip
- **THEN** publication fails with the missing semantic/side variant reported before generated proof is sealed

#### Scenario: Current single-part avatar publishes V1 takes
- **WHEN** an avatar still consists only of the authored `Body` part
- **THEN** its required V1 clips may animate only `Body` and publication does not require invented anatomical parts

### Requirement: Published equipment provides minimum V1 motion coverage
Every published reusable equipment model SHALL author `Idle.V1`, `WalkForward.V1`, `Attack.V1`, `Hit.V1`, and `Death.V1` in addition to its required baseline semantic clips.

#### Scenario: Equipment satisfies minimum V1 coverage
- **WHEN** a reusable equipment model is validated for publication
- **THEN** all five required V1 clips exist and inherit the loop policy of their corresponding semantic motions

#### Scenario: Equipment is missing one required V1 take
- **WHEN** a reusable equipment model omits any required V1 clip
- **THEN** publication fails before generated proof is sealed

#### Scenario: One-part equipment publishes V1 motion
- **WHEN** current one-part equipment authors its required V1 clips
- **THEN** the Mount-bearing part remains compatible with the existing grip-preservation rules and the V1 set does not create attachment-time correction

### Requirement: Required V1 takes are authored motion rather than aliases
A required V1 clip SHALL contain authored animation data that is observably different from the fallback clip it supplements. Publication SHALL reject a V1 clip whose compiled timing and animation tracks are identical to that fallback.

#### Scenario: V1 differs from its baseline
- **WHEN** `Death.V1` changes duration, keyed transforms, interpolation, track coverage, or another compiled animation property relative to `Death`
- **THEN** publication accepts the take as independently authored when all other animation rules pass

#### Scenario: Side-qualified Attack V1 differs from side-only Attack
- **WHEN** `Attack.Left.V1` is required for an avatar
- **THEN** publication compares it with `Attack.Left` rather than treating the generic `Attack` clip as its authored-difference authority

#### Scenario: V1 is an exact copy
- **WHEN** a required V1 clip has compiled timing and track data identical to its fallback clip
- **THEN** publication fails instead of counting the duplicate as multi-animation coverage

### Requirement: Generated state reports required V1 completeness
Generated catalog and health state SHALL expose whether each published reusable model satisfies the store's minimum V1 coverage, derived from authoritative compiled animation data.

#### Scenario: Published model has complete required variants
- **WHEN** catalog and health state are generated for a conforming model
- **THEN** the model reports complete required V1 coverage together with its exact semantic/side variant availability

#### Scenario: Generated state observes incomplete coverage
- **WHEN** a model is missing a required V1 clip
- **THEN** generated validation reports the missing coverage and the model is not considered publication-complete

### Requirement: Sealed evidence is checkout-portable for repository text
Reference and proof evidence SHALL preserve content identity across Git checkout line-ending representation for repository text files while retaining byte-exact identity for binary files. Seal generation and verification SHALL use the same canonical evidence-hash rules.

#### Scenario: LF text is checked out as CRLF
- **WHEN** a sealed repository text input has identical Unicode bytes except for LF versus CRLF or lone-CR line endings
- **THEN** audit verification resolves the same evidence hash and does not mark the evidence stale solely because of line-ending representation

#### Scenario: Sealed text content changes
- **WHEN** any non-line-ending content in a sealed text input changes
- **THEN** audit verification reports stale evidence

#### Scenario: Binary evidence changes by one byte
- **WHEN** any byte in a sealed binary input changes
- **THEN** audit verification reports stale evidence without applying text normalization
