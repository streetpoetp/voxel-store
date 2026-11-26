# unity-baked-preview Specification

## Purpose

Defines a non-authoritative Unity Editor workspace for inspecting the real Hero.Voxel baked outputs of voxel-store assets and comparing every currently published unit together in one scene grid.

## Requirements

### Requirement: Preview uses current published store state
The preview workflow SHALL derive its unit set from the store's current authoritative publication evaluation and SHALL include only assets that currently satisfy publication.

#### Scenario: Published avatars and equipment exist
- **WHEN** preview refresh evaluates the store and published avatar and equipment assets are present
- **THEN** every published asset of both kinds is selected for preview exactly once

#### Scenario: An asset is not currently published
- **WHEN** an asset fails any current publication requirement
- **THEN** preview refresh excludes that asset rather than treating existing baked output as proof of publication

### Requirement: Preview delegates voxel semantics to pinned Hero.Voxel
The preview workspace SHALL consume the complete `hero.studio` Unity package from the repository-pinned Hero Studio Git submodule as one whole embedded package and SHALL use that provider for assembly baking, animation baking, and baked-asset interpretation without creating a selective Hero.Voxel package projection or parallel voxel implementation.

#### Scenario: Pinned provider is available
- **WHEN** preview refresh resolves the exact Hero Studio submodule revision declared by the store
- **THEN** Unity loads the complete `hero.studio` package from that submodule and bakes the unit's current publication-validated generated geometry and animation artifacts through its canonical Hero.Voxel APIs

#### Scenario: Pinned submodule provider is available
- **WHEN** the repository submodule is initialized at the pinned revision
- **THEN** bootstrap exposes the complete `hero.studio` package as one embedded package junction and does not select, copy, or replace individual Hero Studio modules

#### Scenario: Provider is missing or revision-incompatible
- **WHEN** the Hero Studio submodule is missing, uninitialized, or does not match the pinned provider revision
- **THEN** preview refresh fails before baking or presenting new results

#### Scenario: Full provider requires its declared render pipeline
- **WHEN** the complete Hero Studio Unity package declares and uses its supported render pipeline
- **THEN** the preview project configures that render pipeline rather than patching or replacing provider-owned preview shaders

### Requirement: Store source remains authoritative
The preview workspace SHALL derive selected assets and their generated bake inputs from current authoritative store publication state and SHALL NOT require a copied Unity-side authoring source, alternate identity record, or preview-time source regeneration.

#### Scenario: Published source is outside the Unity project
- **WHEN** a published asset has current generated Hero.Voxel geometry/animation artifacts beside its authoritative source under voxel-store `Assets/**/Source/**`
- **THEN** preview baking reads those publication-validated generated artifacts in place and writes only derived Unity outputs inside the preview project

#### Scenario: Preview outputs are deleted
- **WHEN** baked Unity preview outputs are removed while store sources remain intact
- **THEN** the preview can reconstruct them without changing store publication or source authority

### Requirement: Canonical scene shows all published units in a grid
The canonical preview scene SHALL display every unit selected by the current preview refresh in one deterministic, non-overlapping grid while preserving each baked unit's authored world scale.

#### Scenario: Preview contains multiple units
- **WHEN** preview refresh completes with two or more published units
- **THEN** the scene contains one visible grid entry per unit in deterministic identity order with sufficient shared spacing to prevent entries from overlapping

#### Scenario: Avatars and equipment are both published
- **WHEN** the current published set includes both avatars and equipment
- **THEN** both kinds appear together in the canonical scene grid rather than requiring separate scenes

#### Scenario: Units have different authored sizes
- **WHEN** baked units have different world-space bounds
- **THEN** grid layout preserves their baked scale instead of independently normalizing each unit to the same visual size

### Requirement: Grid refresh removes stale preview entries
The canonical grid SHALL be reconstructed from the current preview unit set so saved or previously generated preview state cannot leave retired, unpublished, or removed units visible as current content.

#### Scenario: Published set changes
- **WHEN** a previously previewed unit is no longer published or a newly published unit is added
- **THEN** the next preview refresh removes the stale entry and adds the new entry so the grid exactly matches the current published set

#### Scenario: No units are published
- **WHEN** preview refresh finds no currently published units
- **THEN** the canonical scene contains no generated unit entries and reports the empty preview state

### Requirement: Baked preview assets are derived workspace state
Unity assembly assets, animation assets, captures, and generated grid objects SHALL be treated as disposable preview state and SHALL NOT become voxel-store authoring, publication, or compatibility authority.

#### Scenario: Derived bake differs after source change
- **WHEN** authoritative source changes and preview refresh produces different baked assets
- **THEN** the new derived preview replaces the old preview without requiring an asset identity migration

