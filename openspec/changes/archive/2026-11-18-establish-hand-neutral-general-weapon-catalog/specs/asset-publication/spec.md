# Spec Delta

## MODIFIED Requirements

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

### Requirement: Asset metadata remains gameplay-neutral
Store-level asset contracts SHALL describe visual identity, construction, physical compatibility, reusable physical form, palette/style family, bounds, and authoring evidence only. They SHALL NOT own handed gameplay rules, two-hand occupancy, damage, rarity, class, AI, progression, economy, balance, attack behavior, or other consuming-game semantics.

#### Scenario: Gameplay metadata is placed in an asset manifest
- **WHEN** an asset manifest attempts to declare a gameplay-only field such as damage, rarity, attack behavior, or required hand occupancy as store authority
- **THEN** store validation rejects that field or treats it as outside the supported asset contract rather than publishing it as shared voxel metadata

#### Scenario: Equipment declares a physical form
- **WHEN** equipment declares a form such as sword, bow, spear, or rifle solely to distinguish reusable visual and physical asset coverage
- **THEN** the store accepts that classification without assigning combat behavior, balance, or handed gameplay meaning to it
