# Spec Delta

## ADDED Requirements

### Requirement: Published equipment declares logical attachment capability
Published reusable equipment SHALL use the current manifest contract to declare one shared logical attachment capability and one model-owned attachment anchor name.

#### Scenario: Equipment targets a hand slot
- **WHEN** equipment declares `attachment.capability: Hand`
- **THEN** publication treats `Hand` as a reusable physical compatibility capability and does not require a concrete avatar socket instance

#### Scenario: Equipment declares a form-specific slot
- **WHEN** equipment declares a capability such as `SwordSlot`, `BowSlot`, or another form-specific vocabulary outside the shared physical capability set
- **THEN** publication fails instead of coupling avatar models to equipment forms

#### Scenario: Equipment is side-reusable
- **WHEN** equipment declares capability `Hand` with compatible side `Any`
- **THEN** the same asset identity may bind to a conforming left or right Hand slot without creating left/right duplicate catalog assets

### Requirement: Published avatars own physical equipment slot sockets
A reusable avatar SHALL author its physical equipment slot instances as Hero.Voxel sockets on the avatar model using the canonical equipment-slot naming convention.

#### Scenario: Avatar publishes a hand attachment
- **WHEN** an avatar supports the `Hand` capability
- **THEN** its VoxelSpec contains one or more sockets named `Equip.Hand.<Side>.<Instance>` parented to an authored avatar part, where Side is `Left`, `Right`, or `Center`

#### Scenario: Avatar publishes multiple slots of one kind
- **WHEN** a body can physically carry multiple items requiring the same capability
- **THEN** its VoxelSpec authors multiple distinct `Equip.<Capability>.<Side>.<Instance>` sockets and publication preserves side plus multiplicity rather than collapsing the capability to one slot

#### Scenario: Avatar declares malformed equipment socket identity
- **WHEN** a purported equipment slot does not match `Equip.<Capability>.<Side>.<Instance>`, uses an unsupported side, or references a missing parent part
- **THEN** publication fails at source validation

### Requirement: Published equipment owns one Mount socket
Reusable equipment SHALL author exactly one Hero.Voxel socket whose name matches its manifest attachment anchor, with `Mount` as the baseline anchor name.

#### Scenario: Equipment has a model-owned grip
- **WHEN** equipment declares `attachment.anchor: Mount`
- **THEN** its VoxelSpec contains exactly one `Mount` socket that defines the asset-specific attachment frame

#### Scenario: Equipment relies on model origin as an implicit grip
- **WHEN** a reusable equipment asset lacks its declared Mount socket
- **THEN** publication fails rather than assuming the equipment root/pivot is the attachment point

### Requirement: Reusable models publish semantic animation clips
Every reusable avatar and equipment asset SHALL author the baseline Hero.Voxel semantic clips `Idle`, `WalkForward`, `Attack`, `Hit`, and `Death`.

#### Scenario: Semantic animation set is complete
- **WHEN** source validation compiles a reusable avatar or equipment asset
- **THEN** all five required semantic clips exist with `Idle` and `WalkForward` looping and `Attack`, `Hit`, and `Death` non-looping

#### Scenario: Equipment owns unique attack motion
- **WHEN** two equipment assets share semantic clip name `Attack`
- **THEN** each asset may author different animation tracks/curves while publication treats both as satisfying the same semantic motion contract

#### Scenario: Required semantic clip is missing
- **WHEN** a reusable model omits one required semantic clip or declares incompatible loop semantics
- **THEN** publication fails before generated proof is sealed

#### Scenario: Body declares side-reactive hand attack
- **WHEN** a body publishes both left and right Hand slots and declares side-reactive `Attack` behavior
- **THEN** publication requires distinct `Attack.Left` and `Attack.Right` authored clips in addition to the baseline semantic set

#### Scenario: Current single-part body authors side reaction
- **WHEN** a current avatar has only the authored `Body` part
- **THEN** its side-specific clips may target only `Body`; publication does not assume Torso, Shoulder, Arm, Hand, or other anatomical parts exist

#### Scenario: Current single-part equipment owns visible equipped animation
- **WHEN** an equipment model has one authored part and its Mount is parented to that part
- **THEN** publication requires Mount position to coincide with that part's pivot and rejects equipped semantic position tracks on that Mount-bearing part, allowing authored rotation around the grip without moving the grip point

### Requirement: Generated catalog projects attachment and motion coverage
Generated catalog/health output SHALL project attachment capability, anchor identity, side-aware slot addresses, counts by capability and side, compatible equipment-side policy, and semantic/base-side motion coverage from authoritative manifests and compiled source.

#### Scenario: Equipment is cataloged
- **WHEN** a conforming equipment asset publishes
- **THEN** generated catalog state can report its form, attachment capability, Mount anchor, and semantic motions without becoming their authority

#### Scenario: Avatar is cataloged
- **WHEN** a conforming avatar publishes
- **THEN** generated catalog state reports each available equipment slot capability, all concrete instance names, the count for each capability, and semantic motions
