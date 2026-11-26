# Tasks

## 1. Make provider track state authoritative

- [x] 1.1 Add focused PlayMode/EditMode regression coverage for an externally reset model track while its preview controller remains enabled, plus non-loop completion; verify the pre-fix reset case reproduces no-selection advance risk and the final tests require provider-reported stopped/unselected state.
- [x] 1.2 Remove `VoxelModelMotionPlayer`'s local runtime `playing` state and derive runtime play/advance/status behavior from the current `VoxelModelAnimationTrack`; verify `Update()` never calls `Advance()` without both provider selection and provider `IsPlaying`, while serialized clip/loop/speed remain editable preview intent.
- [x] 1.3 Update the model-motion Inspector/window expectations for the intent-versus-runtime split and verify external reset, stop, clip completion, speed changes, and explicit replay remain observable without restarting unrelated model tracks.

## 2. Make streamed presenter topology authoritative

- [x] 2.1 Extend mutable streamed proof tests/assertions to record `TopologyRevision` before successful attach/detach/rebind/replace and verify it advances monotonically while handle identity, stale-handle behavior, counts, and unaffected track state follow the provider contract.
- [x] 2.2 Remove `hasPrimary` / `hasSecondary` membership shadows from `MutableStreamedActorPreview`; keep opaque handles as identities and resolve current membership through initialized-handle plus presenter `ContainsAttachment`/count/info queries, verifying required-current-handle operations fail closed.
- [x] 2.3 Strengthen release/reacquire and replacement proof so prior activation handles are absent, mutable membership is provider-reported empty after warm reacquire, replacement resets only its provider track, and root/unrelated tracks preserve or reset exactly according to provider lifecycle semantics.
- [x] 2.4 Update the existing mutable streamed capture checkpoints only where needed to attest topology-revision/membership convergence, then verify `mutable-streamed-actor-gallery` remains `CAPTURE_ATTESTED` with valid socket/Mount grip.

## 3. Integration acceptance

- [x] 3.1 Run the relevant voxel-store Unity EditMode and PlayMode suites with non-zero selected tests and verify the original `No voxel model animation clip is selected` lifecycle path no longer throws while independent body/equipment playback remains green.
- [x] 3.2 Run `avatar-motion-gallery` provider-attested capture and verify mixed body/equipment motion, independent phases, explicit Death commands, and attachment lock remain visually unchanged by the authority cleanup.
- [x] 3.3 Run the full voxel-store Tool tests, `scan --write`, and `verify`; verify `Healthy=true` / `NextJob=null` and no authored asset, catalog/publication, compatibility, or provider-pin changes are introduced.
- [x] 3.4 Run strict OpenSpec validation plus `git diff --check` and repository hygiene/scope checks; verify the final change is limited to preview authority convergence and no Player/APK build was performed.

## Workflow follow-up

- Archive the completed OpenSpec change after all tracked acceptance tasks pass.
- Commit and publish the bounded voxel-store change, then verify local and remote HEAD equality.
