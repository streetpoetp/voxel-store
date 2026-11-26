# Spec Delta

## MODIFIED Requirements

### Requirement: Shared preview execution preserves playback controls
The interactive preview and registered capture SHALL drive the same provider-backed per-model animation path. Explicit speed, loop, direct phase, reset, lifecycle, and model-local visual transform controls SHALL remain observable independently for every model track.

#### Scenario: Playback loop differs from authored default
- **WHEN** the user explicitly selects looping or non-looping playback for one model
- **THEN** the preview respects that choice without changing authored clip assets or another model's playback state

#### Scenario: Capture samples a phase
- **WHEN** the registered runner requests deterministic checkpoints
- **THEN** it samples the declared body state, resolves each current attachment reference, explicitly applies that reference through the attachment model track when socket-relative presentation is desired, samples each declared attachment phase, and submits render state before acquiring attested Game View pixels

#### Scenario: Preview stops or switches motion
- **WHEN** playback is stopped, reset, or switched on one model track
- **THEN** that track changes without stale pose or visual transform state influencing it and without resetting unrelated tracks

### Requirement: Preview re-solves attachment after pose evaluation
The preview SHALL resolve current body-socket/bind-Mount attachment references without continuously overwriting animation-owned equipment world transforms. Looping or untranslated equipment takes SHALL follow the current reference. A non-looping take with meaningful authored translation SHALL latch its selection-start reference while displaced and MAY resume the live reference at terminal zero translation. It SHALL NOT use clip-name placement rules.

#### Scenario: Weapon swings around the grip
- **WHEN** an equipment Attack clip independently animates position or rotation on the part containing its Mount
- **THEN** the preview begins that selected take from the current bind-Mount reference and preserves the authored part animation so the visible weapon may move away from the socket reference

#### Scenario: Body motion moves the attachment
- **WHEN** the body clip moves its socket frame while an equipment take is looping or has no meaningful authored translation
- **THEN** preview applies the updated reference through that equipment model track without changing its selected clip, phase, speed, or loop state

#### Scenario: Body moves during a translated non-looping take
- **WHEN** a non-looping equipment take with meaningful authored translation has started from the current attachment reference and the body socket subsequently moves
- **THEN** the equipment model world matrix remains latched to its selection-start reference while the current attachment reference changes independently, so body motion does not drag the in-flight visual take

#### Scenario: Authored translated take returns to the socket
- **WHEN** a latched non-looping equipment take reaches phase 1 and every authored position channel terminates at zero
- **THEN** preview applies the then-current attachment reference so the equipment returns to the body's current socket without a detach/reattach topology operation

#### Scenario: Authored translated take remains displaced
- **WHEN** a latched non-looping equipment take reaches phase 1 with a non-zero terminal authored position
- **THEN** preview keeps its latched model world matrix and terminal authored pose rather than imposing a universal snap-back rule

#### Scenario: Animated Mount differs from current socket
- **WHEN** authored equipment animation moves the evaluated Mount away from the current body socket
- **THEN** preview treats the drift as authored visual motion and keeps the logical binding/reference valid instead of forcing the evaluated Mount back onto the socket

#### Scenario: Attachment contract is incompatible
- **WHEN** the current provider cannot resolve the declared binding reference from the body socket and equipment bind Mount
- **THEN** preview composition fails visibly rather than introducing local compensation, evaluated-Mount feedback, or pair-specific correction

### Requirement: Graphical capture proves mutable streamed composition
The preview capture registry SHALL expose a provider-attested `mutable-streamed-actor-gallery` scenario that captures deterministic mutable-composition checkpoints from the generated runtime gallery.

#### Scenario: Mutable capture runs
- **WHEN** `tools/capture-game-view.ps1 -Scenario mutable-streamed-actor-gallery` completes
- **THEN** attested Game View images cover initial multi-attachment composition, detach, replacement, root-only warm reacquire, and post-reacquire attachment

#### Scenario: A composed checkpoint is captured
- **WHEN** one or more equipment attachments are active
- **THEN** the runner samples independently declared model states, verifies current opaque attachment membership plus finite current attachment references, applies any declared socket-relative reference through the attachment track, submits provider render state, and only then acquires Game View pixels

### Requirement: Graphical capture proves generalized animation variant resolution
The canonical animation capture flow SHALL support deterministic checkpoints for generalized variants of `Idle`, `WalkForward`, `Attack`, `Hit`, and `Death`, including side-qualified variants where applicable.

#### Scenario: Looping variants are captured
- **WHEN** the capture scenario declares checkpoints for authored `Idle` and `WalkForward` variants
- **THEN** provider-attested Game View pixels show the selected variant clips at deterministic normalized phases without changing their looping semantic policy

