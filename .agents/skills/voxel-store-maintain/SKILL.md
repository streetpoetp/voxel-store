---
name: voxel-store-maintain
description: Coordinate one voxel-store asset job by following deterministic current-gate state until the asset is healthy, published, or explicitly blocked.
---

# Voxel Store Maintain

Use this skill only inside the voxel-store repository.

1. Read `AGENTS.md` and `WORKFLOW.md`.
2. Run deterministic scan/plan state. Claim only the first asset-scoped job and its earliest unresolved/current gate. If there is no job, report healthy/no-op and stop.
3. Keep that asset as the only creative/repair owner for this invocation. Never skip its current gate or select a second asset.
4. Dispatch the current gate to its authority instead of restating that authority here:
   - **New asset creation:** use `voxel-store-create-model` for one uncovered collection requirement that requires a new production model.
   - **Reference repair/redesign:** use `voxel-store-product-target` when Product Target acquisition/acceptance is required; use the reference-style/target-fidelity specs plus the pinned Hero.Voxel workflow for remaining reference construction and validation.
   - **Source:** use the density spec/policy when density is required, then the pinned Hero.Voxel workflow and deterministic Tool validation for VoxelSpec/source work.
   - **Proof:** use deterministic generation, visually inspect required proof, and seal only accepted evidence.
   - **Publish:** use Tool scan/verify plus publication, compatibility, coverage, and budget contracts.
5. After a gate passes, re-derive deterministic state and follow the next gate for the same asset. Do not maintain a separate manual stage tracker.
6. If a gate fails, repair that same gate. If required authority, evidence, dependency, or visual acceptance is unavailable, report the block and stop.
7. Stop when the claimed asset is healthy/published or explicitly blocked.

Detailed validity rules remain owned by OpenSpec specs, policy, the specialized Product Target skill, and the pinned Hero.Voxel workflow. This skill owns coordination only.
