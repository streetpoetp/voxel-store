# Proposal

## Why

The current reference-density workflow correctly makes VPU downstream of accepted visual authority, but its selector is biased toward high global-like densities by deriving VPU from required multi-voxel feature spans. Stylized voxel production instead needs VPU to be a **per-unit visual granularity decision**, usually centered on low values and derived from each unit's apparent voxel cadence plus intended world size.

## What Changes

- Define VPU explicitly as a per-unit implementation property; there is no global project VPU for equipment or other stylized units.
- Replace “minimum multi-voxel feature thickness drives VPU upward” as the primary rule with **reference-cadence estimation**: estimate apparent voxel count from accepted construction views, combine it with intended world extent, then evaluate nearby ruler candidates.
- Allow intentional one-voxel visual features such as highlights, bevels, edge bands, straps, and silhouette steps when they remain readable and faithful to the reference.
- Add a repository-owned numeric policy at `policy/vpu-ruler.yaml` containing the low VPU candidate ladder and broad asset-class priors. Priors guide search only; they never clamp a unit.
- Keep per-unit measurements and selected VPU in each asset's `Density/input.yaml` / `Density/evidence.generated.json`, provenance-bound to that unit's accepted reference evidence.
- Update `WORKFLOW.md` and the maintenance skill so agents understand the ruler's ownership and do not encode numeric VPU policy into Product Target, Composition, Part Sheets, or coverage policy.
- Leave selector/tool implementation changes as a separate implementation step after the documentation contract is synchronized.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `reference-density-selection`: density selection becomes per-unit reference-cadence-driven, uses the shared low-VPU ruler as candidate guidance, permits readable one-voxel features, and treats asset-class ranges as priors rather than constraints.

## Impact

- Durable behavior: `openspec/specs/reference-density-selection/spec.md`.
- Numeric policy: new `policy/vpu-ruler.yaml`.
- Workflow ownership: `WORKFLOW.md`.
- Agent execution guidance: `.agents/skills/voxel-store-maintain/SKILL.md`.
- Future selector implementation: `ReferenceDensitySelector`, density input/evidence contracts, tests, and existing high-density sample evidence will need migration after this documentation change.
- No production VoxelSpec or published asset is modified by this documentation synchronization.
