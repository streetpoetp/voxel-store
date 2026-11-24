# Spec Delta

## ADDED Requirements

### Requirement: Runtime gallery uses shared provider realization
The runtime gallery SHALL realize standalone models and composed avatars through the pinned Hero Studio render and actor mechanisms, without a consumer-owned voxel pose evaluator, per-part rendering hierarchy, attachment solver or fallback renderer.

#### Scenario: Gallery realizes a multipart model
- **WHEN** an imported baked model is displayed
- **THEN** all its authored parts are realized by the provider render instance at authored scale and the gallery frames it from provider-evaluated geometry

#### Scenario: Gallery realizes composed avatars
- **WHEN** an avatar has zero or more carried equipment instances
- **THEN** each model has its own provider render instance and the shared actor coordinates their bindings and resolved clips

#### Scenario: Gallery is rebuilt or unloaded
- **WHEN** preview instances are replaced or the gallery exits
- **THEN** the preview releases its owned actors and render instances without stale submissions or duplicate realization

#### Scenario: Provider contract is unavailable
- **WHEN** the published pinned dependency lacks required actor/render capabilities
- **THEN** adoption is blocked rather than copying provider code or retaining a parallel runtime

### Requirement: Preview retains consumer semantic selection
The preview SHALL own motion vocabulary, physical driver selection and ordered exact clip candidates, while shared provider mechanisms resolve and execute the plan. Existing authored motion names and fallbacks SHALL remain compatible.

#### Scenario: Driver side changes
- **WHEN** the selected physical driver changes
- **THEN** the preview rebuilds the resolved plan for the selected side and all active attachments without stale bindings

#### Scenario: The same equipment asset is attached twice
- **WHEN** distinct physical slots reference one equipment asset
- **THEN** independent instances retain independent bindings and sample their selected clips at the common phase

#### Scenario: Required authored clip is missing
- **WHEN** no supplied exact candidate resolves
- **THEN** preview reports the missing required clip and does not substitute procedural motion

### Requirement: Shared preview execution preserves playback controls
The interactive preview and registered capture SHALL drive the same provider-backed animation path. Explicit speed and loop settings, direct phase, reset and lifecycle behavior SHALL remain observable.

#### Scenario: Playback loop differs from authored default
- **WHEN** the user explicitly selects looping or non-looping playback
- **THEN** the preview respects that choice without changing authored clip assets or implementing another actor coordinator

#### Scenario: Capture samples a phase
- **WHEN** the registered runner requests a deterministic checkpoint
- **THEN** it samples the resolved provider actor plan and submits current render state before acquiring attested Game View pixels

#### Scenario: Preview stops or switches motion
- **WHEN** playback is stopped, disabled, or switched
- **THEN** the previous plan is reset and no stale pose or binding influences the new playback

## MODIFIED Requirements

### Requirement: Preview proves canonical equipment frame behavior
The Unity preview SHALL preserve the shared canonical Unity model frame (+X Right, +Y Up, -Z Forward) through authored socket-to-Mount binding. It SHALL expose incorrectly authored model orientation rather than applying pair-specific corrective transforms.

#### Scenario: Canonical equipment frame composes correctly
- **WHEN** a published equipment asset is placed against the required authored socket evidence set
- **THEN** its derived Unity presentation follows the provider's socket-to-bind-anchor transform and evidence identifies the evaluated placement and forward direction

#### Scenario: Composition needs a pair-specific correction
- **WHEN** the preview would require an avatar/equipment-specific correction to look acceptable
- **THEN** proof reports the composition as failing rather than persisting the correction as preview or store authority

### Requirement: Runtime avatar composition uses explicit presentation ownership
The Play Mode preview SHALL represent each composed avatar through one preview presentation owner that exposes the presentation root, baked Body bounds, canonical local orientation and attachment identities, while delegating pose, attachment and synchronized sampling to the shared provider.

#### Scenario: Avatar presentation is created
- **WHEN** the runtime gallery composes a published rigid-Body avatar
- **THEN** exactly one preview presentation owner exposes the Body composition surface and canonical local Right, Up and Forward directions backed by provider realization

#### Scenario: Canonical orientation is queried
- **WHEN** preview motion needs a directional basis for a current canonical avatar
- **THEN** the presentation resolves visual Forward from canonical baked orientation rather than requiring each motion to hard-code an unexplained XYZ direction

### Requirement: WalkForward follows a Crossy Road-style hop grammar
WalkForward SHALL present locomotion as an authored discrete launch, airborne apex, landing and recovery cycle rather than a continuous walk cycle or an in-place arbitrary bounce.

#### Scenario: WalkForward presentation is played
- **WHEN** the preview plays WalkForward
- **THEN** the authored body clip presents a readable hop arc while attached equipment receives coordinated authored motion through the shared actor

#### Scenario: Gallery demonstrates forward travel
- **WHEN** the preview scene chooses to visualize a logical forward step during WalkForward
- **THEN** preview-only locomotion may move the unit root along canonical Forward while the motion player remains presentation-only and does not become production movement authority

### Requirement: Attack and hit respect canonical forward direction
Combat feedback motions SHALL be expressed by authored body and equipment clips relative to semantic Forward rather than raw world/local axis guesses or procedural correction.

#### Scenario: Attack is played
- **WHEN** the preview plays Attack
- **THEN** the authored avatar clip performs backward anticipation followed by a fast drive toward semantic Forward and carried equipment shows a coordinated forward attack trajectory

#### Scenario: Hit is played
- **WHEN** the preview plays Hit
- **THEN** the authored avatar clip recoils opposite semantic Forward and attached equipment exhibits coordinated inertia before non-terminal presentation recovers

### Requirement: Death coordinates avatar and equipment terminal presentation
Death SHALL coordinate authored rigid Body and attached equipment clips as one terminal presentation sequence without modifying authored attachment authority.

#### Scenario: Death is played
- **WHEN** the preview plays Death
- **THEN** the authored Body reaches its terminal pose and equipment performs coordinated follow-through using the shared binding and phase

### Requirement: Preview re-solves attachment after pose evaluation
For each semantic phase the shared actor SHALL sample the body, resolve its current socket against the child's authored bind-pose Mount anchor, and sample the child's authored clip at the same phase. Preview SHALL NOT add animated-anchor compensation or a pair-specific transform correction.

#### Scenario: Weapon swings around the grip
- **WHEN** an equipment Attack clip animates the part containing its Mount
- **THEN** the provider's bind-anchor placement and authored clip produce the intended grip-centered visible motion without unacceptable separation; proof fails if the animated grip drifts, and the owning provider or authored input contract must be resolved before acceptance

#### Scenario: Body motion moves the attachment
- **WHEN** the body clip moves its socket frame
- **THEN** attached equipment follows the evaluated socket using the shared bind-anchor rule without accumulating drift

#### Scenario: Attachment contract is incompatible
- **WHEN** the shared provider cannot meet the existing visible grip acceptance with current authored inputs
- **THEN** adoption remains blocked rather than changing asset source, introducing local compensation, or weakening visual proof
