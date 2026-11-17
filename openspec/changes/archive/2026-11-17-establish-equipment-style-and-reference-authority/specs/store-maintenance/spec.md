# Spec Delta

## MODIFIED Requirements

### Requirement: Maintenance resolves style authority before reference generation
Before reference generation/repair, maintenance SHALL resolve the style inputs applicable to the selected asset kind and the pinned Hero.Voxel workflow. It SHALL accept the Composition Sheet first, then one Part Sheet at a time. Equipment jobs SHALL treat accepted reference authority as upstream of source/proof and SHALL stop on unresolved style authority or visual audit failure.

#### Scenario: Maintenance begins reference generation
- **WHEN** the selected asset job requires a new or repaired reference package and its applicable scoped style-reference set resolves successfully
- **THEN** maintenance conditions generation on that scoped style authority while following Hero.Voxel's canonical Composition-first sequential Part Sheet workflow

#### Scenario: Style authority cannot be resolved
- **WHEN** any applicable scoped style-reference image is unavailable or the asset's style family cannot resolve the required asset-kind reference set
- **THEN** maintenance fails closed on the current asset before generating or accepting replacement construction sheets

#### Scenario: Generated sheet fails visual audit
- **WHEN** the Composition Sheet or current Part Sheet fails either the active scoped-style audit or canonical Hero.Voxel construction audit
- **THEN** maintenance repairs/regenerates that same sheet for the current asset and does not advance to another Part Sheet or asset

#### Scenario: Equipment reference package is accepted
- **WHEN** an equipment Composition Sheet and every required equipment Part Sheet pass visual audit
- **THEN** maintenance may author or repair that equipment's source against the accepted reference package and subsequently run generation, proof, and compatibility gates

#### Scenario: Existing equipment source disagrees with accepted reference authority
- **WHEN** an equipment migration/repair finds that existing VoxelSpec or proof output materially disagrees with the newly accepted visual reference package
- **THEN** maintenance repairs the equipment source to match reference authority rather than rewriting the reference package merely to preserve the existing model
