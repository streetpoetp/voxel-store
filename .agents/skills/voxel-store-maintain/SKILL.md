---
name: voxel-store-maintain
description: Select one deterministic voxel-store maintenance job, then delegate that same asset and current gate to voxel-store-pipeline until healthy, published, or blocked.
---

# Voxel Store Maintain

Use this skill only inside the `voxel-store` repository.

1. Read `AGENTS.md` and `WORKFLOW.md`.
2. Run deterministic scan/plan state. **Claim only the first asset-scoped job** and its earliest unresolved/current gate. If there is no job, report healthy/no-op and stop.
3. Keep that asset as the only creative/repair owner for this invocation. Never skip its current gate or select a second asset.
4. Delegate the same asset to `voxel-store-pipeline` at the canonical stage that owns the Tool-reported current gate:
   - an uncovered new-production requirement enters at **Intent**;
   - an existing repair enters at the earliest matching pipeline stage without replaying already-current upstream authority.
5. Let the pipeline dispatch the stage to its existing Tool/OpenSpec/Hero.Voxel/specialized-skill owner. Do not restate those detailed validity rules here.
6. After progress, re-derive deterministic scan/plan state for the same asset. If authoritative state reports an earlier unresolved gate, delegate that earlier stage; otherwise continue from the newly reported stage.
7. If the current gate remains blocked, report its owning authority/evidence and stop. Do not select a second asset.
8. Stop when the claimed asset is healthy/published or explicitly blocked.

Maintenance owns deterministic job selection; `voxel-store-pipeline` owns the shared one-asset stage navigation. Neither replaces the underlying validation authorities.
