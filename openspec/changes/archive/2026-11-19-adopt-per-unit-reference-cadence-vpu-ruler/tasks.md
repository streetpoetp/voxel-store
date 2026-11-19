# Tasks

## 1. Synchronize density authority documentation

- [x] 1.1 Add `policy/vpu-ruler.yaml` as the single shared numeric VPU search policy and verify it declares per-unit scope, the low candidate ladder, broad non-binding priors, and one-voxel readability semantics.
- [x] 1.2 Synchronize `openspec/specs/reference-density-selection/spec.md` with per-unit reference-cadence selection and verify strict OpenSpec validation passes.
- [x] 1.3 Update `WORKFLOW.md` and `voxel-store-maintain` to document density ownership and verify neither document encodes a global selected VPU.

## 2. Implement the synchronized density contract

- [x] 2.1 Extend density input/evidence contracts with ruler revision/hash and cadence measurements; verify round-trip/staleness tests cover both.
- [x] 2.2 Change candidate estimation to use cadence-derived per-unit VPU plus the canonical ruler neighborhood instead of arbitrary high candidate sequences; verify deterministic selector tests cover two units selecting different VPUs.
- [x] 2.3 Replace universal multi-voxel feature minima with visual/structural readability semantics; verify a one-voxel intentional feature can pass while a disappeared/merged feature fails.
- [x] 2.4 Re-run KnifeStyleTest on low candidates centered near detected cadence and verify the selected VPU is the lowest visually sufficient result.
- [x] 2.5 Recompute affected Sword02/Dagger02 density evidence only after the new selector is proven; verify source/proof/publication gates remain coherent.

## Workflow follow-up

- Archive the change only after selector implementation and migration tasks are complete.
