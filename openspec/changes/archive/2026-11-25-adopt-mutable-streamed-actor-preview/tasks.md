# Tasks

## 1. Provider Adoption

- [x] 1.1 Pin Hero Studio and the voxel workflow lock to the published mutable streamed actor revision; verify bootstrap rejects any mismatch and the workflow fingerprint remains current.
- [x] 1.2 Add the minimum preview runtime assembly references required for streaming actor composition without changing production/store ownership boundaries.

## 2. Mutable Streamed Demo

- [x] 2.1 Add a preview-only baked-asset streaming provider and `MutableStreamedActorPreview` that acquires a root-only actor through `VoxelStreamingService` + `VoxelPresenterPool` + `VoxelActorPresenterPool`.
- [x] 2.2 Exercise two attachments, handle-addressed plans, detach/stale-handle rejection, transactional replacement with same-handle continuity and stale-plan rejection, release/reacquire root-only cleanup, and post-reacquire reattachment.
- [x] 2.3 Extend the runtime gallery builder with one deterministic demo configuration from the sorted baked Avatar/Equipment catalogs while preserving existing gallery model counts and normal capture behavior.

## 3. Proof and Regression

- [x] 3.1 Add a schema-v2 `mutable-streamed-actor-gallery` capture scenario that drives the demo through deterministic checkpoints and validates socket/Mount grip before capture.
- [x] 3.2 Add focused Editor tests and run the full preview EditMode suite; verify existing direct avatar gallery behavior remains green. Full `VoxelStore.Preview.Editor.Tests` result: 33/33 passed on Unity 6000.3.24f1.
- [x] 3.3 Run provider-attested graphical capture for the new scenario and visually inspect attach, detach, replace, root-only reacquire, and reattach checkpoints. Capture produced 5 current attested PNGs with expected lifecycle states and no obvious grip separation.
- [x] 3.4 Run strict OpenSpec validation and `git diff --check`; preserve unrelated dirty work and do not build a Player/APK.
- [x] 3.5 Upgrade to Hero Studio `5277509d13c0039ddee26c7aafafdc10be415b08` and migrate the proof from raw render-instance inspection to presenter-owned read-only identity/membership/socket/bounds queries. Bootstrap passed at the exact pin with unchanged workflow fingerprint; full Tool tests passed; isolated seeded `VoxelStore.Preview.Editor.Tests` passed 33/33 on Unity 6000.3.24f1 while the user's live editor remained open; provider-attested `mutable-streamed-actor-gallery` produced 5 current Game View PNGs covering attach/detach/replace/root-only reacquire/reattach and visual inspection found no obvious grip separation.
