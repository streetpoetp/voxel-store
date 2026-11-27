# Design

## Context

See `proposal.md` for motivation. The current preview project contains a `VoxelStore.Preview.Runtime` assembly with store-owned MonoBehaviours and helpers that wrap Hero Studio render instances, Actor composition, animation tracks, mutable streamed Actor lifecycle, model-frame conventions, and motion candidate selection. Editor builders, capture runners, Editor tests, and PlayMode tests depend on those wrappers.

The pinned Hero Studio already owns the reusable mechanisms the store wrappers coordinate: baked voxel assets, render world/instances, model animation tracks, direct Actor composition, streamed Actor composition/pooling, and generic animation inspection/Scene Debug. Voxel-store's repository contract states that it is not a game project and does not own a voxel runtime.

## Goals / Non-Goals

**Goals:**

- Remove the complete store-owned voxel runtime/model layer rather than preserving thin compatibility wrappers.
- Keep store-specific proof inputs and visual acceptance behavior while driving Hero Studio directly from Editor/capture code.
- Preserve current publication-to-bake validation, store-wide authored clip/socket checks, deterministic visual capture, and representative avatar/equipment composition proof.
- Make tests enforce the store/provider boundary instead of re-testing provider lifecycle mechanics.
- Leave a clear rule for future work: a missing proof convenience does not automatically become a Hero Studio API request.

**Non-Goals:**

- No Hero Studio code, package API, or submodule revision changes.
- No new asset provider, runtime facade, game integration layer, or generic preview framework in voxel-store.
- No migration of voxel-store wrappers into `hero.voxel.workbench` merely to preserve existing shapes.
- No changes to authored voxel assets, animation content, publication identity, or gameplay semantics.
- No APK/Player build requirement.

## Decisions

### 1. Delete the runtime assembly instead of shrinking it

`Preview/Unity/Assets/Preview/Runtime/**` and `VoxelStore.Preview.Runtime.asmdef` are removed as a unit. Store-owned runtime classes are not retained as aliases, adapters, compatibility facades, or serialized components.

This is preferred over keeping "thin" wrappers because even a thin wrapper would continue to establish a second lifecycle/identity surface and would invite future runtime behavior back into voxel-store.

Alternative considered: keep `AvatarPresentation`, `VoxelRenderPresentation`, or `VoxelModelMotionPlayer` as convenience components. Rejected because their only durable value is wrapping provider-owned render/Actor/track state.

### 2. Editor/capture code consumes the lowest appropriate provider capability directly

Store proof orchestration selects baked assets and proof inputs, then directly uses Hero Studio capabilities appropriate to the proof:

```text
single-model inspection
    -> Hero.Voxel Editor preview / Scene Debug

composed avatar/equipment proof
    -> Hero.Voxel.Render
    -> Hero.Voxel.Actor

streaming/pooling proof
    -> not a voxel-store responsibility
```

Composition proof should prefer direct render instances plus `VoxelActorInstance` because the store is proving authored content composition, not production streaming or pooling. This also removes the current need for store-owned in-memory `IVoxelAssetProvider`/resource-handle helpers.

Alternative considered: rebuild proofs on `VoxelGameRuntime` or `VoxelActorPresenterPool`. Rejected as the default because `hero.voxel.game` is a consumer-game convenience layer and streamed pooling is not required to validate store content. A store proof may use a lower provider runtime object when the visual behavior genuinely requires it, but it must not turn provider lifecycle into a store requirement.

### 3. Exact authored clips replace store-owned motion vocabulary/candidate state

Store proof cases keep only exact authored clip names and deterministic phases required for evidence. `AvatarMotion`, `EquipmentSlotSide`, `AvatarMotionClipCandidates`, and duplicated model-frame helpers are removed.

Hero Studio remains responsible for generic animation playback and any common consumer motion-candidate convention. Voxel-store publication/preview validation remains responsible for asserting that required authored clip names exist.

Alternative considered: continue using Hero Studio `VoxelMotionCandidates` from `hero.voxel.game`. Rejected for store proof because candidate fallback is consumer presentation policy; proof should name the authored content it intends to validate exactly.

### 4. Single-model interactive inspection delegates to Hero.Voxel Editor tooling

The store-owned motion-player Inspector/window is removed where its behavior duplicates `VoxelAnimationAssetEditor` and Scene Debug clip selection/play/scrub behavior. The preview workspace may link/select the current baked asset, but it does not own another playback controller.

Store-specific interactive UI may remain only when it selects store proof content or launches store capture/validation; it must not retain runtime animation state.

