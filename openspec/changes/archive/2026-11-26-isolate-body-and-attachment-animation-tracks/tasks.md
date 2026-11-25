# Tasks

## 1. Hero Studio provider foundation

- [x] 1.1 In an idle Hero Studio project, create/apply a provider change for one generic model-local animation track over a single realized voxel model; verify focused provider tests cover independent clip selection, play/advance/sample/stop/reset, loop override, speed, terminal non-looping behavior, and warm zero-allocation advance/sample.
- [x] 1.2 Refactor `hero.voxel.actor` so `VoxelActorInstance` owns composition/topology/placement only and no final public actor-wide animation plan, shared clock, shared normalized phase, or weapon-driven body reaction remains; verify actor tests prove body playback is unchanged by independent weapon commands and composition placement still follows current body sockets.
- [x] 1.3 Refactor `hero.voxel.actor.streaming` so one root track and one attachment track are exposed through current presenter/opaque attachment identities; verify attach/detach/rebind/replace preserve body and unaffected attachment playback, replace resets only the replaced model track, stale handles fail closed, and release/reacquire clears activation-local attachment tracks.
- [x] 1.4 Verify provider evaluation order samples/advances the body, refreshes socket-to-Mount bindings, then samples/advances attachment tracks without using evaluated animated Mount as root-solving input; prove with focused socket/grip tests using divergent body/weapon phases.
- [x] 1.5 Publish the completed Hero Studio provider revision after its package/EditMode suites pass, and record the exact published revision and workflow fingerprint for voxel-store adoption; do not build a Player/APK.

## 2. Voxel-store provider adoption and preview runtime

- [x] 2.1 Update `Dependencies/hero-studio` and `toolchain/hero-voxel.lock.yaml` together to the published provider revision; verify the submodule SHA and lock workflow fingerprint exactly match the published provider.
- [x] 2.2 Migrate direct runtime-gallery composition from actor-wide plans to one body model track plus one independent track per `EquipmentPresentation`; verify EditMode tests cover body Idle + weapon Attack, independent speeds/phases/loop state, and a weapon command leaving body/other equipment state unchanged.
- [x] 2.3 Migrate mutable streamed preview to root/attachment model tracks addressed by existing opaque attachment handles; verify tests cover attach, detach, rebind, replace, stale handles, root-only warm reacquire, and uninterrupted unaffected playback.
- [x] 2.4 Replace driver-side/body-reaction preview state with selected-model clip/candidate state; verify explicit `Attack.Left` remains selectable on the chosen model but attachment side or weapon playback never selects/restarts body animation.
- [x] 2.5 Replace actor-wide `AvatarMotionPlayer` control with a generic per-model preview player/control surface that discovers exact clips from each model's baked animation asset; verify Inspector/window interaction can independently control one body or one concrete equipment instance without broadcasting implicit actor-wide motion.
- [x] 2.6 Preserve socket/Mount compatibility and one-part weapon grip rules under independent playback; verify body socket motion moves the weapon root while the weapon's own rotating clip continues at an unrelated phase and translated Mount-bearing-part clips still fail compatibility.

## 3. Provider-attested graphical proof

- [x] 3.1 Update `avatar-motion-gallery` checkpoints to prove independent composition, including body Idle + weapon Attack, body/weapon different phases or speeds, and body Death + weapon Death issued as two ordinary commands; verify `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery` returns current `CAPTURE_ATTESTED` output and visually inspect attachment lock/no unintended body reaction.
- [x] 3.2 Update `mutable-streamed-actor-gallery` to mutate one attachment while body and another attachment continue playing; verify provider-attested captures prove unaffected playback survives detach/rebind/replace and warm reacquire clears only activation-local state.
- [x] 3.3 Update preview registry/control documentation and assertions so no accepted proof depends on shared actor phase, driver-side body reaction, synchronized Death, or a fresh actor-wide animation plan after topology change; verify repository searches find no active preview authority for those retired behaviors.

## 4. Integration acceptance

- [x] 4.1 Run the full voxel-store Tool test executable, `scan --write`, and `verify`; verify the store remains healthy and publication/catalog output is unchanged except for intentional derived preview/provider metadata.
- [x] 4.2 Run the relevant Unity EditMode suites for direct preview, mutable streamed actors, clip discovery, and independent playback; verify all focused and aggregate suites pass on the pinned provider revision.
- [x] 4.3 Run strict OpenSpec validation and `git diff --check`, confirm the Hero Studio pin/runtime adoption is the only dependency change, and confirm no Player/APK build was performed.

## Workflow follow-up

- Sync the two delta specs into durable `asset-compatibility` and `unity-baked-preview` requirements after implementation evidence is complete.
- Archive this change after strict validation and graphical acceptance pass.
