---
name: voxel-store-product-target
description: Create and accept one high-fidelity Product Target for target-driven voxel-store authoring using an image-generation model and exact-byte project delivery.
---

# Voxel Store Product Target

Use this skill only for target-driven Product Target authoring inside the voxel-store repository. It owns creative image generation and visual acceptance only; deterministic provenance validation remains Tool-owned.

1. Read repository `AGENTS.md` and `WORKFLOW.md`. Confirm the caller owns exactly one current asset job.
2. Resolve the asset's active target-generation style authority. Inspect all applicable `targetReferences.common` plus the matching asset-kind target references. Do not borrow an unrelated kind scope.
3. Read the asset/form intent and any approved user-supplied visual direction. Simple construction boards may clarify form, but they must not cap the richness established by the polished style references.
4. If an approved supplied image already defines the asset at product quality, use the imported Product Target flow instead of generating a new target.
5. Otherwise use a real image-generation model. A Product Target must not be invented with `System.Drawing`, SVG/canvas/PIL drawing scripts, procedural pixel/voxel renderers, VoxelSpec previews, or hand-built geometric diagrams.
6. Build a repository-owned `Reference/target.request.json` before accepting the final result. Record the asset intent, requested visual qualities, negative constraints, and the generation prompt/brief needed to understand the accepted image. Do not put secrets, private URLs, access tokens, or provider credentials in the request.
7. Generate one or more image-model candidates grounded in the applicable style images. The candidate should be a polished product-quality visual target, not a construction diagram. It may use presentation perspective/lighting as long as the intended silhouette, proportions, materials, color hierarchy, and meaningful construction detail are readable.
8. Inspect each candidate visually. Reject and regenerate when it materially misses the approved style language, silhouette, proportions, material/color hierarchy, chunky voxel readability, or meaningful construction richness. Do not accept merely because the requested asset class is recognizable.
9. Deliver the accepted candidate into `Reference/target.png` as the exact generated image bytes. Valid paths are:
   - the image generator writes directly into the project;
   - the generator returns a public/downloadable artifact that the Runner stores byte-for-byte;
   - a trusted file/attachment import copies the exact accepted bytes.
   - on a same-desktop ChatGPT/WebCodex workflow, once the accepted image-model result exists as a local downloaded PNG, use `Tool/import-product-target.ps1 -Source <downloaded-png> -Destination <asset-reference>/target.png`; require source/destination SHA-256 equality before continuing. A screenshot or procedural recreation is not equivalent.
10. If the accepted image is visible to the agent but its exact bytes cannot be transferred into the project, stop and report **blocked: Product Target exact-byte delivery unavailable**. Never redraw, trace, screenshot, or procedurally approximate the visible result.
11. For newly generated targets, write `Reference/target.provenance.json` using provenance version 2:
    - `origin: generated`
    - exact `targetSha256`
    - `generator.kind: image-model`
    - nonempty `generator.provider` and `generator.model`
    - `generator.request.path` and hash for the repository request artifact
    - current target-style authority/hash evidence required by the Tool.
12. Run the deterministic Product Target provenance validation through the normal reference workflow before any Composition authoring.
13. Once accepted, freeze the Product Target. Downstream Composition/Part extraction, density, VoxelSpec, and model proof must converge to it. Do not simplify or replace the target to make an easier model unless the current asset job is explicitly changed into an intentional visual redesign.

Procedural rendering remains allowed downstream for construction-sheet layout, orthographic extraction aids, labels, diagnostic overlays, density candidates, or proof presentation when those outputs still pass their owning visual/construction gates. It is forbidden only as a substitute for newly generated Product Target creative authority.
