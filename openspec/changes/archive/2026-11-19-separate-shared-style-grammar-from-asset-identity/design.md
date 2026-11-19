# Design: Separate shared style grammar from asset identity

## Current behavior

The repository already says shared style authority defines reusable visual language and that Product Targets become asset-specific visual authority. In practice, that wording is underspecified: an agent can interpret a polished equipment exemplar's brown wood, green wraps, and gold accents as mandatory style rather than one asset's design identity.

## Decision

Define two explicit layers:

1. **Shared style grammar / quality bar** — voxel chunkiness, readability, simplification level, silhouette clarity, construction richness, detail restraint, rendering/presentation language, and any other constraint explicitly declared global by style authority.
2. **Asset-specific visual identity** — silhouette design, palette, material hierarchy, accent colors, decorative motifs, wrap patterns, and other choices established by asset intent and frozen by its Product Target.

An exemplar-specific choice becomes a global requirement only when active style authority explicitly says it is global. Mere repetition in one or more reference images is not sufficient.

This keeps a recognizable family while allowing visually differentiated weapons and prevents agents from turning style references into copy templates.

## Why not edit the style manifest now?

Changing `Art/Style/low-resolution-voxel.v1/style.yaml` would change applicable style authority and can stale accepted visual evidence. The current problem is interpretation/execution guidance, not missing style-image bytes. Therefore this change updates the durable contract and workflow guidance only.

## Why not change the VPU ruler?

VPU is an implementation-density decision. The observed issue concerns palette/material/motif identity, so `policy/vpu-ruler.yaml` is intentionally unaffected.

## Validation

- Strict OpenSpec validation.
- Verify the workflow and both authoring skills state the same ownership boundary.
- Verify `policy/vpu-ruler.yaml` and `Art/Style/low-resolution-voxel.v1/style.yaml` remain untouched.
- Run `git diff --check`.
