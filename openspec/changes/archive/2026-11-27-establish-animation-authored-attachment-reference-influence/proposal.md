# Proposal

## Why

The current attachment-motion preview infers transform authority from whether a non-looping clip contains position keys. That heuristic diverges from the provider's effective loop state, cannot express rotation-only detached motion or translated socket-relative motion, and can snap a latched weapon back to a moved socket at the terminal frame.

The repository also still publishes compatibility evidence under the retired socket-lock convention, while `verify` recomputes the new reference contract without checking that tracked proof is current.

## What Changes

- Replace the consumer-side “translated non-looping clip means latched” heuristic with a generic animation-authored attachment-reference influence channel owned by Hero.Voxel clip data.
- Define reference influence as presentation-only data: `1` follows the current attachment reference, `0` preserves animation-owned detached/root motion, and intermediate values blend continuously between them. Do not introduce `SocketAttached`, weapon/Attack semantics, or topology mutation.
- Make provider-backed playback evaluate reference influence from the currently selected clip and phase while respecting the track's effective runtime loop state.
- Migrate direct and streamed preview composition to the same provider-backed reference-influence behavior so body movement, rebind, replacement, reset, and pooling do not create a second attachment-motion policy.
- Author Axe03 Attack/V1 reference-influence curves that start attached, release for their independent motion, and blend back to the body's current socket without a terminal snap.
- Regenerate compatibility proof to the current reference-based convention and make `verify` fail when tracked compatibility evidence is stale or uses an obsolete convention.
- Remove stale `AvatarMotionPlayer`/shared-clock wording from active workflow documentation.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-compatibility`: define generic animation-authored attachment-reference influence while preserving logical binding identity, independent model playback, and gameplay-neutral composition.
- `asset-publication`: allow/publish validated reference-influence animation data and require current compatibility evidence for publication.
- `unity-baked-preview`: replace translation-derived latch policy with provider-backed reference-influence sampling, continuous return behavior, effective-loop correctness, and equivalent direct/streamed proof.

## Impact

- Hero Studio provider prerequisite: `hero.voxel` animation source/compiled/baked data and sampling need a generic scalar reference-influence channel available through `VoxelModelAnimationTrack`; `hero.voxel.actor` remains responsible only for logical bindings/current attachment references.
- Voxel-store dependency pin advances only after the provider change is published.
- Preview runtime changes center on `VoxelModelMotionPlayer`, `AvatarPresentation`, and mutable streamed preview/capture.
- Tool verification and compatibility evidence generation become stricter so stale tracked evidence can no longer coexist with a green store.
- Axe03 source/proof changes are bounded to animation data; no gameplay API, RPC/protocol, Unity package layout, or Player/APK work is introduced.
