# Design

## Context

See proposal.md — Why. The pinned Hero.Voxel API now provides authoritative per-model playback state through `VoxelModelAnimationTrack` and authoritative streamed topology state through `VoxelActorPresenter`. Voxel-store already uses those APIs correctly for animation/composition semantics, but two preview adapters still mirror provider runtime state locally.

The ordinary runtime gallery owns its render instances directly, so `AvatarPresentation` correctly uses low-level `VoxelActorInstance` for composition. The bounded mutable proof intentionally uses `VoxelActorPresenter` because it is proving streaming/pooling/topology behavior. This ownership split remains a constraint.

## Goals / Non-Goals

**Goals:**
- make provider track state the only runtime playback truth observed by interactive model players;
- make streamed presenter queries/revision the only mutable-topology truth in the bounded streamed proof;
- make the observed reset/advance exception impossible under normal preview lifecycle operations;
- strengthen tests/proofs around provider-authoritative reset, completion, topology revision, handle staleness, and unaffected-track preservation.

**Non-Goals:**
- do not migrate ordinary gallery avatars to streamed presenters;
- do not add another voxel-store animation abstraction around `VoxelModelAnimationTrack`;
- do not change semantic clip vocabulary, variants, body/equipment independence, Death coordination, socket/Mount rules, rendering, streaming implementation, or asset formats;
- do not change the Hero Studio provider revision as part of this change unless validation exposes a provider defect;
- do not build Player/APK.

## Decisions

### 1. Separate serialized preview intent from provider runtime state

`VoxelModelMotionPlayer` keeps serialized `clipName`, `loop`, and `speed` because they are editable preview intent used to configure or restart playback. It removes the local runtime `playing` flag.

Runtime observations come from the current model track:
- selected/current clip from provider selection;
- playing/stopped from provider `IsPlaying`;
- phase/duration from provider selection;
- speed/loop execution from provider playback state.

`Update()` obtains the current track and advances only when the track exists, has a selection, and reports playing. If another lifecycle path resets the track, the next frame becomes a no-op rather than calling `Advance()` on an empty selection.

**Alternative considered:** keep the local flag and explicitly synchronize it from every reset/release caller. Rejected because future provider/lifecycle callers can reset the track without knowing about the player, recreating the same two-authority bug.

### 2. Keep provider track lifecycle generic

Play/select/sample/stop/reset continue to issue ordinary commands to one `VoxelModelAnimationTrack`. No recovery path silently reselects the configured clip after an external reset. An external reset is authoritative and remains visible until the user or preview flow explicitly plays/samples again.

This prevents a controller from undoing lifecycle ownership decisions made by avatar reset, pooling, capture cleanup, or other provider-backed code.

### 3. Opaque handles remain local identity; presenter owns membership

`MutableStreamedActorPreview` keeps `primaryHandle` and `secondaryHandle` because opaque handles are the identities needed for later commands and stale-handle proof. It removes `hasPrimary` / `hasSecondary`.

A handle is treated as currently attached only when:
1. there is an active presenter;
2. the handle value is initialized; and
3. the presenter reports `ContainsAttachment(handle)`.

The preview uses presenter attachment counts/info for assertions and request construction. `IsInitialized` alone never means membership.

**Alternative considered:** clear every handle immediately on detach/release and infer membership from default values. Rejected because stale-handle behavior is part of the streamed proof and provider membership is the authoritative contract.

### 4. Topology revision becomes an explicit proof invariant

Before attach, detach, rebind, or successful replace, the proof records `TopologyRevision`; after the operation it requires a strictly newer revision. It also verifies operation-specific state:
- attach: new handle is current and counts reflect the new member;
- detach: detached handle is absent/stale and unaffected tracks retain identity/phase;
- rebind: same handle and same track survive while placement changes;
- replace: same handle survives, replacement identity changes, replacement track is fresh, root/unrelated tracks remain unchanged;
- release/reacquire: previous activation handles are absent, mutable count is zero, retained provider playback state follows warm-reset semantics.

Revision is an observation/proof surface only; preview code does not use revision as a gameplay/state-machine trigger.

### 5. Preserve current direct-versus-streamed ownership split

Direct gallery:
`VoxelRenderPresentation -> VoxelRenderInstance -> VoxelModelAnimationTrack`, with `AvatarPresentation -> VoxelActorInstance` for physical composition.

Mutable proof:
`VoxelActorPresenter` owns streamed composition, handles, tracks, topology, bounds, and membership queries.

No layer is converted merely for API uniformity. This keeps ownership aligned with the provider's intended low-level and streamed paths.

## Risks / Trade-offs

- **Inspector config can differ from current provider runtime state after an external reset** → display runtime status from the provider separately from serialized intent; do not pretend configured intent is currently playing.
- **Removing membership booleans may expose assumptions that relied on local sequencing** → add helpers/assertions around `ContainsAttachment`, counts, and `TopologyRevision`; fail closed when a required current handle is absent.
- **A provider track may be selected but stopped, or playing may end during an update** → gate advance on both selection and provider `IsPlaying`; after advance, future frames naturally stop because provider state changes.
- **Topology-revision checks could become over-specified** → assert monotonic revision change for successful topology mutations, not exact numeric increments.

## Migration Plan

1. Add regression tests for the observed external-reset/stale-local-playback failure and provider-authoritative non-loop completion.
2. Remove `VoxelModelMotionPlayer` runtime playback shadow state and update Inspector/PlayMode expectations.
3. Add streamed topology-revision/membership assertions, then remove `hasPrimary` / `hasSecondary` and route membership through presenter queries.
4. Run focused and aggregate EditMode/PlayMode suites plus the existing provider-attested mutable/animation captures where behavior is visually relevant.
5. Run Tool scan/verify, strict OpenSpec validation, and Git hygiene/diff checks. Archive/commit/publish only after the preview remains healthy.
6. Rollback is a normal revert of the voxel-store change; no asset/provider migration is required.
