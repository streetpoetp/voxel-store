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
The preview proof workflow SHALL support deterministic temporal sampling of a unified Body-plus-equipment presentation trajectory at enough ordered normalized times and views to reveal discontinuities, reversals, hidden equipment, and weak body motion.

#### Scenario: Presentation motion is evaluated
- **WHEN** a presentation proof is captured for a combat or feedback motion
- **THEN** the preview emits ordered samples covering ready, anticipation, strike/contact, follow-through, recovery, and intermediate trajectory states from deterministic views

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

### Requirement: Animation verification uses registered provider-attested Game View capture
Canonical animation proof SHALL run through `tools/capture-game-view.ps1`, backed by a preview-owned schema-v2 registry and graphical `Capture.Editor` runner. Accepted images SHALL be acquired through `Hero.Consumer.Editor.GameViewCapture` and current provider attestation under `Library/Capture/**`. Direct camera/RenderTexture output is diagnostic only.

#### Scenario: Capture scenario is discovered
- **WHEN** avatar-motion visual verification is requested
- **THEN** the launcher resolves the declared preview capture scenario from schema-v2 registry metadata rather than requiring an agent to know or type an arbitrary runner/output pair

#### Scenario: Caller uses only the repository command
- **WHEN** automated verification needs avatar-motion captures
- **THEN** the caller SHALL NOT need to invoke Unity `-executeMethod`, know the fully-qualified runner class, hard-code the output directory, or operate Editor menus/buttons; those details SHALL remain behind `tools/capture-game-view.ps1` and the capture registry

#### Scenario: Capture is invoked by command
- **WHEN** an agent or developer runs `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`
- **THEN** that command resolves the non-consumer `Preview/Unity` workspace and preview registry while reusing the pinned Hero Studio Unity/capture-attestation helpers, launches an owned graphical Editor without `-batchmode`/`-nographics`, waits for that exact child, rejects stale/unattested output, and reports the selected scenario, output directory, attestation path, PNG count, Unity log, and `CAPTURE_ATTESTED` result

#### Scenario: Play Mode reload occurs
- **WHEN** the registered `Capture.Editor` runner enters Play Mode and Unity reloads the domain
- **THEN** capture state survives through Editor session state and a persistent Editor poll/rebind path resumes against the live runtime gallery rather than requiring manual Play/menu interaction

#### Scenario: Representative combat motion is captured
- **WHEN** `WalkForward`, `Attack`, `Hit`, or `Death` requires visual proof
- **THEN** the runner drives the same `AvatarMotionPlayer` semantic implementation used by interactive playback to a declared deterministic checkpoint and invokes `GameViewCapture.CaptureAsync` so the resulting image is a composed graphical Game View capture

#### Scenario: Semantic translation must be visible in pixels
- **WHEN** a captured motion translates along semantic Forward or opposite Forward
- **THEN** the capture runner uses an evidence-only oblique Game View camera pose around the same runtime gallery bounds so that the semantic motion has a visible screen-space component without changing the ordinary gallery camera contract

#### Scenario: Capture completes
- **WHEN** all declared checkpoints have been acquired
- **THEN** the runner resets motion state, exits Play Mode cleanly, clears persisted capture state, exits the owned Editor with success, and the launcher reports success only after validating current provider attestation and readable current PNGs

#### Scenario: Capture pixels are reviewed
- **WHEN** the shared capture contract reports `CAPTURE_ATTESTED`
- **THEN** that result proves only current graphical Game View acquisition and file integrity; agent or human inspection remains responsible for judging whether the motion is visually correct

#### Scenario: Interactive controls are unavailable
- **WHEN** the custom Editor window is closed and no hierarchy object is manually selected
- **THEN** registered Game View capture remains fully executable and does not lose coverage

