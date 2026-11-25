# Tasks

## 1. Replace socket/profile compatibility with the rigid Body contract

- [x] 1.1 Separate useful autonomous capture changes from the disposable `SkeletonGrunt01` hand/socket experiment, restore the avatar source to a clean pre-experiment semantic baseline, and verify the diff contains no retained arm/hand combat or pair-specific grip correction that the new design supersedes.
- [x] 1.2 Update manifest/contracts and validation so rigid-body avatars require exactly one authored `Body`, equipment no longer requires `mount.profile` / `mount.capability`, and legacy profile/socket fields are rejected or treated only as migration inputs; verify focused Tool tests cover valid single-Body assets plus articulated/mount-based rejection cases.
- [x] 1.3 Implement canonical compiled Body-local-bounds extraction and normalized ratio position resolution `center + extents * ratio` with finite `[-1,+1]` input validation; verify unit tests cover center, extrema, proportional scaling across different Body sizes, and invalid ratios/bounds.
- [x] 1.4 Replace profile/socket compatibility result generation with Body-ratio composition evidence keyed by avatar, equipment, and evaluated ratio/convention; verify focused Tool tests and generated health output identify both passing and failing representative compositions.

## 2. Establish the single-Body vertical slice

- [x] 2.1 Convert `SkeletonGrunt01` authoritative VoxelSpec to exactly one `Body` by merging current head/limb geometry, remove sockets and authored combat animation, regenerate Hero.Voxel siblings/proof, and verify generation plus publication/source-structure tests pass.
- [x] 2.2 Migrate `Sword02` to the canonical equipment-local origin/orientation contract without profile/mount metadata or avatar-specific correction data; verify standalone proof and focused compatibility evidence pass against the rigid `SkeletonGrunt01` vertical slice.
- [x] 2.3 Update Unity preview composition to create `PresentationRoot`, derive `SkeletonGrunt01` Body local bounds from the real baked assembly, place `Sword02` by normalized ratio, and verify EditMode tests assert the derived position and canonical equipment frame without any socket GameObject.
- [x] 2.4 Add a two-or-more-equipment preview scenario using independent ratio placements under the same `PresentationRoot`; verify deterministic hierarchy/positions and that adding another equipment instance requires no avatar source change.
- [x] 2.5 Create and keep a runnable Unity sample using the updated single-Body `SkeletonGrunt01`, ratio-placed `Sword02` (plus a multi-equipment variant), `UnitRoot`/`PresentationRoot`, and DOTween whole-composition attack feedback; verify the sample opens/runs directly and uses no sockets, hand parts, mount metadata, or simulation-owned tween motion.

## 3. Prove reusable DOTween whole-composition presentation

- [x] 3.1 Replace hand/arm animation playback in the combat preview with a DOTween sequence that drives only `PresentationRoot` while `UnitRoot` remains unchanged; verify EditMode tests cover authoritative-root stability, tween ownership, cancellation, destruction, and neutral-transform reset.
- [x] 3.2 Preserve and generalize the autonomous Unity capture command so it captures the real Body-plus-ratio-equipment composition at dense ordered times from deterministic front and three-quarter/weapon-side views; verify the generated report contains all expected sample labels/views and captures are reproducible after deletion.
- [x] 3.3 Run the vertical-slice capture proof and inspect the dense trajectory for continuous equipment motion, visible anticipation/contact/follow-through/recovery, and preserved Body/equipment local relationships; record the visual proof outcome and iterate the preview-only DOTween sequence until the evidence is accepted without changing asset identity or simulation semantics.

## 4. Migrate published assets and remove legacy authority

- [x] 4.1 Migrate every currently published avatar to the single `Body` source contract, regenerate its generated siblings/reference-dependent proof, and verify each avatar passes the new source/publication gates without sockets or articulated compatibility parts.
- [x] 4.2 Remove `mount.profile` / `mount.capability` from all currently published equipment manifests, bring each equipment asset into the canonical local-frame convention, regenerate representative Body-ratio compatibility evidence, and verify store health identifies no legacy mount dependency.
- [x] 4.3 Remove obsolete compatibility-profile data and code paths (`CompatibilityProfile`, `MountCapabilityContract`, profile/socket conformance, hand/socket matrix generation, and profile-owned envelope validation) after all migrated assets are green; verify no active source/spec/tool path references `Hand.0`, `Hand.1`, or profile-owned mount capabilities for ordinary composition.
- [x] 4.4 Update repository workflow/documentation to describe single-Body authoring, canonical equipment origin/orientation, normalized Body-bounds ratio placement, multi-equipment ordering, and consumer-owned `PresentationRoot`/DOTween semantics; verify documented commands and examples match the implemented validator and preview behavior.

## 5. Integration and migration closeout

- [x] 5.1 Regenerate catalog/health/proof outputs from authoritative sources and verify a second deterministic regeneration produces no semantic diff.
- [x] 5.2 Run full `VoxelStore.Tool.Tests`, full store `verify`, Unity Preview EditMode tests, and the autonomous Body-plus-equipment capture path; verify all required gates pass and the capture report is generated from current published assets.
- [x] 5.3 Review the final diff and workspace hygiene to confirm only the new rigid-body/ratio-placement architecture, migrated assets/evidence, and reusable capture tooling remain, with no temporary hand/socket experiment files or Unity Editor side effects.

## Workflow follow-up

- After implementation, run the OpenSpec sync workflow so the completed delta specs update the main specs, then validate the synced main-spec state.
- Archive `replace-hand-socket-combat-with-body-ratio-presentation` only after implementation review, runnable sample verification, autonomous visual proof acceptance, and spec sync are complete.
- Validate the archived result and verify the main specs no longer describe hand/socket/profile authority for ordinary avatar/equipment composition.
