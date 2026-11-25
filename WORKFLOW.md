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

The command does **not** regenerate voxel source. It first performs a **bake-only** provider refresh for every published model, producing current `VoxelAssemblyAsset` + matching `VoxelAnimationAsset` pairs under `Preview/Unity/Assets/__VoxelStorePreview/Baked`; it then rebuilds the ignored local scene `Assets/Preview/VoxelStoreRuntimeGallery.generated.unity`, opens it, and frames all discovered models in the Game camera. It does not build the legacy `[Voxel Store Preview Grid]`, so manual and automated runtime-gallery refresh use the same derived bake authority.

Preview bake paths mirror the authoritative model directory, never the asset id. Canonical source `Assets/<Catalog>/<Model>/Source/<Model>` bakes to `Assets/__VoxelStorePreview/Baked/<Catalog>/<Model>/<Model>.assembly.asset` plus sibling `<Model>.animation.asset`. The same rule applies to every published model, including `Avatars` and `Equipment`; ids such as `dungeon.avatar.*` or `foundation.equipment.*` never appear in the baked filesystem path.

Runtime catalog identity comes from this mirrored folder structure. `Avatars` receives avatar socket/motion composition, `Equipment` is attachable and also shown standalone, and any future catalog may be handled as a general standalone animated model. Preview-bake cleanup recursively removes obsolete generated model directories, including the legacy id-based layout, while all baked output remains disposable and reconstructable from authoritative voxel-store sources.

The runtime gallery realizes the complete baked part hierarchy and uses Hero.Voxel authored sockets and animation assets directly.

Avatar entries use the preview-only presentation hierarchy:

```text
Avatar | ...
├─ AvatarPresentation
├─ AvatarMotionPlayer
└─ PresentationRoot
   ├─ Body
   │  └─ Body
   ├─ Equipment | ...
   │  └─ Model
   │     └─ <equipment part>
   └─ Equipment | ...
      └─ Model
         └─ <equipment part>
```

Current published avatars have one authored part named `Body`. They do **not** have authored Torso, Shoulder, Arm, or Hand parts. Physical equipment attachment is represented by side-aware Body sockets such as `Equip.Hand.Left.0` and `Equip.Hand.Right.0`. `AvatarPresentation` owns preview composition and resolves those current animated socket frames. Canonical directions remain local Right `+X`, Up `+Y`, and Forward `-Z`; `VoxelStoreModelFrame` remains the preview-owned implementation point for that semantic basis and for the pinned Hero.Voxel source-vector mapping `(x,y,z) -> (x,z,y)`.

Each equipment manifest declares a reusable attachment capability and side policy, while each equipment VoxelSpec owns an exact `Mount` socket. Current published equipment is one-part, so `Mount` is authored at that part's pivot. `EquipmentPresentation` solves its placement from the current animated body socket and the equipment **bind Mount** frame. It does not use Body bounds, normalized ratios, a pair-specific correction, or a temporary MotionRoot.

`VoxelAnimatedAssembly` owns the provider runtime objects for one baked model: `VoxelAssemblyAsset`, `VoxelAnimationAsset`, `VoxelAssemblyPose`, and `VoxelAssemblyPoseEvaluator`. Authored clips are sampled through `VoxelAnimationSampler`. Current body clips animate only the real `Body` part; left/right variants therefore mean distinct whole-Body motion, not invented limb articulation. Current one-part equipment animation uses rotation/scale on the Mount-bearing part and does not translate that part while equipped, so the grip remains at the body socket.

`AvatarMotionPlayer` owns one semantic phase clock, not procedural tween tracks. The supported semantic motions are:

- `Idle`
- `WalkForward`
- `Attack`, with body-side resolution `Attack.Left` / `Attack.Right` when authored
- `Hit`
- `Death`

Every semantic motion may additionally author model-local take variants with canonical suffix `V<n>` (`n > 0`), for example `Idle.V1`, `WalkForward.V2`, `Death.V3`, or side-qualified `Attack.Left.V2`. Variant counts are independent per motion and per model: `V2` has no global choreography or gameplay meaning. Variants inherit their base semantic loop policy: Idle/WalkForward variants loop; Attack/Hit/Death variants do not.

Published reusable models have a minimum authored V1 floor in addition to the baseline set. Every avatar publishes `Idle.V1`, `WalkForward.V1`, `Attack.Left.V1`, `Attack.Right.V1`, `Hit.V1`, and `Death.V1`; every equipment model publishes `Idle.V1`, `WalkForward.V1`, `Attack.V1`, `Hit.V1`, and `Death.V1`. Each required V1 must compile to animation content different from its direct fallback clip; an exact alias does not satisfy publication. Higher variants such as V2/V3 remain optional and model-local.