Alternative considered: retain the window but replace its implementation with provider calls. Rejected unless the window has store-specific selection value independent of playback; otherwise it is redundant UI around the provider Editor.

### 5. The gallery becomes Editor-generated review state, not a runtime object graph

Catalog discovery, deterministic ordering, grid layout, publication filtering, and derived-scene cleanup stay in Editor code. The gallery no longer serializes store runtime components for model identity, animation state, avatar ownership, or mutable Actor proof state.

If the canonical grid only needs baked static inspection, it should use provider Editor/preview realization appropriate for that purpose. Dynamic composed capture creates ephemeral provider runtime state from the selected baked assets and disposes it after the scenario.

The existing `RuntimeGallery` naming should be removed or renamed where it implies a store runtime contract, but renaming is secondary to deleting runtime ownership.

### 6. Capture scenarios own proof semantics only

Capture runners remain Editor-owned orchestration. They may enter Play Mode when required to execute the real provider runtime, but they construct provider objects directly and hold only scenario-local references.

The avatar/equipment capture keeps content-level checkpoints such as:

- selected body/equipment assets;
- selected socket and `Mount` anchor;
- exact authored clips and normalized phases;
- visual release/return trajectory checkpoints where that trajectory is part of authored content acceptance;
- camera/layout and pixel evidence.

The mutable-streamed Actor scenario is removed because attach/detach/rebind/replace, activation handles, warm reacquire, and pooling are provider conformance concerns rather than store content acceptance.

### 7. Tests move from wrapper/provider conformance to store integration

Delete tests whose subject is a removed store runtime class or whose assertions duplicate Hero Studio runtime behavior. Preserve or add focused tests for:

- current published set -> baked asset discovery;
- required part/socket/`Mount` structure;
- required exact animation clip inventory;
- Editor gallery determinism and stale cleanup;
- direct provider composition smoke using representative current store assets;
- deterministic graphical capture and provider attestation;
- absence of a store-owned runtime assembly or runtime-wrapper references.

Provider mechanics such as track completion, attachment-handle staleness, mutation transactionality, pool reset, and warm reuse remain Hero Studio test responsibility.

### 8. Provider insufficiency fails closed; it does not trigger local runtime reimplementation

If direct migration reveals a required provider behavior is unavailable, implementation stops at that boundary and records the concrete gap. The change must not solve it by recreating the missing behavior in voxel-store.

A Hero Studio follow-up is justified only if the missing behavior is independently generic to Hero Studio or multiple consumers. That decision is outside this change.

## Risks / Trade-offs

- **[Risk] Existing generated gallery scenes reference removed MonoBehaviours.** -> Rebuild derived gallery state from current baked assets and remove stale serialized component references as part of migration; generated preview state is disposable.
- **[Risk] Removing the store motion window reduces one convenient interactive workflow.** -> Rely on provider animation Inspector/Scene Debug and keep only store-specific launch/selection UI that adds content-review value.
- **[Risk] Current capture runners assume serialized runtime components exist before Play Mode.** -> Refactor runners to create scenario-local provider render/Actor state from Editor-selected baked assets after entering the required execution mode.
- **[Risk] Some tests currently provide useful regression coverage while also testing provider mechanics.** -> Preserve the store-content assertion and drop the provider-lifecycle assertion; avoid deleting coverage solely by filename.
- **[Risk] Direct provider usage may expose an actual generic provider gap.** -> Fail closed and separate that finding from this change; do not extend scope automatically.
- **[Trade-off] Fewer store abstractions means capture code knows more Hero Studio public types.** -> This is intentional: the dependency boundary becomes explicit and avoids a second runtime architecture.

## Migration Plan

1. Establish boundary tests/spec checks that identify store-owned runtime assemblies/types and runtime-specific proof contracts targeted for removal.
2. Refactor Editor gallery discovery/layout so it no longer creates or restores store runtime components.
3. Refactor single-model inspection to provider Editor tooling and remove the store motion-player UI/state.
4. Refactor avatar/equipment graphical proof to construct direct provider render instances/Actor state from current baked assets and preserve content-level checkpoints.
5. Remove the mutable-streamed Actor proof and provider-conformance assertions from voxel-store.
6. Delete `Preview/Runtime`, runtime-dependent PlayMode fixtures, obsolete Editor custom inspectors/windows, and stale asmdef references.
7. Regenerate derived preview scene/evidence as needed, run store Tool verification plus focused Unity Editor/capture validation, and confirm no Hero Studio files changed.

Rollback is a normal Git revert because preview assets/scenes are derived and no authored store data or publication identity is migrated.