#### Scenario: Non-looping variants are captured
- **WHEN** the capture scenario declares checkpoints for authored `Attack`, `Hit`, and `Death` variants
- **THEN** provider-attested Game View pixels show the requested takes at deterministic normalized phases and preserve non-looping terminal behavior where applicable

#### Scenario: Side-qualified variant is captured
- **WHEN** capture requests `Attack`, side `Left`, variant `V2` for a model that publishes `Attack.Left.V2`
- **THEN** the attested capture samples that fully qualified take while preserving logical attachment/reference identity even if the authored equipment pose does not keep its evaluated Mount coincident with the side-aware socket

### Requirement: Preview verification covers store-wide required V1 availability
Preview verification SHALL mechanically confirm that every baked published avatar and equipment model exposes the minimum V1 clip set required by publication, while graphical capture remains integration proof for independent composed playback and attachment-reference ownership.

#### Scenario: All baked models satisfy required coverage
- **WHEN** preview validation scans the baked published model set
- **THEN** every avatar and equipment animation asset reports the required exact V1 clip names for its model kind

#### Scenario: One baked model loses a required clip
- **WHEN** a baked model lacks a V1 clip required by publication
- **THEN** preview validation fails and identifies the model and missing clip before the change is accepted

#### Scenario: Graphical integration proof runs
- **WHEN** provider-attested Game View capture executes after store-wide coverage passes
- **THEN** it proves representative independent body/equipment playback plus current attachment-reference use without requiring animated socket/Mount coincidence

### Requirement: Preview exposes independent model animation tracks
Every composed body and equipment instance SHALL expose its own preview animation track with independent clip selection, phase/clock, speed, loop state, play/stop/reset lifecycle, and model-local visual world transform.

#### Scenario: Body and weapon play different motions
- **WHEN** the preview plays body `Idle` and one weapon `Attack`
- **THEN** both tracks advance independently while the weapon remains the same logically bound attachment and its authored visual pose may depart from the current socket reference

#### Scenario: Two weapons use different clocks
- **WHEN** two attached weapons use different speeds, phases, or loop settings
- **THEN** each weapon retains and advances only its own playback state

#### Scenario: Weapon animation changes
- **WHEN** a developer changes an equipment clip, variant, phase, speed, loop, or playing state
- **THEN** the body and all other equipment tracks remain unchanged

### Requirement: Graphical capture proves independent composed animation
The canonical composed-animation capture SHALL prove independent body/equipment playback together with current logical attachment/reference behavior and model-local visual motion through provider-attested Game View pixels.

#### Scenario: Mixed motions are captured
- **WHEN** capture declares body `Idle` and weapon `Attack` at independent checkpoints
- **THEN** attested pixels show the requested mixed motion state without a body reaction inferred from the weapon

#### Scenario: Independent phases are captured
- **WHEN** body and equipment are declared at different normalized phases or playback speeds
- **THEN** attested pixels preserve those independent states while the attachment retains a valid current reference and its authored visual pose is not forced to socket/Mount coincidence

#### Scenario: Consumer-issued Death commands are captured
- **WHEN** the capture consumer separately commands `Death` on the body and `Death` on an attachment in the same update
- **THEN** both ordinary tracks begin their own Death playback without any special shared Death/synchronization API

#### Scenario: Topology mutates during playback
- **WHEN** one attachment is added, removed, rebound, or replaced while body and another attachment continue playing
- **THEN** capture proves unaffected tracks preserve their playback state and current attachment/reference identities remain valid

## ADDED Requirements

### Requirement: Preview proof demonstrates animation-authored equipment spatial variants
The canonical preview proof SHALL include current equipment content whose baseline and variant takes produce observably different authored spatial trajectories without a separate placement-mode state machine.

#### Scenario: Axe idle remains a stable socket-relative hold
- **WHEN** `Axe03` `Idle` is sampled in composed preview
- **THEN** the equipment model track consumes the current attachment reference and the authored Idle pose remains a stable carried presentation

#### Scenario: Axe baseline attack uses authored spatial motion
- **WHEN** `Axe03` baseline `Attack` is sampled at its declared deterministic checkpoints
- **THEN** provider-backed evaluation and Game View proof show authored position plus rotation changing the visible axe trajectory relative to Idle while concurrent body motion does not alter the latched attack root

#### Scenario: Axe V1 attack is spatially distinct
- **WHEN** `Axe03` `Attack.V1` is sampled at the same normalized checkpoint used for baseline `Attack`
- **THEN** its authored visible position/rotation trajectory is observably distinct from baseline `Attack` while using the same logical attachment/reference contract and returning to the then-current reference when its terminal authored translation reaches zero
