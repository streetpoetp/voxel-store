# Spec Delta

## MODIFIED Requirements

### Requirement: Preview remains Editor-only
The preview workspace SHALL exist only for Editor inspection, validation, Play Mode review, and capture orchestration and SHALL NOT introduce a production game runtime, build target, gameplay-facing voxel ownership boundary, or reusable voxel runtime API into voxel-store. Entering Play Mode for the canonical grid SHALL execute pinned Hero.Voxel directly and SHALL NOT make the generated runtime grid persistent product content.

#### Scenario: Developer inspects a baked unit
- **WHEN** a developer selects or previews a baked assembly or animation directly outside the runtime grid
- **THEN** existing Hero.Voxel Editor preview and Scene Debug behavior can be used without adding gameplay semantics or store-owned runtime components

#### Scenario: Grid is generated
- **WHEN** the all-units grid is realized during Play Mode
- **THEN** its provider runtime objects remain disposable Editor review state and are not treated as production runtime content or serialized scene authority

#### Scenario: Proof requires Play Mode execution
- **WHEN** a registered capture scenario or the canonical interactive grid enters Play Mode to exercise the real provider runtime
- **THEN** Editor or capture orchestration drives Hero Studio directly and does not depend on a voxel-store runtime assembly or reusable runtime wrapper

### Requirement: Canonical interactive grid runs through Play Mode Hero.Voxel runtime
The canonical all-model interactive grid SHALL realize current baked content during Unity Play Mode through the pinned Hero.Voxel render and Actor runtime, and SHALL use provider-owned runtime inspection for interactive animation controls instead of rebuilding the grid through Scene Debug.

#### Scenario: Developer refreshes the interactive grid
- **WHEN** Voxel Store/Refresh Preview Grid completes baking from current published state
- **THEN** the preview opens its disposable shell, enters Play Mode, and realizes the grid through a provider VoxelRenderWorld

#### Scenario: Avatar entry carries equipment
- **WHEN** an avatar grid entry is assigned deterministic carried equipment
- **THEN** the avatar and equipment are composed through one existing VoxelActorInstance using the authored avatar socket and equipment Mount frame

#### Scenario: Developer selects a runtime grid host
- **WHEN** a developer previously relied on a per-model hierarchy selection host
- **THEN** the grid contains no such host and the developer selects the realized model directly using the provider Inspector

#### Scenario: Developer selects a rendered grid model
- **WHEN** a developer picks or browses an avatar, carried equipment, or standalone equipment using the provider runtime Inspector
- **THEN** the provider selects its real render instance and exposes its existing canonical track, with preview playback delegated by the grid owner and no selection hosts or store-owned player

#### Scenario: Play Mode ends
- **WHEN** the Editor leaves Play Mode
- **THEN** runtime grid instances and actors are disposed and preview authority is revoked without being serialized as current preview or store authority

#### Scenario: Required provider inspection capability is unavailable
- **WHEN** the pinned Hero Studio provider cannot supply the required Play Mode runtime inspection contract
- **THEN** canonical grid refresh fails clearly rather than restoring a voxel-store playback component or falling back to Scene Debug as a second grid runtime
