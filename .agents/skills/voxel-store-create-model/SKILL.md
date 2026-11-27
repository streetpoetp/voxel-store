---
name: voxel-store-create-model
description: Compatibility entrypoint for one direct new-production voxel asset request; establish the asset intent, then delegate the same job to voxel-store-pipeline.
---

# Voxel Store Create Model

Use this skill only when the caller directly requests one new production asset in `voxel-store`.

1. Read `AGENTS.md` and `WORKFLOW.md`.
2. Confirm the request owns exactly one new production asset job. Establish the requested reusable form and asset-specific intent from the user request; do not select a second asset.
3. Keep this entrypoint limited to intent establishment. Do not maintain a second Product Target/construction/density/source/proof/compatibility/publication flow here.
4. Invoke `voxel-store-pipeline` for that same asset starting at **Intent**. The pipeline and its stage owners decide all downstream gates and any not-applicable stages.
5. If the pipeline reports a blocking authority/evidence/dependency, report that block for the same asset and stop rather than creating substitute evidence or another asset.

This skill exists for compatibility with direct prompts such as `create an axe`, `design a weapon`, or `create voxel model`; the canonical end-to-end production flow is owned by `voxel-store-pipeline`.
