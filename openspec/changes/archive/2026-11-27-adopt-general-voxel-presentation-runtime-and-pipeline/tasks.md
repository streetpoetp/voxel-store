# Tasks

## 1. Advance the pinned Hero Studio provider

- [x] 1.1 Advance `Dependencies/hero-studio` to exact commit `73fa8a8697dec95c1d38dc73bd192aacc52079b1`, update `toolchain/hero-voxel.lock.yaml` with that revision and the SHA-256 of the pinned `voxel-art-workflow.md`, then run the repository bootstrap/provider-lock checks and verify the submodule HEAD, lock revision, workflow hash, and embedded preview package resolve exactly.
- [x] 1.2 Refresh only canonical provider-bound generated evidence whose current inputs include Hero Studio revision/workflow identity, including `Assets/**/Reference/audit.generated.json` and `Assets/**/Proof/Compatibility/*.json` as required by the Tool; run the canonical scan/verification path and verify no active evidence outside archived OpenSpec history still records provider revision `2d7e31e641bbb4507bd02e390dafdf06abd3a6be` or an obsolete workflow hash.

## 2. Converge direct avatar preview on provider general presentation

- [x] 2.1 Add focused Editor tests for composed-avatar high-level attachment identity, one canonical provider track per Body/equipment model, root-only no-composite behavior, independent playback, reset/reinitialize, and provider-owned release/recovery; verify the new assertions fail against the current direct actor/local-resolver implementation before migration.
- [x] 2.2 Add the narrow `VoxelRenderPresentation` external-track adoption/clearing bridge, migrate `AvatarPresentation` and `EquipmentPresentation` to provider direct presentation/high-level attachment handles, delegate single-instance/composite bounds to provider APIs, delete `AttachmentReferenceInfluenceResolver`, update the runtime asmdef dependency direction, and run the focused Avatar Editor tests to green with no duplicate active track state.
- [x] 2.3 Update direct PlayMode/capture integration and `WORKFLOW.md` runtime-gallery documentation so deterministic phase sampling drives provider-owned tracks then refreshes provider presentation composition; run the relevant PlayMode tests plus `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery`, verify provider attestation succeeds, and visually inspect the accepted checkpoints for independent Body/equipment motion and release/return behavior.

## 3. Converge mutable streamed proof on provider streamed presentation

- [x] 3.1 Add focused proof/test assertions for the high-level streamed contract: composite-capable zero-current-attachment acquisition, activation-scoped current/stale handles, unaffected track identity/phase across detach/rebind, replacement preserving the high-level handle while providing a fresh reset track, release/reacquire reset, finite world transforms, retained released position during root movement, and ordered recovery toward rejoin; verify these assertions cannot be satisfied through the current raw-presenter/resolver-shadow surface without migration.
- [x] 3.2 Migrate `MutableStreamedActorPreview` from raw `VoxelActorPresenter` plus resolver dictionary to `VoxelStreamedPresentation` over the existing streaming/presenter pools, remove preview dependence on topology revision/raw attachment-reference matrices/local release-basis state, update runtime/capture assembly references, and run focused Editor/PlayMode tests proving the existing pools remain singular resource/topology owners.
- [x] 3.3 Update `MutableStreamedActorCaptureRunner` checkpoint validation/logging to observable high-level presentation state and run `tools/capture-game-view.ps1 -Scenario mutable-streamed-actor-gallery`; require provider-attested captures for attached, released-moving-root, rebound-released, early/late return, rejoined, detached, replaced, warm root-only reacquire, and reattached checkpoints, then visually inspect the sequence for continuous release/recovery and no stale presentation state.
- [x] 3.4 Reconcile the active `unity-baked-preview` documentation/comments with the migrated provider presentation boundary and verify repository search finds no active voxel-store generic reference-influence resolver, handle-to-resolver dictionary, or requirement for consumer-visible actor topology revision.

## 4. Establish the canonical asset-authoring pipeline skill

- [x] 4.1 Add `.agents/skills/voxel-store-pipeline/SKILL.md` as a thin one-asset coordinator for `Intent -> Product Target -> Construction -> Density -> Source -> Generate -> Runtime Presentation Proof -> Compatibility -> Seal -> Publish`; extend `Tool/VoxelStore.Tool.Tests/Program.cs` to verify the skill exists, enforces one-asset ownership/order, delegates stage validity to existing authorities, and allows Density/Runtime Presentation Proof to be not applicable only through owning contracts, then run `dotnet run --project Tool/VoxelStore.Tool.Tests`.
- [x] 4.2 Reduce `voxel-store-create-model` to a compatibility entrypoint that establishes one new production intent and delegates to `voxel-store-pipeline`; keep `voxel-store-maintain` responsible for deterministic plan/job selection before delegating the same asset/current stage to the pipeline; update `WORKFLOW.md` to show one canonical pipeline and extend the Tool skill assertions so create/maintain cannot retain divergent end-to-end stage definitions.
- [x] 4.3 Verify `voxel-store-product-target` remains the specialized Product Target owner and active guidance does not copy Product Target acceptance, Hero.Voxel construction, density, compatibility, sealing, or publication checklists into the pipeline; run targeted repository text checks plus the Tool test suite.

## 5. Integrated verification and change hygiene

- [x] 5.1 Run `dotnet run --project Tool/VoxelStore.Tool.Tests`, `dotnet run --project Tool/VoxelStore.Tool -- verify`, and the repository's provider/bootstrap checks on the final tree; require healthy dependency resolution, current provider-bound evidence, and no unexpected publication/coverage regression.
- [x] 5.2 Run the proportional Unity 6000.3.24f1 preview EditMode and PlayMode suites after all runtime/skill changes; require non-zero selected test counts with no failures and restore any proven Unity-generated workspace churn. Do not build a Player or APK.
- [x] 5.3 Re-run both registered graphical scenarios (`avatar-motion-gallery` and `mutable-streamed-actor-gallery`) from the final tree, require current provider attestation, and visually confirm the captured final presentation still satisfies the motion/composition proof required by the changed preview spec.
- [x] 5.4 Run strict OpenSpec validation for `adopt-general-voxel-presentation-runtime-and-pipeline`, `git diff --check`, task-owned scope review, and stale-runtime searches; require no untracked required task, no game-specific/runtime authority moved into shared store metadata, and no APK/Player artifact.

## Workflow follow-up

- Sync the completed delta specs into main specs and archive the change after all tracked tasks pass.
- Commit the coherent migration, fetch/rebase normally if upstream advanced, re-run required post-rebase verification, push without force, and verify remote HEAD.
