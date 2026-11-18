# Spec Delta

## MODIFIED Requirements

### Requirement: Reference evidence follows the canonical construction contract
Agent-authored assets SHALL use visually construction-authoritative Hero.Voxel PNG sheets. Legacy reference-contract v2 packages MAY remain valid unchanged. New or intentionally redesigned assets entering the target-driven workflow SHALL use reference-contract v3: one declared Product Target, one accepted 3:2 Composition Sheet that freezes `RequiredParts`, exactly one accepted Part Sheet per required part, and current target-fidelity evidence before geometry authoring is accepted. Acceptance SHALL also be current against active style authority.

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
