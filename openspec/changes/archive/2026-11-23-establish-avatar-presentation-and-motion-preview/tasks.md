# Tasks

## 1. Establish presentation ownership and semantic direction

- [x] 1.1 Add the `AvatarMotion` semantic set (`Idle`, `WalkForward`, `Attack`, `Hit`, `Death`) and focused tests proving the supported values are stable and contain no random/demo-only motion names.
- [x] 1.2 Introduce `AvatarPresentation` to own `UnitRoot`, `PresentationRoot`, Body mesh/bounds, canonical local Right/Up/Forward, and attached equipment presentations; verify canonical current Forward resolves toward the avatar's visible front rather than raw `+Z` assumptions.
- [x] 1.3 Introduce `EquipmentPresentation` with stable Body-ratio `PlacementRoot` plus identity-neutral `MotionRoot`; verify attachment uses the existing ratio math and motion reset cannot alter placement.
- [x] 1.4 Convert one gallery avatar vertical slice to `AvatarPresentation` + two `EquipmentPresentation` instances and verify hierarchy, dimensions, orientation, and socketless composition in the real generated scene.
- [x] 1.5 Audit every published reusable avatar/equipment VoxelSpec by semantic Forward, normalize each nonconforming source independently to the shared canonical Unity model frame (`+X` Right, `+Y` Up, `-Z` Forward; source `-Y` Forward), regenerate machine-owned `.generated.vox` and proof through Hero.Voxel for every changed source, verify `front.png` represents semantic facing/action direction, reseal evidence, and verify no blanket or runtime attachment rotation is required.

## 2. Replace random presenter with coordinated body/equipment motion

- [x] 2.1 Introduce `AvatarMotionPlayer` as the single DOTween owner for avatar `PresentationRoot` and all equipment `MotionRoot`s; verify Play/Stop/switch/disable kill owned tweens and reset neutral transforms deterministically, while destroy kills the owned sequence without requiring restoration of objects being destroyed.
- [x] 2.2 Implement `Idle` as a neutral/general presentation state with no equipment-placement mutation and verify repeated entry/exit produces no transform drift.
- [x] 2.3 Implement `WalkForward` using a Crossy Road-style launch/apex/landing presentation with coordinated equipment secondary motion; verify the reusable motion player does not take production UnitRoot locomotion authority.
- [x] 2.4 Implement `Attack` with backward anticipation, fast semantic-Forward drive, contact/follow-through/recovery, and explicit equipment attack motion; verify the strike moves toward the canonical visible front and equipment MotionRoots visibly animate.
- [x] 2.5 Implement `Hit` as opposite-Forward recoil with equipment inertia and deterministic recovery.
- [x] 2.6 Implement `Death` as terminal Body fall plus coordinated equipment follow-through while preserving equipment PlacementRoots.
- [x] 2.7 Remove random seed/delay state, `PlayNext()`, random motion selection, and the old `VoxelStoreRuntimeAvatarPresenter` after the new motion player is proven; verify no second runtime motion authority remains.

## 3. Add registered Game View capture and optional Editor motion control

