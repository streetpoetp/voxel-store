# Design

## Context

The preview project currently has two materially different realization paths. The canonical interactive grid in VoxelStorePreview.cs uses VoxelSceneDebugBuilder, VoxelSceneDebugController, GameObject mesh hierarchies, and manual socket-to-Mount transform alignment in Edit Mode. AvatarMotionCaptureRunner already uses the real provider runtime: VoxelRenderWorld, VoxelRenderInstance, VoxelActorInstance, canonical root/attachment tracks, Actor refresh, runtime bounds, and provider draw submission.

Hero Studio now adds VoxelRuntimeInspectionController in the Actor Editor assembly. It binds a transient plain selection host to an already-realized runtime instance/track and optionally its owning Actor. It owns the normal Inspector controls and Editor-driven debug playback without adding a serialized preview component or a second animation state machine.

The store should therefore converge the interactive grid onto the same provider runtime path already proven by capture, while keeping capture-specific checkpoints and cameras independent from ordinary inspection.

## Goals / Non-Goals

**Goals:**

- Make the canonical interactive preview grid exist only as a Play Mode realization of current baked Hero.Voxel content.
- Reuse provider VoxelRenderWorld, VoxelActorInstance, canonical model tracks, bounds, and runtime Inspector directly.
- Keep hierarchy entries as plain transient selection hosts with no voxel-store preview/player component.
- Let body and each equipment model remain independently selectable and inspectable through their own canonical tracks.
- Preserve deterministic store-owned grid ordering, equipment selection, layout, bake paths, and content proof/capture rules.
- Keep the preview repository Editor-only and free of reusable voxel runtime ownership.

**Non-Goals:**

- Do not redesign the Hero.Voxel provider runtime or inspection API.
- Do not replace or modify Hero.Voxel Scene Debug; it remains a direct baked-asset/Edit Mode provider capability.
- Do not introduce a voxel-store runtime assembly, Actor wrapper, presenter wrapper, animation controller, clip inventory, or playback state machine.
- Do not change asset source, animation content, sockets, identity, publication, compatibility, or game semantics.
- Do not make the interactive grid responsible for capture checkpoint semantics or graphical attestation.
- Do not build a Player or APK.

## Decisions

### 1. The canonical interactive grid is a Play Mode runtime session

Voxel Store/Refresh Preview Grid remains the human entry point. In Edit Mode it loads the current preview plan, bakes all selected units through the existing provider bake path, opens the existing disposable VoxelStorePreview.unity shell, records a transient Editor-session request to realize the grid, and enters Play Mode.

After Play Mode is entered, Editor orchestration consumes the pending request and creates the runtime grid. The pending request is Editor-session state only and contains no runtime playback or composition authority.

The saved scene remains a shell. Runtime world, instances, actors, selection hosts, inspection registrations, camera/light if needed, and grid realization are never serialized as current store authority.

Alternative considered: keep the grid in Edit Mode and only use the provider runtime Inspector when a separate preview is launched. Rejected because it preserves two interactive realization paths and makes Scene Debug the de facto grid runtime.

### 2. One provider render world owns all realized grid content

The Play Mode grid creates one provider-owned VoxelRenderWorld under a transient runtime root. For each baked unit:

- avatar entries spawn one root instance and create one VoxelActorInstance;
- deterministic carried equipment uses the existing store selection policy and actor attachment to the authored avatar socket and equipment Mount;
- standalone equipment entries spawn one direct VoxelRenderInstance and one canonical VoxelModelAnimationTrack;
- provider runtime bounds feed the existing PreviewGridLayout;
- world or Actor transforms place entries, with no hierarchy correction becoming model authority;
- VoxelRenderWorld.SubmitNow performs normal provider submission.

Voxel-store owns only which assets are selected, which authored socket names are requested, and where review entries are laid out.

Alternative considered: create one render world per grid entry. Rejected because the grid is one review surface and Hero.Voxel already supports multiple instances in one world.

### 3. Plain hierarchy objects are selection handles, not model owners

Each visible review item has a plain transient GameObject for hierarchy selection. The host carries no voxel-store preview/player MonoBehaviour.

Registration rules:

- avatar root host maps to the root instance, actor root track, and owning actor;
- each carried-equipment child host maps to its attachment instance, attachment track, and the same owning actor;
- standalone-equipment host maps to its instance and canonical model track with no actor.

The host name and parentage may describe store identity for review UX, but transform/component state is not runtime model authority. VoxelRuntimeInspectionController owns Inspector association.

