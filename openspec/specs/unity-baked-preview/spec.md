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
The preview workspace SHALL consume the complete repository-owned `hero.studio` Unity package at `Dependencies/hero-studio` as one whole embedded package and SHALL use that provider for assembly baking, animation baking, and baked-asset interpretation without creating a selective Hero.Voxel package projection or parallel voxel implementation. Provider execution SHALL validate current files and the declared workflow fingerprint without invoking Git or inspecting version-control history.

#### Scenario: Pinned provider is available
- **WHEN** preview refresh finds the repository-owned Hero Studio provider and its declared Hero.Voxel workflow fingerprint matches the current provider workflow file
- **THEN** Unity loads the complete `hero.studio` package from that submodule and bakes the unit's current publication-validated generated geometry and animation artifacts through its canonical Hero.Voxel APIs

#### Scenario: Repository-owned provider is available
- **WHEN** the provider files required by the lock are present under `Dependencies/hero-studio`
- **THEN** bootstrap exposes the complete `hero.studio` package as one embedded package junction and does not select, copy, or replace individual Hero Studio modules

#### Scenario: Provider is missing or contract-incompatible
- **WHEN** the Hero Studio provider files are missing or the declared workflow fingerprint does not match the current provider workflow
- **THEN** preview refresh fails before baking or presenting new results

#### Scenario: Git is unavailable
- **WHEN** Git is not installed, is absent from `PATH`, or the working directory has no available version-control metadata
- **THEN** provider resolution, preview-plan, bootstrap verification, and preview refresh continue from repository files and declared fingerprints without invoking Git

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
The preview workspace SHALL exist only for Editor inspection, validation, and capture orchestration and SHALL NOT introduce a production game runtime, build target, gameplay-facing voxel ownership boundary, or reusable voxel runtime API into voxel-store.

#### Scenario: Developer inspects a baked unit
- **WHEN** a developer selects or previews a baked assembly or animation in the workspace
- **THEN** existing Hero.Voxel Editor preview and Scene Debug behavior can be used without adding gameplay semantics or store-owned runtime components

#### Scenario: Grid is generated
- **WHEN** the all-units grid is built in the canonical preview scene
- **THEN** its generated scene objects remain disposable Editor proof state and are not treated as production runtime content

#### Scenario: Proof requires Play Mode execution
- **WHEN** a registered capture scenario enters Play Mode to exercise the real provider runtime
- **THEN** Editor/capture orchestration drives Hero Studio directly and does not depend on a voxel-store runtime assembly or reusable runtime wrapper

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

### Requirement: Preview composes equipment from baked socket frames
The Unity preview SHALL resolve carried equipment from the selected avatar equipment socket and the equipment's baked Mount socket. When published equipment exists, every avatar entry in the canonical Editor grid SHALL visibly include deterministic carried equipment while published equipment remains independently visible in its own grid entries.

#### Scenario: Equipment is attached in the canonical grid
- **WHEN** the canonical Editor grid contains published avatars and equipment
- **THEN** it resolves a compatible `Equip.<Capability>.<Side>.<Instance>` socket and aligns the equipment's `Mount` frame without a Body-ratio placement offset

#### Scenario: Avatar remains visibly armed
- **WHEN** at least one published equipment model is available
- **THEN** every avatar grid entry carries one deterministic right-hand equipment model, and when at least two equipment models are available it also carries a deterministic left-hand equipment model, while standalone equipment entries remain present

#### Scenario: Attachment cannot be solved
- **WHEN** required socket/Mount data is missing, singular, or requires pair-specific correction
- **THEN** preview composition fails visibly instead of falling back to the prior Body-ratio path

### Requirement: Preview loads authored animation for every composed model
The Editor preview SHALL load the Hero.Voxel baked animation asset associated with each discovered body and equipment assembly so every composed model can own independent Scene Debug playback.

#### Scenario: Avatar and weapon are composed
- **WHEN** both models have required baked semantic clips
- **THEN** the preview binds each baked animation asset to its own model-local playback track

#### Scenario: Required animation asset is unavailable
- **WHEN** a model assembly exists but its current semantic animation asset cannot be resolved
- **THEN** independent playback proof fails for that model instead of synthesizing canonical motion procedurally

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
Preview verification SHALL mechanically confirm that every baked published avatar and equipment model exposes the minimum V1 clip set required by publication, while graphical capture remains content-integration evidence rather than a duplicate provider-runtime conformance suite.

#### Scenario: All baked models satisfy required coverage
- **WHEN** preview validation scans the baked published model set
- **THEN** every avatar and equipment animation asset reports the required exact V1 clip names for its model kind

#### Scenario: One baked model loses a required clip
- **WHEN** a baked model lacks a V1 clip required by publication
- **THEN** preview validation fails and identifies the model and missing clip before the change is accepted

#### Scenario: Graphical integration proof runs
- **WHEN** provider-attested graphical capture executes after store-wide coverage passes
- **THEN** it proves representative store content can be composed and animated through Hero Studio without requiring voxel-store to re-test provider lifecycle mechanics

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

### Requirement: Preview proof exposes no voxel-store runtime surface
The Unity preview SHALL remain an Editor proof workspace and SHALL NOT publish a voxel-store runtime assembly, reusable voxel runtime model, playback controller, actor wrapper, presenter wrapper, streaming abstraction, pool, or duplicated model-frame/motion convention.

