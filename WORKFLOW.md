# Voxel Store Workflow

## Bootstrap

1. Read `AGENTS.md`.
2. Initialize the repository-owned Hero Studio provider with `git submodule update --init --recursive`; `Dependencies/hero-studio` plus `toolchain/hero-voxel.lock.yaml` is the single Hero.Voxel dependency authority.
3. Use OpenSpec for architecture, ownership, schema, workflow, or other cross-cutting changes.
4. Start maintenance from deterministic Tool state; do not choose work from generated catalog/status files by hand.

Repository domain roots are explicit: `Assets` is reusable product content; `Styles` is visual authority; `Collections` is desired coverage; `Store` is global policy/history; `Catalog` is derived state. Reusable avatar/equipment compatibility is defined by the global rigid-Body composition contract rather than a separate profile domain.

## Create a new model

Use `.agents/skills/voxel-store-create-model/SKILL.md` for production creation requests such as `design and create an axe` or `create voxel model`.

```text
COLLECTION / USER INTENT
  -> DISTINCT ASSET IDENTITY
  -> SHARED STYLE GRAMMAR
  -> ACQUIRE / ACCEPT PRODUCT TARGET
  -> COMPOSITION / PARTS
  -> DENSITY
  -> VOXELSPEC + PALETTE
  -> PROOF / ITERATE
  -> COMPATIBILITY
  -> PUBLISH
```

Creation has no template-asset or generator/provider dependency. `Styles/**` supplies shared grammar; the accepted Product Target supplies asset-specific visual authority. If Product Target authority cannot be established, production stops before construction/source; there is no alternate authoring lane.

## Lifecycle

The maintenance workflow has one primary path:

```text
PLAN
  -> REFERENCE
  -> SOURCE
  -> PROOF
  -> PUBLISH
```

These are navigation stages only. Existing Tool gates, OpenSpec contracts, policy, and Hero.Voxel rules remain authoritative.

### 1. Plan

Run deterministic scan/plan state and claim only the first asset-scoped job plus its earliest unresolved/current gate.

- If no job exists, the store is healthy: stop.
- Never skip an earlier unresolved gate to work on a later stage.
- One maintenance invocation owns only that selected asset.

### 2. Reference

Reference work establishes accepted visual/construction authority for the current asset.

- Product Target acquisition, visual acceptance, freezing, and provenance handoff are owned by `.agents/skills/voxel-store-product-target/SKILL.md`; no particular generator/provider is required.
- End-to-end new production model creation is owned by `.agents/skills/voxel-store-create-model/SKILL.md`.
- Style and target fidelity are owned by `openspec/specs/reference-style-authority/spec.md` and `openspec/specs/reference-target-fidelity/spec.md`.
- Composition/Part construction semantics are owned by the pinned Hero.Voxel `voxel-art-workflow.md`.
- New or intentionally redesigned assets use the target-driven flow selected by current contracts.
- Compatibility exception: an unchanged legacy v2 repair may preserve its existing reference flow when the Tool selects that work; do not migrate it merely to follow the primary diagram.

### 3. Source

Source work turns accepted reference authority into implementation source.

- If the current asset requires reference-driven density, run the density gate first; `openspec/specs/reference-density-selection/spec.md` and `Store/vpu-ruler.yaml` own that behavior.
- Author/repair VoxelSpec only after upstream gates pass.
- Hero.Voxel owns VoxelSpec parsing/compilation and canonical voxel-authoring semantics.

#### Rigid avatar and equipment composition

Published reusable avatars use exactly one authored rigid part named `Body`.

- Do not author `Head`, `Arm`, `Hand`, `Foot`, socket, mount, or authored combat-animation parts for reusable avatar compatibility.
- `Body` local bounds after VPU normalization are the canonical equipment-placement space.
- Every published reusable model uses one semantic canonical Unity local frame: Right `+X`, Up `+Y`, Forward `-Z`. This facing contract is independent of Hero.Voxel source-coordinate conversion. Forward is the outward normal of semantic FRONT: avatar face/gaze/chest for avatars, and intended game-facing/action direction for equipment when mounted on a forward-facing avatar. Projectile equipment uses launch/target direction; directional melee equipment uses its intended cut/thrust/strike direction. Do not infer Forward from the longest dimension, a broad presentation face, an arbitrary mesh axis, or a coordinate-conversion label alone. Correct source geometry/pivot asset-by-asset before publication; consumers must not store corrective placement rotation.
- A normalized source-space ratio `r = (x, y, z)`, with every component in `[-1,+1]`, resolves as `bodyBounds.center + bodyBounds.extents * r`. Unity presentation maps the Hero.Voxel source axes through the provider's canonical source-to-Unity basis.
- Multiple equipment instances are an ordered consumer presentation list of independent `{ equipmentId, offsetRatio }` values. Adding another equipment instance does not change avatar source.
- Primary/secondary, left/right, weapon class, inventory occupancy, damage, and attack behavior are consumer gameplay semantics and are not voxel-store metadata.

