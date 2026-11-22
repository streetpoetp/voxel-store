# Spec Delta

## MODIFIED Requirements

### Requirement: Store domain roots have single responsibilities
The active repository layout SHALL separate reusable Assets, Styles, Profiles, Collections, Store policy/history, and derived Catalog outputs. Product asset discovery SHALL be restricted to Assets.

#### Scenario: Compatibility profile changes
- **WHEN** a profile under `Profiles/**` changes
- **THEN** compatibility/health evaluation uses that profile as reusable physical contract authority independently of collection policy

## ADDED Requirements

### Requirement: New production models are target-first and template-independent
New production asset authoring SHALL begin from the selected collection requirement or explicit user-requested form, current reusable style authority, applicable compatibility profile, and a new asset-specific design intent. It SHALL establish and accept a new Product Target before construction, density, VoxelSpec, proof, compatibility, or publication, and SHALL NOT depend on another asset as visual/model authority.

#### Scenario: Agent is asked to create a new weapon
- **WHEN** the requested physical form and applicable style/profile authority are known
- **THEN** the agent establishes a distinct asset identity and creates/accepts a new Product Target before authoring VoxelSpec source

#### Scenario: Existing asset could be copied as a shortcut
- **WHEN** another asset has similar geometry, palette, materials, or decorative motifs
- **THEN** the agent may inspect it for repository conventions but does not treat its visual/model choices as authority for the new asset

#### Scenario: Product Target creation is blocked
- **WHEN** a new Product Target cannot be generated, delivered byte-for-byte, or accepted under current style/provenance rules
- **THEN** production model creation stops before VoxelSpec authoring rather than fabricating a model-first substitute
