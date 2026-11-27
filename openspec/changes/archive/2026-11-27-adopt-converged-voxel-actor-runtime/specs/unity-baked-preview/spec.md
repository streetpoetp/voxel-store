# Spec Delta

## MODIFIED Requirements

### Requirement: Preview delegates generic attachment presentation to pinned provider
The Unity preview SHALL delegate generic root/attachment composition, reference-influence/release-basis state, composite bounds, reset, and streamed activation lifecycle directly to pinned Hero Studio Actor and Actor.Streaming. Voxel-store SHALL keep only preview semantics and SHALL NOT depend on retired Presentation packages or mirror Actor composition state.

#### Scenario: Direct avatar equipment is composed
- **WHEN** the runtime gallery composes a Body with one or more equipment instances
- **THEN** one provider Actor owns root/attachment bindings, canonical model tracks, reference-influence composition, release/recovery state, refresh/reset, and composite bounds while voxel-store supplies only its semantic socket/anchor and motion choices

#### Scenario: Streamed mutable equipment is composed
- **WHEN** the mutable streamed proof attaches, rebinds, replaces, detaches, releases, or reacquires equipment
- **THEN** the provider streamed Actor owns activation-scoped attachment identity, mutable topology, model tracks, release/recovery composition, and warm-activation reset without a second Presentation handle or state table

#### Scenario: Provider presentation capability is unavailable
- **WHEN** the pinned Hero Studio revision no longer publishes the former direct or streamed Presentation packages
- **THEN** the preview compiles and runs against Actor / Actor.Streaming alone rather than restoring, copying, or locally emulating the retired abstraction

### Requirement: Runtime gallery uses shared provider realization
The runtime gallery SHALL realize standalone models through pinned Hero Studio render mechanisms and composed avatars through the pinned Actor capability layered over provider render mechanisms. Voxel-store SHALL NOT own a voxel evaluator, generic attachment solver, release-basis resolver, fallback renderer, competing composition lifetime, or compatibility facade for the retired Presentation packages.

#### Scenario: Gallery realizes a multipart model
- **WHEN** an imported baked model is displayed standalone
- **THEN** all its authored parts are realized by the provider render instance at authored scale and the gallery frames it from provider-evaluated geometry

#### Scenario: Gallery realizes composed avatars
- **WHEN** an avatar has zero or more carried equipment instances
- **THEN** each model uses provider-owned model-local playback and one provider Actor coordinates generic root/attachment visual composition while voxel-store supplies only semantic socket/anchor and motion choices

#### Scenario: Gallery is rebuilt or unloaded
- **WHEN** preview instances are replaced or the gallery exits
- **THEN** the preview releases its provider Actor and caller-owned render instances without stale submissions, duplicate realization, or retained attachment visual state

#### Scenario: Provider contract is unavailable
- **WHEN** the pinned dependency lacks the required Actor independent-track/composition or Actor.Streaming mutable-lifecycle capabilities
- **THEN** adoption is blocked rather than copying provider code or retaining a parallel runtime

### Requirement: Shared preview execution preserves playback controls
The interactive preview and registered capture SHALL drive the same provider-owned per-model playback tracks used by the Actor. Explicit speed, loop, direct phase, reset, lifecycle, and model-local visual transform controls SHALL remain observable independently for every model track.

#### Scenario: Playback loop differs from authored default
- **WHEN** the user explicitly selects looping or non-looping playback for one model
- **THEN** the preview respects that choice without changing authored clip assets or another model's playback state

#### Scenario: Capture samples a phase
- **WHEN** the registered runner requests deterministic checkpoints
- **THEN** it samples the declared root and attachment model tracks at their requested phases, refreshes the provider Actor's final attachment composition, submits render state, and only then acquires attested Game View pixels

#### Scenario: Preview stops or switches motion
- **WHEN** playback is stopped, reset, or switched on one model track
- **THEN** that provider-owned track changes without stale pose or attachment visual state influencing it and without resetting unrelated tracks

### Requirement: Runtime avatar composition uses explicit presentation ownership
The Play Mode preview SHALL represent each composed avatar through one preview semantic owner backed by one provider Actor exposing root/bounds/orientation/attachment identities while delegating physical binding, reference-influence composition, and independent model-local animation playback to that Actor. The preview semantic owner SHALL NOT wrap the Actor in a second generic composition abstraction.

