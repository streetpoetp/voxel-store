# Spec Delta

## ADDED Requirements

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
