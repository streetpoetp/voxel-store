---
name: voxel-store-maintain
description: Maintain one bounded voxel-store asset job from deterministic plan through canonical Hero.Voxel reference/source/proof gates.
---

# Voxel Store Maintain

Use this skill only inside the voxel-store repository.

1. Read the repository AGENTS.md and WORKFLOW.md.
2. Run the deterministic store plan command. Claim only the first returned job. Never select a second asset in the same invocation.
3. Resolve the active store referenceStyle for the selected asset kind and visually inspect only its applicable style authority: optional common references plus the matching avatar/equipment scope. Never borrow an unrelated asset-kind scope. The shared visual clause is: chunky low-resolution voxel forms, simplified readable geometry, restrained detail density, clear separated color regions, high silhouette readability, and no realistic textures or painterly/non-voxel forms. Equipment jobs additionally preserve readable silhouette/thickness, proportion language, and visual grip/attachment-region treatment from equipment-scoped references.
4. Resolve the pinned Hero.Voxel provider from toolchain/hero-voxel.lock.yaml. Read the resolved canonical voxel-art-workflow.md before any reference or source authoring. Hero.Voxel remains authoritative for sheet structure, construction orientation, view requirements, lighting rules, part coverage, and implementation-semantic exclusions; do not copy or reinterpret those rules here.
5. If no job is returned, report the store healthy/no-op and make no authoritative asset mutation.
6. If style authority cannot be resolved, provider resolution fails, visual construction intent is ambiguous, or any style/construction visual audit fails, stop on the current sheet and current asset with concrete evidence. Do not advance to another Part Sheet or asset.
7. For a design/repair job, follow the canonical reference sequence exactly: Composition Sheet only, audit/freeze RequiredParts, then one uncovered Part Sheet at a time with audit before advancing.
8. Author/repair the asset manifest, palette use, and VoxelSpec only after reference coverage permits it. For equipment design/redesign, accepted Composition/Part Sheets are upstream authority; existing VoxelSpec/proof may inform explicit migration/recovery but must not override accepted reference intent.
9. Run the deterministic generate command, inspect standalone and isolated-part proof visually, then seal proof only after acceptance.
10. Run scan --write only after source/proof acceptance so compatibility remains a downstream verification layer. Visual attachment intent in equipment references must not be converted into exact pivot/socket/mount-transform or pair-correction authority.
11. If the selected asset still fails, repair the earliest owning gate for that same asset. Do not skip to another asset.
12. End after the claimed asset is healthy or explicitly blocked.

The skill owns creative decisions and visual audit only. Deterministic identity, source layout, generation, compatibility, coverage, catalog, health, and job priority remain Tool-owned.
