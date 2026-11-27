# Design

## Context

See proposal.md for motivation.

Voxel-store is source/evidence authority for reusable voxel assets, while Hero Studio/Hero.Voxel is the runtime/tool provider. The Unity workspace is Editor-only proof infrastructure. Today the repository is pinned to Hero Studio `2d7e31e641bbb4507bd02e390dafdf06abd3a6be`; its preview directly owns `VoxelActorInstance` composition, a local reference-influence resolver, streamed handle-to-resolver state, and low-level actor-topology proof. The published Hero Studio revision `73fa8a8697dec95c1d38dc73bd192aacc52079b1` adds product-neutral direct and streamed presentation modules that own exactly those generic concerns.

The preview still legitimately owns consumer semantics: `Body`, side-aware `Equip.Hand.*` socket choice, equipment `Mount`, semantic motion/variant candidates, capture phase choreography, gallery layout, and visual acceptance. Those semantics must not move into Hero Studio.

A second issue is workflow ownership. `voxel-store-create-model`, `voxel-store-maintain`, and WORKFLOW.md each describe overlapping stage sequences. The new pipeline must coordinate existing authorities without becoming another validation implementation.

## Goals / Non-Goals

**Goals:**
- Pin voxel-store to the exact Hero Studio presentation-capable revision and keep dependency lock/fingerprint coherent.
- Remove voxel-store-owned generic attachment reference-influence composition and streamed presentation state.
- Preserve all observable direct/streamed preview behavior through provider presentation APIs.
- Keep one canonical provider model track per realized model in composed preview.
- Preserve or strengthen visual/lifecycle evidence while dropping proof that depends on lower actor implementation details.
- Establish one thin, reusable asset-authoring pipeline skill and make create/maintenance entrypoints delegate to it.

**Non-Goals:**
- No production game runtime or consumer gameplay API is added to voxel-store.
- No asset geometry, motion vocabulary, balance, equipment behavior, or publication rules are redesigned.
- No Hero Studio provider changes are part of this change; voxel-store consumes the published provider contract as-is.
- No Player/APK build is required.
- The pipeline does not duplicate Tool, OpenSpec, Product Target, Hero.Voxel workflow, preview, compatibility, sealing, or publication validity rules.

## Decisions

### 1. Pin the exact published provider revision

Advance `Dependencies/hero-studio` to:

`73fa8a8697dec95c1d38dc73bd192aacc52079b1`

Then update `toolchain/hero-voxel.lock.yaml` to that exact revision and recompute the SHA-256 of the pinned `src/client/unity/docs/voxel-art-workflow.md`.

Why:
- The migration depends on `hero.voxel.presentation` and `hero.voxel.presentation.streaming`.
- Pinning the known published commit avoids silently absorbing unrelated future Hero Studio main changes.

Alternative considered:
- Track provider main/latest during apply. Rejected because voxel-store's contract requires exact reproducible dependency authority.

### 2. Direct avatar composition uses provider general presentation

`AvatarPresentation` remains the preview semantic owner, but its generic composition owner becomes one provider `VoxelPresentation` around the caller-owned Body render instance.

For each equipment instance:
- voxel-store still chooses its socket name and the `Mount` anchor;
- provider presentation creates and owns the high-level attachment handle and generic release/reference-influence visual state;
- `EquipmentPresentation` stores only the high-level presentation handle plus its preview semantic fields;
- the local `AttachmentReferenceInfluenceResolver` is deleted.

Root-only avatars still use the same high-level presentation surface. Provider `VoxelPresentation` remains root-only internally until an attachment is added, so a zero-equipment avatar does not acquire actor topology.

Why:
- This aligns preview with the reusable provider contract while retaining voxel-store semantics.
- It removes the exact duplicated resolver that prompted the extraction.

Alternative considered:
- Keep direct avatar preview on low-level `VoxelActorInstance` and migrate only streamed proof. Rejected because it would leave two generic presentation implementations with subtly different reset/recovery behavior.

### 3. Composed preview has one canonical provider track per model

The new provider presentation creates the root and attachment `VoxelModelAnimationTrack` objects that own playback state. Existing `VoxelModelMotionPlayer` must drive those exact track instances.

