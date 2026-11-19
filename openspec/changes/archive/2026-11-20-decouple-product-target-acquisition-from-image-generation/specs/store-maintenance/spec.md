# Spec Delta

## MODIFIED Requirements

### Requirement: New production models are target-first and template-independent
New production asset authoring SHALL begin from the selected collection requirement or explicit user-requested form, current reusable style authority, applicable compatibility profile, and a new asset-specific design intent. It SHALL acquire and accept a new Product Target before construction, density, VoxelSpec, proof, compatibility, or publication, and SHALL NOT depend on another asset as visual/model authority.

#### Scenario: Agent is asked to create a new weapon
- **WHEN** the requested physical form and applicable style/profile authority are known
- **THEN** the agent establishes a distinct asset identity and acquires/accepts a new Product Target before authoring VoxelSpec source

#### Scenario: Existing asset could be copied as a shortcut
- **WHEN** another asset has similar geometry, palette, materials, or decorative motifs
- **THEN** the agent may inspect it for repository conventions but does not treat its visual/model choices as authority for the new asset

#### Scenario: Product Target creation is blocked
- **WHEN** no acquisition path can establish repository-owned Product Target bytes that pass current provenance, style, and visual acceptance
- **THEN** production model creation stops before Composition, density, or VoxelSpec authoring rather than fabricating a model-first substitute

#### Scenario: Blocked production creation has no sample fallback
- **WHEN** Product Target authority remains blocked for a new production asset
- **THEN** the workflow reports the blocked Product Target gate and does not continue the asset as Samples content, experimental source, or any alternate publication lane
