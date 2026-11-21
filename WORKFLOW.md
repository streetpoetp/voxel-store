# Voxel Store Workflow

## Bootstrap

1. Read `AGENTS.md`.
2. Initialize the repository-owned Hero Studio provider with `git submodule update --init --recursive`; `Dependencies/hero-studio` plus `toolchain/hero-voxel.lock.yaml` is the single Hero.Voxel dependency authority.
3. Use OpenSpec for architecture, ownership, schema, workflow, or other cross-cutting changes.4. Start maintenance from deterministic Tool state; do not choose work from generated catalog/status files by hand.

Repository domain roots are explicit: `Assets` is reusable product content; `Styles` is visual authority; `Profiles` is physical compatibility; `Collections` is desired coverage; `Store` is global policy/history; `Catalog` is derived state.

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

### 4. Proof

Generate deterministic output and proof for the current source, visually inspect the required evidence, and seal only accepted proof.

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
Open `Preview/Unity/` with Unity `6000.3.24f1`. In the Editor run:

`Voxel Store/Refresh All & Open Preview Grid`

The command derives the current published set through `VoxelStore.Tool preview-plan`, reads each publication-validated `.generated.vox` and generated animation manifest in place, bakes them through the pinned full Hero Studio package into ignored `Assets/__VoxelStorePreview/Baked/**`, and reconstructs the canonical scene grid. Preview refresh does not rewrite voxel-store source or generated publication artifacts. Deleting the derived Unity bake root is safe; refresh recreates it from store authority.

## Finalization
- Run the applicable deterministic validation and Tool tests when the pinned provider is available.
- Run strict OpenSpec validation for spec/workflow changes.
- Run `git diff --check`.
- Preserve unrelated dirty work.