`VoxelRenderPresentation` therefore remains a preview-specific scene/lifecycle bridge, but gains a narrow internal way to adopt/clear an externally supplied provider track for a caller-owned render instance. In a composed avatar:
- Body adopts `VoxelPresentation.RootTrack`;
- each equipment wrapper adopts `VoxelPresentation.GetAttachmentTrack(handle)`;
- `VoxelModelMotionPlayer` continues to resolve semantic clip candidates through `VoxelRenderPresentation.EnsureAnimationTrack()`, which now returns the adopted provider track;
- releasing/reinitializing composition clears the adopted track before disposing the caller-owned render instance.

Standalone preview models may continue to create their own single-model track through `VoxelRenderPresentation`.

Why:
- Creating a second track wrapper for the same render instance would split selected clip/phase/referenceInfluence state between the motion player and presentation owner.
- Reusing the provider-owned track preserves current motion-player UX without coupling `VoxelModelMotionPlayer` directly to avatar composition.

Alternative considered:
- Teach `VoxelModelMotionPlayer` to locate tracks through `AvatarPresentation`. Rejected because that would couple a general preview motion component to one composition shape and make standalone models special cases.

### 4. Direct update order is semantic sampling, then provider refresh

For deterministic phase sampling:
1. update/sync the Body root world transform;
2. sample the Body provider track;
3. sample each attachment provider track independently;
4. call provider presentation refresh once so generic attachment reference-influence composition uses the current root/socket pose and current attachment samples;
5. submit render state.

Reset uses provider presentation reset so playback and retained generic release-basis state are cleared together.

Bounds for composed avatars come from provider presentation. Single-instance bounds in `VoxelRenderPresentation` delegate to provider `VoxelRenderInstance.GetWorldBounds()`; the preview-local duplicate bounds transform implementation is removed.

### 5. Streamed proof uses the high-level streamed presentation facade

The proof still constructs:
- one `VoxelStreamingService`,
- one `VoxelPresenterPool`,
- one zero-fixed-attachment `VoxelActorPresenterPool`.

It then acquires `VoxelStreamedPresentation.AcquireCompositeAsync(...)`, not a raw `VoxelActorPresenter`, because the proof is explicitly runtime-attachment-capable even while its current attachment count is zero.

The proof uses high-level activation-scoped attachment handles for attach/rebind/detach/replace and gets model tracks through the presentation facade. The store-owned resolver dictionary is removed.

Why:
- The existing pools remain singular resource/topology authorities.
- The high-level facade supplies the reusable active presentation lifecycle and stale-handle/reset behavior without introducing another pool.

Alternative considered:
- Acquire simple root-only presentation first and upgrade to composite on first attach. Rejected because the provider contract intentionally selects the composite-capable path up front when runtime mutation is part of the presentation capability.

### 6. Streamed evidence moves from hidden topology internals to observable presentation behavior

The current proof asserts actor `TopologyRevision`, raw attachment-reference matrices, and local resolver state. Those are lower-layer implementation details intentionally hidden by the high-level provider.

The migrated proof instead verifies:
- attachment count and current/stale high-level handle membership;
- independent root/unaffected attachment track identity and normalized phase;
- replacement preserves high-level handle but supplies a fresh replacement track with reset playback;
- release/reacquire invalidates prior activation handles and clears runtime-added attachments/state;
- all exposed model world transforms are finite;
- a released attachment remains at its retained visual position while the root moves;
- ordered recovery samples move continuously away from the retained release position toward the final rejoined position;
- provider-attested Game View captures remain visually inspectable at every existing checkpoint.

Exact socket/reference math remains provider-owned and is covered by Hero Studio's own presentation tests. Voxel-store verifies the observable consumer result rather than re-exposing private actor reference state.

### 7. Pipeline skill is a coordinator, not a rule owner

Add `.agents/skills/voxel-store-pipeline/SKILL.md` as the canonical one-asset pipeline:

`Intent -> Product Target -> Construction -> Density -> Source -> Generate -> Runtime Presentation Proof -> Compatibility -> Seal -> Publish`

