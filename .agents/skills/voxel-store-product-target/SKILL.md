---
name: voxel-store-product-target
description: Acquire, visually accept, freeze, and provenance-bind one high-fidelity Product Target for target-driven voxel-store authoring without requiring a specific generator or provider.
---

# Voxel Store Product Target

Use this skill only for target-driven Product Target authoring inside the voxel-store repository. It owns Product Target acquisition and visual acceptance; deterministic provenance validation remains Tool-owned.

1. Read repository `AGENTS.md` and `WORKFLOW.md`. Confirm the caller owns exactly one current asset job.
2. Resolve the asset's active target style authority. Inspect all applicable `targetReferences.common` plus the matching asset-kind target references. Do not borrow an unrelated kind scope.
3. Read the asset/form intent and any approved user-supplied visual direction. Separate **shared grammar/quality constraints** from **asset-specific identity**. Incidental exemplar choices such as palette, material recipe, grip/wrap pattern, accent color, or decorative motif are not global rules unless active style authority explicitly declares them family-wide.
4. Acquire a polished asset-specific Product Target candidate through any available method that is independent of downstream VoxelSpec/proof authority. Valid acquisition may include an approved supplied/imported image, repository-authored visual work, an external generator, or another future authoring method. No generator, provider, model, or application is mandatory.
5. Do not create Product Target authority from the asset's VoxelSpec, generated VOX, proof renders, density candidates, or other downstream implementation evidence. Product Target must remain upstream design authority.
6. Inspect each candidate visually. Reject it when it materially misses the approved style grammar, asset silhouette/proportions, material/color hierarchy, chunky voxel readability, or meaningful construction richness. Also reject mechanical cross-asset cloning when an unconstrained asset merely repeats another exemplar's palette/material/motif recipe without design intent.
7. Store the accepted candidate as repository-owned `Reference/target.png` bytes. If an external candidate is used, preserve the exact accepted bytes when importing it; do not substitute a screenshot, redraw, trace, or approximate recreation and claim it is the same candidate.
8. If one candidate cannot become repository-owned exactly, try another valid acquisition path when available. If no available path can establish repository-owned target bytes that pass visual/style acceptance, stop and report **blocked: Product Target authority unavailable**.
9. Write `Reference/target.provenance.json` using acquisition-neutral provenance version 3 for newly accepted targets:
   - every origin records exact `targetSha256`;
   - `origin: repository-authored` records current target-style authority/hash evidence and does not require source or generator metadata;
   - `origin: imported` records source path/hash plus `exact` or `canonical-derivative` relation;
   - generator/provider/model/request metadata is optional traceability only and never required to make v3 Product Target authority valid.
10. Run deterministic Product Target provenance validation through the normal reference workflow before any Composition authoring.
11. Once accepted, freeze the Product Target. Downstream Composition/Part extraction, density, VoxelSpec, and model proof must converge to it. Do not simplify or replace the target to make an easier model unless the current asset job explicitly becomes an intentional visual redesign.

The optional `Tool/import-product-target.ps1` helper can copy an external PNG byte-for-byte into the repository when convenient. It is a transport utility, not a required acquisition path or Product Target authority.

Procedural or generated rendering remains allowed for downstream construction-sheet layout, orthographic extraction aids, labels, diagnostic overlays, density candidates, or proof presentation when those outputs pass their owning gates. Those downstream outputs must never be promoted backward into Product Target authority.
