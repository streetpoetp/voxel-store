# Spec Delta

## ADDED Requirements

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

### Requirement: Runtime avatar composition uses explicit presentation ownership
The Play Mode preview SHALL represent each composed avatar through an `AvatarPresentation` owner that exposes the avatar `PresentationRoot`, baked Body bounds, canonical local orientation, and attached equipment presentations without owning semantic motion playback.

#### Scenario: Avatar presentation is created
- **WHEN** the runtime gallery composes a published rigid-Body avatar
- **THEN** exactly one `AvatarPresentation` owns the Body composition surface and exposes canonical local Right, Up, and Forward directions

#### Scenario: Canonical orientation is queried
- **WHEN** preview motion needs a directional basis for a current canonical avatar
- **THEN** `AvatarPresentation` resolves visual Forward from the canonical baked orientation rather than requiring each motion to hard-code an unexplained XYZ direction

### Requirement: Equipment presentation separates placement from animation
Each carried equipment instance SHALL be represented by an `EquipmentPresentation` whose stable `PlacementRoot` is positioned from the existing normalized Body-bounds ratio contract and whose child `MotionRoot` is reserved for temporary presentation animation.

#### Scenario: Equipment is attached to an avatar
- **WHEN** `AvatarPresentation` attaches published equipment at a valid Body ratio
- **THEN** the equipment `PlacementRoot` resolves the canonical Body-ratio position and remains stable while its `MotionRoot` starts at local identity

#### Scenario: Equipment already shares avatar canonical orientation
- **WHEN** published equipment is attached to a conforming avatar
- **THEN** its `PlacementRoot` keeps identity rotation because both assets already use canonical Unity Right `+X`, Up `+Y`, Forward `-Z`

#### Scenario: Weapon motion is played
- **WHEN** a semantic avatar motion includes equipment animation
- **THEN** the motion modifies the equipment `MotionRoot` without changing the authoritative Body-ratio placement stored by the `PlacementRoot`

#### Scenario: Motion ends or is cancelled
- **WHEN** equipment motion completes, stops, switches, or the preview object is disabled
- **THEN** every equipment `MotionRoot` returns to its neutral local transform while the `PlacementRoot` remains unchanged

### Requirement: One motion player owns body and equipment presentation animation
A composed avatar SHALL use one `AvatarMotionPlayer` as the semantic DOTween presentation owner for both avatar-body motion and all attached equipment motion. It SHALL NOT own combat results, damage timing, AI decisions, or production simulation state.

#### Scenario: Semantic motion starts
- **WHEN** `AvatarMotionPlayer.Play` receives a supported `AvatarMotion`
- **THEN** it builds the corresponding body and equipment presentation tracks from the avatar's semantic orientation and current attached equipment presentations

#### Scenario: Another motion is selected
- **WHEN** a different semantic motion is requested while one is active
- **THEN** the current sequence is killed, body and equipment presentation transforms reset to neutral, and the new motion starts without accumulated transform drift

### Requirement: Preview motion grammar is semantic and deterministic
The runtime preview SHALL expose the semantic motion set `Idle`, `WalkForward`, `Attack`, `Hit`, and `Death` and SHALL NOT select motions or timing randomly for normal comparative evaluation.

#### Scenario: Runtime gallery enters Play Mode
- **WHEN** no developer changes the current preview selection
- **THEN** all avatar motion players use the explicitly configured `Motion` rather than independently choosing random motion types or delays

#### Scenario: Same motion is applied to all avatars
- **WHEN** the preview target scope is all avatars
- **THEN** every avatar executes the same semantic motion so differences in model proportions and equipment behavior can be compared directly

### Requirement: WalkForward follows a Crossy Road-style hop grammar
`WalkForward` SHALL present locomotion as a discrete launch, airborne apex, landing, and recovery cycle rather than a continuous walk cycle or an in-place arbitrary bounce.

#### Scenario: WalkForward presentation is played
- **WHEN** `AvatarMotionPlayer` plays `WalkForward`
- **THEN** the avatar presentation performs a readable hop arc with launch, apex, descent, landing, and recovery phases while attached equipment receives coordinated secondary motion

#### Scenario: Gallery demonstrates forward travel
- **WHEN** the preview scene chooses to visualize a logical forward step during `WalkForward`
- **THEN** preview-only locomotion may move the avatar's UnitRoot along `AvatarPresentation` Forward while the reusable motion player remains presentation-only and does not become production movement authority

### Requirement: Attack and hit respect canonical forward direction
Combat feedback motions SHALL be expressed relative to `AvatarPresentation` semantic Forward rather than raw world/local axis guesses.

#### Scenario: Attack is played
- **WHEN** `AvatarMotionPlayer` plays `Attack`
- **THEN** the avatar performs a small backward anticipation followed by a fast drive toward semantic Forward, and carried equipment performs a coordinated forward attack trajectory from its MotionRoot

#### Scenario: Hit is played
- **WHEN** `AvatarMotionPlayer` plays `Hit`
- **THEN** the avatar recoils opposite semantic Forward and attached equipment exhibits coordinated inertia before all non-terminal presentation transforms recover

### Requirement: Death coordinates avatar and equipment terminal presentation
`Death` SHALL coordinate the rigid Body and attached equipment as one terminal presentation sequence without modifying equipment placement authority or requiring sockets/bones.

#### Scenario: Death is played
- **WHEN** `AvatarMotionPlayer` plays `Death`
- **THEN** the Body presentation falls into the configured terminal pose and attached equipment MotionRoots perform coordinated follow-through while their PlacementRoots remain unchanged

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
