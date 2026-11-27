# Spec Delta

## ADDED Requirements

### Requirement: Preview delegates generic attachment presentation to pinned provider
The Unity preview SHALL delegate generic attachment reference-influence composition, retained release-basis state, high-level attachment visual lifecycle, and streamed activation reset behavior to the pinned Hero Studio general presentation capability. Voxel-store SHALL retain only preview/product semantics such as model selection, socket/anchor names, motion candidates, phases, capture checkpoints, and visual acceptance.

#### Scenario: Direct avatar equipment is composed
- **WHEN** the runtime gallery composes a Body with one or more equipment instances
- **THEN** the pinned provider owns generic attachment visual composition and the preview does not maintain a second reference-influence resolver or release-basis state

#### Scenario: Streamed mutable equipment is composed
- **WHEN** the mutable streamed proof attaches, rebinds, replaces, detaches, releases, or reacquires equipment
- **THEN** high-level attachment identity and visual-state reset come from the pinned provider presentation contract while voxel-store retains only current semantic proof inputs and checkpoints

#### Scenario: Provider presentation capability is unavailable
- **WHEN** the pinned Hero Studio revision lacks the required direct or streamed general presentation capability
- **THEN** preview adoption fails closed rather than copying provider presentation logic back into voxel-store

## MODIFIED Requirements

### Requirement: Runtime gallery uses shared provider realization
The runtime gallery SHALL realize standalone models through pinned Hero Studio render mechanisms and composed avatars through the pinned general presentation capability layered over provider actor/render mechanisms. Voxel-store SHALL NOT own a voxel evaluator, generic attachment solver, release-basis resolver, fallback renderer, or competing presentation lifetime.

#### Scenario: Gallery realizes a multipart model
- **WHEN** an imported baked model is displayed standalone
- **THEN** all its authored parts are realized by the provider render instance at authored scale and the gallery frames it from provider-evaluated geometry

#### Scenario: Gallery realizes composed avatars
- **WHEN** an avatar has zero or more carried equipment instances
- **THEN** each model uses provider-owned model-local playback and the provider presentation capability coordinates generic root/attachment visual composition while voxel-store supplies only its semantic socket/anchor and motion choices

#### Scenario: Gallery is rebuilt or unloaded
- **WHEN** preview instances are replaced or the gallery exits
- **THEN** the preview releases its provider presentation owner and caller-owned render instances without stale submissions, duplicate realization, or retained attachment visual state

#### Scenario: Provider contract is unavailable
- **WHEN** the published pinned dependency lacks the required general presentation, independent-track actor/render, or streamed-presentation capabilities
- **THEN** adoption is blocked rather than copying provider code or retaining a parallel runtime

### Requirement: Shared preview execution preserves playback controls
The interactive preview and registered capture SHALL drive the same provider-owned per-model playback tracks used by general presentation. Explicit speed, loop, direct phase, reset, lifecycle, and model-local visual transform controls SHALL remain observable independently for every model track.

#### Scenario: Playback loop differs from authored default
- **WHEN** the user explicitly selects looping or non-looping playback for one model
- **THEN** the preview respects that choice without changing authored clip assets or another model's playback state

#### Scenario: Capture samples a phase
- **WHEN** the registered runner requests deterministic checkpoints
- **THEN** it samples the declared root and attachment model tracks at their requested phases, asks the provider presentation to refresh final attachment composition, submits render state, and only then acquires attested Game View pixels

#### Scenario: Preview stops or switches motion
- **WHEN** playback is stopped, reset, or switched on one model track
- **THEN** that provider-owned track changes without stale pose or attachment visual state influencing it and without resetting unrelated tracks

### Requirement: Runtime gallery provides bounded mutable streamed actor proof
The Unity preview SHALL provide one bounded preview-only proof that realizes current baked avatar/equipment assets through the pinned general streamed presentation capability over the existing resource and presenter pools while preserving independent model playback.

#### Scenario: Streamed proof initializes
- **WHEN** the dedicated mutable-streamed capture scenario starts
- **THEN** it acquires one composite-capable streamed presentation with a root and zero current mutable attachments through the existing pools, without a parallel renderer, cache, pool, or resolver

#### Scenario: Attachments mutate during one activation
- **WHEN** the proof adds, detaches, rebinds, or replaces equipment while tracks are active
- **THEN** high-level opaque attachment identities and stale-handle rules remain correct while root and unaffected attachment playback continue unchanged

#### Scenario: Actor is released and reacquired
- **WHEN** the proof releases the streamed presentation to its warm pool and reacquires it
- **THEN** prior runtime-added attachments and activation-local playback/visual state are absent, prior high-level handles fail closed, root presentation remains reusable, and new attachments can be added normally

#### Scenario: Existing gallery verification runs
- **WHEN** ordinary runtime-gallery or avatar-motion verification runs
- **THEN** the mutable proof surface does not alter model counts, render-instance assertions, composition behavior, or independent track semantics

### Requirement: Graphical capture proves mutable streamed composition
The preview capture registry SHALL expose a provider-attested `mutable-streamed-actor-gallery` scenario that captures deterministic high-level mutable-presentation checkpoints from the generated runtime gallery.

#### Scenario: Mutable capture runs
- **WHEN** `tools/capture-game-view.ps1 -Scenario mutable-streamed-actor-gallery` completes
- **THEN** attested Game View images cover initial multi-attachment composition, released root movement, continuous recovery, rebind, detach, replacement, root-only warm reacquire, and post-reacquire attachment

#### Scenario: A composed checkpoint is captured
- **WHEN** one or more equipment attachments are active
- **THEN** the runner samples independently declared provider model tracks, verifies current high-level attachment membership and finite visual transforms, refreshes provider presentation composition, submits provider render state, and only then acquires Game View pixels

### Requirement: Streamed presenter is the sole mutable topology authority
The lower streamed presenter SHALL remain the sole provider authority for logical mutable topology, while voxel-store consumes only the general streamed presentation's activation-scoped opaque identities, current membership/counts, model tracks, and high-level mutation operations. Preview proof SHALL NOT require lower topology revision or maintain parallel membership, resolver, or release-basis state.

#### Scenario: Attachment is added
- **WHEN** the preview successfully attaches equipment during one streamed presentation activation
- **THEN** the high-level attachment identity becomes current, attachment count reflects the new model, and no voxel-store topology or resolver shadow is created

#### Scenario: Attachment is detached
- **WHEN** one mutable attachment is detached
- **THEN** high-level membership no longer contains its identity, the old identity fails closed, and unaffected model tracks preserve their state

#### Scenario: Attachment is rebound
- **WHEN** an active attachment is rebound to another valid socket
- **THEN** the same high-level identity and model track remain current while subsequent provider-owned visual composition uses the rebound reference

#### Scenario: Attachment is replaced
- **WHEN** an active attachment is transactionally replaced
- **THEN** the same high-level identity identifies the replacement, the replacement receives fresh provider-defined playback/visual state, and root/unrelated attachment tracks remain unchanged

#### Scenario: Presenter is released and reacquired
- **WHEN** the proof releases the streamed presentation to its warm pool and reacquires it
- **THEN** prior activation identities fail closed, current attachment count returns to the configured baseline, provider-defined retained track/visual state is reset, and no preview membership or resolver shadow requires synchronization