This lets developers inspect body and equipment independently using the exact provider track that capture and runtime already use.

### 4. The grid does not own a second update loop

Voxel-store does not add an Update method, Editor timer, preview player, or mirrored selected clip/phase/loop/speed state. Hero Studio's Editor inspection layer advances only registered playing tracks and refreshes the supplied Actor.

Grid creation may choose an initial authored clip/phase only when the store explicitly needs a deterministic initial review pose. Any subsequent interactive clip selection, play, or scrub comes from the provider Inspector.

### 5. Scene Debug remains provider tooling, but is no longer the canonical grid realization

The existing direct provider test that proves a current baked model can still be opened by Hero.Voxel Scene Debug may remain. The interactive grid itself no longer calls VoxelSceneDebugBuilder.Create, VoxelSceneDebugController.Register, or performs manual GameObject socket alignment.

Documentation distinguishes direct baked-asset/Edit Mode Scene Debug inspection from the Play Mode canonical all-model runtime grid.

### 6. Provider adoption advances the gitlink without changing semantic lock identity

The store advances Dependencies/hero-studio to published provider commit 393d1ef196d90b094d86750c370173cab6aebde9. It preserves the existing heroStudioRevision and workflowSha256 in toolchain/hero-voxel.lock.yaml because those values identify the semantic Hero.Voxel Tool/workflow contract used by compatibility evidence, not every source-control-only Editor change.

The target provider's canonical voxel-art-workflow.md hash is verified byte-identical to the current lock. The repository already demonstrates this distinction: the pre-change Hero Studio gitlink is newer than heroStudioRevision while current compatibility evidence remains valid. Therefore this Editor-only adoption must not rewrite the semantic lock identity or regenerate compatibility evidence.

Provider bootstrap and Tool validation remain version-control independent at execution time; this repository change is the explicit dependency adoption event.

### 7. Existing capture proof remains separate and becomes the runtime-reference implementation

AvatarMotionCaptureRunner already proves the desired provider runtime ownership. The interactive grid should reuse the same composition concepts but not share capture-specific checkpoint state.

avatar-motion-gallery remains the canonical graphical and attested content proof. Interactive-grid validation proves Play Mode realization uses current baked assets, all planned entries exist, carried equipment count matches policy, runtime bounds are finite and non-overlapping after layout, render submission contains parts, selection hosts contain no voxel-store preview/player component, and provider inspection registrations exist for expected root/equipment tracks.

## Risks / Trade-offs

- **Play Mode transition is asynchronous.** The menu action can no longer return a fully realized grid synchronously in Edit Mode. Use a small Editor-session launch marker and a deterministic EnteredPlayMode realization hook; tests cover transition and cleanup.
- **Domain reload loses ordinary static state.** Persist only the launch intent in Unity SessionState; reload actual plan and baked assets from authoritative current files after entering Play Mode.
- **Selection hosts are not render objects.** Treat hosts strictly as selection/identity UX; runtime world and Actor transforms remain authority and layout code updates those directly.
- **Standalone equipment lacks an Actor.** Create the provider model track directly over its render instance and register that pair without an Actor.
- **Old SceneView batch proof becomes invalid.** Replace it with Play Mode/runtime assertions; keep graphical content judgment in the existing Game View capture flow.
- **Provider revision adoption can stale semantic evidence if workflow bytes changed.** Compute the target workflow hash before changing the lock; regenerate semantic evidence only when the workflow fingerprint actually changes.

## Migration Plan

1. Advance the Hero Studio submodule to the published Play Mode-inspection revision and verify the target provider workflow fingerprint.
2. Preserve the existing semantic lock revision and workflow SHA when the target Tool/workflow contract is unchanged.
3. Add focused tests for Play Mode-only grid realization and provider inspection registration before removing the old Scene Debug grid path.
4. Refactor interactive grid orchestration into Edit Mode launch plus EnteredPlayMode runtime realization.
5. Replace Scene Debug avatar/equipment composition with VoxelRenderWorld, VoxelActorInstance, and canonical tracks.
6. Remove obsolete manual socket-transform and SceneView-grid assumptions from the canonical grid path.
7. Preserve the separate direct Scene Debug provider regression and the existing avatar-motion-gallery capture proof.
8. Update WORKFLOW.md, run Preview Editor/Play Mode tests, Tool tests and verify, the graphical capture regression, strict OpenSpec validation, and git diff --check.

Rollback is a normal repository revert restoring the previous Hero Studio gitlink and lock revision plus the Edit Mode grid implementation. No asset-authority migration is involved.