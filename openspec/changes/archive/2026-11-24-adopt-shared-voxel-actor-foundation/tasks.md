# Tasks

## 1. Provider and baseline gates

- [x] 1.1 Record initial consumer Git status, asset/source exclusions and current canonical preview contracts; verify existing unrelated collection/profile/archive changes are preserved.
- [x] 1.2 Resolve a published Hero Studio revision containing the actor API; verify remote availability and exact render/actor API compatibility, including explicit loop override and bind-anchor contract. If absent, report the provider prerequisite and block rather than implementing a local substitute.
- [x] 1.3 Update gitlink and toolchain lock revision/workflow SHA-256 atomically; run existing bootstrap and verify complete embedded package identity matches the exact pin.

## 2. Shared runtime gallery realization

- [x] 2.1 Replace runtime gallery per-part scene realization with one provider render world and serialized baked references; verify all multipart geometry is rendered by provider instances and no duplicate render hierarchy exists.
- [x] 2.2 Implement preview-owned initialization, matrix synchronization and teardown of caller-owned instances; test repeated initialization, scene exit and no stale submissions.
- [x] 2.3 Replace runtime gallery bounds calculation using provider-evaluated part matrices and baked geometry; test deterministic nonoverlapping grid, authored scale, camera framing, standalone equipment and unknown imported catalog.
- [x] 2.4 Keep imported catalog discovery and generated-only cleanup intact; run corresponding preview tests and update preview usage docs where realization changes.

## 3. Shared actor composition and semantics

- [x] 3.1 Refactor AvatarPresentation and EquipmentPresentation into preview-only owners of semantic configuration and provider handles/binding IDs; verify zero/N bindings, distinct Left/Right sockets and repeated same-asset instances.
- [x] 3.2 Delegate exact candidate resolution and normalized pose/attachment sampling to provider actor plans; preserve Motion.Side then Motion fallback, driver selection and missing-clip failure; migrate focused semantic tests.
- [x] 3.3 Verify actual authored bind-anchor alignment, animated grip trajectory and canonical frame for representative body/equipment clips, including Mount-containing animated parts; fail closed and report provider/authored-input prerequisite if unacceptable, with no pair-specific correction.
- [x] 3.4 Remove VoxelAnimatedAssembly, local attachment matrix math and per-part runtime pose path; update asmdefs, references and affected tests; source audit confirms no duplicate execution or fallback path remains.

## 4. Playback, controls and capture

- [x] 4.1 Delegate playback to shared actor plan execution with selected speed and explicit loop policy; test authored-loop override in both directions, differing clip durations, switching, stop/reset/disable and driver change without stale plans.
- [x] 4.2 Adapt preview window and Inspector to the provider-backed player; verify selected/all target scope, immediate motion/loop/speed restart and pre-Play configuration.
- [x] 4.3 Adapt AvatarMotionCaptureRunner to shared direct-phase sampling and render submission, preserving registry and attestation; test deterministic checkpoint selection and reset/teardown independent of interactive controls.
- [x] 4.4 Update preview instructions and test assertions to use provider matrices and observable behavior rather than removed part GameObjects, including Left/Right reactions and repeated equipment drift.

## 5. Integrated acceptance

- [x] 5.1 Run focused preview EditMode/PlayMode regression and existing store verification; record actual results, including unchanged asset publication/source authority. No APK or production build.
- [x] 5.2 Run registered tools/capture-game-view.ps1 -Scenario avatar-motion-gallery for real graphical proof without manual UI automation; validate current attestation, review dense images for forward orientation, grip trajectory, synchronized authored motions and gallery composition. Report visual failures rather than accepting attestation alone.
- [x] 5.3 Verify strict OpenSpec validity, source/diff hygiene, exact dependency pin, deletion of duplicated runtime mechanisms and preservation of baseline unrelated changes; do not mark unresolved prerequisites or unexecuted proof complete.

## Workflow follow-up

- Sync and archive only after all tracked acceptance gates pass; publish only scoped changes under the repository workflow, preserving unrelated dirty paths.
