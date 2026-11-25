# Spec Delta

## REMOVED Requirements

### Requirement: Death coordinates avatar and equipment terminal presentation
**Reason**: Death is not a special actor-level synchronization event. Body and equipment remain independent animation tracks.

**Migration**: A consumer that wants a coherent unit death explicitly issues ordinary Death/stop/freeze commands to each relevant model instance.

### Requirement: Preview motion context preserves selected slot side
**Reason**: Attachment side must not implicitly drive body animation selection.

**Migration**: Side-qualified clips remain selectable explicitly on the model that owns them; weapon playback never selects a body-side reaction.

### Requirement: One semantic phase drives body and equipment authored clips
**Reason**: Body and attachment tracks now own independent clocks and normalized phases.

**Migration**: Capture and interactive preview declare each model's own clip/phase/speed/loop state.

### Requirement: Graphical capture proves authored synchronized animation
**Reason**: Synchronized actor-wide animation is replaced by independent per-model playback.

**Migration**: Use the independent composed-animation capture requirement below.

## ADDED Requirements

### Requirement: Preview exposes independent model animation tracks
Every composed body and equipment instance SHALL expose its own preview animation track with independent clip selection, phase/clock, speed, loop state, and play/stop/reset lifecycle.

#### Scenario: Body and weapon play different motions
- **WHEN** the preview plays body `Idle` and one weapon `Attack`
- **THEN** both tracks advance independently while the weapon remains physically attached to the current body socket

#### Scenario: Two weapons use different clocks
- **WHEN** two attached weapons use different speeds, phases, or loop settings
- **THEN** each weapon retains and advances only its own playback state

#### Scenario: Weapon animation changes
- **WHEN** a developer changes an equipment clip, variant, phase, speed, loop, or playing state
- **THEN** the body and all other equipment tracks remain unchanged

### Requirement: Graphical capture proves independent composed animation
The canonical composed-animation capture SHALL prove independent body/equipment playback together with current socket/Mount attachment through provider-attested Game View pixels.

#### Scenario: Mixed motions are captured
- **WHEN** capture declares body `Idle` and weapon `Attack` at independent checkpoints
- **THEN** attested pixels show the requested mixed motion state without a body reaction inferred from the weapon

#### Scenario: Independent phases are captured
- **WHEN** body and equipment are declared at different normalized phases or playback speeds
- **THEN** attested pixels preserve those independent states while the equipment remains locked to the evaluated body socket

#### Scenario: Consumer-issued Death commands are captured
- **WHEN** the capture consumer separately commands `Death` on the body and `Death` on an attachment in the same update
- **THEN** both ordinary tracks begin their own Death playback without any special shared Death/synchronization API

#### Scenario: Topology mutates during playback
- **WHEN** one attachment is added, removed, rebound, or replaced while body and another attachment continue playing
- **THEN** capture proves unaffected tracks preserve their playback state and attachment placement remains valid

## MODIFIED Requirements

### Requirement: Preview captures dense deterministic presentation trajectories
The preview proof workflow SHALL support deterministic temporal sampling of independently declared model trajectories at enough ordered times and views to reveal discontinuities, reversals, hidden equipment, weak body motion, and attachment separation.

#### Scenario: Presentation motion is evaluated
- **WHEN** a presentation proof is captured for a body or equipment motion
- **THEN** the preview emits ordered samples covering the declared model-local trajectory states from deterministic views

#### Scenario: Key poses hide a trajectory defect
- **WHEN** sparse key-pose samples appear acceptable but intermediate temporal samples reveal a discontinuity or reversal
- **THEN** the dense capture evidence remains available to classify the presentation as visually failing

### Requirement: Runtime gallery uses shared provider realization
The runtime gallery SHALL realize standalone models and composed avatars through pinned Hero Studio render/actor mechanisms, without a consumer-owned voxel evaluator, attachment solver, or fallback renderer. The actor SHALL own composition while model animation tracks remain independent.

#### Scenario: Gallery realizes a multipart model
- **WHEN** an imported baked model is displayed
- **THEN** all its authored parts are realized by the provider render instance at authored scale and the gallery frames it from provider-evaluated geometry

