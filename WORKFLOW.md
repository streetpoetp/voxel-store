# Voxel Store Workflow

## Bootstrap

1. Read `AGENTS.md`.
2. Use the repository-owned Hero Studio provider already present at `Dependencies/hero-studio`; that directory plus `toolchain/hero-voxel.lock.yaml` is the single Hero.Voxel dependency authority. Store Tool/preview execution does not invoke Git or inspect version-control history.
3. Use OpenSpec for architecture, ownership, schema, workflow, or other cross-cutting changes.
4. Start maintenance from deterministic Tool state; do not choose work from generated catalog/status files by hand.

Repository domain roots are explicit: `Assets` is reusable product content; `Styles` is visual authority; `Collections` is desired coverage; `Store` is global policy/history, including reusable visual animation profiles; `Catalog` is derived state. Physical avatar/equipment compatibility remains model-owned socket/Mount data and is independent of visual animation-profile classification.

### Canonical model identity

Active model identity is mechanically tied to its reusable model structure:

```text
Assets/Avatars/Wraith01
  source.base = ./Source/Wraith01
  ModelKey    = wraith-01
  id          = avatar.wraith-01
  unitRef     = voxel-store/avatar/wraith-01

Assets/Equipment/Rifle02
  source.base = ./Source/Rifle02
  ModelKey    = rifle-02
  id          = equipment.rifle-02
  unitRef     = voxel-store/equipment/rifle-02
```

`ModelKey` is the deterministic lowercase-kebab projection of UpperCamel words, acronym runs, and decimal runs in `ModelName`; decimal digits, including leading zeroes, are preserved. Tool validates catalog, kind, ModelName, source base, id, and UnitRef as one contract. Collection/origin namespaces are not reusable identity. After publication, an ordinary repository move that preserves kind and ModelName preserves identity; changing ModelName or kind requires an explicit identity migration and retirement of the previous pair.

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
- **Source:** pinned Hero.Voxel authoring semantics plus deterministic Tool/source validation. Resolve the manifest animation profile here and author only its meaningful families; every declared family carries a default plus V1/V2/V3 with distinct choreography.
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
- Avatars own concrete equipment sockets such as `Equip.Hand.Left.0` and `Equip.Hand.Right.0`; equipment owns an exact `Mount` socket. Generic socket-to-Mount composition is the only reusable placement contract.
- Every published reusable model uses one semantic canonical Unity local frame: Right `+X`, Up `+Y`, Forward `-Z`. This facing contract is independent of Hero.Voxel source-coordinate conversion. Forward is the outward normal of semantic FRONT: avatar face/gaze/chest for avatars, and intended game-facing/action direction for equipment when mounted on a forward-facing avatar. Projectile equipment uses launch/target direction; directional melee equipment uses its intended cut/thrust/strike direction. Do not infer Forward from the longest dimension, a broad presentation face, an arbitrary mesh axis, or a coordinate-conversion label alone. Correct source geometry/pivot asset-by-asset before publication; consumers must not store corrective placement rotation.
- Multiple equipment instances bind independently to concrete compatible sockets. Adding another equipment instance does not change avatar or equipment source.
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
dotnet run --project Tool/VoxelStore.Tool -- accept-animation-review <asset-id> <game-view-capture.json>
dotnet run --project Tool/VoxelStore.Tool -- seal-reference <asset-id>
dotnet run --project Tool/VoxelStore.Tool -- seal <asset-id>
```

Current Tool builds resolve Hero.Voxel directly from `Dependencies/hero-studio`; no `HERO_STUDIO_ROOT` or developer-local provider checkout is required.

## Unity baked preview
The Unity workspace under `Preview/Unity/` is Editor-owned inspection/validation tooling for real baked Hero.Voxel outputs. Its canonical all-model review grid executes the pinned provider runtime only while Unity is in Play Mode; the workspace is not voxel-store source authority, publication authority, a production game runtime, or a reusable voxel runtime API.

From the repository root:

```powershell
./Preview/Bootstrap.ps1
```

The bootstrap requires the repository-owned Hero Studio provider files to already exist, verifies the Hero.Voxel workflow fingerprint declared by `toolchain/hero-voxel.lock.yaml`, then exposes the **entire** `hero.studio` package through one ignored embedded-package junction at `Preview/Unity/Packages/hero.studio`. It does not initialize/fetch a dependency, invoke Git, inspect `HEAD`, or use version-control state as provider authority. The junction points to `Dependencies/hero-studio/src/client/unity/Packages/hero.studio`; it does not select modules, copy provider code, or patch provider-owned shaders.
Open `Preview/Unity/` with Unity `6000.3.24f1`.

The visible `Voxel Store` menu exposes one store-owned review action:

- `Voxel Store/Refresh Preview Grid`

### Editor preview and content proof

`Voxel Store/Refresh Preview Grid` bakes every currently published model, opens the disposable shell at `Assets/Preview/VoxelStorePreview.unity`, records only a transient Editor-session launch intent, and enters Play Mode. After Play Mode begins, Editor orchestration reloads the current preview-plan and realizes the canonical grid through one pinned Hero.Voxel `VoxelRenderWorld`. Avatar entries use direct `VoxelActorInstance` composition; standalone equipment uses direct `VoxelRenderInstance` plus its canonical `VoxelModelAnimationTrack`. The saved scene remains a shell and never serializes the generated runtime grid.

Avatar grid entries also show deterministic carried equipment for composition review: the first selected equipment is attached by Hero Studio as `Equip.Hand.Right.0 -> Mount`, and when more than one equipment model exists a second deterministic equipment model is attached as `Equip.Hand.Left.0 -> Mount`. No GameObject transform correction or pair-specific placement record is maintained by voxel-store. Equipment models still keep their own standalone grid entries, so the grid remains a complete published-content review surface.

Preview bake paths mirror the authoritative model directory, never the asset id. Canonical source `Assets/<Catalog>/<Model>/Source/<Model>` bakes to `Assets/__VoxelStorePreview/Baked/<Catalog>/<Model>/<Model>.assembly.asset` plus sibling `<Model>.animation.asset`. Baked output remains disposable and reconstructable from authoritative voxel-store source.

Published avatars own a `Body` part and authored equipment sockets such as `Equip.Hand.Left.0` and `Equip.Hand.Right.0`; equipment owns an exact `Mount` socket. These are content contracts. Generic socket-to-Mount composition, reference influence, model animation tracks, bounds, and render lifetime are Hero Studio responsibilities.

Published reusable models satisfy the Tool-resolved animation profile: every declared family exposes its default plus V1/V2/V3, while irrelevant families are absent. After the grid realizes its ordinary render instances and canonical tracks, the grid owner delegates one disposable preview scope to Hero Studio through `VoxelRuntimeInspectionController.BeginPreview(world)`. The provider window `Hero/Voxel/Debug/Runtime Inspector` opens on interactive launch; select the actual model using opt-in Scene View picking or the live-world browser. The provider owns clip selection, play/pause, reset, scrubbing, speed, and loop controls for each delegated track. There are no per-model hierarchy hosts, colliders, store-owned picker, player, timer, clip inventory, or mirrored playback state. Direct baked-asset inspection outside the canonical grid may still use Hero.Voxel asset Inspector / Scene Debug in Edit Mode as separate provider tooling.

Canonical composed-content proof remains command-driven from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File tools\capture-game-view.ps1 -Scenario avatar-motion-gallery
```