Variant selection is explicit consumer presentation context, never voxel-store randomness or gameplay state. For one model instance, requested motion `M`, physical side `S`, and variant `V` resolve in order `M.S.V -> M.V -> M.S -> M`. The body and each equipment instance may request different model-local variants while remaining phase-locked.

The body clip owns wall-clock cadence. The body clip and every equipment clip are sampled at the same normalized phase, even when their authored durations differ. The selected driver binding determines physical side context; optional variants remain independent. The motion player does not own combat results, damage timing, AI, inventory occupancy, variant-selection policy, or production locomotion.

Canonical animation proof is command-driven from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File tools\capture-game-view.ps1 -Scenario avatar-motion-gallery
```

The pinned streamed-actor provider is also exercised by one bounded preview-only consumer proof:

```powershell
powershell -ExecutionPolicy Bypass -File tools\capture-game-view.ps1 -Scenario mutable-streamed-actor-gallery
```

The generated runtime gallery serializes one `Mutable Streamed Actor Proof` configuration from the sorted baked catalogs but does not initialize it during ordinary gallery playback/capture. The dedicated scenario acquires a root-only streamed avatar through `VoxelStreamingService -> VoxelPresenterPool -> VoxelActorPresenterPool`, attaches two equipment instances by opaque handles, detaches one, transactionally replaces the other while retaining the handle, rebuilds handle-addressed animation plans after topology mutation, releases/reacquires the warm actor to prove the prior loadout is absent, and reattaches equipment. Every composed checkpoint validates the current avatar socket against the child `Mount` before provider-attested Game View capture. This remains preview/evidence infrastructure and does not define a production loadout API or gameplay semantics.

The command bootstraps/verifies the pinned Hero Studio package, resolves `Preview/Unity`, performs the bake-only provider refresh so current assembly/animation assets exist, builds only `[Voxel Store Runtime Gallery]`, resolves the schema-v2 capture registry, launches one owned **graphical** Unity Editor, invokes the registered `Capture.Editor` runner internally, survives Play Mode/domain reload through `SessionState`, samples deterministic authored baseline plus representative `V1` checkpoints for `Idle`, `WalkForward`, side-aware `Attack`, `Hit`, and `Death`, and captures composed runtime pixels through `Hero.Consumer.Editor.GameViewCapture`. Before capture, the runner rejects any live renderer outside `[Voxel Store Runtime Gallery]` and validates renderer/mesh/material/shader integrity, preventing the legacy debug grid from contaminating proof pixels. Accepted output lives under `Preview/Unity/Library/Capture/AvatarMotionGallery/` and must include current `game-view-capture.json` provider attestation.

`CAPTURE_ATTESTED` proves acquisition integrity only. Inspect the attested PNGs to verify the actual authored presentation: baseline and selected V1 takes must be visibly distinguishable where authored, Left- and Right-driven Attack must preserve side-aware body response, equipment must show its independently resolved authored motion, and its Mount must remain visually attached to the selected socket. Idle/Walk looping poses plus Hit/Death non-looping presentation must remain semantically readable.

`Voxel Store/Avatar Motion Preview` remains optional interactive exploration. It uses the same `AvatarMotionPlayer` authored-playback API and exposes separate Body Variant and Equipment Variant selections; clearing either returns that scope to ordinary side/base fallback. The `AvatarMotionPlayer` component Inspector enumerates exact clip names from the selected avatar's bound baked `VoxelAnimationAsset` and parses a selected name back into semantic motion, physical side, and optional variant state; it does not maintain a hard-coded clip/variant list. Individual `EquipmentPresentation` components also keep model-local variant state, so different attachments may be inspected with different takes. Opening the window, pressing buttons, selecting hierarchy objects, or editing the Inspector is not a completion gate.

The runtime gallery's direct camera/RenderTexture screenshot remains diagnostic-only for static framing. It is not canonical animation proof. Generated scenes, materials, captures, and reports remain ignored preview state; refreshing the gallery recreates derived scene/evidence without changing authoritative voxel source.


## Finalization
- Run the applicable deterministic validation and Tool tests when the pinned provider is available.
- Run strict OpenSpec validation for spec/workflow changes.
- Run `git diff --check`.
- Preserve unrelated dirty work.
