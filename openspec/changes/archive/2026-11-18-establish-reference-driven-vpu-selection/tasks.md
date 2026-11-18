# Tasks

## 1. Density contracts and deterministic selection

- [x] 1.1 Add repository contracts for `Density/input.yaml`, candidate visual audits, and `Density/evidence.generated.json`; validate positive target world extent, exact accepted Part IDs/sheets, unique meaningful feature ids, positive pixel/required-voxel spans, and supported importance values, and verify focused contract tests reject malformed or stale inputs.
- [x] 1.2 Implement deterministic feature-to-VPU estimation and the 16-VPU candidate ladder from the accepted density input; verify tests cover repeated-input determinism, one-step-below-estimate search, monotonic candidates, the 256-axis fail-closed case, and representative Staff-like measurements without hardcoding a final Staff VPU.
- [x] 1.3 Implement candidate-evidence validation and lowest-sufficient selection: require every declared feature result, normalized-size evidence, integer bounds, occupied voxels, axis compliance, preview/source hashes, and greedy metrics; verify tests reject lost features/over-budget candidates, select the lowest of multiple passing candidates, and return no selection when all candidates fail.

## 2. Density evidence command and provenance

- [x] 2.1 Add a VoxelStore Tool density-selection operation that reads current accepted reference evidence, `Density/input.yaml`, and candidate audits, then computes/prints selection evidence and writes `Density/evidence.generated.json` only when requested; verify command tests produce identical evidence for unchanged inputs.
- [x] 2.2 Bind generated density evidence to the current reference-audit hash, density-input hash, candidate source/proof/audit hashes, estimate, candidate sequence, and selected VPU; verify changing a Part Sheet/reference audit, target world extent, feature measurement, or candidate evidence invalidates the prior selection.

## 3. Publication and maintenance integration

- [x] 3.1 Add manifest opt-in `density: reference-driven` and apply the equipment `density` health gate only to opted-in equipment after `reference` and before `source`; require current selected density evidence there and require canonical VoxelSpec VPU to equal the selected VPU in the source gate, while avatars and legacy non-opted-in equipment remain unaffected; verify focused publication tests cover legacy, current, missing, stale, and source-VPU-mismatch cases.
- [x] 3.2 Extend maintenance planning so opted-in equipment with accepted reference and missing/stale/failed density evidence remains the current asset's blocking work before source authoring, while legacy equipment keeps the prior flow; verify planner tests do not skip to another asset and proceed to source only after density selection succeeds.

## 4. Staff01 product-quality reference authority

- [x] 4.1 Opt Staff01 into `density: reference-driven`, refresh its canonical Composition Sheet from the approved product-quality staff target, visually accept it, and freeze the intended rigid-part ownership without embedding VPU/runtime details; verify the sheet remains canonical 3:2 and preserves the crystal, cage, gold structure, segmented shaft, and material hierarchy.
- [x] 4.2 Refresh and visually accept Staff01's neutral local Part Sheet with consistent orthographic scale/views and sufficient construction detail for density measurement; reseal reference evidence and verify the previous density evidence, if any, is stale while the other nine weapon reference/source packages remain unchanged.

## 5. Staff01 reference-driven VPU proof

- [x] 5.1 Author Staff01 `Density/input.yaml` from the accepted Part Sheet with target world long axis 1.5, object pixel extent, and explicit meaningful features such as shaft thickness transitions, gold collars, purple cage bars/gaps, and crystal facets; verify each measurement is auditable on the Part Sheet and the selector derives its estimate/candidates without a task-specified final VPU.
- [x] 5.2 Author/render Staff01 candidate VoxelSpecs in ascending selector order against the same accepted reference and normalized 1.5-unit length; for each candidate record feature survival, bounds, occupied voxels, greedy metrics, and proof hashes, stopping after the first passing candidate plus one higher completeness check, and verify any newly discovered meaningful feature causes density input to be amended and selection rerun.
- [x] 5.3 Run the density selector to produce Staff01 `Density/evidence.generated.json`; verify it chooses the lowest sufficient evaluated VPU, remains within Hero.Voxel's 256-axis and active source-voxel budgets, records greedy complexity evidence, and regenerates byte-identically from unchanged reference/input/candidate evidence.

## 6. Promote selected Staff01 density to canonical source

- [x] 6.1 Author Staff01 canonical palette/VoxelSpec at the selected VPU only after density evidence passes, preserving asset id, UnitRef, form, `humanoid.medium.v2 / Hand`, one-rigid-part ownership, pivot/world-size intent, and 1.5-unit long axis; regenerate Hero.Voxel output and verify the density/source/budget gates all pass.
- [x] 6.2 Generate focused/standalone Staff01 proof and visually audit it against the accepted high-detail Part Sheet and approved product-quality target; reseal proof/compatibility and verify all four Hand.0/Hand.1 x Idle compatibility rows pass without changing the other nine weapon sources.

## 7. Integration convergence

- [x] 7.1 Run the full VoxelStore build/test suite and authoritative scan; verify 11 published assets / 10 equipment / 40 passing compatibility rows / 0 coverage gaps / `NextJob=null`, run the planner twice as no-ops, and verify hashes for the other nine weapon canonical sources are unchanged.
- [x] 7.2 Run `openspec validate establish-reference-driven-vpu-selection --strict`, `git diff --check`, deterministic density/candidate/source regeneration checks, active-tree semantic audits, and workspace hygiene review; verify the change is strict-valid and contains only selector/gate/maintenance infrastructure plus the intended Staff01 reference-density migration.
