# Tasks

## 1. Provider pin

- [x] 1.1 Fetch the repository-owned `Dependencies/hero-studio` submodule, checkout published Hero Studio revision `61bf50c135d509d4b59e051143475f708dd78aa1`, recompute the pinned `voxel-art-workflow.md` SHA, update `toolchain/hero-voxel.lock.yaml`, and verify submodule status plus provider resolution/bootstrap report exactly that revision and fingerprint.

## 2. Store contract and Tool validation

- [x] 2.1 Update focused Tool tests first to prove finite position tracks on valid one-part Mount-bearing equipment are accepted, existing invalid Mount/frame/semantic contracts still fail closed, and compatibility passes when the bind reference is valid even though the evaluated animated Mount drifts; verify the old implementation fails these new cases before the production change and the focused tests pass afterward.
- [x] 2.2 Refactor `StoreEngine` equipment composition and compatibility evaluation to remove the categorical Mount-bearing position-track ban and animated-Mount coincidence gate while preserving bind-Mount reference validation, finite pose evaluation, evidence output, density independence, and every existing rigid/single-Mount/canonical-frame rule; verify the focused Tool suite and affected store verification pass.
- [x] 2.3 Update affected durable store documentation, including `WORKFLOW.md`, from continuous socket/Mount lock to attachment-reference plus animation-authored visual motion, and run repository text scans to verify no active authority still claims one-part equipment position tracks are forbidden or animated Mount must continuously coincide with the body socket.

## 3. Unity preview ownership migration

- [x] 3.1 Update `RuntimeGalleryPlayModeTests` first to prove current attachment references are explicitly applied through equipment model tracks, authored evaluated Mount drift is allowed without changing binding identity, translated non-looping equipment motion remains latched while the body socket moves and returns to the then-current reference at terminal zero translation, independent playback controls remain model-local, and externally owned provider transforms survive bounds/submission queries; verify the old preview contract is incompatible and the focused PlayMode suite passes after migration.
- [x] 3.2 Migrate `VoxelRenderPresentation`, `AvatarPresentation`, and `VoxelModelMotionPlayer` from removed actor `Reset`/`RefreshBindings` propagation to the pinned provider attachment-reference plus model-track transform flow, deriving live-follow versus latched visual behavior from the selected clip's ordinary loop/position data with no clip-name or placement-mode state; verify focused preview EditMode/PlayMode tests pass and a source scan finds no runtime caller of the removed provider placement APIs.
- [x] 3.3 Migrate ordinary avatar-motion capture and mutable streamed actor preview/capture to handle/binding-addressed current attachment references and model-track world transforms, replacing animated grip-lock assertions with membership/reference/root-transform validation while preserving stale-handle/topology/playback checks; verify the relevant preview/capture test suites pass.

## 4. Axe03 animation-authored spatial proof

- [x] 4.1 Author `Assets/Equipment/Axe03/Source/Axe03.voxel.yaml` so baseline `Attack` and `Attack.V1` use materially distinct finite position-plus-rotation trajectories while `Idle` remains a stable carried hold and all existing semantic/V1/one-part/Mount/canonical-frame constraints remain valid; verify provider compilation and deterministic pose comparison show Idle has no translation and the same attack checkpoint yields distinct V0/V1 spatial poses.
- [x] 4.2 Regenerate only the machine-owned Axe03 generated siblings/proof through the normal Tool pipeline and visually inspect its current provider-generated animation proof for stable Idle plus readable, distinct baseline Attack and Attack.V1 trajectories; seal Axe03 only after that visual PASS, verify it republishes, then run `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery` and visually inspect the focused provider-attested Axe03 Idle/V0/V1 Game View checkpoints for integration PASS.

## 5. Integration verification

- [x] 5.1 Run the full Tool test/verification surface plus strict OpenSpec validation; verify all published assets remain valid under the new reference contract and no production validation path still rejects authored equipment translation solely because animated Mount drifts.
- [x] 5.2 Run the affected Unity EditMode and PlayMode suites against the pinned provider revision with non-zero selected tests and zero failures, and verify Unity leaves no unrelated project-settings, metadata, or generated-source dirt.
- [x] 5.3 Run `git diff --check`, final source/API/semantic boundary scans, and workspace hygiene; verify the change introduces no new VoxelSpec/manifest schema, Unity GUID/package layout, RPC/protocol, gameplay-specific attachment state, or Player/APK build.

## Workflow follow-up

- Sync the validated `asset-compatibility`, `asset-publication`, and `unity-baked-preview` deltas and archive the change after repository Verify passes.
- Create one coherent task-owned commit, fetch/rebase latest upstream if needed, rerun relevant verification after any rebase, push normally, and verify remote HEAD equals the local committed HEAD.
