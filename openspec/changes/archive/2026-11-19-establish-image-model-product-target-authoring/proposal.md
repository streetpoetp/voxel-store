# Proposal

## Why

The target-driven workflow currently says to “generate” a Product Target but does not distinguish an image-generation model from a procedural drawing script. As a result, agents can understand the desired visual level yet still satisfy the workflow with low-fidelity `System.Drawing`/SVG-style output that becomes authoritative and drags every downstream voxel artifact down to that quality.

## What Changes

- Require newly generated Product Targets to be authored by an actual image-generation model grounded in the active high-fidelity target style references and asset form intent.
- Introduce generated Product Target provenance v2 that records the image-model generator identity and a repository-owned, hash-bound generation request/creative brief.
- Keep existing generated provenance v1 valid for unchanged published assets; maintenance must not create new v1 generated targets.
- Fail closed at the Product Target stage when the agent lacks a usable image-generation capability or cannot place the exact generated image bytes into the project. Procedural redraw is not an acceptable fallback.
- Add a dedicated Product Target authoring skill that owns candidate generation, visual iteration, exact-byte project delivery, acceptance, request capture, and provenance.
- Add a same-desktop exact-byte import bridge so ChatGPT/WebCodex can move an accepted downloaded image-model PNG into the project with SHA-256 equality instead of redrawing it.
- Keep deterministic/scripted rendering available for downstream construction sheets, annotations, diagnostics, and proof tooling, but not for inventing generated Product Target visual authority.
- Preserve the downstream authority chain: accepted Product Target -> Composition/Part extraction -> density -> VoxelSpec -> Hero.Voxel model -> model-to-target visual audit.
- Remove tracked Sword02/Dagger02 procedural target-generator scripts so they are not copied as the canonical pattern; their already-accepted Product Target PNGs/provenance remain valid legacy evidence.
- Add `Art/Samples/AxeStyleTest` as an end-to-end proof that an image-model Product Target can remain frozen while downstream construction and Hero.Voxel geometry iterate toward it.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `reference-target-fidelity`: generated Product Targets must use image-model authoring and auditable generator/request provenance, with fail-closed exact-byte delivery.
- `asset-publication`: new/generated target-driven publication must require current image-model Product Target provenance before construction/source authority may advance.

## Impact

- `Tool/VoxelStore.Tool/Contracts.cs` and Product Target provenance validation.
- Tool regression tests for generated provenance v2 and backward-compatible v1.
- `.agents/skills/voxel-store-maintain/SKILL.md` plus a dedicated Product Target authoring skill.
- `WORKFLOW.md` target-driven flow and failure semantics.
- `Tool/import-product-target.ps1` exact-byte transport helper and `Art/Samples/AxeStyleTest` target-to-model proof.
- Existing Sword02/Dagger02 procedural `generate-target.ps1` helpers are removed; accepted PNGs and v1 provenance are not rewritten.
- No hard dependency on one image provider is introduced. The agent may use any available image-generation capability that can deliver the exact accepted image into the project.