### Requirement: Editor control optionally selects preview motion
The Unity preview SHALL provide an `AvatarMotionPreviewWindow` that allows a developer to select the semantic motion, loop state, playback speed, and target scope for interactive exploration of the generated runtime gallery. This window SHALL NOT be required for automated verification.

#### Scenario: Developer selects a motion before Play Mode
- **WHEN** a developer selects a semantic motion in the preview window while the Editor is not playing
- **THEN** the selected preview state is applied to the generated gallery configuration and is used when Play Mode begins

#### Scenario: Developer switches motion during Play Mode
- **WHEN** a developer selects another semantic motion while the generated gallery is running
- **THEN** the window sends the explicit selection to the live target `AvatarMotionPlayer` instances and they switch through the normal kill/reset/play lifecycle

#### Scenario: Developer edits Motion in the component Inspector during Play Mode
- **WHEN** the serialized `Motion`, Loop, or Speed field on an active `AvatarMotionPlayer` changes in the Inspector
- **THEN** the component immediately restarts playback from neutral using the new configured values without requiring disable/enable or scene restart

#### Scenario: Developer targets one avatar
- **WHEN** target scope is selected-avatar and a gallery avatar is selected
- **THEN** only that avatar's motion player is controlled while all-avatar scope continues to broadcast to every gallery avatar

#### Scenario: Verification runs without the Editor window
- **WHEN** automated verification runs with no `AvatarMotionPreviewWindow` interaction
- **THEN** the registered provider-attested Game View capture scenario remains the authoritative proof path and produces current composed runtime pixels without depending on the window

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
The runtime gallery SHALL load the Hero.Voxel baked animation asset associated with each discovered avatar and equipment assembly.

#### Scenario: Avatar and weapon are composed
- **WHEN** both models have required baked semantic clips
- **THEN** the preview binds their baked animation assets to the composed presentations

#### Scenario: Required animation asset is unavailable
- **WHEN** a model assembly exists but its current semantic animation asset cannot be resolved
- **THEN** synchronized motion proof fails instead of synthesizing canonical motion procedurally

### Requirement: Preview motion context preserves selected slot side
The runtime preview SHALL keep the selected physical driver binding, including Left/Right/Center side, as part of semantic motion state so body animation can react differently to the active attachment side.

#### Scenario: Left-hand Attack is selected
- **WHEN** the preview drives `Attack` from equipment bound to a Left Hand slot
- **THEN** it resolves the body's left-side Attack variant and shows the corresponding left-handed body response

#### Scenario: Right-hand Attack is selected
- **WHEN** the same equipment asset is instead bound to a Right Hand slot and drives `Attack`
- **THEN** it resolves the body's right-side Attack variant without requiring a different equipment asset identity

### Requirement: One semantic phase drives body and equipment authored clips
The runtime preview SHALL use one body-owned normalized semantic phase to sample both the body clip and every attached equipment clip.

#### Scenario: Attack plays in real time
- **WHEN** `Attack` playback advances
- **THEN** body wall-clock cadence advances one normalized phase and every equipment `Attack` clip is sampled at that exact phase

#### Scenario: Motion is sampled for capture
- **WHEN** command-driven capture requests a deterministic normalized phase
- **THEN** body/equipment authored clips, selected driver side variant, and attachment sockets are evaluated at that same phase before Game View capture

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

### Requirement: Graphical capture proves authored synchronized animation
The canonical `avatar-motion-gallery` capture scenario SHALL prove current model-owned socket attachment and authored body/equipment semantic animation through provider-attested Game View pixels.

#### Scenario: Capture runs
- **WHEN** `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery` completes
- **THEN** attested checkpoints show body and equipment sampled at declared semantic phases, visibly locked to the correct side-aware socket, with separate left/right body reactions proven for a side-reactive hand motion

#### Scenario: Legacy procedural motion still appears authoritative
- **WHEN** visual proof depends on the old Body-ratio/DOTween motion path instead of baked sockets and authored clips
- **THEN** the change is not complete
