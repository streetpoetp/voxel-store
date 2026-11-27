# Design

## Context

Voxel-store is a source/evidence authority for reusable voxel assets; it is not a voxel runtime owner. Its Unity workspace is Editor-only proof infrastructure and is currently pinned to Hero Studio revision `73fa8a8697dec95c1d38dc73bd192aacc52079b1`, where the preview adopted `VoxelPresentation` and `VoxelStreamedPresentation`.

Hero Studio has since converged those wrapper layers into `hero.voxel.actor` and `hero.voxel.actor.streaming`. The Actor boundary now owns the same generic direct/streamed composition responsibilities that voxel-store needs: root/attachment identity, canonical model tracks, reference-influence composition, release/recovery, refresh/reset, bounds, mutable streamed topology, stale-handle semantics, replacement, and warm reuse. The retired Presentation packages therefore must not remain part of the consumer contract.

The preview still legitimately owns consumer semantics: `Body`, side-aware `Equip.Hand.*` socket choice, equipment `Mount`, semantic motion/variant candidates, capture phase choreography, gallery layout, and visual acceptance.

## Goals / Non-Goals

**Goals:**
- Advance the repository-owned Hero Studio pin to the exact published revision containing the Actor convergence and keep the lock/workflow fingerprint coherent.
- Migrate direct composed preview from `VoxelPresentation` to the provider Actor API without changing visible composition or independent playback behavior.
- Migrate the mutable streamed proof from `VoxelStreamedPresentation` to Actor.Streaming while preserving mutation, stale-handle, release/recovery, replacement, reset, bounds, and capture evidence.
- Remove all assembly references/imports/types whose only purpose is the retired Presentation layers.
- Preserve one canonical provider model track per realized model.
- Reconcile active spec/workflow documentation with the Render / Actor / Actor.Streaming boundary.
- Keep the existing no-Player/no-APK constraint.

**Non-Goals:**
- No Hero Studio provider refactor is part of this change.
- No production game runtime or gameplay/loadout API is added to voxel-store.
- No asset geometry, animation vocabulary, publication rules, or store metadata semantics are redesigned.
- No local compatibility facade reproducing the retired Presentation API will be introduced.

## Decisions

### 1. Pin the exact converged provider revision before consumer edits

Apply first resolves the published Hero Studio commit that contains the completed Presentation-to-Actor convergence, verifies that it is reachable by `Dependencies/hero-studio`, and advances the submodule and `toolchain/hero-voxel.lock.yaml` together. The lock's workflow SHA-256 is recomputed from that exact submodule revision.

The restored handoff identifies the convergence as already published; apply must verify the exact provider commit from the submodule itself before treating any remembered SHA as authority.

Why:
- Consumer code must be written against the exact API it will pin.
- This avoids guessing type/member names from the pre-convergence API or from conversation history.

### 2. Direct avatar composition uses Actor directly

`AvatarPresentation` remains a voxel-store semantic MonoBehaviour, but its generic provider owner becomes the direct Actor type from `Hero.Voxel.Actor.Runtime.Unity`.

The direct migration preserves the existing ownership shape:
- Body remains a caller-owned `VoxelRenderInstance`.
- the Actor owns the root canonical animation track;
- each equipment instance is attached through the Actor's existing binding identity rather than a second presentation handle;
- `EquipmentPresentation` stores only that Actor binding identity plus preview semantic fields;
- Body/equipment `VoxelRenderPresentation` wrappers adopt the exact root/attachment tracks owned by the Actor;
- reset, refresh, reference-influence composition, release/recovery, and composite bounds delegate directly to Actor.

No `VoxelPresentation` compatibility wrapper is added.

### 3. Preserve one canonical provider track per model

`VoxelRenderPresentation` continues to serve two preview cases:
- standalone model: it may own a local `VoxelModelAnimationTrack`;
- composed actor model: it adopts the exact track owned by the Actor.

`VoxelModelMotionPlayer` remains unaware of actor topology and continues to drive whichever canonical track its `VoxelRenderPresentation` exposes. Release/reinitialize clears adopted tracks before render-instance disposal.

This preserves current Inspector and capture behavior while preventing duplicate playback/reference state.

### 4. Streamed proof consumes Actor.Streaming directly

`MutableStreamedActorPreview` keeps the existing `VoxelStreamingService -> VoxelPresenterPool -> VoxelActorPresenterPool` resource/topology chain, but it no longer constructs or owns `VoxelStreamedPresentation`.

The proof acquires the provider's streamed Actor/presenter directly from the actor pool and uses its existing activation-scoped `VoxelActorAttachmentHandle` for:
- attach;
- detach;
- rebind;
- replace;
- membership/stale-handle checks;
- attachment-track lookup.

