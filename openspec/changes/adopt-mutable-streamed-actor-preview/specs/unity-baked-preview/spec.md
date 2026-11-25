## ADDED Requirements

### Requirement: Runtime gallery provides bounded mutable streamed actor proof
The Unity preview SHALL provide one bounded, preview-only consumer proof that realizes current baked avatar/equipment assets through the pinned mutable streamed actor API without replacing the existing direct runtime-gallery composition path.

#### Scenario: Streamed proof initializes
- **WHEN** the dedicated mutable-streamed capture scenario starts
- **THEN** it acquires one root-only actor through `VoxelStreamingService`, `VoxelPresenterPool`, and `VoxelActorPresenterPool` using current baked store assets and no parallel renderer/cache

#### Scenario: Attachments mutate during one activation
- **WHEN** the proof adds, detaches, and replaces equipment
- **THEN** operations use opaque attachment handles, replacement preserves the selected handle, stale handles fail closed, and each topology change is followed by a fresh handle-addressed animation plan

#### Scenario: Actor is released and reacquired
- **WHEN** the proof releases the actor to its warm pool and reacquires it
- **THEN** runtime-added attachments are absent, prior handles fail closed, root presentation remains reusable, and new attachments can be added normally

#### Scenario: Existing gallery verification runs
- **WHEN** ordinary runtime-gallery or `avatar-motion-gallery` verification runs
- **THEN** the mutable proof surface does not alter its model counts, render-instance assertions, or direct actor-composition behavior

### Requirement: Graphical capture proves mutable streamed composition
The preview capture registry SHALL expose a provider-attested `mutable-streamed-actor-gallery` scenario that captures deterministic mutable-composition checkpoints from the generated runtime gallery.

#### Scenario: Mutable capture runs
- **WHEN** `tools/capture-game-view.ps1 -Scenario mutable-streamed-actor-gallery` completes
- **THEN** attested Game View images cover initial multi-attachment composition, detach, replacement, root-only warm reacquire, and post-reacquire attachment

#### Scenario: A composed checkpoint is captured
- **WHEN** one or more equipment attachments are active
- **THEN** the runner samples one shared normalized actor phase, validates each child `Mount` against its current root socket within the existing grip tolerance, submits provider render state, and only then acquires Game View pixels