#### Scenario: Gallery realizes composed avatars
- **WHEN** an avatar has zero or more carried equipment instances
- **THEN** each model has its own provider render instance and model-local playback state while the shared actor coordinates only physical bindings/composition

#### Scenario: Gallery is rebuilt or unloaded
- **WHEN** preview instances are replaced or the gallery exits
- **THEN** the preview releases its owned actors, model tracks, and render instances without stale submissions or duplicate realization

#### Scenario: Provider contract is unavailable
- **WHEN** the published pinned dependency lacks required independent-track actor/render capabilities
- **THEN** adoption is blocked rather than copying provider code or retaining a parallel runtime

### Requirement: Preview retains consumer semantic selection
The preview SHALL own model-local motion vocabulary and ordered exact clip candidates while shared provider mechanisms resolve and execute each selected model track independently. Existing authored motion names and fallbacks SHALL remain compatible.

#### Scenario: Driver side changes
- **WHEN** the selected physical driver side changes
- **THEN** no model track changes implicitly; an explicit side-qualified body selection such as `Attack.Left` affects only the body when the developer separately chooses it

#### Scenario: The same equipment asset is attached twice
- **WHEN** distinct physical slots reference one equipment asset
- **THEN** independent instances retain independent bindings and independent selected clips/playback states

#### Scenario: Required authored clip is missing
- **WHEN** no supplied exact candidate resolves for one selected model track
- **THEN** preview reports the missing required clip for that model and does not substitute procedural motion or alter another track

### Requirement: Shared preview execution preserves playback controls
The interactive preview and registered capture SHALL drive the same provider-backed per-model animation path. Explicit speed, loop, direct phase, reset, and lifecycle controls SHALL remain observable independently for every model track.

#### Scenario: Playback loop differs from authored default
- **WHEN** the user explicitly selects looping or non-looping playback for one model
- **THEN** the preview respects that choice without changing authored clip assets or another model's playback state

#### Scenario: Capture samples a phase
- **WHEN** the registered runner requests deterministic checkpoints
- **THEN** it samples each declared model track at its own requested phase, refreshes current attachment placement, and submits render state before acquiring attested Game View pixels

#### Scenario: Preview stops or switches motion
- **WHEN** playback is stopped, reset, or switched on one model track
- **THEN** that track changes without stale pose influencing it and without resetting unrelated tracks

### Requirement: Runtime avatar composition uses explicit presentation ownership
The Play Mode preview SHALL represent each composed avatar through one composition owner exposing root/bounds/orientation/attachment identities while delegating physical binding and independent model-local animation playback to the pinned provider.

#### Scenario: Avatar presentation is created
- **WHEN** the runtime gallery composes a published rigid-Body avatar
- **THEN** exactly one composition owner exposes the Body composition surface and canonical local Right, Up and Forward directions backed by provider realization

#### Scenario: Canonical orientation is queried
- **WHEN** preview motion needs a directional basis for a current canonical model
- **THEN** the presentation resolves visual Forward from canonical baked orientation rather than requiring each motion to hard-code an unexplained XYZ direction

### Requirement: WalkForward follows a Crossy Road-style hop grammar
WalkForward SHALL present body locomotion as an authored discrete launch, airborne apex, landing and recovery cycle rather than a continuous walk cycle or an in-place arbitrary bounce.

#### Scenario: WalkForward presentation is played
- **WHEN** the preview plays WalkForward on the body
- **THEN** the authored body clip presents a readable hop arc and attached equipment animation tracks remain whatever the consumer independently selected

#### Scenario: Gallery demonstrates forward travel
- **WHEN** the preview scene chooses to visualize a logical forward step during WalkForward
- **THEN** preview-only locomotion may move the unit root along canonical Forward while model animation players remain presentation-only and do not become production movement authority

### Requirement: Attack and hit respect canonical forward direction
Authored body and equipment Attack/Hit motions SHALL use semantic Forward rather than raw axis guesses, but playing one model's Attack/Hit SHALL NOT implicitly command another model.