The skill contains:
- one-asset ownership;
- ordered stage navigation;
- pointers to the owning authority for each stage;
- explicit applicability handling for Density and Runtime Presentation Proof;
- deterministic re-validation after stage completion;
- fail-closed behavior.

It does not restate detailed Product Target acceptance, construction semantics, VPU rules, source schema, proof acceptance, compatibility equations, or publication criteria.

`voxel-store-create-model` becomes a compatibility entrypoint:
- validate that the caller requests one new production asset;
- establish the requested form/intent;
- delegate to `voxel-store-pipeline` from Intent.

`voxel-store-maintain` remains responsible for deterministic scan/plan selection:
- select exactly one job/current gate;
- delegate that job to the pipeline at the corresponding stage;
- re-run deterministic state after stage progress.

`voxel-store-product-target` remains unchanged as the specialized Product Target authority.

WORKFLOW.md presents the same pipeline and removes the competing create-model stage definition.

### 8. Evidence and validation remain proportional

Apply must update:
- runtime asmdef references to include provider presentation assemblies and remove direct actor dependencies that are no longer needed;
- direct Avatar Editor/PlayMode tests to assert high-level attachment identity, canonical track sharing, root-only behavior, independent playback, reset/reinitialize, bounds, and release/recovery;
- mutable streamed capture/runtime proof to the high-level facade and observable checkpoints;
- runtime-gallery documentation to describe provider presentation ownership;
- compatibility evidence/provider lock metadata that fingerprints Hero Studio revision/workflow;
- skill/workflow tests or repository checks that validate skill entrypoints and the canonical pipeline.

Verification should include:
- store Tool tests/verify and provider lock checks;
- focused Unity Editor tests for preview runtime;
- PlayMode runtime-gallery tests;
- the two registered graphical capture scenarios when their current evidence contract requires them;
- no Player/APK build.

## Risks / Trade-offs

- **[Track ownership split]** A preview model could accidentally retain a locally created track while provider presentation uses a different track. → Add explicit track adoption/clearing and tests that the motion player observes the presentation-owned track instance.
- **[Proof appears weaker after removing topology revision/reference matrix]** The old proof exposed lower actor internals. → Replace them with higher-level membership, stale-handle, track identity/playback, finite-transform, release/recovery trajectory, and provider-attested visual evidence; rely on provider tests for hidden reference math.
- **[Provider pin changes other package behavior]** The new Hero Studio commit contains the presentation modules plus any earlier commits in its history. → Pin the exact known commit, run provider bootstrap/lock validation, full store verification, and focused Unity regressions before publication.
- **[Preview wrapper gains external-track mode]** Lifecycle mistakes could leave a stale adopted track after provider disposal. → Clear adopted tracks before render-instance disposal and prove release/reinitialize behavior.
- **[Pipeline duplicates validation over time]** A large orchestration skill could drift from Tool/spec authority. → Keep the pipeline as stage routing plus authority pointers only; detailed validity remains in existing owners.
- **[Runtime presentation proof becomes an accidental universal gate]** Not every asset contract requires the same Unity capture. → Evaluate the stage for every job but allow explicit not-applicable only when the owning proof/compatibility contract permits it.

## Migration Plan

1. Advance the Hero Studio submodule to the exact presentation-capable revision and update/verify the provider lock fingerprint.
2. Add provider presentation assembly references to the preview runtime and introduce the narrow external-track adoption bridge.
3. Migrate direct avatar/equipment composition to provider general presentation and delete the local resolver.
4. Migrate mutable streamed proof/capture to the high-level streamed presentation facade and observable presentation evidence.
5. Update focused Editor/PlayMode tests and runtime-gallery documentation; remove obsolete low-level assertions.
6. Add the canonical pipeline skill, reduce create-model to a compatibility entrypoint, update maintenance delegation, and align WORKFLOW.md.
7. Run store/provider validation, focused Unity tests, required captures, hygiene checks, and OpenSpec validation.
8. Archive, commit, rebase normally if upstream advanced, reverify as required, and push only verified task-owned changes.

Rollback is a normal Git revert of this coherent change, including the provider submodule pin and lock. No persistent data migration is involved.
