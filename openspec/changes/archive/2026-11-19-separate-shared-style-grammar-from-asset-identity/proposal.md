# Proposal: Separate shared style grammar from asset identity

## Why

Recent weapon samples exposed an authoring failure mode: agents can correctly reuse `low-resolution-voxel.v1` yet mechanically copy incidental colors, materials, wraps, and accent motifs from earlier equipment. That makes distinct assets look like palette variants from one set even when the shared style contract only intended a common voxel grammar and quality bar.

## What Changes

- Clarify that shared style authority owns reusable visual grammar and quality level, not automatically every exemplar's palette/material/motif recipe.
- Make the per-asset Product Target own silhouette identity, palette, material hierarchy, and decorative motifs unless the active style authority explicitly declares one of those as a family-wide invariant.
- Require target authoring to distinguish deliberate global style constraints from incidental exemplar-specific design choices.
- Synchronize the repository workflow and target-authoring/maintenance skills with this boundary.

## Impact

- Durable contract: `openspec/specs/reference-style-authority/spec.md`.
- Workflow guidance: `WORKFLOW.md`.
- Agent guidance: `.agents/skills/voxel-store-product-target/SKILL.md` and `.agents/skills/voxel-store-maintain/SKILL.md`.
- No change to `Art/Style/low-resolution-voxel.v1/style.yaml`, so current style-authority hashes are intentionally preserved.
- No change to `policy/vpu-ruler.yaml`; density policy remains independent of visual identity.