#### Scenario: Attack is played
- **WHEN** the preview independently plays Attack on the body
- **THEN** the authored body clip performs its semantic-forward trajectory while equipment tracks keep their independently selected state

#### Scenario: Equipment Attack is played
- **WHEN** the preview independently plays Attack on a weapon
- **THEN** that weapon performs its semantic-forward authored motion while body playback remains unchanged

#### Scenario: Hit is played
- **WHEN** the preview independently plays Hit on the body
- **THEN** the authored body clip recoils opposite semantic Forward without implicitly changing equipment tracks

### Requirement: Animation verification uses registered provider-attested Game View capture
Canonical animation proof SHALL run through `tools/capture-game-view.ps1`, backed by a preview-owned schema-v2 registry and graphical `Capture.Editor` runner. Accepted images SHALL use `Hero.Consumer.Editor.GameViewCapture` and current provider attestation under `Library/Capture/**`; direct camera/RenderTexture output is diagnostic only.

#### Scenario: Capture scenario is discovered
- **WHEN** avatar-motion visual verification is requested
- **THEN** the launcher resolves the declared preview capture scenario from schema-v2 registry metadata rather than requiring an agent to know or type an arbitrary runner/output pair

#### Scenario: Caller uses only the repository command
- **WHEN** automated verification needs avatar-motion captures
- **THEN** the caller SHALL NOT need to invoke Unity `-executeMethod`, know the fully-qualified runner class, hard-code the output directory, or operate Editor menus/buttons; those details SHALL remain behind `tools/capture-game-view.ps1` and the capture registry

#### Scenario: Capture is invoked by command
- **WHEN** an agent or developer runs `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`
- **THEN** that command resolves the non-consumer preview workspace/registry, reuses pinned capture-attestation helpers, launches an owned graphical Editor, rejects stale/unattested output, and reports current capture artifacts plus `CAPTURE_ATTESTED`

#### Scenario: Play Mode reload occurs
- **WHEN** the registered `Capture.Editor` runner enters Play Mode and Unity reloads the domain
- **THEN** capture state survives and resumes against the live runtime gallery without manual Play/menu interaction

#### Scenario: Representative combat motion is captured
- **WHEN** body or equipment `WalkForward`, `Attack`, `Hit`, or `Death` requires visual proof
- **THEN** the runner drives the same independent model-track path used by interactive playback to declared deterministic checkpoints before invoking Game View capture

#### Scenario: Semantic translation must be visible in pixels
- **WHEN** a captured model motion translates along semantic Forward or opposite Forward
- **THEN** the capture runner uses an evidence-only oblique Game View camera pose around the same runtime gallery bounds so that motion has a visible screen-space component without changing the ordinary gallery camera contract

#### Scenario: Capture completes
- **WHEN** all declared checkpoints have been acquired
- **THEN** the runner resets declared preview track state, exits Play Mode cleanly, clears persisted capture state, exits the owned Editor with success, and reports success only after validating current provider attestation and readable current PNGs

#### Scenario: Capture pixels are reviewed
- **WHEN** the shared capture contract reports `CAPTURE_ATTESTED`
- **THEN** that result proves only current graphical Game View acquisition and file integrity; agent or human inspection remains responsible for judging whether the motion is visually correct

#### Scenario: Interactive controls are unavailable
- **WHEN** the custom Editor window is closed and no hierarchy object is manually selected
- **THEN** registered Game View capture remains fully executable and does not lose coverage

### Requirement: Editor control optionally selects preview motion
The Unity preview SHALL provide an Editor control that selects a concrete body or equipment model track plus that track's clip, loop state, speed, and phase. The control SHALL NOT be required for automated verification.

#### Scenario: Developer selects a motion before Play Mode
- **WHEN** a developer configures a body or equipment track while the Editor is not playing
- **THEN** that model-local preview state is applied to the generated gallery configuration and used when Play Mode begins

#### Scenario: Developer switches motion during Play Mode
- **WHEN** a developer selects another clip on one live model track
- **THEN** only that track switches through its normal reset/play lifecycle

