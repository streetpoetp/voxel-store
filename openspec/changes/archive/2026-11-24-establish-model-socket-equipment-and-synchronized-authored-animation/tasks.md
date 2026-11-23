# Tasks

## 1. Establish attachment and animation contracts

- [x] 1.1 Add current manifest-contract support for equipment `attachment.capability`, `attachment.anchor`, and reusable compatible-side policy (`Any`/`Left`/`Right`); verify tool tests accept `Hand/Mount/Any`, reject form-specific capabilities such as `SwordSlot`, and do not require left/right duplicate asset identities.
- [x] 1.2 Replace rigid-avatar/equipment validators that forbid sockets/animations with validators for `Equip.<Capability>.<Side>.<Instance>`, exact equipment `Mount`, canonical model frame, baseline semantic clips, and declared side-reactive variants; verify focused StoreEngine tests cover unsupported/missing side, malformed/duplicate sockets, and missing/incorrect-loop clips.
- [x] 1.3 Update catalog/health projection to expose attachment capability/anchor, every side-aware slot address, counts by capability+side, equipment compatible-side policy, and base/side semantic motion coverage from authoritative source; verify generated catalog remains reconstructible and non-authoritative.
- [x] 1.4 Update publication/compatibility documentation for socket and semantic-animation ownership and verify `openspec validate establish-model-socket-equipment-and-synchronized-authored-animation --strict` passes.

## 2. Migrate published avatar models

- [x] 2.1 Define the baseline shared slot kinds and lateral semantics for current rigid avatars; author required multiplicity as distinct `Equip.<Capability>.<Side>.<Instance>` sockets directly on each Body, including realistic Left/Right hand separation, and verify every socket compiles/evaluates through Hero.Voxel.
- [x] 2.2 Author baseline `Idle`, `WalkForward`, `Attack`, `Hit`, and `Death` clips in every published avatar plus required `Attack.Left`/`Attack.Right` variants for bodies with side-reactive Hand behavior; verify every current clip targets only the actual single `Body` part and proves distinct whole-Body left/right reactions without assuming limb parts.
- [x] 2.3 Regenerate/reseal avatar source/proof after socket/animation migration and verify existing id, UnitRef, VPU, canonical model frame, and visual identity remain unchanged.

## 3. Migrate published equipment catalog

- [x] 3.1 Move every published equipment manifest to the new attachment contract with one shared logical capability and `attachment.anchor: Mount`; verify no equipment names a concrete avatar instance or form-specific slot.
- [x] 3.2 Author one exact `Mount` socket in every published equipment VoxelSpec at its asset-specific grip/mount frame; for current one-part equipment place Mount at the authored part pivot, verify generation preserves canonical model Forward, and verify no runtime pair-specific corrective offset/rotation is required.
- [x] 3.3 Author asset-owned `Idle`, `WalkForward`, `Attack`, `Hit`, and `Death` clips for every equipment model, with form-appropriate unique `Attack` curves; for current one-part equipment use rotation/scale only on the Mount-bearing part while equipped (no position track), and verify Hero.Voxel proof demonstrates actual catalog animation.
- [x] 3.4 Regenerate/reseal all equipment and verify id, UnitRef, form, VPU, canonical frame, and accepted visual authority remain stable.

## 4. Replace ratio compatibility with socket/animation compatibility

- [x] 4.1 Implement capability + compatible-side to deterministic concrete-avatar-socket-set resolution and equipment-root solving as `currentBodySocket × inverse(bind Mount)`; verify tests cover Left/Right distinction, multiple same-kind same-side slots, repeated equipment instances, capacity exhaustion, side incompatibility, and missing capability.
- [x] 4.2 Implement deterministic side-aware semantic clip resolution (`Motion.Side` -> `Motion`) plus normalized-phase sampling; verify left/right driver bindings select distinct body clips, equipment may fall back to its base clip, body owns wall-clock phase, and differing authored durations do not drift.
- [x] 4.3 Solve equipment attachment each sample from the current animated body socket and equipment bind Mount, then sample equipment animation relative to that root; verify tests prove one-part weapon rotation is visible, Mount-at-pivot keeps grip position stable, evaluated-Mount inverse solving would cancel motion, and prohibited position tracks are rejected.
- [x] 4.4 Replace ratio-placement compatibility evidence with socket/Mount/motion/phase evidence, regenerate compatibility for the full published matrix, and verify store health identifies missing sockets/clips or pair-specific correction needs causally.
- [x] 4.5 Remove obsolete Body-ratio attachment authority from compatibility code after the full socket path is green; verify searches show no publication/compatibility fallback to ratio placement.

## 5. Converge Unity preview to authored synchronized playback

- [x] 5.1 Extend runtime-gallery discovery/composition to resolve each model's baked `VoxelAnimationAsset` alongside its assembly; verify models with missing/stale animation assets fail the synchronized preview path explicitly.
- [x] 5.2 Replace static ratio attachment presentation with evaluated side-aware body socket + equipment Mount binding while preserving the canonical model frame; verify EditMode tests cover exact neutral alignment, Left/Right distinction, multiple same-side instances, and repeated copies of one equipment asset bound independently.
- [x] 5.3 Replace procedural DOTween semantic motion construction with one semantic phase owner carrying an optional driver binding; resolve `Motion.Left`/`Motion.Right`, sample the current single Body clip, update body socket attachment roots from bind Mounts, then sample equipment clips; verify distinct whole-Body left/right reactions, visible one-part weapon rotation around grip, switch/reset, different clip durations, and no drift.
- [x] 5.4 Keep optional Editor motion controls as commands over the authored semantic playback API and update WORKFLOW.md; verify manual UI is not required for completion.
- [x] 5.5 Update `AvatarMotionCaptureRunner` to sample authored clips/socket alignment at deterministic phases and run `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`; require current `CAPTURE_ATTESTED` output and inspect representative left-hand vs right-hand Attack frames plus sword/spear/bow/rifle frames for distinct body-side reaction, synchronized asset-owned weapon motion, and stable Mount lock.
- [x] 5.6 Remove obsolete ratio-placement and procedural canonical-animation preview code/docs after graphical proof passes; verify searches show one attachment authority and one authored animation path.

## 6. Integration closeout

- [x] 6.1 Run store scan/write + verify after all regenerated evidence, and verify catalog/health are reconstructible from current manifests/source/proof.
- [x] 6.2 Run applicable tool tests, Unity EditMode/PlayMode tests, strict OpenSpec validation, provider-attested Game View capture, and `git diff --check`; verify no unrelated dirty workspace is modified.
- [x] 6.3 Review the final diff for identity stability, provider ownership, and absence of Body-ratio/socket dual authority; verify every tracked task has direct evidence before completion.

## Workflow follow-up

- Sync and archive `establish-model-socket-equipment-and-synchronized-authored-animation` only after implementation and graphical proof are accepted.
- Publish the archived change in a scoped commit while preserving unrelated workspace changes.
