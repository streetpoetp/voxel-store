# Spec Delta

## MODIFIED Requirements

### Requirement: Tool dependency resolution is explicit and portable
Maintenance SHALL resolve the Hero.Voxel authoring/tool contract from the repository-owned `Dependencies/hero-studio` Git submodule, pinned to the exact revision declared by repository dependency state and aligned with `toolchain/hero-voxel.lock.yaml`. Production maintenance SHALL NOT require a developer-local Hero Studio checkout or `HERO_STUDIO_ROOT`.

#### Scenario: Repository is moved to another machine
- **WHEN** the store is checked out at a different filesystem location and its Git submodules are initialized
- **THEN** maintenance resolves the declared Hero Studio/Hero.Voxel contract from the repository submodule without requiring the original developer-local checkout path or environment variable

#### Scenario: Declared Hero.Voxel tool cannot be resolved
- **WHEN** maintenance requires Hero.Voxel verification but `Dependencies/hero-studio` is missing, uninitialized, or otherwise unavailable
- **THEN** the run fails closed with a dependency-resolution error and does not mark affected assets as healthy

#### Scenario: Declared Hero Studio submodule is unavailable
- **WHEN** repository bootstrap finds that `Dependencies/hero-studio` has not been initialized
- **THEN** the dependency check reports the missing submodule and instructs the user to initialize repository submodules before Hero.Voxel work proceeds
#### Scenario: Submodule revision does not match provider lock
- **WHEN** the checked-out Hero Studio submodule revision differs from `toolchain/hero-voxel.lock.yaml`
- **THEN** maintenance fails closed with a revision-mismatch error before Hero.Voxel work proceeds