The capture runner performs the bake-only refresh, enters an empty Play Mode scene, discovers current baked avatar/equipment assets, then creates a temporary Hero.Voxel `VoxelRenderWorld`, caller-owned render instances, and one direct `VoxelActorInstance` per avatar. Voxel-store supplies only selected store assets, socket/`Mount`, exact authored clip names, deterministic phases, camera/layout, checkpoints, and visual acceptance. Provider runtime state is scenario-local and disposed when capture completes.

Voxel-store does not own or re-prove streamed Actor pooling, attach/detach/rebind/replace transactionality, activation-handle staleness, warm reacquire, or provider track lifecycle. Those are Hero Studio contracts. A missing provider capability fails closed and is evaluated separately; it is not recreated in voxel-store.

`CAPTURE_ATTESTED` proves acquisition integrity only. Inspect the PNGs before running `accept-animation-review`: every Tool-planned take must remain readable and physically appropriate to its profile; loops, terminal poses, equipment Drop, and any Attack release/return trajectory must be visually acceptable. The accepted review is then sealed as downstream evidence; capture alone never publishes an asset.

Generated Play Mode world objects, provider runtime instances, baked Unity assets, reports, and captures remain derived evidence. Rebuilding or leaving Play Mode revokes the preview scope before disposing the runtime grid; none of them become voxel-store authoring, publication, compatibility, or runtime authority.

## Finalization
- Run the applicable deterministic validation and Tool tests when the pinned provider is available.
- Run strict OpenSpec validation for spec/workflow changes.
- Run `git diff --check`.
- Preserve unrelated dirty work.