#### Scenario: Developer edits Motion in the component Inspector during Play Mode
- **WHEN** Clip, Loop, Speed, or Phase changes on one active model player
- **THEN** only that model player updates without requiring disable/enable or scene restart

#### Scenario: Developer targets one avatar
- **WHEN** one composed avatar and a concrete body/equipment model instance within it are selected
- **THEN** preview controls address that selected model instance rather than broadcasting one actor-wide motion

#### Scenario: Verification runs without the Editor window
- **WHEN** automated verification runs with no custom window interaction
- **THEN** registered provider-attested Game View capture remains the authoritative proof path and produces current composed runtime pixels

### Requirement: Preview loads authored animation for every composed model
The runtime gallery SHALL load the Hero.Voxel baked animation asset associated with each discovered body and equipment assembly so every model can own independent playback.

#### Scenario: Avatar and weapon are composed
- **WHEN** both models have required baked semantic clips
- **THEN** the preview binds each baked animation asset to its own model-local playback track

#### Scenario: Required animation asset is unavailable
- **WHEN** a model assembly exists but its current semantic animation asset cannot be resolved
- **THEN** independent playback proof fails for that model instead of synthesizing canonical motion procedurally

### Requirement: Preview re-solves attachment after pose evaluation
The preview SHALL evaluate body animation, resolve each current body socket against the child's bind-pose Mount anchor, and then evaluate each attachment's independent animation state. It SHALL NOT add animated-anchor compensation or pair-specific transform correction.

#### Scenario: Weapon swings around the grip
- **WHEN** an equipment Attack clip independently animates the part containing its Mount
- **THEN** bind-anchor placement and the weapon's own clip produce grip-centered visible motion without unacceptable separation

#### Scenario: Body motion moves the attachment
- **WHEN** the body clip moves its socket frame while the weapon is at any independent clip/phase
- **THEN** attached equipment follows the evaluated body socket using the bind-anchor rule without changing the weapon's playback state

#### Scenario: Attachment contract is incompatible
- **WHEN** the shared provider cannot meet visible grip acceptance with current authored inputs
- **THEN** adoption remains blocked rather than changing asset source, introducing local compensation, or weakening visual proof

### Requirement: Runtime gallery provides bounded mutable streamed actor proof
The Unity preview SHALL provide one bounded preview-only proof that realizes current baked avatar/equipment assets through the pinned mutable streamed actor API while preserving independent model playback.

#### Scenario: Streamed proof initializes
- **WHEN** the dedicated mutable-streamed capture scenario starts
- **THEN** it acquires one root-only actor through the current streaming/presenter pools using current baked store assets and no parallel renderer/cache

#### Scenario: Attachments mutate during one activation
- **WHEN** the proof adds, detaches, rebinds, or replaces equipment while tracks are playing
- **THEN** opaque attachment handles and stale-handle rules remain correct while body and unaffected attachment playback continue unchanged

#### Scenario: Actor is released and reacquired
- **WHEN** the proof releases the actor to its warm pool and reacquires it
- **THEN** runtime-added attachments and their playback state are absent, prior handles fail closed, root presentation remains reusable, and new attachments can be added normally

#### Scenario: Existing gallery verification runs
- **WHEN** ordinary runtime-gallery or avatar-motion verification runs
- **THEN** the mutable proof surface does not alter model counts, render-instance assertions, composition behavior, or independent track semantics

### Requirement: Graphical capture proves mutable streamed composition
The preview capture registry SHALL expose a provider-attested `mutable-streamed-actor-gallery` scenario that captures deterministic mutable-composition checkpoints from the generated runtime gallery.

#### Scenario: Mutable capture runs
- **WHEN** `tools/capture-game-view.ps1 -Scenario mutable-streamed-actor-gallery` completes
- **THEN** attested Game View images cover initial multi-attachment composition, detach, replacement, root-only warm reacquire, and post-reacquire attachment

#### Scenario: A composed checkpoint is captured
- **WHEN** one or more equipment attachments are active
- **THEN** the runner samples independently declared model states, validates each child `Mount` against its current root socket within grip tolerance, submits provider render state, and only then acquires Game View pixels

