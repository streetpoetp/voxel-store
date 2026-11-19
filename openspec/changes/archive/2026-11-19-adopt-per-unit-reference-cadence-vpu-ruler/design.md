# Design

## Context

See proposal.md for motivation. The current density architecture already has the correct stage boundary: accepted v3 reference evidence precedes density, and density precedes source. The implementation problem is inside density selection: `Density/input.yaml` can drive estimates into a high candidate sequence such as 64/80/96 because feature preservation is expressed through multi-voxel span requirements.

The project also already stores the selected result per unit in `<Unit>/Density/evidence.generated.json`; that location remains appropriate.

## Goals / Non-Goals

**Goals:**

- Keep one shared numeric ruler while allowing every unit to select its own VPU.
- Bias stylized voxel authoring toward low densities without hard-coding one density by asset kind.
- Derive the search center from apparent reference cadence plus intended world extent.
- Preserve deterministic candidate selection and staleable evidence.
- Keep Product Target and construction references implementation-neutral.

**Non-Goals:**

- Selecting one VPU for all equipment.
- Encoding VPU into `policy/coverage.yaml`.
- Turning broad industry ranges into hard validation bounds.
- Automatically changing world size to make a preferred VPU fit.
- Updating existing Sword02/Dagger02 VoxelSpecs or density evidence in this documentation-only synchronization.

## Decisions

### 1. Keep four separate density-authority documents

Each document has one job:

1. `openspec/specs/reference-density-selection/spec.md` — durable behavioral contract: what density selection must mean.
2. `policy/vpu-ruler.yaml` — shared numeric search policy: candidate ladder, broad priors, and default readability semantics.
3. `WORKFLOW.md` — lifecycle/ownership: when density runs and which authority feeds it.
4. `<Unit>/Density/input.yaml` + `evidence.generated.json` — per-unit measurements, evaluated candidates, and selected result.

Alternative considered: put numeric ranges directly in WORKFLOW.md. Rejected because workflow describes stage ownership, not tuning policy.

Alternative considered: put VPU in `policy/coverage.yaml`. Rejected because coverage owns required catalog forms, not visual granularity.

### 2. The ruler is shared; VPU is not

`policy/vpu-ruler.yaml` contains a canonical low-value candidate ladder:

`2, 4, 6, 8, 10, 12, 16, 20, 24, 32, 40, 48`.

It also contains broad priors such as large-structure, character, equipment, and small-accessory. These priors influence search neighborhood/order only. They do not reject candidates outside the typical range.

This preserves a coherent project vocabulary without turning the ruler into a global VPU table.

### 3. Apparent voxel cadence replaces multi-voxel feature span as the primary estimator

Density input should measure stable voxel-style cues from accepted orthographic construction evidence. The primary estimate is:

`estimatedVpu = apparentVoxelCountAlongMeasuredExtent / intendedWorldExtent`.

The estimate is snapped/expanded into nearby canonical ruler candidates. Visual candidate evaluation then determines the lowest sufficient VPU.

Meaningful construction relationships remain acceptance evidence, but they no longer automatically demand an arbitrary 2- or 3-voxel minimum.

### 4. One voxel is a legitimate authored unit

Low-resolution voxel art intentionally uses one-voxel highlights, bevels, edges, straps, gaps, and silhouette steps. Candidate evaluation asks whether a feature is still readable and semantically distinct, not whether it occupies a universal minimum number of voxels.

A candidate still fails when quantization removes or merges the feature.

### 5. Per-unit evidence owns the final selection

The selected VPU remains stored in generated density evidence beside the asset. Future evidence should include at minimum:

- ruler version/hash;
- intended world extent;
- cadence measurement(s);
- estimated VPU;
- candidate sequence;
- per-candidate structural/style result;
- selected VPU and reason.

The source VoxelSpec then uses that selected VPU. No upstream visual-reference document gains implementation authority.

### 6. Existing high-VPU evidence becomes migration evidence, not new truth

Sword02/Dagger02 evidence remains untouched during this documentation sync. Once selector code changes, their current 64/80/96-derived evidence will become stale by ruler revision and should be recomputed intentionally.

The KnifeStyleTest is the first useful migration case because it already demonstrates why “three-voxel edge required” pushes density too high.

## Risks / Trade-offs

- **[Risk] Apparent cadence can be ambiguous in painterly or perspective-heavy targets.** → Measure accepted orthographic Part Sheets, allow multiple cadence cues, and fail closed when confidence is insufficient.
- **[Risk] Low-density bias can oversimplify semantically important shapes.** → Candidate acceptance still requires structural fidelity and model-to-target visual audit.
- **[Risk] Priors become accidental clamps in agent behavior.** → Spec and skill explicitly state that priors guide search only.
- **[Risk] Existing density evidence disagrees with the new ruler.** → Ruler revision participates in staleness; migration is explicit rather than silent.

## Migration Plan

1. Synchronize spec, ruler policy, WORKFLOW.md, and maintenance skill.
2. Update density input/evidence contracts and selector implementation to load the ruler and cadence measurements.
3. Add deterministic tests for low candidate ladders, per-unit divergence, one-voxel feature acceptance, and ruler staleness.
4. Re-run KnifeStyleTest using low candidates around the detected cadence.
5. Recompute Sword02/Dagger02 only after the new selector is proven.
