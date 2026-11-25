# Design

## Context

See `proposal.md` for motivation. The current provider already exposes model-local pose primitives on `VoxelRenderInstance` (`SetAnimation`, `SetAnimationTime`, `ClearAnimation`), but `VoxelActorInstance` and `VoxelActorPresenter` wrap those primitives in one actor-wide `VoxelActorAnimationPlan`, one elapsed clock, one normalized phase, and topology-wide playback invalidation. Voxel-store preview then adds driver-side inference and one actor motion controller on top.

Physical composition is already correct and remains authoritative: body sockets are evaluated from the body pose, equipment roots are solved from the current body socket multiplied by inverse bind-Mount, and equipment animation is evaluated relative to that root.

## Goals / Non-Goals

**Goals:**
- Make animation state model-local for the body and every attachment.
- Keep actor ownership limited to composition, topology, visibility/custom data propagation, and attachment placement.
- Preserve opaque attachment handles, streaming/pooling, stale-handle protection, socket/Mount placement, and warm reuse.
- Allow consumers to coordinate any event, including Death, by issuing ordinary commands to multiple independent model tracks.
- Ensure topology mutations do not reset unaffected model playback.

**Non-Goals:**
- No `Die`, `PlayDeath`, synchronization-group, combo, attack-driver, or gameplay-event API.
- No automatic body reaction to weapon animation or attachment side.
- No requirement for body and equipment clips to share duration, phase, speed, loop state, semantic motion, or variant.
- No new procedural animation, animated-Mount compensation, pair-specific transform correction, or gameplay ownership in voxel-store.
- No APK/Player build or Android performance re-characterization as part of this change.

## Decisions

### 1. Actor becomes composition-only

`VoxelActorInstance` retains root/binding ownership, attach/remove/rebind, topology identity, socket-to-Mount placement, visibility/custom data propagation, bounds/inspection, reset/dispose, and `RefreshBindings()`-style placement refresh.

Actor-wide motion-template/animation-plan state, elapsed seconds, current normalized phase, loop state, `Start`, actor-wide `Advance`, and actor-wide `SampleNormalized` are removed from the final provider-facing actor contract.

`VoxelActorPresenter`/streaming wrappers follow the same boundary: they own pooled composition and opaque attachment handles, not one animation plan spanning every model.

This is preferred over adding independent sub-clocks inside `VoxelActorAnimationPlan` because a single plan would still couple topology lifetime, invalidation, and model playback ownership.

### 2. Introduce one generic model-local playback track

The provider will expose one reusable model animation track abstraction adjacent to a realized model/render instance. A track owns only one model's resolved clip/candidate result, elapsed time and normalized phase, effective loop setting, playback speed, and playing/stopped state.

The generic operations are ordinary animation operations such as resolve/select, play, sample, advance, stop/clear, and reset. They contain no Body, Weapon, Death, Attack, side-driver, or gameplay-event semantics.

The track delegates pose evaluation to the existing render-instance animation primitives rather than introducing another evaluator.

### 3. Streaming actors expose model tracks by model identity/attachment handle

A streamed actor exposes one root model track and one attachment model track per active attachment handle. Attachment lookup remains handle-addressed and fails closed for stale handles.

Topology behavior is:
- attach: create/acquire a cleared track for the new attachment; existing tracks continue unchanged;
- detach: discard only that attachment track; body and other attachment tracks continue;
- rebind: retain the same attachment model/track and playback state while changing only placement binding;
- replace: preserve the attachment handle as today, replace the model realization, and reset only that handle's model-local playback because clip identity/catalog ownership changed;
- actor release/reacquire: all activation-local attachment tracks are discarded together with their activation, matching current pooling ownership.

### 4. Placement and animation evaluation have an explicit order without a shared clock

For a composed presentation update:
1. Advance/sample the body model track from its own state.
2. Refresh actor bindings from the now-current body socket matrices using inverse bind-Mount.
3. Advance/sample each attachment model track from its own state.
4. Submit the provider render world.

The ordering ensures body animation can move the physical socket before equipment placement is solved. Attachment phase is unrelated to body phase. The evaluated animated equipment Mount is never used to re-solve its root, preserving authored weapon motion around the grip.

### 5. Consumer coordination is multiple ordinary commands

The provider does not model a synchronized event. If a game decides a unit died, it can issue ordinary commands in the same update, for example: body track plays `Death`; sword track plays `Death`; shield track stops or plays `Death`; another attachment uses any consumer-selected behavior.

Those commands may start on the same frame, but each track keeps its own duration/phase/loop lifecycle. Missing clips and fallback policy remain consumer decisions. No model observes another model's playback state to infer a reaction.

### 6. Preview becomes a per-model control surface

The generated gallery keeps one composition owner for an avatar but no longer keeps one actor-wide motion player. Body and each equipment instance expose a generic preview model player bound to that model's baked animation asset.

Inspector/window choices are discovered from the selected model's exact baked clip inventory. Controls target one model instance, or explicitly broadcast as repeated independent commands, never one implicit composed motion.

Existing semantic clip parsing/candidate ordering may remain as a convenience for one selected model; it must not propagate side, motion, variant, phase, speed, or loop state to other models.

### 7. Verification proves divergence plus attachment lock

Provider tests must prove body Idle while weapon Attack; two attachments with different clips/speeds/phases/loop states; weapon commands leave body state unchanged; attach/detach/rebind/replace preserve unaffected playback; warm acquire/release clears activation-local tracks; and warm per-track advance/sample remains allocation-safe.

Voxel-store preview/capture must prove representative mixed motion, independent phases, consumer-issued body Death plus weapon Death using ordinary commands, and socket/Mount lock while body and weapon phases diverge.

## Risks / Trade-offs

- **Breaking provider API** → Migrate Hero Studio tests/consumers in the provider change, publish one coherent revision, then update voxel-store's submodule and lock together.
- **More playback state objects** → Keep tracks lightweight and pool/reuse them with the same presenter lifetime; add warm allocation assertions.
- **Update-order regressions can cause visible grip lag** → Test and capture body-sample → binding-refresh → attachment-sample ordering explicitly.
- **Replace cannot safely preserve an old clip on a new model** → Preserve the attachment handle but clear only the replacement model's playback state.
- **Consumers may want synchronized timing later** → Keep synchronization outside this foundation; consumers can command/sample multiple tracks explicitly without expanding actor semantics.

## Migration Plan

1. In an idle Hero Studio project, propose/apply the provider change that separates actor composition from generic model-local animation playback; migrate provider actor-streaming tests and publish the provider revision.
2. Update voxel-store's `Dependencies/hero-studio` pin and `toolchain/hero-voxel.lock.yaml` to that published revision.
3. Migrate direct preview composition from actor-wide animation plans to one body track plus one track per equipment instance.
4. Migrate mutable streamed preview proof to root/attachment track access keyed by existing opaque attachment handles.
5. Replace driver-side/body-reaction preview controls with selected-model controls and model-local clip discovery.
6. Update EditMode tests and provider-attested capture scenarios for independent playback and topology isolation.
7. Run Tool/store/OpenSpec/Unity EditMode/capture validation. Do not build a Player/APK.

Rollback is one coherent pin rollback plus reversal of the voxel-store preview adoption; store asset/source formats do not change.
