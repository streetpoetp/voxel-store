# Proposal

## Why

The current reference-first workflow can produce structurally valid Composition/Part Sheets that are visually much poorer than the style/source image they are meant to preserve. Once a simplified sheet becomes construction authority, later VPU selection only reproduces that simplification more accurately, so visual fidelity must be protected before construction authority is frozen.

## What Changes

- Introduce an asset-specific **Product Target** image as the visual-fidelity authority for new/redesigned assets. The target may be an imported approved image or a generated canonical derivative grounded in the active style references and asset intent.
- Promote at least one polished voxel-art source image into shared `common` style authority so equipment Product Targets inherit the same visual richness as the approved character/style references; simplified equipment construction boards remain supplemental form guidance rather than the sole fidelity source.
- Introduce a target-driven reference-package revision for migrated assets: Product Target -> Composition Sheet -> Part Sheet(s), with current provenance binding between every stage.
- Add a hard target-to-construction visual-fidelity audit before a Composition Sheet or Part Sheet may be accepted. The audit must cover silhouette, proportions, construction richness, material/color hierarchy, and meaningful detail density; a technically valid but materially simplified sheet fails.
- Make reference sheets visual-first: large readable asset views and construction evidence are primary; text annotations remain secondary and cannot substitute for visible construction detail.
- Forbid reference-stage preselection of exact voxel bounds, VPU, or tiny voxel budgets. Product-scale/density decisions remain downstream of accepted visual authority and, for migrated equipment, the reference-driven density stage.
- Preserve legacy reference-contract v2 assets without forcing an immediate catalog migration. New/redesigned assets use target-driven v3 reference authority when they enter the new workflow.
- Update the autonomous maintenance skill/workflow so a failed target-fidelity audit keeps ownership on the same target/sheet rather than allowing the workflow to advance to parts, source, density, or another asset.
- Add a bounded end-to-end proof showing that a detailed style-derived weapon target produces a Composition/Part package and a compiled/rendered non-published voxel model that preserve the target's visual richness instead of collapsing into the current tiny schematic form.

## Capabilities

### New Capabilities

- `reference-target-fidelity`: owns asset-specific Product Target authority, provenance-bound target-to-sheet visual-fidelity evidence, acceptance criteria, and staleness rules.

### Modified Capabilities

- `asset-publication`: target-driven v3 reference packages must include current Product Target and fidelity evidence before construction authority/source publication can advance; legacy v2 packages remain valid until migrated.
- `store-maintenance`: new/redesigned asset jobs become target-first and must repair the earliest Product Target/Composition/Part fidelity failure before advancing.
- `reference-style-authority`: distinguish shared high-fidelity visual-language authority, kind-specific construction examples, and each asset's Product Target; generated targets must consume applicable common + kind-specific inputs instead of being limited to simplified equipment boards.

## Impact

- `Tool/VoxelStore.Tool`: reference-contract loading/validation, target/fidelity provenance checks, reference sealing, and failure evidence.
- `Tool/VoxelStore.Tool.Tests`: v2 compatibility plus v3 target/fidelity acceptance, staleness, and fail-closed regressions.
- `.agents/skills/voxel-store-maintain/SKILL.md`: target-first creative workflow, image-first Composition/Part acceptance, and no premature bounds/VPU rule.
- Target-driven reference packages gain `Reference/target.png` (or declared equivalent) plus structured fidelity evidence alongside `composition.png` and `Parts/*.png`.
- Hero.Voxel remains unchanged and authoritative for construction-sheet semantics, compilation, preview, and runtime meshing.
- The completed reference-driven VPU selector remains downstream and is not replaced by this change.
