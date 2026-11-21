# Spec Delta

## MODIFIED Requirements

### Requirement: Preview delegates voxel semantics to pinned Hero.Voxel
The preview workspace SHALL consume the complete `hero.studio` Unity package from the repository-pinned Hero Studio Git submodule as one whole embedded package and SHALL use that provider for assembly baking, animation baking, and baked-asset interpretation without creating a selective Hero.Voxel package projection or parallel voxel implementation.

#### Scenario: Pinned provider is available
- **WHEN** preview refresh resolves the exact Hero Studio submodule revision declared by the store
- **THEN** Unity loads the complete `hero.studio` package from that submodule and bakes the unit's current publication-validated generated geometry and animation artifacts through its canonical Hero.Voxel APIs

#### Scenario: Pinned submodule provider is available
- **WHEN** the repository submodule is initialized at the pinned revision
- **THEN** bootstrap exposes the complete `hero.studio` package as one embedded package junction and does not select, copy, or replace individual Hero Studio modules

#### Scenario: Provider is missing or revision-incompatible
- **WHEN** the Hero Studio submodule is missing, uninitialized, or does not match the pinned provider revision
- **THEN** preview refresh fails before baking or presenting new results

#### Scenario: Full provider requires its declared render pipeline
- **WHEN** the complete Hero Studio Unity package declares and uses its supported render pipeline
- **THEN** the preview project configures that render pipeline rather than patching or replacing provider-owned preview shaders