The streamed Actor directly owns `Advance/Refresh/Reset`, release-basis/reference-influence composition, bounds, and warm-activation reset.

No second handle mapping or shadow attachment table is introduced.

### 5. Keep proof at the public Actor boundary

The capture proof verifies only public consumer-observable Actor behavior:
- current attachment count/membership;
- stale-handle rejection;
- unaffected track identity and phase across mutation;
- replacement track/reset semantics;
- finite root/attachment world transforms and reference influence;
- retained released position while the root moves;
- ordered recovery toward rejoin;
- warm release/reacquire reset;
- provider-attested pixels at the existing checkpoints.

It does not require lower internal binding tables, topology revisions, or duplicated release-basis calculations.

### 6. Assembly and documentation boundary becomes explicit

`VoxelStore.Preview.Runtime.asmdef` removes references to:
- `Hero.Voxel.Presentation.Runtime.Unity`;
- `Hero.Voxel.Presentation.Streaming.Runtime.Unity`.

It keeps only the Render, Render.Streaming, Actor, Actor.Streaming, base runtime/streaming, and UniTask dependencies actually used.

Active documentation describes:
- **Render** = one realized model;
- **Actor** = one composed visual entity (root + attachments + independent tracks + final attachment composition);
- **Actor.Streaming** = pooled/streamed Actor with mutable attachments and warm lifecycle;
- **voxel-store** = authored content, semantic preview choices, pipeline, and proof.

Generic English UI/component names such as `AvatarPresentation` may remain when they describe voxel-store preview semantics; provider package/type names and ownership language must not imply a separate Presentation runtime layer.

### 7. Validation proves compatibility rather than merely compilation

Apply must run:
- repository provider/bootstrap lock checks;
- `dotnet run --project Tool/VoxelStore.Tool.Tests`;
- `dotnet run --project Tool/VoxelStore.Tool -- verify`;
- focused preview EditMode tests for direct and streamed Actor behavior;
- proportional PlayMode runtime-gallery tests;
- `avatar-motion-gallery` graphical capture;
- `mutable-streamed-actor-gallery` graphical capture;
- stale active-reference searches for retired provider Presentation namespaces/types;
- strict OpenSpec validation and `git diff --check`.

No Player or APK is built.

## Implementation notes from pinned API inspection

- The published provider revision used by this migration is `76147075200a0e222ce86221f51e7e4e6062a3a1`; the workflow SHA-256 remains unchanged.
- Direct composition uses `VoxelActorInstance` plus `VoxelActorBindingId` exactly; no compatibility handle layer is required.
- Streamed composition uses canonical `VoxelActorDescriptor`, `VoxelActorPresenterPool`, `VoxelActorPresenter`, and `VoxelActorAttachmentHandle`.
- A streamed presenter is returned to its owner through `VoxelActorPresenterPool.Release(...)`; consumer code must not invent presenter disposal ownership.

## Risks / Trade-offs

- **Actor API details changed during convergence.** The plan intentionally requires pinning and inspecting the exact provider revision before editing consumers; do not mechanically rename types from memory.
- **Direct binding identity may differ from the old high-level handle contract.** Focus tests on current membership, canonical track identity, remove/rebind behavior, reset, and bounds rather than recreating a wrapper identity layer.
- **Warm streamed lifecycle can regress if ownership is split.** Keep pool acquisition/release singular and prove stale handles plus zero-runtime-attachment reacquire.
- **Documentation may retain misleading old terminology.** Search active code/docs/specs after migration and distinguish legitimate voxel-store semantic component names from retired provider package/type names.
- **Provider pin may include unrelated changes.** Pin one exact verified revision and run the full store/provider verification surface before publication.

## Migration Plan

1. Resolve and pin the exact published Hero Studio convergence revision; update and verify the provider lock/workflow fingerprint.
2. Read the pinned Actor and Actor.Streaming public APIs and update focused tests to describe the post-convergence consumer contract.
3. Migrate direct avatar/equipment composition and canonical-track adoption to Actor; remove direct Presentation references.
4. Migrate mutable streamed proof/capture to Actor.Streaming and `VoxelActorAttachmentHandle`; remove streamed Presentation references.
5. Reconcile runtime assembly dependencies, `WORKFLOW.md`, active `unity-baked-preview` wording, and comments.
6. Run Tool/provider checks, focused Unity tests, both registered graphical capture scenarios, stale-reference searches, strict OpenSpec validation, and `git diff --check`.
7. After apply is complete and verified, sync/archive/commit/publish in the normal repository workflow.

Rollback is a coherent Git revert of the consumer migration plus provider submodule/lock update. No persistent data migration is involved.
