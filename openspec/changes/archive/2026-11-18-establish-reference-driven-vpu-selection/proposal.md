# Proposal

## Why

Weapon authoring density is currently chosen manually even though accepted Part Sheets already contain the visual detail that should determine required voxel resolution. The successful VPU64 staff proof showed that higher density works, but production authoring needs a repeatable way to choose the **lowest sufficient VPU from reference fidelity requirements** instead of hardcoding 64, 96, or one global value.

## What Changes

- Add reference-driven VPU selection after accepted Part Sheets and before VoxelSpec source authoring.
- Derive density evidence from accepted orthographic Part Sheets rather than arbitrary source/marketing images, so perspective, labels, multiple subjects, and decorative context do not drive voxel resolution.
- Record meaningful visual-feature measurements, target world dimensions, the estimated VPU, evaluated candidate VPUs, per-candidate acceptance evidence, and the selected minimum sufficient VPU in generated/staleable evidence.
- Select the lowest candidate that preserves required reference features while satisfying Hero.Voxel's 256-axis limit, store source-voxel budget, and normalized world-size constraints; record greedy-mesh complexity as selection evidence without inventing a new runtime budget in this change.
- Add a publication `density` gate between `reference` and `source`; source authored at an unproven or stale VPU cannot publish.
- Introduce an explicit migration opt-in on equipment manifests (`density: reference-driven`); only opted-in equipment is governed by the new density gate during migration, while existing non-opted-in equipment remains valid legacy content until migrated individually.
- Extend autonomous maintenance so accepted equipment references trigger density analysis/selection before source authoring.
- Prove the workflow end-to-end on Staff01: refresh its canonical reference package to the approved product-quality staff target, let the selector choose among candidate densities instead of preselecting VPU64, then author Staff01 source at the selected density.
- Keep Hero.Voxel unchanged: it remains the compiler/preview/runtime-mesh provider; voxel-store owns product-reference density decisions.

## Capabilities

### New Capabilities

- `reference-density-selection`: derives, evaluates, records, and invalidates reference-driven authoring-density decisions from accepted Part Sheets and normalized product-size constraints.

### Modified Capabilities

- `asset-publication`: require current reference-density evidence as an explicit publication gate before source authority is accepted for reference-driven equipment.
- `store-maintenance`: run reference-density selection autonomously after equipment reference acceptance and before source authoring/repair.

## Impact

- New density-evidence contracts and selector logic in `Tool/VoxelStore.Tool`, plus regression tests.
- Publication health/gate ordering gains a `density` gate between reference and source.
- Equipment reference audit hashes become upstream provenance for density evidence; a changed accepted Part Sheet stales the previous density decision.
- Staff01 opts into `density: reference-driven` and migrates reference/source/proof plus candidate-density evidence end-to-end; the other nine weapon manifests/sources remain legacy and out of scope.
- Existing hand-neutral compatibility, asset identities, world-space scale normalization, Hero.Voxel source ownership, and gameplay-neutral metadata boundaries remain unchanged.
