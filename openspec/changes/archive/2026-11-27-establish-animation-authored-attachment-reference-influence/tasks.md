# Tasks

## 1. Hero Studio provider foundation

- [x] 1.1 Open an idle Hero Studio feature workspace (prefer `H:\projects\hero-studio.feature` only if clean/idle), inspect the current Hero.Voxel/OpenSpec state, and adopt/create the bounded provider change for optional clip-level reference influence plus model-track exposure; verify the provider proposal/spec/design/tasks preserve the accepted boundary that presentation—not actor/root—owns final attachment transform composition.
- [x] 1.2 Add failing Hero.Voxel Tool tests for an optional clip-level scalar `referenceInfluence` channel, then implement source DTO, compiler validation, compiled representation, generated animation-manifest serialization, and round-trip behavior; verify missing channel defaults to `1`, step/linear/smoothstep compile, and NaN/infinite/out-of-range values fail closed.
- [x] 1.3 Extend baked `VoxelAnimationClip` data and animation sampling with the scalar channel and expose the current sampled value through `VoxelModelAnimationTrack`; verify provider tests cover direct phase sampling plus runtime loop overrides in both directions and prove consumers need not inspect authored `clip.Loop`.
- [x] 1.4 Verify Hero.Voxel.Actor binding/reference APIs and topology behavior remain unchanged while sampled influence changes; cover influence `1`, `0`, intermediate values, reset, rebind, replace, stale handles, and no topology mutation in provider tests without adding automatic placement.
- [x] 1.5 Exercise sampled reference influence through mutable streamed actor/presenter tests including detach, rebind, replace, stale handles, warm-pool reacquire, and unaffected playback; verify streaming exposes the same model-track influence/reference primitives without introducing streamed placement policy.
- [x] 1.6 Run provider Tool/Unity/OpenSpec/hygiene gates, publish the Hero Studio change, and record the published commit/fingerprint; verify no gameplay-specific API, RPC/protocol change, package-layout change, or Player/APK build was introduced.

## 2. Adopt the provider in voxel-store

- [x] 2.1 Advance `Dependencies/hero-studio` and `toolchain/hero-voxel.lock.yaml` together to the published provider revision; verify the submodule SHA, lock commit, and workflow fingerprint match exactly and the voxel-store workspace contains no unrelated dependency changes.
- [x] 2.2 Compare the old/new pinned Hero.Voxel workflow, and when the fingerprint delta is limited to animation-reference guidance, mechanically re-attest unchanged reference audits to the new fingerprint; then re-run the pinned-provider bake/compile path on unchanged legacy assets before consumer migration and verify clips without `referenceInfluence` bake successfully with effective influence `1` and existing semantic/V1 inventories remain unchanged.

## 3. Replace heuristic attachment authority in preview

- [x] 3.1 Update `RuntimeGalleryPlayModeTests` first so the old consumer policy fails cases for translated-following influence `1`, rotation-only released influence `0`, intermediate blending, moving-body continuous return, and effective runtime loop overrides; verify each new test fails for the intended old heuristic before production edits.
- [x] 3.2 Replace `VoxelModelMotionPlayer.ShouldFollowAttachmentReference` and translation/loop inspection with provider-sampled reference influence plus one shared voxel-store presentation resolver state per direct equipment binding; verify PlayMode tests prove body movement does not drag influence-`0` motion, intermediate influence converges continuously, and reset/no-selection returns to the live reference.
- [x] 3.3 Migrate `AvatarPresentation` lifecycle and bounds/submission paths to apply only resolver output while keeping actor binding identity/reference queries authoritative; verify tests cover initialization, exact sampling, natural `Play/Advance`, disable/reset, moving UnitRoot, and no terminal-frame snap.
- [x] 3.4 Migrate `MutableStreamedActorPreview` and streamed capture to the same provider resolver keyed by opaque handles; verify tests/capture cover released root motion during actor movement, continuous rejoin, rebind while released, replacement freshness, detach cleanup, stale handles, and warm-pool reset with no streamed-only placement rule.
- [x] 3.5 Update ordinary avatar capture validation to observe provider-sampled influence/resolver state rather than assuming root/reference equality at every checkpoint; verify provider-attested capture can validate both fully influenced and released/intermediate states without animated-Mount coincidence.

## 4. Make compatibility evidence current and verifiable

- [x] 4.1 Add Tool tests that copy current repository-style v1/v2 compatibility evidence, prove `verify` fails obsolete/missing/stale tracked proof even when live compatibility recomputation passes, and prove `scan --write` regenerates deterministic current evidence.
- [x] 4.2 Advance compatibility evidence to a reference-influence-aware convention containing current provider identity/fingerprint plus sampled reference influence at canonical checkpoints; verify generated evidence still records bind-reference error and animated Mount drift without treating drift as failure.
- [x] 4.3 Refactor `StoreEngine` so expected compatibility evidence is produced in memory for both write and verify flows, `scan --write` persists it, and `verify` compares tracked canonical text against expected evidence; verify stale proof becomes the affected publication gate rather than a green `StoreSnapshot.Healthy`.
- [x] 4.4 Regenerate all current tracked `Assets/Avatars/**/Proof/Compatibility/*.json` files through the normal Tool path and verify all 154 files use the current convention, carry current provider identity/reference-influence fields, regenerate byte-identically, and leave no unrelated proof/catalog changes.

## 5. Author and prove Axe03 release/return

- [x] 5.1 Add explicit `referenceInfluence` curves to Axe03 baseline `Attack` and `Attack.V1` while leaving Idle/Walk at default influence `1`; verify provider compilation shows both attacks start influenced, contain a released interval, return over multiple keyed phases, and retain distinct ordinary position/rotation trajectories.
- [x] 5.2 Extend deterministic preview tests/checkpoints so a Body socket moves after Axe attack start and ordered samples cover pre-release, released, early-return, late-return, and fully rejoined phases; verify numerical root/reference distance decreases across the authored return interval rather than only becoming zero at phase `1`.
- [x] 5.3 Regenerate/seal only Axe03 machine-owned animation/proof outputs and run `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`; visually inspect attested Idle, V0 release/return, and V1 release/return frames and accept only if motion is readable, V0/V1 remain distinct, and no terminal teleport is visible.

## 6. Documentation and integration acceptance

- [x] 6.1 Update `WORKFLOW.md` to remove retired `AvatarMotionPlayer` hierarchy/API and shared-clock wording and document independent `VoxelModelMotionPlayer` tracks plus animation-authored reference influence; verify repository scans find no active documentation claiming translation implies detach, one semantic phase clock, or continuous animated-Mount lock.
- [x] 6.2 Run the full Tool test executable, `verify`, strict OpenSpec validation, and compatibility-evidence freshness checks; verify the store is healthy only with current tracked evidence and all published assets remain valid under default influence `1` unless they explicitly author the new channel.
- [x] 6.3 Run affected/full Unity EditMode and PlayMode suites with non-zero selected tests plus both provider-attested capture scenarios; verify direct and streamed attachment behavior, loop overrides, release/rejoin continuity, topology lifecycle, and Axe visual proof all pass on the pinned provider revision.
- [x] 6.4 Run final source/API/semantic scans, `git diff --check`, and workspace hygiene; verify there is no `SocketAttached`, translation-derived detach heuristic, gameplay-specific attachment API/state, RPC/protocol change, Unity GUID/package-layout churn, unrelated generated dirt, or Player/APK build.

## Workflow follow-up

- Sync the three delta specs into the main specs only after all tracked tasks pass.
- Archive the completed change, validate the archive, then commit/reconcile/push the task-owned voxel-store changes.
