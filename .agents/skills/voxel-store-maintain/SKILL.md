---
name: voxel-store-maintain
description: Maintain one bounded voxel-store asset job from deterministic plan through target/reference, density/source, proof, and publication gates.
---

# Voxel Store Maintain

Use this skill only inside the voxel-store repository.

1. Read the repository AGENTS.md and WORKFLOW.md.
2. Run the deterministic store plan command. Claim only the first returned job. Never select a second asset in the same invocation.
3. Resolve the active store referenceStyle for the selected asset kind. Preserve the distinction between construction-style authority (`references`) and target-generation authority (`targetReferences`). For target-driven work, inspect the applicable target common references plus the matching avatar/equipment target scope; never borrow an unrelated asset-kind scope. Shared polished references define the attainable visual grammar and quality bar, while simpler kind-specific boards are supplemental form/construction guidance and must not cap visual richness. Do not reinterpret incidental exemplar palette, material hierarchy, accent colors, grip/wrap patterns, or decorative motifs as family-wide requirements unless active style authority explicitly declares them global; unconstrained identity belongs to the asset Product Target.
4. Resolve the pinned Hero.Voxel provider from toolchain/hero-voxel.lock.yaml. Read the resolved canonical voxel-art-workflow.md before any reference or source authoring. Hero.Voxel remains authoritative for sheet structure, construction orientation, view requirements, lighting rules, part coverage, and implementation-semantic exclusions; do not copy or reinterpret those rules here.
5. If no job is returned, report the store healthy/no-op and make no authoritative asset mutation.
6. Choose the reference flow from the asset job:
   - Unchanged legacy reference-contract v2 repair may remain Composition-first.
   - New asset design or intentional visual redesign uses target-driven reference-contract v3.
7. For target-driven v3 work, establish and visually accept the Product Target before construction sheets. If an approved supplied image is suitable, use the imported-target flow. Otherwise invoke the `voxel-store-product-target` skill: new generated targets must come from a real image-generation model, must be delivered into the project as the exact accepted bytes, and must use generated provenance v2. Never fall back to `System.Drawing`, SVG/canvas/PIL drawing scripts, procedural voxel/pixel renderers, screenshots, tracing, or local approximations as generated Product Target authority. If image generation or exact-byte delivery is unavailable, keep the current asset blocked at Product Target authoring.
8. Generate the v3 Composition Sheet from the accepted Product Target. Audit it against the Product Target for silhouette, proportions, construction richness, material/color hierarchy, meaningful detail density, visual-evidence dominance, and implementation neutrality. Only after all criteria pass may the Composition Sheet freeze `RequiredParts`.
9. Generate exactly one uncovered Part Sheet at a time. Audit that same sheet against both the accepted Product Target and the pinned Hero.Voxel construction rules before advancing. If Product Target, Composition, or current Part Sheet fails any visual/style/construction audit, repair that same evidence for the current asset; do not advance to another sheet, density/source stage, or asset.
10. Product Target, Composition, and Part Sheets must remain implementation-neutral. Never freeze exact VPU, exact W x H x D voxel dimensions, source voxel budgets, exact pivots, sockets, mount transforms, or pair corrections in visual-reference authority. Relative visual proportions/thickness are allowed. Implementation density belongs downstream.
11. After reference coverage is accepted, run reference-driven density selection when the equipment manifest declares `density: reference-driven`. Read `policy/vpu-ruler.yaml`; treat its low VPU ladder and asset-class ranges as shared search guidance only, never as a global VPU or hard clamp. Derive each unit's estimate from apparent voxel cadence in accepted Part Sheets plus intended world extent, evaluate nearby canonical candidates, allow intentional one-voxel features when they remain readable, and choose the lowest visually/structurally sufficient VPU for that unit. Otherwise proceed directly to source.
12. Author/repair the asset manifest, palette use, and VoxelSpec only after all applicable reference and density gates permit it. Accepted Product Target/Composition/Part authority is upstream; existing VoxelSpec/proof may inform explicit migration/recovery but must not override accepted visual intent.
13. Run the deterministic generate command, inspect standalone and isolated-part proof visually, then seal proof only after acceptance.
14. Run scan --write only after source/proof acceptance so compatibility remains a downstream verification layer. Visual attachment intent in equipment references must not be converted into exact pivot/socket/mount-transform or pair-correction authority.
15. If the selected asset still fails, repair the earliest owning gate for that same asset. Do not skip to another asset.
16. End after the claimed asset is healthy or explicitly blocked.

The skill owns creative decisions and visual audit only. Deterministic identity, target/reference provenance, fidelity-evidence staleness, source layout, density calculation, generation, compatibility, coverage, catalog, health, and job priority remain Tool-owned.