#### Scenario: Avatar presentation is created
- **WHEN** the runtime gallery composes a published rigid-Body avatar
- **THEN** exactly one provider Actor backs the Body composition surface and canonical local Right, Up and Forward directions exposed by the preview semantic owner

#### Scenario: Canonical orientation is queried
- **WHEN** preview motion needs a directional basis for a current canonical model
- **THEN** the preview resolves visual Forward from canonical baked orientation rather than requiring each motion to hard-code an unexplained XYZ direction

### Requirement: Runtime gallery provides bounded mutable streamed actor proof
The Unity preview SHALL provide one bounded preview-only proof that realizes current baked avatar/equipment assets through the pinned streamed Actor capability over the existing resource and actor presenter pools while preserving independent model playback.

#### Scenario: Streamed proof initializes
- **WHEN** the dedicated mutable-streamed capture scenario starts
- **THEN** it acquires one composite-capable streamed Actor with a root and zero current mutable attachments through the existing pools, without a parallel renderer, cache, pool, resolver, or Presentation facade

#### Scenario: Attachments mutate during one activation
- **WHEN** the proof adds, detaches, rebinds, or replaces equipment while tracks are active
- **THEN** Actor attachment identities and stale-handle rules remain correct while root and unaffected attachment playback continue unchanged

#### Scenario: Actor is released and reacquired
- **WHEN** the proof releases the streamed Actor to its warm pool and reacquires it
- **THEN** prior runtime-added attachments and activation-local playback/visual state are absent, prior activation handles fail closed, root presentation remains reusable, and new attachments can be added normally

#### Scenario: Existing gallery verification runs
- **WHEN** ordinary runtime-gallery or avatar-motion verification runs
- **THEN** the mutable proof surface does not alter model counts, render-instance assertions, composition behavior, or independent track semantics

### Requirement: Graphical capture proves mutable streamed composition
The preview capture registry SHALL expose a provider-attested `mutable-streamed-actor-gallery` scenario that captures deterministic high-level mutable-Actor checkpoints from the generated runtime gallery.

#### Scenario: Mutable capture runs
- **WHEN** `tools/capture-game-view.ps1 -Scenario mutable-streamed-actor-gallery` completes
- **THEN** attested Game View images cover initial multi-attachment composition, released root movement, continuous recovery, rebind, detach, replacement, root-only warm reacquire, and post-reacquire attachment

#### Scenario: A composed checkpoint is captured
- **WHEN** one or more equipment attachments are active
- **THEN** the runner samples independently declared provider model tracks, verifies current Actor attachment membership and finite visual transforms, refreshes Actor composition, submits provider render state, and only then acquires Game View pixels

### Requirement: Streamed presenter is the sole mutable topology authority
The provider streamed Actor SHALL remain the sole authority for logical mutable topology and final composite visual state. Voxel-store SHALL consume only the Actor's activation-scoped attachment identities, current membership/counts, model tracks, bounds, and mutation operations and SHALL NOT maintain parallel membership, resolver, release-basis, or Presentation-handle state.

#### Scenario: Attachment is added
- **WHEN** the preview successfully attaches equipment during one streamed Actor activation
- **THEN** the Actor attachment identity becomes current, attachment count reflects the new model, and no voxel-store topology or resolver shadow is created

#### Scenario: Attachment is detached
- **WHEN** one mutable attachment is detached
- **THEN** Actor membership no longer contains its identity, the old identity fails closed, and unaffected model tracks preserve their state

#### Scenario: Attachment is rebound
- **WHEN** an active attachment is rebound to another valid socket
- **THEN** the same Actor attachment identity and model track remain current while subsequent provider-owned visual composition uses the rebound reference

#### Scenario: Attachment is replaced
- **WHEN** an active attachment is transactionally replaced
- **THEN** the provider-defined Actor attachment identity semantics are preserved, the replacement receives fresh provider-defined playback/visual state, and root/unrelated attachment tracks remain unchanged

#### Scenario: Presenter is released and reacquired
- **WHEN** the proof releases the streamed Actor to its warm pool and reacquires it
- **THEN** prior activation identities fail closed, current attachment count returns to the configured baseline, provider-defined retained track/visual state is reset, and no preview membership or resolver shadow requires synchronization
