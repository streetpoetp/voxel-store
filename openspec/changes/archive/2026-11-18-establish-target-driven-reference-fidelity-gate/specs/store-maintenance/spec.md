# Spec Delta

## MODIFIED Requirements

### Requirement: Maintenance resolves style authority before reference generation
Before reference generation/repair, maintenance SHALL resolve the applicable style inputs and pinned Hero.Voxel workflow. For unchanged legacy v2 assets it may preserve the existing Composition-first flow. For new or intentionally redesigned target-driven assets it SHALL establish/accept Product Target first, then accept the Composition Sheet, then one Part Sheet at a time, and SHALL stop on unresolved style/target authority or visual-fidelity failure.

#### Scenario: Maintenance begins reference generation
- **WHEN** the selected asset job requires a new or repaired reference package and its applicable scoped style-reference set resolves successfully
- **THEN** maintenance follows the reference flow required by that package version, using Product Target first for target-driven v3 and Composition-first for unchanged legacy v2

#### Scenario: Style authority cannot be resolved
- **WHEN** any applicable scoped style-reference image is unavailable or the asset's style family cannot resolve its required authority
- **THEN** maintenance fails closed on the current asset before generating or accepting replacement construction sheets

#### Scenario: Generated sheet fails visual audit
- **WHEN** the current Product Target, Composition Sheet, or Part Sheet fails the applicable style, target-fidelity, or Hero.Voxel construction audit
- **THEN** maintenance repairs/regenerates that same evidence for the current asset and does not advance to another sheet or asset

#### Scenario: Equipment reference package is accepted
- **WHEN** an equipment package satisfies all visual/reference requirements of its declared contract version
- **THEN** maintenance may author or repair downstream density/source against the accepted authority and subsequently run generation, proof, and compatibility gates

#### Scenario: Maintenance begins legacy reference repair
- **WHEN** an unchanged v2 asset requires reference repair and its applicable scoped style-reference set resolves successfully
- **THEN** maintenance may follow the existing Composition-first sequential Part Sheet workflow without forcing target-driven migration

#### Scenario: Maintenance begins target-driven reference generation
- **WHEN** a new or intentionally redesigned asset requires a target-driven reference package and its applicable scoped style-reference set resolves successfully
- **THEN** maintenance first establishes and visually accepts the Product Target, then generates/audits Composition and Part Sheets against that target while following Hero.Voxel construction rules

#### Scenario: Style or Product Target authority cannot be resolved
- **WHEN** applicable style inputs are unavailable, Product Target provenance is unresolved, or the target itself fails visual acceptance
- **THEN** maintenance fails closed on the current asset before accepting downstream construction sheets

#### Scenario: Generated Composition Sheet fails target fidelity
- **WHEN** the Composition Sheet is structurally valid but materially simplifies the current Product Target's silhouette, proportions, construction richness, material/color hierarchy, or meaningful detail density
- **THEN** maintenance repairs/regenerates that Composition Sheet for the current asset and does not advance to Part Sheets, density, source, or another asset

#### Scenario: Generated Part Sheet fails target fidelity
- **WHEN** the current Part Sheet fails either target-fidelity or canonical Hero.Voxel construction audit
- **THEN** maintenance repairs/regenerates that same Part Sheet and does not advance to another part, source, or asset

#### Scenario: Target-driven equipment reference package is accepted
- **WHEN** Product Target, Composition Sheet, every required Part Sheet, target-fidelity evidence, and style acceptance are current
- **THEN** maintenance may advance to density selection when applicable and subsequently to source, generation, proof, and compatibility gates

#### Scenario: Existing equipment source disagrees with accepted reference authority
- **WHEN** an equipment migration/repair finds that existing VoxelSpec or proof output materially disagrees with the newly accepted Product Target/construction package
- **THEN** maintenance repairs downstream density/source/proof to match visual authority rather than simplifying the Product Target or construction sheets to preserve the existing model
