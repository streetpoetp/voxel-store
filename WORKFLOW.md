# Voxel Store Workflow

## Bootstrap

1. Read `AGENTS.md`.
2. Resolve the pinned Hero.Voxel provider through `toolchain/hero-voxel.lock.yaml` and `HERO_STUDIO_ROOT` when the current command requires the Tool to build or Hero.Voxel to run.
3. Use OpenSpec for architecture, ownership, schema, workflow, or other cross-cutting changes.
4. Start maintenance from deterministic Tool state; do not choose work from generated catalog/status files by hand.

Repository domain roots are explicit: `Assets` is reusable product content; `Styles` is visual authority; `Profiles` is physical compatibility; `Collections` is desired coverage; `Samples` is non-published experimentation; `Store` is global policy/history; `Catalog` is derived state.

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

- Product Target creation is owned by `.agents/skills/voxel-store-product-target/SKILL.md`.
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

Current Tool builds require a compatible `HERO_STUDIO_ROOT`. Making repository-local planning independent of that provider is a separate architecture change: `decouple-store-planning-from-hero-voxel-provider`.

## Finalization

- Run the applicable deterministic validation and Tool tests when the pinned provider is available.
- Run strict OpenSpec validation for spec/workflow changes.
- Run `git diff --check`.
- Preserve unrelated dirty work.
