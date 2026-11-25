# Design

## Context

The current runtime gallery owns a preview-only direct composition path: each avatar/equipment model has a `VoxelRenderPresentation`, while `AvatarPresentation` coordinates those render instances through `VoxelActorInstance`. That path remains useful for authored animation and socket/Mount inspection, but it does not consume the new streamed `VoxelActorPresenterPool` mutation API.

The pinned provider now supplies `VoxelStreamingService`, `VoxelPresenterPool`, root-only `VoxelActorPresenterPool`, opaque `VoxelActorAttachmentHandle`, `AttachAsync`, `Detach`, transactional `ReplaceAsync`, and handle-keyed `CreatePlan`.

## Decisions

### 1. Add a focused proof component instead of rewriting the gallery

A single preview-only `MutableStreamedActorPreview` is serialized into the generated runtime gallery with references to one avatar and three deterministic equipment baked asset pairs. It initializes only when the dedicated capture runner asks it to run, so the existing gallery render-instance counts and avatar-motion capture remain unchanged.

### 2. Use an in-memory preview provider over real baked assets

The component implements a small `IVoxelAssetProvider` backed by the exact serialized `VoxelAssemblyAsset` + `VoxelAnimationAsset` pairs already produced by the preview bake. `VoxelStreamingService` and `VoxelPresenterPool` retain loading/residency/presenter lifetime authority; the adapter does not introduce another voxel evaluator, renderer, cache, or gameplay model.

### 3. Prove the mutation lifecycle in deterministic checkpoints

The runner drives these states in one actor activation sequence:

1. acquire root-only actor, attach primary Right and secondary Left equipment, build a handle-keyed Attack plan, sample one normalized phase;
2. detach secondary, prove its handle is stale, rebuild/sample the plan;
3. replace primary with a third equipment unit, prove the same handle resolves the new unit and the pre-replacement plan fails closed, then rebuild/sample;
4. release actor to its warm actor pool and reacquire it, prove attachment count is zero and the old handle is stale, then sample root-only;
5. reattach one equipment after reacquire and sample again, proving the new activation remains usable.

Every composed checkpoint validates root socket to child `Mount` position within the existing capture tolerance before provider-attested Game View acquisition.

### 4. Keep consumer semantics preview-owned

The proof selects current preview motion candidates (`Attack`, side, `V1`) and current socket names. Hero Studio remains unaware of inventory, weapon class, loadout meaning, or combat policy.

## Risks / Trade-offs

- The proof uses one deterministic avatar/equipment subset rather than every combination; exhaustive model/socket/animation coverage remains owned by existing store validation and `avatar-motion-gallery`.
- A small preview asset-provider adapter is required because baked assets are not stored under Unity `Resources`; the adapter owns no caching and returns caller-owned asset references only.
- The dedicated streamed demo is not the main interactive gallery path; this intentionally avoids rewriting a currently verified preview surface merely to prove a provider API.

## Validation

- focused EditMode tests for deterministic demo selection/configuration and provider identity checks;
- existing full preview EditMode suite remains green;
- `tools/capture-game-view.ps1 -Scenario mutable-streamed-actor-gallery` returns current provider attestation and the declared checkpoint images;
- visually inspect attachment continuity/replacement/root-only reacquire captures;
- strict OpenSpec validation and `git diff --check`; no Player/APK build.