#### Scenario: Repository is checked out on another machine
- **WHEN** another machine resolves the pinned provider and rebuilds the preview workspace
- **THEN** machine-local baked outputs can be recreated without relying on committed Unity bake artifacts or developer-local paths

### Requirement: Preview remains Editor-only
The preview workspace SHALL exist for Unity Editor inspection and SHALL NOT introduce a production game runtime, build target, or gameplay-facing voxel ownership boundary into voxel-store.

#### Scenario: Developer inspects a baked unit
- **WHEN** a developer selects or previews a baked assembly or animation in the workspace
- **THEN** existing Hero.Voxel Editor preview and Scene Debug behavior can be used without adding gameplay semantics to the store

#### Scenario: Grid is generated
- **WHEN** the all-units grid is built in the canonical preview scene
- **THEN** its generated scene objects remain preview-only and are not treated as production runtime content

### Requirement: Preview proves canonical equipment frame behavior
The Unity preview SHALL preserve the shared canonical Unity model frame (+X Right, +Y Up, -Z Forward) through authored socket-to-Mount binding. It SHALL expose incorrectly authored model orientation rather than applying pair-specific corrective transforms.

#### Scenario: Canonical equipment frame composes correctly
- **WHEN** a published equipment asset is placed against the required authored socket evidence set
- **THEN** its derived Unity presentation follows the provider's socket-to-bind-anchor transform and evidence identifies the evaluated placement and forward direction

#### Scenario: Composition needs a pair-specific correction
- **WHEN** the preview would require an avatar/equipment-specific correction to look acceptable
- **THEN** proof reports the composition as failing rather than persisting the correction as preview or store authority

### Requirement: Preview captures dense deterministic presentation trajectories
The preview proof workflow SHALL support deterministic temporal sampling of independently declared model trajectories at enough ordered times and views to reveal discontinuities, reversals, hidden equipment, weak body motion, and attachment separation.

#### Scenario: Presentation motion is evaluated
- **WHEN** a presentation proof is captured for a body or equipment motion
- **THEN** the preview emits ordered samples covering the declared model-local trajectory states from deterministic views

#### Scenario: Key poses hide a trajectory defect
- **WHEN** sparse key-pose samples appear acceptable but intermediate temporal samples reveal a discontinuity or reversal
- **THEN** the dense capture evidence remains available to classify the presentation as visually failing

### Requirement: Runtime gallery discovers human-imported baked catalogs
The runtime gallery SHALL discover human-imported `VoxelAssemblyAsset` models under the preview baked root from folder identity `.../<Catalog>/<Model>/Source/<Model>.asset` and SHALL NOT require provider plan/Kind registration to display them. Imported catalog trees are human-owned inputs and SHALL be preserved by stale generated-output cleanup.

#### Scenario: Imported model is discovered
- **WHEN** a baked assembly exists at a path matching `.../<Catalog>/<Model>/Source/<Model>.asset`
- **THEN** the gallery derives catalog and model identity from those folders and includes the model without a code/catalog registration change

#### Scenario: Known catalog is composed
- **WHEN** the derived catalog is `Avatars` or `Equipment`
- **THEN** avatars receive avatar presentation/motion behavior, equipment may be sampled as carried presentation and is also shown standalone

#### Scenario: Unknown catalog is imported
- **WHEN** the derived catalog has another name
- **THEN** the model remains valid and is shown as a standalone general model rather than being rejected

#### Scenario: Multipart assembly is displayed
- **WHEN** an imported assembly contains multiple baked parts and bind hierarchy/transforms
- **THEN** the gallery realizes the complete part hierarchy for visualization without requiring a one-part or socketless special case

#### Scenario: Stale generated output is cleaned
- **WHEN** legacy preview-bake cleanup runs
- **THEN** it deletes only positively identified generated unit directories and does not delete human-imported catalog folders

#### Scenario: Carried equipment is selected
- **WHEN** the gallery needs carried equipment to exercise avatar composition
- **THEN** it chooses deterministic samples from the sorted Equipment catalog and treats that pairing as preview-only comparison data with no gameplay/loadout semantics

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

### Requirement: Play Mode presentation remains preview-only
Runtime MonoBehaviours used by the generated gallery MAY execute during Unity Editor Play Mode, but they SHALL remain preview/evaluation infrastructure and SHALL NOT become a production-game runtime dependency or store authority.

#### Scenario: Generated gallery is played
- **WHEN** the developer enters Play Mode in the generated runtime gallery scene
- **THEN** avatar composition and motion run as ordinary MonoBehaviours in the Editor Play Mode environment without changing store publication, compatibility, simulation, or source authority