- [x] 3.1 Add preview-owned schema-v2 `Capture/CaptureRegistry.json` with opaque `avatar-motion-gallery` scenario, exact `Capture.Editor` `RunFromCommandLine` runner, `Library/Capture/AvatarMotionGallery` output, and `captureKind=GameView`; verify the registry owns discovery metadata only.
- [x] 3.2 Add `Capture.Editor/AvatarMotionCaptureRunner` and Editor-only asmdef referencing `Hero.Consumer.Editor`; follow the Core Raid lifecycle pattern with `SessionState` + `[InitializeOnLoad]` polling across Play Mode/domain reload, but drive voxel-store `AvatarMotionPlayer` directly without introducing an Evaluation dependency.
- [x] 3.3 Add repository command `tools/capture-game-view.ps1 [-Scenario <name>]` as the sole normal automation entry point for the non-consumer `Preview/Unity` workspace. Verify `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery` resolves the registry, reuses pinned Hero Studio Unity-project and Game View registry/attestation helpers, launches only an owned graphical Editor, never uses `-batchmode`/`-nographics`, waits for the exact child, rejects stale evidence, and reports scenario/output/attestation/PNG-count/log plus `CAPTURE_ATTESTED` without callers supplying `-executeMethod`, runner names, or output paths.
- [x] 3.4 Capture deterministic representative phases for `WalkForward`, `Attack`, `Hit`, and `Death` by driving the same `AvatarMotionPlayer` implementation and calling `Hero.Consumer.Editor.GameViewCapture.CaptureAsync` at each checkpoint; verify camera RenderTexture output is not accepted as canonical proof.
- [x] 3.5 Keep `AvatarMotionPreviewWindow` with Motion, Loop, Speed, target scope, Play, Stop, and Reset as optional exploration UX; verify it uses the runtime player's public API rather than driving animation through `EditorApplication.update`, and verify capture proof does not depend on the window being open.
- [x] 3.6 Support optional pre-Play selection and live Play Mode switching for all avatars and selected-avatar scope; verify switching kills/resets previous body and equipment tracks without making manual interaction a completion gate.
- [x] 3.7 Update the runtime gallery builder to compose every avatar through the new presentation/equipment/motion components and preserve the discovered model-catalog grid plus standalone non-avatar entries.

## 4. Prove general combat presentation behavior

- [x] 4.1 Add EditMode tests for Body bounds, canonical direction, equipment PlacementRoot/MotionRoot separation, multi-equipment composition, and neutral reset.
- [x] 4.2 Add PlayMode tests that explicitly select each semantic motion and verify `AvatarMotionPlayer` animates only presentation-owned transforms, keeps authoritative UnitRoot unchanged except when a preview-only locomotion driver is intentionally active, and animates equipment during combat motions.
- [x] 4.3 Run `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`; require current provider `CAPTURE_ATTESTED` output, then inspect the command-reported attested PNGs to verify Walk reads as a Crossy Road hop, Attack drives visibly forward, Hit recoils backward, Death is terminal, and weapon motion remains attached but independently animated.
- [x] 4.4 Verify through the registered runner/runtime structure that every checkpoint applies one semantic motion consistently across all avatars with no random scheduling and no `@Socket.*`, Hand, Arm, mount, or pair-specific correction objects; verify provider capture success is not misreported as a semantic PASS verdict.

## 5. Documentation and closeout

- [x] 5.1 Update `WORKFLOW.md` to describe `AvatarPresentation`, `EquipmentPresentation`, `AvatarMotionPlayer`, semantic direction, body/equipment motion ownership, `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery` as the canonical proof command, the registry/`Capture.Editor`/provider-attestation implementation behind that command, and `AvatarMotionPreviewWindow` only as optional interactive exploration.
- [x] 5.2 Run the applicable preview EditMode tests, PlayMode tests, `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`, store verification regression, and `git diff --check`; confirm the capture command returns current provider-attested graphical Game View evidence without manual Editor menu/button/Inspector interaction or camera RenderTexture proof, generated capture state stays under `Library/Capture/**`, and the equipment-orientation migration preserves asset identity, VPU, voxel count, physical scale, and Body-ratio placement semantics.
- [x] 5.3 Review the final diff and remove obsolete random-presenter code/documentation plus obsolete `AvatarMotionGalleryCapture` camera-render proof authority so only one presentation/motion architecture and one canonical runtime Game View capture path remain.

## Workflow follow-up

- Sync the `asset-compatibility` and `unity-baked-preview` deltas into the main specs after implementation and validation.
- Archive `establish-avatar-presentation-and-motion-preview` only after the runtime gallery, registered provider-attested graphical Game View animation proof, optional motion-selection Editor workflow, body/equipment animation review, and final hygiene review are accepted.