### 4. Proof

Generate deterministic output and proof for the current source, visually inspect the required evidence, and seal only accepted proof. `front.png` is semantic-frame evidence: its camera stands on Unity `-Z` and looks toward `+Z`, so a conforming model's outward semantic Forward points toward the viewer at Unity `-Z`.
The generated Hero.Voxel `Proof/.../front.png` owns this semantic-frame proof. A `FRONT` label embedded in `Reference/Parts/*.png` is reference/product-target evidence and is not, by itself, model-frame authority; for ranged equipment it may intentionally show a broad side view while the generated semantic front is muzzle/target-facing.

Compilation is not visual acceptance. If proof fails, repair the same current asset/gate.

### 5. Publish

Re-scan and verify derived state after the asset passes its upstream gates. Compatibility, budget, derived catalog, selected-collection coverage, and publication remain Tool/spec/domain-contract-owned.

Commit only verified repository-owned work. Push only when a real upstream remote and usable authentication exist.

## Gate loop

After every gate:

```text
PASS    -> re-derive Tool state for the same asset and follow its next gate
FAIL    -> repair the same gate on the same asset
BLOCKED -> report the blocking authority/evidence/dependency and stop
HEALTHY -> stop
```

Do not maintain a second manual stage tracker beside deterministic Tool state.

## Common commands

From the repository root:

```text
dotnet run --project Tool/VoxelStore.Tool -- plan
dotnet run --project Tool/VoxelStore.Tool -- scan --write
dotnet run --project Tool/VoxelStore.Tool -- verify
dotnet run --project Tool/VoxelStore.Tool -- density <asset-id> [--write]
dotnet run --project Tool/VoxelStore.Tool -- generate <asset-id>
dotnet run --project Tool/VoxelStore.Tool -- seal-reference <asset-id>
dotnet run --project Tool/VoxelStore.Tool -- seal <asset-id>
```

Current Tool builds resolve Hero.Voxel directly from `Dependencies/hero-studio`; no `HERO_STUDIO_ROOT` or developer-local provider checkout is required.

## Unity baked preview
The Unity workspace under `Preview/Unity/` is an Editor-only inspection surface for real baked Hero.Voxel outputs. It is not voxel-store source authority, publication authority, or a production runtime.

From the repository root:

```powershell
git submodule update --init --recursive
./Preview/Bootstrap.ps1
```

The bootstrap initializes and verifies the exact Hero Studio submodule revision and Hero.Voxel workflow fingerprint, then exposes the **entire** `hero.studio` package through one ignored embedded-package junction at `Preview/Unity/Packages/hero.studio`. The junction points to `Dependencies/hero-studio/src/client/unity/Packages/hero.studio`; it does not select modules, copy provider code, or patch provider-owned shaders.
Open `Preview/Unity/` with Unity `6000.3.24f1`.

The visible `Voxel Store` menu intentionally exposes only the current manual workflow:

- `Voxel Store/Refresh Runtime Gallery Scene`
- `Voxel Store/Avatar Motion Preview`

Legacy preview-grid refresh and deterministic motion capture remain internal/batch helpers and are not exposed as normal menu children.

### Runtime gallery sample

The primary manual evaluation surface is the generated Play Mode gallery:

`Voxel Store/Refresh Runtime Gallery Scene`

The command does **not** bake or regenerate model sources. It discovers human-imported baked `VoxelAssemblyAsset` models under `Preview/Unity/Assets/__VoxelStorePreview/Baked`, rebuilds the ignored local scene `Assets/Preview/VoxelStoreRuntimeGallery.generated.unity`, opens it, and frames all discovered models in the Game camera.

Runtime catalog identity comes from folder structure rather than provider `Kind` metadata. Any imported model matching `.../<Catalog>/<Model>/Source/<Model>.asset` is discovered; the folder immediately above `<Model>` is the catalog name. Prefix folders before `<Catalog>` are irrelevant, so a human may import a complete model package/tree without editing runtime-gallery code. `Avatars` receives avatar presentation/motion behavior, `Equipment` is attachable and also shown standalone, and any other catalog is shown as a general standalone model.