### Requirement: Preview motion state supports model-local animation variants
The runtime preview SHALL allow optional model-local variant selection independently for the body and each attached equipment instance without coupling their semantic motion or playback phase.

#### Scenario: Developer previews an Idle variant
- **WHEN** preview state selects semantic `Idle` and body variant `V2`
- **THEN** the body track plays the resolved Idle V2 take without changing attachment tracks

#### Scenario: Developer previews a Death variant
- **WHEN** preview state selects semantic `Death` and body variant `V3`
- **THEN** the body track plays its non-looping Death V3 take and reaches its own terminal pose

#### Scenario: Body and equipment use different variants
- **WHEN** preview state selects body `Idle.V2` and one equipment instance `Attack.V1`
- **THEN** the two model tracks resolve and play their selections independently

#### Scenario: Variant selection is cleared
- **WHEN** a developer clears the variant selection for one model instance
- **THEN** only that model returns to its side/base candidate behavior without choosing an optional variant automatically

### Requirement: Preview candidate construction preserves side and variant precedence
Preview playback SHALL construct ordered clip candidates per selected model instance so an explicitly requested side+variant take is preferred before generic variant, side-only, and base semantic clips.

#### Scenario: Left attack V2 is previewed
- **WHEN** preview requests `Attack`, explicit side `Left`, and variant `V2` for one selected model
- **THEN** that model's candidate order is `Attack.Left.V2`, `Attack.V2`, `Attack.Left`, `Attack`

#### Scenario: Walk variant has no side context
- **WHEN** preview requests `WalkForward` variant `V2` for one model without a physical side qualifier
- **THEN** that model resolves `WalkForward.V2` before `WalkForward`

### Requirement: Component Inspector discovers motion clips from the selected model
The Unity component Inspector SHALL derive selectable motion clips from the baked animation asset bound to the selected body or equipment model instance. It SHALL NOT maintain a separate hard-coded motion/variant list.

#### Scenario: Model publishes multiple Death takes
- **WHEN** the selected model's baked animation asset contains `Death`, `Death.V1`, and `Death.V2`
- **THEN** the Inspector offers those exact clip names for that model track

#### Scenario: Model publishes only one Death take
- **WHEN** the selected model's baked animation asset contains `Death` but no Death variants
- **THEN** the Inspector offers only the authored Death clip and does not invent another variant

#### Scenario: Two models publish different clip sets
- **WHEN** body and equipment have different baked clip inventories
- **THEN** selecting each model instance yields that model's own exact choices rather than a shared actor-wide option list

### Requirement: Runtime gallery exposes the current baked animation inventory
The generated runtime gallery SHALL bind every composed body/equipment model instance to its own current baked animation asset so Inspector discovery and interactive playback observe that model's authored clip inventory.

#### Scenario: Published source is rebaked
- **WHEN** a model gains a new required V1 clip and the runtime gallery is refreshed
- **THEN** that model's baked animation asset and Inspector choices include the new clip without an Inspector code change

#### Scenario: Baked animation asset is incomplete or stale
- **WHEN** the gallery cannot bind the current animation asset required by a discovered model
- **THEN** preview validation fails visibly instead of falling back to a manually maintained clip list

### Requirement: Preview verification covers store-wide required V1 availability
Preview verification SHALL mechanically confirm that every baked published avatar and equipment model exposes the minimum V1 clip set required by publication, while graphical capture remains integration proof for independent composed playback.

#### Scenario: All baked models satisfy required coverage
- **WHEN** preview validation scans the baked published model set
- **THEN** every avatar and equipment animation asset reports the required exact V1 clip names for its model kind

#### Scenario: One baked model loses a required clip
- **WHEN** a baked model lacks a V1 clip required by publication
- **THEN** preview validation fails and identifies the model and missing clip before the change is accepted

#### Scenario: Graphical integration proof runs
- **WHEN** provider-attested Game View capture executes after store-wide coverage passes
- **THEN** it proves representative independent body/equipment playback and socket/Mount lock without replacing the per-model coverage gate
