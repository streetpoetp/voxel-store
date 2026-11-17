# Proposal

## Why

`voxel-store` currently accepts schematic SVG diagrams as canonical reference evidence even though Hero.Voxel requires visually construction-authoritative Composition and Part Sheets. The store also names a style family but has no concrete visual style authority, so autonomous agents can satisfy reference coverage without preserving the approved low-resolution voxel look.

## What Changes

- **BREAKING** Replace schematic SVG reference acceptance with explicit image-sheet authority: one declared Composition Sheet image plus exactly one declared Part Sheet image per `RequiredParts` ID.
- **BREAKING** Require reference publication evidence to satisfy the canonical Hero.Voxel visual-construction contract rather than merely proving that named reference files exist and are hash-sealed.
- Establish `low-resolution-voxel.v1` as the project reference-image style authority using the three approved uploaded character sheets as canonical style references.
- Keep style authority separate from construction authority: the uploaded character sheets control voxel chunkiness, proportion language, detail density, palette/material simplification, silhouette readability, and presentation character, while Hero.Voxel remains authoritative for Composition/Part Sheet structure and construction semantics.
- Extend store policy so the active style family resolves to concrete style-reference image assets rather than a string-only label.
- Make `reference.yaml` explicitly map the Composition Sheet and each Part ID to its accepted reference image instead of deriving `Parts/<PartId>.svg` in code.
- Update the maintenance agent flow to read the approved style references before image generation, then follow the pinned Hero.Voxel sequential Composition -> Part Sheet audit flow without duplicating Hero.Voxel semantics in the store skill.
- Migrate the existing `CombatFoundation.v1` avatar/equipment reference packages from schematic SVGs to real low-resolution voxel image sheets, reseal evidence, and require the store to return healthy only after the migrated package passes the new gates.

## Capabilities

### New Capabilities

- `reference-style-authority`: Defines repository-owned visual style families, concrete style-reference image assets, and the boundary between style authority and Hero.Voxel construction authority.

### Modified Capabilities

- `asset-publication`: Tightens reference-package publication so declared image sheets, exact Part-ID mapping, visual-construction acceptance, and current style authority are required before geometry can qualify for publication.
- `store-maintenance`: Requires autonomous reference generation/repair to resolve the active style authority before generation while preserving Hero.Voxel's sequential Composition/Part Sheet workflow and fail-closed visual audit behavior.

## Impact

- Affects `policy/store.yaml`, store YAML contracts, reference validation/audit logic, tests, and `.agents/skills/voxel-store-maintain/SKILL.md`.
- Adds repository-owned style-reference image assets under `Art/Style/low-resolution-voxel.v1/`.
- Changes each asset's `Reference/reference.yaml` from implicit SVG naming to explicit image paths and migrates existing `composition.svg` / `Parts/*.svg` authority to image sheets.
- Existing `CombatFoundation.v1` reference audit hashes and derived publication state must be regenerated after migration.
- Hero.Voxel remains the only authority for Composition/Part Sheet semantics, VoxelSpec compilation, preview generation, and downstream voxel runtime technology.
