# Proposal

## Why

The current runtime gallery proves Body-ratio composition, but its `VoxelStoreRuntimeAvatarPresenter` mixes avatar composition, equipment attachment, random animation selection, DOTween lifecycle, and motion definitions in one component. The resulting random slash/hop/recoil/twist showcase is difficult to compare across avatars and does not express a stable general combat-game motion grammar. Weapon motion is also only an accidental consequence of whole-root movement rather than an explicit part of attack presentation.

The approved direction is to separate composition from motion, give the objects responsibility-revealing names, animate both avatar and carried equipment through one semantic motion owner, and make verification command-driven through a registered graphical Unity capture scenario. Interactive Editor controls may still select motions for exploration, but repeated menu/button/Inspector interaction is too expensive to be a required proof path.

## What Changes

- Replace the mixed `VoxelStoreRuntimeAvatarPresenter` responsibility with:
  - `AvatarPresentation`: owns canonical Body orientation, Body bounds, `PresentationRoot`, and equipment composition.
  - `EquipmentPresentation`: owns one equipment instance's stable Body-ratio `PlacementRoot`, animation-only `MotionRoot`, and model root.
  - `AvatarMotionPlayer`: owns semantic DOTween motion playback for both the avatar presentation root and every attached equipment motion root.
- Replace random motion choice/timing with an explicit semantic `AvatarMotion` set: `Idle`, `WalkForward`, `Attack`, `Hit`, and `Death`.
- Make canonical avatar direction explicit in `AvatarPresentation`; current canonical visual forward resolves to local Unity `-Z`, so attack drive and weapon attack motion are defined relative to semantic Forward rather than raw XYZ guesses.
- Audit every published equipment model by semantic facing/action and normalize it into the same canonical Unity model frame as avatars: Right `+X`, Up `+Y`, Forward `-Z`. Avatar Forward is the outward face/gaze/chest direction. Equipment Forward is its intended functional direction when attached to a forward-facing avatar: projectile launch/target for ranged equipment and primary thrust/cut/strike/pick/sweep for directional melee equipment. Forward is never inferred from the longest dimension, a broad presentation face, an arbitrary mesh axis, or Hero.Voxel coordinate conversion. Corrections are asset-specific in authoritative voxel source, never a blanket rotation or attachment-time runtime compensation.
- Define `WalkForward` as a Crossy Road-style discrete hop grammar. The motion layer owns the presentation hop/landing; any forward UnitRoot travel used only to demonstrate locomotion in the gallery remains preview-driver behavior rather than production simulation authority.
- Define `Attack` as backward anticipation followed by a fast forward body drive and explicit equipment motion on each `EquipmentPresentation.MotionRoot`, then contact/follow-through/recovery.
- Define `Hit` as backward recoil with equipment inertia and `Death` as a terminal body fall with coordinated equipment follow-through.
- Add an `AvatarMotionPreviewWindow` that selects one motion, loop state, speed, and target scope (all avatars or selected avatar). Changing the selection during Play Mode switches live players immediately after kill/reset; no `EditorApplication.update` animation loop owns motion.
- Borrow Core Raid's registered runtime Game View capture architecture, but expose it through one repository command: `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`. The command is the canonical automation surface and owns preview-project resolution, schema-v2 `CaptureRegistry.json` scenario lookup, graphical Unity launch/process lifetime, Play Mode/domain-reload continuation, registered `Capture.Editor` runner invocation, and current provider attestation. The runner calls `Hero.Consumer.Editor.GameViewCapture.CaptureAsync(...)` at semantic motion checkpoints and emits composed Game View PNGs plus `game-view-capture.json` under `Library/Capture/**`. Callers SHALL NOT need to know or invoke Unity `-executeMethod`, runner class names, output directories, or Editor menus/buttons directly.
- Update the generated runtime gallery so all avatars are composed through `AvatarPresentation` and `EquipmentPresentation`, receive an `AvatarMotionPlayer`, and default to the same explicitly selected motion instead of independent randomness.
- Make the runtime gallery discover human-imported baked `VoxelAssemblyAsset` models from folder identity `.../<Catalog>/<Model>/Source/<Model>.asset` instead of requiring provider plan/Kind registration. `Avatars` receives avatar composition/motion, `Equipment` is attachable and standalone, other catalogs are standalone; imported catalogs are human-owned inputs and stale generated-output cleanup must preserve them. The gallery realizes complete baked part hierarchies. Carried-equipment selection is deterministic preview-only sampling from the catalog and carries no gameplay/inventory meaning.
- Retire random scheduling, `PlayNext()`, random motion selection, and the generic `*Wrapper` / mixed presenter naming from the preview architecture.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `unity-baked-preview`: refine the evaluation surface so avatar composition, equipment placement, body/equipment motion playback, canonical direction, registered provider-attested Game View capture, and optional Editor-selected semantic motion are explicit and independently testable while remaining preview-only.
- `asset-compatibility`: strengthen the canonical composition frame so avatars and equipment share one published Unity model orientation and corrective runtime rotations are forbidden.

## Impact

- Primarily affects `Preview/Unity/Assets/Preview/Runtime/**`, `Preview/Unity/Assets/Preview/Editor/**`, new preview-owned `Capture/**` and `Capture.Editor/**` surfaces, the repository command `tools/capture-game-view.ps1`, preview tests, the generated runtime-gallery composition, and `WORKFLOW.md`.
- Changes authoritative equipment voxel orientation only where each asset's semantic-front audit requires it, preserving asset identity, VPU, voxel count, colors, physical scale, Body-ratio placement math, damage/hit timing, AI, and production gameplay simulation.
- Removes the current random motion showcase rather than preserving it as an alternate mode, avoiding two competing preview motion authorities.
- Adds explicit weapon/equipment animation tracks beneath stable Body-ratio placement, so animation can move a weapon without corrupting its attachment position.
