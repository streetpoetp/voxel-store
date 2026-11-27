# Voxel Store Workflow

## Bootstrap

1. Read `AGENTS.md`.
2. Use the repository-owned Hero Studio provider already present at `Dependencies/hero-studio`; that directory plus `toolchain/hero-voxel.lock.yaml` is the single Hero.Voxel dependency authority. Store Tool/preview execution does not invoke Git or inspect version-control history.
3. Use OpenSpec for architecture, ownership, schema, workflow, or other cross-cutting changes.
4. Start maintenance from deterministic Tool state; do not choose work from generated catalog/status files by hand.

Repository domain roots are explicit: `Assets` is reusable product content; `Styles` is visual authority; `Collections` is desired coverage; `Store` is global policy/history; `Catalog` is derived state. Reusable avatar/equipment compatibility is defined by the global rigid-Body composition contract rather than a separate profile domain.

## Canonical asset pipeline

`.agents/skills/voxel-store-pipeline/SKILL.md` is the single end-to-end coordinator for one asset job.

- A direct request such as `create an axe` enters through `voxel-store-create-model`, which establishes exactly one new-production intent and delegates that same job to the pipeline at **Intent**.
- Autonomous maintenance enters through `voxel-store-maintain`, which runs deterministic Tool scan/plan, claims only the first asset/current gate, and delegates that same job to the corresponding pipeline stage.
- Neither entrypoint owns a second lifecycle. Detailed validity remains with the Tool, OpenSpec specs, `voxel-store-product-target`, store policy, and the pinned Hero.Voxel workflow.

```text
INTENT
  -> PRODUCT TARGET
  -> CONSTRUCTION
  -> DENSITY
  -> SOURCE
  -> GENERATE
  -> RUNTIME PRESENTATION PROOF
  -> COMPATIBILITY
  -> SEAL
  -> PUBLISH
```

The pipeline always evaluates that order for the same asset. A stage is not applicable only when its owning contract explicitly permits that result.

Stage ownership is deliberately delegated:

- **Intent:** explicit user intent or deterministic Collection/Tool selection plus current store identity/style contracts.
- **Product Target:** `voxel-store-product-target` plus current target/style/provenance contracts.
- **Construction:** the pinned Hero.Voxel `voxel-art-workflow.md` plus target/style fidelity contracts.
- **Density:** `reference-density-selection`, active policy, and deterministic Tool evaluation when applicable.
- **Source:** pinned Hero.Voxel authoring semantics plus deterministic Tool/source validation.
- **Generate:** Tool/Hero.Voxel generation plus the current proof contracts for derived output.
- **Runtime Presentation Proof:** `unity-baked-preview` and current proof/compatibility contracts when they require provider-backed runtime evidence.
- **Compatibility:** `asset-compatibility` plus Tool evaluation.
- **Seal:** current accepted reference/proof evidence plus Tool sealing commands.
- **Publish:** `asset-publication` plus Tool scan/verify and derived publication state.

### Maintenance selection

Maintenance planning is a preface to the shared pipeline, not a second lifecycle. Run deterministic scan/plan state and claim only the first asset-scoped job plus its earliest unresolved/current gate.

- If no job exists, the store is healthy: stop.
- Never skip an earlier unresolved gate to work on a later stage.
- One maintenance invocation owns only that selected asset.
- After any stage progress, re-derive Tool state for that same asset before advancing.

### Rigid avatar and equipment composition

Published reusable avatars use exactly one authored rigid part named `Body`.

- Do not author `Head`, `Arm`, `Hand`, `Foot`, socket, mount, or authored combat-animation parts for reusable avatar compatibility.
- `Body` local bounds after VPU normalization are the canonical equipment-placement space.
- Every published reusable model uses one semantic canonical Unity local frame: Right `+X`, Up `+Y`, Forward `-Z`. This facing contract is independent of Hero.Voxel source-coordinate conversion. Forward is the outward normal of semantic FRONT: avatar face/gaze/chest for avatars, and intended game-facing/action direction for equipment when mounted on a forward-facing avatar. Projectile equipment uses launch/target direction; directional melee equipment uses its intended cut/thrust/strike direction. Do not infer Forward from the longest dimension, a broad presentation face, an arbitrary mesh axis, or a coordinate-conversion label alone. Correct source geometry/pivot asset-by-asset before publication; consumers must not store corrective placement rotation.
- A normalized source-space ratio `r = (x, y, z)`, with every component in `[-1,+1]`, resolves as `bodyBounds.center + bodyBounds.extents * r`. Unity presentation maps the Hero.Voxel source axes through the provider's canonical source-to-Unity basis.
- Multiple equipment instances are an ordered consumer presentation list of independent `{ equipmentId, offsetRatio }` values. Adding another equipment instance does not change avatar source.
- Primary/secondary, left/right, weapon class, inventory occupancy, damage, and attack behavior are consumer gameplay semantics and are not voxel-store metadata.

### Proof evidence notes