#### Scenario: Preview runtime files are consumed by a production build
- **WHEN** a production consumer or build graph is evaluated
- **THEN** voxel-store preview presentation/motion components are not required as production gameplay architecture

### Requirement: Preview composes equipment from baked socket frames
The Unity preview SHALL resolve carried equipment from the selected avatar equipment socket and the equipment's baked Mount socket.

#### Scenario: Equipment is attached in the runtime gallery
- **WHEN** the gallery composes an avatar and equipment pair
- **THEN** it resolves a compatible `Equip.<Capability>.<Side>.<Instance>` socket and aligns the equipment's `Mount` frame without a Body-ratio placement offset

#### Scenario: Attachment cannot be solved
- **WHEN** required socket/Mount data is missing, singular, or requires pair-specific correction
- **THEN** preview composition fails visibly instead of falling back to the prior Body-ratio path

### Requirement: Preview loads authored animation for every composed model
The runtime gallery SHALL load the Hero.Voxel baked animation asset associated with each discovered body and equipment assembly so every model can own independent playback.

#### Scenario: Avatar and weapon are composed
- **WHEN** both models have required baked semantic clips
- **THEN** the preview binds each baked animation asset to its own model-local playback track

#### Scenario: Required animation asset is unavailable
- **WHEN** a model assembly exists but its current semantic animation asset cannot be resolved
- **THEN** independent playback proof fails for that model instead of synthesizing canonical motion procedurally

### Requirement: Preview re-solves attachment after pose evaluation
The preview SHALL resolve the current body-socket/bind-Mount attachment reference and combine it with the attachment clip's sampled reference influence. Reference influence, not translation presence, clip name, or authored loop policy, SHALL determine how the current reference contributes to the attachment visual root.

#### Scenario: Weapon remains socket-relative
- **WHEN** an equipment clip samples reference influence `1`
- **THEN** preview applies the current attachment reference to the model root while preserving that model's independently authored position, rotation, and scale animation

#### Scenario: Weapon swings around the grip
- **WHEN** an equipment clip independently animates position or rotation on the part containing its Mount while sampled reference influence is `1`
- **THEN** preview keeps the model root on the current bind-Mount reference and preserves the authored part animation so the visible weapon can move around the grip without changing logical binding

#### Scenario: Weapon releases from body movement
- **WHEN** an equipment clip transitions from influence `1` toward `0`
- **THEN** preview retains a visual reference basis for that release and progressively reduces contribution from subsequent Body socket movement without changing the logical binding

#### Scenario: Body moves during zero influence
- **WHEN** the Body socket moves while an equipment clip samples reference influence `0`
- **THEN** the equipment visual root remains independent of that Body motion while the current attachment reference continues to update separately

#### Scenario: Body motion moves the attachment
- **WHEN** the Body clip moves its socket frame while an equipment take samples reference influence `1`
- **THEN** preview applies the updated reference without changing that equipment track's selected clip, phase, speed, effective loop state, or model-local animation

#### Scenario: Body moves during a translated non-looping take
- **WHEN** a translated non-looping equipment take has started from the current attachment reference, its authored influence releases below `1`, and the Body socket subsequently moves
- **THEN** Body motion contributes to the equipment visual root only according to sampled reference influence rather than because translation or non-looping policy implies a latch

#### Scenario: Weapon blends back to a moved socket
- **WHEN** a released attachment's authored influence rises continuously from `0` toward `1` after the Body socket has moved
- **THEN** the model root continuously converges on the then-current attachment reference without a terminal-frame snap or topology operation

#### Scenario: Authored translated take returns to the socket
- **WHEN** a translated equipment take raises authored reference influence back to `1` after a released interval
- **THEN** preview converges to and resumes the then-current attachment reference according to that authored influence rather than waiting for terminal translation to trigger a snap

#### Scenario: Authored translated take remains displaced
- **WHEN** a translated equipment take reaches its terminal phase with sampled reference influence below `1`
- **THEN** preview preserves the corresponding released/blended visual root and terminal authored pose rather than imposing a universal snap-back rule

#### Scenario: Clip contains translation but remains attached
- **WHEN** an equipment clip has authored position keys and samples reference influence `1`
- **THEN** preview keeps the model root on the live attachment reference and treats the position keys only as model-local visual animation

#### Scenario: Clip contains no translation but releases
- **WHEN** an equipment clip has no authored position keys and samples reference influence below `1`
- **THEN** preview permits independent visual-root motion according to the influence contract and does not require translation as an implicit detach marker

