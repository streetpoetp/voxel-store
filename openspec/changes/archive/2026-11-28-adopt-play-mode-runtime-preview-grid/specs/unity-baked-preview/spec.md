# Spec Delta

## ADDED Requirements

### Requirement: Canonical interactive grid runs through Play Mode Hero.Voxel runtime
The canonical all-model interactive grid SHALL realize current baked content during Unity Play Mode through the pinned Hero.Voxel render and Actor runtime, and SHALL use provider-owned runtime inspection for interactive animation controls instead of rebuilding the grid through Scene Debug.

#### Scenario: Developer refreshes the interactive grid
- **WHEN** Voxel Store/Refresh Preview Grid completes baking from current published state
- **THEN** the preview opens its disposable shell, enters Play Mode, and realizes the grid through a provider VoxelRenderWorld

#### Scenario: Avatar entry carries equipment
- **WHEN** an avatar grid entry is assigned deterministic carried equipment
- **THEN** the avatar and equipment are composed through one existing VoxelActorInstance using the authored avatar socket and equipment Mount frame

#### Scenario: Developer selects a runtime grid host
- **WHEN** a developer selects an avatar, carried-equipment, or standalone-equipment host in the Play Mode hierarchy
- **THEN** Hero Studio's runtime Inspector is registered against that model's canonical runtime track without requiring a voxel-store preview/player component

#### Scenario: Play Mode ends
- **WHEN** the Editor leaves Play Mode
- **THEN** runtime grid instances, actors, inspection registrations, and selection hosts disappear without being serialized as current preview or store authority

#### Scenario: Required provider inspection capability is unavailable
- **WHEN** the pinned Hero Studio provider cannot supply the required Play Mode runtime inspection contract
- **THEN** canonical grid refresh fails clearly rather than restoring a voxel-store playback component or falling back to Scene Debug as a second grid runtime

## MODIFIED Requirements

### Requirement: Preview remains Editor-only
The preview workspace SHALL exist only for Editor inspection, validation, Play Mode review, and capture orchestration and SHALL NOT introduce a production game runtime, build target, gameplay-facing voxel ownership boundary, or reusable voxel runtime API into voxel-store. Entering Play Mode for the canonical grid SHALL execute pinned Hero.Voxel directly and SHALL NOT make the generated runtime grid persistent product content.

#### Scenario: Developer inspects a baked unit
- **WHEN** a developer selects or previews a baked assembly or animation directly outside the runtime grid
- **THEN** existing Hero.Voxel Editor preview and Scene Debug behavior can be used without adding gameplay semantics or store-owned runtime components

#### Scenario: Grid is generated
- **WHEN** the all-units grid is realized during Play Mode
- **THEN** its provider runtime objects and plain selection hosts remain disposable Editor review state and are not treated as production runtime content or serialized scene authority

#### Scenario: Proof requires Play Mode execution
- **WHEN** a registered capture scenario or the canonical interactive grid enters Play Mode to exercise the real provider runtime
- **THEN** Editor or capture orchestration drives Hero Studio directly and does not depend on a voxel-store runtime assembly or reusable runtime wrapper

### Requirement: Preview loads authored animation for every composed model
The interactive preview SHALL load the Hero.Voxel baked animation asset associated with each discovered body and equipment assembly so every realized model owns an independent canonical provider model track available to the shared runtime Inspector.

#### Scenario: Avatar and weapon are composed
- **WHEN** both models have required baked semantic clips
- **THEN** the preview realizes each baked animation asset on its own provider model track and registers those tracks for independent inspection

#### Scenario: Required animation asset is unavailable
- **WHEN** a model assembly exists but its current semantic animation asset cannot be resolved
- **THEN** independent playback proof fails for that model instead of synthesizing canonical motion procedurally or creating a local playback controller

### Requirement: Baked animation inspection uses provider Editor capability
Interactive animation inspection SHALL use the pinned Hero Studio Editor capability appropriate to the target: direct baked-asset/Edit Mode inspection may use provider asset or Scene Debug tooling, while the canonical all-model Play Mode grid SHALL use provider runtime inspection over the current realized model track. Voxel-store SHALL NOT own a runtime motion player or duplicated clip inventory.

#### Scenario: Developer inspects authored variants
- **WHEN** a realized grid model's baked animation contains baseline and variant clips
- **THEN** provider runtime inspection exposes the model's actual authored clip inventory and supports selecting or scrubbing those clips through its canonical track without a store-maintained motion list

#### Scenario: Developer inspects a baked asset directly in Edit Mode
- **WHEN** a developer opens a baked assembly or animation outside the canonical runtime grid
- **THEN** existing Hero.Voxel asset Inspector or Scene Debug behavior remains available as separate provider tooling

#### Scenario: Baked clip inventory changes
- **WHEN** rebaking adds or removes authored clips
- **THEN** inspection reflects the current baked animation asset without changing voxel-store runtime code because no such runtime layer exists