# Proposal: Simplify gate-driven voxel-store workflow

## Why

The current authoring workflow is correct but difficult to execute because lifecycle rules are repeated across `WORKFLOW.md`, `voxel-store-maintain`, `voxel-store-product-target`, and durable OpenSpec specifications. Agents must re-derive the same stage ordering and ownership rules before answering a simpler operational question: **what is the current asset's next gate?**

The repository already has deterministic scan/plan and publication gates. The workflow should use those gates as the primary control flow instead of duplicating a long manual stage sequence in multiple documents.

## What Changes

- Make the Tool-selected asset and earliest failing gate the canonical maintenance control flow.
- Reduce the main lifecycle to five concepts: **Plan -> Reference -> Source -> Proof -> Publish**.
- Make `voxel-store-maintain` a thin coordinator that executes the current gate, verifies it, and asks the Tool for the next gate on the same asset.
- Keep specialized creative detail in `voxel-store-product-target`; do not duplicate Product Target rules in the coordinator.
- Keep detailed validity rules in OpenSpec specs and numeric policy files; `WORKFLOW.md` becomes lifecycle/ownership guidance only.
- Move legacy v2 behavior out of the primary path and describe it as a compatibility exception selected by the Tool.
- Preserve the rule that a failed gate remains owned by the same asset and earliest stage until repaired or explicitly blocked.

## Non-goals

- Do not change the VPU ruler or density-selection semantics. Its current purpose-driven path is `Store/vpu-ruler.yaml`.
- Do not change Product Target fidelity semantics or Hero.Voxel construction semantics.
- Do not change selected-collection coverage or compatibility semantics.
- Do not make `plan`/`scan` provider-independent in this change. Decoupling those commands from `HERO_STUDIO_ROOT` is a separate Tool architecture follow-up.

## Impact

- Lifecycle documentation: `WORKFLOW.md`.
- Coordinator skill: `.agents/skills/voxel-store-maintain/SKILL.md`.
- Product Target skill only where needed to clarify its ownership boundary.
- Durable maintenance contract: `openspec/specs/store-maintenance/spec.md`.
- No production asset, style authority, VPU semantics, selected-collection semantics, or Hero.Voxel contract changes are owned by this workflow-only change.
