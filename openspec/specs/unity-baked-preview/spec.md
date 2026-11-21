# unity-baked-preview Specification

## Purpose

Defines a non-authoritative Unity Editor workspace for inspecting the real Hero.Voxel baked outputs of voxel-store assets and comparing every currently published unit together in one scene grid.

## Requirements

### Requirement: Preview uses current published store state
The preview workflow SHALL derive its unit set from the store's current authoritative publication evaluation and SHALL include only assets that currently satisfy publication.

#### Scenario: Published avatars and equipment exist
- **WHEN** preview refresh evaluates the store and published avatar and equipment assets are present
- **THEN** every published asset of both kinds is selected for preview exactly once

#### Scenario: An asset is not currently published
- **WHEN** an asset fails any current publication requirement
- **THEN** preview refresh excludes that asset rather than treating existing baked output as proof of publication

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
### Requirement: Store source remains authoritative
The preview workspace SHALL derive selected assets and their generated bake inputs from current authoritative store publication state and SHALL NOT require a copied Unity-side authoring source, alternate identity record, or preview-time source regeneration.

#### Scenario: Published source is outside the Unity project
- **WHEN** a published asset has current generated Hero.Voxel geometry/animation artifacts beside its authoritative source under voxel-store `Assets/**/Source/**`
- **THEN** preview baking reads those publication-validated generated artifacts in place and writes only derived Unity outputs inside the preview project

#### Scenario: Preview outputs are deleted
- **WHEN** baked Unity preview outputs are removed while store sources remain intact
- **THEN** the preview can reconstruct them without changing store publication or source authority

### Requirement: Canonical scene shows all published units in a grid
The canonical preview scene SHALL display every unit selected by the current preview refresh in one deterministic, non-overlapping grid while preserving each baked unit's authored world scale.

#### Scenario: Preview contains multiple units
- **WHEN** preview refresh completes with two or more published units
- **THEN** the scene contains one visible grid entry per unit in deterministic identity order with sufficient shared spacing to prevent entries from overlapping

#### Scenario: Avatars and equipment are both published
- **WHEN** the current published set includes both avatars and equipment
- **THEN** both kinds appear together in the canonical scene grid rather than requiring separate scenes

#### Scenario: Units have different authored sizes
- **WHEN** baked units have different world-space bounds
- **THEN** grid layout preserves their baked scale instead of independently normalizing each unit to the same visual size

### Requirement: Grid refresh removes stale preview entries
The canonical grid SHALL be reconstructed from the current preview unit set so saved or previously generated preview state cannot leave retired, unpublished, or removed units visible as current content.

#### Scenario: Published set changes
- **WHEN** a previously previewed unit is no longer published or a newly published unit is added
- **THEN** the next preview refresh removes the stale entry and adds the new entry so the grid exactly matches the current published set

#### Scenario: No units are published
- **WHEN** preview refresh finds no currently published units
- **THEN** the canonical scene contains no generated unit entries and reports the empty preview state

### Requirement: Baked preview assets are derived workspace state
Unity assembly assets, animation assets, captures, and generated grid objects SHALL be treated as disposable preview state and SHALL NOT become voxel-store authoring, publication, or compatibility authority.

#### Scenario: Derived bake differs after source change
- **WHEN** authoritative source changes and preview refresh produces different baked assets
- **THEN** the new derived preview replaces the old preview without requiring an asset identity migration

#### Scenario: Repository is checked out on another machine
- **WHEN** another machine resolves the pinned provider and rebuilds the preview workspace
- **THEN** machine-local baked outputs can be recreated without relying on committed Unity bake artifacts or developer-local paths

### Requirement: Preview remains Editor-only
The preview workspace SHALL exist for Unity Editor inspection and SHALL NOT introduce a production game runtime, build target, or gameplay-facing voxel ownership boundary into voxel-store.

#### Scenario: Developer inspects a baked unit
- **WHEN** a developer selects or previews a baked assembly or animation in the workspace
- **THEN** existing Hero.Voxel Editor preview and Scene Debug behavior can be used without adding gameplay semantics to the store

#### Scenario: Grid is generated
- **WHEN** the all-units grid is built in the canonical preview scene
- **THEN** its generated scene objects remain preview-only and are not treated as production runtime content
