# Tasks

## 1. Advance and inspect the converged Hero Studio provider

- [x] 1.1 Resolve the exact published Hero Studio revision containing the completed Presentation-to-Actor convergence, verify it is reachable from `Dependencies/hero-studio`, advance the submodule to that exact commit, update `toolchain/hero-voxel.lock.yaml` revision/workflow SHA-256, and run the repository bootstrap/provider-lock checks. Do not use a developer-local provider checkout as authority.
- [x] 1.2 Inspect the pinned `hero.voxel.actor` and `hero.voxel.actor.streaming` public types, handles, pool acquisition/release, track access, refresh/reset, bounds, and mutation semantics before editing voxel-store; record any API-name differences from the pre-convergence plan in the implementation notes rather than creating compatibility wrappers.
- [x] 1.3 Refresh only canonical provider-bound generated evidence whose inputs include the Hero Studio revision/workflow identity, as required by the Tool, and verify no active provider-bound evidence remains pinned to the previous `73fa8a8697dec95c1d38dc73bd192aacc52079b1` revision.

## 2. Migrate direct composed preview to Actor

- [x] 2.1 Add/update focused Editor tests for the post-convergence direct Actor contract: one Actor per composed avatar, existing Actor binding identity for equipment, one canonical provider track per Body/equipment model, independent playback, reference-influence release/recovery, reset/reinitialize, removal/rebind behavior where supported, and composite bounds.
- [x] 2.2 Migrate `AvatarPresentation` from `VoxelPresentation` to the pinned direct Actor API; keep voxel-store semantic socket/`Mount` selection, adopt Actor-owned root/attachment tracks through `VoxelRenderPresentation`, and delegate refresh/reset/bounds/reference-influence composition directly to Actor.
- [x] 2.3 Migrate `EquipmentPresentation` from `VoxelPresentationAttachmentHandle` to the provider's existing Actor binding identity, remove direct Presentation namespace/assembly dependencies, and verify no second handle map, release-basis state, or generic attachment resolver is introduced.
- [x] 2.4 Run focused direct preview EditMode/PlayMode tests and `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`; require provider attestation and verify the existing independent Body/equipment motion plus release/return checkpoints remain visually valid.

## 3. Migrate mutable streamed proof to Actor.Streaming

- [x] 3.1 Add/update focused proof assertions for the streamed Actor boundary: composite-capable root with zero current mutable attachments, activation-scoped `VoxelActorAttachmentHandle` membership/staleness, unaffected track identity/phase across detach/rebind, replacement semantics with fresh provider track state, finite transforms/reference influence, release/recovery trajectory, and warm reacquire reset.
- [x] 3.2 Migrate `MutableStreamedActorPreview` from `VoxelStreamedPresentation` and `VoxelStreamedPresentationAttachmentHandle` to the pinned streamed Actor/presenter and its existing attachment handle, keeping the current `VoxelStreamingService -> VoxelPresenterPool -> VoxelActorPresenterPool` chain as the only resource/topology pool path.
- [x] 3.3 Remove `Hero.Voxel.Presentation.Streaming.Runtime.Unity` usage and any second attachment mapping/state that only existed for the retired facade; use Actor-owned track lookup, mutation, world transform/visibility, refresh/reset, bounds, and warm lifecycle directly.
- [x] 3.4 Update mutable streamed capture validation/logging to the public Actor state and run `tools/capture-game-view.ps1 -Scenario mutable-streamed-actor-gallery`; require attested checkpoints for attach, released root movement, recovery/rejoin, rebind, detach, replacement, root-only warm reacquire, and post-reacquire attach.

## 4. Reconcile package boundaries, documentation, and regression coverage

- [x] 4.1 Remove `Hero.Voxel.Presentation.Runtime.Unity` and `Hero.Voxel.Presentation.Streaming.Runtime.Unity` from `VoxelStore.Preview.Runtime.asmdef` and any other active assembly/import surface; keep only the Render / Actor / streaming dependencies actually consumed.
- [x] 4.2 Update `WORKFLOW.md`, active comments, and `unity-baked-preview` documentation to state the provider boundary as Render = one realized model, Actor = composed visual entity, Actor.Streaming = pooled/streamed Actor, while voxel-store owns content/semantic choices/proof. Remove stale provider `VoxelPresentation` / `VoxelStreamedPresentation` terminology.
- [x] 4.3 Run repository stale-reference checks for retired Presentation namespaces/types/handle names outside archived OpenSpec history and require no active consumer dependency remains. Do not rename voxel-store semantic component classes solely because their English names contain “Presentation” unless they imply the retired provider layer.

## 5. Integrated verification and hygiene

- [x] 5.1 Run `dotnet run --project Tool/VoxelStore.Tool.Tests`, `dotnet run --project Tool/VoxelStore.Tool -- verify`, and provider/bootstrap checks on the final tree; require healthy dependency resolution and no publication/coverage regression.
- [x] 5.2 Run proportional Unity 6000.3.24f1 preview EditMode and PlayMode suites with non-zero selected test counts and zero failures; restore unrelated Unity-generated workspace churn. Do not build a Player or APK.
- [x] 5.3 Re-run both registered graphical scenarios (`avatar-motion-gallery` and `mutable-streamed-actor-gallery`) from the final tree, require current provider attestation, and visually confirm the final composition/motion proof still satisfies the modified preview contract.
- [x] 5.4 Run strict OpenSpec validation for `adopt-converged-voxel-actor-runtime`, `git diff --check`, task-owned scope review, and workspace hygiene checks; require no active retired provider Presentation dependency and no Player/APK artifact.

## Workflow follow-up

- Sync the completed delta spec into the main spec and archive the change only after all tracked tasks pass.
- Commit the coherent migration, fetch/rebase normally if upstream advanced, re-run required post-rebase verification, push without force, and verify remote HEAD.