#### Scenario: Clip omits reference influence
- **WHEN** an equipment clip has no authored reference-influence channel
- **THEN** preview uses influence `1` for backward-compatible live-reference behavior

#### Scenario: Animated Mount differs from current socket
- **WHEN** authored equipment animation moves the evaluated Mount away from the current body socket
- **THEN** preview treats that difference as authored visual motion and keeps the logical binding/reference valid instead of solving from the evaluated Mount

#### Scenario: Attachment contract is incompatible
- **WHEN** the current provider cannot resolve the declared binding reference from the body socket and equipment bind Mount
- **THEN** preview composition fails visibly rather than introducing local compensation, evaluated-Mount feedback, or pair-specific correction

### Requirement: Preview samples reference influence from provider playback state
Preview SHALL consume reference influence sampled for the provider track's current selected clip and normalized phase. It SHALL NOT infer visual attachment authority from translation keys, clip names, configured loop intent, or duplicated consumer playback state.

#### Scenario: Runtime loop override loops a normally terminal clip
- **WHEN** a consumer plays an authored non-looping equipment clip with effective runtime loop enabled
- **THEN** reference influence repeats with the provider track's wrapped phase and preview does not force terminal attachment behavior from the clip's authored loop flag

#### Scenario: Runtime loop override terminates a normally looping clip
- **WHEN** a consumer plays an authored looping equipment clip with effective runtime loop disabled
- **THEN** reference influence follows the provider track to its terminal phase and preview does not keep looping attachment behavior from the clip's authored loop flag

#### Scenario: Provider track is reset
- **WHEN** an attachment model track is reset and has no selected clip
- **THEN** preview returns that attachment to default influence `1` and the current attachment reference without retaining stale release state

#### Scenario: Direct phase sampling is requested
- **WHEN** a consumer samples an attachment clip at an exact normalized phase
- **THEN** preview uses the reference influence at that same provider-sampled phase and produces the corresponding attachment visual root from current binding/reference state

### Requirement: Direct and streamed attachment preview share reference-influence semantics
Direct avatar composition and mutable streamed actor composition SHALL apply the same reference-influence contract for equivalent selected clips, phases, binding references, rebinds, replacements, resets, and pool reacquisition.

#### Scenario: Streamed attachment releases while root moves
- **WHEN** a streamed attachment samples influence `0` and the actor root/body socket moves
- **THEN** the streamed attachment visual root remains independent in the same way as direct avatar composition while its opaque handle remains current

#### Scenario: Streamed attachment rejoins a moved reference
- **WHEN** a streamed attachment's influence rises from `0` to `1` after its current attachment reference changed
- **THEN** its visual root converges continuously to that current reference without handle replacement or a streamed-only snap rule

#### Scenario: Streamed attachment is rebound while released
- **WHEN** a streamed attachment is rebound while influence is below `1`
- **THEN** the same handle remains valid, the current reference changes to the rebound socket, and subsequent visual convergence follows authored influence

#### Scenario: Streamed attachment is replaced
- **WHEN** a streamed attachment is replaced with a new model under the preserved public handle
- **THEN** the replacement starts with fresh provider-defined playback/reference-influence state while root and unrelated attachment playback remain unchanged

#### Scenario: Warm pooled actor is reacquired
- **WHEN** a streamed actor is released and reacquired from the warm pool
- **THEN** prior activation reference-influence state is absent together with prior mutable attachments and playback state

### Requirement: Canonical proof covers continuous reference release and return
Provider-attested preview proof SHALL demonstrate a moving Body together with an attachment release interval and a continuous return interval so a terminal snap cannot satisfy the proof.

#### Scenario: Axe attack releases from a moving body
- **WHEN** Axe03 Attack is captured while the Body socket moves after the attack begins
- **THEN** attested checkpoints show the Axe trajectory continuing independently during its low-influence interval while logical binding identity remains unchanged

#### Scenario: Axe attack returns continuously
- **WHEN** Axe03 Attack reference influence rises toward `1` after the Body socket has moved
- **THEN** ordered checkpoints show the Axe converging toward the current socket over multiple phases rather than teleporting only at phase `1`

#### Scenario: Axe V1 remains spatially distinct
- **WHEN** Axe03 Attack.V1 is captured at the same ordered phases as baseline Attack
- **THEN** its authored position/rotation trajectory remains observably distinct while both takes use the same generic reference-influence contract

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
- **THEN** the runner samples independently declared model states, verifies current opaque attachment membership plus finite current attachment references, applies any declared socket-relative reference through the attachment track, submits provider render state, and only then acquires Game View pixels

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

