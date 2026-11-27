# Tasks

## 1. Establish the runtime-free preview boundary

- [x] 1.1 Add/update focused Editor tests or static boundary assertions that fail while `VoxelStore.Preview.Runtime`, store-owned runtime wrappers, duplicated motion/model-frame helpers, or runtime asmdef references remain; verify the new boundary checks fail on the pre-migration shape and pass after the targeted removals.
- [x] 1.2 Refactor preview assembly references so Editor, Capture.Editor, and Editor.Tests depend directly on the pinned Hero Studio assemblies they actually consume and no longer require `VoxelStore.Preview.Runtime`; verify Unity recompiles the affected assemblies without introducing a replacement runtime facade.
- [x] 1.3 Update `WORKFLOW.md` and nearby preview documentation to state that voxel-store owns Editor proof/content acceptance only and Hero Studio owns reusable voxel execution; verify repository stale-text checks no longer describe voxel-store as owning runtime playback, Actor lifecycle, or streaming/pooling behavior.

## 2. Convert gallery and interactive inspection to Editor-only orchestration

- [x] 2.1 Refactor catalog discovery/gallery construction so it keeps publication filtering, deterministic catalog/model ordering, grid layout, stale cleanup, and derived-scene rebuild while creating no `AvatarPresentation`, `EquipmentPresentation`, `VoxelRenderPresentation`, `VoxelModelMotionPlayer`, or mutable streamed proof components; verify focused gallery Editor tests still cover current baked discovery, deterministic layout, and stale removal.
- [x] 2.2 Remove or rename `RuntimeGallery`-specific code/scene terminology where it implies a voxel-store runtime contract, while preserving the same store review entry point and deterministic generated preview behavior; verify refresh produces a valid derived preview scene with all currently published units and no missing-script components.
- [x] 2.3 Remove the store-owned motion player Inspector/window/preferences and route single-model interactive inspection to the current Hero.Voxel animation asset Inspector/Scene Debug workflow; verify a baked model with multiple authored clips exposes those exact clips through provider Editor tooling without a voxel-store motion list.

## 3. Replace avatar/equipment wrappers with direct provider composition proof

- [x] 3.1 Refactor the representative avatar/equipment proof path to construct caller-owned provider render instances and one direct Hero.Voxel Actor from current baked assets, with store code supplying only selected assets, socket/`Mount`, exact clip names, phases, camera/layout, and acceptance checkpoints; verify no new generic wrapper class replaces the deleted runtime presentation classes.
- [x] 3.2 Migrate the avatar/equipment capture runner to sample provider-owned model tracks directly at deterministic phases, refresh provider composition, submit render state, and capture evidence; verify `tools/capture-game-view.ps1 -Scenario avatar-motion-gallery` completes with current provider attestation and preserves the required mixed-motion, release/return, variant, and Death content checkpoints.
- [x] 3.3 Rewrite the former `AvatarPresentationTests`/related PlayMode assertions into store-integration tests for baked part/socket/`Mount` validity, exact authored clip availability, representative direct Actor composition, finite bounds/transforms, and capture inputs; verify the tests no longer assert provider track lifecycle, wrapper identity, or internal Actor mechanics already covered upstream.

## 4. Remove streamed/provider-conformance proof from voxel-store

- [x] 4.1 Delete `MutableStreamedActorPreview` and its store-owned in-memory asset-provider/resource-handle helpers, unregister `mutable-streamed-actor-gallery`, and remove the corresponding capture runner; verify repository search finds no voxel-store implementation of `IVoxelAssetProvider`, streamed presenter pool orchestration, activation-handle bookkeeping, or warm-reacquire proof code.
- [x] 4.2 Remove tests/spec-facing checks whose only purpose is to validate attach/detach/rebind/replace transactionality, attachment-handle staleness, pool reset, warm reuse, or direct-vs-streamed equivalence; preserve any store-content assertion by moving it to the direct composition proof, and verify focused Editor tests still cover store-specific asset acceptance.
- [x] 4.3 Update capture registry/scripts and documentation so only store-content graphical scenarios remain; verify the removed mutable-streamed scenario is no longer discoverable while the retained avatar/content capture commands still run from the repository workflow.

## 5. Delete the store runtime layer and duplicate conventions

- [x] 5.1 Remove `Preview/Unity/Assets/Preview/Runtime/**`, including `VoxelStore.Preview.Runtime.asmdef`, `VoxelRenderPresentation`, `AvatarPresentation`, `EquipmentPresentation`, `VoxelModelMotionPlayer`, `MutableStreamedActorPreview`, `AvatarMotion`, `AvatarMotionClipCandidates`, and `VoxelStoreModelFrame`; verify the directory/runtime asmdef is absent and Unity has no missing compile references to those types.
- [x] 5.2 Remove obsolete custom editors, generated-scene restoration code, PlayMode fixtures, and asmdef references that existed only for the deleted runtime components; if a file contains independent store-content validation, migrate that assertion before deletion and verify the surviving Editor test assemblies have non-zero selected test counts and zero task-owned failures.
- [x] 5.3 Run an active-code stale-reference scan for `VoxelStore.Preview.Runtime` and all removed wrapper/helper type names, excluding archived OpenSpec history, and verify zero active matches remain; also verify no new voxel-store runtime/model/composition/playback/streaming abstraction has been introduced under another name.

## 6. Final integration and boundary verification

- [x] 6.1 Run `dotnet run --project Tool/VoxelStore.Tool.Tests` and `dotnet run --project Tool/VoxelStore.Tool -- verify`; verify publication/compatibility state remains healthy and authored store assets/identity are unchanged by the preview cleanup.
- [x] 6.2 Run proportional Unity Editor tests for preview discovery, baked validation, direct provider composition, and capture integration, plus the retained graphical avatar/content scenario; verify all task-owned suites pass and provider-attested captures remain visually valid without requiring a Player/APK build.
- [x] 6.3 Run strict OpenSpec validation, `git diff --check`, task-owned scope review, and a direct Git diff against `Dependencies/hero-studio`; verify the change is valid, has no whitespace errors, leaves the Hero Studio submodule/source untouched, and changes no authored voxel content except derived preview/evidence that must be regenerated.
- [x] 6.4 Verify the final Unity preview exposes no voxel-store runtime assembly/API, still lets developers inspect current baked animation assets through Hero.Voxel Editor tooling, still builds the current published review gallery, and still produces canonical store-content graphical evidence.

## Workflow follow-up

- Archive the change after implementation and review requirements are satisfied.
- Verify the archived `unity-baked-preview` specification reflects the runtime-free Editor proof boundary.
