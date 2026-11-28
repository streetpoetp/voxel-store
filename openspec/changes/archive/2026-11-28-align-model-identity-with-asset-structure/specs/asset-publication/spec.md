# Spec Delta

## ADDED Requirements

### Requirement: Active asset identity follows canonical model structure
Every active asset SHALL use one canonical `ModelName` shared by its model directory and source base. Its published id and UnitRef SHALL be derived from kind plus the deterministic `ModelKey` for that `ModelName`, without collection, origin, gameplay-domain, or other non-structural namespace segments.

#### Scenario: Avatar identity matches its model structure
- **WHEN** an avatar is authored at `Assets/Avatars/Wraith01/asset.yaml` with source base `./Source/Wraith01`
- **THEN** its canonical identity is `id: avatar.wraith-01` and `unitRef: voxel-store/avatar/wraith-01`

#### Scenario: Equipment identity matches its model structure
- **WHEN** equipment is authored at `Assets/Equipment/Rifle02/asset.yaml` with source base `./Source/Rifle02`
- **THEN** its canonical identity is `id: equipment.rifle-02` and `unitRef: voxel-store/equipment/rifle-02`

#### Scenario: Manifest identity drifts from the model structure
- **WHEN** an active asset's directory name, source-base model name, id, or UnitRef does not resolve to one canonical ModelName/ModelKey identity
- **THEN** validation fails before the asset can publish or appear in preview-plan output

#### Scenario: Identity embeds a legacy ownership namespace
- **WHEN** an active asset id or UnitRef inserts a collection/origin segment such as `dungeon`, `foundation`, or `humanoid` that is not part of the canonical ModelName
- **THEN** validation rejects the identity even when the value is otherwise unique

### Requirement: ModelKey derivation is deterministic
The store SHALL derive `ModelKey` from `ModelName` by tokenizing UpperCamel words, acronym runs, and decimal runs, lowercasing alphabetic tokens, preserving decimal digits including leading zeroes, and joining tokens with `-`. The same ModelName SHALL resolve to the same ModelKey everywhere store identity is evaluated.

#### Scenario: Single-word model has a numeric variant
- **WHEN** `ModelName` is `Rifle02`
- **THEN** `ModelKey` is `rifle-02`

#### Scenario: Multi-word model has a numeric variant
- **WHEN** `ModelName` is `FoundationGuard01` or `WarPick01`
- **THEN** the corresponding ModelKey is `foundation-guard-01` or `war-pick-01`

#### Scenario: Model name contains an acronym run
- **WHEN** a future valid ModelName contains an acronym run followed by another word or decimal run
- **THEN** the acronym is treated as one lowercase token rather than being split into one-character identity segments

### Requirement: Legacy active identities migrate explicitly and remain reserved
When an active published asset is moved onto the canonical identity convention, the migration SHALL replace its id and UnitRef as one explicit authority change and SHALL reserve the complete previous identity pair in retired identity history. The store SHALL NOT expose legacy aliases as active identity.

#### Scenario: Existing published asset adopts the canonical identity
- **WHEN** `dungeon.avatar.wraith-01` / `voxel-store/avatar/dungeon/wraith-01` is explicitly migrated while its canonical ModelName remains `Wraith01`
- **THEN** the active manifest becomes `avatar.wraith-01` / `voxel-store/avatar/wraith-01` and the previous pair is recorded as retired

#### Scenario: Migrated legacy identity is reused later
- **WHEN** a future asset attempts to declare an id or UnitRef reserved by this migration
- **THEN** validation fails under the existing retired-identity rule

#### Scenario: Migration updates identity-bearing evidence
- **WHEN** an active id or UnitRef changes under this migration
- **THEN** compatibility evidence and derived catalog/health/preview projections are regenerated or renamed from current authority so no active derived record retains the legacy identity

#### Scenario: Migration does not redesign the model
- **WHEN** an asset receives only the approved identity migration
- **THEN** its model directory, source model name, visual/reference authority, geometry, animation, sockets, form, and other non-identity publication semantics remain unchanged

## MODIFIED Requirements

### Requirement: Published identity is independent of repository collection paths
The canonical asset kind and ModelName SHALL determine identity when an asset is first created or explicitly identity-migrated. After publication, moving the same asset through non-semantic repository organization while preserving kind and ModelName SHALL NOT change its published id or UnitRef. Renaming ModelName or changing kind is an identity migration, not an ordinary path move.

#### Scenario: Foundation-owned asset path moves to reusable Assets root
- **WHEN** a published asset moves from a collection-specific legacy path to the reusable `Assets/**` structure while preserving its asset kind and canonical ModelName
- **THEN** the move is treated as repository-structure migration and its published id and UnitRef remain unchanged

#### Scenario: Published asset moves without changing kind or ModelName
- **WHEN** a published asset is reorganized while its kind and canonical ModelName remain unchanged
- **THEN** the move is treated as repository-structure migration and its published id and UnitRef remain unchanged

#### Scenario: Published model directory is renamed
- **WHEN** a published asset's canonical ModelName changes through a directory/source-model rename
- **THEN** validation requires an explicit identity migration that updates the canonical id/UnitRef and reserves the previous published identity rather than silently deriving a new identity