### Requirement: Interactive preview can select variants without becoming verification authority
Interactive preview controls SHALL expose available or explicitly entered model-local variant selection for semantic motions, while registered command-driven Game View capture remains the canonical verification path.

#### Scenario: Developer switches variant during Play Mode
- **WHEN** a developer changes the selected variant while a generated gallery target is running
- **THEN** the target restarts or resamples through the normal authored motion path using the new variant selection

#### Scenario: Interactive preview is not used
- **WHEN** automated animation verification runs without opening the preview window or editing an Inspector
- **THEN** variant-aware capture remains fully executable from the repository capture command

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

### Requirement: Exact Inspector clip selection preserves semantic motion state
Selecting an exact authored clip in the Inspector SHALL map the clip back to its semantic motion, optional physical side, and optional model-local variant without changing the underlying animation vocabulary.

#### Scenario: Side-qualified variant is selected
- **WHEN** a developer selects `Attack.Left.V1`
- **THEN** preview state represents semantic `Attack`, physical side `Left`, and model-local variant `V1`

#### Scenario: Side-neutral variant is selected
- **WHEN** a developer selects `Death.V1`
- **THEN** preview state represents semantic `Death` and variant `V1` without inventing a side

#### Scenario: Baseline clip is selected
- **WHEN** a developer selects `Idle`
- **THEN** preview state represents semantic `Idle` with no optional variant selected

### Requirement: Runtime gallery exposes the current baked animation inventory
The generated runtime gallery SHALL bind every composed body/equipment model instance to its own current baked animation asset so Inspector discovery and interactive playback observe that model's authored clip inventory.

#### Scenario: Published source is rebaked
- **WHEN** a model gains a new required V1 clip and the runtime gallery is refreshed
- **THEN** that model's baked animation asset and Inspector choices include the new clip without an Inspector code change

#### Scenario: Baked animation asset is incomplete or stale
- **WHEN** the gallery cannot bind the current animation asset required by a discovered model
- **THEN** preview validation fails visibly instead of falling back to a manually maintained clip list

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

### Requirement: Provider track is the sole runtime playback authority
The interactive preview SHALL treat each provider-backed model animation track as the sole runtime authority for selected clip, playing/stopped state, phase, speed, loop behavior, and reset lifecycle. Preview-owned fields MAY retain user configuration intent but SHALL NOT maintain a second runtime playback state that can diverge from the provider track.

#### Scenario: Provider track is reset outside the player
- **WHEN** avatar or model lifecycle code resets a model track while an interactive preview controller remains enabled
- **THEN** the controller observes the provider track as stopped/unselected and does not advance an empty track on a later frame

#### Scenario: Provider track completes a non-looping clip
- **WHEN** a model track reaches its non-looping terminal phase
- **THEN** interactive preview state reports playback stopped from provider state without requiring a separate local playing flag to converge

#### Scenario: Playback is stopped or reset
- **WHEN** preview code stops or resets one model
- **THEN** subsequent update behavior derives from the provider track's current selection and playing state while unrelated model tracks remain unchanged

#### Scenario: Preview configuration is edited
- **WHEN** a developer changes configured clip, loop, or speed intent in the preview control
- **THEN** those values may be retained as preview configuration, but actual runtime playback status remains whatever the provider track reports

### Requirement: Streamed presenter is the sole mutable topology authority
The mutable streamed preview SHALL treat the active streamed presenter as the sole authority for attachment membership, counts, identity, and topology revision. Preview code SHALL retain opaque handles only as identities and SHALL NOT mirror current membership in independent booleans or positional state.

#### Scenario: Attachment is added
- **WHEN** the preview successfully attaches equipment during one presenter activation
- **THEN** current membership and counts are read from presenter queries and topology revision advances from its prior value

#### Scenario: Attachment is detached
- **WHEN** one mutable attachment is detached
- **THEN** presenter membership no longer contains its handle, the old handle fails closed, topology revision advances, and unaffected tracks preserve their state

#### Scenario: Attachment is rebound
- **WHEN** an active attachment is rebound to another valid socket
- **THEN** the same handle remains current, topology revision advances, and the attachment track/playback state is preserved

#### Scenario: Attachment is replaced
- **WHEN** an active attachment is transactionally replaced
- **THEN** the same public handle identifies the replacement, topology revision advances, the replacement receives provider-defined fresh playback state, and root/unrelated attachment tracks remain unchanged

#### Scenario: Presenter is released and reacquired
- **WHEN** the proof releases the presenter to its warm pool and reacquires it
- **THEN** prior activation handles fail closed, provider-reported mutable membership is empty, provider-defined retained track state is reset, and no preview membership shadow requires manual synchronization

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
