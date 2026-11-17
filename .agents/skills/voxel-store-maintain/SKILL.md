---
name: voxel-store-maintain
description: Maintain one bounded voxel-store asset job from deterministic plan through canonical Hero.Voxel reference/source/proof gates.
---

# Voxel Store Maintain

Use this skill only inside the voxel-store repository.

1. Read the repository AGENTS.md and WORKFLOW.md.
2. Run the deterministic store plan command. Claim only the first returned job. Never select a second asset in the same invocation.
3. Resolve the pinned Hero.Voxel provider from toolchain/hero-voxel.lock.yaml. Read the resolved canonical voxel-art-workflow.md before any reference or source authoring. Do not copy or reinterpret that workflow here.
4. If no job is returned, report the store healthy/no-op and make no authoritative asset mutation.
5. If provider resolution fails, visual construction intent is ambiguous, or any visual audit fails, stop on the current job with concrete evidence.
6. For a design/repair job, follow the canonical reference sequence exactly: Composition Sheet only, audit/freeze RequiredParts, then one uncovered Part Sheet at a time with audit before advancing.
7. Author/repair the asset manifest, palette use, and VoxelSpec only after reference coverage permits it.
8. Run the deterministic generate command, inspect standalone and isolated-part proof visually, then seal proof only after acceptance.
9. Run scan --write so compatibility, publication, catalog, health, and next work are derived from current evidence.
10. If the selected asset still fails, repair the earliest owning gate for that same asset. Do not skip to another asset.
11. End after the claimed asset is healthy or explicitly blocked.

The skill owns creative decisions and visual audit only. Deterministic identity, source layout, generation, compatibility, coverage, catalog, health, and job priority remain Tool-owned.