Generated deterministic proof remains downstream evidence. `front.png` is semantic-frame evidence: its camera stands on Unity `-Z` and looks toward `+Z`, so a conforming model's outward semantic Forward points toward the viewer at Unity `-Z`.
The generated Hero.Voxel `Proof/.../front.png` owns this semantic-frame proof. A `FRONT` label embedded in `Reference/Parts/*.png` is reference/Product Target evidence and is not, by itself, model-frame authority; for ranged equipment it may intentionally show a broad side view while the generated semantic front is muzzle/target-facing.

Compilation is not visual acceptance. If evidence fails its owning proof contract, repair the same asset/stage.

## Gate loop

After every stage for the same asset:

```text
PASS           -> re-derive authoritative state and continue
NOT APPLICABLE -> continue only when the stage owner permits it
FAIL           -> repair the same stage
BLOCKED        -> report the blocking authority/evidence/dependency and stop
PUBLISHED      -> stop
```

Do not maintain a second manual stage tracker or validation checklist beside deterministic Tool state and the owning contracts.


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
./Preview/Bootstrap.ps1
```

The bootstrap requires the repository-owned Hero Studio provider files to already exist, verifies the Hero.Voxel workflow fingerprint declared by `toolchain/hero-voxel.lock.yaml`, then exposes the **entire** `hero.studio` package through one ignored embedded-package junction at `Preview/Unity/Packages/hero.studio`. It does not initialize/fetch a dependency, invoke Git, inspect `HEAD`, or use version-control state as provider authority. The junction points to `Dependencies/hero-studio/src/client/unity/Packages/hero.studio`; it does not select modules, copy provider code, or patch provider-owned shaders.
Open `Preview/Unity/` with Unity `6000.3.24f1`.

The visible `Voxel Store` menu exposes one store-owned review action:

- `Voxel Store/Refresh Preview Grid`

### Editor preview and content proof

`Voxel Store/Refresh Preview Grid` performs a bake-only provider refresh for every published model and rebuilds the canonical Editor-only grid in `Assets/Preview/VoxelStorePreview.unity`. Baked `VoxelAssemblyAsset` and matching `VoxelAnimationAsset` files live under `Preview/Unity/Assets/__VoxelStorePreview/Baked/<Catalog>/<Model>/`. The grid uses pinned Hero.Voxel Editor `VoxelSceneDebugBuilder` / Scene Debug behavior; voxel-store does not create a runtime presenter, animation player, Actor wrapper, renderer, or model-frame abstraction.

Preview bake paths mirror the authoritative model directory, never the asset id. Canonical source `Assets/<Catalog>/<Model>/Source/<Model>` bakes to `Assets/__VoxelStorePreview/Baked/<Catalog>/<Model>/<Model>.assembly.asset` plus sibling `<Model>.animation.asset`. Baked output remains disposable and reconstructable from authoritative voxel-store source.

Published avatars own a `Body` part and authored equipment sockets such as `Equip.Hand.Left.0` and `Equip.Hand.Right.0`; equipment owns an exact `Mount` socket. These are content contracts. Generic socket-to-Mount composition, reference influence, model animation tracks, bounds, and render lifetime are Hero Studio responsibilities.

Published reusable models keep the required authored V1 floor. Interactive clip inspection uses the current baked `VoxelAnimationAsset` through Hero.Voxel's own Inspector / Scene Debug tooling; voxel-store does not maintain a second clip inventory or playback state.

Canonical composed-content proof remains command-driven from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File tools\capture-game-view.ps1 -Scenario avatar-motion-gallery
```

The capture runner performs the bake-only refresh, enters an empty Play Mode scene, discovers current baked avatar/equipment assets, then creates a temporary Hero.Voxel `VoxelRenderWorld`, caller-owned render instances, and one direct `VoxelActorInstance` per avatar. Voxel-store supplies only selected store assets, socket/`Mount`, exact authored clip names, deterministic phases, camera/layout, checkpoints, and visual acceptance. Provider runtime state is scenario-local and disposed when capture completes.

Voxel-store does not own or re-prove streamed Actor pooling, attach/detach/rebind/replace transactionality, activation-handle staleness, warm reacquire, or provider track lifecycle. Those are Hero Studio contracts. A missing provider capability fails closed and is evaluated separately; it is not recreated in voxel-store.

`CAPTURE_ATTESTED` proves acquisition integrity only. Inspect the PNGs to verify authored content: baseline/V1 takes remain readable, Left/Right body Attack remains distinct where authored, equipment shows its independently authored motion, and socket/Mount composition is visually valid through release/return trajectories.

Generated preview scenes, baked Unity assets, reports, and captures remain derived evidence. They never become voxel-store authoring, publication, compatibility, or runtime authority.

## Finalization
- Run the applicable deterministic validation and Tool tests when the pinned provider is available.
- Run strict OpenSpec validation for spec/workflow changes.
- Run `git diff --check`.
- Preserve unrelated dirty work.