#### Scenario: Preview assemblies are inspected
- **WHEN** the Unity preview project is refreshed after this change
- **THEN** store-owned preview code is limited to Editor, capture, and validation orchestration and no voxel-store runtime API is available to consumers

#### Scenario: A proof needs voxel execution
- **WHEN** a store-specific proof needs rendering, animation, actor composition, attachment mutation, streaming, or pooling
- **THEN** the proof consumes the pinned Hero Studio capability directly instead of defining an equivalent store-owned runtime abstraction

### Requirement: Preview gallery discovery remains Editor-owned
The preview gallery SHALL discover current baked catalog assets and construct deterministic review layouts through Editor-only orchestration without requiring runtime components to retain model identity, composition state, or playback state.

#### Scenario: Imported baked model is discovered
- **WHEN** a baked assembly exists under the preview baked catalog tree
- **THEN** Editor discovery derives its catalog/model identity and includes it in review without adding a runtime registration layer

#### Scenario: Gallery is refreshed
- **WHEN** the current published/baked set changes
- **THEN** Editor orchestration rebuilds the generated review state from current data without relying on serialized runtime wrapper state from the previous gallery

### Requirement: Store proof consumes Hero Studio directly
Store-specific composition and capture SHALL operate directly on pinned Hero Studio public render, Actor, animation, and Editor-preview capabilities. Voxel-store SHALL supply only proof-case content choices, socket/anchor names, exact authored clips/phases, layout, checkpoints, and visual acceptance rules.

#### Scenario: Avatar equipment proof is composed
- **WHEN** capture composes a published avatar with equipment
- **THEN** Hero Studio owns render instances, actor bindings, animation tracks, attachment composition, and bounds while voxel-store owns only the selected store assets and proof inputs

#### Scenario: Provider behavior is already covered upstream
- **WHEN** a proof would only restate track lifecycle, handle staleness, pool reuse, warm reacquire, or mutable-topology semantics already owned by Hero Studio
- **THEN** voxel-store does not add or retain a separate behavioral proof for that provider mechanism

### Requirement: Baked animation inspection uses provider Editor capability
Interactive animation inspection SHALL use the pinned Hero Studio Editor inspection and Scene Debug behavior over the current baked animation asset rather than a voxel-store-owned runtime motion player or duplicated clip inventory.

#### Scenario: Developer inspects authored variants
- **WHEN** a baked animation asset contains baseline and variant clips
- **THEN** provider Editor inspection exposes the asset's actual authored clip inventory and supports selecting/scrubbing those clips without a store-maintained motion list

#### Scenario: Baked clip inventory changes
- **WHEN** rebaking adds or removes authored clips
- **THEN** inspection reflects the current baked animation asset without changing voxel-store runtime code because no such runtime layer exists

### Requirement: Preview consumes provider attachment composition directly
Store-specific avatar/equipment proof SHALL use pinned Hero Studio attachment composition directly and SHALL NOT insert a voxel-store runtime owner or mirrored composition state between proof inputs and provider objects.

#### Scenario: Avatar equipment proof is composed
- **WHEN** a proof composes a Body with one or more equipment assets
- **THEN** Hero Studio owns root/attachment bindings, model tracks, reference-influence composition, refresh/reset, and bounds while voxel-store supplies only selected assets, socket/anchor names, and exact proof clips/phases

#### Scenario: Provider capability is unavailable
- **WHEN** the pinned Hero Studio revision lacks a capability required by an existing proof
- **THEN** the proof fails clearly or is redesigned around available provider behavior rather than restoring, copying, or locally emulating the missing runtime abstraction

### Requirement: Preview proof retains only content selection
The preview SHALL own only proof-case choices such as asset, socket, exact authored clip, variant, phase, and acceptance checkpoint. Clip execution and model-local playback semantics SHALL remain provider-owned, and voxel-store SHALL NOT maintain a second motion vocabulary or candidate-resolution engine.

#### Scenario: Proof selects a side-qualified clip
- **WHEN** a capture case selects an authored clip such as `Attack.Left.V1`
- **THEN** that exact baked clip is requested for the selected model without a voxel-store candidate builder or actor-wide motion state

#### Scenario: The same equipment asset is inspected twice
- **WHEN** two proof placements use one equipment asset with different exact clips or phases
- **THEN** the provider instances remain independent while voxel-store retains only the two proof-case selections

#### Scenario: Required authored clip is missing
- **WHEN** a proof case names a clip that is absent from the selected baked animation asset
- **THEN** preview validation identifies the model and missing authored clip without procedural fallback or local semantic substitution

### Requirement: Graphical capture proves store content composition
The canonical composed-animation capture SHALL prove authored body/equipment content can be sampled independently and composed correctly through pinned Hero Studio behavior using deterministic provider-attested pixels.

#### Scenario: Mixed motions are captured
- **WHEN** capture declares body `Idle` and weapon `Attack` at independent checkpoints
- **THEN** attested pixels show the requested mixed authored motion state without a body reaction inferred from the weapon

#### Scenario: Independent phases are captured
- **WHEN** body and equipment are declared at different normalized phases
- **THEN** attested pixels preserve the requested authored states and visually valid attachment composition

#### Scenario: Separate Death clips are captured
- **WHEN** capture independently samples authored `Death` clips on body and equipment
- **THEN** attested pixels show each model's selected authored Death state without requiring a shared synchronization API in voxel-store
