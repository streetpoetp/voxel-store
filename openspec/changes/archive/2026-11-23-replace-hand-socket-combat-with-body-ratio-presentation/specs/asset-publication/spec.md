# Spec Delta

## ADDED Requirements

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

## MODIFIED Requirements

### Requirement: Publication status is evidence-derived
An asset SHALL be publishable only when its manifest, identity, reference-package coverage, authoritative source, deterministic generation, required standalone visual proof, applicable rigid-body composition proof, and declared budget checks all pass. Publication status SHALL be derived from current evidence rather than a manually editable approval flag.

#### Scenario: All publication gates pass
- **WHEN** every required publication gate for an asset passes against current authoritative inputs
- **THEN** generated health and catalog outputs report the asset as published

#### Scenario: A required gate fails after publication
- **WHEN** current validation finds that any required publication gate for a previously published asset no longer passes
- **THEN** regenerated health/catalog state no longer reports that asset as healthy published content and identifies the failing gate

## REMOVED Requirements

### Requirement: Asset records own stable semantic identity
**Reason**: The identity rule is being rewritten to remove compatibility-classification and physical-hand assumptions from the stable asset contract.
**Migration**: Use `Asset records own stable placement-neutral semantic identity`.

### Requirement: Equipment reference authority remains implementation-neutral
**Reason**: The requirement is being rewritten to make normalized Body-ratio placement and consumer indices explicitly downstream and non-authoritative.
**Migration**: Use `Equipment reference authority remains ratio-placement-neutral`.
