# Proposal

## Why

The preview currently shadows Hero.Voxel runtime state in two places: `VoxelModelMotionPlayer` keeps a local `playing` flag even though `VoxelModelAnimationTrack` owns authoritative playback state, and `MutableStreamedActorPreview` keeps local attachment-membership booleans even though `VoxelActorPresenter` owns current membership/topology. Those duplicate truths can drift, as demonstrated by `VoxelModelMotionPlayer.Update()` advancing a provider track after the provider selection was reset and throwing `No voxel model animation clip is selected`.

## What Changes

- Remove preview-owned runtime playback truth that duplicates `VoxelModelAnimationTrack`; serialized clip/loop/speed remain preview configuration, while selected clip, playing/stopped state, phase, speed, loop policy, and lifecycle state come from the provider track.
- Make `VoxelModelMotionPlayer.Update()` advance only an actually selected, provider-playing track, so external reset/release paths cannot leave a stale local play flag that advances an empty track.
- Remove mutable streamed preview membership booleans and use `VoxelActorPresenter` membership/count/info queries plus opaque handles as the only topology authority.
- Strengthen streamed proof around `TopologyRevision`, stale handles, rebind preservation, replacement-local reset, detach isolation, and warm release/reacquire.
- Keep direct avatar composition on caller-owned `VoxelRenderInstance` + `VoxelActorInstance`; do not migrate ordinary gallery avatars to streamed presenters.
- Keep body/equipment animation independent and consumer-coordinated; do not add Death/Attack/synchronization/body-reaction semantics.
- Do not change authored voxel assets, sockets/Mount compatibility, catalog/publication authority, render architecture, or Player/APK behavior.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `unity-baked-preview`: make pinned Hero.Voxel playback and streamed presenter state the sole runtime authorities for preview animation and mutable topology, with preview code retaining only intent/configuration and opaque identities.

## Impact

Affected code is expected to be limited to preview runtime adapters and focused proof surfaces, primarily `Preview/Unity/Assets/Preview/Runtime/VoxelModelMotionPlayer.cs`, `MutableStreamedActorPreview.cs`, their Editor/PlayMode tests, and mutable capture assertions where topology revision is proven. The existing Hero Studio pin/API, direct `AvatarPresentation` composition architecture, voxel assets, compatibility rules, generated catalog, and rendering backend remain unchanged.