The runtime gallery realizes the complete baked part hierarchy and does not require a one-part/socketless asset. Sockets are not needed for gallery visualization. Human-imported catalog folders are not stale preview-bake output and cleanup must preserve them.

Avatar entries use the preview-only presentation hierarchy:

```text
Avatar | ...
├─ AvatarPresentation
├─ AvatarMotionPlayer
└─ PresentationRoot
   ├─ Body
   ├─ Equipment | ...
   │  └─ MotionRoot
   │     └─ Model
   └─ Equipment | ...
      └─ MotionRoot
         └─ Model
```

`AvatarPresentation` owns Body bounds, semantic canonical orientation, and Body-ratio equipment composition. Canonical directions are local Right `+X`, Up `+Y`, and Forward `-Z`; the Forward value describes semantic facing, not Hero.Voxel coordinate conversion. `VoxelStoreModelFrame` is the preview-owned single implementation point for that semantic basis and for the pinned Hero.Voxel source-vector mapping `(x,y,z) -> (x,z,y)`, so the preview does not depend on unpublished or dirty provider code.

Each carried `EquipmentPresentation` separates stable placement from temporary animation. Equipment already shares the avatar canonical model frame (`+X` Right, `+Y` Up, `-Z` Forward), so its stable placement object keeps identity rotation at the normalized Body-bounds ratio. Only the child `MotionRoot` is animated.

`AvatarMotionPlayer` owns semantic DOTween presentation for both the avatar `PresentationRoot` and every equipment `MotionRoot`. The supported general motions are:

- `Idle`
- `WalkForward` — discrete Crossy Road-style launch/apex/landing hop presentation
- `Attack` — backward anticipation, fast semantic-Forward drive, weapon motion, follow-through, recovery
- `Hit` — opposite-Forward recoil plus equipment inertia
- `Death` — terminal Body fall plus coordinated equipment follow-through

The motion player does not own combat results, damage timing, AI, or production locomotion. `UnitRoot` remains authoritative; ordinary motion changes only presentation-owned transforms.

Canonical animation proof is command-driven from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File tools\capture-game-view.ps1 -Scenario avatar-motion-gallery
```

The command bootstraps/verifies the pinned Hero Studio package, resolves `Preview/Unity/Assets/Preview/Capture/CaptureRegistry.json`, launches one owned **graphical** Unity Editor, invokes the registered `Capture.Editor` runner internally, survives Play Mode/domain reload through `SessionState`, samples deterministic `WalkForward`, `Attack`, `Hit`, and `Death` checkpoints through `AvatarMotionPlayer`, and captures composed runtime pixels through `Hero.Consumer.Editor.GameViewCapture`. Accepted output lives under `Preview/Unity/Library/Capture/AvatarMotionGallery/` and must include current `game-view-capture.json` provider attestation. The command reports scenario, output directory, attestation path, PNG count, Unity log, and `VOXEL_STORE_GAME_VIEW_RESULT=CAPTURE_ATTESTED`.

`CAPTURE_ATTESTED` proves only that current graphical Game View pixels were acquired correctly. Inspect the reported PNGs to judge animation semantics: Walk must read as a hop, Attack must drive toward semantic Forward while equipment Forward stays aligned, Hit must recoil opposite Forward, and Death must read as terminal follow-through.

`Voxel Store/Avatar Motion Preview` remains optional interactive exploration. It can select Motion, Loop, Speed, and target scope (`All Avatars` or `Selected Avatar`) and uses the same `AvatarMotionPlayer` API, but opening the window, pressing Play/Stop/Reset, selecting hierarchy objects, or editing the Inspector is not a completion gate.

The runtime gallery's direct camera/RenderTexture screenshot remains a diagnostic-only convenience for static framing. It does not count as canonical animation proof because it does not establish provider-attested composed Game View pixels. The former `AvatarMotionGalleryCapture` RenderTexture automation path has been removed.

Generated scenes, materials, captures, and reports remain ignored preview state. Imported baked model catalogs are human-owned gallery inputs and are preserved; refreshing the runtime gallery only recreates the scene/evidence around those imported assets.

## Finalization
- Run the applicable deterministic validation and Tool tests when the pinned provider is available.
- Run strict OpenSpec validation for spec/workflow changes.
- Run `git diff --check`.
- Preserve unrelated dirty work.
